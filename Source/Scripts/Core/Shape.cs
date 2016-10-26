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
		Color[] _colors;
		Vector2[] _polygonPoints;

		/// <summary>
		/// 
		/// </summary>
		public Shape()
		{
			CreateGameObject("Shape");
			graphics = new NGraphics(gameObject);
			graphics.texture = NTexture.Empty;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool empty
		{
			get { return _type == 0; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _fillColor; }
			set
			{
				if (!_fillColor.Equals(value))
				{
					_fillColor = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lineSize"></param>
		/// <param name="lineColor"></param>
		/// <param name="fillColor"></param>
		public void DrawRect(int lineSize, Color lineColor, Color fillColor)
		{
			_type = 1;
			_lineSize = lineSize;
			_lineColor = lineColor;
			_fillColor = fillColor;
			_colors = null;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lineSize"></param>
		/// <param name="colors"></param>
		public void DrawRect(int lineSize, Color[] colors)
		{
			_type = 1;
			_lineSize = lineSize;
			_colors = colors;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fillColor"></param>
		public void DrawEllipse(Color fillColor)
		{
			_type = 2;
			_fillColor = fillColor;
			_colors = null;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="colors"></param>
		public void DrawEllipse(Color[] colors)
		{
			_type = 2;
			_colors = colors;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		/// <param name="fillColor"></param>
		public void DrawPolygon(Vector2[] points, Color fillColor)
		{
			_type = 3;
			_polygonPoints = points;
			_fillColor = fillColor;
			_colors = null;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		/// <param name="colors"></param>
		public void DrawPolygon(Vector2[] points, Color[] colors)
		{
			_type = 3;
			_polygonPoints = points;
			_colors = colors;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			_type = 0;
			_touchDisabled = true;
			graphics.ClearMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
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
							graphics.DrawRect(_contentRect, _lineSize, _lineColor, _fillColor, _colors);
						else if (_type == 2)
							graphics.DrawEllipse(_contentRect, _fillColor, _colors);
						else
							graphics.DrawPolygon(_polygonPoints, _fillColor, _colors);
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
