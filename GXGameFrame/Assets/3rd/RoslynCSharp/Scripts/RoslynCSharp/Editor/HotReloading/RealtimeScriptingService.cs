using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoslynCSharp.HotReloading
{
    [InitializeOnLoad]
    public class RealtimeScriptingService
    {
        // Private
        private static ScriptDomain domain = null;
        private static RealtimeScriptWatcher watcher = null;

        // Constructor
        static RealtimeScriptingService()
        {
            // Check if hot reloading is enabled
            if(RoslynCSharp.Settings.AllowHotReloading == true)
                OnInitialize();
        }

        // Methods
        private static void OnInitialize()
        {
            // Create hot reload domain
            domain = ScriptDomain.CreateDomain("hotreload");

            // Create watcher
            watcher = new RealtimeScriptWatcher(domain, Application.dataPath);
        }
    }
}
