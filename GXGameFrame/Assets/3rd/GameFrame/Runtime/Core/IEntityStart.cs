namespace GameFrame
{
    public interface IInitializeSystem : ISystem
    {
        void Initialize();
    }

    public interface IInitializeSystem<in TP1> : ISystem
    {
        void Initialize(TP1 p1);
    }

    public interface IInitializeSystem<in TP1, in TP2> : ISystem
    {
        void Initialize(TP1 p1, TP2 p2);
    }
}