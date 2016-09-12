using UnityEngine;
using FairyGUI;

public class RenderTextureMain : MonoBehaviour
{
	GComponent _mainView;
	Window3 _testWin;

	void Start()
	{
		Application.targetFrameRate = 60;
		GRoot.inst.SetContentScaleFactor(1136, 640);
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/RenderTexture");

		_mainView = UIPackage.CreateObject("RenderTexture", "Main").asCom;
		_mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
		_mainView.AddRelation(GRoot.inst, RelationType.Size);
		GRoot.inst.AddChild(_mainView);

		_testWin = new Window3();

		_mainView.GetChild("n2").onClick.Add(() => { _testWin.Show();  });
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}