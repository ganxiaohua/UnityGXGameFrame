using System;

namespace GameFrame
{
    public class UIOpenEvent : IMessenger<Type>
    {
        public void Send(Type type)
        {
            UIManager.Instance.UIOpened(type);
        }

        public void Clear()
        {
            
        }
    }
}