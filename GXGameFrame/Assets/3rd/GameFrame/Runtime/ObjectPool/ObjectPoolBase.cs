namespace GameFrame
{
    public interface IObjectPoolBase:IReference
    {
        void Update(float elapseSeconds, float realElapseSeconds);
    }
}