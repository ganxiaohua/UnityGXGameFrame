using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.AddressableAssets.Utility;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using ResourceManager = UnityEngine.ResourceManagement.ResourceManager;

namespace UnityEngine.AddressableAssets.Initialization
{
    internal class InitializationOperation : AsyncOperationBase<IResourceLocator>
    {
        AsyncOperationHandle<ResourceManagerRuntimeData> m_rtdOp;
        AsyncOperationHandle<IResourceLocator> m_loadCatalogOp;
        string m_ProviderSuffix;
        AddressablesImpl m_Addressables;
        ResourceManagerDiagnostics m_Diagnostics;
        InitalizationObjectsOperation m_InitGroupOps;

        public InitializationOperation(AddressablesImpl aa)
        {
            m_Addressables = aa;
            m_Diagnostics = new ResourceManagerDiagnostics(aa.ResourceManager);
        }

        protected override float Progress
        {
            get
            {
                if (m_rtdOp.IsValid())
                    return m_rtdOp.PercentComplete;
                return 0f;
            }
        }

        protected override string DebugName
        {
            get { return "InitializationOperation"; }
        }

        internal static AsyncOperationHandle<IResourceLocator> CreateInitializationOperation(AddressablesImpl aa, string playerSettingsLocation, string providerSuffix)
        {
            var jp = new JsonAssetProvider();
            aa.ResourceManager.ResourceProviders.Add(jp);
            var tdp = new TextDataProvider();
            aa.ResourceManager.ResourceProviders.Add(tdp);
            aa.ResourceManager.ResourceProviders.Add(new ContentCatalogProvider(aa.ResourceManager));

            var runtimeDataLocation = new ResourceLocationBase("RuntimeData", playerSettingsLocation, typeof(JsonAssetProvider).FullName, typeof(ResourceManagerRuntimeData));

            var initOp = new InitializationOperation(aa);
            initOp.m_rtdOp = aa.ResourceManager.ProvideResource<ResourceManagerRuntimeData>(runtimeDataLocation);
            initOp.m_ProviderSuffix = providerSuffix;
            initOp.m_InitGroupOps = new InitalizationObjectsOperation();
            initOp.m_InitGroupOps.Init(initOp.m_rtdOp, aa);

            var groupOpHandle = aa.ResourceManager.StartOperation(initOp.m_InitGroupOps, initOp.m_rtdOp);

            return aa.ResourceManager.StartOperation<IResourceLocator>(initOp, groupOpHandle);
        }

        /// <inheritdoc />
        protected override bool InvokeWaitForCompletion()
        {
            if (IsDone)
                return true;

            if (m_rtdOp.IsValid() && !m_rtdOp.IsDone)
                m_rtdOp.WaitForCompletion();

            m_RM?.Update(Time.unscaledDeltaTime);

            if (!HasExecuted)
                InvokeExecute();

            if (m_loadCatalogOp.IsValid() && !m_loadCatalogOp.IsDone)
            {
                m_loadCatalogOp.WaitForCompletion();
                m_RM?.Update(Time.unscaledDeltaTime); //We need completion callbacks to get triggered.
            }

            return m_rtdOp.IsDone && m_loadCatalogOp.IsDone;
        }

        protected override void Execute()
        {
            Addressables.LogFormat("Addressables - runtime data operation completed with status = {0}, result = {1}.", m_rtdOp.Status, m_rtdOp.Result);
            if (m_rtdOp.Result == null)
            {
                Addressables.LogWarningFormat("Addressables - Unable to load runtime data at location {0}.", m_rtdOp);
                Complete(Result, false, string.Format("Addressables - Unable to load runtime data at location {0}.", m_rtdOp));
                return;
            }
            Addressables.LogFormat("Initializing Addressables version {0}.", m_rtdOp.Result.AddressablesVersion);
            var rtd = m_rtdOp.Result;

            m_Addressables.ResourceManager.postProfilerEvents = rtd.ProfileEvents;
            WebRequestQueue.SetMaxConcurrentRequests(rtd.MaxConcurrentWebRequests);
            m_Addressables.CatalogRequestsTimeout = rtd.CatalogRequestsTimeout;
            foreach (var catalogLocation in rtd.CatalogLocations)
            {
                if (catalogLocation.Data != null && catalogLocation.Data is ProviderLoadRequestOptions loadData)
                {
                    loadData.WebRequestTimeout = rtd.CatalogRequestsTimeout;
                }
            }

            m_Addressables.Release(m_rtdOp);
            if (rtd.CertificateHandlerType != null)
                m_Addressables.ResourceManager.CertificateHandlerInstance = Activator.CreateInstance(rtd.CertificateHandlerType) as CertificateHandler;

#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString() != rtd.BuildTarget)
                Addressables.LogErrorFormat("Addressables - runtime data was built with a different build target.  Expected {0}, but data was built with {1}.  Certain assets may not load correctly including shaders.  You can rebuild player content via the Addressables window.", UnityEditor.EditorUserBuildSettings.activeBuildTarget, rtd.BuildTarget);
#endif
            if (!rtd.LogResourceManagerExceptions)
                ResourceManager.ExceptionHandler = null;

            if (!rtd.ProfileEvents)
            {
                m_Diagnostics.Dispose();
                m_Diagnostics = null;
                m_Addressables.ResourceManager.ClearDiagnosticCallbacks();
            }

            Addressables.Log("Addressables - loading initialization objects.");

            ContentCatalogProvider ccp = m_Addressables.ResourceManager.ResourceProviders
                .FirstOrDefault(rp => rp.GetType() == typeof(ContentCatalogProvider)) as ContentCatalogProvider;
            if (ccp != null)
            {
                ccp.DisableCatalogUpdateOnStart = rtd.DisableCatalogUpdateOnStartup;
                ccp.IsLocalCatalogInBundle = rtd.IsLocalCatalogInBundle;
            }

            var locMap = new ResourceLocationMap("CatalogLocator", rtd.CatalogLocations);
            m_Addressables.AddResourceLocator(locMap);
            IList<IResourceLocation> catalogs;
            if (!locMap.Locate(ResourceManagerRuntimeData.kCatalogAddress, typeof(ContentCatalogData), out catalogs))
            {
                Addressables.LogWarningFormat(
                    "Addressables - Unable to find any catalog locations in the runtime data.");
                m_Addressables.RemoveResourceLocator(locMap);
                Complete(Result, false, "Addressables - Unable to find any catalog locations in the runtime data.");
            }
            else
            {
                Addressables.LogFormat("Addressables - loading content catalogs, {0} found.", catalogs.Count);
                IResourceLocation remoteHashLocation = null;
                if (catalogs[0].Dependencies.Count == 2 && rtd.DisableCatalogUpdateOnStartup)
                {
                    remoteHashLocation = catalogs[0].Dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote];
                    catalogs[0].Dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = catalogs[0].Dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache];
                }
                m_loadCatalogOp = LoadContentCatalogInternal(catalogs, 0, locMap, remoteHashLocation);
            }
        }

        static void LoadProvider(AddressablesImpl addressables, ObjectInitializationData providerData, string providerSuffix)
        {
            //don't add providers that have the same id...
            var indexOfExistingProvider = -1;
            var newProviderId = string.IsNullOrEmpty(providerSuffix) ? providerData.Id : (providerData.Id + providerSuffix);
            for (int i = 0; i < addressables.ResourceManager.ResourceProviders.Count; i++)
            {
                var rp = addressables.ResourceManager.ResourceProviders[i];
                if (rp.ProviderId == newProviderId)
                {
                    indexOfExistingProvider = i;
                    break;
                }
            }

            //if not re-initializing, just use the old provider
            if (indexOfExistingProvider >= 0 && string.IsNullOrEmpty(providerSuffix))
                return;

            var provider = providerData.CreateInstance<IResourceProvider>(newProviderId);
            if (provider != null)
            {
                if (indexOfExistingProvider < 0 || !string.IsNullOrEmpty(providerSuffix))
                {
                    Addressables.LogFormat("Addressables - added provider {0} with id {1}.", provider, provider.ProviderId);
                    addressables.ResourceManager.ResourceProviders.Add(provider);
                }
                else
                {
                    Addressables.LogFormat("Addressables - replacing provider {0} at index {1}.", provider, indexOfExistingProvider);
                    addressables.ResourceManager.ResourceProviders[indexOfExistingProvider] = provider;
                }
            }
            else
            {
                Addressables.LogWarningFormat("Addressables - Unable to load resource provider from {0}.", providerData);
            }
        }

        static AsyncOperationHandle<IResourceLocator> OnCatalogDataLoaded(AddressablesImpl addressables, AsyncOperationHandle<ContentCatalogData> op, string providerSuffix, IResourceLocation remoteHashLocation)
        {
            var data = op.Result;
            addressables.Release(op);
            if (data == null)
            {
                var opException = op.OperationException != null ? new Exception("Failed to load content catalog.", op.OperationException) : new Exception("Failed to load content catalog.");
                return addressables.ResourceManager.CreateCompletedOperationWithException<IResourceLocator>(null, opException);
            }
            else
            {
                if (data.ResourceProviderData != null)
                    foreach (var providerData in data.ResourceProviderData)
                        LoadProvider(addressables, providerData, providerSuffix);
                if (addressables.InstanceProvider == null)
                {
                    var prov = data.InstanceProviderData.CreateInstance<IInstanceProvider>();
                    if (prov != null)
                        addressables.InstanceProvider = prov;
                }
                if (addressables.SceneProvider == null)
                {
                    var prov = data.SceneProviderData.CreateInstance<ISceneProvider>();
                    if (prov != null)
                        addressables.SceneProvider = prov;
                }
                if (remoteHashLocation != null)
                    data.location.Dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = remoteHashLocation;

                ResourceLocationMap locMap = data.CreateCustomLocator(data.location.PrimaryKey, providerSuffix);
                addressables.AddResourceLocator(locMap, data.localHash, data.location);
                addressables.AddResourceLocator(new DynamicResourceLocator(addressables));
                return addressables.ResourceManager.CreateCompletedOperation<IResourceLocator>(locMap, string.Empty);
            }
        }

        public static AsyncOperationHandle<IResourceLocator> LoadContentCatalog(AddressablesImpl addressables, IResourceLocation loc, string providerSuffix, IResourceLocation remoteHashLocation = null)
        {
            Type provType = typeof(ProviderOperation<ContentCatalogData>);
            var catalogOp = addressables.ResourceManager.CreateOperation<ProviderOperation<ContentCatalogData>>(provType, provType.GetHashCode(), null, null);

            IResourceProvider catalogProvider = null;
            foreach (IResourceProvider provider in addressables.ResourceManager.ResourceProviders)
            {
                if (provider is ContentCatalogProvider)
                {
                    catalogProvider = provider;
                    break;
                }
            }

            var dependencies = addressables.ResourceManager.CreateGroupOperation<string>(loc.Dependencies, true);
            catalogOp.Init(addressables.ResourceManager, catalogProvider, loc, dependencies, true);
            var catalogHandle = addressables.ResourceManager.StartOperation(catalogOp, dependencies);
            dependencies.Release();

            var chainOp = addressables.ResourceManager.CreateChainOperation(catalogHandle, res => OnCatalogDataLoaded(addressables, res, providerSuffix, remoteHashLocation));
            return chainOp;
        }

        public AsyncOperationHandle<IResourceLocator> LoadContentCatalog(IResourceLocation loc, string providerSuffix, IResourceLocation remoteHashLocation)
        {
            return LoadContentCatalog(m_Addressables, loc, providerSuffix, remoteHashLocation);
        }

        //Attempts to load each catalog in order, stopping at first success.
        internal AsyncOperationHandle<IResourceLocator> LoadContentCatalogInternal(IList<IResourceLocation> catalogs, int index, ResourceLocationMap locMap, IResourceLocation remoteHashLocation)
        {
            Addressables.LogFormat("Addressables - loading content catalog from {0}.", m_Addressables.ResourceManager.TransformInternalId(catalogs[index]));
            var loadOp = LoadContentCatalog(catalogs[index], m_ProviderSuffix, remoteHashLocation);
            if (loadOp.IsDone)
                LoadOpComplete(loadOp, catalogs, locMap, index, remoteHashLocation);
            else
                loadOp.Completed += op =>
                {
                    LoadOpComplete(op, catalogs, locMap, index, remoteHashLocation);
                };
            return loadOp;
        }

        void LoadOpComplete(AsyncOperationHandle<IResourceLocator> op, IList<IResourceLocation> catalogs, ResourceLocationMap locMap, int index, IResourceLocation remoteHashLocation)
        {
            if (op.Result != null)
            {
                m_Addressables.RemoveResourceLocator(locMap);
                Result = op.Result;
                Complete(Result, true, string.Empty);
                m_Addressables.Release(op);
                Addressables.Log("Addressables - initialization complete.");
            }
            else
            {
                Addressables.LogFormat("Addressables - failed to load content catalog from {0}.", op);
                if (index + 1 >= catalogs.Count)
                {
                    Addressables.LogWarningFormat("Addressables - initialization failed.", op);
                    m_Addressables.RemoveResourceLocator(locMap);
                    if (op.OperationException != null)
                        Complete(Result, false, op.OperationException);
                    else
                        Complete(Result, false, "LoadContentCatalogInternal");
                    m_Addressables.Release(op);
                }
                else
                {
                    m_loadCatalogOp = LoadContentCatalogInternal(catalogs, index + 1, locMap, remoteHashLocation);
                    m_Addressables.Release(op);
                }
            }
        }
    }
}
