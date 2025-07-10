using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class FontManager
    {
        public static Dictionary<string, BaseFont> sFontFactory = new Dictionary<string, BaseFont>();

        static bool _checkTextMeshPro;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="font"></param>
        /// <param name="alias"></param>
        static public void RegisterFont(BaseFont font, string alias = null)
        {
            sFontFactory[font.name] = font;
            if (alias != null)
                sFontFactory[alias] = font;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="font"></param>
        static public void UnregisterFont(BaseFont font)
        {
            List<string> toDelete = new List<string>();
            foreach (KeyValuePair<string, BaseFont> kv in sFontFactory)
            {
                if (kv.Value == font)
                    toDelete.Add(kv.Key);
            }

            foreach (string key in toDelete)
                sFontFactory.Remove(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static public BaseFont GetFont(string name)
        {
            BaseFont font;
            if (name.StartsWith(UIPackage.URL_PREFIX))
            {
                font = UIPackage.GetItemAssetByURL(name) as BaseFont;
                if (font != null)
                    return font;
            }

            if (sFontFactory.TryGetValue(name, out font))
                return font;

            object asset = Resources.Load(name);

            if (asset == null)
                asset = Resources.Load("Fonts/" + name);

            //Try to use new API in Uinty5 to load
            if (asset == null)
            {
                if (name.IndexOf(",") != -1)
                {
                    string[] arr = name.Split(',');
                    int cnt = arr.Length;
                    for (int i = 0; i < cnt; i++)
                        arr[i] = arr[i].Trim();
                    asset = Font.CreateDynamicFontFromOSFont(arr, 16);
                }
                else
                    asset = Font.CreateDynamicFontFromOSFont(name, 16);
            }

            if (asset == null)
                return Fallback(name);

            if (asset is Font nativeFont)
            {
                font = new DynamicFont();
                font.name = name;
                sFontFactory.Add(name, font);

                AppendSystemFontsFromUIConfig(nativeFont);

                ((DynamicFont)font).nativeFont = nativeFont;
            }
#if FAIRYGUI_TMPRO
            else if (asset is TMPro.TMP_FontAsset tmpFontAsset)
            {
                font = new TMPFont();
                font.name = name;
                sFontFactory.Add(name, font);
                // if (name == UIConfig.defaultFont) // apply to all may be better
                {
                    ; ((TMPFont)font).SetFallbackSystemFontFamily(UIConfig.systemFontFamily);
                }

                ; ((TMPFont)font).fontAsset = tmpFontAsset;
            }
#endif
            else
            {
                if (asset.GetType().Name.Contains("TMP_FontAsset"))
                {
                    if (!_checkTextMeshPro)
                    {
                        _checkTextMeshPro = true;
                        Debug.LogWarning("To enable TextMeshPro support, add script define symbol: FAIRYGUI_TMPRO");
                    }
                }

                return Fallback(name);
            }

            return font;
        }

        static void AppendSystemFontsFromUIConfig(Font nativeFont)
        {
            if (UIConfig.systemFontFamily.Length == 0) return;

            nativeFont.fontNames = nativeFont.fontNames
                .Concat(UIConfig.systemFontFamily)
                .Distinct()
                .ToArray();
        }

        static BaseFont Fallback(string name)
        {
            if (name != UIConfig.defaultFont)
            {
                BaseFont ff;
                if (sFontFactory.TryGetValue(UIConfig.defaultFont, out ff))
                {
                    sFontFactory[name] = ff;
                    return ff;
                }
            }

            Font asset = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            if (asset == null)
                throw new Exception("Failed to load font '" + name + "'");

            AppendSystemFontsFromUIConfig(asset);

            BaseFont font = new DynamicFont();
            font.name = name;
            ((DynamicFont)font).nativeFont = asset;

            sFontFactory.Add(name, font);
            return font;
        }

        /// <summary>
        /// 
        /// </summary>
        static public void Clear()
        {
            foreach (KeyValuePair<string, BaseFont> kv in sFontFactory)
                kv.Value.Dispose();

            sFontFactory.Clear();

            SystemFontService.Clear();
        }

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitializeOnLoad()
        {
            Clear();
            _checkTextMeshPro = false;
        }
#endif
    }
}
