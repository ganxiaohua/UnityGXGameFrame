namespace GameFrame.Runtime
{
    public interface IFixedUpdateForEndEcsSystem : ISystem
    {
        void OnFixedUpdateForEndEcs(float elapseSeconds, float realElapseSeconds);
    }
}
