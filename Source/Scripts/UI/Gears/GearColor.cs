using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	class GearColorValue
	{
		public Color color;

		public GearColorValue(Color color)
		{
			this.color = color;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearColor : GearBase
	{
		Dictionary<string, GearColorValue> _storage;
		GearColorValue _default;

		public GearColor(GObject owner)
			: base(owner)
		{
		}

		protected override void Init()
		{
			_default = new GearColorValue(((IColorGear)_owner).color);
			_storage = new Dictionary<string, GearColorValue>();
		}

		override protected void AddStatus(string pageId, string value)
		{
			if (value == "-")
				return;

			Color col = ToolSet.ConvertFromHtmlColor(value);
			if (pageId == null)
				_default.color = col;
			else
				_storage[pageId] = new GearColorValue(col);
		}

		override public void Apply()
		{
			_owner._gearLocked = true;

			GearColorValue cv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out cv))
				cv = _default;

			((IColorGear)_owner).color = cv.color;

			_owner._gearLocked = false;
		}

		override public void UpdateState()
		{
			if (_controller == null || _owner._gearLocked || _owner.underConstruct)
				return;

			GearColorValue cv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out cv))
				_storage[_controller.selectedPageId] = new GearColorValue(((IColorGear)_owner).color);
			else
				cv.color = ((IColorGear)_owner).color;
		}
	}
}
