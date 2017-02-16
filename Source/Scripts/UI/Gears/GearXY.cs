using System.Collections.Generic;
using DG.Tweening;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	class GearXYValue
	{
		public float x;
		public float y;

		public GearXYValue(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearXY : GearBase
	{
		public Tweener tweener { get; private set; }

		Dictionary<string, GearXYValue> _storage;
		GearXYValue _default;
		GearXYValue _tweenTarget;

		public GearXY(GObject owner)
			: base(owner)
		{
		}

		protected override void Init()
		{
			_default = new GearXYValue(_owner.x, _owner.y);
			_storage = new Dictionary<string, GearXYValue>();
		}

		override protected void AddStatus(string pageId, string value)
		{
			if (value == "-") //历史遗留处理
				return;

			string[] arr = value.Split(',');
			if (pageId == null)
			{
				_default.x = int.Parse(arr[0]);
				_default.y = int.Parse(arr[1]);
			}
			else
				_storage[pageId] = new GearXYValue(int.Parse(arr[0]), int.Parse(arr[1]));
		}

		override public void Apply()
		{
			GearXYValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				gv = _default;

			if (tween && UIPackage._constructing == 0 && !disableAllTweenEffect)
			{
				if (tweener != null)
				{
					if (_tweenTarget.x != gv.x || _tweenTarget.y != gv.y)
					{
						tweener.Kill(true);
						tweener = null;
					}
					else
						return;
				}

				if (_owner.x != gv.x || _owner.y != gv.y)
				{
					if (_owner.CheckGearController(0, _controller))
						_displayLockToken = _owner.AddDisplayLock();
					_tweenTarget = gv;

					tweener = DOTween.To(() => new Vector2(_owner.x, _owner.y), v =>
					{
						_owner._gearLocked = true;
						_owner.SetXY(v.x, v.y);
						_owner._gearLocked = false;
					}, new Vector2(gv.x, gv.y), tweenTime)
					.SetEase(easeType)
					.SetUpdate(true)
					.OnUpdate(()=>
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
				_owner.SetXY(gv.x, gv.y);
				_owner._gearLocked = false;
			}
		}

		override public void UpdateState()
		{
			GearXYValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				_storage[_controller.selectedPageId] = new GearXYValue(_owner.x, _owner.y);
			else
			{
				gv.x = _owner.x;
				gv.y = _owner.y;
			}
		}

		override public void UpdateFromRelations(float dx, float dy)
		{
			if (_controller != null && _storage != null)
			{
				foreach (GearXYValue gv in _storage.Values)
				{
					gv.x += dx;
					gv.y += dy;
				}
				_default.x += dx;
				_default.y += dy;

				UpdateState();
			}
		}
	}
}
