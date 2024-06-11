using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;

namespace GameFrame.Editor
{
    public class ComponentView : OdinEditorWindow
    {
        private class PeClass
        {
            public PropertyTree Tree;
            public ECSComponent EcsComponent;
        }

        private List<PeClass> m_EcsComponents = new List<PeClass>();

        private ECSEntity m_EcsEntity;

        private static ComponentView sWindow;

        public static void Init(ECSEntity ecsEntity)
        {
            if (sWindow != null)
                sWindow.Close();
            sWindow = GetWindow<ComponentView>();
            sWindow.m_EcsComponents.Clear();
            sWindow.m_EcsEntity = ecsEntity;
            sWindow.titleContent.text = ecsEntity.Name;
            
            List<int> comIndexs = ecsEntity.ECSComponentArray.Indexs;
            List<ECSComponent> ecsComponents = new List<ECSComponent>();
            foreach (var index in comIndexs)
            {
                ECSComponent ecsComponent = ecsEntity.GetComponent(index);
                ecsComponents.Add(ecsComponent);
            }
            for (int i = 0; i < ecsComponents.Count; i++)
            {
                PeClass pe = new PeClass();
                pe.EcsComponent = ecsComponents[i];
                pe.Tree = PropertyTree.Create(ecsComponents[i]);
                sWindow.m_EcsComponents.Add(pe);
            }
        }

        public static void Destroy()
        {
            sWindow?.Close();
        }


        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            for (int i = 0; i < m_EcsComponents.Count; i++)
            {
                m_EcsComponents[i].Tree.Draw(false);
            }
        }
        
        protected override void OnImGUI()
        {
            base.OnImGUI();
            Repaint();
        }

        protected override void OnDestroy()
        {
            foreach (var t in m_EcsComponents)
            {
                t.Tree.Dispose();
            }
            sWindow = null;
        }
    }
}