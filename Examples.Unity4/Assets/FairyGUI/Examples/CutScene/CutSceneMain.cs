using System.Collections;
using UnityEngine;
using FairyGUI;

/// <summary>
/// Demonstrated the simple flow of a game.
/// </summary>
public class CutSceneMain : MonoBehaviour
{
	void Start()
	{
		Application.targetFrameRate = 60;
		GRoot.inst.SetContentScaleFactor(1136, 640);
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/CutScene");

		LevelManager.inst.Init();
		LevelManager.inst.LoadLevel("scene1");
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}