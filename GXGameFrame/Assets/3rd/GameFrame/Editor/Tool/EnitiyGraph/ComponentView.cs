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

        public static void Init(List<ECSComponent> ecsComponents, ECSEntity ecsEntity)
        {
            if (sWindow != null)
                sWindow.Close();
            sWindow = GetWindow<ComponentView>();
            sWindow.m_EcsComponents.Clear();
            sWindow.m_EcsEntity = ecsEntity;
            for (int i = 0; i < ecsComponents.Count; i++)
            {
                PeClass pe = new PeClass();
                pe.EcsComponent = ecsComponents[i];
                pe.Tree = PropertyTree.Create(ecsComponents[i]);
                sWindow.m_EcsComponents.Add(pe);
            }
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            for (int i = 0; i < m_EcsComponents.Count; i++)
            {
                m_EcsComponents[i].Tree.Draw(false);
            }
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