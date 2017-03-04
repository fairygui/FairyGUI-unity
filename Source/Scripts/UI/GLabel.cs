using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GLabel class.
	/// </summary>
	public class GLabel : GComponent, IColorGear
	{
		protected GObject _titleObject;
		protected GObject _iconObject;

		public GLabel()
		{
		}

		/// <summary>
		/// Icon of the label.
		/// </summary>
		override public string icon
		{
			get
			{
				if (_iconObject != null)
					return _iconObject.icon;
				else
					return null;
			}

			set
			{
				if (_iconObject != null)
					_iconObject.icon = value;
				UpdateGear(7);
			}
		}

		/// <summary>
		/// Title of the label.
		/// </summary>
		public string title
		{
			get
			{
				if (_titleObject != null)
					return _titleObject.text;
				else
					return null;
			}
			set
			{
				if (_titleObject != null)
					_titleObject.text = value;
				UpdateGear(6);
			}
		}

		/// <summary>
		/// Same of the title.
		/// </summary>
		override public string text
		{
			get { return this.title; }
			set { this.title = value; }
		}

		/// <summary>
		/// If title is input text.
		/// </summary>
		public bool editable
		{
			get
			{
				if (_titleObject is GTextInput)
					return _titleObject.asTextInput.editable;
				else
					return false;
			}

			set
			{
				if (_titleObject is GTextInput)
					_titleObject.asTextInput.editable = value;
			}
		}

		/// <summary>
		/// Title color of the label
		/// </summary>
		public Color titleColor
		{
			get
			{
				if (_titleObject is GTextField)
					return ((GTextField)_titleObject).color;
				else if (_titleObject is GLabel)
					return ((GLabel)_titleObject).titleColor;
				else if (_titleObject is GButton)
					return ((GButton)_titleObject).titleColor;
				else
					return Color.black;
			}
			set
			{
				if (_titleObject is GTextField)
					((GTextField)_titleObject).color = value;
				else if (_titleObject is GLabel)
					((GLabel)_titleObject).titleColor = value;
				else if (_titleObject is GButton)
					((GButton)_titleObject).titleColor = value;
			}
		}

		public int titleFontSize
		{
			get
			{
				if (_titleObject is GTextField)
					return ((GTextField)_titleObject).textFormat.size;
				else if (_titleObject is GLabel)
					return ((GLabel)_titleObject).titleFontSize;
				else if (_titleObject is GButton)
					return ((GButton)_titleObject).titleFontSize;
				else
					return 0;
			}
			set
			{
				if (_titleObject is GTextField)
				{
					TextFormat tf = ((GTextField)_titleObject).textFormat;
					tf.size = value;
					((GTextField)_titleObject).textFormat = tf;
				}
				else if (_titleObject is GLabel)
					((GLabel)_titleObject).titleFontSize = value;
				else if (_titleObject is GButton)
					((GButton)_titleObject).titleFontSize = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return this.titleColor; }
			set
			{
				this.titleColor = value;
				UpdateGear(4);
			}
		}

		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

			_titleObject = GetChild("title");
			_iconObject = GetChild("icon");
		}

		override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

			XML xml = cxml.GetNode("Label");
			if (xml == null)
				return;

			string str;
			str = xml.GetAttribute("title");
			if (str != null)
				this.title = str;
			str = xml.GetAttribute("icon");
			if (str != null)
				this.icon = str;
			str = xml.GetAttribute("titleColor");
			if (str != null)
				this.titleColor = ToolSet.ConvertFromHtmlColor(str);
			str = xml.GetAttribute("titleFontSize");
			if (str != null)
				this.titleFontSize = int.Parse(str);

			if (_titleObject is GTextInput)
			{
				GTextInput input = ((GTextInput)_titleObject);
				str = xml.GetAttribute("prompt");
				if (str != null)
					input.promptText = str;

				str = xml.GetAttribute("restrict");
				if (str != null)
					input.restrict = str;

				input.maxLength = xml.GetAttributeInt("maxLength", input.maxLength);
				input.keyboardType = xml.GetAttributeInt("keyboardType", input.keyboardType);
				input.displayAsPassword = xml.GetAttributeBool("password", input.displayAsPassword);
			}
		}
	}
}
