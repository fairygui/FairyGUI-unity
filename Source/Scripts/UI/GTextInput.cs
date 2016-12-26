using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GTextInput : GTextField
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
		public InputTextField inputTextField { get; private set; }

		public GTextInput()
		{
			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");

			this.focusable = true;
			_textField.autoSize = AutoSizeType.None;
			_textField.wordWrap = false;
		}

		public override string text
		{
			get
			{
				_text = inputTextField.text;
				return _text;
			}
			set
			{
				base.text = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool editable
		{
			get { return inputTextField.editable; }
			set { inputTextField.editable = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool hideInput
		{
			get { return inputTextField.hideInput; }
			set { inputTextField.hideInput = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int maxLength
		{
			get { return inputTextField.maxLength; }
			set { inputTextField.maxLength = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string restrict
		{
			get { return inputTextField.restrict; }
			set { inputTextField.restrict = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool displayAsPassword
		{
			get { return inputTextField.displayAsPassword; }
			set { inputTextField.displayAsPassword = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int caretPosition
		{
			get { return inputTextField.caretPosition; }
			set { inputTextField.caretPosition = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string promptText
		{
			get { return inputTextField.promptText; }
			set { inputTextField.promptText = value; }
		}

		/// <summary>
		/// <see cref="UnityEngine.TouchScreenKeyboardType"/>
		/// </summary>
		public int keyboardType
		{
			get { return inputTextField.keyboardType; }
			set { inputTextField.keyboardType = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<uint, Emoji> emojies
		{
			get { return inputTextField.emojies; }
			set { inputTextField.emojies = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceSelection(string value)
		{
			inputTextField.ReplaceSelection(value);
		}

		override protected void UpdateTextFieldText()
		{
			inputTextField.text = _text;
		}

		override protected void CreateDisplayObject()
		{
			inputTextField = new InputTextField();
			inputTextField.gOwner = this;
			displayObject = inputTextField;

			_textField = inputTextField.textField;
		}

		public override void Setup_BeforeAdd(Utils.XML xml)
		{
			base.Setup_BeforeAdd(xml);

			string str = xml.GetAttribute("prompt");
			if (str != null)
				inputTextField.promptText = str;
			inputTextField.displayAsPassword = xml.GetAttributeBool("password", false);
			inputTextField.restrict = xml.GetAttribute("restrict");
			inputTextField.maxLength = xml.GetAttributeInt("maxLength", int.MaxValue);
			inputTextField.keyboardType = xml.GetAttributeInt("keyboardType");
		}
	}
}