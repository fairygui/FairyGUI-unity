using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
#endif
using UnityEditor;
using FairyGUI;

namespace FairyGUIEditor
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(UIPainter))]
    public class UIPainterEditor : Editor
    {
        SerializedProperty packageName;
        SerializedProperty componentName;
        SerializedProperty renderCamera;
        SerializedProperty fairyBatching;
        SerializedProperty touchDisabled;
        SerializedProperty sortingOrder;

        string[] propertyToExclude;

        void OnEnable()
        {
            packageName = serializedObject.FindProperty("packageName");
            componentName = serializedObject.FindProperty("componentName");
            renderCamera = serializedObject.FindProperty("renderCamera");
            fairyBatching = serializedObject.FindProperty("fairyBatching");
            touchDisabled = serializedObject.FindProperty("touchDisabled");
            sortingOrder = serializedObject.FindProperty("sortingOrder");

            propertyToExclude = new string[] { "m_Script", "packageName", "componentName", "packagePath",
                "renderCamera", "fairyBatching", "touchDisabled","sortingOrder"
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UIPainter panel = target as UIPainter;

            DrawPropertiesExcluding(serializedObject, propertyToExclude);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Package Name");
            if (GUILayout.Button(packageName.stringValue, "ObjectField"))
                EditorWindow.GetWindow<PackagesWindow>(true, "Select a UI Component").SetSelection(packageName.stringValue, componentName.stringValue);

            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
#if UNITY_2018_3_OR_NEWER
                bool isPrefab = PrefabUtility.GetPrefabAssetType(panel) != PrefabAssetType.NotAPrefab;
#else
                bool isPrefab = PrefabUtility.GetPrefabType(panel) == PrefabType.Prefab;
#endif
                panel.SendMessage("OnUpdateSource", new object[] { null, null, null, !isPrefab });

#if UNITY_5_3_OR_NEWER
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#elif UNITY_5
                EditorApplication.MarkSceneDirty();
#else
                EditorUtility.SetDirty(panel);
#endif
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Component Name");
            if (GUILayout.Button(componentName.stringValue, "ObjectField"))
                EditorWindow.GetWindow<PackagesWindow>(true, "Select a UI Component").SetSelection(packageName.stringValue, componentName.stringValue);
            EditorGUILayout.EndHorizontal();
            int oldSortingOrder = panel.sortingOrder;
            EditorGUILayout.PropertyField(sortingOrder);
            EditorGUILayout.PropertyField(renderCamera);
            EditorGUILayout.PropertyField(fairyBatching);
            EditorGUILayout.PropertyField(touchDisabled);

            if (serializedObject.ApplyModifiedProperties())
            {
#if UNITY_2018_3_OR_NEWER
                bool isPrefab = PrefabUtility.GetPrefabAssetType(panel) != PrefabAssetType.NotAPrefab;
#else
                bool isPrefab = PrefabUtility.GetPrefabType(panel) == PrefabType.Prefab;
#endif
                if (!isPrefab)
                {
                    panel.ApplyModifiedProperties(sortingOrder.intValue != oldSortingOrder);
                }
            }
        }
    }
}
