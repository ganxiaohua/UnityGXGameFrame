using System;

namespace GameFrame
{
    public class UIShowComponent : Entity, IStart, IClear
    {
        public override void Initialize()
        {
            base.Initialize();
            UIBase = ((ParameterP1<UIViewBase>) Parameter).Param1;
            this.AddSystem<UIShowComponentSystem.UIShowComponentStartSystem>();
            this.AddSystem<UIShowComponentSystem.UIShowComponentClearSystem>();
        }

        public UIViewBase UIBase;
    }
}