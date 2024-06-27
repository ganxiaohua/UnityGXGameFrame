namespace GameFrame
{
    public interface IStartSystem : ISystem
    {
        void Start();
    }

    public interface IStartSystem<in TP1> : ISystem
    {
        void Start(TP1 p1);
    }

    public interface IStartSystem<in TP1, in TP2> : ISystem
    {
        void Start(TP1 p1, TP2 p2);
    }
}