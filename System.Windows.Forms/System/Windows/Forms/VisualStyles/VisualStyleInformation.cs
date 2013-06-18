namespace System.Windows.Forms.VisualStyles
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    public static class VisualStyleInformation
    {
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer;

        public static string Author
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszValueBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Author, pszValueBuff, pszValueBuff.Capacity);
                    return pszValueBuff.ToString();
                }
                return string.Empty;
            }
        }

        public static string ColorScheme
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszColorBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetCurrentThemeName(null, 0, pszColorBuff, pszColorBuff.Capacity, null, 0);
                    return pszColorBuff.ToString();
                }
                return string.Empty;
            }
        }

        public static string Company
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszValueBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Company, pszValueBuff, pszValueBuff.Capacity);
                    return pszValueBuff.ToString();
                }
                return string.Empty;
            }
        }

        public static Color ControlHighlightHot
        {
            get
            {
                if (!Application.RenderWithVisualStyles)
                {
                    return SystemColors.ButtonHighlight;
                }
                if (visualStyleRenderer == null)
                {
                    visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Normal);
                }
                else
                {
                    visualStyleRenderer.SetParameters(VisualStyleElement.Button.PushButton.Normal);
                }
                return visualStyleRenderer.GetColor(ColorProperty.AccentColorHint);
            }
        }

        public static string Copyright
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszValueBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Copyright, pszValueBuff, pszValueBuff.Capacity);
                    return pszValueBuff.ToString();
                }
                return string.Empty;
            }
        }

        public static string Description
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszValueBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Description, pszValueBuff, pszValueBuff.Capacity);
                    return pszValueBuff.ToString();
                }
                return string.Empty;
            }
        }

        public static string DisplayName
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszValueBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.DisplayName, pszValueBuff, pszValueBuff.Capacity);
                    return pszValueBuff.ToString();
                }
                return string.Empty;
            }
        }

        public static bool IsEnabledByUser
        {
            get
            {
                if (!IsSupportedByOS)
                {
                    return false;
                }
                return System.Windows.Forms.SafeNativeMethods.IsAppThemed();
            }
        }

        public static bool IsSupportedByOS
        {
            get
            {
                return OSFeature.Feature.IsPresent(OSFeature.Themes);
            }
        }

        public static int MinimumColorDepth
        {
            get
            {
                if (!Application.RenderWithVisualStyles)
                {
                    return 0;
                }
                if (visualStyleRenderer == null)
                {
                    visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.Window.Caption.Active);
                }
                else
                {
                    visualStyleRenderer.SetParameters(VisualStyleElement.Window.Caption.Active);
                }
                int piValue = 0;
                System.Windows.Forms.SafeNativeMethods.GetThemeSysInt(new HandleRef(null, visualStyleRenderer.Handle), VisualStyleSystemProperty.MinimumColorDepth, ref piValue);
                return piValue;
            }
        }

        public static string Size
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszSizeBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetCurrentThemeName(null, 0, null, 0, pszSizeBuff, pszSizeBuff.Capacity);
                    return pszSizeBuff.ToString();
                }
                return string.Empty;
            }
        }

        public static bool SupportsFlatMenus
        {
            get
            {
                if (!Application.RenderWithVisualStyles)
                {
                    return false;
                }
                if (visualStyleRenderer == null)
                {
                    visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.Window.Caption.Active);
                }
                else
                {
                    visualStyleRenderer.SetParameters(VisualStyleElement.Window.Caption.Active);
                }
                return System.Windows.Forms.SafeNativeMethods.GetThemeSysBool(new HandleRef(null, visualStyleRenderer.Handle), VisualStyleSystemProperty.SupportsFlatMenus);
            }
        }

        public static Color TextControlBorder
        {
            get
            {
                if (!Application.RenderWithVisualStyles)
                {
                    return SystemColors.WindowFrame;
                }
                if (visualStyleRenderer == null)
                {
                    visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.TextBox.TextEdit.Normal);
                }
                else
                {
                    visualStyleRenderer.SetParameters(VisualStyleElement.TextBox.TextEdit.Normal);
                }
                return visualStyleRenderer.GetColor(ColorProperty.BorderColor);
            }
        }

        internal static string ThemeFilename
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszThemeFileName = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetCurrentThemeName(pszThemeFileName, pszThemeFileName.Capacity, null, 0, null, 0);
                    return pszThemeFileName.ToString();
                }
                return string.Empty;
            }
        }

        public static string Url
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszValueBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Url, pszValueBuff, pszValueBuff.Capacity);
                    return pszValueBuff.ToString();
                }
                return string.Empty;
            }
        }

        public static string Version
        {
            get
            {
                if (IsEnabledByUser)
                {
                    StringBuilder pszValueBuff = new StringBuilder(0x200);
                    System.Windows.Forms.SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Version, pszValueBuff, pszValueBuff.Capacity);
                    return pszValueBuff.ToString();
                }
                return string.Empty;
            }
        }
    }
}

