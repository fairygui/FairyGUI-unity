using UnityEngine;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class RichTextField : Container
	{
		/// <summary>
		/// 
		/// </summary>
		public IHtmlPageContext htmlPageContext { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public HtmlParseOptions htmlParseOptions { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<uint, Emoji> emojies { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public TextField textField { get; private set; }

		List<IHtmlObject> _toCollect;
		EventCallback0 _refreshObjectsDelegate;

		public RichTextField()
		{
			CreateGameObject("RichTextField");
			this.opaque = true;

			htmlPageContext = HtmlPageContext.inst;
			htmlParseOptions = new HtmlParseOptions();

			this.textField = new TextField();
			textField.EnableRichSupport(this);
			AddChild(textField);

			_refreshObjectsDelegate = InternalRefreshObjects;
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public string text
		{
			get { return textField.text; }
			set { textField.text = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public string htmlText
		{
			get { return textField.htmlText; }
			set { textField.htmlText = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public TextFormat textFormat
		{
			get { return textField.textFormat; }
			set { textField.textFormat = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IHtmlObject GetHtmlObject(string name)
		{
			List<HtmlElement> elements = textField.htmlElements;
			int count = elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = elements[i];
				if (element.htmlObject != null && name.Equals(element.name, System.StringComparison.OrdinalIgnoreCase))
					return element.htmlObject;
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public IHtmlObject GetHtmlObjectAt(int index)
		{
			List<HtmlElement> elements = textField.htmlElements;
			return elements[index].htmlObject;
		}

		/// <summary>
		/// 
		/// </summary>
		public int htmlObjectCount
		{
			get { return textField.htmlElements.Count; }
		}

		override protected void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			textField.size = this.size;

			base.OnSizeChanged(widthChanged, heightChanged);
		}

		internal void CleanupObjects()
		{
			List<HtmlElement> elements = textField.htmlElements;
			int count = elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = elements[i];
				if (element.htmlObject != null)
				{
					if (UpdateContext.working)
					{
						//Update里不允许增删对象。放到延迟队列里
						if (_toCollect == null)
							_toCollect = new List<IHtmlObject>();
						_toCollect.Add(element.htmlObject);
					}
					else
					{
						element.htmlObject.Remove();
						htmlPageContext.FreeObject(element.htmlObject);
					}
				}
			}
		}

		internal void RefreshObjects()
		{
			if (UpdateContext.working)
				UpdateContext.OnEnd += _refreshObjectsDelegate;
			else
				InternalRefreshObjects();
		}

		virtual protected void InternalRefreshObjects()
		{
			List<HtmlElement> elements = textField.htmlElements;
			int count = _toCollect != null ? _toCollect.Count : 0;
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					IHtmlObject htmlObject = _toCollect[i];
					htmlObject.Remove();
					htmlPageContext.FreeObject(htmlObject);
				}
				_toCollect.Clear();
			}

			count = elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = elements[i];
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
	}
}
