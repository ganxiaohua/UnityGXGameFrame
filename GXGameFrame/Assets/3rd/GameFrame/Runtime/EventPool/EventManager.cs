using System;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEditor.UI;

namespace GameFrame
{
    public class EventManager : Singleton<EventManager>
    {
        public T CreateEvent<T>() where T : class, IMessenger, new()
        {
            T t = ReferencePool.Acquire<T>();
            return t;
        }

        public void Send<T>(T t) where T : class, IMessenger, new()
        {
            if (t == null)
            {
                throw new Exception($"{t.GetType().Name} is null");
            }

            t.Send();
            RecycleEvent(t);
        }

        private void RecycleEvent(IReference messager)
        {
            if (messager == null)
            {
                throw new Exception($"{messager.GetType().Name} is null");
            }
            ReferencePool.Release(messager);
        }
    }
}