using System;

namespace GameFrame
{
    /// <summary>
    /// 表示这个组件需要绑定和视觉相关的参数
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewBindAttribute : Attribute
    {
        public Type BindType;
        public ViewBindAttribute(Type type)
        {
            BindType = type;
        }
        
    }
}