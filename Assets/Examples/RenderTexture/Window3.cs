using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

public class Window3 : Window
{
    RenderImage _renderImage;

    public Window3()
    {
    }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("RenderTexture", "TestWin").asCom;
        this.SetXY(200, 50);

        _renderImage = new RenderImage(contentPane.GetChild("holder").asGraph);
        //RenderImage是不透明的，可以设置最多两张图片作为背景图
        _renderImage.SetBackground(contentPane.GetChild("frame").asCom.GetChild("n0"), contentPane.GetChild("n20"));

        contentPane.GetChild("btnLeft").onTouchBegin.Add(__clickLeft);
        contentPane.GetChild("btnRight").onTouchBegin.Add(__clickRight);
    }

    override protected void OnShown()
    {
        _renderImage.LoadModel("Role/npc");
        _renderImage.modelRoot.localPosition = new Vector3(0, -1.0f, 5f);
        _renderImage.modelRoot.localScale = new Vector3(1, 1, 1);
        _renderImage.modelRoot.localRotation = Quaternion.Euler(0, 120, 0);
    }

    void __clickLeft()
    {
        _renderImage.StartRotate(-2);
        Stage.inst.onTouchEnd.Add(__touchEnd);
    }

    void __clickRight()
    {
        _renderImage.StartRotate(2);
        Stage.inst.onTouchEnd.Add(__touchEnd);
    }

    void __touchEnd()
    {
        _renderImage.StopRotate();
        Stage.inst.onTouchEnd.Remove(__touchEnd);
    }
}
