using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 接收用户输入的文本控件。因为支持直接输入表情，所以从RichTextField派生。
	/// </summary>
	public class InputTextField : RichTextField
	{
		/// <summary>
		/// 
		/// </summary>
		public EventListener onFocusIn { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onFocusOut { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onChanged { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int maxLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int keyboardType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool editable { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool hideInput { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textField"></param>
		/// <param name="text"></param>
		public delegate void CopyHandler(InputTextField textField, string text);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textField"></param>
		public delegate void PasteHandler(InputTextField textField);

		/// <summary>
		/// 
		/// </summary>
		public static CopyHandler onCopy;

		/// <summary>
		/// 
		/// </summary>
		public static PasteHandler onPaste;

		string _restrict;
		Regex _restrictPattern;
		bool _displayAsPassword;
		string _promptText;
		string _decodedPromptText;

		bool _editing;
		int _caretPosition;
		TextField.CharPosition? _selectionStart;

		EventCallback1 _touchMoveDelegate;

		static Shape _caret;
		static SelectionShape _selectionShape;
		static float _nextBlink;

		const int GUTTER_X = 2;
		const int GUTTER_Y = 2;

		public InputTextField()
		{
			gameObject.name = "InputTextField";

			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");

			maxLength = int.MaxValue;
			editable = true;

			/* 因为InputTextField定义了ClipRect，而ClipRect是四周缩进了2个像素的（GUTTER)，默认的点击测试
			 * 是使用ClipRect的，那会造成无法点击四周的空白区域。所以这里自定义了一个HitArea
			 */
			this.hitArea = new RectHitTest();
			this.touchChildren = false;

			_touchMoveDelegate = __touchMove;

			onFocusIn.Add(__focusIn);
			onFocusOut.AddCapture(__focusOut);
			onKeyDown.AddCapture(__keydown);
			onTouchBegin.AddCapture(__touchBegin);
			onTouchEnd.AddCapture(__touchEnd);
		}

		/// <summary>
		/// 
		/// </summary>
		public override string text
		{
			get
			{
				return base.text;
			}
			set
			{
				base.text = value;
				ClearSelection();
				if (_caretPosition > textField.text.Length)
					_caretPosition = textField.text.Length;
				UpdateAlternativeText();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override TextFormat textFormat
		{
			get
			{
				return base.textFormat;
			}
			set
			{
				base.textFormat = value;
				if (_editing)
				{
					_caret.height = textField.textFormat.size;
					_caret.DrawRect(0, Color.clear, textField.textFormat.color);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
		public int caretPosition
		{
			get { return _caretPosition; }
			set
			{
				_caretPosition = value;
				if (_caretPosition > textField.text.Length)
					_caretPosition = textField.text.Length;

				if (_editing)
				{
					_selectionStart = null;
					AdjustCaret(GetCharPosition(_caretPosition));
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string promptText
		{
			get
			{
				return _promptText;
			}
			set
			{
				_promptText = value;
				if (!string.IsNullOrEmpty(_promptText))
					_decodedPromptText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
				else
					_decodedPromptText = null;
				UpdateAlternativeText();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool displayAsPassword
		{
			get { return _displayAsPassword; }
			set
			{
				if (_displayAsPassword != value)
				{
					_displayAsPassword = value;
					UpdateAlternativeText();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceSelection(string value)
		{
			string leftText = null;
			string rightText = null;
			if (_selectionStart != null)
			{
				TextField.CharPosition cp = (TextField.CharPosition)_selectionStart;
				ClearSelection();

				if (cp.caretIndex < _caretPosition)
				{
					leftText = textField.text.Substring(0, cp.caretIndex);
					rightText = textField.text.Substring(_caretPosition);

					_caretPosition = cp.caretIndex;
				}
				else
				{
					leftText = textField.text.Substring(0, _caretPosition);
					rightText = textField.text.Substring(cp.caretIndex);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(value))
					return;

				leftText = textField.text.Substring(0, _caretPosition);
				rightText = textField.text.Substring(_caretPosition);
			}

			if (!string.IsNullOrEmpty(value))
			{
				value = ValidateInput(value);
				if (leftText.Length + rightText.Length + value.Length > maxLength)
					value = value.Substring(0, Math.Max(0, maxLength - leftText.Length - rightText.Length));

				this.text = leftText + value + rightText;
				_caretPosition += value.Length;
			}
			else
				this.text = leftText + rightText;

			//这里立即更新，使内含的图片等对象能够立即创建，避免延迟在Update里才创建，那样会引起闪烁
			textField.Redraw();

			onChanged.Call();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceText(string value)
		{
			if (value == textField.text)
				return;

			value = ValidateInput(value);

			if (value.Length > maxLength)
				value = value.Substring(0, maxLength);

			this.text = value;
			textField.Redraw();
			onChanged.Call();
		}

		void UpdateAlternativeText()
		{
			if (!_editing && this.text.Length == 0 && !string.IsNullOrEmpty(_decodedPromptText))
			{
				textField.SetAlternativeText(_decodedPromptText, true);
			}
			else
			{
				if (_displayAsPassword)
					textField.SetAlternativeText(EncodePasswordText(this.text), false);
				else
					textField.SetAlternativeText(null, false);
			}
		}

		string EncodePasswordText(string value)
		{
			int textLen = value.Length;
			StringBuilder tmp = new StringBuilder(textLen);
			for (int i = 0; i < textLen; i++)
				tmp.Append("*");
			return tmp.ToString();
		}

		void ClearSelection()
		{
			if (_selectionStart != null)
			{
				if (_selectionShape != null)
					_selectionShape.Clear();
				_selectionStart = null;
			}
		}

		string GetSelection()
		{
			if (_selectionStart == null)
				return string.Empty;

			TextField.CharPosition cp = (TextField.CharPosition)_selectionStart;
			if (cp.caretIndex < _caretPosition)
				return textField.text.Substring(cp.caretIndex, _caretPosition - cp.caretIndex);
			else
				return textField.text.Substring(_caretPosition, cp.caretIndex - _caretPosition);
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

		void AdjustCaret(TextField.CharPosition cp)
		{
			_caretPosition = cp.caretIndex;

			Vector2 pos = GetCharLocation(cp);
			TextField.LineInfo line = textField.lines[cp.lineIndex];
			pos.y = line.y + textField.y;
			Vector2 newPos = pos;

			if (newPos.x < textField.textFormat.size)
				newPos.x += Math.Min(50, (int)(_contentRect.width / 2));
			else if (newPos.x > _contentRect.width - GUTTER_X - textField.textFormat.size)
				newPos.x -= Math.Min(50, (int)(_contentRect.width / 2));

			if (newPos.x < GUTTER_X)
				newPos.x = GUTTER_X;
			else if (newPos.x > _contentRect.width - GUTTER_X)
				newPos.x = Math.Max(GUTTER_X, _contentRect.width - GUTTER_X);

			if (newPos.y < GUTTER_Y)
				newPos.y = GUTTER_Y;
			else if (newPos.y + line.height >= _contentRect.height - GUTTER_Y)
				newPos.y = Math.Max(GUTTER_Y, _contentRect.height - line.height - GUTTER_Y);

			pos += MoveContent(newPos - pos);

			if (_editing)
			{
				if (line.height > 0) //将光标居中
					pos.y += (int)(line.height - textField.textFormat.size) / 2;

				_caret.SetPosition(pos.x, pos.y, 0);

				Vector2 cursorPos = _caret.LocalToGlobal(new Vector2(0, _caret.height));
				Input.compositionCursorPos = cursorPos;

				_nextBlink = Time.time + 0.5f;
				_caret.graphics.enabled = true;

				if (_selectionStart != null)
					UpdateSelection(cp);
			}
		}

		Vector2 MoveContent(Vector2 delta)
		{
			float ox = textField.x;
			float oy = textField.y;
			float nx = ox + delta.x;
			float ny = oy + delta.y;
			if (_contentRect.width - nx > textField.textWidth)
				nx = _contentRect.width - textField.textWidth;
			if (_contentRect.height - ny > textField.textHeight)
				ny = _contentRect.height - textField.textHeight;
			if (nx > 0)
				nx = 0;
			if (ny > 0)
				ny = 0;
			nx = (int)nx;
			ny = (int)ny;

			if (nx != ox || ny != oy)
			{
				textField.SetXY(nx, ny);

				List<HtmlElement> elements = textField.htmlElements;
				int count = elements.Count;
				for (int i = 0; i < count; i++)
				{
					HtmlElement element = elements[i];
					if (element.htmlObject != null)
						element.htmlObject.SetPosition(element.position.x + nx, element.position.y + ny);
				}
			}

			delta.x = nx - ox;
			delta.y = ny - oy;
			return delta;
		}

		void UpdateSelection(TextField.CharPosition cp)
		{
			TextField.CharPosition start = (TextField.CharPosition)_selectionStart;
			if (start.caretIndex == cp.caretIndex)
			{
				_selectionShape.Clear();
				return;
			}

			if (start.caretIndex > cp.caretIndex)
			{
				TextField.CharPosition tmp = start;
				start = cp;
				cp = tmp;
			}

			Vector2 v1 = GetCharLocation(start);
			Vector2 v2 = GetCharLocation(cp);

			List<Rect> rects = _selectionShape.rects;
			if (rects == null)
				rects = new List<Rect>(2);
			else
				rects.Clear();
			textField.GetLinesShape(start.lineIndex, v1.x - textField.x, cp.lineIndex, v2.x - textField.x, false, rects);
			_selectionShape.rects = rects;
			_selectionShape.xy = textField.xy;
		}

		TextField.CharPosition GetCharPosition(int caretIndex)
		{
			if (caretIndex < textField.charPositions.Count)
				return textField.charPositions[caretIndex];
			else
			{
				TextField.CharPosition cp = new TextField.CharPosition();
				cp.caretIndex = caretIndex;
				cp.lineIndex = (short)Math.Max(0, textField.lines.Count - 1);
				return cp;
			}
		}

		/// <summary>
		/// 通过本地坐标获得字符索引位置
		/// </summary>
		/// <param name="location">本地坐标</param>
		/// <returns></returns>
		TextField.CharPosition GetCharPosition(Vector2 location)
		{
			TextField.CharPosition result = new TextField.CharPosition();
			if (textField.charPositions.Count == 0)
			{
				result.lineIndex = 0;
				result.caretIndex = 0;
				return result;
			}

			location.x -= textField.x;
			location.y -= textField.y;

			List<TextField.LineInfo> lines = textField.lines;
			int len = lines.Count;
			TextField.LineInfo line;
			int i;
			for (i = 0; i < len; i++)
			{
				line = lines[i];
				if (line.y + line.height > location.y)
					break;
			}
			if (i == len)
				i = len - 1;

			result.lineIndex = (short)i;

			len = textField.charPositions.Count;
			TextField.CharPosition v;
			int firstInLine = -1;
			for (i = 0; i < len; i++)
			{
				v = textField.charPositions[i];
				if (v.lineIndex == result.lineIndex)
				{
					if (firstInLine == -1)
						firstInLine = i;
					if (v.offsetX > location.x)
					{
						if (i > firstInLine)
						{
							//最后一个字符有点难点
							if (v.offsetX - location.x < 2)
								result.caretIndex = i;
							else
								result.caretIndex = i - 1;
						}
						else
							result.caretIndex = firstInLine;
						return result;
					}
				}
				else if (firstInLine != -1)
					break;
			}
			result.caretIndex = i - 1;
			return result;
		}

		/// <summary>
		/// 获得字符的坐标。这个坐标是容器的坐标，不是文本里的坐标。
		/// </summary>
		/// <param name="cp"></param>
		/// <returns></returns>
		Vector2 GetCharLocation(TextField.CharPosition cp)
		{
			TextField.LineInfo line = textField.lines[cp.lineIndex];
			Vector2 pos;
			if (line.text.Length == 0 || textField.charPositions.Count == 0)
			{
				if (textField.align == AlignType.Center)
					pos.x = (int)(_contentRect.width / 2);
				else
					pos.x = GUTTER_X;
			}
			else
			{
				TextField.CharPosition v = textField.charPositions[Math.Min(cp.caretIndex, textField.charPositions.Count - 1)];
				pos.x = v.offsetX - 1;
			}
			pos.x += textField.x;
			pos.y = textField.y + line.y;
			return pos;
		}

		void __touchBegin(EventContext context)
		{
			if (!_editing || textField.lines.Count == 0)
				return;

			ClearSelection();

			Vector3 v = Stage.inst.touchPosition;
			v = this.GlobalToLocal(v);
			TextField.CharPosition cp = GetCharPosition(v);

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

			TextField.CharPosition cp = GetCharPosition(v);
			if (cp.caretIndex != _caretPosition)
				AdjustCaret(cp);
		}

		void __touchEnd(EventContext context)
		{
			Stage.inst.onTouchMove.RemoveCapture(_touchMoveDelegate);

			if (isDisposed)
				return;

			if (_selectionStart != null && ((TextField.CharPosition)_selectionStart).caretIndex == _caretPosition)
				_selectionStart = null;
		}

		protected override void InternalRefreshObjects()
		{
			base.InternalRefreshObjects();

			if (_editing)
			{
				SetChildIndex(_selectionShape, this.numChildren - 1);
				SetChildIndex(_caret, this.numChildren - 2);
			}

			TextField.CharPosition cp = GetCharPosition(_caretPosition);
			AdjustCaret(cp);
		}

		protected override void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			base.OnSizeChanged(widthChanged, heightChanged);

			Rect rect = _contentRect;
			rect.x += GUTTER_X;
			rect.y += GUTTER_Y;
			rect.width -= GUTTER_X * 2;
			//高度不减GUTTER_X * 2，因为怕高度不小心截断文字
			this.clipRect = rect;
			((RectHitTest)this.hitArea).rect = _contentRect;
		}

		public override void Update(UpdateContext context)
		{
			base.Update(context);

			if (_editing)
			{
				if (_nextBlink < Time.time)
				{
					_nextBlink = Time.time + 0.5f;
					_caret.graphics.enabled = !_caret.graphics.enabled;
				}
			}
		}

		public override void Dispose()
		{
			if (_editing)
			{
				_caret.RemoveFromParent();
				_selectionShape.RemoveFromParent();
			}
			base.Dispose();
		}

		void DoCopy(string value)
		{
#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
			CopyPastePatch.OnCopy(this, value);
#else
			if (onCopy != null)
				onCopy(this, value);
#endif
		}

		void DoPaste()
		{
#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
			CopyPastePatch.OnPaste(this);
#else
			if (onPaste != null)
				onPaste(this);
#endif
		}

		static void CreateCaret()
		{
			_caret = new Shape();
			_caret.gameObject.name = "InputCaret";
			_caret.touchable = false;
			_caret._skipInFairyBatching = true;
			_caret.graphics.dontClip = true;
			_caret.home = Stage.inst.cachedTransform;

			_selectionShape = new SelectionShape();
			_selectionShape.gameObject.name = "InputSelection";
			_selectionShape.color = UIConfig.inputHighlightColor;
			_selectionShape._skipInFairyBatching = true;
			_selectionShape.touchable = false;
			_selectionShape.home = Stage.inst.cachedTransform;
		}

		void __focusIn(EventContext context)
		{
			if (!editable || !Application.isPlaying)
				return;

			if (Stage.keyboardInput)
			{
				Stage.inst.OpenKeyboard(textField.text, false, _displayAsPassword ? false : !textField.singleLine,
					_displayAsPassword, false, null, keyboardType, hideInput);
			}
			else
			{
				Input.imeCompositionMode = IMECompositionMode.On;
				_editing = true;

				if (_caret == null)
					CreateCaret();

				if (!string.IsNullOrEmpty(_promptText))
					UpdateAlternativeText();

				float caretSize;
				//如果界面缩小过，光标很容易看不见，这里放大一下
				if (UIConfig.inputCaretSize == 1 && GRoot.contentScaleFactor < 1)
					caretSize = (float)UIConfig.inputCaretSize / GRoot.contentScaleFactor;
				else
					caretSize = UIConfig.inputCaretSize;
				_caret.SetSize(caretSize, textField.textFormat.size);
				_caret.DrawRect(0, Color.clear, textField.textFormat.color);
				AddChild(_caret);

				_selectionShape.Clear();
				AddChild(_selectionShape);

				_caretPosition = textField.text.Length;
				TextField.CharPosition cp = GetCharPosition(_caretPosition);
				AdjustCaret(cp);
				_selectionStart = cp;
			}
		}

		void __focusOut(EventContext contxt)
		{
			if (Stage.keyboardInput)
			{
				Stage.inst.CloseKeyboard();
			}
			else if (_editing)
			{
				_editing = false;

				if (!string.IsNullOrEmpty(_promptText))
					UpdateAlternativeText();

				Input.imeCompositionMode = IMECompositionMode.Auto;
				_caret.RemoveFromParent();
				_selectionShape.RemoveFromParent();
			}
		}

		void __keydown(EventContext context)
		{
			if (!_editing || context.isDefaultPrevented)
				return;

			InputEvent evt = context.inputEvent;

			switch (evt.keyCode)
			{
				case KeyCode.Backspace:
					{
						context.PreventDefault();
						if (_selectionStart != null)
							ReplaceSelection(null);
						else if (_caretPosition > 0)
						{
							int tmp = _caretPosition;
							_caretPosition--;
							ReplaceText(textField.text.Substring(0, tmp - 1) + textField.text.Substring(tmp));
						}

						break;
					}

				case KeyCode.Delete:
					{
						context.PreventDefault();
						if (_selectionStart != null)
							ReplaceSelection(null);
						else if (_caretPosition < textField.text.Length)
							ReplaceText(textField.text.Substring(0, _caretPosition) + textField.text.Substring(_caretPosition + 1));

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
							TextField.CharPosition cp = GetCharPosition(_caretPosition - 1);
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
						if (_caretPosition < textField.text.Length)
						{
							TextField.CharPosition cp = GetCharPosition(_caretPosition + 1);
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

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == 0)
							return;

						TextField.LineInfo line = textField.lines[cp.lineIndex - 1];
						cp = GetCharPosition(new Vector2(_caret.x, line.y + textField.y));
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

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == textField.lines.Count - 1)
							cp.caretIndex = textField.charPositions.Count - 1;
						else
						{
							TextField.LineInfo line = textField.lines[cp.lineIndex + 1];
							cp = GetCharPosition(new Vector2(_caret.x, line.y + textField.y));
						}
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

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						TextField.LineInfo line = textField.lines[cp.lineIndex];
						cp = GetCharPosition(new Vector2(int.MinValue, line.y + textField.y));
						AdjustCaret(cp);
						break;
					}

				case KeyCode.End:
					{
						context.PreventDefault();
						ClearSelection();

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						TextField.LineInfo line = textField.lines[cp.lineIndex];
						cp = GetCharPosition(new Vector2(int.MaxValue, line.y + textField.y));
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
							AdjustCaret(GetCharPosition(textField.text.Length));
						}
						break;
					}

				//Copy
				case KeyCode.C:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
								DoCopy(s);
						}
						break;
					}

				//Paste
				case KeyCode.V:
					{
						if (evt.ctrl)
						{
							context.PreventDefault();
							DoPaste();
						}
						break;
					}

				//Cut
				case KeyCode.X:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
							{
								DoCopy(s);
								ReplaceSelection(null);
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

							if (!textField.singleLine)
								ReplaceSelection("\n");
						}
						break;
					}
			}
		}
	}
}
