namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.Internal;

    public sealed class TextRenderer
    {
        private TextRenderer()
        {
        }

        internal static Color DisabledTextColor(Color backColor)
        {
            Color controlDark = SystemColors.ControlDark;
            if (ControlPaint.IsDarker(backColor, SystemColors.Control))
            {
                controlDark = ControlPaint.Dark(backColor);
            }
            return controlDark;
        }

        public static void DrawText(IDeviceContext dc, string text, Font font, Point pt, Color foreColor)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            IntPtr hdc = dc.GetHdc();
            try
            {
                using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                {
                    using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                    {
                        graphics.DrawText(text, font2, pt, foreColor);
                    }
                }
            }
            finally
            {
                dc.ReleaseHdc();
            }
        }

        public static void DrawText(IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            IntPtr hdc = dc.GetHdc();
            try
            {
                using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                {
                    using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                    {
                        graphics.DrawText(text, font2, bounds, foreColor);
                    }
                }
            }
            finally
            {
                dc.ReleaseHdc();
            }
        }

        public static void DrawText(IDeviceContext dc, string text, Font font, Point pt, Color foreColor, Color backColor)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            IntPtr hdc = dc.GetHdc();
            try
            {
                using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                {
                    using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                    {
                        graphics.DrawText(text, font2, pt, foreColor, backColor);
                    }
                }
            }
            finally
            {
                dc.ReleaseHdc();
            }
        }

        public static void DrawText(IDeviceContext dc, string text, Font font, Point pt, Color foreColor, TextFormatFlags flags)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, flags))
            {
                using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                {
                    wrapper.WindowsGraphics.DrawText(text, font2, pt, foreColor, GetIntTextFormatFlags(flags));
                }
            }
        }

        public static void DrawText(IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            IntPtr hdc = dc.GetHdc();
            try
            {
                using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                {
                    using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                    {
                        graphics.DrawText(text, font2, bounds, foreColor, backColor);
                    }
                }
            }
            finally
            {
                dc.ReleaseHdc();
            }
        }

        public static void DrawText(IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, TextFormatFlags flags)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, flags))
            {
                using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                {
                    wrapper.WindowsGraphics.DrawText(text, font2, bounds, foreColor, GetIntTextFormatFlags(flags));
                }
            }
        }

        public static void DrawText(IDeviceContext dc, string text, Font font, Point pt, Color foreColor, Color backColor, TextFormatFlags flags)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, flags))
            {
                using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                {
                    wrapper.WindowsGraphics.DrawText(text, font2, pt, foreColor, backColor, GetIntTextFormatFlags(flags));
                }
            }
        }

        public static void DrawText(IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor, TextFormatFlags flags)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, flags))
            {
                using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                {
                    wrapper.WindowsGraphics.DrawText(text, font2, bounds, foreColor, backColor, GetIntTextFormatFlags(flags));
                }
            }
        }

        private static IntTextFormatFlags GetIntTextFormatFlags(TextFormatFlags flags)
        {
            if ((((ulong) flags) & 18446744073692774400L) == 0L)
            {
                return (IntTextFormatFlags) flags;
            }
            return (((IntTextFormatFlags) flags) & ((IntTextFormatFlags) 0xffffff));
        }

        public static Size MeasureText(string text, Font font)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }
            using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font))
            {
                return WindowsGraphicsCacheManager.MeasurementGraphics.MeasureText(text, font2);
            }
        }

        public static Size MeasureText(IDeviceContext dc, string text, Font font)
        {
            Size size;
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            IntPtr hdc = dc.GetHdc();
            try
            {
                using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                {
                    using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                    {
                        size = graphics.MeasureText(text, font2);
                    }
                }
            }
            finally
            {
                dc.ReleaseHdc();
            }
            return size;
        }

        public static Size MeasureText(string text, Font font, Size proposedSize)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }
            using (WindowsGraphicsCacheManager.GetWindowsFont(font))
            {
                return WindowsGraphicsCacheManager.MeasurementGraphics.MeasureText(text, WindowsGraphicsCacheManager.GetWindowsFont(font), proposedSize);
            }
        }

        public static Size MeasureText(IDeviceContext dc, string text, Font font, Size proposedSize)
        {
            Size size;
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            IntPtr hdc = dc.GetHdc();
            try
            {
                using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                {
                    using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                    {
                        size = graphics.MeasureText(text, font2, proposedSize);
                    }
                }
            }
            finally
            {
                dc.ReleaseHdc();
            }
            return size;
        }

        public static Size MeasureText(string text, Font font, Size proposedSize, TextFormatFlags flags)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }
            using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font))
            {
                return WindowsGraphicsCacheManager.MeasurementGraphics.MeasureText(text, font2, proposedSize, GetIntTextFormatFlags(flags));
            }
        }

        public static Size MeasureText(IDeviceContext dc, string text, Font font, Size proposedSize, TextFormatFlags flags)
        {
            Size size;
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }
            WindowsFontQuality fontQuality = WindowsFont.WindowsFontQualityFromTextRenderingHint(dc as Graphics);
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, flags))
            {
                using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(font, fontQuality))
                {
                    size = wrapper.WindowsGraphics.MeasureText(text, font2, proposedSize, GetIntTextFormatFlags(flags));
                }
            }
            return size;
        }
    }
}

