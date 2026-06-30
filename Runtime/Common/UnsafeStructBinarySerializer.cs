using System;
using System.IO;

namespace GameFrame.Runtime
{
    public static unsafe class UnsafeStructBinarySerializer
    {
        public static int SizeOf<T>() where T : unmanaged
        {
            return sizeof(T);
        }

        public static byte[] Serialize<T>(in T value) where T : unmanaged
        {
            var size = sizeof(T);
            var bytes = new byte[size];
            var copy = value;

            fixed (byte* destination = bytes)
            {
                Buffer.MemoryCopy(&copy, destination, size, size);
            }

            return bytes;
        }

        public static T Deserialize<T>(byte[] bytes) where T : unmanaged
        {
            var size = sizeof(T);
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (bytes.Length != size)
            {
                throw new InvalidDataException($"{typeof(T).Name} binary size mismatch. Expected {size} bytes, got {bytes.Length} bytes.");
            }

            T value = default;
            fixed (byte* source = bytes)
            {
                Buffer.MemoryCopy(source, &value, size, size);
            }

            return value;
        }

        public static void WriteToFile<T>(string filePath, in T value) where T : unmanaged
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filePath, Serialize(value));
        }

        public static T ReadFromFile<T>(string filePath) where T : unmanaged
        {
            return Deserialize<T>(File.ReadAllBytes(filePath));
        }
    }
}