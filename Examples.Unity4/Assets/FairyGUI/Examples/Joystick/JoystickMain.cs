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
		GRoot.inst.SetContentScaleFactor(1136, 640);
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/Joystick");

		_mainView = UIPackage.CreateObject("Joystick", "Main").asCom;
		_mainView.fairyBatching = true;
		_mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
		_mainView.AddRelation(GRoot.inst, RelationType.Size);
		GRoot.inst.AddChild(_mainView);

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