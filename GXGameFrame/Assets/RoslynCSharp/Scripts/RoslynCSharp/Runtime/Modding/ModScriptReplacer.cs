using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoslynCSharp.Modding
{
    public enum ScriptReplacerOptions
    {
        Default = CopySerializeFields | DestroyOriginalScript,

        DontRequireAttribute = 1,
        CopySerializeFields = 2,
        CopyNonSerializeFields = 4,
        DestroyOriginalScript = 8,
        DisableOriginalScript = 16,
        ReplaceDisabledScripts = 32,
        RequireExplicitTypeMatches = 64,
    }

    /// <summary>
    /// A useful utility class intended to support modding use cases.
    /// Allows you to replace an existing mono behaviour script component with an externally compiled replacement, allowing functionality to be added or changed.
    /// Only mono behaviour scripts that define the <see cref="ModReplaceableBehaviourAttribute"/> attribute will be considered during the replacement phase.
    /// Using the attribute, you can specify which name, base class and interface imeplementations the external compiled code must use in order to be considered as a replacement.
    /// </summary>
    public static class ModScriptReplacer
    {
        // Types
        private struct ScriptReplacementInfo
        {
            // Public
            public string replaceName;
            public Type requireBaseType;
            public Type[] requireInterfaceTypes;
        }
        
        // Methods
        /// <summary>
        /// Replace all mod replaceable script behaviours in the active scene with appropriate types in the specified <see cref="ScriptAssembly"/>.
        /// </summary>
        /// <param name="scriptAssembly">An externally compiled or loaded <see cref="ScriptAssembly"/> that will be used for replacement</param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForActiveScene(ScriptAssembly scriptAssembly, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            ModScriptReplacerReport report;
            return ReplaceScriptsForActiveScene(scriptAssembly, out report, options);
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours in the active scene with appropriate types in the specified <see cref="ScriptAssembly"/>.
        /// This overload outputs a <see cref="ModScriptReplacerReport"/> which contains any errors or warnings that occured during script replacement, as well as info messages for scripts that were replaced successfully.
        /// </summary>
        /// <param name="scriptAssembly">An externally compiled or loaded <see cref="ScriptAssembly"/> that will be used for replacement</param>
        /// <param name="report"></param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForActiveScene(ScriptAssembly scriptAssembly, out ModScriptReplacerReport report, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            return ReplaceScriptsForScene(SceneManager.GetActiveScene(), scriptAssembly, out report, options);
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours in the active scene with specified <see cref="ScriptType"/> if it is a suitable match.
        /// </summary>
        /// <param name="scriptType">An externally compiled or loaded <see cref="ScriptType"/> that will be used for replacement</param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForActiveScene(ScriptType scriptType, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            ModScriptReplacerReport report;
            return ReplaceScriptsForActiveScene(scriptType, out report, options);
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours in the active scene with specified <see cref="ScriptType"/> if it is a suitable match.
        /// This overload outputs a <see cref="ModScriptReplacerReport"/> which contains any errors or warnings that occured during script replacement, as well as info messages for scripts that were replaced successfully.
        /// </summary>
        /// <param name="scriptType">An externally compiled or loaded <see cref="ScriptType"/> that will be used for replacement</param>
        /// <param name="report"></param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForActiveScene(ScriptType scriptType, out ModScriptReplacerReport report, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            return ReplaceScriptsForScene(SceneManager.GetActiveScene(), scriptType, out report, options);
        }



        /// <summary>
        /// Replace all mod replaceable script behaviours in the specified scene with appropriate types in the specified <see cref="ScriptAssembly"/>.
        /// </summary>
        /// <param name="targetScene">The scene that should be scanned for replaceable behaviours</param>
        /// <param name="scriptAssembly">An externally compiled or loaded <see cref="ScriptAssembly"/> that will be used for replacement</param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForScene(Scene targetScene, ScriptAssembly scriptAssembly, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            ModScriptReplacerReport report;
            return ReplaceScriptsForScene(targetScene, scriptAssembly, out report, options);
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours in the specified scene with appropriate types in the specified <see cref="ScriptAssembly"/>.
        /// This overload outputs a <see cref="ModScriptReplacerReport"/> which contains any errors or warnings that occured during script replacement, as well as info messages for scripts that were replaced successfully.
        /// </summary>
        /// <param name="targetScene">The scene that should be scanned for replaceable behaviours</param>
        /// <param name="scriptAssembly">An externally compiled or loaded <see cref="ScriptAssembly"/> that will be used for replacement</param>
        /// <param name="report"></param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForScene(Scene targetScene, ScriptAssembly scriptAssembly, out ModScriptReplacerReport report, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            bool failed = false;
            report = new ModScriptReplacerReport();

            bool includeInactive = (options & ScriptReplacerOptions.ReplaceDisabledScripts) != 0;

            foreach (GameObject gameObject in targetScene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive))
                {
                    if (ReplaceScriptBehaviourImpl(behaviour, scriptAssembly, ref report, options) == false)
                        failed = true;
                }
            }
            return failed == false;
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours in the specified scene with specified <see cref="ScriptType"/> if it is a suitable match.
        /// </summary>
        /// <param name="targetScene">The scene that should be scanned for replaceable behaviours</param>
        /// <param name="scriptType">An externally compiled or loaded <see cref="ScriptType"/> that will be used for replacement</param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForScene(Scene targetScene, ScriptType scriptType)
        {
            ModScriptReplacerReport report;
            return ReplaceScriptsForScene(targetScene, scriptType, out report);
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours in the specified scene with specified <see cref="ScriptType"/> if it is a suitable match.
        /// This overload outputs a <see cref="ModScriptReplacerReport"/> which contains any errors or warnings that occured during script replacement, as well as info messages for scripts that were replaced successfully.
        /// </summary>
        /// <param name="targetScene">The scene that should be scanned for replaceable behaviours</param>
        /// <param name="scriptType">An externally compiled or loaded <see cref="ScriptType"/> that will be used for replacement</param>
        /// <param name="report"></param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForScene(Scene targetScene, ScriptType scriptType, out ModScriptReplacerReport report, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            bool failed = false;
            report = new ModScriptReplacerReport();

            bool includeInactive = (options & ScriptReplacerOptions.ReplaceDisabledScripts) != 0;

            foreach (GameObject gameObject in targetScene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive))
                {
                    if (ReplaceScriptBehaviourImpl(behaviour, scriptType, ref report, options) == false)
                        failed = true;
                }
            }
            return failed == false;
        }



        /// <summary>
        /// Replace all mod replaceable script behaviours on the specified game object with appropriate types in the specified <see cref="ScriptAssembly"/>.
        /// </summary>
        /// <param name="gameObject">The game object that should be scanned for replaceable behaviours</param>
        /// <param name="scriptAssembly">An externally compiled or loaded <see cref="ScriptAssembly"/> that will be used for replacement</param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForObject(GameObject gameObject, ScriptAssembly scriptAssembly, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            ModScriptReplacerReport report;
            return ReplaceScriptsForObject(gameObject, scriptAssembly, out report, options);
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours on the specified game object with appropriate types in the specified <see cref="ScriptAssembly"/>.
        /// This overload outputs a <see cref="ModScriptReplacerReport"/> which contains any errors or warnings that occured during script replacement, as well as info messages for scripts that were replaced successfully.
        /// </summary>
        /// <param name="gameObject">The game object that should be scanned for replaceable behaviours</param>
        /// <param name="scriptAssembly">An externally compiled or loaded <see cref="ScriptAssembly"/> that will be used for replacement</param>
        /// <param name="report"></param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForObject(GameObject gameObject, ScriptAssembly scriptAssembly, out ModScriptReplacerReport report, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            bool failed = false;
            report = new ModScriptReplacerReport();

            bool includeInactive = (options & ScriptReplacerOptions.ReplaceDisabledScripts) != 0;

            foreach (MonoBehaviour behaviour in gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive))
            {
                if (ReplaceScriptBehaviourImpl(behaviour, scriptAssembly, ref report, options) == false)
                    failed = true;
            }
            return failed == false;
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours on the specified game object with specified <see cref="ScriptType"/> if it is a suitable match.
        /// </summary>
        /// <param name="gameObject">The game object that should be scanned for replaceable behaviours</param>
        /// <param name="scriptType">An externally compiled or loaded <see cref="ScriptType"/> that will be used for replacement</param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForObject(GameObject gameObject, ScriptType scriptType, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            ModScriptReplacerReport report;
            return ReplaceScriptsForObject(gameObject, scriptType, out report, options);
        }

        /// <summary>
        /// Replace all mod replaceable script behaviours on the specified game object with specified <see cref="ScriptType"/> if it is a suitable match.
        /// This overload outputs a <see cref="ModScriptReplacerReport"/> which contains any errors or warnings that occured during script replacement, as well as info messages for scripts that were replaced successfully.
        /// </summary>
        /// <param name="gameObject">The game object that should be scanned for replaceable behaviours</param>
        /// <param name="scriptType">An externally compiled or loaded <see cref="ScriptType"/> that will be used for replacement</param>
        /// <param name="report"></param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptsForObject(GameObject gameObject, ScriptType scriptType, out ModScriptReplacerReport report, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            bool failed = false;
            report = new ModScriptReplacerReport();

            bool includeInactive = (options & ScriptReplacerOptions.ReplaceDisabledScripts) != 0;

            foreach (MonoBehaviour behaviour in gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive))
            {
                if (ReplaceScriptBehaviourImpl(behaviour, scriptType, ref report, options) == false)
                    failed = true;
            }
            return failed == false;
        }



        /// <summary>
        /// Attempt to replace the specified mod replaceable script behaviour with appropriate a type in the specified <see cref="ScriptAssembly"/>.
        /// </summary>
        /// <param name="behaviour">The mono behaviour script that should be replaced if a suitable match is found</param>
        /// <param name="scriptAssembly">An externally compiled or loaded <see cref="ScriptAssembly"/> that will be used for replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptBehaviour(MonoBehaviour behaviour, ScriptAssembly scriptAssembly, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            ModScriptReplacerReport report;
            return ReplaceScriptBehaviour(behaviour, scriptAssembly, out report, options);
        }

        /// <summary>
        /// Attempt to replace the specified mod replaceable script behaviour with appropriate a type in the specified <see cref="ScriptAssembly"/>.
        /// This overload outputs a <see cref="ModScriptReplacerReport"/> which contains any errors or warnings that occured during script replacement, as well as info messages for scripts that were replaced successfully.
        /// </summary>
        /// <param name="behaviour">The mono behaviour script that should be replaced if a suitable match is found</param>
        /// <param name="scriptAssembly">An externally compiled or loaded <see cref="ScriptAssembly"/> that will be used for replacement</param>
        /// <param name="report"></param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptBehaviour(MonoBehaviour behaviour, ScriptAssembly scriptAssembly, out ModScriptReplacerReport report, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            report = new ModScriptReplacerReport();
            return ReplaceScriptBehaviourImpl(behaviour, scriptAssembly, ref report, options);
        }

        /// <summary>
        /// Attempt to replace the specified mod replaceable script behaviour with appropriate a type in the specified <see cref="ScriptAssembly"/>.
        /// </summary>
        /// <param name="behaviour">The mono behaviour script that should be replaced if a suitable match is found</param>
        /// <param name="scriptType">An externally compiled or loaded <see cref="ScriptType"/> that will be used for replacement</param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptBehaviour(MonoBehaviour behaviour, ScriptType scriptType, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            ModScriptReplacerReport report;
            return ReplaceScriptBehaviour(behaviour, scriptType, out report, options);
        }

        /// <summary>
        /// Attempt to replace the specified mod replaceable script behaviour with appropriate a type in the specified <see cref="ScriptAssembly"/>.
        /// This overload outputs a <see cref="ModScriptReplacerReport"/> which contains any errors or warnings that occured during script replacement, as well as info messages for scripts that were replaced successfully.
        /// </summary>
        /// <param name="behaviour">The mono behaviour script that should be replaced if a suitable match is found</param>
        /// <param name="scriptType">An externally compiled or loaded <see cref="ScriptType"/> that will be used for replacement</param>
        /// <param name="report">A report which will contain useful warnings and errors if the replacement fails</param>
        /// <param name="options">Additional options for more control over script replacement</param>
        /// <returns>True if all scripts replecements were sucessful, or if there were no suitable scripts to replace. False if there were one or more issues when replacing scripts</returns>
        public static bool ReplaceScriptBehaviour(MonoBehaviour behaviour, ScriptType scriptType, out ModScriptReplacerReport report, ScriptReplacerOptions options = ScriptReplacerOptions.Default)
        {
            report = new ModScriptReplacerReport();
            return ReplaceScriptBehaviourImpl(behaviour, scriptType, ref report, options);
        }



        private static bool ReplaceScriptBehaviourImpl(MonoBehaviour behaviour, ScriptAssembly scriptAssembly, ref ModScriptReplacerReport report, ScriptReplacerOptions options)
        {
            // Check for null behaviour
            if (behaviour == null)
                report.AddErrorFormat("Target replaceable behaviour '{0}' has been destroyed and will be skipped", behaviour);

            // Check for attribute
            Type behaviourType = behaviour.GetType();

            // Create the replacement info
            ScriptReplacementInfo replaceInfo = new ScriptReplacementInfo
            {
                replaceName = behaviourType.Name,
            };

            if ((options & ScriptReplacerOptions.DontRequireAttribute) == 0)
            {
                // Check for attribute
                if (behaviourType.IsDefined(typeof(ModReplaceableBehaviourAttribute), false) == false)
                    return true;

                // Get the attribute
                ModReplaceableBehaviourAttribute attrib = behaviourType.GetCustomAttributes(typeof(ModReplaceableBehaviourAttribute), false)[0] as ModReplaceableBehaviourAttribute;

                // Set replacement info
                replaceInfo.replaceName = (string.IsNullOrEmpty(attrib.ReplaceScriptName) == true) ? behaviourType.Name : attrib.ReplaceScriptName;
                replaceInfo.requireBaseType = attrib.RequireBaseType;
                replaceInfo.requireInterfaceTypes = attrib.RequireInterfaceTypes;
            }

            bool failed = false;            

            // Check all behaviour types
            foreach(ScriptType scriptType in scriptAssembly.EnumerateAllMonoBehaviourTypes())
            {
                // Check for replaceable script
                if (CheckReplacementScriptMatch(behaviourType, behaviour, replaceInfo, scriptType, ref report) == false)
                {
                    failed = true;
                    continue;
                }

                // Replace the script
                ReplaceScriptBehaviourInstance(behaviourType, behaviour, scriptType, ref report, options);
            }

            return failed == false;
        }

        private static bool ReplaceScriptBehaviourImpl(MonoBehaviour behaviour, ScriptType scriptType, ref ModScriptReplacerReport report, ScriptReplacerOptions options)
        {
            // Check for null behaviour
            if (behaviour == null)
            {
                report.AddErrorFormat("Target replaceable behaviour '{0}' has been destroyed and will be skipped", behaviour);
                return false;
            }

            // Check for attribute
            Type behaviourType = behaviour.GetType();

            // Create the replacement info
            ScriptReplacementInfo replaceInfo = new ScriptReplacementInfo
            {
                replaceName = behaviourType.Name,
            };

            // Check for attribute
            if ((options & ScriptReplacerOptions.DontRequireAttribute) == 0)
            {
                // Check for attribute
                if (behaviourType.IsDefined(typeof(ModReplaceableBehaviourAttribute), false) == false)
                    return true;

                // Get the attribute
                ModReplaceableBehaviourAttribute attrib = behaviourType.GetCustomAttributes(typeof(ModReplaceableBehaviourAttribute), false)[0] as ModReplaceableBehaviourAttribute;

                // Set replacement info
                replaceInfo.replaceName = (string.IsNullOrEmpty(attrib.ReplaceScriptName) == true) ? behaviourType.Name : attrib.ReplaceScriptName;
                replaceInfo.requireBaseType = attrib.RequireBaseType;
                replaceInfo.requireInterfaceTypes = attrib.RequireInterfaceTypes;
            }
            

            // Check for replaceable script
            if (CheckReplacementScriptMatch(behaviourType, behaviour, replaceInfo, scriptType, ref report) == false)
                return false;

            // Replace the script
            ReplaceScriptBehaviourInstance(behaviourType, behaviour, scriptType, ref report, options);

            return true;
        }

        //private static bool CheckReplacementScriptMatch(Type behaviourType, MonoBehaviour behaviour, ModReplaceableBehaviourAttribute attribute, ScriptType scriptType, ref ModScriptReplacerReport report)
        private static bool CheckReplacementScriptMatch(Type behaviourType, MonoBehaviour behaviour, in ScriptReplacementInfo replaceInfo, ScriptType scriptType, ref ModScriptReplacerReport report)
        {
            // Check for matching name
            if (scriptType.Name == replaceInfo.replaceName)
            {
                // Check for base type
                if (replaceInfo.requireBaseType != null && scriptType.SystemType.BaseType != replaceInfo.requireBaseType)
                {
                    report.AddErrorFormat("Script type '{0}' cannot be used as a replacement script because it does not derive from required base type '{1}'", scriptType, replaceInfo.requireBaseType);
                    return false;
                }

                // Check for interface types
                if (replaceInfo.requireInterfaceTypes != null && replaceInfo.requireInterfaceTypes.Length > 0)
                {
                    bool implementsAllInterfaces = true;

                    // Get the interfaces that are actually implemented by the external code
                    Type[] implementedInterfaces = scriptType.SystemType.GetInterfaces();

                    foreach (Type interfaceType in replaceInfo.requireInterfaceTypes)
                    {
                        if (Array.Exists(implementedInterfaces, i => i == interfaceType) == false)
                        {
                            report.AddErrorFormat("Script type '{0}' cannot be used as a replacement script beacuse it does not implement the required interface type '{1}'", scriptType, interfaceType);
                            implementsAllInterfaces = false;
                        }
                    }

                    if (implementsAllInterfaces == false)
                        return false;
                }
            }
            else
            {
                // Not matching name
                return false;
            }

            // The script is a match
            return true;
        }

        private static void ReplaceScriptBehaviourInstance(Type behaviourType, MonoBehaviour behaviour, ScriptType scriptType, ref ModScriptReplacerReport report, ScriptReplacerOptions options)
        {
            // Create instance
            ScriptProxy scriptProxy = scriptType.CreateInstance(behaviour.gameObject);

            if (scriptProxy != null)
                report.AddMessageFormat("Created script instance of type '{0}' to replace existing script '{1}'", scriptType, behaviour);

            // Copy all serialized fields
            CopySerializedFields(behaviourType, behaviour, scriptProxy, ref report, options);


            try
            {
                // Invoke replaced callback
                if (behaviour is IModScriptReplacedReceiver)
                    ((IModScriptReplacedReceiver)behaviour).OnWillReplaceScript(scriptProxy.MonoBehaviourInstance);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }

            if ((options & ScriptReplacerOptions.DestroyOriginalScript) != 0)
            {
                GameObject.Destroy(behaviour);
            }
            else if((options & ScriptReplacerOptions.DisableOriginalScript) != 0)
            {
                // Disable original script
                behaviour.enabled = false;
            }
        }

        private static void CopySerializedFields(Type behaviourType, object behaviourInstance, ScriptProxy proxy, ref ModScriptReplacerReport report, ScriptReplacerOptions options)
        {
            // Get all fields
            foreach(FieldInfo field in behaviourType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
            {
                // Check for serializable
                if(field.IsPublic == true || field.IsDefined(typeof(SerializeField), false) == true || (options & ScriptReplacerOptions.CopyNonSerializeFields) != 0)
                {
                    // Check for marked as non-serialized
                    if (field.IsDefined(typeof(NonSerializedAttribute), false) == true && (options & ScriptReplacerOptions.CopyNonSerializeFields) == 0)
                        continue;


                    // We can copy the field
                    try
                    {
                        // Copy the field value
                        proxy.Fields[field.Name] = field.GetValue(behaviourInstance);

                        // Field was copied
                        report.AddMessageFormat("\tCopied field '{0}' from replacement source, with value '{1}' of type '{2}'", field.Name, field.GetValue(behaviourInstance), field.FieldType);
                    }
                    catch(TargetException)
                    {
                        report.AddWarningFormat("\tThe script type '{0}' does not define a serialized field named '{1}' of type '{2}'", proxy.ScriptType, field.Name, field.FieldType);
                    }
                    catch(ArgumentException)
                    {
                        report.AddWarningFormat("\tThe script type '{0}' defines a serialized field named '{1}', but the field type is not compatible with the corresponding behaviour field. Expected field type '{2}'", proxy.ScriptType, field.Name, field.FieldType);
                    }
                }
            }
        }
    }
}
