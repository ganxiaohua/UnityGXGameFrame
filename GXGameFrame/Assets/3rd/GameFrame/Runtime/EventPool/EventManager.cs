using System;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public class EventManager : Singleton<EventManager>
    {
        public void Send<T, P1>(P1 p1) where T : class, IMessenger<P1>, new()
        {
            T t = ReferencePool.Acquire<T>();
            t.Send(p1);
            RecycleEvent(t);
        }

        public void Send<T, P1, P2>(P1 p1, P2 p2) where T : class, IMessenger<P1, P2>, new()
        {
            T t = ReferencePool.Acquire<T>();
            t.Send(p1, p2);
            RecycleEvent(t);
        }

        public void Send<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where T : class, IMessenger<P1, P2, P3>, new()
        {
            T t = ReferencePool.Acquire<T>();
            t.Send(p1, p2, p3);
            RecycleEvent(t);
        }

        public async UniTask SendAsyn<T, P1>(P1 p1) where T : class, IMessengerAsyn<P1>, new()
        {
            T t = ReferencePool.Acquire<T>();
            await t.Send(p1);
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