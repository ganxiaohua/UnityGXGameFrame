using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFrame
{
    public class GameObjectPoolBaes : ObjectBase
    {
        private DefaultAssetReference defaultAssetReference;
        public string assetName { get; private set; }
        private Transform parent;
        private GameObject prefab;
        public GameObject Obj { get; private set; }


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
            }

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
        }

        public void SetAssetPath(string assetPath)
        {
            this.assetName = assetPath;
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
        }

        /// <summary>
        /// 清理对象基类。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (Obj == null)
            {
                return;
            }

            this.defaultAssetReference?.UnrefAssets(false);
            this.defaultAssetReference = null;
            Object.Destroy(Obj);
            assetName = null;
            Obj = null;
            parent = null;
            prefab = null;
        }
    }
}