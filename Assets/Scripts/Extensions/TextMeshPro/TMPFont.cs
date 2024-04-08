#if FAIRYGUI_TMPRO

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using TMPro;

namespace FairyGUI
{
    /// <summary>
    /// TextMeshPro text adapter for FairyGUI. Most of codes were taken from TextMeshPro! 
    /// Note that not all of TextMeshPro features are supported.
    /// </summary>
    public class TMPFont : BaseFont
    {
        protected TMP_FontAsset _fontAsset;

        FontStyles _style;
        float _scale;
        float _padding;
        float _stylePadding;
        float _ascent;
        float _lineHeight;
        float _boldMultiplier;
        FontWeight _defaultFontWeight;
        FontWeight _fontWeight;
        TextFormat _topTextFormat;
        TextFormat _format;
        float _fontSizeScale;
        TMP_Character _char;
        NTexture[] _atlasTextures;

        float _gradientScale;
        float _ratioA;
        float _ratioB;
        float _ratioC;

        int _formatVersion;
        TMPFont _fallbackFont;
        List<TMPFont> _fallbackFonts;
        VertexBuffer[] _subMeshBuffers;
        bool _preparing;

        public TMPFont()
        {
            this.canTint = true;
            this.shader = "FairyGUI/TextMeshPro/Distance Field";
            this.keepCrisp = true;

            _defaultFontWeight = FontWeight.Medium;
            _fallbackFonts = new List<TMPFont>();
            _atlasTextures = new NTexture[0];
            _subMeshBuffers = new VertexBuffer[0];
        }

        override public void Dispose()
        {
            Release();
        }

        public TMP_FontAsset fontAsset
        {
            get { return _fontAsset; }
            set
            {
                _fontAsset = value;
                Init();
            }
        }

        public FontWeight fontWeight
        {
            get { return _defaultFontWeight; }
            set { _defaultFontWeight = value; }
        }

        void Release()
        {
            foreach (var tex in _atlasTextures)
            {
                if (tex != null)
                    tex.Dispose();
            }
        }

        void Init()
        {
            Release();

            mainTexture = WrapAtlasTexture(0);

            // _ascent = _fontAsset.faceInfo.ascentLine;
            // _lineHeight = _fontAsset.faceInfo.lineHeight;
            _ascent = _fontAsset.faceInfo.pointSize;
            _lineHeight = _fontAsset.faceInfo.pointSize * 1.25f;
            _gradientScale = fontAsset.atlasPadding + 1;
        }

        void OnCreateNewMaterial(Material mat)
        {
            mat.SetFloat(ShaderUtilities.ID_TextureWidth, mainTexture.width);
            mat.SetFloat(ShaderUtilities.ID_TextureHeight, mainTexture.height);
            mat.SetFloat(ShaderUtilities.ID_GradientScale, _gradientScale);
            mat.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
            mat.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
        }

        public override void Prepare(TextFormat format)
        {
            _preparing = true;
            _topTextFormat = _format = format;
        }

        static List<NGraphics> subInstancesCopy = new List<NGraphics>();
        public override bool BuildGraphics(NGraphics graphics)
        {
            _preparing = false;
            _format = _topTextFormat;

            if (graphics.subInstances != null)
                subInstancesCopy.AddRange(graphics.subInstances);

            UpdatePaddings();
            UpdateGraphics(graphics);

            bool changed = CreateSubGraphics(graphics, true);

            for (int i = 0; i < _fallbackFonts.Count; i++)
            {
                _fallbackFonts[i]._format = _format;
                if (_fallbackFonts[i].CreateSubGraphics(graphics, false))
                    changed = true;
            }

            if (subInstancesCopy.Count > 0)
            {
                foreach (var g in subInstancesCopy)
                {
                    if (g != null)
                    {
                        g.texture = null;
                        g.enabled = false;
                    }
                }
                subInstancesCopy.Clear();
            }

            return changed;
        }

        bool CreateSubGraphics(NGraphics graphics, bool paddingsUpdated)
        {
            bool changed = false;
            for (int i = 0; i < _atlasTextures.Length; i++)
            {
                var texture = _atlasTextures[i];
                if (texture == null || texture.lastActive == 0)
                    continue;

                texture.lastActive = 0;

                NGraphics sub = null;
                if (graphics.subInstances != null)
                {
                    int index = graphics.subInstances.FindIndex(g => g.texture == texture);
                    if (index == -1)
                        index = graphics.subInstances.FindIndex(g => g.texture == null);
                    if (index != -1)
                    {
                        sub = graphics.subInstances[index];
                        subInstancesCopy[index] = null;
                    }
                }
                if (sub == null)
                {
                    sub = graphics.CreateSubInstance("SubTextField");
                    sub.meshFactory = new TMPFont_SubMesh();
                    sub.SetShaderAndTexture(shader, texture);
                    graphics.subInstances.Add(sub);
                    changed = true;
                }
                else if (sub.texture != texture)
                {
                    sub.SetShaderAndTexture(shader, texture);
                    sub.enabled = true;
                    changed = true;
                }

                if (!paddingsUpdated)
                {
                    paddingsUpdated = true;
                    UpdatePaddings();
                }

                UpdateGraphics(sub);
                ((TMPFont_SubMesh)sub.meshFactory).font = this;
                ((TMPFont_SubMesh)sub.meshFactory).atlasIndex = i;
            }

            return changed;
        }

        void UpdateGraphics(NGraphics graphics)
        {
            graphics.userData.x = _padding;
            graphics.userData.y = _stylePadding;

            MaterialPropertyBlock block = graphics.materialPropertyBlock;

            block.SetFloat(ShaderUtilities.ID_ScaleRatio_A, _ratioA);
            block.SetFloat(ShaderUtilities.ID_ScaleRatio_B, _ratioB);
            block.SetFloat(ShaderUtilities.ID_ScaleRatio_C, _ratioC);

            block.SetFloat(ShaderUtilities.ID_FaceDilate, _format.faceDilate);

            if (_format.outline > 0)
            {
                graphics.ToggleKeyword("OUTLINE_ON", true);

                block.SetFloat(ShaderUtilities.ID_OutlineWidth, _format.outline);
                block.SetColor(ShaderUtilities.ID_OutlineColor, _format.outlineColor);
                block.SetFloat(ShaderUtilities.ID_OutlineSoftness, _format.outlineSoftness);
            }
            else
            {
                graphics.ToggleKeyword("OUTLINE_ON", false);

                block.SetFloat(ShaderUtilities.ID_OutlineWidth, 0);
                block.SetFloat(ShaderUtilities.ID_OutlineSoftness, 0);
            }

            if (_format.shadowOffset.x != 0 || _format.shadowOffset.y != 0)
            {
                graphics.ToggleKeyword("UNDERLAY_ON", true);

                block.SetColor(ShaderUtilities.ID_UnderlayColor, _format.shadowColor);
                block.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, _format.shadowOffset.x);
                block.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -_format.shadowOffset.y);
                block.SetFloat(ShaderUtilities.ID_UnderlaySoftness, _format.underlaySoftness);
            }
            else
            {
                graphics.ToggleKeyword("UNDERLAY_ON", false);

                block.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0);
                block.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, 0);
                block.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0);
            }
        }

        override public void StartDraw(NGraphics graphics)
        {
            _padding = graphics.userData.x;
            _stylePadding = graphics.userData.y;

            if (graphics.subInstances != null)
            {
                foreach (var g in graphics.subInstances)
                {
                    if (g.texture == null)
                        continue;

                    var mesh = g.meshFactory as TMPFont_SubMesh;
                    if (mesh != null)
                    {
                        mesh.font._padding = g.userData.x;
                        mesh.font._stylePadding = g.userData.y;
                        mesh.font._subMeshBuffers[mesh.atlasIndex] = mesh.source = VertexBuffer.Begin();
                    }
                }
            }
        }

        NTexture WrapAtlasTexture(int index)
        {
            if (_atlasTextures.Length != fontAsset.atlasTextures.Length)
            {
                Array.Resize(ref _atlasTextures, fontAsset.atlasTextures.Length);
                Array.Resize(ref _subMeshBuffers, _atlasTextures.Length);
            }

            var texture = _atlasTextures[index];
            if (texture == null)
            {
                texture = new NTexture(_fontAsset.atlasTextures[index]);
                texture.destroyMethod = DestroyMethod.None;
                _atlasTextures[index] = texture;

                var manager = texture.GetMaterialManager(this.shader);
                manager.onCreateNewMaterial += OnCreateNewMaterial;
            }
            else if (texture.nativeTexture != _fontAsset.atlasTextures[index])
            {
                texture.Reload(_fontAsset.atlasTextures[index], null);
            }

            return texture;
        }

        override public void SetFormat(TextFormat format, float fontSizeScale)
        {
            _format = format;
            _formatVersion++;

            _fontSizeScale = fontSizeScale;
            float size = _format.size * fontSizeScale;
            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript || _format.specialStyle == TextFormat.SpecialStyle.Superscript)
                size *= SupScale;

            _scale = size / _fontAsset.faceInfo.pointSize * _fontAsset.faceInfo.scale;
            _style = FontStyles.Normal;
            if (_format.bold)
            {
                _style |= FontStyles.Bold;
                _fontWeight = FontWeight.Bold;
                _boldMultiplier = 1 + _fontAsset.boldSpacing * 0.01f;
            }
            else
            {
                _fontWeight = _defaultFontWeight;
                _boldMultiplier = 1.0f;
            }
            if (_format.italic)
                _style |= FontStyles.Italic;

            _format.FillVertexColors(vertexColors);
        }

        override public bool GetGlyph(char ch, out float width, out float height, out float baseline)
        {
            if (!GetCharacterFromFontAsset(ch, _style, _fontWeight))
            {
                width = height = baseline = 0;
                return false;
            }

            var font = _fallbackFont ?? this;

            width = _char.glyph.metrics.horizontalAdvance * font._boldMultiplier * font._scale;
            height = font._lineHeight * font._scale;
            baseline = font._ascent * font._scale;

            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript)
            {
                height /= SupScale;
                baseline /= SupScale;
            }
            else if (_format.specialStyle == TextFormat.SpecialStyle.Superscript)
            {
                height = height / SupScale + baseline * SupOffset;
                baseline *= (SupOffset + 1 / SupScale);
            }

            height = Mathf.RoundToInt(height);
            baseline = Mathf.RoundToInt(baseline);

            return true;
        }

        bool GetCharacterFromFontAsset(uint unicode, FontStyles fontStyle, FontWeight fontWeight)
        {
            bool isAlternativeTypeface;

            // #region OLD_TMP_VERION
            // TMP_FontAsset actualAsset;
            // _char = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, _fontAsset, true, fontStyle, fontWeight,
            //     out isAlternativeTypeface, out actualAsset
            // );
            // if (_char == null)
            //     return false;
            // #endregion

            #region NEW_TMP_VERION
            _char = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, _fontAsset, true, fontStyle, fontWeight,
                out isAlternativeTypeface
            );
            if (_char == null)
                return false;

            TMP_FontAsset actualAsset = _char.textAsset as TMP_FontAsset;
            #endregion

            if (actualAsset != _fontAsset) //fallback
            {
                if (_fallbackFont == null || _fallbackFont._fontAsset != actualAsset)
                {
                    _fallbackFont = _fallbackFonts.Find(x => x._fontAsset == actualAsset);
                    if (_fallbackFont == null && _preparing)
                    {
                        _fallbackFont = new TMPFont();
                        _fallbackFont.fontAsset = actualAsset;
                        _fallbackFonts.Add(_fallbackFont);
                    }
                }
                if (_fallbackFont != null)
                {
                    if (_fallbackFont._formatVersion != _formatVersion)
                    {
                        _fallbackFont.SetFormat(_format, _fontSizeScale);
                        _fallbackFont._formatVersion = _formatVersion;
                    }

                    _fallbackFont._char = _char;
                }
            }
            else
                _fallbackFont = null;

            if (_preparing)
            {
                if (_fallbackFont != null)
                {
                    _fallbackFont.WrapAtlasTexture(_char.glyph.atlasIndex).lastActive = 1;
                }
                else if (_char.glyph.atlasIndex != 0)
                {
                    WrapAtlasTexture(_char.glyph.atlasIndex).lastActive = 1;
                }
            }

            return true;
        }

        static Vector3 bottomLeft;
        static Vector3 topLeft;
        static Vector3 topRight;
        static Vector3 bottomRight;

        static Vector4 uvBottomLeft;
        static Vector4 uvTopLeft;
        static Vector4 uvTopRight;
        static Vector4 uvBottomRight;

        static Vector4 uv2BottomLeft;
        static Vector4 uv2TopLeft;
        static Vector4 uv2TopRight;
        static Vector4 uv2BottomRight;

        static Color32[] vertexColors = new Color32[4];

        override public void DrawGlyph(VertexBuffer vb, float x, float y)
        {
            if (_fallbackFont != null)
            {
                _fallbackFont.DrawGlyph(vb, x, y);
                return;
            }

            vb = _subMeshBuffers[_char.glyph.atlasIndex] ?? vb;
            GlyphMetrics metrics = _char.glyph.metrics;
            GlyphRect rect = _char.glyph.glyphRect;

            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript)
                y = y - Mathf.RoundToInt(_ascent * _scale * SupOffset);
            else if (_format.specialStyle == TextFormat.SpecialStyle.Superscript)
                y = y + Mathf.RoundToInt(_ascent * _scale * (1 / SupScale - 1 + SupOffset));

            topLeft.x = x + (metrics.horizontalBearingX - _padding - _stylePadding) * _scale;
            topLeft.y = y + (metrics.horizontalBearingY + _padding) * _scale;
            bottomRight.x = topLeft.x + (metrics.width + _padding * 2 + _stylePadding * 2) * _scale;
            bottomRight.y = topLeft.y - (metrics.height + _padding * 2) * _scale;

            topRight.x = bottomRight.x;
            topRight.y = topLeft.y;
            bottomLeft.x = topLeft.x;
            bottomLeft.y = bottomRight.y;

            #region Handle Italic & Shearing
            if (((_style & FontStyles.Italic) == FontStyles.Italic))
            {
                // Shift Top vertices forward by half (Shear Value * height of character) and Bottom vertices back by same amount. 
                float shear_value = _fontAsset.italicStyle * 0.01f;
                Vector3 topShear = new Vector3(shear_value * ((metrics.horizontalBearingY + _padding + _stylePadding) * _scale), 0, 0);
                Vector3 bottomShear = new Vector3(shear_value * (((metrics.horizontalBearingY - metrics.height - _padding - _stylePadding)) * _scale), 0, 0);

                topLeft += topShear;
                bottomLeft += bottomShear;
                topRight += topShear;
                bottomRight += bottomShear;
            }
            #endregion Handle Italics & Shearing

            vb.vertices.Add(bottomLeft);
            vb.vertices.Add(topLeft);
            vb.vertices.Add(topRight);
            vb.vertices.Add(bottomRight);

            float u = (rect.x - _padding - _stylePadding) / _fontAsset.atlasWidth;
            float v = (rect.y - _padding - _stylePadding) / _fontAsset.atlasHeight;
            float uw = rect.width > 0 ? (rect.width + _padding * 2 + _stylePadding * 2) / _fontAsset.atlasWidth : 0;
            float vw = rect.height > 0 ? (rect.height + _padding * 2 + _stylePadding * 2) / _fontAsset.atlasHeight : 0;

            uvBottomLeft = new Vector2(u, v);
            uvTopLeft = new Vector2(u, v + vw);
            uvTopRight = new Vector2(u + uw, v + vw);
            uvBottomRight = new Vector2(u + uw, v);

            float xScale = _scale * 0.01f;
            if (_format.bold)
                xScale *= -1;
            uv2BottomLeft = new Vector2(0, xScale);
            uv2TopLeft = new Vector2(511, xScale);
            uv2TopRight = new Vector2(2093567, xScale);
            uv2BottomRight = new Vector2(2093056, xScale);

            vb.uvs.Add(uvBottomLeft);
            vb.uvs.Add(uvTopLeft);
            vb.uvs.Add(uvTopRight);
            vb.uvs.Add(uvBottomRight);

            vb.uvs2.Add(uv2BottomLeft);
            vb.uvs2.Add(uv2TopLeft);
            vb.uvs2.Add(uv2TopRight);
            vb.uvs2.Add(uv2BottomRight);

            vb.colors.Add(vertexColors[0]);
            vb.colors.Add(vertexColors[1]);
            vb.colors.Add(vertexColors[2]);
            vb.colors.Add(vertexColors[3]);
        }

        override public void DrawLine(VertexBuffer vb, float x, float y, float width, int fontSize, int type)
        {
            if (!GetCharacterFromFontAsset('_', _style, FontWeight.Regular))
                return;

            if (_fallbackFont != null)
                _fallbackFont.InternalDrawLine(vb, x, y, width, fontSize, type);
            else
                InternalDrawLine(vb, x, y, width, fontSize, type);
        }

        void InternalDrawLine(VertexBuffer vb, float x, float y, float width, int fontSize, int type)
        {
            vb = _subMeshBuffers[_char.glyph.atlasIndex] ?? vb;

            float thickness;
            float offset;

            if (type == 0)
            {
                thickness = _fontAsset.faceInfo.underlineThickness;
                offset = _fontAsset.faceInfo.underlineOffset;
            }
            else
            {
                thickness = _fontAsset.faceInfo.strikethroughThickness;
                offset = _fontAsset.faceInfo.strikethroughOffset;
            }

            GlyphRect rect = _char.glyph.glyphRect;
            float scale = (float)fontSize / _fontAsset.faceInfo.pointSize * _fontAsset.faceInfo.scale;
            float segmentWidth = _char.glyph.metrics.width / 2 * scale;
            width += _padding * 2;
            if (width < _char.glyph.metrics.width * scale)
                segmentWidth = width / 2f;

            // UNDERLINE VERTICES FOR (3) LINE SEGMENTS
            #region UNDERLINE VERTICES

            thickness = thickness * scale;
            if (thickness < 1)
                thickness = 1;
            offset = Mathf.RoundToInt(offset * scale);

            x -= _padding;
            y += offset;

            // Front Part of the Underline
            topLeft.x = x;
            topLeft.y = y + _padding * scale;
            bottomRight.x = x + segmentWidth;
            bottomRight.y = y - thickness - _padding * scale;

            topRight.x = bottomRight.x;
            topRight.y = topLeft.y;
            bottomLeft.x = topLeft.x;
            bottomLeft.y = bottomRight.y;

            vb.vertices.Add(bottomLeft);
            vb.vertices.Add(topLeft);
            vb.vertices.Add(topRight);
            vb.vertices.Add(bottomRight);

            // Middle Part of the Underline
            topLeft = topRight;
            bottomLeft = bottomRight;

            topRight.x = x + width - segmentWidth;
            bottomRight.x = topRight.x;

            vb.vertices.Add(bottomLeft);
            vb.vertices.Add(topLeft);
            vb.vertices.Add(topRight);
            vb.vertices.Add(bottomRight);

            // End Part of the Underline
            topLeft = topRight;
            bottomLeft = bottomRight;

            topRight.x = x + width;
            bottomRight.x = topRight.x;

            vb.vertices.Add(bottomLeft);
            vb.vertices.Add(topLeft);
            vb.vertices.Add(topRight);
            vb.vertices.Add(bottomRight);

            #endregion

            // UNDERLINE UV0
            #region HANDLE UV0

            // Calculate UV required to setup the 3 Quads for the Underline.
            Vector2 uv0 = new Vector2((rect.x - _padding) / _fontAsset.atlasWidth, (rect.y - _padding) / _fontAsset.atlasHeight);  // bottom left
            Vector2 uv1 = new Vector2(uv0.x, (rect.y + rect.height + _padding) / _fontAsset.atlasHeight);  // top left
            Vector2 uv2 = new Vector2((rect.x - _padding + (float)rect.width / 2) / _fontAsset.atlasWidth, uv1.y); // Mid Top Left
            Vector2 uv3 = new Vector2(uv2.x, uv0.y); // Mid Bottom Left
            Vector2 uv4 = new Vector2((rect.x + _padding + (float)rect.width / 2) / _fontAsset.atlasWidth, uv1.y); // Mid Top Right
            Vector2 uv5 = new Vector2(uv4.x, uv0.y); // Mid Bottom right
            Vector2 uv6 = new Vector2((rect.x + _padding + rect.width) / _fontAsset.atlasWidth, uv1.y); // End Part - Bottom Right
            Vector2 uv7 = new Vector2(uv6.x, uv0.y); // End Part - Top Right

            vb.uvs.Add(uv0);
            vb.uvs.Add(uv1);
            vb.uvs.Add(uv2);
            vb.uvs.Add(uv3);

            // Middle Part of the Underline
            vb.uvs.Add(new Vector2(uv2.x - uv2.x * 0.001f, uv0.y));
            vb.uvs.Add(new Vector2(uv2.x - uv2.x * 0.001f, uv1.y));
            vb.uvs.Add(new Vector2(uv2.x + uv2.x * 0.001f, uv1.y));
            vb.uvs.Add(new Vector2(uv2.x + uv2.x * 0.001f, uv0.y));

            // Right Part of the Underline

            vb.uvs.Add(uv5);
            vb.uvs.Add(uv4);
            vb.uvs.Add(uv6);
            vb.uvs.Add(uv7);

            #endregion

            // UNDERLINE UV2
            #region HANDLE UV2 - SDF SCALE
            // UV1 contains Face / Border UV layout.
            float segUv1 = segmentWidth / width;
            float segUv2 = 1 - segUv1;

            //Calculate the xScale or how much the UV's are getting stretched on the X axis for the middle section of the underline.
            float xScale = scale * 0.01f;

            vb.uvs2.Add(PackUV(0, 0, xScale));
            vb.uvs2.Add(PackUV(0, 1, xScale));
            vb.uvs2.Add(PackUV(segUv1, 1, xScale));
            vb.uvs2.Add(PackUV(segUv1, 0, xScale));

            vb.uvs2.Add(PackUV(segUv1, 0, xScale));
            vb.uvs2.Add(PackUV(segUv1, 1, xScale));
            vb.uvs2.Add(PackUV(segUv2, 1, xScale));
            vb.uvs2.Add(PackUV(segUv2, 0, xScale));

            vb.uvs2.Add(PackUV(segUv2, 0, xScale));
            vb.uvs2.Add(PackUV(segUv2, 1, xScale));
            vb.uvs2.Add(PackUV(1, 1, xScale));
            vb.uvs2.Add(PackUV(1, 0, xScale));

            #endregion

            // UNDERLINE VERTEX COLORS
            #region
            // Alpha is the lower of the vertex color or tag color alpha used.

            for (int i = 0; i < 12; i++)
                vb.colors.Add(vertexColors[0]);

            #endregion
        }

        Vector2 PackUV(float x, float y, float xScale)
        {
            double x0 = (int)(x * 511);
            double y0 = (int)(y * 511);

            return new Vector2((float)((x0 * 4096) + y0), xScale);
        }

        override public bool HasCharacter(char ch)
        {
            return _fontAsset.HasCharacter(ch, true);
        }

        override public int GetLineHeight(int size)
        {
            return Mathf.RoundToInt(_lineHeight * ((float)size / _fontAsset.faceInfo.pointSize * _fontAsset.faceInfo.scale));
        }

        void UpdatePaddings()
        {
            UpdateShaderRatios();

            _padding = GetPadding();
            _stylePadding = (((_style & FontStyles.Bold) == FontStyles.Bold) ? _fontAsset.boldStyle : _fontAsset.normalStyle)
                / 4.0f * _gradientScale * _ratioA;
            // Clamp overall padding to Gradient Scale size.
            if (_stylePadding + _padding > _gradientScale)
                _padding = _gradientScale - _stylePadding;
        }

        float GetPadding()
        {
            Vector4 padding = Vector4.zero;
            Vector4 maxPadding = Vector4.zero;

            float faceDilate = _format.faceDilate * _ratioA;
            float outlineSoftness = _format.outline > 0 ? _format.outlineSoftness : 0;
            float faceSoftness = outlineSoftness * _ratioA;
            float outlineThickness = _format.outline * _ratioA;

            float uniformPadding = outlineThickness + faceSoftness + faceDilate;

            // Underlay padding contribution
            if (_format.shadowOffset.x != 0 || _format.shadowOffset.y != 0)
            {
                float offsetX = _format.shadowOffset.x * _ratioC;
                float offsetY = -_format.shadowOffset.y * _ratioC;
                float dilate = _format.faceDilate * _ratioC;
                float softness = _format.underlaySoftness * _ratioC;

                padding.x = Mathf.Max(padding.x, faceDilate + dilate + softness - offsetX);
                padding.y = Mathf.Max(padding.y, faceDilate + dilate + softness - offsetY);
                padding.z = Mathf.Max(padding.z, faceDilate + dilate + softness + offsetX);
                padding.w = Mathf.Max(padding.w, faceDilate + dilate + softness + offsetY);
            }

            padding.x = Mathf.Max(padding.x, uniformPadding);
            padding.y = Mathf.Max(padding.y, uniformPadding);
            padding.z = Mathf.Max(padding.z, uniformPadding);
            padding.w = Mathf.Max(padding.w, uniformPadding);

            padding.x = Mathf.Min(padding.x, 1);
            padding.y = Mathf.Min(padding.y, 1);
            padding.z = Mathf.Min(padding.z, 1);
            padding.w = Mathf.Min(padding.w, 1);

            maxPadding.x = maxPadding.x < padding.x ? padding.x : maxPadding.x;
            maxPadding.y = maxPadding.y < padding.y ? padding.y : maxPadding.y;
            maxPadding.z = maxPadding.z < padding.z ? padding.z : maxPadding.z;
            maxPadding.w = maxPadding.w < padding.w ? padding.w : maxPadding.w;

            padding *= _gradientScale;

            // Set UniformPadding to the maximum value of any of its components.
            uniformPadding = Mathf.Max(padding.x, padding.y);
            uniformPadding = Mathf.Max(padding.z, uniformPadding);
            uniformPadding = Mathf.Max(padding.w, uniformPadding);

            return uniformPadding + 1.25f;
        }

        // Scale Ratios to ensure property ranges are optimum in Material Editor
        void UpdateShaderRatios()
        {
            _ratioA = _ratioB = _ratioC = 1;

            float clamp = 1;

            float weight = Mathf.Max(fontAsset.normalStyle, fontAsset.boldStyle) / 4.0f;
            float range = (weight + _format.faceDilate) * (_gradientScale - clamp);

            // Compute Ratio A
            float outlineSoftness = _format.outline > 0 ? _format.outlineSoftness : 0;
            float t = Mathf.Max(1, weight + _format.faceDilate + _format.outline + outlineSoftness);
            _ratioA = (_gradientScale - clamp) / (_gradientScale * t);

            // Compute Ratio B
            // no glow support yet

            // Compute Ratio C
            if (_format.shadowOffset.x != 0 || _format.shadowOffset.y != 0)
            {
                t = Mathf.Max(1, Mathf.Max(Mathf.Abs(_format.shadowOffset.x), Mathf.Abs(-_format.shadowOffset.y)) + _format.faceDilate + _format.underlaySoftness);
                _ratioC = Mathf.Max(0, _gradientScale - clamp - range) / (_gradientScale * t);
            }
        }
    }

    class TMPFont_SubMesh : IMeshFactory
    {
        public TMPFont font;
        public int atlasIndex;
        public VertexBuffer source;

        public void OnPopulateMesh(VertexBuffer vb)
        {
            if (source != null)
            {
                vb.Append(source);
                vb.AddTriangles();
                source = null;
            }
        }
    }
}

#endif