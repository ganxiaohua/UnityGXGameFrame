using System.Text;

namespace GameFrame.Runtime
{
    public static class UnitConversion
    {
        public static string PrettySeconds(this float seconds)
        {
            if (float.IsInfinity(seconds))
                return "INF";
            return $"{seconds:0.0}s";
        }

        public static string PrettyMemory(this long byteCount)
        {
            double kb = byteCount / 1024.0;
            if (kb < 1)
                return $"{byteCount}B";
            double mb = kb / 1024.0;
            if (mb < 1)
                return $"{kb:0.00}KB";
            double gb = mb / 1024.0;
            if (gb < 1)
                return $"{mb:0.00}MB";
            return $"{gb:0.00}GB";
        }

        public static StringBuilder AppendTab(this StringBuilder sb, int tabCount, string value)
        {
            sb.Append('\t', tabCount);
            sb.Append(value);
            return sb;
        }

        public static StringBuilder TrimBack(this StringBuilder sb, char c)
        {
            while (sb.Length > 0 && sb[sb.Length - 1] == c)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb;
        }
    }
}