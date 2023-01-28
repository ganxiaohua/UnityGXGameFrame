using System.Collections;
using System.Collections.Generic;
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
        UnityEngine.Debug.Log("DEB1");

        await UniTask.DelayFrame(30);

        UnityEngine.Debug.Log("DEB END1");
    }
    
    async UniTaskVoid Test2()
    {
        UnityEngine.Debug.Log("DEB2");

        await UniTask.DelayFrame(30);

        UnityEngine.Debug.Log("DEB END2");
    }

    async UniTask Start()
    {
        NativeArray<int> result = new NativeArray<int>(1, Allocator.TempJob);
        gxhs jobData = new gxhs();
        jobData.a = 10;
        jobData.b = 10;
        jobData.result = result;
        JobHandle handle = jobData.Schedule();
        await handle;
        Debug.Log(result[0]);
        result.Dispose();
        await Test1();
        Test2();
        UnityEngine.Debug.Log("Start");
    }

    // Update is called once per frame
    void Update()
    {
    }
}