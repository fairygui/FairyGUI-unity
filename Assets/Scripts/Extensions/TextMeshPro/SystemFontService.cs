#if FAIRYGUI_TMPRO

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace FairyGUI
{
    public struct FontName
    {
        public string familyName; // NameID 1
        public string subfamilyName; // NameID 2
        public string exactName; // similar to FullName
        public string filePath;

        public static FontName Create(string familyName, string subfamilyName, string filePath)
        {
            return new()
            {
                familyName = familyName,
                subfamilyName = subfamilyName,
                exactName = familyName + " " + subfamilyName,
                filePath = filePath,
            };
        }
    }

    public interface IFontNameResolver
    {
        void GetFontNames(string filePath, List<FontName> results);
    }

    public static class SystemFontService
    {
        private static Dictionary<string/*filePath*/, List<FontName>> s_lutFontNames = new();
        private static Dictionary<string/*filePath*/, TMP_FontAsset> s_lutFontAssets = new();
        public static IFontNameResolver fontNameResolver = new LightWeightFontNameResolver();

        public static IReadOnlyList<FontName> GetFontNames(string filePath)
        {
            return GetFontNamesInternal(filePath);
        }

        private static List<FontName> GetFontNamesInternal(string filePath)
        {
            if (!s_lutFontNames.TryGetValue(filePath, out var cachedResults))
            {
                s_lutFontNames[filePath] = cachedResults = new();
                fontNameResolver.GetFontNames(filePath, cachedResults);
            }

            return cachedResults;
        }

        public static List<string> ResolveInstalledFonts(string[] fontFamily, List<string> resultPaths = null)
        {
            resultPaths ??= new();
            var systemFontPaths = Font.GetPathsToOSFonts();

            foreach (var queryingName in fontFamily)
            {
                foreach (var fontPath in Font.GetPathsToOSFonts())
                {
                    var names = GetFontNamesInternal(fontPath);
                    if (IsFontMatch(queryingName, names))
                    {
                        resultPaths.Add(fontPath);
                    }
                }
            }

            return resultPaths;
        }

        private static bool IsFontMatch(string inputName, List<FontName> fontNames)
        {
            foreach (var fontName in fontNames)
            {
                if (string.Equals(fontName.exactName, inputName, System.StringComparison.InvariantCultureIgnoreCase)) return true;
                if (string.Equals(fontName.familyName, inputName, System.StringComparison.InvariantCultureIgnoreCase)) return true;
            }

            return false;
        }

        public static TMP_FontAsset GetSystemFontAsset(string fontPath, TMP_FontAsset originFontAsset = null)
        {
            if (!s_lutFontAssets.TryGetValue(fontPath, out var fontAsset))
            {
                var atlasWidth = originFontAsset?.atlasWidth ?? 2048;
                var atlasHeight = originFontAsset?.atlasHeight ?? 2048;

                var nativeFont = new Font(fontPath);
                s_lutFontAssets[fontPath]
                    = fontAsset
                    = TMP_FontAsset.CreateFontAsset(nativeFont, 60, 9, GlyphRenderMode.SDFAA, atlasWidth, atlasHeight);
            }

            return fontAsset;
        }

        public static void Clear()
        {
            s_lutFontNames.Clear();
            foreach (var (path, fontAsset) in s_lutFontAssets)
            {
                Object.Destroy(fontAsset.sourceFontFile);
                Object.Destroy(fontAsset);
            }

            s_lutFontAssets.Clear();
        }
    }
}

#endif
