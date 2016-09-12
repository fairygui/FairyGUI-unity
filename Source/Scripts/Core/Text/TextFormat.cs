using UnityEngine;

namespace FairyGUI
{
	public class TextFormat
	{
		public int size;
		public string font;
		public Color color;
		public int lineSpacing;
		public int letterSpacing;
		public bool bold;
		public bool underline;
		public bool italic;

		public Color32[] gradientColor;

		public void SetColor(uint value)
		{
			uint rr = (value >> 16) & 0x0000ff;
			uint gg = (value >> 8) & 0x0000ff;
			uint bb = value & 0x0000ff;
			float r = rr / 255.0f;
			float g = gg / 255.0f;
			float b = bb / 255.0f;
			color = new Color(r, g, b, 1);
		}

		public bool EqualStyle(TextFormat aFormat)
		{
			return size == aFormat.size && color.Equals(aFormat.color)
				&& bold == aFormat.bold && underline == aFormat.underline
				&& italic == aFormat.italic
				&& gradientColor == aFormat.gradientColor;
		}

		public void CopyFrom(TextFormat source)
		{
			this.size = source.size;
			this.font = source.font;
			this.color = source.color;
			this.lineSpacing = source.lineSpacing;
			this.letterSpacing = source.letterSpacing;
			this.bold = source.bold;
			this.underline = source.underline;
			this.italic = source.italic;
			if (source.gradientColor != null)
			{
				this.gradientColor = new Color32[4];
				source.gradientColor.CopyTo(this.gradientColor, 0);
			}
			else
				this.gradientColor = null;
		}
	}
}
