using UnityEngine;
using FairyGUI;

public class ParticlesMain : MonoBehaviour
{
	GComponent _mainView;

	void Start()
	{
		Application.targetFrameRate = 60;
		GRoot.inst.SetContentScaleFactor(1136, 640);
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/Particles");

		UIObjectFactory.SetPackageItemExtension("ui://Particles/CoolComponent", typeof(CoolComponent));

		_mainView = UIPackage.CreateObject("Particles", "Main").asCom;
		_mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
		_mainView.AddRelation(GRoot.inst, RelationType.Size);
		GRoot.inst.AddChild(_mainView);

		Object prefab = Resources.Load("Flame");
		GameObject go = (GameObject)Object.Instantiate(prefab);
		_mainView.GetChild("holder").asGraph.SetNativeObject(new GoWrapper(go));

		_mainView.GetChild("c0").draggable = true;
		_mainView.GetChild("c1").draggable = true;
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}