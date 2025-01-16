using System;
using FairyGUI;

namespace GameFrame
{
    public interface IView:IDisposable
    {
        void Link(Entity ecsEntity,string path);
    }
    
    public interface IUIView
    {
        void Link(Entity ecsEntity,GObject root,bool isMainPanel);
    }
    
    
    public interface IEceView:IDisposable,IUpdateSystem
    {
        void Link(ECSEntity ecsEntity);
    }
}