using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Runtime
{
    public partial class GameObjectProxy : IVersions, IDisposable
    {
        public GameObject gameObject { get; private set; }

        public Transform transform { get; private set; }

        public int Versions { get; private set; }

        public Vector3 LocalPosition
        {
            get => transform.localPosition;
            set => transform.localPosition = value;
        }

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public Quaternion LocalRotation
        {
            get => transform.localRotation;
            set => transform.localRotation = value;
        }

        public Quaternion Rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }

        public Vector3 scale
        {
            get => transform.localScale;
            set => transform.localScale = value;
        }

        public bool Visible
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        public bool AutoLayers { get; set; } = true;

        public object BindingSource { get; private set; }

        public GameObjectState State { get; private set; }

        public Action onBeforeUnbind;
        public Action<GameObjectProxy> onAfterBind;

        protected GameObjectPoolBaes bindedGameObject { get; private set; }

        public GameObject BindingTarget => bindedGameObject?.Obj;

        private UniqueTimer recycleTimer;

        private UnityGameObjectItem unityGameObjectItem;

        public virtual void Initialize(object initData = null)
        {
            unityGameObjectItem = UnitGoPool.Instance.Spawn(null);
            gameObject = unityGameObjectItem.gameObject;
            transform = gameObject.transform;

            State = GameObjectState.Unload;

            recycleTimer = new UniqueTimer(OnRequestRecycle);

#if UNITY_EDITOR
            recycleTimer.Name = this.GetType().Name;
#endif
        }

        public void BindFromPrefab(GameObject prefab, Transform parent = null)
        {
            Assert.AreNotEqual(GameObjectState.Destroy, State, $"GameObjectProxy({this}) already destroyed");
            if (CheckIdenticalBindingSource(prefab, parent))
                return;

            Versions++;
            BindingSource = prefab;
            var goBase = GameObjectPool.Instance.InstantiateGameObject(prefab, parent);
            goBase.Obj.hideFlags = HideFlags.None;
            BindInternal(goBase, parent);
        }


        public void BindFromPrefab(GameObject prefab)
        {
            BindFromPrefab(prefab, transform.parent);
        }

        public async UniTask<bool> BindFromAssetAsync(string asset, Transform parent = null, CancellationToken cancelToken = default)
        {
            Assert.AreNotEqual(GameObjectState.Destroy, State, $"GameObjectProxy({this}) already destroyed");
            if (CheckIdenticalBindingSource(asset, parent))
                return true;

            Versions++;
            BindingSource = asset;

            var prevVersion = Versions;

            State = GameObjectState.Loading;

            GameObjectPoolBaes go = null;
            try
            {
                go = await GameObjectPool.Instance.GetAsync(asset, transform, cancelToken);
            }
            catch (Exception e)
            {
                if (go != null) GameObjectPool.Instance.Release(asset, go);
                if (e is not OperationCanceledException)
                    Debug.LogException(e);
                return false;
            }

            if (prevVersion != Versions)
            {
                // operation is obsolete
                if (go != null)
                    GameObjectPool.Instance.Release(asset, go);
                return false;
            }

            BindInternal(go, parent);

            return true;
        }

        public async UniTask<bool> BindFromAssetAsync(string asset, CancellationToken cancelToken = default)
        {
            return await BindFromAssetAsync(asset, transform.parent, cancelToken);
        }

        public void BindFromAsset(string asset)
        {
            BindFromAssetAsync(asset, transform.parent).Forget();
        }

        public bool BindFromDifferentSource(object source)
        {
            if (BindingSource != source)
            {
                if (source == null)
                    Unbind();
                else if (source is string asset)
                    BindFromAsset(asset);
                else
                    BindFromPrefab((GameObject) source);
                return true;
            }

            return false;
        }

        private bool CheckIdenticalBindingSource(object source, Transform parent)
        {
            if (State == GameObjectState.Loaded && source.Equals(BindingSource))
            {
                SetParentInternal(parent);
                OnAfterBind(BindingTarget);
                onAfterBind?.Invoke(this);
                return true;
            }

            return false;
        }

        private void BindInternal(GameObjectPoolBaes go, Transform parent)
        {
            var source = BindingSource;

            Unbind();

            Assert.AreNotEqual(null, go, $"Bind invalid GameObject from source: {source}");

            bindedGameObject = go;
            BindingSource = source;

            BindingTarget.hideFlags = HideFlags.None;

            SetParentInternal(parent);

            var trans = BindingTarget.transform;
            trans.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            trans.localScale = Vector3.one;

            State = GameObjectState.Loaded;
            OnAfterBind(BindingTarget);
            onAfterBind?.Invoke(this);
#if UNITY_EDITOR
            DebugInspector.Track(this);
#endif
        }

        private void SetParentInternal(Transform parent)
        {
            if (transform.parent != parent)
            {
                transform.SetParent(parent, false);
            }

            if (!AutoLayers)
            {
                gameObject.layer = BindingTarget.layer;
                return;
            }


            if (parent != null && parent.gameObject.layer != gameObject.layer)
            {
                transform.SetLayerRecursive(parent.gameObject.layer);
            }

            if (BindingTarget && BindingTarget.layer != gameObject.layer)
            {
                BindingTarget.transform.SetLayerRecursive(gameObject.layer);
            }
        }

        public bool Unbind()
        {
            Assert.AreNotEqual(GameObjectState.Destroy, State, $"GameObjectProxy({this}) already destroyed");
            if (State == GameObjectState.Unload) return false;
            Versions++;
            if (BindingSource != null && BindingTarget)
            {
                onBeforeUnbind?.Invoke();
                OnBeforeUnbind();

                GameObjectPool.Instance.Release(BindingSource, bindedGameObject);
            }

            bindedGameObject = null;
            BindingSource = null;
            State = GameObjectState.Unload;
            return true;
        }

        protected virtual void OnBeforeUnbind()
        {
        }

        protected virtual void OnAfterBind(GameObject go)
        {
        }


        public void Recycle(float delay)
        {
            recycleTimer.Schedule(delay);
        }

        public void RecycleImmediate()
        {
            recycleTimer.Cancel();
            OnRequestRecycle();
        }

        public virtual void OnBeforeRecycle()
        {
            Versions++;
        }

        protected virtual void OnRequestRecycle()
        {
            Unbind();
        }

        public virtual void Dispose()
        {
            Unbind();
            UnitGoPool.Instance.UnSpawn(unityGameObjectItem);
            recycleTimer.Cancel();
            gameObject = null;
            transform = null;
            State = GameObjectState.Destroy;
        }
    }
}