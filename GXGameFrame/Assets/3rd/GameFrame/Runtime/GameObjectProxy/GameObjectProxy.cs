using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFrame;
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

        public void BindFromEmpty(Transform parent = null)
        {
            BindFromPrefab(EmptyPrefab, parent);
        }

        public void BindFromPrefab(GameObject prefab, Transform parent = null)
        {
            version++;
            Unbind();
            var go = GameObjectPool.Instance.InstantiateGameObject(prefab, parent);
            go.hideFlags = HideFlags.None;
            this.gameObject = go;
            this.transform = go.transform;
            this.binded = true;
            this.prefab = prefab;
            OnAfterBind();
            onAfterBind?.Invoke();
        }

        public async UniTask<bool> BindFromAssetAsync( string asset, Transform parent = null,
            CancellationToken cancelToken = default)
        {
            version++;
            int prevVersion = version;

            var go = default(GameObject);
            try
            {
                go = await GameObjectPool.Instance.GetAsync(asset, transform, cancelToken);
            }
            catch (Exception e)
            {
                if (go) GameObjectPool.Instance.Release(asset, go);
                if (e is not OperationCanceledException)
                    Debug.LogException(e);
                return false;
            }

            if (prevVersion != version)
            {
                // operation is obsolete
                GameObjectPool.Instance.Release(asset, go);
                return false;
            }

            Unbind();
            this.gameObject = go;
            this.transform = go.transform;
            this.transform.parent = parent;
            this.asset = asset;
            this.binded = true;
            OnAfterBind();
            onAfterBind?.Invoke();
            return true;
        }

        public bool Unbind()
        {
            version++;
            if (!binded) return false;
            onBeforeUnbind?.Invoke();
            OnBeforeUnbind();
            if (prefab)
                GameObjectPool.Instance.Release(prefab, gameObject);
            else
                GameObjectPool.Instance.Release(asset, gameObject);
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