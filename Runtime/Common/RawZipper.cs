#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;

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

        private bool disposed;

        public async UniTask<bool> OpenAsync(string param, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            if (IsOpen && string.Equals(path, param, StringComparison.Ordinal))
                return true;

            InvalidateAndClose();
            int version = Versions;
            path = param;

            fileHandle = AsyncReadManager.OpenFileAsync(path);
            var openJob = fileHandle.JobHandle;

            while (!openJob.IsCompleted)
            {
                if (version != Versions)
                    return false;

                if (cancellationToken.IsCancellationRequested)
                {
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

            if (IsOpen)
                return true;

            InvalidateAndClose();
            return false;
        }

        public UniTask<T> LoadAsync<T>(long offset, CancellationToken cancellationToken = default)
            where T : unmanaged
        {
            if (cancellationToken.IsCancellationRequested)
                return UniTask.FromResult(default(T));

            long byteCount = UnsafeUtility.SizeOf<T>();
            var request = new ValueRead<T>(
                Versions, offset, byteCount, cancellationToken);
            BeginRead(request, UnsafeUtility.AlignOf<T>());
            return request.Task;
        }

        public UniTask<T[]> LoadArrayAsync<T>(long offset, int count,
            CancellationToken cancellationToken = default) where T : unmanaged
        {
            if (cancellationToken.IsCancellationRequested)
                return UniTask.FromResult<T[]>(default);
            if (count == 0)
                return UniTask.FromResult(Array.Empty<T>());

            long byteCount = (long) UnsafeUtility.SizeOf<T>() * count;
            var request = new ArrayRead<T>(
                Versions, offset, byteCount, cancellationToken, count);
            BeginRead(request, UnsafeUtility.AlignOf<T>());
            return request.Task;
        }

        public UniTask<T[]> LoadRangeAsync<T>(long offset, int byteLength,
            CancellationToken cancellationToken = default) where T : unmanaged
        {
            int elementSize = UnsafeUtility.SizeOf<T>();
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
            memoryPool.Dispose();
        }

        /// <summary>
        /// Checks queued raw reads and completes their UniTasks on the calling thread.
        /// The owner must call this once per frame.
        /// </summary>
        public void Update()
        {
            if (pendingReads.Count == 0)
                return;

            completedReads.Clear();

            for (int i = pendingReads.Count - 1; i >= 0; i--)
            {
                PendingRead request = pendingReads[i];
                request.TryCancel(Versions);
                if (!request.IsCompleted)
                    continue;

                pendingReads.RemoveAt(i);
                CompleteRead(request);
                completedReads.Add(request);
            }

            for (int i = 0; i < completedReads.Count; i++)
                completedReads[i].Publish(Versions);
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
                    CompleteRead(invalidatedReads[i]);
                }
            }

            if (fileHandle.IsValid())
                fileHandle.Close(fileHandle.JobHandle).Complete();

            fileHandle = default;
            path = null;

            if (invalidatedReads != null)
            {
                for (int i = 0; i < invalidatedReads.Length; i++)
                    invalidatedReads[i].Publish(Versions);
            }
        }

        private void BeginRead(PendingRead request, int alignment)
        {
            if (!IsOpen)
            {
                request.Publish(Versions);
                return;
            }

            RawZipperMemory memory = memoryPool.Rent(request.ExpectedBytes, alignment);
            ReadCommandArray commandArray = memory.CreateCommandArray(request.Offset, request.ExpectedBytes);
            ReadHandle readHandle = AsyncReadManager.Read(in fileHandle, commandArray);
            if (!readHandle.IsValid())
            {
                memoryPool.Return(memory);
                request.Publish(Versions);
                return;
            }

            request.Attach(readHandle, memory);
            pendingReads.Add(request);
        }

        private void CompleteRead(PendingRead request)
        {
            ReadHandle readHandle = request.Handle;
            RawZipperMemory memory = request.Memory;

            readHandle.JobHandle.Complete();
            if (!request.IsInvalid(Versions))
                request.PrepareSuccess(memory.Buffer);

            readHandle.Dispose();
            request.Detach();
            memoryPool.Return(memory);
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

        private abstract class PendingRead
        {
            public long Offset { get; }

            public long ExpectedBytes { get; }

            public ReadHandle Handle { get; private set; }

            public RawZipperMemory Memory { get; private set; }

            public bool IsCompleted => Handle.JobHandle.IsCompleted;

            private readonly int version;

            private readonly CancellationToken cancellationToken;

            private bool cancellationRequested;

            private bool published;

            private bool hasResult;

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
                if (Handle.Status == ReadStatus.InProgress)
                    Handle.Cancel();
            }

            public void PrepareSuccess(IntPtr buffer)
            {
                CaptureResult(buffer);
                hasResult = true;
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

                if (hasResult)
                    PublishResult();
                else
                    PublishDefault();
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
