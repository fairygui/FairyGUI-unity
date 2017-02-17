using System;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class StageEngine : MonoBehaviour
	{
		public int ObjectsOnStage;
		public int GraphicsOnStage;

		public static bool beingQuit;

		void LateUpdate()
		{
			Stage.inst.InternalUpdate();

			ObjectsOnStage = Stats.ObjectCount;
			GraphicsOnStage = Stats.GraphicsCount;
		}

#if FAIRYGUI_DLL || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
		void OnGUI()
		{
			Stage.inst.HandleGUIEvents(Event.current);
		}
#endif

#if !UNITY_5_4_OR_NEWER
		void OnLevelWasLoaded()
		{
			StageCamera.CheckMainCamera();
		}
#endif

		void OnApplicationQuit()
		{
			beingQuit = true;

			if (Application.isEditor)
				UIPackage.RemoveAllPackages(true);
		}
	}
}