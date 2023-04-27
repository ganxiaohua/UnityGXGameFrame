using System;
using System.Collections.Generic;

namespace RoslynCSharp
{
    public sealed class ScriptExecution
    {
        // Private
        private HashSet<ScriptProxy> behaviourProxies = new HashSet<ScriptProxy>();
        private HashSet<ScriptProxy> instanceProxies = new HashSet<ScriptProxy>();
        private Stack<ScriptProxy> deadInstances = new Stack<ScriptProxy>();

        // Properties
        public IEnumerable<ScriptProxy> Proxies
        {
            get
            {
                RemoveDeadInstances();

                foreach (ScriptProxy proxy in behaviourProxies)
                    yield return proxy;

                foreach (ScriptProxy proxy in instanceProxies)
                    yield return proxy;
            }
        }

        public IReadOnlyCollection<ScriptProxy> BehaviourProxies
        {
            get
            {
                RemoveDeadInstances();
                return behaviourProxies;
            }
        }

        public IReadOnlyCollection<ScriptProxy> InstanceProxies
        {
            get
            {
                RemoveDeadInstances();
                return instanceProxies;
            }
        }

        // Methods
        public void AddScriptProxy(ScriptProxy proxy)
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));

            if(proxy.IsDisposed == false)
            {
                if(proxy.IsMonoBehaviour == true)
                {
                    behaviourProxies.Add(proxy);
                }
                else if(proxy.IsUnityObject == false)
                {
                    instanceProxies.Add(proxy);
                }
            }
        }

        public void Terminate()
        {
            foreach(ScriptProxy proxy in Proxies)
            {
                if (proxy.IsDisposed == false)
                    proxy.Dispose();
            }

            behaviourProxies.Clear();
            instanceProxies.Clear();
        }

        private void RemoveDeadInstances()
        {
            // Mark dead behaviours
            foreach(ScriptProxy proxy in behaviourProxies)
            {
                if (proxy.IsDisposed == true || proxy.MonoBehaviourInstance == null)
                    deadInstances.Push(proxy);
            }

            // Cleanup dead behaviours
            while (deadInstances.Count > 0)
                behaviourProxies.Remove(deadInstances.Pop());

            // Mark dead instances
            foreach(ScriptProxy proxy in instanceProxies)
            {
                if (proxy.IsDisposed == true)
                    deadInstances.Push(proxy);
            }

            // Cleanup dead instances
            while (deadInstances.Count > 0)
                instanceProxies.Remove(deadInstances.Pop());
        }
    }
}
