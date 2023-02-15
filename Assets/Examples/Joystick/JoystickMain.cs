using UnityEngine;
using FairyGUI;

public class JoystickMain : MonoBehaviour
{
    GComponent _mainView;
    GTextField _text;
    JoystickModule _joystick;

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPanel>().ui;

        _text = _mainView.GetChild("n9").asTextField;

        _joystick = new JoystickModule(_mainView);
        _joystick.onMove.Add(__joystickMove);
        _joystick.onEnd.Add(__joystickEnd);
    }

    void __joystickMove(EventContext context)
    {
        float degree = (float)context.data;
        _text.text = "" + degree;
    }

    void __joystickEnd()
    {
        _text.text = "";
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}