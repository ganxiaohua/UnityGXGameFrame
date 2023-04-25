using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class PluginConflictDetector
{
    // Private
    private static string[] errorCodes = new string[]
    {
        "CS8356",
    };

    // Constructor
    static PluginConflictDetector()
    {
        Application.logMessageReceived += (string condition, string stackTrace, LogType type) =>
        {
            foreach (string err in errorCodes)
            {
                if (condition.Contains(err) == true)
                {
                    Debug.LogWarning("[Roslyn C#] It looks like the project contains one or more plugin conflicts. Please take a look at the 'Plugin Conflict' section of the user guide to fix the problem.");
                    return;
                }
            }
        };
    }
}
