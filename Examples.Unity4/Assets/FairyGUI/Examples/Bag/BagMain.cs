using UnityEngine;
using FairyGUI;

/// <summary>
/// A game bag demo, demonstrated how to customize loader to load icons not in the UI package.
/// </summary>
public class BagMain : MonoBehaviour
{
	GComponent _mainView;
	BagWindow _bagWindow;

	void Start()
	{
		Application.targetFrameRate = 60;
		GRoot.inst.SetContentScaleFactor(1136, 640);
		Stage.inst.onKeyDown.Add(OnKeyDown);
		
		//Register custom loader class
		UIObjectFactory.SetLoaderExtension(typeof(MyGLoader));

		UIPackage.AddPackage("UI/Bag");

		_mainView = UIPackage.CreateObject("Bag", "Main").asCom;
		_mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
		_mainView.AddRelation(GRoot.inst, RelationType.Size);
		GRoot.inst.AddChild(_mainView);

		_bagWindow = new BagWindow();

		_mainView.GetChild("bagBtn").onClick.Add(() => { _bagWindow.Show(); });
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}