using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Common.Runtime
{
    public interface IGameObjectPool
    {
        UniTask<GameObject> GetAsync(string asset, Transform parent = null, CancellationToken cancelToken = default);

        UniTask<GameObject> GetAsync(GameObject prefab, Transform parent = null, CancellationToken cancelToken = default);

        GameObject Get(GameObject prefab, Transform parent = null);

        void Release(string asset, GameObject go);

        void Release(GameObject prefab, GameObject go);
    }
}