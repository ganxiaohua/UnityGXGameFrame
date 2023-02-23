using System;

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
        //全部包含
        private int[] m_AllOfIndices;

        //任意一个
        private int[] m_AnyOfIndices;

        //除了这个之外
        private int[] m_NoneOfIndices;

        //除了这个之外
        private string[] m_IndicesName;
        public static Matcher SetAllOfIndices(params Type[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.m_AllOfIndices = new int[count];
            matcher.m_IndicesName = new string[count];
            for (int i = 0; i < count; i++)
            {
                matcher.m_AllOfIndices[i] = snitiyHasCodes[i].GetHashCode();
                matcher.m_IndicesName[i] = snitiyHasCodes[i].Name;
            }

            return matcher;
        }

        public static Matcher SetAnyOfIndices(params Type[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.m_AnyOfIndices = new int[count];
            matcher.m_IndicesName = new string[count];
            for (int i = 0; i < count; i++)
            {
                matcher.m_AnyOfIndices[i] = snitiyHasCodes[i].GetHashCode();
                matcher.m_IndicesName[i] = snitiyHasCodes[i].Name;
            }

            return matcher;
        }

        public static Matcher SetNonefIndices(params Type[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.m_NoneOfIndices = new int[count];
            matcher.m_IndicesName = new string[count];
            for (int i = 0; i < count; i++)
            {
                matcher.m_NoneOfIndices[i] = snitiyHasCodes[i].GetHashCode();
                matcher.m_IndicesName[i] = snitiyHasCodes[i].Name;
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
        }
    }
}