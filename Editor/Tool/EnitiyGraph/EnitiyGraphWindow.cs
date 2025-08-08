using GameFrame.Runtime;
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
            else if (EditorApplication.isPlaying && graphView == null && SceneFactory.GetPlayerScene() != null)
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
            Button serchComp = new Button(() => { graphView.FindNodeComp(searchField.text); });
            serchComp.text = "搜索成员组件";
            // var scene = SceneFactory.GetPlayerScene();
            var serchcpb = new Button(() => { graphView.FindNodecapability(searchField.text); });
            serchcpb.text = "搜索成员能力(如果是ECC模式)";
            refreshBtn.text = "刷新";
            toolbar.Add(refreshBtn);
            toolbar.Add(searchField);
            toolbar.Add(serchComp);
            toolbar.Add(serchcpb);
            rootVisualElement.Add(toolbar);
        }
    }
}