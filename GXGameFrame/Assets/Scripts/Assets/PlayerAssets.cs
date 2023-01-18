using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAssets : ScriptableObject
{
    public static readonly string Filename = $"{nameof(PlayerAssets).ToLower()}.json";
    public List<string> data = new List<string>();
    public int version;

    private Dictionary<string, bool> dict = new Dictionary<string, bool>();

    public bool Contains(string key)
    {
        if (data.Count > 0 && dict.Count == 0)
        {
            foreach(var item in data)
            {
                if (dict.ContainsKey(item))
                    continue;
                dict.Add(item, true);
            }
        }
        return dict.ContainsKey(key);
    }

}