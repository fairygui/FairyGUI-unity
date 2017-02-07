using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 两个指头捏或者放的手势。
	/// </summary>
	public class PinchGesture : EventDispatcher
	{
		/// <summary>
		/// 
		/// </summary>
		public GObject host { get; private set; }

		/// <summary>
		/// 当两个手指开始呈捏手势时派发该事件。
		/// </summary>
		public EventListener onBegin { get; private set; }
		/// <summary>
		/// 当其中一个手指离开屏幕时派发该事件。
		/// </summary>
		public EventListener onEnd { get; private set; }
		/// <summary>
		/// 当手势动作时派发该事件。
		/// </summary>
		public EventListener onAction { get; private set; }

		/// <summary>
		/// 总共缩放的量。
		/// </summary>
		public float scale;

		/// <summary>
		/// 从上次通知后的改变量。
		/// </summary>
		public float delta;

		float _startDistance;
		float _lastScale;
		int[] _touches;
		bool _started;

		public PinchGesture(GObject host)
		{
			this.host = host;
			Enable(true);

			_touches = new int[2];

			onBegin = new EventListener(this, "onPinchBegin");
			onEnd = new EventListener(this, "onPinchEnd");
			onAction = new EventListener(this, "onPinchAction");
		}

		public void Dispose()
		{
			Enable(false);
			host = null;
		}

		public void Enable(bool value)
		{
			if (value)
				host.onTouchBegin.Add(__touchBegin);
			else
			{
				_started = false;
				host.onTouchBegin.Remove(__touchBegin);
				Stage.inst.onTouchMove.Remove(__touchMove);
				Stage.inst.onTouchEnd.Remove(__touchEnd);
			}
		}

		void __touchBegin(EventContext context)
		{
			if (Stage.inst.touchCount == 2)
			{
				if (!_started)
				{
					Stage.inst.GetAllTouch(_touches);
					Vector2 pt1 = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[0]));
					Vector2 pt2 = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[1]));
					_startDistance = Vector2.Distance(pt1, pt2);

					Stage.inst.onTouchMove.Add(__touchMove);
					Stage.inst.onTouchEnd.Add(__touchEnd);
				}
			}
		}

		void __touchMove(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			Vector2 pt1 = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[0]));
			Vector2 pt2 = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[1]));
			float dist = Vector2.Distance(pt1, pt2);

			if (!_started && Mathf.Abs(dist - _startDistance) > UIConfig.touchDragSensitivity)
			{
				_started = true;
				scale = 1;
				_lastScale = 1;

				onBegin.Call(evt);
			}

			if (_started)
			{
				float ss = dist / _startDistance;
				delta = ss - _lastScale;
				_lastScale = ss;
				this.scale += delta;
				onAction.Call(evt);
			}
		}

		void __touchEnd(EventContext context)
		{
			Stage.inst.onTouchMove.Remove(__touchMove);
			Stage.inst.onTouchEnd.Remove(__touchEnd);

			if (_started)
			{
				_started = false;
				onEnd.Call(context.inputEvent);
			}
		}
	}
}
