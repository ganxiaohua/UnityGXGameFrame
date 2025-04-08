using System;

namespace GameFrame
{
    public interface IObjectPoolBase:IDisposable
    {
        void Update(float elapseSeconds, float realElapseSeconds);
    }
}