using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	class GearSizeValue
	{
		public float width;
		public float height;
		public float scaleX;
		public float scaleY;

		public GearSizeValue(float width, float height, float scaleX, float scaleY)
		{
			this.width = width;
			this.height = height;
			this.scaleX = scaleX;
			this.scaleY = scaleY;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearSize : GearBase, ITweenListener
	{
		Dictionary<string, GearSizeValue> _storage;
		GearSizeValue _default;
		GTweener _tweener;

		public GearSize(GObject owner)
			: base(owner)
		{

		}

		protected override void Init()
		{
			_default = new GearSizeValue(_owner.width, _owner.height, _owner.scaleX, _owner.scaleY);
			_storage = new Dictionary<string, GearSizeValue>();
		}

		override protected void AddStatus(string pageId, string value)
		{
			if (value == "-" || value.Length == 0)
				return;

			string[] arr = value.Split(',');
			GearSizeValue gv;
			if (pageId == null)
				gv = _default;
			else
			{
				gv = new GearSizeValue(0, 0, 1, 1);
				_storage[pageId] = gv;
			}
			gv.width = int.Parse(arr[0]);
			gv.height = int.Parse(arr[1]);
			if (arr.Length > 2)
			{
				gv.scaleX = float.Parse(arr[2]);
				gv.scaleY = float.Parse(arr[3]);
			}
		}

		override public void Apply()
		{
			GearSizeValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				gv = _default;

			if (tween && UIPackage._constructing == 0 && !disableAllTweenEffect)
			{
				if (_tweener != null)
				{
					if (_tweener.endValue.x != gv.width || _tweener.endValue.y != gv.height
						|| _tweener.endValue.z != gv.scaleX || _tweener.endValue.w != gv.scaleY)
					{
						_tweener.Kill(true);
						_tweener = null;
					}
					else
						return;
				}

				bool a = gv.width != _owner.width || gv.height != _owner.height;
				bool b = gv.scaleX != _owner.scaleX || gv.scaleY != _owner.scaleY;
				if (a || b)
				{
					if (_owner.CheckGearController(0, _controller))
						_displayLockToken = _owner.AddDisplayLock();

					_tweener = GTween.To(new Vector4(_owner.width, _owner.height, _owner.scaleX, _owner.scaleY),
						new Vector4(gv.width, gv.height, gv.scaleX, gv.scaleY), tweenTime)
						.SetDelay(delay)
						.SetEase(easeType)
						.SetUserData((a ? 1 : 0) + (b ? 2 : 0))
						.SetTarget(this)
						.SetListener(this);
				}
			}
			else
			{
				_owner._gearLocked = true;
				_owner.SetSize(gv.width, gv.height, _owner.CheckGearController(1, _controller));
				_owner.SetScale(gv.scaleX, gv.scaleY);
				_owner._gearLocked = false;
			}
		}

		public void OnTweenStart(GTweener tweener)
		{
		}

		public void OnTweenUpdate(GTweener tweener)
		{
			_owner._gearLocked = true;
			int flag = (int)tweener.userData;
			if ((flag & 1) != 0)
				_owner.SetSize(tweener.value.x, tweener.value.y, _owner.CheckGearController(1, _controller));
			if ((flag & 2) != 0)
				_owner.SetScale(tweener.value.z, tweener.value.w);
			_owner._gearLocked = false;

			_owner.InvalidateBatchingState();
		}

		public void OnTweenComplete(GTweener tweener)
		{
			_tweener = null;
			if (_displayLockToken != 0)
			{
				_owner.ReleaseDisplayLock(_displayLockToken);
				_displayLockToken = 0;
			}
			_owner.OnGearStop.Call(this);
		}

		override public void UpdateState()
		{
			GearSizeValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				_storage[_controller.selectedPageId] = new GearSizeValue(_owner.width, _owner.height, _owner.scaleX, _owner.scaleY);
			else
			{
				gv.width = _owner.width;
				gv.height = _owner.height;
				gv.scaleX = _owner.scaleX;
				gv.scaleY = _owner.scaleY;
			}
		}

		override public void UpdateFromRelations(float dx, float dy)
		{
			if (_controller != null && _storage != null)
			{
				foreach (GearSizeValue gv in _storage.Values)
				{
					gv.width += dx;
					gv.height += dy;
				}
				_default.width += dx;
				_default.height += dy;

				UpdateState();
			}
		}
	}
}
