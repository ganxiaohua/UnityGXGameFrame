using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.ResourceProviders
{
    /// <summary>
    /// Base class for IResourceProvider.
    /// </summary>
    public abstract class ResourceProviderBase : IResourceProvider, IInitializableObject
    {
        /// <summary>
        /// The unique identifier of the provider.
        /// </summary>
        protected string m_ProviderId;
        /// <summary>
        /// The extra behavior of the provider.
        /// </summary>
        protected ProviderBehaviourFlags m_BehaviourFlags = ProviderBehaviourFlags.None;

        /// <inheritdoc/>
        public virtual string ProviderId
        {
            get
            {
                if (string.IsNullOrEmpty(m_ProviderId))
                    m_ProviderId = GetType().FullName;

                return m_ProviderId;
            }
        }

        /// <inheritdoc/>
        public virtual bool Initialize(string id, string data)
        {
            m_ProviderId = id;
            return !string.IsNullOrEmpty(m_ProviderId);
        }

        /// <inheritdoc/>
        public virtual bool CanProvide(Type t, IResourceLocation location)
        {
            return GetDefaultType(location).IsAssignableFrom(t);
        }

        /// <summary>
        /// Converts information about the resource provider to a formatted string.
        /// </summary>
        /// <returns>Returns information about the resource provider.</returns>
        public override string ToString()
        {
            return ProviderId;
        }

        /// <summary>
        /// Release the specified object that was created from the specified location.
        /// </summary>
        /// <param name="location">The location of the object</param>
        /// <param name="obj">The object to release.</param>
        public virtual void Release(IResourceLocation location, object obj)
        {
        }

        /// <summary>
        /// Get the default type of object that this provider can provide.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public virtual Type GetDefaultType(IResourceLocation location)
        {
            return typeof(object);
        }

        /// <summary>
        /// Provide the object specified in the provideHandle.
        /// </summary>
        /// <param name="provideHandle">Contains all data needed to provide the requested object.</param>
        public abstract void Provide(ProvideHandle provideHandle);

        /// <inheritdoc/>
        public virtual AsyncOperationHandle<bool> InitializeAsync(ResourceManager rm, string id, string data)
        {
            BaseInitAsyncOp baseInitOp = new BaseInitAsyncOp();
            baseInitOp.Init(() => Initialize(id, data));
            return rm.StartOperation(baseInitOp, default);
        }

        ProviderBehaviourFlags IResourceProvider.BehaviourFlags { get { return m_BehaviourFlags; } }

        class BaseInitAsyncOp : AsyncOperationBase<bool>
        {
            private Func<bool> m_CallBack;

            public void Init(Func<bool> callback)
            {
                m_CallBack = callback;
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
                if (m_CallBack != null)
                    Complete(m_CallBack(), true, "");
                else
                    Complete(true, true, "");
            }
        }
    }

    /// <summary>
    /// Contains options used in Resource Provider load requests.  ProviderLoadRequestOptions are used to specify
    /// parameters such as whether or not to ignore load failures and UnityWebRequest timeouts.
    /// </summary>
    [Serializable]
    public class ProviderLoadRequestOptions
    {
        [SerializeField] private bool m_IgnoreFailures = false;
        private int m_WebRequestTimeout = 0;

        /// <summary>
        /// Creates a memberwise clone of a given ProviderLoadRequestOption.
        /// </summary>
        /// <returns>The newly created ProviderLoadRequestOption object</returns>
        public ProviderLoadRequestOptions Copy()
        {
            return (ProviderLoadRequestOptions) this.MemberwiseClone();
        }

        /// <summary>
        /// IgnoreFailures for provider load requests
        /// </summary>
        public bool IgnoreFailures
        {
            get { return m_IgnoreFailures; }
            set { m_IgnoreFailures = value; }
        }

        /// <summary>
        /// UnityWebRequest Timeout
        /// </summary>
        public int WebRequestTimeout
        {
            get => m_WebRequestTimeout;
            set => m_WebRequestTimeout = value;
        }
    }
}
