using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace RoslynCSharp.Implementation
{
    internal sealed class ScriptTypeImpl : ScriptType
    {
        // Private
        private ScriptAssembly assembly = null;
        private ScriptType parentType = null;
        private ScriptType[] nestedTypes = null;
        private Type systemType = null;

        private ICollection<object> customAttributes = null;

        // Properties
        public override ScriptAssembly Assembly
        {
            get { return assembly; }
        }

        public override ScriptType Parent
        {
            get { return parentType; }
        }

        public override Type SystemType
        {
            get { return systemType; }
        }
        

        public override bool IsNestedType
        {
            get { return parentType != null; }
        }

        public override bool HasNestedTypes
        {
            get { return nestedTypes.Length > 0; }
        }

        public override ScriptType[] NestedTypes
        {
            get { return nestedTypes; }
        }

        public override ICollection<object> CustomAttributes
        {
            get
            {
                if (customAttributes == null)
                    customAttributes = new HashSet<object>(systemType.GetCustomAttributes(false));

                return customAttributes;
            }
        }

        // Construction
        protected override void ConstructInstance(ScriptAssembly assembly, ScriptType parent, ScriptType[] nestedTypes, Type systemType)
        {
            this.assembly = assembly;
            this.parentType = parent;
            this.nestedTypes = nestedTypes;
            this.systemType = systemType;
        }

        // Methods
        protected override ScriptProxy CreateInstanceImpl(object[] args)
        {
            object instance = null;

            try
            {
                instance = Activator.CreateInstance(systemType, BindingFlags.Default, null, args, null);
            }
            catch(MissingMethodException)
            {
                if (args.Length > 0)
                    return null;

                instance = FormatterServices.GetUninitializedObject(SystemType);
            }

            if(instance != null)
            {
                return ScriptProxy.CreateScriptProxy<ScriptProxyImpl>(this, instance);
            }
            return null;
        }

        protected override ScriptProxy CreateMonoBehaviourInstanceImpl(GameObject parent)
        {
            // Check for no parent
            if (parent == null)
                throw new InvalidOperationException("A non-destroyed game object instance must be provided for MonoBehaviour components");

            // Add the component
            object instance = parent.AddComponent(systemType);

            // Check for valid instance
            if(instance != null)
            {
                return ScriptProxy.CreateScriptProxy<ScriptProxyImpl>(this, instance);
            }
            return null;
        }

        protected override ScriptProxy CreateScriptableObjectInstanceImpl()
        {
            // Create instance
            ScriptableObject scriptableObject = ScriptableObject.CreateInstance(systemType);

            // Check for valid instance
            if (scriptableObject != null)
            {
                return ScriptProxy.CreateScriptProxy<ScriptProxyImpl>(this, scriptableObject);
            }
            return null;
        }

        protected override EventInfo FindEventImpl(string name, BindingFlags bindingAttrib)
        {
            return systemType.GetEvent(name, bindingAttrib);
        }

        protected override FieldInfo FindFieldImpl(string name, BindingFlags bindingAttrib)
        {
            return systemType.GetField(name, bindingAttrib);
        }

        protected override MethodInfo FindMethodImpl(string name, BindingFlags bindingAttrib)
        {
            return systemType.GetMethod(name, bindingAttrib);
        }

        protected override PropertyInfo FindPropertyImpl(string name, BindingFlags bindingAttrib)
        {
            return systemType.GetProperty(name, bindingAttrib);
        }
    }
}
