using System;

namespace GameFrame.Runtime
{
    public interface IObjectPoolBase:IDisposable
    {
        void Update(float elapseSeconds, float realElapseSeconds);
    }
}