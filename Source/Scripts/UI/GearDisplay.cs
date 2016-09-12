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
		public List<string> pages { get; private set; }

		public GearDisplay(GObject owner)
			: base(owner)
		{
		}

		override protected void AddStatus(string pageId, string value)
		{
		}

		override protected void Init()
		{
			if (pages != null)
				pages.Clear();
			else
				pages = new List<string>();
		}

		override public void Apply()
		{
			if (_controller == null || pages == null || pages.Count == 0 || pages.Contains(_controller.selectedPageId))
				_owner.internalVisible++;
			else
				_owner.internalVisible = 0;
		}

		override public void UpdateState()
		{
		}
	}
}
