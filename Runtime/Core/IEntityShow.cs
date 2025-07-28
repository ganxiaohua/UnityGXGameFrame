namespace GameFrame.Runtime
{
    public interface IShowSystem : ISystem
    {
        void OnShow();
    }

    public interface IShowSystem<P1> : ISystem
    {
        void Show(P1 p1);
    }

    public interface IShowSystem<P1, P2> : ISystem
    {
        void Show(P1 p1, P2 p2);
    }
}