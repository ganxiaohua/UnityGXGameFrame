using UnityEngine;

namespace GameFrame.Runtime
{
    public class UnityGameObjectItem : ObjectBase
    {
        public GameObject gameObject { get; private set; }

        public override void Initialize(object initData = null)
        {
            base.Initialize(initData);
            gameObject = new GameObject();
        }

        public override void OnSpawn(object obj)
        {
            base.OnSpawn(obj);
            gameObject.transform.SetParent(null);
        }

        public override void OnUnspawn()
        {
#if UNITY_EDITOR
            var goCacheParent = GameObjectPool.ObjectCacheArea.transform.Find("ComGameObject");
            if (goCacheParent == null)
            {
                var go = new GameObject();
                goCacheParent = go.transform;
                goCacheParent.name = "ComGameObject";
                goCacheParent.transform.SetParent(GameObjectPool.ObjectCacheArea.transform);
            }
#else
            goCacheParent = GameObjectPool.ObjectCacheArea.transform;
#endif
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (!(component is Transform))
                {
                    Object.Destroy(component);
                }
            }

            gameObject.transform.SetParent(goCacheParent);
            base.OnUnspawn();
        }

        public override void Dispose()
        {
            Object.Destroy(gameObject);
            base.Dispose();
        }
    }
}