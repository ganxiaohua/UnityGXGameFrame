namespace GameFrame
{
    public interface IUpdateSystem:ISystem
    {
        void OnUpdate( float elapseSeconds, float realElapseSeconds);
    }
}