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

        public bool IsMatch(string searchString)
        {
            return componentType.Name.ToLower().Contains(searchString.ToLower());
        }
    }

    public class ComponentView : OdinEditorWindow
    {
        private static ComponentView sWindow;

        [ShowInInspector] [ShowIf("isShowAllEcsComponents")] [Searchable]
        private static List<ComponentInfo> sAllEcsComponents = new();

        private EffEntity effEntity;

        private bool isShowAllEcsComponents;
        private Dictionary<int, PropertyTree> ecsComponentsTree = new();

        private List<int> waitRemoveList = new();


        public static void Init(EffEntity effEntity)
        {
            sWindow ??= GetWindow<ComponentView>();
            sWindow.titleContent.text = string.IsNullOrEmpty(effEntity.Name) ? "Entity" : effEntity.Name;
            sWindow.effEntity = effEntity;
            sWindow.ecsComponentsTree.Clear();
            sWindow.isShowAllEcsComponents = false;
            if (ComponentsID2Type.Count == 0)
                return;
            Action<Type> action = sWindow.AddComponent;
            var baseType = typeof(EffComponent);
            if (sAllEcsComponents.Count == 0)
            {
                var derivedTypes = Enumerable.ToList(Enumerable.Where(GamePlayAssembly.GetAssembly().GetTypes(), type => type.IsSubclassOf(baseType)));
                foreach (var item in derivedTypes)
                {
                    var componet = new ComponentInfo();
                    componet.Init(item, action);
                    sAllEcsComponents.Add(componet);
                }

                derivedTypes = Enumerable.ToList(Enumerable.Where(typeof(GXGameFrame).Assembly.GetTypes(), type => type.IsSubclassOf(baseType)));
                foreach (var item in derivedTypes)
                {
                    var componet = new ComponentInfo();
                    componet.Init(item, action);
                    sAllEcsComponents.Add(componet);
                }
            }
        }


        protected override unsafe void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            if (effEntity.State == IEntity.EntityState.IsClear)
            {
                Close();
                return;
            }

            for (var i = effEntity.world.MaxComponentCount - 1; i >= 0; i--)
            {
                if (ComponentsID2Type.ComponentsTypes[i] == typeof(CapabilityComponent) || !effEntity.HasComponent(i))
                    continue;
                var cid = i;
                var comID = $"Com_{cid}";
                var t = EditorPrefs.GetBool(comID, false);
                byte* dataPtr = effEntity.world.GetCompBytes(effEntity.ID, i);
                object ecsComponent = Marshal.PtrToStructure(new IntPtr(dataPtr), ComponentsID2Type.ComponentsTypes[i]);
                object ecsComponentExternal = InvokeGetDataIfExists(ecsComponent);
                var lineRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
                EditorGUI.DrawRect(lineRect, new Color(1, 1, 1, 0.1f));
                EditorGUILayout.BeginHorizontal();
                t = EditorGUILayout.Foldout(t, ecsComponent.GetType().Name, true);
                EditorPrefs.SetBool(comID, t);
                if (GUILayout.Button("删除", GUILayout.Width(60))) RemoveComponent(cid);
                EditorGUILayout.EndHorizontal();
                if (t)
                {
                    if (!ecsComponentsTree.TryGetValue(cid, out var tree))
                    {
                        tree = PropertyTree.Create(ecsComponentExternal ?? ecsComponent);
                        tree.OnPropertyValueChanged += ChangeComponent;
                        ecsComponentsTree.Add(cid, tree);
                    }

                    tree.Draw(false);
                }

                EditorGUILayout.Space(5);
            }

            if (GUILayout.Button("Add  Component")) isShowAllEcsComponents = true;
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();
            Repaint();
        }

        private void RemoveComponent(int index)
        {
            effEntity.RemoveComponent(index);
        }

        private void ChangeComponent(InspectorProperty property, int selectionIndex)
        {
            var type = property.Tree.TargetType;
            var fields = property.Tree.WeakTargets[0].GetType().GetFields();
            var fieldValue = fields[0].GetValue(property.Tree.WeakTargets[0]);

            var comType = GamePlayAssembly.GetAssembly().GetType($"Auto{type.Name}");
            var methodInfo = comType.GetMethod($"Set{type.Name}", BindingFlags.Static | BindingFlags.Public);
            methodInfo.Invoke(null, new[] {effEntity, fieldValue});
        }

        private void AddComponent(Type type)
        {
            if (!isShowAllEcsComponents) return;
            var comType = GamePlayAssembly.GetAssembly().GetType($"Auto{type.Name}");
            var methodInfo = comType.GetMethod($"Add{type.Name}", BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(EffEntity)}, null);
            methodInfo.Invoke(null, new object[] {effEntity});
            isShowAllEcsComponents = false;
        }

        public static void Destroy()
        {
            sWindow?.Close();
        }

        protected override void OnDestroy()
        {
            foreach (var t in ecsComponentsTree.Values) t.Dispose();

            sWindow = null;
            waitRemoveList.Clear();
        }

        public static object InvokeGetDataIfExists(object target, params object[] parameters)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Type type = target.GetType();

            MethodInfo method = type.GetMethod("GetData", BindingFlags.Public | BindingFlags.Instance);

            if (method != null)
            {
                try
                {
                    // 调用方法
                    object result = method.Invoke(target, parameters);
                    return result;
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException ?? ex;
                }
            }
            else
            {
                return null;
            }
        }
    }
}