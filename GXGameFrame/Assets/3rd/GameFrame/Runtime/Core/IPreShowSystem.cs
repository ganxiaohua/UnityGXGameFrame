namespace GameFrame
{
    public interface IPreShow
    {
        
    }
    public interface IPreShowSystem:ISystem
    {
        void PreShow(bool isFirstShow);
    }
}