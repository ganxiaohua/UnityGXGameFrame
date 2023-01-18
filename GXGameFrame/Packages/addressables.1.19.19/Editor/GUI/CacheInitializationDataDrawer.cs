using System;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;

namespace UnityEditor.AddressableAssets.GUI
{
    [CustomPropertyDrawer(typeof(CacheInitializationData), true)]
    class CacheInitializationDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Vector2 labelSize = EditorStyles.label.CalcSize(label);
            Rect rectForGUIRow = new Rect(position.x, position.y, position.width, labelSize.y + EditorGUIUtility.standardVerticalSpacing);
            DrawGUI(rectForGUIRow, property, false);
            EditorGUI.EndProperty();
        }

        private float DrawGUI(Rect rectForGUIRow, SerializedProperty property, bool isPreview)
        {
            // y position moves downward to render each row
            rectForGUIRow.y = DrawBundleCompressionGUI(rectForGUIRow, property, isPreview);
            rectForGUIRow.y = DrawCacheDirectoryOverrideGUI(rectForGUIRow, property, isPreview);
            rectForGUIRow.y = DrawExpirationDelayGUI(rectForGUIRow, property, isPreview);
            rectForGUIRow.y = DrawMaxCacheSizeGUI(rectForGUIRow, property, isPreview);
            return rectForGUIRow.y;
        }

        internal float DrawBundleCompressionGUI(Rect rectForGUIRow, SerializedProperty property, bool isPreview)
        {
            var rectStartYPos = rectForGUIRow.y;
            if (!isPreview)
            {
                var prop = property.FindPropertyRelative("m_CompressionEnabled");
                prop.boolValue = EditorGUI.Toggle(rectForGUIRow, new GUIContent("Compress Bundles", "Bundles are recompressed into LZ4 format to optimize load times."), prop.boolValue);
            }
            return rectStartYPos + rectForGUIRow.height + EditorGUIUtility.standardVerticalSpacing;
        }

        internal float DrawCacheDirectoryOverrideGUI(Rect rectForGUIRow, SerializedProperty property, bool isPreview)
        {
            var rectStartYPos = rectForGUIRow.y;
            if (!isPreview)
            {
                var prop = property.FindPropertyRelative("m_CacheDirectoryOverride");
                prop.stringValue = EditorGUI.TextField(rectForGUIRow, new GUIContent("Cache Directory Override", "Specify custom directory for cache.  Leave blank for default."), prop.stringValue);
            }
            return rectStartYPos + rectForGUIRow.height + EditorGUIUtility.standardVerticalSpacing;
        }

        internal float DrawExpirationDelayGUI(Rect rectForGUIRow, SerializedProperty property, bool isPreview)
        {
            var rectStartYPos = rectForGUIRow.y;
            if (!isPreview)
            {
                var prop = property.FindPropertyRelative("m_ExpirationDelay");
                prop.intValue = EditorGUI.IntSlider(rectForGUIRow, new GUIContent("[Obsolete] Expiration Delay (in seconds)", "Controls how long items are left in the cache before deleting."), prop.intValue, 0, 12960000);
                rectForGUIRow.y += rectForGUIRow.height + EditorGUIUtility.standardVerticalSpacing;
                var ts = new TimeSpan(0, 0, prop.intValue);
                EditorGUI.LabelField(new Rect(rectForGUIRow.x + 16, rectForGUIRow.y, rectForGUIRow.width - 16, rectForGUIRow.height), new GUIContent(NicifyTimeSpan(ts)));
            }
            return rectStartYPos + (rectForGUIRow.height + EditorGUIUtility.standardVerticalSpacing) * 2;
        }

        internal float DrawMaxCacheSizeGUI(Rect rectForGUIRow, SerializedProperty property, bool isPreview)
        {
            var rectStartYPos = rectForGUIRow.y;
            var limProp = property.FindPropertyRelative("m_LimitCacheSize");
            if (!isPreview)
            {
                limProp.boolValue = EditorGUI.ToggleLeft(rectForGUIRow, new GUIContent("Limit Cache Size"), limProp.boolValue);

                if (limProp.boolValue)
                {
                    rectForGUIRow.y += rectForGUIRow.height + EditorGUIUtility.standardVerticalSpacing;

                    var prop = property.FindPropertyRelative("m_MaximumCacheSize");
                    if (prop.longValue == long.MaxValue)
                        prop.longValue = (1024 * 1024 * 1024);//default to 1GB

                    var mb = (prop.longValue / (1024 * 1024));
                    var val = EditorGUI.LongField(new Rect(rectForGUIRow.x + 16, rectForGUIRow.y, rectForGUIRow.width - 16, rectForGUIRow.height), new GUIContent("Maximum Cache Size (in MB)", "Controls how large the cache can get before deleting."), mb);
                    if (val != mb)
                        prop.longValue = val * (1024 * 1024);

                    rectForGUIRow.y += rectForGUIRow.height;
                }
            }

            var totalHeight = rectStartYPos + rectForGUIRow.height + EditorGUIUtility.standardVerticalSpacing;
            if (limProp.boolValue)
                totalHeight += rectForGUIRow.height; // add extra line for rendered m_MaximumCacheSize, no extra border space b/c it's the last line
            return totalHeight;
        }

        string NicifyTimeSpan(TimeSpan ts)
        {
            if (ts.Days >= 1)
                return string.Format("{0} days, {1} hours, {2} minutes, {3} seconds.", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            if (ts.Hours >= 1)
                return string.Format("{0} hours, {1} minutes, {2} seconds.", ts.Hours, ts.Minutes, ts.Seconds);
            if (ts.Minutes >= 1)
                return string.Format("{0} minutes, {1} seconds.", ts.Minutes, ts.Seconds);
            return string.Format("{0} seconds.", ts.Seconds);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Vector2 labelSize = EditorStyles.label.CalcSize(label);
            Rect rectForGUIRowWithHeight = new Rect(0, 0, 0, labelSize.y + EditorGUIUtility.standardVerticalSpacing);
            return DrawGUI(rectForGUIRowWithHeight, property, true);
        }
    }
}
