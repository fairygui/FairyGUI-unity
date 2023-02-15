using UnityEngine;
using FairyGUI;

public class ExtensionMain : MonoBehaviour
{
    GComponent _mainView;
    GList _list;

    void Awake()
    {
        UIPackage.AddPackage("UI/Extension");
        UIObjectFactory.SetPackageItemExtension("ui://Extension/mailItem", typeof(MailItem));

    }

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPanel>().ui;

        _list = _mainView.GetChild("mailList").asList;
        for (int i = 0; i < 10; i++)
        {
            MailItem item = (MailItem)_list.AddItemFromPool();
            item.setFetched(i % 3 == 0);
            item.setRead(i % 2 == 0);
            item.setTime("5 Nov 2015 16:24:33");
            item.title = "Mail title here";
        }

        _list.EnsureBoundsCorrect();
        float delay = 0f;
        for (int i = 0; i < 10; i++)
        {
            MailItem item = (MailItem)_list.GetChildAt(i);
            if (_list.IsChildInView(item))
            {
                item.PlayEffect(delay);
                delay += 0.2f;
            }
            else
                break;
        }
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}