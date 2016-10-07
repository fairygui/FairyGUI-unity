using System;
using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearDisplay : GearBase
	{
		/// <summary>
		/// Pages involed in this gear.
		/// </summary>
		public string[] pages { get; set; }

		public GearDisplay(GObject owner)
			: base(owner)
		{
		}

		override protected void AddStatus(string pageId, string value)
		{
		}

		override protected void Init()
		{
			pages = null;
		}

		override public void Apply()
		{
			if (_controller == null || pages == null || pages.Length == 0 || Array.IndexOf(pages, _controller.selectedPageId) != -1)
				_owner.internalVisible++;
			else
				_owner.internalVisible = 0;
		}

		override public void UpdateState()
		{
		}
	}
}
