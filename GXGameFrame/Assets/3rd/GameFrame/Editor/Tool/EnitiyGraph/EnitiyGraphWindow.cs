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

        // 通过Menu即可打开对应window, 注意这种函数必须是static函数
        [MenuItem("Tool/打开实体审查")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("打开实体审查");
        }

        private void OnEnable()
        {
            GraphView = new EnitiyGraphView();
            GraphView.Init(this);
            GraphView.Show();
            CreateTools();
        }

        // 关闭窗口时销毁graphView
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