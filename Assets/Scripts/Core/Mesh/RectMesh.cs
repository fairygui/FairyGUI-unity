using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class RectMesh : IMeshFactory, IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		public Rect? drawRect;

		/// <summary>
		/// 
		/// </summary>
		public float lineWidth;

		/// <summary>
		/// 
		/// </summary>
		public Color32 lineColor;

		/// <summary>
		/// 
		/// </summary>
		public Color32? fillColor;

		/// <summary>
		/// 
		/// </summary>
		public Color32[] colors;

		public RectMesh()
		{
			lineColor = Color.black;
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			Rect rect = drawRect != null ? (Rect)drawRect : vb.contentRect;
			Color32 color = fillColor != null ? (Color32)fillColor : vb.vertexColor;
			if (lineWidth == 0)
			{
				if (color.a != 0)//optimized
					vb.AddQuad(rect, color);
			}
			else
			{
				Rect part;

				//left,right
				part = Rect.MinMaxRect(rect.x, rect.y, lineWidth, rect.height);
				vb.AddQuad(part, lineColor);
				part = Rect.MinMaxRect(rect.xMax - lineWidth, 0, rect.xMax, rect.yMax);
				vb.AddQuad(part, lineColor);

				//top, bottom
				part = Rect.MinMaxRect(lineWidth, rect.x, rect.xMax - lineWidth, lineWidth);
				vb.AddQuad(part, lineColor);
				part = Rect.MinMaxRect(lineWidth, rect.yMax - lineWidth, rect.xMax - lineWidth, rect.yMax);
				vb.AddQuad(part, lineColor);

				//middle
				if (color.a != 0)//optimized
				{
					part = Rect.MinMaxRect(lineWidth, lineWidth, rect.xMax - lineWidth, rect.yMax - lineWidth);
					vb.AddQuad(part, color);
				}
			}

			if (colors != null)
				vb.RepeatColors(colors, 0, vb.currentVertCount);

			vb.AddTriangles();
		}

		public bool HitTest(Rect contentRect, Vector2 point)
		{
			if (drawRect != null)
				return ((Rect)drawRect).Contains(point);
			else
				return contentRect.Contains(point);
		}
	}
}
