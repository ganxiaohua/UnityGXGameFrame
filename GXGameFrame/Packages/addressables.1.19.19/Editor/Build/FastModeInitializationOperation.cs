using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.AddressableAssets.Utility;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace UnityEditor.AddressableAssets.Settings
{
    internal class FastModeInitializationOperation : AsyncOperationBase<IResourceLocator>
    {
        AddressablesImpl m_addressables;
        AddressableAssetSettings m_settings;
        internal ResourceManagerDiagnostics m_Diagnostics;
        AsyncOperationHandle<IList<AsyncOperationHandle>> groupOp;
        public FastModeInitializationOperation(AddressablesImpl addressables, AddressableAssetSettings settings)
        {
            m_addressables = addressables;
            m_settings = settings;
            m_addressables.ResourceManager.RegisterForCallbacks();
            m_Diagnostics = new ResourceManagerDiagnostics(m_addressables.ResourceManager);
        }

        static T GetBuilderOfType<T>(AddressableAssetSettings settings) where T : class, IDataBuilder
        {
            foreach (var db in settings.DataBuilders)
            {
                var b = db;
                if (b.GetType() == typeof(T))
                    return b as T;
            }
            return null;
        }

        ///<inheritdoc />
        protected override bool InvokeWaitForCompletion()
        {
            if (IsDone)
                return true;

            m_RM?.Update(Time.unscaledDeltaTime);
            if(!HasExecuted)
                InvokeExecute();
            return true;
        }

        protected override void Execute()
        {
            var db = GetBuilderOfType<BuildScriptFastMode>(m_settings);
            if (db == null)
                UnityEngine.Debug.Log($"Unable to find {nameof(BuildScriptFastMode)} builder in settings assets. Using default Instance and Scene Providers.");

            var locator = new AddressableAssetSettingsLocator(m_settings);
            m_addressables.AddResourceLocator(locator);
            m_addressables.AddResourceLocator(new DynamicResourceLocator(m_addressables));
            m_addressables.ResourceManager.postProfilerEvents = ProjectConfigData.PostProfilerEvents;
            if (!m_addressables.ResourceManager.postProfilerEvents)
            {
                m_Diagnostics.Dispose();
                m_Diagnostics = null;
                m_addressables.ResourceManager.ClearDiagnosticCallbacks();
            }

            if (!m_settings.buildSettings.LogResourceManagerExceptions)
                ResourceManager.ExceptionHandler = null;

            //NOTE: for some reason, the data builders can get lost from the settings asset during a domain reload - this only happens in tests and custom instance and scene providers are not needed
            m_addressables.InstanceProvider = db == null ? new InstanceProvider() : ObjectInitializationData.CreateSerializedInitializationData(db.instanceProviderType.Value).CreateInstance<IInstanceProvider>();
            m_addressables.SceneProvider = db == null ? new SceneProvider() : ObjectInitializationData.CreateSerializedInitializationData(db.sceneProviderType.Value).CreateInstance<ISceneProvider>();
            m_addressables.ResourceManager.ResourceProviders.Add(new AssetDatabaseProvider());
            m_addressables.ResourceManager.ResourceProviders.Add(new TextDataProvider());
            m_addressables.ResourceManager.ResourceProviders.Add(new JsonAssetProvider());
            m_addressables.ResourceManager.ResourceProviders.Add(new LegacyResourcesProvider());
            m_addressables.ResourceManager.ResourceProviders.Add(new AtlasSpriteProvider());
            m_addressables.ResourceManager.ResourceProviders.Add(new ContentCatalogProvider(m_addressables.ResourceManager));
            WebRequestQueue.SetMaxConcurrentRequests(m_settings.MaxConcurrentWebRequests);
            m_addressables.CatalogRequestsTimeout = m_settings.CatalogRequestsTimeout;

            if (m_settings.InitializationObjects.Count == 0)
            {
                Complete(locator, true, null);
            }
            else
            {
                List<AsyncOperationHandle> initOperations = new List<AsyncOperationHandle>();
                foreach (var io in m_settings.InitializationObjects)
                {
                    if (io is IObjectInitializationDataProvider)
                    {
                        var ioData = (io as IObjectInitializationDataProvider).CreateObjectInitializationData();
                        var h = ioData.GetAsyncInitHandle(m_addressables.ResourceManager);
                        initOperations.Add(h);
                    }
                }

                groupOp = m_addressables.ResourceManager.CreateGenericGroupOperation(initOperations, true);
                groupOp.Completed += op =>
                {
                    bool success = op.Status == AsyncOperationStatus.Succeeded;
                    Complete(locator, success, success ? "" : $"{op.DebugName}, status={op.Status}, result={op.Result} failed initialization.");
                    m_addressables.Release(op);
                };
            }
        }
    }
}
