using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public interface IMessenger : IReference
    {
    }

    public interface IMessenger<P1> : IMessenger
    {
        void Send(P1 p1);
    }

    public interface IMessenger<P1, P2> : IMessenger
    {
        void Send(P1 p1, P2 p2);
    }

    public interface IMessenger<P1, P2, P3> : IMessenger
    {
        void Send(P1 p1, P2 p2, P3 p3);
    }

    public interface IMessengerAsyn<P1> : IMessenger
    {
        UniTask Send(P1 p1);
    }
}