using System;
using System.Buffers;

namespace GameFrame
{
    public class ByteSequenceSegment : ReadOnlySequenceSegment<byte>,IReference
    {
        public const int SegmentCapacity = 512;

        public byte[] Buffer { get; private set; }
        
        public ByteSequenceSegment()
        {
            Buffer = new byte[SegmentCapacity];
            Memory = new ReadOnlyMemory<byte>(Buffer);
        }
        
        public void SetNext(ByteSequenceSegment next)
        {
            next.RunningIndex = RunningIndex + Memory.Length;
            Next = next;
        }

        public void Clear()
        {
            Next = null;
            RunningIndex = 0;
        }
    }
}