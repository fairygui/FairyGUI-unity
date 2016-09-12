using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	public class DynamicFont : BaseFont
	{
		protected Font _font;
		protected Dictionary<int, int> _cachedBaseline;

		float _lastBaseLine;
		int _lastFontSize;
		int _size;
		FontStyle _style;

		static CharacterInfo sTempChar;
		static GlyphInfo glyhInfo = new GlyphInfo();

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

		override public void SetFormat(TextFormat format)
		{
			_size = format.size;
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

		override public GlyphInfo GetGlyph(char ch)
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
				glyhInfo.vert.xMin = sTempChar.minX;
				glyhInfo.vert.yMin = sTempChar.minY - baseline;
				glyhInfo.vert.xMax = sTempChar.maxX;
				if (sTempChar.glyphWidth == 0) //zero width, space etc
					glyhInfo.vert.xMax = glyhInfo.vert.xMin + _size / 2;
				glyhInfo.vert.yMax = sTempChar.maxY - baseline;
				glyhInfo.uvTopLeft = sTempChar.uvTopLeft;
				glyhInfo.uvBottomLeft = sTempChar.uvBottomLeft;
				glyhInfo.uvTopRight = sTempChar.uvTopRight;
				glyhInfo.uvBottomRight = sTempChar.uvBottomRight;

				glyhInfo.width = sTempChar.advance;
				glyhInfo.height = sTempChar.size;
				if (customBold)
					glyhInfo.width++;
#else
				glyhInfo.vert.xMin = sTempChar.vert.xMin;
				glyhInfo.vert.yMin = sTempChar.vert.yMax - baseline;
				glyhInfo.vert.xMax = sTempChar.vert.xMax;
				if (sTempChar.vert.width == 0) //zero width, space etc
					glyhInfo.vert.xMax = glyhInfo.vert.xMin + _size / 2;
				glyhInfo.vert.yMax = sTempChar.vert.yMin - baseline;
				if (!sTempChar.flipped)
				{
					glyhInfo.uvBottomLeft = new Vector2(sTempChar.uv.xMin, sTempChar.uv.yMin);
					glyhInfo.uvTopLeft = new Vector2(sTempChar.uv.xMin, sTempChar.uv.yMax);
					glyhInfo.uvTopRight = new Vector2(sTempChar.uv.xMax, sTempChar.uv.yMax);
					glyhInfo.uvBottomRight = new Vector2(sTempChar.uv.xMax, sTempChar.uv.yMin);
				}
				else
				{
					glyhInfo.uvBottomLeft = new Vector2(sTempChar.uv.xMin, sTempChar.uv.yMin);
					glyhInfo.uvTopLeft = new Vector2(sTempChar.uv.xMax, sTempChar.uv.yMin);
					glyhInfo.uvTopRight = new Vector2(sTempChar.uv.xMax, sTempChar.uv.yMax);
					glyhInfo.uvBottomRight = new Vector2(sTempChar.uv.xMin, sTempChar.uv.yMax);
				}

				glyhInfo.width = Mathf.CeilToInt(sTempChar.width);
				glyhInfo.height = sTempChar.size;
				if (customBold)
					glyhInfo.width++;
#endif
				return glyhInfo;
			}
			else
				return null;
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
