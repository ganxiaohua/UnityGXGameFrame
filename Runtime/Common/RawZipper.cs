#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
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

        private readonly RawZipperMemoryPool memoryPool = new();

        private readonly List<PendingRead> pendingReads = new();

        private readonly List<PendingRead> completedReads = new();

        private bool isUpdating;

        private bool disposed;

        public async UniTask<bool> OpenAsync(string param, CancellationToken cancellationToken = default)
        {
            if (disposed)
            {
                Debug.LogError($"{nameof(RawZipper)} has already been disposed.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(param))
            {
                Debug.LogError("Raw file path cannot be null or empty.");
                return false;
            }

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
            catch (Exception exception)
            {
                string failedPath = path;
                if (version == Versions)
                    InvalidateAndClose();

                Debug.LogError($"Failed to open raw file: {failedPath}\n{exception}");
                return false;
            }
        }

        public UniTask<T> LoadAsync<T>(long offset, CancellationToken cancellationToken = default)
            where T : unmanaged
        {
            long byteCount = UnsafeUtility.SizeOf<T>();
            if (!ValidateRange(offset, byteCount))
                return UniTask.FromResult(default(T));

            if (disposed)
            {
                Debug.LogError($"{nameof(RawZipper)} has already been disposed.");
                return UniTask.FromResult(default(T));
            }

            if (cancellationToken.IsCancellationRequested)
                return UniTask.FromResult(default(T));

            var request = new ValueRead<T>(
                Versions, offset, byteCount, cancellationToken);
            BeginRead(request, UnsafeUtility.AlignOf<T>());
            return request.Task;
        }

        public UniTask<T[]> LoadArrayAsync<T>(long offset, int count,
            CancellationToken cancellationToken = default) where T : unmanaged
        {
            if (offset < 0)
            {
                Debug.LogError($"Raw file offset cannot be negative: {offset}.");
                return UniTask.FromResult<T[]>(default);
            }

            if (count < 0)
            {
                Debug.LogError($"Raw file element count cannot be negative: {count}.");
                return UniTask.FromResult<T[]>(default);
            }

            if (disposed)
            {
                Debug.LogError($"{nameof(RawZipper)} has already been disposed.");
                return UniTask.FromResult<T[]>(default);
            }

            if (cancellationToken.IsCancellationRequested)
                return UniTask.FromResult<T[]>(default);
            if (count == 0)
                return UniTask.FromResult(Array.Empty<T>());

            long byteCount = (long) UnsafeUtility.SizeOf<T>() * count;
            if (!ValidateRange(offset, byteCount))
                return UniTask.FromResult<T[]>(default);

            var request = new ArrayRead<T>(
                Versions, offset, byteCount, cancellationToken, count);
            BeginRead(request, UnsafeUtility.AlignOf<T>());
            return request.Task;
        }

        public UniTask<T[]> LoadRangeAsync<T>(long offset, int byteLength,
            CancellationToken cancellationToken = default) where T : unmanaged
        {
            if (byteLength < 0)
            {
                Debug.LogError($"Raw file byte length cannot be negative: {byteLength}.");
                return UniTask.FromResult<T[]>(default);
            }

            int elementSize = UnsafeUtility.SizeOf<T>();
            if (byteLength % elementSize != 0)
            {
                Debug.LogError(
                    $"The requested raw file range ({byteLength} bytes) is not aligned to " +
                    $"{typeof(T).Name} ({elementSize} bytes).");
                return UniTask.FromResult<T[]>(default);
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
            if (disposed)
                return;

            disposed = true;
            InvalidateAndClose();

            try
            {
                memoryPool.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to dispose raw file memory pool.\n{exception}");
            }
        }

        /// <summary>
        /// Checks queued raw reads and completes their UniTasks on the calling thread.
        /// The owner must call this once per frame.
        /// </summary>
        public void Update()
        {
            if (disposed || isUpdating || pendingReads.Count == 0)
                return;

            isUpdating = true;
            completedReads.Clear();

            for (int i = pendingReads.Count - 1; i >= 0; i--)
            {
                PendingRead request = pendingReads[i];
                request.TryCancel(Versions);
                if (!request.IsCompleted)
                    continue;

                pendingReads.RemoveAt(i);
                CompleteRead(request, false);
                completedReads.Add(request);
            }

            for (int i = 0; i < completedReads.Count; i++)
                Publish(completedReads[i]);

            isUpdating = false;
        }

        private void InvalidateAndClose()
        {
            Versions++;

            PendingRead[] invalidatedReads = null;
            if (pendingReads.Count > 0)
            {
                invalidatedReads = pendingReads.ToArray();
                pendingReads.Clear();

                for (int i = 0; i < invalidatedReads.Length; i++)
                {
                    invalidatedReads[i].TryCancel(Versions);
                    CompleteRead(invalidatedReads[i], true);
                }
            }

            string closingPath = path;
            try
            {
                if (fileHandle.IsValid())
                    fileHandle.Close(fileHandle.JobHandle).Complete();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to close raw file: {closingPath}\n{exception}");
            }
            finally
            {
                fileHandle = default;
                path = null;
            }

            if (invalidatedReads != null)
            {
                for (int i = 0; i < invalidatedReads.Length; i++)
                    Publish(invalidatedReads[i]);
            }
        }

        private void BeginRead(PendingRead request, int alignment)
        {
            if (!IsOpen)
            {
                request.PrepareDefault();
                Publish(request);
                return;
            }

            RawZipperMemory memory = null;
            ReadHandle readHandle = default;
            try
            {
                memory = memoryPool.Rent(request.ExpectedBytes, alignment);
                ReadCommandArray commandArray =
                    memory.CreateCommandArray(request.Offset, request.ExpectedBytes);
                readHandle = AsyncReadManager.Read(in fileHandle, commandArray);
                if (!readHandle.IsValid())
                {
                    Debug.LogError($"Failed to queue raw file read: {path}");
                    ReturnMemory(memory);
                    request.PrepareDefault();
                    Publish(request);
                    return;
                }

                request.Attach(readHandle, memory);
                pendingReads.Add(request);
            }
            catch (Exception exception)
            {
                if (readHandle.IsValid())
                {
                    try
                    {
                        if (readHandle.Status == ReadStatus.InProgress)
                            readHandle.Cancel();
                        readHandle.JobHandle.Complete();
                        readHandle.Dispose();
                    }
                    catch (Exception cleanupException)
                    {
                        Debug.LogError(
                            $"Failed to clean up an unqueued raw file read.\n{cleanupException}");
                    }
                }

                ReturnMemory(memory);

                request.Detach();
                request.PrepareDefault();
                Debug.LogError(
                    $"Failed to queue raw file read. Path:{path}, Offset:{request.Offset}, " +
                    $"Bytes:{request.ExpectedBytes}\n{exception}");
                Publish(request);
            }
        }

        private void CompleteRead(PendingRead request, bool forceDefault)
        {
            ReadHandle readHandle = request.Handle;
            RawZipperMemory memory = request.Memory;

            try
            {
                if (readHandle.IsValid())
                    readHandle.JobHandle.Complete();

                if (forceDefault || request.IsInvalid(Versions))
                {
                    request.PrepareDefault();
                }
                else if (!ValidateRead(
                             readHandle.Status,
                             readHandle.GetBytesRead(),
                             request.Offset,
                             request.ExpectedBytes))
                {
                    request.PrepareDefault();
                }
                else
                {
                    request.PrepareSuccess(memory.Buffer);
                }
            }
            catch (Exception exception)
            {
                request.PrepareDefault();
                Debug.LogError(
                    $"Failed to complete raw file read. Path:{path}, Offset:{request.Offset}, " +
                    $"Bytes:{request.ExpectedBytes}\n{exception}");
            }
            finally
            {
                if (readHandle.IsValid())
                {
                    try
                    {
                        readHandle.Dispose();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(
                            $"Failed to dispose raw file read handle. Path:{path}, " +
                            $"Offset:{request.Offset}\n{exception}");
                    }
                }

                request.Detach();
                ReturnMemory(memory);
            }
        }

        private void ReturnMemory(RawZipperMemory memory)
        {
            if (memory == null || !memory.IsInUse)
                return;

            try
            {
                memoryPool.Return(memory);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to return raw file read memory.\n{exception}");
            }
        }

        private void Publish(PendingRead request)
        {
            try
            {
                request.Publish(Versions);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to publish raw file read result.\n{exception}");
            }
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

        private static bool ValidateRange(long offset, long byteCount)
        {
            if (offset < 0)
            {
                Debug.LogError($"Raw file offset cannot be negative: {offset}.");
                return false;
            }

            if (byteCount <= 0)
            {
                Debug.LogError($"Raw file byte count must be positive: {byteCount}.");
                return false;
            }

            if (offset > long.MaxValue - byteCount)
            {
                Debug.LogError(
                    $"Raw file range exceeds the supported offset. Offset:{offset}, Bytes:{byteCount}.");
                return false;
            }

            return true;
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

        private enum ReadCompletionState
        {
            None,
            Success,
            Default
        }

        private abstract class PendingRead
        {
            public long Offset { get; }

            public long ExpectedBytes { get; }

            public ReadHandle Handle { get; private set; }

            public RawZipperMemory Memory { get; private set; }

            public bool IsCompleted
            {
                get
                {
                    try
                    {
                        return !Handle.IsValid() || Handle.JobHandle.IsCompleted;
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(
                            $"Failed to check raw file read state at offset {Offset}.\n{exception}");
                        return true;
                    }
                }
            }

            private readonly int version;

            private readonly CancellationToken cancellationToken;

            private ReadCompletionState completionState;

            private bool cancellationRequested;

            private bool published;

            protected PendingRead(int version, long offset, long expectedBytes,
                CancellationToken cancellationToken)
            {
                this.version = version;
                this.cancellationToken = cancellationToken;
                Offset = offset;
                ExpectedBytes = expectedBytes;
            }

            public void Attach(ReadHandle handle, RawZipperMemory memory)
            {
                Handle = handle;
                Memory = memory;
            }

            public void Detach()
            {
                Handle = default;
                Memory = null;
            }

            public bool IsInvalid(int currentVersion)
            {
                return version != currentVersion || cancellationToken.IsCancellationRequested;
            }

            public void TryCancel(int currentVersion)
            {
                if (cancellationRequested || !IsInvalid(currentVersion))
                    return;

                cancellationRequested = true;
                try
                {
                    if (Handle.IsValid() && Handle.Status == ReadStatus.InProgress)
                        Handle.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Failed to cancel raw file read at offset {Offset}.\n{exception}");
                }
            }

            public void PrepareSuccess(IntPtr buffer)
            {
                CaptureResult(buffer);
                completionState = ReadCompletionState.Success;
            }

            public void PrepareDefault()
            {
                completionState = ReadCompletionState.Default;
            }

            public void Publish(int currentVersion)
            {
                if (published)
                    return;

                published = true;
                if (IsInvalid(currentVersion))
                {
                    PublishDefault();
                    return;
                }

                switch (completionState)
                {
                    case ReadCompletionState.Success:
                        PublishResult();
                        break;
                    default:
                        PublishDefault();
                        break;
                }
            }

            protected abstract void CaptureResult(IntPtr buffer);

            protected abstract void PublishResult();

            protected abstract void PublishDefault();
        }

        private sealed class ValueRead<T> : PendingRead where T : unmanaged
        {
            public UniTask<T> Task => completionSource.Task;

            private readonly UniTaskCompletionSource<T> completionSource = new();

            private T result;

            public ValueRead(int version, long offset, long expectedBytes,
                CancellationToken cancellationToken)
                : base(version, offset, expectedBytes, cancellationToken)
            {
            }

            protected override void CaptureResult(IntPtr buffer)
            {
                result = ReadValue<T>(buffer);
            }

            protected override void PublishResult()
            {
                completionSource.TrySetResult(result);
            }

            protected override void PublishDefault()
            {
                completionSource.TrySetResult(default);
            }
        }

        private sealed class ArrayRead<T> : PendingRead where T : unmanaged
        {
            public UniTask<T[]> Task => completionSource.Task;

            private readonly UniTaskCompletionSource<T[]> completionSource = new();

            private readonly int count;

            private T[] result;

            public ArrayRead(int version, long offset, long expectedBytes,
                CancellationToken cancellationToken, int count)
                : base(version, offset, expectedBytes, cancellationToken)
            {
                this.count = count;
            }

            protected override void CaptureResult(IntPtr buffer)
            {
                result = ReadArray<T>(buffer, count, ExpectedBytes);
            }

            protected override void PublishResult()
            {
                completionSource.TrySetResult(result);
            }

            protected override void PublishDefault()
            {
                completionSource.TrySetResult(default);
            }
        }
    }
}
#endif
