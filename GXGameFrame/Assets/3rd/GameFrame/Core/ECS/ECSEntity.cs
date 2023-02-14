namespace GameFrame
{
    public abstract class ECSEntity : Entity
    {
        protected override void ThisInit()
        {
            InitComponent();
        }

        public abstract void InitComponent();
    }
}