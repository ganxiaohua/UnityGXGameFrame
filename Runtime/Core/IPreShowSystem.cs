namespace GameFrame.Runtime
{
    public interface IPreShow
    {
        
    }
    public interface IPreShowSystem:ISystem
    {
        void OnPreShow(bool isFirstShow);
    }
}