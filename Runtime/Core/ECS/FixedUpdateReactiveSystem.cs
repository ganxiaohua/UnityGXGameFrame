namespace GameFrame
{
    public abstract class FixedUpdateReactiveSystem:ReactiveBaseSystem, IFixedUpdateSystem
    {
        public void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            Do();
        }
    }
}