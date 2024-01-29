using System.Collections.Generic;

namespace GameFrame
{
    public interface IAllOfMatcher
    {
    }

    public interface IAnyOfMatcher
    {
    }

    public interface INoneOfIndices
    {
    }

    public class Matcher : IAllOfMatcher, INoneOfIndices, IAnyOfMatcher, IReference
    {
        private static int ApplyHash(int hash, int[] indices, int i1, int i2)
        {
            if (indices != null)
            {
                for (int index = 0; index < indices.Length; ++index)
                    hash ^= indices[index] * i1;
                hash ^= indices.Length * i2;
            }

            return hash;
        }
        
        private int m_hash;

        //以下三个只会选择其中一个
        //全部包含
        private int[] m_AllOfIndices;

        //任意一个
        private int[] m_AnyOfIndices;

        //除了这个之外
        private int[] m_NoneOfIndices;


        public HashSet<int> Indices { get; } = new HashSet<int>();

        //用于查看
        private string[] m_IndicesName;

        private static Dictionary<int, Matcher> Matchers = new Dictionary<int, Matcher>();

        public static Matcher SetAllOfIndices(params int[] snitiyHasCodes)
        {
            int hascode = ApplyHash(typeof(Matcher).GetHashCode(), snitiyHasCodes, 3, 53);
            if (!Matchers.TryGetValue(hascode, out Matcher matcher))
            {
                int count = snitiyHasCodes.Length;
                matcher = CreateMatcher();
                matcher.m_AllOfIndices = new int[count];
#if UNITY_EDITOR
                matcher.m_IndicesName = new string[count];
#endif
                for (int i = 0; i < count; i++)
                {
                    matcher.m_AllOfIndices[i] = snitiyHasCodes[i];
                    matcher.Indices.Add(snitiyHasCodes[i]);
                }

                matcher.m_hash = hascode;
                Matchers.Add(hascode, matcher);
            }

            return matcher;
        }

        public static Matcher SetAnyOfIndices(params int[] snitiyHasCodes)
        {
            int hascode = ApplyHash(typeof(Matcher).GetHashCode(), snitiyHasCodes, 647, 683);
            if (!Matchers.TryGetValue(hascode, out Matcher matcher))
            {
                int count = snitiyHasCodes.Length;
                matcher = CreateMatcher();
                matcher.m_AnyOfIndices = new int[count];
#if UNITY_EDITOR
                matcher.m_IndicesName = new string[count];
#endif
                for (int i = 0; i < count; i++)
                {
                    matcher.m_AnyOfIndices[i] = snitiyHasCodes[i];
                    matcher.Indices.Add(snitiyHasCodes[i]);
                }

                Matchers.Add(hascode, matcher);
            }

            return matcher;
        }

        public static Matcher SetNonefIndices(params int[] snitiyHasCodes)
        {
            int hascode = ApplyHash(typeof(Matcher).GetHashCode(), snitiyHasCodes, 307, 367);
            if (!Matchers.TryGetValue(hascode, out Matcher matcher))
            {
                int count = snitiyHasCodes.Length;
                matcher = CreateMatcher();
                matcher.m_NoneOfIndices = new int[count];
#if UNITY_EDITOR
                matcher.m_IndicesName = new string[count];
#endif
                for (int i = 0; i < count; i++)
                {
                    matcher.m_NoneOfIndices[i] = snitiyHasCodes[i];
                    matcher.Indices.Add(snitiyHasCodes[i]);
                }
                Matchers.Add(hascode, matcher);
            }

            return matcher;
        }

        private static Matcher CreateMatcher()
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            return matcher;
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
            Matchers.Remove(this.m_hash);
            m_NoneOfIndices = null;
            m_AllOfIndices = null;
            m_NoneOfIndices = null;
        }
    }
}