using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
using Unity.Services.CCD.Management;
using Unity.Services.CCD.Management.Apis.Badges;
using Unity.Services.CCD.Management.Apis.Buckets;
using Unity.Services.CCD.Management.Badges;
using Unity.Services.CCD.Management.Buckets;
using Unity.Services.CCD.Management.Http;
using Unity.Services.CCD.Management.Models;
#endif

namespace UnityEditor.AddressableAssets.Settings
{
    /// <summary>
    /// Scriptable Object that holds data source setting information for the profile data source dropdown window
    /// </summary>
    public class ProfileDataSourceSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        const string DEFAULT_PATH = "Assets/AddressableAssetsData";
        const string DEFAULT_NAME = "ProfileDataSourceSettings";
        const string CONTENT_RANGE_HEADER = "Content-Range";
        static string DEFAULT_SETTING_PATH = $"{DEFAULT_PATH}/{DEFAULT_NAME}.asset";

        /// <summary>
        /// Group types that exist within the settings object
        /// </summary>
        [SerializeField]
        public List<ProfileGroupType> profileGroupTypes = new List<ProfileGroupType>();

        /// <summary>
        /// Creates, if needed, and returns the profile data source settings for the project
        /// </summary>
        /// <param name="path">Desired path to put settings</param>
        /// <param name="settingName">Desired name for settings</param>
        /// <returns></returns>
        public static ProfileDataSourceSettings Create(string path = null, string settingName = null)
        {
            ProfileDataSourceSettings aa;
            var assetPath = DEFAULT_SETTING_PATH;

            if (path != null && settingName != null)
            {
                assetPath = $"{path}/{settingName}.asset";
            }

            aa = AssetDatabase.LoadAssetAtPath<ProfileDataSourceSettings>(assetPath);
            if (aa == null)
            {
                Directory.CreateDirectory(path != null ? path : DEFAULT_PATH);
                aa = CreateInstance<ProfileDataSourceSettings>();
                AssetDatabase.CreateAsset(aa, assetPath);
                aa = AssetDatabase.LoadAssetAtPath<ProfileDataSourceSettings>(assetPath);
                aa.profileGroupTypes = CreateDefaultGroupTypes();
                EditorUtility.SetDirty(aa);
            }
            return aa;
        }

        /// <summary>
        /// Gets the profile data source settings for the project
        /// </summary>
        /// <param name="path"></param>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public static ProfileDataSourceSettings GetSettings(string path = null, string settingName = null)
        {
            ProfileDataSourceSettings aa;
            var assetPath = DEFAULT_SETTING_PATH;

            if (path != null && settingName != null)
            {
                assetPath = $"{path}/{settingName}.asset";
            }

            aa = AssetDatabase.LoadAssetAtPath<ProfileDataSourceSettings>(assetPath);
            if (aa == null)
                return Create();
            return aa;
        }

        /// <summary>
        /// Creates a list of default group types that are automatically added on ProfileDataSourceSettings object creation
        /// </summary>
        /// <returns>List of ProfileGroupTypes: Built-In and Editor Hosted</returns>
        public static List<ProfileGroupType> CreateDefaultGroupTypes() => new List<ProfileGroupType>{CreateBuiltInGroupType(), CreateEditorHostedGroupType()};

        static ProfileGroupType CreateBuiltInGroupType()
        {
            ProfileGroupType defaultBuiltIn = new ProfileGroupType(AddressableAssetSettings.LocalGroupTypePrefix);
            defaultBuiltIn.AddVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kBuildPath, AddressableAssetSettings.kLocalBuildPathValue));
            defaultBuiltIn.AddVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kLoadPath, AddressableAssetSettings.kLocalLoadPathValue));
            return defaultBuiltIn;
        }

        static ProfileGroupType CreateEditorHostedGroupType()
        {
            ProfileGroupType defaultRemote = new ProfileGroupType(AddressableAssetSettings.EditorHostedGroupTypePrefix);
            defaultRemote.AddVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kBuildPath, AddressableAssetSettings.kRemoteBuildPathValue));
            defaultRemote.AddVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kLoadPath, AddressableAssetSettings.RemoteLoadPathValue));
            return defaultRemote;
        }

        /// <summary>
        /// Given a valid profileGroupType, searches the settings and returns, if exists, the profile group type
        /// </summary>
        /// <param name="groupType"></param>
        /// <returns>ProfileGroupType if found, null otherwise</returns>
        public ProfileGroupType FindGroupType(ProfileGroupType groupType)
        {
            ProfileGroupType result = null;
            if (!groupType.IsValidGroupType())
            {
                throw new ArgumentException("Group Type is not valid. Group Type must include a build path and load path variables");
            }

            foreach (ProfileGroupType settingsGroupType in profileGroupTypes)
            {
                var buildPath = groupType.GetVariableBySuffix(AddressableAssetSettings.kBuildPath);
                var foundBuildPath = settingsGroupType.ContainsVariable(buildPath);
                var loadPath = groupType.GetVariableBySuffix(AddressableAssetSettings.kLoadPath);
                var foundLoadPath = settingsGroupType.ContainsVariable(loadPath);
                if (foundBuildPath && foundLoadPath)
                {
                    result = settingsGroupType;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a list of ProfileGroupType that matches the given prefix
        /// </summary>
        /// <param name="prefix">prefix to search by</param>
        /// <returns>List of ProfileGroupType</returns>
        public List<ProfileGroupType> GetGroupTypesByPrefix(string prefix)
        {
            return profileGroupTypes.Where((groupType) => groupType.GroupTypePrefix.StartsWith(prefix)).ToList();
        }

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
        /// <summary>
        /// Updates the CCD buckets and badges with the data source settings
        /// </summary>
        /// <param name="projectId">Project Id connected to Unity Services</param>
        /// <param name="showInfoLog">Whether or not to show debug logs or not</param>
        /// <returns>List of ProfileGroupType</returns>
        public static async Task<List<ProfileGroupType>> UpdateCCDDataSourcesAsync(string projectId, bool showInfoLog)
        {
            if (showInfoLog) Addressables.Log("Syncing CCD Buckets and Badges.");
            var settings = GetSettings();
            var profileGroupTypes = new List<ProfileGroupType>();
            profileGroupTypes.AddRange(CreateDefaultGroupTypes());

            await CCDManagementAPIService.SetConfigurationAuthHeader(CloudProjectSettings.accessToken);
            var bucketDictionary = await GetAllBucketsAsync(projectId);
            foreach (var kvp in bucketDictionary)
            {
                var bucket = kvp.Value;
                var badges = await GetAllBadgesAsync(projectId, bucket.Id.ToString());
                if (badges.Count == 0) badges.Add(new CcdBadge(name: "latest"));
                foreach (var badge in badges)
                {
                    var groupType = new ProfileGroupType($"CCD{ProfileGroupType.k_PrefixSeparator}{projectId}{ProfileGroupType.k_PrefixSeparator}{bucket.Id}{ProfileGroupType.k_PrefixSeparator}{badge.Name}");
                    groupType.AddVariable(new ProfileGroupType.GroupTypeVariable($"{nameof(CcdBucket)}{nameof(CcdBucket.Name)}", bucket.Name));
                    groupType.AddVariable(new ProfileGroupType.GroupTypeVariable($"{nameof(CcdBucket)}{nameof(CcdBucket.Id)}", bucket.Id.ToString()));
                    groupType.AddVariable(new ProfileGroupType.GroupTypeVariable($"{nameof(CcdBadge)}{nameof(CcdBadge.Name)}", badge.Name));
                    groupType.AddVariable(new ProfileGroupType.GroupTypeVariable(nameof(CcdBucket.Attributes.PromoteOnly), bucket.Attributes.PromoteOnly.ToString()));

                    string buildPath = $"{AddressableAssetSettings.kCCDBuildDataPath}/{bucket.Id}/{badge.Name}";
                    groupType.AddVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kBuildPath, buildPath));

                    string loadPath = $"https://{projectId}.client-api.unity3dusercontent.com/client_api/v1/buckets/{bucket.Id}/release_by_badge/{badge.Name}/entry_by_path/content/?path=";
                    groupType.AddVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kLoadPath, loadPath));

                    profileGroupTypes.Add(groupType);
                }
            }
            settings.profileGroupTypes = profileGroupTypes;
            if (showInfoLog) Addressables.Log("Successfully synced CCD Buckets and Badges.");
            EditorUtility.SetDirty(settings);
            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(settings);
            return settings.profileGroupTypes;
        }

        private static async Task<Dictionary<Guid, CcdBucket>> GetAllBucketsAsync(string projectId)
        {
            int numBuckets = Int32.MaxValue;
            int page = 1;
            List<CcdBucket> buckets = new List<CcdBucket>();
            BucketsApiClient client = new BucketsApiClient(new HttpClient());
            do
            {
                ListBucketsByProjectRequest request = new ListBucketsByProjectRequest(projectId, page);
                Response<List<CcdBucket>> response = await client.ListBucketsByProjectAsync(request);
                if (response.Result.Count > 0)
                    buckets.AddRange(response.Result);
                if (page == 1)
                {
                    response.Headers.TryGetValue(CONTENT_RANGE_HEADER, out string contentLength);
                    // content-range: items x-y/z => grab z
                    numBuckets = Int32.Parse(contentLength.Split('/')[1]);
                }
                page++;
            } while (buckets.Count < numBuckets);
            return buckets.ToDictionary(kvp => kvp.Id, kvp => kvp);
        }

        private static async Task<List<CcdBadge>> GetAllBadgesAsync(string projectId, string bucketId)
        {
            int numBadges = Int32.MaxValue;
            int page = 1;
            List<CcdBadge> badges = new List<CcdBadge>();
            BadgesApiClient client = new BadgesApiClient(new HttpClient());
            do
            {
                ListBadgesRequest request = new ListBadgesRequest(bucketId, projectId, page);
                Response<List<CcdBadge>> response = await client.ListBadgesAsync(request);
                if (response.Result.Count > 0)
                    badges.AddRange(response.Result);
                if (page == 1)
                {
                    response.Headers.TryGetValue(CONTENT_RANGE_HEADER, out string contentLength);
                    // content-range: items x-y/z => grab z
                    numBadges = Int32.Parse(contentLength.Split('/')[1]);
                }
                page++;
            } while (badges.Count < numBadges);
            return badges;
        }
#endif
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Ensure static Group types have the correct string
            // Local
            var types = GetGroupTypesByPrefix(AddressableAssetSettings.LocalGroupTypePrefix);
            if (types == null || types.Count == 0)
                profileGroupTypes.Add(CreateBuiltInGroupType());
            else
            {
                types[0].AddOrUpdateVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kBuildPath, 
                    AddressableAssetSettings.kLocalBuildPathValue));
                types[0].AddOrUpdateVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kLoadPath,
                    AddressableAssetSettings.kLocalLoadPathValue));
            }
            
            // Editor Hosted
            types = GetGroupTypesByPrefix(AddressableAssetSettings.EditorHostedGroupTypePrefix);
            if (types.Count == 0)
                profileGroupTypes.Add(CreateEditorHostedGroupType());
            else
            {
                types[0].AddOrUpdateVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kBuildPath,
                    AddressableAssetSettings.kRemoteBuildPathValue));
                types[0].AddOrUpdateVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kLoadPath,
                    AddressableAssetSettings.RemoteLoadPathValue));
            }
        }
    }
}
