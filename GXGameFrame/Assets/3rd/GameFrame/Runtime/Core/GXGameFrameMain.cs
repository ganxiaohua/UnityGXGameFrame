using System;
using UnityEngine;

namespace GameFrame
{
    public class GXGameFrameMain:SingletonMono<GXGameFrameMain>
    {
        public void Start()
        {
            EnitityHouse.Instance.Init();
        }

        public void Update()
        {
            EnitityHouse.Instance.Update();
            ObjectPoolManager.Instance.Update(Time.deltaTime,Time.realtimeSinceStartup);
        }

        public void LateUpdate()
        {
            
        }

        public void OnDisable()
        {
            EnitityHouse.Instance.Disable();
        }
    }
}