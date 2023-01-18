#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.ResourceProviders
{
    /// <summary>
    /// Provides assets loaded via the AssetDatabase API.  This provider is only available in the editor and is used for fast iteration or to simulate asset bundles when in play mode.
    /// </summary>
    [DisplayName("Assets from AssetDatabase Provider")]
    public class AssetDatabaseProvider : ResourceProviderBase
    {
        float m_LoadDelay = .1f;

        private static Object[] LoadAllAssetRepresentationsAtPath(string assetPath)
        {
            return AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        }

        internal static Object LoadAssetSubObject(string assetPath, string subObjectName, Type type)
        {
            var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            foreach (var o in objs)
            {
                if (o.name == subObjectName)
                {
                    if (type.IsAssignableFrom(o.GetType()))
                        return o;
                }
            }
            return null;
        }

        private static Object LoadMainAssetAtPath(string assetPath)
        {
            return AssetDatabase.LoadMainAssetAtPath(assetPath);
        }

        internal static object LoadAssetAtPath(string assetPath, ProvideHandle provideHandle)
        {
            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, provideHandle.Location.ResourceType);
            obj = obj != null && provideHandle.Type.IsAssignableFrom(obj.GetType()) ? obj : null;
            return obj;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AssetDatabaseProvider() {}

        /// <summary>
        /// Constructor that allows for a sepcified delay for all requests.
        /// </summary>
        /// <param name="delay">Time in seconds for each delay call.</param>
        public AssetDatabaseProvider(float delay = .25f)
        {
            m_LoadDelay = delay;
        }

        internal static Object[] LoadAssetsWithSubAssets(string assetPath)
        {
            var subObjects = LoadAllAssetRepresentationsAtPath(assetPath);
            var allObjects = new Object[subObjects.Length + 1];
            allObjects[0] = LoadMainAssetAtPath(assetPath);
            for (int i = 0; i < subObjects.Length; i++)
                allObjects[i + 1] = subObjects[i];
            return allObjects;
        }

        class InternalOp
        {
            ProvideHandle m_ProvideHandle;
            bool m_Loaded;
            public void Start(ProvideHandle provideHandle, float loadDelay)
            {
                m_Loaded = false;
                m_ProvideHandle = provideHandle;
                m_ProvideHandle.SetWaitForCompletionCallback(WaitForCompletionHandler);
                if (loadDelay < 0)
                    LoadImmediate();
                else
                    DelayedActionManager.AddAction((Action)LoadImmediate, loadDelay);
            }

            private bool WaitForCompletionHandler()
            {
                LoadImmediate();
                return true;
            }

            void LoadImmediate()
            {
                if (m_Loaded)
                    return;
                m_Loaded = true;
                string assetPath = m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location);
                object result = null;
                if (m_ProvideHandle.Type.IsArray)
                    result = ResourceManagerConfig.CreateArrayResult(m_ProvideHandle.Type, LoadAssetsWithSubAssets(assetPath));
                else if (m_ProvideHandle.Type.IsGenericType && typeof(IList<>) == m_ProvideHandle.Type.GetGenericTypeDefinition())
                    result = ResourceManagerConfig.CreateListResult(m_ProvideHandle.Type, LoadAssetsWithSubAssets(assetPath));
                else
                {
                    if (ResourceManagerConfig.ExtractKeyAndSubKey(assetPath, out string mainPath, out string subKey))
                        result = LoadAssetSubObject(mainPath, subKey, m_ProvideHandle.Type);
                    else
                        result = LoadAssetAtPath(assetPath, m_ProvideHandle);
                }
                m_ProvideHandle.Complete(result, result != null, result == null ? new Exception($"Unable to load asset of type {m_ProvideHandle.Type} from location {m_ProvideHandle.Location}.") : null);
            }
        }

        /// <inheritdoc/>
        public override bool CanProvide(Type t, IResourceLocation location)
        {
            return base.CanProvide(t, location);
        }

        public override void Provide(ProvideHandle provideHandle)
        {
            new InternalOp().Start(provideHandle, m_LoadDelay);
        }
    }
}
#endif
