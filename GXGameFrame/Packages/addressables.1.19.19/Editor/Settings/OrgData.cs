using System;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Settings
{
    [Serializable]
    internal class OrgData
    {
        [SerializeField]
        internal string id;
        [SerializeField]
        internal string name;
        [SerializeField]
        internal string foreign_key;
        [SerializeField]
        internal string billable_user_fk;
        [SerializeField]
        internal string org_identifier;
        [SerializeField]
        internal string orgIdentifier;

        internal static OrgData ParseOrgData(string data)
        {
            var orgData = JsonUtility.FromJson<OrgData>(data);
            if (orgData.id == null)
            {
                throw new ArgumentException("Unable to parse org data.");
            }
            return orgData;
        }
    }
}
