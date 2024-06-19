using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Runtime
{
    public abstract class GameObjectProxy
    {
        public GameObject gameObject { get; private set; }

        public Transform transform { get; private set; }

        public string asset { get; private set; }

        public GameObject prefab { get; private set; }

        public int version { get; private set; }

        public bool binded { get; private set; }

        public event Action onBeforeUnbind;
        public event Action onAfterBind;

        private static GameObject _EmptyPrefab;

        private static GameObject EmptyPrefab
        {
            get
            {
                if (!_EmptyPrefab)
                {
                    _EmptyPrefab = new GameObject("_Empty");
                    _EmptyPrefab.hideFlags = HideFlags.HideAndDontSave;
                }

                return _EmptyPrefab;
            }
        }

        public void BindFromEmpty(IGameObjectPool pool, Transform parent = null)
        {
            BindFromPrefab(pool, EmptyPrefab, parent);
        }

        public void BindFromPrefab(IGameObjectPool pool, GameObject prefab, Transform parent = null)
        {
            version++;
            Unbind(pool);
            var go = pool.Get(prefab, parent);
            go.hideFlags = HideFlags.None;
            this.gameObject = go;
            this.transform = go.transform;
            this.binded = true;
            this.prefab = prefab;
            OnAfterBind();
            onAfterBind?.Invoke();
        }

        public async UniTask<bool> BindFromAssetAsync(IGameObjectPool pool, string asset, Transform parent = null,
            CancellationToken cancelToken = default)
        {
            version++;
            int prevVersion = version;
            
            var go = default(GameObject);
            try
            {
                go = await pool.GetAsync(asset, transform, cancelToken);
            }
            catch (Exception e)
            {
                if (go) pool.Release(asset, go);
                if (e is not OperationCanceledException)
                    Debug.LogException(e);
                return false;
            }
            
            if (prevVersion != version)
            {
                // operation is obsolete
                pool.Release(asset, go);
                return false;
            }

            Unbind(pool);
            this.gameObject = go;
            this.transform = go.transform;
            this.transform.parent = parent;
            this.asset = asset;
            this.binded = true;
            OnAfterBind();
            onAfterBind?.Invoke();
            return true;
        }

        public bool Unbind(IGameObjectPool pool)
        {
            version++;
            if (!binded) return false;
            onBeforeUnbind?.Invoke();
            OnBeforeUnbind();
            if (prefab)
                pool.Release(prefab, gameObject);
            else
                pool.Release(asset, gameObject);
            gameObject = null;
            transform = null;
            asset = null;
            prefab = null;
            binded = false;
            return true;
        }

        protected virtual void OnBeforeUnbind()
        {

        }

        protected virtual void OnAfterBind()
        {

        }
    }
}