using System.Collections;
using UnityEngine;
using FairyGUI;

public class ModalWaitingMain : MonoBehaviour
{
	GComponent _mainView;
	Window4 _testWin;

	void Awake()
	{
		UIPackage.AddPackage("UI/ModalWaiting");
		UIConfig.globalModalWaiting = UIPackage.GetItemURL("ModalWaiting", "GlobalModalWaiting");
		UIConfig.windowModalWaiting = UIPackage.GetItemURL("ModalWaiting", "WindowModalWaiting");

		UIObjectFactory.SetPackageItemExtension(UIPackage.GetItemURL("ModalWaiting", "GlobalModalWaiting"), typeof(GlobalWaiting));
	}

	void Start()
	{
		Application.targetFrameRate = 60;
		Stage.inst.onKeyDown.Add(OnKeyDown);

		_mainView = this.GetComponent<UIPanel>().ui;

		_testWin = new Window4();

		_mainView.GetChild("n0").onClick.Add(() => {  _testWin.Show(); });

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