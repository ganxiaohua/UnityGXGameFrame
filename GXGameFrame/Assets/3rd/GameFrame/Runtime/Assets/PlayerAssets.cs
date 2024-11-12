using System.Collections.Generic;
using UnityEngine;

public class PlayerAssets : ScriptableObject
{
    public static readonly string Filename = $"{nameof(PlayerAssets).ToLower()}.json";
    public List<string> dataList = new List<string>();
    public int Version;

    private Dictionary<string, bool> dict = new Dictionary<string, bool>();

    public bool Contains(string key)
    {
        if (dataList.Count > 0 && dict.Count == 0)
        {
            foreach (var item in dataList)
            {
                if (dict.ContainsKey(item))
                    continue;
                dict.Add(item, true);
            }
        }

        return dict.ContainsKey(key);
    }
}