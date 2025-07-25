namespace GameFrame
{
    internal static class ComponentsIdidGenerator<T> where T : ECSComponent
    {
        private static int _Next = 0;
        public static int Next => _Next++;
    }

    public static class ComponentsID<T> where T : ECSComponent
    {
        public static readonly int TID = ComponentsIdidGenerator<T>.Next;
    }
}