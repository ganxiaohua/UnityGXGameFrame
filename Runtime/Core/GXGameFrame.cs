using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame : SingletonMono<GXGameFrame>
    {
        internal static bool IsQuitting { get; private set; }

        public RootEntity RootEntity { get; private set; }
        

        public void Init()
        {
            IsQuitting = false;
            TimerSystem.Instance.Clear();
            InexactCountdownSystem.Instance.Clear();
            RootEntity = ReferencePool.Acquire<RootEntity>();
            RootEntity.OnDirty(null, 0);
            RootEntity.AddComponent<UIRootComponents>();
            RootEntity.AddComponent<FsmComponents>();
            InitializeUISystem();
        }

        public void Update()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            AssetManager.Instance.Update(datetime);
            EntityHouse.Instance.Update(datetime, realtimeSinceStartup);
            ObjectPoolManager.Instance.Update(datetime, realtimeSinceStartup);
            TimerSystem.Instance.Update(realtimeSinceStartup);
            InexactCountdownSystem.Instance.Update(realtimeSinceStartup);
            UpdateUISystem();
            ReferencePool.Update(datetime, realtimeSinceStartup);
        }

        static partial void InitializeUISystem();
        static partial void UpdateUISystem();
        static partial void DisposeUISystem();

        public void LateUpdate()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            EntityHouse.Instance.LateUpdate(datetime, realtimeSinceStartup);
        }

        public void FixedUpdate()
        {
            float datetime = Time.fixedDeltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            EntityHouse.Instance.FixedUpdate(datetime, realtimeSinceStartup);
        }

        public void OnApplicationQuit()
        {
            IsQuitting = true;
            DisposeUISystem();
            ReferencePool.Release(RootEntity);
            TimerSystem.Instance.Clear();
            InexactCountdownSystem.Instance.Clear();
            //必须最后处理
            YooAssets.Destroy();
            // EntityHouse.Instance.Disable();
            // ObjectPoolManager.Instance.Disable();
            // AssetManager.Instance.Dispose();
        }
    }
}
