using System;

namespace GameFrame
{
    public static partial class ByteHelper
    {
        public static void WriteInt32(this byte[] buffer, Int32 value, int offset = 0, bool bigEndian = DefaultBigEndian)
        {
            Assert.IsTrue(offset >= 0 && offset + 4 <= buffer.Length, $"Index out of range");
            if (bigEndian)
            {
                buffer[offset + 0] = (byte)(value >> 24);
                buffer[offset + 1] = (byte)(value >> 16);
                buffer[offset + 2] = (byte)(value >> 8);
                buffer[offset + 3] = (byte)value;
            }
            else
            {
                buffer[offset + 0] = (byte)value;
                buffer[offset + 1] = (byte)(value >> 8);
                buffer[offset + 2] = (byte)(value >> 16);
                buffer[offset + 3] = (byte)(value >> 24);
            }
        }

        public static void WriteUInt16(this byte[] buffer, UInt16 value, int offset = 0, bool bigEndian = DefaultBigEndian)
        {
            Assert.IsTrue(offset >= 0 && offset + 2 <= buffer.Length, $"Index out of range");
            if (bigEndian)
            {
                buffer[offset + 0] = (byte)(value >> 8);
                buffer[offset + 1] = (byte)value;
            }
            else
            {
                buffer[offset + 0] = (byte)value;
                buffer[offset + 1] = (byte)(value >> 8);
            }
        }
        
        public static Int32 ReadInt32(this byte[] buffer, int offset = 0, bool bigEndian = DefaultBigEndian)
        {
            Assert.IsTrue(offset >= 0 && offset + 4 <= buffer.Length, $"Index out of range");
            if (bigEndian)
            {
                return (buffer[offset + 0] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];
            }
            else
            {
                return (buffer[offset + 3] << 24) | (buffer[offset + 2] << 16) | (buffer[offset + 1] << 8) | buffer[offset + 0];
            }
        }
        
        public static UInt16 ReadUInt16(this byte[] buffer, int offset = 0, bool bigEndian = DefaultBigEndian)
        {
            Assert.IsTrue(offset >= 0 && offset + 2 <= buffer.Length, $"Index out of range");
            if (bigEndian)
            {
                return (UInt16)((buffer[offset + 0] << 8) | buffer[offset + 1]);
            }
            else
            {
                return (UInt16)((buffer[offset + 1] << 8) | buffer[offset + 0]);
            }
        }
    }
}