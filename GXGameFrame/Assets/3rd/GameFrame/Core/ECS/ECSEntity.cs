namespace GameFrame
{
    public abstract class ECSEntity : Entity
    {
        protected override Entity Create<T>(bool isComponent)
        {
            ECSEntity entity = base.Create<T>(isComponent) as ECSEntity;
            entity.InitComponent();
            return entity;
        }

        public abstract void InitComponent();
    }
}