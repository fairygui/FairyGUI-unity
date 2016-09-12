using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GTextInput : GTextField
	{
		string _promptText;
		bool _displayAsPassword;

		public GTextInput()
		{
			this.focusable = true;
			_textField.autoSize = false;
			_textField.wordWrap = false;
			_textField.onChanged.AddCapture(__onChanged);
			_textField.onFocusIn.AddCapture(__onFocusIn);
			_textField.onFocusOut.AddCapture(__onFocusOut);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool editable
		{
			get { return _textField.input; }
			set { _textField.input = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int maxLength
		{
			get { return _textField.maxLength; }
			set { _textField.maxLength = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string restrict
		{
			get { return _textField.restrict; }
			set { _textField.restrict = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		override public bool displayAsPassword
		{
			get
			{
				return _displayAsPassword;
			}
			set
			{
				_displayAsPassword = value;
				_textField.displayAsPassword = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int caretPosition
		{
			get { return _textField.caretPosition; }
			set { _textField.caretPosition = value; }
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
				UpdateTextFieldText();
			}
		}

		/// <summary>
		/// <see cref="UnityEngine.TouchScreenKeyboardType"/>
		/// </summary>
		public int keyboardType
		{
			get { return _textField.keyboardType; }
			set { _textField.keyboardType = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceSelection(string value)
		{
			_textField.ReplaceSelection(value);
			_text = _textField.text;
			UpdateSize();
		}

		override protected void CreateDisplayObject()
		{
			base.CreateDisplayObject();

			_textField.input = true;
			RichTextField richTextField = new RichTextField(_textField);
			_textField.gOwner = null;
			richTextField.gOwner = this;
			displayObject = richTextField;
		}

		public override void Setup_BeforeAdd(Utils.XML xml)
		{
			base.Setup_BeforeAdd(xml);

			_promptText = xml.GetAttribute("prompt");
			_textField.restrict = xml.GetAttribute("restrict");
			_textField.maxLength = xml.GetAttributeInt("maxLength", int.MaxValue);
			_textField.keyboardType = xml.GetAttributeInt("keyboardType");
		}

		public override void Setup_AfterAdd(XML xml)
		{
			base.Setup_AfterAdd(xml);

			if (string.IsNullOrEmpty(_text))
			{
				if (!string.IsNullOrEmpty(_promptText))
				{
					_textField.displayAsPassword = false;
					_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
				}
			}
		}

		override protected void UpdateTextFieldText()
		{
			if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
			{
				_textField.displayAsPassword = false;
				_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
			}
			else
			{
				_textField.displayAsPassword = _displayAsPassword;
				_textField.text = _text;
			}
		}

		void __onChanged(EventContext context)
		{
			_text = _textField.text;
			UpdateSize();
		}

		void __onFocusIn(EventContext context)
		{
			if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
			{
				_textField.displayAsPassword = _displayAsPassword;
				_textField.text = string.Empty;
			}
		}

		void __onFocusOut(EventContext context)
		{
			_text = _textField.text;
			if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
			{
				_textField.displayAsPassword = false;
				_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
			}
		}
	}
}