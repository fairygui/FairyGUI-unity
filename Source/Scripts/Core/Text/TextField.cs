using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class TextField : DisplayObject
	{
		VertAlignType _verticalAlign;
		TextFormat _textFormat;
		bool _input;
		string _text;
		AutoSizeType _autoSize;
		bool _wordWrap;
		bool _singleLine;
		bool _html;

		int _stroke;
		Color _strokeColor;
		Vector2 _shadowOffset;

		List<HtmlElement> _elements;
		List<LineInfo> _lines;
		List<CharPosition> _charPositions;

		BaseFont _font;
		float _textWidth;
		float _textHeight;
		bool _textChanged;
		int _yOffset;
		string _alternativeText;
		bool _alternatvieHtml;
		float _fontSizeScale;
		float _renderScale;

		RichTextField _richTextField;

		const int GUTTER_X = 2;
		const int GUTTER_Y = 2;
		const char E_TAG = (char)1;
		static float[] STROKE_OFFSET = new float[]
		{
			 -1f, 0f, 1f, 0f,
			0f, -1f, 0f, 1f
		};
		static float[] BOLD_OFFSET = new float[]
		{
			-0.5f, 0f, 0.5f, 0f,
			0f, -0.5f, 0f, 0.5f
		};

		public TextField()
		{
			_touchDisabled = true;

			_textFormat = new TextFormat();
			_textFormat.size = 12;
			_textFormat.lineSpacing = 3;
			_strokeColor = Color.black;
			_fontSizeScale = 1;
			_renderScale = UIContentScaler.scaleFactor;

			_wordWrap = true;
			_text = string.Empty;

			_elements = new List<HtmlElement>(1);
			_lines = new List<LineInfo>(1);

			CreateGameObject("TextField");
			graphics = new NGraphics(gameObject);
		}

		internal void EnableRichSupport(RichTextField richTextField)
		{
			_richTextField = richTextField;
			if (richTextField is InputTextField)
			{
				_input = true;
				EnableCharPositionSupport();
			}
		}

		public void EnableCharPositionSupport()
		{
			if (_charPositions == null)
			{
				_charPositions = new List<CharPosition>();
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public TextFormat textFormat
		{
			get { return _textFormat; }
			set
			{
				_textFormat = value;

				string fontName = _textFormat.font;
				if (string.IsNullOrEmpty(fontName))
					fontName = UIConfig.defaultFont;
				if (_font == null || _font.name != fontName)
				{
					_font = FontManager.GetFont(fontName);
					if (_font != null)
						graphics.SetShaderAndTexture(_font.shader, _font.mainTexture);
				}
				if (_alternativeText != null || !string.IsNullOrEmpty(_text))
					_textChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AlignType align
		{
			get { return _textFormat.align; }
			set
			{
				if (_textFormat.align != value)
				{
					_textFormat.align = value;
					if (_alternativeText != null || !string.IsNullOrEmpty(_text))
						_textChanged = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public VertAlignType verticalAlign
		{
			get
			{
				return _verticalAlign;
			}
			set
			{
				if (_verticalAlign != value)
				{
					_verticalAlign = value;
					ApplyVertAlign();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string text
		{
			get { return _text; }
			set
			{
				_text = value;
				_textChanged = true;
				_html = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string htmlText
		{
			get { return _text; }
			set
			{
				_text = value;
				_textChanged = true;
				_html = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AutoSizeType autoSize
		{
			get { return _autoSize; }
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					_textChanged = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool wordWrap
		{
			get { return _wordWrap; }
			set
			{
				_wordWrap = value;
				_textChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool singleLine
		{
			get { return _singleLine; }
			set
			{
				_singleLine = value;
				_textChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int stroke
		{
			get
			{
				return _stroke;
			}
			set
			{
				if (_stroke != value)
				{
					_stroke = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color strokeColor
		{
			get
			{
				return _strokeColor;
			}
			set
			{
				if (_strokeColor != value)
				{
					_strokeColor = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 shadowOffset
		{
			get
			{
				return _shadowOffset;
			}
			set
			{
				_shadowOffset = value;
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float textWidth
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _textWidth;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float textHeight
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _textHeight;
			}
		}

		public List<HtmlElement> htmlElements
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _elements;
			}
		}

		public List<LineInfo> lines
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _lines;
			}
		}

		public List<CharPosition> charPositions
		{
			get
			{
				if (_textChanged)
					BuildLines();

				if (_requireUpdateMesh)
					BuildMesh();

				return _charPositions;
			}
		}

		public RichTextField richTextField
		{
			get { return _richTextField; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="html"></param>
		public void SetAlternativeText(string value, bool html)
		{
			if (_alternativeText != value || _alternatvieHtml != html)
			{
				_alternativeText = value;
				_alternatvieHtml = html;
				_textChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Redraw()
		{
			if (_textChanged)
				BuildLines();

			if (_requireUpdateMesh)
				BuildMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="startLine"></param>
		/// <param name="startCharX"></param>
		/// <param name="endLine"></param>
		/// <param name="endCharX"></param>
		/// <param name="clipped"></param>
		/// <param name="resultRects"></param>
		public void GetLinesShape(int startLine, float startCharX, int endLine, float endCharX,
			bool clipped,
			List<Rect> resultRects)
		{
			LineInfo line1 = _lines[startLine];
			LineInfo line2 = _lines[endLine];
			if (startLine == endLine)
			{
				Rect r = Rect.MinMaxRect(startCharX, line1.y, endCharX, line1.y + line1.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
			}
			else if (startLine == endLine - 1)
			{
				Rect r = Rect.MinMaxRect(startCharX, line1.y, GUTTER_X + line1.width, line1.y + line1.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
				r = Rect.MinMaxRect(GUTTER_X, line1.y + line1.height, endCharX, line2.y + line2.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
			}
			else
			{
				Rect r = Rect.MinMaxRect(startCharX, line1.y, GUTTER_X + line1.width, line1.y + line1.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
				for (int i = startLine + 1; i < endLine; i++)
				{
					LineInfo line = _lines[i];
					r = Rect.MinMaxRect(GUTTER_X, r.yMax, GUTTER_X + line.width, line.y + line.height);
					if (clipped)
						resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
					else
						resultRects.Add(r);
				}
				r = Rect.MinMaxRect(GUTTER_X, r.yMax, endCharX, line2.y + line2.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
			}
		}

		override protected void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			if (!_updatingSize)
			{
				if (_wordWrap && widthChanged)
					_textChanged = true;
				else if (_autoSize != AutoSizeType.None)
					_requireUpdateMesh = true;

				if (_verticalAlign != VertAlignType.Top)
					ApplyVertAlign();
			}

			base.OnSizeChanged(widthChanged, heightChanged);
		}

		public override void EnsureSizeCorrect()
		{
			if (_textChanged && _autoSize != AutoSizeType.None)
				BuildLines();
		}

		public override void Update(UpdateContext context)
		{
			if (_font != null)
			{
				if (_font.keepCrisp && _renderScale != UIContentScaler.scaleFactor)
					_textChanged = true;

				if (_font.mainTexture != graphics.texture)
				{
					if (!_textChanged)
						RequestText();
					graphics.texture = _font.mainTexture;
					_requireUpdateMesh = true;
				}

				if (_textChanged)
					BuildLines();

				if (_requireUpdateMesh)
					BuildMesh();
			}

			base.Update(context);
		}

		/// <summary>
		/// 准备字体纹理
		/// </summary>
		void RequestText()
		{
			int count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];
				if (element.type == HtmlElementType.Text)
				{
					_font.SetFormat(element.format, _fontSizeScale);
					_font.PrepareCharacters(element.text);
					_font.PrepareCharacters("_-*");
				}
			}
		}

		void BuildLines()
		{
			_textChanged = false;
			_requireUpdateMesh = true;
			_renderScale = UIContentScaler.scaleFactor;

			Cleanup();

			bool html = false;
			if (_alternativeText != null)
			{
				if (_alternativeText.Length > 0)
				{
					html = _alternatvieHtml;
					if (_alternatvieHtml)
						HtmlParser.inst.Parse(_alternativeText, _textFormat, _elements,
							_richTextField != null ? _richTextField.htmlParseOptions : null);
					else
					{
						HtmlElement element = HtmlElement.GetElement(HtmlElementType.Text);
						element.text = _alternativeText;
						element.format.CopyFrom(_textFormat);
						_elements.Add(element);
					}
				}
			}
			else
			{
				if (_text.Length > 0)
				{
					html = _html;
					if (_html)
						HtmlParser.inst.Parse(_text, _textFormat, _elements,
							_richTextField != null ? _richTextField.htmlParseOptions : null);
					else
					{
						HtmlElement element = HtmlElement.GetElement(HtmlElementType.Text);
						element.text = _text;
						element.format.CopyFrom(_textFormat);
						_elements.Add(element);
					}
				}
			}

			if (_elements.Count == 0)
			{
				LineInfo emptyLine = LineInfo.Borrow();
				emptyLine.width = 0;
				emptyLine.height = 0;
				emptyLine.text = string.Empty;
				emptyLine.y = emptyLine.y2 = GUTTER_Y;
				_lines.Add(emptyLine);

				_textWidth = 0;
				_textHeight = 0;
				_fontSizeScale = 1;

				BuildLinesFinal();

				return;
			}

			if (_richTextField != null && _richTextField.emojies != null)
			{
				html = true;
				HandleEmojies();
			}

			int letterSpacing = _textFormat.letterSpacing;
			int lineSpacing = _textFormat.lineSpacing - 1;
			float rectWidth = _contentRect.width - GUTTER_X * 2;
			float lineWidth = 0, lineHeight = 0, lineTextHeight = 0;
			float glyphWidth = 0, glyphHeight = 0;
			int wordChars = 0;
			float wordStart = 0;
			bool wordPossible = false;
			float lastLineHeight = 0;
			TextFormat format = _textFormat;
			_font.SetFormat(format, _fontSizeScale);
			bool wrap;
			if (_input)
			{
				letterSpacing++;
				wrap = !_singleLine;
			}
			else
				wrap = _wordWrap && !_singleLine;
			float lineY = GUTTER_Y;
			_fontSizeScale = 1;

			RequestText();

			LineInfo line;
			StringBuilder lineBuffer = new StringBuilder();

			int count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];

				if (html)
				{
					//Special tag, indicates the start of an element
					lineBuffer.Append(E_TAG);
					lineBuffer.Append((char)(i + 33));
				}

				if (element.type == HtmlElementType.Text)
				{
					format = element.format;
					_font.SetFormat(format, _fontSizeScale);

					int textLength = element.text.Length;
					for (int offset = 0; offset < textLength; ++offset)
					{
						char ch = element.text[offset];
						if (ch == E_TAG)
							ch = '?';

						if (ch == '\r')
						{
							if (offset != textLength - 1 && element.text[offset + 1] == '\n')
								continue;

							ch = '\n';
						}

						if (ch == '\n')
						{
							lineBuffer.Append(ch);
							line = LineInfo.Borrow();
							line.width = lineWidth;
							if (lineTextHeight == 0)
							{
								if (lastLineHeight == 0)
									lastLineHeight = format.size;
								if (lineHeight == 0)
									lineHeight = lastLineHeight;
								lineTextHeight = lineHeight;
							}
							line.height = lineHeight;
							lastLineHeight = lineHeight;
							line.textHeight = lineTextHeight;
							line.text = lineBuffer.ToString();
							line.y = line.y2 = lineY;
							lineY += (line.height + lineSpacing);
							if (line.width > _textWidth)
								_textWidth = line.width;
							_lines.Add(line);
							lineBuffer.Length = 0;
							lineWidth = 0;
							lineHeight = 0;
							lineTextHeight = 0;
							wordChars = 0;
							continue;
						}

						if (ch == ' ')
						{
							wordChars = 0;
							wordPossible = true;
						}
						else if (wordPossible && (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z'))
						{
							if (wordChars == 0)
								wordStart = lineWidth;
							else if (wordChars > 10)
								wordChars = int.MinValue;

							wordChars++;
						}
						else
						{
							wordChars = 0;
							wordPossible = false;
						}

						if (_font.GetGlyphSize(ch, out glyphWidth, out glyphHeight))
						{
							if (glyphHeight > lineTextHeight)
								lineTextHeight = glyphHeight;

							if (glyphHeight > lineHeight)
								lineHeight = glyphHeight;

							if (lineWidth != 0)
								lineWidth += letterSpacing;
							lineWidth += glyphWidth;
						}

						if (!wrap || lineWidth <= rectWidth)
						{
							lineBuffer.Append(ch);
						}
						else
						{
							line = LineInfo.Borrow();
							line.height = lineHeight;
							line.textHeight = lineTextHeight;
							if (lineBuffer.Length == 0) //the line cannt fit even a char
							{
								line.text = ch.ToString();
								wordChars = 0;
								wordPossible = false;
							}
							else if (wordChars > 0 && wordStart > 0) //if word had broken, move it to new line
							{
								lineBuffer.Append(ch);
								int len = lineBuffer.Length - wordChars;
								line.text = lineBuffer.ToString(0, len);
								line.width = wordStart;
								lineBuffer.Remove(0, len);

								lineWidth -= wordStart;
								wordStart = 0;
							}
							else
							{
								line.text = lineBuffer.ToString();
								line.width = lineWidth - (glyphWidth + letterSpacing);
								lineBuffer.Length = 0;

								lineBuffer.Append(ch);
								lineWidth = glyphWidth;
								lineHeight = glyphHeight;
								lineTextHeight = glyphHeight;
								wordChars = 0;
								wordPossible = false;
							}
							line.y = line.y2 = lineY;
							lineY += (line.height + lineSpacing);
							if (line.width > _textWidth)
								_textWidth = line.width;

							_lines.Add(line);
						}
					}
				}
				else
				{
					wordChars = 0;
					wordPossible = false;

					IHtmlObject htmlObject = null;
					if (_richTextField != null)
					{
						element.space = (int)(rectWidth - lineWidth - 4);
						htmlObject = _richTextField.htmlPageContext.CreateObject(_richTextField, element);
						element.htmlObject = htmlObject;
					}
					if (htmlObject != null)
					{
						glyphWidth = (int)htmlObject.width;
						glyphHeight = (int)htmlObject.height;
						if (glyphWidth == 0)
							continue;

						glyphWidth += 2;
					}
					else
						continue;

					if (glyphHeight > lineHeight)
						lineHeight = glyphHeight;

					if (lineWidth != 0)
						lineWidth += letterSpacing;
					lineWidth += glyphWidth;

					if (wrap && lineWidth > rectWidth && glyphWidth < rectWidth)
					{
						line = LineInfo.Borrow();
						line.height = lineHeight;
						line.textHeight = lineTextHeight;
						int len = lineBuffer.Length;
						line.text = lineBuffer.ToString(0, len - 2);
						line.width = lineWidth - (glyphWidth + letterSpacing);
						lineBuffer.Remove(0, len - 2);
						lineWidth = glyphWidth;
						line.y = line.y2 = lineY;
						lineY += (line.height + lineSpacing);
						if (line.width > _textWidth)
							_textWidth = line.width;

						lineTextHeight = 0;
						lineHeight = glyphHeight;
						wordChars = 0;
						wordPossible = false;
						_lines.Add(line);
					}
				}
			}

			if (lineWidth > 0 || _lines.Count == 0 || _lines[_lines.Count - 1].text.EndsWith("\n"))
			{
				line = LineInfo.Borrow();
				line.width = lineWidth;
				if (lineHeight == 0)
					lineHeight = lastLineHeight;
				if (lineTextHeight == 0)
					lineTextHeight = lineHeight;
				line.height = lineHeight;
				line.textHeight = lineTextHeight;
				line.text = lineBuffer.ToString();
				line.y = line.y2 = lineY;
				if (line.width > _textWidth)
					_textWidth = line.width;
				_lines.Add(line);
			}

			if (_textWidth > 0)
				_textWidth += GUTTER_X * 2;

			line = _lines[_lines.Count - 1];
			_textHeight = line.y + line.height + GUTTER_Y;

			_textWidth = Mathf.CeilToInt(_textWidth);
			_textHeight = Mathf.CeilToInt(_textHeight);
			if (_autoSize == AutoSizeType.Shrink && _textWidth > rectWidth)
			{
				_fontSizeScale = rectWidth / _textWidth;
				_textWidth = rectWidth;
				_textHeight = Mathf.CeilToInt(_textHeight * _fontSizeScale);

				//调整各行的大小
				int lineCount = _lines.Count;
				for (int i = 0; i < lineCount; ++i)
				{
					line = _lines[i];
					line.y *= _fontSizeScale;
					line.y2 *= _fontSizeScale;
					line.height *= _fontSizeScale;
					line.width *= _fontSizeScale;
					line.textHeight *= _fontSizeScale;
				}
			}
			else
				_fontSizeScale = 1;

			BuildLinesFinal();
		}

		bool _updatingSize; //防止重复调用BuildLines
		void BuildLinesFinal()
		{
			if (!_input)
			{
				if (_autoSize == AutoSizeType.Both)
				{
					_updatingSize = true;
					if (_richTextField != null)
						_richTextField.SetSize(_textWidth, _textHeight);
					else
						SetSize(_textWidth, _textHeight);
					_updatingSize = false;
				}
				else if (_autoSize == AutoSizeType.Height)
				{
					_updatingSize = true;
					if (_richTextField != null)
						_richTextField.height = _textHeight;
					else
						this.height = _textHeight;
					_updatingSize = false;
				}
			}

			_yOffset = 0;
			ApplyVertAlign();
		}

		void HandleEmojies()
		{
			int count = _elements.Count;
			Emoji gchar;
			int i = 0;
			int offset;
			int textLength;
			uint key;
			int rm;

			while (i < count)
			{
				HtmlElement element = _elements[i];
				if (element.type == HtmlElementType.Text)
				{
					textLength = element.text.Length;
					offset = 0;
					while (offset < textLength)
					{
						char ch = element.text[offset];
						if (char.IsHighSurrogate(ch) && offset < textLength - 1)
						{
							rm = 2;
							key = ((uint)element.text[offset + 1] & 0x03FF) + ((((uint)ch & 0x03FF) + 0x40) << 10);
						}
						else
						{
							rm = 1;
							key = (uint)ch;
						}
						if (_richTextField.emojies.TryGetValue(key, out gchar))
						{
							HtmlElement imageElement = HtmlElement.GetElement(HtmlElementType.Image);
							imageElement.Set("src", gchar.url);
							if (gchar.width != 0)
								imageElement.Set("width", gchar.width);
							if (gchar.height != 0)
								imageElement.Set("height", gchar.height);

							if (textLength <= 1)
							{
								_elements.RemoveAt(i);
								HtmlElement.ReturnElement(element);
								_elements.Insert(i, imageElement);
								break;
							}
							else if (offset == 0)
							{
								element.text = element.text.Substring(rm);
								_elements.Insert(i, imageElement);

								count++;
								textLength -= rm;
								offset = 0;
								i++;
							}
							else if (offset == textLength - rm)
							{
								element.text = element.text.Substring(0, offset);
								_elements.Insert(i + 1, imageElement);

								count++;
								i++;
								break;
							}
							else
							{
								HtmlElement element2 = HtmlElement.GetElement(HtmlElementType.Text);
								element2.text = element.text.Substring(offset + rm);
								element2.format.CopyFrom(element.format);
								_elements.Insert(i + 1, element2);

								element.text = element.text.Substring(0, offset);
								_elements.Insert(i + 1, imageElement);

								count += 2;
								textLength = textLength - offset - rm;
								offset = 0;
								i += 2;
								element = element2;
							}
						}
						else
							offset++;
					}
				}
				i++;
			}
		}

		static List<Vector3> sCachedVerts = new List<Vector3>();
		static List<Vector2> sCachedUVs = new List<Vector2>();
		static List<Color32> sCachedCols = new List<Color32>();
		static GlyphInfo glyph = new GlyphInfo();
		static GlyphInfo glyph2 = new GlyphInfo();

		void BuildMesh()
		{
			_requireUpdateMesh = false;

			if (_textWidth == 0 && _lines.Count == 1)
			{
				graphics.ClearMesh();

				if (_charPositions != null && _charPositions.Count == 0)
					_charPositions.Add(new CharPosition());

				if (_richTextField != null)
					_richTextField.RefreshObjects();

				return;
			}

			int letterSpacing = _textFormat.letterSpacing;
			float rectWidth = _contentRect.width - GUTTER_X * 2;
			TextFormat format = _textFormat;
			_font.SetFormat(format, _fontSizeScale);
			Color32 color = format.color;
			Color32[] gradientColor = format.gradientColor;
			bool boldVertice = format.bold && (_font.customBold || (format.italic && _font.customBoldAndItalic));
			if (_input)
				letterSpacing++;
			if (_charPositions != null)
				_charPositions.Clear();

			if (_fontSizeScale != 1) //不为1，表示在Shrink的作用下，字体变小了，所以要重新请求
				RequestText();

			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector2 u0, u1, u2, u3;
			float specFlag;

			List<Vector3> vertList = sCachedVerts;
			List<Vector2> uvList = sCachedUVs;
			List<Color32> colList = sCachedCols;
			vertList.Clear();
			uvList.Clear();
			colList.Clear();

			HtmlLink currentLink = null;
			float linkStartX = 0;
			int linkStartLine = 0;

			float charX = 0;
			float xIndent;
			float yIndent = 0;
			bool clipped = !_input && _autoSize == AutoSizeType.None;
			bool lineClipped;
			AlignType lineAlign;

			int lineCount = _lines.Count;
			for (int i = 0; i < lineCount; ++i)
			{
				LineInfo line = _lines[i];
				int textLength = line.text.Length;
				if (textLength == 0)
					continue;

				lineClipped = clipped && i != 0 && line.y + line.height > _contentRect.height; //超出区域，剪裁
				lineAlign = format.align;
				if (line.text[0] == E_TAG)
				{
					int elementIndex = (int)line.text[1] - 33;
					HtmlElement element = _elements[elementIndex];
					lineAlign = element.format.align;
				}
				if (lineAlign == AlignType.Center)
					xIndent = (int)((rectWidth - line.width) / 2);
				else if (lineAlign == AlignType.Right)
					xIndent = rectWidth - line.width;
				else
					xIndent = 0;
				if (_input && xIndent < 0)
					xIndent = 0;

				charX = GUTTER_X + xIndent;

				for (int j = 0; j < textLength; j++)
				{
					char ch = line.text[j];
					if (ch == E_TAG)
					{
						int elementIndex = (int)line.text[++j] - 33;
						HtmlElement element = _elements[elementIndex];
						if (element.type == HtmlElementType.Text)
						{
							format = element.format;
							_font.SetFormat(format, _fontSizeScale);
							color = format.color;
							gradientColor = format.gradientColor;
							boldVertice = format.bold && (_font.customBold || (format.italic && _font.customBoldAndItalic));
						}
						else if (element.type == HtmlElementType.Link)
						{
							currentLink = (HtmlLink)element.htmlObject;
							if (currentLink != null)
							{
								element.position = Vector2.zero;
								currentLink.SetPosition(0, 0);
								linkStartX = charX;
								linkStartLine = i;
							}
						}
						else if (element.type == HtmlElementType.LinkEnd)
						{
							if (currentLink != null)
							{
								currentLink.SetArea(linkStartLine, linkStartX, i, charX);
								currentLink = null;
							}
						}
						else
						{
							IHtmlObject htmlObj = element.htmlObject;
							if (htmlObj != null)
							{
								if (_charPositions != null)
								{
									CharPosition cp = new CharPosition();
									cp.lineIndex = (short)i;
									cp.charIndex = (short)j;
									cp.caretIndex = _charPositions.Count;
									cp.vertCount = -1 - elementIndex; //借用
									cp.offsetX = (int)charX;
									_charPositions.Add(cp);
								}
								element.position = new Vector2(charX + 1, line.y + (int)((line.height - htmlObj.height) / 2));
								htmlObj.SetPosition(element.position.x, element.position.y);
								if (lineClipped || clipped && charX + htmlObj.width > _contentRect.width - GUTTER_X)
									element.status |= 1;
								else
									element.status &= 254;
								charX += htmlObj.width + letterSpacing + 2;
							}
						}
						continue;
					}

					if (_font.GetGlyph(ch, glyph))
					{
						if (lineClipped || clipped && charX != 0 && charX + glyph.width > _contentRect.width - GUTTER_X + 0.5f) //超出区域，剪裁
						{
							charX += letterSpacing + glyph.width;
							continue;
						}

						yIndent = (int)((line.height + line.textHeight) / 2 - glyph.height);
						v0.x = charX + glyph.vert.xMin;
						v0.y = -line.y - yIndent + glyph.vert.yMin;
						v1.x = charX + glyph.vert.xMax;
						v1.y = -line.y - yIndent + glyph.vert.yMax;
						u0 = glyph.uvBottomLeft;
						u1 = glyph.uvTopLeft;
						u2 = glyph.uvTopRight;
						u3 = glyph.uvBottomRight;
						specFlag = 0;

						if (_font.hasChannel)
						{
							//对于由BMFont生成的字体，使用这个特殊的设置告诉着色器告诉用的是哪个通道
							if (glyph.channel != 0)
							{
								specFlag = 10 * (glyph.channel - 1);
								u0.x += specFlag;
								u1.x += specFlag;
								u2.x += specFlag;
								u3.x += specFlag;
							}
						}
						else if (_font.canLight && format.bold)
						{
							//对于动态字体，使用这个特殊的设置告诉着色器这个文字不需要点亮（粗体亮度足够，不需要）
							specFlag = 10;
							u0.x += specFlag;
							u1.x += specFlag;
							u2.x += specFlag;
							u3.x += specFlag;
						}

						if (!boldVertice)
						{
							uvList.Add(u0);
							uvList.Add(u1);
							uvList.Add(u2);
							uvList.Add(u3);

							vertList.Add(v0);
							vertList.Add(new Vector3(v0.x, v1.y));
							vertList.Add(new Vector3(v1.x, v1.y));
							vertList.Add(new Vector3(v1.x, v0.y));

							if (gradientColor != null)
							{
								colList.Add(gradientColor[1]);
								colList.Add(gradientColor[0]);
								colList.Add(gradientColor[2]);
								colList.Add(gradientColor[3]);
							}
							else
							{
								colList.Add(color);
								colList.Add(color);
								colList.Add(color);
								colList.Add(color);
							}
						}
						else
						{
							for (int b = 0; b < 4; b++)
							{
								uvList.Add(u0);
								uvList.Add(u1);
								uvList.Add(u2);
								uvList.Add(u3);

								float fx = BOLD_OFFSET[b * 2];
								float fy = BOLD_OFFSET[b * 2 + 1];

								vertList.Add(new Vector3(v0.x + fx, v0.y + fy));
								vertList.Add(new Vector3(v0.x + fx, v1.y + fy));
								vertList.Add(new Vector3(v1.x + fx, v1.y + fy));
								vertList.Add(new Vector3(v1.x + fx, v0.y + fy));

								if (gradientColor != null)
								{
									colList.Add(gradientColor[1]);
									colList.Add(gradientColor[0]);
									colList.Add(gradientColor[2]);
									colList.Add(gradientColor[3]);
								}
								else
								{
									colList.Add(color);
									colList.Add(color);
									colList.Add(color);
									colList.Add(color);
								}
							}
						}

						if (format.underline)
						{
							if (_font.GetGlyph('_', glyph2))
							{
								//取中点的UV
								if (glyph2.uvBottomLeft.x != glyph2.uvBottomRight.x)
									u0.x = (glyph2.uvBottomLeft.x + glyph2.uvBottomRight.x) * 0.5f;
								else
									u0.x = (glyph2.uvBottomLeft.x + glyph2.uvTopLeft.x) * 0.5f;
								u0.x += specFlag;

								if (glyph2.uvBottomLeft.y != glyph2.uvTopLeft.y)
									u0.y = (glyph2.uvBottomLeft.y + glyph2.uvTopLeft.y) * 0.5f;
								else
									u0.y = (glyph2.uvBottomLeft.y + glyph2.uvBottomRight.y) * 0.5f;

								uvList.Add(u0);
								uvList.Add(u0);
								uvList.Add(u0);
								uvList.Add(u0);

								v0.y = -line.y - yIndent + glyph2.vert.yMin - 1;
								v1.y = -line.y - yIndent + glyph2.vert.yMax - 1;

								float tmpX = charX + letterSpacing + glyph.width;

								vertList.Add(new Vector3(charX, v0.y));
								vertList.Add(new Vector3(charX, v1.y));
								vertList.Add(new Vector3(tmpX, v1.y));
								vertList.Add(new Vector3(tmpX, v0.y));

								colList.Add(color);
								colList.Add(color);
								colList.Add(color);
								colList.Add(color);
							}
							else
								format.underline = false;
						}

						if (_charPositions != null)
						{
							CharPosition cp = new CharPosition();
							cp.lineIndex = (short)i;
							cp.charIndex = (short)j;
							cp.caretIndex = _charPositions.Count;
							cp.vertCount = ((boldVertice ? 4 : 1) + (format.underline ? 1 : 0)) * 4;
							cp.offsetX = (int)charX;
							_charPositions.Add(cp);
						}

						charX += letterSpacing + glyph.width;
					}
					else //if GetGlyph failed
					{
						if (_charPositions != null)
						{
							CharPosition cp = new CharPosition();
							cp.lineIndex = (short)i;
							cp.charIndex = (short)j;
							cp.caretIndex = _charPositions.Count;
							cp.vertCount = 0;
							cp.offsetX = (int)charX;
							_charPositions.Add(cp);
						}

						charX += letterSpacing;
					}
				}//text loop
			}//line loop

			if (_charPositions != null)
			{
				CharPosition cp = new CharPosition();
				cp.lineIndex = (short)(lineCount - 1);
				cp.caretIndex = _charPositions.Count;
				cp.offsetX = (int)charX;
				_charPositions.Add(cp);
			}

			bool hasShadow = _shadowOffset.x != 0 || _shadowOffset.y != 0;
			if ((_stroke != 0 || hasShadow) && _font.canOutline)
			{
				int count = vertList.Count;
				int allocCount = count;
				if (_stroke != 0)
					allocCount += count * 4;
				if (hasShadow)
					allocCount += count;
				graphics.Alloc(allocCount);

				Vector3[] vertBuf = graphics.vertices;
				Vector2[] uvBuf = graphics.uv;
				Color32[] colBuf = graphics.colors;

				int start = allocCount - count;
				vertList.CopyTo(0, vertBuf, start, count);
				uvList.CopyTo(0, uvBuf, start, count);
				if (_font.canTint)
				{
					for (int i = 0; i < count; i++)
						colBuf[start + i] = colList[i];
				}
				else
				{
					for (int i = 0; i < count; i++)
						colBuf[start + i] = Color.white;
				}

				Color32 strokeColor = _strokeColor;
				if (_stroke != 0)
				{
					start = allocCount - count * 5;
					for (int j = 0; j < 4; j++)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vert = vertList[i];
							Vector2 u = uvList[i];

							//使用这个特殊的设置告诉着色器这个是描边
							if (_font.canOutline)
								u.y = 10 + u.y;
							uvBuf[start] = u;
							vertBuf[start] = new Vector3(vert.x + STROKE_OFFSET[j * 2] * _stroke, vert.y + STROKE_OFFSET[j * 2 + 1] * _stroke, 0);
							colBuf[start] = strokeColor;
							start++;
						}
					}
				}

				if (hasShadow)
				{
					for (int i = 0; i < count; i++)
					{
						Vector3 vert = vertList[i];
						Vector2 u = uvList[i];

						//使用这个特殊的设置告诉着色器这个是描边
						if (_font.canOutline)
							u.y = 10 + u.y;
						uvBuf[i] = u;
						vertBuf[i] = new Vector3(vert.x + _shadowOffset.x, vert.y - _shadowOffset.y, 0);
						colBuf[i] = strokeColor;
					}
				}
			}
			else
			{
				int count = vertList.Count;
				graphics.Alloc(count);
				vertList.CopyTo(0, graphics.vertices, 0, count);
				uvList.CopyTo(0, graphics.uv, 0, count);
				Color32[] colors = graphics.colors;
				if (_font.canTint)
				{
					for (int i = 0; i < count; i++)
						colors[i] = colList[i];
				}
				else
				{
					for (int i = 0; i < count; i++)
						colors[i] = Color.white;
				}
			}

			graphics.FillTriangles();
			graphics.UpdateMesh();

			if (_richTextField != null)
				_richTextField.RefreshObjects();
		}

		void Cleanup()
		{
			if (_richTextField != null)
				_richTextField.CleanupObjects();

			HtmlElement.ReturnElements(_elements);
			LineInfo.Return(_lines);
			_textWidth = 0;
			_textHeight = 0;
			if (_charPositions != null)
				_charPositions.Clear();
		}

		void ApplyVertAlign()
		{
			int oldOffset = _yOffset;
			if (_autoSize == AutoSizeType.Both || _autoSize == AutoSizeType.Height
				|| _verticalAlign == VertAlignType.Top)
				_yOffset = 0;
			else
			{
				float dh;
				if (_textHeight == 0)
					dh = _contentRect.height - this.textFormat.size;
				else
					dh = _contentRect.height - _textHeight;
				if (dh < 0)
					dh = 0;
				if (_verticalAlign == VertAlignType.Middle)
					_yOffset = (int)(dh / 2);
				else
					_yOffset = (int)dh;
			}

			if (oldOffset != _yOffset)
			{
				int cnt = _lines.Count;
				for (int i = 0; i < cnt; i++)
					_lines[i].y = _lines[i].y2 + _yOffset;
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public class LineInfo
		{
			/// <summary>
			/// 
			/// </summary>
			public float width;

			/// <summary>
			/// 
			/// </summary>
			public float height;
			/// <summary>
			/// 
			/// </summary>
			public float textHeight;

			/// <summary>
			/// 
			/// </summary>
			public string text;

			/// <summary>
			/// 
			/// </summary>
			public float y;

			internal float y2;

			static Stack<LineInfo> pool = new Stack<LineInfo>();

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public static LineInfo Borrow()
			{
				if (pool.Count > 0)
				{
					LineInfo ret = pool.Pop();
					ret.width = 0;
					ret.height = 0;
					ret.textHeight = 0;
					ret.text = null;
					ret.y = 0;
					return ret;
				}
				else
					return new LineInfo();
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="value"></param>
			public static void Return(LineInfo value)
			{
				pool.Push(value);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="values"></param>
			public static void Return(List<LineInfo> values)
			{
				int cnt = values.Count;
				for (int i = 0; i < cnt; i++)
					pool.Push(values[i]);

				values.Clear();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public struct CharPosition
		{
			/// <summary>
			/// 字符所在的行索引
			/// </summary>
			public short lineIndex;

			/// <summary>
			/// 字符在所在行的索引
			/// </summary>
			public short charIndex;

			/// <summary>
			/// 字符的x偏移
			/// </summary>
			public int offsetX;

			/// <summary>
			/// 字符占用的顶点数量。如果小于0，用于表示一个图片。对应的图片索引为-vertCount-1
			/// </summary>
			public int vertCount;

			internal int caretIndex;
		}
	}
}
