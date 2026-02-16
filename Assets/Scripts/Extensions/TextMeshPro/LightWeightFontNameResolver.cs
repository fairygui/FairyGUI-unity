#if FAIRYGUI_TMPRO
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace FairyGUI
{
    public class LightWeightFontNameResolver : IFontNameResolver
    {
        public void GetFontNames(string filePath, List<FontName> results)
        {
            LightWeightFontFaceImpl.Default.GetFontFamilyNames(filePath, results);
        }
    }
}

#endif
