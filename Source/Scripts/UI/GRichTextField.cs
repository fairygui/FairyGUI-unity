using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// GRichTextField class.
	/// </summary>
	public class GRichTextField : GTextField
	{
		public GRichTextField()
			: base()
		{
		}

		override protected void CreateDisplayObject()
		{
			base.CreateDisplayObject();

			RichTextField richTextField = new RichTextField(_textField);
			_textField.gOwner = null;
			richTextField.gOwner = this;
			displayObject = richTextField;
		}

		override protected void UpdateTextFieldText()
		{
			if (_ubbEnabled)
				_textField.htmlText = UBBParser.inst.Parse(_text);
			else
				_textField.htmlText = _text;
		}

		public IHtmlObject GetHtmlObject(string name)
		{
			return _textField.richTextField.GetHtmlObject(name);
		}

		public IHtmlObject GetHtmlObjectAt(int index)
		{
			return _textField.richTextField.GetHtmlObjectAt(index);
		}

		public int htmlObjectCount
		{
			get { return _textField.richTextField.htmlObjectCount; }
		}
	}
}
