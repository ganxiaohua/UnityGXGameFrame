using System;

namespace GameFrame.Runtime
{
    public interface IObjectBase:IDisposable
    {
        public void Initialize();

        public void Recycle();

        public void Destroy();
    }
}