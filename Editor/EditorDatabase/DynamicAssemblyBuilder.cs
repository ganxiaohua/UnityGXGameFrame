using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GameFrame.Editor;
using UnityEditor;
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


        private const int CompileTimeoutMs = 60000;
        private static readonly Regex DiagnosticPattern = new Regex(@"(?:^|:\s+)(?<kind>warning|error) CS\d+:", RegexOptions.CultureInvariant);

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
            var assemblyName = "DynamicComponents_" + Guid.NewGuid().ToString("N");
            var buildResult = new BuildResult {AssemblyName = assemblyName};
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string tempDirectory = null;

            try
            {
                if (sourceFiles.Count == 0)
                {
                    buildResult.Success = false;
                    buildResult.Errors.Add("没有添加任何源文件");
                    Debug.LogError("[DynamicAssemblyBuilder] 编译失败：没有添加任何源文件");
                    return buildResult;
                }

                for (int i = 0; i < sourceFiles.Count; i++)
                {
                    if (!File.Exists(sourceFiles[i]))
                    {
                        buildResult.Errors.Add($"源文件不存在：{sourceFiles[i]}");
                    }
                }

                if (buildResult.Errors.Count > 0)
                {
                    buildResult.Success = false;
                    return buildResult;
                }

                var dotnetPath = Path.Combine(EditorApplication.applicationContentsPath, "NetCoreRuntime", "dotnet.exe");
                var compilerPath = Path.Combine(EditorApplication.applicationContentsPath, "DotNetSdkRoslyn", "csc.dll");
                if (!File.Exists(dotnetPath) || !File.Exists(compilerPath))
                {
                    AddError(buildResult, $"找不到 Unity 自带的 Roslyn 编译器：{dotnetPath}；{compilerPath}");
                    return buildResult;
                }

                tempDirectory = Path.Combine(Path.GetTempPath(), assemblyName);
                Directory.CreateDirectory(tempDirectory);
                var responseFile = Path.Combine(tempDirectory, "components.rsp");
                var assemblyPath = Path.Combine(tempDirectory, assemblyName + ".dll");
                var references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var reference in CSharpRunner.GetReferences())
                    references.Add(Path.GetFullPath(reference));
                // GetReferences 排除了 mscorlib；Roslyn 禁用默认引用后需要显式加入 Unity 的核心程序集。
                references.Add(Path.GetFullPath(typeof(object).Assembly.Location));

                // 所有长参数放入响应文件，避免引用及源文件路径总长度超过 Windows 进程参数限制。
                using (var writer = new StreamWriter(responseFile, false, new UTF8Encoding(false)))
                {
                    writer.WriteLine("/target:library");
                    // 与当前 Unity 6000.3 的脚本编译设置一致。
                    writer.WriteLine("/langversion:9.0");
                    writer.WriteLine("/unsafe+");
                    writer.WriteLine("/nologo");
                    writer.WriteLine("/utf8output");
                    writer.WriteLine("/preferreduilang:en-US");
                    writer.WriteLine("/fullpaths");
                    writer.WriteLine($"/out:\"{assemblyPath}\"");
                    foreach (var reference in references)
                        writer.WriteLine($"/reference:\"{reference}\"");
                    foreach (var sourceFile in sourceFiles)
                        writer.WriteLine($"\"{sourceFile}\"");
                }

                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dotnetPath,
                        Arguments = $"exec \"{compilerPath}\" /noconfig /nostdlib+ \"@{responseFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };
                    process.Start();
                    // 同时读取两个输出流，避免大量诊断信息填满管道后阻塞编译进程。
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(CompileTimeoutMs))
                    {
                        process.Kill();
                        process.WaitForExit();
                        AddError(buildResult, $"Roslyn 编译超时（{CompileTimeoutMs / 1000} 秒）");
                        return buildResult;
                    }

                    var output = outputTask.GetAwaiter().GetResult() + "\n" + errorTask.GetAwaiter().GetResult();
                    ReadDiagnostics(output, buildResult);
                    if (process.ExitCode != 0 || buildResult.Errors.Count > 0)
                    {
                        if (buildResult.Errors.Count == 0)
                            AddError(buildResult, $"Roslyn 编译失败，退出码：{process.ExitCode}\n{output.Trim()}");
                        return buildResult;
                    }
                }

                if (!File.Exists(assemblyPath))
                {
                    AddError(buildResult, $"Roslyn 未生成程序集：{assemblyPath}");
                    return buildResult;
                }

                // 按字节加载，避免程序集文件被锁定，保证重复生成后可以清理临时文件。
                buildResult.CompiledAssembly = Assembly.Load(File.ReadAllBytes(assemblyPath));
                buildResult.Success = true;

                Debug.Log($"[DynamicAssemblyBuilder] ✓ 程序集 {assemblyName} 编译成功！耗时：{stopwatch.ElapsedMilliseconds}ms");
                return buildResult;
            }
            catch (Exception e)
            {
                buildResult.Success = false;
                buildResult.Errors.Add($"编译异常：{e.Message}\n{e.StackTrace}");
                Debug.LogError($"[DynamicAssemblyBuilder] ✗ 编译失败：{e.Message}");

                return buildResult;
            }
            finally
            {
                stopwatch.Stop();
                buildResult.CompileTimeMs = stopwatch.ElapsedMilliseconds;
                if (tempDirectory != null && Directory.Exists(tempDirectory))
                {
                    try
                    {
                        File.Delete(Path.Combine(tempDirectory, "components.rsp"));
                        File.Delete(Path.Combine(tempDirectory, assemblyName + ".dll"));
                        Directory.Delete(tempDirectory);
                    }
                    catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
                    {
                        Debug.LogWarning($"[DynamicAssemblyBuilder] 无法清理临时目录 {tempDirectory}：{e.Message}");
                    }
                }
            }
        }
        


        private static void ReadDiagnostics(string output, BuildResult buildResult)
        {
            using var reader = new StringReader(output);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var match = DiagnosticPattern.Match(line);
                if (!match.Success)
                    continue;
                // 保留 Roslyn 原始诊断中的文件路径、行列和错误编号。
                if (match.Groups["kind"].Value == "warning")
                {
                    buildResult.Warnings.Add(line);
                    Debug.LogWarning($"[DynamicAssemblyBuilder] {line}");
                }
                else
                {
                    AddError(buildResult, line);
                }
            }
        }

        private static void AddError(BuildResult buildResult, string message)
        {
            buildResult.Errors.Add(message);
            Debug.LogError($"[DynamicAssemblyBuilder] {message}");
        }

        public void Destroy()
        {
            sourceFiles.Clear();
        }

        public int SourceFileCount => sourceFiles.Count;
    }
}
