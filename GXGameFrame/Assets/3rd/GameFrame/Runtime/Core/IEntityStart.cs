namespace GameFrame
{
    public interface IStartSystem : ISystem
    {
        void Start();
    }

    public interface IStartSystem<P1> : ISystem
    {
        void Start(P1 p1);
    }

    public interface IStartSystem<P1, P2> : ISystem
    {
        void Start(P1 p1, P2 p2);
    }
}