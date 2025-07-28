namespace GameFrame.Runtime
{
    public abstract class FixedUpdateReactiveSystem:ReactiveBaseSystem, IFixedUpdateSystem
    {
        public void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            Do();
        }
    }
}