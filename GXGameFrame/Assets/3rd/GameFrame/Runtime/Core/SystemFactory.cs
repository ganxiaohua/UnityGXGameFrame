using System;

namespace GameFrame
{
    public static class SystemFactory
    {
        public static void AddSystem<T>(this Entity entity) where T : class,ISystem
        {
            EnitityHouse.Instance.AddSystem<T>(entity);
            
        }
        
        public static void RemoveSystem<T>(this Entity entity) where T : class,ISystem
        {
            EnitityHouse.Instance.RemoveSystem<T>(entity);
        }
    }
}