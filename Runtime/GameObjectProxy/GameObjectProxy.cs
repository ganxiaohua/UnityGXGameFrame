using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFrame.Runtime;
using UnityEngine;

namespace Common.Runtime
{
    public abstract class GameObjectProxy
    {
        protected GameObjectPoolBaes GoBase { get; private set; }

        public GameObject Go => GoBase.Obj;

        public Transform Trans => GoBase.Tra;

        public string Asset { get; private set; }

        public GameObject Prefab { get; private set; }

        public int Version { get; private set; }

        public bool IsBind { get; private set; }


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
            Version++;
            Unbind();
            var goBase = GameObjectPool.Instance.InstantiateGameObject(prefab, parent);
            goBase.Obj.hideFlags = HideFlags.None;
            this.GoBase = goBase;
            this.IsBind = true;
            this.Prefab = prefab;
            OnAfterBind();
            onAfterBind?.Invoke();
        }

        public async UniTask<bool> BindFromAssetAsync(string asset, Transform parent = null,
                CancellationToken cancelToken = default)
        {
            Version++;
            int prevVersion = Version;

            var go = default(GameObjectPoolBaes);
            try
            {
                go = await GameObjectPool.Instance.GetAsync(asset, parent, cancelToken);
                if (go == null)
                    return false;
            }
            catch (Exception e)
            {
                if (go != null) GameObjectPool.Instance.Release(asset, go);
                if (e is not OperationCanceledException)
                    Debug.LogException(e);
                return false;
            }

            if (prevVersion != Version)
            {
                GameObjectPool.Instance.Release(asset, go);
                return false;
            }

            Unbind();
            this.GoBase = go;
            this.Asset = asset;
            this.IsBind = true;
            OnAfterBind();
            onAfterBind?.Invoke();
            return true;
        }

        public bool Unbind()
        {
            Version++;
            if (!IsBind) return false;
            onBeforeUnbind?.Invoke();
            OnBeforeUnbind();
            if (GoBase != null)
            {
                if (Prefab)
                    GameObjectPool.Instance.Release(Prefab, GoBase);
                else
                    GameObjectPool.Instance.Release(Asset, GoBase);
            }

            GoBase = null;
            Asset = null;
            Prefab = null;
            IsBind = false;
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