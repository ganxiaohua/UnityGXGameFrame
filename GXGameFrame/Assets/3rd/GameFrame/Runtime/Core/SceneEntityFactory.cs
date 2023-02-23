namespace GameFrame
{
    public static class SceneEntityFactory
    {
        public static SceneEntity CreateScene<T>(Entity parent = null) where T : SceneEntityType
        {
            SceneEntity sceneEntity = ReferencePool.Acquire<SceneEntity>();
            sceneEntity.Init<T>(parent);
            return sceneEntity;
        }

        public static void RemoveScene<T>() where T : SceneEntityType
        {
            SceneEntity sceneEntity = EnitityHouse.Instance.GetScene<T>();
            RemoveScene(sceneEntity);
        }
        public static void RemoveScene (SceneEntity sceneEntity)
        {
            ReferencePool.Release(sceneEntity);
        }
    }
}