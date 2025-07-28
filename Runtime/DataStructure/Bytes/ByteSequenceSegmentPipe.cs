using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace GameFrame.Runtime
{
    public class ByteSequenceSegmentPipe : IDisposable
    {
        private ByteSequenceSegment head; // read
        private ByteSequenceSegment tail; // write

        private int headPosition;
        private int tailPosition;
        
        private int HeadRemain => ByteSequenceSegment.SegmentCapacity - headPosition;
        private int TailRemain => ByteSequenceSegment.SegmentCapacity - tailPosition;
        
        public int Length { get; private set; }

        public ByteSequenceSegmentPipe()
        {
            headPosition = ByteSequenceSegment.SegmentCapacity;
            tailPosition = ByteSequenceSegment.SegmentCapacity;
            Length = 0;
        }
        
        private void CheckWrite()
        {
            if (TailRemain > 0)
                return;
            
            var next = ReferencePool.Acquire<ByteSequenceSegment>();
            if (tail == null)
            {
                head = tail = next;
                headPosition = tailPosition = 0;
            }
            else
            {
                tail.SetNext(next);
                tail = next;
                tailPosition = 0;
            }
        }

        private void CheckRead()
        {
            if (HeadRemain > 0 && Length > 0)
                return;

            var next = head.Next;
            ReferencePool.Release(head);
            if (next == null)
            {
                head = tail = null;
                headPosition = tailPosition = ByteSequenceSegment.SegmentCapacity;
            }
            else
            {
                head = (ByteSequenceSegment)next;
                headPosition = 0;
            }
        }
        
        public void Write(byte[] buffer, int offset, int count)
        {
            if (count < 0 || buffer.Length < offset + count) throw new ArgumentOutOfRangeException();

            int n = count;
            
            while (count > 0)
            {
                CheckWrite();
                int k = TailRemain >= count ? count : TailRemain;
                Array.Copy(buffer, offset, tail.Buffer, tailPosition, k);
                offset += k;
                count -= k;
                tailPosition += k;
            }
            
            Length += n;
        }
        
        public void Write(IntPtr buffer, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException();

            int n = count;
            
            while (count > 0)
            {
                CheckWrite();
                int k = TailRemain >= count ? count : TailRemain;
                Marshal.Copy(buffer, tail.Buffer, tailPosition, k);
                buffer = IntPtr.Add(buffer, k);
                count -= k;
                tailPosition += k;
            }
            
            Length += n;
        }
        
        public void Write(Stream stream)
        {
            int count = (int)(stream.Length - stream.Position);
            
            int n = count;

            while (count > 0)
            {
                CheckWrite();
                int k = TailRemain >= count ? count : TailRemain;
                k = stream.Read(tail.Buffer, tailPosition, k);
                count -= k;
                tailPosition += k;
            }
            
            Length += n;
        }
        
        public int Read(byte[] buffer, int offset, int count)
        {
            if (count < 0 || buffer.Length < offset + count) throw new ArgumentOutOfRangeException();
            
            if (Length < count) count = Length;
            if (count <= 0) return 0;
            
            int n = count;
            
            while (count > 0)
            {
                CheckRead();
                int k = HeadRemain >= count ? count : HeadRemain;
                Array.Copy(head.Buffer, headPosition, buffer, offset, k);
                offset += k;
                count -= k;
                headPosition += k;
            }
            
            Length -= n;
            
            CheckRead(); // recycle empty head
            
            return n;
        }

        /// <summary>
        /// Read buffer without ddvance position
        /// </summary>
        public int Peek(byte[] buffer, int offset, int count)
        {
            if (count < 0 || buffer.Length < offset + count) throw new ArgumentOutOfRangeException();
            
            if (Length < count) count = Length;
            if (count <= 0) return 0;
            
            int n = count;

            var node = head;
            var position = headPosition;

            while (count > 0)
            {
                int remain = ByteSequenceSegment.SegmentCapacity - position;
                if (remain == 0)
                {
                    node = (ByteSequenceSegment)node.Next;
                    position = 0;
                    remain = ByteSequenceSegment.SegmentCapacity;
                }
                int k = remain >= count ? count : remain;
                Array.Copy(node.Buffer, position, buffer, offset, k);
                offset += k;
                count -= k;
                position += k;
            }
            
            return n;
        }
        
        /// <summary>
        /// Advance read position only
        /// </summary>
        public int Advance(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException();

            if (Length < count) count = Length;
            if (count <= 0) return 0;
            
            int n = count;
            
            while (count > 0)
            {
                CheckRead();
                int k = HeadRemain >= count ? count : HeadRemain;
                count -= k;
                headPosition += k;
            }
            
            Length -= n;
            
            CheckRead(); // recycle empty head
            
            return n;
        }

        public void Dispose()
        {
            Advance(Length);
        }

        public ReadOnlySequence<byte> CreateSequence()
        {
            if (Length == 0) return new ReadOnlySequence<byte>();
            return new ReadOnlySequence<byte>(head, headPosition, tail, tailPosition);
        }

        public ReadOnlySequence<byte> CreateSequence(int offset, int count)
        {
            if (count <= 0) return new ReadOnlySequence<byte>();
            if (Length < offset + count) throw new ArgumentOutOfRangeException(); 

            var sequence = new ReadOnlySequence<byte>(head, headPosition, tail, tailPosition);
            sequence = sequence.Slice(offset, count);
            return sequence;
        }
    }
}