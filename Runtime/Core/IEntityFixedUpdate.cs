namespace GameFrame
{
    public interface IFixedUpdateSystem : ISystem
    {
        void FixedUpdate(float elapseSeconds, float realElapseSeconds);
    }
}