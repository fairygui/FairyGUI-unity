using System;
using System.Collections.Generic;
using FairyGUI;

public class Window1 : Window
{
    public Window1()
    {
    }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("Basics", "WindowA").asCom;
        this.Center();
    }

    override protected void OnShown()
    {
        GList list = this.contentPane.GetChild("n6").asList;
        list.RemoveChildrenToPool();

        for (int i = 0; i < 6; i++)
        {
            GButton item = list.AddItemFromPool().asButton;
            item.title = "" + i;
            item.icon = UIPackage.GetItemURL("Basics", "r4");
        }
    }
}
