using System;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        public Panel AddUIComponent(Type type)
        {
            return (Panel) RootEntity.GetComponent<UIRootComponents>().AddComponent(type);
        }

        public void RemoveUIComponent(Panel panel)
        {
            RootEntity.GetComponent<UIRootComponents>().RemoveComponent(panel);
        }

        public FsmController AddFsmComponent(Type type)
        {
            return (FsmController) RootEntity.GetComponent<FsmComponents>().AddComponent(type);
        }


        public void RemoveFsmComponent(FsmController fsm)
        {
            RootEntity.GetComponent<FsmComponents>().RemoveComponent(fsm);
        }

        public FsmController AddFsmChild(Type type)
        {
            return (FsmController) RootEntity.GetComponent<FsmComponents>().AddChild(type);
        }

        public void RemoveFsmChild(FsmController child)
        {
            RootEntity.GetComponent<FsmComponents>().RemoveChild(child);
        }
    }
}