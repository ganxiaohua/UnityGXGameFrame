using System.IO;
using UnityEngine;

public class PlayerVersion
{
    public static readonly string Filename = $"{AddressablesHelper.GetPlatformName()}_{nameof(PlayerVersion)}";
    public string bundleVersion = "1.0.0";
    public int bundleVersionCode;
    public string updateURL;

    static PlayerVersion s_version;
    public static PlayerVersion version
    {
        get
        {
            if (s_version == null)
            {
                var textAsset = Resources.Load<TextAsset>(Filename);
                if (textAsset != null)
                {
                    s_version = JsonUtility.FromJson<PlayerVersion>(textAsset.text);
                }
                else
                {
                    s_version = new PlayerVersion();
                }
            }
            return s_version;
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
