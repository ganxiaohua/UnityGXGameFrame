using System;

namespace GameFrame
{
    public static class SystemFactory
    {
        public static void AddSystem<T>(this IEntity entity) where T : class,ISystem
        {
            EnitityHouse.Instance.AddSystem<T>(entity);
        }
        
        public static void AddSystem(this IEntity entity,Type type)
        {
            EnitityHouse.Instance.AddSystem(entity,type);
        }
        
        public static void AddStartSystem<P1>(this IEntity entity,P1 p1)
        {
            EnitityHouse.Instance.AddStartSystem(entity,p1);
        }
        
        public static void AddStartSystem<P1,P2>(this IEntity entity,P1 p1,P2 p2)
        {
            EnitityHouse.Instance.AddStartSystem(entity,p1,p2);
        }
        
        public static void RemoveSystem<T>(this IEntity entity) where T : class,ISystem
        {
            EnitityHouse.Instance.RemoveSystem<T>(entity);
        }
    }
}