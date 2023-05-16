
namespace GameFrame
{
    public class ObjectPoolFactory: Singleton<ObjectPoolManager>
    {

        public static void CreatePool( string path, int defaultSize, int expireTime)
        {
             GXGameFrame.Instance.MainScene.GetComponent<GameObjectPoolComponent>().CreatePool(path,defaultSize,expireTime);
        }
        public static void RemovePool( string path)
        {
            GXGameFrame.Instance.MainScene.GetComponent<GameObjectPoolComponent>().RemovePool(path);
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