#if FAIRYGUI_TMPRO
namespace FairyGUI
{
    // see: https://learn.microsoft.com/en-us/windows/win32/intl/code-page-identifiers
    public enum EncodingCodePage : int
    {
        Unicode = 1200,
        UTF8 = 65001,
        UTF16LE = 1200,
        UTF16BE = 1201,
        GB2312 = 936,
        KS_C_5601_1987 = 949,
        Big5 = 950,
        Macintosh = 10000,
    }
}

#endif
