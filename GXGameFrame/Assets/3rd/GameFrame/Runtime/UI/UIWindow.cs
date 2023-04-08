using System;

namespace GameFrame
{
    public class UIWindow : Entity, IStart, IUpdate, IClear,IShow,IHide
    {
        public override void Initialize()
        {
            base.Initialize();
        }
        public UIViewBase UIBase;
    }
}