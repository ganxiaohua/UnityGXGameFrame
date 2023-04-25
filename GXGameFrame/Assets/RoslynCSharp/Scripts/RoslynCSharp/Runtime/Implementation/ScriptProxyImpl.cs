using System;
using UnityEngine;

namespace RoslynCSharp.Implementation
{
    internal sealed class ScriptProxyImpl : ScriptProxy
    {
        // Private
        private ScriptType scriptType = null;
        private object instance = null;

        // Properties
        public override ScriptAssembly Assembly
        {
            get 
            {
                CheckDisposed();
                return scriptType.Assembly; 
            }
        }

        public override ScriptType ScriptType
        {
            get
            {
                CheckDisposed();
                return scriptType;
            }
        }

        public override object Instance
        {
            get
            {
                CheckDisposed();
                return instance;
            }
        }
        public override bool IsDisposed
        {
            get { return instance == null; }
        }

        // Construction
        protected override void ConstructInstance(ScriptType type, object instance)
        {
            this.scriptType = type;
            this.instance = instance;
        }

        // Methods
        public override void Dispose()
        {
            // Make sure the object has not already been disposed
            CheckDisposed();

            // Check for Unity object
            if (IsUnityObject == true)
            {
                if (Application.isPlaying == true)
                    UnityEngine.Object.Destroy(UnityInstance);
                else
                    UnityEngine.Object.DestroyImmediate(UnityInstance, false);
            }

            // Call the dispose method correctly
            if (instance is IDisposable)
                (instance as IDisposable).Dispose();

            // Unset reference
            scriptType = null;
            instance = null;
        }
    }
}
