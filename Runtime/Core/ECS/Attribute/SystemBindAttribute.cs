using System;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 凡是系统的都加上
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SystemBindAttribute : Attribute
    {
        public Type EntityType;
    }
}