using System;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 指定绑定的ecs实体类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AssignBindAttribute:Attribute
    {
        public AssignBindAttribute(Type type)
        {
            BindType = type;
        }
        public Type BindType;
    }
}