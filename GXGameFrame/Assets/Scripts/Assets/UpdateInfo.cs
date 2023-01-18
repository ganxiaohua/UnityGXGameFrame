using UnityEngine;

public class UpdateInfo : ScriptableObject
{
    public static readonly string Filename = $"{nameof(UpdateInfo).ToLower()}.json";
    public int version;
    public long timestamp;
}