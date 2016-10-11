using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GMovieClip class.
	/// </summary>
	public class GMovieClip : GObject, IAnimationGear, IColorGear
	{
		/// <summary>
		/// 
		/// </summary>
		public EventListener onPlayEnd { get; private set; }

		MovieClip _content;

		public GMovieClip()
		{
			_sizeImplType = 1;

			onPlayEnd = new EventListener(this, "onPlayEnd");
		}

		override protected void CreateDisplayObject()
		{
			_content = new MovieClip();
			_content.gOwner = this;
			_content.playState.ignoreTimeScale = true;
			displayObject = _content;
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
		public FlipType flip
		{
			get { return _content.flip; }
			set { _content.flip = value; }
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
		/// Play from the start to end, repeat times, set to endAt on complete.
		/// 从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
		/// </summary>
		/// <param name="start">Start frame</param>
		/// <param name="end">End frame. -1 indicates the last frame.</param>
		/// <param name="times">Repeat times. 0 indicates infinite loop.</param>
		/// <param name="endAt">Stop frame. -1 indicates to equal to the end parameter.</param>
		public void SetPlaySettings(int start, int end, int times, int endAt)
		{
			((MovieClip)displayObject).SetPlaySettings(start, end, times, endAt);
		}

		override public void ConstructFromResource()
		{
			sourceWidth = packageItem.width;
			sourceHeight = packageItem.height;
			initWidth = sourceWidth;
			initHeight = sourceHeight;

			_content.interval = packageItem.interval;
			_content.swing = packageItem.swing;
			_content.repeatDelay = packageItem.repeatDelay;
			_content.SetData(packageItem.texture, packageItem.frames, new Rect(0, 0, sourceWidth, sourceHeight));

			SetSize(sourceWidth, sourceHeight);
		}

		override public void Setup_BeforeAdd(XML xml)
		{
			base.Setup_BeforeAdd(xml);

			string str;

			str = xml.GetAttribute("frame");
			if (str != null)
				_content.currentFrame = int.Parse(str);
			_content.playing = xml.GetAttributeBool("playing", true);

			str = xml.GetAttribute("color");
			if (str != null)
				_content.color = ToolSet.ConvertFromHtmlColor(str);

			str = xml.GetAttribute("flip");
			if (str != null)
				_content.flip = FieldTypes.ParseFlipType(str);
		}
	}
}
