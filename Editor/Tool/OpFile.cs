using System;
using System.IO;
using UnityEngine;

namespace GameFrame.Editor
{
    public static class OpFile
    {
        public static bool CreateDirectory(string path)
        {
            if (Directory.Exists(path))
                return false;
            if (File.Exists(path))
                return false;
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }

            return true;
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static string[] GetSubDirectories(string filePath)
        {
            return Directory.GetDirectories(filePath);
        }

        public static void DeleteFilesInDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(directoryPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException ex)
                {
                    Debug.Log($"删除文件 {file} 时出错：{ex.Message}");
                }
            }
        }
    }
}