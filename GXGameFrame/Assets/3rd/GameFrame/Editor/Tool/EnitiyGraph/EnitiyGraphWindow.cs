using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class DialogueGraphWindow : EditorWindow
    {
        private EntityGraphView GraphView;

        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("实体审查");
        }
        
        protected void OnGUI()
        {
            if (!EditorApplication.isPlaying && GraphView != null)
            {
                GraphView.Clear();
                rootVisualElement.Clear();
                GraphView = null;
                return;
            }
            else if (EditorApplication.isPlaying && GraphView == null)
            {
                GraphView = new EntityGraphView();
                GraphView.Init(this);
                GraphView.Show();
                CreateTools();
            }
            
            GraphView?.Update();
        }

        private void OnDisable()
        {
            GraphView?.Clear();
            GraphView = null;
        }

        private void CreateTools()
        {
            Toolbar toolbar = new Toolbar();
            Button a = new Button(() => { GraphView.FollowNode(null); });
            a.text = "刷新";
            toolbar.Add(a);
            rootVisualElement.Add(toolbar);
        }
    }
}