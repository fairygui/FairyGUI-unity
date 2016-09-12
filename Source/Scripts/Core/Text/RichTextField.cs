using UnityEngine;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Emoji
	{
		/// <summary>
		/// 代表图片资源url。
		/// </summary>
		public string url;

		/// <summary>
		/// 图片宽度。不设置（0）则表示使用原始宽度。
		/// </summary>
		public int width;

		/// <summary>
		/// 图片高度。不设置（0）则表示使用原始高度。
		/// </summary>
		public int height;

		public Emoji(string url, int width, int height)
		{
			this.url = url;
			this.width = width;
			this.height = height;
		}

		public Emoji(string url)
		{
			this.url = url;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class RichTextField : Container
	{
		public EventListener onFocusIn { get; private set; }
		public EventListener onFocusOut { get; private set; }
		public EventListener onChanged { get; private set; }

		public IHtmlPageContext htmlPageContext { get; set; }
		public HtmlParseOptions htmlParseOptions { get; private set; }
		public Dictionary<uint, Emoji> emojies { get; set; }

		public TextField textField { get; private set; }

		public RichTextField()
		{
			Create(new TextField());
		}

		public RichTextField(TextField textField)
		{
			Create(textField);
		}

		void Create(TextField textField)
		{
			CreateGameObject("RichTextField");
			this.opaque = true;

			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");

			htmlPageContext = HtmlPageContext.inst;
			htmlParseOptions = new HtmlParseOptions();

			this.textField = textField;
			textField.richTextField = this;
			AddChild(textField);

			graphics = textField.graphics;
		}

		public string text
		{
			get { return textField.text; }
			set { textField.text = value; }
		}

		public string htmlText
		{
			get { return textField.htmlText; }
			set { textField.htmlText = value; }
		}

		public TextFormat textFormat
		{
			get { return textField.textFormat; }
			set { textField.textFormat = value; }
		}

		public IHtmlObject GetHtmlObject(string name)
		{
			List<HtmlElement> elements = textField.GetHtmlElements();
			int count = elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = elements[i];
				if (element.htmlObject != null && name.Equals(element.name, System.StringComparison.OrdinalIgnoreCase))
					return element.htmlObject;
			}

			return null;
		}

		public IHtmlObject GetHtmlObjectAt(int index)
		{
			List<HtmlElement> elements = textField.GetHtmlElements();
			return elements[index].htmlObject;
		}

		public int htmlObjectCount
		{
			get { return textField.GetHtmlElements().Count; }
		}

		override protected void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			textField.size = this.size;

			base.OnSizeChanged(widthChanged, heightChanged);
		}

		public override void Update(UpdateContext context)
		{
			if (textField.input)
			{
				textField._BeforeClip(context);
				base.Update(context);
				textField._AfterClip(context);
			}
			else
				base.Update(context);
		}
	}
}
