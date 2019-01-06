using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class CompositeMesh : IMeshFactory, IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly List<IMeshFactory> elements;

		public CompositeMesh()
		{
			elements = new List<IMeshFactory>();
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			int cnt = elements.Count;
			if (cnt == 0)
				elements[0].OnPopulateMesh(vb);
			else
			{
				VertexBuffer vb2 = VertexBuffer.Begin();
				vb2.contentRect = vb.contentRect;
				vb2.uvRect = vb.uvRect;
				vb2.vertexColor = vb.vertexColor;

				for (int i = 0; i < cnt; i++)
				{
					vb2.Clear();
					elements[i].OnPopulateMesh(vb2);
					vb.Append(vb2);
				}

				vb2.End();
			}
		}

		public bool HitTest(Rect contentRect, Vector2 point)
		{
			if (!contentRect.Contains(point))
				return false;

			bool flag = false;
			int cnt = elements.Count;
			for (int i = 0; i < cnt; i++)
			{
				IHitTest ht = elements[i] as IHitTest;
				if (ht != null)
				{
					if (ht.HitTest(contentRect, point))
						return true;
				}
				else
					flag = true;
			}

			return flag;
		}
	}
}