using UnityEngine;

namespace GameFrame.Runtime
{
    public class EffEntityView : GameObjectProxy
    {
        public EffEntity BindEntity { get; private set; }

        public override void Initialize(object initData)
        {
            base.Initialize(initData);
            AutoLayers = false;
            BindEntity = (EffEntity) initData;
        }
        
        protected override void OnAfterBind(GameObject go)
        {
            var bind = go.AddComponent<ViewEffBindEnitiy>();
            bind.Entity = BindEntity;
        }
        
        protected override void OnBeforeUnbind()
        {
            Object.Destroy(BindingTarget.GetComponent<ViewEffBindEnitiy>());
        }

        public virtual void TickActive(float delatTime, float realElapseSeconds)
        {
        }
    }
}