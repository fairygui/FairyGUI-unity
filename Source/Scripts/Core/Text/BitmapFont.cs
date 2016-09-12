using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	public class BitmapFont : BaseFont
	{
		public class BMGlyph
		{
			public int offsetX;
			public int offsetY;
			public int width;
			public int height;
			public int advance;
			public int lineHeight;
			public Rect uvRect;
			public int channel;//0-n/a, 1-r,2-g,3-b,4-alpha
		}

		public int size;
		public bool resizable;
		Dictionary<int, BMGlyph> _dict;
		float scale;

		static GlyphInfo glyhInfo = new GlyphInfo();

		public BitmapFont(PackageItem item)
		{
			this.packageItem = item;
			this.name = UIPackage.URL_PREFIX + packageItem.owner.id + packageItem.id;
			this.canTint = true;
			this.canLight = false;
			this.canOutline = true;
			this.hasChannel = false;
			this.shader = ShaderConfig.bmFontShader;

			_dict = new Dictionary<int, BMGlyph>();
			this.scale = 1;
		}

		public void AddChar(char ch, BMGlyph glyph)
		{
			_dict[ch] = glyph;
		}

		override public void SetFormat(TextFormat format)
		{
			if (resizable)
				this.scale = (float)format.size / size;
		}

		override public bool GetGlyphSize(char ch, out int width, out int height)
		{
			BMGlyph bg;
			if (ch == ' ')
			{
				width = Mathf.CeilToInt(size * scale / 2);
				height = Mathf.CeilToInt(size * scale);
				return true;
			}
			else if (_dict.TryGetValue((int)ch, out bg))
			{
				width = Mathf.CeilToInt(bg.advance * scale);
				height = Mathf.CeilToInt(bg.lineHeight * scale);
				return true;
			}
			else
			{
				width = 0;
				height = 0;
				return false;
			}
		}

		override public GlyphInfo GetGlyph(char ch)
		{
			BMGlyph bg;
			if (ch == ' ')
			{
				glyhInfo.width = Mathf.CeilToInt(size * scale / 2);
				glyhInfo.height = Mathf.CeilToInt(size * scale);
				glyhInfo.vert.xMin = 0;
				glyhInfo.vert.xMax = glyhInfo.width * scale;
				glyhInfo.vert.yMin = -glyhInfo.height * scale;
				glyhInfo.vert.yMax = 0;
				glyhInfo.uvTopLeft = Vector2.zero;
				glyhInfo.uvBottomLeft = Vector2.zero;
				glyhInfo.uvTopRight = Vector2.zero;
				glyhInfo.uvBottomRight = Vector2.zero;
				glyhInfo.channel = 0;
				return glyhInfo;
			}
			else if (_dict.TryGetValue((int)ch, out bg))
			{
				glyhInfo.width = Mathf.CeilToInt(bg.advance * scale);
				glyhInfo.height = Mathf.CeilToInt(bg.lineHeight * scale);
				glyhInfo.vert.xMin = bg.offsetX * scale;
				glyhInfo.vert.xMax = (bg.offsetX + bg.width) * scale;
				glyhInfo.vert.yMin = -bg.height * scale;
				glyhInfo.vert.yMax = 0;
				glyhInfo.uvBottomLeft = new Vector2(bg.uvRect.xMin, bg.uvRect.yMin);
				glyhInfo.uvTopLeft = new Vector2(bg.uvRect.xMin, bg.uvRect.yMax);
				glyhInfo.uvTopRight = new Vector2(bg.uvRect.xMax, bg.uvRect.yMax);
				glyhInfo.uvBottomRight = new Vector2(bg.uvRect.xMax, bg.uvRect.yMin);
				glyhInfo.channel = bg.channel;
				return glyhInfo;
			}
			else
				return null;
		}
	}
}
