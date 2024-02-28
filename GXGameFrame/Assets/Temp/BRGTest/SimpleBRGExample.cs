using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

// The PackedMatrix is a convenience type that converts matrices into
// the format that Unity-provided SRP shaders expect.
struct PackedMatrix
{
    public float c0x;
    public float c0y;
    public float c0z;
    public float c1x;
    public float c1y;
    public float c1z;
    public float c2x;
    public float c2y;
    public float c2z;
    public float c3x;
    public float c3y;
    public float c3z;
 
    public PackedMatrix(Matrix4x4 m)
    {
        c0x = m.m00;
        c0y = m.m10;
        c0z = m.m20;
        c1x = m.m01;
        c1y = m.m11;
        c1z = m.m21;
        c2x = m.m02;
        c2y = m.m12;
        c2z = m.m22;
        c3x = m.m03;
        c3y = m.m13;
        c3z = m.m23;
    }
 
    public void SetData(Matrix4x4 m)
    {
        c0x = m.m00;
        c0y = m.m10;
        c0z = m.m20;
        c1x = m.m01;
        c1y = m.m11;
        c1z = m.m21;
        c2x = m.m02;
        c2y = m.m12;
        c2z = m.m22;
        c3x = m.m03;
        c3y = m.m13;
        c3z = m.m23;
    }
}
public class SimpleBRGExample : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    // public TextMeshProUGUI text;
    // public TextMeshProUGUI workerCountText;
    private BatchRendererGroup m_BRG;
 
    private GraphicsBuffer m_InstanceData;
    private BatchID m_BatchID;
    private BatchMeshID m_MeshID;
    private BatchMaterialID m_MaterialID;
 
    // Some helper constants to make calculations more convenient.
    private const int kSizeOfMatrix = sizeof(float) * 4 * 4;
    private const int kSizeOfPackedMatrix = sizeof(float) * 4 * 3;
    private const int kSizeOfFloat4 = sizeof(float) * 4;
    private const int kBytesPerInstance = (kSizeOfPackedMatrix * 2) + kSizeOfFloat4;
    private const int kExtraBytes = kSizeOfMatrix * 2;
    [SerializeField] private int kNumInstances = 20000;
    [SerializeField] private int m_RowCount = 200;
    private Matrix4x4[] matrices;
    private PackedMatrix[] objectToWorld;
    private PackedMatrix[] worldToObject;
    private Vector4[] colors;
 
    private void Start()
    {
        m_BRG = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);
        m_MeshID = m_BRG.RegisterMesh(mesh);
        m_MaterialID = m_BRG.RegisterMaterial(material);
        AllocateInstanceDateBuffer();
        PopulateInstanceDataBuffer();
 
        // text.text = kNumInstances.ToString();
        random = new Unity.Mathematics.Random(1);
        m_TargetPoints = new float3[kNumInstances];
        var offset = new Vector3(m_RowCount, 0, Mathf.CeilToInt(kNumInstances / (float)m_RowCount)) * 0.5f;
        m_randomRange = new float4(-offset.x, offset.x, -offset.z, offset.z);
        for (int i = 0; i < m_TargetPoints.Length; i++)
        {
            var newTargetPos = new float3();
            newTargetPos.x = random.NextFloat(m_randomRange.x, m_randomRange.y);
            newTargetPos.z = random.NextFloat(m_randomRange.z, m_randomRange.w);
            m_TargetPoints[i] = newTargetPos;
        }
        
    }
 
    float3[] m_TargetPoints;
    Unity.Mathematics.Random random;
    Vector4 m_randomRange;
    private uint byteAddressObjectToWorld;
    private uint byteAddressWorldToObject;
    private uint byteAddressColor;
 
    private void Update()
    {
        NativeArray<Matrix4x4> tempMatrices = new NativeArray<Matrix4x4>(matrices, Allocator.TempJob);
        NativeArray<float3> tempTargetPoints = new NativeArray<float3>(m_TargetPoints, Allocator.TempJob);//worldToObject
        NativeArray<PackedMatrix> tempobjectToWorldArr = new NativeArray<PackedMatrix>(matrices.Length, Allocator.TempJob);
        NativeArray<PackedMatrix> tempWorldToObjectArr = new NativeArray<PackedMatrix>(matrices.Length, Allocator.TempJob);
        random = new Unity.Mathematics.Random((uint)Time.frameCount);
        var moveJob = new RandomMoveJob
        {
            matrices = tempMatrices,
            targetMovePoints = tempTargetPoints,
            random = random,
            m_DeltaTime = Time.deltaTime * 4f,
            randomPostionRange = m_randomRange,
            obj2WorldArr = tempobjectToWorldArr,
            world2ObjArr = tempWorldToObjectArr
        };
        var moveJobHandle = moveJob.Schedule(tempMatrices.Length, 64);
        moveJobHandle.Complete();
        matrices = moveJob.matrices.ToArray();
        m_TargetPoints = moveJob.targetMovePoints.ToArray();
        m_InstanceData.SetData(moveJob.obj2WorldArr, 0, (int)(byteAddressObjectToWorld / kSizeOfPackedMatrix), objectToWorld.Length);
        m_InstanceData.SetData(moveJob.world2ObjArr, 0, (int)(byteAddressWorldToObject / kSizeOfPackedMatrix), worldToObject.Length);
        tempMatrices.Dispose();
        tempTargetPoints.Dispose();
        tempobjectToWorldArr.Dispose();
        tempWorldToObjectArr.Dispose();
 
        // workerCountText.text = $"JobWorkerCount:{JobsUtility.JobWorkerCount}";
    }
 
    private void AllocateInstanceDateBuffer()
    {
        m_InstanceData = new GraphicsBuffer(GraphicsBuffer.Target.Raw,
            BufferCountForInstances(kBytesPerInstance, kNumInstances, kExtraBytes),
            sizeof(int));
    }
    private void RefreshData()
    {
        m_InstanceData.SetData(objectToWorld, 0, (int)(byteAddressObjectToWorld / kSizeOfPackedMatrix), objectToWorld.Length);
        m_InstanceData.SetData(worldToObject, 0, (int)(byteAddressWorldToObject / kSizeOfPackedMatrix), worldToObject.Length);
    }
    private void PopulateInstanceDataBuffer()
    {
        // Place a zero matrix at the start of the instance data buffer, so loads from address 0 return zero.
        var zero = new Matrix4x4[1] { Matrix4x4.zero };
 
        // Create transform matrices for three example instances.
        matrices = new Matrix4x4[kNumInstances];
        // Convert the transform matrices into the packed format that shaders expects.
        objectToWorld = new PackedMatrix[kNumInstances];
        // Also create packed inverse matrices.
        worldToObject = new PackedMatrix[kNumInstances];
        // Make all instances have unique colors.
        colors = new Vector4[kNumInstances];
 
        var offset = new Vector3(m_RowCount, 0, Mathf.CeilToInt(kNumInstances / (float)m_RowCount)) * 0.5f;
        for (int i = 0; i < kNumInstances; i++)
        {
            matrices[i] = Matrix4x4.Translate(new Vector3(i % m_RowCount, 0, i / m_RowCount) - offset);
            objectToWorld[i] = new PackedMatrix(matrices[i]);
            worldToObject[i] = new PackedMatrix(matrices[0].inverse);
            colors[i] = UnityEngine.Random.ColorHSV();
        }
 
        // In this simple example, the instance data is placed into the buffer like this:
        // Offset | Description
        //      0 | 64 bytes of zeroes, so loads from address 0 return zeroes
        //     64 | 32 uninitialized bytes to make working with SetData easier, otherwise unnecessary
        //     96 | unity_ObjectToWorld, three packed float3x4 matrices
        //    240 | unity_WorldToObject, three packed float3x4 matrices
        //    384 | _BaseColor, three float4s
 
        // Calculates start addresses for the different instanced properties. unity_ObjectToWorld starts at 
        // address 96 instead of 64 which means 32 bits are left uninitialized. This is because the 
        // computeBufferStartIndex parameter requires the start offset to be divisible by the size of the source
        // array element type. In this case, it's the size of PackedMatrix, which is 48.
        byteAddressObjectToWorld = kSizeOfPackedMatrix * 2;
        byteAddressWorldToObject = byteAddressObjectToWorld + kSizeOfPackedMatrix * (uint)kNumInstances;
        byteAddressColor = byteAddressWorldToObject + kSizeOfPackedMatrix * (uint)kNumInstances;
 
        // Upload the instance data to the GraphicsBuffer so the shader can load them.
        m_InstanceData.SetData(zero, 0, 0, 1);
        m_InstanceData.SetData(objectToWorld, 0, (int)(byteAddressObjectToWorld / kSizeOfPackedMatrix), objectToWorld.Length);
        m_InstanceData.SetData(worldToObject, 0, (int)(byteAddressWorldToObject / kSizeOfPackedMatrix), worldToObject.Length);
        m_InstanceData.SetData(colors, 0, (int)(byteAddressColor / kSizeOfFloat4), colors.Length);
 
        // Set up metadata values to point to the instance data. Set the most significant bit 0x80000000 in each
        // which instructs the shader that the data is an array with one value per instance, indexed by the instance index.
        // Any metadata values that the shader uses and not set here will be zero. When such a value is used with
        // UNITY_ACCESS_DOTS_INSTANCED_PROP (i.e. without a default), the shader interprets the
        // 0x00000000 metadata value and loads from the start of the buffer. The start of the buffer which is
        // is a zero matrix so this sort of load is guaranteed to return zero, which is a reasonable default value.
        var metadata = new NativeArray<MetadataValue>(3, Allocator.Temp);
        metadata[0] = new MetadataValue { NameID = Shader.PropertyToID("unity_ObjectToWorld"), Value = 0x80000000 | byteAddressObjectToWorld, };
        metadata[1] = new MetadataValue { NameID = Shader.PropertyToID("unity_WorldToObject"), Value = 0x80000000 | byteAddressWorldToObject, };
        metadata[2] = new MetadataValue { NameID = Shader.PropertyToID("_BaseColor"), Value = 0x80000000 | byteAddressColor, };
 
        // Finally, create a batch for the instances, and make the batch use the GraphicsBuffer with the
        // instance data, as well as the metadata values that specify where the properties are. 
        m_BatchID = m_BRG.AddBatch(metadata, m_InstanceData.bufferHandle);
    }
 
    // Raw buffers are allocated in ints. This is a utility method that calculates
    // the required number of ints for the data.
    int BufferCountForInstances(int bytesPerInstance, int numInstances, int extraBytes = 0)
    {
        // Round byte counts to int multiples
        bytesPerInstance = (bytesPerInstance + sizeof(int) - 1) / sizeof(int) * sizeof(int);
        extraBytes = (extraBytes + sizeof(int) - 1) / sizeof(int) * sizeof(int);
        int totalBytes = bytesPerInstance * numInstances + extraBytes;
        return totalBytes / sizeof(int);
    }
 
 
    private void OnDisable()
    {
        m_BRG.Dispose();
    }
 
    public unsafe JobHandle OnPerformCulling(
        BatchRendererGroup rendererGroup,
        BatchCullingContext cullingContext,
        BatchCullingOutput cullingOutput,
        IntPtr userContext)
    {
        // UnsafeUtility.Malloc() requires an alignment, so use the largest integer type's alignment
        // which is a reasonable default.
        int alignment = UnsafeUtility.AlignOf<long>();
 
        // Acquire a pointer to the BatchCullingOutputDrawCommands struct so you can easily
        // modify it directly.
        var drawCommands = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();
        // Allocate memory for the output arrays. In a more complicated implementation, you would calculate
        // the amount of memory to allocate dynamically based on what is visible.
        // This example assumes that all of the instances are visible and thus allocates
        // memory for each of them. The necessary allocations are as follows:
        // - a single draw command (which draws kNumInstances instances)
        // - a single draw range (which covers our single draw command)
        // - kNumInstances visible instance indices.
        // You must always allocate the arrays using Allocator.TempJob.
        drawCommands->drawCommands = (BatchDrawCommand*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawCommand>(), alignment, Allocator.TempJob);
        drawCommands->drawRanges = (BatchDrawRange*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawRange>(), alignment, Allocator.TempJob);
        drawCommands->visibleInstances = (int*)UnsafeUtility.Malloc(kNumInstances * sizeof(int), alignment, Allocator.TempJob);
        drawCommands->drawCommandPickingInstanceIDs = null;
 
        drawCommands->drawCommandCount = 1;
        drawCommands->drawRangeCount = 1;
        drawCommands->visibleInstanceCount = kNumInstances;
 
        // This example doens't use depth sorting, so it leaves instanceSortingPositions as null.
        drawCommands->instanceSortingPositions = null;
        drawCommands->instanceSortingPositionFloatCount = 0;
 
        // Configure the single draw command to draw kNumInstances instances
        // starting from offset 0 in the array, using the batch, material and mesh
        // IDs registered in the Start() method. It doesn't set any special flags.
        drawCommands->drawCommands[0].visibleOffset = 0;
        drawCommands->drawCommands[0].visibleCount = (uint)kNumInstances;
        drawCommands->drawCommands[0].batchID = m_BatchID;
        drawCommands->drawCommands[0].materialID = m_MaterialID;
        drawCommands->drawCommands[0].meshID = m_MeshID;
        drawCommands->drawCommands[0].submeshIndex = 0;
        drawCommands->drawCommands[0].splitVisibilityMask = 0xff;
        drawCommands->drawCommands[0].flags = 0;
        drawCommands->drawCommands[0].sortingPosition = 0;
 
        // Configure the single draw range to cover the single draw command which
        // is at offset 0.
        drawCommands->drawRanges[0].drawCommandsBegin = 0;
        drawCommands->drawRanges[0].drawCommandsCount = 1;
 
        // This example doesn't care about shadows or motion vectors, so it leaves everything
        // at the default zero values, except the renderingLayerMask which it sets to all ones
        // so Unity renders the instances regardless of mask settings.
        drawCommands->drawRanges[0].filterSettings = new BatchFilterSettings { renderingLayerMask = 0xffffffff, };
 
        // Finally, write the actual visible instance indices to the array. In a more complicated
        // implementation, this output would depend on what is visible, but this example
        // assumes that everything is visible.
        for (int i = 0; i < kNumInstances; ++i)
            drawCommands->visibleInstances[i] = i;
 
        // This simple example doesn't use jobs, so it returns an empty JobHandle.
        // Performance-sensitive applications are encouraged to use Burst jobs to implement
        // culling and draw command output. In this case, this function returns a
        // handle here that completes when the Burst jobs finish.
        return new JobHandle();
    }
}
[BurstCompile]
partial struct RandomMoveJob : IJobParallelFor
{
    [ReadOnly]
    public Unity.Mathematics.Random random;
    [ReadOnly]
    public float4 randomPostionRange;
    [ReadOnly]
    public float m_DeltaTime;
 
    public NativeArray<Matrix4x4> matrices;
    public NativeArray<float3> targetMovePoints;
    public NativeArray<PackedMatrix> obj2WorldArr;
    public NativeArray<PackedMatrix> world2ObjArr;
    [BurstCompile]
    public void Execute(int index)
    {
        float3 curPos = matrices[index].GetPosition();
        float3 dir = targetMovePoints[index] - curPos;
        if (Unity.Mathematics.math.lengthsq(dir) < 0.4f)
        {
            var newTargetPos = targetMovePoints[index];
            newTargetPos.x = random.NextFloat(randomPostionRange.x, randomPostionRange.y);
            newTargetPos.z = random.NextFloat(randomPostionRange.z, randomPostionRange.w);
            targetMovePoints[index] = newTargetPos;
        }
 
        dir = math.normalizesafe(targetMovePoints[index] - curPos, Vector3.forward);
        curPos += dir * m_DeltaTime;// math.lerp(curPos, targetMovePoints[index], m_DeltaTime);
 
        var mat = matrices[index];
        mat.SetTRS(curPos, Quaternion.LookRotation(dir), Vector3.one);
        matrices[index] = mat;
        var item = obj2WorldArr[index];
        item.SetData(mat);
        obj2WorldArr[index] = item;
 
        item = world2ObjArr[index];
        item.SetData(mat.inverse);
        world2ObjArr[index] = item;
    }
}