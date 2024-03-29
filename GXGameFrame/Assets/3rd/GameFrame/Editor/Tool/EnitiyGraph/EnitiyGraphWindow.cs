using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class DialogueGraphWindow : EditorWindow
    {
        private EnitiyGraphView GraphView;
        
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("实体审查");
        }

        private void OnEnable()
        {
            GraphView = new EnitiyGraphView();
            GraphView.Init(this);
            GraphView.Show();
            CreateTools();
        }
        
        private void OnDisable()
        {
            GraphView.Clear();
        }

        private void CreateTools()
        {
            Toolbar toolbar = new Toolbar();
            Button a = new Button(() => {GraphView.FollowNode(null); });
            a.text = "刷新";
            toolbar.Add(a);
            rootVisualElement.Add(toolbar);
        }
    }
    
}