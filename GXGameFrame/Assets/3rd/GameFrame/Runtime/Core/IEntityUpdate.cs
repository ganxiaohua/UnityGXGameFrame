namespace GameFrame
{
    public interface IUpdateSystem:ISystem
    {
        void Update( float elapseSeconds, float realElapseSeconds);
    }
}