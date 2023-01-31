using System;

namespace GameFrame
{
    public class GXGmeFrameMain:SingletonMono<GXGmeFrameMain>
    {
        public void Start()
        {
            EnitityHouse.Instance.Init();
        }

        public void Update()
        {
            EnitityHouse.Instance.Update();
        }

        public void LateUpdate()
        {
            
        }

        public void OnDisable()
        {
            
        }
    }
}