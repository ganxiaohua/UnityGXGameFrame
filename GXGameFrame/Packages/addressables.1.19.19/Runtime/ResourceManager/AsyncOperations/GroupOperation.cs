using System;
using System.Collections.Generic;
using System.Security;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.AsyncOperations
{
    class GroupOperation : AsyncOperationBase<IList<AsyncOperationHandle>>, ICachable
    {
        [Flags]
        public enum GroupOperationSettings
        {
            None = 0,
            ReleaseDependenciesOnFailure = 1,
            AllowFailedDependencies = 2
        }

        Action<AsyncOperationHandle> m_InternalOnComplete;
        int m_LoadedCount;
        GroupOperationSettings m_Settings;
        string debugName = null;
        private const int k_MaxDisplayedLocationLength = 45;

        public GroupOperation()
        {
            m_InternalOnComplete = OnOperationCompleted;
            Result = new List<AsyncOperationHandle>();
        }

        ///<inheritdoc />
        protected  override bool InvokeWaitForCompletion()
        {
            //If Result is null then we've auto released and need to return
            if (IsDone || Result == null)
                return true;

            foreach (var r in Result)
            {
                r.WaitForCompletion();
                if (Result == null)
                    return true;
            }

            m_RM?.Update(Time.unscaledDeltaTime);
            if (!IsDone && Result != null)
                Execute();
            m_RM?.Update(Time.unscaledDeltaTime);
            return IsDone;
        }

        IOperationCacheKey ICachable.Key { get; set; }

        internal IList<AsyncOperationHandle> GetDependentOps()
        {
            return Result;
        }

        /// <inheritdoc />
        public override void GetDependencies(List<AsyncOperationHandle> deps)
        {
            deps.AddRange(Result);
        }

        internal override void ReleaseDependencies()
        {
            for (int i = 0; i < Result.Count; i++)
                if (Result[i].IsValid())
                    Result[i].Release();
            Result.Clear();
        }

        internal override DownloadStatus GetDownloadStatus(HashSet<object> visited)
        {
            var status = new DownloadStatus() { IsDone = IsDone };
            for (int i = 0; i < Result.Count; i++)
            {
                if (Result[i].IsValid())
                {
                    var depStatus = Result[i].InternalGetDownloadStatus(visited);
                    status.DownloadedBytes += depStatus.DownloadedBytes;
                    status.TotalBytes += depStatus.TotalBytes;
                }
            }
            return status;
        }

        HashSet<string> m_CachedDependencyLocations = new HashSet<string>();

        private bool DependenciesAreUnchanged(List<AsyncOperationHandle> deps)
        {
            if (m_CachedDependencyLocations.Count != deps.Count) return false;
            foreach (var d in deps)
                if (!m_CachedDependencyLocations.Contains(d.LocationName))
                    return false;
            return true;
        }

        protected override string DebugName
        {
            get
            {
                List<AsyncOperationHandle> deps = new List<AsyncOperationHandle>();
                GetDependencies(deps);

                if (deps.Count == 0)
                    return "Dependencies";

                //Only recalculate DebugName if a name hasn't been generated for currently held dependencies
                if (debugName != null && DependenciesAreUnchanged(deps))
                    return debugName;

                m_CachedDependencyLocations.Clear();

                string toBeDisplayed = "Dependencies [";
                for (var i = 0; i < deps.Count; i++)
                {
                    var d = deps[i];

                    var locationString = d.LocationName;
                    m_CachedDependencyLocations.Add(locationString);

                    if (locationString == null)
                        continue;

                    //Prevent location display from being excessively long
                    if (locationString.Length > k_MaxDisplayedLocationLength)
                    {
                        locationString = AsyncOperationBase<object>.ShortenPath(locationString, true);
                        locationString = locationString.Substring(0, Math.Min(k_MaxDisplayedLocationLength, locationString.Length)) + "...";
                    }

                    if (i == deps.Count - 1)
                        toBeDisplayed += locationString;
                    else
                        toBeDisplayed += locationString + ", ";
                }

                toBeDisplayed += "]";

                debugName = toBeDisplayed;

                return debugName;
            }
        }

        protected override void Execute()
        {
            m_LoadedCount = 0;
            for (int i = 0; i < Result.Count; i++)
            {
                if (Result[i].IsDone)
                    m_LoadedCount++;
                else
                    Result[i].Completed += m_InternalOnComplete;
            }
            CompleteIfDependenciesComplete();
        }

        private void CompleteIfDependenciesComplete()
        {
            if (m_LoadedCount == Result.Count)
            {
                bool success = true;
                OperationException ex = null;
                if (!m_Settings.HasFlag(GroupOperationSettings.AllowFailedDependencies))
                {
                    for (int i = 0; i < Result.Count; i++)
                    {
                        if (Result[i].Status != AsyncOperationStatus.Succeeded)
                        {
                            success = false;
                            ex = new OperationException("GroupOperation failed because one of its dependencies failed", Result[i].OperationException);
                            break;
                        }
                    }
                }
                Complete(Result, success, ex, m_Settings.HasFlag(GroupOperationSettings.ReleaseDependenciesOnFailure));
            }
        }

        protected override void Destroy()
        {
            ReleaseDependencies();
        }

        protected override float Progress
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < Result.Count; i++)
                {
                    var handle = Result[i];
                    if (!handle.IsDone)
                        total += handle.PercentComplete;
                    else
                        total++;
                }

                return total / Result.Count;
            }
        }


        public void Init(List<AsyncOperationHandle> operations, bool releaseDependenciesOnFailure = true, bool allowFailedDependencies = false)
        {
            Result = new List<AsyncOperationHandle>(operations);
            m_Settings = releaseDependenciesOnFailure ? GroupOperationSettings.ReleaseDependenciesOnFailure : GroupOperationSettings.None;
            if (allowFailedDependencies)
                m_Settings |= GroupOperationSettings.AllowFailedDependencies;
        }

        public void Init(List<AsyncOperationHandle> operations, GroupOperationSettings settings)
        {
            Result = new List<AsyncOperationHandle>(operations);
            m_Settings = settings;
        }

        void OnOperationCompleted(AsyncOperationHandle op)
        {
            m_LoadedCount++;
            CompleteIfDependenciesComplete();
        }
    }
}
