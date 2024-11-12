using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Matcher : IDisposable, IEquatable<Matcher>
    {
        private int hash;
        private bool isHashCached = false;


        //全部包含
        private int[] allOfIndices;

        //任意一个
        private int[] anyOfIndices;

        //除了这个之外
        private int[] noneOfIndices;

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
            matcher.allOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public static Matcher SetAny(params int[] snitiyHasCodes)
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            matcher.anyOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public static Matcher SetNoneOf(params int[] snitiyHasCodes)
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            matcher.noneOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public Matcher All(params int[] snitiyHasCodes)
        {
            allOfIndices = Set(snitiyHasCodes);
            return this;
        }

        public Matcher Any(params int[] snitiyHasCodes)
        {
            anyOfIndices = Set(snitiyHasCodes);
            return this;
        }

        public Matcher NoneOf(params int[] snitiyHasCodes)
        {
            noneOfIndices = Set(snitiyHasCodes);
            return this;
        }


        public static void RemoveMatcher(Matcher matcher)
        {
            ReferencePool.Release(matcher);
        }

        public bool Match(ECSEntity entity)
        {
            if (this.allOfIndices != null && !entity.HasComponents(this.allOfIndices) ||
                this.anyOfIndices != null && !entity.HasAnyComponent(this.anyOfIndices))
                return false;
            return this.noneOfIndices == null || !entity.HasAnyComponent(this.noneOfIndices);
        }


        public void Dispose()
        {
            noneOfIndices = null;
            allOfIndices = null;
            noneOfIndices = null;
            Indices.Clear();
            isHashCached = false;
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
                this.hash = ApplyHash(ApplyHash(ApplyHash(this.GetType().GetHashCode(), this.allOfIndices, 3, 53), this.anyOfIndices, 307, 367),
                    this.noneOfIndices, 647, 683);
                this.isHashCached = true;
            }

            return this.hash;
        }

        public bool Equals(Matcher obj)
        {
            if (obj == null || obj.GetType() != this.GetType() || obj.GetHashCode() != this.GetHashCode())
                return false;
            Matcher matcher = obj;
            return equalIndices(matcher.allOfIndices, this.allOfIndices) && equalIndices(matcher.anyOfIndices, anyOfIndices) &&
                   equalIndices(matcher.noneOfIndices, this.noneOfIndices);
        }
        
        public override bool Equals(object obj) => Equals((Matcher)obj);

        private bool equalIndices(int[] i1, int[] i2)
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