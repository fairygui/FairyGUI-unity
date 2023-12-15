#if FAIRYGUI_TMPRO
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FairyGUI
{
    public class TMPSubTextField : DisplayObject, IMeshFactory
    {
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

        private List<short> _verticesCount = new List<short>();
        public List<short> VerticesCount => _verticesCount;

        private VertexBuffer _preparedVertexBuffer;

        public TMPSubTextField(TextField textField)
        {
            _textField = textField;
            _flags |= Flags.TouchDisabled;

            CreateGameObject("SubTextField");
            graphics = new NGraphics(gameObject);
            graphics.meshFactory = this;

            gOwner = textField.gOwner;
        }

        public void Init(TMPFont font)
        {
            visible = true;
            this.font = font;
            PrepareVertexBuffer();
            _preparedVertexBuffer.Clear();
        }

        private void PrepareVertexBuffer()
        {
            if (_preparedVertexBuffer == null)
                _preparedVertexBuffer = VertexBuffer.Begin();
        }

        private void DisposeVertexBuffer()
        {
            if (_preparedVertexBuffer == null) return;
            _preparedVertexBuffer.End();
            _preparedVertexBuffer = null;
        }
        
        public void OnPopulateMesh(VertexBuffer vb)
        {
            if (_preparedVertexBuffer == null) return;
            List<Vector3> vertList = vb.vertices;
            List<Vector2> uvList = vb.uvs;
            List<Vector2> uv2List = vb.uvs2;
            List<Color32> colList = vb.colors;
            
            vertList.AddRange(_preparedVertexBuffer.vertices);
            uvList.AddRange(_preparedVertexBuffer.uvs);
            uv2List.AddRange(_preparedVertexBuffer.uvs2);
            colList.AddRange(_preparedVertexBuffer.colors);

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

        public int DrawGlyph(TMP_Character ch, float px, float py)
        {
            PrepareVertexBuffer();
            List<Vector3> vertList = _preparedVertexBuffer.vertices;
            List<Vector2> uvList = _preparedVertexBuffer.uvs;
            List<Vector2> uv2List = _preparedVertexBuffer.uvs2;
            List<Color32> colList = _preparedVertexBuffer.colors;
            int index = _verticesCount.Count;
            _verticesCount.Add((short)_font.DrawGlyph(ch, px, py, vertList, uvList, uv2List, colList));
            return index;
        }

        public int DrawLine(int lineType, float px, float py, float width, int fontSize)
        {
            PrepareVertexBuffer();
            List<Vector3> vertList = _preparedVertexBuffer.vertices;
            List<Vector2> uvList = _preparedVertexBuffer.uvs;
            List<Vector2> uv2List = _preparedVertexBuffer.uvs2;
            List<Color32> colList = _preparedVertexBuffer.colors;
            int index = _verticesCount.Count;
            _verticesCount.Add((short)_font.DrawLine(px, py, width, fontSize, lineType, vertList, uvList, uv2List, colList));
            return index;
        }

        public bool ForceUpdateMesh()
        {
            graphics.SetMeshDirty();
            bool update = graphics.UpdateMesh();
            DisposeVertexBuffer();
            return update;
        }
        
        public void CleanUp()
        {
            DisposeVertexBuffer();
            _verticesCount.Clear();
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