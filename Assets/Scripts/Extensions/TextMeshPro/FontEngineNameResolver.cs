#if FAIRYGUI_TMPRO

using System.Collections.Generic;
using UnityEngine.TextCore.LowLevel;

namespace FairyGUI
{
    // FontEngine loads all infos besides names and caches permanently. This implement will significantly increase memory unsage.
    public class FontEngineNameResolver : IFontNameResolver
    {
        public void GetFontNames(string filePath, List<FontName> results)
        {
            var error = FontEngine.LoadFontFace(filePath);
            if (error is not FontEngineError.Success) return;

            var numFaces = FontEngine.GetFontFaces().Length; // a collection when it is ttc format
            for (int i = 0; i < numFaces; i++)
            {
                // Dont worry: unity caches the result as TextCore:FontFaceCache
                FontEngine.LoadFontFace(filePath, 0/*pointSize: default is 0*/, i);
                var faceInfo = FontEngine.GetFaceInfo();
                results.Add(FontName.Create(faceInfo.familyName, faceInfo.styleName, filePath));
            }
        }
    }
}

#endif
