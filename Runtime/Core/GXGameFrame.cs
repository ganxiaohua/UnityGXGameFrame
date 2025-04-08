using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public class GXGameFrame : Singleton<GXGameFrame>
    {
        public RootEntity RootEntity { get; private set; }

        public async UniTask Start()
        {
            RootEntity = ReferencePool.Acquire<RootEntity>();
            RootEntity.OnDirty(null, 0);
            RootEntity.AddComponent<UIComponent>();
            var assetsFsmController = await RootEntity.AddComponent<AssetsFsmController>();
            if (assetsFsmController.TaskState != TaskState.Succ)
            {
                Debugger.LogError("资源流程失败");
            }

            RootEntity.RemoveComponent<AssetsFsmController>();
        }

        public void Update()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            AssetManager.Instance.Update(datetime);
            EntityHouse.Instance.Update(datetime, realtimeSinceStartup);
            ObjectPoolManager.Instance.Update(datetime, realtimeSinceStartup);
            UIManager.Instance.Update(datetime, realtimeSinceStartup);
            ReferencePool.Update(datetime, realtimeSinceStartup);
        }

        public void LateUpdate()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            EntityHouse.Instance.LateUpdate(datetime, realtimeSinceStartup);
        }

        public void FixedUpdate()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            EntityHouse.Instance.FixedUpdate(datetime, realtimeSinceStartup);
        }

        public void OnDisable()
        {
            ReferencePool.Release(RootEntity);
            UIManager.Instance.Disable();
            EntityHouse.Instance.Disable();
            ObjectPoolManager.Instance.Disable();
        }
    }
}