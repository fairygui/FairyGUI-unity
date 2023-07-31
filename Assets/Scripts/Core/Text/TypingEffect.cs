using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 文字打字效果。先调用Start，然后Print。
    /// </summary>
    public class TypingEffect
    {
        protected TextField _textField;
#if FAIRYGUI_TMPRO
        protected List<Vector3[]> _backupVertsList = new List<Vector3[]>();
        protected List<Vector3[]> _verticesList = new List<Vector3[]>();
        protected List<int> _mainLayerStartList = new List<int>();
        protected List<int> _strokeLayerStartList = new List<int>();
        protected List<int> _vertIndexList = new List<int>();
        protected List<int> _mainLayerVertCountList = new List<int>();
#else
        protected Vector3[] _backupVerts;
        protected Vector3[] _vertices;
        protected int _mainLayerStart;
        protected int _strokeLayerStart;
        protected int _vertIndex;
        protected int _mainLayerVertCount;
#endif

        protected bool _stroke;
        protected bool _shadow;

        protected int _printIndex;
        protected int _strokeDrawDirs;

        protected bool _started;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textField"></param>
        public TypingEffect(TextField textField)
        {
            _textField = textField;
            _textField.EnableCharPositionSupport();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textField"></param>
        public TypingEffect(GTextField textField)
        {
            if (textField is GRichTextField)
                _textField = ((RichTextField)textField.displayObject).textField;
            else
                _textField = (TextField)textField.displayObject;
            _textField.EnableCharPositionSupport();
        }

        /// <summary>
        /// 总输出次数
        /// </summary>
        public int totalTimes
        {
            get
            {
                int times = 0;
                List<TextField.CharPosition> charPositions = _textField.charPositions;
                for (int i = 0; i < charPositions.Count - 1; i++)
                {
                    if (charPositions[i].imgIndex > 0) //这是一个图片
                        times++;
                    else if (!char.IsWhiteSpace(_textField.parsedText[i]))
                        times++;
                }
                return times;
            }
        }

        /// <summary>
        /// 开始打字效果。可以重复调用重复启动。
        /// </summary>
        public void Start()
        {
            _textField.graphics.meshModifier -= OnMeshModified;
            _textField.Redraw();
            _textField.graphics.meshModifier += OnMeshModified;

            _stroke = false;
            _shadow = false;
            _strokeDrawDirs = 4;
            _printIndex = 0;
            _started = true;

#if FAIRYGUI_TMPRO
            int vertCount = 0;
            int meshVertCount = 0;
            _backupVertsList.Clear();
            _mainLayerStartList.Clear();
            _strokeLayerStartList.Clear();
            _vertIndexList.Clear();
            _mainLayerVertCountList.Clear();
            if (_verticesList.Capacity < _textField.activeSubMeshCount + 1)
                _verticesList.Capacity = _textField.activeSubMeshCount + 1;
            for (int i = 0, count = _textField.activeSubMeshCount + 1; i < count; i++)
            {
                _vertIndexList.Add(0);
                var displayObject = _textField.GetChildAt(i);
                meshVertCount = displayObject.graphics.mesh.vertexCount;
                vertCount += meshVertCount;
                var backupVerts = displayObject.graphics.mesh.vertices;
                _backupVertsList.Add(backupVerts);
                Vector3[] vertices;
                if (i < _verticesList.Count)
                {
                    vertices = _verticesList[i];
                    if (vertices == null || vertices.Length != meshVertCount)
                    {
                        vertices = new Vector3[meshVertCount];
                        _verticesList[i] = vertices;
                    }
                }
                else
                {
                    vertices = new Vector3[meshVertCount];
                    _verticesList.Add(vertices);
                }
                Vector3 zero = Vector3.zero;
                for (int j = 0; j < meshVertCount; j++)
                    vertices[j] = zero;
                displayObject.graphics.mesh.vertices = vertices;
            }
#else
            _mainLayerStart = 0;
            _mainLayerVertCount = 0;
            _vertIndex = 0;
            int vertCount = _textField.graphics.mesh.vertexCount;
            _backupVerts = _textField.graphics.mesh.vertices;
            if (_vertices == null || _vertices.Length != vertCount)
                _vertices = new Vector3[vertCount];
            Vector3 zero = Vector3.zero;
            for (int i = 0; i < vertCount; i++)
                _vertices[i] = zero;
            _textField.graphics.mesh.vertices = _vertices;
#endif

            //隐藏所有混排的对象
            if (_textField.richTextField != null)
            {
                int ec = _textField.richTextField.htmlElementCount;
                for (int i = 0; i < ec; i++)
                    _textField.richTextField.ShowHtmlObject(i, false);
            }

#if FAIRYGUI_TMPRO
            int allMainLayerVertCount = 0;
            for (int i = 0, childCount = _textField.activeSubMeshCount + 1; i < childCount; i++)
            {
                var displayObject = _textField.GetChildAt(i);
                meshVertCount = displayObject.graphics.mesh.vertexCount;
                int mainLayerVertCount = 0;
                if (i == 0)
                {
                    int charCount = _textField.charPositions.Count;
                    for (int j = 0; j < charCount; j++)
                    {
                        TextField.CharPosition cp = _textField.charPositions[j];
                        mainLayerVertCount += cp.vertCount;
                    }
                }
                else
                {
                    var toRendererChars = _textField.GetSubTextField(i - 1).toRendererChars;
                    for (int j = 0, count = toRendererChars.Count; j < count; j++)
                        mainLayerVertCount  += toRendererChars[j].vertCount;
                }
                _mainLayerVertCountList.Add(mainLayerVertCount);
                if (mainLayerVertCount < meshVertCount) //说明有描边或者阴影
                {
                    int repeat = meshVertCount / mainLayerVertCount;
                    _mainLayerStartList.Add(meshVertCount - meshVertCount / repeat);
                    _strokeLayerStartList.Add(repeat % 2 == 0 ? (meshVertCount / repeat) : 0);
                }
                else
                {
                    _mainLayerStartList.Add(0);
                    _strokeLayerStartList.Add(0);
                }
                allMainLayerVertCount += mainLayerVertCount;
            }
            
            if (allMainLayerVertCount < vertCount) //说明有描边或者阴影
            {
                int repeat = vertCount / allMainLayerVertCount;
                _stroke = repeat > 2;
                _shadow = repeat % 2 == 0;
                _strokeDrawDirs = repeat > 8 ? 8 : 4;
            }
#else
            int charCount = _textField.charPositions.Count;
            for (int i = 0; i < charCount; i++)
            {
                TextField.CharPosition cp = _textField.charPositions[i];
                _mainLayerVertCount += cp.vertCount;
            }

            if (_mainLayerVertCount < vertCount) //说明有描边或者阴影
            {
                int repeat = vertCount / _mainLayerVertCount;
                _stroke = repeat > 2;
                _shadow = repeat % 2 == 0;
                _mainLayerStart = vertCount - vertCount / repeat;
                _strokeLayerStart = _shadow ? (vertCount / repeat) : 0;
                _strokeDrawDirs = repeat > 8 ? 8 : 4;
            }
#endif
        }

        /// <summary>
        /// 输出一个字符。如果已经没有剩余的字符，返回false。
        /// </summary>
        /// <returns></returns>
        public bool Print()
        {
            if (!_started)
                return false;

            TextField.CharPosition cp;
            List<TextField.CharPosition> charPositions = _textField.charPositions;
            int listCnt = charPositions.Count;

            while (_printIndex < listCnt - 1) //最后一个是占位的，无效的，所以-1
            {
                cp = charPositions[_printIndex++];
#if FAIRYGUI_TMPRO
                if (cp.vertCount > 0 || cp.subIndex > 0)
                {
                    int vertCount = cp.subIndex > 0 ? _textField.GetSubTextField(cp.subIndex - 1).toRendererChars[cp.subCharIndex].vertCount : cp.vertCount;
                    if (vertCount > 0)
                        output(vertCount, cp.subIndex);
                }
                
                // Draw lines
                if (cp.drawLineSubIndex > 0)
                {
                    var toRendererChars = _textField.GetSubTextField(cp.drawLineSubIndex - 1).toRendererChars;
                    for (byte i = 0, count = cp.drawLineCount; i < count; i++)
                    {
                        int vertCount = toRendererChars[cp.drawLineSubStartCharIndex + i].vertCount;
                        if (vertCount > 0)
                            output(vertCount, cp.drawLineSubIndex);
                    }
                }
#else
                if (cp.vertCount > 0)
                    output(cp.vertCount);
#endif
                if (cp.imgIndex > 0) //这是一个图片
                {
                    _textField.richTextField.ShowHtmlObject(cp.imgIndex - 1, true);
                    return true;
                }
                else if (!char.IsWhiteSpace(_textField.parsedText[_printIndex - 1]))
                    return true;
            }

            Cancel();
            return false;
        }

#if FAIRYGUI_TMPRO
        private void output(int vertCount, int subIndex)
        {
            int start, end;

            var mainLayerStart = _mainLayerStartList[subIndex];
            var vertIndex = _vertIndexList[subIndex];
            var vertices = _verticesList[subIndex];
            var backupVerts = _backupVertsList[subIndex];
            var strokeLayerStart = _strokeLayerStartList[subIndex];
            var mainLayerVertCount = _mainLayerVertCountList[subIndex];
            start = mainLayerStart + vertIndex;
            end = start + vertCount;
            for (int i = start; i < end; i++)
                vertices[i] = backupVerts[i];

            if (_stroke)
            {
                start = strokeLayerStart + vertIndex;
                end = start + vertCount;
                for (int i = start; i < end; i++)
                {
                    for (int j = 0; j < _strokeDrawDirs; j++)
                    {
                        int k = i + mainLayerVertCount * j;
                        vertices[k] = backupVerts[k];
                    }
                }
            }

            if (_shadow)
            {
                start = vertIndex;
                end = start + vertCount;
                for (int i = start; i < end; i++)
                {
                    vertices[i] = backupVerts[i];
                }
            }

            _textField.GetChildAt(subIndex).graphics.mesh.vertices = vertices;

            _vertIndexList[subIndex] = vertIndex + vertCount;
        }
#else
        private void output(int vertCount)
        {
            int start, end;

            start = _mainLayerStart + _vertIndex;
            end = start + vertCount;
            for (int i = start; i < end; i++)
                _vertices[i] = _backupVerts[i];

            if (_stroke)
            {
                start = _strokeLayerStart + _vertIndex;
                end = start + vertCount;
                for (int i = start; i < end; i++)
                {
                    for (int j = 0; j < _strokeDrawDirs; j++)
                    {
                        int k = i + _mainLayerVertCount * j;
                        _vertices[k] = _backupVerts[k];
                    }
                }
            }

            if (_shadow)
            {
                start = _vertIndex;
                end = start + vertCount;
                for (int i = start; i < end; i++)
                {
                    _vertices[i] = _backupVerts[i];
                }
            }

            _textField.graphics.mesh.vertices = _vertices;

            _vertIndex += vertCount;
        }
#endif

        /// <summary>
        /// 打印的协程。
        /// </summary>
        /// <param name="interval">每个字符输出的时间间隔</param>
        /// <returns></returns>
        public IEnumerator Print(float interval)
        {
            while (Print())
                yield return new WaitForSeconds(interval);
        }

        /// <summary>
        /// 使用固定时间间隔完成整个打印过程。
        /// </summary>
        /// <param name="interval"></param>
        public void PrintAll(float interval)
        {
            Timers.inst.StartCoroutine(Print(interval));
        }

        public void Cancel()
        {
            if (!_started)
                return;

            _started = false;
            _textField.graphics.meshModifier -= OnMeshModified;
            _textField.SetMeshDirty();
        }

        /// <summary>
        /// 当打字过程中，文本可能会由于字体纹理更改而发生字体重建，要处理这种情况。
        /// 图片对象不需要处理，因为HtmlElement.status里设定的隐藏标志不会因为Mesh更新而被冲掉。
        /// </summary>
        void OnMeshModified()
        {
#if FAIRYGUI_TMPRO
            for (int i = 0, count = _backupVertsList.Count; i < count; i++)
            {
                if (_textField.GetChildAt(i).graphics.mesh.vertexCount != _backupVertsList[i].Length) //可能文字都改了
                {
                    Cancel();
                    return;
                }
            }

            _backupVertsList.Clear();
            for (int i = 0, count = _textField.activeSubMeshCount + 1; i < count; i++)
            {
                var displayObject = _textField.GetChildAt(i);
                var backupVerts = displayObject.graphics.mesh.vertices;
                _backupVertsList.Add(backupVerts);
                var vertices = _verticesList[i];
                Vector3 zero = Vector3.zero;
                for (int j = 0, len = vertices.Length; j < len; j++)
                {
                    if (vertices[j] != zero)
                        vertices[j] = backupVerts[j];
                }

                displayObject.graphics.mesh.vertices = vertices;
            }
#else
            if (_textField.graphics.mesh.vertexCount != _backupVerts.Length) //可能文字都改了
            {
                Cancel();
                return;
            }

            _backupVerts = _textField.graphics.mesh.vertices;

            int vertCount = _vertices.Length;
            Vector3 zero = Vector3.zero;
            for (int i = 0; i < vertCount; i++)
            {
                if (_vertices[i] != zero)
                    _vertices[i] = _backupVerts[i];
            }

            _textField.graphics.mesh.vertices = _vertices;
#endif
        }
    }
}
