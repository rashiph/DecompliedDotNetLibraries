namespace System.Windows.Forms.Internal
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal sealed class WindowsFont : MarshalByRefObject, ICloneable, IDisposable
    {
        private const string defaultFaceName = "Microsoft Sans Serif";
        private const int defaultFontHeight = 13;
        private const float defaultFontSize = 8.25f;
        private bool everOwnedByCacheManager;
        private float fontSize;
        private IntPtr hFont;
        private int lineSpacing;
        private IntNativeMethods.LOGFONT logFont;
        private const int LogFontNameOffset = 0x1c;
        private bool ownedByCacheManager;
        private bool ownHandle;
        private FontStyle style;

        public WindowsFont(string faceName) : this(faceName, 8.25f, FontStyle.Regular, 1, WindowsFontQuality.Default)
        {
        }

        public WindowsFont(string faceName, float size) : this(faceName, size, FontStyle.Regular, 1, WindowsFontQuality.Default)
        {
        }

        private WindowsFont(IntNativeMethods.LOGFONT lf, bool createHandle)
        {
            this.fontSize = -1f;
            this.logFont = lf;
            if (this.logFont.lfFaceName == null)
            {
                this.logFont.lfFaceName = "Microsoft Sans Serif";
            }
            this.style = FontStyle.Regular;
            if (lf.lfWeight == 700)
            {
                this.style |= FontStyle.Bold;
            }
            if (lf.lfItalic == 1)
            {
                this.style |= FontStyle.Italic;
            }
            if (lf.lfUnderline == 1)
            {
                this.style |= FontStyle.Underline;
            }
            if (lf.lfStrikeOut == 1)
            {
                this.style |= FontStyle.Strikeout;
            }
            if (createHandle)
            {
                this.CreateFont();
            }
        }

        public WindowsFont(string faceName, float size, FontStyle style) : this(faceName, size, style, 1, WindowsFontQuality.Default)
        {
        }

        public WindowsFont(string faceName, float size, FontStyle style, byte charSet, WindowsFontQuality fontQuality)
        {
            this.fontSize = -1f;
            this.logFont = new IntNativeMethods.LOGFONT();
            int num = (int) Math.Ceiling((double) ((WindowsGraphicsCacheManager.MeasurementGraphics.DeviceContext.DpiY * size) / 72f));
            this.logFont.lfHeight = -num;
            this.logFont.lfFaceName = (faceName != null) ? faceName : "Microsoft Sans Serif";
            this.logFont.lfCharSet = charSet;
            this.logFont.lfOutPrecision = 4;
            this.logFont.lfQuality = (byte) fontQuality;
            this.logFont.lfWeight = ((style & FontStyle.Bold) == FontStyle.Bold) ? 700 : 400;
            this.logFont.lfItalic = ((style & FontStyle.Italic) == FontStyle.Italic) ? ((byte) 1) : ((byte) 0);
            this.logFont.lfUnderline = ((style & FontStyle.Underline) == FontStyle.Underline) ? ((byte) 1) : ((byte) 0);
            this.logFont.lfStrikeOut = ((style & FontStyle.Strikeout) == FontStyle.Strikeout) ? ((byte) 1) : ((byte) 0);
            this.style = style;
            this.CreateFont();
        }

        public object Clone()
        {
            return new WindowsFont(this.logFont, true);
        }

        private void CreateFont()
        {
            this.hFont = IntUnsafeNativeMethods.CreateFontIndirect(this.logFont);
            if (this.hFont == IntPtr.Zero)
            {
                this.logFont.lfFaceName = "Microsoft Sans Serif";
                this.logFont.lfOutPrecision = 7;
                this.hFont = IntUnsafeNativeMethods.CreateFontIndirect(this.logFont);
            }
            IntUnsafeNativeMethods.GetObject(new HandleRef(this, this.hFont), this.logFont);
            this.ownHandle = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        internal void Dispose(bool disposing)
        {
            bool flag = false;
            if ((this.ownHandle && (!this.ownedByCacheManager || !disposing)) && ((this.everOwnedByCacheManager || !disposing) || !DeviceContexts.IsFontInUse(this)))
            {
                IntUnsafeNativeMethods.DeleteObject(new HandleRef(this, this.hFont));
                this.hFont = IntPtr.Zero;
                this.ownHandle = false;
                flag = true;
            }
            if (disposing && (flag || !this.ownHandle))
            {
                GC.SuppressFinalize(this);
            }
        }

        public override bool Equals(object font)
        {
            WindowsFont font2 = font as WindowsFont;
            if (font2 == null)
            {
                return false;
            }
            return ((font2 == this) || ((((this.Name == font2.Name) && (this.LogFontHeight == font2.LogFontHeight)) && ((this.Style == font2.Style) && (this.CharSet == font2.CharSet))) && (this.Quality == font2.Quality)));
        }

        ~WindowsFont()
        {
            this.Dispose(false);
        }

        public static WindowsFont FromFont(Font font)
        {
            return FromFont(font, WindowsFontQuality.Default);
        }

        public static WindowsFont FromFont(Font font, WindowsFontQuality fontQuality)
        {
            string name = font.FontFamily.Name;
            if (((name != null) && (name.Length > 1)) && (name[0] == '@'))
            {
                name = name.Substring(1);
            }
            return new WindowsFont(name, font.SizeInPoints, font.Style, font.GdiCharSet, fontQuality);
        }

        public static WindowsFont FromHdc(IntPtr hdc)
        {
            return FromHfont(IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(null, hdc), 6));
        }

        public static WindowsFont FromHfont(IntPtr hFont)
        {
            return FromHfont(hFont, false);
        }

        public static WindowsFont FromHfont(IntPtr hFont, bool takeOwnership)
        {
            IntNativeMethods.LOGFONT lp = new IntNativeMethods.LOGFONT();
            IntUnsafeNativeMethods.GetObject(new HandleRef(null, hFont), lp);
            return new WindowsFont(lp, false) { hFont = hFont, ownHandle = takeOwnership };
        }

        public override int GetHashCode()
        {
            return ((((((int) this.Style) << 13) | (((int) this.Style) >> 0x13)) ^ ((this.CharSet << 0x1a) | (this.CharSet >> 6))) ^ ((int) ((((uint) this.Size) << 7) | (((uint) this.Size) >> 0x19))));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}: Name={1}, Size={2} points, Height={3} pixels, Sytle={4}]", new object[] { base.GetType().Name, this.logFont.lfFaceName, this.Size, this.Height, this.Style });
        }

        public static WindowsFontQuality WindowsFontQualityFromTextRenderingHint(Graphics g)
        {
            if (g != null)
            {
                switch (g.TextRenderingHint)
                {
                    case TextRenderingHint.SingleBitPerPixelGridFit:
                        return WindowsFontQuality.Proof;

                    case TextRenderingHint.SingleBitPerPixel:
                        return WindowsFontQuality.Draft;

                    case TextRenderingHint.AntiAliasGridFit:
                        return WindowsFontQuality.AntiAliased;

                    case TextRenderingHint.AntiAlias:
                        return WindowsFontQuality.AntiAliased;

                    case TextRenderingHint.ClearTypeGridFit:
                        if ((Environment.OSVersion.Version.Major != 5) || (Environment.OSVersion.Version.Minor < 1))
                        {
                            return WindowsFontQuality.ClearType;
                        }
                        return WindowsFontQuality.ClearTypeNatural;
                }
            }
            return WindowsFontQuality.Default;
        }

        public byte CharSet
        {
            get
            {
                return this.logFont.lfCharSet;
            }
        }

        public int Height
        {
            get
            {
                if (this.lineSpacing == 0)
                {
                    WindowsGraphics measurementGraphics = WindowsGraphicsCacheManager.MeasurementGraphics;
                    measurementGraphics.DeviceContext.SelectFont(this);
                    IntNativeMethods.TEXTMETRIC textMetrics = measurementGraphics.GetTextMetrics();
                    this.lineSpacing = textMetrics.tmHeight;
                }
                return this.lineSpacing;
            }
        }

        public IntPtr Hfont
        {
            get
            {
                return this.hFont;
            }
        }

        public bool Italic
        {
            get
            {
                return (this.logFont.lfItalic == 1);
            }
        }

        public int LogFontHeight
        {
            get
            {
                return this.logFont.lfHeight;
            }
        }

        public string Name
        {
            get
            {
                return this.logFont.lfFaceName;
            }
        }

        public bool OwnedByCacheManager
        {
            get
            {
                return this.ownedByCacheManager;
            }
            set
            {
                if (value)
                {
                    this.everOwnedByCacheManager = true;
                }
                this.ownedByCacheManager = value;
            }
        }

        public WindowsFontQuality Quality
        {
            get
            {
                return (WindowsFontQuality) this.logFont.lfQuality;
            }
        }

        public float Size
        {
            get
            {
                if (this.fontSize < 0f)
                {
                    WindowsGraphics measurementGraphics = WindowsGraphicsCacheManager.MeasurementGraphics;
                    measurementGraphics.DeviceContext.SelectFont(this);
                    IntNativeMethods.TEXTMETRIC textMetrics = measurementGraphics.GetTextMetrics();
                    int num = (this.logFont.lfHeight > 0) ? textMetrics.tmHeight : (textMetrics.tmHeight - textMetrics.tmInternalLeading);
                    this.fontSize = (num * 72f) / ((float) measurementGraphics.DeviceContext.DpiY);
                }
                return this.fontSize;
            }
        }

        public FontStyle Style
        {
            get
            {
                return this.style;
            }
        }
    }
}

