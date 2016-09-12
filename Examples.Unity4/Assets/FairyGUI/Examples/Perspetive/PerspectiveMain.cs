using UnityEngine;
using FairyGUI;

public class PerspectiveMain : MonoBehaviour
{
	GComponent _mainView;
	GList _list;

	void Start()
	{
		Application.targetFrameRate = 60;
		GRoot.inst.SetContentScaleFactor(1136, 640);
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/Perspective");

		_mainView = UIPackage.CreateObject("Perspective", "Main").asCom;
		_mainView.AddRelation(GRoot.inst, RelationType.Size);
		GRoot.inst.AddChild(_mainView);

		//GComponent g1 = GameObject.Find("UIPanel1").GetComponent<UIPanel>().ui;

		GComponent g2 = GameObject.Find("UIPanel2").GetComponent<UIPanel>().ui;
		_list = g2.GetChild("mailList").asList;
		_list.SetVirtual();
		_list.itemRenderer = (int index, GObject obj) => { obj.text = index + " Mail title here"; };
		_list.numItems = 20;
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}