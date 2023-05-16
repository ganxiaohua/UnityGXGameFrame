using System;
using cfg;
using Cysharp.Threading.Tasks;
using GXGame;
using UnityEngine;

namespace GameFrame
{
    public class GXGameFrame : Singleton<GXGameFrame>
    {
        public MainScene MainScene { get; private set; }
        public CheckUpdate CheckUpdate;

        public async UniTask Start()
        {
            AutoBindSystem.Instance.AddSystem();
            EnitityHouse.Instance.Init();
            MainScene = ReferencePool.Acquire<MainScene>();
            MainScene.AddComponent<UIComponent>();
            MainScene.AddComponent<GameObjectPoolComponent>();
            await MainScene.AddComponent<AssetInitComponent>().WaitLoad();
            Config.Instance.LoadTable();
        }

        public void Update()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.unscaledDeltaTime;
            EnitityHouse.Instance.Update(datetime, realtimeSinceStartup);
            ObjectPoolManager.Instance.Update(datetime, realtimeSinceStartup);
            UIManager.Instance.Update(datetime, realtimeSinceStartup);
            ReferencePool.Update(datetime, realtimeSinceStartup);
            if (CheckUpdate != null)
            {
                CheckUpdate.Update();
            }
        }

        public void LateUpdate()
        {
        }

        public void OnDisable()
        {
            ReferencePool.Release(MainScene);
            ObjectPoolManager.Instance.DeleteAll();
            EnitityHouse.Instance.Disable();
        }
    }
}