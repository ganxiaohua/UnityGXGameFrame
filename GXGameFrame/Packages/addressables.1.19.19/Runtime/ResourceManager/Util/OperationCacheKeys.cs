using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.ResourceManagement.Util
{
    internal interface IOperationCacheKey : IEquatable<IOperationCacheKey> {}

    internal sealed class LocationCacheKey : IOperationCacheKey
    {
        readonly IResourceLocation m_Location;
        readonly Type m_DesiredType;

        public LocationCacheKey(IResourceLocation location, Type desiredType)
        {
            if (location == null)
                throw new NullReferenceException($"Resource location cannot be null.");
            if (desiredType == null)
                throw new NullReferenceException($"Desired type cannot be null.");

            m_Location = location;
            m_DesiredType = desiredType;
        }

        public override int GetHashCode()
        {
            return m_Location.Hash(m_DesiredType);
        }

        public override bool Equals(object obj) => Equals(obj as LocationCacheKey);
        public bool Equals(IOperationCacheKey other) => Equals(other as LocationCacheKey);
        bool Equals(LocationCacheKey other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;
            return LocationUtils.LocationEquals(m_Location, other.m_Location) && Equals(m_DesiredType, other.m_DesiredType);
        }
    }

    internal sealed class DependenciesCacheKey : IOperationCacheKey
    {
        readonly IList<IResourceLocation> m_Dependencies;
        readonly int m_DependenciesHash;
        public DependenciesCacheKey(IList<IResourceLocation> dependencies, int dependenciesHash)
        {
            m_Dependencies = dependencies;
            m_DependenciesHash = dependenciesHash;
        }

        public override int GetHashCode()
        {
            return m_DependenciesHash;
        }

        public override bool Equals(object obj) => Equals(obj as DependenciesCacheKey);
        public bool Equals(IOperationCacheKey other) => Equals(other as DependenciesCacheKey);
        bool Equals(DependenciesCacheKey other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;
            return LocationUtils.DependenciesEqual(m_Dependencies, other.m_Dependencies);
        }
    }

    internal sealed class AsyncOpHandlesCacheKey : IOperationCacheKey
    {
        readonly HashSet<AsyncOperationHandle> m_Handles;
        public AsyncOpHandlesCacheKey(IList<AsyncOperationHandle> handles)
        {
            m_Handles = new HashSet<AsyncOperationHandle>(handles);
        }

        public override int GetHashCode()
        {
            return m_Handles.GetHashCode();
        }

        public override bool Equals(object obj) => Equals(obj as AsyncOpHandlesCacheKey);
        public bool Equals(IOperationCacheKey other) => Equals(other as AsyncOpHandlesCacheKey);
        bool Equals(AsyncOpHandlesCacheKey other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;
            return m_Handles.SetEquals(other.m_Handles);
        }
    }

    internal static class LocationUtils
    {
        // TODO : Added equality methods here to prevent a minor version bump since we intend to stay on v1.18.x for a while.
        // Ideally this should have been the Equals() implementation of IResourceLocation
        public static bool LocationEquals(IResourceLocation loc1, IResourceLocation loc2)
        {
            if (ReferenceEquals(loc1, loc2)) return true;
            if (ReferenceEquals(loc1, null)) return false;
            if (ReferenceEquals(loc2, null)) return false;

            return (loc1.InternalId.Equals(loc2.InternalId)
                && loc1.ProviderId.Equals(loc2.ProviderId)
                && loc1.ResourceType.Equals(loc2.ResourceType));
        }

        public static bool DependenciesEqual(IList<IResourceLocation> deps1, IList<IResourceLocation> deps2)
        {
            if (ReferenceEquals(deps1, deps2)) return true;
            if (ReferenceEquals(deps1, null)) return false;
            if (ReferenceEquals(deps2, null)) return false;
            if (deps1.Count != deps2.Count)
                return false;

            for (int i = 0; i < deps1.Count; i++)
            {
                if (!LocationEquals(deps1[i], deps2[i]))
                    return false;
            }

            return true;
        }
    }
}
