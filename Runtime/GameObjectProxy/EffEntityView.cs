using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFrame.Runtime
{
    public class EffEntityView : GameObjectProxy
    {
        public class Input : IDisposable
        {
            public EffEntity Entity;
            public Transform Parent;
            public object UserData;

            public void Dispose()
            {
                Entity = null;
                Parent = null;
            }
        }

        public Transform parent { get; private set; }
        public EffEntity BindEntity { get; private set; }

        public object UserData;

        private ViewEffBindEnitiy viewEffBindEnitiy;

        public override void Initialize(object initData)
        {
            var input = (Input) initData;
            base.Initialize(initData);
            AutoLayers = false;
            BindEntity = input.Entity;
            parent = input.Parent;
            UserData = input.UserData;
            viewEffBindEnitiy = gameObject.AddComponent<ViewEffBindEnitiy>();
            viewEffBindEnitiy.Entity = BindEntity;
        }

        public override void Dispose()
        {
            Object.Destroy(viewEffBindEnitiy);
            base.Dispose();
        }

        public virtual void TickActive(float delatTime, float realElapseSeconds)
        {
        }
    }
}