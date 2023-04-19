using System;
using FairyGUI;

namespace GameFrame
{
    public class UIViewBase : Window, IView
    {
        protected IEntity m_UIBase;


        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject("Home", "HomeWindow").asCom;
            base.OnInit();
        }

        protected override void OnShown()
        {
            base.OnShown();
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void DoShowAnimation()
        {
            base.DoShowAnimation();
        }

        protected override void DoHideAnimation()
        {
            base.DoHideAnimation();
            //等待动画播放完毕之后发送一个事件
            EventManager.Instance.Send<UICloseEvent, Type>(m_UIBase.GetType());
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Clear()
        {
            Dispose();
        }

        public void Link(Entity uiBase, string path)
        {
            m_UIBase = uiBase;
        }
    }
}