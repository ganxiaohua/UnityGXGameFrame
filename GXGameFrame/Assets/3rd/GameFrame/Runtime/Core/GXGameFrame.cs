using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public class GXGameFrame : Singleton<GXGameFrame>
    {
        public MainScene MainScene { get; private set; }

        public async UniTask Start()
        {
            MainScene = ReferencePool.Acquire<MainScene>();
            MainScene.Initialize(null, null, 0);
            MainScene.AddComponent<UIComponent>();
            await MainScene.AddComponent<AssetInitComponent>().WaitLoad();
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
            ReferencePool.Release(MainScene);
            UIManager.Instance.Disable();
            EntityHouse.Instance.Disable();
            ObjectPoolManager.Instance.Disable();
        }
    }
}