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
		protected Dictionary<int, int> _cachedBaseline;

		float _lastBaseLine;
		int _lastFontSize;
		int _size;
		FontStyle _style;

		static CharacterInfo sTempChar;

		internal static bool textRebuildFlag;

		public DynamicFont(string name)
		{
			this.name = name;
			this.canTint = true;
			this.canOutline = true;
			this.hasChannel = false;

			if (UIConfig.renderingTextBrighterOnDesktop && !Application.isMobilePlatform)
			{
				this.shader = ShaderConfig.textBrighterShader;
				this.canLight = true;
			}
			else
				this.shader = ShaderConfig.textShader;

			//The fonts in mobile platform have no default bold effect.
			if (name.ToLower().IndexOf("bold") == -1)
				this.customBold = Application.isMobilePlatform;

			_cachedBaseline = new Dictionary<int, int>();

			LoadFont();
		}

		void LoadFont()
		{
			//Try to load name.ttf in Resources
			_font = (Font)Resources.Load(name, typeof(Font));

			//Try to load name.ttf in Resources/Fonts/
			if (_font == null)
				_font = (Font)Resources.Load("Fonts/" + name, typeof(Font));

#if UNITY_5
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

#if (UNITY_4_7 || UNITY_5)
			Font.textureRebuilt += textureRebuildCallback;
#else
			_font.textureRebuildCallback += textureRebuildCallback;
#endif

			this.mainTexture = new NTexture(_font.material.mainTexture);
		}

		override public void SetFormat(TextFormat format, float fontSizeScale)
		{
			if (fontSizeScale == 1)
				_size = format.size;
			else
				_size = Mathf.FloorToInt((float)format.size * fontSizeScale);
			if (format.bold && !customBold)
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

		override public bool GetGlyphSize(char ch, out int width, out int height)
		{
			if (_font.GetCharacterInfo(ch, out sTempChar, _size, _style))
			{
#if UNITY_5
				width = sTempChar.advance;
#else
				width = Mathf.CeilToInt(sTempChar.width);
#endif
				height = sTempChar.size;
				if (customBold)
					width++;
				return true;
			}
			else
			{
				width = 0;
				height = 0;
				return false;
			}
		}

		override public bool GetGlyph(char ch, GlyphInfo glyph)
		{
			if (_font.GetCharacterInfo(ch, out sTempChar, _size, _style))
			{
				float baseline;
				if (_lastFontSize == _size)
					baseline = _lastBaseLine;
				else
				{
					_lastFontSize = _size;
					baseline = GetBaseLine(_size);
					_lastBaseLine = baseline;
				}
#if UNITY_5
				glyph.vert.xMin = sTempChar.minX;
				glyph.vert.yMin = sTempChar.minY - baseline;
				glyph.vert.xMax = sTempChar.maxX;
				if (sTempChar.glyphWidth == 0) //zero width, space etc
					glyph.vert.xMax = glyph.vert.xMin + _size / 2;
				glyph.vert.yMax = sTempChar.maxY - baseline;
				glyph.uvTopLeft = sTempChar.uvTopLeft;
				glyph.uvBottomLeft = sTempChar.uvBottomLeft;
				glyph.uvTopRight = sTempChar.uvTopRight;
				glyph.uvBottomRight = sTempChar.uvBottomRight;

				glyph.width = sTempChar.advance;
				glyph.height = sTempChar.size;
				if (customBold)
					glyph.width++;
#else
				glyph.vert.xMin = sTempChar.vert.xMin;
				glyph.vert.yMin = sTempChar.vert.yMax - baseline;
				glyph.vert.xMax = sTempChar.vert.xMax;
				if (sTempChar.vert.width == 0) //zero width, space etc
					glyph.vert.xMax = glyph.vert.xMin + _size / 2;
				glyph.vert.yMax = sTempChar.vert.yMin - baseline;
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
				if (customBold)
					glyph.width++;
#endif
				return true;
			}
			else
				return false;
		}

#if (UNITY_5 || UNITY_4_7)
		void textureRebuildCallback(Font targetFont)
		{
			if (_font != targetFont)
				return;
			mainTexture = new NTexture(_font.material.mainTexture);

			textRebuildFlag = true;
			//Debug.Log("Font texture rebuild: " + name + "," + mainTexture.width + "," + mainTexture.height);
		}
#else
		void textureRebuildCallback()
		{
			mainTexture = new NTexture(_font.material.mainTexture);

			textRebuildFlag = true;

			//Debug.Log("Font texture rebuild: " + name + "," + mainTexture.width + "," + mainTexture.height);
		}
#endif

		int GetBaseLine(int size)
		{
			int result;
			if (!_cachedBaseline.TryGetValue(size, out result))
			{
				CharacterInfo charInfo;
				_font.RequestCharactersInTexture("f|体_j", size, FontStyle.Normal);

#if UNITY_5
				float y0 = float.MinValue;
				if (_font.GetCharacterInfo('f', out charInfo, size, FontStyle.Normal))
					y0 = Mathf.Max(y0, charInfo.maxY);
				if (_font.GetCharacterInfo('|', out charInfo, size, FontStyle.Normal))
					y0 = Mathf.Max(y0, charInfo.maxY);
				if (_font.GetCharacterInfo('体', out charInfo, size, FontStyle.Normal))
					y0 = Mathf.Max(y0, charInfo.maxY);

				//find the most bottom position
				float y1 = float.MaxValue;
				if (_font.GetCharacterInfo('_', out charInfo, size, FontStyle.Normal))
					y1 = Mathf.Min(y1, charInfo.minY);
				if (_font.GetCharacterInfo('|', out charInfo, size, FontStyle.Normal))
					y1 = Mathf.Min(y1, charInfo.minY);
				if (_font.GetCharacterInfo('j', out charInfo, size, FontStyle.Normal))
					y1 = Mathf.Min(y1, charInfo.minY);
#else
				float y0 = float.MinValue;
				if (_font.GetCharacterInfo('f', out charInfo, size, FontStyle.Normal))
					y0 = Mathf.Max(y0, charInfo.vert.yMin);
				if (_font.GetCharacterInfo('|', out charInfo, size, FontStyle.Normal))
					y0 = Mathf.Max(y0, charInfo.vert.yMin);
				if (_font.GetCharacterInfo('体', out charInfo, size, FontStyle.Normal))
					y0 = Mathf.Max(y0, charInfo.vert.yMin);

				//find the most bottom position
				float y1 = float.MaxValue;
				if (_font.GetCharacterInfo('_', out charInfo, size, FontStyle.Normal))
					y1 = Mathf.Min(y1, charInfo.vert.yMax);
				if (_font.GetCharacterInfo('|', out charInfo, size, FontStyle.Normal))
					y1 = Mathf.Min(y1, charInfo.vert.yMax);
				if (_font.GetCharacterInfo('j', out charInfo, size, FontStyle.Normal))
					y1 = Mathf.Min(y1, charInfo.vert.yMax);
#endif

				result = (int)(y0 + (y0 - y1 - size) * 0.5f);
				_cachedBaseline.Add(size, result);
			}

			return result;
		}
	}
}
