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
        protected EffEntity BindEntity { get; private set; }

        public override bool AutoLayers { get; set; } = false;

        protected object UserData;

        private ViewEffBindEnitiy viewEffBindEnitiy;

        public virtual bool notNeedBuind => false;


        public override void Initialize(object initData)
        {
            var input = (Input) initData;
            base.Initialize(initData);
            BindEntity = input.Entity;
            parent = input.Parent;
            UserData = input.UserData;
            viewEffBindEnitiy = gameObject.AddComponent<ViewEffBindEnitiy>();
            viewEffBindEnitiy.Entity = BindEntity;
        }

        protected override void OnAfterBind(GameObject go)
        {
            base.OnAfterBind(go);
            if (BindEntity.IsAction)
                BindEntity.AddComponentNoGet<BindingTargetOverComp>();
        }

        protected override void OnBeforeUnbind()
        {
            if (BindEntity.IsAction)
                BindEntity.RemoveComponent(ComponentsID<BindingTargetOverComp>.TID);
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