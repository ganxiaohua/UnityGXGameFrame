using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class Matcher : IDisposable, IEquatable<Matcher>
    {
        private int hash;
        private bool isHashCached = false;


        //全部包含
        public int[] AllOfIndices { get; private set; }

        //任意一个
        public int[] AnyOfIndices{ get; private set; }

        //除了这个之外
        public int[] NoneOfIndices{ get; private set; } 

        public HashSet<int> Indices { get; } = new HashSet<int>();

        //用于查看
        private string[] indicesName;

        private int[] Set(params int[] snitiyHasCodes)
        {
            Array.Sort(snitiyHasCodes);
            int count = snitiyHasCodes.Length;
            var arr = new int[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = snitiyHasCodes[i];
                Indices.Add(snitiyHasCodes[i]);
            }

            return arr;
        }

        public static Matcher SetAll(params int[] snitiyHasCodes)
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            matcher.AllOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public static Matcher SetAny(params int[] snitiyHasCodes)
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            matcher.AnyOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public static Matcher SetNoneOf(params int[] snitiyHasCodes)
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            matcher.NoneOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public Matcher All(params int[] snitiyHasCodes)
        {
            AllOfIndices = Set(snitiyHasCodes);
            return this;
        }

        public Matcher Any(params int[] snitiyHasCodes)
        {
            AnyOfIndices = Set(snitiyHasCodes);
            return this;
        }

        public Matcher NoneOf(params int[] snitiyHasCodes)
        {
            NoneOfIndices = Set(snitiyHasCodes);
            return this;
        }


        public static void ClearMatcher(Matcher matcher)
        {
            ReferencePool.Release(matcher);
        }

        public bool Match(EffEntity entity)
        {
            if (this.AllOfIndices != null && !entity.HasComponents(this.AllOfIndices) ||
                this.AnyOfIndices != null && !entity.HasAnyComponent(this.AnyOfIndices))
                return false;
            return this.NoneOfIndices == null || !entity.HasAnyComponent(this.NoneOfIndices);
        }


        public void Dispose()
        {
            NoneOfIndices = null;
            AllOfIndices = null;
            NoneOfIndices = null;
            Indices.Clear();
            isHashCached = false;
            hash = 0;
        }

        private int ApplyHash(int hash, int[] indices, int i1, int i2)
        {
            if (indices != null)
            {
                for (int index = 0; index < indices.Length; ++index)
                    hash ^= (indices[index] * i1);
                hash ^= (indices.Length * i2);
            }

            return hash;
        }

        public override int GetHashCode()
        {
            if (!this.isHashCached)
            {
                this.hash = ApplyHash(ApplyHash(ApplyHash(this.GetType().GetHashCode(), this.AllOfIndices, 3, 53), this.AnyOfIndices, 307, 367),
                    this.NoneOfIndices, 647, 683);
                this.isHashCached = true;
            }

            return this.hash;
        }

        public bool Equals(Matcher obj)
        {
            if (obj == null || obj.GetType() != this.GetType() || obj.GetHashCode() != this.GetHashCode())
                return false;
            return EqualIndices(obj.AllOfIndices, this.AllOfIndices) && EqualIndices(obj.AnyOfIndices, AnyOfIndices) &&
                   EqualIndices(obj.NoneOfIndices, this.NoneOfIndices);
        }

        public override bool Equals(object obj) => Equals((Matcher) obj);

        private bool EqualIndices(int[] i1, int[] i2)
        {
            if (i1 == null != (i2 == null))
                return false;
            if (i1 == null)
                return true;
            if (i1.Length != i2.Length)
                return false;
            for (int index = 0; index < i1.Length; ++index)
            {
                if (i1[index] != i2[index])
                    return false;
            }

            return true;
        }
    }
}