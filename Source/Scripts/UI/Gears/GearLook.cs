using System.Collections.Generic;
using DG.Tweening;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	class GearLookValue
	{
		public float alpha;
		public float rotation;
		public bool grayed;

		public GearLookValue(float alpha, float rotation, bool grayed)
		{
			this.alpha = alpha;
			this.rotation = rotation;
			this.grayed = grayed;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearLook : GearBase
	{
		public Tweener tweener { get; private set; }

		Dictionary<string, GearLookValue> _storage;
		GearLookValue _default;
		GearLookValue _tweenTarget;

		public GearLook(GObject owner)
			: base(owner)
		{
		}

		protected override void Init()
		{
			_default = new GearLookValue(_owner.alpha, _owner.rotation, _owner.grayed);
			_storage = new Dictionary<string, GearLookValue>();
		}

		override protected void AddStatus(string pageId, string value)
		{
			if (value == "-")
				return;

			string[] arr = value.Split(',');
			if (pageId == null)
			{
				_default.alpha = float.Parse(arr[0]);
				_default.rotation = float.Parse(arr[1]);
				_default.grayed = int.Parse(arr[2]) == 1;
			}
			else
				_storage[pageId] = new GearLookValue(float.Parse(arr[0]), float.Parse(arr[1]), int.Parse(arr[2]) == 1);
		}

		override public void Apply()
		{
			GearLookValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				gv = _default;

			if (tween && UIPackage._constructing == 0 && !disableAllTweenEffect)
			{
				_owner._gearLocked = true;
				_owner.grayed = gv.grayed;
				_owner._gearLocked = false;

				if (tweener != null)
				{
					if (_tweenTarget.alpha != gv.alpha || _tweenTarget.rotation != gv.rotation)
					{
						tweener.Kill(true);
						tweener = null;
					}
					else
						return;
				}

				bool a = gv.alpha != _owner.alpha;
				bool b = gv.rotation != _owner.rotation;
				if (a || b)
				{
					if (_owner.CheckGearController(0, _controller))
						_displayLockToken = _owner.AddDisplayLock();
					_tweenTarget = gv;

					tweener = DOTween.To(() => new Vector2(_owner.alpha, _owner.rotation), val =>
					{
						_owner._gearLocked = true;
						if (a)
							_owner.alpha = val.x;
						if (b)
							_owner.rotation = val.y;
						_owner._gearLocked = false;
					}, new Vector2(gv.alpha, gv.rotation), tweenTime)
					.SetEase(easeType)
					.SetUpdate(true)
					.OnUpdate(() =>
					{
						if (b)
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
						if (b)
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
				_owner.alpha = gv.alpha;
				_owner.rotation = gv.rotation;
				_owner.grayed = gv.grayed;
				_owner._gearLocked = false;
			}
		}

		override public void UpdateState()
		{
			GearLookValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				_storage[_controller.selectedPageId] = new GearLookValue(_owner.alpha, _owner.rotation, _owner.grayed);
			else
			{
				gv.alpha = _owner.alpha;
				gv.rotation = _owner.rotation;
				gv.grayed = _owner.grayed;
			}
		}
	}
}
