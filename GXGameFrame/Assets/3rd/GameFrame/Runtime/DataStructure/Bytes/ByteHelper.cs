using System;

namespace GameFrame
{
    public static partial class ByteHelper
    {
        public const bool DefaultBigEndian = true;
        
        public static readonly byte[] SharedBuffer4 = new byte[4];
        
        public static void WriteInt32(this ByteSequenceSegmentPipe pipe, Int32 value, bool bigEndian = DefaultBigEndian)
        {
            var sharedBuffer = SharedBuffer4;
            if (bigEndian)
            {
                sharedBuffer[0] = (byte)(value >> 24);
                sharedBuffer[1] = (byte)(value >> 16);
                sharedBuffer[2] = (byte)(value >> 8);
                sharedBuffer[3] = (byte)value;
            }
            else
            {
                sharedBuffer[0] = (byte)value;
                sharedBuffer[1] = (byte)(value >> 8);
                sharedBuffer[2] = (byte)(value >> 16);
                sharedBuffer[3] = (byte)(value >> 24);
            }
            pipe.Write(sharedBuffer, 0, 4);
        }

        public static void WriteUInt16(this ByteSequenceSegmentPipe pipe, UInt16 value, bool bigEndian = DefaultBigEndian)
        {
            var sharedBuffer = SharedBuffer4;
            if (bigEndian)
            {
                sharedBuffer[0] = (byte)(value >> 8);
                sharedBuffer[1] = (byte)value;
            }
            else
            {
                sharedBuffer[0] = (byte)value;
                sharedBuffer[1] = (byte)(value >> 8);
            }
            pipe.Write(sharedBuffer, 0, 2);
        }
        
        public static void WriteByte(this ByteSequenceSegmentPipe pipe, byte value)
        {
            var sharedBuffer = SharedBuffer4;
            sharedBuffer[0] = value;
            pipe.Write(sharedBuffer, 0, 1);
        }

        public static Int32 ReadInt32(this ByteSequenceSegmentPipe pipe, bool bigEndian = DefaultBigEndian)
        {
            var sharedBuffer =SharedBuffer4;
            pipe.Read(sharedBuffer, 0, 4);
            if (bigEndian)
            {
                return (sharedBuffer[0] << 24) | (sharedBuffer[1] << 16) | (sharedBuffer[2] << 8) | sharedBuffer[3];
            }
            else
            {
                return (sharedBuffer[3] << 24) | (sharedBuffer[2] << 16) | (sharedBuffer[1] << 8) | sharedBuffer[0];
            }
        }
        
        public static UInt16 ReadUInt16(this ByteSequenceSegmentPipe pipe, bool bigEndian = DefaultBigEndian)
        {
            var sharedBuffer = SharedBuffer4;
            pipe.Read(sharedBuffer, 0, 2);
            if (bigEndian)
            {
                return (UInt16)((sharedBuffer[0] << 8) | sharedBuffer[1]);
            }
            else
            {
                return (UInt16)((sharedBuffer[1] << 8) | sharedBuffer[0]);
            }
        }
        
        public static byte ReadByte(this ByteSequenceSegmentPipe pipe)
        {
            var sharedBuffer = SharedBuffer4;
            pipe.Read(sharedBuffer, 0, 1);
            return sharedBuffer[0];
        }
        
        /// <summary>
        /// Read int32 without ddvance position
        /// </summary>
        public static Int32 PeekInt32(this ByteSequenceSegmentPipe pipe, bool bigEndian = DefaultBigEndian)
        {
            var sharedBuffer = SharedBuffer4;
            pipe.Peek(sharedBuffer, 0, 4);
            if (bigEndian)
            {
                return (sharedBuffer[0] << 24) | (sharedBuffer[1] << 16) | (sharedBuffer[2] << 8) | sharedBuffer[3];
            }
            else
            {
                return (sharedBuffer[3] << 24) | (sharedBuffer[2] << 16) | (sharedBuffer[1] << 8) | sharedBuffer[0];
            }
        }
        
        /// <summary>
        /// Read uint16 without ddvance position
        /// </summary>    
        public static UInt16 PeekUInt16(this ByteSequenceSegmentPipe pipe, bool bigEndian = DefaultBigEndian)
        {
            var sharedBuffer = SharedBuffer4;
            pipe.Peek(sharedBuffer, 0, 2);
            if (bigEndian)
            {
                return (UInt16)((sharedBuffer[0] << 8) | sharedBuffer[1]);
            }
            else
            {
                return (UInt16)((sharedBuffer[1] << 8) | sharedBuffer[0]);
            }
        }
        
        /// <summary>
        /// Read byte without ddvance position
        /// </summary>    
        public static byte PeekByte(this ByteSequenceSegmentPipe pipe)
        {
            var sharedBuffer = SharedBuffer4;
            pipe.Peek(sharedBuffer, 0, 1);
            return sharedBuffer[0];
        }
    }
}