//重写

namespace GameFrame.Runtime
{
    public class GoWrapperProxyPool : Singleton<GoWrapperProxyPool>
    {
        private ObjectPool<GoWrapperProxy> objectPool = ObjectPoolManager.Instance.CreateObjectPool<GoWrapperProxy>("GoWrapperProxyPool", 128, 10);

        public GameObjectProxy Spawn()
        {
            return objectPool.Spawn();
        }

        public void UnSpawn(GoWrapperProxy go)
        {
            objectPool.UnSpawn(go);
        }

        public void Dispose()
        {
            ObjectPoolManager.Instance.DeleteObjectPool(objectPool);
            objectPool = null;
        }
    }
}