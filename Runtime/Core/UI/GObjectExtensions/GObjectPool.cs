using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace GameFrame.Runtime
{
    public sealed class GObjectPool
    {
        public const int DefaultSize = 8;

        private readonly string package;
        private readonly string name;
        private readonly int size;
        private readonly bool fromResources;
        private readonly Queue<GObject> queue;
        public bool Initialized { get; private set; }

        public GObjectPool(string package, string name, int size = DefaultSize, bool fromResources = false)
        {
            this.package = package;
            this.name = name;
            this.size = size;
            this.fromResources = fromResources;
            this.queue = new Queue<GObject>(size > DefaultSize ? DefaultSize : size);
        }

        public async UniTask InitializeAsync(CancellationToken cancelToken = default)
        {
            Assert.IsFalse(Initialized, $"GObjectPool<{package}, {name}> is already initialized");
            var go = await UISystem.Instance.CreateGObjectAsync(package, name, fromResources, cancelToken);
            if (go == null)
                return;
            Initialized = true;
            Release(go);
        }

        public void Clear()
        {
            while (queue.TryDequeue(out var item))
            {
                item.Dispose();
            }
        }

        public GObject Get()
        {
            // try initialize synchronized
            if (!Initialized && UIPackageHelper.IsPackageReady(package))
                Initialized = true;

            Assert.IsTrue(Initialized, $"GObjectPool<{package}, {name}> is not initialized");
            if (queue.TryDequeue(out var go))
                return go;
            return UISystem.Instance.CreateGObject(package, name);
        }

        public void Release(GObject go)
        {
            if (go == null || go.isDisposed)
                return;
            Assert.IsTrue(Initialized, $"GObjectPool<{package}, {name}> is not initialized");
            Assert.IsFalse(queue.Contains(go), "Repeat enqueue occurred");
            if (queue.Count >= size)
            {
                go.Dispose();
            }
            else
            {
                var root = Stage.inst.cachedTransform;
#if UNITY_EDITOR
                using (new Profiler("[EditorOnly]GObjectPool.SearchRoot"))
                {
                    var debugRootKey = $"GObjectPool<{package}, {name}>";
                    var debugRoot = root.Find(debugRootKey);
                    if (!debugRoot)
                    {
                        debugRoot = new GameObject(debugRootKey).transform;
                        debugRoot.SetParent(root);
                    }

                    root = debugRoot;
                }
#endif
                go.RemoveFromParent();
                go.displayObject.cachedTransform.SetParent(root, false);
                go.visible = false;
                queue.Enqueue(go);
            }
        }
    }
}