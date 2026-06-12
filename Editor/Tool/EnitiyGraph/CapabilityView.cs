using System.Collections.Generic;
using GameFrame.Runtime;
using UnityEditor;
using UnityEngine;
using OdinEditorWindow = Sirenix.OdinInspector.Editor.OdinEditorWindow;

namespace GameFrame.Editor
{
    public class CapabilityView : OdinEditorWindow
    {
        private enum CapabilityType
        {
            NotAction = 1,
            Action,
            Lock
        }

        private const int FrameSize = 500;
        private const float HeaderHeight = 78f;
        private const float LabelWidth = 230f;
        private const float RowHeight = 28f;
        private const float TrackHeight = 16f;
        private const float RulerHeight = 22f;
        private const float SectionHeaderHeight = 30f;
        private const float ToolbarHeight = 36f;
        private const float ContentPadding = 12f;
        private const int GridStep = 50;
        private const string FocusedCapabilityEditorPrefsPrefix = "GameFrame.Editor.CapabilityView.Focused.";

        private static readonly Color UnknownColor = new Color(0.18f, 0.20f, 0.23f, 1f);
        private static readonly Color IdleColor = new Color(0.36f, 0.40f, 0.46f, 1f);
        private static readonly Color ActiveColor = new Color(0.00f, 0.74f, 0.92f, 1f);
        private static readonly Color LockedColor = new Color(1.00f, 0.72f, 0.22f, 1f);
        private static readonly Color DimAccentColor = new Color(0.38f, 0.41f, 0.46f, 1f);
        private static readonly Color[] StateColor = {UnknownColor, IdleColor, ActiveColor, LockedColor};

        private static readonly Color DarkBackground = new Color(0.105f, 0.115f, 0.13f, 1f);
        private static readonly Color LightBackground = new Color(0.78f, 0.80f, 0.84f, 1f);
        private static readonly Color DarkPanel = new Color(0.145f, 0.16f, 0.185f, 1f);
        private static readonly Color LightPanel = new Color(0.90f, 0.91f, 0.93f, 1f);
        private static readonly Color DarkTrack = new Color(0.075f, 0.085f, 0.10f, 1f);
        private static readonly Color LightTrack = new Color(0.68f, 0.70f, 0.74f, 1f);
        private static readonly Color DarkFocusedRow = new Color(0.12f, 0.20f, 0.24f, 1f);
        private static readonly Color LightFocusedRow = new Color(0.82f, 0.94f, 0.98f, 1f);
        private static readonly Color DarkDimmedRow = new Color(0.08f, 0.09f, 0.105f, 1f);
        private static readonly Color LightDimmedRow = new Color(0.76f, 0.77f, 0.79f, 1f);
        private static readonly Color DarkDimmedTrack = new Color(0.12f, 0.13f, 0.15f, 1f);
        private static readonly Color LightDimmedTrack = new Color(0.58f, 0.59f, 0.62f, 1f);
        private static readonly Color GridColor = new Color(1f, 1f, 1f, 0.07f);

        private static CapabilityView sWindow;

        private EffEntity effEntity;
        private ECCWorld eccWorld;
        private readonly List<CapabilityBase> capabilityBaseUpdateMode = new List<CapabilityBase>();
        private readonly List<CapabilityBase> capabilityBaseFixUpdateMode = new List<CapabilityBase>();
        private ArrayExSimilar[] updateMode;
        private ArrayExSimilar[] fixUpdateMode;
        private int tailFrame;
        private int headFrame;
        private bool isShowFocusedOnly;
        private Vector2 scrollPosition = Vector2.zero;

        private GUIStyle headerTitleStyle;
        private GUIStyle headerSubTitleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle labelStyle;
        private GUIStyle mutedLabelStyle;
        private GUIStyle dimmedLabelStyle;
        private GUIStyle focusButtonStyle;
        private GUIStyle pillStyle;
        private GUIStyle centerStyle;

        private static Color BackgroundColor => EditorGUIUtility.isProSkin ? DarkBackground : LightBackground;
        private static Color PanelColor => EditorGUIUtility.isProSkin ? DarkPanel : LightPanel;
        private static Color TrackColor => EditorGUIUtility.isProSkin ? DarkTrack : LightTrack;
        private static Color FocusedRowColor => EditorGUIUtility.isProSkin ? DarkFocusedRow : LightFocusedRow;
        private static Color DimmedRowColor => EditorGUIUtility.isProSkin ? DarkDimmedRow : LightDimmedRow;
        private static Color DimmedTrackColor => EditorGUIUtility.isProSkin ? DarkDimmedTrack : LightDimmedTrack;
        private static Color PrimaryText => EditorGUIUtility.isProSkin ? new Color(0.91f, 0.93f, 0.96f, 1f) : new Color(0.12f, 0.13f, 0.15f, 1f);
        private static Color SecondaryText => EditorGUIUtility.isProSkin ? new Color(0.62f, 0.67f, 0.72f, 1f) : new Color(0.34f, 0.36f, 0.40f, 1f);
        private static Color DimmedText => EditorGUIUtility.isProSkin ? new Color(0.42f, 0.45f, 0.50f, 1f) : new Color(0.50f, 0.52f, 0.56f, 1f);

        public static void Init(EffEntity effEntity)
        {
            sWindow ??= GetWindow<CapabilityView>();
            sWindow.titleContent.text = string.IsNullOrEmpty(effEntity.Name) ? "Entity" : effEntity.Name;
            sWindow.effEntity = effEntity;
            sWindow.eccWorld = (ECCWorld) effEntity.world;
            sWindow.Start();
            sWindow.Show();
        }

        private void Start()
        {
            tailFrame = 0;
            headFrame = 0;
            isShowFocusedOnly = false;
            scrollPosition = Vector2.zero;
            capabilityBaseUpdateMode.Clear();
            capabilityBaseFixUpdateMode.Clear();
            eccWorld.GetCapability(effEntity, capabilityBaseUpdateMode, capabilityBaseFixUpdateMode);
            updateMode = new ArrayExSimilar[capabilityBaseUpdateMode.Count];
            fixUpdateMode = new ArrayExSimilar[capabilityBaseFixUpdateMode.Count];
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            EnsureStyles();
            EditorGUI.DrawRect(new Rect(0f, 0f, position.width, position.height), BackgroundColor);

            if (effEntity == null)
            {
                DrawCenteredMessage("No entity selected.");
                return;
            }

            if (effEntity.State == IEntity.EntityState.IsClear)
            {
                DrawCenteredMessage("Entity has been cleared.");
                return;
            }

            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                SetData(capabilityBaseUpdateMode, updateMode);
                SetData(capabilityBaseFixUpdateMode, fixUpdateMode);
                tailFrame++;
                if (tailFrame >= FrameSize)
                {
                    headFrame = (tailFrame + 1) % FrameSize;
                }
            }

            DrawHeader();
            GUILayout.Space(8f);
            DrawToolbar();
            GUILayout.Space(6f);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
            GUILayout.Space(4f);
            DrawSection("Update", "Per-frame capability sampling", capabilityBaseUpdateMode, updateMode, ActiveColor);
            GUILayout.Space(10f);
            DrawSection("Fixed Update", "Physics-step capability sampling", capabilityBaseFixUpdateMode, fixUpdateMode, LockedColor);
            GUILayout.Space(12f);
            EditorGUILayout.EndScrollView();
        }

        private void SetData(List<CapabilityBase> baseMode, ArrayExSimilar[] capabilityTypes)
        {
            int count = baseMode.Count;
            for (int i = 0; i < count; i++)
            {
                var capability = baseMode[i];
                capabilityTypes[i] ??= new ArrayExSimilar(FrameSize, capability.GetType().Name);
                bool isLock = false;
                if (capability.TagList != null)
                    isLock = eccWorld.IsBindCapability(effEntity, capability.TagList);
                capabilityTypes[i].Add(tailFrame % FrameSize, (int) (isLock ? CapabilityType.Lock : (capability.IsActive ? CapabilityType.Action : CapabilityType.NotAction)), headFrame);
            }
        }

        private void DrawHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, HeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 2f), ActiveColor);

            string entityName = string.IsNullOrEmpty(effEntity.Name) ? "Entity" : effEntity.Name;
            GUI.Label(new Rect(rect.x + 16f, rect.y + 10f, rect.width - 150f, 24f), "Capability Timeline", headerTitleStyle);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 35f, rect.width - 150f, 18f),
                $"{entityName}   ID:{effEntity.ID}   State:{effEntity.State}   Frames:{Mathf.Min(tailFrame, FrameSize)}/{FrameSize}", headerSubTitleStyle);

            DrawStatusPill(new Rect(rect.xMax - 118f, rect.y + 14f, 96f, 24f));

            Rect legendRect = new Rect(rect.x + 16f, rect.y + 55f, rect.width - 32f, 18f);
            float x = 0f;
            x = DrawLegendItem(legendRect, x, "Active", ActiveColor);
            x = DrawLegendItem(legendRect, x + 18f, "Idle", IdleColor);
            x = DrawLegendItem(legendRect, x + 18f, "Locked", LockedColor);
            DrawLegendItem(legendRect, x + 18f, $"Focused {CountFocusedRows()}", CountFocusedRows() > 0 ? LockedColor : DimAccentColor);
        }

        private void DrawToolbar()
        {
            int totalCount = capabilityBaseUpdateMode.Count + capabilityBaseFixUpdateMode.Count;
            int focusedCount = CountFocusedRows();
            Rect rect = GUILayoutUtility.GetRect(0f, ToolbarHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(1f, 1f, 1f, 0.08f));

            GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, Mathf.Max(180f, rect.width - 292f), 20f),
                $"Runtime capabilities: {totalCount}   Focused: {focusedCount}", sectionStyle);

            Rect clearFocusRect = new Rect(rect.xMax - 276f, rect.y + 5f, 126f, 25f);
            EditorGUI.BeginDisabledGroup(focusedCount == 0);
            if (GUI.Button(clearFocusRect, "Clear Focus"))
            {
                ClearFocusedCapabilities();
                GUIUtility.ExitGUI();
            }

            EditorGUI.EndDisabledGroup();

            Rect focusRect = new Rect(rect.xMax - 142f, rect.y + 5f, 128f, 25f);
            if (GUI.Button(focusRect, isShowFocusedOnly ? "Show All" : "Only Focused"))
            {
                isShowFocusedOnly = !isShowFocusedOnly;
            }
        }

        private void DrawStatusPill(Rect rect)
        {
            bool recording = EditorApplication.isPlaying && !EditorApplication.isPaused;
            string text = recording ? "Recording" : EditorApplication.isPaused ? "Paused" : "Stopped";
            Color color = recording ? ActiveColor : EditorApplication.isPaused ? LockedColor : IdleColor;
            EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, 0.22f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), color);
            GUI.Label(rect, text, pillStyle);
        }

        private float DrawLegendItem(Rect area, float x, string label, Color color)
        {
            Rect swatch = new Rect(area.x + x, area.y + 4f, 10f, 10f);
            EditorGUI.DrawRect(swatch, color);
            Vector2 labelSize = mutedLabelStyle.CalcSize(new GUIContent(label));
            GUI.Label(new Rect(swatch.xMax + 6f, area.y, labelSize.x + 4f, area.height), label, mutedLabelStyle);
            return x + swatch.width + 6f + labelSize.x;
        }

        private void DrawSection(string title, string subtitle, List<CapabilityBase> capabilities, ArrayExSimilar[] timelines, Color accent)
        {
            var focused = new List<int>();
            var normal = new List<int>();
            CollectCapabilityIndices(capabilities, focused, normal);
            int displayCount = isShowFocusedOnly ? focused.Count : capabilities.Count;

            DrawSectionHeader(title, subtitle, displayCount, capabilities.Count, accent);
            DrawRuler();

            if (displayCount == 0)
            {
                DrawEmptyRow(capabilities.Count == 0 ? "No capabilities registered." : "No focused capabilities.");
                return;
            }

            int rowIndex = 0;
            rowIndex = DrawCapabilityGroup(capabilities, timelines, focused, rowIndex, focused.Count > 0);
            if (!isShowFocusedOnly)
                DrawCapabilityGroup(capabilities, timelines, normal, rowIndex, focused.Count > 0);
        }

        private int DrawCapabilityGroup(List<CapabilityBase> capabilities, ArrayExSimilar[] timelines, List<int> indices, int rowIndex, bool hasFocusedRows)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                int capabilityIndex = indices[i];
                string name = capabilities[capabilityIndex].GetType().Name;
                bool focused = IsCapabilityFocused(capabilities[capabilityIndex]);
                ArrayExSimilar timeline = timelines != null && capabilityIndex < timelines.Length ? timelines[capabilityIndex] : null;
                DrawTrack(name, timeline, rowIndex, capabilities[capabilityIndex], focused, hasFocusedRows && !focused);
                rowIndex++;
            }

            return rowIndex;
        }

        private void DrawSectionHeader(string title, string subtitle, int displayCount, int totalCount, Color accent)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, SectionHeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4f, rect.height), accent);
            string countText = displayCount == totalCount ? $"{title}  ({totalCount})" : $"{title}  ({displayCount}/{totalCount})";
            GUI.Label(new Rect(rect.x + 12f, rect.y + 4f, LabelWidth, 20f), countText, sectionStyle);
            GUI.Label(new Rect(rect.x + LabelWidth, rect.y + 5f, rect.width - LabelWidth - 12f, 18f), subtitle, mutedLabelStyle);
        }

        private void DrawRuler()
        {
            Rect rect = GUILayoutUtility.GetRect(LabelWidth + 260f, RulerHeight, GUILayout.ExpandWidth(true));
            Rect labelRect = new Rect(rect.x + ContentPadding, rect.y, LabelWidth - ContentPadding, RulerHeight);
            Rect timelineRect = GetTimelineRect(rect);

            GUI.Label(labelRect, "Capability", mutedLabelStyle);
            GUI.Label(new Rect(timelineRect.x, rect.y, 160f, RulerHeight), $"Last {FrameSize} frames", mutedLabelStyle);

            float scale = timelineRect.width / FrameSize;
            for (int frame = GridStep; frame <= FrameSize; frame += GridStep)
            {
                float x = timelineRect.x + frame * scale;
                Color color = frame % 100 == 0 ? new Color(1f, 1f, 1f, 0.16f) : GridColor;
                EditorGUI.DrawRect(new Rect(x, timelineRect.y + 5f, 1f, 12f), color);
                if (frame % 100 == 0)
                {
                    GUI.Label(new Rect(x + 3f, rect.y, 40f, RulerHeight), frame.ToString(), mutedLabelStyle);
                }
            }
        }

        private void DrawTrack(string name, ArrayExSimilar timeline, int rowIndex, CapabilityBase capability, bool focused, bool dimmed)
        {
            Rect rowRect = GUILayoutUtility.GetRect(LabelWidth + 260f, RowHeight, GUILayout.ExpandWidth(true));
            if (focused)
                EditorGUI.DrawRect(rowRect, FocusedRowColor);
            else if (dimmed)
                EditorGUI.DrawRect(rowRect, DimmedRowColor);
            else if (rowIndex % 2 == 1)
                EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 1f, EditorGUIUtility.isProSkin ? 0.018f : 0.12f));

            Color accent = focused ? LockedColor : dimmed ? DimAccentColor : ActiveColor;
            EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, 3f, rowRect.height), accent);

            Rect labelRect = new Rect(rowRect.x + ContentPadding, rowRect.y + 3f, LabelWidth - ContentPadding * 1.5f - 74f, RowHeight - 6f);
            Rect focusRect = new Rect(rowRect.x + LabelWidth - 72f, rowRect.y + 4f, 64f, RowHeight - 8f);
            Rect timelineRect = GetTimelineRect(rowRect);
            Rect trackRect = new Rect(timelineRect.x, rowRect.y + (RowHeight - TrackHeight) * 0.5f, timelineRect.width, TrackHeight);

            GUI.Label(labelRect, name, dimmed ? dimmedLabelStyle : labelStyle);
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = focused ? new Color(LockedColor.r, LockedColor.g, LockedColor.b, 0.9f) : previousBackground;
            if (GUI.Button(focusRect, focused ? "Focused" : "Focus", focusButtonStyle))
            {
                SetCapabilityFocused(capability, !focused);
                GUIUtility.ExitGUI();
            }

            GUI.backgroundColor = previousBackground;

            EditorGUI.DrawRect(trackRect, dimmed ? DimmedTrackColor : TrackColor);
            DrawTimelineGrid(trackRect);

            if (timeline == null)
            {
                GUI.Label(trackRect, "waiting for samples", centerStyle);
                return;
            }

            var datas = timeline.GetData();
            if (datas.Count == 0)
            {
                GUI.Label(trackRect, "no samples", centerStyle);
                return;
            }

            float scale = timelineRect.width / FrameSize;
            float offsetX = 0f;
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                float width = Mathf.Max(1f, data.Count * scale);
                Color color = dimmed ? DimAccentColor : GetStateColor(data.State);
                Rect segmentRect = new Rect(trackRect.x + offsetX, trackRect.y, Mathf.Min(width, trackRect.width - offsetX), trackRect.height);
                if (segmentRect.width <= 0f)
                    break;

                EditorGUI.DrawRect(segmentRect, color);
                EditorGUI.DrawRect(new Rect(segmentRect.x, segmentRect.y, segmentRect.width, 1f), new Color(1f, 1f, 1f, 0.18f));
                offsetX += width;
            }

            float currentX = Mathf.Min(trackRect.x + offsetX, trackRect.xMax - 1f);
            EditorGUI.DrawRect(new Rect(currentX, trackRect.y - 3f, 2f, trackRect.height + 6f), new Color(1f, 1f, 1f, 0.72f));
        }

        private void DrawTimelineGrid(Rect rect)
        {
            float scale = rect.width / FrameSize;
            for (int frame = GridStep; frame < FrameSize; frame += GridStep)
            {
                float x = rect.x + frame * scale;
                Color color = frame % 100 == 0 ? new Color(1f, 1f, 1f, 0.12f) : GridColor;
                EditorGUI.DrawRect(new Rect(x, rect.y, 1f, rect.height), color);
            }
        }

        private void DrawEmptyRow(string text)
        {
            Rect rect = GUILayoutUtility.GetRect(LabelWidth + 260f, RowHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, EditorGUIUtility.isProSkin ? 0.025f : 0.16f));
            GUI.Label(rect, text, centerStyle);
        }

        private void DrawCenteredMessage(string text)
        {
            GUILayout.FlexibleSpace();
            Rect rect = GUILayoutUtility.GetRect(0f, 36f, GUILayout.ExpandWidth(true));
            GUI.Label(rect, text, centerStyle);
            GUILayout.FlexibleSpace();
        }

        private void CollectCapabilityIndices(List<CapabilityBase> capabilities, List<int> focused, List<int> normal)
        {
            focused.Clear();
            normal.Clear();
            for (int i = 0; i < capabilities.Count; i++)
            {
                if (IsCapabilityFocused(capabilities[i]))
                    focused.Add(i);
                else
                    normal.Add(i);
            }
        }

        private int CountFocusedRows()
        {
            return CountFocusedRows(capabilityBaseUpdateMode) + CountFocusedRows(capabilityBaseFixUpdateMode);
        }

        private int CountFocusedRows(List<CapabilityBase> capabilities)
        {
            int count = 0;
            for (int i = 0; i < capabilities.Count; i++)
            {
                if (IsCapabilityFocused(capabilities[i]))
                    count++;
            }

            return count;
        }

        private void ClearFocusedCapabilities()
        {
            ClearFocusedCapabilities(capabilityBaseUpdateMode);
            ClearFocusedCapabilities(capabilityBaseFixUpdateMode);
            isShowFocusedOnly = false;
        }

        private void ClearFocusedCapabilities(List<CapabilityBase> capabilities)
        {
            for (int i = 0; i < capabilities.Count; i++)
            {
                SetCapabilityFocused(capabilities[i], false);
            }
        }

        private static bool IsCapabilityFocused(CapabilityBase capability)
        {
            return capability != null && EditorPrefs.GetBool(GetCapabilityFocusKey(capability), false);
        }

        private static void SetCapabilityFocused(CapabilityBase capability, bool focused)
        {
            if (capability == null)
                return;

            EditorPrefs.SetBool(GetCapabilityFocusKey(capability), focused);
        }

        private static string GetCapabilityFocusKey(CapabilityBase capability)
        {
            var type = capability.GetType();
            return $"{FocusedCapabilityEditorPrefsPrefix}{type.FullName ?? type.Name}";
        }

        private Rect GetTimelineRect(Rect rowRect)
        {
            float x = rowRect.x + LabelWidth;
            float width = Mathf.Max(260f, rowRect.width - LabelWidth - ContentPadding);
            return new Rect(x, rowRect.y, width, rowRect.height);
        }

        private Color GetStateColor(int state)
        {
            if (state >= 0 && state < StateColor.Length)
                return StateColor[state];
            return UnknownColor;
        }

        private void EnsureStyles()
        {
            if (headerTitleStyle != null)
                return;

            headerTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft
            };
            headerTitleStyle.normal.textColor = PrimaryText;

            headerSubTitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };
            headerSubTitleStyle.normal.textColor = SecondaryText;

            sectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };
            sectionStyle.normal.textColor = PrimaryText;

            labelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };
            labelStyle.normal.textColor = PrimaryText;

            mutedLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };
            mutedLabelStyle.normal.textColor = SecondaryText;

            dimmedLabelStyle = new GUIStyle(labelStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };
            dimmedLabelStyle.normal.textColor = DimmedText;

            focusButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 10
            };
            focusButtonStyle.normal.textColor = PrimaryText;

            pillStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            pillStyle.normal.textColor = PrimaryText;

            centerStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
            centerStyle.normal.textColor = SecondaryText;
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                Repaint();
        }

        public static void Destroy()
        {
            sWindow?.Close();
        }

        protected override void OnDestroy()
        {
            sWindow = null;
        }
    }
}
