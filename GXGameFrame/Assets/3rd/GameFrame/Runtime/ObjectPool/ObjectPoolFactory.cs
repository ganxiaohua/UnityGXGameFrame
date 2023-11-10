

namespace GameFrame
{
    public class ObjectPoolFactory: Singleton<ObjectPoolFactory>
    {
        public static ObjectPool<GameObjectObjectBase> CreatePool(string path, int defaultSize, int expireTime)
        {
           return  GXGameFrame.Instance.MainScene.GetComponent<GameObjectPoolComponent>().CreatePool(path,defaultSize,expireTime);
        }
        public static void RemovePool( string name)
        {
            GXGameFrame.Instance.MainScene.GetComponent<GameObjectPoolComponent>().RemovePool(name);
        }
        
        
        public static GameObjectObjectBase GetObject(string path)
        {
            return GXGameFrame.Instance.MainScene.GetComponent<GameObjectPoolComponent>().Get(path);
        }
        
        public static void RecycleObject(GameObjectObjectBase go)
        {
            GXGameFrame.Instance.MainScene.GetComponent<GameObjectPoolComponent>().Recycle(go);
        }
    }
}