#if FAIRYGUI_TMPRO
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FairyGUI
{
    public class TMPSubTextField : DisplayObject, IMeshFactory
    {
        public enum SubMeshDrawType : byte
        {
            Char = 0,
            Underline = 1,
            Strikethrough = 2,
        }
        
        public struct CharInfo
        {
            public SubMeshDrawType type;
            /// <summary>
            /// 绘制字符的信息
            /// </summary>
            public TMP_Character character;
            /// <summary>
            /// 绘制line的信息
            /// </summary>
            public float width;
            public int fontSize;

            public short lineIndex;
            public float px;
            public float py;
            /// <summary>
            /// 字符占用的顶点数量。
            /// </summary>
            public short vertCount;

            public CharInfo(TMP_Character character, short lineIndex, float px, float py)
            {
                type = SubMeshDrawType.Char;
                this.character = character;
                width = 0;
                fontSize = 0;
                this.lineIndex = lineIndex;
                this.px = px;
                this.py = py;
                vertCount = 0;
            }
            
            public CharInfo(SubMeshDrawType type, short lineIndex, float px, float py, float width, int fontSize)
            {
                this.type = type;
                character = null;
                this.width = width;
                this.fontSize = fontSize;
                this.lineIndex = lineIndex;
                this.px = px;
                this.py = py;
                vertCount = 0;
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
                if (charInfo.type == SubMeshDrawType.Char)
                    charInfo.vertCount = (short)_font.DrawGlyph(charInfo.character, charInfo.px, charInfo.py, vertList, uvList, uv2List, colList);
                else
                    charInfo.vertCount = (short)_font.DrawLine(charInfo.px, charInfo.py, charInfo.width, charInfo.fontSize,
                        charInfo.type == SubMeshDrawType.Underline ? 0 : 1, vertList, uvList, uv2List, colList);
                
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

        public int AddToRendererLine(int lineType, short lineIndex, float px, float py, float width, int fontSize)
        {
            int index = _toRendererChars.Count;
            _toRendererChars.Add(new CharInfo(lineType == 0 ? SubMeshDrawType.Underline : SubMeshDrawType.Strikethrough, lineIndex, px, py, width, fontSize));
            return index;
        }

        public bool ForceUpdateMesh()
        {
            graphics.SetMeshDirty();
            return graphics.UpdateMesh();
        }
        
        public void CleanUp()
        {
            _toRendererChars.Clear();
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