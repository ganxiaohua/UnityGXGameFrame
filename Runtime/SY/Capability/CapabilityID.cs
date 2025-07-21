namespace SH.GameFrame
{
    internal static class CapabilityIDGenerator<TRoot>
    {
        private static ushort _Next = 0;
        public static uint Next => _Next++;
    }

    public static class CapabilityID<T, TRoot> 
    {
        public static readonly uint TID = CapabilityIDGenerator<TRoot>.Next;
    }
}