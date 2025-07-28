namespace GameFrame.Runtime
{
    public interface IFixedUpdateSystem : ISystem
    {
        void OnFixedUpdate(float elapseSeconds, float realElapseSeconds);
    }
}