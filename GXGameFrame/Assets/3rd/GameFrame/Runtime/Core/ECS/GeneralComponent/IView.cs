using FairyGUI;

namespace GameFrame
{
    public interface IView:IReference
    {
        void Link(Entity ecsEntity,string path);
    }
    
    public interface IUIView
    {
        void Link(Entity ecsEntity,GObject root,bool isMainPanel);
    }
    
    
    public interface IEceView:IReference,IUpdateSystem
    {
        void Link(ECSEntity ecsEntity);
    }
}