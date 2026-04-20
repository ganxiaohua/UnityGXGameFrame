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

        public Transform Parent { get; private set; }
        protected EffEntity BindEntity { get; private set; }

        public override bool AutoLayers { get; set; } = false;

        protected object UserData;

        private ViewEffBindEnitiy viewEffBindEnitiy;

        private Action bindOver;

        public virtual bool NotNeedBuind => false;


        public override void Initialize(object initData)
        {
            var input = (Input) initData;
            base.Initialize(initData);
            BindEntity = input.Entity;
            Parent = input.Parent;
            UserData = input.UserData;
            viewEffBindEnitiy = gameObject.AddComponent<ViewEffBindEnitiy>();
            viewEffBindEnitiy.Entity = BindEntity;
        }

        protected override void OnAfterBind(GameObject go)
        {
            base.OnAfterBind(go);
            if (BindEntity.IsAction)
                BindEntity.AddComponentNoGet<BindingTargetOverComp>();
            bindOver?.Invoke();
        }

        protected override void OnBeforeUnbind()
        {
            if (BindEntity.IsAction)
                BindEntity.RemoveComponent(ComponentsID<BindingTargetOverComp>.TID);
        }

        public void SetBindOver(Action action)
        {
            bindOver = action;
        }

        public override void Dispose()
        {
            Object.Destroy(viewEffBindEnitiy);
            bindOver = null;
            base.Dispose();
        }

        public virtual void TickActive(float delatTime, float realElapseSeconds)
        {
        }
    }
}