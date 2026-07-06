using System;
using System.Collections.Generic;
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

        public static byte[] SerializeArray<T>(IReadOnlyList<T> values) where T : unmanaged
        {
            if (values == null || values.Count == 0)
            {
                return Array.Empty<byte>();
            }

            var itemSize = sizeof(T);
            var byteCount = itemSize * values.Count;
            var bytes = new byte[byteCount];

            fixed (byte* destination = bytes)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    var copy = values[i];
                    Buffer.MemoryCopy(&copy, destination + i * itemSize, itemSize, itemSize);
                }
            }

            return bytes;
        }

        public static T[] DeserializeArray<T>(byte[] bytes) where T : unmanaged
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var itemSize = sizeof(T);
            if (bytes.Length == 0)
            {
                return Array.Empty<T>();
            }

            if (bytes.Length % itemSize != 0)
            {
                throw new InvalidDataException($"{typeof(T).Name} binary array size mismatch. Item size {itemSize} bytes, got {bytes.Length} bytes.");
            }

            var count = bytes.Length / itemSize;
            var values = new T[count];
            fixed (byte* source = bytes)
            {
                for (int i = 0; i < count; i++)
                {
                    T value = default;
                    Buffer.MemoryCopy(source + i * itemSize, &value, itemSize, itemSize);
                    values[i] = value;
                }
            }

            return values;
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


        public static void WriteArrayToFile<T>(string filePath, IReadOnlyList<T> values) where T : unmanaged
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filePath, SerializeArray(values));
        }

        public static T[] ReadArrayFromFile<T>(string filePath) where T : unmanaged
        {
            return DeserializeArray<T>(File.ReadAllBytes(filePath));
        }
    }
}
