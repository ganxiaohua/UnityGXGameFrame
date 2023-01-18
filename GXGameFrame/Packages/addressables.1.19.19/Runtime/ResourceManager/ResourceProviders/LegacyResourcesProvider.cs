using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.ResourceProviders
{
    /// <summary>
    /// Provides assets loaded via Resources.LoadAsync API.
    /// </summary>
    [DisplayName("Assets from Legacy Resources")]
    public class LegacyResourcesProvider : ResourceProviderBase
    {
        internal class InternalOp
        {
            ResourceRequest m_RequestOperation;
            ProvideHandle m_ProvideHandle;

            public void Start(ProvideHandle provideHandle)
            {
                m_ProvideHandle = provideHandle;

                provideHandle.SetProgressCallback(PercentComplete);
                provideHandle.SetWaitForCompletionCallback(WaitForCompletionHandler);
                m_RequestOperation = Resources.LoadAsync(m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location), m_ProvideHandle.Type);
                m_RequestOperation.completed += AsyncOperationCompleted;
            }

            private bool WaitForCompletionHandler()
            {
                return m_RequestOperation != null && m_RequestOperation.isDone;
            }

            private void AsyncOperationCompleted(AsyncOperation op)
            {
                var request = op as ResourceRequest;
                object result = request != null ? request.asset : null;
                result = result != null && m_ProvideHandle.Type.IsAssignableFrom(result.GetType()) ? result : null;
                m_ProvideHandle.Complete(result, result != null, result == null ? new Exception($"Unable to load asset of type {m_ProvideHandle.Type} from location {m_ProvideHandle.Location}.") : null);
            }

            public float PercentComplete() { return m_RequestOperation != null ? m_RequestOperation.progress : 0.0f; }
        }

        /// <inheritdoc/>
        public override void Provide(ProvideHandle pi)
        {
            Type t = pi.Type;
            bool isList = t.IsGenericType && typeof(IList<>) == t.GetGenericTypeDefinition();
            var internalId = pi.ResourceManager.TransformInternalId(pi.Location);
            if (t.IsArray || isList)
            {
                object result = null;
                if (t.IsArray)
                    result = ResourceManagerConfig.CreateArrayResult(t, Resources.LoadAll(internalId, t.GetElementType()));
                else
                    result = ResourceManagerConfig.CreateListResult(t, Resources.LoadAll(internalId, t.GetGenericArguments()[0]));

                pi.Complete(result, result != null, result == null ? new Exception($"Unable to load asset of type {pi.Type} from location {pi.Location}.") : null);
            }
            else
            {
                if (ResourceManagerConfig.ExtractKeyAndSubKey(internalId, out string mainPath, out string subKey))
                {
                    var objs = Resources.LoadAll(mainPath, pi.Type);
                    object result = null;
                    foreach (var o in objs)
                    {
                        if (o.name == subKey)
                        {
                            if (pi.Type.IsAssignableFrom(o.GetType()))
                            {
                                result = o;
                                break;
                            }
                        }
                    }
                    pi.Complete(result, result != null, result == null ? new Exception($"Unable to load asset of type {pi.Type} from location {pi.Location}.") : null);
                }
                else
                {
                    new InternalOp().Start(pi);
                }
            }
        }

        /// <inheritdoc/>
        public override void Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            var obj = asset as Object;
            //GameObjects cannot be resleased via Object.Destroy because they are considered an asset
            //but they can't be unloaded via Resources.UnloadAsset since they are NOT an asset?
            if (obj != null && !(obj is GameObject))
                Resources.UnloadAsset(obj);
        }
    }
}
