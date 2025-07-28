namespace GameFrame.Runtime
{
    public static class EcsChangeEventState
    {
        public const ushort AddType = 0;
        public const ushort RemoveType = 1;
        public const ushort UpdateType = 2;

        public enum ChangeEventState : ushort
        {
            Add = 1 << AddType,
            Remove = 1 << RemoveType,
            Update = 1 << UpdateType,
            AddRemove = 1 << AddType | 1 << RemoveType,
            AddUpdate = 1 << AddType | 1 << UpdateType,
            RemoveUpdate = 1 << RemoveType | 1 << UpdateType,
            AddRemoveUpdate = 1 << AddType | 1 << UpdateType | 1 << RemoveType,
        }
    }
}