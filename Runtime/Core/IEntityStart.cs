namespace GameFrame.Runtime
{
    public interface IInitializeSystem : ISystem
    {
        void OnInitialize();
    }

    public interface IInitializeSystem<in TP1> : ISystem
    {
        void OnInitialize(TP1 param);
    }

    public interface IInitializeSystem<in TP1, in TP2> : ISystem
    {
        void OnInitialize(TP1 p1, TP2 p2);
    }
}