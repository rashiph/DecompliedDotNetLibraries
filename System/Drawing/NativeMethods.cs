namespace System.Drawing
{
    using System;
    using System.Runtime.InteropServices;

    internal class NativeMethods
    {
        internal const int BI_BITFIELDS = 3;
        internal const int BI_RGB = 0;
        internal const int BITMAPINFO_MAX_COLORSIZE = 0x100;
        internal const int DEFAULT_GUI_FONT = 0x11;
        internal const int DIB_RGB_COLORS = 0;
        public const int MAX_PATH = 260;
        internal static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        internal const int OBJ_DC = 3;
        internal const int OBJ_ENHMETADC = 12;
        internal const int OBJ_MEMDC = 10;
        internal const int OBJ_METADC = 4;
        public const byte PC_NOCOLLAPSE = 4;
        internal const int SM_REMOTESESSION = 0x1000;
        internal const int SPI_GETICONTITLELOGFONT = 0x1f;
        internal const int SPI_GETNONCLIENTMETRICS = 0x29;

        [StructLayout(LayoutKind.Sequential)]
        internal struct BITMAPINFO_FLAT
        {
            public int bmiHeader_biSize;
            public int bmiHeader_biWidth;
            public int bmiHeader_biHeight;
            public short bmiHeader_biPlanes;
            public short bmiHeader_biBitCount;
            public int bmiHeader_biCompression;
            public int bmiHeader_biSizeImage;
            public int bmiHeader_biXPelsPerMeter;
            public int bmiHeader_biYPelsPerMeter;
            public int bmiHeader_biClrUsed;
            public int bmiHeader_biClrImportant;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x400)]
            public byte[] bmiColors;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class BITMAPINFOHEADER
        {
            public int biSize = 40;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class NONCLIENTMETRICS
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Drawing.NativeMethods.NONCLIENTMETRICS));
            public int iBorderWidth;
            public int iScrollWidth;
            public int iScrollHeight;
            public int iCaptionWidth;
            public int iCaptionHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public SafeNativeMethods.LOGFONT lfCaptionFont;
            public int iSmCaptionWidth;
            public int iSmCaptionHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public SafeNativeMethods.LOGFONT lfSmCaptionFont;
            public int iMenuWidth;
            public int iMenuHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public SafeNativeMethods.LOGFONT lfMenuFont;
            [MarshalAs(UnmanagedType.Struct)]
            public SafeNativeMethods.LOGFONT lfStatusFont;
            [MarshalAs(UnmanagedType.Struct)]
            public SafeNativeMethods.LOGFONT lfMessageFont;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PALETTEENTRY
        {
            public byte peRed;
            public byte peGreen;
            public byte peBlue;
            public byte peFlags;
        }

        public enum RegionFlags
        {
            ERROR,
            NULLREGION,
            SIMPLEREGION,
            COMPLEXREGION
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }
    }
}

