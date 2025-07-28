using System;

namespace GameFrame.Runtime
{
    public interface ISystem : IDisposable
    {
    }

    public interface ISystemCarryover : IDisposable
    {
        public object Carryover { get; set; }
    }
}