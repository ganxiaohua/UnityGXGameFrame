using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Matcher : IReference, IEquatable<Matcher>
    {
        private int m_Hash;
        private bool m_IsHashCached = false;


        //全部包含
        private int[] m_AllOfIndices;

        //任意一个
        private int[] m_AnyOfIndices;

        //除了这个之外
        private int[] m_NoneOfIndices;

        public HashSet<int> Indices { get; } = new HashSet<int>();

        //用于查看
        private string[] m_IndicesName;

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
            matcher.m_AllOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public static Matcher SetAny(params int[] snitiyHasCodes)
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            matcher.m_AnyOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public static Matcher SetNoneOf(params int[] snitiyHasCodes)
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            matcher.m_NoneOfIndices = matcher.Set(snitiyHasCodes);
            return matcher;
        }

        public Matcher All(params int[] snitiyHasCodes)
        {
            m_AllOfIndices = Set(snitiyHasCodes);
            return this;
        }

        public Matcher Any(params int[] snitiyHasCodes)
        {
            m_AnyOfIndices = Set(snitiyHasCodes);
            return this;
        }

        public Matcher NoneOf(params int[] snitiyHasCodes)
        {
            m_NoneOfIndices = Set(snitiyHasCodes);
            return this;
        }


        public static void RemoveMatcher(Matcher matcher)
        {
            ReferencePool.Release(matcher);
        }

        public bool Match(ECSEntity entity)
        {
            if (this.m_AllOfIndices != null && !entity.HasComponents(this.m_AllOfIndices) ||
                this.m_AnyOfIndices != null && !entity.HasAnyComponent(this.m_AnyOfIndices))
                return false;
            return this.m_NoneOfIndices == null || !entity.HasAnyComponent(this.m_NoneOfIndices);
        }


        public void Clear()
        {
            m_NoneOfIndices = null;
            m_AllOfIndices = null;
            m_NoneOfIndices = null;
            Indices.Clear();
            m_IsHashCached = false;
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
            if (!this.m_IsHashCached)
            {
                this.m_Hash = ApplyHash(ApplyHash(ApplyHash(this.GetType().GetHashCode(), this.m_AllOfIndices, 3, 53), this.m_AnyOfIndices, 307, 367),
                    this.m_NoneOfIndices, 647, 683);
                this.m_IsHashCached = true;
            }

            return this.m_Hash;
        }

        public bool Equals(Matcher obj)
        {
            if (obj == null || obj.GetType() != this.GetType() || obj.GetHashCode() != this.GetHashCode())
                return false;
            Matcher matcher = obj;
            return equalIndices(matcher.m_AllOfIndices, this.m_AllOfIndices) && equalIndices(matcher.m_AnyOfIndices, m_AnyOfIndices) &&
                   equalIndices(matcher.m_NoneOfIndices, this.m_NoneOfIndices);
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