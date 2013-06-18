namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class ProfessionalColors
    {
        [ThreadStatic]
        private static object colorFreshnessKey;
        [ThreadStatic]
        private static string colorScheme;
        [ThreadStatic]
        private static ProfessionalColorTable professionalColorTable;

        static ProfessionalColors()
        {
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(ProfessionalColors.OnUserPreferenceChanged);
            SetScheme();
        }

        private ProfessionalColors()
        {
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            SetScheme();
            if (e.Category == UserPreferenceCategory.Color)
            {
                colorFreshnessKey = new object();
            }
        }

        private static void SetScheme()
        {
            if (VisualStyleRenderer.IsSupported)
            {
                colorScheme = VisualStyleInformation.ColorScheme;
            }
            else
            {
                colorScheme = null;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedGradientBeginDescr")]
        public static System.Drawing.Color ButtonCheckedGradientBegin
        {
            get
            {
                return ColorTable.ButtonCheckedGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedGradientEndDescr")]
        public static System.Drawing.Color ButtonCheckedGradientEnd
        {
            get
            {
                return ColorTable.ButtonCheckedGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedGradientMiddleDescr")]
        public static System.Drawing.Color ButtonCheckedGradientMiddle
        {
            get
            {
                return ColorTable.ButtonCheckedGradientMiddle;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedHighlightDescr")]
        public static System.Drawing.Color ButtonCheckedHighlight
        {
            get
            {
                return ColorTable.ButtonCheckedHighlight;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonCheckedHighlightBorderDescr")]
        public static System.Drawing.Color ButtonCheckedHighlightBorder
        {
            get
            {
                return ColorTable.ButtonCheckedHighlightBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedBorderDescr")]
        public static System.Drawing.Color ButtonPressedBorder
        {
            get
            {
                return ColorTable.ButtonPressedBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedGradientBeginDescr")]
        public static System.Drawing.Color ButtonPressedGradientBegin
        {
            get
            {
                return ColorTable.ButtonPressedGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedGradientEndDescr")]
        public static System.Drawing.Color ButtonPressedGradientEnd
        {
            get
            {
                return ColorTable.ButtonPressedGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedGradientMiddleDescr")]
        public static System.Drawing.Color ButtonPressedGradientMiddle
        {
            get
            {
                return ColorTable.ButtonPressedGradientMiddle;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedHighlightDescr")]
        public static System.Drawing.Color ButtonPressedHighlight
        {
            get
            {
                return ColorTable.ButtonPressedHighlight;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonPressedHighlightBorderDescr")]
        public static System.Drawing.Color ButtonPressedHighlightBorder
        {
            get
            {
                return ColorTable.ButtonPressedHighlightBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedBorderDescr")]
        public static System.Drawing.Color ButtonSelectedBorder
        {
            get
            {
                return ColorTable.ButtonSelectedBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedGradientBeginDescr")]
        public static System.Drawing.Color ButtonSelectedGradientBegin
        {
            get
            {
                return ColorTable.ButtonSelectedGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedGradientEndDescr")]
        public static System.Drawing.Color ButtonSelectedGradientEnd
        {
            get
            {
                return ColorTable.ButtonSelectedGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedGradientMiddleDescr")]
        public static System.Drawing.Color ButtonSelectedGradientMiddle
        {
            get
            {
                return ColorTable.ButtonSelectedGradientMiddle;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedHighlightDescr")]
        public static System.Drawing.Color ButtonSelectedHighlight
        {
            get
            {
                return ColorTable.ButtonSelectedHighlight;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsButtonSelectedHighlightBorderDescr")]
        public static System.Drawing.Color ButtonSelectedHighlightBorder
        {
            get
            {
                return ColorTable.ButtonSelectedHighlightBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsCheckBackgroundDescr")]
        public static System.Drawing.Color CheckBackground
        {
            get
            {
                return ColorTable.CheckBackground;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsCheckPressedBackgroundDescr")]
        public static System.Drawing.Color CheckPressedBackground
        {
            get
            {
                return ColorTable.CheckPressedBackground;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsCheckSelectedBackgroundDescr")]
        public static System.Drawing.Color CheckSelectedBackground
        {
            get
            {
                return ColorTable.CheckSelectedBackground;
            }
        }

        internal static object ColorFreshnessKey
        {
            get
            {
                return colorFreshnessKey;
            }
        }

        internal static string ColorScheme
        {
            get
            {
                return colorScheme;
            }
        }

        internal static ProfessionalColorTable ColorTable
        {
            get
            {
                if (professionalColorTable == null)
                {
                    professionalColorTable = new ProfessionalColorTable();
                }
                return professionalColorTable;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsGripDarkDescr")]
        public static System.Drawing.Color GripDark
        {
            get
            {
                return ColorTable.GripDark;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsGripLightDescr")]
        public static System.Drawing.Color GripLight
        {
            get
            {
                return ColorTable.GripLight;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginGradientBeginDescr")]
        public static System.Drawing.Color ImageMarginGradientBegin
        {
            get
            {
                return ColorTable.ImageMarginGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginGradientEndDescr")]
        public static System.Drawing.Color ImageMarginGradientEnd
        {
            get
            {
                return ColorTable.ImageMarginGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginGradientMiddleDescr")]
        public static System.Drawing.Color ImageMarginGradientMiddle
        {
            get
            {
                return ColorTable.ImageMarginGradientMiddle;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginRevealedGradientBeginDescr")]
        public static System.Drawing.Color ImageMarginRevealedGradientBegin
        {
            get
            {
                return ColorTable.ImageMarginRevealedGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginRevealedGradientEndDescr")]
        public static System.Drawing.Color ImageMarginRevealedGradientEnd
        {
            get
            {
                return ColorTable.ImageMarginRevealedGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsImageMarginRevealedGradientMiddleDescr")]
        public static System.Drawing.Color ImageMarginRevealedGradientMiddle
        {
            get
            {
                return ColorTable.ImageMarginRevealedGradientMiddle;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuBorderDescr")]
        public static System.Drawing.Color MenuBorder
        {
            get
            {
                return ColorTable.MenuBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemBorderDescr")]
        public static System.Drawing.Color MenuItemBorder
        {
            get
            {
                return ColorTable.MenuItemBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemPressedGradientBeginDescr")]
        public static System.Drawing.Color MenuItemPressedGradientBegin
        {
            get
            {
                return ColorTable.MenuItemPressedGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemPressedGradientEndDescr")]
        public static System.Drawing.Color MenuItemPressedGradientEnd
        {
            get
            {
                return ColorTable.MenuItemPressedGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemPressedGradientMiddleDescr")]
        public static System.Drawing.Color MenuItemPressedGradientMiddle
        {
            get
            {
                return ColorTable.MenuItemPressedGradientMiddle;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemSelectedDescr")]
        public static System.Drawing.Color MenuItemSelected
        {
            get
            {
                return ColorTable.MenuItemSelected;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemSelectedGradientBeginDescr")]
        public static System.Drawing.Color MenuItemSelectedGradientBegin
        {
            get
            {
                return ColorTable.MenuItemSelectedGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuItemSelectedGradientEndDescr")]
        public static System.Drawing.Color MenuItemSelectedGradientEnd
        {
            get
            {
                return ColorTable.MenuItemSelectedGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuStripGradientBeginDescr")]
        public static System.Drawing.Color MenuStripGradientBegin
        {
            get
            {
                return ColorTable.MenuStripGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsMenuStripGradientEndDescr")]
        public static System.Drawing.Color MenuStripGradientEnd
        {
            get
            {
                return ColorTable.MenuStripGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsOverflowButtonGradientBeginDescr")]
        public static System.Drawing.Color OverflowButtonGradientBegin
        {
            get
            {
                return ColorTable.OverflowButtonGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsOverflowButtonGradientEndDescr")]
        public static System.Drawing.Color OverflowButtonGradientEnd
        {
            get
            {
                return ColorTable.OverflowButtonGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsOverflowButtonGradientMiddleDescr")]
        public static System.Drawing.Color OverflowButtonGradientMiddle
        {
            get
            {
                return ColorTable.OverflowButtonGradientMiddle;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsRaftingContainerGradientBeginDescr")]
        public static System.Drawing.Color RaftingContainerGradientBegin
        {
            get
            {
                return ColorTable.RaftingContainerGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsRaftingContainerGradientEndDescr")]
        public static System.Drawing.Color RaftingContainerGradientEnd
        {
            get
            {
                return ColorTable.RaftingContainerGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsSeparatorDarkDescr")]
        public static System.Drawing.Color SeparatorDark
        {
            get
            {
                return ColorTable.SeparatorDark;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsSeparatorLightDescr")]
        public static System.Drawing.Color SeparatorLight
        {
            get
            {
                return ColorTable.SeparatorLight;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsStatusStripGradientBeginDescr")]
        public static System.Drawing.Color StatusStripGradientBegin
        {
            get
            {
                return ColorTable.StatusStripGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsStatusStripGradientEndDescr")]
        public static System.Drawing.Color StatusStripGradientEnd
        {
            get
            {
                return ColorTable.StatusStripGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripBorderDescr")]
        public static System.Drawing.Color ToolStripBorder
        {
            get
            {
                return ColorTable.ToolStripBorder;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripContentPanelGradientBeginDescr")]
        public static System.Drawing.Color ToolStripContentPanelGradientBegin
        {
            get
            {
                return ColorTable.ToolStripContentPanelGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripContentPanelGradientEndDescr")]
        public static System.Drawing.Color ToolStripContentPanelGradientEnd
        {
            get
            {
                return ColorTable.ToolStripContentPanelGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripDropDownBackgroundDescr")]
        public static System.Drawing.Color ToolStripDropDownBackground
        {
            get
            {
                return ColorTable.ToolStripDropDownBackground;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripGradientBeginDescr")]
        public static System.Drawing.Color ToolStripGradientBegin
        {
            get
            {
                return ColorTable.ToolStripGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripGradientEndDescr")]
        public static System.Drawing.Color ToolStripGradientEnd
        {
            get
            {
                return ColorTable.ToolStripGradientEnd;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripGradientMiddleDescr")]
        public static System.Drawing.Color ToolStripGradientMiddle
        {
            get
            {
                return ColorTable.ToolStripGradientMiddle;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripPanelGradientBeginDescr")]
        public static System.Drawing.Color ToolStripPanelGradientBegin
        {
            get
            {
                return ColorTable.ToolStripPanelGradientBegin;
            }
        }

        [System.Windows.Forms.SRDescription("ProfessionalColorsToolStripPanelGradientEndDescr")]
        public static System.Drawing.Color ToolStripPanelGradientEnd
        {
            get
            {
                return ColorTable.ToolStripPanelGradientEnd;
            }
        }
    }
}

