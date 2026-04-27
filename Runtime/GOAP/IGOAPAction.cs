namespace GameFrame.Runtime
{
    public interface IGOAPAction
    {
        float Cost { get; }

        GOAPState Preconditions { get; }

        GOAPState Effects { get; }

        bool CheckProceduralPrecondition();

        bool Update(float deltaTime);

        bool IsRunning { get; }

        void Start();

        void OnComplete();

        void OnAbort();
    }
}