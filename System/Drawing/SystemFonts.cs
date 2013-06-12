namespace System.Drawing
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;

    public sealed class SystemFonts
    {
        private static readonly object SystemFontsKey = new object();

        private SystemFonts()
        {
        }

        private static Font FontInPoints(Font font)
        {
            return new Font(font.FontFamily, font.SizeInPoints, font.Style, GraphicsUnit.Point, font.GdiCharSet, font.GdiVerticalFont);
        }

        public static Font GetFontByName(string systemFontName)
        {
            if ("CaptionFont".Equals(systemFontName))
            {
                return CaptionFont;
            }
            if ("DefaultFont".Equals(systemFontName))
            {
                return DefaultFont;
            }
            if ("DialogFont".Equals(systemFontName))
            {
                return DialogFont;
            }
            if ("IconTitleFont".Equals(systemFontName))
            {
                return IconTitleFont;
            }
            if ("MenuFont".Equals(systemFontName))
            {
                return MenuFont;
            }
            if ("MessageBoxFont".Equals(systemFontName))
            {
                return MessageBoxFont;
            }
            if ("SmallCaptionFont".Equals(systemFontName))
            {
                return SmallCaptionFont;
            }
            if ("StatusFont".Equals(systemFontName))
            {
                return StatusFont;
            }
            return null;
        }

        private static bool IsCriticalFontException(Exception ex)
        {
            return ((((!(ex is ExternalException) && !(ex is ArgumentException)) && (!(ex is OutOfMemoryException) && !(ex is InvalidOperationException))) && !(ex is NotImplementedException)) && !(ex is FileNotFoundException));
        }

        public static Font CaptionFont
        {
            get
            {
                Font defaultFont = null;
                System.Drawing.NativeMethods.NONCLIENTMETRICS pvParam = new System.Drawing.NativeMethods.NONCLIENTMETRICS();
                if (UnsafeNativeMethods.SystemParametersInfo(0x29, pvParam.cbSize, pvParam, 0) && (pvParam.lfCaptionFont != null))
                {
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        defaultFont = Font.FromLogFont(pvParam.lfCaptionFont);
                    }
                    catch (Exception exception)
                    {
                        if (IsCriticalFontException(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (defaultFont == null)
                    {
                        defaultFont = DefaultFont;
                    }
                    else if (defaultFont.Unit != GraphicsUnit.Point)
                    {
                        defaultFont = FontInPoints(defaultFont);
                    }
                }
                defaultFont.SetSystemFontName("CaptionFont");
                return defaultFont;
            }
        }

        public static Font DefaultFont
        {
            get
            {
                Font font = null;
                bool flag = false;
                if (((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major <= 4)) && ((UnsafeNativeMethods.GetSystemDefaultLCID() & 0x3ff) == 0x11))
                {
                    try
                    {
                        font = new Font("MS UI Gothic", 9f);
                    }
                    catch (Exception exception)
                    {
                        if (IsCriticalFontException(exception))
                        {
                            throw;
                        }
                    }
                }
                if (font == null)
                {
                    flag = (UnsafeNativeMethods.GetSystemDefaultLCID() & 0x3ff) == 1;
                }
                if (flag)
                {
                    try
                    {
                        font = new Font("Tahoma", 8f);
                    }
                    catch (Exception exception2)
                    {
                        if (IsCriticalFontException(exception2))
                        {
                            throw;
                        }
                    }
                }
                if (font == null)
                {
                    IntPtr stockObject = UnsafeNativeMethods.GetStockObject(0x11);
                    try
                    {
                        Font font2 = null;
                        IntSecurity.ObjectFromWin32Handle.Assert();
                        try
                        {
                            font2 = Font.FromHfont(stockObject);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                        try
                        {
                            font = FontInPoints(font2);
                        }
                        finally
                        {
                            font2.Dispose();
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                if (font == null)
                {
                    try
                    {
                        font = new Font("Tahoma", 8f);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                if (font == null)
                {
                    font = new Font(FontFamily.GenericSansSerif, 8f);
                }
                if (font.Unit != GraphicsUnit.Point)
                {
                    font = FontInPoints(font);
                }
                font.SetSystemFontName("DefaultFont");
                return font;
            }
        }

        public static Font DialogFont
        {
            get
            {
                Font defaultFont = null;
                if ((UnsafeNativeMethods.GetSystemDefaultLCID() & 0x3ff) == 0x11)
                {
                    defaultFont = DefaultFont;
                }
                else if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
                {
                    defaultFont = DefaultFont;
                }
                else
                {
                    try
                    {
                        defaultFont = new Font("MS Shell Dlg 2", 8f);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                if (defaultFont == null)
                {
                    defaultFont = DefaultFont;
                }
                else if (defaultFont.Unit != GraphicsUnit.Point)
                {
                    defaultFont = FontInPoints(defaultFont);
                }
                defaultFont.SetSystemFontName("DialogFont");
                return defaultFont;
            }
        }

        public static Font IconTitleFont
        {
            get
            {
                Font defaultFont = null;
                SafeNativeMethods.LOGFONT pvParam = new SafeNativeMethods.LOGFONT();
                if (UnsafeNativeMethods.SystemParametersInfo(0x1f, Marshal.SizeOf(pvParam), pvParam, 0) && (pvParam != null))
                {
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        defaultFont = Font.FromLogFont(pvParam);
                    }
                    catch (Exception exception)
                    {
                        if (IsCriticalFontException(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (defaultFont == null)
                    {
                        defaultFont = DefaultFont;
                    }
                    else if (defaultFont.Unit != GraphicsUnit.Point)
                    {
                        defaultFont = FontInPoints(defaultFont);
                    }
                }
                defaultFont.SetSystemFontName("IconTitleFont");
                return defaultFont;
            }
        }

        public static Font MenuFont
        {
            get
            {
                Font defaultFont = null;
                System.Drawing.NativeMethods.NONCLIENTMETRICS pvParam = new System.Drawing.NativeMethods.NONCLIENTMETRICS();
                if (UnsafeNativeMethods.SystemParametersInfo(0x29, pvParam.cbSize, pvParam, 0) && (pvParam.lfMenuFont != null))
                {
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        defaultFont = Font.FromLogFont(pvParam.lfMenuFont);
                    }
                    catch (Exception exception)
                    {
                        if (IsCriticalFontException(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (defaultFont == null)
                    {
                        defaultFont = DefaultFont;
                    }
                    else if (defaultFont.Unit != GraphicsUnit.Point)
                    {
                        defaultFont = FontInPoints(defaultFont);
                    }
                }
                defaultFont.SetSystemFontName("MenuFont");
                return defaultFont;
            }
        }

        public static Font MessageBoxFont
        {
            get
            {
                Font defaultFont = null;
                System.Drawing.NativeMethods.NONCLIENTMETRICS pvParam = new System.Drawing.NativeMethods.NONCLIENTMETRICS();
                if (UnsafeNativeMethods.SystemParametersInfo(0x29, pvParam.cbSize, pvParam, 0) && (pvParam.lfMessageFont != null))
                {
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        defaultFont = Font.FromLogFont(pvParam.lfMessageFont);
                    }
                    catch (Exception exception)
                    {
                        if (IsCriticalFontException(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (defaultFont == null)
                    {
                        defaultFont = DefaultFont;
                    }
                    else if (defaultFont.Unit != GraphicsUnit.Point)
                    {
                        defaultFont = FontInPoints(defaultFont);
                    }
                }
                defaultFont.SetSystemFontName("MessageBoxFont");
                return defaultFont;
            }
        }

        public static Font SmallCaptionFont
        {
            get
            {
                Font defaultFont = null;
                System.Drawing.NativeMethods.NONCLIENTMETRICS pvParam = new System.Drawing.NativeMethods.NONCLIENTMETRICS();
                if (UnsafeNativeMethods.SystemParametersInfo(0x29, pvParam.cbSize, pvParam, 0) && (pvParam.lfSmCaptionFont != null))
                {
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        defaultFont = Font.FromLogFont(pvParam.lfSmCaptionFont);
                    }
                    catch (Exception exception)
                    {
                        if (IsCriticalFontException(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (defaultFont == null)
                    {
                        defaultFont = DefaultFont;
                    }
                    else if (defaultFont.Unit != GraphicsUnit.Point)
                    {
                        defaultFont = FontInPoints(defaultFont);
                    }
                }
                defaultFont.SetSystemFontName("SmallCaptionFont");
                return defaultFont;
            }
        }

        public static Font StatusFont
        {
            get
            {
                Font defaultFont = null;
                System.Drawing.NativeMethods.NONCLIENTMETRICS pvParam = new System.Drawing.NativeMethods.NONCLIENTMETRICS();
                if (UnsafeNativeMethods.SystemParametersInfo(0x29, pvParam.cbSize, pvParam, 0) && (pvParam.lfStatusFont != null))
                {
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        defaultFont = Font.FromLogFont(pvParam.lfStatusFont);
                    }
                    catch (Exception exception)
                    {
                        if (IsCriticalFontException(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (defaultFont == null)
                    {
                        defaultFont = DefaultFont;
                    }
                    else if (defaultFont.Unit != GraphicsUnit.Point)
                    {
                        defaultFont = FontInPoints(defaultFont);
                    }
                }
                defaultFont.SetSystemFontName("StatusFont");
                return defaultFont;
            }
        }
    }
}

