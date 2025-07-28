using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using GameFrame.Runtime;

namespace Common.Runtime
{
    public interface IGameObjectPool
    {
        UniTask<GameObjectPoolBaes> GetAsync(string asset, Transform parent = null, CancellationToken cancelToken = default);

        GameObjectPoolBaes InstantiateGameObject(GameObject prefab, Transform parent = null);

        void Release(object key, GameObjectPoolBaes go);
    }
}