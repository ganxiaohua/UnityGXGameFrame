using System;
using cfg;
using Cysharp.Threading.Tasks;
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
            Config.Instance.LoadTable();
            MainScene = ReferencePool.Acquire<MainScene>();
            MainScene.AddComponent<UIComponent>();
            await AddressablesHelper.InitializeAsync();
            CheckUpdate = new CheckUpdate();
            await CheckUpdate.CheckVersions();
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
            CheckUpdate.Disable();
            ReferencePool.Release(MainScene);
            ObjectPoolManager.Instance.DeleteAll();
            EnitityHouse.Instance.Disable();
        }
    }
}