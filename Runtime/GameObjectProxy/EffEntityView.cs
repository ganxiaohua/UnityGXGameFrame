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

        public virtual void TickActive(float delatTime, float realElapseSeconds)
        {
        }
    }
}