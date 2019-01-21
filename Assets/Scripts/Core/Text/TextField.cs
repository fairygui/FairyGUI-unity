using System.Collections.Generic;
using System.Text;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class TextField : DisplayObject, IMeshFactory
	{
		VertAlignType _verticalAlign;
		TextFormat _textFormat;
		bool _input;
		string _text;
		AutoSizeType _autoSize;
		bool _wordWrap;
		bool _singleLine;
		bool _html;
#if RTL_TEXT_SUPPORT
		RTLSupport.DirectionType _textDirection = RTLSupport.DirectionType.UNKNOW;
#endif
		int _maxWidth;

		int _stroke;
		Color _strokeColor;
		Vector2 _shadowOffset;

		List<HtmlElement> _elements;
		List<LineInfo> _lines;
		List<CharPosition> _charPositions;

		BaseFont _font;
		float _textWidth;
		float _textHeight;
		float _minHeight;
		bool _textChanged;
		int _yOffset;
		float _fontSizeScale;
		float _renderScale;
		string _parsedText;
		bool _updatingSize; //防止重复调用BuildLines

		RichTextField _richTextField;

		const int GUTTER_X = 2;
		const int GUTTER_Y = 2;
		static float[] STROKE_OFFSET = new float[]
		{
			 -1, 0, 1, 0,
			0, -1, 0, 1,
			-1, -1, 1, -1,
			-1, 1, 1, 1
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
			_strokeColor = Color.black;
			_fontSizeScale = 1;
			_renderScale = UIContentScaler.scaleFactor;

			_wordWrap = false;
			_text = string.Empty;
			_parsedText = string.Empty;

			_elements = new List<HtmlElement>(0);
			_lines = new List<LineInfo>(1);

			CreateGameObject("TextField");
			graphics = new NGraphics(gameObject);
			graphics.meshFactory = this;
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
				_textChanged = true;
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
					graphics.SetShaderAndTexture(_font.shader, _font.mainTexture);
				}
				if (!string.IsNullOrEmpty(_text))
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
					if (!string.IsNullOrEmpty(_text))
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

		public string parsedText
		{
			get { return _parsedText; }
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
					graphics.SetMeshDirty();
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
					graphics.SetMeshDirty();
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
				graphics.SetMeshDirty();
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

		/// <summary>
		/// 
		/// </summary>
		public int maxWidth
		{
			get { return _maxWidth; }
			set
			{
				if (_maxWidth != value)
				{
					_maxWidth = value;
					_textChanged = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public List<HtmlElement> htmlElements
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _elements;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public List<LineInfo> lines
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _lines;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public List<CharPosition> charPositions
		{
			get
			{
				if (_textChanged)
					BuildLines();

				graphics.UpdateMesh();

				return _charPositions;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public RichTextField richTextField
		{
			get { return _richTextField; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Redraw()
		{
			if (_font == null)
			{
				_font = FontManager.GetFont(UIConfig.defaultFont);
				graphics.SetShaderAndTexture(_font.shader, _font.mainTexture);
				_textChanged = true;
			}

			if (_font.keepCrisp && _renderScale != UIContentScaler.scaleFactor)
				_textChanged = true;

			if (_font.mainTexture != graphics.texture)
			{
				if (!_textChanged)
					RequestText();
				graphics.texture = _font.mainTexture;
			}

			if (_textChanged)
				BuildLines();

			return graphics.UpdateMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		public bool HasCharacter(char ch)
		{
			return _font.HasCharacter(ch);
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
				_minHeight = _contentRect.height;

				if (_wordWrap && widthChanged)
					_textChanged = true;
				else if (_autoSize != AutoSizeType.None)
					graphics.SetMeshDirty();

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
			if (_richTextField == null) //如果是richTextField，会在update前主动调用了Redraw
				Redraw();
			base.Update(context);
		}

		/// <summary>
		/// 准备字体纹理
		/// </summary>
		void RequestText()
		{
			if (!_html)
			{
				_font.SetFormat(_textFormat, _fontSizeScale);
				_font.PrepareCharacters(_parsedText);
				_font.PrepareCharacters("_-*");
			}
			else
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

			if ((_font is DynamicFont) && DynamicFont.textRebuildFlag)
				graphics.texture = _font.mainTexture;
		}

		void BuildLines()
		{
			_textChanged = false;
			graphics.SetMeshDirty();
			_renderScale = UIContentScaler.scaleFactor;
			_fontSizeScale = 1;

			Cleanup();

			if (_text.Length == 0)
			{
				LineInfo emptyLine = LineInfo.Borrow();
				emptyLine.width = emptyLine.height = 0;
				emptyLine.charIndex = emptyLine.charCount = 0;
				emptyLine.y = emptyLine.y2 = GUTTER_Y;
				_lines.Add(emptyLine);

				_textWidth = _textHeight = 0;
			}
			else
			{
				ParseText();
				BuildLines2();

				if (_autoSize == AutoSizeType.Shrink)
					DoShrink();
			}

			if (!_input && _autoSize == AutoSizeType.Both)
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
				float h = _textHeight;
				if (_input && h < _minHeight)
					h = _minHeight;
				if (_richTextField != null)
					_richTextField.height = h;
				else
					this.height = h;
				_updatingSize = false;
			}

			_yOffset = 0;
			ApplyVertAlign();
		}

		void ParseText()
		{
#if RTL_TEXT_SUPPORT
			_textDirection = RTLSupport.DetectTextDirection(_text);
#endif
			if (_html)
			{
				HtmlParser.inst.Parse(_text, _textFormat, _elements,
					_richTextField != null ? _richTextField.htmlParseOptions : null);

				_parsedText = string.Empty;
			}
			else
				_parsedText = _text;

			int elementCount = _elements.Count;
			if (elementCount == 0)
			{
#if RTL_TEXT_SUPPORT
				if (_textDirection != RTLSupport.DirectionType.UNKNOW)
					_parsedText = RTLSupport.DoMapping(_parsedText);
#endif

				bool flag = _input || _richTextField != null && _richTextField.emojies != null;
				if (!flag)
				{
					//检查文本中是否有需要转换的字符，如果没有，节省一个new StringBuilder的操作。
					int cnt = _parsedText.Length;
					for (int i = 0; i < cnt; i++)
					{
						char ch = _parsedText[i];
						if (ch == '\r' || ch == '\t' || char.IsHighSurrogate(ch))
						{
							flag = true;
							break;
						}
					}
				}

				if (flag)
				{
					StringBuilder buffer = new StringBuilder();
					ParseText(buffer, _parsedText, -1);
					elementCount = _elements.Count;
					_parsedText = buffer.ToString();
				}
			}
			else
			{
				StringBuilder buffer = new StringBuilder();
				int i = 0;
				while (i < elementCount)
				{
					HtmlElement element = _elements[i];
					element.charIndex = buffer.Length;
					if (element.type == HtmlElementType.Text)
					{
#if RTL_TEXT_SUPPORT
						if (_textDirection != RTLSupport.DirectionType.UNKNOW)
							element.text = RTLSupport.DoMapping(element.text);
#endif
						i = ParseText(buffer, element.text, i);
						elementCount = _elements.Count;
					}
					else if (element.isEntity)
						buffer.Append(' ');
					i++;
				}
				_parsedText = buffer.ToString();
			}
		}

		void BuildLines2()
		{
			int letterSpacing = _textFormat.letterSpacing;
			int lineSpacing = _textFormat.lineSpacing - 1;
			float rectWidth = _contentRect.width - GUTTER_X * 2;
			float glyphWidth = 0, glyphHeight = 0;
			short wordChars = 0;
			float wordStart = 0;
			bool wordPossible = false;
			int supSpace = 0, subSpace = 0;

			TextFormat format = _textFormat;
			_font.SetFormat(format, _fontSizeScale);
			bool wrap;
			if (_input)
			{
#if RTL_TEXT_SUPPORT
				if (_textDirection == RTLSupport.DirectionType.UNKNOW)
					letterSpacing++;
#else
				letterSpacing++;
#endif
				wrap = !_singleLine;
			}
			else
			{
				wrap = _wordWrap && !_singleLine;
				if (_maxWidth > 0)
				{
					wrap = true;
					rectWidth = _maxWidth - GUTTER_X * 2;
				}
			}
			_textWidth = _textHeight = 0;

			RequestText();

			int elementCount = _elements.Count;
			int elementIndex = 0;
			HtmlElement element = null;
			if (elementCount > 0)
				element = _elements[elementIndex];
			int textLength = _parsedText.Length;

			LineInfo line = LineInfo.Borrow();
			_lines.Add(line);
			line.y = line.y2 = GUTTER_Y;

			for (int charIndex = 0; charIndex < textLength; charIndex++)
			{
				char ch = _parsedText[charIndex];

				glyphWidth = glyphHeight = 0;

				while (element != null && element.charIndex == charIndex)
				{
					if (element.type == HtmlElementType.Text)
					{
						format = element.format;
						_font.SetFormat(format, _fontSizeScale);
					}
					else
					{
						IHtmlObject htmlObject = element.htmlObject;
						if (_richTextField != null && htmlObject == null)
						{
							element.space = (int)(rectWidth - line.width - 4);
							htmlObject = _richTextField.htmlPageContext.CreateObject(_richTextField, element);
							element.htmlObject = htmlObject;
						}
						if (htmlObject != null)
						{
							glyphWidth = (int)htmlObject.width;
							glyphHeight = (int)htmlObject.height;

							glyphWidth += 2;
						}

						if (element.isEntity)
							ch = '\0'; //字符只是用作占位，不需要显示
					}

					elementIndex++;
					if (elementIndex < elementCount)
						element = _elements[elementIndex];
					else
						element = null;
				}

				line.charCount++;
				if (ch == '\0' || ch == '\n')
				{
					wordChars = 0;
					wordPossible = false;
				}
				else
				{
					if (char.IsWhiteSpace(ch))
					{
						wordChars = 0;
						wordPossible = true;
					}
					else if (wordPossible && (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9' || ch == '.' || ch == '"' || ch == '\''
#if RTL_TEXT_SUPPORT
						|| (_textDirection != RTLSupport.DirectionType.UNKNOW && RTLSupport.IsArabicLetter(ch))
#endif
					))
					{
						if (wordChars == 0)
							wordStart = line.width;
						else if (wordChars > 10)
							wordChars = short.MinValue;

						wordChars++;
					}
					else
					{
						wordChars = 0;
						wordPossible = false;
					}

					if (_font.GetGlyphSize(ch, out glyphWidth, out glyphHeight))
					{
						if (glyphHeight > line.textHeight)
							line.textHeight = glyphHeight;

						if (format.specialStyle == TextFormat.SpecialStyle.Subscript)
							subSpace = (int)(glyphHeight * 0.333f);
						else if (format.specialStyle == TextFormat.SpecialStyle.Superscript)
							supSpace = (int)(glyphHeight * 0.333f);
					}
				}

				if (glyphWidth > 0)
				{
					if (glyphHeight > line.height)
						line.height = glyphHeight;

					if (line.width != 0)
						line.width += letterSpacing;
					line.width += glyphWidth;
				}

				if (ch == '\n' || wrap && line.width > rectWidth && format.specialStyle == TextFormat.SpecialStyle.None)
				{
					if (line.textHeight == 0)
					{
						if (line.height == 0)
						{
							if (_lines.Count == 1)
								line.height = format.size;
							else
								line.height = _lines[_lines.Count - 2].height;
						}
						line.textHeight = line.height;
					}
					if (supSpace != 0)
						line.height = Mathf.Max(line.textHeight + supSpace, line.height);

					LineInfo newLine = LineInfo.Borrow();
					_lines.Add(newLine);
					newLine.y = line.y + (line.height + lineSpacing);
					if (newLine.y < GUTTER_Y)
						newLine.y = GUTTER_Y;
					newLine.y2 = newLine.y;

					if (ch == '\n' || line.charCount == 1) //the line cannt fit even a char
					{
						wordChars = 0;
						wordPossible = false;
					}
					else if (wordChars > 0 && wordStart > 0) //if word had broken, move it to new line
					{
						newLine.charCount = wordChars;
						newLine.width = line.width - wordStart;
						newLine.height = line.height;
						newLine.textHeight = line.textHeight;

						line.charCount -= wordChars;
						line.width = wordStart;

						wordStart = 0;
					}
					else //move last char to new line
					{
						newLine.charCount = 1;
						newLine.width = glyphWidth;
						newLine.height = glyphHeight;
						if (ch != '\0')
							newLine.textHeight = newLine.height;

						line.charCount -= 1;
						line.width = line.width - (glyphWidth + letterSpacing);

						wordChars = 0;
						wordPossible = false;
					}

					newLine.charIndex = line.charIndex + line.charCount;
					if (line.width > _textWidth)
						_textWidth = line.width;

					if (subSpace != 0 && subSpace > lineSpacing)
						supSpace = subSpace - (lineSpacing > 0 ? lineSpacing : 0);
					subSpace = 0;

					line = newLine;
				}
			}

			line = _lines[_lines.Count - 1];
			if (line.textHeight == 0)
			{
				if (line.height == 0)
				{
					if (_lines.Count == 1)
						line.height = format.size;
					else
						line.height = _lines[_lines.Count - 2].height;
				}
				line.textHeight = line.height;
			}
			if (subSpace > 0)
				line.height += subSpace;

			if (line.width > _textWidth)
				_textWidth = line.width;
			if (_textWidth > 0)
				_textWidth += GUTTER_X * 2;
			_textHeight = line.y + line.height + GUTTER_Y;

			_textWidth = Mathf.CeilToInt(_textWidth);
			_textHeight = Mathf.CeilToInt(_textHeight);
		}

		void DoShrink()
		{
			if (_lines.Count > 1 && _textHeight > _contentRect.height)
			{
				//多行的情况，涉及到自动换行，得用二分法查找最合适的比例，会消耗多一点计算资源
				int low = 0;
				int high = _textFormat.size;

				//先尝试猜测一个比例
				_fontSizeScale = Mathf.Sqrt(_contentRect.height / _textHeight);
				int cur = Mathf.FloorToInt(_fontSizeScale * _textFormat.size);

				while (true)
				{
					LineInfo.Return(_lines);
					BuildLines2();

					if (_textWidth > _contentRect.width || _textHeight > _contentRect.height)
						high = cur;
					else
						low = cur;
					if (high - low > 1 || high != low && cur == high)
					{
						cur = low + (high - low) / 2;
						_fontSizeScale = (float)cur / _textFormat.size;
					}
					else
						break;
				}
			}
			else if (_textWidth > _contentRect.width)
			{
				_fontSizeScale = _contentRect.width / _textWidth;

				LineInfo.Return(_lines);
				BuildLines2();

				if (_textWidth > _contentRect.width) //如果还超出，缩小一点再来一次
				{
					int size = Mathf.FloorToInt(_textFormat.size * _fontSizeScale);
					size--;
					_fontSizeScale = (float)size / _textFormat.size;

					LineInfo.Return(_lines);
					BuildLines2();
				}
			}
		}

		int ParseText(StringBuilder buffer, string source, int elementIndex)
		{
			int textLength = source.Length;
			int j = 0;
			int appendPos = 0;
			bool hasEmojies = _richTextField != null && _richTextField.emojies != null;
			while (j < textLength)
			{
				char ch = source[j];
				if (ch == '\r')
				{
					buffer.Append(source, appendPos, j - appendPos);
					if (j != textLength - 1 && source[j + 1] == '\n')
						j++;
					appendPos = j + 1;
					buffer.Append('\n');
				}
				else if (ch == '\t')
				{
					buffer.Append(source, appendPos, j - appendPos);
					appendPos = j + 1;
					buffer.Append(' ');
				}
				else
				{
					bool highSurrogate = char.IsHighSurrogate(ch);
					if (hasEmojies)
					{
						uint emojiKey = 0;
						Emoji emoji;
						if (highSurrogate)
							emojiKey = ((uint)source[j + 1] & 0x03FF) + ((((uint)ch & 0x03FF) + 0x40) << 10);
						else
							emojiKey = ch;
						if (_richTextField.emojies.TryGetValue(emojiKey, out emoji))
						{
							HtmlElement imageElement = HtmlElement.GetElement(HtmlElementType.Image);
							imageElement.Set("src", emoji.url);
							if (emoji.width != 0)
								imageElement.Set("width", emoji.width);
							if (emoji.height != 0)
								imageElement.Set("height", emoji.height);
							if (highSurrogate)
								imageElement.text = source.Substring(j, 2);
							else
								imageElement.text = source.Substring(j, 1);
							_elements.Insert(++elementIndex, imageElement);

							buffer.Append(source, appendPos, j - appendPos);
							appendPos = j;
							imageElement.charIndex = buffer.Length;
						}
					}

					if (highSurrogate)
					{
						buffer.Append(source, appendPos, j - appendPos);
						appendPos = j + 2;
						j++;//跳过lowSurrogate
						buffer.Append(' ');
					}
				}
				j++;
			}
			if (appendPos < textLength)
				buffer.Append(source, appendPos, j - appendPos);

			return elementIndex;
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			if (_textWidth == 0 && _lines.Count == 1)
			{
				if (_charPositions != null)
				{
					_charPositions.Clear();
					_charPositions.Add(new CharPosition());
				}

				if (_richTextField != null)
					_richTextField.RefreshObjects();

				return;
			}

			int letterSpacing = _textFormat.letterSpacing;
			float rectWidth = _contentRect.width - GUTTER_X * 2;
			TextFormat format = _textFormat;
			Color32 color = format.color;
			_font.SetFormat(format, _fontSizeScale);
			Color32[] gradientColor = format.gradientColor;
			bool boldVertice = format.bold && (_font.customBold || (format.italic && _font.customBoldAndItalic));

			if (_input)
			{
#if RTL_TEXT_SUPPORT
				if (_textDirection == RTLSupport.DirectionType.UNKNOW)
					letterSpacing++;
#else
				letterSpacing++;
#endif
			}

			if (_charPositions != null)
				_charPositions.Clear();

			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector4 u0, u1, u2, u3;

			List<Vector3> vertList = vb.vertices;
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
			List<Vector4> uvList = vb.uv0;
#else
			List<Vector2> uvList = vb.uv0;
#endif
			List<Color32> colList = vb.colors;

			HtmlLink currentLink = null;
			float linkStartX = 0;
			int linkStartLine = 0;

			float charX = 0;
			float xIndent;
			int yIndent = 0;
			bool clipped = !_input && _autoSize == AutoSizeType.None;
			bool lineClipped;
			AlignType lineAlign;
			VertAlignType lineVAlign = UIConfig.richTextRowVerticalAlign;
			float lastGlyphHeight = 0;
			GlyphInfo glyph = new GlyphInfo();

			int elementIndex = 0;
			int elementCount = _elements.Count;
			HtmlElement element = null;
			if (elementCount > 0)
				element = _elements[elementIndex];

			int lineCount = _lines.Count;
			for (int i = 0; i < lineCount; ++i)
			{
				LineInfo line = _lines[i];
				if (line.charCount == 0)
					continue;

				lineClipped = clipped && i != 0 && line.y + line.height > _contentRect.height; //超出区域，剪裁
				lineAlign = format.align;
				if (element != null && element.charIndex == line.charIndex)
					lineAlign = element.format.align;
				else
					lineAlign = format.align;
#if RTL_TEXT_SUPPORT
				if (_textDirection == RTLSupport.DirectionType.RTL)
				{
					if (lineAlign == AlignType.Center)
						xIndent = (int)((rectWidth + line.width) / 2);
					else if (lineAlign == AlignType.Right)
						xIndent = rectWidth;
					else
						xIndent = Mathf.Ceil(line.width) + GUTTER_X * 2;

					if (xIndent > rectWidth)
						xIndent = rectWidth;

					charX = xIndent - GUTTER_X;
				}
				else
#endif
				{
					if (lineAlign == AlignType.Center)
						xIndent = (int)((rectWidth - line.width) / 2);
					else if (lineAlign == AlignType.Right)
						xIndent = rectWidth - line.width;
					else
						xIndent = 0;

					if (xIndent < 0)
						xIndent = 0;

					charX = GUTTER_X + xIndent;
				}

#if RTL_TEXT_SUPPORT
				string strLine = "";
				int charCount = line.charCount;
				if (_textDirection != RTLSupport.DirectionType.UNKNOW)
				{
					strLine = _parsedText.Substring(line.charIndex, line.charCount);
					if (_textDirection == RTLSupport.DirectionType.RTL)
						strLine = RTLSupport.ConvertLineR(strLine);
					else
						strLine = RTLSupport.ConvertLineL(strLine);
					charCount = strLine.Length;
				}
				for (int j = 0; j < charCount; j++)
				{
					int charIndex = line.charIndex + j;
					char ch = _textDirection != RTLSupport.DirectionType.UNKNOW ? strLine[j] : _parsedText[charIndex];
#else
				for (int j = 0; j < line.charCount; j++)
				{
					int charIndex = line.charIndex + j;
					char ch = _parsedText[charIndex];
#endif
					while (element != null && charIndex == element.charIndex)
					{
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
#if RTL_TEXT_SUPPORT
								if (_textDirection == RTLSupport.DirectionType.RTL)
									charX -= htmlObj.width - 2;
#endif

								if (_charPositions != null)
								{
									CharPosition cp = new CharPosition();
									cp.lineIndex = (short)i;
									cp.charIndex = _charPositions.Count;
									cp.vertCount = (short)(-1 - elementIndex); //借用
									cp.offsetX = (int)charX;
									_charPositions.Add(cp);
								}
								if (lineVAlign == VertAlignType.Bottom)
									yIndent = (int)(line.height - htmlObj.height);
								else if (lineVAlign == VertAlignType.Middle)
									yIndent = (int)((line.height - htmlObj.height) / 2);
								else
									yIndent = 0;

								element.position = new Vector2(charX + 1, line.y + yIndent);
								htmlObj.SetPosition(element.position.x, element.position.y);
								if (lineClipped || clipped && (element.position.x < GUTTER_X || element.position.x + htmlObj.width > _contentRect.width - GUTTER_X))
									element.status |= 1;
								else
									element.status &= 254;

#if RTL_TEXT_SUPPORT
								if (_textDirection == RTLSupport.DirectionType.RTL)
									charX -= letterSpacing;
								else
#endif
								{
									charX += htmlObj.width + letterSpacing + 2;
								}
							}
						}

						if (element.isEntity)
							ch = '\0';

						elementIndex++;
						if (elementIndex < elementCount)
							element = _elements[elementIndex];
						else
							element = null;
					}

					if (ch == '\0')
						continue;

					if (_font.GetGlyph(ch, ref glyph))
					{
#if RTL_TEXT_SUPPORT
						if (_textDirection == RTLSupport.DirectionType.RTL)
						{
							if (lineClipped || clipped && (rectWidth < 7 || charX != (xIndent - GUTTER_X)) && charX < GUTTER_X - 0.5f) //超出区域，剪裁
							{
								charX -= (letterSpacing + glyph.width);
								continue;
							}

							charX -= glyph.width;
						}
						else
#endif
						{
							if (lineClipped || clipped && (rectWidth < 7 || charX != (GUTTER_X + xIndent)) && charX + glyph.width > _contentRect.width - GUTTER_X + 0.5f) //超出区域，剪裁
							{
								charX += letterSpacing + glyph.width;
								continue;
							}
						}

						if (lineVAlign == VertAlignType.Bottom)
							yIndent = (int)(line.height - glyph.height);
						else if (lineVAlign == VertAlignType.Middle)
							yIndent = (int)((line.height + line.textHeight) / 2 - glyph.height);
						else
							yIndent = (int)(line.textHeight - glyph.height);

						if (format.specialStyle == TextFormat.SpecialStyle.Subscript)
							yIndent += (int)(glyph.height * 0.333f);
						else if (format.specialStyle == TextFormat.SpecialStyle.Superscript)
							yIndent -= (int)(lastGlyphHeight - glyph.height * 0.667f);
						else
							lastGlyphHeight = glyph.height;

						v0.x = charX + glyph.vertMin.x;
						v0.y = line.y + yIndent + glyph.vertMin.y;
						v1.x = charX + glyph.vertMax.x;
						v1.y = line.y + yIndent + glyph.vertMax.y;
						u0 = glyph.uvBottomLeft;
						u1 = glyph.uvTopLeft;
						u2 = glyph.uvTopRight;
						u3 = glyph.uvBottomRight;
						u0.w = u1.w = u2.w = u3.w = 1;

						if (!boldVertice)
						{
							uvList.Add(u0);
							uvList.Add(u1);
							uvList.Add(u2);
							uvList.Add(u3);

							vertList.Add(new Vector3(v0.x, -v1.y, 0));
							vertList.Add(new Vector3(v0.x, -v0.y, 0));
							vertList.Add(new Vector3(v1.x, -v0.y, 0));
							vertList.Add(new Vector3(v1.x, -v1.y, 0));

							if (!_font.canTint)
							{
								colList.Add(Color.white);
								colList.Add(Color.white);
								colList.Add(Color.white);
								colList.Add(Color.white);
							}
							else if (gradientColor != null)
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

								vertList.Add(new Vector3(v0.x + fx, -(v1.y + fy), 0));
								vertList.Add(new Vector3(v0.x + fx, -(v0.y + fy), 0));
								vertList.Add(new Vector3(v1.x + fx, -(v0.y + fy), 0));
								vertList.Add(new Vector3(v1.x + fx, -(v1.y + fy), 0));

								if (!_font.canTint)
								{
									colList.Add(Color.white);
									colList.Add(Color.white);
									colList.Add(Color.white);
									colList.Add(Color.white);
								}
								else if (gradientColor != null)
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
							float glyphWidth = glyph.width;
							if (_font.GetGlyph('_', ref glyph))
							{
								glyph.width = glyphWidth;
								//取中点的UV
								if (glyph.uvBottomLeft.x != glyph.uvBottomRight.x)
									u0.x = (glyph.uvBottomLeft.x + glyph.uvBottomRight.x) * 0.5f;
								else
									u0.x = (glyph.uvBottomLeft.x + glyph.uvTopLeft.x) * 0.5f;

								if (glyph.uvBottomLeft.y != glyph.uvTopLeft.y)
									u0.y = (glyph.uvBottomLeft.y + glyph.uvTopLeft.y) * 0.5f;
								else
									u0.y = (glyph.uvBottomLeft.y + glyph.uvBottomRight.y) * 0.5f;

								uvList.Add(u0);
								uvList.Add(u0);
								uvList.Add(u0);
								uvList.Add(u0);

								v0.y = line.y + yIndent + glyph.vertMin.y + 1;
								v1.y = line.y + yIndent + glyph.vertMax.y + 1;
								if (v1.y - v0.y > 2)
									v1.y = v0.y + 2;

								float tmpX = charX + letterSpacing + glyph.width;

								vertList.Add(new Vector3(charX, -v1.y, 0));
								vertList.Add(new Vector3(charX, -v0.y, 0));
								vertList.Add(new Vector3(tmpX, -v0.y, 0));
								vertList.Add(new Vector3(tmpX, -v1.y, 0));

								if (!_font.canTint)
								{
									colList.Add(Color.white);
									colList.Add(Color.white);
									colList.Add(Color.white);
									colList.Add(Color.white);
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
								format.underline = false;
						}

						if (_charPositions != null)
						{
							CharPosition cp = new CharPosition();
							cp.lineIndex = (short)i;
							cp.charIndex = _charPositions.Count;
							cp.vertCount = (short)(((boldVertice ? 4 : 1) + (format.underline ? 1 : 0)) * 4);
							cp.offsetX = (int)charX;
							_charPositions.Add(cp);
						}
#if RTL_TEXT_SUPPORT
						if (_textDirection == RTLSupport.DirectionType.RTL)
						{
							charX -= letterSpacing;
						}
						else
#endif
						{

							charX += letterSpacing + glyph.width;
						}
					}
					else //if GetGlyph failed
					{
						if (_charPositions != null)
						{
							CharPosition cp = new CharPosition();
							cp.lineIndex = (short)i;
							cp.charIndex = _charPositions.Count;
							cp.vertCount = 0;
							cp.offsetX = (int)charX;
							_charPositions.Add(cp);
						}

#if RTL_TEXT_SUPPORT
						if (_textDirection == RTLSupport.DirectionType.RTL)
						{
							charX -= letterSpacing;
						}
						else
#endif
						{
							charX += letterSpacing;
						}
					}
				}//text loop
			}//line loop

			if (element != null && element.type == HtmlElementType.LinkEnd && currentLink != null)
				currentLink.SetArea(linkStartLine, linkStartX, lineCount - 1, charX);

			if (_charPositions != null)
			{
				CharPosition cp = new CharPosition();
				cp.lineIndex = (short)(lineCount - 1);
				cp.charIndex = _charPositions.Count;
				cp.offsetX = (int)charX;
				_charPositions.Add(cp);
			}

			int count = vertList.Count;
			if (count > 65000)
			{
				Debug.LogWarning("Text is too large. A mesh may not have more than 65000 vertices.");
				vertList.RemoveRange(65000, count - 65000);
				count = 65000;
			}

			bool hasShadow = _shadowOffset.x != 0 || _shadowOffset.y != 0;
			int allocCount = count;
			int drawDirs = 0;
			if (_stroke != 0)
			{
				drawDirs = UIConfig.enhancedTextOutlineEffect ? 8 : 4;
				allocCount += count * drawDirs;
			}
			if (hasShadow)
				allocCount += count;
			if (allocCount > 65000)
			{
				Debug.LogWarning("Text is too large. Outline/shadow effect cannot be completed.");
				allocCount = count;
			}

			if (allocCount != count)
			{
				VertexBuffer vb2 = VertexBuffer.Begin();
				List<Vector3> vertList2 = vb2.vertices;
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
				List<Vector4> uvList2 = vb2.uv0;
#else
				List<Vector2> uvList2 = vb2.uv0;
#endif
				List<Color32> colList2 = vb2.colors;

				Color32 strokeColor = _strokeColor;
				if (_stroke != 0)
				{
					for (int j = 0; j < drawDirs; j++)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vert = vertList[i];
							Vector4 u = uvList[i];

							uvList2.Add(u);
							vertList2.Add(new Vector3(vert.x + STROKE_OFFSET[j * 2] * _stroke, vert.y + STROKE_OFFSET[j * 2 + 1] * _stroke, 0));
							colList2.Add(strokeColor);
						}
					}
				}

				if (hasShadow)
				{
					for (int i = 0; i < count; i++)
					{
						Vector3 vert = vertList[i];
						Vector4 u = uvList[i];

						uvList2.Add(u);
						vertList2.Add(new Vector3(vert.x + _shadowOffset.x, vert.y - _shadowOffset.y, 0));
						colList2.Add(strokeColor);
					}
				}

				vb.Insert(vb2);
				vb2.End();
			}

			vb.AddTriangles();

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
			_parsedText = string.Empty;
#if RTL_TEXT_SUPPORT
			_textDirection = RTLSupport.DirectionType.UNKNOW;
#endif
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

				graphics.SetMeshDirty();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public class LineInfo
		{
			/// <summary>
			/// 行的宽度
			/// </summary>
			public float width;

			/// <summary>
			/// 行的高度
			/// </summary>
			public float height;

			/// <summary>
			/// 行内文本的高度
			/// </summary>
			public float textHeight;

			/// <summary>
			/// 行首的字符索引
			/// </summary>
			public int charIndex;

			/// <summary>
			/// 行包括的字符个数
			/// </summary>
			public short charCount;

			/// <summary>
			/// 行的y轴位置
			/// </summary>
			public float y;

			/// <summary>
			/// 行的y轴位置的备份
			/// </summary>
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
					ret.width = ret.height = ret.textHeight = 0;
					ret.y = ret.y2 = 0;
					ret.charIndex = ret.charCount = 0;
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
			/// 字符索引
			/// </summary>
			public int charIndex;

			/// <summary>
			/// 字符所在的行索引
			/// </summary>
			public short lineIndex;

			/// <summary>
			/// 字符的x偏移
			/// </summary>
			public int offsetX;

			/// <summary>
			/// 字符占用的顶点数量。如果小于0，用于表示一个图片。对应的图片索引为-vertCount-1
			/// </summary>
			public short vertCount;
		}
	}
}
