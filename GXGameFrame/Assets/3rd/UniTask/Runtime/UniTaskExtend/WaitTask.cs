using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks
{
    public interface IWaitType
    {
        int Error { get; set; }
    }


    public static class WaitTask
    {
        public static Dictionary<Type, CancellationTokenSource> WaitSource = new Dictionary<Type, CancellationTokenSource>();

        public static async UniTask Wait<T>() where T : IWaitType
        {
            Type type = typeof(T);
            await Wait(type);
        }

        public static async UniTask Wait(Type type)
        {
            if (WaitSource.ContainsKey(type))
            {
                Debug.LogError($"have type = {type}");
                return;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            WaitSource.Add(type, cts);
            await UniTask.WaitUntilCanceled(cts.Token);
        }

        public static void Notify<T>() where T : IWaitType
        {
            Type type = typeof(T);
            Notify(type);
        }

        public static void Notify(Type type)
        {
            if (!WaitSource.TryGetValue(type, out var cts))
            {
                Debug.LogError($"not have type = {type}");
                return;
            }

            WaitSource.Remove(type);
            cts.Cancel();
            cts.Dispose();
        }
    }
}