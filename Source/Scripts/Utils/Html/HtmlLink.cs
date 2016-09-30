using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class HtmlLink : IHtmlObject
	{
		public SelectionShape shape { get; private set; }

		RichTextField _owner;
		HtmlElement _element;

		EventCallback1 _clickHandler;
		EventCallback1 _rolloverHandler;
		EventCallback0 _rolloutHandler;

		public HtmlLink()
		{
			shape = new SelectionShape();

			_clickHandler = (EventContext context) =>
			{
				_owner.onClickLink.BubbleCall(_element.GetString("href"));
			};
			_rolloverHandler = (EventContext context) =>
			{
				context.CaptureTouch();
				if (_owner.htmlParseOptions.linkHoverBgColor.a > 0)
					shape.color = _owner.htmlParseOptions.linkHoverBgColor;
			};
			_rolloutHandler = () =>
			{
				if (_owner.htmlParseOptions.linkHoverBgColor.a > 0)
					shape.color = _owner.htmlParseOptions.linkBgColor;
			};
		}

		public HtmlElement element
		{
			get { return _element; }
		}

		public float width
		{
			get { return 0; }
		}

		public float height
		{
			get { return 0; }
		}

		public void Create(RichTextField owner, HtmlElement element)
		{
			_owner = owner;
			_element = element;
			shape.onClick.Add(_clickHandler);
			if (!Stage.touchScreen)
			{
				shape.onRollOver.Add(_rolloverHandler);
				shape.onRollOut.Add(_rolloutHandler);
			}
			else
			{
				shape.onTouchBegin.Add(_rolloverHandler);
				shape.onTouchEnd.Add(_rolloutHandler);
			}
			shape.color = _owner.htmlParseOptions.linkBgColor;
		}

		public void SetArea(int startLine, float startCharX, int endLine, float endCharX)
		{
			List<Rect> rects = shape.rects;
			if (rects == null)
				rects = new List<Rect>(2);
			_owner.textField.GetLinesShape(startLine, startCharX, endLine, endCharX, true, rects);
			shape.rects = rects;
		}

		public void SetPosition(float x, float y)
		{
			shape.SetXY(x, y);
		}

		public void Add()
		{
			//add below text
			_owner.AddChildAt(shape, 0);
		}

		public void Remove()
		{
			if (shape.parent != null)
				_owner.RemoveChild(shape);
		}

		public void Release()
		{
			shape.RemoveEventListeners();

			_owner = null;
			_element = null;
		}

		public void Dispose()
		{
			shape.Dispose();
		}
	}
}
