using System;
using cfg;
using UnityEngine;

namespace GameFrame
{
    public class GXGameFrame : Singleton<GXGameFrame>
    {
        public MainScene MainScene { get; private set; }


        public void Start()
        {
            AutoBindSystem.Instance.AddSystem();
            EnitityHouse.Instance.Init();
            Config.Instance.LoadTable();
            MainScene = ReferencePool.Acquire<MainScene>();
            MainScene.AddComponent<UIComponent>();
        }

        public void Update()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.unscaledDeltaTime;
            EnitityHouse.Instance.Update(datetime, realtimeSinceStartup);
            ObjectPoolManager.Instance.Update(datetime, realtimeSinceStartup);
            UIManager.Instance.Update(datetime, realtimeSinceStartup);
            ReferencePool.Update(datetime, realtimeSinceStartup);
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