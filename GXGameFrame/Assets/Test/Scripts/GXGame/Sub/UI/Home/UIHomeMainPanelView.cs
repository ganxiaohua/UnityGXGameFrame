
using System;
using FairyGUI;
using GameFrame;
using UnityEngine;

namespace GXGame
{
    public partial class UIHomeMainPanelView : UIViewBase
    {
        private UIHomeMainPanel m_UIHomeMainPanel;
        protected override void OnInit()
        {
            base.OnInit();
            contentPane = UIPackage.CreateObject("Home", "HomeMainPanel").asCom;
            m_UIHomeMainPanel = (UIHomeMainPanel)UIBase;
            BtnAdventure.onClick.Add(m_UIHomeMainPanel.OpenCard);
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

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Clear()
        {
            Dispose();
        }

        public void OpenCardUI()
        {
            
        }
    }
}