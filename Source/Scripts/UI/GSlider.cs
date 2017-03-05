using System;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GSlider : GComponent
	{
		double _max;
		double _value;
		ProgressTitleType _titleType;

		GTextField _titleObject;
		GMovieClip _aniObject;
		GObject _barObjectH;
		GObject _barObjectV;
		float _barMaxWidth;
		float _barMaxHeight;
		float _barMaxWidthDelta;
		float _barMaxHeightDelta;
		GObject _gripObject;
		Vector2 _clickPos;
		float _clickPercent;
		int _touchId;

		EventCallback1 _touchMoveDelegate;

		/// <summary>
		/// 
		/// </summary>
		public EventListener onChanged { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onGripTouchEnd { get; private set; }

		public GSlider()
		{
			_value = 50;
			_max = 100;

			onChanged = new EventListener(this, "onChanged");
			onGripTouchEnd = new EventListener(this, "onGripTouchEnd");

			_touchMoveDelegate = __gripTouchMove;
		}

		/// <summary>
		/// 
		/// </summary>
		public ProgressTitleType titleType
		{
			get
			{
				return _titleType;
			}
			set
			{
				if (_titleType != value)
				{
					_titleType = value;
					Update();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double max
		{
			get
			{
				return _max;
			}
			set
			{
				if (_max != value)
				{
					_max = value;
					Update();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double value
		{
			get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					_value = value;
					Update();
				}
			}
		}

		private void Update()
		{
			float percent = (float)Math.Min(_value / _max, 1);
			UpdateWidthPercent(percent);
		}

		private void UpdateWidthPercent(float percent)
		{
			if (_titleObject != null)
			{
				switch (_titleType)
				{
					case ProgressTitleType.Percent:
						_titleObject.text = Mathf.RoundToInt(percent * 100) + "%";
						break;

					case ProgressTitleType.ValueAndMax:
						_titleObject.text = Math.Round(_value) + "/" + Math.Round(max);
						break;

					case ProgressTitleType.Value:
						_titleObject.text = "" + Math.Round(_value);
						break;

					case ProgressTitleType.Max:
						_titleObject.text = "" + Math.Round(_max);
						break;
				}
			}

			if (_barObjectH != null)
				_barObjectH.width = Mathf.RoundToInt((this.width - _barMaxWidthDelta) * percent);
			if (_barObjectV != null)
				_barObjectV.height = Mathf.RoundToInt((this.height - _barMaxHeightDelta) * percent);
			if (_aniObject != null)
				_aniObject.frame = Mathf.RoundToInt(percent * 100);

			InvalidateBatchingState(true);
		}

		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

			XML xml = cxml.GetNode("Slider");

			string str;
			str = xml.GetAttribute("titleType");
			if (str != null)
				_titleType = FieldTypes.ParseProgressTitleType(str);
			else
				_titleType = ProgressTitleType.Percent;

			_titleObject = GetChild("title") as GTextField;
			_barObjectH = GetChild("bar");
			_barObjectV = GetChild("bar_v");
			_aniObject = GetChild("ani") as GMovieClip;
			_gripObject = GetChild("grip");

			if (_barObjectH != null)
			{
				_barMaxWidth = _barObjectH.width;
				_barMaxWidthDelta = this.width - _barMaxWidth;
			}
			if (_barObjectV != null)
			{
				_barMaxHeight = _barObjectV.height;
				_barMaxHeightDelta = this.height - _barMaxHeight;
			}

			if (_gripObject != null)
			{
				_gripObject.onTouchBegin.Add(__gripTouchBegin);
				_gripObject.onTouchEnd.Add(__gripTouchEnd);
			}
		}

		override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

			XML xml = cxml.GetNode("Slider");
			if (xml != null)
			{
				_value = xml.GetAttributeInt("value");
				_max = xml.GetAttributeInt("max");
			}
			Update();
		}

		override protected void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if (_barObjectH != null)
				_barMaxWidth = this.width - _barMaxWidthDelta;
			if (_barObjectV != null)
				_barMaxHeight = this.height - _barMaxHeightDelta;

			if (!this.underConstruct)
				Update();
		}

		private void __gripTouchBegin(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			_touchId = evt.touchId;

			_clickPos = this.GlobalToLocal(new Vector2(evt.x, evt.y));
			_clickPercent = (float)(_value / _max);

			context.CaptureTouch();
			Stage.inst.onTouchMove.Add(_touchMoveDelegate);
		}

		private void __gripTouchMove(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			if (_touchId != evt.touchId)
				return;

			Vector2 pt = this.GlobalToLocal(new Vector2(evt.x, evt.y));
			if (float.IsNaN(pt.x))
				return;

			float deltaX = pt.x - _clickPos.x;
			float deltaY = pt.y - _clickPos.y;

			float percent;
			if (_barObjectH != null)
				percent = _clickPercent + deltaX / _barMaxWidth;
			else
				percent = _clickPercent + deltaY / _barMaxHeight;
			if (percent > 1)
				percent = 1;
			else if (percent < 0)
				percent = 0;

			double newValue = percent * _max;
			if (newValue != _value)
			{
				_value = newValue;
				onChanged.Call();
			}
			UpdateWidthPercent(percent);
		}

		private void __gripTouchEnd(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			if (_touchId != evt.touchId)
				return;

			Stage.inst.onTouchMove.Remove(_touchMoveDelegate);

			if (displayObject == null || displayObject.isDisposed)
				return;

			float percent = (float)(_value / _max);
			UpdateWidthPercent(percent);

			onGripTouchEnd.Call();
		}
	}
}
