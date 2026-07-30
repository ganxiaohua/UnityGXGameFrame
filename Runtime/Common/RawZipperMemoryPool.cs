#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;

namespace GameFrame.Runtime
{
    internal sealed class RawZipperMemoryPool : IDisposable
    {
        private readonly List<RawZipperMemory> memories = new();

        private bool disposed;

        public RawZipperMemory Rent(long requiredCapacity, int requiredAlignment)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RawZipperMemoryPool));
            if (requiredCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(requiredCapacity));

            requiredAlignment = RawZipperMemory.NormalizeAlignment(requiredAlignment);

            RawZipperMemory bestFit = null;
            RawZipperMemory expandable = null;
            for (int i = 0; i < memories.Count; i++)
            {
                RawZipperMemory memory = memories[i];
                if (memory.IsInUse)
                    continue;

                if (memory.CanFit(requiredCapacity, requiredAlignment))
                {
                    if (bestFit == null || memory.Capacity < bestFit.Capacity)
                        bestFit = memory;
                }
                else if (expandable == null || memory.Capacity > expandable.Capacity)
                {
                    expandable = memory;
                }
            }

            RawZipperMemory result = bestFit ?? expandable;
            if (result == null)
            {
                result = new RawZipperMemory();
                memories.Add(result);
            }

            try
            {
                result.EnsureCapacity(requiredCapacity, requiredAlignment);
                result.MarkInUse();
                return result;
            }
            catch
            {
                if (result.Capacity == 0)
                {
                    memories.Remove(result);
                    result.Dispose();
                }

                throw;
            }
        }

        public void Return(RawZipperMemory memory)
        {
            if (memory == null)
                return;
            if (disposed)
                throw new ObjectDisposedException(nameof(RawZipperMemoryPool));

            memory.MarkAvailable();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            for (int i = 0; i < memories.Count; i++)
                memories[i].Dispose();
            memories.Clear();
        }
    }

    internal sealed class RawZipperMemory : IDisposable
    {
        private const long MinimumBufferCapacity = 256;
        private const int MinimumBufferAlignment = 16;

        public IntPtr Buffer { get; private set; }

        public long Capacity { get; private set; }

        public int Alignment { get; private set; }

        public bool IsInUse { get; private set; }

        private IntPtr command;

        private bool disposed;

        public unsafe RawZipperMemory()
        {
            void* commandBuffer = UnsafeUtility.Malloc(
                    UnsafeUtility.SizeOf<ReadCommand>(),
                    UnsafeUtility.AlignOf<ReadCommand>(),
                    Allocator.Persistent);
            if (commandBuffer == null)
                throw new OutOfMemoryException("Unable to allocate a raw read command.");

            command = (IntPtr) commandBuffer;
        }

        public bool CanFit(long requiredCapacity, int requiredAlignment)
        {
            return Buffer != IntPtr.Zero &&
                   Capacity >= requiredCapacity &&
                   Alignment >= requiredAlignment;
        }

        public unsafe void EnsureCapacity(long requiredCapacity, int requiredAlignment)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RawZipperMemory));
            if (IsInUse)
                throw new InvalidOperationException("An active raw read buffer cannot be resized.");

            requiredAlignment = NormalizeAlignment(requiredAlignment);
            if (CanFit(requiredCapacity, requiredAlignment))
                return;

            long expandedCapacity = ExpandCapacity(Capacity, requiredCapacity);
            int expandedAlignment = Math.Max(Alignment, requiredAlignment);
            void* expandedBuffer = UnsafeUtility.Malloc(
                    expandedCapacity, expandedAlignment, Allocator.Persistent);
            if (expandedBuffer == null)
            {
                throw new OutOfMemoryException(
                        $"Unable to allocate {expandedCapacity} bytes for a raw read buffer.");
            }

            if (Buffer != IntPtr.Zero)
                UnsafeUtility.Free(Buffer.ToPointer(), Allocator.Persistent);

            Buffer = (IntPtr) expandedBuffer;
            Capacity = expandedCapacity;
            Alignment = expandedAlignment;
        }

        public unsafe ReadCommandArray CreateCommandArray(long offset, long size)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RawZipperMemory));
            if (!IsInUse || Buffer == IntPtr.Zero || command == IntPtr.Zero)
                throw new InvalidOperationException("The raw read memory has not been rented.");
            if (size <= 0 || size > Capacity)
                throw new ArgumentOutOfRangeException(nameof(size));

            var readCommand = (ReadCommand*) command.ToPointer();
            *readCommand = new ReadCommand
            {
                    Buffer = Buffer.ToPointer(),
                    Offset = offset,
                    Size = size
            };

            return new ReadCommandArray
            {
                    ReadCommands = readCommand,
                    CommandCount = 1
            };
        }

        public void MarkInUse()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RawZipperMemory));
            if (IsInUse)
                throw new InvalidOperationException("The raw read memory is already in use.");

            IsInUse = true;
        }

        public void MarkAvailable()
        {
            if (!IsInUse)
                throw new InvalidOperationException("The raw read memory has already been returned.");

            IsInUse = false;
        }

        public unsafe void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            IsInUse = false;

            if (command != IntPtr.Zero)
            {
                UnsafeUtility.Free(command.ToPointer(), Allocator.Persistent);
                command = IntPtr.Zero;
            }

            if (Buffer != IntPtr.Zero)
            {
                UnsafeUtility.Free(Buffer.ToPointer(), Allocator.Persistent);
                Buffer = IntPtr.Zero;
            }

            Capacity = 0;
            Alignment = 0;
        }

        internal static int NormalizeAlignment(int alignment)
        {
            int normalized = Math.Max(alignment, MinimumBufferAlignment);
            if ((normalized & (normalized - 1)) == 0)
                return normalized;

            int powerOfTwo = MinimumBufferAlignment;
            while (powerOfTwo < normalized)
                powerOfTwo = checked(powerOfTwo * 2);
            return powerOfTwo;
        }

        private static long ExpandCapacity(long currentCapacity, long requiredCapacity)
        {
            long result = Math.Max(currentCapacity, MinimumBufferCapacity);
            while (result < requiredCapacity)
            {
                if (result > long.MaxValue / 2)
                    return requiredCapacity;

                result *= 2;
            }

            return result;
        }
    }
}
#endif
