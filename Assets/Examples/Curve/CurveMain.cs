using UnityEngine;
using FairyGUI;

public class CurveMain : MonoBehaviour
{
    GComponent _mainView;
    GList _list;

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPainter>().ui;
    
        _list = _mainView.GetChild("list").asList;
        _list.SetVirtualAndLoop();
        _list.itemRenderer = RenderListItem;
        _list.numItems = 5;
        _list.scrollPane.onScroll.Add(DoSpecialEffect);

        DoSpecialEffect();
    }

    void DoSpecialEffect()
    {
        //change the scale according to the distance to middle
        float midX = _list.scrollPane.posX + _list.viewWidth / 2;
        int cnt = _list.numChildren;
        for (int i = 0; i < cnt; i++)
        {
            GObject obj = _list.GetChildAt(i);
            float dist = Mathf.Abs(midX - obj.x - obj.width / 2);
            if (dist > obj.width) //no intersection
                obj.SetScale(1, 1);
            else
            {
                float ss = 1 + (1 - dist / obj.width) * 0.24f;
                obj.SetScale(ss, ss);
            }
        }
    }

    void RenderListItem(int index, GObject obj)
    {
        GButton item = (GButton)obj;
        item.SetPivot(0.5f, 0.5f);
        item.icon = UIPackage.GetItemURL("Curve", "n" + (index + 1));
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}