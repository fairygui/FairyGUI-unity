using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FairyGUI.Utils;

namespace FairyGUI
{
	class GearColorValue
	{
		public Color color;
		public Color strokeColor;

		public GearColorValue()
		{
			//兼容旧版本。如果a值为0，则表示不设置
			strokeColor = Color.clear;
		}

		public GearColorValue(Color color, Color strokeColor)
		{
			this.color = color;
			this.strokeColor = strokeColor;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearColor : GearBase
	{
		public Tweener tweener { get; private set; }

		Dictionary<string, GearColorValue> _storage;
		GearColorValue _default;
		GearColorValue _tweenTarget;

		public GearColor(GObject owner)
			: base(owner)
		{
		}

		protected override void Init()
		{
			_default = new GearColorValue();
			_default.color = ((IColorGear)_owner).color;
			if (_owner is ITextColorGear)
				_default.strokeColor = ((ITextColorGear)_owner).strokeColor;
			_storage = new Dictionary<string, GearColorValue>();
		}

		override protected void AddStatus(string pageId, string value)
		{
			if (value == "-")
				return;

			Color col1;
			Color col2;
			int pos = value.IndexOf(",");
			if (pos == -1)
			{
				col1 = ToolSet.ConvertFromHtmlColor(value);
				col2 = Color.clear;
			}
			else
			{
				col1 = ToolSet.ConvertFromHtmlColor(value.Substring(0, pos));
				col2 = ToolSet.ConvertFromHtmlColor(value.Substring(pos + 1));
			}

			if (pageId == null)
			{
				_default.color = col1;
				_default.strokeColor = col2;
			}
			else
				_storage[pageId] = new GearColorValue(col1, col2);
		}

		override public void Apply()
		{
			GearColorValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				gv = _default;

			if (tween && UIPackage._constructing == 0 && !disableAllTweenEffect)
			{
				if ((_owner is ITextColorGear) && gv.strokeColor.a > 0)
				{
					_owner._gearLocked = true;
					((ITextColorGear)_owner).strokeColor = gv.strokeColor;
					_owner._gearLocked = false;
				}

				if (tweener != null)
				{
					if (_tweenTarget.color != gv.color)
					{
						tweener.Kill(true);
						tweener = null;
					}
					else
						return;
				}

				if (((IColorGear)_owner).color != gv.color)
				{
					if (_owner.CheckGearController(0, _controller))
						_displayLockToken = _owner.AddDisplayLock();
					_tweenTarget = gv;

					tweener = DOTween.To(() => ((IColorGear)_owner).color, v =>
					{
						_owner._gearLocked = true;
						((IColorGear)_owner).color = v;
						_owner._gearLocked = false;
					}, gv.color, tweenTime)
					.SetEase(easeType)
					.SetUpdate(true)
					.OnUpdate(() =>
					{
						_owner.InvalidateBatchingState();
					})
					.OnComplete(() =>
					{
						tweener = null;
						if (_displayLockToken != 0)
						{
							_owner.ReleaseDisplayLock(_displayLockToken);
							_displayLockToken = 0;
						}
						_owner.InvalidateBatchingState();
						_owner.OnGearStop.Call(this);
					});

					if (delay > 0)
						tweener.SetDelay(delay);
				}
			}
			else
			{
				_owner._gearLocked = true;
				((IColorGear)_owner).color = gv.color;
				if ((_owner is ITextColorGear) && gv.strokeColor.a > 0)
					((ITextColorGear)_owner).strokeColor = gv.strokeColor;
				_owner._gearLocked = false;
			}
		}

		override public void UpdateState()
		{
			GearColorValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				_storage[_controller.selectedPageId] = gv = new GearColorValue();
			gv.color = ((IColorGear)_owner).color;
			if (_owner is ITextColorGear)
				gv.strokeColor = ((ITextColorGear)_owner).strokeColor;
		}
	}
}
