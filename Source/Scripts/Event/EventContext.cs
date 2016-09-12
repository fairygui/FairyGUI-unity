using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class EventContext
	{
		public EventDispatcher sender { get; internal set; }
		public object initiator { get; internal set; }
		public InputEvent inputEvent { get; internal set; }
		public string type;
		public object data;

		internal bool _defaultPrevented;
		internal bool _stopsPropagation;
		internal bool _touchEndCapture;

		internal List<EventBridge> callChain = new List<EventBridge>();

		public void StopPropagation()
		{
			_stopsPropagation = true;
		}

		public void PreventDefault()
		{
			_defaultPrevented = true;
		}

		public void CaptureTouch()
		{
			_touchEndCapture = true;
		}

		public bool isDefaultPrevented
		{
			get { return _defaultPrevented; }
		}

		static Stack<EventContext> pool = new Stack<EventContext>();
		internal static EventContext Get()
		{
			if (pool.Count > 0)
			{
				EventContext context = pool.Pop();
				context._stopsPropagation = false;
				context._defaultPrevented = false;
				context._touchEndCapture = false;
				return context;
			}
			else
				return new EventContext();
		}

		internal static void Return(EventContext value)
		{
			pool.Push(value);
		}
	}

}
