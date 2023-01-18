using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.AsyncOperations
{
    internal interface IGenericProviderOperation
    {
        void Init(ResourceManager rm, IResourceProvider provider, IResourceLocation location, AsyncOperationHandle<IList<AsyncOperationHandle>> depOp);
        void Init(ResourceManager rm, IResourceProvider provider, IResourceLocation location, AsyncOperationHandle<IList<AsyncOperationHandle>> depOp, bool releaseDependenciesOnFailure);
        int ProvideHandleVersion { get; }
        IResourceLocation Location { get; }
        int DependencyCount { get; }
        void GetDependencies(IList<object> dstList);
        TDepObject GetDependency<TDepObject>(int index);
        void SetProgressCallback(Func<float> callback);
        void ProviderCompleted<T>(T result, bool status, Exception e);
        Type RequestedType { get; }
        void SetDownloadProgressCallback(Func<DownloadStatus> callback);
        void SetWaitForCompletionCallback(Func<bool> callback);
    }

    [UnityEngine.Scripting.Preserve]
    internal class ProviderOperation<TObject> : AsyncOperationBase<TObject>, IGenericProviderOperation, ICachable
    {
        private bool m_ReleaseDependenciesOnFailure = true;
        private Action<int, object, bool, Exception> m_CompletionCallback;
        private Action<int, IList<object>> m_GetDepCallback;
        private Func<float> m_GetProgressCallback;
        private Func<DownloadStatus> m_GetDownloadProgressCallback;
        private Func<bool> m_WaitForCompletionCallback;
        private DownloadStatus m_DownloadStatus;
        private IResourceProvider m_Provider;
        internal AsyncOperationHandle<IList<AsyncOperationHandle>> m_DepOp;
        private IResourceLocation m_Location;
        private int m_ProvideHandleVersion;
        private bool m_NeedsRelease;
        IOperationCacheKey ICachable.Key { get; set; }
        private ResourceManager m_ResourceManager;
        private const float k_OperationWaitingToCompletePercentComplete = 0.99f;
        public int ProvideHandleVersion { get { return m_ProvideHandleVersion; } }
        public IResourceLocation Location { get { return m_Location; } }
        public void SetDownloadProgressCallback(Func<DownloadStatus> callback)
        {
            m_GetDownloadProgressCallback = callback;
            if (m_GetDownloadProgressCallback != null)
                m_DownloadStatus = m_GetDownloadProgressCallback();
        }

        public void SetWaitForCompletionCallback(Func<bool> callback)
        {
            m_WaitForCompletionCallback = callback;
        }

        ///<inheritdoc />
        protected  override bool InvokeWaitForCompletion()
        {
            if (IsDone)
                return true;
            if (m_DepOp.IsValid() && !m_DepOp.IsDone)
                m_DepOp.WaitForCompletion();
            if (m_WaitForCompletionCallback == null)
                return false;
            m_RM?.Update(Time.unscaledDeltaTime);
            if (!HasExecuted)
                InvokeExecute();
            if (m_WaitForCompletionCallback == null)
                return false;
            return m_WaitForCompletionCallback.Invoke();
        }

        internal override DownloadStatus GetDownloadStatus(HashSet<object> visited)
        {
            var depDLS = m_DepOp.IsValid() ? m_DepOp.InternalGetDownloadStatus(visited) : default;

            if (m_GetDownloadProgressCallback != null)
                m_DownloadStatus = m_GetDownloadProgressCallback();

            if (Status == AsyncOperationStatus.Succeeded)
                m_DownloadStatus.DownloadedBytes = m_DownloadStatus.TotalBytes;

            return new DownloadStatus() { DownloadedBytes = m_DownloadStatus.DownloadedBytes + depDLS.DownloadedBytes, TotalBytes = m_DownloadStatus.TotalBytes + depDLS.TotalBytes, IsDone = IsDone };
        }

        public ProviderOperation()
        {
        }

        /// <inheritdoc />
        public override void GetDependencies(List<AsyncOperationHandle> deps)
        {
            if (m_DepOp.IsValid())
                deps.Add(m_DepOp);
        }

        internal override void ReleaseDependencies()
        {
            if (m_DepOp.IsValid())
                m_DepOp.Release();
        }

        protected override string DebugName
        {
            get
            {
                return string.Format("Resource<{0}>({1})", typeof(TObject).Name, m_Location == null ? "Invalid" : ShortenPath(m_Location.InternalId, true));
            }
        }

        internal const string kInvalidHandleMsg = "The ProvideHandle is invalid. After the handle has been completed, it can no longer be used";

        public void GetDependencies(IList<object> dstList)
        {
            dstList.Clear();

            if (!m_DepOp.IsValid())
                return;

            if (m_DepOp.Result == null)
                return;

            for (int i = 0; i < m_DepOp.Result.Count; i++)
                dstList.Add(m_DepOp.Result[i].Result);
        }

        public Type RequestedType { get { return typeof(TObject); } }

        public int DependencyCount
        {
            get
            {
                return (!m_DepOp.IsValid() || m_DepOp.Result == null) ? 0 : m_DepOp.Result.Count;
            }
        }

        public TDepObject GetDependency<TDepObject>(int index)
        {
            if (!m_DepOp.IsValid() || m_DepOp.Result == null)
                throw new Exception("Cannot get dependency because no dependencies were available");

            return (TDepObject)(m_DepOp.Result[index].Result);
        }

        public void SetProgressCallback(Func<float> callback)
        {
            m_GetProgressCallback = callback;
        }

        public void ProviderCompleted<T>(T result, bool status, Exception e)
        {
            m_ProvideHandleVersion++;
            m_GetProgressCallback = null;
            m_GetDownloadProgressCallback = null;
            m_WaitForCompletionCallback = null;
            m_NeedsRelease = status;

            ProviderOperation<T> top = this as ProviderOperation<T>;
            if (top != null)
            {
                top.Result = result;
            }
            else if (result == null && !typeof(TObject).IsValueType)
            {
                Result = (TObject)(object)null;
            }
            else if (result != null && typeof(TObject).IsAssignableFrom(result.GetType()))
            {
                Result = (TObject)(object)result;
            }
            else
            {
                string errorMsg = string.Format("Provider of type {0} with id {1} has provided a result of type {2} which cannot be converted to requested type {3}. The operation will be marked as failed.", m_Provider.GetType().ToString(), m_Provider.ProviderId, typeof(T), typeof(TObject));
                Complete(Result, false, errorMsg);
                throw new Exception(errorMsg);
            }

            Complete(Result, status, e, m_ReleaseDependenciesOnFailure);
        }

        protected override float Progress
        {
            get
            {
                try
                {
                    float numberOfOps = 1f;
                    float total = 0f;
                    if (m_GetProgressCallback != null)
                        total += m_GetProgressCallback();

                    if (!m_DepOp.IsValid() || m_DepOp.Result == null || m_DepOp.Result.Count == 0)
                    {
                        total++;
                        numberOfOps++;
                    }
                    else
                    {
                        foreach (var handle in m_DepOp.Result)
                        {
                            total += handle.PercentComplete;
                            numberOfOps++;
                        }
                    }

                    float result = total / numberOfOps;
                    //This is done because all AssetBundle operations (m_DepOp.Result) can complete as well as the
                    //BundledAssetRequest operation (m_GetProgressCallBack) but this overall operation hasn't completed yet.
                    //Once the operation has a chance to complete we short circut calling into Progress here and just return 1.0f
                    return Mathf.Min(result, k_OperationWaitingToCompletePercentComplete);
                }
                catch
                {
                    return 0.0f;
                }
            }
        }

        protected override void Execute()
        {
            Debug.Assert(m_DepOp.IsDone);

            if (m_DepOp.IsValid() && m_DepOp.Status == AsyncOperationStatus.Failed && (m_Provider.BehaviourFlags & ProviderBehaviourFlags.CanProvideWithFailedDependencies) == 0)
            {
                ProviderCompleted(default(TObject), false, new Exception("Dependency Exception", m_DepOp.OperationException));
            }
            else
            {
                try
                {
                    m_Provider.Provide(new ProvideHandle(m_ResourceManager, this));
                }
                catch (Exception e)
                {
                    ProviderCompleted(default(TObject), false, e);
                }
            }
        }

        public void Init(ResourceManager rm, IResourceProvider provider, IResourceLocation location, AsyncOperationHandle<IList<AsyncOperationHandle>> depOp)
        {
            m_DownloadStatus = default;
            m_ResourceManager = rm;
            m_DepOp = depOp;
            if (m_DepOp.IsValid())
                m_DepOp.Acquire();
            m_Provider = provider;
            m_Location = location;
            m_ReleaseDependenciesOnFailure = true;
            SetWaitForCompletionCallback(WaitForCompletionHandler);
        }

        public void Init(ResourceManager rm, IResourceProvider provider, IResourceLocation location, AsyncOperationHandle<IList<AsyncOperationHandle>> depOp, bool releaseDependenciesOnFailure)
        {
            m_DownloadStatus = default;
            m_ResourceManager = rm;
            m_DepOp = depOp;
            if (m_DepOp.IsValid())
                m_DepOp.Acquire();
            m_Provider = provider;
            m_Location = location;
            m_ReleaseDependenciesOnFailure = releaseDependenciesOnFailure;
            SetWaitForCompletionCallback(WaitForCompletionHandler);
        }

        bool WaitForCompletionHandler()
        {
            if (IsDone)
                return true;

            if (!m_DepOp.IsDone)
                m_DepOp.WaitForCompletion();
            if (!HasExecuted)
                InvokeExecute();

            return IsDone;
        }

        protected override void Destroy()
        {
            if (m_NeedsRelease)
                m_Provider.Release(m_Location, Result);
            if (m_DepOp.IsValid())
                m_DepOp.Release();
            Result = default(TObject);
        }
    }
}
