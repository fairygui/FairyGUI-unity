using System.Collections.Generic;

namespace FairyGUI
{
	public class FontManager
	{
		static Dictionary<string, BaseFont> sFontFactory = new Dictionary<string, BaseFont>();

		static public void RegisterFont(BaseFont font, string alias)
		{
			if (!sFontFactory.ContainsKey(font.name))
				sFontFactory.Add(font.name, font);
			if (alias != null)
			{
				if (!sFontFactory.ContainsKey(alias))
					sFontFactory.Add(alias, font);
			}
		}

		static public void UnregisterFont(BaseFont font)
		{
			sFontFactory.Remove(font.name);
		}

		static public BaseFont GetFont(string name)
		{
			BaseFont ret;
			if (!sFontFactory.TryGetValue(name, out ret))
			{
				ret = new DynamicFont(name);
				sFontFactory.Add(name, ret);
			}

			if (ret.packageItem!=null && !ret.packageItem.decoded)
				ret.packageItem.Load();

			return ret;
		}

		static public void Clear()
		{
			sFontFactory.Clear();
		}
	}
}
