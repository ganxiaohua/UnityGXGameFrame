using System.Collections.Generic;
using System.Threading;
using GameFrame;
using GameFrame.Timer;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WaitReleaseAsset : IReference
{
    private AsyncOperationHandle AsyncOperationHandle;
    private const int MaxWaitAsset = 60;
    private static List<WaitReleaseAsset> WaitReleaseAssetQueue;

    public int TimerID { get; private set; }

    public static void CreateWaitReleaseAsset(AsyncOperationHandle asyncOperationHandle, long time)
    {
        WaitReleaseAsset waitrelase = ReferencePool.Acquire<WaitReleaseAsset>();
        waitrelase.Init(asyncOperationHandle, time);
    }

    public void Init(AsyncOperationHandle asyncOperationHandle, long time)
    {
        if (WaitReleaseAssetQueue == null)
            WaitReleaseAssetQueue = new List<WaitReleaseAsset>(MaxWaitAsset);
        AsyncOperationHandle = asyncOperationHandle;
        WaitReleaseAssetQueue.Add(this);
        Check();
        WaitRelease(time);
    }

    /// <summary>
    /// 检查有没有超过最大极限MaxWaitAsset
    /// </summary>
    private void Check()
    {
        //如果说等待删除的列表中超过了MaxWaitAsset 清空四分之一.
        if (WaitReleaseAssetQueue.Count >= MaxWaitAsset)
        {
            for (int i = MaxWaitAsset / 4; i >= 0; i--)
            {
                // Timers.inst.Remove(WaitReleaseAssetQueue[i].Remove);
                TimerComponent.Instance.CancelTimer(TimerID);
                WaitReleaseAssetQueue[i].Remove();
            }
        }
    }

    public void WaitRelease(long time)
    {
        TimerID = TimerComponent.Instance.AddOnceTimer(time, Remove);
    }

    public void Remove()
    {
        ReferencePool.Release(this);
        WaitReleaseAssetQueue.Remove(this);
        Addressables.Release(AsyncOperationHandle);
    }

    public void Clear()
    {
        
    }
}