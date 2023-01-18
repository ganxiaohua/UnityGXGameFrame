using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnityEditor.AddressableAssets.Settings
{
    /// <summary>
    /// Used to store path pairs and act as an abstraction between path pairs and profile variables. 
    /// </summary>
    [Serializable]
    public class ProfileGroupType
    {
        /// <summary>
        /// Used to store path values, identified by postfix
        /// </summary>
        ///
        [Serializable]
        internal class GroupTypeVariable
        {
            /// <summary>
            /// Constructor for variables
            /// </summary>
            /// <param name="suffix"></param>
            /// <param name="value"></param>
            internal GroupTypeVariable(string suffix, string value)
            {
                m_Suffix = suffix;
                m_Value = value;
            }

            /// <summary>
            /// Postfix of a GroupTypeVariable
            /// </summary>
            ///
            [SerializeField]
            internal string m_Suffix;
            internal string Suffix
            {
                get { return m_Suffix; }
                set { m_Suffix = value; }
            }

            /// <summary>
            /// Specified Value
            /// </summary>
            [SerializeField]
            internal string m_Value;
            internal string Value
            {
                get { return m_Value; }
                set { m_Value = value; }
            }
        }

        internal const char k_PrefixSeparator = '.';


        [SerializeField]
        string m_GroupTypePrefix;

        /// <summary>
        /// Common prefix used in determining a path pair
        /// </summary>
        internal string GroupTypePrefix
        {
            get { return m_GroupTypePrefix; }
            set { m_GroupTypePrefix = value; }
        }

        /// <summary>
        /// Group of variables that share a common prefix
        /// </summary>
        ///
        [SerializeField]
        internal List<GroupTypeVariable> m_Variables;
        internal List<GroupTypeVariable> Variables
        {
            get { return m_Variables; }
        }

        /// <summary>
        /// ctors for profile group type
        /// </summary>
        internal ProfileGroupType() { }

        internal ProfileGroupType(string prefix)
        {
            m_GroupTypePrefix = prefix;
            m_Variables = new List<GroupTypeVariable>();
        }

        /// <summary>
        /// Returns the full variable name
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>the full name of a variable</returns>
        internal string GetName(GroupTypeVariable variable)
        {
            return m_GroupTypePrefix + k_PrefixSeparator + variable.Suffix;
        }

        /// <summary>
        /// Adds a variable in the group
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>True if the variable was added, false if the variable already exists</returns>
        internal bool AddVariable(GroupTypeVariable variable)
        {
            GroupTypeVariable exists = m_Variables.Where(ps => ps.Suffix == variable.Suffix).FirstOrDefault();
            if (exists != null)
            {
                Addressables.LogErrorFormat("{0} already exists.", GetName(variable));
                return false;
            }
            m_Variables.Add(variable);
            return true;
        }

        
        // Adds a variable to the group, or updates the value if already exists
        internal void AddOrUpdateVariable(GroupTypeVariable variable)
        {
            foreach (GroupTypeVariable typeVariable in m_Variables)
            {
                if (typeVariable.Suffix == variable.Suffix)
                {
                    typeVariable.Value = variable.Value;
                    return;
                }
            }

            m_Variables.Add(variable);
        }

        /// <summary>
        /// Removes a variable from the group
        /// </summary>
        /// <param name="variable"></param>
        internal void RemoveVariable(GroupTypeVariable variable)
        {
            GroupTypeVariable exists = m_Variables.Where(ps => ps.Suffix == variable.Suffix).FirstOrDefault();
            if (exists == null)
            {
                Addressables.LogErrorFormat("{0} does not exist.", GetName(variable));
                return;
            }
            else
            {
                m_Variables.Remove(variable);
            }
        }

        /// <summary>
        /// Gets a variable by its suffix name
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns>the variable if exists, null otherwise</returns>
        internal GroupTypeVariable GetVariableBySuffix(string suffix)
        {
            return m_Variables.Where(var => var.m_Suffix == suffix).FirstOrDefault();
        }

        /// <summary>
        /// Returns true if a group type has a certain variable
        /// </summary>
        /// <param name="groupTypeVariable">group type variable</param>
        /// <returns>true if the group type contains the variable, false otherwise</returns>
        internal bool ContainsVariable(GroupTypeVariable groupTypeVariable)
        {
            return m_Variables.Any(var => var.Suffix == groupTypeVariable.Suffix && var.Value == groupTypeVariable.Value);
        }

        //UI magic to group the path pairs from profile variables
        internal static List<ProfileGroupType> CreateGroupTypes(AddressableAssetProfileSettings.BuildProfile buildProfile)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            Dictionary<string, ProfileGroupType> groups = new Dictionary<string, ProfileGroupType>();
            foreach (var profileEntry in settings.profileSettings.profileEntryNames)
            {
                string[] parts = profileEntry.ProfileName.Split(k_PrefixSeparator);
                if (parts.Length > 1)
                {
                    string prefix = String.Join(k_PrefixSeparator.ToString(), parts, 0, parts.Length - 1);
                    string suffix = parts[parts.Length - 1];
                    string profileEntryValue = buildProfile.GetValueById(profileEntry.Id);
                    ProfileGroupType group;
                    groups.TryGetValue(prefix, out group);
                    if (group == null)
                    {
                        group = new ProfileGroupType(prefix);
                    }
                    GroupTypeVariable variable = new GroupTypeVariable(suffix, profileEntryValue);
                    group.AddVariable(variable);
                    groups[prefix] = group;
                }
            }

            List<ProfileGroupType> groupList = new List<ProfileGroupType>();
            groupList.AddRange(groups.Values.Where(group => group.IsValidGroupType()));
            return groupList;
        }

        /// <summary>
        /// Determines if the group type is a valid
        /// </summary>
        /// <returns>True, if the group type has a prefix, a build path, and a load path, false otherwise</returns>
        internal bool IsValidGroupType()
        {
            return m_GroupTypePrefix != null && GetVariableBySuffix("BuildPath") != null && GetVariableBySuffix("LoadPath") != null;
        }
    }

}