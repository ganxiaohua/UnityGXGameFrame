using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableAssetsIntegrationTests
{
    internal class ManualPercentCompleteOperation : AsyncOperationBase<GameObject>
    {
        public ManualPercentCompleteOperation(float percentComplete)
        {
            m_PercentComplete = percentComplete;
            m_DownloadStatus = new DownloadStatus();
        }

        public ManualPercentCompleteOperation(DownloadStatus status)
        {
            m_DownloadStatus = status;
            m_PercentComplete = 0f;
        }

        public ManualPercentCompleteOperation(float percentComplete, DownloadStatus status)
        {
            m_DownloadStatus = status;
            m_PercentComplete = percentComplete;
        }

        public float m_PercentComplete;
        public DownloadStatus m_DownloadStatus;

        protected override void Execute()
        {
            Complete(new GameObject(), true, "");
        }

        protected override float Progress => m_PercentComplete;

        internal override DownloadStatus GetDownloadStatus(HashSet<object> visited)
        {
            return m_DownloadStatus;
        }
    }
}
