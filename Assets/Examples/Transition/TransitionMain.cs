using UnityEngine;
using FairyGUI;

public class TransitionMain : MonoBehaviour
{
    GComponent _mainView;
    GGroup _btnGroup;
    GComponent _g1;
    GComponent _g2;
    GComponent _g3;
    GComponent _g4;
    GComponent _g5;
    GComponent _g6;

    float _startValue;
    float _endValue;

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPanel>().ui;

        _btnGroup = _mainView.GetChild("g0").asGroup;

        _g1 = UIPackage.CreateObject("Transition", "BOSS").asCom;
        _g2 = UIPackage.CreateObject("Transition", "BOSS_SKILL").asCom;
        _g3 = UIPackage.CreateObject("Transition", "TRAP").asCom;

        _g4 = UIPackage.CreateObject("Transition", "GoodHit").asCom;
        _g5 = UIPackage.CreateObject("Transition", "PowerUp").asCom;
        _g5.GetTransition("t0").SetHook("play_num_now", __playNum);

        _g6 = UIPackage.CreateObject("Transition", "PathDemo").asCom;

        _mainView.GetChild("btn0").onClick.Add(() => { __play(_g1); });
        _mainView.GetChild("btn1").onClick.Add(() => { __play(_g2); });
        _mainView.GetChild("btn2").onClick.Add(() => { __play(_g3); });
        _mainView.GetChild("btn3").onClick.Add(__play4);
        _mainView.GetChild("btn4").onClick.Add(__play5);
        _mainView.GetChild("btn5").onClick.Add(() => { __play(_g6); });
    }

    void __play(GComponent target)
    {
        _btnGroup.visible = false;
        GRoot.inst.AddChild(target);
        Transition t = target.GetTransition("t0");

        t.Play(() =>
        {
            _btnGroup.visible = true;
            GRoot.inst.RemoveChild(target);
        });
    }

    void __play4()
    {
        _btnGroup.visible = false;
        _g4.x = GRoot.inst.width - _g4.width - 20;
        _g4.y = 100;
        GRoot.inst.AddChild(_g4);
        Transition t = _g4.GetTransition("t0");
        t.Play(3, 0, () =>
        {
            _btnGroup.visible = true;
            GRoot.inst.RemoveChild(_g4);
        });
    }

    void __play5()
    {
        _btnGroup.visible = false;
        _g5.x = 20;
        _g5.y = GRoot.inst.height - _g5.height - 100;
        GRoot.inst.AddChild(_g5);
        Transition t = _g5.GetTransition("t0");
        _startValue = 10000;
        int add = UnityEngine.Random.Range(1000, 3000);
        _endValue = _startValue + add;
        _g5.GetChild("value").text = "" + _startValue;
        _g5.GetChild("add_value").text = "" + add;
        t.Play(() =>
        {
            _btnGroup.visible = true;
            GRoot.inst.RemoveChild(_g5);
        });
    }

    void __playNum()
    {
        GTween.To(_startValue, _endValue, 0.3f).SetEase(EaseType.Linear)
            .OnUpdate((GTweener tweener) => { _g5.GetChild("value").text = "" + Mathf.Floor(tweener.value.x); });

    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}