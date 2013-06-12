namespace System.Windows.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms.VisualStyles;

    public class ProfessionalColorTable
    {
        private const string aeroFileName = "aero.msstyles";
        private object colorFreshnessKey;
        private string lastKnownColorScheme = string.Empty;
        private const string lunaFileName = "luna.msstyles";
        private const string normalColorScheme = "NormalColor";
        private const string oliveColorScheme = "HomeStead";
        private Dictionary<KnownColors, Color> professionalRGB;
        private const string royaleColorScheme = "Royale";
        private const string royaleFileName = "royale.msstyles";
        private const string silverColorScheme = "Metallic";
        private bool useSystemColors;
        private bool usingSystemColors;

        internal Color FromKnownColor(KnownColors color)
        {
            if ((ProfessionalColors.ColorFreshnessKey != this.colorFreshnessKey) || (ProfessionalColors.ColorScheme != this.lastKnownColorScheme))
            {
                this.ResetRGBTable();
            }
            this.colorFreshnessKey = ProfessionalColors.ColorFreshnessKey;
            this.lastKnownColorScheme = ProfessionalColors.ColorScheme;
            return this.ColorTable[color];
        }

        private static Color GetAlphaBlendedColor(Graphics g, Color src, Color dest, int alpha)
        {
            int red = ((src.R * alpha) + ((0xff - alpha) * dest.R)) / 0xff;
            int green = ((src.G * alpha) + ((0xff - alpha) * dest.G)) / 0xff;
            int blue = ((src.B * alpha) + ((0xff - alpha) * dest.B)) / 0xff;
            int num4 = ((src.A * alpha) + ((0xff - alpha) * dest.A)) / 0xff;
            if (g == null)
            {
                return Color.FromArgb(num4, red, green, blue);
            }
            return g.GetNearestColor(Color.FromArgb(num4, red, green, blue));
        }

        private static Color GetAlphaBlendedColorHighRes(Graphics graphics, Color src, Color dest, int alpha)
        {
            int num;
            int num2;
            int num6 = alpha;
            if (num6 < 100)
            {
                num2 = 100 - num6;
                num = 100;
            }
            else
            {
                num2 = 0x3e8 - num6;
                num = 0x3e8;
            }
            int red = (((num6 * src.R) + (num2 * dest.R)) + (num / 2)) / num;
            int green = (((num6 * src.G) + (num2 * dest.G)) + (num / 2)) / num;
            int blue = (((num6 * src.B) + (num2 * dest.B)) + (num / 2)) / num;
            if (graphics == null)
            {
                return Color.FromArgb(red, green, blue);
            }
            return graphics.GetNearestColor(Color.FromArgb(red, green, blue));
        }

        internal void InitBlueLunaColors(ref Dictionary<KnownColors, Color> rgbTable)
        {
            rgbTable[KnownColors.msocbvcrCBBdrOuterDocked] = Color.FromArgb(0xc4, 0xcd, 0xda);
            rgbTable[KnownColors.msocbvcrCBBdrOuterDocked] = Color.FromArgb(0xc4, 0xcd, 0xda);
            rgbTable[KnownColors.msocbvcrCBBdrOuterFloating] = Color.FromArgb(0x2a, 0x66, 0xc9);
            rgbTable[KnownColors.msocbvcrCBBkgd] = Color.FromArgb(0xc4, 0xdb, 0xf9);
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseDown] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelected] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelectedMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrCBCtlBkgd] = Color.FromArgb(0xc4, 0xdb, 0xf9);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelected] = Color.FromArgb(0xff, 0xc0, 0x6f);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextDisabled] = Color.FromArgb(0x8d, 0x8d, 0x8d);
            rgbTable[KnownColors.msocbvcrCBCtlTextLight] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBDockSeparatorLine] = Color.FromArgb(0, 0x35, 0x91);
            rgbTable[KnownColors.msocbvcrCBDragHandle] = Color.FromArgb(0x27, 0x41, 0x76);
            rgbTable[KnownColors.msocbvcrCBDragHandleShadow] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBDropDownArrow] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzBegin] = Color.FromArgb(0x9e, 190, 0xf5);
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzEnd] = Color.FromArgb(0xc4, 0xda, 250);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedBegin] = Color.FromArgb(0xcb, 0xdd, 0xf6);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedEnd] = Color.FromArgb(0x72, 0x9b, 0xd7);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle] = Color.FromArgb(0xa1, 0xc5, 0xf9);
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdBegin] = Color.FromArgb(0xe3, 0xef, 0xff);
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdEnd] = Color.FromArgb(0x7b, 0xa4, 0xe0);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownBegin] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownEnd] = Color.FromArgb(0xff, 0xdf, 0x9a);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownMiddle] = Color.FromArgb(0xff, 0xb1, 0x6d);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverBegin] = Color.FromArgb(0xff, 0xff, 0xde);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverEnd] = Color.FromArgb(0xff, 0xcb, 0x88);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverMiddle] = Color.FromArgb(0xff, 0xe1, 0xac);
            rgbTable[KnownColors.msocbvcrCBGradOptionsBegin] = Color.FromArgb(0x7f, 0xb1, 250);
            rgbTable[KnownColors.msocbvcrCBGradOptionsEnd] = Color.FromArgb(0, 0x35, 0x91);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMiddle] = Color.FromArgb(0x52, 0x7f, 0xd0);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverBegin] = Color.FromArgb(0xff, 0xff, 0xde);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverEnd] = Color.FromArgb(0xff, 0xc1, 0x76);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverMiddle] = Color.FromArgb(0xff, 0xe1, 0xac);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedBegin] = Color.FromArgb(0xfe, 140, 0x49);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedEnd] = Color.FromArgb(0xff, 0xdd, 0x98);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedMiddle] = Color.FromArgb(0xff, 0xb8, 0x74);
            rgbTable[KnownColors.msocbvcrCBGradSelectedBegin] = Color.FromArgb(0xff, 0xdf, 0x9a);
            rgbTable[KnownColors.msocbvcrCBGradSelectedEnd] = Color.FromArgb(0xff, 0xa6, 0x4c);
            rgbTable[KnownColors.msocbvcrCBGradSelectedMiddle] = Color.FromArgb(0xff, 0xc3, 0x74);
            rgbTable[KnownColors.msocbvcrCBGradVertBegin] = Color.FromArgb(0xe3, 0xef, 0xff);
            rgbTable[KnownColors.msocbvcrCBGradVertEnd] = Color.FromArgb(0x7b, 0xa4, 0xe0);
            rgbTable[KnownColors.msocbvcrCBGradVertMiddle] = Color.FromArgb(0xcb, 0xe1, 0xfc);
            rgbTable[KnownColors.msocbvcrCBIconDisabledDark] = Color.FromArgb(0x61, 0x7a, 0xac);
            rgbTable[KnownColors.msocbvcrCBIconDisabledLight] = Color.FromArgb(0xe9, 0xec, 0xf2);
            rgbTable[KnownColors.msocbvcrCBLabelBkgnd] = Color.FromArgb(0xba, 0xd3, 0xf5);
            rgbTable[KnownColors.msocbvcrCBLabelBkgnd] = Color.FromArgb(0xba, 0xd3, 0xf5);
            rgbTable[KnownColors.msocbvcrCBLowColorIconDisabled] = Color.FromArgb(0x6d, 150, 0xd0);
            rgbTable[KnownColors.msocbvcrCBMainMenuBkgd] = Color.FromArgb(0x99, 0xcc, 0xff);
            rgbTable[KnownColors.msocbvcrCBMenuBdrOuter] = Color.FromArgb(0, 0x2d, 150);
            rgbTable[KnownColors.msocbvcrCBMenuBkgd] = Color.FromArgb(0xf6, 0xf6, 0xf6);
            rgbTable[KnownColors.msocbvcrCBMenuCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBMenuCtlTextDisabled] = Color.FromArgb(0x8d, 0x8d, 0x8d);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgd] = Color.FromArgb(0xcb, 0xe1, 0xfc);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgdDropped] = Color.FromArgb(0xac, 0xb7, 0xc9);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgdDropped] = Color.FromArgb(0xac, 0xb7, 0xc9);
            rgbTable[KnownColors.msocbvcrCBMenuShadow] = Color.FromArgb(0x5f, 130, 0xea);
            rgbTable[KnownColors.msocbvcrCBMenuSplitArrow] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrCBOptionsButtonShadow] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBShadow] = Color.FromArgb(0x3b, 0x61, 0x9c);
            rgbTable[KnownColors.msocbvcrCBSplitterLine] = Color.FromArgb(0x6a, 140, 0xcb);
            rgbTable[KnownColors.msocbvcrCBSplitterLineLight] = Color.FromArgb(0xf1, 0xf9, 0xff);
            rgbTable[KnownColors.msocbvcrCBTearOffHandle] = Color.FromArgb(0xa9, 0xc7, 240);
            rgbTable[KnownColors.msocbvcrCBTearOffHandleMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrCBTitleBkgd] = Color.FromArgb(0x2a, 0x66, 0xc9);
            rgbTable[KnownColors.msocbvcrCBTitleText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDisabledFocuslessHighlightedText] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrDisabledHighlightedText] = Color.FromArgb(0xbb, 0xce, 0xec);
            rgbTable[KnownColors.msocbvcrDlgGroupBoxText] = Color.FromArgb(0, 70, 0xd5);
            rgbTable[KnownColors.msocbvcrDocTabBdr] = Color.FromArgb(0, 0x35, 0x9a);
            rgbTable[KnownColors.msocbvcrDocTabBdrDark] = Color.FromArgb(0x75, 0xa6, 0xf1);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseDown] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseDown] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseDown] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0, 0, 0x80);
            rgbTable[KnownColors.msocbvcrDocTabBdrSelected] = Color.FromArgb(0x3b, 0x61, 0x9c);
            rgbTable[KnownColors.msocbvcrDocTabBkgd] = Color.FromArgb(0xba, 0xd3, 0xf5);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDocTabBkgdSelected] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDocTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextSelected] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = Color.FromArgb(0xba, 0xd3, 0xf5);
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = Color.FromArgb(0xba, 0xd3, 0xf5);
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = Color.FromArgb(0x5e, 0x5e, 0x5e);
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = Color.FromArgb(0x5e, 0x5e, 0x5e);
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = Color.FromArgb(0x81, 0xa9, 0xe2);
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = Color.FromArgb(0x81, 0xa9, 0xe2);
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDWTabTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrGDHeaderBdr] = Color.FromArgb(0x59, 0x59, 0xac);
            rgbTable[KnownColors.msocbvcrGDHeaderBkgd] = Color.FromArgb(0xef, 0xeb, 0xde);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBdr] = Color.FromArgb(0x7e, 0x7d, 0x68);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgd] = Color.FromArgb(0xef, 0xeb, 0xde);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgdSelected] = Color.FromArgb(0xff, 0xc0, 0x6f);
            rgbTable[KnownColors.msocbvcrGDHeaderSeeThroughSelection] = Color.FromArgb(0xbf, 0xbf, 0xdf);
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = Color.FromArgb(0x4a, 0x7a, 0xc9);
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = Color.FromArgb(0x4a, 0x7a, 0xc9);
            rgbTable[KnownColors.msocbvcrGSPGroupContentDarkBkgd] = Color.FromArgb(0xb9, 0xd0, 0xf1);
            rgbTable[KnownColors.msocbvcrGSPGroupContentLightBkgd] = Color.FromArgb(0xdd, 0xec, 0xfe);
            rgbTable[KnownColors.msocbvcrGSPGroupContentText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrGSPGroupContentTextDisabled] = Color.FromArgb(150, 0x91, 0x85);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderDarkBkgd] = Color.FromArgb(0x65, 0x8f, 0xe0);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderLightBkgd] = Color.FromArgb(0xc4, 0xdb, 0xf9);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = Color.FromArgb(0, 0x2d, 0x86);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = Color.FromArgb(0, 0x2d, 0x86);
            rgbTable[KnownColors.msocbvcrGSPGroupline] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrGSPGroupline] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrGSPHyperlink] = Color.FromArgb(0, 0x3d, 0xb2);
            rgbTable[KnownColors.msocbvcrGSPLightBkgd] = Color.FromArgb(0xdd, 0xec, 0xfe);
            rgbTable[KnownColors.msocbvcrHyperlink] = Color.FromArgb(0, 0x3d, 0xb2);
            rgbTable[KnownColors.msocbvcrHyperlinkFollowed] = Color.FromArgb(170, 0, 170);
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = Color.FromArgb(0x3b, 0x61, 0x9c);
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = Color.FromArgb(0x3b, 0x61, 0x9c);
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = Color.FromArgb(0x9e, 190, 0xf5);
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = Color.FromArgb(0x9e, 190, 0xf5);
            rgbTable[KnownColors.msocbvcrJotNavUIGradEnd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = Color.FromArgb(0xc4, 0xda, 250);
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = Color.FromArgb(0xc4, 0xda, 250);
            rgbTable[KnownColors.msocbvcrJotNavUIText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrListHeaderArrow] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrNetLookBkgnd] = Color.FromArgb(0xe3, 0xef, 0xff);
            rgbTable[KnownColors.msocbvcrOABBkgd] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrOBBkgdBdr] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrOBBkgdBdrContrast] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOGMDIParentWorkspaceBkgd] = Color.FromArgb(0x90, 0x99, 0xae);
            rgbTable[KnownColors.msocbvcrOGRulerActiveBkgd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOGRulerBdr] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrOGRulerBkgd] = Color.FromArgb(0xd8, 0xe7, 0xfc);
            rgbTable[KnownColors.msocbvcrOGRulerInactiveBkgd] = Color.FromArgb(0x9e, 190, 0xf5);
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdr] = Color.FromArgb(0x4b, 120, 0xca);
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdrHighlight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOGRulerTabStopTicks] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrOGRulerText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrOGTaskPaneGroupBoxHeaderBkgd] = Color.FromArgb(0xba, 0xd3, 0xf5);
            rgbTable[KnownColors.msocbvcrOGWorkspaceBkgd] = Color.FromArgb(0x90, 0x99, 0xae);
            rgbTable[KnownColors.msocbvcrOLKFlagNone] = Color.FromArgb(0xf2, 240, 0xe4);
            rgbTable[KnownColors.msocbvcrOLKFolderbarDark] = Color.FromArgb(0, 0x35, 0x91);
            rgbTable[KnownColors.msocbvcrOLKFolderbarLight] = Color.FromArgb(0x59, 0x87, 0xd6);
            rgbTable[KnownColors.msocbvcrOLKFolderbarText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKGridlines] = Color.FromArgb(0xea, 0xe9, 0xe1);
            rgbTable[KnownColors.msocbvcrOLKGroupLine] = Color.FromArgb(0x7b, 0xa4, 0xe0);
            rgbTable[KnownColors.msocbvcrOLKGroupNested] = Color.FromArgb(0xfd, 0xee, 0xc9);
            rgbTable[KnownColors.msocbvcrOLKGroupShaded] = Color.FromArgb(190, 0xda, 0xfb);
            rgbTable[KnownColors.msocbvcrOLKGroupText] = Color.FromArgb(0x37, 0x68, 0xb9);
            rgbTable[KnownColors.msocbvcrOLKIconBar] = Color.FromArgb(0xfd, 0xf7, 0xe9);
            rgbTable[KnownColors.msocbvcrOLKInfoBarBkgd] = Color.FromArgb(0x90, 0x99, 0xae);
            rgbTable[KnownColors.msocbvcrOLKInfoBarText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKPreviewPaneLabelText] = Color.FromArgb(0x90, 0x99, 0xae);
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorDark] = Color.FromArgb(0xbb, 0x55, 3);
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorLight] = Color.FromArgb(0xfb, 200, 0x4f);
            rgbTable[KnownColors.msocbvcrOLKWBActionDividerLine] = Color.FromArgb(0xd7, 0xe4, 0xfb);
            rgbTable[KnownColors.msocbvcrOLKWBButtonDark] = Color.FromArgb(0x7b, 0xa4, 0xe0);
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = Color.FromArgb(0xcb, 0xe1, 0xfc);
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = Color.FromArgb(0xcb, 0xe1, 0xfc);
            rgbTable[KnownColors.msocbvcrOLKWBDarkOutline] = Color.FromArgb(0, 0x2d, 150);
            rgbTable[KnownColors.msocbvcrOLKWBFoldersBackground] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonDark] = Color.FromArgb(0xf7, 190, 0x57);
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonLight] = Color.FromArgb(0xff, 0xff, 220);
            rgbTable[KnownColors.msocbvcrOLKWBLabelText] = Color.FromArgb(50, 0x45, 0x69);
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonDark] = Color.FromArgb(0xf8, 0xde, 0x80);
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonLight] = Color.FromArgb(0xe8, 0x7f, 8);
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonDark] = Color.FromArgb(0xee, 0x93, 0x11);
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonLight] = Color.FromArgb(0xfb, 230, 0x94);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterDark] = Color.FromArgb(0, 0x35, 0x91);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = Color.FromArgb(0x59, 0x87, 0xd6);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = Color.FromArgb(0x59, 0x87, 0xd6);
            rgbTable[KnownColors.msocbvcrPlacesBarBkgd] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabAreaBkgd] = Color.FromArgb(0xc3, 0xda, 0xf9);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabBdr] = Color.FromArgb(0x3b, 0x61, 0x9c);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabInactiveBkgd] = Color.FromArgb(0x9e, 190, 0xf5);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelected] = Color.FromArgb(0x3d, 0x6c, 0xc0);
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelectedMouseOver] = Color.FromArgb(0x3d, 0x6c, 0xc0);
            rgbTable[KnownColors.msocbvcrPPSlideBdrInactiveSelected] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrPPSlideBdrMouseOver] = Color.FromArgb(0x3d, 0x6c, 0xc0);
            rgbTable[KnownColors.msocbvcrPubPrintDocScratchPageBkgd] = Color.FromArgb(0x90, 0x99, 0xae);
            rgbTable[KnownColors.msocbvcrPubWebDocScratchPageBkgd] = Color.FromArgb(0xbd, 0xc2, 0xcf);
            rgbTable[KnownColors.msocbvcrSBBdr] = Color.FromArgb(0xd3, 0xd3, 0xd3);
            rgbTable[KnownColors.msocbvcrScrollbarBkgd] = Color.FromArgb(0xfb, 0xfb, 0xf8);
            rgbTable[KnownColors.msocbvcrToastGradBegin] = Color.FromArgb(220, 0xec, 0xfe);
            rgbTable[KnownColors.msocbvcrToastGradEnd] = Color.FromArgb(0xa7, 0xc5, 0xee);
            rgbTable[KnownColors.msocbvcrWPBdrInnerDocked] = Color.FromArgb(0xb9, 0xd4, 0xf9);
            rgbTable[KnownColors.msocbvcrWPBdrOuterDocked] = Color.FromArgb(0xc4, 0xda, 250);
            rgbTable[KnownColors.msocbvcrWPBdrOuterFloating] = Color.FromArgb(0x2a, 0x66, 0xc9);
            rgbTable[KnownColors.msocbvcrWPBkgd] = Color.FromArgb(0xdd, 0xec, 0xfe);
            rgbTable[KnownColors.msocbvcrWPCtlBdr] = Color.FromArgb(0x7f, 0x9d, 0xb9);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDisabled] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrWPCtlBkgd] = Color.FromArgb(0xa9, 0xc7, 240);
            rgbTable[KnownColors.msocbvcrWPCtlBkgdDisabled] = Color.FromArgb(0xde, 0xde, 0xde);
            rgbTable[KnownColors.msocbvcrWPCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlTextDisabled] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPGroupline] = Color.FromArgb(0x7b, 0xa4, 0xe0);
            rgbTable[KnownColors.msocbvcrWPInfoTipBkgd] = Color.FromArgb(0xff, 0xff, 0xcc);
            rgbTable[KnownColors.msocbvcrWPInfoTipText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPNavBarBkgnd] = Color.FromArgb(0x4a, 0x7a, 0xc9);
            rgbTable[KnownColors.msocbvcrWPText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPTextDisabled] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrWPTitleBkgdActive] = Color.FromArgb(0x7b, 0xa4, 0xe0);
            rgbTable[KnownColors.msocbvcrWPTitleBkgdInactive] = Color.FromArgb(0x94, 0xbb, 0xef);
            rgbTable[KnownColors.msocbvcrWPTitleTextActive] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPTitleTextInactive] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrXLFormulaBarBkgd] = Color.FromArgb(0x9e, 190, 0xf5);
        }

        private void InitCommonColors(ref Dictionary<KnownColors, Color> rgbTable)
        {
            if (!DisplayInformation.LowResolution)
            {
                using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                {
                    rgbTable[KnownColors.ButtonPressedHighlight] = GetAlphaBlendedColor(graphics, SystemColors.Window, GetAlphaBlendedColor(graphics, SystemColors.Highlight, SystemColors.Window, 160), 50);
                    rgbTable[KnownColors.ButtonCheckedHighlight] = GetAlphaBlendedColor(graphics, SystemColors.Window, GetAlphaBlendedColor(graphics, SystemColors.Highlight, SystemColors.Window, 80), 20);
                    rgbTable[KnownColors.ButtonSelectedHighlight] = rgbTable[KnownColors.ButtonCheckedHighlight];
                    return;
                }
            }
            rgbTable[KnownColors.ButtonPressedHighlight] = SystemColors.Highlight;
            rgbTable[KnownColors.ButtonCheckedHighlight] = SystemColors.ControlLight;
            rgbTable[KnownColors.ButtonSelectedHighlight] = SystemColors.ControlLight;
        }

        internal void InitOliveLunaColors(ref Dictionary<KnownColors, Color> rgbTable)
        {
            rgbTable[KnownColors.msocbvcrCBBdrOuterDocked] = Color.FromArgb(0x51, 0x5e, 0x33);
            rgbTable[KnownColors.msocbvcrCBBdrOuterDocked] = Color.FromArgb(0x51, 0x5e, 0x33);
            rgbTable[KnownColors.msocbvcrCBBdrOuterFloating] = Color.FromArgb(0x74, 0x86, 0x5e);
            rgbTable[KnownColors.msocbvcrCBBkgd] = Color.FromArgb(0xd1, 0xde, 0xad);
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseDown] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelected] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelectedMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrCBCtlBkgd] = Color.FromArgb(0xd1, 0xde, 0xad);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelected] = Color.FromArgb(0xff, 0xc0, 0x6f);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextDisabled] = Color.FromArgb(0x8d, 0x8d, 0x8d);
            rgbTable[KnownColors.msocbvcrCBCtlTextLight] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBDockSeparatorLine] = Color.FromArgb(0x60, 0x77, 0x42);
            rgbTable[KnownColors.msocbvcrCBDragHandle] = Color.FromArgb(0x51, 0x5e, 0x33);
            rgbTable[KnownColors.msocbvcrCBDragHandleShadow] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBDropDownArrow] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzBegin] = Color.FromArgb(0xd9, 0xd9, 0xa7);
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzEnd] = Color.FromArgb(0xf2, 0xf1, 0xe4);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedBegin] = Color.FromArgb(230, 230, 0xd1);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedEnd] = Color.FromArgb(160, 0xb1, 0x74);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle] = Color.FromArgb(0xba, 0xc9, 0x8f);
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdBegin] = Color.FromArgb(0xed, 240, 0xd6);
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdEnd] = Color.FromArgb(0xb5, 0xc4, 0x8f);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownBegin] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownEnd] = Color.FromArgb(0xff, 0xdf, 0x9a);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownMiddle] = Color.FromArgb(0xff, 0xb1, 0x6d);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverBegin] = Color.FromArgb(0xff, 0xff, 0xde);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverEnd] = Color.FromArgb(0xff, 0xcb, 0x88);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverMiddle] = Color.FromArgb(0xff, 0xe1, 0xac);
            rgbTable[KnownColors.msocbvcrCBGradOptionsBegin] = Color.FromArgb(0xba, 0xcc, 150);
            rgbTable[KnownColors.msocbvcrCBGradOptionsEnd] = Color.FromArgb(0x60, 0x77, 0x6b);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMiddle] = Color.FromArgb(0x8d, 160, 0x6b);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverBegin] = Color.FromArgb(0xff, 0xff, 0xde);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverEnd] = Color.FromArgb(0xff, 0xc1, 0x76);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverMiddle] = Color.FromArgb(0xff, 0xe1, 0xac);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedBegin] = Color.FromArgb(0xfe, 140, 0x49);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedEnd] = Color.FromArgb(0xff, 0xdd, 0x98);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedMiddle] = Color.FromArgb(0xff, 0xb8, 0x74);
            rgbTable[KnownColors.msocbvcrCBGradSelectedBegin] = Color.FromArgb(0xff, 0xdf, 0x9a);
            rgbTable[KnownColors.msocbvcrCBGradSelectedEnd] = Color.FromArgb(0xff, 0xa6, 0x4c);
            rgbTable[KnownColors.msocbvcrCBGradSelectedMiddle] = Color.FromArgb(0xff, 0xc3, 0x74);
            rgbTable[KnownColors.msocbvcrCBGradVertBegin] = Color.FromArgb(0xff, 0xff, 0xed);
            rgbTable[KnownColors.msocbvcrCBGradVertEnd] = Color.FromArgb(0xb5, 0xc4, 0x8f);
            rgbTable[KnownColors.msocbvcrCBGradVertMiddle] = Color.FromArgb(0xce, 220, 0xa7);
            rgbTable[KnownColors.msocbvcrCBIconDisabledDark] = Color.FromArgb(0x83, 0x90, 0x71);
            rgbTable[KnownColors.msocbvcrCBIconDisabledLight] = Color.FromArgb(0xf3, 0xf4, 240);
            rgbTable[KnownColors.msocbvcrCBLabelBkgnd] = Color.FromArgb(0xda, 0xe3, 0xbb);
            rgbTable[KnownColors.msocbvcrCBLabelBkgnd] = Color.FromArgb(0xda, 0xe3, 0xbb);
            rgbTable[KnownColors.msocbvcrCBLowColorIconDisabled] = Color.FromArgb(0x9f, 0xae, 0x7a);
            rgbTable[KnownColors.msocbvcrCBMainMenuBkgd] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrCBMenuBdrOuter] = Color.FromArgb(0x75, 0x8d, 0x5e);
            rgbTable[KnownColors.msocbvcrCBMenuBkgd] = Color.FromArgb(0xf4, 0xf4, 0xee);
            rgbTable[KnownColors.msocbvcrCBMenuCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBMenuCtlTextDisabled] = Color.FromArgb(0x8d, 0x8d, 0x8d);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgd] = Color.FromArgb(0xd8, 0xe3, 0xb6);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgdDropped] = Color.FromArgb(0xad, 0xb5, 0x9d);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgdDropped] = Color.FromArgb(0xad, 0xb5, 0x9d);
            rgbTable[KnownColors.msocbvcrCBMenuShadow] = Color.FromArgb(0x86, 0x94, 0x6c);
            rgbTable[KnownColors.msocbvcrCBMenuSplitArrow] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBOptionsButtonShadow] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBShadow] = Color.FromArgb(0x60, 0x80, 0x58);
            rgbTable[KnownColors.msocbvcrCBSplitterLine] = Color.FromArgb(0x60, 0x80, 0x58);
            rgbTable[KnownColors.msocbvcrCBSplitterLineLight] = Color.FromArgb(0xf4, 0xf7, 0xde);
            rgbTable[KnownColors.msocbvcrCBTearOffHandle] = Color.FromArgb(0xc5, 0xd4, 0x9f);
            rgbTable[KnownColors.msocbvcrCBTearOffHandleMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrCBTitleBkgd] = Color.FromArgb(0x74, 0x86, 0x5e);
            rgbTable[KnownColors.msocbvcrCBTitleText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDisabledFocuslessHighlightedText] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrDisabledHighlightedText] = Color.FromArgb(220, 0xe0, 0xd0);
            rgbTable[KnownColors.msocbvcrDlgGroupBoxText] = Color.FromArgb(0x99, 0x54, 10);
            rgbTable[KnownColors.msocbvcrDocTabBdr] = Color.FromArgb(0x60, 0x77, 0x6b);
            rgbTable[KnownColors.msocbvcrDocTabBdrDark] = Color.FromArgb(0xb0, 0xc2, 140);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseDown] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseDown] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseDown] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0x3f, 0x5d, 0x38);
            rgbTable[KnownColors.msocbvcrDocTabBdrSelected] = Color.FromArgb(0x60, 0x80, 0x58);
            rgbTable[KnownColors.msocbvcrDocTabBkgd] = Color.FromArgb(0xda, 0xe3, 0xbb);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDocTabBkgdSelected] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDocTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextSelected] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = Color.FromArgb(0xda, 0xe3, 0xbb);
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = Color.FromArgb(0xda, 0xe3, 0xbb);
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = Color.FromArgb(0xb7, 0xc6, 0x91);
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = Color.FromArgb(0xb7, 0xc6, 0x91);
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDWTabTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrGDHeaderBdr] = Color.FromArgb(0xbf, 0xbf, 0xdf);
            rgbTable[KnownColors.msocbvcrGDHeaderBkgd] = Color.FromArgb(0xef, 0xeb, 0xde);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBdr] = Color.FromArgb(0x7e, 0x7d, 0x68);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgd] = Color.FromArgb(0xef, 0xeb, 0xde);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgdSelected] = Color.FromArgb(0xff, 0xc0, 0x6f);
            rgbTable[KnownColors.msocbvcrGDHeaderSeeThroughSelection] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = Color.FromArgb(0x9f, 0xab, 0x80);
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = Color.FromArgb(0x9f, 0xab, 0x80);
            rgbTable[KnownColors.msocbvcrGSPGroupContentDarkBkgd] = Color.FromArgb(0xd9, 0xe3, 0xbb);
            rgbTable[KnownColors.msocbvcrGSPGroupContentLightBkgd] = Color.FromArgb(230, 0xea, 0xd0);
            rgbTable[KnownColors.msocbvcrGSPGroupContentText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrGSPGroupContentTextDisabled] = Color.FromArgb(150, 0x91, 0x85);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderDarkBkgd] = Color.FromArgb(0xa1, 0xb0, 0x80);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderLightBkgd] = Color.FromArgb(210, 0xdf, 0xae);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = Color.FromArgb(90, 0x6b, 70);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = Color.FromArgb(90, 0x6b, 70);
            rgbTable[KnownColors.msocbvcrGSPGroupline] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrGSPGroupline] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrGSPHyperlink] = Color.FromArgb(0, 0x3d, 0xb2);
            rgbTable[KnownColors.msocbvcrGSPLightBkgd] = Color.FromArgb(0xf3, 0xf2, 0xe7);
            rgbTable[KnownColors.msocbvcrHyperlink] = Color.FromArgb(0, 0x3d, 0xb2);
            rgbTable[KnownColors.msocbvcrHyperlinkFollowed] = Color.FromArgb(170, 0, 170);
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = Color.FromArgb(0x60, 0x80, 0x58);
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = Color.FromArgb(0x60, 0x80, 0x58);
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = Color.FromArgb(0xd9, 0xd9, 0xa7);
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = Color.FromArgb(0xd9, 0xd9, 0xa7);
            rgbTable[KnownColors.msocbvcrJotNavUIGradEnd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = Color.FromArgb(0xf2, 0xf1, 0xe4);
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = Color.FromArgb(0xf2, 0xf1, 0xe4);
            rgbTable[KnownColors.msocbvcrJotNavUIText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrListHeaderArrow] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrNetLookBkgnd] = Color.FromArgb(0xff, 0xff, 0xed);
            rgbTable[KnownColors.msocbvcrOABBkgd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOBBkgdBdr] = Color.FromArgb(0xd3, 0xd3, 0xd3);
            rgbTable[KnownColors.msocbvcrOBBkgdBdrContrast] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrOGMDIParentWorkspaceBkgd] = Color.FromArgb(0x97, 160, 0x7b);
            rgbTable[KnownColors.msocbvcrOGRulerActiveBkgd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOGRulerBdr] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrOGRulerBkgd] = Color.FromArgb(0xe2, 0xe7, 0xbf);
            rgbTable[KnownColors.msocbvcrOGRulerInactiveBkgd] = Color.FromArgb(0xab, 0xc0, 0x8a);
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdr] = Color.FromArgb(0x75, 0x8d, 0x5e);
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdrHighlight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOGRulerTabStopTicks] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrOGRulerText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrOGTaskPaneGroupBoxHeaderBkgd] = Color.FromArgb(0xda, 0xe3, 0xbb);
            rgbTable[KnownColors.msocbvcrOGWorkspaceBkgd] = Color.FromArgb(0x97, 160, 0x7b);
            rgbTable[KnownColors.msocbvcrOLKFlagNone] = Color.FromArgb(0xf2, 240, 0xe4);
            rgbTable[KnownColors.msocbvcrOLKFolderbarDark] = Color.FromArgb(0x60, 0x77, 0x42);
            rgbTable[KnownColors.msocbvcrOLKFolderbarLight] = Color.FromArgb(0xaf, 0xc0, 130);
            rgbTable[KnownColors.msocbvcrOLKFolderbarText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKGridlines] = Color.FromArgb(0xea, 0xe9, 0xe1);
            rgbTable[KnownColors.msocbvcrOLKGroupLine] = Color.FromArgb(0xb5, 0xc4, 0x8f);
            rgbTable[KnownColors.msocbvcrOLKGroupNested] = Color.FromArgb(0xfd, 0xee, 0xc9);
            rgbTable[KnownColors.msocbvcrOLKGroupShaded] = Color.FromArgb(0xaf, 0xba, 0x91);
            rgbTable[KnownColors.msocbvcrOLKGroupText] = Color.FromArgb(0x73, 0x89, 0x54);
            rgbTable[KnownColors.msocbvcrOLKIconBar] = Color.FromArgb(0xfd, 0xf7, 0xe9);
            rgbTable[KnownColors.msocbvcrOLKInfoBarBkgd] = Color.FromArgb(0x97, 160, 0x7b);
            rgbTable[KnownColors.msocbvcrOLKInfoBarText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKPreviewPaneLabelText] = Color.FromArgb(0x97, 160, 0x7b);
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorDark] = Color.FromArgb(0xbb, 0x55, 3);
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorLight] = Color.FromArgb(0xfb, 200, 0x4f);
            rgbTable[KnownColors.msocbvcrOLKWBActionDividerLine] = Color.FromArgb(200, 0xd4, 0xac);
            rgbTable[KnownColors.msocbvcrOLKWBButtonDark] = Color.FromArgb(0xb0, 0xbf, 0x8a);
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = Color.FromArgb(0xea, 240, 0xcf);
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = Color.FromArgb(0xea, 240, 0xcf);
            rgbTable[KnownColors.msocbvcrOLKWBDarkOutline] = Color.FromArgb(0x60, 0x80, 0x58);
            rgbTable[KnownColors.msocbvcrOLKWBFoldersBackground] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonDark] = Color.FromArgb(0xf7, 190, 0x57);
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonLight] = Color.FromArgb(0xff, 0xff, 220);
            rgbTable[KnownColors.msocbvcrOLKWBLabelText] = Color.FromArgb(50, 0x45, 0x69);
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonDark] = Color.FromArgb(0xf8, 0xde, 0x80);
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonLight] = Color.FromArgb(0xe8, 0x7f, 8);
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonDark] = Color.FromArgb(0xee, 0x93, 0x11);
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonLight] = Color.FromArgb(0xfb, 230, 0x94);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterDark] = Color.FromArgb(0x40, 0x51, 0x3b);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = Color.FromArgb(120, 0x8e, 0x6f);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = Color.FromArgb(120, 0x8e, 0x6f);
            rgbTable[KnownColors.msocbvcrPlacesBarBkgd] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabAreaBkgd] = Color.FromArgb(0xf2, 240, 0xe4);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabBdr] = Color.FromArgb(0x60, 0x80, 0x58);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabInactiveBkgd] = Color.FromArgb(0xce, 220, 0xa7);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelected] = Color.FromArgb(0x6b, 0x81, 0x6b);
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelectedMouseOver] = Color.FromArgb(0x6b, 0x81, 0x6b);
            rgbTable[KnownColors.msocbvcrPPSlideBdrInactiveSelected] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrPPSlideBdrMouseOver] = Color.FromArgb(0x6b, 0x81, 0x6b);
            rgbTable[KnownColors.msocbvcrPubPrintDocScratchPageBkgd] = Color.FromArgb(0x97, 160, 0x7b);
            rgbTable[KnownColors.msocbvcrPubWebDocScratchPageBkgd] = Color.FromArgb(0xc1, 0xc6, 0xb0);
            rgbTable[KnownColors.msocbvcrSBBdr] = Color.FromArgb(0xd3, 0xd3, 0xd3);
            rgbTable[KnownColors.msocbvcrScrollbarBkgd] = Color.FromArgb(0xf9, 0xf9, 0xf7);
            rgbTable[KnownColors.msocbvcrToastGradBegin] = Color.FromArgb(0xed, 0xf2, 0xd4);
            rgbTable[KnownColors.msocbvcrToastGradEnd] = Color.FromArgb(0xbf, 0xce, 0x99);
            rgbTable[KnownColors.msocbvcrWPBdrInnerDocked] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrWPBdrOuterDocked] = Color.FromArgb(0xf2, 0xf1, 0xe4);
            rgbTable[KnownColors.msocbvcrWPBdrOuterFloating] = Color.FromArgb(0x74, 0x86, 0x5e);
            rgbTable[KnownColors.msocbvcrWPBkgd] = Color.FromArgb(0xf3, 0xf2, 0xe7);
            rgbTable[KnownColors.msocbvcrWPCtlBdr] = Color.FromArgb(0xa4, 0xb9, 0x7f);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDisabled] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrWPCtlBkgd] = Color.FromArgb(0xc5, 0xd4, 0x9f);
            rgbTable[KnownColors.msocbvcrWPCtlBkgdDisabled] = Color.FromArgb(0xde, 0xde, 0xde);
            rgbTable[KnownColors.msocbvcrWPCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlTextDisabled] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPGroupline] = Color.FromArgb(0xbc, 0xbb, 0xb1);
            rgbTable[KnownColors.msocbvcrWPInfoTipBkgd] = Color.FromArgb(0xff, 0xff, 0xcc);
            rgbTable[KnownColors.msocbvcrWPInfoTipText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPNavBarBkgnd] = Color.FromArgb(0x74, 0x86, 0x5e);
            rgbTable[KnownColors.msocbvcrWPText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPTextDisabled] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrWPTitleBkgdActive] = Color.FromArgb(0xd8, 0xe3, 0xb6);
            rgbTable[KnownColors.msocbvcrWPTitleBkgdInactive] = Color.FromArgb(0xbc, 0xcd, 0x83);
            rgbTable[KnownColors.msocbvcrWPTitleTextActive] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPTitleTextInactive] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrXLFormulaBarBkgd] = Color.FromArgb(0xd9, 0xd9, 0xa7);
        }

        private void InitRoyaleColors(ref Dictionary<KnownColors, Color> rgbTable)
        {
            rgbTable[KnownColors.msocbvcrCBBkgd] = Color.FromArgb(0xee, 0xed, 240);
            rgbTable[KnownColors.msocbvcrCBDragHandle] = Color.FromArgb(0xbd, 0xbc, 0xbf);
            rgbTable[KnownColors.msocbvcrCBSplitterLine] = Color.FromArgb(0xc1, 0xc1, 0xc4);
            rgbTable[KnownColors.msocbvcrCBTitleBkgd] = Color.FromArgb(0xa7, 0xa6, 170);
            rgbTable[KnownColors.msocbvcrCBTitleText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBBdrOuterFloating] = Color.FromArgb(0x8e, 0x8d, 0x91);
            rgbTable[KnownColors.msocbvcrCBBdrOuterDocked] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrCBTearOffHandle] = Color.FromArgb(0xee, 0xed, 240);
            rgbTable[KnownColors.msocbvcrCBTearOffHandleMouseOver] = Color.FromArgb(0xc2, 0xcf, 0xe5);
            rgbTable[KnownColors.msocbvcrCBCtlBkgd] = Color.FromArgb(0xee, 0xed, 240);
            rgbTable[KnownColors.msocbvcrCBCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextDisabled] = Color.FromArgb(0xb0, 0xaf, 0xb3);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver] = Color.FromArgb(0xc2, 0xcf, 0xe5);
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseOver] = Color.FromArgb(0x33, 0x5e, 0xa8);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseDown] = Color.FromArgb(0x99, 0xaf, 0xd4);
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseDown] = Color.FromArgb(0x33, 0x5e, 0xa8);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseDown] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelected] = Color.FromArgb(0xe2, 0xe5, 0xee);
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelected] = Color.FromArgb(0x33, 0x5e, 0xa8);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver] = Color.FromArgb(0x33, 0x5e, 0xa8);
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelectedMouseOver] = Color.FromArgb(0x33, 0x5e, 0xa8);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBCtlTextLight] = Color.FromArgb(0xa7, 0xa6, 170);
            rgbTable[KnownColors.msocbvcrCBMainMenuBkgd] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrCBMenuBkgd] = Color.FromArgb(0xfc, 0xfc, 0xfc);
            rgbTable[KnownColors.msocbvcrCBMenuCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBMenuCtlTextDisabled] = Color.FromArgb(0xc1, 0xc1, 0xc4);
            rgbTable[KnownColors.msocbvcrCBMenuBdrOuter] = Color.FromArgb(0x86, 0x85, 0x88);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgd] = Color.FromArgb(0xee, 0xed, 240);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgdDropped] = Color.FromArgb(0xe4, 0xe2, 230);
            rgbTable[KnownColors.msocbvcrCBMenuSplitArrow] = Color.FromArgb(0xa7, 0xa6, 170);
            rgbTable[KnownColors.msocbvcrWPBkgd] = Color.FromArgb(0xf5, 0xf4, 0xf6);
            rgbTable[KnownColors.msocbvcrWPText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPTitleBkgdActive] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPTitleBkgdInactive] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPTitleTextActive] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPTitleTextInactive] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPBdrOuterFloating] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPBdrOuterDocked] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlBdr] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDisabled] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlTextDisabled] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlBkgdDisabled] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPGroupline] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrSBBdr] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOBBkgdBdr] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOBBkgdBdrContrast] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOABBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGDHeaderBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGDHeaderBdr] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBdr] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGDHeaderSeeThroughSelection] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgdSelected] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrCBSplitterLineLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBShadow] = Color.FromArgb(0xee, 0xed, 240);
            rgbTable[KnownColors.msocbvcrCBOptionsButtonShadow] = Color.FromArgb(0xf5, 0xf4, 0xf6);
            rgbTable[KnownColors.msocbvcrWPNavBarBkgnd] = Color.FromArgb(0xc1, 0xc1, 0xc4);
            rgbTable[KnownColors.msocbvcrWPBdrInnerDocked] = Color.FromArgb(0xf5, 0xf4, 0xf6);
            rgbTable[KnownColors.msocbvcrCBLabelBkgnd] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrCBIconDisabledLight] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrCBIconDisabledDark] = Color.FromArgb(0xa7, 0xa6, 170);
            rgbTable[KnownColors.msocbvcrCBLowColorIconDisabled] = Color.FromArgb(0xb0, 0xaf, 0xb3);
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzBegin] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzEnd] = Color.FromArgb(0xfb, 250, 0xfb);
            rgbTable[KnownColors.msocbvcrCBGradVertBegin] = Color.FromArgb(0xfc, 0xfc, 0xfc);
            rgbTable[KnownColors.msocbvcrCBGradVertMiddle] = Color.FromArgb(0xf5, 0xf4, 0xf6);
            rgbTable[KnownColors.msocbvcrCBGradVertEnd] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrCBGradOptionsBegin] = Color.FromArgb(0xf2, 0xf2, 0xf2);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMiddle] = Color.FromArgb(0xe0, 0xe0, 0xe1);
            rgbTable[KnownColors.msocbvcrCBGradOptionsEnd] = Color.FromArgb(0xa7, 0xa6, 170);
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdBegin] = Color.FromArgb(0xfc, 0xfc, 0xfc);
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdEnd] = Color.FromArgb(0xf5, 0xf4, 0xf6);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedBegin] = Color.FromArgb(0xf7, 0xf6, 0xf8);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle] = Color.FromArgb(0xf1, 240, 0xf2);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedEnd] = Color.FromArgb(0xe4, 0xe2, 230);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedBegin] = Color.FromArgb(0xe2, 0xe5, 0xee);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedMiddle] = Color.FromArgb(0xe2, 0xe5, 0xee);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedEnd] = Color.FromArgb(0xe2, 0xe5, 0xee);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverBegin] = Color.FromArgb(0xc2, 0xcf, 0xe5);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverMiddle] = Color.FromArgb(0xc2, 0xcf, 0xe5);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverEnd] = Color.FromArgb(0xc2, 0xcf, 0xe5);
            rgbTable[KnownColors.msocbvcrCBGradSelectedBegin] = Color.FromArgb(0xe2, 0xe5, 0xee);
            rgbTable[KnownColors.msocbvcrCBGradSelectedMiddle] = Color.FromArgb(0xe2, 0xe5, 0xee);
            rgbTable[KnownColors.msocbvcrCBGradSelectedEnd] = Color.FromArgb(0xe2, 0xe5, 0xee);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverBegin] = Color.FromArgb(0xc2, 0xcf, 0xe5);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverMiddle] = Color.FromArgb(0xc2, 0xcf, 0xe5);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverEnd] = Color.FromArgb(0xc2, 0xcf, 0xe5);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownBegin] = Color.FromArgb(0x99, 0xaf, 0xd4);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownMiddle] = Color.FromArgb(0x99, 0xaf, 0xd4);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownEnd] = Color.FromArgb(0x99, 0xaf, 0xd4);
            rgbTable[KnownColors.msocbvcrNetLookBkgnd] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrCBMenuShadow] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBDockSeparatorLine] = Color.FromArgb(0x33, 0x5e, 0xa8);
            rgbTable[KnownColors.msocbvcrCBDropDownArrow] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrOLKGridlines] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKGroupText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKGroupLine] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKGroupShaded] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKGroupNested] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKIconBar] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKFlagNone] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKFolderbarLight] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKFolderbarDark] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKFolderbarText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBButtonDark] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonLight] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonDark] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonLight] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonDark] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonLight] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonDark] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBDarkOutline] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterDark] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBActionDividerLine] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBLabelText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKWBFoldersBackground] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorLight] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorDark] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKInfoBarBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKInfoBarText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOLKPreviewPaneLabelText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrHyperlink] = Color.FromArgb(0, 0x3d, 0xb2);
            rgbTable[KnownColors.msocbvcrHyperlinkFollowed] = Color.FromArgb(170, 0, 170);
            rgbTable[KnownColors.msocbvcrOGWorkspaceBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGMDIParentWorkspaceBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGRulerBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGRulerActiveBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGRulerInactiveBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGRulerText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGRulerTabStopTicks] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGRulerBdr] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdr] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdrHighlight] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrXLFormulaBarBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrCBDragHandleShadow] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOGTaskPaneGroupBoxHeaderBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabAreaBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabInactiveBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabBdr] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelected] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrPPSlideBdrInactiveSelected] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrPPSlideBdrMouseOver] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelectedMouseOver] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDlgGroupBoxText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrScrollbarBkgd] = Color.FromArgb(0xed, 0xeb, 0xef);
            rgbTable[KnownColors.msocbvcrListHeaderArrow] = Color.FromArgb(0x9b, 0x9a, 0x9c);
            rgbTable[KnownColors.msocbvcrDisabledHighlightedText] = Color.FromArgb(0xbc, 0xca, 0xe2);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = Color.FromArgb(0xeb, 0xe9, 0xed);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDisabledFocuslessHighlightedText] = Color.FromArgb(0xa7, 0xa6, 170);
            rgbTable[KnownColors.msocbvcrWPCtlTextMouseDown] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPTextDisabled] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPInfoTipBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrWPInfoTipText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseOver] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWTabTextMouseOver] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseDown] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDWTabTextMouseDown] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPLightBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderLightBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderDarkBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPGroupContentLightBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPGroupContentDarkBkgd] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPGroupContentText] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPGroupContentTextDisabled] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPGroupline] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrGSPHyperlink] = Color.FromArgb(0xff, 0x33, 0x99);
            rgbTable[KnownColors.msocbvcrDocTabBkgd] = Color.FromArgb(0xd4, 0xd4, 0xe2);
            rgbTable[KnownColors.msocbvcrDocTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabBdr] = Color.FromArgb(0x76, 0x74, 0x92);
            rgbTable[KnownColors.msocbvcrDocTabBdrLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDocTabBdrDark] = Color.FromArgb(0xba, 0xb9, 0xce);
            rgbTable[KnownColors.msocbvcrDocTabBkgdSelected] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDocTabTextSelected] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabBdrSelected] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseOver] = Color.FromArgb(0xc1, 210, 0xee);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0x31, 0x6a, 0xc5);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0x31, 0x6a, 0xc5);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0x31, 0x6a, 0xc5);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0x31, 0x6a, 0xc5);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseDown] = Color.FromArgb(0x9a, 0xb7, 0xe4);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseDown] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseDown] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseDown] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrToastGradBegin] = Color.FromArgb(0xf6, 0xf4, 0xec);
            rgbTable[KnownColors.msocbvcrToastGradEnd] = Color.FromArgb(0xb3, 0xb2, 0xcc);
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = Color.FromArgb(0xec, 0xe9, 0xd8);
            rgbTable[KnownColors.msocbvcrJotNavUIGradEnd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrJotNavUIText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrPlacesBarBkgd] = Color.FromArgb(0xe0, 0xdf, 0xe3);
            rgbTable[KnownColors.msocbvcrPubPrintDocScratchPageBkgd] = Color.FromArgb(0x98, 0xb5, 0xe2);
            rgbTable[KnownColors.msocbvcrPubWebDocScratchPageBkgd] = Color.FromArgb(0xc1, 210, 0xee);
        }

        internal void InitSilverLunaColors(ref Dictionary<KnownColors, Color> rgbTable)
        {
            rgbTable[KnownColors.msocbvcrCBBdrOuterDocked] = Color.FromArgb(0xad, 0xae, 0xc1);
            rgbTable[KnownColors.msocbvcrCBBdrOuterFloating] = Color.FromArgb(0x7a, 0x79, 0x99);
            rgbTable[KnownColors.msocbvcrCBBkgd] = Color.FromArgb(0xdb, 0xda, 0xe4);
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseDown] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelected] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelectedMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrCBCtlBkgd] = Color.FromArgb(0xdb, 0xda, 0xe4);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelected] = Color.FromArgb(0xff, 0xc0, 0x6f);
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextDisabled] = Color.FromArgb(0x8d, 0x8d, 0x8d);
            rgbTable[KnownColors.msocbvcrCBCtlTextLight] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBDockSeparatorLine] = Color.FromArgb(110, 0x6d, 0x8f);
            rgbTable[KnownColors.msocbvcrCBDragHandle] = Color.FromArgb(0x54, 0x54, 0x75);
            rgbTable[KnownColors.msocbvcrCBDragHandleShadow] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBDropDownArrow] = Color.FromArgb(0xe0, 0xdf, 0xe3);
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzBegin] = Color.FromArgb(0xd7, 0xd7, 0xe5);
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzEnd] = Color.FromArgb(0xf3, 0xf3, 0xf7);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedBegin] = Color.FromArgb(0xd7, 0xd7, 0xe2);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedEnd] = Color.FromArgb(0x76, 0x74, 0x97);
            rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle] = Color.FromArgb(0xb8, 0xb9, 0xca);
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdBegin] = Color.FromArgb(0xe8, 0xe9, 0xf2);
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdEnd] = Color.FromArgb(0xac, 170, 0xc2);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownBegin] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownEnd] = Color.FromArgb(0xff, 0xdf, 0x9a);
            rgbTable[KnownColors.msocbvcrCBGradMouseDownMiddle] = Color.FromArgb(0xff, 0xb1, 0x6d);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverBegin] = Color.FromArgb(0xff, 0xff, 0xde);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverEnd] = Color.FromArgb(0xff, 0xcb, 0x88);
            rgbTable[KnownColors.msocbvcrCBGradMouseOverMiddle] = Color.FromArgb(0xff, 0xe1, 0xac);
            rgbTable[KnownColors.msocbvcrCBGradOptionsBegin] = Color.FromArgb(0xba, 0xb9, 0xce);
            rgbTable[KnownColors.msocbvcrCBGradOptionsEnd] = Color.FromArgb(0x76, 0x74, 0x92);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMiddle] = Color.FromArgb(0x9c, 0x9b, 180);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverBegin] = Color.FromArgb(0xff, 0xff, 0xde);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverEnd] = Color.FromArgb(0xff, 0xc1, 0x76);
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverMiddle] = Color.FromArgb(0xff, 0xe1, 0xac);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedBegin] = Color.FromArgb(0xfe, 140, 0x49);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedEnd] = Color.FromArgb(0xff, 0xdd, 0x98);
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedMiddle] = Color.FromArgb(0xff, 0xb8, 0x74);
            rgbTable[KnownColors.msocbvcrCBGradSelectedBegin] = Color.FromArgb(0xff, 0xdf, 0x9a);
            rgbTable[KnownColors.msocbvcrCBGradSelectedEnd] = Color.FromArgb(0xff, 0xa6, 0x4c);
            rgbTable[KnownColors.msocbvcrCBGradSelectedMiddle] = Color.FromArgb(0xff, 0xc3, 0x74);
            rgbTable[KnownColors.msocbvcrCBGradVertBegin] = Color.FromArgb(0xf9, 0xf9, 0xff);
            rgbTable[KnownColors.msocbvcrCBGradVertEnd] = Color.FromArgb(0x93, 0x91, 0xb0);
            rgbTable[KnownColors.msocbvcrCBGradVertMiddle] = Color.FromArgb(0xe1, 0xe2, 0xec);
            rgbTable[KnownColors.msocbvcrCBIconDisabledDark] = Color.FromArgb(0x7a, 0x79, 0x99);
            rgbTable[KnownColors.msocbvcrCBIconDisabledLight] = Color.FromArgb(0xf7, 0xf5, 0xf9);
            rgbTable[KnownColors.msocbvcrCBLabelBkgnd] = Color.FromArgb(0xd4, 0xd4, 0xe2);
            rgbTable[KnownColors.msocbvcrCBLabelBkgnd] = Color.FromArgb(0xd4, 0xd4, 0xe2);
            rgbTable[KnownColors.msocbvcrCBLowColorIconDisabled] = Color.FromArgb(0xa8, 0xa7, 190);
            rgbTable[KnownColors.msocbvcrCBMainMenuBkgd] = Color.FromArgb(0xc6, 200, 0xd7);
            rgbTable[KnownColors.msocbvcrCBMenuBdrOuter] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrCBMenuBkgd] = Color.FromArgb(0xfd, 250, 0xff);
            rgbTable[KnownColors.msocbvcrCBMenuCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBMenuCtlTextDisabled] = Color.FromArgb(0x8d, 0x8d, 0x8d);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgd] = Color.FromArgb(0xd6, 0xd3, 0xe7);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgdDropped] = Color.FromArgb(0xb9, 0xbb, 200);
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgdDropped] = Color.FromArgb(0xb9, 0xbb, 200);
            rgbTable[KnownColors.msocbvcrCBMenuShadow] = Color.FromArgb(0x9a, 140, 0xb0);
            rgbTable[KnownColors.msocbvcrCBMenuSplitArrow] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrCBOptionsButtonShadow] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBShadow] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrCBSplitterLine] = Color.FromArgb(110, 0x6d, 0x8f);
            rgbTable[KnownColors.msocbvcrCBSplitterLineLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrCBTearOffHandle] = Color.FromArgb(0xc0, 0xc0, 0xd3);
            rgbTable[KnownColors.msocbvcrCBTearOffHandleMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrCBTitleBkgd] = Color.FromArgb(0x7a, 0x79, 0x99);
            rgbTable[KnownColors.msocbvcrCBTitleText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDisabledFocuslessHighlightedText] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrDisabledHighlightedText] = Color.FromArgb(0x3b, 0x3b, 0x3f);
            rgbTable[KnownColors.msocbvcrDlgGroupBoxText] = Color.FromArgb(7, 70, 0xd5);
            rgbTable[KnownColors.msocbvcrDocTabBdr] = Color.FromArgb(0x76, 0x74, 0x92);
            rgbTable[KnownColors.msocbvcrDocTabBdrDark] = Color.FromArgb(0xba, 0xb9, 0xce);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseDown] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrLight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseDown] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseDown] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
            rgbTable[KnownColors.msocbvcrDocTabBdrSelected] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrDocTabBkgd] = Color.FromArgb(0xd4, 0xd4, 0xe2);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDocTabBkgdSelected] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDocTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDocTabTextSelected] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = Color.FromArgb(0xd4, 0xd4, 0xe2);
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = Color.FromArgb(0xd4, 0xd4, 0xe2);
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = Color.FromArgb(0x94, 0x94, 0x94);
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = Color.FromArgb(0x94, 0x94, 0x94);
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = Color.FromArgb(0xab, 0xa9, 0xc2);
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = Color.FromArgb(0xab, 0xa9, 0xc2);
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseDown] = Color.FromArgb(0xfe, 0x80, 0x3e);
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseOver] = Color.FromArgb(0xff, 0xee, 0xc2);
            rgbTable[KnownColors.msocbvcrDWTabTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrDWTabTextMouseOver] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = Color.FromArgb(0xe0, 0xdf, 0xe3);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = Color.FromArgb(0xe0, 0xdf, 0xe3);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrGDHeaderBdr] = Color.FromArgb(0xbf, 0xbf, 0xdf);
            rgbTable[KnownColors.msocbvcrGDHeaderBkgd] = Color.FromArgb(0xef, 0xeb, 0xde);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBdr] = Color.FromArgb(0x7e, 0x7d, 0x68);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgd] = Color.FromArgb(0xdf, 0xdf, 0xea);
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgdSelected] = Color.FromArgb(0xff, 0xc0, 0x6f);
            rgbTable[KnownColors.msocbvcrGDHeaderSeeThroughSelection] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = Color.FromArgb(0xa2, 0xa2, 0xb5);
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = Color.FromArgb(0xa2, 0xa2, 0xb5);
            rgbTable[KnownColors.msocbvcrGSPGroupContentDarkBkgd] = Color.FromArgb(0xd4, 0xd5, 0xe5);
            rgbTable[KnownColors.msocbvcrGSPGroupContentLightBkgd] = Color.FromArgb(0xe3, 0xe3, 0xec);
            rgbTable[KnownColors.msocbvcrGSPGroupContentText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrGSPGroupContentTextDisabled] = Color.FromArgb(150, 0x91, 0x85);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderDarkBkgd] = Color.FromArgb(0xa9, 0xa8, 0xbf);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderLightBkgd] = Color.FromArgb(0xd0, 0xd0, 0xdf);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = Color.FromArgb(0x5c, 0x5b, 0x79);
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = Color.FromArgb(0x5c, 0x5b, 0x79);
            rgbTable[KnownColors.msocbvcrGSPGroupline] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrGSPGroupline] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrGSPHyperlink] = Color.FromArgb(0, 0x3d, 0xb2);
            rgbTable[KnownColors.msocbvcrGSPLightBkgd] = Color.FromArgb(0xee, 0xee, 0xf4);
            rgbTable[KnownColors.msocbvcrHyperlink] = Color.FromArgb(0, 0x3d, 0xb2);
            rgbTable[KnownColors.msocbvcrHyperlinkFollowed] = Color.FromArgb(170, 0, 170);
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = Color.FromArgb(0xd7, 0xd7, 0xe5);
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = Color.FromArgb(0xd7, 0xd7, 0xe5);
            rgbTable[KnownColors.msocbvcrJotNavUIGradEnd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = Color.FromArgb(0xf3, 0xf3, 0xf7);
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = Color.FromArgb(0xf3, 0xf3, 0xf7);
            rgbTable[KnownColors.msocbvcrJotNavUIText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrListHeaderArrow] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrNetLookBkgnd] = Color.FromArgb(0xf9, 0xf9, 0xff);
            rgbTable[KnownColors.msocbvcrOABBkgd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOBBkgdBdr] = Color.FromArgb(0xd3, 0xd3, 0xd3);
            rgbTable[KnownColors.msocbvcrOBBkgdBdrContrast] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrOGMDIParentWorkspaceBkgd] = Color.FromArgb(0x9b, 0x9a, 0xb3);
            rgbTable[KnownColors.msocbvcrOGRulerActiveBkgd] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOGRulerBdr] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrOGRulerBkgd] = Color.FromArgb(0xdf, 0xdf, 0xea);
            rgbTable[KnownColors.msocbvcrOGRulerInactiveBkgd] = Color.FromArgb(0xb1, 0xb0, 0xc3);
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdr] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdrHighlight] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOGRulerTabStopTicks] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrOGRulerText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrOGTaskPaneGroupBoxHeaderBkgd] = Color.FromArgb(0xd4, 0xd4, 0xe2);
            rgbTable[KnownColors.msocbvcrOGWorkspaceBkgd] = Color.FromArgb(0x9b, 0x9a, 0xb3);
            rgbTable[KnownColors.msocbvcrOLKFlagNone] = Color.FromArgb(0xef, 0xef, 0xf4);
            rgbTable[KnownColors.msocbvcrOLKFolderbarDark] = Color.FromArgb(110, 0x6d, 0x8f);
            rgbTable[KnownColors.msocbvcrOLKFolderbarLight] = Color.FromArgb(0xa8, 0xa7, 0xbf);
            rgbTable[KnownColors.msocbvcrOLKFolderbarText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKGridlines] = Color.FromArgb(0xea, 0xe9, 0xe1);
            rgbTable[KnownColors.msocbvcrOLKGroupLine] = Color.FromArgb(0xa5, 0xa4, 0xbd);
            rgbTable[KnownColors.msocbvcrOLKGroupNested] = Color.FromArgb(0xfd, 0xee, 0xc9);
            rgbTable[KnownColors.msocbvcrOLKGroupShaded] = Color.FromArgb(0xe5, 0xe5, 0xeb);
            rgbTable[KnownColors.msocbvcrOLKGroupText] = Color.FromArgb(0x70, 0x6f, 0x91);
            rgbTable[KnownColors.msocbvcrOLKIconBar] = Color.FromArgb(0xfd, 0xf7, 0xe9);
            rgbTable[KnownColors.msocbvcrOLKInfoBarBkgd] = Color.FromArgb(0x9b, 0x9a, 0xb3);
            rgbTable[KnownColors.msocbvcrOLKInfoBarText] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKPreviewPaneLabelText] = Color.FromArgb(0x9b, 0x9a, 0xb3);
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorDark] = Color.FromArgb(0xbb, 0x55, 3);
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorLight] = Color.FromArgb(0xfb, 200, 0x4f);
            rgbTable[KnownColors.msocbvcrOLKWBActionDividerLine] = Color.FromArgb(0xcc, 0xce, 0xdb);
            rgbTable[KnownColors.msocbvcrOLKWBButtonDark] = Color.FromArgb(0x93, 0x91, 0xb0);
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = Color.FromArgb(0xe1, 0xe2, 0xec);
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = Color.FromArgb(0xe1, 0xe2, 0xec);
            rgbTable[KnownColors.msocbvcrOLKWBDarkOutline] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrOLKWBFoldersBackground] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonDark] = Color.FromArgb(0xf7, 190, 0x57);
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonLight] = Color.FromArgb(0xff, 0xff, 220);
            rgbTable[KnownColors.msocbvcrOLKWBLabelText] = Color.FromArgb(50, 0x45, 0x69);
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonDark] = Color.FromArgb(0xf8, 0xde, 0x80);
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonLight] = Color.FromArgb(0xe8, 0x7f, 8);
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonDark] = Color.FromArgb(0xee, 0x93, 0x11);
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonLight] = Color.FromArgb(0xfb, 230, 0x94);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterDark] = Color.FromArgb(110, 0x6d, 0x8f);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = Color.FromArgb(0xa8, 0xa7, 0xbf);
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = Color.FromArgb(0xa8, 0xa7, 0xbf);
            rgbTable[KnownColors.msocbvcrPlacesBarBkgd] = Color.FromArgb(0xe0, 0xdf, 0xe3);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabAreaBkgd] = Color.FromArgb(0xf3, 0xf3, 0xf7);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabBdr] = Color.FromArgb(0x7c, 0x7c, 0x94);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabInactiveBkgd] = Color.FromArgb(0xd7, 0xd7, 0xe5);
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelected] = Color.FromArgb(0x8e, 0x8e, 170);
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelectedMouseOver] = Color.FromArgb(0x8e, 0x8e, 170);
            rgbTable[KnownColors.msocbvcrPPSlideBdrInactiveSelected] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrPPSlideBdrMouseOver] = Color.FromArgb(0x8e, 0x8e, 170);
            rgbTable[KnownColors.msocbvcrPubPrintDocScratchPageBkgd] = Color.FromArgb(0x9b, 0x9a, 0xb3);
            rgbTable[KnownColors.msocbvcrPubWebDocScratchPageBkgd] = Color.FromArgb(0xc3, 0xc3, 210);
            rgbTable[KnownColors.msocbvcrSBBdr] = Color.FromArgb(0xec, 0xea, 0xda);
            rgbTable[KnownColors.msocbvcrScrollbarBkgd] = Color.FromArgb(0xf7, 0xf7, 0xf9);
            rgbTable[KnownColors.msocbvcrToastGradBegin] = Color.FromArgb(0xef, 0xef, 0xf7);
            rgbTable[KnownColors.msocbvcrToastGradEnd] = Color.FromArgb(0xb3, 0xb2, 0xcc);
            rgbTable[KnownColors.msocbvcrWPBdrInnerDocked] = Color.FromArgb(0xff, 0xff, 0xff);
            rgbTable[KnownColors.msocbvcrWPBdrOuterDocked] = Color.FromArgb(0xf3, 0xf3, 0xf7);
            rgbTable[KnownColors.msocbvcrWPBdrOuterFloating] = Color.FromArgb(0x7a, 0x79, 0x99);
            rgbTable[KnownColors.msocbvcrWPBkgd] = Color.FromArgb(0xee, 0xee, 0xf4);
            rgbTable[KnownColors.msocbvcrWPCtlBdr] = Color.FromArgb(0xa5, 0xac, 0xb2);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlBdrDisabled] = Color.FromArgb(0x80, 0x80, 0x80);
            rgbTable[KnownColors.msocbvcrWPCtlBkgd] = Color.FromArgb(0xc0, 0xc0, 0xd3);
            rgbTable[KnownColors.msocbvcrWPCtlBkgdDisabled] = Color.FromArgb(0xde, 0xde, 0xde);
            rgbTable[KnownColors.msocbvcrWPCtlText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPCtlTextDisabled] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrWPCtlTextMouseDown] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPGroupline] = Color.FromArgb(0xa1, 160, 0xbb);
            rgbTable[KnownColors.msocbvcrWPInfoTipBkgd] = Color.FromArgb(0xff, 0xff, 0xcc);
            rgbTable[KnownColors.msocbvcrWPInfoTipText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPNavBarBkgnd] = Color.FromArgb(0x7a, 0x79, 0x99);
            rgbTable[KnownColors.msocbvcrWPText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPText] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPTextDisabled] = Color.FromArgb(0xac, 0xa8, 0x99);
            rgbTable[KnownColors.msocbvcrWPTitleBkgdActive] = Color.FromArgb(0xb8, 0xbc, 0xea);
            rgbTable[KnownColors.msocbvcrWPTitleBkgdInactive] = Color.FromArgb(0xc6, 0xc6, 0xd9);
            rgbTable[KnownColors.msocbvcrWPTitleTextActive] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrWPTitleTextInactive] = Color.FromArgb(0, 0, 0);
            rgbTable[KnownColors.msocbvcrXLFormulaBarBkgd] = Color.FromArgb(0xd7, 0xd7, 0xe5);
        }

        internal void InitSystemColors(ref Dictionary<KnownColors, Color> rgbTable)
        {
            this.usingSystemColors = true;
            this.InitCommonColors(ref rgbTable);
            Color buttonFace = SystemColors.ButtonFace;
            Color buttonShadow = SystemColors.ButtonShadow;
            Color highlight = SystemColors.Highlight;
            Color window = SystemColors.Window;
            Color empty = Color.Empty;
            Color controlText = SystemColors.ControlText;
            Color buttonHighlight = SystemColors.ButtonHighlight;
            Color grayText = SystemColors.GrayText;
            Color highlightText = SystemColors.HighlightText;
            Color windowText = SystemColors.WindowText;
            Color color11 = buttonFace;
            Color color12 = buttonFace;
            Color color13 = buttonFace;
            Color color14 = highlight;
            Color color15 = highlight;
            bool lowResolution = DisplayInformation.LowResolution;
            bool highContrast = DisplayInformation.HighContrast;
            if (lowResolution)
            {
                color14 = window;
            }
            else if (!highContrast)
            {
                color11 = GetAlphaBlendedColorHighRes(null, buttonFace, window, 0x17);
                color12 = GetAlphaBlendedColorHighRes(null, buttonFace, window, 50);
                color13 = SystemColors.ButtonFace;
                color14 = GetAlphaBlendedColorHighRes(null, highlight, window, 30);
                color15 = GetAlphaBlendedColorHighRes(null, highlight, window, 50);
            }
            if (lowResolution || highContrast)
            {
                rgbTable[KnownColors.msocbvcrCBBkgd] = buttonFace;
                rgbTable[KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver] = SystemColors.ControlLight;
                rgbTable[KnownColors.msocbvcrCBDragHandle] = controlText;
                rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzEnd] = buttonFace;
                rgbTable[KnownColors.msocbvcrCBGradOptionsBegin] = buttonShadow;
                rgbTable[KnownColors.msocbvcrCBGradOptionsMiddle] = buttonShadow;
                rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedBegin] = buttonShadow;
                rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle] = buttonShadow;
                rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedEnd] = buttonShadow;
                rgbTable[KnownColors.msocbvcrCBMenuBdrOuter] = controlText;
                rgbTable[KnownColors.msocbvcrCBMenuBkgd] = window;
                rgbTable[KnownColors.msocbvcrCBSplitterLine] = buttonShadow;
            }
            else
            {
                rgbTable[KnownColors.msocbvcrCBBkgd] = GetAlphaBlendedColorHighRes(null, window, buttonFace, 0xa5);
                rgbTable[KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver] = GetAlphaBlendedColorHighRes(null, highlight, window, 50);
                rgbTable[KnownColors.msocbvcrCBDragHandle] = GetAlphaBlendedColorHighRes(null, buttonShadow, window, 0x4b);
                rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzEnd] = GetAlphaBlendedColorHighRes(null, buttonFace, window, 0xcd);
                rgbTable[KnownColors.msocbvcrCBGradOptionsBegin] = GetAlphaBlendedColorHighRes(null, buttonFace, window, 70);
                rgbTable[KnownColors.msocbvcrCBGradOptionsMiddle] = GetAlphaBlendedColorHighRes(null, buttonFace, window, 90);
                rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedBegin] = GetAlphaBlendedColorHighRes(null, buttonFace, window, 40);
                rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle] = GetAlphaBlendedColorHighRes(null, buttonFace, window, 70);
                rgbTable[KnownColors.msocbvcrCBGradMenuIconBkgdDroppedEnd] = GetAlphaBlendedColorHighRes(null, buttonFace, window, 90);
                rgbTable[KnownColors.msocbvcrCBMenuBdrOuter] = GetAlphaBlendedColorHighRes(null, controlText, buttonShadow, 20);
                rgbTable[KnownColors.msocbvcrCBMenuBkgd] = GetAlphaBlendedColorHighRes(null, buttonFace, window, 0x8f);
                rgbTable[KnownColors.msocbvcrCBSplitterLine] = GetAlphaBlendedColorHighRes(null, buttonShadow, window, 70);
            }
            rgbTable[KnownColors.msocbvcrCBCtlBkgdSelected] = lowResolution ? SystemColors.ControlLight : highlight;
            rgbTable[KnownColors.msocbvcrCBBdrOuterDocked] = buttonFace;
            rgbTable[KnownColors.msocbvcrCBBdrOuterDocked] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBBdrOuterFloating] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseDown] = highlight;
            rgbTable[KnownColors.msocbvcrCBCtlBdrMouseOver] = highlight;
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelected] = highlight;
            rgbTable[KnownColors.msocbvcrCBCtlBdrSelectedMouseOver] = highlight;
            rgbTable[KnownColors.msocbvcrCBCtlBkgd] = empty;
            rgbTable[KnownColors.msocbvcrCBCtlBkgdLight] = window;
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseDown] = highlight;
            rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver] = window;
            rgbTable[KnownColors.msocbvcrCBCtlText] = controlText;
            rgbTable[KnownColors.msocbvcrCBCtlTextDisabled] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBCtlTextLight] = grayText;
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseDown] = highlightText;
            rgbTable[KnownColors.msocbvcrCBCtlTextMouseOver] = windowText;
            rgbTable[KnownColors.msocbvcrCBDockSeparatorLine] = empty;
            rgbTable[KnownColors.msocbvcrCBDragHandleShadow] = window;
            rgbTable[KnownColors.msocbvcrCBDropDownArrow] = empty;
            rgbTable[KnownColors.msocbvcrCBGradMainMenuHorzBegin] = buttonFace;
            rgbTable[KnownColors.msocbvcrCBGradMouseOverEnd] = color14;
            rgbTable[KnownColors.msocbvcrCBGradMouseOverBegin] = color14;
            rgbTable[KnownColors.msocbvcrCBGradMouseOverMiddle] = color14;
            rgbTable[KnownColors.msocbvcrCBGradOptionsEnd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverBegin] = empty;
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverEnd] = empty;
            rgbTable[KnownColors.msocbvcrCBGradOptionsMouseOverMiddle] = empty;
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedBegin] = empty;
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedEnd] = empty;
            rgbTable[KnownColors.msocbvcrCBGradOptionsSelectedMiddle] = empty;
            rgbTable[KnownColors.msocbvcrCBGradSelectedBegin] = empty;
            rgbTable[KnownColors.msocbvcrCBGradSelectedEnd] = empty;
            rgbTable[KnownColors.msocbvcrCBGradSelectedMiddle] = empty;
            rgbTable[KnownColors.msocbvcrCBGradVertBegin] = color11;
            rgbTable[KnownColors.msocbvcrCBGradVertMiddle] = color12;
            rgbTable[KnownColors.msocbvcrCBGradVertEnd] = color13;
            rgbTable[KnownColors.msocbvcrCBGradMouseDownBegin] = color15;
            rgbTable[KnownColors.msocbvcrCBGradMouseDownMiddle] = color15;
            rgbTable[KnownColors.msocbvcrCBGradMouseDownEnd] = color15;
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdBegin] = color11;
            rgbTable[KnownColors.msocbvcrCBGradMenuTitleBkgdEnd] = color12;
            rgbTable[KnownColors.msocbvcrCBIconDisabledDark] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBIconDisabledLight] = buttonFace;
            rgbTable[KnownColors.msocbvcrCBLabelBkgnd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBLowColorIconDisabled] = empty;
            rgbTable[KnownColors.msocbvcrCBMainMenuBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrCBMenuCtlText] = windowText;
            rgbTable[KnownColors.msocbvcrCBMenuCtlTextDisabled] = grayText;
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgd] = empty;
            rgbTable[KnownColors.msocbvcrCBMenuIconBkgdDropped] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBMenuShadow] = empty;
            rgbTable[KnownColors.msocbvcrCBMenuSplitArrow] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBOptionsButtonShadow] = empty;
            rgbTable[KnownColors.msocbvcrCBShadow] = rgbTable[KnownColors.msocbvcrCBBkgd];
            rgbTable[KnownColors.msocbvcrCBSplitterLineLight] = buttonHighlight;
            rgbTable[KnownColors.msocbvcrCBTearOffHandle] = empty;
            rgbTable[KnownColors.msocbvcrCBTearOffHandleMouseOver] = empty;
            rgbTable[KnownColors.msocbvcrCBTitleBkgd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrCBTitleText] = buttonHighlight;
            rgbTable[KnownColors.msocbvcrDisabledFocuslessHighlightedText] = grayText;
            rgbTable[KnownColors.msocbvcrDisabledHighlightedText] = grayText;
            rgbTable[KnownColors.msocbvcrDlgGroupBoxText] = controlText;
            rgbTable[KnownColors.msocbvcrDocTabBdr] = buttonShadow;
            rgbTable[KnownColors.msocbvcrDocTabBdrDark] = buttonFace;
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseDown] = highlight;
            rgbTable[KnownColors.msocbvcrDocTabBdrDarkMouseOver] = SystemColors.MenuText;
            rgbTable[KnownColors.msocbvcrDocTabBdrLight] = buttonFace;
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseDown] = highlight;
            rgbTable[KnownColors.msocbvcrDocTabBdrLightMouseOver] = SystemColors.MenuText;
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseDown] = highlight;
            rgbTable[KnownColors.msocbvcrDocTabBdrMouseOver] = SystemColors.MenuText;
            rgbTable[KnownColors.msocbvcrDocTabBdrSelected] = buttonShadow;
            rgbTable[KnownColors.msocbvcrDocTabBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseDown] = highlight;
            rgbTable[KnownColors.msocbvcrDocTabBkgdMouseOver] = highlight;
            rgbTable[KnownColors.msocbvcrDocTabBkgdSelected] = window;
            rgbTable[KnownColors.msocbvcrDocTabText] = controlText;
            rgbTable[KnownColors.msocbvcrDocTabTextMouseDown] = highlightText;
            rgbTable[KnownColors.msocbvcrDocTabTextMouseOver] = highlight;
            rgbTable[KnownColors.msocbvcrDocTabTextSelected] = windowText;
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrDWActiveTabBkgd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = buttonFace;
            rgbTable[KnownColors.msocbvcrDWActiveTabText] = controlText;
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = buttonShadow;
            rgbTable[KnownColors.msocbvcrDWActiveTabTextDisabled] = controlText;
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrDWInactiveTabBkgd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = buttonHighlight;
            rgbTable[KnownColors.msocbvcrDWInactiveTabText] = controlText;
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseDown] = buttonFace;
            rgbTable[KnownColors.msocbvcrDWTabBkgdMouseOver] = buttonFace;
            rgbTable[KnownColors.msocbvcrDWTabTextMouseDown] = controlText;
            rgbTable[KnownColors.msocbvcrDWTabTextMouseOver] = controlText;
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedBkgd] = SystemColors.InactiveCaption;
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = controlText;
            rgbTable[KnownColors.msocbvcrFocuslessHighlightedText] = SystemColors.InactiveCaptionText;
            rgbTable[KnownColors.msocbvcrGDHeaderBdr] = highlight;
            rgbTable[KnownColors.msocbvcrGDHeaderBkgd] = window;
            rgbTable[KnownColors.msocbvcrGDHeaderCellBdr] = buttonShadow;
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrGDHeaderCellBkgdSelected] = empty;
            rgbTable[KnownColors.msocbvcrGDHeaderSeeThroughSelection] = highlight;
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrGSPDarkBkgd] = window;
            rgbTable[KnownColors.msocbvcrGSPGroupContentDarkBkgd] = window;
            rgbTable[KnownColors.msocbvcrGSPGroupContentLightBkgd] = window;
            rgbTable[KnownColors.msocbvcrGSPGroupContentText] = windowText;
            rgbTable[KnownColors.msocbvcrGSPGroupContentTextDisabled] = grayText;
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderDarkBkgd] = window;
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderLightBkgd] = window;
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = controlText;
            rgbTable[KnownColors.msocbvcrGSPGroupHeaderText] = windowText;
            rgbTable[KnownColors.msocbvcrGSPGroupline] = buttonShadow;
            rgbTable[KnownColors.msocbvcrGSPGroupline] = window;
            rgbTable[KnownColors.msocbvcrGSPHyperlink] = empty;
            rgbTable[KnownColors.msocbvcrGSPLightBkgd] = window;
            rgbTable[KnownColors.msocbvcrHyperlink] = empty;
            rgbTable[KnownColors.msocbvcrHyperlinkFollowed] = empty;
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = buttonShadow;
            rgbTable[KnownColors.msocbvcrJotNavUIBdr] = windowText;
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = buttonFace;
            rgbTable[KnownColors.msocbvcrJotNavUIGradBegin] = window;
            rgbTable[KnownColors.msocbvcrJotNavUIGradEnd] = window;
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = buttonFace;
            rgbTable[KnownColors.msocbvcrJotNavUIGradMiddle] = window;
            rgbTable[KnownColors.msocbvcrJotNavUIText] = windowText;
            rgbTable[KnownColors.msocbvcrListHeaderArrow] = controlText;
            rgbTable[KnownColors.msocbvcrNetLookBkgnd] = empty;
            rgbTable[KnownColors.msocbvcrOABBkgd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOBBkgdBdr] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOBBkgdBdrContrast] = window;
            rgbTable[KnownColors.msocbvcrOGMDIParentWorkspaceBkgd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOGRulerActiveBkgd] = window;
            rgbTable[KnownColors.msocbvcrOGRulerBdr] = controlText;
            rgbTable[KnownColors.msocbvcrOGRulerBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrOGRulerInactiveBkgd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdr] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOGRulerTabBoxBdrHighlight] = buttonHighlight;
            rgbTable[KnownColors.msocbvcrOGRulerTabStopTicks] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOGRulerText] = windowText;
            rgbTable[KnownColors.msocbvcrOGTaskPaneGroupBoxHeaderBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrOGWorkspaceBkgd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKFlagNone] = buttonHighlight;
            rgbTable[KnownColors.msocbvcrOLKFolderbarDark] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKFolderbarLight] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKFolderbarText] = window;
            rgbTable[KnownColors.msocbvcrOLKGridlines] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKGroupLine] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKGroupNested] = buttonFace;
            rgbTable[KnownColors.msocbvcrOLKGroupShaded] = buttonFace;
            rgbTable[KnownColors.msocbvcrOLKGroupText] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKIconBar] = buttonFace;
            rgbTable[KnownColors.msocbvcrOLKInfoBarBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrOLKInfoBarText] = controlText;
            rgbTable[KnownColors.msocbvcrOLKPreviewPaneLabelText] = windowText;
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorDark] = highlight;
            rgbTable[KnownColors.msocbvcrOLKTodayIndicatorLight] = buttonFace;
            rgbTable[KnownColors.msocbvcrOLKWBActionDividerLine] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKWBButtonDark] = buttonFace;
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = buttonFace;
            rgbTable[KnownColors.msocbvcrOLKWBButtonLight] = buttonHighlight;
            rgbTable[KnownColors.msocbvcrOLKWBDarkOutline] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKWBFoldersBackground] = window;
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonDark] = empty;
            rgbTable[KnownColors.msocbvcrOLKWBHoverButtonLight] = empty;
            rgbTable[KnownColors.msocbvcrOLKWBLabelText] = windowText;
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonDark] = empty;
            rgbTable[KnownColors.msocbvcrOLKWBPressedButtonLight] = empty;
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonDark] = empty;
            rgbTable[KnownColors.msocbvcrOLKWBSelectedButtonLight] = empty;
            rgbTable[KnownColors.msocbvcrOLKWBSplitterDark] = buttonShadow;
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = buttonFace;
            rgbTable[KnownColors.msocbvcrOLKWBSplitterLight] = buttonShadow;
            rgbTable[KnownColors.msocbvcrPlacesBarBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabAreaBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabBdr] = buttonShadow;
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabInactiveBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrPPOutlineThumbnailsPaneTabText] = windowText;
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelected] = highlight;
            rgbTable[KnownColors.msocbvcrPPSlideBdrActiveSelectedMouseOver] = highlight;
            rgbTable[KnownColors.msocbvcrPPSlideBdrInactiveSelected] = grayText;
            rgbTable[KnownColors.msocbvcrPPSlideBdrMouseOver] = highlight;
            rgbTable[KnownColors.msocbvcrPubPrintDocScratchPageBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrPubWebDocScratchPageBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrSBBdr] = buttonShadow;
            rgbTable[KnownColors.msocbvcrScrollbarBkgd] = buttonShadow;
            rgbTable[KnownColors.msocbvcrToastGradBegin] = buttonFace;
            rgbTable[KnownColors.msocbvcrToastGradEnd] = buttonFace;
            rgbTable[KnownColors.msocbvcrWPBdrInnerDocked] = empty;
            rgbTable[KnownColors.msocbvcrWPBdrOuterDocked] = buttonFace;
            rgbTable[KnownColors.msocbvcrWPBdrOuterFloating] = buttonShadow;
            rgbTable[KnownColors.msocbvcrWPBkgd] = window;
            rgbTable[KnownColors.msocbvcrWPCtlBdr] = buttonShadow;
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = buttonShadow;
            rgbTable[KnownColors.msocbvcrWPCtlBdrDefault] = controlText;
            rgbTable[KnownColors.msocbvcrWPCtlBdrDisabled] = buttonShadow;
            rgbTable[KnownColors.msocbvcrWPCtlBkgd] = buttonFace;
            rgbTable[KnownColors.msocbvcrWPCtlBkgdDisabled] = buttonFace;
            rgbTable[KnownColors.msocbvcrWPCtlText] = controlText;
            rgbTable[KnownColors.msocbvcrWPCtlTextDisabled] = buttonShadow;
            rgbTable[KnownColors.msocbvcrWPCtlTextMouseDown] = highlightText;
            rgbTable[KnownColors.msocbvcrWPGroupline] = buttonShadow;
            rgbTable[KnownColors.msocbvcrWPInfoTipBkgd] = SystemColors.Info;
            rgbTable[KnownColors.msocbvcrWPInfoTipText] = SystemColors.InfoText;
            rgbTable[KnownColors.msocbvcrWPNavBarBkgnd] = buttonFace;
            rgbTable[KnownColors.msocbvcrWPText] = controlText;
            rgbTable[KnownColors.msocbvcrWPText] = windowText;
            rgbTable[KnownColors.msocbvcrWPTextDisabled] = grayText;
            rgbTable[KnownColors.msocbvcrWPTitleBkgdActive] = highlight;
            rgbTable[KnownColors.msocbvcrWPTitleBkgdInactive] = buttonFace;
            rgbTable[KnownColors.msocbvcrWPTitleTextActive] = highlightText;
            rgbTable[KnownColors.msocbvcrWPTitleTextInactive] = controlText;
            rgbTable[KnownColors.msocbvcrXLFormulaBarBkgd] = buttonFace;
        }

        internal void InitThemedColors(ref Dictionary<KnownColors, Color> rgbTable)
        {
            string colorScheme = VisualStyleInformation.ColorScheme;
            string fileName = Path.GetFileName(VisualStyleInformation.ThemeFilename);
            bool flag = false;
            if (string.Equals("luna.msstyles", fileName, StringComparison.OrdinalIgnoreCase))
            {
                switch (colorScheme)
                {
                    case "NormalColor":
                        this.InitBlueLunaColors(ref rgbTable);
                        this.usingSystemColors = false;
                        flag = true;
                        break;

                    case "HomeStead":
                        this.InitOliveLunaColors(ref rgbTable);
                        this.usingSystemColors = false;
                        flag = true;
                        break;

                    case "Metallic":
                        this.InitSilverLunaColors(ref rgbTable);
                        this.usingSystemColors = false;
                        flag = true;
                        break;
                }
            }
            else if (string.Equals("aero.msstyles", fileName, StringComparison.OrdinalIgnoreCase))
            {
                this.InitSystemColors(ref rgbTable);
                this.usingSystemColors = true;
                flag = true;
                rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver] = rgbTable[KnownColors.ButtonSelectedHighlight];
                rgbTable[KnownColors.msocbvcrCBCtlBkgdSelected] = rgbTable[KnownColors.msocbvcrCBCtlBkgdMouseOver];
            }
            else if (string.Equals("royale.msstyles", fileName, StringComparison.OrdinalIgnoreCase) && ((colorScheme == "NormalColor") || (colorScheme == "Royale")))
            {
                this.InitRoyaleColors(ref rgbTable);
                this.usingSystemColors = false;
                flag = true;
            }
            if (!flag)
            {
                this.InitSystemColors(ref rgbTable);
                this.usingSystemColors = true;
            }
            this.InitCommonColors(ref rgbTable);
        }

        private void ResetRGBTable()
        {
            if (this.professionalRGB != null)
            {
                this.professionalRGB.Clear();
            }
            this.professionalRGB = null;
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedGradientBeginDescr")]
        public virtual Color ButtonCheckedGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradSelectedBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedGradientEndDescr")]
        public virtual Color ButtonCheckedGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradSelectedEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedGradientMiddleDescr")]
        public virtual Color ButtonCheckedGradientMiddle
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradSelectedMiddle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedHighlightDescr")]
        public virtual Color ButtonCheckedHighlight
        {
            get
            {
                return this.FromKnownColor(KnownColors.ButtonCheckedHighlight);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedHighlightBorderDescr")]
        public virtual Color ButtonCheckedHighlightBorder
        {
            get
            {
                return SystemColors.Highlight;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedBorderDescr")]
        public virtual Color ButtonPressedBorder
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBCtlBdrMouseOver);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedGradientBeginDescr")]
        public virtual Color ButtonPressedGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMouseDownBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedGradientEndDescr")]
        public virtual Color ButtonPressedGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMouseDownEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedGradientMiddleDescr")]
        public virtual Color ButtonPressedGradientMiddle
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMouseDownMiddle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedHighlightDescr")]
        public virtual Color ButtonPressedHighlight
        {
            get
            {
                return this.FromKnownColor(KnownColors.ButtonPressedHighlight);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedHighlightBorderDescr")]
        public virtual Color ButtonPressedHighlightBorder
        {
            get
            {
                return SystemColors.Highlight;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedBorderDescr")]
        public virtual Color ButtonSelectedBorder
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBCtlBdrMouseOver);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedGradientBeginDescr")]
        public virtual Color ButtonSelectedGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMouseOverBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedGradientEndDescr")]
        public virtual Color ButtonSelectedGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMouseOverEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedGradientMiddleDescr")]
        public virtual Color ButtonSelectedGradientMiddle
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMouseOverMiddle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedHighlightDescr")]
        public virtual Color ButtonSelectedHighlight
        {
            get
            {
                return this.FromKnownColor(KnownColors.ButtonSelectedHighlight);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedHighlightBorderDescr")]
        public virtual Color ButtonSelectedHighlightBorder
        {
            get
            {
                return this.ButtonPressedBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsCheckBackgroundDescr")]
        public virtual Color CheckBackground
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBCtlBkgdSelected);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsCheckPressedBackgroundDescr")]
        public virtual Color CheckPressedBackground
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsCheckSelectedBackgroundDescr")]
        public virtual Color CheckSelectedBackground
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver);
            }
        }

        private Dictionary<KnownColors, Color> ColorTable
        {
            get
            {
                if (this.UseSystemColors)
                {
                    if (!this.usingSystemColors || (this.professionalRGB == null))
                    {
                        if (this.professionalRGB == null)
                        {
                            this.professionalRGB = new Dictionary<KnownColors, Color>(0xd4);
                        }
                        this.InitSystemColors(ref this.professionalRGB);
                    }
                }
                else if (ToolStripManager.VisualStylesEnabled)
                {
                    if (this.usingSystemColors || (this.professionalRGB == null))
                    {
                        if (this.professionalRGB == null)
                        {
                            this.professionalRGB = new Dictionary<KnownColors, Color>(0xd4);
                        }
                        this.InitThemedColors(ref this.professionalRGB);
                    }
                }
                else if (!this.usingSystemColors || (this.professionalRGB == null))
                {
                    if (this.professionalRGB == null)
                    {
                        this.professionalRGB = new Dictionary<KnownColors, Color>(0xd4);
                    }
                    this.InitSystemColors(ref this.professionalRGB);
                }
                return this.professionalRGB;
            }
        }

        internal Color ComboBoxBorder
        {
            get
            {
                return this.ButtonSelectedHighlightBorder;
            }
        }

        internal Color ComboBoxButtonGradientBegin
        {
            get
            {
                return this.MenuItemPressedGradientBegin;
            }
        }

        internal Color ComboBoxButtonGradientEnd
        {
            get
            {
                return this.MenuItemPressedGradientEnd;
            }
        }

        internal Color ComboBoxButtonOnOverflow
        {
            get
            {
                return this.ToolStripDropDownBackground;
            }
        }

        internal Color ComboBoxButtonPressedGradientBegin
        {
            get
            {
                return this.ButtonPressedGradientBegin;
            }
        }

        internal Color ComboBoxButtonPressedGradientEnd
        {
            get
            {
                return this.ButtonPressedGradientEnd;
            }
        }

        internal Color ComboBoxButtonSelectedGradientBegin
        {
            get
            {
                return this.MenuItemSelectedGradientBegin;
            }
        }

        internal Color ComboBoxButtonSelectedGradientEnd
        {
            get
            {
                return this.MenuItemSelectedGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsGripDarkDescr")]
        public virtual Color GripDark
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBDragHandle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsGripLightDescr")]
        public virtual Color GripLight
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBDragHandleShadow);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginGradientBeginDescr")]
        public virtual Color ImageMarginGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradVertBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginGradientEndDescr")]
        public virtual Color ImageMarginGradientEnd
        {
            get
            {
                if (!this.usingSystemColors)
                {
                    return this.FromKnownColor(KnownColors.msocbvcrCBGradVertEnd);
                }
                return SystemColors.Control;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginGradientMiddleDescr")]
        public virtual Color ImageMarginGradientMiddle
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradVertMiddle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginRevealedGradientBeginDescr")]
        public virtual Color ImageMarginRevealedGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMenuIconBkgdDroppedBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginRevealedGradientEndDescr")]
        public virtual Color ImageMarginRevealedGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMenuIconBkgdDroppedEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginRevealedGradientMiddleDescr")]
        public virtual Color ImageMarginRevealedGradientMiddle
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuBorderDescr")]
        public virtual Color MenuBorder
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBMenuBdrOuter);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemBorderDescr")]
        public virtual Color MenuItemBorder
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBCtlBdrSelected);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemPressedGradientBeginDescr")]
        public virtual Color MenuItemPressedGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMenuTitleBkgdBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemPressedGradientEndDescr")]
        public virtual Color MenuItemPressedGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMenuTitleBkgdEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemPressedGradientMiddleDescr")]
        public virtual Color MenuItemPressedGradientMiddle
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemSelectedDescr")]
        public virtual Color MenuItemSelected
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBCtlBkgdMouseOver);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemSelectedGradientBeginDescr")]
        public virtual Color MenuItemSelectedGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMouseOverBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemSelectedGradientEndDescr")]
        public virtual Color MenuItemSelectedGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMouseOverEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuStripGradientBeginDescr")]
        public virtual Color MenuStripGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuStripGradientEndDescr")]
        public virtual Color MenuStripGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsOverflowButtonGradientBeginDescr")]
        public virtual Color OverflowButtonGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradOptionsBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsOverflowButtonGradientEndDescr")]
        public virtual Color OverflowButtonGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradOptionsEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsOverflowButtonGradientMiddleDescr")]
        public virtual Color OverflowButtonGradientMiddle
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradOptionsMiddle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsRaftingContainerGradientBeginDescr")]
        public virtual Color RaftingContainerGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsRaftingContainerGradientEndDescr")]
        public virtual Color RaftingContainerGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsSeparatorDarkDescr")]
        public virtual Color SeparatorDark
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBSplitterLine);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsSeparatorLightDescr")]
        public virtual Color SeparatorLight
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBSplitterLineLight);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsStatusStripGradientBeginDescr")]
        public virtual Color StatusStripGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsStatusStripGradientEndDescr")]
        public virtual Color StatusStripGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzEnd);
            }
        }

        internal Color TextBoxBorder
        {
            get
            {
                return this.ButtonSelectedHighlightBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripBorderDescr")]
        public virtual Color ToolStripBorder
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBShadow);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripContentPanelGradientBeginDescr")]
        public virtual Color ToolStripContentPanelGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripContentPanelGradientEndDescr")]
        public virtual Color ToolStripContentPanelGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripDropDownBackgroundDescr")]
        public virtual Color ToolStripDropDownBackground
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBMenuBkgd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripGradientBeginDescr")]
        public virtual Color ToolStripGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradVertBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripGradientEndDescr")]
        public virtual Color ToolStripGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradVertEnd);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripGradientMiddleDescr")]
        public virtual Color ToolStripGradientMiddle
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradVertMiddle);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripPanelGradientBeginDescr")]
        public virtual Color ToolStripPanelGradientBegin
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzBegin);
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripPanelGradientEndDescr")]
        public virtual Color ToolStripPanelGradientEnd
        {
            get
            {
                return this.FromKnownColor(KnownColors.msocbvcrCBGradMainMenuHorzEnd);
            }
        }

        public bool UseSystemColors
        {
            get
            {
                return this.useSystemColors;
            }
            set
            {
                if (this.useSystemColors != value)
                {
                    this.useSystemColors = value;
                    this.ResetRGBTable();
                }
            }
        }

        internal enum KnownColors
        {
            ButtonCheckedHighlight = 0xd4,
            ButtonPressedHighlight = 0xd3,
            ButtonSelectedHighlight = 210,
            lastKnownColor = 0xd4,
            msocbvcrCBBdrOuterDocked = 0,
            msocbvcrCBBdrOuterFloating = 1,
            msocbvcrCBBkgd = 2,
            msocbvcrCBCtlBdrMouseDown = 3,
            msocbvcrCBCtlBdrMouseOver = 4,
            msocbvcrCBCtlBdrSelected = 5,
            msocbvcrCBCtlBdrSelectedMouseOver = 6,
            msocbvcrCBCtlBkgd = 7,
            msocbvcrCBCtlBkgdLight = 8,
            msocbvcrCBCtlBkgdMouseDown = 9,
            msocbvcrCBCtlBkgdMouseOver = 10,
            msocbvcrCBCtlBkgdSelected = 11,
            msocbvcrCBCtlBkgdSelectedMouseOver = 12,
            msocbvcrCBCtlText = 13,
            msocbvcrCBCtlTextDisabled = 14,
            msocbvcrCBCtlTextLight = 15,
            msocbvcrCBCtlTextMouseDown = 0x10,
            msocbvcrCBCtlTextMouseOver = 0x11,
            msocbvcrCBDockSeparatorLine = 0x12,
            msocbvcrCBDragHandle = 0x13,
            msocbvcrCBDragHandleShadow = 20,
            msocbvcrCBDropDownArrow = 0x15,
            msocbvcrCBGradMainMenuHorzBegin = 0x16,
            msocbvcrCBGradMainMenuHorzEnd = 0x17,
            msocbvcrCBGradMenuIconBkgdDroppedBegin = 0x18,
            msocbvcrCBGradMenuIconBkgdDroppedEnd = 0x19,
            msocbvcrCBGradMenuIconBkgdDroppedMiddle = 0x1a,
            msocbvcrCBGradMenuTitleBkgdBegin = 0x1b,
            msocbvcrCBGradMenuTitleBkgdEnd = 0x1c,
            msocbvcrCBGradMouseDownBegin = 0x1d,
            msocbvcrCBGradMouseDownEnd = 30,
            msocbvcrCBGradMouseDownMiddle = 0x1f,
            msocbvcrCBGradMouseOverBegin = 0x20,
            msocbvcrCBGradMouseOverEnd = 0x21,
            msocbvcrCBGradMouseOverMiddle = 0x22,
            msocbvcrCBGradOptionsBegin = 0x23,
            msocbvcrCBGradOptionsEnd = 0x24,
            msocbvcrCBGradOptionsMiddle = 0x25,
            msocbvcrCBGradOptionsMouseOverBegin = 0x26,
            msocbvcrCBGradOptionsMouseOverEnd = 0x27,
            msocbvcrCBGradOptionsMouseOverMiddle = 40,
            msocbvcrCBGradOptionsSelectedBegin = 0x29,
            msocbvcrCBGradOptionsSelectedEnd = 0x2a,
            msocbvcrCBGradOptionsSelectedMiddle = 0x2b,
            msocbvcrCBGradSelectedBegin = 0x2c,
            msocbvcrCBGradSelectedEnd = 0x2d,
            msocbvcrCBGradSelectedMiddle = 0x2e,
            msocbvcrCBGradVertBegin = 0x2f,
            msocbvcrCBGradVertEnd = 0x30,
            msocbvcrCBGradVertMiddle = 0x31,
            msocbvcrCBIconDisabledDark = 50,
            msocbvcrCBIconDisabledLight = 0x33,
            msocbvcrCBLabelBkgnd = 0x34,
            msocbvcrCBLowColorIconDisabled = 0x35,
            msocbvcrCBMainMenuBkgd = 0x36,
            msocbvcrCBMenuBdrOuter = 0x37,
            msocbvcrCBMenuBkgd = 0x38,
            msocbvcrCBMenuCtlText = 0x39,
            msocbvcrCBMenuCtlTextDisabled = 0x3a,
            msocbvcrCBMenuIconBkgd = 0x3b,
            msocbvcrCBMenuIconBkgdDropped = 60,
            msocbvcrCBMenuShadow = 0x3d,
            msocbvcrCBMenuSplitArrow = 0x3e,
            msocbvcrCBOptionsButtonShadow = 0x3f,
            msocbvcrCBShadow = 0x40,
            msocbvcrCBSplitterLine = 0x41,
            msocbvcrCBSplitterLineLight = 0x42,
            msocbvcrCBTearOffHandle = 0x43,
            msocbvcrCBTearOffHandleMouseOver = 0x44,
            msocbvcrCBTitleBkgd = 0x45,
            msocbvcrCBTitleText = 70,
            msocbvcrDisabledFocuslessHighlightedText = 0x47,
            msocbvcrDisabledHighlightedText = 0x48,
            msocbvcrDlgGroupBoxText = 0x49,
            msocbvcrDocTabBdr = 0x4a,
            msocbvcrDocTabBdrDark = 0x4b,
            msocbvcrDocTabBdrDarkMouseDown = 0x4c,
            msocbvcrDocTabBdrDarkMouseOver = 0x4d,
            msocbvcrDocTabBdrLight = 0x4e,
            msocbvcrDocTabBdrLightMouseDown = 0x4f,
            msocbvcrDocTabBdrLightMouseOver = 80,
            msocbvcrDocTabBdrMouseDown = 0x51,
            msocbvcrDocTabBdrMouseOver = 0x52,
            msocbvcrDocTabBdrSelected = 0x53,
            msocbvcrDocTabBkgd = 0x54,
            msocbvcrDocTabBkgdMouseDown = 0x55,
            msocbvcrDocTabBkgdMouseOver = 0x56,
            msocbvcrDocTabBkgdSelected = 0x57,
            msocbvcrDocTabText = 0x58,
            msocbvcrDocTabTextMouseDown = 0x59,
            msocbvcrDocTabTextMouseOver = 90,
            msocbvcrDocTabTextSelected = 0x5b,
            msocbvcrDWActiveTabBkgd = 0x5c,
            msocbvcrDWActiveTabText = 0x5d,
            msocbvcrDWActiveTabTextDisabled = 0x5e,
            msocbvcrDWInactiveTabBkgd = 0x5f,
            msocbvcrDWInactiveTabText = 0x60,
            msocbvcrDWTabBkgdMouseDown = 0x61,
            msocbvcrDWTabBkgdMouseOver = 0x62,
            msocbvcrDWTabTextMouseDown = 0x63,
            msocbvcrDWTabTextMouseOver = 100,
            msocbvcrFocuslessHighlightedBkgd = 0x65,
            msocbvcrFocuslessHighlightedText = 0x66,
            msocbvcrGDHeaderBdr = 0x67,
            msocbvcrGDHeaderBkgd = 0x68,
            msocbvcrGDHeaderCellBdr = 0x69,
            msocbvcrGDHeaderCellBkgd = 0x6a,
            msocbvcrGDHeaderCellBkgdSelected = 0x6b,
            msocbvcrGDHeaderSeeThroughSelection = 0x6c,
            msocbvcrGSPDarkBkgd = 0x6d,
            msocbvcrGSPGroupContentDarkBkgd = 110,
            msocbvcrGSPGroupContentLightBkgd = 0x6f,
            msocbvcrGSPGroupContentText = 0x70,
            msocbvcrGSPGroupContentTextDisabled = 0x71,
            msocbvcrGSPGroupHeaderDarkBkgd = 0x72,
            msocbvcrGSPGroupHeaderLightBkgd = 0x73,
            msocbvcrGSPGroupHeaderText = 0x74,
            msocbvcrGSPGroupline = 0x75,
            msocbvcrGSPHyperlink = 0x76,
            msocbvcrGSPLightBkgd = 0x77,
            msocbvcrHyperlink = 120,
            msocbvcrHyperlinkFollowed = 0x79,
            msocbvcrJotNavUIBdr = 0x7a,
            msocbvcrJotNavUIGradBegin = 0x7b,
            msocbvcrJotNavUIGradEnd = 0x7c,
            msocbvcrJotNavUIGradMiddle = 0x7d,
            msocbvcrJotNavUIText = 0x7e,
            msocbvcrListHeaderArrow = 0x7f,
            msocbvcrNetLookBkgnd = 0x80,
            msocbvcrOABBkgd = 0x81,
            msocbvcrOBBkgdBdr = 130,
            msocbvcrOBBkgdBdrContrast = 0x83,
            msocbvcrOGMDIParentWorkspaceBkgd = 0x84,
            msocbvcrOGRulerActiveBkgd = 0x85,
            msocbvcrOGRulerBdr = 0x86,
            msocbvcrOGRulerBkgd = 0x87,
            msocbvcrOGRulerInactiveBkgd = 0x88,
            msocbvcrOGRulerTabBoxBdr = 0x89,
            msocbvcrOGRulerTabBoxBdrHighlight = 0x8a,
            msocbvcrOGRulerTabStopTicks = 0x8b,
            msocbvcrOGRulerText = 140,
            msocbvcrOGTaskPaneGroupBoxHeaderBkgd = 0x8d,
            msocbvcrOGWorkspaceBkgd = 0x8e,
            msocbvcrOLKFlagNone = 0x8f,
            msocbvcrOLKFolderbarDark = 0x90,
            msocbvcrOLKFolderbarLight = 0x91,
            msocbvcrOLKFolderbarText = 0x92,
            msocbvcrOLKGridlines = 0x93,
            msocbvcrOLKGroupLine = 0x94,
            msocbvcrOLKGroupNested = 0x95,
            msocbvcrOLKGroupShaded = 150,
            msocbvcrOLKGroupText = 0x97,
            msocbvcrOLKIconBar = 0x98,
            msocbvcrOLKInfoBarBkgd = 0x99,
            msocbvcrOLKInfoBarText = 0x9a,
            msocbvcrOLKPreviewPaneLabelText = 0x9b,
            msocbvcrOLKTodayIndicatorDark = 0x9c,
            msocbvcrOLKTodayIndicatorLight = 0x9d,
            msocbvcrOLKWBActionDividerLine = 0x9e,
            msocbvcrOLKWBButtonDark = 0x9f,
            msocbvcrOLKWBButtonLight = 160,
            msocbvcrOLKWBDarkOutline = 0xa1,
            msocbvcrOLKWBFoldersBackground = 0xa2,
            msocbvcrOLKWBHoverButtonDark = 0xa3,
            msocbvcrOLKWBHoverButtonLight = 0xa4,
            msocbvcrOLKWBLabelText = 0xa5,
            msocbvcrOLKWBPressedButtonDark = 0xa6,
            msocbvcrOLKWBPressedButtonLight = 0xa7,
            msocbvcrOLKWBSelectedButtonDark = 0xa8,
            msocbvcrOLKWBSelectedButtonLight = 0xa9,
            msocbvcrOLKWBSplitterDark = 170,
            msocbvcrOLKWBSplitterLight = 0xab,
            msocbvcrPlacesBarBkgd = 0xac,
            msocbvcrPPOutlineThumbnailsPaneTabAreaBkgd = 0xad,
            msocbvcrPPOutlineThumbnailsPaneTabBdr = 0xae,
            msocbvcrPPOutlineThumbnailsPaneTabInactiveBkgd = 0xaf,
            msocbvcrPPOutlineThumbnailsPaneTabText = 0xb0,
            msocbvcrPPSlideBdrActiveSelected = 0xb1,
            msocbvcrPPSlideBdrActiveSelectedMouseOver = 0xb2,
            msocbvcrPPSlideBdrInactiveSelected = 0xb3,
            msocbvcrPPSlideBdrMouseOver = 180,
            msocbvcrPubPrintDocScratchPageBkgd = 0xb5,
            msocbvcrPubWebDocScratchPageBkgd = 0xb6,
            msocbvcrSBBdr = 0xb7,
            msocbvcrScrollbarBkgd = 0xb8,
            msocbvcrToastGradBegin = 0xb9,
            msocbvcrToastGradEnd = 0xba,
            msocbvcrWPBdrInnerDocked = 0xbb,
            msocbvcrWPBdrOuterDocked = 0xbc,
            msocbvcrWPBdrOuterFloating = 0xbd,
            msocbvcrWPBkgd = 190,
            msocbvcrWPCtlBdr = 0xbf,
            msocbvcrWPCtlBdrDefault = 0xc0,
            msocbvcrWPCtlBdrDisabled = 0xc1,
            msocbvcrWPCtlBkgd = 0xc2,
            msocbvcrWPCtlBkgdDisabled = 0xc3,
            msocbvcrWPCtlText = 0xc4,
            msocbvcrWPCtlTextDisabled = 0xc5,
            msocbvcrWPCtlTextMouseDown = 0xc6,
            msocbvcrWPGroupline = 0xc7,
            msocbvcrWPInfoTipBkgd = 200,
            msocbvcrWPInfoTipText = 0xc9,
            msocbvcrWPNavBarBkgnd = 0xca,
            msocbvcrWPText = 0xcb,
            msocbvcrWPTextDisabled = 0xcc,
            msocbvcrWPTitleBkgdActive = 0xcd,
            msocbvcrWPTitleBkgdInactive = 0xce,
            msocbvcrWPTitleTextActive = 0xcf,
            msocbvcrWPTitleTextInactive = 0xd0,
            msocbvcrXLFormulaBarBkgd = 0xd1
        }
    }
}

