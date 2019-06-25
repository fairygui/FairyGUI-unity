using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class DynamicFont : BaseFont
	{
		protected Font _font;
		protected class RenderInfo
		{
			public int yIndent;//越大，字显示越偏下
			public int height;
		}
		protected Dictionary<int, RenderInfo> _renderInfo;

		RenderInfo _lastRenderInfo;
		int _lastFontSize;
		int _size;
		FontStyle _style;
		bool _bold;

		static CharacterInfo sTempChar;

		internal static bool textRebuildFlag;

		public DynamicFont(string name) : this(name, null)
		{
		}

		public DynamicFont(string name, Font font)
		{
			this.name = name;
			this.canTint = true;
			this.keepCrisp = true;
			this.shader = ShaderConfig.textShader;
			_lastFontSize = -1;

			//The fonts in mobile platform have no default bold effect.
			if (name.ToLower().IndexOf("bold") == -1)
				this.customBold = Application.isMobilePlatform;

			_renderInfo = new Dictionary<int, RenderInfo>();

			if (font == null)
				LoadFont();
			else
				_font = font;

			this.nativeFont = _font;
		}

		public Font nativeFont
		{
			get { return _font; }
			set
			{
				if (_font != null)
				{
#if (UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER)
					Font.textureRebuilt -= textureRebuildCallback;
#else
					_font.textureRebuildCallback -= textureRebuildCallback;
#endif
				}
				_font = value;
#if (UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER)
				Font.textureRebuilt += textureRebuildCallback;
#else
				_font.textureRebuildCallback += textureRebuildCallback;
#endif
				if (mainTexture != null)
					mainTexture.Dispose();
				mainTexture = new NTexture(_font.material.mainTexture);
				mainTexture.destroyMethod = DestroyMethod.None;

				_renderInfo.Clear();
			}
		}

		void LoadFont()
		{
			//Try to load name.ttf in Resources
			_font = (Font)Resources.Load(name, typeof(Font));

			//Try to load name.ttf in Resources/Fonts/
			if (_font == null)
				_font = (Font)Resources.Load("Fonts/" + name, typeof(Font));

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
			//Try to use new API in Uinty5 to load
			if (_font == null)
			{
				if (name.IndexOf(",") != -1)
				{
					string[] arr = name.Split(',');
					int cnt = arr.Length;
					for (int i = 0; i < cnt; i++)
						arr[i] = arr[i].Trim();
					_font = Font.CreateDynamicFontFromOSFont(arr, 16);
				}
				else
					_font = Font.CreateDynamicFontFromOSFont(name, 16);
			}
#endif
			if (_font == null)
			{
				if (name != UIConfig.defaultFont)
				{
					DynamicFont bf = FontManager.GetFont(UIConfig.defaultFont) as DynamicFont;
					if (bf != null)
						_font = bf._font;
				}

				//Try to use Unity builtin resource
				if (_font == null)
					_font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

				if (_font == null)
					throw new Exception("Cant load font '" + name + "'");
			}
			else
			{
				_font.hideFlags = DisplayOptions.hideFlags;
				_font.material.hideFlags = DisplayOptions.hideFlags;
				_font.material.mainTexture.hideFlags = DisplayOptions.hideFlags;
			}
		}

		override public void SetFormat(TextFormat format, float fontSizeScale)
		{
			if (keepCrisp)
			{
				_size = Mathf.FloorToInt((float)format.size * fontSizeScale * UIContentScaler.scaleFactor);
			}
			else
			{
				if (fontSizeScale == 1)
					_size = format.size;
				else
					_size = Mathf.FloorToInt((float)format.size * fontSizeScale);
			}
			if (_size == 0)
				_size = 1;

			_bold = format.bold;
			if (_bold && !customBold)
			{
				if (format.italic)
				{
					if (customBoldAndItalic)
						_style = FontStyle.Italic;
					else
						_style = FontStyle.BoldAndItalic;
				}
				else
					_style = FontStyle.Bold;
			}
			else
			{
				if (format.italic)
					_style = FontStyle.Italic;
				else
					_style = FontStyle.Normal;
			}
		}

		override public void PrepareCharacters(string text)
		{
			_font.RequestCharactersInTexture(text, _size, _style);
		}

		override public bool GetGlyphSize(char ch, out float width, out float height)
		{
			if (_font.GetCharacterInfo(ch, out sTempChar, _size, _style))
			{
				RenderInfo ri;
				if (_lastFontSize == _size)
					ri = _lastRenderInfo;
				else
				{
					_lastFontSize = _size;
					ri = _lastRenderInfo = GetRenderInfo(_size);
				}
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
				width = sTempChar.advance;
#else
				width = Mathf.CeilToInt(sTempChar.width);
#endif
				height = ri.height;
				if (_bold && customBold)
					width++;

				if (keepCrisp)
				{
					width /= UIContentScaler.scaleFactor;
					height /= UIContentScaler.scaleFactor;
				}

				return true;
			}
			else
			{
				width = 0;
				height = 0;
				return false;
			}
		}

		override public bool GetGlyph(char ch, ref GlyphInfo glyph)
		{
			if (_font.GetCharacterInfo(ch, out sTempChar, _size, _style))
			{
				RenderInfo ri;
				if (_lastFontSize == _size) //避免一次查表
					ri = _lastRenderInfo;
				else
				{
					_lastFontSize = _size;
					ri = _lastRenderInfo = GetRenderInfo(_size);
				}
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
				glyph.vertMin.x = sTempChar.minX;
				glyph.vertMin.y = ri.yIndent - sTempChar.maxY;
				glyph.vertMax.x = sTempChar.maxX;
				if (sTempChar.glyphWidth == 0) //zero width, space etc
					glyph.vertMax.x = glyph.vertMin.x + _size / 2;
				glyph.vertMax.y = ri.yIndent - sTempChar.minY;

				glyph.uvBottomLeft = sTempChar.uvBottomLeft;
				glyph.uvTopLeft = sTempChar.uvTopLeft;
				glyph.uvTopRight = sTempChar.uvTopRight;
				glyph.uvBottomRight = sTempChar.uvBottomRight;

				glyph.width = sTempChar.advance;
				glyph.height = ri.height;
				if (_bold && customBold)
					glyph.width++;
#else
				glyph.vertMin.x = sTempChar.vert.xMin;
				glyph.vertMin.y = ri.yIndent - sTempChar.vert.yMin;
				glyph.vertMax.x = sTempChar.vert.xMax;
				if (sTempChar.vert.width == 0) //zero width, space etc
					glyph.vertMax.x = glyph.vertMax.x + _size / 2;
				glyph.vertMax.y = ri.yIndent - sTempChar.vert.yMax;

				if (!sTempChar.flipped)
				{
					glyph.uvBottomLeft = new Vector2(sTempChar.uv.xMin, sTempChar.uv.yMin);
					glyph.uvTopLeft = new Vector2(sTempChar.uv.xMin, sTempChar.uv.yMax);
					glyph.uvTopRight = new Vector2(sTempChar.uv.xMax, sTempChar.uv.yMax);
					glyph.uvBottomRight = new Vector2(sTempChar.uv.xMax, sTempChar.uv.yMin);
				}
				else
				{
					glyph.uvBottomLeft = new Vector2(sTempChar.uv.xMin, sTempChar.uv.yMin);
					glyph.uvTopLeft = new Vector2(sTempChar.uv.xMax, sTempChar.uv.yMin);
					glyph.uvTopRight = new Vector2(sTempChar.uv.xMax, sTempChar.uv.yMax);
					glyph.uvBottomRight = new Vector2(sTempChar.uv.xMin, sTempChar.uv.yMax);
				}

				glyph.width = Mathf.CeilToInt(sTempChar.width);
				glyph.height = sTempChar.size;
				if (_bold && customBold)
					glyph.width++;
#endif
				if (keepCrisp)
				{
					glyph.vertMin /= UIContentScaler.scaleFactor;
					glyph.vertMax /= UIContentScaler.scaleFactor;
					glyph.width /= UIContentScaler.scaleFactor;
					glyph.height /= UIContentScaler.scaleFactor;
				}

				return true;
			}
			else
				return false;
		}

		override public bool HasCharacter(char ch)
		{
			return _font.HasCharacter(ch);
		}

#if (UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_4_7)
		void textureRebuildCallback(Font targetFont)
		{
			if (_font != targetFont)
				return;

			if (mainTexture != null)
				mainTexture.Dispose();
			mainTexture = new NTexture(_font.material.mainTexture);
			mainTexture.destroyMethod = DestroyMethod.None;

			textRebuildFlag = true;

			//Debug.Log("Font texture rebuild: " + name + "," + mainTexture.width + "," + mainTexture.height);
		}
#else
		void textureRebuildCallback()
		{
			if (mainTexture != null)
				mainTexture.Dispose();
			mainTexture = new NTexture(_font.material.mainTexture);
			mainTexture.destroyMethod = DestroyMethod.None;

			textRebuildFlag = true;

			//Debug.Log("Font texture rebuild: " + name + "," + mainTexture.width + "," + mainTexture.height);
		}
#endif

		const string TEST_STRING = "fj|_我案愛爱";
		RenderInfo GetRenderInfo(int size)
		{
			RenderInfo result;
			if (!_renderInfo.TryGetValue(size, out result))
			{
				result = new RenderInfo();

				CharacterInfo charInfo;
				_font.RequestCharactersInTexture(TEST_STRING, size, FontStyle.Normal);

				float y0 = float.MinValue;
				float y1 = float.MaxValue;
				int glyphHeight = size;
				int cnt = TEST_STRING.Length;

				for (int i = 0; i < cnt; i++)
				{
					char ch = TEST_STRING[i];
					if (_font.GetCharacterInfo(ch, out charInfo, size, FontStyle.Normal))
					{
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
						y0 = Mathf.Max(y0, charInfo.maxY);
						y1 = Mathf.Min(y1, charInfo.minY);
						glyphHeight = Math.Max(glyphHeight, charInfo.glyphHeight);
#else
						y0 = Mathf.Max(y0, charInfo.vert.yMin);
						y1 = Mathf.Min(y1, charInfo.vert.yMax);
#endif
					}
				}

				int displayHeight = (int)(y0 - y1);
				result.height = Math.Max(glyphHeight, displayHeight);
				result.yIndent = (int)y0;
				if (displayHeight < glyphHeight)
					result.yIndent++;

				_renderInfo.Add(size, result);
			}

			return result;
		}
	}
}
