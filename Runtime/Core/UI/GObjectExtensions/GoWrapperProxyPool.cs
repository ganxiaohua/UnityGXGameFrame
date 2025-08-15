using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

//重写
namespace GameFrame.Runtime
{
    public static class GoWrapperProxyPool
    {
        private const int MaxPoolSize = 64;

        private static readonly Queue<GoWrapperProxy> queue = new();

        public static GoWrapperProxy Get()
        {
            if (queue.TryDequeue(out var proxy))
            {
                return proxy;
            }

            return new GoWrapperProxy(new GNativeObject());
        }

        public static void Release(GoWrapperProxy proxy)
        {
            // GoWrapper was disposed?
            if (proxy.Wrapper.isDisposed)
                return;
            Assert.IsFalse(queue.Contains(proxy), "Repeat enqueue occurred");
            if (queue.Count >= MaxPoolSize)
            {
                proxy.Dispose();
            }
            else
            {
                proxy.OnBeforeRecycle();
                proxy.Unbind();
                proxy.onBeforeUnbind = null;
                proxy.onAfterBind = null;
                proxy.Userdata = null;

                var root = Stage.inst.cachedTransform;
#if UNITY_EDITOR
                using (new Profiler("[EditorOnly]GoWrapperProxyPool.SearchRoot"))
                {
                    var debugRootKey = $"GoWrapperProxyPool";
                    var debugRoot = root.Find(debugRootKey);
                    if (!debugRoot)
                    {
                        debugRoot = new GameObject(debugRootKey).transform;
                        debugRoot.SetParent(root);
                    }

                    root = debugRoot;
                }
#endif
                proxy.SetParent(null);
                proxy.transform.SetParent(root, false);
                proxy.Visible = false;
                proxy.scale = Vector2.one;
                proxy.rotation = 0f;
                proxy.SortingOrder = 0; // reset sorting order to zero
                queue.Enqueue(proxy);
            }
        }

        public static void Clear()
        {
            while (queue.TryDequeue(out var proxy))
            {
                proxy.Dispose();
            }
        }
    }
}