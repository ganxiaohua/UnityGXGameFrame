using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Diagnostics;

namespace UnityEngine.AddressableAssets.Utility
{
    internal class DiagnosticInfo
    {
        public string DisplayName;
        public int ObjectId;
        public int[] Dependencies;
        public DiagnosticEvent CreateEvent(string category, ResourceManager.DiagnosticEventType eventType, int frame, int val)
        {
            return new DiagnosticEvent(category, DisplayName, ObjectId, (int)eventType, frame, val, Dependencies);
        }
    }

    internal class ResourceManagerDiagnostics : IDisposable
    {
        ResourceManager m_ResourceManager;
        private const int k_NumberOfCompletedOpResultEntriesToShow = 4;
        private const int k_MaximumCompletedOpResultEntryLength = 30;

        /// <summary>
        /// This class is responsible for passing events from the resource manager to the event collector,
        /// </summary>
        /// <param name="resourceManager"></param>
        public ResourceManagerDiagnostics(ResourceManager resourceManager)
        {
            resourceManager.RegisterDiagnosticCallback(OnResourceManagerDiagnosticEvent);
            m_ResourceManager = resourceManager;
        }

        Dictionary<int, DiagnosticInfo> m_cachedDiagnosticInfo = new Dictionary<int, DiagnosticInfo>();

        internal int SumDependencyNameHashCodes(AsyncOperationHandle handle)
        {
            List<AsyncOperationHandle> deps = new List<AsyncOperationHandle>();
            handle.GetDependencies(deps);
            
            int sumOfDependencyHashes = 0;
            foreach (var d in deps)
                unchecked
                {
                    sumOfDependencyHashes += d.DebugName.GetHashCode() + SumDependencyNameHashCodes(d);
                }
            return sumOfDependencyHashes;
        }

        internal int CalculateHashCode(AsyncOperationHandle handle)
        {
            if (handle.DebugName.Contains("CompletedOperation"))
                return CalculateCompletedOperationHashcode(handle);

            int sumOfDependencyHashes = SumDependencyNameHashCodes(handle);
            bool nameChangesWithState = handle.DebugName.Contains("result=") && handle.DebugName.Contains("status=");
            
            // We default to the regular hash code in the case of operations with names that change with their state
            // since their names aren't a reliable way to reference them
            
            if (nameChangesWithState)
                return handle.GetHashCode();
            //its okay if this overflows
            unchecked
            {
                return handle.DebugName.GetHashCode() + sumOfDependencyHashes;
            }
        }

        internal int CalculateCompletedOperationHashcode(AsyncOperationHandle handle)
        {
            unchecked
            {
                if (handle.Result == null)
                    return handle.GetHashCode();
                return handle.Result.GetHashCode() + handle.Result.GetType().GetHashCode();
            }
        }

        internal string GenerateCompletedOperationDisplayName(AsyncOperationHandle handle)
        {
            if (handle.Result == null)
                return handle.DebugName;
            if (handle.Result.GetType().IsGenericType && handle.Result is IList resultList)
            {
                string completedOpResultString = handle.DebugName;
                if (resultList.Count > 0)
                {
                    StringBuilder completedOpResultStringBuilder = new StringBuilder("[");
                    for (int i = 0; i < resultList.Count && i < k_NumberOfCompletedOpResultEntriesToShow; i++)
                    {
                        var entry = resultList[i];
                        if (k_MaximumCompletedOpResultEntryLength <= entry.ToString().Length)
                        {
                            completedOpResultStringBuilder.Append(entry.ToString().Substring(0, k_MaximumCompletedOpResultEntryLength));
                            completedOpResultStringBuilder.Append("..., ");
                        }
                        else
                        {
                            completedOpResultStringBuilder.Append(entry.ToString().Substring(0, entry.ToString().Length));
                            completedOpResultStringBuilder.Append(", ");
                        }
                    }
                    completedOpResultStringBuilder.Remove(completedOpResultStringBuilder.Length - 2, 2);
                    completedOpResultStringBuilder.Append("]");
                    completedOpResultString = completedOpResultStringBuilder.ToString(); 
                }

                return handle.DebugName + " Result type: List, result: " + completedOpResultString;
            }
            return handle.DebugName + " Result type: " + handle.Result.GetType();
        }

        void OnResourceManagerDiagnosticEvent(ResourceManager.DiagnosticEventContext eventContext)
        {
            var hashCode = CalculateHashCode(eventContext.OperationHandle);
            DiagnosticInfo diagInfo = null;
            
            if (eventContext.Type == ResourceManager.DiagnosticEventType.AsyncOperationDestroy)
            {
                if (m_cachedDiagnosticInfo.TryGetValue(hashCode, out diagInfo))
                    m_cachedDiagnosticInfo.Remove(hashCode);
            }
            else
            {
                if (!m_cachedDiagnosticInfo.TryGetValue(hashCode, out diagInfo))
                {
                    List<AsyncOperationHandle> deps = new List<AsyncOperationHandle>();
                    eventContext.OperationHandle.GetDependencies(deps);
                    var depIds = new int[deps.Count];
                    
                    for (int i = 0; i < depIds.Length; i++)
                        depIds[i] = CalculateHashCode(deps[i]);

                    if (eventContext.OperationHandle.DebugName.Contains("CompletedOperation"))
                    {
                        string displayName = GenerateCompletedOperationDisplayName(eventContext.OperationHandle);
                        m_cachedDiagnosticInfo.Add(hashCode, diagInfo = new DiagnosticInfo() { ObjectId = hashCode, DisplayName = displayName, Dependencies = depIds });
                    }
                    else
                        m_cachedDiagnosticInfo.Add(hashCode, diagInfo = new DiagnosticInfo() { ObjectId = hashCode, DisplayName = eventContext.OperationHandle.DebugName, Dependencies = depIds });
                }
            }

            if (diagInfo != null)
                DiagnosticEventCollectorSingleton.Instance.PostEvent(diagInfo.CreateEvent("ResourceManager", eventContext.Type, Time.frameCount, eventContext.EventValue));
        }

        public void Dispose()
        {
            m_ResourceManager?.UnregisterDiagnosticCallback(OnResourceManagerDiagnosticEvent);
            if (DiagnosticEventCollectorSingleton.Exists)
                DiagnosticEventCollectorSingleton.DestroySingleton();
        }
    }
}