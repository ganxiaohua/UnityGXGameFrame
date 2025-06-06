namespace GameFrame
{
    public static class BytesSize
    {
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
    }
}