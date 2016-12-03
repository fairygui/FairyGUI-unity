using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GLoader class
	/// </summary>
	public class GLoader : GObject, IAnimationGear, IColorGear
	{
		/// <summary>
		/// Display an error sign if the loader fails to load the content.
		/// </summary>
		public bool showErrorSign;

		string _url;
		AlignType _align;
		VertAlignType _verticalAlign;
		bool _autoSize;
		FillType _fill;
		bool _updatingLayout;
		PackageItem _contentItem;
		float _contentWidth;
		float _contentHeight;
		float _contentSourceWidth;
		float _contentSourceHeight;

		MovieClip _content;
		GObject _errorSign;

		static GObjectPool errorSignPool;

		public GLoader()
		{
			_url = string.Empty;
			_align = AlignType.Left;
			_verticalAlign = VertAlignType.Top;
			showErrorSign = true;
		}

		override protected void CreateDisplayObject()
		{
			displayObject = new Container("GLoader");
			displayObject.gOwner = this;
			_content = new MovieClip();
			((Container)displayObject).AddChild(_content);
			((Container)displayObject).opaque = true;
		}

		override public void Dispose()
		{
			if (_content.texture != null)
			{
				if (_contentItem == null)
					FreeExternal(image.texture);
			}
			if (_errorSign != null)
				_errorSign.Dispose();
			_content.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// 
		/// </summary>
		public string url
		{
			get { return _url; }
			set
			{
				if (_url == value)
					return;

				_url = value;
				LoadContent();
				UpdateGear(7);
			}
		}

		override public string icon
		{
			get { return _url; }
			set { this.url = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public AlignType align
		{
			get { return _align; }
			set
			{
				if (_align != value)
				{
					_align = value;
					UpdateLayout();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public VertAlignType verticalAlign
		{
			get { return _verticalAlign; }
			set
			{
				if (_verticalAlign != value)
				{
					_verticalAlign = value;
					UpdateLayout();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FillType fill
		{
			get { return _fill; }
			set
			{
				if (_fill != value)
				{
					_fill = value;
					UpdateLayout();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool autoSize
		{
			get { return _autoSize; }
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					UpdateLayout();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool playing
		{
			get { return _content.playing; }
			set
			{
				_content.playing = value;
				UpdateGear(5);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int frame
		{
			get { return _content.currentFrame; }
			set
			{
				_content.currentFrame = value;
				UpdateGear(5);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Material material
		{
			get { return _content.material; }
			set { _content.material = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string shader
		{
			get { return _content.shader; }
			set { _content.shader = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _content.color; }
			set
			{
				_content.color = value;
				UpdateGear(4);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FillMethod fillMethod
		{
			get { return _content.fillMethod; }
			set { _content.fillMethod = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int fillOrigin
		{
			get { return _content.fillOrigin; }
			set { _content.fillOrigin = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool fillClockwise
		{
			get { return _content.fillClockwise; }
			set { _content.fillClockwise = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float fillAmount
		{
			get { return _content.fillAmount; }
			set { _content.fillAmount = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Image image
		{
			get { return _content; }
		}

		/// <summary>
		/// 
		/// </summary>
		public MovieClip movieClip
		{
			get { return _content; }
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture texture
		{
			get
			{
				return _content.texture;
			}

			set
			{
				this.url = null;

				_content.texture = value;
				if (value != null)
				{
					_contentSourceWidth = value.width;
					_contentSourceHeight = value.height;
				}
				else
				{
					_contentSourceWidth = _contentHeight = 0;
				}

				UpdateLayout();
			}
		}

		override public IFilter filter
		{
			get { return _content.filter; }
			set { _content.filter = value; }
		}

		override public BlendMode blendMode
		{
			get { return _content.blendMode; }
			set { _content.blendMode = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		protected void LoadContent()
		{
			ClearContent();

			if (string.IsNullOrEmpty(_url))
				return;

			if (_url.StartsWith(UIPackage.URL_PREFIX))
				LoadFromPackage(_url);
			else
				LoadExternal();
		}

		protected void LoadFromPackage(string itemURL)
		{
			_contentItem = UIPackage.GetItemByURL(itemURL);

			if (_contentItem != null)
			{
				_contentItem.Load();
				if (_contentItem.type == PackageItemType.Image)
				{
					_content.texture = _contentItem.texture;
					_content.scale9Grid = _contentItem.scale9Grid;
					_content.scaleByTile = _contentItem.scaleByTile;
					_content.tileGridIndice = _contentItem.tileGridIndice;

					_contentSourceWidth = _contentItem.width;
					_contentSourceHeight = _contentItem.height;
					UpdateLayout();
				}
				else if (_contentItem.type == PackageItemType.MovieClip)
				{
					_contentSourceWidth = _contentItem.width;
					_contentSourceHeight = _contentItem.height;

					_content.interval = _contentItem.interval;
					_content.swing = _contentItem.swing;
					_content.repeatDelay = _contentItem.repeatDelay;
					_content.SetData(_contentItem.texture, _contentItem.frames, new Rect(0, 0, _contentSourceWidth, _contentSourceHeight));

					UpdateLayout();
				}
				else
					SetErrorState();
			}
			else
				SetErrorState();
		}

		virtual protected void LoadExternal()
		{
			Texture2D tex = (Texture2D)Resources.Load(this.url, typeof(Texture2D));
			if (tex != null)
				onExternalLoadSuccess(new NTexture(tex));
			else
				onExternalLoadFailed();
		}

		virtual protected void FreeExternal(NTexture texture)
		{
		}

		protected void onExternalLoadSuccess(NTexture texture)
		{
			_content.texture = texture;
			_contentSourceWidth = texture.width;
			_contentSourceHeight = texture.height;
			_content.scale9Grid = null;
			_content.scaleByTile = false;
			UpdateLayout();
		}

		protected void onExternalLoadFailed()
		{
			SetErrorState();
		}

		private void SetErrorState()
		{
			if (!showErrorSign || !Application.isPlaying)
				return;

			if (_errorSign == null)
			{
				if (UIConfig.loaderErrorSign != null)
				{
					if (errorSignPool == null)
						errorSignPool = new GObjectPool(Stage.inst.CreatePoolManager("LoaderErrorSignPool"));

					_errorSign = errorSignPool.GetObject(UIConfig.loaderErrorSign);
				}
				else
					return;
			}

			if (_errorSign != null)
			{
				_errorSign.width = this.width;
				_errorSign.height = this.height;
				_errorSign.grayed = grayed;
				((Container)displayObject).AddChild(_errorSign.displayObject);
			}
		}

		private void ClearErrorState()
		{
			if (_errorSign != null)
			{
				((Container)displayObject).RemoveChild(_errorSign.displayObject);
				errorSignPool.ReturnObject(_errorSign);
				_errorSign = null;
			}
		}

		private void UpdateLayout()
		{
			if (_content.texture == null && _content.frameCount == 0)
			{
				if (_autoSize)
				{
					_updatingLayout = true;
					this.SetSize(50, 30);
					_updatingLayout = false;
				}
				return;
			}

			_contentWidth = _contentSourceWidth;
			_contentHeight = _contentSourceHeight;

			if (_autoSize)
			{
				_updatingLayout = true;
				if (_contentWidth == 0)
					_contentWidth = 50;
				if (_contentHeight == 0)
					_contentHeight = 30;
				this.SetSize(_contentWidth, _contentHeight);
				_content.SetScale(1, 1);
				if (_content.texture != null)
					_content.SetNativeSize();
				_updatingLayout = false;
			}
			else
			{
				float sx = 1, sy = 1;
				if (_fill != FillType.None)
				{
					sx = this.width / _contentSourceWidth;
					sy = this.height / _contentSourceHeight;

					if (sx != 1 || sy != 1)
					{
						if (_fill == FillType.ScaleMatchHeight)
							sx = sy;
						else if (_fill == FillType.ScaleMatchWidth)
							sy = sx;
						else if (_fill == FillType.Scale)
						{
							if (sx > sy)
								sx = sy;
							else
								sy = sx;
						}
						_contentWidth = Mathf.FloorToInt(_contentSourceWidth * sx);
						_contentHeight = Mathf.FloorToInt(_contentSourceHeight * sy);
					}
				}

				if (_content.texture != null)
					_content.size = new Vector2(_contentWidth, _contentHeight);
				else
					_content.SetScale(sx, sy);

				float nx;
				float ny;
				if (_align == AlignType.Center)
					nx = Mathf.FloorToInt((this.width - _contentWidth) / 2);
				else if (_align == AlignType.Right)
					nx = Mathf.FloorToInt(this.width - _contentWidth);
				else
					nx = 0;
				if (_verticalAlign == VertAlignType.Middle)
					ny = Mathf.FloorToInt((this.height - _contentHeight) / 2);
				else if (_verticalAlign == VertAlignType.Bottom)
					ny = Mathf.FloorToInt(this.height - _contentHeight);
				else
					ny = 0;
				_content.SetXY(nx, ny);
			}
		}

		private void ClearContent()
		{
			ClearErrorState();

			if (_content.texture != null)
			{
				if (_contentItem == null)
					FreeExternal(image.texture);
				_content.texture = null;
			}

			_content.Clear();
			_contentItem = null;
		}

		override protected void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if (!_updatingLayout)
				UpdateLayout();
		}

		override public void Setup_BeforeAdd(XML xml)
		{
			base.Setup_BeforeAdd(xml);

			string str;
			str = xml.GetAttribute("url");
			if (str != null)
				_url = str;

			str = xml.GetAttribute("align");
			if (str != null)
				_align = FieldTypes.ParseAlign(str);

			str = xml.GetAttribute("vAlign");
			if (str != null)
				_verticalAlign = FieldTypes.ParseVerticalAlign(str);

			str = xml.GetAttribute("fill");
			if (str != null)
				_fill = FieldTypes.ParseFillType(str);

			_autoSize = xml.GetAttributeBool("autoSize", false);

			str = xml.GetAttribute("errorSign");
			if (str != null)
				showErrorSign = str == "true";

			str = xml.GetAttribute("frame");
			if (str != null)
				_content.currentFrame = int.Parse(str);
			_content.playing = xml.GetAttributeBool("playing", true);

			str = xml.GetAttribute("color");
			if (str != null)
				_content.color = ToolSet.ConvertFromHtmlColor(str);

			str = xml.GetAttribute("fillMethod");
			if (str != null)
				_content.fillMethod = FieldTypes.ParseFillMethod(str);

			if (_content.fillMethod != FillMethod.None)
			{
				_content.fillOrigin = xml.GetAttributeInt("fillOrigin");
				_content.fillClockwise = xml.GetAttributeBool("fillClockwise", true);
				_content.fillAmount = (float)xml.GetAttributeInt("fillAmount", 100) / 100;
			}

			if (_url != null)
				LoadContent();
		}
	}
}
