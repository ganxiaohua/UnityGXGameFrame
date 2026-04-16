namespace GameFrame.Runtime
{
    public interface ILateUpdateSystem : ISystem
    {
        void OnLateUpdate(float elapseSeconds, float realElapseSeconds);
    }
}