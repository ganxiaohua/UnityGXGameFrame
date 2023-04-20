using System;
using FairyGUI;
using GameFrame;
using UnityEngine;


public partial class UIViewHome : UIViewBase
{
    private UIWindow UIWindow;
    protected override void OnInit()
    {
        base.OnInit();
        contentPane = UIPackage.CreateObject("Home", "HomeWindow").asCom;
        UIWindow = (UIWindow)UIBase;
    }

    protected override void OnShown()
    {
        base.OnShown();
        BtnDevelop.onClick.Add(UIWindow.Cilck);
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