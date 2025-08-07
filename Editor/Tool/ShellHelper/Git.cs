using System.IO;

namespace GameFrame.Editor
{
    public static class Git
    {
        private class GitLogHandler : IShellLogHandler
        {
            public bool LogIsError(string log)
            {
                if (log.Contains("Changes not staged for commit"))
                    return true;
                return false;
            }

            public bool ErrorIsLog(string err)
            {
                if (string.IsNullOrEmpty(err))
                    return true;
                if (err.StartsWith("Cloning into "))
                    return true;
                if (err.StartsWith("Everything up-to-date"))
                    return true;
                if (err.StartsWith("remote: "))
                    return true;
                if (err.StartsWith("To "))
                    return true;
                if (err.StartsWith("   "))
                    return true;
                if (err.StartsWith("warning: "))
                    return true;
                if (err.StartsWith("The file will have its original line endings in your working directory"))
                    return true;
                if (err.StartsWith("From "))
                    return true;
                if (err.StartsWith(" * [new branch]"))
                    return true;
                if (err.StartsWith("Updating files: "))
                    return true;
                if (err.StartsWith("Switched to"))
                    return true;
                if (err.StartsWith("Reset branch"))
                    return true;
                if (err.StartsWith("git: 'credential-manager-core'"))
                    return true;
                if (err.StartsWith("Fetching submodule"))
                    return true;
                if (err.StartsWith("Filtering content:"))
                    return true;
                return false;
            }
        }

        public static bool Clone(string url, string destDirectory)
        {
            destDirectory = Path.GetFullPath(destDirectory);
            return Execute($"clone {url} {Path.GetFileName(destDirectory)}", Path.GetDirectoryName(destDirectory));
        }

        public static bool Pull(string destDirectory)
        {
            return Execute("pull", destDirectory);
        }

        public static bool Add(string destDirectory, string file)
        {
            return Execute($"add {file}", destDirectory);
        }

        public static bool Commit(string destDirectory, string message, bool addDirtys = false)
        {
            if (addDirtys)
                return Execute($"commit -am \"{message}\"", destDirectory);
            else
                return Execute($"commit -m \"{message}\"", destDirectory);
        }

        public static bool Push(string destDirectory, string branch)
        {
            return Execute($"push origin {branch}", destDirectory);
        }

        public static bool Reset(string destDirectory, bool hard = false, string branch = "HEAD")
        {
            if (hard)
                return Execute($"reset --hard {branch}", destDirectory);
            else
                return Execute($"reset {branch}", destDirectory);
        }

        public static bool Clean(string destDirectory)
        {
            return Execute("clean -df", destDirectory);
        }

        public static bool Execute(string arguments, string destDirectory)
        {
            return ShellHelper.Start("git.exe", arguments, destDirectory, new GitLogHandler());
        }
    }
}