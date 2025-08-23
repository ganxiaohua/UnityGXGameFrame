using UnityEngine;

namespace GameFrame.Runtime
{
    public class EffEntityView : GameObjectProxy
    {
        public EffEntity BindEntity { get; private set; }
        private ViewEffBindEnitiy viewEffBindEnitiy;
        public override void Initialize(object initData)
        {
            base.Initialize(initData);
            AutoLayers = false;
            BindEntity = (EffEntity) initData;
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