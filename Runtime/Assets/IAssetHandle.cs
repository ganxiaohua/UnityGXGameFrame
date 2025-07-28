using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameFrame.Runtime
{
    public interface IAssetHandle
    {
        bool IsValid { get; }

        bool IsDone { get; }

        object Result { get; }

        /// <summary>
        /// reference of extra assets
        /// </summary>
        IAssetReference AssetReference { get; }

        void Initialize(object key,Type type);

        UniTask GetTask(CancellationToken cancellationToken);

        void Finish();

        void Release();
    }
}