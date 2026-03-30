using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        public Panel AddUIComponents(Type type)
        {
            return (Panel) RootEntity.GetComponent<UIRootComponents>().AddComponent(type);
        }

        public void RemoveUIComponents(Panel panel)
        {
            RootEntity.GetComponent<UIRootComponents>().RemoveComponent(panel);
        }

        public FsmController AddFsmComponents(Type type)
        {
            return (FsmController) RootEntity.GetComponent<FsmComponents>().AddComponent(type);
        }
        
        public FsmController AddFsmChild(Type type)
        {
            return (FsmController) RootEntity.GetComponent<FsmComponents>().AddChild(type);
        }
        
        public void RemoveFsmChild(FsmController child)
        {
              RootEntity.GetComponent<FsmComponents>().RemoveChild(child);
        }

        public void RemoveFsmComponents(FsmController fsm)
        {
            RootEntity.GetComponent<FsmComponents>().RemoveComponent(fsm);
        }
    }
}