namespace GameFrame.Runtime.Runtime.SH
{
    internal static class CapabilityIDGenerator<TUpdateMode>
    {
        private static int _Next = 0;
        public static int Next => _Next++;
    }

    public static class CapabilityID<T,TUpdateMode> 
    {
        public static readonly int TID = CapabilityIDGenerator<TUpdateMode>.Next;
    }
}