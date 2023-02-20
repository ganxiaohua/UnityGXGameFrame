using UnityEngine;

namespace GameFrame
{
    public interface IView:IReference
    {
        void Link(Entity ecsEntity,string path);
    }
    public interface IEceView:IReference
    {
        void Link(ECSEntity ecsEntity,string path);
    }
}