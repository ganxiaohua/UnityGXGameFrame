namespace GameFrame
{
    public interface IObjectBase:IReference
    {
        public void Initialize();

        public void Recycle();

        public void Destroy();
    }
}