using System.Threading;
using Cysharp.Threading.Tasks;
using GameFrame.Runtime;
using UnityEngine;

namespace Common.Runtime
{
    public interface IGameObjectPool
    {
        UniTask<GameObjectPoolBase> GetAsync(string asset, Transform parent = null, CancellationToken cancelToken = default);

        GameObjectPoolBase InstantiateGameObject(GameObject prefab, Transform parent = null);

        void Release(object key, GameObjectPoolBase go);
    }
}