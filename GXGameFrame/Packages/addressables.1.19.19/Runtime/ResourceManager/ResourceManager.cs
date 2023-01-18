using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;

namespace UnityEngine.ResourceManagement
{
    /// <summary>
    /// Entry point for ResourceManager API
    /// </summary>
    public class ResourceManager : IDisposable
    {
        /// <summary>
        /// Options for event types that will be sent by the ResourceManager
        /// </summary>
        public enum DiagnosticEventType
        {
            /// <summary>
            /// Use to indicate that an operation failed.
            /// </summary>
            AsyncOperationFail,
            /// <summary>
            /// Use to indicate that an operation was created.
            /// </summary>
            AsyncOperationCreate,
            /// <summary>
            /// Use to indicate the percentage of completion for an operation was updated.
            /// </summary>
            AsyncOperationPercentComplete,
            /// <summary>
            /// Use to indicate that an operation has completed.
            /// </summary>
            AsyncOperationComplete,
            /// <summary>
            /// Use to indicate that the reference count of an operation was modified.
            /// </summary>
            AsyncOperationReferenceCount,
            /// <summary>
            /// Use to indicate that an operation was destroyed.
            /// </summary>
            AsyncOperationDestroy,
        }

        internal bool postProfilerEvents = false;

        /// <summary>
        /// Container for information associated with a Diagnostics event.
        /// </summary>
        public struct DiagnosticEventContext
        {
            /// <summary>
            /// Operation handle for the event.
            /// </summary>
            public AsyncOperationHandle OperationHandle { get; }

            /// <summary>
            /// The type of diagnostic event.
            /// </summary>
            public DiagnosticEventType Type { get; }

            /// <summary>
            /// The value for this event.
            /// </summary>
            public int EventValue { get; }

            /// <summary>
            /// The IResourceLocation being provided by the operation triggering this event.
            /// This value is null if the event is not while providing a resource.
            /// </summary>
            public IResourceLocation Location { get; }

            /// <summary>
            /// Addition data included with this event.
            /// </summary>
            public object Context { get; }

            /// <summary>
            /// Any error that occured.
            /// </summary>
            public string Error { get; }

            /// <summary>
            /// Construct a new DiagnosticEventContext.
            /// </summary>
            /// <param name="op">Operation handle for the event.</param>
            /// <param name="type">The type of diagnostic event.</param>
            /// <param name="eventValue">The value for this event.</param>
            /// <param name="error">Any error that occured.</param>
            /// <param name="context">Additional context data.</param>
            public DiagnosticEventContext(AsyncOperationHandle op, DiagnosticEventType type, int eventValue = 0, string error = null, object context = null)
            {
                OperationHandle = op;
                Type = type;
                EventValue = eventValue;
                Location = op.m_InternalOp != null && op.m_InternalOp is IGenericProviderOperation gen
                    ? gen.Location
                    : null;
                Error = error;
                Context = context;
            }
        }

        /// <summary>
        /// Global exception handler.  This will be called whenever an IAsyncOperation.OperationException is set to a non-null value.
        /// </summary>
        /// <example>
        /// <code source="../../Tests/Editor/DocExampleCode/AddExceptionHandler.cs" region="doc_AddExceptionHandler" title="Adding a global exception hanlder"/>
        /// </example>
        public static Action<AsyncOperationHandle, Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// Functor to transform internal ids before being used by the providers.
        /// See the [TransformInternalId](xref:addressables-api-transform-internal-id) documentation for more details.
        /// </summary>
        public Func<IResourceLocation, string> InternalIdTransformFunc { get; set; }

        /// <summary>
        /// Checks for an internal id transform function and uses it to modify the internal id value.
        /// </summary>
        /// <param name="location">The location to transform the internal id of.</param>
        /// <returns>If a transform func is set, use it to pull the local id; otherwise, the InternalId property of the location is used.</returns>
        public string TransformInternalId(IResourceLocation location)
        {
            return InternalIdTransformFunc == null ? location.InternalId : InternalIdTransformFunc(location);
        }

        /// <summary>
        /// Delegate that can be used to override the web request options before being sent.
        /// </summary>
        /// <remarks>
        /// The web request passed to this delegate has already been preconfigured internally. Override at your own risk.
        /// </remarks>
        public Action<UnityWebRequest> WebRequestOverride { get; set; }

        internal bool CallbackHooksEnabled = true; // tests might need to disable the callback hooks to manually pump updating

        ListWithEvents<IResourceProvider> m_ResourceProviders = new ListWithEvents<IResourceProvider>();
        IAllocationStrategy m_allocator;

        // list of all the providers in s_ResourceProviders that implement IUpdateReceiver
        ListWithEvents<IUpdateReceiver> m_UpdateReceivers = new ListWithEvents<IUpdateReceiver>();
        List<IUpdateReceiver> m_UpdateReceiversToRemove = null;
        //this prevents removing receivers during iteration
        bool m_UpdatingReceivers = false;
        //this prevents re-entrance into the Update method, which can cause stack overflow and infinite loops
        bool m_InsideUpdateMethod = false;
        internal int OperationCacheCount { get { return m_AssetOperationCache.Count; } }
        internal int InstanceOperationCount { get { return m_TrackedInstanceOperations.Count; } }
        //cache of type + providerId to IResourceProviders for faster lookup
        internal Dictionary<int, IResourceProvider> m_providerMap = new Dictionary<int, IResourceProvider>();
        Dictionary<IOperationCacheKey, IAsyncOperation> m_AssetOperationCache = new Dictionary<IOperationCacheKey, IAsyncOperation>();
        HashSet<InstanceOperation> m_TrackedInstanceOperations = new HashSet<InstanceOperation>();
        DelegateList<float> m_UpdateCallbacks = DelegateList<float>.CreateWithGlobalCache();
        List<IAsyncOperation> m_DeferredCompleteCallbacks = new List<IAsyncOperation>();

        bool m_InsideExecuteDeferredCallbacksMethod = false;
        List<DeferredCallbackRegisterRequest> m_DeferredCallbacksToRegister = null;
        private struct DeferredCallbackRegisterRequest
        {
            internal IAsyncOperation operation;
            internal bool incrementRefCount;
        }

        Action<AsyncOperationHandle, DiagnosticEventType, int, object> m_obsoleteDiagnosticsHandler; // For use in working with Obsolete RegisterDiagnosticCallback method.
        Action<DiagnosticEventContext> m_diagnosticsHandler;
        Action<IAsyncOperation> m_ReleaseOpNonCached;
        Action<IAsyncOperation> m_ReleaseOpCached;
        Action<IAsyncOperation> m_ReleaseInstanceOp;
        static int s_GroupOperationTypeHash = typeof(GroupOperation).GetHashCode();
        static int s_InstanceOperationTypeHash = typeof(InstanceOperation).GetHashCode();
        /// <summary>
        /// Add an update reveiver.
        /// </summary>
        /// <param name="receiver">The object to add. The Update method will be called until the object is removed. </param>
        public void AddUpdateReceiver(IUpdateReceiver receiver)
        {
            if (receiver == null)
                return;
            m_UpdateReceivers.Add(receiver);
        }

        /// <summary>
        /// Remove update receiver.
        /// </summary>
        /// <param name="receiver">The object to remove.</param>
        public void RemoveUpdateReciever(IUpdateReceiver receiver)
        {
            if (receiver == null)
                return;

            if (m_UpdatingReceivers)
            {
                if (m_UpdateReceiversToRemove == null)
                    m_UpdateReceiversToRemove = new List<IUpdateReceiver>();
                m_UpdateReceiversToRemove.Add(receiver);
            }
            else
            {
                m_UpdateReceivers.Remove(receiver);
            }
        }

        /// <summary>
        /// The allocation strategy object.
        /// </summary>
        public IAllocationStrategy Allocator { get { return m_allocator; } set { m_allocator = value; } }

        /// <summary>
        /// Gets the list of configured <see cref="IResourceProvider"/> objects. Resource Providers handle load and release operations for <see cref="IResourceLocation"/> objects.
        /// </summary>
        /// <value>The resource providers list.</value>
        public IList<IResourceProvider> ResourceProviders { get { return m_ResourceProviders; } }

        /// <summary>
        /// The CertificateHandler instance object.
        /// </summary>
        public CertificateHandler CertificateHandlerInstance { get; set; }

        /// <summary>
        /// Constructor for the resource manager.
        /// </summary>
        /// <param name="alloc">The allocation strategy to use.</param>
        public ResourceManager(IAllocationStrategy alloc = null)
        {
            m_ReleaseOpNonCached = OnOperationDestroyNonCached;
            m_ReleaseOpCached = OnOperationDestroyCached;
            m_ReleaseInstanceOp = OnInstanceOperationDestroy;
            m_allocator = alloc == null ? new LRUCacheAllocationStrategy(1000, 1000, 100, 10) : alloc;
            m_ResourceProviders.OnElementAdded += OnObjectAdded;
            m_ResourceProviders.OnElementRemoved += OnObjectRemoved;
            m_UpdateReceivers.OnElementAdded += x => RegisterForCallbacks();
        }

        private void OnObjectAdded(object obj)
        {
            IUpdateReceiver updateReceiver = obj as IUpdateReceiver;
            if (updateReceiver != null)
                AddUpdateReceiver(updateReceiver);
        }

        private void OnObjectRemoved(object obj)
        {
            IUpdateReceiver updateReceiver = obj as IUpdateReceiver;
            if (updateReceiver != null)
                RemoveUpdateReciever(updateReceiver);
        }

        bool m_RegisteredForCallbacks = false;
        internal void RegisterForCallbacks()
        {
            if (CallbackHooksEnabled && !m_RegisteredForCallbacks)
            {
                m_RegisteredForCallbacks = true;
                MonoBehaviourCallbackHooks.Instance.OnUpdateDelegate += Update;
            }
        }

        /// <summary>
        /// Clears out the diagnostics callback handler.
        /// </summary>
        [Obsolete("ClearDiagnosticsCallback is Obsolete, use ClearDiagnosticCallbacks instead.")]
        public void ClearDiagnosticsCallback()
        {
            m_diagnosticsHandler = null;
            m_obsoleteDiagnosticsHandler = null;
        }

        /// <summary>
        /// Clears out the diagnostics callbacks handler.
        /// </summary>
        public void ClearDiagnosticCallbacks()
        {
            m_diagnosticsHandler = null;
            m_obsoleteDiagnosticsHandler = null;
        }

        /// <summary>
        /// Unregister a handler for diagnostic events.
        /// </summary>
        /// <param name="func">The event handler function.</param>
        public void UnregisterDiagnosticCallback(Action<DiagnosticEventContext> func)
        {
            if (m_diagnosticsHandler != null)
                m_diagnosticsHandler -= func;
            else
                Debug.LogError("No Diagnostic callbacks registered, cannot remove callback.");
        }

        /// <summary>
        /// Register a handler for diagnostic events.
        /// </summary>
        /// <param name="func">The event handler function.</param>
        [Obsolete]
        public void RegisterDiagnosticCallback(Action<AsyncOperationHandle, ResourceManager.DiagnosticEventType, int, object> func)
        {
            m_obsoleteDiagnosticsHandler = func;
        }

        /// <summary>
        /// Register a handler for diagnostic events.
        /// </summary>
        /// <param name="func">The event handler function.</param>
        public void RegisterDiagnosticCallback(Action<DiagnosticEventContext> func)
        {
            m_diagnosticsHandler += func;
        }

        internal void PostDiagnosticEvent(DiagnosticEventContext context)
        {
            m_diagnosticsHandler?.Invoke(context);

            if (m_obsoleteDiagnosticsHandler == null)
                return;
            m_obsoleteDiagnosticsHandler(context.OperationHandle, context.Type, context.EventValue, string.IsNullOrEmpty(context.Error) ? context.Context : context.Error);
        }

        /// <summary>
        /// Gets the appropriate <see cref="IResourceProvider"/> for the given <paramref name="location"/> and <paramref name="type"/>.
        /// </summary>
        /// <returns>The resource provider. Or null if an appropriate provider cannot be found</returns>
        /// <param name="t">The desired object type to be loaded from the provider.</param>
        /// <param name="location">The resource location.</param>
        public IResourceProvider GetResourceProvider(Type t, IResourceLocation location)
        {
            if (location != null)
            {
                IResourceProvider prov = null;
                var hash = location.ProviderId.GetHashCode() * 31 + (t == null ? 0 : t.GetHashCode());
                if (!m_providerMap.TryGetValue(hash, out prov))
                {
                    for (int i = 0; i < ResourceProviders.Count; i++)
                    {
                        var p = ResourceProviders[i];
                        if (p.ProviderId.Equals(location.ProviderId, StringComparison.Ordinal) && (t == null || p.CanProvide(t, location)))
                        {
                            m_providerMap.Add(hash, prov = p);
                            break;
                        }
                    }
                }
                return prov;
            }
            return null;
        }

        Type GetDefaultTypeForLocation(IResourceLocation loc)
        {
            var provider = GetResourceProvider(null, loc);
            if (provider == null)
                return typeof(object);
            Type t = provider.GetDefaultType(loc);
            return t != null ? t : typeof(object);
        }

        private int CalculateLocationsHash(IList<IResourceLocation> locations, Type t = null)
        {
            if (locations == null || locations.Count == 0)
                return 0;
            int hash = 17;
            foreach (var loc in locations)
            {
                Type t2 = t != null ? t : GetDefaultTypeForLocation(loc);
                hash = hash * 31 + loc.Hash(t2);
            }
            return hash;
        }

        /// <summary>
        /// Load the <typeparamref name="TObject"/> at the specified <paramref name="location"/>.
        /// </summary>
        /// <returns>An async operation.</returns>
        /// <param name="location">Location to load.</param>
        /// <param name="releaseDependenciesOnFailure">When true, if the operation fails, dependencies will be released.</param>
        /// <typeparam name="TObject">Object type to load.</typeparam>
        private AsyncOperationHandle ProvideResource(IResourceLocation location, Type desiredType = null, bool releaseDependenciesOnFailure = true)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            IResourceProvider provider = null;
            if (desiredType == null)
            {
                provider = GetResourceProvider(desiredType, location);
                if (provider == null)
                {
                    var ex = new UnknownResourceProviderException(location);
                    return CreateCompletedOperationInternal<object>(null, false, ex, releaseDependenciesOnFailure);
                }
                desiredType = provider.GetDefaultType(location);
            }

            IAsyncOperation op;
            var key = new LocationCacheKey(location, desiredType);
            if (m_AssetOperationCache.TryGetValue(key, out op))
            {
                op.IncrementReferenceCount();
                return new AsyncOperationHandle(op, location.ToString());;
            }

            Type provType;
            if (!m_ProviderOperationTypeCache.TryGetValue(desiredType, out provType))
                m_ProviderOperationTypeCache.Add(desiredType, provType = typeof(ProviderOperation<>).MakeGenericType(new Type[] { desiredType }));
            op = CreateOperation<IAsyncOperation>(provType, provType.GetHashCode(), key, m_ReleaseOpCached);

            // Calculate the hash of the dependencies
            int depHash = location.DependencyHashCode;
            var depOp = location.HasDependencies ?
                ProvideResourceGroupCached(location.Dependencies, depHash, null, null, releaseDependenciesOnFailure) :
                default(AsyncOperationHandle<IList<AsyncOperationHandle>>);
            if (provider == null)
                provider = GetResourceProvider(desiredType, location);

            ((IGenericProviderOperation)op).Init(this, provider, location, depOp, releaseDependenciesOnFailure);

            var handle = StartOperation(op, depOp);
            handle.LocationName = location.ToString();

            if (depOp.IsValid())
                depOp.Release();

            return handle;
        }

        Dictionary<Type, Type> m_ProviderOperationTypeCache = new Dictionary<Type, Type>();

        /// <summary>
        /// Load the <typeparamref name="TObject"/> at the specified <paramref name="location"/>.
        /// </summary>
        /// <returns>An async operation.</returns>
        /// <param name="location">Location to load.</param>
        /// <typeparam name="TObject">Object type to load.</typeparam>
        public AsyncOperationHandle<TObject> ProvideResource<TObject>(IResourceLocation location)
        {
            AsyncOperationHandle handle = ProvideResource(location, typeof(TObject));
            return handle.Convert<TObject>();
        }

        /// <summary>
        /// Registers an operation with the ResourceManager. The operation will be executed when the <paramref name="dependency"/> completes.
        /// This should only be used when creating custom operations.
        /// </summary>
        /// <returns>The AsyncOperationHandle used to access the result and status of the operation.</returns>
        /// <param name="operation">The custom AsyncOperationBase object</param>
        /// <param name="dependency">Execution of the operation will not occur until this handle completes. A default handle can be passed if no dependency is required.</param>
        /// <typeparam name="TObject">Object type associated with this operation.</typeparam>
        public AsyncOperationHandle<TObject> StartOperation<TObject>(AsyncOperationBase<TObject> operation, AsyncOperationHandle dependency)
        {
            operation.Start(this, dependency, m_UpdateCallbacks);
            return operation.Handle;
        }

        internal AsyncOperationHandle StartOperation(IAsyncOperation operation, AsyncOperationHandle dependency)
        {
            operation.Start(this, dependency, m_UpdateCallbacks);
            return operation.Handle;
        }

        class CompletedOperation<TObject> : AsyncOperationBase<TObject>
        {
            bool m_Success;
            Exception m_Exception;
            bool m_ReleaseDependenciesOnFailure;
            public CompletedOperation() {}
            public void Init(TObject result, bool success, string errorMsg, bool releaseDependenciesOnFailure = true)
            {
                Init(result, success, !string.IsNullOrEmpty(errorMsg) ? new Exception(errorMsg) : null, releaseDependenciesOnFailure);
            }

            public void Init(TObject result, bool success, Exception exception, bool releaseDependenciesOnFailure = true)
            {
                Result = result;
                m_Success = success;
                m_Exception = exception;
                m_ReleaseDependenciesOnFailure = releaseDependenciesOnFailure;
            }

            protected override string DebugName
            {
                get { return "CompletedOperation";}
            }

            ///<inheritdoc />
            protected  override bool InvokeWaitForCompletion()
            {
                m_RM?.Update(Time.unscaledDeltaTime);
                if (!HasExecuted)
                    InvokeExecute();
                return true;
            }

            protected override void Execute()
            {
                Complete(Result, m_Success, m_Exception, m_ReleaseDependenciesOnFailure);
            }
        }

        void OnInstanceOperationDestroy(IAsyncOperation o)
        {
            m_TrackedInstanceOperations.Remove(o as InstanceOperation);
            Allocator.Release(o.GetType().GetHashCode(), o);
        }

        void OnOperationDestroyNonCached(IAsyncOperation o)
        {
            Allocator.Release(o.GetType().GetHashCode(), o);
        }

        void OnOperationDestroyCached(IAsyncOperation o)
        {
            Allocator.Release(o.GetType().GetHashCode(), o);
            var cachable = o as ICachable;
            if (cachable?.Key != null)
                RemoveOperationFromCache(cachable.Key);
        }

        internal T CreateOperation<T>(Type actualType, int typeHash, IOperationCacheKey cacheKey, Action<IAsyncOperation> onDestroyAction) where T : IAsyncOperation
        {
            if (cacheKey == null)
            {
                var op = (T)Allocator.New(actualType, typeHash);
                op.OnDestroy = onDestroyAction;
                return op;
            }
            else
            {
                var op = (T)Allocator.New(actualType, typeHash);
                op.OnDestroy = onDestroyAction;
                if (op is ICachable cachable)
                {
                    cachable.Key = cacheKey;
                    AddOperationToCache(cacheKey, op);
                }
                return op;
            }
        }

        internal void AddOperationToCache(IOperationCacheKey key, IAsyncOperation operation)
        {
            if (!IsOperationCached(key))
                m_AssetOperationCache.Add(key, operation);
        }

        internal bool RemoveOperationFromCache(IOperationCacheKey key)
        {
            if (!IsOperationCached(key))
                return true;

            return m_AssetOperationCache.Remove(key);
        }

        internal bool IsOperationCached(IOperationCacheKey key)
        {
            return m_AssetOperationCache.ContainsKey(key);
        }

        internal int CachedOperationCount()
        {
            return m_AssetOperationCache.Count;
        }

        /// <summary>
        /// Creates an operation that has already completed with a specified result and error message./>.
        /// </summary>
        /// <param name="result">The result that the operation will provide.</param>
        /// <param name="errorMsg">The error message if the operation should be in the failed state. Otherwise null or empty string.</param>
        /// <typeparam name="TObject">Object type.</typeparam>
        /// <returns>The operation handle used for the completed operation.</returns>
        public AsyncOperationHandle<TObject> CreateCompletedOperation<TObject>(TObject result, string errorMsg)
        {
            var success = string.IsNullOrEmpty(errorMsg);
            return CreateCompletedOperationInternal(result, success, !success ? new Exception(errorMsg) : null);
        }

        /// <summary>
        /// Creates an operation that has already completed with a specified result and error message./>.
        /// </summary>
        /// <param name="result">The result that the operation will provide.</param>
        /// <param name="exception">The exception with an error message if the operation should be in the failed state. Otherwise null.</param>
        /// <typeparam name="TObject">Object type.</typeparam>
        /// <returns>The operation handle used for the completed operation.</returns>
        public AsyncOperationHandle<TObject> CreateCompletedOperationWithException<TObject>(TObject result, Exception exception)
        {
            return CreateCompletedOperationInternal(result, exception == null, exception);
        }

        internal AsyncOperationHandle<TObject> CreateCompletedOperationInternal<TObject>(TObject result, bool success, Exception exception, bool releaseDependenciesOnFailure = true)
        {
            var cop = CreateOperation<CompletedOperation<TObject>>(typeof(CompletedOperation<TObject>), typeof(CompletedOperation<TObject>).GetHashCode(), null, m_ReleaseOpNonCached);
            cop.Init(result, success, exception, releaseDependenciesOnFailure);
            return StartOperation(cop, default(AsyncOperationHandle));
        }

        /// <summary>
        /// Release the operation associated with the specified handle
        /// </summary>
        /// <param name="handle">The handle to release.</param>
        public void Release(AsyncOperationHandle handle)
        {
            handle.Release();
        }

        /// <summary>
        /// Increment reference count of operation handle.
        /// </summary>
        /// <param name="handle">The handle to the resource to increment the reference count for.</param>
        public void Acquire(AsyncOperationHandle handle)
        {
            handle.Acquire();
        }

        private GroupOperation AcquireGroupOpFromCache(IOperationCacheKey key)
        {
            IAsyncOperation opGeneric;
            if (m_AssetOperationCache.TryGetValue(key, out opGeneric))
            {
                opGeneric.IncrementReferenceCount();
                return (GroupOperation)opGeneric;
            }
            return null;
        }

        /// <summary>
        /// Create a group operation for a set of locations.
        /// </summary>
        /// <typeparam name="T">The expected object type for the operations.</typeparam>
        /// <param name="locations">The list of locations to load.</param>
        /// <returns>The operation for the entire group.</returns>
        public AsyncOperationHandle<IList<AsyncOperationHandle>> CreateGroupOperation<T>(IList<IResourceLocation> locations)
        {
            var op = CreateOperation<GroupOperation>(typeof(GroupOperation), s_GroupOperationTypeHash, null, m_ReleaseOpNonCached);
            var ops = new List<AsyncOperationHandle>(locations.Count);
            foreach (var loc in locations)
                ops.Add(ProvideResource<T>(loc));

            op.Init(ops);
            return StartOperation(op, default);
        }

        /// <summary>
        /// Create a group operation for a set of locations.
        /// </summary>
        /// <typeparam name="T">The expected object type for the operations.</typeparam>
        /// <param name="locations">The list of locations to load.</param>
        /// <param name="allowFailedDependencies">The operation succeeds if any grouped locations fail.</param>
        /// <returns>The operation for the entire group.</returns>
        internal AsyncOperationHandle<IList<AsyncOperationHandle>> CreateGroupOperation<T>(IList<IResourceLocation> locations, bool allowFailedDependencies)
        {
            var op = CreateOperation<GroupOperation>(typeof(GroupOperation), s_GroupOperationTypeHash, null, m_ReleaseOpNonCached);
            var ops = new List<AsyncOperationHandle>(locations.Count);
            foreach (var loc in locations)
                ops.Add(ProvideResource<T>(loc));

            GroupOperation.GroupOperationSettings settings = GroupOperation.GroupOperationSettings.None;
            if (allowFailedDependencies)
                settings |= GroupOperation.GroupOperationSettings.AllowFailedDependencies;
            op.Init(ops, settings);
            return StartOperation(op, default);
        }

        /// <summary>
        /// Create a group operation for a set of AsyncOperationHandles
        /// </summary>
        /// <param name="operations">The list of operations that need to complete.</param>
        /// <param name="releasedCachedOpOnComplete">Determine if the cached operation should be released or not.</param>
        /// <returns>The operation for the entire group</returns>
        public AsyncOperationHandle<IList<AsyncOperationHandle>> CreateGenericGroupOperation(List<AsyncOperationHandle> operations, bool releasedCachedOpOnComplete = false)
        {
            var op = CreateOperation<GroupOperation>(typeof(GroupOperation), s_GroupOperationTypeHash, new AsyncOpHandlesCacheKey(operations), releasedCachedOpOnComplete ? m_ReleaseOpCached : m_ReleaseOpNonCached);
            op.Init(operations);
            return StartOperation(op, default);
        }

        internal AsyncOperationHandle<IList<AsyncOperationHandle>> ProvideResourceGroupCached(
            IList<IResourceLocation> locations, int groupHash, Type desiredType, Action<AsyncOperationHandle> callback, bool releaseDependenciesOnFailure = true)
        {
            var depsKey = new DependenciesCacheKey(locations, groupHash);
            GroupOperation op = AcquireGroupOpFromCache(depsKey);
            AsyncOperationHandle<IList<AsyncOperationHandle>> handle;
            if (op == null)
            {
                op = CreateOperation<GroupOperation>(typeof(GroupOperation), s_GroupOperationTypeHash, depsKey, m_ReleaseOpCached);
                var ops = new List<AsyncOperationHandle>(locations.Count);
                foreach (var loc in locations)
                    ops.Add(ProvideResource(loc, desiredType, releaseDependenciesOnFailure));

                op.Init(ops, releaseDependenciesOnFailure);

                handle = StartOperation(op, default(AsyncOperationHandle));
            }
            else
            {
                handle = op.Handle;
            }

            if (callback != null)
            {
                var depOps = op.GetDependentOps();
                for (int i = 0; i < depOps.Count; i++)
                {
                    depOps[i].Completed += callback;
                }
            }

            return handle;
        }

        /// <summary>
        /// Asynchronously load all objects in the given collection of <paramref name="locations"/>.
        /// If any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// </summary>
        /// <returns>An async operation that will complete when all individual async load operations are complete.</returns>
        /// <param name="locations">locations to load.</param>
        /// <param name="callback">This callback will be invoked once for each object that is loaded.</param>
        /// <typeparam name="TObject">Object type to load.</typeparam>
        public AsyncOperationHandle<IList<TObject>> ProvideResources<TObject>(IList<IResourceLocation> locations, Action<TObject> callback = null)
        {
            return ProvideResources(locations, true, callback);
        }

        /// <summary>
        /// Asynchronously load all objects in the given collection of <paramref name="locations"/>.
        /// </summary>
        /// <returns>An async operation that will complete when all individual async load operations are complete.</returns>
        /// <param name="locations">locations to load.</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        /// When true, if any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// When false, if any matching location fails, the returned .Result will be an IList of size equal to the number of locations attempted.  Any failed location will
        /// correlate to a null in the IList, while successful loads will correlate to a TObject in the list. The .Status will still be Failed.
        /// When true, op does not need to be released if anything fails, when false, it must always be released.
        /// </param>
        /// <param name="callback">This callback will be invoked once for each object that is loaded.</param>
        /// <typeparam name="TObject">Object type to load.</typeparam>
        public AsyncOperationHandle<IList<TObject>> ProvideResources<TObject>(IList<IResourceLocation> locations, bool releaseDependenciesOnFailure, Action<TObject> callback = null)
        {
            if (locations == null)
                return CreateCompletedOperation<IList<TObject>>(null, "Null Location");

            Action<AsyncOperationHandle> callbackGeneric = null;
            if (callback != null)
            {
                callbackGeneric = (x) => callback((TObject)(x.Result));
            }
            var typelessHandle = ProvideResourceGroupCached(locations, CalculateLocationsHash(locations, typeof(TObject)), typeof(TObject), callbackGeneric, releaseDependenciesOnFailure);
            var chainOp = CreateChainOperation<IList<TObject>>(typelessHandle, (resultHandle) =>
            {
                AsyncOperationHandle<IList<AsyncOperationHandle>> handleToHandles = resultHandle.Convert<IList<AsyncOperationHandle>>();

                var list = new List<TObject>();
                Exception exception = null;
                if (handleToHandles.Status == AsyncOperationStatus.Succeeded)
                {
                    foreach (var r in handleToHandles.Result)
                        list.Add(r.Convert<TObject>().Result);
                }
                else
                {
                    bool foundSuccess = false;
                    if (!releaseDependenciesOnFailure)
                    {
                        foreach (AsyncOperationHandle handle in handleToHandles.Result)
                        {
                            if (handle.Status == AsyncOperationStatus.Succeeded)
                            {
                                list.Add(handle.Convert<TObject>().Result);
                                foundSuccess = true;
                            }
                            else
                                list.Add(default(TObject));
                        }
                    }

                    if (!foundSuccess)
                    {
                        list = null;
                        exception = new ResourceManagerException("ProvideResources failed", handleToHandles.OperationException);
                    }
                    else
                    {
                        exception = new ResourceManagerException("Partial success in ProvideResources.  Some items failed to load. See earlier logs for more info.", handleToHandles.OperationException);
                    }
                }

                return CreateCompletedOperationInternal<IList<TObject>>(list, exception == null, exception, releaseDependenciesOnFailure);
            }, releaseDependenciesOnFailure);

            // chain operation holds the dependency
            typelessHandle.Release();
            return chainOp;
        }

        /// <summary>
        /// Create a chain operation to handle dependencies.
        /// </summary>
        /// <typeparam name="TObject">The type of operation handle to return.</typeparam>
        /// <typeparam name="TObjectDependency">The type of the dependency operation.</typeparam>
        /// <param name="dependentOp">The dependency operation.</param>
        /// <param name="callback">The callback method that will create the dependent operation from the dependency operation.</param>
        /// <returns>The operation handle.</returns>
        public AsyncOperationHandle<TObject> CreateChainOperation<TObject, TObjectDependency>(AsyncOperationHandle<TObjectDependency> dependentOp, Func<AsyncOperationHandle<TObjectDependency>, AsyncOperationHandle<TObject>> callback)
        {
            var op = CreateOperation<ChainOperation<TObject, TObjectDependency>>(typeof(ChainOperation<TObject, TObjectDependency>), typeof(ChainOperation<TObject, TObjectDependency>).GetHashCode(), null, null);
            op.Init(dependentOp, callback, true);
            return StartOperation(op, dependentOp);
        }

        /// <summary>
        /// Create a chain operation to handle dependencies.
        /// </summary>
        /// <typeparam name="TObject">The type of operation handle to return.</typeparam>
        /// <param name="dependentOp">The dependency operation.</param>
        /// <param name="callback">The callback method that will create the dependent operation from the dependency operation.</param>
        /// <returns>The operation handle.</returns>
        public AsyncOperationHandle<TObject> CreateChainOperation<TObject>(AsyncOperationHandle dependentOp, Func<AsyncOperationHandle, AsyncOperationHandle<TObject>> callback)
        {
            var cOp = new ChainOperationTypelessDepedency<TObject>();
            cOp.Init(dependentOp, callback, true);
            return StartOperation(cOp, dependentOp);
        }

        /// <summary>
        /// Create a chain operation to handle dependencies.
        /// </summary>
        /// <typeparam name="TObject">The type of operation handle to return.</typeparam>
        /// <typeparam name="TObjectDependency">The type of the dependency operation.</typeparam>
        /// <param name="dependentOp">The dependency operation.</param>
        /// <param name="callback">The callback method that will create the dependent operation from the dependency operation.</param>
        /// <param name="releaseDependenciesOnFailure"> Whether to release dependencies if the created operation has failed.</param>
        /// <returns>The operation handle.</returns>
        public AsyncOperationHandle<TObject> CreateChainOperation<TObject, TObjectDependency>(AsyncOperationHandle<TObjectDependency> dependentOp, Func<AsyncOperationHandle<TObjectDependency>, AsyncOperationHandle<TObject>> callback, bool releaseDependenciesOnFailure = true)
        {
            var op = CreateOperation<ChainOperation<TObject, TObjectDependency>>(typeof(ChainOperation<TObject, TObjectDependency>), typeof(ChainOperation<TObject, TObjectDependency>).GetHashCode(), null, null);
            op.Init(dependentOp, callback, releaseDependenciesOnFailure);
            return StartOperation(op, dependentOp);
        }

        /// <summary>
        /// Create a chain operation to handle dependencies.
        /// </summary>
        /// <typeparam name="TObject">The type of operation handle to return.</typeparam>
        /// <param name="dependentOp">The dependency operation.</param>
        /// <param name="callback">The callback method that will create the dependent operation from the dependency operation.</param>
        /// <param name="releaseDependenciesOnFailure"> Whether to release dependencies if the created operation has failed.</param>
        /// <returns>The operation handle.</returns>
        public AsyncOperationHandle<TObject> CreateChainOperation<TObject>(AsyncOperationHandle dependentOp, Func<AsyncOperationHandle, AsyncOperationHandle<TObject>> callback, bool releaseDependenciesOnFailure = true)
        {
            var cOp = new ChainOperationTypelessDepedency<TObject>();
            cOp.Init(dependentOp, callback, releaseDependenciesOnFailure);
            return StartOperation(cOp, dependentOp);
        }

        internal class InstanceOperation : AsyncOperationBase<GameObject>
        {
            AsyncOperationHandle<GameObject> m_dependency;
            InstantiationParameters m_instantiationParams;
            IInstanceProvider m_instanceProvider;
            GameObject m_instance;
            Scene m_scene;

            public void Init(ResourceManager rm, IInstanceProvider instanceProvider, InstantiationParameters instantiationParams, AsyncOperationHandle<GameObject> dependency)
            {
                m_RM = rm;
                m_dependency = dependency;
                m_instanceProvider = instanceProvider;
                m_instantiationParams = instantiationParams;
                m_scene = default(Scene);
            }

            internal override DownloadStatus GetDownloadStatus(HashSet<object> visited)
            {
                return m_dependency.IsValid() ? m_dependency.InternalGetDownloadStatus(visited) : new DownloadStatus() { IsDone = IsDone };
            }

            /// <inheritdoc />
            public override void GetDependencies(List<AsyncOperationHandle> deps)
            {
                deps.Add(m_dependency);
            }

            protected override string DebugName
            {
                get
                {
                    if (m_instanceProvider == null)
                        return "Instance<Invalid>";
                    return string.Format("Instance<{0}>({1}", m_instanceProvider.GetType().Name, m_dependency.IsValid() ? m_dependency.DebugName : "Invalid");
                }
            }

            public Scene InstanceScene() => m_scene;

            protected override void Destroy()
            {
                m_instanceProvider.ReleaseInstance(m_RM, m_instance);
            }

            protected override float Progress
            {
                get
                {
                    return m_dependency.PercentComplete;
                }
            }

            ///<inheritdoc />
            protected  override bool InvokeWaitForCompletion()
            {
                if (m_dependency.IsValid() && !m_dependency.IsDone)
                    m_dependency.WaitForCompletion();

                m_RM?.Update(Time.unscaledDeltaTime);
                if (m_instance == null && !HasExecuted)
                    InvokeExecute();

                return IsDone;
            }

            protected override void Execute()
            {
                Exception e = m_dependency.OperationException;
                if (m_dependency.Status == AsyncOperationStatus.Succeeded)
                {
                    m_instance = m_instanceProvider.ProvideInstance(m_RM, m_dependency, m_instantiationParams);
                    if (m_instance != null)
                        m_scene = m_instance.scene;
                    Complete(m_instance, true, null);
                }
                else
                {
                    Complete(m_instance, false, string.Format("Dependency operation failed with {0}.", e));
                }
            }
        }


        /// <summary>
        /// Load a scene at a specificed resource location.
        /// </summary>
        /// <param name="sceneProvider">The scene provider instance.</param>
        /// <param name="location">The location of the scene.</param>
        /// <param name="loadMode">The load mode for the scene.</param>
        /// <param name="activateOnLoad">If false, the scene will be loaded in the background and not activated when complete.</param>
        /// <param name="priority">The priority for the load operation.</param>
        /// <returns>Async operation handle that will complete when the scene is loaded.  If activateOnLoad is false, then Activate() will need to be called on the SceneInstance returned.</returns>
        public AsyncOperationHandle<SceneInstance> ProvideScene(ISceneProvider sceneProvider, IResourceLocation location, LoadSceneMode loadMode, bool activateOnLoad, int priority)
        {
            if (sceneProvider == null)
                throw new NullReferenceException("sceneProvider is null");

            return sceneProvider.ProvideScene(this, location, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Release a scene.
        /// </summary>
        /// <param name="sceneProvider">The scene provider.</param>
        /// <param name="sceneLoadHandle">The operation handle used to load the scene.</param>
        /// <returns>An operation handle for the unload.</returns>
        public AsyncOperationHandle<SceneInstance> ReleaseScene(ISceneProvider sceneProvider, AsyncOperationHandle<SceneInstance> sceneLoadHandle)
        {
            if (sceneProvider == null)
                throw new NullReferenceException("sceneProvider is null");
            //           if (sceneLoadHandle.ReferenceCount == 0)
            //               return CreateCompletedOperation<SceneInstance>(default(SceneInstance), "");
            return sceneProvider.ReleaseScene(this, sceneLoadHandle);
        }

        /// <summary>
        /// Asynchronously instantiate a prefab (GameObject) at the specified <paramref name="location"/>.
        /// </summary>
        /// <returns>Async operation that will complete when the prefab is instantiated.</returns>
        /// <param name="provider">An implementation of IInstanceProvider that will be used to instantiate and destroy the GameObject.</param>
        /// <param name="location">Location of the prefab.</param>
        /// <param name="instantiateParameters">A struct containing the parameters to pass the the Instantiation call.</param>
        public AsyncOperationHandle<GameObject> ProvideInstance(IInstanceProvider provider, IResourceLocation location, InstantiationParameters instantiateParameters)
        {
            if (provider == null)
                throw new NullReferenceException("provider is null.  Assign a valid IInstanceProvider object before using.");

            if (location == null)
                throw new ArgumentNullException("location");

            var depOp = ProvideResource<GameObject>(location);
            var baseOp = CreateOperation<InstanceOperation>(typeof(InstanceOperation), s_InstanceOperationTypeHash, null, m_ReleaseInstanceOp);
            baseOp.Init(this, provider, instantiateParameters, depOp);
            m_TrackedInstanceOperations.Add(baseOp);
            return StartOperation<GameObject>(baseOp, depOp);
        }

        /// <summary>
        /// Releases all instances the given scence.
        /// </summary>
        /// <param name="scene">The scene whose instances should be released.</param>
        public void CleanupSceneInstances(Scene scene)
        {
            List<InstanceOperation> handlesToRelease = null;
            foreach (var h in m_TrackedInstanceOperations)
            {
                if (h.Result == null && scene == h.InstanceScene())
                {
                    if (handlesToRelease == null)
                        handlesToRelease = new List<InstanceOperation>();
                    handlesToRelease.Add(h);
                }
            }
            if (handlesToRelease != null)
            {
                foreach (var h in handlesToRelease)
                {
                    m_TrackedInstanceOperations.Remove(h);
                    h.DecrementReferenceCount();
                }
            }
        }

        private void ExecuteDeferredCallbacks()
        {
            m_InsideExecuteDeferredCallbacksMethod = true;
            for (int i = 0; i < m_DeferredCompleteCallbacks.Count; i++)
            {
                m_DeferredCompleteCallbacks[i].InvokeCompletionEvent();
                m_DeferredCompleteCallbacks[i].DecrementReferenceCount();
            }
            m_DeferredCompleteCallbacks.Clear();
            m_InsideExecuteDeferredCallbacksMethod = false;
        }

        internal void RegisterForDeferredCallback(IAsyncOperation op, bool incrementRefCount = true)
        {
            if (CallbackHooksEnabled && m_InsideExecuteDeferredCallbacksMethod)
            {
                if (m_DeferredCallbacksToRegister == null)
                    m_DeferredCallbacksToRegister = new List<DeferredCallbackRegisterRequest>();
                m_DeferredCallbacksToRegister.Add
                    (
                        new DeferredCallbackRegisterRequest()
                        {
                            operation = op,
                            incrementRefCount = incrementRefCount
                        }
                    );
            }
            else
            {
                if (incrementRefCount)
                    op.IncrementReferenceCount();
                m_DeferredCompleteCallbacks.Add(op);
                RegisterForCallbacks();
            }
        }

        internal void Update(float unscaledDeltaTime)
        {
            if (m_InsideUpdateMethod)
                throw new Exception("Reentering the Update method is not allowed.  This can happen when calling WaitForCompletion on an operation while inside of a callback.");
            m_InsideUpdateMethod = true;
            m_UpdateCallbacks.Invoke(unscaledDeltaTime);
            m_UpdatingReceivers = true;
            for (int i = 0; i < m_UpdateReceivers.Count; i++)
                m_UpdateReceivers[i].Update(unscaledDeltaTime);
            m_UpdatingReceivers = false;
            if (m_UpdateReceiversToRemove != null)
            {
                foreach (var r in m_UpdateReceiversToRemove)
                    m_UpdateReceivers.Remove(r);
                m_UpdateReceiversToRemove = null;
            }
            if (m_DeferredCallbacksToRegister != null)
            {
                foreach (DeferredCallbackRegisterRequest callback in m_DeferredCallbacksToRegister)
                    RegisterForDeferredCallback(callback.operation, callback.incrementRefCount);
                m_DeferredCallbacksToRegister = null;
            }
            ExecuteDeferredCallbacks();
            m_InsideUpdateMethod = false;
        }

        /// <summary>
        /// Disposes internal resources used by the resource manager
        /// </summary>
        public void Dispose()
        {
            if (MonoBehaviourCallbackHooks.Exists && m_RegisteredForCallbacks)
            {
                MonoBehaviourCallbackHooks.Instance.OnUpdateDelegate -= Update;
                m_RegisteredForCallbacks = false;
            }
        }
    }
}
