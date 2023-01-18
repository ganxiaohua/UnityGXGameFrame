using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class HideResourceFoldersScope : IDisposable
{
    List<string> m_TempDirectories = new List<string>();
    string m_TempDirName = "TempResourcesTestFolder";

    public HideResourceFoldersScope()
    {
        //Ensuring we have clean Resources folders
        foreach (var directory in AllResourcesDirectories())
        {
            string tempDirectory = directory.Replace("Resources", m_TempDirName);
            DirectoryUtility.DirectoryCopy(directory, tempDirectory, true);
            Directory.Delete(directory, true);
            m_TempDirectories.Add(tempDirectory);
        }
    }

    void IDisposable.Dispose()
    {
        //Cleanup
        foreach (string tempDir in m_TempDirectories)
        {
            string originalDirectory = tempDir.Replace(m_TempDirName, "Resources");
            if (Directory.Exists(originalDirectory))
                Directory.Delete(originalDirectory, true);
            DirectoryUtility.DirectoryCopy(tempDir, originalDirectory, true);
            AssetDatabase.DeleteAsset(tempDir);
        }
    }

    List<string> AllResourcesDirectories()
    {
        List<string> result = new List<string>();
        foreach (var resourcesDir in Directory.GetDirectories("Assets", "Resources", SearchOption.AllDirectories))
            result.Add(resourcesDir);
        return result;
    }
}
