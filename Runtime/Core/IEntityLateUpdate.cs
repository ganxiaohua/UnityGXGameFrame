namespace GameFrame
{
    public interface ILateUpdateSystem:ISystem
    {
        void LateUpdate(float elapseSeconds, float realElapseSeconds);
    }
}