using System;
using FairyGUI.Utils;
using UnityEngine;
using DG.Tweening;

namespace FairyGUI
{
	/// <summary>
	/// GProgressBar class.
	/// </summary>
	public class GProgressBar : GComponent
	{
		double _max;
		double _value;
		ProgressTitleType _titleType;
		bool _reverse;

		GTextField _titleObject;
		GMovieClip _aniObject;
		GObject _barObjectH;
		GObject _barObjectV;
		float _barMaxWidth;
		float _barMaxHeight;
		float _barMaxWidthDelta;
		float _barMaxHeightDelta;
		float _barStartX;
		float _barStartY;

		Tweener _tweener;

		public GProgressBar()
		{
			_value = 50;
			_max = 100;
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
					Update(_value);
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
					Update(_value);
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
				if (_tweener != null)
				{
					_tweener.Kill(true);
					_tweener = null;
				}

				if (_value != value)
				{
					_value = value;
					Update(_value);
				}
			}
		}

		/// <summary>
		/// 动态改变进度值。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="duration"></param>
		public Tweener TweenValue(double value, float duration)
		{
			if (_value != value)
			{
				if (_tweener != null)
					_tweener.Kill(false);

				double oldValue = _value;
				_value = value;
				_tweener = DOTween.To(() => oldValue, v => { Update(v); }, value, duration)
					.SetEase(Ease.Linear).OnComplete(() => { _tweener = null; });

				return _tweener;
			}
			else
				return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newValue"></param>
		public void Update(double newValue)
		{
			float percent = (float)Math.Min(newValue / _max, 1);
			if (_titleObject != null)
			{
				switch (_titleType)
				{
					case ProgressTitleType.Percent:
						_titleObject.text = Mathf.RoundToInt(percent * 100) + "%";
						break;

					case ProgressTitleType.ValueAndMax:
						_titleObject.text = Math.Round(newValue) + "/" + Math.Round(max);
						break;

					case ProgressTitleType.Value:
						_titleObject.text = "" + Math.Round(newValue);
						break;

					case ProgressTitleType.Max:
						_titleObject.text = "" + Math.Round(_max);
						break;
				}
			}

			float fullWidth = this.width - _barMaxWidthDelta;
			float fullHeight = this.height - _barMaxHeightDelta;
			if (!_reverse)
			{
				if (_barObjectH != null)
				{
					if ((_barObjectH is GImage) && ((GImage)_barObjectH).fillMethod != FillMethod.None)
						((GImage)_barObjectH).fillAmount = percent;
					else if ((_barObjectH is GLoader) && ((GLoader)_barObjectH).fillMethod != FillMethod.None)
						((GLoader)_barObjectH).fillAmount = percent;
					else
						_barObjectH.width = Mathf.RoundToInt(fullWidth * percent);
				}
				if (_barObjectV != null)
				{
					if ((_barObjectV is GImage) && ((GImage)_barObjectV).fillMethod != FillMethod.None)
						((GImage)_barObjectV).fillAmount = percent;
					else if ((_barObjectV is GLoader) && ((GLoader)_barObjectV).fillMethod != FillMethod.None)
						((GLoader)_barObjectV).fillAmount = percent;
					else
						_barObjectV.height = Mathf.RoundToInt(fullHeight * percent);
				}
			}
			else
			{
				if (_barObjectH != null)
				{
					if ((_barObjectH is GImage) && ((GImage)_barObjectH).fillMethod != FillMethod.None)
						((GImage)_barObjectH).fillAmount = 1 - percent;
					else if ((_barObjectH is GLoader) && ((GLoader)_barObjectH).fillMethod != FillMethod.None)
						((GLoader)_barObjectH).fillAmount = 1 - percent;
					else
					{
						_barObjectH.width = Mathf.RoundToInt(fullWidth * percent);
						_barObjectH.x = _barStartX + (fullWidth - _barObjectH.width);
					}
				}
				if (_barObjectV != null)
				{
					if ((_barObjectV is GImage) && ((GImage)_barObjectV).fillMethod != FillMethod.None)
						((GImage)_barObjectV).fillAmount = 1 - percent;
					else if ((_barObjectV is GLoader) && ((GLoader)_barObjectV).fillMethod != FillMethod.None)
						((GLoader)_barObjectV).fillAmount = 1 - percent;
					else
					{
						_barObjectV.height = Mathf.RoundToInt(fullHeight * percent);
						_barObjectV.y = _barStartY + (fullHeight - _barObjectV.height);
					}
				}
			}
			if (_aniObject != null)
				_aniObject.frame = Mathf.RoundToInt(percent * 100);

			InvalidateBatchingState(true);
		}

		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

			XML xml = cxml.GetNode("ProgressBar");

			string str;
			str = xml.GetAttribute("titleType");
			if (str != null)
				_titleType = FieldTypes.ParseProgressTitleType(str);
			else
				_titleType = ProgressTitleType.Percent;
			_reverse = xml.GetAttributeBool("reverse", false);

			_titleObject = GetChild("title") as GTextField;
			_barObjectH = GetChild("bar");
			_barObjectV = GetChild("bar_v");
			_aniObject = GetChild("ani") as GMovieClip;

			if (_barObjectH != null)
			{
				_barMaxWidth = _barObjectH.width;
				_barMaxWidthDelta = this.width - _barMaxWidth;
				_barStartX = _barObjectH.x;
			}
			if (_barObjectV != null)
			{
				_barMaxHeight = _barObjectV.height;
				_barMaxHeightDelta = this.height - _barMaxHeight;
				_barStartY = _barObjectV.y;
			}
		}

		override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

			XML xml = cxml.GetNode("ProgressBar");
			if (xml != null)
			{
				_value = xml.GetAttributeInt("value");
				_max = xml.GetAttributeInt("max");
			}

			Update(_value);
		}

		override protected void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if (_barObjectH != null)
				_barMaxWidth = this.width - _barMaxWidthDelta;
			if (_barObjectV != null)
				_barMaxHeight = this.height - _barMaxHeightDelta;

			if (!this.underConstruct)
				Update(_value);
		}

		public override void Dispose()
		{
			if (_tweener != null)
				_tweener.Kill(false);
			base.Dispose();
		}
	}
}
