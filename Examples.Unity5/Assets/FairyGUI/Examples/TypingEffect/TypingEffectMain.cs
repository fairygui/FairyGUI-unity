using UnityEngine;
using FairyGUI;
using DG.Tweening;

public class TypingEffectMain : MonoBehaviour
{
	GComponent _mainView;
	TypingEffect _te1;
	TypingEffect _te2;

	void Awake()
	{
		Application.targetFrameRate = 60;
		Stage.inst.onKeyDown.Add(OnKeyDown);
	}

	void Start()
	{
		_mainView = this.GetComponent<UIPanel>().ui;

		_te1 = new TypingEffect(_mainView.GetChild("n5").asTextField);
		_te1.Start();
		Timers.inst.StartCoroutine(_te1.Print(0.050f));

		_te2 = new TypingEffect(_mainView.GetChild("n12").asTextField);
		_te2.Start();
	}

	void Update()
	{
		if (_te2 != null)
		{
			if (!_te2.Print())
				_te2 = null;
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