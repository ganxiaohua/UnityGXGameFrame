using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GXGame;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using OdinEditorWindow = Sirenix.OdinInspector.Editor.OdinEditorWindow;
using PropertyTree = Sirenix.OdinInspector.Editor.PropertyTree;

namespace GameFrame.Editor
{
    [Serializable]
    public struct ComponentInfo : ISearchFilterable
    {
        [ShowInInspector] [HorizontalGroup("Component", 0.4f)] [LabelText("")]
        private string componentName;

        [HideInInspector] private Type ComponentType;

        [HideInInspector] private Action<Type> func;

        public void Init(Type type, Action<Type> func)
        {
            ComponentType = type;
            componentName = type.Name;
            this.func = func;
        }

        [Button]
        [HorizontalGroup("Component", 0.2f)]
        public void Add()
        {
            func?.Invoke(ComponentType);
        }

        public bool IsMatch(string searchString)
        {
            return ComponentType.Name.ToLower().Contains(searchString.ToLower());
        }
    }

    public class ComponentView : OdinEditorWindow
    {
        private Dictionary<int, PropertyTree> m_EcsComponentsTree = new Dictionary<int, PropertyTree>();

        private static ComponentView sWindow;

        private ECSEntity ecsEntity;

        private List<int> waitRemoveList = new List<int>();

        private bool isShowAllEcsComponents = false;

        [ShowInInspector] [ShowIf("isShowAllEcsComponents")] [Searchable] [ListDrawerSettings(IsReadOnly = true)]
        private static List<ComponentInfo> allEcsComponents = new List<ComponentInfo>();

        public static void Init(ECSEntity ecsEntity)
        {
            sWindow ??= GetWindow<ComponentView>();
            sWindow.titleContent.text = string.IsNullOrEmpty(ecsEntity.Name) ? "Entity" : ecsEntity.Name;
            sWindow.ecsEntity = ecsEntity;
            sWindow.m_EcsComponentsTree.Clear();
            sWindow.isShowAllEcsComponents = false;

            Action<Type> action = sWindow.AddComponent;
            Type baseType = typeof(ECSComponent);

            if (allEcsComponents.Count == 0)
            {
                List<Type> derivedTypes = typeof(Main).Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)).ToList();
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

            List<int> comIndexs = ecsEntity.ECSComponentArray.Indexs;
            waitRemoveList.Clear();
            foreach (var key in m_EcsComponentsTree.Keys)
            {
                if (!comIndexs.Contains(key))
                {
                    waitRemoveList.Add(key);
                }
            }

            foreach (var key in waitRemoveList)
            {
                m_EcsComponentsTree[key].Dispose();
                m_EcsComponentsTree.Remove(key);
            }

            for (int i = comIndexs.Count - 1; i >= 0; i--)
            {
                int cid = comIndexs[i];
                string comID = $"Com_{cid}";
                bool t = EditorPrefs.GetBool(comID, false);
                ECSComponent ecsComponent = ecsEntity.GetComponent(cid);
                Rect lineRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
                EditorGUI.DrawRect(lineRect, new Color(1, 1, 1, 0.1f));
                EditorGUILayout.BeginHorizontal();
                t = EditorGUILayout.Foldout(t, ecsComponent.GetType().Name, true);
                EditorPrefs.SetBool(comID, t);
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    RemoveComponent(cid);
                }

                EditorGUILayout.EndHorizontal();
                if (t)
                {
                    if (!m_EcsComponentsTree.TryGetValue(cid, out var tree))
                    {
                        tree = PropertyTree.Create(ecsComponent);
                        m_EcsComponentsTree.Add(cid, tree);
                    }

                    tree.Draw(false);
                }

                EditorGUILayout.Space(5);
            }

            if (GUILayout.Button("Add  Component"))
            {
                isShowAllEcsComponents = true;
            }
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();
            Repaint();
        }

        protected override void OnDestroy()
        {
            foreach (var t in m_EcsComponentsTree.Values)
            {
                t.Dispose();
            }

            sWindow = null;
            waitRemoveList.Clear();
        }

        private void RemoveComponent(int index)
        {
            ecsEntity.RemoveComponent(index);
        }

        private void AddComponent(Type type)
        {
            if (!isShowAllEcsComponents) return;
            Type comType = typeof(Main).Assembly.GetType($"Auto{type.Name}");
            MethodInfo methodInfo = comType.GetMethod($"Add{type.Name}", BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof(ECSEntity)}, null);
            methodInfo.Invoke(null, new object[] {ecsEntity});
            isShowAllEcsComponents = false;
        }
    }
}