namespace GameFrame.Runtime
{
    public abstract class UpdateReactiveSystem : ReactiveBaseSystem, IUpdateSystem
    {
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            Do();
        }
    }
}