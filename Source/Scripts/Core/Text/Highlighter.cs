using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	class Highlighter
	{
		public Transform cachedTransform { get; private set; }
		public NGraphics grahpics { get; private set; }
		public GameObject gameObject { get; private set; }

		Color _color;
		List<Rect> _rects;

		public Highlighter()
		{
			gameObject = new GameObject("Highlighter");
			gameObject.hideFlags = HideFlags.HideInHierarchy;
			Object.DontDestroyOnLoad(gameObject);
			cachedTransform = gameObject.transform;

			grahpics = new NGraphics(gameObject);
			grahpics.texture = NTexture.Empty;
			grahpics.enabled = false;

			_color = UIConfig.inputHighlightColor;
			_rects = new List<Rect>();
		}

		public void SetParent(Transform parent)
		{
			if (parent != null)
			{
				cachedTransform.parent = parent;
				cachedTransform.localPosition = new Vector3(0, 0, -7f);
				cachedTransform.localScale = new Vector3(1, 1, 1);
				cachedTransform.localEulerAngles = new Vector3(0, 0, 0);
				gameObject.layer = parent.gameObject.layer;
			}
			else
			{
				cachedTransform.parent = null;
				grahpics.enabled = false;
			}
		}

		public void BeginUpdate()
		{
			_rects.Clear();
		}

		public void AddRect(Rect rect)
		{
			_rects.Add(rect);
		}

		public void EndUpdate()
		{
			if (_rects.Count == 0)
			{
				grahpics.enabled = false;
				return;
			}
			grahpics.enabled = true;

			int count = _rects.Count * 4;
			grahpics.Alloc(count);
			Rect uvRect = new Rect(0, 0, 1, 1);
			for (int i = 0; i < count; i += 4)
			{
				grahpics.FillVerts(i, _rects[i / 4]);
				grahpics.FillUV(i, uvRect);
			}
			grahpics.FillColors(_color);
			grahpics.FillTriangles();
			grahpics.UpdateMesh();
		}

		public void Clear()
		{
			grahpics.enabled = false;
		}
	}
}
