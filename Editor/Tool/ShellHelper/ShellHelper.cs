using System;
using System.Diagnostics;
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
                using var process = new System.Diagnostics.Process
                {
                        StartInfo = new ProcessStartInfo(fileName, arguments)
                        {
                                WorkingDirectory = workingDirectory,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                                UseShellExecute = true,
                        }
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
                        if (logHandler.LogIsError(log))
                        {
                            errSB.AppendLine(log);
                            UnityEngine.Debug.LogError(log);
                        }
                        else
                        {
                            logSB.AppendLine(log);
                            UnityEngine.Debug.Log(log);
                        }
                    };
                    process.ErrorDataReceived += (_, args) =>
                    {
                        var err = args.Data;
                        if (logHandler.ErrorIsLog(err))
                        {
                            logSB.AppendLine(err);
                            UnityEngine.Debug.Log(err);
                        }
                        else
                        {
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
                UnityEngine.Debug.Log($"exit({LastExecuteExitCode}) {cmd}");
                return redirect ? string.IsNullOrEmpty(LastExecuteErr) : LastExecuteExitCode == 0;
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