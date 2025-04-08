namespace GameFrame
{
    public abstract class UpdateReactiveSystem : ReactiveBaseSystem, IUpdateSystem
    {
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            Do(elapseSeconds, realElapseSeconds);
        }
    }
}