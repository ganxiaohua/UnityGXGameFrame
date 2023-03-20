namespace GameFrame
{
    public interface IAddressee:IReference
    {
        void Do(IMessenger messenger);
    }
}