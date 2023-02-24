using System;

using Cysharp.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct gxhs : IJob
{
    public int a;
    public int b;
    public NativeArray<int> result;

    public void Execute()
    {
        result[0] = a + b;
    }
}

public class UnTaskTest : MonoBehaviour
{
    // Start is called before the first frame update

    async UniTask Test1()
    {
        // UnityEngine.Debug.Log("DEB1");
        //
        // await UniTask.DelayFrame(30, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        //
        // UnityEngine.Debug.Log("DEB END1");
    }

    async UniTaskVoid Test2()
    {
        // UnityEngine.Debug.Log("DEB2");
        //
        // await UniTask.DelayFrame(30);
        //
        // UnityEngine.Debug.Log("DEB END2");
    }

    async UniTask Start()
    {
        // NativeArray<int> result = new NativeArray<int>(1, Allocator.TempJob);
        // gxhs jobData = new gxhs();
        // jobData.a = 10;
        // jobData.b = 10;
        // jobData.result = result;
        // JobHandle handle = jobData.Schedule();
        // await handle;
        // Debug.Log(result[0]);
        // result.Dispose();
        // await Test1();
        // Test2();
        UnityEngine.Debug.Log("Start");
    }
    
    public async UniTask Wait()
    {

        try
        {
            Debug.Log("调用之前");
            await WaitTask.Wait<IWaitType>();
            Debug.Log("调用之后");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            throw;
        }
        finally
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Wait();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            WaitTask.Notify<IWaitType>();
        }
    }
}