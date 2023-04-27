using System;
using System.Collections.Generic;
using System.Reflection;

namespace RoslynCSharp
{
    public class ScriptEventHandlerProxy : IScriptEventProxy
    {
        // Types
        private class ScriptEventHandlerDummy : ScriptEventHandler
        {
            // Constructor
            public ScriptEventHandlerDummy()
                : base(null, null)
            {
            }

            // Methods
            public override void AddListener(Delegate methodDelegate)
            {
                // Do nothing
            }

            public override void RemoveListener(Delegate methodDelegate)
            {
                // Do nothing
            }
        }


        // Private
        private static readonly ScriptEventHandlerDummy dummyEventHandler = new ScriptEventHandlerDummy();

        private ScriptType scriptType = null;
        private ScriptProxy scriptProxy = null;
        private bool isStatic = false;
        private bool throwOnError = true;

        private Dictionary<EventInfo, ScriptEventHandler> eventHandlers = null;

        // Properties
        public ScriptEventHandler this[string name]
        {
            get { return GetEvent(name); }
        }

        // Constructor
        public ScriptEventHandlerProxy(ScriptType type, ScriptProxy proxy, bool isStatic, bool throwOnError)
        {
            this.scriptType = type;
            this.scriptProxy = proxy;
            this.isStatic = isStatic;
            this.throwOnError = throwOnError;
        }

        // Methods
        public ScriptEventHandler GetEvent(string name)
        {
            try
            {
                // Try to get an event with the specified name
                EventInfo info = scriptType.FindCachedEvent(name, isStatic);

                // Check for event not found
                if(info == null)
                    throw new TargetException(string.Format("Type '{0}' does not define an event called '{1}'", scriptType, name));

                // Select the target instance
                object instance = (scriptProxy != null)
                    ? scriptProxy.Instance
                    : null;

                // Create cache collection
                if (eventHandlers == null)
                    eventHandlers = new Dictionary<EventInfo, ScriptEventHandler>();

                ScriptEventHandler handler;
                if(eventHandlers.TryGetValue(info, out handler) == false)
                {
                    handler = new ScriptEventHandler(info, instance);
                    eventHandlers.Add(info, handler);
                }

                return handler;
            }
            catch
            {
                if (throwOnError == true)
                    throw;
            }
            return dummyEventHandler;
        }
    }
}
