using System;

namespace GameFrame
{
    public class UICloseEvent : IMessenger<Type>
    {
        public void Send(Type Type)
        {
            UIManager.Instance.UIClose(Type);
        }

        public void Clear()
        {
        }
    }
}