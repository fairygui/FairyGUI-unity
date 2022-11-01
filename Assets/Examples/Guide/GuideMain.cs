using UnityEngine;
using FairyGUI;

public class GuideMain : MonoBehaviour
{
    GComponent _mainView;
    GComponent _guideLayer;

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPanel>().ui;

        _guideLayer = UIPackage.CreateObject("Guide", "GuideLayer").asCom;
        _guideLayer.SetSize(GRoot.inst.width, GRoot.inst.height);
        _guideLayer.AddRelation(GRoot.inst, RelationType.Size);

        GObject bagBtn = _mainView.GetChild("bagBtn");
        bagBtn.onClick.Add(() =>
        {
            _guideLayer.RemoveFromParent();
        });

        _mainView.GetChild("n2").onClick.Add(() =>
        {
            GRoot.inst.AddChild(_guideLayer); //!!Before using TransformRect(or GlobalToLocal), the object must be added first
            Rect rect = bagBtn.TransformRect(new Rect(0, 0, bagBtn.width, bagBtn.height), _guideLayer);

            GObject window = _guideLayer.GetChild("window");
            window.size = new Vector2((int)rect.size.x, (int)rect.size.y);
            window.TweenMove(new Vector2((int)rect.position.x, (int)rect.position.y), 0.5f);
        });
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}