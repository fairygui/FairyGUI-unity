using FairyGUI;
using UnityEngine;

public class ScrollPaneHeader : GComponent
{
    Controller _c1;

    public override void ConstructFromXML(FairyGUI.Utils.XML xml)
    {
        base.ConstructFromXML(xml);

        _c1 = this.GetController("c1");

        this.onSizeChanged.Add(OnSizeChanged);
    }

    void OnSizeChanged()
    {
        if (_c1.selectedIndex == 2 || _c1.selectedIndex == 3)
            return;

        if (this.height > this.sourceHeight)
            _c1.selectedIndex = 1;
        else
            _c1.selectedIndex = 0;
    }

    public bool ReadyToRefresh
    {
        get { return _c1.selectedIndex == 1; }
    }

    public void SetRefreshStatus(int value)
    {
        _c1.selectedIndex = value;
    }
}
