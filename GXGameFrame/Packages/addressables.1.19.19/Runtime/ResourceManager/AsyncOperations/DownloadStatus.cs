using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.ResourceManagement.AsyncOperations
{
    /// <summary>
    /// Contains download information for async operations.
    /// </summary>
    public struct DownloadStatus
    {
        /// <summary>
        /// The number of bytes downloaded by the operation and all of its dependencies.
        /// </summary>
        public long TotalBytes;
        /// <summary>
        /// The total number of bytes needed to download by the operation and dependencies.
        /// </summary>
        public long DownloadedBytes;

        /// <summary>
        /// Is the operation completed.  This is used to determine if the computed Percent should be 0 or 1 when TotalBytes is 0.
        /// </summary>
        public bool IsDone;
        
        /// <summary>
        /// Returns the computed percent complete as a float value between 0 &amp; 1.  If TotalBytes == 0, 1 is returned.
        /// </summary>
        public float Percent => (TotalBytes > 0) ? ((float)DownloadedBytes / (float)TotalBytes) : (IsDone ? 1.0f : 0f);
    }
}
