namespace GameFrame.Runtime
{
    public interface ILateUpdateSystem:ISystem
    {
        void LateUpdate(float elapseSeconds, float realElapseSeconds);
    }
}