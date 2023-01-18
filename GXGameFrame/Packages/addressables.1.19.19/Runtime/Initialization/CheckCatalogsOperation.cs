using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.AddressableAssets
{
    class CheckCatalogsOperation : AsyncOperationBase<List<string>>
    {
        AddressablesImpl m_Addressables;
        List<string> m_LocalHashes;
        List<AddressablesImpl.ResourceLocatorInfo> m_LocatorInfos;
        AsyncOperationHandle<IList<AsyncOperationHandle>> m_DepOp;
        public CheckCatalogsOperation(AddressablesImpl aa)
        {
            m_Addressables = aa;
        }

        public AsyncOperationHandle<List<string>> Start(List<AddressablesImpl.ResourceLocatorInfo> locatorInfos)
        {
            m_LocatorInfos = new List<AddressablesImpl.ResourceLocatorInfo>(locatorInfos.Count);
            m_LocalHashes = new List<string>(locatorInfos.Count);
            var locations = new List<IResourceLocation>(locatorInfos.Count);
            foreach (var rl in locatorInfos)
            {
                if (rl.CanUpdateContent)
                {
                    locations.Add(rl.HashLocation);
                    m_LocalHashes.Add(rl.LocalHash);
                    m_LocatorInfos.Add(rl);
                }
            }

            ContentCatalogProvider ccp = m_Addressables.ResourceManager.ResourceProviders
                .FirstOrDefault(rp => rp.GetType() == typeof(ContentCatalogProvider)) as ContentCatalogProvider;
            if (ccp != null)
                ccp.DisableCatalogUpdateOnStart = false;

            m_DepOp = m_Addressables.ResourceManager.CreateGroupOperation<string>(locations);
            return m_Addressables.ResourceManager.StartOperation(this, m_DepOp);
        }

        /// <inheritdoc />
        protected override bool InvokeWaitForCompletion()
        {
            if (IsDone)
                return true;
            if (m_DepOp.IsValid() && !m_DepOp.IsDone)
                m_DepOp.WaitForCompletion();

            m_RM?.Update(Time.unscaledDeltaTime);

            if (!HasExecuted)
                InvokeExecute();

            m_RM?.Update(Time.unscaledDeltaTime);
            return IsDone;
        }

        protected override void Destroy()
        {
            m_Addressables.Release(m_DepOp);
        }

        /// <inheritdoc />
        public override void GetDependencies(List<AsyncOperationHandle> dependencies)
        {
            dependencies.Add(m_DepOp);
        }

        internal static List<string> ProcessDependentOpResults(IList<AsyncOperationHandle> results,
            List<AddressablesImpl.ResourceLocatorInfo> locatorInfos, List<string> localHashes, out string errorString, out bool success)
        {
            var result = new List<string>();
            List<string> errorMsgList = new List<string>();
            for (int i = 0; i < results.Count; i++)
            {
                var remHashOp = results[i];
                var remoteHash = remHashOp.Result as string;
                if (!string.IsNullOrEmpty(remoteHash) && remoteHash != localHashes[i])
                {
                    result.Add(locatorInfos[i].Locator.LocatorId);
                    locatorInfos[i].ContentUpdateAvailable = true;
                }
                else if (remHashOp.OperationException != null)
                {
                    result.Add(null);
                    locatorInfos[i].ContentUpdateAvailable = false;
                    errorMsgList.Add(remHashOp.OperationException.Message);
                }
            }

            errorString = null;

            if (errorMsgList.Count > 0)
            {
                if (errorMsgList.Count == result.Count)
                {
                    result = null;
                    errorString = "CheckCatalogsOperation failed with the following errors: ";
                }
                else
                {
                    errorString = "Partial success in CheckCatalogsOperation with the following errors: ";
                }
                
                foreach (string str in errorMsgList)
                    errorString = errorString + "\n" + str;
            }

            success = errorMsgList.Count == 0;
            return result;
        }

        protected override void Execute()
        {
            var result = ProcessDependentOpResults(m_DepOp.Result, m_LocatorInfos, m_LocalHashes, out string errorString, out bool success);
            Complete(result, success, errorString);
        }
    }
}
