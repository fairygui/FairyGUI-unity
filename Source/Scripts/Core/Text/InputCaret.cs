using UnityEngine;

namespace FairyGUI
{
	class InputCaret
	{
		public Transform cachedTransform { get; private set; }
		public NGraphics grahpics { get; private set; }
		public GameObject gameObject { get; private set; }

		float _nextBlink;
		Vector2 _size;

		public InputCaret()
		{
			gameObject = new GameObject("InputCaret");
			gameObject.hideFlags = HideFlags.HideInHierarchy;
			Object.DontDestroyOnLoad(gameObject);

			grahpics = new NGraphics(gameObject);
			grahpics.texture = NTexture.Empty;
			grahpics.enabled = false;

			_size = new Vector2(UIConfig.inputCaretSize + 0.5f, 1);

			cachedTransform = gameObject.transform;
		}

		public void SetParent(Transform parent)
		{
			if (parent != null)
			{
				cachedTransform.parent = parent;
				cachedTransform.localPosition = new Vector3(0, 0, 0);
				cachedTransform.localScale = new Vector3(1, 1, 1);
				cachedTransform.localEulerAngles = new Vector3(0, 0, 0);
				gameObject.layer = parent.gameObject.layer;
				_nextBlink = Time.time + 0.5f;
				grahpics.enabled = true;

				Input.imeCompositionMode = IMECompositionMode.On;
				Vector2 cp = StageCamera.main.WorldToScreenPoint(cachedTransform.TransformPoint(new Vector3(0, 0, 0)));
				cp.y += _size.y;
				Input.compositionCursorPos = cp;
			}
			else
			{
				cachedTransform.parent = null;
				grahpics.enabled = false;
				Input.imeCompositionMode = IMECompositionMode.Off;
			}
		}

		public void SetPosition(Vector2 pos)
		{
			cachedTransform.localPosition = new Vector3(pos.x, -pos.y, 0);
			Vector2 cp = StageCamera.main.WorldToScreenPoint(cachedTransform.TransformPoint(new Vector3(0, 0, 0)));
			cp.y += _size.y;
			Input.compositionCursorPos = cp;

			_nextBlink = Time.time + 0.5f;
			grahpics.enabled = true;
		}

		public void SetSizeAndColor(int size, Color color)
		{
			_size.y = size;
			grahpics.SetOneQuadMesh(new Rect(0, 0, _size.x, size + 1), new Rect(0, 0, 1, 1), color);
		}

		public void Blink()
		{
			if (_nextBlink < Time.time)
			{
				_nextBlink = Time.time + 0.5f;
				grahpics.enabled = !grahpics.enabled;
			}
		}
	}
}
