using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class DialogueGraphWindow : EditorWindow
    {
        private DialogueGraphView _graphView;

        // 通过Menu即可打开对应window, 注意这种函数必须是static函数
        [MenuItem("Tool/打开实体审查")]
        public static void OpenDialogueGraphWindow()
        {
            // 定义了创建并打开Window的方法
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("打开实体审查");
        }

        private void OnEnable()
        {
            _graphView = new DialogueGraphView
            {
                name = "实体检查器"
            };

            // 让graphView铺满整个Editor窗口
            _graphView.StretchToParentSize();
            // 把它添加到EditorWindow的可视化Root元素下面
            rootVisualElement.Add(_graphView);
        }

        // 关闭窗口时销毁graphView
        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
    
}