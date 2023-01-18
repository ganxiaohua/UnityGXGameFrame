using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.Analytics;

namespace UnityEditor.AddressableAssets
{
    internal static class AddressableAnalytics
    {
        private const string VendorKey = "unity.addressables";
        private const string EventName = "addressables";
        private static bool _eventRegistered = false;
        static readonly HashSet<string> builtInBuildScriptNames = new HashSet<string>{"Default Build Script", "Use Asset Database (fastest)", "Use Existing Build (requires built groups)", "Simulate Groups (advanced)"};

        [Serializable]
        private struct AnalyticsData
        {
            public string BuildScriptName;
            public int NumberOfAddressableAssets;
        };

        internal static void Report(AddressableAssetSettings currentSettings)
        {
            //The event shouldn't be able to report if this is disabled but if we know we're not going to report
            //Lets early out and not waste time gathering all the data
            if (!EditorAnalytics.enabled)
                return;

            ReportImpl(currentSettings);
        }

        private static void ReportImpl(AddressableAssetSettings currentSettings)
        {
            if (!_eventRegistered)
            {
                //If the event isn't registered, attempt to register it.  If unsuccessful, return.
                if (!RegisterEvent())
                    return;
            }

            //Gather how many addressable assets we have
            int numberOfAddressableAssets = 0;
            foreach (var group in currentSettings.groups)
            {
                if (group == null)
                    continue;
                numberOfAddressableAssets += group.entries.Count;
            }
                

            string buildScriptName = currentSettings.ActivePlayerDataBuilder.Name;
            
            AnalyticsData data = new AnalyticsData()
            {
                BuildScriptName = builtInBuildScriptNames.Contains(buildScriptName) ? buildScriptName : "Custom Build Script",
                NumberOfAddressableAssets = numberOfAddressableAssets,
            };

            //Report
            EditorAnalytics.SendEventWithLimit(EventName, data);
        }

        //Returns true if registering the event was successful
        private static bool RegisterEvent()
        {
            AnalyticsResult registerEvent = EditorAnalytics.RegisterEventWithLimit(EventName, 100, 100, VendorKey);
            if (registerEvent == AnalyticsResult.Ok)
                _eventRegistered = true;

            return _eventRegistered;
        }
    }
}
