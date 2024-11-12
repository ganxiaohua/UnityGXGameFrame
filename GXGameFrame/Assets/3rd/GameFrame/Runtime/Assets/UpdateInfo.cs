using UnityEngine;

public class UpdateInfo : ScriptableObject
{
    public static readonly string Filename = $"{nameof(UpdateInfo).ToLower()}.json";
    public int Version;
    public long Timestamp;
}