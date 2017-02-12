using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GTextField : GObject, ITextColorGear
	{
		protected TextField _textField;
		protected string _text;
		protected bool _ubbEnabled;
		protected bool _updatingSize;

		public GTextField()
			: base()
		{
			TextFormat tf = _textField.textFormat;
			tf.font = UIConfig.defaultFont;
			tf.size = 12;
			tf.color = Color.black;
			tf.lineSpacing = 3;
			tf.letterSpacing = 0;
			_textField.textFormat = tf;

			_text = string.Empty;
			_textField.autoSize = AutoSizeType.Both;
			_textField.wordWrap = false;
		}

		override protected void CreateDisplayObject()
		{
			_textField = new TextField();
			_textField.gOwner = this;
			displayObject = _textField;
		}

		/// <summary>
		/// 
		/// </summary>
		override public string text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value == null)
					value = string.Empty;
				_text = value;
				UpdateTextFieldText();
				UpdateSize();
				UpdateGear(6);
			}
		}

		virtual protected void UpdateTextFieldText()
		{
			if (_ubbEnabled)
				_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(_text));
			else
				_textField.text = _text;
		}

		/// <summary>
		/// 
		/// </summary>
		public TextFormat textFormat
		{
			get
			{
				return _textField.textFormat;
			}
			set
			{
				_textField.textFormat = value;
				if (!underConstruct)
					UpdateSize();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get
			{
				return _textField.textFormat.color;
			}
			set
			{
				if (!_textField.textFormat.color.Equals(value))
				{
					TextFormat tf = _textField.textFormat;
					tf.color = value;
					_textField.textFormat = tf;
					UpdateGear(4);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AlignType align
		{
			get { return _textField.align; }
			set { _textField.align = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public VertAlignType verticalAlign
		{
			get { return _textField.verticalAlign; }
			set { _textField.verticalAlign = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool singleLine
		{
			get { return _textField.singleLine; }
			set { _textField.singleLine = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int stroke
		{
			get { return _textField.stroke; }
			set { _textField.stroke = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Color strokeColor
		{
			get { return _textField.strokeColor; }
			set
			{
				_textField.strokeColor = value;
				UpdateGear(4);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 shadowOffset
		{
			get { return _textField.shadowOffset; }
			set { _textField.shadowOffset = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool UBBEnabled
		{
			get { return _ubbEnabled; }
			set { _ubbEnabled = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public AutoSizeType autoSize
		{
			get { return _textField.autoSize; }
			set
			{
				_textField.autoSize = value;
				if (value == AutoSizeType.Both)
				{
					_textField.wordWrap = false;

					if (!underConstruct)
						this.SetSize(_textField.textWidth, _textField.textHeight);
				}
				else
				{
					_textField.wordWrap = true;

					if (value == AutoSizeType.Height)
					{
						if (!underConstruct)
							this.height = _textField.textHeight;
					}
					else
						displayObject.SetSize(this.width, this.height);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float textWidth
		{
			get { return _textField.textWidth; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float textHeight
		{
			get { return _textField.textHeight; }
		}

		protected void UpdateSize()
		{
			if (_updatingSize)
				return;

			_updatingSize = true;

			if (_textField.autoSize == AutoSizeType.Both)
				this.size = displayObject.size;
			else if (_textField.autoSize == AutoSizeType.Height)
				this.height = displayObject.height;

			_updatingSize = false;
		}

		override protected void HandleSizeChanged()
		{
			if (_updatingSize)
				return;

			if (underConstruct)
				displayObject.SetSize(this.width, this.height);
			else if (_textField.autoSize != AutoSizeType.Both)
			{
				if (_textField.autoSize == AutoSizeType.Height)
				{
					displayObject.width = this.width;//先调整宽度，让文本重排
					if (this._text != string.Empty) //文本为空时，1是本来就不需要调整， 2是为了防止改掉文本为空时的默认高度，造成关联错误
						SetSizeDirectly(this.width, displayObject.height);
				}
				else
					displayObject.SetSize(this.width, this.height);
			}
		}

		override public void Setup_BeforeAdd(XML xml)
		{
			base.Setup_BeforeAdd(xml);

			TextFormat tf = _textField.textFormat;

			string str;
			str = xml.GetAttribute("font");
			if (str != null)
				tf.font = str;

			str = xml.GetAttribute("fontSize");
			if (str != null)
				tf.size = int.Parse(str);

			str = xml.GetAttribute("color");
			if (str != null)
				tf.color = ToolSet.ConvertFromHtmlColor(str);

			str = xml.GetAttribute("align");
			if (str != null)
				this.align = FieldTypes.ParseAlign(str);

			str = xml.GetAttribute("vAlign");
			if (str != null)
				this.verticalAlign = FieldTypes.ParseVerticalAlign(str);

			str = xml.GetAttribute("leading");
			if (str != null)
				tf.lineSpacing = int.Parse(str);

			str = xml.GetAttribute("letterSpacing");
			if (str != null)
				tf.letterSpacing = int.Parse(str);

			_ubbEnabled = xml.GetAttributeBool("ubb", false);

			str = xml.GetAttribute("autoSize");
			if (str != null)
				this.autoSize = FieldTypes.ParseAutoSizeType(str);

			tf.underline = xml.GetAttributeBool("underline", false);
			tf.italic = xml.GetAttributeBool("italic", false);
			tf.bold = xml.GetAttributeBool("bold", false);
			this.singleLine = xml.GetAttributeBool("singleLine", false);
			str = xml.GetAttribute("strokeColor");
			if (str != null)
			{
				this.strokeColor = ToolSet.ConvertFromHtmlColor(str);
				this.stroke = xml.GetAttributeInt("strokeSize", 1);
			}

			str = xml.GetAttribute("shadowColor");
			if (str != null)
			{
				this.strokeColor = ToolSet.ConvertFromHtmlColor(str);
				this.shadowOffset = xml.GetAttributeVector("shadowOffset");
			}

			_textField.textFormat = tf;
		}

		override public void Setup_AfterAdd(XML xml)
		{
			base.Setup_AfterAdd(xml);

			string str = xml.GetAttribute("text");
			if (str != null && str.Length > 0)
				this.text = str;
		}
	}
}
