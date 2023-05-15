using System.IO;
using UnityEngine;

public class PlayerVersion
{
    public static readonly string Filename = $"{AddressablesHelper.GetPlatformName()}_{nameof(PlayerVersion)}";
    public string BundleVersion = "1.0.0";
    public int BundleVersionCode;
    public string UpdateURL;

    static PlayerVersion sVersion;
    public static PlayerVersion Version
    {
        get
        {
            if (sVersion == null)
            {
                var textAsset = Resources.Load<TextAsset>(Filename);
                if (textAsset != null)
                {
                    sVersion = JsonUtility.FromJson<PlayerVersion>(textAsset.text);
                }
                else
                {
                    sVersion = new PlayerVersion();
                }
            }
            return sVersion;
        }
    }

    public void Save()
    {
        var path = $"{Application.dataPath}/Resources/{Filename}.json";
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(path, JsonUtility.ToJson(this));
    }

}
