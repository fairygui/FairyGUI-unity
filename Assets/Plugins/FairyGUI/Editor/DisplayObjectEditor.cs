using UnityEngine;
using UnityEditor;
using FairyGUI;

namespace FairyGUIEditor
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(DisplayObjectInfo))]
    public class DisplayObjectEditor : Editor
    {
        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            DisplayObject obj = (target as DisplayObjectInfo).displayObject;
            if (obj == null)
                return;

            EditorGUILayout.LabelField(obj.GetType().Name + ": " + obj.id, (GUIStyle)"OL Title");
            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.TextField("Name", obj.name);
            if (EditorGUI.EndChangeCheck())
                obj.name = name;
            if (obj is Container)
            {
                EditorGUI.BeginChangeCheck();
                bool fairyBatching = EditorGUILayout.Toggle("FairyBatching", ((Container)obj).fairyBatching);
                if (EditorGUI.EndChangeCheck())
                    ((Container)obj).fairyBatching = fairyBatching;
            }

            GObject gObj = obj.gOwner;
            if (gObj != null)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField(gObj.GetType().Name + ": " + gObj.id, (GUIStyle)"OL Title");
                EditorGUILayout.Separator();

                if (!string.IsNullOrEmpty(gObj.resourceURL))
                {
                    PackageItem pi = UIPackage.GetItemByURL(gObj.resourceURL);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Resource");
                    EditorGUILayout.LabelField(pi.name + "@" + pi.owner.name);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.BeginChangeCheck();
                name = EditorGUILayout.TextField("Name", gObj.name);
                if (EditorGUI.EndChangeCheck())
                    gObj.name = name;

                if (gObj.parent != null)
                {
                    string[] options = new string[gObj.parent.numChildren];
                    int[] values = new int[options.Length];
                    for (int i = 0; i < options.Length; i++)
                    {
                        options[i] = i.ToString();
                        values[i] = i;
                    }
                    EditorGUI.BeginChangeCheck();
                    int childIndex = EditorGUILayout.IntPopup("Child Index", gObj.parent.GetChildIndex(gObj), options, values);
                    if (EditorGUI.EndChangeCheck())
                        gObj.parent.SetChildIndex(gObj, childIndex);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Child Index");
                    EditorGUILayout.LabelField("No Parent");
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.BeginChangeCheck();
                Vector3 position = EditorGUILayout.Vector3Field("Position", gObj.position);
                if (EditorGUI.EndChangeCheck())
                    gObj.position = position;

                EditorGUI.BeginChangeCheck();
                Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", new Vector3(gObj.rotationX, gObj.rotationY, gObj.rotation));
                if (EditorGUI.EndChangeCheck())
                {
                    gObj.rotationX = rotation.x;
                    gObj.rotationY = rotation.y;
                    gObj.rotation = rotation.z;
                }

                EditorGUI.BeginChangeCheck();
                Vector2 scale = EditorGUILayout.Vector2Field("Scale", gObj.scale);
                if (EditorGUI.EndChangeCheck())
                    gObj.scale = scale;

                EditorGUI.BeginChangeCheck();
                Vector2 skew = EditorGUILayout.Vector2Field("Skew", gObj.skew);
                if (EditorGUI.EndChangeCheck())
                    gObj.skew = skew;

                EditorGUI.BeginChangeCheck();
                Vector2 size = EditorGUILayout.Vector2Field("Size", gObj.size);
                if (EditorGUI.EndChangeCheck())
                    gObj.size = size;

                EditorGUI.BeginChangeCheck();
                Vector2 pivot = EditorGUILayout.Vector2Field("Pivot", gObj.pivot);
                if (EditorGUI.EndChangeCheck())
                    gObj.pivot = pivot;

                EditorGUI.BeginChangeCheck();
                string text = EditorGUILayout.TextField("Text", gObj.text);
                if (EditorGUI.EndChangeCheck())
                    gObj.text = text;

                EditorGUI.BeginChangeCheck();
                string icon = EditorGUILayout.TextField("Icon", gObj.icon);
                if (EditorGUI.EndChangeCheck())
                    gObj.icon = icon;
            }
        }
    }
}
