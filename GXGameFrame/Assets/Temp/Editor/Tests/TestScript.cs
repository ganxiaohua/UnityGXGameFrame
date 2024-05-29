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
        pipe.Write(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, 0, 10);
        var sequence = pipe.CreateSequence();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 100000; i++)
        {
            var buffer = new byte[10];
            sequence.CopyTo(new Span<byte>(buffer, 0, 10));
        }

        stopwatch.Stop();
        Debug.Log("copyto" + (stopwatch.ElapsedMilliseconds));

        stopwatch.Reset();

        stopwatch.Start();
        for (int i = 0; i < 100000; i++)
        {
            var buffer = sequence.ToArray();
        }

        stopwatch.Stop();
        Debug.Log("toarray" + (stopwatch.ElapsedMilliseconds));
    }

    [Test]
    public void Quest()
    {
        QueryList<int> queryList = new QueryList<int>(0, true);
        queryList.Add(1);
        queryList.Add(2);
        queryList.Add(3);
        queryList.Add(4);
        queryList.Add(5);
        queryList.Add(6);
        foreach (var item in queryList)
        {
            if (item == 2)
            {
                queryList.Remove(3);
            }

            Debug.Log(item);
        }
    }
}