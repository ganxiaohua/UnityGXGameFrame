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
        //以下三个只会选择其中一个
        //全部包含
        private int[] m_AllOfIndices;

        //任意一个
        private int[] m_AnyOfIndices;

        //除了这个之外
        private int[] m_NoneOfIndices;

        //用于查看
        private string[] m_IndicesName;

        public static Matcher SetAllOfIndices(params int[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.m_AllOfIndices = new int[count];
#if UNITY_EDITOR
            matcher.m_IndicesName = new string[count];
#endif
            for (int i = 0; i < count; i++)
            {
                matcher.m_AllOfIndices[i] = snitiyHasCodes[i];
#if UNITY_EDITOR
                matcher.m_IndicesName[i] = Components.ComponentTypes[snitiyHasCodes[i]].Name;
#endif
            }

            return matcher;
        }

        public static Matcher SetAnyOfIndices(params int[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.m_AnyOfIndices = new int[count];
#if UNITY_EDITOR
            matcher.m_IndicesName = new string[count];
#endif
            for (int i = 0; i < count; i++)
            {
                matcher.m_AnyOfIndices[i] = snitiyHasCodes[i];
#if UNITY_EDITOR
                matcher.m_IndicesName[i] = Components.ComponentTypes[snitiyHasCodes[i]].Name;
#endif
            }

            return matcher;
        }

        public static Matcher SetNonefIndices(params int[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.m_NoneOfIndices = new int[count];
#if UNITY_EDITOR
            matcher.m_IndicesName = new string[count];
#endif
            for (int i = 0; i < count; i++)
            {
                matcher.m_NoneOfIndices[i] = snitiyHasCodes[i];
#if UNITY_EDITOR
                matcher.m_IndicesName[i] = Components.ComponentTypes[snitiyHasCodes[i]].Name;
#endif
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