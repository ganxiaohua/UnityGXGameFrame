using UnityEngine;

namespace GameFrame.Runtime
{
    public partial class GameObjectProxy : ObjectBase
    {
        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        public override void OnUnspawn()
        {
            Unbind();
            Transform goCacheParent = null;
#if UNITY_EDITOR
            goCacheParent = GameObjectPool.ObjectCacheArea.transform.Find("ComGameObject");
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


            gameObject.transform.SetParent(goCacheParent);
            base.OnUnspawn();
        }

        public override void Dispose()
        {
            Unbind();
            recycleTimer.Cancel();
            GameObject.Destroy(gameObject);
            gameObject = null;
            transform = null;
            State = GameObjectState.Destroy;
            Userdata = null;
            base.Dispose();
        }
    }
}