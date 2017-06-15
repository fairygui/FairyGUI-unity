using DG.Tweening;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	abstract public class GearBase
	{
		public static bool disableAllTweenEffect = false;

		/// <summary>
		/// Use tween to apply change.
		/// </summary>
		public bool tween;

		/// <summary>
		/// Ease type.
		/// </summary>
		public Ease easeType;

		/// <summary>
		/// Tween duration in seconds.
		/// </summary>
		public float tweenTime;

		/// <summary>
		/// Tween delay in seconds.
		/// </summary>
		public float delay;

		protected GObject _owner;
		protected Controller _controller;
		protected uint _displayLockToken;

		public GearBase(GObject owner)
		{
			_owner = owner;
			easeType = Ease.OutQuad;
			tweenTime = 0.3f;
			delay = 0;
		}

		/// <summary>
		/// Controller object.
		/// </summary>
		public Controller controller
		{
			get
			{
				return _controller;
			}

			set
			{
				if (value != _controller)
				{
					_controller = value;
					if (_controller != null)
						Init();
				}
			}
		}

		public void Setup(XML xml)
		{
			string str;

			_controller = _owner.parent.GetController(xml.GetAttribute("controller"));
			if (_controller == null)
				return;

			Init();

			str = xml.GetAttribute("tween");
			if (str != null)
				tween = true;

			str = xml.GetAttribute("ease");
			if (str != null)
				easeType = FieldTypes.ParseEaseType(str);

			str = xml.GetAttribute("duration");
			if (str != null)
				tweenTime = float.Parse(str);

			str = xml.GetAttribute("delay");
			if (str != null)
				delay = float.Parse(str);

			if (this is GearDisplay)
			{
				string[] pages = xml.GetAttributeArray("pages");
				if (pages != null)
					((GearDisplay)this).pages = pages;
			}
			else
			{
				string[] pages = xml.GetAttributeArray("pages");
				string[] values = xml.GetAttributeArray("values", '|');

				if (pages != null)
				{
					int cnt1 = pages.Length;
					int cnt2 = values != null ? values.Length : 0;
					for (int i = 0; i < cnt1; i++)
					{
						if (i < cnt2)
							str = values[i];
						else
							str = string.Empty;
						AddStatus(pages[i], str);
					}
				}
				str = xml.GetAttribute("default");
				if (str != null)
					AddStatus(null, str);
			}
		}

		virtual public void UpdateFromRelations(float dx, float dy)
		{
		}

		abstract protected void AddStatus(string pageId, string value);
		abstract protected void Init();

		/// <summary>
		/// Call when controller active page changed.
		/// </summary>
		abstract public void Apply();

		/// <summary>
		/// Call when object's properties changed.
		/// </summary>
		abstract public void UpdateState();
	}
}
