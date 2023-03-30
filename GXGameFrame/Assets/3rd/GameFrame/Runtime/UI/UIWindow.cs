using System;

namespace GameFrame
{
    public class UIWindow : Entity, IStart, IUpdate, IClear
    {
        public override void Initialize()
        {
            base.Initialize();
            this.AddSystem<UIWindowSystem.UIWindowStartSystem>();
            this.AddSystem<UIWindowSystem.UIWindowUpdateSystem>();
            this.AddSystem<UIWindowSystem.UIWindowClearSystem>();
        }
        public UIViewBase UIBase;
    }
}