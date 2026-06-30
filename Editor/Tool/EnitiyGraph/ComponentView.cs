using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using GameFrame.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Enumerable = System.Linq.Enumerable;
using OdinEditorWindow = Sirenix.OdinInspector.Editor.OdinEditorWindow;
using PropertyTree = Sirenix.OdinInspector.Editor.PropertyTree;

namespace GameFrame.Editor
{
    [Serializable]
    public struct ComponentInfo : ISearchFilterable
    {
        [ShowInInspector] [HorizontalGroup("Component", 0.4f)] [LabelText("")] [ReadOnly]
        private string componentName;

        [HideInInspector]
        private Type componentType;

        [HideInInspector]
        private Action<Type> func;

        public void Init(Type type, Action<Type> func)
        {
            componentType = type;
            componentName = type.Name;
            this.func = func;
        }

        [Button]
        [HorizontalGroup("Component", 0.2f)]
        public void Add()
        {
            func?.Invoke(componentType);
        }

        public string Name => componentName;

        public string Namespace => componentType?.Namespace ?? "Global Namespace";

        public bool IsMatch(string searchString)
        {
            return componentType.Name.ToLower().Contains(searchString.ToLower());
        }
    }

    public class ComponentView : OdinEditorWindow
    {
        private const float HeaderHeight = 76f;
        private const float ToolbarHeight = 36f;
        private const float ComponentHeaderHeight = 34f;
        private const float ContentPadding = 12f;
        private const string FocusedComponentEditorPrefsPrefix = "GameFrame.Editor.ComponentView.Focused.";

        private static readonly Color AccentColor = new Color(0.00f, 0.74f, 0.92f, 1f);
        private static readonly Color DangerColor = new Color(1.00f, 0.35f, 0.28f, 1f);
        private static readonly Color CapabilityColor = new Color(1.00f, 0.72f, 0.22f, 1f);
        private static readonly Color DimAccentColor = new Color(0.38f, 0.41f, 0.46f, 1f);
        private static readonly Color DarkBackground = new Color(0.105f, 0.115f, 0.13f, 1f);
        private static readonly Color LightBackground = new Color(0.78f, 0.80f, 0.84f, 1f);
        private static readonly Color DarkPanel = new Color(0.145f, 0.16f, 0.185f, 1f);
        private static readonly Color LightPanel = new Color(0.90f, 0.91f, 0.93f, 1f);
        private static readonly Color DarkCard = new Color(0.12f, 0.135f, 0.16f, 1f);
        private static readonly Color LightCard = new Color(0.94f, 0.95f, 0.97f, 1f);
        private static readonly Color DarkFocusedCard = new Color(0.12f, 0.20f, 0.24f, 1f);
        private static readonly Color LightFocusedCard = new Color(0.82f, 0.94f, 0.98f, 1f);
        private static readonly Color DarkDimmedCard = new Color(0.09f, 0.10f, 0.115f, 1f);
        private static readonly Color LightDimmedCard = new Color(0.78f, 0.79f, 0.81f, 1f);

        private static ComponentView sWindow;

        private static List<ComponentInfo> sAllEcsComponents = new();

        private EffEntity effEntity;
        private bool isShowAllEcsComponents;
        private bool isShowFocusedOnly;
        private readonly Dictionary<int, PropertyTree> ecsComponentsTree = new();
        private readonly List<int> waitRemoveList = new();
        private Vector2 scrollPosition;
        private Vector2 addScrollPosition;
        private string componentSearch = string.Empty;

        private GUIStyle headerTitleStyle;
        private GUIStyle headerSubTitleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle componentTitleStyle;
        private GUIStyle mutedLabelStyle;
        private GUIStyle dimmedTitleStyle;
        private GUIStyle dimmedMutedLabelStyle;
        private GUIStyle pillStyle;
        private GUIStyle centeredStyle;
        private GUIStyle removeButtonStyle;
        private GUIStyle focusButtonStyle;

        private static Color BackgroundColor => EditorGUIUtility.isProSkin ? DarkBackground : LightBackground;
        private static Color PanelColor => EditorGUIUtility.isProSkin ? DarkPanel : LightPanel;
        private static Color CardColor => EditorGUIUtility.isProSkin ? DarkCard : LightCard;
        private static Color FocusedCardColor => EditorGUIUtility.isProSkin ? DarkFocusedCard : LightFocusedCard;
        private static Color DimmedCardColor => EditorGUIUtility.isProSkin ? DarkDimmedCard : LightDimmedCard;
        private static Color PrimaryText => EditorGUIUtility.isProSkin ? new Color(0.91f, 0.93f, 0.96f, 1f) : new Color(0.12f, 0.13f, 0.15f, 1f);
        private static Color SecondaryText => EditorGUIUtility.isProSkin ? new Color(0.62f, 0.67f, 0.72f, 1f) : new Color(0.34f, 0.36f, 0.40f, 1f);
        private static Color DimmedText => EditorGUIUtility.isProSkin ? new Color(0.42f, 0.45f, 0.50f, 1f) : new Color(0.50f, 0.52f, 0.56f, 1f);

        public static void Init(EffEntity effEntity)
        {
            sWindow ??= GetWindow<ComponentView>();
            sWindow.titleContent.text = string.IsNullOrEmpty(effEntity.Name) ? "Entity" : effEntity.Name;
            sWindow.effEntity = effEntity;
            sWindow.scrollPosition = Vector2.zero;
            sWindow.addScrollPosition = Vector2.zero;
            sWindow.componentSearch = string.Empty;
            sWindow.isShowAllEcsComponents = false;
            sWindow.isShowFocusedOnly = false;
            sWindow.ClearPropertyTrees();
            sWindow.BuildComponentOptions();
        }

        protected override unsafe void OnBeginDrawEditors()
        {
            DrawWindowContent();
        }

        private unsafe void DrawWindowContent()
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
                Close();
                return;
            }

            SyncPropertyTreesWithComponents();

            int componentCount = CountVisibleComponents();
            int focusedComponentCount = CountFocusedComponents();
            int displayComponentCount = isShowFocusedOnly ? focusedComponentCount : componentCount;
            DrawHeader(componentCount, focusedComponentCount);
            GUILayout.Space(8f);
            DrawToolbar(componentCount, focusedComponentCount);
            GUILayout.Space(6f);

            if (isShowAllEcsComponents)
            {
                DrawAddComponentPanel();
                GUILayout.Space(6f);
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
            if (displayComponentCount == 0)
            {
                DrawEmptyState(componentCount > 0 && isShowFocusedOnly);
            }
            else
            {
                DrawComponents(focusedComponentCount);
            }

            GUILayout.Space(8f);
            EditorGUILayout.EndScrollView();
        }

        private unsafe void DrawComponents(int focusedComponentCount)
        {
            var focused = new List<int>();
            var normal = new List<int>();
            CollectVisibleComponentIds(focused, normal);

            DrawComponentGroup(focused, focusedComponentCount);
            if (!isShowFocusedOnly)
                DrawComponentGroup(normal, focusedComponentCount);
        }

        private unsafe void DrawComponentGroup(List<int> componentIds, int focusedComponentCount)
        {
            for (int i = 0; i < componentIds.Count; i++)
            {
                int cid = componentIds[i];
                if (!TryGetVisibleComponentType(cid, out Type componentType))
                    continue;

                byte* dataPtr = effEntity.world.GetCompBytes(effEntity.ID, cid);
                object ecsComponent = Marshal.PtrToStructure(new IntPtr(dataPtr), componentType);
                object ecsComponentExternal = InvokeGetDataIfExists(ecsComponent);
                bool focused = IsComponentFocused(componentType);
                DrawComponentCard(cid, componentType, ecsComponentExternal ?? ecsComponent, focused, focusedComponentCount > 0 && !focused);
                GUILayout.Space(5f);
            }
        }

        private void DrawHeader(int componentCount, int focusedComponentCount)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, HeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 2f), AccentColor);

            string entityName = string.IsNullOrEmpty(effEntity.Name) ? "Entity" : effEntity.Name;
            GUI.Label(new Rect(rect.x + 16f, rect.y + 10f, rect.width - 170f, 24f), "Component Inspector", headerTitleStyle);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 35f, rect.width - 170f, 18f),
                $"{entityName}   ID:{effEntity.ID}   State:{effEntity.State}", headerSubTitleStyle);

            DrawPill(new Rect(rect.xMax - 132f, rect.y + 14f, 110f, 24f), $"{componentCount} Components", AccentColor);
            DrawPill(new Rect(rect.x + 16f, rect.y + 55f, 128f, 18f), "Capability hidden", CapabilityColor);
            DrawPill(new Rect(rect.x + 152f, rect.y + 55f, 106f, 18f), $"{focusedComponentCount} Focused", focusedComponentCount > 0 ? CapabilityColor : DimAccentColor);
        }

        private void DrawToolbar(int componentCount, int focusedComponentCount)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, ToolbarHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(1f, 1f, 1f, 0.08f));

            GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, Mathf.Max(180f, rect.width - 440f), 20f), $"Runtime ECS components: {componentCount}   Focused: {focusedComponentCount}", sectionStyle);

            Rect clearFocusRect = new Rect(rect.xMax - 410f, rect.y + 5f, 126f, 25f);
            EditorGUI.BeginDisabledGroup(focusedComponentCount == 0);
            if (GUI.Button(clearFocusRect, "Clear Focus"))
            {
                ClearFocusedComponents();
                GUIUtility.ExitGUI();
            }

            EditorGUI.EndDisabledGroup();

            Rect focusRect = new Rect(rect.xMax - 276f, rect.y + 5f, 126f, 25f);
            if (GUI.Button(focusRect, isShowFocusedOnly ? "Show All" : "Only Focused"))
            {
                isShowFocusedOnly = !isShowFocusedOnly;
            }

            Rect addRect = new Rect(rect.xMax - 142f, rect.y + 5f, 128f, 25f);
            if (GUI.Button(addRect, isShowAllEcsComponents ? "Hide Add Panel" : "Add Component"))
            {
                isShowAllEcsComponents = !isShowAllEcsComponents;
            }
        }

        private void DrawComponentCard(int cid, Type componentType, object target, bool focused, bool dimmed)
        {
            string key = $"Com_{cid}";
            bool expanded = EditorPrefs.GetBool(key, false);
            Color accent = focused ? CapabilityColor : dimmed ? DimAccentColor : AccentColor;
            Color cardColor = focused ? FocusedCardColor : dimmed ? DimmedCardColor : CardColor;
            GUIStyle titleStyle = dimmed ? dimmedTitleStyle : componentTitleStyle;
            GUIStyle namespaceStyle = dimmed ? dimmedMutedLabelStyle : mutedLabelStyle;

            Rect headerRect = GUILayoutUtility.GetRect(0f, ComponentHeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, cardColor);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, 4f, headerRect.height), accent);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1f, headerRect.width, 1f), new Color(1f, 1f, 1f, 0.07f));

            Rect foldoutRect = new Rect(headerRect.x + 12f, headerRect.y + 7f, 22f, 20f);
            expanded = EditorGUI.Foldout(foldoutRect, expanded, GUIContent.none, true);

            float titleWidth = Mathf.Max(80f, headerRect.width - 288f);
            Rect titleRect = new Rect(headerRect.x + 34f, headerRect.y + 4f, titleWidth, 18f);
            GUI.Label(titleRect, componentType.Name, titleStyle);

            Rect metaRect = new Rect(headerRect.x + 34f, headerRect.y + 19f, titleWidth, 13f);
            GUI.Label(metaRect, componentType.Namespace ?? "Global Namespace", namespaceStyle);

            Rect focusRect = new Rect(headerRect.xMax - 222f, headerRect.y + 6f, 68f, 22f);
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = focused ? new Color(CapabilityColor.r, CapabilityColor.g, CapabilityColor.b, 0.9f) : previousBackground;
            if (GUI.Button(focusRect, focused ? "Focused" : "Focus", focusButtonStyle))
            {
                SetComponentFocused(componentType, !focused);
                GUIUtility.ExitGUI();
            }

            GUI.backgroundColor = previousBackground;

            DrawPill(new Rect(headerRect.xMax - 148f, headerRect.y + 7f, 52f, 20f), $"CID {cid}", accent);

            Rect removeRect = new Rect(headerRect.xMax - 88f, headerRect.y + 6f, 72f, 22f);
            if (GUI.Button(removeRect, "Remove", removeButtonStyle))
            {
                RemoveComponent(cid);
                GUIUtility.ExitGUI();
            }

            EditorPrefs.SetBool(key, expanded);

            if (!expanded)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(4f);
                var tree = GetOrCreateComponentTree(cid, target);

                tree.UpdateTree();
                tree.Draw(false);
                GUILayout.Space(4f);
            }
        }

        private PropertyTree GetOrCreateComponentTree(int cid, object target)
        {
            if (ecsComponentsTree.TryGetValue(cid, out var tree))
            {
                if (IsTreeTargetCurrent(tree, target))
                    return tree;

                DisposeComponentTree(cid, tree);
            }

            tree = PropertyTree.Create(target);
            tree.OnPropertyValueChanged += ChangeComponent;
            ecsComponentsTree.Add(cid, tree);
            return tree;
        }

        private static bool IsTreeTargetCurrent(PropertyTree tree, object target)
        {
            if (tree == null || target == null || tree.TargetType != target.GetType() || tree.WeakTargets.Count == 0)
                return false;

            object currentTarget = tree.WeakTargets[0];
            return ReferenceEquals(currentTarget, target) || Equals(currentTarget, target);
        }

        private void SyncPropertyTreesWithComponents()
        {
            waitRemoveList.Clear();
            foreach (var cid in ecsComponentsTree.Keys)
            {
                if (!TryGetVisibleComponentType(cid, out _))
                    waitRemoveList.Add(cid);
            }

            foreach (int cid in waitRemoveList)
            {
                DisposeComponentTree(cid, ecsComponentsTree[cid]);
            }

            waitRemoveList.Clear();
        }

        private void DisposeComponentTree(int cid, PropertyTree tree)
        {
            tree.OnPropertyValueChanged -= ChangeComponent;
            tree.Dispose();
            ecsComponentsTree.Remove(cid);
        }

        private void DrawPill(Rect rect, string text, Color color)
        {
            EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, 0.20f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), color);
            GUI.Label(rect, text, pillStyle);
        }

        private void DrawEmptyState(bool focusedFilterActive)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 64f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, CardColor);
            GUI.Label(rect, focusedFilterActive ? "No focused components on this entity." : "No visible components on this entity.", centeredStyle);
        }

        private void DrawAddComponentPanel()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 34f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.14f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4f, rect.height), AccentColor);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, 150f, 18f), "Add Component", sectionStyle);

            Rect searchRect = new Rect(rect.x + 160f, rect.y + 6f, Mathf.Max(120f, rect.width - 300f), 22f);
            componentSearch = EditorGUI.TextField(searchRect, componentSearch);

            Rect closeRect = new Rect(rect.xMax - 92f, rect.y + 6f, 78f, 22f);
            if (GUI.Button(closeRect, "Close"))
            {
                isShowAllEcsComponents = false;
                GUIUtility.ExitGUI();
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                addScrollPosition = EditorGUILayout.BeginScrollView(addScrollPosition, GUILayout.MaxHeight(220f));
                int shownCount = 0;
                for (int i = 0; i < sAllEcsComponents.Count; i++)
                {
                    var info = sAllEcsComponents[i];
                    if (!string.IsNullOrEmpty(componentSearch) && !info.IsMatch(componentSearch))
                        continue;

                    DrawAddComponentRow(info);
                    shownCount++;
                }

                if (shownCount == 0)
                {
                    Rect emptyRect = GUILayoutUtility.GetRect(0f, 34f, GUILayout.ExpandWidth(true));
                    GUI.Label(emptyRect, "No matching components.", centeredStyle);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawAddComponentRow(ComponentInfo info)
        {
            Rect rowRect = GUILayoutUtility.GetRect(0f, 32f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rowRect, CardColor);
            EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.yMax - 1f, rowRect.width, 1f), new Color(1f, 1f, 1f, 0.06f));

            GUI.Label(new Rect(rowRect.x + 10f, rowRect.y + 3f, rowRect.width - 112f, 16f), info.Name, componentTitleStyle);
            GUI.Label(new Rect(rowRect.x + 10f, rowRect.y + 17f, rowRect.width - 112f, 12f), info.Namespace, mutedLabelStyle);

            Rect addRect = new Rect(rowRect.xMax - 82f, rowRect.y + 5f, 68f, 22f);
            if (GUI.Button(addRect, "Add"))
            {
                info.Add();
                GUIUtility.ExitGUI();
            }
        }

        private void DrawCenteredMessage(string text)
        {
            GUILayout.FlexibleSpace();
            Rect rect = GUILayoutUtility.GetRect(0f, 36f, GUILayout.ExpandWidth(true));
            GUI.Label(rect, text, centeredStyle);
            GUILayout.FlexibleSpace();
        }

        private void CollectVisibleComponentIds(List<int> focused, List<int> normal)
        {
            focused.Clear();
            normal.Clear();
            for (var i = effEntity.world.MaxComponentCount - 1; i >= 0; i--)
            {
                if (!TryGetVisibleComponentType(i, out Type componentType))
                    continue;

                if (IsComponentFocused(componentType))
                    focused.Add(i);
                else
                    normal.Add(i);
            }
        }

        private bool TryGetVisibleComponentType(int cid, out Type componentType)
        {
            componentType = null;
            if (cid < 0 || cid >= ComponentsID2Type.ComponentsTypes.Count)
                return false;

            componentType = ComponentsID2Type.ComponentsTypes[cid];
            if (componentType == typeof(CapabilityComponent))
                return false;

            return effEntity.HasComponent(cid);
        }

        private int CountFocusedComponents()
        {
            int count = 0;
            for (int i = 0; i < effEntity.world.MaxComponentCount; i++)
            {
                if (TryGetVisibleComponentType(i, out Type componentType) && IsComponentFocused(componentType))
                    count++;
            }

            return count;
        }

        private void ClearFocusedComponents()
        {
            for (int i = 0; i < effEntity.world.MaxComponentCount; i++)
            {
                if (TryGetVisibleComponentType(i, out Type componentType))
                    SetComponentFocused(componentType, false);
            }

            isShowFocusedOnly = false;
        }

        private int CountVisibleComponents()
        {
            int count = 0;
            for (int i = 0; i < effEntity.world.MaxComponentCount; i++)
            {
                if (TryGetVisibleComponentType(i, out _))
                    count++;
            }

            return count;
        }

        private static bool IsComponentFocused(Type componentType)
        {
            return componentType != null && EditorPrefs.GetBool(GetComponentFocusKey(componentType), false);
        }

        private static void SetComponentFocused(Type componentType, bool focused)
        {
            if (componentType == null)
                return;

            EditorPrefs.SetBool(GetComponentFocusKey(componentType), focused);
        }

        private static string GetComponentFocusKey(Type componentType)
        {
            return $"{FocusedComponentEditorPrefsPrefix}{componentType.FullName ?? componentType.Name}";
        }

        protected override void OnImGUI()
        {
            DrawWindowContent();
            Repaint();
        }

        private void RemoveComponent(int index)
        {
            if (ecsComponentsTree.TryGetValue(index, out var tree))
                DisposeComponentTree(index, tree);

            effEntity.RemoveComponent(index);
        }

        private void ChangeComponent(InspectorProperty property, int selectionIndex)
        {
            var type = property.Tree.TargetType;
            var fields = property.Tree.WeakTargets[0].GetType().GetFields();
            if (fields.Length == 0)
                return;

            var fieldValue = fields[0].GetValue(property.Tree.WeakTargets[0]);
            var comType = GamePlayAssembly.GetAssembly().GetType($"Auto{type.Name}");
            var methodInfo = comType?.GetMethod($"Set{type.Name}", BindingFlags.Static | BindingFlags.Public);
            methodInfo?.Invoke(null, new[] {effEntity, fieldValue});
        }

        private void AddComponent(Type type)
        {
            if (!isShowAllEcsComponents)
                return;

            var comType = GamePlayAssembly.GetAssembly().GetType($"Auto{type.Name}");
            var methodInfo = comType?.GetMethod($"Add{type.Name}", BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(EffEntity)}, null);
            methodInfo?.Invoke(null, new object[] {effEntity});
            isShowAllEcsComponents = false;
        }

        private void BuildComponentOptions()
        {
            sAllEcsComponents.Clear();
            if (ComponentsID2Type.Count == 0)
                return;

            Action<Type> action = AddComponent;
            var baseType = typeof(EffComponent);
            AddComponentOptions(GamePlayAssembly.GetAssembly().GetTypes(), baseType, action);
            AddComponentOptions(typeof(GXGameFrame).Assembly.GetTypes(), baseType, action);
        }

        private static void AddComponentOptions(Type[] types, Type baseType, Action<Type> action)
        {
            var derivedTypes = Enumerable.ToList(Enumerable.Where(types, type =>
                type.IsValueType &&
                !type.IsAbstract &&
                baseType.IsAssignableFrom(type)));

            foreach (var item in derivedTypes)
            {
                var component = new ComponentInfo();
                component.Init(item, action);
                sAllEcsComponents.Add(component);
            }
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

            componentTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };
            componentTitleStyle.normal.textColor = PrimaryText;

            mutedLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };
            mutedLabelStyle.normal.textColor = SecondaryText;

            dimmedTitleStyle = new GUIStyle(componentTitleStyle);
            dimmedTitleStyle.normal.textColor = DimmedText;

            dimmedMutedLabelStyle = new GUIStyle(mutedLabelStyle);
            dimmedMutedLabelStyle.normal.textColor = DimmedText;

            pillStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            pillStyle.normal.textColor = PrimaryText;

            centeredStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
            centeredStyle.normal.textColor = SecondaryText;

            removeButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold
            };
            removeButtonStyle.normal.textColor = DangerColor;
            removeButtonStyle.hover.textColor = Color.white;

            focusButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 10
            };
            focusButtonStyle.normal.textColor = PrimaryText;
        }

        public static void Destroy()
        {
            sWindow?.Close();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected override void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            ClearPropertyTrees();
            effEntity = null;
            sWindow = null;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                Close();
        }

        private void ClearPropertyTrees()
        {
            foreach (var tree in ecsComponentsTree.Values)
            {
                tree.OnPropertyValueChanged -= ChangeComponent;
                tree.Dispose();
            }

            ecsComponentsTree.Clear();
            waitRemoveList.Clear();
        }

        public static object InvokeGetDataIfExists(object target, params object[] parameters)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Type type = target.GetType();
            MethodInfo method = type.GetMethod("GetData", BindingFlags.Public | BindingFlags.Instance);
            return method?.Invoke(target, parameters);
        }
    }
}
