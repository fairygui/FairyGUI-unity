#if FAIRYGUI_TMPRO
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FairyGUI
{
    public class TMPSubTextField : DisplayObject, IMeshFactory
    {
        public struct CharInfo
        {
            public TMP_Character character;
            public short lineIndex;
            public float px;
            public float py;
            /// <summary>
            /// 字符占用的顶点数量。
            /// </summary>
            public short vertCount;

            public CharInfo(TMP_Character character, short lineIndex, float px, float py)
            {
                this.character = character;
                this.lineIndex = lineIndex;
                this.px = px;
                this.py = py;
                vertCount = 0;
            }
        }
        
        public struct LineInfo
        {
            public short lineIndex;
            public float px;
            public float py;
            public float width;
            public int fontSize;

            public LineInfo(short lineIndex, float px, float py, float width, int fontSize)
            {
                this.lineIndex = lineIndex;
                this.px = px;
                this.py = py;
                this.width = width;
                this.fontSize = fontSize;
            }
            
            public void Set(short lineIndex, float px, float py, float width, int fontSize)
            {
                this.lineIndex = lineIndex;
                this.px = px;
                this.py = py;
                this.width = width;
                this.fontSize = fontSize;
            }
        }

        private TextField _textField;
        private TMPFont _font { set; get; }
        public TMPFont font
        {
            get => _font;
            set
            {
                if (_font == value) return;
                _font = value;
                graphics.SetShaderAndTexture(_font.shader, _font.mainTexture);
            }
        }

        private List<CharInfo> _toRendererChars = new List<CharInfo>();
        public List<CharInfo> toRendererChars => _toRendererChars;
        private List<LineInfo> _underlineInfos = new List<LineInfo>();
        private List<LineInfo> _strikethroughInfos = new List<LineInfo>();

        public TMPSubTextField(TextField textField)
        {
            _textField = textField;
            _flags |= Flags.TouchDisabled;

            CreateGameObject("SubTextField");
            graphics = new NGraphics(gameObject);
            graphics.meshFactory = this;

            gOwner = textField.gOwner;
        }
        
        public void OnPopulateMesh(VertexBuffer vb)
        {
            List<Vector3> vertList = vb.vertices;
            List<Vector2> uvList = vb.uvs;
            List<Vector2> uv2List = vb.uvs2;
            List<Color32> colList = vb.colors;

            int underlineIndex = 0;
            int strikethroughIndex = 0;
            for (int i = 0, charCount = _toRendererChars.Count; i < charCount; i++)
            {
                var charInfo = _toRendererChars[i];
                charInfo.vertCount = (short)_font.DrawGlyph(charInfo.character, charInfo.px, charInfo.py, vertList, uvList, uv2List, colList);

                // End of line
                if (i == charCount - 1 || charInfo.lineIndex != _toRendererChars[i + 1].lineIndex)
                {
                    int lineVertCount = 0;
                    for (int infoCount = _underlineInfos.Count; underlineIndex < infoCount; underlineIndex++)
                    {
                        var lineInfo = _underlineInfos[underlineIndex];
                        if (lineInfo.lineIndex == charInfo.lineIndex)
                        {
                            lineVertCount += _font.DrawLine(lineInfo.px, lineInfo.py, lineInfo.width, lineInfo.fontSize, 0, vertList, uvList, uv2List, colList);
                            underlineIndex++; // Move next
                            break;
                        }
                    }
                    for (int infoCount = _strikethroughInfos.Count; strikethroughIndex < infoCount; strikethroughIndex++)
                    {
                        var lineInfo = _strikethroughInfos[strikethroughIndex];
                        if (lineInfo.lineIndex == charInfo.lineIndex)
                        {
                            lineVertCount += _font.DrawLine(lineInfo.px, lineInfo.py, lineInfo.width, lineInfo.fontSize, 1, vertList, uvList, uv2List, colList);
                            strikethroughIndex++; // Move next
                            break;
                        }
                    }
                    if (lineVertCount > 0)
                        charInfo.vertCount += (short) lineVertCount;
                }
                
                _toRendererChars[i] = charInfo;
            }

            int count = vertList.Count;
            if (count > 65000)
            {
                Debug.LogWarning("Text is too large. A mesh may not have more than 65000 vertices.");
                vertList.RemoveRange(65000, count - 65000);
                colList.RemoveRange(65000, count - 65000);
                uvList.RemoveRange(65000, count - 65000);
                if (uv2List.Count > 0)
                    uv2List.RemoveRange(65000, count - 65000);
                count = 65000;
            }
            
            vb.AddTriangles();
        }

        public int AddToRendererChar(TMP_Character ch, short lineIndex, float px, float py)
        {
            int index = _toRendererChars.Count;
            _toRendererChars.Add(new CharInfo(ch, lineIndex, px, py));
            return index;
        }

        public void AddUnderline(short lineIndex, float px, float py, float width, int fontSize)
        {
            _underlineInfos.Add(new LineInfo(lineIndex, px, py, width, fontSize));
        }
        
        public void AddStrikethrough(short lineIndex, float px, float py, float width, int fontSize)
        {
            _strikethroughInfos.Add(new LineInfo(lineIndex, px, py, width, fontSize));
        }

        public bool ForceUpdateMesh()
        {
            graphics.SetMeshDirty();
            return graphics.UpdateMesh();
        }
        
        public void CleanUp()
        {
            _toRendererChars.Clear();
            _underlineInfos.Clear();
            _strikethroughInfos.Clear();
        }
        
        public void Clear()
        {
            CleanUp();
            graphics.mesh.Clear();
            graphics.SetMeshDirty();
        }
    }
}

#endif