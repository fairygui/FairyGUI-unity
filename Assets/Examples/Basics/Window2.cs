using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

public class Window2 : Window
{
    public Window2()
    {
    }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("Basics", "WindowB").asCom;
        this.Center();
    }

    override protected void DoShowAnimation()
    {
        this.SetScale(0.1f, 0.1f);
        this.SetPivot(0.5f, 0.5f);
        this.TweenScale(new Vector2(1, 1), 0.3f).OnComplete(this.OnShown);
    }

    override protected void DoHideAnimation()
    {
        this.TweenScale(new Vector2(0.1f, 0.1f), 0.3f).OnComplete(this.HideImmediately);
    }

    override protected void OnShown()
    {
        contentPane.GetTransition("t1").Play();
    }

    override protected void OnHide()
    {
        contentPane.GetTransition("t1").Stop();
    }
}
