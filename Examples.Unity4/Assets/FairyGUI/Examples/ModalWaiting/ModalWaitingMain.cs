using System.Collections;
using UnityEngine;
using FairyGUI;

public class ModalWaitingMain : MonoBehaviour
{
	GComponent _mainView;
	Window4 _testWin;

	void Start()
	{
		Application.targetFrameRate = 60;
		GRoot.inst.SetContentScaleFactor(1136, 640);
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/ModalWaiting");
		UIConfig.globalModalWaiting = "ui://ModalWaiting/GlobalModalWaiting";
		UIConfig.windowModalWaiting = "ui://ModalWaiting/WindowModalWaiting";

		UIObjectFactory.SetPackageItemExtension("ui://ModalWaiting/GlobalModalWaiting", typeof(GlobalWaiting));

		_mainView = UIPackage.CreateObject("ModalWaiting", "Main").asCom;
		_mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
		_mainView.AddRelation(GRoot.inst, RelationType.Size);
		GRoot.inst.AddChild(_mainView);

		_testWin = new Window4();

		_mainView.GetChild("n0").onClick.Add(() => { _testWin.Show(); });

		StartCoroutine(WaitSomeTime());
	}

	IEnumerator WaitSomeTime()
	{
		GRoot.inst.ShowModalWait();

		yield return new WaitForSeconds(3);

		GRoot.inst.CloseModalWait();
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}