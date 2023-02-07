using System;

namespace GameFrame
{
    public class Matcher:IReference
    {
        private int[] m_EnitiyHasCode;
        
        public int[] EnitiyHasCode => this.m_EnitiyHasCode;
        
        
        public static void CreateMatcher(int[] snitiyHasCodes)
        {
            Matcher matcher =  ReferencePool.Acquire<Matcher>();
            matcher.m_EnitiyHasCode = snitiyHasCodes;
        }
        
        
        public void Clear()
        {
            
        }
    }
}