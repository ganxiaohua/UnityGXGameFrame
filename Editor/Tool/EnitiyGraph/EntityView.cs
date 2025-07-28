using System;
using System.Collections.Generic;
using System.Reflection;
using GameFrame.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace GameFrame.Editor
{
    public class EntityView : OdinEditorWindow
    {
        private static EntityView sEntityPanel;
        private PropertyTree propertyTree;
        private IEntity target;
        private static List<Type> filterType = new() {typeof(Entity), typeof(FsmController), typeof(UIEntity)};

        public static void Init(IEntity entity)
        {
            sEntityPanel = GetWindow<EntityView>();
            sEntityPanel.titleContent.text = string.IsNullOrEmpty(entity.Name) ? "Entity" : entity.Name;
            sEntityPanel.target = entity;
        }

        protected override void OnBeginDrawEditors()
        {
            if (propertyTree == null)
            {
                propertyTree = PropertyTree.Create(target);
                this.propertyTree.AttributeProcessorLocator = new CustomEnitiyAttributeProcessorLocator();
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
        private class CustomEntityAttributeProcessor : OdinAttributeProcessor<IEntity>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
            {
                if (member is FieldInfo or PropertyInfo)
                {
                    for (int i = 0; i < filterType.Count; i++)
                    {
                        if (member.DeclaringType == filterType[i])
                        {
                            return;
                        }
                    }
                    if (member.DeclaringType == typeof(World) && member.Name == "Multiple")
                    {
                        attributes.Add<ShowInInspectorAttribute>();
                    }
                    else if(member.DeclaringType != typeof(World))
                    {
                        attributes.Add<ShowInInspectorAttribute>();
                        attributes.Add<ReadOnlyAttribute>();
                    }
                }
            }
        }

        private class CustomEnitiyAttributeProcessorLocator : OdinAttributeProcessorLocator
        {
            private static readonly CustomEntityAttributeProcessor Processor = new CustomEntityAttributeProcessor();

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