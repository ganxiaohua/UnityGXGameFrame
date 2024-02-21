using System.IO;
using UnityEngine;

namespace GameFrame.Editor
{
    public static class OpFile
    {
        public static void DeleteFilesInDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.Log($"目录 {directoryPath} 不存在！");
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