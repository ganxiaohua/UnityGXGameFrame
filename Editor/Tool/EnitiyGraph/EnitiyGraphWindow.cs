using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class DialogueGraphWindow : EditorWindow
    {
        private EntityGraphView graphView;

        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("实体审查");
        }

        protected void OnGUI()
        {
            if (!EditorApplication.isPlaying && graphView != null)
            {
                graphView.Dispose();
                rootVisualElement.Clear();
                graphView = null;
            }
            else if (EditorApplication.isPlaying && graphView == null)
            {
                graphView = new EntityGraphView();
                graphView.Init(this);
                graphView.Show();
                CreateTools();
            }

            graphView?.Update();
        }


        private void OnDisable()
        {
            graphView?.Dispose();
            graphView = null;
        }

        private void CreateTools()
        {
            Toolbar toolbar = new Toolbar();
            TextField searchField = new TextField();
            searchField.style.width = 150;
            Button refreshBtn = new Button(() => { graphView.FollowNode(null); });
            Button serchBtn = new Button(() => { graphView.FindNode(searchField.text); });
            serchBtn.text = "搜索";
            refreshBtn.text = "刷新";
            toolbar.Add(refreshBtn);
            toolbar.Add(searchField);
            toolbar.Add(serchBtn);
            rootVisualElement.Add(toolbar);
        }
    }
}