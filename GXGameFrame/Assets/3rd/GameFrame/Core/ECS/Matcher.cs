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

    public class Matcher : IAllOfMatcher,INoneOfIndices, IAnyOfMatcher, IReference
    {
        //全部包含
        private int[] AllOfIndices;
        //任意一个
        private int[] AnyOfIndices;
        //除了这个之外
        private int[] NoneOfIndices; 
        
        public static Matcher SetAllOfIndices(params Type[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.AllOfIndices = new int[count];
            for (int i = 0; i <count ; i++)
            {
                matcher.AllOfIndices[i] = snitiyHasCodes[i].GetHashCode();
            }
            return matcher;
        }
        
        public static Matcher SetAnyOfIndices(params Type[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.AnyOfIndices = new int[count];
            for (int i = 0; i <count ; i++)
            {
                matcher.AnyOfIndices[i] = snitiyHasCodes[i].GetHashCode();
            }
            return matcher;
        }
        
        public static Matcher SetNonefIndices(params Type[] snitiyHasCodes)
        {
            int count = snitiyHasCodes.Length;
            Matcher matcher = CreateMatcher();
            matcher.NoneOfIndices = new int[count];
            for (int i = 0; i <count ; i++)
            {
                matcher.NoneOfIndices[i] = snitiyHasCodes[i].GetHashCode();
            }
            return matcher;
        }

        private static Matcher CreateMatcher()
        {
            Matcher matcher = ReferencePool.Acquire<Matcher>();
            return matcher;
        }

        public bool Match(Entity entity)
        {
            if (this.AllOfIndices != null && !entity.HasComponents(this.AllOfIndices) || this.AnyOfIndices != null && !entity.HasAnyComponent(this.AnyOfIndices))
                return false;
            return this.NoneOfIndices == null || !entity.HasAnyComponent(this.NoneOfIndices);
        }
        


        public void Clear()
        {
        }
    }
}