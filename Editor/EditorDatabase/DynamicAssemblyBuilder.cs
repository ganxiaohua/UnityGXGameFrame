using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameFrame.Editor;
using Microsoft.CSharp;
using UnityEngine;

namespace GameFrame.Runtime
{
    public class DynamicAssemblyBuilder
    {
        public class BuildResult
        {
            public bool Success { get; internal set; }


            public Assembly CompiledAssembly { get; internal set; }


            public List<string> Errors { get; internal set; } = new List<string>();


            public List<string> Warnings { get; internal set; } = new List<string>();

            public string AssemblyName { get; internal set; }

            public float CompileTimeMs { get; internal set; }
        }


        private string assemblyName;
        private CSharpCodeProvider provider;

        private List<string> sourceFiles = new List<string>();

        public DynamicAssemblyBuilder AddSourceFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"源文件不存在：{filePath}");
                return this;
            }

            sourceFiles.Add(Path.GetFullPath(filePath));
            Debug.Log($"[DynamicAssemblyBuilder] 添加源文件：{filePath}");
            return this;
        }

        public DynamicAssemblyBuilder AddSourceFiles(params string[] filePaths)
        {
            foreach (var path in filePaths)
            {
                AddSourceFile(path);
            }

            return this;
        }


        public DynamicAssemblyBuilder AddSourceFromDirectory(string directory, string searchPattern = "*.cs", bool recursive = true)
        {
            if (!Directory.Exists(directory))
            {
                Debug.LogWarning($"目录不存在：{directory}");
                return this;
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(directory, searchPattern, searchOption);

            foreach (var file in files)
            {
                if (!file.EndsWith("Debug.cs"))
                    AddSourceFile(file);
            }

            Debug.Log($"[DynamicAssemblyBuilder] 从目录添加了 {files.Length} 个文件：{directory}");
            return this;
        }


        public BuildResult Build()
        {
            var buildResult = new BuildResult {AssemblyName = assemblyName};
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (sourceFiles.Count == 0)
                {
                    buildResult.Success = false;
                    buildResult.Errors.Add("没有添加任何源文件");
                    Debug.LogError("[DynamicAssemblyBuilder] 编译失败：没有添加任何源文件");
                    return buildResult;
                }

                string[] sources = new string[sourceFiles.Count];
                for (int i = 0; i < sourceFiles.Count; i++)
                {
                    if (!File.Exists(sourceFiles[i]))
                    {
                        buildResult.Errors.Add($"源文件不存在：{sourceFiles[i]}");
                        continue;
                    }

                    sources[i] = File.ReadAllText(sourceFiles[i]);
                }

                if (buildResult.Errors.Count > 0)
                {
                    buildResult.Success = false;
                    return buildResult;
                }

                provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateExecutable = false;
                parameters.GenerateInMemory = true;
                parameters.ReferencedAssemblies.AddRange(CSharpRunner.GetReferences().ToArray());

                CompilerResults compilerResults = provider.CompileAssemblyFromSource(parameters, sources);
                stopwatch.Stop();
                buildResult.CompileTimeMs = stopwatch.ElapsedMilliseconds;

                if (compilerResults.Errors.HasErrors)
                {
                    buildResult.Success = false;
                    foreach (CompilerError error in compilerResults.Errors)
                    {
                        if (!string.IsNullOrEmpty(error.ErrorText))
                        {
                            string errorMsg = $"[{Path.GetFileName(error.FileName)}] 错误 {error.ErrorNumber}: {error.ErrorText} (行:{error.Line}, 列:{error.Column})";
                            buildResult.Errors.Add(errorMsg);
                            Debug.LogError($"[DynamicAssemblyBuilder] {errorMsg}");
                        }
                    }

                    // 收集警告
                    foreach (CompilerError warning in compilerResults.Errors)
                    {
                        if (!string.IsNullOrEmpty(warning.ErrorText))
                        {
                            string warnMsg = $"[{Path.GetFileName(warning.FileName)}] 警告 {warning.ErrorNumber}: {warning.ErrorText} (行:{warning.Line}, 列:{warning.Column})";
                            buildResult.Warnings.Add(warnMsg);
                            Debug.LogWarning($"[DynamicAssemblyBuilder] {warnMsg}");
                        }
                    }

                    return buildResult;
                }

                // 编译成功
                buildResult.Success = true;
                buildResult.CompiledAssembly = compilerResults.CompiledAssembly;

                Debug.Log($"[DynamicAssemblyBuilder] ✓ 程序集 {assemblyName} 编译成功！耗时：{buildResult.CompileTimeMs}ms");
                return buildResult;
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                buildResult.CompileTimeMs = stopwatch.ElapsedMilliseconds;
                buildResult.Success = false;
                buildResult.Errors.Add($"编译异常：{e.Message}\n{e.StackTrace}");
                Debug.LogError($"[DynamicAssemblyBuilder] ✗ 编译失败：{e.Message}");

                return buildResult;
            }
        }


        public void Destroy()
        {
            sourceFiles.Clear();
            provider?.Dispose();
        }

        public int SourceFileCount => sourceFiles.Count;
    }
}