using System;

namespace GameFrame
{
    public interface IObjectBase:IDisposable
    {
        public void Initialize();

        public void Recycle();

        public void Destroy();
    }
}