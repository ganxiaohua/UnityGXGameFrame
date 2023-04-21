using System;
using FairyGUI;
using GameFrame;
using UnityEngine;


public partial class UIHomeView : UIViewBase
{
    private UIHome m_UIHome;
    protected override void OnInit()
    {
        base.OnInit();
        contentPane = UIPackage.CreateObject("Home", "HomeWindow").asCom;
        m_UIHome = (UIHome)UIBase;
    }

    protected override void OnShown()
    {
        base.OnShown();
        // BtnDevelop.onClick.Add(m_UIHome.Cilck);
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
    
    
    
    
}