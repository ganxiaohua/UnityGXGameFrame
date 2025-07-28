using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFrame.Runtime
{
    public class GameObjectPoolBaes : ObjectBase
    {
        private DefaultAssetReference defaultAssetReference;
        public string AssetName { get; private set; }
        private Transform parent;
        private GameObject prefab;
        public GameObject Obj { get; private set; }

        public Transform Tra { get; private set; }

        private bool isNew;


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="prefab"></param>
        public override void Initialize(object prefab)
        {
            if (prefab == null)
            {
                Debugger.LogError("GameObjectBase prefabs is null");
                return;
            }

            this.prefab = (GameObject) prefab;
        }

        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        public override void OnSpawn()
        {
            if (Obj == null)
            {
                Obj = Object.Instantiate(prefab);
                isNew = true;
            }
            else
            {
                isNew = false;
            }

            Tra = Obj.transform;
            Obj.SetActive(true);
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        public override void OnUnspawn()
        {
            if (Obj == null)
            {
                return;
            }

            Obj.SetActive(false);
#if UNITY_EDITOR
            
            var areaName = AssetName.Replace("/", "_");
            var goCacheParent = GameObjectPool.ObjectCacheArea.transform.Find(areaName);
            if (goCacheParent == null)
            {
                var go = new GameObject();
                goCacheParent = go.transform;
                goCacheParent.name = areaName;
                goCacheParent.transform.SetParent(GameObjectPool.ObjectCacheArea.transform);
            }

            Obj.transform.SetParent(goCacheParent);
#else
            Obj.transform.SetParent(GameObjectPool.ObjectCacheArea.transform);
#endif
        }

        public void SetAssetPath(string assetPath)
        {
            this.AssetName = assetPath;
        }

        public void SetParent(Transform parent)
        {
            this.parent = parent;
            if (Obj != null)
            {
                Obj.transform.parent = parent;
            }
        }

        public void SetAssetReference(DefaultAssetReference defaultAssetReference)
        {
            this.defaultAssetReference = defaultAssetReference;
            if (isNew)
            {
                DefaultAssetReferenceContainer.Bind(Obj, defaultAssetReference);
            }
        }

        /// <summary>
        /// 清理对象基类。
        /// </summary>
        public override void Dispose()
        {
            if (Obj == null)
            {
                return;
            }

            this.defaultAssetReference?.UnrefAssets(false);
            this.defaultAssetReference = null;
            Object.Destroy(Obj);
            AssetName = null;
            Obj = null;
            parent = null;
            prefab = null;
            base.Dispose();
        }
    }
}