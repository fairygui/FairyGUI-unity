using UnityEngine;
using FairyGUI;

public class PerspectiveMain : MonoBehaviour
{
    GList _list;

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        //GComponent g1 = GameObject.Find("UIPanel1").GetComponent<UIPanel>().ui;

        GComponent g2 = GameObject.Find("UIPanel2").GetComponent<UIPanel>().ui;
        _list = g2.GetChild("mailList").asList;
        _list.SetVirtual();
        _list.itemRenderer = (int index, GObject obj) => { obj.text = index + " Mail title here"; };
        _list.numItems = 20;
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}