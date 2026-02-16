#if FAIRYGUI_TMPRO
using System;
using System.Collections.Generic;
#if !UNITY_WEBGL
using System.IO;
#endif
using System.Linq;
using System.Text;
using UnityEngine;

//see: https://learn.microsoft.com/en-us/typography/opentype/spec/otff

namespace FairyGUI
{
    public class LightWeightFontFaceImpl
    {
        const string TTC_TAG = "ttcf";
        public static readonly LightWeightFontFaceImpl Default = new();
        private static HashSet<EncodingCodePage> supportedEncodingCodePages = new();

        static LightWeightFontFaceImpl()
        {
            foreach (var encoding in Encoding.GetEncodings())
            {
                supportedEncodingCodePages.Add((EncodingCodePage)encoding.CodePage);
            }
        }

        static Encoding GetEncoding(EncodingCodePage codePage)
        {
            if (supportedEncodingCodePages.Contains(codePage))
            {
                try
                {
                    return Encoding.GetEncoding((int)codePage);
                }
                catch
                {
                    supportedEncodingCodePages.Remove(codePage);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resultNames">Pass a optional result container to avoid memory alloc</param>
        /// <returns><paramref name="resultNames"/> if provided, othewise, a new collection</returns>
        public List<FontName> GetFontFamilyNames(string path, List<FontName> resultNames = null)
        {
            resultNames ??= new();
#if !UNITY_WEBGL

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new BEReader(stream);
            var isTTC = reader.ReadString(4) == TTC_TAG;
            reader.Position = 0;

            if (isTTC)
            {
                var context = new TTCContext();
                // Debug.LogWarning("ttc " + path);
                LoadTTC(reader, context);

                foreach (var (offset, fontContext) in context.fonts)
                {
                    resultNames.Add(FontName.Create(
                        fontContext.ExtractString(NameID.FamilyName, reader),
                        fontContext.ExtractString(NameID.SubfamilyName, reader),
                        path));
                }
            }
            else
            {
                var fontContext = new TTFContext();
                // Debug.LogWarning("ttf " + path);
                LoadTTF(reader, fontContext);

                resultNames.Add(FontName.Create(
                    fontContext.ExtractString(NameID.FamilyName, reader),
                    fontContext.ExtractString(NameID.SubfamilyName, reader),
                    path));
            }
#endif // end of Non-WebGL plaform

            return resultNames;
        }

        private static void LoadTTC(BEReader reader, TTCContext context)
        {
            reader.Position = TTC_TAG.Length;
            var majorVersion = reader.ReadUInt16();
            var minorVersion = reader.ReadUInt16();
            var numFonts = reader.ReadUInt32();
            for (var i = 0; i < numFonts; i++)
            {
                var tableDirectoryOffset = reader.ReadUInt32();
                context.fonts[tableDirectoryOffset] = new TTFContext();
            }

            if (majorVersion > 1)
            {
                var dsigTag = reader.ReadUInt32();
                var dsigLength = reader.ReadUInt32();
                var dsigOffset = reader.ReadUInt32();
            }

            foreach (var (offset, fontContext) in context.fonts)
            {
                reader.Position = offset;
                LoadTTF(reader, fontContext);
            }
        }

        private static void LoadTTF(BEReader reader, TTFContext context)
        {
            var sfntVersion = reader.ReadUInt32();
            var numTables = reader.ReadUInt16();
            var searchRange = reader.ReadUInt16();
            var entrySelector = reader.ReadUInt16();
            var rangeShift = reader.ReadUInt16();

            for (var i = 0; i < numTables; i++)
            {
                var tag = reader.ReadString(4);
                var checkSum = reader.ReadUInt32();
                var offset = reader.ReadUInt32();
                var length = reader.ReadUInt32();

                if (tag == "name")
                {
                    reader.Position = offset;
                    var version = reader.ReadUInt16();
                    var count = reader.ReadUInt16();
                    var storageOffset = reader.ReadUInt16() + offset;
                    for (int j = 0; j < count; j++)
                    {
                        var platformID = (PlatformID)reader.ReadUInt16();
                        var encodingID = reader.ReadUInt16();
                        var languageID = (LanguageID)reader.ReadUInt16();
                        var nameID = (NameID)reader.ReadUInt16();
                        var stringLength = reader.ReadUInt16();
                        var stringOffset = reader.ReadUInt16();
                        if (languageID is LanguageID.English || !context.records.ContainsKey(nameID))
                        {
                            EncodingCodePage codePage = platformID switch
                            {
                                PlatformID.Unicode => EncodingCodePage.UTF16BE,
                                PlatformID.Macintosh => EncodingCodePage.Macintosh,
                                PlatformID.Windows => encodingID switch
                                {
                                    3 => EncodingCodePage.GB2312,
                                    4 => EncodingCodePage.KS_C_5601_1987,
                                    5 => EncodingCodePage.Big5,
                                    _ => EncodingCodePage.UTF16BE,
                                },
                                _ => EncodingCodePage.UTF8,
                            };

                            var encoding = GetEncoding(codePage);

                            if (encoding != null)
                            {
                                context.records[nameID] = new()
                                {
                                    offset = storageOffset + stringOffset,
                                    length = stringLength,
                                    encoding = encoding,
                                };
                            }
                        }
                    }

                    break;
                }
            }
        }

        private struct StringRecord
        {
            public long offset;
            public ushort length;
            public Encoding encoding;
        }

        private class TTFContext
        {
            public Dictionary<NameID, StringRecord> records = new();
            public string ExtractString(NameID nameID, BEReader reader)
            {
                if (records.TryGetValue(nameID, out var record))
                {
                    reader.Position = record.offset;
                    return reader.ReadString(record.length, record.encoding);
                }

                return null;
            }
        }

        private class TTCContext
        {
            public Dictionary<long, TTFContext> fonts = new();
        }

    }
}

#endif
