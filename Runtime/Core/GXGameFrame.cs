using UnityEngine;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame : SingletonMono<GXGameFrame>
    {
        public RootEntity RootEntity { get; private set; }

        public void Init()
        {
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
            DisposeUISystem();
            ReferencePool.Release(RootEntity);
            // EntityHouse.Instance.Disable();
            // ObjectPoolManager.Instance.Disable();
            // AssetManager.Instance.Dispose();
        }
    }
}
