using FairyGUI;

public class Card : GButton
{
    GObject _back;
    GObject _front;

    public override void ConstructFromXML(FairyGUI.Utils.XML xml)
    {
        base.ConstructFromXML(xml);

        _back = GetChild("n0");
        _front = GetChild("icon");
        _front.visible = false;
    }

    public bool opened
    {
        get
        {
            return _front.visible;
        }

        set
        {
            GTween.Kill(this);

            _front.visible = value;
            _back.visible = !value;
        }
    }

    public void SetPerspective()
    {
        _front.displayObject.perspective = true;
        _back.displayObject.perspective = true;
    }

    public void Turn()
    {
        if (GTween.IsTweening(this))
            return;

        bool toOpen = !_front.visible;
        GTween.To(0, 180, 0.8f).SetTarget(this).SetEase(EaseType.QuadOut).OnUpdate(TurnInTween).SetUserData(toOpen);
    }

    void TurnInTween(GTweener tweener)
    {
        bool toOpen = (bool)tweener.userData;
        float v = tweener.value.x;
        if (toOpen)
        {
            _back.rotationY = v;
            _front.rotationY = -180 + v;
            if (v > 90)
            {
                _front.visible = true;
                _back.visible = false;
            }
        }
        else
        {
            _back.rotationY = -180 + v;
            _front.rotationY = v;
            if (v > 90)
            {
                _front.visible = false;
                _back.visible = true;
            }
        }
    }
}