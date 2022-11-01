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
        SerializedProperty scaleMode;
        SerializedProperty screenMatchMode;
        SerializedProperty designResolutionX;
        SerializedProperty designResolutionY;
        SerializedProperty fallbackScreenDPI;
        SerializedProperty defaultSpriteDPI;
        SerializedProperty constantScaleFactor;
        SerializedProperty ignoreOrientation;

        string[] propertyToExclude;

        void OnEnable()
        {
            scaleMode = serializedObject.FindProperty("scaleMode");
            screenMatchMode = serializedObject.FindProperty("screenMatchMode");
            designResolutionX = serializedObject.FindProperty("designResolutionX");
            designResolutionY = serializedObject.FindProperty("designResolutionY");
            fallbackScreenDPI = serializedObject.FindProperty("fallbackScreenDPI");
            defaultSpriteDPI = serializedObject.FindProperty("defaultSpriteDPI");
            constantScaleFactor = serializedObject.FindProperty("constantScaleFactor");
            ignoreOrientation = serializedObject.FindProperty("ignoreOrientation");

            propertyToExclude = new string[] { "m_Script", "scaleMode", "screenMatchMode", "designResolutionX", "designResolutionY",
                    "fallbackScreenDPI", "defaultSpriteDPI", "constantScaleFactor", "ignoreOrientation"};
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, propertyToExclude);

            EditorGUILayout.PropertyField(scaleMode);
            if ((UIContentScaler.ScaleMode)scaleMode.enumValueIndex == UIContentScaler.ScaleMode.ScaleWithScreenSize)
            {
                EditorGUILayout.PropertyField(designResolutionX);
                EditorGUILayout.PropertyField(designResolutionY);
                EditorGUILayout.PropertyField(screenMatchMode);
                EditorGUILayout.PropertyField(ignoreOrientation);
            }
            else if ((UIContentScaler.ScaleMode)scaleMode.enumValueIndex == UIContentScaler.ScaleMode.ConstantPhysicalSize)
            {
                EditorGUILayout.PropertyField(fallbackScreenDPI);
                EditorGUILayout.PropertyField(defaultSpriteDPI);
            }
            else
                EditorGUILayout.PropertyField(constantScaleFactor);

            if (serializedObject.ApplyModifiedProperties())
                (target as UIContentScaler).ApplyModifiedProperties();
        }
    }
}
