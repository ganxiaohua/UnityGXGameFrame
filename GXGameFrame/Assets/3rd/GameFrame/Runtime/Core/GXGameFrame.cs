using Cysharp.Threading.Tasks;
using UnityEngine;
// using cfg;
// using GXGame;

namespace GameFrame
{
    public class GXGameFrame : Singleton<GXGameFrame>
    {
        public MainScene MainScene { get; private set; }

        public async UniTask Start()
        {
            MainScene = ReferencePool.Acquire<MainScene>();
            MainScene.AddComponent<UIComponent>();
            await MainScene.AddComponent<AssetInitComponent>().WaitLoad();
            Config.Instance.LoadTable();
        }

        public void Update()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            AssetSystem.Instance.Update(datetime);
            EnitityHouse.Instance.Update(datetime, realtimeSinceStartup);
            ObjectPoolManager.Instance.Update(datetime, realtimeSinceStartup);
            UIManager.Instance.Update(datetime, realtimeSinceStartup);
            ReferencePool.Update(datetime, realtimeSinceStartup);
        }

        public void LateUpdate()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            EnitityHouse.Instance.LateUpdate(datetime, realtimeSinceStartup);
        }

        public void FixedUpdate()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            EnitityHouse.Instance.FixedUpdate(datetime, realtimeSinceStartup);
        }

        public void OnDisable()
        {
            UIManager.Instance.Disable();
            // ReferencePool.Release(MainScene);
            EnitityHouse.Instance.Disable();
            ObjectPoolManager.Instance.Disable();
        }
    }
}