using UnityEngine;
using FairyGUI;
public class CooldownMain : MonoBehaviour
{
    GComponent _mainView;

    GButton _btn0;
    GImage _mask0;
    float _time1;

    GButton _btn1;
    GImage _mask1;
    float _time2;

    void Start()
    {
        Application.targetFrameRate = 60;

        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.gameObject.GetComponent<UIPanel>().ui;

        _btn0 = _mainView.GetChild("b0").asButton;
        _btn0.icon = "Cooldown/k0";
        _time1 = 5;
        _mask0 = _btn0.GetChild("mask").asImage;

        _btn1 = _mainView.GetChild("b1").asButton;
        _btn1.icon = "Cooldown/k1";
        _time2 = 10;
        _mask1 = _btn1.GetChild("mask").asImage;

    }

    void Update()
    {
        _time1 -= Time.deltaTime;
        if (_time1 < 0)
            _time1 = 5;
        _mask0.fillAmount = 1 - (5 - _time1) / 5f;

        _time2 -= Time.deltaTime;
        if (_time2 < 0)
            _time2 = 10;
        _btn1.text = string.Empty + Mathf.RoundToInt(_time2);
        _mask1.fillAmount = 1 - (10 - _time2) / 10f;
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }

}