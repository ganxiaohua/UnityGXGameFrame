using System;

namespace GameFrame.Runtime.WorldData
{
    public interface IWorldData : IDisposable
    {
        public void Initialize();
    }
}