using System;

namespace GameFrame.Runtime
{
    public partial interface EffComponent : IDisposable { }

    /// <summary>Runtime identity only. Use World.IsAlive to check the owning world and lifetime.</summary>
    public readonly struct EffEntityHandle : IEquatable<EffEntityHandle>, IContinuousID
    {
        public int Id { get; }
        public ulong Generation { get; }
        public bool IsValid => Generation != 0;
        int IContinuousID.ID => Id;

        public EffEntityHandle(int id, ulong generation)
        {
            Id = id;
            Generation = generation;
        }

        public bool Equals(EffEntityHandle other) => Id == other.Id && Generation == other.Generation;
        public override bool Equals(object obj) => obj is EffEntityHandle other && Equals(other);
        public override int GetHashCode() => unchecked((Id * 397) ^ Generation.GetHashCode());
        public static bool operator ==(EffEntityHandle left, EffEntityHandle right) => left.Equals(right);
        public static bool operator !=(EffEntityHandle left, EffEntityHandle right) => !left.Equals(right);
        public override string ToString() => $"Entity({Id}:{Generation})";
    }
}
