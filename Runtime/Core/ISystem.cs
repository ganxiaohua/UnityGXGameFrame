using System;

namespace GameFrame
{
    public interface ISystem : IDisposable
    {
    }

    public interface ISystemCarryover : IDisposable
    {
        public object Carryover { get; set; }
    }
}