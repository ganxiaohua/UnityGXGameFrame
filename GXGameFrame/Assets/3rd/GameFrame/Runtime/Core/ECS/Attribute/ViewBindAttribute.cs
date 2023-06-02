using System;

namespace GameFrame
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewBindAttribute : Attribute
    {
        public ViewBindAttribute()
        {
        }

        public ViewBindAttribute(Type type)
        {
            BindType = type;
        }

        public Type BindType;
    }
}