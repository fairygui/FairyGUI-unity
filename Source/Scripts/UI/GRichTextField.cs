using System.Collections.Generic;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// GRichTextField class.
	/// </summary>
	public class GRichTextField : GTextField
	{
		/// <summary>
		/// 
		/// </summary>
		public RichTextField richTextField { get; private set; }

		public GRichTextField()
			: base()
		{
		}

		override protected void CreateDisplayObject()
		{
			richTextField = new RichTextField();
			richTextField.gOwner = this;
			displayObject = richTextField;

			_textField = richTextField.textField;
		}

		override protected void UpdateTextFieldText()
		{
			if (_ubbEnabled)
				_textField.htmlText = UBBParser.inst.Parse(_text);
			else
				_textField.htmlText = _text;
		}

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<uint, Emoji> emojies
		{
			get { return richTextField.emojies; }
			set { richTextField.emojies = value; }
		}
	}
}
