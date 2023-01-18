using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Threading.Tasks;

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
using Unity.Services.CCD.Management.Models;
#endif

namespace UnityEditor.AddressableAssets.GUI
{
    internal class ProfileDataSourceDropdownWindow : PopupWindowContent
    {
        internal Rect m_WindowRect;
        internal enum DropdownState { None, BuiltIn, EditorHosted, CCD, Custom };
        internal DropdownState state;
        internal ProfileGroupType m_GroupType;
        internal const float k_Margin = 4;
        internal const float k_MaxHeight = 286;
        internal Vector2 scrollPos;
        internal delegate void ValueChangedEventHandler(object sender, DropdownWindowEventArgs e);
        internal event ValueChangedEventHandler ValueChanged;

        internal enum CCDDropdownState { Bucket, Badge };
        internal CCDDropdownState CCDState = CCDDropdownState.Bucket;

        //temp variables
        internal List<ProfileGroupType> m_ProfileGroupTypes = new List<ProfileGroupType>();
        internal string m_BucketId;
        internal string m_BucketName;
        internal ProfileGroupType m_Bucket;
        internal bool m_isRefreshingCCDDataSources;
        
        private ProfileDataSourceSettings m_ProfileDataSource;

        internal ProfileDataSourceSettings dataSourceSettings
        {
            get
            {
                if (m_ProfileDataSource == null)
                    m_ProfileDataSource = ProfileDataSourceSettings.GetSettings();
                return m_ProfileDataSource;
            }
        }
        
        static GUIStyle dropdownTitleStyle;
        static GUIStyle menuOptionStyle;
        static GUIStyle horizontalBarStyle;

        internal static string externalLinkIcon = EditorGUIUtility.isProSkin ? "d_ScaleTool" : "ScaleTool";
        internal static string nextIcon = EditorGUIUtility.isProSkin ? "d_tab_next" : "tab_next";
        internal static string backIcon = EditorGUIUtility.isProSkin ? "d_tab_prev" : "tab_prev";
        internal static string refreshIcon = EditorGUIUtility.isProSkin ? "d_refresh" : "refresh";
        internal static string infoIcon = EditorGUIUtility.isProSkin ? "d_UnityEditor.InspectorWindow" : "UnityEditor.InspectorWindow";
        internal static OrgData m_Organization;

        List<BaseOption> options = new List<BaseOption>();

        public ProfileDataSourceDropdownWindow(Rect fieldRect, ProfileGroupType groupType)
        {
            m_GroupType = groupType;
            m_WindowRect = fieldRect;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(m_WindowRect.width, m_WindowRect.height);
        }

        public async override void OnOpen()
        {
            options.Add(new BuiltInOption());
            options.Add(new EditorHostedOption());
            options.Add(new CCDOption());
            options.Add(new CustomOption());

            var blackTexture = new Texture2D(2, 2);
            blackTexture.SetPixels(new Color[4] { Color.black, Color.black, Color.black, Color.black });
            blackTexture.Apply();

            dropdownTitleStyle = new GUIStyle()
            {
                name = "datasource-dropdown-title",
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 26,
                border = new RectOffset(1, 1, 1, 1),
                normal = new GUIStyleState()
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };

            menuOptionStyle = new GUIStyle()
            {
                name = "datasource-dropdown-option",
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                fixedHeight = 20,
                padding = new RectOffset(20, 2, 0, 0),
                normal = new GUIStyleState()
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };

            horizontalBarStyle = new GUIStyle()
            {
                normal =
                {
                    background = blackTexture,
                    scaledBackgrounds = new Texture2D[1]{ blackTexture }
                },
                fixedHeight = 1,
                stretchHeight = false
            };

            if (!string.IsNullOrEmpty(CloudProjectSettings.projectId))
            {
                m_Organization = await GetOrgData();
            }

            SyncProfileGroupTypes();
        }

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
        public override async void OnGUI(Rect window)
#else
        public override void OnGUI(Rect window)
#endif
        {
            Event evt = Event.current;
            Rect horizontalBarRect = new Rect(0, 30, 0, 0);
            Rect backButtonRect = new Rect(5, 0, 30, 30);
            Rect refreshButtonRect = new Rect(window.width - 30 + k_Margin, 0, 30, 30);
            switch (state)
            {
                case DropdownState.None:
                    EditorGUILayout.LabelField("Bundle Locations", dropdownTitleStyle);
                    EditorGUILayout.Space(10);
                    EditorGUI.LabelField(horizontalBarRect, "", new GUIStyle(horizontalBarStyle) { fixedWidth = window.width });
                    //List all options
                    foreach (var option in options)
                    {
                        option.Draw(() =>
                        {
                            state = option.state;
                            switch (option.state)
                            {
                                case DropdownState.BuiltIn:
                                case DropdownState.EditorHosted:
                                    var args = new DropdownWindowEventArgs();
                                    args.GroupType = m_GroupType;
                                    args.Option = option;
                                    args.IsCustom = false;
                                    OnValueChanged(args);
                                    return;
                                case DropdownState.Custom:
                                    var custom = new DropdownWindowEventArgs();
                                    custom.GroupType = m_GroupType;
                                    custom.Option = option;
                                    custom.IsCustom = true;
                                    OnValueChanged(custom);
                                    return;
                                default:
                                    return;
                            }
                        });
                    }
                    return;
                case DropdownState.CCD:
                    switch (CCDState)
                    {
                        case CCDDropdownState.Bucket:
                            EditorGUI.LabelField(backButtonRect, EditorGUIUtility.IconContent(backIcon));
                            if (evt.type == EventType.MouseDown && backButtonRect.Contains(evt.mousePosition))
                            {
                                state = DropdownState.None;
                                CCDState = CCDDropdownState.Bucket;
                                m_WindowRect.height = 120;
                                return;
                            }
#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
                            if (CloudProjectSettings.projectId != String.Empty)
                            {
                                EditorGUI.LabelField(refreshButtonRect, EditorGUIUtility.IconContent(refreshIcon));
                                if (evt.type == EventType.MouseDown && refreshButtonRect.Contains(evt.mousePosition) && !m_isRefreshingCCDDataSources)
                                {
                                    m_isRefreshingCCDDataSources = true;
                                    await ProfileDataSourceSettings.UpdateCCDDataSourcesAsync(CloudProjectSettings.projectId, true);
                                    SyncProfileGroupTypes();
                                    m_isRefreshingCCDDataSources = false;
                                    return;
                                }
                            }
#endif

                            EditorGUILayout.LabelField("Cloud Content Delivery Buckets", dropdownTitleStyle);
                            EditorGUILayout.Space(10);
                            EditorGUI.LabelField(horizontalBarRect, "", new GUIStyle(horizontalBarStyle) { fixedWidth = window.width });

                            if (CloudProjectSettings.projectId == String.Empty)
                            {
                                EditorStyles.helpBox.fontSize = 12;
                                EditorGUILayout.LabelField("Connecting to Cloud Content Delivery requires enabling Cloud Project Settings in the Services Window.", EditorStyles.helpBox);
                            }
                            else
                            {
#if !ENABLE_CCD                 //Used to Display whether or not a user has the CCD Package
                                EditorStyles.helpBox.fontSize = 12;
                                EditorGUILayout.HelpBox("Connecting to Cloud Content Delivery requires the CCD Management SDK Package", MessageType.Warning);
                                var installPackageButton = GUILayout.Button("Install CCD Management SDK Package");
                                if (installPackageButton)
                                {
                                    editorWindow.Close();
                                    AddressableAssetUtility.InstallCCDPackage();
                                }
#else
                                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

                                m_WindowRect.height = m_ProfileGroupTypes.Count > 0 ? k_MaxHeight : 80;


                                Dictionary<string, ProfileGroupType> buckets = new Dictionary<string, ProfileGroupType>();
                                m_ProfileGroupTypes.ForEach((groupType) =>
                                {
                                    var parts = groupType.GroupTypePrefix.Split(ProfileGroupType.k_PrefixSeparator);
                                    var bucketId = parts[2];
                                    var bucketName = groupType.GetVariableBySuffix($"{nameof(CcdBucket)}{nameof(CcdBucket.Name)}");
                                    if (!buckets.ContainsKey(bucketId))
                                        buckets.Add(bucketId, groupType);
                                });

                                CCDOption.DrawBuckets(buckets,
                                    (KeyValuePair<string, ProfileGroupType> bucket) =>
                                    {
                                        CCDState = CCDDropdownState.Badge;
                                        m_BucketId = bucket.Key;
                                        m_Bucket = bucket.Value;
                                    });
                                EditorGUILayout.EndScrollView();

#endif
                            }
                            break;
#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
                        case CCDDropdownState.Badge:
                            EditorGUI.LabelField(backButtonRect, EditorGUIUtility.IconContent(backIcon));
                            if (evt.type == EventType.MouseDown && backButtonRect.Contains(evt.mousePosition))
                            {
                                state = DropdownState.CCD;
                                CCDState = CCDDropdownState.Bucket;
                                m_WindowRect.height = 120;
                            }
                            EditorGUI.LabelField(refreshButtonRect, EditorGUIUtility.IconContent(refreshIcon));
                            if (evt.type == EventType.MouseDown && refreshButtonRect.Contains(evt.mousePosition) && !m_isRefreshingCCDDataSources)
                            {
                                m_isRefreshingCCDDataSources = true;
                                await ProfileDataSourceSettings.UpdateCCDDataSourcesAsync(CloudProjectSettings.projectId, true);
                                SyncProfileGroupTypes();
                                m_isRefreshingCCDDataSources = false;
                                return;
                            }
                            EditorGUILayout.LabelField(String.Format("{0} Badges", m_Bucket.GetVariableBySuffix($"{nameof(CcdBucket)}{nameof(CcdBucket.Name)}").Value), dropdownTitleStyle);
                            EditorGUILayout.Space(10);
                            EditorGUI.LabelField(horizontalBarRect, "", new GUIStyle(horizontalBarStyle) { fixedWidth = window.width });
                            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                            if (bool.Parse(m_Bucket.GetVariableBySuffix("PromoteOnly").Value))
                            {
                                const string promotionOnlyBucketInfo = "Using Build & Release directly to this bucket is not supported, but you can load content.";
                                EditorStyles.helpBox.fontSize = 11;
                                EditorStyles.helpBox.margin = new RectOffset(20, 20, 5, 5);
                                EditorGUILayout.HelpBox(promotionOnlyBucketInfo, MessageType.Info);

                            }
                            var selectedProfileGroupTypes = m_ProfileGroupTypes.Where(groupType =>
                                groupType.GroupTypePrefix.StartsWith(
                                    String.Join(
                                    ProfileGroupType.k_PrefixSeparator.ToString(), new string[] { "CCD", CloudProjectSettings.projectId, m_BucketId }
                                    ))).ToList();

                            m_WindowRect.height = m_ProfileGroupTypes.Count > 0 ? k_MaxHeight : 80;

                            HashSet<ProfileGroupType> groupTypes = new HashSet<ProfileGroupType>();
                            selectedProfileGroupTypes.ForEach((groupType) =>
                            {
                                var parts = groupType.GroupTypePrefix.Split(ProfileGroupType.k_PrefixSeparator);
                                var badgeName = String.Join(ProfileGroupType.k_PrefixSeparator.ToString(), parts, 3, parts.Length - 3);
                                if (!groupTypes.Contains(groupType))
                                    groupTypes.Add(groupType);
                            });


                            CCDOption.DrawBadges(groupTypes, m_BucketId, (ProfileGroupType groupType) =>
                            {
                                var args = new DropdownWindowEventArgs();
                                args.GroupType = m_GroupType;
                                args.Option = new CCDOption();
                                args.Option.BuildPath = groupType.GetVariableBySuffix("BuildPath").Value;
                                args.Option.LoadPath = groupType.GetVariableBySuffix("LoadPath").Value;
                                args.IsCustom = false;
                                OnValueChanged(args);
                                editorWindow.Close();
                            });
                            EditorGUILayout.EndScrollView();
                            break;
                        default:
                            CCDState = CCDDropdownState.Bucket;
                            break;

#endif
                    }
                    break;
                case DropdownState.BuiltIn:
                case DropdownState.EditorHosted:
                default:
                    editorWindow.Close();
                    break;
            }

        }

        private void SyncProfileGroupTypes()
        {
            m_ProfileGroupTypes = dataSourceSettings.GetGroupTypesByPrefix("CCD" + ProfileGroupType.k_PrefixSeparator + CloudProjectSettings.projectId);
        }

        private async Task<OrgData> GetOrgData()
        {
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + CloudProjectSettings.accessToken);
                var response = await client.GetAsync(String.Format("https://api.unity.com/v1/core/api/orgs/{0}", CloudProjectSettings.organizationId));
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to retrieve org data.");
                }
                var data = await response.Content.ReadAsStringAsync();
                return OrgData.ParseOrgData(data);
            }
        }

        protected virtual void OnValueChanged(DropdownWindowEventArgs e)
        {
            ValueChangedEventHandler handler = ValueChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public class DropdownWindowEventArgs : EventArgs
        {
            public ProfileGroupType GroupType { get; set; }
            public BaseOption Option { get; set; }
            public bool IsCustom { get; set; }
        }

        internal abstract class BaseOption
        {
            internal string OptionName;
            internal DropdownState state;
            internal string BuildPath;
            internal string LoadPath;
            internal abstract void Draw(Action action);

            internal static void DrawMenuItem(string title, string displayIcon, Action action)
            {
                Rect labelRect = EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(title, menuOptionStyle);
                EditorGUILayout.EndHorizontal();
                if (displayIcon != null)
                {
                    Rect linkRect = new Rect(labelRect.x + labelRect.width - menuOptionStyle.fixedHeight - 10, labelRect.y, menuOptionStyle.fixedHeight, menuOptionStyle.fixedHeight);
                    EditorGUI.LabelField(linkRect, EditorGUIUtility.IconContent(displayIcon));
                }
                if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
                {
                    action.Invoke();
                }
            }

            internal static void DrawMenuItemWithArg<T>(string title, Action<T> action, T arg, string infoIcon = null, string displayIcon = null)
            {
                Rect labelRect = EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(title, menuOptionStyle);
                EditorGUILayout.EndHorizontal();

                if (infoIcon != null)
                {
                    Rect infoRect = new Rect(labelRect.x, labelRect.y + 2, menuOptionStyle.fixedHeight, menuOptionStyle.fixedHeight);
                    EditorGUI.LabelField(infoRect, EditorGUIUtility.IconContent(infoIcon));
                }

                if (displayIcon != null)
                {
                    Rect linkRect = new Rect(labelRect.x + labelRect.width - menuOptionStyle.fixedHeight, labelRect.y, menuOptionStyle.fixedHeight, menuOptionStyle.fixedHeight);
                    EditorGUI.LabelField(linkRect, EditorGUIUtility.IconContent(displayIcon));
                }
                if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
                {
                    action.Invoke(arg);
                }
            }

        }

        internal class BuiltInOption : BaseOption
        {
            internal BuiltInOption()
            {
                OptionName = "Built-In";
                state = DropdownState.BuiltIn;
                BuildPath = AddressableAssetSettings.kLocalBuildPathValue;
                LoadPath = AddressableAssetSettings.kLocalLoadPathValue;
            }

            internal override void Draw(Action action)
            {
                DrawMenuItem(OptionName, null, action);
            }
        }

        internal class EditorHostedOption : BaseOption
        {
            internal EditorHostedOption()
            {
                OptionName = "Editor Hosted";
                state = DropdownState.EditorHosted;
                BuildPath = AddressableAssetSettings.kRemoteBuildPathValue;
                LoadPath = AddressableAssetSettings.RemoteLoadPathValue;
            }

            internal override void Draw(Action action)
            {
                DrawMenuItem(OptionName, null, action);
            }
        }

        internal class CustomOption : BaseOption
        {
            internal CustomOption()
            {
                OptionName = "Custom";
                state = DropdownState.Custom;
                BuildPath = null;
                LoadPath = null;
            }

            internal override void Draw(Action action)
            {
                DrawMenuItem(OptionName, null, action);
            }
        }


        internal class CCDOption : BaseOption
        {

            internal CCDOption()
            {
                OptionName = "Cloud Content Delivery";
                state = DropdownState.CCD;
                BuildPath = null;
                LoadPath = null;
            }

            internal override void Draw(Action action)
            {
                DrawMenuItem(OptionName, nextIcon, action);
            }

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
            internal static void DrawBuckets(Dictionary<string, ProfileGroupType> buckets, Action<KeyValuePair<string, ProfileGroupType>> action)
            {
                if (buckets.Count > 0)
                {
                    foreach (var bucket in buckets)
                    {
                        bool showInfo = bool.Parse(bucket.Value.GetVariableBySuffix(nameof(CcdBucket.Attributes.PromoteOnly)).Value);
                        DrawMenuItemWithArg(
                            bucket.Value.GetVariableBySuffix($"{nameof(CcdBucket)}{nameof(CcdBucket.Name)}").Value,
                            action,
                            new KeyValuePair<string, ProfileGroupType>(bucket.Key, bucket.Value),
                            showInfo ? infoIcon : null,
                            nextIcon);
                    }
                }
                else
                {
                    DrawCompleteCCDOnBoarding();
                }
                DrawCreateBucket();
            }

            internal static void DrawBadges(HashSet<ProfileGroupType> groupTypes, string bucketId, Action<ProfileGroupType> action)
            {
                if (groupTypes.Count > 0)
                {
                    foreach (var groupType in groupTypes)
                    {
                        DrawMenuItemWithArg(groupType.GetVariableBySuffix($"{nameof(CcdBadge)}{nameof(CcdBadge.Name)}").Value, action, groupType);
                    }
                }

                DrawCreateBadge(bucketId);
            }
#endif

            internal static void DrawCreateBucket()
            {
                DrawMenuItem("<a>Create new bucket</a>", null, () =>
                {
                    Application.OpenURL(
                        String.Format("https://dashboard.unity3d.com/organizations/{0}/projects/{1}/cloud-content-delivery",
                        m_Organization.foreign_key,
                        CloudProjectSettings.projectId));
                });
                var lastRect = GUILayoutUtility.GetLastRect();
                lastRect.y += 2;
                EditorGUI.LabelField(lastRect, "<a>_____________________</a>", menuOptionStyle);
            }

            internal static void DrawCreateBadge(string bucketId)
            {
                DrawMenuItem("<a>Create new badge</a>", null, () =>
                {
                    Application.OpenURL(
                        String.Format("https://dashboard.unity3d.com/organizations/{0}/projects/{1}/cloud-content-delivery/buckets/{2}/badges",
                        m_Organization.foreign_key,
                        CloudProjectSettings.projectId,
                        bucketId));
                });
                var lastRect = GUILayoutUtility.GetLastRect();
                lastRect.y += 2;
                EditorGUI.LabelField(lastRect, "<a>____________________</a>", menuOptionStyle);
            }

            internal static void DrawCompleteCCDOnBoarding()
            {
                DrawMenuItem("<a>Complete CCD Onboarding</a>", null, () =>
                {
                    Application.OpenURL(
                        String.Format("https://dashboard.unity3d.com/organizations/{0}/projects/{1}/cloud-content-delivery/onboarding",
                        m_Organization.foreign_key,
                        CloudProjectSettings.projectId));
                });
                var lastRect = GUILayoutUtility.GetLastRect();
                lastRect.y += 2;
                EditorGUI.LabelField(lastRect, "<a>______________________________</a>", menuOptionStyle);
            }
        }
    }
}