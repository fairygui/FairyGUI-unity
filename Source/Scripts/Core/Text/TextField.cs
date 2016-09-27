using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	public class TextField : DisplayObject
	{
		public EventListener onFocusIn { get; private set; }
		public EventListener onFocusOut { get; private set; }
		public EventListener onChanged { get; private set; }

		public RichTextField richTextField { get; internal set; }

		AlignType _align;
		VertAlignType _verticalAlign;
		TextFormat _textFormat;
		bool _input;
		string _text;
		bool _autoSize;
		bool _wordWrap;
		bool _displayAsPassword;
		bool _singleLine;
		int _maxLength;
		bool _html;
		int _caretPosition;
		CharPosition? _selectionStart;
		InputCaret _caret;
		Highlighter _highlighter;
		int _stroke;
		Color _strokeColor;
		Vector2 _shadowOffset;
		string _restrict;
		Regex _restrictPattern;

		List<HtmlElement> _elements;
		List<LineInfo> _lines;
		List<IHtmlObject> _toCollect;
		List<int> _charPositions;

#pragma warning disable 0649
		IMobileInputAdapter _mobileInputAdapter;
#pragma warning restore 0649

		BaseFont _font;
		float _textWidth;
		float _textHeight;
		bool _textChanged;

		EventCallback1 _touchMoveDelegate;
		EventCallback0 _onChangedDelegate;

		static EventCallback0 inputCompleteDelegate = () =>
		{
			//将促使一个onFocusOut事件的调用。如果不focus-out，则在键盘关闭后，玩家再次点击文本，focus-in不会触发，键盘不会打开
			//另外也使开发者有机会得到一个键盘关闭的通知
			Stage.inst.focus = null;
		};

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
			_optimizeNotTouchable = true;

			_textFormat = new TextFormat();
			_textFormat.size = 12;
			_textFormat.lineSpacing = 3;
			_strokeColor = new Color(0, 0, 0, 1);

			_wordWrap = true;
			_displayAsPassword = false;
			_maxLength = int.MaxValue;
			_text = string.Empty;

			_elements = new List<HtmlElement>(1);
			_lines = new List<LineInfo>(1);

			CreateGameObject("TextField");
			graphics = new NGraphics(gameObject);

			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");

			_touchMoveDelegate = __touchMove;
			_onChangedDelegate = OnChanged;
		}

		public TextFormat textFormat
		{
			get { return _textFormat; }
			set
			{
				_textFormat = value;

				string fontName = _textFormat.font;
				if (_font == null || _font.name != fontName)
				{
					_font = FontManager.GetFont(fontName);
					if (_font != null)
						graphics.SetShaderAndTexture(_font.shader, _font.mainTexture);
				}
				if (!string.IsNullOrEmpty(_text))
					_textChanged = true;
			}
		}

		internal BaseFont font
		{
			get { return _font; }
		}

		public AlignType align
		{
			get { return _align; }
			set
			{
				if (_align != value)
				{
					_align = value;
					if (!string.IsNullOrEmpty(_text))
						_textChanged = true;
				}
			}
		}

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

		public bool input
		{
			get { return _input; }
			set
			{
				if (_input != value)
				{
					_input = value;
					_optimizeNotTouchable = !_input;

					if (_input)
					{
						onFocusIn.Add(__focusIn);
						onFocusOut.AddCapture(__focusOut);
						onKeyDown.AddCapture(__keydown);
						onTouchBegin.AddCapture(__touchBegin);
						onTouchEnd.AddCapture(__touchEnd);

						if (Stage.touchScreen && _mobileInputAdapter == null)
						{
#if !(UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR)
							_mobileInputAdapter = new MobileInputAdapter();
#endif
						}

						if (_charPositions == null)
							_charPositions = new List<int>();
					}
					else
					{
						onFocusIn.Remove(__focusIn);
						onFocusOut.RemoveCapture(__focusOut);
						onKeyDown.RemoveCapture(__keydown);
						onTouchBegin.RemoveCapture(__touchBegin);
						onTouchEnd.RemoveCapture(__touchEnd);

						if (_charPositions != null)
							_charPositions.Clear();
					}
				}
			}
		}

		/// <summary>
		/// <see cref="UnityEngine.TouchScreenKeyboardType"/>
		/// </summary>
		public int keyboardType
		{
			get
			{
				return _mobileInputAdapter != null ? _mobileInputAdapter.keyboardType : 0;
			}
			set
			{
				if (_mobileInputAdapter != null)
					_mobileInputAdapter.keyboardType = value;
			}
		}

		public string restrict
		{
			get { return _restrict; }
			set
			{
				_restrict = value;
				if (string.IsNullOrEmpty(_restrict))
					_restrictPattern = null;
				else
					_restrictPattern = new Regex(value);
			}
		}

		public string text
		{
			get { return _text; }
			set
			{
				_text = value;
				_textChanged = true;
				_html = false;
				ClearSelection();
				if (_caretPosition > _text.Length)
					_caretPosition = _text.Length;
			}
		}

		public string htmlText
		{
			get { return _text; }
			set
			{
				_text = value;
				_textChanged = true;
				_html = true;
				ClearSelection();
				if (_caretPosition > _text.Length)
					_caretPosition = _text.Length;
			}
		}

		public bool autoSize
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

		public bool wordWrap
		{
			get { return _wordWrap; }
			set { _wordWrap = value; _textChanged = true; }
		}

		public bool singleLine
		{
			get { return _singleLine; }
			set { _singleLine = value; _textChanged = true; }
		}

		public bool displayAsPassword
		{
			get { return _displayAsPassword; }
			set { _displayAsPassword = value; }
		}

		public int maxLength
		{
			get { return _maxLength; }
			set { _maxLength = value; }
		}

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

		public float textWidth
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _textWidth;
			}
		}
		public float textHeight
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _textHeight;
			}
		}

		override protected void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			if (_wordWrap && widthChanged)
				_textChanged = true;
			else if (!_autoSize)
				_requireUpdateMesh = true;

			if (_verticalAlign != VertAlignType.Top)
				ApplyVertAlign();

			base.OnSizeChanged(widthChanged, heightChanged);
		}

		public override Rect GetBounds(DisplayObject targetSpace)
		{
			if (_textChanged && _autoSize)
				BuildLines();

			return base.GetBounds(targetSpace);
		}

		public int caretPosition
		{
			get { return _caretPosition; }
			set
			{
				_caretPosition = value;
				if (_caretPosition > _text.Length)
					_caretPosition = _text.Length;

				if (_caret != null)
				{
					_selectionStart = null;
					AdjustCaret(GetCharPosition(_caretPosition));
				}
			}
		}

		public void ReplaceSelection(string value)
		{
			if (!_input)
				return;

			InsertText(value);
			OnChanged();
		}

		public override void Update(UpdateContext context)
		{
			if (_mobileInputAdapter != null)
			{
				if (_mobileInputAdapter.done)
					UpdateContext.OnEnd += inputCompleteDelegate;

				string s = _mobileInputAdapter.GetInput();

				if (s != null && s != _text)
				{
					s = ValidateInput(s);

					if (s.Length > _maxLength)
						s = s.Substring(0, _maxLength);

					this.text = s;
					UpdateContext.OnEnd += _onChangedDelegate;
				}
			}

			if (_caret != null)
			{
				if (!string.IsNullOrEmpty(Input.inputString))
				{
					StringBuilder sb = new StringBuilder();
					int len = Input.inputString.Length;
					for (int i = 0; i < len; ++i)
					{
						char ch = Input.inputString[i];
						if (ch >= ' ') sb.Append(ch.ToString());
					}
					if (sb.Length > 0)
					{
						InsertText(sb.ToString());
						UpdateContext.OnEnd += _onChangedDelegate;
					}
				}
			}

			if (_font != null)
			{
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

			if (_input && richTextField == null)
			{
				_BeforeClip(context);
				base.Update(context);
				_AfterClip(context);
			}
			else
				base.Update(context);
		}

		internal void _BeforeClip(UpdateContext context)
		{
			Rect rect = _contentRect;
			rect.x += GUTTER_X;
			rect.y += GUTTER_Y;
			rect.width -= GUTTER_X * 2;
			if (richTextField != null)
				context.EnterClipping(this.id, richTextField.TransformRect(rect, null), null);
			else
				context.EnterClipping(this.id, this.TransformRect(rect, null), null);
		}

		internal void _AfterClip(UpdateContext context)
		{
			if (_highlighter != null)
				_highlighter.grahpics.UpdateMaterial(context);

			context.LeaveClipping();

			if (_caret != null) //不希望光标发生剪切，所以放到LeaveClipping后
			{
				_caret.grahpics.UpdateMaterial(context);
				_caret.Blink();
			}
		}

		//准备字体纹理
		void RequestText()
		{
			int count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];
				if (element.type == HtmlElementType.Text)
				{
					_font.SetFormat(element.format);
					_font.PrepareCharacters(element.text);
					_font.PrepareCharacters("_-*");
				}
			}
		}

		void BuildLines()
		{
			Cleanup();

			_textChanged = false;
			_requireUpdateMesh = true;

			if (_caret != null)
				_caret.SetSizeAndColor(_textFormat.size, _textFormat.color);

			if (_text.Length == 0)
			{
				SetEmpty();
				return;
			}

			if (_displayAsPassword)
			{
				int textLen = _text.Length;
				StringBuilder tmp = new StringBuilder(textLen);
				for (int i = 0; i < textLen; i++)
					tmp.Append("*");

				HtmlElement element = HtmlElement.GetElement(HtmlElementType.Text);
				element.text = tmp.ToString();
				element.format.CopyFrom(_textFormat);
				_elements.Add(element);
			}
			else if (_html)
				HtmlParser.inst.Parse(_text, _textFormat, _elements, richTextField != null ? richTextField.htmlParseOptions : null);
			else
			{
				HtmlElement element = HtmlElement.GetElement(HtmlElementType.Text);
				element.text = _text;
				element.format.CopyFrom(_textFormat);
				_elements.Add(element);
			}

			if (_elements.Count == 0)
			{
				SetEmpty();
				return;
			}

			bool html = _html;
			if (richTextField != null && richTextField.emojies != null)
			{
				html = true;
				HandleGraphicCharacters();
			}

			int letterSpacing = _textFormat.letterSpacing;
			int lineSpacing = _textFormat.lineSpacing - 1;
			float rectWidth = _contentRect.width - GUTTER_X * 2;
			float lineWidth = 0, lineHeight = 0, lineTextHeight = 0;
			int glyphWidth = 0, glyphHeight = 0;
			int wordChars = 0;
			float wordStart = 0;
			bool wordPossible = false;
			float lastLineHeight = 0;
			TextFormat format = _textFormat;
			_font.SetFormat(format);
			bool wrap;
			if (_input)
			{
				letterSpacing++;
				wrap = !_singleLine;
				_charPositions.Clear();
			}
			else
				wrap = _wordWrap && !_singleLine;
			float lineY = GUTTER_Y;

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
					_font.SetFormat(format);

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
							line.y = lineY;
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
								if (!_input)
									line.text = line.text.TrimEnd();
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
							line.y = lineY;
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
					if (richTextField != null)
					{
						element.space = (int)(rectWidth - lineWidth);
						htmlObject = richTextField.htmlPageContext.CreateObject(richTextField, element);
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
						line.y = lineY;
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
				line.y = lineY;
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
			if (_autoSize)
			{
				if (richTextField != null)
					richTextField.SetSize(_textWidth, _textHeight);
				else
					SetSize(_textWidth, _textHeight);

				//因为OnSizeChanged里有设置_textChanged，这里加一句保证不会重复进入。
				_textChanged = false;
			}
			else
				ApplyVertAlign();
		}

		void HandleGraphicCharacters()
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
						if (richTextField.emojies.TryGetValue(key, out gchar))
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

		void SetEmpty()
		{
			LineInfo emptyLine = LineInfo.Borrow();
			emptyLine.width = 0;
			emptyLine.height = 0;
			emptyLine.text = string.Empty;
			emptyLine.y = GUTTER_Y;
			_lines.Add(emptyLine);
			if (_input)
				_charPositions.Clear();

			_textWidth = 0;
			_textHeight = 0;
			if (_autoSize)
			{
				if (richTextField != null)
					richTextField.SetSize(_textWidth, _textHeight);
				else
					SetSize(_textWidth, _textHeight);

				//因为OnSizeChanged里有设置_textChanged，这里加一句保证不会重复进入。
				_textChanged = false;
			}
			else
				ApplyVertAlign();
		}

		static List<Vector3> sCachedVerts = new List<Vector3>();
		static List<Vector2> sCachedUVs = new List<Vector2>();
		static List<Color32> sCachedCols = new List<Color32>();
		void BuildMesh()
		{
			_requireUpdateMesh = false;

			if (_textWidth == 0 && _lines.Count == 1)
			{
				if (_input)
				{
					_charPositions.Clear();
					_charPositions.Add(0);
				}

				graphics.ClearMesh();
				if (_caret != null)
				{
					CharPosition cp = GetCharPosition(_caretPosition);
					AdjustCaret(cp);
				}

				if (_toCollect != null && _toCollect.Count > 0)
					UpdateContext.OnEnd += HandleObjects;
				return;
			}

			int letterSpacing = _textFormat.letterSpacing;
			float rectWidth = _contentRect.width - GUTTER_X * 2;
			TextFormat format = _textFormat;
			_font.SetFormat(format);
			Color32 color = format.color;
			Color32[] gradientColor = format.gradientColor;
			bool boldVertice = format.bold && (_font.customBold || (format.italic && _font.customBoldAndItalic));
			if (_input)
			{
				letterSpacing++;
				_charPositions.Clear();
			}

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
			int linkStartLineIndex = 0;
			LineInfo linkStartLine = null;

			float charX = 0;
			float tmpX;
			float lineIndent;
			float charIndent = 0;
			bool hasObject = _toCollect != null && _toCollect.Count > 0;
			bool clipped = !_input && !_autoSize;
			bool lineClipped;

			int lineCount = _lines.Count;
			for (int i = 0; i < lineCount; ++i)
			{
				LineInfo line = _lines[i];
				lineClipped = clipped && i != 0 && line.y + line.height > _contentRect.height; //超出区域，剪裁

				if (_align == AlignType.Center)
					lineIndent = (int)((rectWidth - line.width) / 2);
				else if (_align == AlignType.Right)
					lineIndent = rectWidth - line.width;
				else
					lineIndent = 0;

				charX = GUTTER_X + lineIndent;

				int textLength = line.text.Length;
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
							_font.SetFormat(format);
							color = format.color;
							gradientColor = format.gradientColor;
							boldVertice = format.bold && (_font.customBold || (format.italic && _font.customBoldAndItalic));
						}
						else if (element.type == HtmlElementType.Link)
						{
							currentLink = (HtmlLink)element.htmlObject;
							if (currentLink != null)
							{
								linkStartX = charX;
								linkStartLineIndex = i;
								linkStartLine = line;
								hasObject = true;
							}
						}
						else if (element.type == HtmlElementType.LinkEnd)
						{
							if (currentLink != null)
							{
								if (linkStartLineIndex == i)
								{
									Rect r = Rect.MinMaxRect(linkStartX, line.y, charX, line.y + line.height);
									currentLink.SetArea(r);
								}
								else if (linkStartLineIndex == i - 1)
								{
									Rect r0 = Rect.MinMaxRect(linkStartX, linkStartLine.y, linkStartLine.width, linkStartLine.y + linkStartLine.height);
									Rect r1 = Rect.MinMaxRect(GUTTER_X, line.y, charX, line.y + line.height);
									currentLink.SetArea(r0, r1);
								}
								else
								{
									Rect r0 = Rect.MinMaxRect(linkStartX, linkStartLine.y, linkStartLine.width, linkStartLine.y + linkStartLine.height);
									Rect r1 = Rect.MinMaxRect(GUTTER_X, linkStartLine.y + linkStartLine.height, _contentRect.width - GUTTER_X * 2, line.y);
									Rect r2 = Rect.MinMaxRect(GUTTER_X, line.y, charX, line.y + line.height);
									currentLink.SetArea(r0, r1, r2);
								}

								currentLink = null;
							}
						}
						else
						{
							IHtmlObject htmlObj = element.htmlObject;
							if (htmlObj != null)
							{
								if (_input)
									_charPositions.Add(((int)charX << 16) + i);
								element.position = new Vector2(charX + 1, line.y + (int)((line.height - htmlObj.height) / 2));
								htmlObj.SetPosition(element.position.x - _positionOffset.x, element.position.y - _positionOffset.y);
								element.hidden = lineClipped || clipped && charX + htmlObj.width > _contentRect.width - GUTTER_X;
								charX += htmlObj.width + letterSpacing + 2;
								hasObject = true;
							}
						}
						continue;
					}

					if (_input)
						_charPositions.Add(((int)charX << 16) + i);

					GlyphInfo glyph = _font.GetGlyph(ch);
					if (glyph != null)
					{
						if (lineClipped || clipped && charX != 0 && charX + glyph.width > _contentRect.width - GUTTER_X) //超出区域，剪裁
						{
							charX += letterSpacing + glyph.width;
							continue;
						}

						tmpX = charX;
						charIndent = (int)((line.height + line.textHeight) / 2) - glyph.height;
						v0.x = charX + glyph.vert.xMin;
						v0.y = -line.y - charIndent + glyph.vert.yMin;
						v1.x = charX + glyph.vert.xMax;
						v1.y = -line.y - charIndent + glyph.vert.yMax;
						u0 = glyph.uvBottomLeft;
						u1 = glyph.uvTopLeft;
						u2 = glyph.uvTopRight;
						u3 = glyph.uvBottomRight;
						specFlag = 0;

						if (_font.hasChannel)
						{
							//对于由BMFont生成的字体，使用这个特殊的设置告诉着色器告诉用的是哪个通道
							specFlag = 10 * (glyph.channel == 0 ? 3 : (glyph.channel - 1));
							u0.x += specFlag;
							u1.x += specFlag;
							u2.x += specFlag;
							u3.x += specFlag;
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

						charX += letterSpacing + glyph.width;

						if (format.underline)
						{
							glyph = _font.GetGlyph('_');
							if (glyph == null)
								continue;

							//取中点的UV
							if (glyph.uvBottomLeft.x != glyph.uvBottomRight.x)
								u0.x = (glyph.uvBottomLeft.x + glyph.uvBottomRight.x) * 0.5f;
							else
								u0.x = (glyph.uvBottomLeft.x + glyph.uvTopLeft.x) * 0.5f;
							u0.x += specFlag;

							if (glyph.uvBottomLeft.y != glyph.uvTopLeft.y)
								u0.y = (glyph.uvBottomLeft.y + glyph.uvTopLeft.y) * 0.5f;
							else
								u0.y = (glyph.uvBottomLeft.y + glyph.uvBottomRight.y) * 0.5f;

							uvList.Add(u0);
							uvList.Add(u0);
							uvList.Add(u0);
							uvList.Add(u0);

							v0.y = -line.y - charIndent + glyph.vert.yMin - 1;
							v1.y = -line.y - charIndent + glyph.vert.yMax - 1;

							vertList.Add(new Vector3(tmpX, v0.y));
							vertList.Add(new Vector3(tmpX, v1.y));
							vertList.Add(new Vector3(charX, v1.y));
							vertList.Add(new Vector3(charX, v0.y));

							colList.Add(color);
							colList.Add(color);
							colList.Add(color);
							colList.Add(color);
						}
					}
					else //if (glyph != null)
					{
						charX += letterSpacing;
					}
				}//text loop
			}//line loop

			if (_input)
				_charPositions.Add(((int)charX << 16) + lineCount - 1);

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
					for (int j = 0; j < 4; j++)
					{
						start = j * count;
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
					start = allocCount - count - count;
					for (int i = 0; i < count; i++)
					{
						Vector3 vert = vertList[i];
						Vector2 u = uvList[i];

						//使用这个特殊的设置告诉着色器这个是描边
						if (_font.canOutline)
							u.y = 10 + u.y;
						uvBuf[start] = u;
						vertBuf[start] = new Vector3(vert.x + _shadowOffset.x, vert.y - _shadowOffset.y, 0);
						colBuf[start] = strokeColor;
						start++;
					}
				}
			}
			else
			{
				int count = vertList.Count;
				graphics.Alloc(count);
				vertList.CopyTo(0, graphics.vertices, 0, count);
				uvList.CopyTo(0, graphics.uv, 0, count);
				if (_font.canTint)
				{
					for (int i = 0; i < count; i++)
						graphics.colors[i] = colList[i];
				}
				else
				{
					for (int i = 0; i < count; i++)
						graphics.colors[i] = Color.white;
				}
			}

			graphics.FillTriangles();
			graphics.UpdateMesh();

			if (hasObject)
				UpdateContext.OnEnd += HandleObjects;

			if (_caret != null)
			{
				CharPosition cp = GetCharPosition(_caretPosition);
				AdjustCaret(cp);
			}
		}

		void HandleObjects()
		{
			int count = _toCollect != null ? _toCollect.Count : 0;
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					IHtmlObject htmlObject = _toCollect[i];
					htmlObject.Remove();
					richTextField.htmlPageContext.FreeObject(htmlObject);
				}
				_toCollect.Clear();
			}

			count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];
				if (element.htmlObject != null)
				{
					if (element.hidden)
					{
						if (element.added)
						{
							element.added = false;
							element.htmlObject.Remove();
						}
					}
					else
					{
						if (!element.added)
						{
							element.added = true;
							element.htmlObject.Add();
						}
					}
				}
			}
		}

		void Cleanup()
		{
			if (richTextField != null)
			{
				int count = _elements.Count;
				for (int i = 0; i < count; i++)
				{
					HtmlElement element = _elements[i];
					if (element.htmlObject != null)
					{
						//不能立刻remove，因为可能在Update里，Update里不允许增删对象。放到延迟队列里
						if (_toCollect == null)
							_toCollect = new List<IHtmlObject>();
						_toCollect.Add(element.htmlObject);
					}
				}
			}

			HtmlElement.ReturnElements(_elements);
			LineInfo.Return(_lines);
			_textWidth = 0;
			_textHeight = 0;
		}

		CharPosition GetCharPosition(int charIndex)
		{
			CharPosition cp;
			cp.charIndex = charIndex;

			if (charIndex < _charPositions.Count)
				cp.lineIndex = _charPositions[charIndex] & 0xFFFF;
			else
				cp.lineIndex = Math.Max(0, _lines.Count - 1);
			return cp;
		}

		CharPosition GetCharPosition(Vector3 location)
		{
			CharPosition result;
			if (_charPositions.Count == 0)
			{
				result.lineIndex = 0;
				result.charIndex = 0;
				return result;
			}

			int len = _lines.Count;
			LineInfo line;
			int i;
			for (i = 0; i < len; i++)
			{
				line = _lines[i];
				if (line.y + line.height > location.y)
					break;
			}
			if (i == len)
				i = len - 1;

			result.lineIndex = i;

			len = _charPositions.Count;
			int v;
			int firstInLine = -1;
			for (i = 0; i < len; i++)
			{
				v = _charPositions[i];
				if ((v & 0xFFFF) == result.lineIndex)
				{
					if (firstInLine == -1)
						firstInLine = i;
					if (((v >> 16) & 0xFFFF) > location.x)
					{
						result.charIndex = i > firstInLine ? i - 1 : firstInLine;
						return result;
					}
				}
				else if (firstInLine != -1)
					break;
			}
			result.charIndex = i - 1;
			return result;
		}

		Vector2 GetCharLocation(CharPosition cp)
		{
			LineInfo line = _lines[cp.lineIndex];
			Vector2 pos;
			if (line.text.Length == 0 || _charPositions.Count == 0)
			{
				if (_align == AlignType.Center)
					pos.x = _contentRect.width / 2;
				else
					pos.x = GUTTER_X;
			}
			else
			{
				int v = _charPositions[Math.Min(cp.charIndex, _charPositions.Count - 1)];
				pos.x = ((v >> 16) & 0xFFFF) - 1;
			}
			pos.y = line.y;
			return pos;
		}

		void ClearSelection()
		{
			if (_selectionStart != null)
			{
				if (_highlighter != null)
					_highlighter.Clear();
				_selectionStart = null;
			}
		}

		void DeleteSelection()
		{
			if (_selectionStart == null)
				return;

			CharPosition cp = (CharPosition)_selectionStart;
			if (cp.charIndex < _caretPosition)
			{
				this.text = _text.Substring(0, cp.charIndex) + _text.Substring(_caretPosition);
				_caretPosition = cp.charIndex;
			}
			else
				this.text = _text.Substring(0, _caretPosition) + _text.Substring(cp.charIndex);
			ClearSelection();
		}

		string GetSelection()
		{
			if (_selectionStart == null)
				return string.Empty;

			CharPosition cp = (CharPosition)_selectionStart;
			if (cp.charIndex < _caretPosition)
				return _text.Substring(cp.charIndex, _caretPosition - cp.charIndex);
			else
				return _text.Substring(_caretPosition, cp.charIndex - _caretPosition);
		}

		void InsertText(string value)
		{
			if (_selectionStart != null)
				DeleteSelection();

			value = ValidateInput(value);
			if (value.Length == 0)
				return;

			if (_text.Length + value.Length > _maxLength)
				value = value.Substring(0, Math.Max(0, _maxLength - _text.Length));

			this.text = _text.Substring(0, _caretPosition) + value + _text.Substring(_caretPosition);
			_caretPosition += value.Length;
		}

		string ValidateInput(string source)
		{
			if (_restrict != null)
			{
				StringBuilder sb = new StringBuilder();
				Match mc = _restrictPattern.Match(source);
				int lastPos = 0;
				string s;
				while (mc != Match.Empty)
				{
					if (mc.Index != lastPos)
					{
						//保留tab和回车
						for (int i = lastPos; i < mc.Index; i++)
						{
							if (source[i] == '\n' || source[i] == '\t')
								sb.Append(source[i]);
						}
					}

					s = mc.ToString();
					lastPos = mc.Index + s.Length;
					sb.Append(s);

					mc = mc.NextMatch();
				}
				for (int i = lastPos; i < source.Length; i++)
				{
					if (source[i] == '\n' || source[i] == '\t')
						sb.Append(source[i]);
				}

				return sb.ToString();
			}
			else
				return source;
		}

		void OnChanged()
		{
			onChanged.Call();
			if (richTextField != null)
				richTextField.onChanged.Call();
		}

		void ApplyVertAlign()
		{
			float yOffset = 0;
			if (_verticalAlign == VertAlignType.Top)
				yOffset = 0;
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
					yOffset = (int)(dh / 2);
				else
					yOffset = dh;
			}

			yOffset = -yOffset;
			if (yOffset != _positionOffset.y)
				SetPositionOffset(new Vector2(_positionOffset.x, yOffset));
		}

		override protected void SetPositionOffset(Vector2 value)
		{
			base.SetPositionOffset(value);

			if (richTextField != null)
			{
				int count = _elements.Count;
				for (int i = 0; i < count; i++)
				{
					HtmlElement element = _elements[i];
					if (element.htmlObject != null)
						element.htmlObject.SetPosition(element.position.x - value.x, element.position.y - value.y);
				}
			}
		}

		void AdjustCaret(CharPosition cp)
		{
			_caretPosition = cp.charIndex;
			Vector2 pos = GetCharLocation(cp);

			Vector2 offset = _positionOffset;
			if (pos.x - offset.x < _textFormat.size)
			{
				float move = pos.x - (int)Math.Min(50, _contentRect.width / 2);
				if (move < 0)
					move = 0;
				else if (move + _contentRect.width > _textWidth)
					move = Math.Max(0, _textWidth - _contentRect.width);
				offset.x = move;
			}
			else if (pos.x - offset.x > _contentRect.width - _textFormat.size)
			{
				float move = pos.x - (int)Math.Min(50, _contentRect.width / 2);
				if (move < 0)
					move = 0;
				else if (move + _contentRect.width > _textWidth)
					move = Math.Max(0, _textWidth - _contentRect.width);
				offset.x = move;
			}

			LineInfo line = _lines[cp.lineIndex];
			if (line.y - offset.y < 0)
			{
				float move = line.y - GUTTER_Y;
				offset.y = move;
			}
			else if (line.y + line.height - offset.y >= _contentRect.height)
			{
				float move = line.y + line.height + GUTTER_Y - _contentRect.height;
				if (move < 0)
					move = 0;
				if (line.y - move >= 0)
					offset.y = move;
			}

			SetPositionOffset(offset);

			if (line.height > 0) //将光标居中
				pos.y += (int)(line.height - _textFormat.size) / 2;
			_caret.SetPosition(pos);

			if (_selectionStart != null)
				UpdateHighlighter(cp);
		}

		void UpdateHighlighter(CharPosition cp)
		{
			CharPosition start = (CharPosition)_selectionStart;
			if (start.charIndex > cp.charIndex)
			{
				CharPosition tmp = start;
				start = cp;
				cp = tmp;
			}

			LineInfo line1;
			LineInfo line2;
			Vector2 v1, v2;
			line1 = _lines[start.lineIndex];
			line2 = _lines[cp.lineIndex];
			v1 = GetCharLocation(start);
			v2 = GetCharLocation(cp);

			_highlighter.BeginUpdate();
			if (start.lineIndex == cp.lineIndex)
			{
				Rect r = Rect.MinMaxRect(v1.x, line1.y, v2.x, line1.y + line1.height);
				_highlighter.AddRect(r);
			}
			else
			{
				Rect r = Rect.MinMaxRect(v1.x, line1.y, _contentRect.width - GUTTER_X * 2, line1.y + line1.height);
				_highlighter.AddRect(r);

				for (int i = start.lineIndex + 1; i < cp.lineIndex; i++)
				{
					LineInfo line = _lines[i];
					r = Rect.MinMaxRect(GUTTER_X, line.y, _contentRect.width - GUTTER_X * 2, line.y + line.height);
					if (i == start.lineIndex)
						r.yMin = line1.y + line1.height;
					if (i == cp.lineIndex - 1)
						r.yMax = line2.y;
					_highlighter.AddRect(r);
				}

				r = Rect.MinMaxRect(GUTTER_X, line2.y, v2.x, line2.y + line2.height);
				_highlighter.AddRect(r);
			}
			_highlighter.EndUpdate();
		}

		public List<HtmlElement> GetHtmlElements()
		{
			if (_textChanged)
				BuildLines();

			return _elements;
		}

		void OpenKeyboard()
		{
			_mobileInputAdapter.OpenKeyboard(_text, false, displayAsPassword ? false : !_singleLine, displayAsPassword, false, null);
		}

		void __focusIn(EventContext context)
		{
			if (_mobileInputAdapter != null)
			{
				OpenKeyboard();
			}
			else
			{
				_caret = Stage.inst.inputCaret;
				_caret.grahpics.sortingOrder = this.renderingOrder + 1;
				_caret.SetParent(cachedTransform);
				_caret.SetSizeAndColor(_textFormat.size, _textFormat.color);

				_highlighter = Stage.inst.highlighter;
				_highlighter.grahpics.sortingOrder = this.renderingOrder + 2;
				_highlighter.SetParent(cachedTransform);

				_caretPosition = _text.Length;
				CharPosition cp = GetCharPosition(_caretPosition);
				AdjustCaret(cp);
				_selectionStart = cp;
			}
		}

		void __focusOut(EventContext contxt)
		{
			if (_mobileInputAdapter != null)
			{
				_mobileInputAdapter.CloseKeyboard();
			}

			if (_caret != null)
			{
				_caret.SetParent(null);
				_caret = null;
				_highlighter.SetParent(null);
				_highlighter = null;
			}
		}

		void __keydown(EventContext context)
		{
			if (_caret == null || context.isDefaultPrevented)
				return;

			InputEvent evt = context.inputEvent;

			switch (evt.keyCode)
			{
				case KeyCode.Backspace:
					{
						context.PreventDefault();
						if (_selectionStart != null)
						{
							DeleteSelection();
							OnChanged();
						}
						else if (_caretPosition > 0)
						{
							int tmp = _caretPosition; //this.text 会修改_caretPosition
							_caretPosition--;
							this.text = _text.Substring(0, tmp - 1) + _text.Substring(tmp);
							OnChanged();
						}

						break;
					}

				case KeyCode.Delete:
					{
						context.PreventDefault();
						if (_selectionStart != null)
						{
							DeleteSelection();
							OnChanged();
						}
						else if (_caretPosition < _text.Length)
						{
							this.text = _text.Substring(0, _caretPosition) + _text.Substring(_caretPosition + 1);
							OnChanged();
						}

						break;
					}

				case KeyCode.LeftArrow:
					{
						context.PreventDefault();
						if (evt.shift)
						{
							if (_selectionStart == null)
								_selectionStart = GetCharPosition(_caretPosition);
						}
						else
							ClearSelection();
						if (_caretPosition > 0)
						{
							CharPosition cp = GetCharPosition(_caretPosition - 1);

							AdjustCaret(cp);
						}
						break;
					}

				case KeyCode.RightArrow:
					{
						context.PreventDefault();
						if (evt.shift)
						{
							if (_selectionStart == null)
								_selectionStart = GetCharPosition(_caretPosition);
						}
						else
							ClearSelection();
						if (_caretPosition < _text.Length)
						{
							CharPosition cp = GetCharPosition(_caretPosition + 1);
							AdjustCaret(cp);
						}
						break;
					}

				case KeyCode.UpArrow:
					{
						context.PreventDefault();
						if (evt.shift)
						{
							if (_selectionStart == null)
								_selectionStart = GetCharPosition(_caretPosition);
						}
						else
							ClearSelection();

						CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == 0)
							return;

						LineInfo line = _lines[cp.lineIndex - 1];
						cp = GetCharPosition(new Vector3(_caret.cachedTransform.localPosition.x + _positionOffset.x, line.y, 0));
						AdjustCaret(cp);
						break;
					}


				case KeyCode.DownArrow:
					{
						context.PreventDefault();
						if (evt.shift)
						{
							if (_selectionStart == null)
								_selectionStart = GetCharPosition(_caretPosition);
						}
						else
							ClearSelection();

						CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == _lines.Count - 1)
							return;

						LineInfo line = _lines[cp.lineIndex + 1];
						cp = GetCharPosition(new Vector3(_caret.cachedTransform.localPosition.x + _positionOffset.x, line.y, 0));
						AdjustCaret(cp);
						break;
					}

				case KeyCode.PageUp:
					{
						context.PreventDefault();
						ClearSelection();

						break;
					}

				case KeyCode.PageDown:
					{
						context.PreventDefault();
						ClearSelection();

						break;
					}

				case KeyCode.Home:
					{
						context.PreventDefault();
						ClearSelection();

						CharPosition cp = GetCharPosition(_caretPosition);
						LineInfo line = _lines[cp.lineIndex];
						cp = GetCharPosition(new Vector3(int.MinValue, line.y, 0));
						AdjustCaret(cp);
						break;
					}

				case KeyCode.End:
					{
						context.PreventDefault();
						ClearSelection();

						CharPosition cp = GetCharPosition(_caretPosition);
						LineInfo line = _lines[cp.lineIndex];
						cp = GetCharPosition(new Vector3(int.MaxValue, line.y, 0));
						AdjustCaret(cp);

						break;
					}

				//Select All
				case KeyCode.A:
					{
						if (evt.ctrl)
						{
							context.PreventDefault();
							_selectionStart = GetCharPosition(0);
							AdjustCaret(GetCharPosition(_text.Length));
						}
						break;
					}

				// Copy
				case KeyCode.C:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
								Stage.inst.onCopy.Call(s);
						}
						break;
					}

				// Paste
				case KeyCode.V:
					{
						if (evt.ctrl)
						{
							context.PreventDefault();
							Stage.inst.onPaste.Call(this);
						}
						break;
					}

				// Cut
				case KeyCode.X:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
							{
								Stage.inst.onCopy.Call(s);
								DeleteSelection();
								OnChanged();
							}
						}
						break;
					}

				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					{
						if (!evt.ctrl && !evt.shift)
						{
							context.PreventDefault();

							if (!_singleLine)
							{
								InsertText("\n");
								OnChanged();
							}
						}
						break;
					}
			}
		}

		void __touchBegin(EventContext context)
		{
			if (_caret == null || _lines.Count == 0)
				return;

			ClearSelection();

			CharPosition cp;
			if (_textChanged) //maybe the text changed in user's touchBegin
			{
				cp.charIndex = 0;
				cp.lineIndex = 0;
			}
			else
			{
				Vector3 v = Stage.inst.touchPosition;
				v = this.GlobalToLocal(v);

				v.x += _positionOffset.x;
				v.y += _positionOffset.y;
				cp = GetCharPosition(v);
			}

			AdjustCaret(cp);
			_selectionStart = cp;

			context.CaptureTouch();
			Stage.inst.onTouchMove.AddCapture(_touchMoveDelegate);
		}

		void __touchMove(EventContext context)
		{
			if (isDisposed)
				return;

			if (_selectionStart == null)
				return;

			Vector3 v = Stage.inst.touchPosition;
			v = this.GlobalToLocal(v);
			if (float.IsNaN(v.x))
				return;

			v.x += _positionOffset.x;
			v.y += _positionOffset.y;

			CharPosition cp = GetCharPosition(v);
			if (cp.charIndex != _caretPosition)
				AdjustCaret(cp);
		}

		void __touchEnd(EventContext context)
		{
			Stage.inst.onTouchMove.RemoveCapture(_touchMoveDelegate);

			if (isDisposed)
				return;

			if (_selectionStart != null && ((CharPosition)_selectionStart).charIndex == _caretPosition)
				_selectionStart = null;
		}

		class LineInfo
		{
			public float width;
			public float height;
			public float textHeight;
			public string text;
			public float y;

			static Stack<LineInfo> pool = new Stack<LineInfo>();

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

			public static void Return(LineInfo value)
			{
				pool.Push(value);
			}

			public static void Return(List<LineInfo> values)
			{
				int cnt = values.Count;
				for (int i = 0; i < cnt; i++)
					pool.Push(values[i]);

				values.Clear();
			}
		}

		struct CharPosition
		{
			public int charIndex;
			public int lineIndex;
		}
	}

}
