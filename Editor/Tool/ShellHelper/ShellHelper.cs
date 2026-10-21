using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GameFrame.Editor
{
    public interface IShellLogHandler
    {
        bool LogIsError(string log);
        bool ErrorIsLog(string err);
    }

    public static class ShellHelper
    {
        public static string LastExecuteLog { get; private set; }
        public static string LastExecuteErr { get; private set; }
        public static int LastExecuteExitCode { get; private set; }

        public static bool Start(string fileName, string arguments, string workingDirectory = "", IShellLogHandler logHandler = null)
        {
            if (string.IsNullOrEmpty(workingDirectory))
                workingDirectory = Environment.CurrentDirectory;

            string cmd = $"{fileName} [{arguments}] {workingDirectory}";

            UnityEngine.Debug.Log($"execute: {cmd}");

            try
            {
                var startInfo = new ProcessStartInfo(fileName, arguments)
                {
                    WorkingDirectory = Path.GetFullPath(workingDirectory),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                };
                var extension = Path.GetExtension(fileName);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                    (string.Equals(extension, ".bat", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(extension, ".cmd", StringComparison.OrdinalIgnoreCase)))
                {
                    // cmd /s /c needs an outer pair of quotes around the quoted script and its arguments.
                    startInfo.FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";
                    startInfo.Arguments = $"/d /s /c \"\"{Path.GetFullPath(fileName)}\" {arguments}\"";
                }

                using var process = new System.Diagnostics.Process
                {
                    StartInfo = startInfo
                };
                StringBuilder logSB = new StringBuilder();
                StringBuilder errSB = new StringBuilder();
                var redirect = logHandler != null;
                if (redirect)
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.OutputDataReceived += (_, args) =>
                    {
                        var log = args.Data;
                        if (log == null)
                            return;
                        if (logHandler.LogIsError(log))
                        {
                            lock (errSB)
                                errSB.AppendLine(log);
                            UnityEngine.Debug.LogError(log);
                        }
                        else
                        {
                            lock (logSB)
                                logSB.AppendLine(log);
                            UnityEngine.Debug.Log(log);
                        }
                    };
                    process.ErrorDataReceived += (_, args) =>
                    {
                        var err = args.Data;
                        if (err == null)
                            return;
                        if (logHandler.ErrorIsLog(err))
                        {
                            lock (logSB)
                                logSB.AppendLine(err);
                            UnityEngine.Debug.Log(err);
                        }
                        else
                        {
                            lock (errSB)
                                errSB.AppendLine(err);
                            UnityEngine.Debug.LogError(err);
                        }
                    };
                }

                process.Start();
                if (redirect)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                process.WaitForExit();
                LastExecuteLog = logSB.ToString();
                LastExecuteErr = errSB.ToString();
                LastExecuteExitCode = process.ExitCode;
                var success = LastExecuteExitCode == 0 && (!redirect || string.IsNullOrEmpty(LastExecuteErr));
                if (success)
                    UnityEngine.Debug.Log($"exit({LastExecuteExitCode}) {cmd}");
                else
                    UnityEngine.Debug.LogError($"exit({LastExecuteExitCode}) {cmd}");
                return success;
            }
            catch (Exception e)
            {
                LastExecuteLog = "";
                LastExecuteErr = $"{e.GetType().Name} {cmd}\n{e.Message}";
                LastExecuteExitCode = -1;
                UnityEngine.Debug.LogException(e);
            }

            return false;
        }
    }
}
