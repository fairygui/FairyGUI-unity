using UnityEngine;
using FairyGUI;

public class VirtualListMain : MonoBehaviour
{
    GComponent _mainView;
    GList _list;

    void Awake()
    {
        UIPackage.AddPackage("UI/VirtualList");
        UIObjectFactory.SetPackageItemExtension("ui://VirtualList/mailItem", typeof(MailItem));
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPanel>().ui;
        _mainView.GetChild("n6").onClick.Add(() => { _list.AddSelection(500, true); });
        _mainView.GetChild("n7").onClick.Add(() => { _list.scrollPane.ScrollTop(); });
        _mainView.GetChild("n8").onClick.Add(() => { _list.scrollPane.ScrollBottom(); });

        _list = _mainView.GetChild("mailList").asList;
        _list.SetVirtual();

        _list.itemRenderer = RenderListItem;
        _list.numItems = 1000;
    }

    void RenderListItem(int index, GObject obj)
    {
        MailItem item = (MailItem)obj;
        item.setFetched(index % 3 == 0);
        item.setRead(index % 2 == 0);
        item.setTime("5 Nov 2015 16:24:33");
        item.title = index + " Mail title here";
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}