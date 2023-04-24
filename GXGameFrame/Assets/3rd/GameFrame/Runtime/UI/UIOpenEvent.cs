using System;

namespace GameFrame
{
    public class UIOpenEvent : IMessenger<Type>
    {
        public void Send(Type Type)
        {
            // UIManager.Instance.GetUIClose(Type);
        }

        public void Clear()
        {
            
        }
    }
}