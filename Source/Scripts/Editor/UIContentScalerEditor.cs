using UnityEngine;
using UnityEditor;
using FairyGUI;

namespace FairyGUIEditor
{
	/// <summary>
	/// 
	/// </summary>
	[CustomEditor(typeof(UIContentScaler))]
	public class UIContentScalerEditor : Editor
	{
		string[] propertyToExclude;
		
		void OnEnable()
		{
			propertyToExclude = new string[] { "m_Script" };
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawPropertiesExcluding(serializedObject, propertyToExclude);

			if (serializedObject.ApplyModifiedProperties())
				(target as UIContentScaler).ApplyModifiedProperties();
		}
	}
}
