using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFrame
{
    public class PoolObject : ObjectBase
    {
        private Transform mParent;
        private GameObject mPrefab;
        private GameObject mObj;
        public GameObject Obj => mObj;
        

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

            mPrefab = (GameObject) prefab;
        }

        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        public override void OnSpawn()
        {
            if (mObj == null)
            {
                mObj = Object.Instantiate(mPrefab);
            }

            mObj.SetActive(true);
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        public override void OnUnspawn()
        {
            if (mObj == null)
            {
                return;
            }

            mObj.SetActive(false);
        }

        public void SetParent(Transform parent)
        {
            mParent = parent;
            if (mObj != null)
            {
                mObj.transform.parent = parent;
            }
        }

        /// <summary>
        /// 清理对象基类。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (mObj == null)
            {
                return;
            }
            Object.Destroy(mObj);
            mObj = null;
            mParent = null;
            mPrefab = null;
        }
    }
}