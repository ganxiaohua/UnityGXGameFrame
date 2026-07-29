#if !UNITY_WEBGL
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace GameFrame.Runtime
{
    public class RawZipper : IDisposable, IVersions
    {
        public string Path => path;

        public int Versions { get; private set; }

        public bool IsOpen => fileHandle.IsValid() && fileHandle.Status == FileStatus.Open;

        private string path;

        private FileHandle fileHandle;

        private JobHandle readDependency;

        public async UniTask<bool> OpenAsync(string param, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(param))
                throw new ArgumentException("Raw file path cannot be null or empty.", nameof(param));

            if (cancellationToken.IsCancellationRequested)
                return false;

            if (IsOpen && string.Equals(path, param, StringComparison.Ordinal))
                return true;

            InvalidateAndClose();
            int version = Versions;
            path = param;

            try
            {
                fileHandle = AsyncReadManager.OpenFileAsync(path);
                var openJob = fileHandle.JobHandle;

                while (!openJob.IsCompleted)
                {
                    if (version != Versions)
                        return false;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        if (version == Versions)
                            InvalidateAndClose();
                        return false;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update);
                }

                openJob.Complete();

                if (version != Versions)
                    return false;

                if (cancellationToken.IsCancellationRequested)
                {
                    InvalidateAndClose();
                    return false;
                }

                if (!IsOpen)
                {
                    string failedPath = path;
                    InvalidateAndClose();
                    Debug.LogError($"Failed to open raw file: {failedPath}");
                    return false;
                }

                return true;
            }
            catch
            {
                if (version == Versions)
                    InvalidateAndClose();
                throw;
            }
        }

        public async UniTask<T> LoadAsync<T>(long offset, CancellationToken cancellationToken = default) where T : unmanaged
        {
            long byteCount = UnsafeUtility.SizeOf<T>();
            ValidateRange(offset, byteCount);

            if (cancellationToken.IsCancellationRequested)
                return default;

            int version = Versions;
            using var operation = BeginRead(offset, byteCount, UnsafeUtility.AlignOf<T>());
            if (!operation.IsValid)
                return default;

            if (!await WaitForReadAsync(operation.Handle, version, cancellationToken))
                return default;

            if (!ValidateRead(operation.Status, operation.BytesRead, offset, byteCount))
                return default;

            return ReadValue<T>(operation.Buffer);
        }

        public async UniTask<T[]> LoadArrayAsync<T>(long offset, int count, CancellationToken cancellationToken = default) where T : unmanaged
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset cannot be negative.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count cannot be negative.");

            if (cancellationToken.IsCancellationRequested)
                return default;
            if (count == 0)
                return Array.Empty<T>();

            long byteCount = checked((long) UnsafeUtility.SizeOf<T>() * count);
            ValidateRange(offset, byteCount);

            int version = Versions;
            using var operation = BeginRead(offset, byteCount, UnsafeUtility.AlignOf<T>());
            if (!operation.IsValid)
                return default;

            if (!await WaitForReadAsync(operation.Handle, version, cancellationToken))
                return default;

            if (!ValidateRead(operation.Status, operation.BytesRead, offset, byteCount))
                return default;

            return ReadArray<T>(operation.Buffer, count, byteCount);
        }

        public UniTask<T[]> LoadRangeAsync<T>(long offset, int byteLength,
                CancellationToken cancellationToken = default) where T : unmanaged
        {
            if (byteLength < 0)
                throw new ArgumentOutOfRangeException(nameof(byteLength), byteLength, "Byte length cannot be negative.");

            int elementSize = UnsafeUtility.SizeOf<T>();
            if (byteLength % elementSize != 0)
            {
                throw new InvalidDataException(
                        $"The requested range ({byteLength} bytes) is not aligned to {typeof(T).Name} ({elementSize} bytes).");
            }

            return LoadArrayAsync<T>(offset, byteLength / elementSize, cancellationToken);
        }

        public UniTask<byte[]> LoadBytesAsync(long offset, int byteLength,
                CancellationToken cancellationToken = default)
        {
            return LoadArrayAsync<byte>(offset, byteLength, cancellationToken);
        }

        public void Dispose()
        {
            InvalidateAndClose();
        }

        private void InvalidateAndClose()
        {
            Versions++;

            if (fileHandle.IsValid())
            {
                var closeDependency = JobHandle.CombineDependencies(fileHandle.JobHandle, readDependency);
                fileHandle.Close(closeDependency).Complete();
            }

            fileHandle = default;
            readDependency = default;
            path = null;
        }

        private unsafe ReadOperation BeginRead(long offset, long byteCount, int alignment)
        {
            if (!IsOpen)
                return default;

            void* buffer = null;
            ReadCommand* command = null;
            ReadHandle readHandle = default;

            try
            {
                buffer = UnsafeUtility.Malloc(byteCount, alignment, Allocator.Persistent);
                if (buffer == null)
                    throw new OutOfMemoryException($"Failed to allocate {byteCount} bytes for raw file reading.");

                command = (ReadCommand*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<ReadCommand>(), UnsafeUtility.AlignOf<ReadCommand>(), Allocator.Persistent);
                if (command == null)
                    throw new OutOfMemoryException("Failed to allocate a raw file read command.");

                *command = new ReadCommand
                {
                        Buffer = buffer,
                        Offset = offset,
                        Size = byteCount
                };

                var commandArray = new ReadCommandArray
                {
                        ReadCommands = command,
                        CommandCount = 1
                };

                readHandle = AsyncReadManager.Read(in fileHandle, commandArray);
                if (!readHandle.IsValid())
                {
                    Debug.LogError($"Failed to queue raw file read: {path}");
                    UnsafeUtility.Free(command, Allocator.Persistent);
                    UnsafeUtility.Free(buffer, Allocator.Persistent);
                    return default;
                }

                readDependency = JobHandle.CombineDependencies(readDependency, readHandle.JobHandle);
                return new ReadOperation(readHandle, (IntPtr) buffer, (IntPtr) command);
            }
            catch
            {
                if (readHandle.IsValid())
                {
                    readHandle.JobHandle.Complete();
                    readHandle.Dispose();
                }

                if (command != null)
                    UnsafeUtility.Free(command, Allocator.Persistent);
                if (buffer != null)
                    UnsafeUtility.Free(buffer, Allocator.Persistent);
                throw;
            }
        }

        private async UniTask<bool> WaitForReadAsync(ReadHandle readHandle, int version, CancellationToken cancellationToken)
        {
            while (!readHandle.JobHandle.IsCompleted)
            {
                if (version != Versions || cancellationToken.IsCancellationRequested)
                {
                    if (readHandle.IsValid() && readHandle.Status == ReadStatus.InProgress)
                        readHandle.Cancel();
                    if (readHandle.IsValid())
                        readHandle.JobHandle.Complete();
                    return false;
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (readHandle.IsValid())
                readHandle.JobHandle.Complete();
            return version == Versions && !cancellationToken.IsCancellationRequested;
        }

        private bool ValidateRead(ReadStatus status, long bytesRead, long offset, long expectedBytes)
        {
            if (status == ReadStatus.Complete && bytesRead == expectedBytes)
                return true;

            Debug.LogError(
                    $"Raw file range could not be read completely. Path:{path}, Offset:{offset}, " +
                    $"Expected:{expectedBytes}, Read:{bytesRead}, Status:{status}");
            return false;
        }

        private static void ValidateRange(long offset, long byteCount)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset cannot be negative.");
            if (byteCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount, "Byte count must be positive.");

            _ = checked(offset + byteCount);
        }

        private static unsafe T ReadValue<T>(IntPtr buffer) where T : unmanaged
        {
            return *(T*) buffer.ToPointer();
        }

        private static unsafe T[] ReadArray<T>(IntPtr buffer, int count, long byteCount) where T : unmanaged
        {
            var result = new T[count];
            fixed (T* destination = result)
                UnsafeUtility.MemCpy(destination, buffer.ToPointer(), byteCount);
            return result;
        }
        
        private struct ReadOperation : IDisposable
        {
            public bool IsValid => readHandle.IsValid();

            public ReadStatus Status => readHandle.Status;

            public long BytesRead => readHandle.GetBytesRead();

            public IntPtr Buffer => buffer;

            public ReadHandle Handle => readHandle;

            private ReadHandle readHandle;

            private IntPtr buffer;

            private IntPtr command;

            private bool disposed;

            public ReadOperation(ReadHandle readHandle, IntPtr buffer, IntPtr command)
            {
                this.readHandle = readHandle;
                this.buffer = buffer;
                this.command = command;
                disposed = false;
            }

            public unsafe void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                if (readHandle.IsValid())
                {
                    readHandle.JobHandle.Complete();
                    readHandle.Dispose();
                    readHandle = default;
                }

                if (command != IntPtr.Zero)
                {
                    UnsafeUtility.Free(command.ToPointer(), Allocator.Persistent);
                    command = IntPtr.Zero;
                }

                if (buffer != IntPtr.Zero)
                {
                    UnsafeUtility.Free(buffer.ToPointer(), Allocator.Persistent);
                    buffer = IntPtr.Zero;
                }
            }
        }
    }
}
#endif
