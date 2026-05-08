using System;

namespace GameFrame.Runtime
{
    public interface IGOAPGoal : IDisposable
    {
        GOAPState DesiredState { get; }

        float Priority { get; }

        bool IsValid();

        void UpdatePriority();

        void OnActivate();

        void OnDeactivate();
    }
}