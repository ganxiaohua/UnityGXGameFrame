using UnityEngine;

namespace GameFrame
{
    public interface IView:IReference
    {
        void Link(ECSEntity ecsEntity,string path);
    }
}