using System;

namespace GameFrame
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SystemBindAttribute : Attribute
    {
        public Type EnitiyType;
    }
}