using System.Collections.Generic;

namespace GameFrame
{
    public class AssetVersion
    {
        public static readonly string Filename = $"{nameof(AssetVersion).ToLower()}.json";
        public int version;
        public long timestamp;
        public List<string> data = new List<string>();
    }
}