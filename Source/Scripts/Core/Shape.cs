using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Shape : DisplayObject
	{
		int _type;
		int _lineSize;
		Color _lineColor;
		Color _fillColor;
		Vector2[] _polygonPoints;

		public Shape()
		{
			CreateGameObject("Shape");
			graphics = new NGraphics(gameObject);
			graphics.texture = NTexture.Empty;
		}

		public bool empty
		{
			get { return _type == 0; }
		}

		public void DrawRect(int lineSize, Color lineColor, Color fillColor)
		{
			_type = 1;
			_optimizeNotTouchable = false;
			_lineSize = lineSize;
			_lineColor = lineColor;
			_fillColor = fillColor;
			_requireUpdateMesh = true;
		}

		public void DrawEllipse(Color fillColor)
		{
			_type = 2;
			_optimizeNotTouchable = false;
			_lineSize = 0;
			_lineColor = Color.clear;
			_fillColor = fillColor;
			_requireUpdateMesh = true;
		}

		public void DrawPolygon(Color fillColor, Vector2[] points)
		{
			_type = 3;
			_optimizeNotTouchable = false;
			_lineSize = 0;
			_lineColor = Color.clear;
			_fillColor = fillColor;
			_requireUpdateMesh = true;
			_polygonPoints = points;
		}

		public void Clear()
		{
			_type = 0;
			_optimizeNotTouchable = true;
			graphics.ClearMesh();
		}

		public override void Update(UpdateContext context)
		{
			if (_requireUpdateMesh)
			{
				_requireUpdateMesh = false;
				if (_type != 0)
				{
					if (_contentRect.width > 0 && _contentRect.height > 0)
					{
						if (_type == 1)
							graphics.DrawRect(_contentRect, _lineSize, _lineColor, _fillColor);
						else if (_type == 2)
							graphics.DrawEllipse(_contentRect, _fillColor);
						else
							graphics.DrawPolygon(_polygonPoints, _fillColor);
					}
					else
						graphics.ClearMesh();
				}
			}

			base.Update(context);
		}

		protected override DisplayObject HitTest()
		{
			if (_type != 3)
				return base.HitTest();
			else
			{
				Vector2 localPoint = WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);
				if (!_contentRect.Contains(localPoint))
					return null;

				// Algorithm & implementation thankfully taken from:
				// -> http://alienryderflex.com/polygon/
				// inspired by Starling
				int len = _polygonPoints.Length;
				int i;
				int j = len - 1;
				bool oddNodes = false;

				for (i = 0; i < len; ++i)
				{
					float ix = _polygonPoints[i].x;
					float iy = _polygonPoints[i].y;
					float jx = _polygonPoints[j].x;
					float jy = _polygonPoints[j].y;

					if ((iy < localPoint.y && jy >= localPoint.y || jy < localPoint.y && iy >= localPoint.y) && (ix <= localPoint.x || jx <= localPoint.x))
					{
						if (ix + (localPoint.y - iy) / (jy - iy) * (jx - ix) < localPoint.x)
							oddNodes = !oddNodes;
					}

					j = i;
				}

				return oddNodes ? this : null;
			}
		}
	}
}
