using System;

namespace GameFrame.Runtime
{
    public partial interface EffComponent : IDisposable { }


    public readonly struct EffEntity : IEquatable<EffEntity>, IContinuousID
    {
        public int Id { get; }
        public ulong Generation { get; }
        public bool IsValid => Generation != 0;
        int IContinuousID.ID => Id;

        public EffEntity(int id, ulong generation)
        {
            Id = id;
            Generation = generation;
        }

        public bool Equals(EffEntity other) => Id == other.Id && Generation == other.Generation;
        public override bool Equals(object obj) => obj is EffEntity other && Equals(other);
        public override int GetHashCode() => unchecked((Id * 397) ^ Generation.GetHashCode());
        public static bool operator ==(EffEntity left, EffEntity right) => left.Equals(right);
        public static bool operator !=(EffEntity left, EffEntity right) => !left.Equals(right);
        public override string ToString() => $"Entity({Id}:{Generation})";
    }
    
    
}
