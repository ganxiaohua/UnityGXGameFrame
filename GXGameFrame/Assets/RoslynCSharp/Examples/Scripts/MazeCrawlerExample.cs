using RoslynCSharp.Compiler;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// Main type fomr the included example maze crawler game.
    /// Manages the scripting and UI elements of the game.
    /// </summary>
    public class MazeCrawlerExample : MonoBehaviour
    {
        // Private
        private string activeCSharpSource = null;
        private ScriptProxy activeCrawlerScript = null;
        private ScriptDomain domain = null;

        // Public
        /// <summary>
        /// The main code editor input field.
        /// </summary>
        public InputField runCrawlerInput;
        /// <summary>
        /// The run code button.
        /// </summary>
        public Button runCrawlerButton;
        /// <summary>
        /// The stop code button.
        /// </summary>
        public Button stopCrawlerButton;
        /// <summary>
        /// The restart code button.
        /// </summary>
        public Button restartCrawlerButton;
        /// <summary>
        /// The edit code button.
        /// </summary>
        public Button editCodeButton;

        /// <summary>
        /// The code editor window root game object.
        /// </summary>
        public GameObject codeEditorWindow;
        /// <summary>
        /// The code editor window close button.
        /// </summary>
        public Button codeEditorCloseButton;
        /// <summary>
        /// The code editor button load template button.
        /// </summary>
        public Button codeEditorLoadTemplateButton;
        /// <summary>
        /// The code editor button load solution button.
        /// </summary>
        public Button codeEditorLoadSolutionButton;

        /// <summary>
        /// The maze mouse crawler game object.
        /// </summary>
        public GameObject mazeMouse;
        /// <summary>
        /// The breadcrumb object that is dropped after every move.
        /// </summary>
        public GameObject breadcrumbPrefab;
        /// <summary>
        /// The code template for an empty script.
        /// </summary>
        public TextAsset mazeCodeTemplate;
        /// <summary>
        /// The completed code for the maze craler.
        /// </summary>
        public TextAsset mazeCodeSolution;
        /// <summary>
        /// The speed that the crawler moves around the maze.
        /// </summary>
        public float mouseSpeed = 5f;
        /// <summary>
        /// When true the maze solution will be loaded instead of the blank code template.
        /// </summary>
        public bool showCompletedCodeOnStartup = false;

        public AssemblyReferenceAsset[] assemblyReferences;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Awake()
        {
            runCrawlerButton.onClick.AddListener(RunCrawler);
            stopCrawlerButton.onClick.AddListener(StopCrawler);
            restartCrawlerButton.onClick.AddListener(RestartCrawler);
            editCodeButton.onClick.AddListener(() => codeEditorWindow.SetActive(true));
            codeEditorCloseButton.onClick.AddListener(() => codeEditorWindow.SetActive(false));
            codeEditorLoadTemplateButton.onClick.AddListener(() => runCrawlerInput.text = mazeCodeTemplate.text);
            codeEditorLoadSolutionButton.onClick.AddListener(() => runCrawlerInput.text = mazeCodeSolution.text);
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Create the domain
            domain = ScriptDomain.CreateDomain("MazeCrawlerCode", true);

            // Add assembly references
            foreach (AssemblyReferenceAsset reference in assemblyReferences)
                domain.RoslynCompilerService.ReferenceAssemblies.Add(reference);

            if (showCompletedCodeOnStartup == true)
            {
                // Load the solution code
                runCrawlerInput.text = mazeCodeSolution.text;
            }
            else
            {
                // Load the template code
                runCrawlerInput.text = mazeCodeTemplate.text;
            }            
        }

        /// <summary>
        /// Main run method.
        /// This causes any modified code to be recompiled and executed on the mouse crawler.
        /// </summary>
        public void RunCrawler()
        {
            // Get the C# code from the input field
            string cSharpSource = runCrawlerInput.text;

            // Dont recompile the same code
            if (activeCSharpSource != cSharpSource || activeCrawlerScript == null)
            {
                // Remove any other scripts
                StopCrawler();

                //try
                {
                    // Compile code
                    ScriptType type = domain.CompileAndLoadMainSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyReferences );

                    // Check for null
                    if (type == null)
                    {
                        if (domain.RoslynCompilerService.LastCompileResult.Success == false)
                            throw new Exception("Maze crawler code contained errors. Please fix and try again");
                        else if (domain.SecurityResult.IsSecurityVerified == false)
                            throw new Exception("Maze crawler code failed code security verification");
                        else
                            throw new Exception("Maze crawler code does not define a class. You must include one class definition of any name that inherits from 'RoslynCSharp.Example.MazeCrawler'");
                    }

                    // Check for base class
                    if (type.IsSubTypeOf<MazeCrawler>() == false)
                        throw new Exception("Maze crawler code must define a single type that inherits from 'RoslynCSharp.Example.MazeCrawler'");


                    

                    // Create an instance
                    activeCrawlerScript = type.CreateInstance(mazeMouse);
                    activeCSharpSource = cSharpSource;

                    // Set speed value
                    activeCrawlerScript.Fields["breadcrumbPrefab"] = breadcrumbPrefab;
                    activeCrawlerScript.Fields["moveSpeed"] = mouseSpeed;
                }
                //catch (Exception e)
                //{
                //    // Show the code editor window
                //    codeEditorWindow.SetActive(true);
                //    throw e;
                //}
            }
            else
            {
                // Get the maze crawler instance
                MazeCrawler mazeCrawler = activeCrawlerScript.GetInstanceAs<MazeCrawler>(false);

                // Call the restart method
                mazeCrawler.Restart();
            }
        }

        /// <summary>
        /// Causes the mouse crawler to stop moving and reset to its initial position.
        /// </summary>
        public void StopCrawler()
        {
            if(activeCrawlerScript != null)
            {
                // Get the maze crawler instance
                MazeCrawler mazeCrawler = activeCrawlerScript.GetInstanceAs<MazeCrawler>(false);

                // Call the restart method
                mazeCrawler.Restart();

                // Destroy script
                activeCrawlerScript.Dispose();
                activeCrawlerScript = null;
            }
        }

        /// <summary>
        /// Causes the mouse crawler to reset its initial position and start crawling again.
        /// </summary>
        public void RestartCrawler()
        {
            if(activeCrawlerScript != null)
            {
                ScriptType type = activeCrawlerScript.ScriptType;

                // Get the maze crawler instance
                MazeCrawler mazeCrawler = activeCrawlerScript.GetInstanceAs<MazeCrawler>(false);

                // Call the restart method
                mazeCrawler.Restart();

                // Remove and re-add script to reset it
                activeCrawlerScript.Dispose();
                activeCrawlerScript = type.CreateInstance(mazeMouse);

                activeCrawlerScript.Fields["breadcrumbPrefab"] = breadcrumbPrefab;
                activeCrawlerScript.Fields["moveSpeed"] = mouseSpeed;
            }
        }
    }
}
