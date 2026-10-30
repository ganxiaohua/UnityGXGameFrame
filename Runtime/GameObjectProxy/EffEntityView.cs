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
            public World World;
            public Transform Parent;
            public object UserData;

            public void Dispose()
            {
                Entity = default;
                World = default;
                Parent = null;
                UserData = null;
            }
        }

        public Transform Parent { get; private set; }
        protected World World { get; private set; }
        protected EffEntity BindEntity { get; private set; }

        public override bool AutoLayers { get; set; } = false;

        protected object UserData;

        private ViewEffBindEnitiy viewEffBindEnitiy;

        private Action bindOver;

        public virtual bool NotNeedBind => false;


        public override void Initialize(object initData)
        {
            var input = (Input) initData;
            base.Initialize(initData);
            BindEntity = input.Entity;
            World = input.World;
            Parent = input.Parent;
            UserData = input.UserData;
            viewEffBindEnitiy = gameObject.AddComponent<ViewEffBindEnitiy>();
            viewEffBindEnitiy.Entity = BindEntity;
            viewEffBindEnitiy.World = World;
        }

        protected override void OnAfterBind(GameObject go)
        {
            base.OnAfterBind(go);
            if (World != null && World.IsAlive(BindEntity))
                World.AddCompNoGet<BindingTargetOverComp>(BindEntity);
            bindOver?.Invoke();
        }

        protected override void OnBeforeUnbind()
        {
            if (World != null && World.IsAlive(BindEntity))
                World.RemoveComp(BindEntity, ComponentsID<BindingTargetOverComp>.TID);
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
            if (viewEffBindEnitiy != null)
            {
                viewEffBindEnitiy.Entity = default;
                viewEffBindEnitiy.World = default;
            }
            viewEffBindEnitiy = null;
            BindEntity = default;
            World = default;
            Parent = null;
            UserData = null;
        }

        public virtual void TickActive(float delatTime, float realElapseSeconds)
        {
        }
    }
}
