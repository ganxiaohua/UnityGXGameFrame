using System;
using UnityEngine;

namespace GameFrame
{
    struct MainScene : IScene
    {
        
    }

    public class GXGameFrame : Singleton<GXGameFrame>
    {
        public SceneEntity MainScene;

        public void Start()
        {
            EnitityHouse.Instance.Init();
            MainScene = ReferencePool.Acquire<SceneEntity>();
            EnitityHouse.Instance.AddSceneEntity<MainScene>(MainScene);
        }

        public void Update()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            EnitityHouse.Instance.Update(datetime, realtimeSinceStartup);
            ObjectPoolManager.Instance.Update(datetime, realtimeSinceStartup);
            ReferencePool.Update(datetime, realtimeSinceStartup);
        }

        public void LateUpdate()
        {
        }

        public void OnDisable()
        {
            // EnitityHouse.Instance.RemoveEntity(MainScene);
            ReferencePool.Release(MainScene);
            ObjectPoolManager.Instance.DeleteAll();
            EnitityHouse.Instance.Disable();
        }
    }
}