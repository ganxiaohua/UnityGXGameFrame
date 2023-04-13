using System;
using UnityEngine;

namespace GameFrame
{
    class MainScene : IScene
    {
        public void Start(SceneEntity sceneEntity)
        {
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Clear()
        {
        }
    }

    public class GXGameFrame : Singleton<GXGameFrame>
    {
        public SceneEntity MainScene { get; private set; }


        public void Start()
        {
            AutoBindSystem.Instance.AddSystem();
            EnitityHouse.Instance.Init();
            MainScene = ReferencePool.Acquire<SceneEntity>();
            EnitityHouse.Instance.AddSceneEntity<MainScene>(MainScene);
            MainScene.AddComponent<UIComponent>();
        }

        public void Update()
        {
            float datetime = Time.deltaTime;
            float realtimeSinceStartup = Time.realtimeSinceStartup;
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
            // EnitityHouse.Instance.RemoveEntity(MainScene);
            ReferencePool.Release(MainScene);
            ObjectPoolManager.Instance.DeleteAll();
            EnitityHouse.Instance.Disable();
        }
    }
}