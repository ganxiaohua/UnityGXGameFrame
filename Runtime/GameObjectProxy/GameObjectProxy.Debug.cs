#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime
{
    public partial class GameObjectProxy
    {
        [HideMonoScript]
        public sealed class DebugInspector : MonoBehaviour
        {
            [ShowInInspector] private int Version => proxy.Versions;
            [ShowInInspector] private GameObjectState State => proxy.State;
            [ShowInInspector] private string asset { get; set; }
            [ShowInInspector] private GameObject prefab { get; set; }

            private GameObjectProxy proxy;

            public static void Track(GameObjectProxy proxy)
            {
                using (new Profiler("[EditorOnly]GameObjectProxy.Track"))
                {
                    if (!proxy.gameObject.TryGetComponent<DebugInspector>(out var debug))
                        debug = proxy.gameObject.AddComponent<DebugInspector>();
                    debug.proxy = proxy;
                    debug.asset = proxy.BindingSource as string;
                    debug.prefab = proxy.BindingSource as GameObject;
                    if (debug.asset != null)
                    {
                        proxy.gameObject.name = debug.asset;
                    }
                    else if (debug.prefab != null)
                    {
                        proxy.gameObject.name = debug.prefab.name;
                    }
                }
            }

            [Button("Load"), ButtonGroup]
            private void Load()
            {
                if (!string.IsNullOrEmpty(asset))
                    proxy.BindFromAssetAsync(asset, proxy.transform.parent).Forget();
                else if (prefab)
                    proxy.BindFromPrefab(prefab, proxy.transform.parent);
            }

            [Button("Unload"), ButtonGroup]
            private void Unload()
            {
                proxy.Unbind();
            }
        }
    }
}
#endif