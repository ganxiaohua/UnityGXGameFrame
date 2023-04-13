using System;

namespace GameFrame
{
    public class UIWindow : Entity, IStart, IUpdate, IClear,IShow,IHide
    {
        public UIViewBase UIBase;
    }
}