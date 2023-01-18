using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ResourceManagement.AsyncOperations
{
    /// <summary>
    /// Handle for internal operations.  This allows for reference counting and checking for valid references.
    /// </summary>
    /// <typeparam name="TObject">The object type of the underlying operation.</typeparam>
    public struct AsyncOperationHandle<TObject> : IEnumerator, IEquatable<AsyncOperationHandle<TObject>>
    {
        internal AsyncOperationBase<TObject> m_InternalOp;
        int m_Version;
        string m_LocationName;

        internal string LocationName
        {
            get { return m_LocationName; }
            set { m_LocationName = value; }
        }

        bool m_UnloadSceneOpExcludeReleaseCallback;
        internal bool UnloadSceneOpExcludeReleaseCallback
        {
            get { return m_UnloadSceneOpExcludeReleaseCallback; }
            set { m_UnloadSceneOpExcludeReleaseCallback = value; }
        }

        /// <summary>
        /// Conversion from typed to non typed handles.  This does not increment the reference count.
        /// To convert from non-typed back, use AsyncOperationHandle.Convert&lt;T&gt;()
        /// </summary>
        /// <param name="obj">The typed handle to convert.</param>
        /// <returns>Returns the converted operation handle.</returns>
        static public implicit operator AsyncOperationHandle(AsyncOperationHandle<TObject> obj)
        {
            return new AsyncOperationHandle(obj.m_InternalOp, obj.m_Version, obj.m_LocationName);
        }

        internal AsyncOperationHandle(AsyncOperationBase<TObject> op)
        {
            m_InternalOp = op;
            m_Version = op?.Version ?? 0;
            m_LocationName = null;
            m_UnloadSceneOpExcludeReleaseCallback = false;
        }

        /// <summary>
        /// Return the current download status for this operation and its dependencies.
        /// </summary>
        /// <returns>The download status.</returns>
        public DownloadStatus GetDownloadStatus()
        {
            return InternalGetDownloadStatus(new HashSet<object>());
        }

        internal DownloadStatus InternalGetDownloadStatus(HashSet<object> visited)
        {
            if (visited == null)
                visited = new HashSet<object>();
            return visited.Add(InternalOp) ? InternalOp.GetDownloadStatus(visited) : new DownloadStatus() { IsDone = IsDone };
        }

        internal AsyncOperationHandle(IAsyncOperation op)
        {
            m_InternalOp = (AsyncOperationBase<TObject>)op;
            m_Version = op?.Version ?? 0;
            m_LocationName = null;
            m_UnloadSceneOpExcludeReleaseCallback = false;
        }

        internal AsyncOperationHandle(IAsyncOperation op, int version)
        {
            m_InternalOp = (AsyncOperationBase<TObject>)op;
            m_Version = version;
            m_LocationName = null;
            m_UnloadSceneOpExcludeReleaseCallback = false;
        }

        internal AsyncOperationHandle(IAsyncOperation op, string locationName)
        {
            m_InternalOp = (AsyncOperationBase<TObject>)op;
            m_Version = op?.Version ?? 0;
            m_LocationName = locationName;
            m_UnloadSceneOpExcludeReleaseCallback = false;
        }

        internal AsyncOperationHandle(IAsyncOperation op, int version, string locationName)
        {
            m_InternalOp = (AsyncOperationBase<TObject>)op;
            m_Version = version;
            m_LocationName = locationName;
            m_UnloadSceneOpExcludeReleaseCallback = false;
        }

        /// <summary>
        /// Acquire a new handle to the internal operation.  This will increment the reference count, therefore the returned handle must also be released.
        /// </summary>
        /// <returns>A new handle to the operation.  This handle must also be released.</returns>
        internal AsyncOperationHandle<TObject> Acquire()
        {
            InternalOp.IncrementReferenceCount();
            return this;
        }

        /// <summary>
        /// Completion event for the internal operation.  If this is assigned on a completed operation, the callback is deferred until the LateUpdate of the current frame.
        /// </summary>
        public event Action<AsyncOperationHandle<TObject>> Completed
        {
            add { InternalOp.Completed += value; }
            remove { InternalOp.Completed -= value; }
        }

        /// <summary>
        /// Completion event for non-typed callback handlers.  If this is assigned on a completed operation, the callback is deferred until the LateUpdate of the current frame.
        /// </summary>
        public event Action<AsyncOperationHandle> CompletedTypeless
        {
            add { InternalOp.CompletedTypeless += value; }
            remove { InternalOp.CompletedTypeless -= value; }
        }

        /// <summary>
        /// Debug name of the operation.
        /// </summary>
        public string DebugName
        {
            get
            {
                if (!IsValid())
                    return "InvalidHandle";
                return ((IAsyncOperation)InternalOp).DebugName;
            }
        }

        /// <summary>
        /// Get dependency operations.
        /// </summary>
        /// <param name="deps">The list of AsyncOperationHandles that are dependencies of a given AsyncOperationHandle</param>
        public void GetDependencies(List<AsyncOperationHandle> deps)
        {
            InternalOp.GetDependencies(deps);
        }

        /// <summary>
        /// Event for handling the destruction of the operation.
        /// </summary>
        public event Action<AsyncOperationHandle> Destroyed
        {
            add { InternalOp.Destroyed += value; }
            remove { InternalOp.Destroyed -= value; }
        }

        /// <summary>
        /// Provide equality for this struct.
        /// </summary>
        /// <param name="other">The operation to compare to.</param>
        /// <returns>True if the the operation handles reference the same AsyncOperation and the version is the same.</returns>
        public bool Equals(AsyncOperationHandle<TObject> other)
        {
            return m_Version == other.m_Version && m_InternalOp == other.m_InternalOp;
        }

        /// <summary>
        /// Get hash code of this struct.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_InternalOp == null ? 0 : m_InternalOp.GetHashCode() * 17 + m_Version;
        }

        /// <summary>
        /// Synchronously complete the async operation.
        /// </summary>
        /// <returns>The result of the operation or null.</returns>
        public TObject WaitForCompletion()
        {
#if !UNITY_2021_1_OR_NEWER
            AsyncOperationHandle.IsWaitingForCompletion = true;
            try
            {
                if (IsValid() && !InternalOp.IsDone)
                    InternalOp.WaitForCompletion();
                if (IsValid())
                    return Result;
            }
            finally
            {
                AsyncOperationHandle.IsWaitingForCompletion = false;
                m_InternalOp?.m_RM?.Update(Time.unscaledDeltaTime);
            }
#else
            if (IsValid() && !InternalOp.IsDone)
                InternalOp.WaitForCompletion();

            m_InternalOp?.m_RM?.Update(Time.unscaledDeltaTime);
            if (IsValid())
                return Result;
#endif
            return default(TObject);
        }

        AsyncOperationBase<TObject> InternalOp
        {
            get
            {
                if (m_InternalOp == null || m_InternalOp.Version != m_Version)
                    throw new Exception("Attempting to use an invalid operation handle");
                return m_InternalOp;
            }
        }

        /// <summary>
        /// True if the operation is complete.
        /// </summary>
        public bool IsDone
        {
            get { return !IsValid() || InternalOp.IsDone; }
        }

        /// <summary>
        /// Check if the handle references an internal operation.
        /// </summary>
        /// <returns>True if valid.</returns>
        public bool IsValid()
        {
            return m_InternalOp != null && m_InternalOp.Version == m_Version;
        }

        /// <summary>
        /// The exception for a failed operation.  This will be null unless Status is failed.
        /// </summary>
        public Exception OperationException
        {
            get { return InternalOp.OperationException; }
        }

        /// <summary>
        /// The progress of the internal operation.
        /// This is evenly weighted between all sub-operations. For example, a LoadAssetAsync call could potentially
        /// be chained with InitializeAsync and have multiple dependent operations that download and load content.
        /// In that scenario, PercentComplete would reflect how far the overal operation was, and would not accurately
        /// represent just percent downloaded or percent loaded into memory.
        /// For accurate download percentages, use GetDownloadStatus(). 
        /// </summary>
        public float PercentComplete
        {
            get { return InternalOp.PercentComplete; }
        }

        /// <summary>
        /// The current reference count of the internal operation.
        /// </summary>
        internal int ReferenceCount
        {
            get { return InternalOp.ReferenceCount; }
        }

        /// <summary>
        /// Release the handle.  If the internal operation reference count reaches 0, the resource will be released.
        /// </summary>
        internal void Release()
        {
            InternalOp.DecrementReferenceCount();
            m_InternalOp = null;
        }

        /// <summary>
        /// The result object of the operations.
        /// </summary>
        public TObject Result
        {
            get { return InternalOp.Result; }
        }

        /// <summary>
        /// The status of the internal operation.
        /// </summary>
        public AsyncOperationStatus Status
        {
            get { return InternalOp.Status; }
        }

        /// <summary>
        /// Return a Task object to wait on when using async await.
        /// </summary>
        public System.Threading.Tasks.Task<TObject> Task
        {
            get { return InternalOp.Task; }
        }

        object IEnumerator.Current
        {
            get { return Result; }
        }

        /// <summary>
        /// Overload for <see cref="IEnumerator.MoveNext"/>.
        /// </summary>
        /// <returns>Returns true if the enumerator can advance to the next element in the collectin. Returns false otherwise.</returns>
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        /// <summary>
        /// Overload for <see cref="IEnumerator.Reset"/>.
        /// </summary>
        void IEnumerator.Reset() {}
    }

    /// <summary>
    /// Non typed operation handle.  This allows for reference counting and checking for valid references.
    /// </summary>
    public struct AsyncOperationHandle : IEnumerator
    {
#if !UNITY_2021_1_OR_NEWER
        private static bool m_IsWaitingForCompletion = false;
        internal static bool IsWaitingForCompletion
        {
            get { return m_IsWaitingForCompletion; }
            set { m_IsWaitingForCompletion = value; }
        }
#endif
        
        internal IAsyncOperation m_InternalOp;
        int m_Version;
        string m_LocationName;

        internal string LocationName
        {
            get { return m_LocationName; }
            set { m_LocationName = value; }
        }

        internal AsyncOperationHandle(IAsyncOperation op)
        {
            m_InternalOp = op;
            m_Version = op?.Version ?? 0;
            m_LocationName = null;
        }

        internal AsyncOperationHandle(IAsyncOperation op, int version)
        {
            m_InternalOp = op;
            m_Version = version;
            m_LocationName = null;
        }

        internal AsyncOperationHandle(IAsyncOperation op, string locationName)
        {
            m_InternalOp = op;
            m_Version = op?.Version ?? 0;
            m_LocationName = locationName;
        }

        internal AsyncOperationHandle(IAsyncOperation op, int version, string locationName)
        {
            m_InternalOp = op;
            m_Version = version;
            m_LocationName = locationName;
        }

        /// <summary>
        /// Acquire a new handle to the internal operation.  This will increment the reference count, therefore the returned handle must also be released.
        /// </summary>
        /// <returns>A new handle to the operation.  This handle must also be released.</returns>
        internal AsyncOperationHandle Acquire()
        {
            InternalOp.IncrementReferenceCount();
            return this;
        }

        /// <summary>
        /// Completion event for the internal operation.  If this is assigned on a completed operation, the callback is deferred until the LateUpdate of the current frame.
        /// </summary>
        public event Action<AsyncOperationHandle> Completed
        {
            add { InternalOp.CompletedTypeless += value; }
            remove { InternalOp.CompletedTypeless -= value; }
        }

        /// <summary>
        /// Converts handle to be typed.  This does not increment the reference count.
        /// To convert back to non-typed, implicit conversion is available.
        /// </summary>
        /// <typeparam name="T">The type of the handle.</typeparam>
        /// <returns>A new handle that is typed.</returns>
        public AsyncOperationHandle<T> Convert<T>()
        {
            return new AsyncOperationHandle<T>(InternalOp, m_Version, m_LocationName);
        }

        /// <summary>
        /// Provide equality for this struct.
        /// </summary>
        /// <param name="other">The operation to compare to.</param>
        /// <returns>True if the the operation handles reference the same AsyncOperation and the version is the same.</returns>
        public bool Equals(AsyncOperationHandle other)
        {
            return m_Version == other.m_Version && m_InternalOp == other.m_InternalOp;
        }

        /// <summary>
        /// Debug name of the operation.
        /// </summary>
        public string DebugName
        {
            get
            {
                if (!IsValid())
                    return "InvalidHandle";
                return InternalOp.DebugName;
            }
        }

        /// <summary>
        /// Event for handling the destruction of the operation.
        /// </summary>
        public event Action<AsyncOperationHandle> Destroyed
        {
            add { InternalOp.Destroyed += value; }
            remove { InternalOp.Destroyed -= value; }
        }

        /// <summary>
        /// Get dependency operations.
        /// </summary>
        /// <param name="deps"></param>
        public void GetDependencies(List<AsyncOperationHandle> deps)
        {
            InternalOp.GetDependencies(deps);
        }

        /// <summary>
        /// Get hash code of this struct.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_InternalOp == null ? 0 : m_InternalOp.GetHashCode() * 17 + m_Version;
        }

        IAsyncOperation InternalOp
        {
            get
            {
                if (m_InternalOp == null || m_InternalOp.Version != m_Version)
                    throw new Exception("Attempting to use an invalid operation handle");
                return m_InternalOp;
            }
        }

        /// <summary>
        /// True if the operation is complete.
        /// </summary>
        public bool IsDone
        {
            get { return !IsValid() || InternalOp.IsDone; }
        }

        /// <summary>
        /// Check if the internal operation is not null and has the same version of this handle.
        /// </summary>
        /// <returns>True if valid.</returns>
        public bool IsValid()
        {
            return m_InternalOp != null && m_InternalOp.Version == m_Version;
        }

        /// <summary>
        /// The exception for a failed operation.  This will be null unless Status is failed.
        /// </summary>
        public Exception OperationException
        {
            get { return InternalOp.OperationException; }
        }

        /// <summary>
        /// The progress of the internal operation.
        /// This is evenly weighted between all sub-operations. For example, a LoadAssetAsync call could potentially
        /// be chained with InitializeAsync and have multiple dependent operations that download and load content.
        /// In that scenario, PercentComplete would reflect how far the overal operation was, and would not accurately
        /// represent just percent downloaded or percent loaded into memory.
        /// For accurate download percentages, use GetDownloadStatus(). 
        /// </summary>
        public float PercentComplete
        {
            get { return InternalOp.PercentComplete; }
        }

        /// <summary>
        /// Return the current download status for this operation and its dependencies.  In some instances, the information will not be available.  This can happen if the operation
        /// is dependent on the initialization operation for addressables.  Once the initialization operation completes, the information returned will be accurate.
        /// </summary>
        /// <returns>The download status.</returns>
        public DownloadStatus GetDownloadStatus()
        {
            return InternalGetDownloadStatus(new HashSet<object>());
        }

        internal DownloadStatus InternalGetDownloadStatus(HashSet<object> visited)
        {
            if (visited == null)
                visited = new HashSet<object>();
            return visited.Add(InternalOp) ? InternalOp.GetDownloadStatus(visited) : new DownloadStatus() { IsDone = IsDone };
        }
        /// <summary>
        /// The current reference count of the internal operation.
        /// </summary>
        internal int ReferenceCount
        {
            get { return InternalOp.ReferenceCount; }
        }

        /// <summary>
        /// Release the handle.  If the internal operation reference count reaches 0, the resource will be released.
        /// </summary>
        internal void Release()
        {
            InternalOp.DecrementReferenceCount();
            m_InternalOp = null;
        }

        /// <summary>
        /// The result object of the operations.
        /// </summary>
        public object Result
        {
            get { return InternalOp.GetResultAsObject(); }
        }

        /// <summary>
        /// The status of the internal operation.
        /// </summary>
        public AsyncOperationStatus Status
        {
            get { return InternalOp.Status; }
        }

        /// <summary>
        /// Return a Task object to wait on when using async await.
        /// </summary>
        public System.Threading.Tasks.Task<object> Task
        {
            get { return InternalOp.Task; }
        }

        object IEnumerator.Current
        {
            get { return Result; }
        }

        /// <summary>
        /// Overload for <see cref="IEnumerator.MoveNext"/>.
        /// </summary>
        /// <returns>Returns true if the enumerator can advance to the next element in the collectin. Returns false otherwise.</returns>
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        /// <summary>
        /// Overload for <see cref="IEnumerator.Reset"/>.
        /// </summary>
        void IEnumerator.Reset() {}

        /// <summary>
        /// Synchronously complete the async operation.
        /// </summary>
        /// <returns>The result of the operation or null.</returns>
        public object WaitForCompletion()
        {
#if !UNITY_2021_1_OR_NEWER
            IsWaitingForCompletion = true;
            try
            {
                if (IsValid() && !InternalOp.IsDone)
                    InternalOp.WaitForCompletion();
                if (IsValid())
                    return Result;
            }
            finally
            {
                IsWaitingForCompletion = false;
            }
#else
            if (IsValid() && !InternalOp.IsDone)
                InternalOp.WaitForCompletion();
            if (IsValid())
                return Result;
#endif
            return null;
        }
    }
}
