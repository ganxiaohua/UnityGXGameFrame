using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace GameFrame.Editor
{
    public class EntityView : OdinEditorWindow
    {
        private static EntityView sEntityPanel;
        private PropertyTree propertyTree;
        private IEntity target;

        public static void Init(IEntity entity)
        {
            sEntityPanel ??= GetWindow<EntityView>();
            sEntityPanel.titleContent.text = string.IsNullOrEmpty(entity.Name) ? "Entity" : entity.Name;
            sEntityPanel.target = entity;
        }

        protected override void OnBeginDrawEditors()
        {
            if (propertyTree == null)
            {
                propertyTree = PropertyTree.Create(target);
                this.propertyTree.AttributeProcessorLocator = new CustomMinionAttributeProcessorLocator();
            }

            propertyTree.Draw(false);
        }

        public static void Destroy()
        {
            sEntityPanel?.Close();
        }

        protected override void OnDestroy()
        {
            sEntityPanel = null;
            propertyTree?.Dispose();
            propertyTree = null;
        }

        [OdinDontRegister]
        private class CustomMinionAttributeProcessor : OdinAttributeProcessor<IEntity>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
            {
                if (member is FieldInfo or PropertyInfo)
                {
                    if (member.DeclaringType != typeof(Entity) || member.DeclaringType != typeof(FsmController))
                    {
                        attributes.Add<ShowInInspectorAttribute>();
                        attributes.Add<ReadOnlyAttribute>();
                    }
                }
            }
        }

        private class CustomMinionAttributeProcessorLocator : OdinAttributeProcessorLocator
        {
            private static readonly CustomMinionAttributeProcessor Processor = new CustomMinionAttributeProcessor();

            public override List<OdinAttributeProcessor> GetChildProcessors(InspectorProperty parentProperty, MemberInfo member)
            {
                return new List<OdinAttributeProcessor>() {Processor};
            }

            public override List<OdinAttributeProcessor> GetSelfProcessors(InspectorProperty property)
            {
                return new List<OdinAttributeProcessor>() {Processor};
            }
        }
    }
}