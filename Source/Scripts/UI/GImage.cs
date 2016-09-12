using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GImage class.
	/// </summary>
	public class GImage : GObject, IColorGear
	{
		public GearColor gearColor { get; private set; }

		Image _content;

		public GImage()
		{
			gearColor = new GearColor(this);
		}

		override protected void CreateDisplayObject()
		{
			_content = new Image();
			_content.gOwner = this;
			displayObject = _content;
		}

		/// <summary>
		/// Color of the image. 
		/// </summary>
		public Color color
		{
			get { return _content.color; }
			set
			{
				_content.color = value;
				if (gearColor.controller != null)
					gearColor.UpdateState();
			}
		}

		/// <summary>
		/// Flip type.
		/// </summary>
		/// <seealso cref="FlipType"/>
		public FlipType flip
		{
			get { return _content.flip; }
			set { _content.flip = value; }
		}

		/// <summary>
		/// Fill method.
		/// </summary>
		/// <seealso cref="FillMethod"/>
		public FillMethod fillMethod
		{
			get { return _content.fillMethod; }
			set { _content.fillMethod = value; }
		}

		/// <summary>
		/// Fill origin.
		/// </summary>
		/// <seealso cref="OriginHorizontal"/>
		/// <seealso cref="OriginVertical"/>
		/// <seealso cref="Origin90"/>
		/// <seealso cref="Origin180"/>
		/// <seealso cref="Origin360"/>
		public int fillOrigin
		{
			get { return _content.fillOrigin; }
			set { _content.fillOrigin = value; }
		}

		/// <summary>
		/// Fill clockwise if true.
		/// </summary>
		public bool fillClockwise
		{
			get { return _content.fillClockwise; }
			set { _content.fillClockwise = value; }
		}

		/// <summary>
		/// Fill amount. (0~1)
		/// </summary>
		public float fillAmount
		{
			get { return _content.fillAmount; }
			set { _content.fillAmount = value; }
		}

		/// <summary>
		/// Set texture directly. The image wont own the texture.
		/// </summary>
		public NTexture texture
		{
			get { return _content.texture; }
			set
			{
				if (value != null)
				{
					sourceWidth = value.width;
					sourceHeight = value.height;
				}
				else
				{
					sourceWidth = 0;
					sourceHeight = 0;
				}
				initWidth = sourceWidth;
				initHeight = sourceHeight;
				_content.texture = value;
			}
		}

		/// <summary>
		/// Set material.
		/// </summary>
		public Material material
		{
			get { return _content.material; }
			set { _content.material = value; }
		}

		/// <summary>
		/// Set shader.
		/// </summary>
		public string shader
		{
			get { return _content.shader; }
			set { _content.shader = value; }
		}

		override public void ConstructFromResource(PackageItem pkgItem)
		{
			_packageItem = pkgItem;
			sourceWidth = _packageItem.width;
			sourceHeight = _packageItem.height;
			initWidth = sourceWidth;
			initHeight = sourceHeight;
			_content.scale9Grid = _packageItem.scale9Grid;
			_content.scaleByTile = _packageItem.scaleByTile;

			_packageItem.Load();

			_content.texture = _packageItem.texture;

			SetSize(sourceWidth, sourceHeight);
		}

		override public void HandleControllerChanged(Controller c)
		{
			base.HandleControllerChanged(c);

			if (gearColor.controller == c)
				gearColor.Apply();
		}

		override public void Setup_BeforeAdd(XML xml)
		{
			base.Setup_BeforeAdd(xml);

			string str;
			str = xml.GetAttribute("color");
			if (str != null)
				this.color = ToolSet.ConvertFromHtmlColor(str);

			str = xml.GetAttribute("flip");
			if (str != null)
				_content.flip = FieldTypes.ParseFlipType(str);

			str = xml.GetAttribute("fillMethod");
			if (str != null)
				_content.fillMethod = FieldTypes.ParseFillMethod(str);

			if (_content.fillMethod != FillMethod.None)
			{
				_content.fillOrigin = xml.GetAttributeInt("fillOrigin");
				_content.fillClockwise = xml.GetAttributeBool("fillClockwise", true);
				_content.fillAmount = (float)xml.GetAttributeInt("fillAmount", 100) / 100;
			}
		}

		override public void Setup_AfterAdd(XML xml)
		{
			base.Setup_AfterAdd(xml);

			XML cxml = xml.GetNode("gearColor");
			if (cxml != null)
				gearColor.Setup(cxml);
		}
	}
}
