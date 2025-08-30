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
            var goCacheParent = GameObjectPool.ObjectCacheArea.transform.Find("ComGameObject");
            if (goCacheParent == null)
            {
                var go = new GameObject();
                goCacheParent = go.transform;
                goCacheParent.name = "ComGameObject";
                goCacheParent.transform.SetParent(GameObjectPool.ObjectCacheArea.transform);
            }
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