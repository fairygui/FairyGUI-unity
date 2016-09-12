using System;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class StageEngine : MonoBehaviour
	{
		public int ObjectTotal;
		public int ObjectOnStage;

		void LateUpdate()
		{
			ObjectOnStage = Stage.inst.InternalUpdate();
			ObjectTotal = (int)DisplayObject._gInstanceCounter;
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
	}
}