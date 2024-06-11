using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class ComponentView : OdinEditorWindow
    {
        private Dictionary<int, PropertyTree> m_EcsComponentsTree = new Dictionary<int, PropertyTree>();

        private static ComponentView sWindow;

        private ECSEntity ecsEntity;

        private List<int> waitRemoveList = new List<int>();

        public static void Init(ECSEntity ecsEntity)
        {
            if (sWindow != null)
                sWindow.Close();
            sWindow = GetWindow<ComponentView>();
            sWindow.titleContent.text = ecsEntity.Name;
            sWindow.ecsEntity = ecsEntity;
            sWindow.m_EcsComponentsTree.Clear();
        }

        public static void Destroy()
        {
            sWindow?.Close();
        }


        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
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

            for (int i = comIndexs.Count-1; i >=0; i--)
            {
                int index = comIndexs[i];
                ECSComponent ecsComponent = ecsEntity.GetComponent(index);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Foldout(true, ecsComponent.GetType().Name, true);
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    DestroyComponent(index);
                }
                
                EditorGUILayout.EndHorizontal();
                if (!m_EcsComponentsTree.TryGetValue(index, out var tree))
                {
                    tree = PropertyTree.Create(ecsComponent);
                    m_EcsComponentsTree.Add(index, tree);
                }
                tree.Draw(false);
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

        public void DestroyComponent(int index)
        {
            ecsEntity.RemoveComponent(index);
            m_EcsComponentsTree.Remove(index);
        }
    }
}