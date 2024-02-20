using System;
using System.Buffers;
using System.Diagnostics;
using GameFrame;
using NUnit.Framework;
using Debug = UnityEngine.Debug;

public class TestScript
{
    [Test]
    public void Bytes()
    {
        ByteSequenceSegmentPipe pipe = ReferencePool.Acquire<ByteSequenceSegmentPipe>();
        pipe.Write(new byte[] {1,2,3,4,5,6,7,8,9,10}, 0, 10);
        var sequence = pipe.CreateSequence();
        
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 100000; i++)
        {
            var buffer = new byte[10];
            sequence.CopyTo(new Span<byte>(buffer, 0, 10));   
        }
        stopwatch.Stop();
        Debug.Log("copyto"  +(stopwatch.ElapsedMilliseconds));

        stopwatch.Reset();
        
        stopwatch.Start();
        for (int i = 0; i < 100000; i++)
        {
            var buffer =   sequence.ToArray();
        }
        stopwatch.Stop();
        Debug.Log("toarray"  +(stopwatch.ElapsedMilliseconds));
    }
    
    
}