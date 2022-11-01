using FairyGUI;
using UnityEngine;

public class BagWindow : Window
{
    GList _list;

    public BagWindow()
    {
    }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("Bag", "BagWin").asCom;
        this.Center();
        this.modal = true;

        _list = this.contentPane.GetChild("list").asList;
        _list.onClickItem.Add(__clickItem);
        _list.itemRenderer = RenderListItem;
        _list.numItems = 45;
    }

    void RenderListItem(int index, GObject obj)
    {
        GButton button = (GButton)obj;
        button.icon = "i" + UnityEngine.Random.Range(0, 10);
        button.title = "" + UnityEngine.Random.Range(0, 100);
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

    void __clickItem(EventContext context)
    {
        GButton item = (GButton)context.data;
        this.contentPane.GetChild("n11").asLoader.url = item.icon;
        this.contentPane.GetChild("n13").text = item.icon;
    }
}
