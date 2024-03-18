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
#if UNITY_2019_1_OR_NEWER
        bool _guiControllersFoldout = true;
        bool _guiTransitionsFoldout = true;
        bool _guiTextFormatFoldout = true;
#endif

        void OnEnable()
        {
            EditorApplication.update += _onEditorAppUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= _onEditorAppUpdate;
        }

        void _onEditorAppUpdate()
        {
            Repaint();
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
                float alpha = EditorGUILayout.Slider("Alpha", gObj.alpha, 0, 1);
                if (EditorGUI.EndChangeCheck())
                    gObj.alpha = alpha;

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

                //Draw Color Field
                var objType = gObj.GetType();
                var colorProperty = objType.GetProperty("color");
                if (colorProperty != null)
                {
                    EditorGUI.BeginChangeCheck();
                    Color color = (Color)colorProperty.GetValue(gObj);
                    color = EditorGUILayout.ColorField("Color", color);
                    if (EditorGUI.EndChangeCheck())
                    {
                        colorProperty.SetValue(gObj, color);
                    }
                }

                EditorGUI.BeginChangeCheck();
                string tooltips = EditorGUILayout.TextField("Tooltips", gObj.tooltips);
                if (EditorGUI.EndChangeCheck())
                    gObj.tooltips = tooltips;

                if (!(gObj is GImage))
                {
                    EditorGUI.BeginChangeCheck();
                    bool touchable = EditorGUILayout.Toggle("Touchable", gObj.touchable);
                    if (EditorGUI.EndChangeCheck())
                        gObj.touchable = touchable;

                    EditorGUI.BeginChangeCheck();
                    bool draggable = EditorGUILayout.Toggle("Draggable", gObj.draggable);
                    if (EditorGUI.EndChangeCheck())
                        gObj.draggable = draggable;
                }

#if UNITY_2019_1_OR_NEWER
                TextFormat textFormat = null;
                if (gObj is GTextField gTxt)
                {
                    textFormat = gTxt.textFormat;
                }

                if (textFormat != null)
                {
                    _guiTextFormatFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_guiTextFormatFoldout, "Text Format");
                    EditorGUI.BeginChangeCheck();
                    if (_guiTextFormatFoldout)
                    {

                        var initLabelWidth = EditorGUIUtility.labelWidth;

                        var richStyle = new GUIStyle(GUI.skin.label);
                        richStyle.richText = true;
                        EditorGUIUtility.labelWidth = 60;
                        textFormat.font = EditorGUILayout.TextField("Font", textFormat.font);
                        textFormat.align = (AlignType)EditorGUILayout.EnumPopup("Align", textFormat.align);

                        EditorGUIUtility.labelWidth = initLabelWidth;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 40;
                        textFormat.size = EditorGUILayout.IntField("Size", textFormat.size);
                        textFormat.color = EditorGUILayout.ColorField("Color", textFormat.color);
                        EditorGUIUtility.labelWidth = initLabelWidth;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 40;
                        textFormat.outline = EditorGUILayout.FloatField("Outline", textFormat.outline);
                        textFormat.outlineColor = EditorGUILayout.ColorField("Color", textFormat.outlineColor);
                        EditorGUIUtility.labelWidth = initLabelWidth;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 50;
                        textFormat.shadowOffset = EditorGUILayout.Vector2Field("Shadow Offset", textFormat.shadowOffset);
                        textFormat.shadowColor = EditorGUILayout.ColorField("Color", textFormat.shadowColor);
                        EditorGUIUtility.labelWidth = initLabelWidth;
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.BeginHorizontal();
                        textFormat.italic = EditorGUILayout.ToggleLeft("<i>I</i>", textFormat.italic, richStyle, GUILayout.Width(30));
                        textFormat.bold = EditorGUILayout.ToggleLeft("<b>B</b>", textFormat.bold, richStyle, GUILayout.Width(30));
                        textFormat.underline = EditorGUILayout.ToggleLeft("U̲", textFormat.underline, richStyle, GUILayout.Width(30));
                        textFormat.strikethrough = EditorGUILayout.ToggleLeft(" S̶ ̶ ̶", textFormat.strikethrough, richStyle, GUILayout.Width(36));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 90;
                        textFormat.lineSpacing = EditorGUILayout.IntField("Line Spacing", textFormat.lineSpacing);
                        textFormat.letterSpacing = EditorGUILayout.IntField("Letter Spacing", textFormat.letterSpacing);
                        EditorGUIUtility.labelWidth = initLabelWidth;
                        EditorGUILayout.EndHorizontal();
                        textFormat.specialStyle = (TextFormat.SpecialStyle)EditorGUILayout.EnumPopup("Special Style", textFormat.specialStyle);

                    }
                    if (EditorGUI.EndChangeCheck())
                        gObj.asTextField.textFormat = textFormat;

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                if (gObj is GComponent gComp)
                {
                    EditorGUI.BeginChangeCheck();
                    bool opaque = EditorGUILayout.Toggle("Opaque", gComp.opaque);
                    if (EditorGUI.EndChangeCheck())
                        gComp.opaque = opaque;

                    var headerLabelStyle = new GUIStyle(GUI.skin.label);
                    headerLabelStyle.fontStyle = FontStyle.Bold;

                    if (gComp.Controllers.Count > 0)
                    {
                        _guiControllersFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_guiControllersFoldout, "Controllers");
                        if (_guiControllersFoldout)
                        {
                            foreach (var ctl in gComp.Controllers)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(ctl.name, headerLabelStyle, GUILayout.MaxWidth(ctl.name.Length * 15));

                                for (var i = 0; i < ctl.pageCount; i++)
                                {
                                    var btnName = ctl.GetPageId(i) + ": " + ctl.GetPageName(i);
                                    var btnStyle = new GUIStyle("ButtonMid");
                                    if (ctl.selectedIndex == i)
                                    {
                                        btnStyle.normal.textColor = Color.green;
                                        btnStyle.hover.textColor = Color.yellow;
                                        btnStyle.fontStyle = FontStyle.Bold;
                                    }
                                    if (GUILayout.Button(btnName, btnStyle))
                                    {
                                        ctl.selectedIndex = i;
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndFoldoutHeaderGroup();
                    }

                    if (gComp.Transitions.Count > 0)
                    {
                        _guiTransitionsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_guiTransitionsFoldout, "Transitions");
                        if (_guiTransitionsFoldout)
                        {
                            foreach (var transition in gComp.Transitions)
                            {
                                EditorGUILayout.BeginHorizontal();
                                var labelStyle = new GUIStyle(headerLabelStyle);
                                if (transition.playing)
                                {
                                    labelStyle.normal.textColor = Color.yellow;
                                }

                                EditorGUILayout.LabelField($"{transition.name} - {transition.totalDuration}s", labelStyle, GUILayout.MinWidth(150));
                                EditorGUI.BeginChangeCheck();
                                var timeScale = transition.timeScale;


                                if (EditorGUI.EndChangeCheck())
                                    transition.timeScale = timeScale;


                                if (GUILayout.Button("▶", GUILayout.Width(20)))
                                {
                                    transition.Play();
                                }

                                if (GUILayout.Button("■", GUILayout.Width(20)))
                                {
                                    transition.Stop();
                                }

                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndFoldoutHeaderGroup();
                    }
                }
#endif
            }
        }
    }
}
