using System.Collections;
using UnityEngine;
using FairyGUI;

public class EmitNumbersMain : MonoBehaviour
{
	GComponent _mainView;
	Transform _npc1;
	Transform _npc2;
	bool _finished;

	void Start()
	{
		Application.targetFrameRate = 60;
		GRoot.inst.SetContentScaleFactor(1136, 640);
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/EmitNumbers");

		_mainView = UIPackage.CreateObject("EmitNumbers", "Main").asCom;
		_mainView.fairyBatching = true;
		_mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
		_mainView.AddRelation(GRoot.inst, RelationType.Size);
		GRoot.inst.AddChild(_mainView);

		_npc1 = GameObject.Find("npc1").transform;
		_npc2 = GameObject.Find("npc2").transform;

		StartCoroutine(RunTest());
	}

	void OnDisable()
	{
		_finished = true;
	}

	IEnumerator RunTest()
	{
		while (!_finished)
		{
			EmitManager.inst.Emit(_npc1, 0, UnityEngine.Random.Range(100, 100000), UnityEngine.Random.Range(0, 10) == 5);
			EmitManager.inst.Emit(_npc2, 1, UnityEngine.Random.Range(100, 100000), UnityEngine.Random.Range(0, 10) == 5);
			yield return new WaitForSeconds(0.3f);
		}
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}