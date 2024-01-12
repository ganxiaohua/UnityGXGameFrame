namespace GameFrame
{
    public static class SystemFactory
    {
        public static void AddEcsSystem<T>(this Context entity) where T : class,ISystem
        {
            EnitityHouse.Instance.AddEcsSystem<T>(entity);
        }
        
        
        public static void RemoveSystem<T>(this Context entity) where T : class,ISystem
        {
            EnitityHouse.Instance.RemoveSystem<T>(entity);
        }
    }
}