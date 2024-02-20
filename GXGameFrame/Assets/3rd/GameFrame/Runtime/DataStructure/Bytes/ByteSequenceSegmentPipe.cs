using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace GameFrame
{
    public class ByteSequenceSegmentPipe : IReference
    {
        private ByteSequenceSegment m_Head; // read
        private ByteSequenceSegment m_Tail; // write

        private int m_HeadPosition;
        private int m_TailPosition;
        
        private int HeadRemain => ByteSequenceSegment.SegmentCapacity - m_HeadPosition;
        private int TailRemain => ByteSequenceSegment.SegmentCapacity - m_TailPosition;
        
        public int Length { get; private set; }

        public ByteSequenceSegmentPipe()
        {
            m_HeadPosition = ByteSequenceSegment.SegmentCapacity;
            m_TailPosition = ByteSequenceSegment.SegmentCapacity;
            Length = 0;
        }
        
        private void CheckWrite()
        {
            if (TailRemain > 0)
                return;
            
            var next = ReferencePool.Acquire<ByteSequenceSegment>();
            if (m_Tail == null)
            {
                m_Head = m_Tail = next;
                m_HeadPosition = m_TailPosition = 0;
            }
            else
            {
                m_Tail.SetNext(next);
                m_Tail = next;
                m_TailPosition = 0;
            }
        }

        private void CheckRead()
        {
            if (HeadRemain > 0 && Length > 0)
                return;

            var next = m_Head.Next;
            ReferencePool.Release(m_Head);
            if (next == null)
            {
                m_Head = m_Tail = null;
                m_HeadPosition = m_TailPosition = ByteSequenceSegment.SegmentCapacity;
            }
            else
            {
                m_Head = (ByteSequenceSegment)next;
                m_HeadPosition = 0;
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
                Array.Copy(buffer, offset, m_Tail.Buffer, m_TailPosition, k);
                offset += k;
                count -= k;
                m_TailPosition += k;
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
                Marshal.Copy(buffer, m_Tail.Buffer, m_TailPosition, k);
                buffer = IntPtr.Add(buffer, k);
                count -= k;
                m_TailPosition += k;
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
                k = stream.Read(m_Tail.Buffer, m_TailPosition, k);
                count -= k;
                m_TailPosition += k;
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
                Array.Copy(m_Head.Buffer, m_HeadPosition, buffer, offset, k);
                offset += k;
                count -= k;
                m_HeadPosition += k;
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

            var node = m_Head;
            var position = m_HeadPosition;

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
                m_HeadPosition += k;
            }
            
            Length -= n;
            
            CheckRead(); // recycle empty head
            
            return n;
        }

        public void Clear()
        {
            Advance(Length);
        }

        public ReadOnlySequence<byte> CreateSequence()
        {
            if (Length == 0) return new ReadOnlySequence<byte>();
            return new ReadOnlySequence<byte>(m_Head, m_HeadPosition, m_Tail, m_TailPosition);
        }

        public ReadOnlySequence<byte> CreateSequence(int offset, int count)
        {
            if (count <= 0) return new ReadOnlySequence<byte>();
            if (Length < offset + count) throw new ArgumentOutOfRangeException(); 

            var sequence = new ReadOnlySequence<byte>(m_Head, m_HeadPosition, m_Tail, m_TailPosition);
            sequence = sequence.Slice(offset, count);
            return sequence;
        }
    }
}