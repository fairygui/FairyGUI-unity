using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("FairyGUI/UI Content Scaler")]
	public class UIContentScaler : MonoBehaviour
	{
		/// <summary>
		/// 
		/// </summary>
		public enum ScaleMode
		{
			ConstantPixelSize,
			ScaleWithScreenSize
		}

		/// <summary>
		/// 
		/// </summary>
		public enum ScreenMatchMode
		{
			MatchWidthOrHeight,
			MatchWidth,
			MatchHeight
		}

		/// <summary>
		/// 
		/// </summary>
		public ScaleMode scaleMode;

		/// <summary>
		/// 
		/// </summary>
		public ScreenMatchMode screenMatchMode;

		/// <summary>
		/// 
		/// </summary>
		public int designResolutionX;

		/// <summary>
		/// 
		/// </summary>
		public int designResolutionY;

		[System.NonSerialized]
		public static float scaleFactor = 1;

		[System.NonSerialized]
		bool _changed;

		void OnEnable()
		{
			if (Application.isPlaying)
			{
				if (scaleMode == ScaleMode.ScaleWithScreenSize)
					GRoot.inst.SetContentScaleFactor(designResolutionX, designResolutionY, screenMatchMode);
			}
			else //Screen width/height is not reliable in OnEnable in editmode
				_changed = true;
		}

		void Update()
		{
			if (_changed)
			{
				_changed = false;
				ApplyChange();
			}
		}

		void OnDestroy()
		{
			if (!Application.isPlaying)
				scaleFactor = 1;
		}

		//For UIContentScalerEditor Only, as the Screen.width/height is not correct in OnInspectorGUI
		/// <summary>
		/// 
		/// </summary>
		public void ApplyModifiedProperties()
		{
			_changed = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ApplyChange()
		{
			if (designResolutionX == 0 || designResolutionY == 0)
				return;

			int dx = designResolutionX;
			int dy = designResolutionY;
			if (Screen.width > Screen.height && dx < dy || Screen.width < Screen.height && dx > dy) 
			{
				//scale should not change when orientation change
				int tmp = dx;
				dx = dy;
				dy = tmp;
			}

			if (scaleMode == ScaleMode.ScaleWithScreenSize)
			{
				if (screenMatchMode == ScreenMatchMode.MatchWidthOrHeight)
				{
					float s1 = (float)Screen.width / dx;
					float s2 = (float)Screen.height / dy;
					scaleFactor = Mathf.Min(s1, s2);
				}
				else if (screenMatchMode == ScreenMatchMode.MatchWidth)
					scaleFactor = (float)Screen.width / dx;
				else
					scaleFactor = (float)Screen.height / dy;
			}
			else
				scaleFactor = 1;

			StageCamera.screenSizeVer++;
		}
	}
}
