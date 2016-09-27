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
		public Shape shape { get; private set; }

		RichTextField _owner;
		HtmlElement _element;
		Vector2[] _points8;
		Vector2[] _points12;
		EventCallback1 _clickHandler;
		EventCallback1 _rolloverHandler;
		EventCallback0 _rolloutHandler;

		public HtmlLink()
		{
			shape = new Shape();
			_clickHandler = (EventContext context) =>
			{
				_owner.onClickLink.BubbleCall(_element.GetString("href"));
			};
			_rolloverHandler = (EventContext context) =>
			{
				context.CaptureTouch();
				if (_owner.htmlParseOptions.linkHoverBgColor.a > 0)
					shape.graphics.Tint(_owner.htmlParseOptions.linkHoverBgColor);
			};
			_rolloutHandler = () =>
			{
				if (_owner.htmlParseOptions.linkBgColor.a > 0)
					shape.graphics.Tint(_owner.htmlParseOptions.linkBgColor);
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
		}

		public void SetArea(Rect rect)
		{
			Rect contentRect = _owner.contentRect;
			rect = ToolSet.Intersection(ref contentRect, ref rect);
			if (rect.width == 0 || rect.height == 0)
				shape.Clear();
			else
			{
				shape.SetXY(rect.x, rect.y);
				shape.SetSize(rect.width, rect.height);
				shape.DrawRect(0, Color.clear, _owner.htmlParseOptions.linkBgColor);
			}
		}

		public void SetArea(Rect r0, Rect r1)
		{
			r1.yMin = r0.yMax;

			Rect contentRect = _owner.contentRect;
			r0 = ToolSet.Intersection(ref contentRect, ref r0);
			r1 = ToolSet.Intersection(ref contentRect, ref r1);

			Rect unionRect = ToolSet.Union(ref r0, ref r1);
			if (unionRect.width == 0 || unionRect.height == 0)
				shape.Clear();
			else
			{
				r0.position -= unionRect.position;
				r1.position -= unionRect.position;
				shape.SetXY(unionRect.x, unionRect.y);
				shape.SetSize(unionRect.width, unionRect.height);

				if (_points8 == null)
					_points8 = new Vector2[8];

				_points8[0] = new Vector2(r0.xMin, r0.yMax);
				_points8[1] = new Vector2(r0.xMin, r0.yMin);
				_points8[2] = new Vector2(r0.xMax, r0.yMin);
				_points8[3] = new Vector2(r0.xMax, r0.yMax);

				_points8[4] = new Vector2(r1.xMax, r1.yMin);
				_points8[5] = new Vector2(r1.xMax, r1.yMax);
				_points8[6] = new Vector2(r1.xMin, r1.yMax);
				_points8[7] = new Vector2(r1.xMin, r1.yMin);

				shape.DrawPolygon(_owner.htmlParseOptions.linkBgColor, _points8);
			}
		}

		public void SetArea(Rect r0, Rect r1, Rect r2)
		{
			Rect contentRect = _owner.contentRect;
			r0 = ToolSet.Intersection(ref contentRect, ref r0);
			r1 = ToolSet.Intersection(ref contentRect, ref r1);
			r2 = ToolSet.Intersection(ref contentRect, ref r2);

			Rect unionRect = ToolSet.Union(ref r0, ref r1);
			unionRect = ToolSet.Union(ref unionRect, ref r2);
			if (unionRect.width == 0 || unionRect.height == 0)
				shape.Clear();
			else
			{
				r0.position -= unionRect.position;
				r1.position -= unionRect.position;
				r2.position -= unionRect.position;
				shape.SetXY(unionRect.x, unionRect.y);
				shape.SetSize(unionRect.width, unionRect.height);

				if (_points12 == null)
					_points12 = new Vector2[12];

				_points12[0] = new Vector2(r0.xMin, r0.yMax);
				_points12[1] = new Vector2(r0.xMin, r0.yMin);
				_points12[2] = new Vector2(r0.xMax, r0.yMin);
				_points12[3] = new Vector2(r0.xMax, r0.yMax);

				_points12[4] = new Vector2(r1.xMax, r1.yMin);
				_points12[5] = new Vector2(r1.xMax, r1.yMax);

				_points12[6] = new Vector2(r2.xMax, r2.yMin);
				_points12[7] = new Vector2(r2.xMax, r2.yMax);
				_points12[8] = new Vector2(r2.xMin, r2.yMax);
				_points12[9] = new Vector2(r2.xMin, r2.yMin);

				_points12[10] = new Vector2(r1.xMin, r1.yMax);
				_points12[11] = new Vector2(r1.xMin, r1.yMin);

				shape.DrawPolygon(_owner.htmlParseOptions.linkBgColor, _points12);
			}
		}

		public void SetPosition(float x, float y)
		{
			//nothing
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
