using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using OdinEditorWindow = Sirenix.OdinInspector.Editor.OdinEditorWindow;
using PropertyTree = Sirenix.OdinInspector.Editor.PropertyTree;

namespace GameFrame.Editor
{
    [Serializable]
    public struct ComponentInfo : ISearchFilterable
    {
        [ShowInInspector] [HorizontalGroup("Component", 0.4f)] [LabelText("")] [ReadOnly]
        private string componentName;

        [HideInInspector] private Type componentType;

        [HideInInspector] private Action<Type> func;

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
        private static List<ComponentInfo> allEcsComponents = new();

        private Assembly assembly;

        private ECSEntity ecsEntity;

        private bool isShowAllEcsComponents;
        private Dictionary<int, PropertyTree> ecsComponentsTree = new();

        private List<int> waitRemoveList = new();

        protected override void OnDestroy()
        {
            foreach (var t in ecsComponentsTree.Values) t.Dispose();

            sWindow = null;
            waitRemoveList.Clear();
        }

        public static void Init(ECSEntity ecsEntity)
        {
            sWindow ??= GetWindow<ComponentView>();
            sWindow.titleContent.text = string.IsNullOrEmpty(ecsEntity.Name) ? "Entity" : ecsEntity.Name;
            sWindow.ecsEntity = ecsEntity;
            sWindow.ecsComponentsTree.Clear();
            sWindow.isShowAllEcsComponents = false;
            if (GXComponents.ComponentTypes.Length == 0)
                return;
            Action<Type> action = sWindow.AddComponent;
            var baseType = typeof(ECSComponent);
            if (allEcsComponents.Count == 0)
            {
                var derivedTypes = GXComponents.ComponentTypes[0].Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)).ToList();
                foreach (var item in derivedTypes)
                {
                    var componet = new ComponentInfo();
                    componet.Init(item, action);
                    allEcsComponents.Add(componet);
                }

                derivedTypes = typeof(GXGameFrame).Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)).ToList();
                foreach (var item in derivedTypes)
                {
                    var componet = new ComponentInfo();
                    componet.Init(item, action);
                    allEcsComponents.Add(componet);
                }
            }
        }

        public static void Destroy()
        {
            sWindow?.Close();
        }


        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            if (ecsEntity.State == IEntity.EntityState.IsClear)
            {
                Close();
                return;
            }


            var comIndexs = ecsEntity.EcsComponentArray.indexList;
            waitRemoveList.Clear();
            foreach (var key in ecsComponentsTree.Keys)
                if (!comIndexs.Contains(key))
                    waitRemoveList.Add(key);

            foreach (var key in waitRemoveList)
            {
                ecsComponentsTree[key].Dispose();
                ecsComponentsTree.Remove(key);
            }

            for (var i = comIndexs.Count - 1; i >= 0; i--)
            {
                var cid = comIndexs[i];
                var comID = $"Com_{cid}";
                var t = EditorPrefs.GetBool(comID, false);
                var ecsComponent = ecsEntity.GetComponent(cid);
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
                        tree = PropertyTree.Create(ecsComponent);
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
            ecsEntity.RemoveComponent(index);
        }

        private void ChangeComponent(InspectorProperty property, int selectionIndex)
        {
            var type = property.Tree.TargetType;
            var fields = property.Tree.WeakTargets[0].GetType().GetFields();
            var fieldValue = fields[0].GetValue(property.Tree.WeakTargets[0]);
            var comType = GXComponents.ComponentTypes[0].Assembly.GetType($"Auto{type.Name}");
            var methodInfo = comType.GetMethod($"Set{type.Name}", BindingFlags.Static | BindingFlags.Public);
            methodInfo.Invoke(null, new[] {ecsEntity, fieldValue});
        }

        private void AddComponent(Type type)
        {
            if (!isShowAllEcsComponents) return;
            var comType = GXComponents.ComponentTypes[0].Assembly.GetType($"Auto{type.Name}");
            var methodInfo = comType.GetMethod($"Add{type.Name}", BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(ECSEntity)}, null);
            methodInfo.Invoke(null, new object[] {ecsEntity});
            isShowAllEcsComponents = false;
        }
    }
}