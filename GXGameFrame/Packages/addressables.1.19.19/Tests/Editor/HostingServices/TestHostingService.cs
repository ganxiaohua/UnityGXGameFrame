using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Tests.HostingServices
{
    class TestHostingService : AbstractTestHostingService
    {
        public TestHostingService()
        {
            HostingServiceContentRoots = new List<string>();
            ProfileVariables = new Dictionary<string, string>();
        }

        public override void StartHostingService()
        {
            IsHostingServiceRunning = true;
        }

        public override void StopHostingService()
        {
            IsHostingServiceRunning = false;
        }

        public override void OnBeforeSerialize(KeyDataStore dataStore)
        {
            dataStore.SetData(BaseHostingService.k_InstanceIdKey, InstanceId);
        }

        public override void OnAfterDeserialize(KeyDataStore dataStore)
        {
            InstanceId = dataStore.GetData(BaseHostingService.k_InstanceIdKey, -1);
        }
    }
}
