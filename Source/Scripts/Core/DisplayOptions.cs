using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class DisplayOptions
	{
		/// <summary>
		/// use only in edit mode. use array to avoid unity null reference checking
		/// </summary>
		public static Transform[] defaultRoot;

		/// <summary>
		/// 
		/// </summary>
		public static HideFlags hideFlags = HideFlags.None;

		/// <summary>
		/// 
		/// </summary>
		public static void SetEditModeHideFlags()
		{
#if UNITY_5
	#if SHOW_HIERARCHY_EDIT_MODE
			hideFlags = HideFlags.DontSaveInEditor;
	#else
			hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
	#endif
#else
			hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
#endif
		}
	}
}
