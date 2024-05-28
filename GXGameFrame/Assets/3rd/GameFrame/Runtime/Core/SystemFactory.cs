namespace GameFrame
{
    public static class SystemFactory
    {
        public static void AddEcsSystem<T>(this World entity) where T : class,ISystem
        {
            EnitityHouse.Instance.AddEcsSystem<T>(entity);
        }
        
        
        public static void RemoveSystem<T>(this World entity) where T : class,ISystem
        {
            EnitityHouse.Instance.RemoveSystem<T>(entity);
        }
    }
}