
using System;
using FairyGUI;
using GameFrame;
using UnityEngine;

namespace GXGame
{
    public partial class UICardWindowView : UIViewBase
    {
        private UICardWindow m_UICardWindow;
        protected override void OnInit()
        {
            base.OnInit();
            contentPane = UIPackage.CreateObject("Card", "CardWindow").asCom;
            m_UICardWindow = (UICardWindow)UIBase;
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
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds,realElapseSeconds);
        }

        public override void Clear()
        {
            
            base.Clear();
        }
    }
}