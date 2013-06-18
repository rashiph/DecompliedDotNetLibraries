namespace System.Windows.Forms.Internal
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal class IntNativeMethods
    {
        public const int ALTERNATE = 1;
        public const int ANSI_CHARSET = 0;
        public const int ANTIALIASED_QUALITY = 4;
        public const int BI_BITFIELDS = 3;
        public const int BI_RGB = 0;
        public const int BITMAPINFO_MAX_COLORSIZE = 0x100;
        public const int BITSPIXEL = 12;
        public const int BLACKNESS = 0x42;
        public const int BS_HATCHED = 2;
        public const int BS_SOLID = 0;
        public const int CAPTUREBLT = 0x40000000;
        public const int CLEARTYPE_NATURAL_QUALITY = 6;
        public const int CLEARTYPE_QUALITY = 5;
        public const int CLIP_DEFAULT_PRECIS = 0;
        public const int CP_ACP = 0;
        public const int DEFAULT_CHARSET = 1;
        public const int DEFAULT_GUI_FONT = 0x11;
        public const int DEFAULT_QUALITY = 0;
        public const int DIB_RGB_COLORS = 0;
        public const int DRAFT_QUALITY = 1;
        public const int DSTINVERT = 0x550009;
        public const int DT_BOTTOM = 8;
        public const int DT_CALCRECT = 0x400;
        public const int DT_CENTER = 1;
        public const int DT_EDITCONTROL = 0x2000;
        public const int DT_END_ELLIPSIS = 0x8000;
        public const int DT_EXPANDTABS = 0x40;
        public const int DT_EXTERNALLEADING = 0x200;
        public const int DT_HIDEPREFIX = 0x100000;
        public const int DT_INTERNAL = 0x1000;
        public const int DT_LEFT = 0;
        public const int DT_MODIFYSTRING = 0x10000;
        public const int DT_NOCLIP = 0x100;
        public const int DT_NOFULLWIDTHCHARBREAK = 0x80000;
        public const int DT_NOPREFIX = 0x800;
        public const int DT_PATH_ELLIPSIS = 0x4000;
        public const int DT_PREFIXONLY = 0x200000;
        public const int DT_RIGHT = 2;
        public const int DT_RTLREADING = 0x20000;
        public const int DT_SINGLELINE = 0x20;
        public const int DT_TABSTOP = 0x80;
        public const int DT_TOP = 0;
        public const int DT_VCENTER = 4;
        public const int DT_WORD_ELLIPSIS = 0x40000;
        public const int DT_WORDBREAK = 0x10;
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_DEFAULT = 0x1200;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FW_BOLD = 700;
        public const int FW_DONTCARE = 0;
        public const int FW_NORMAL = 400;
        public const int HOLLOW_BRUSH = 5;
        public const int MaxTextLengthInWin9x = 0x2000;
        public const int MERGECOPY = 0xc000ca;
        public const int MERGEPAINT = 0xbb0226;
        public const int NONANTIALIASED_QUALITY = 3;
        public const int NOTSRCCOPY = 0x330008;
        public const int NOTSRCERASE = 0x1100a6;
        public const int OBJ_BITMAP = 7;
        public const int OBJ_BRUSH = 2;
        public const int OBJ_DC = 3;
        public const int OBJ_ENHMETADC = 12;
        public const int OBJ_EXTPEN = 11;
        public const int OBJ_FONT = 6;
        public const int OBJ_MEMDC = 10;
        public const int OBJ_METADC = 4;
        public const int OBJ_PEN = 1;
        public const int OUT_DEFAULT_PRECIS = 0;
        public const int OUT_TT_ONLY_PRECIS = 7;
        public const int OUT_TT_PRECIS = 4;
        public const int PATCOPY = 0xf00021;
        public const int PATINVERT = 0x5a0049;
        public const int PATPAINT = 0xfb0a09;
        public const int PROOF_QUALITY = 2;
        public const int SPI_GETICONTITLELOGFONT = 0x1f;
        public const int SPI_GETNONCLIENTMETRICS = 0x29;
        public const int SRCAND = 0x8800c6;
        public const int SRCCOPY = 0xcc0020;
        public const int SRCERASE = 0x440328;
        public const int SRCINVERT = 0x660046;
        public const int SRCPAINT = 0xee0086;
        public const int WHITENESS = 0xff0062;
        public const int WINDING = 2;

        [StructLayout(LayoutKind.Sequential)]
        public class DRAWTEXTPARAMS
        {
            private int cbSize;
            public int iTabLength;
            public int iLeftMargin;
            public int iRightMargin;
            public int uiLengthDrawn;
            public DRAWTEXTPARAMS()
            {
                this.cbSize = Marshal.SizeOf(typeof(IntNativeMethods.DRAWTEXTPARAMS));
            }

            public DRAWTEXTPARAMS(IntNativeMethods.DRAWTEXTPARAMS original)
            {
                this.cbSize = Marshal.SizeOf(typeof(IntNativeMethods.DRAWTEXTPARAMS));
                this.iLeftMargin = original.iLeftMargin;
                this.iRightMargin = original.iRightMargin;
                this.iTabLength = original.iTabLength;
            }

            public DRAWTEXTPARAMS(int leftMargin, int rightMargin)
            {
                this.cbSize = Marshal.SizeOf(typeof(IntNativeMethods.DRAWTEXTPARAMS));
                this.iLeftMargin = leftMargin;
                this.iRightMargin = rightMargin;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class LOGBRUSH
        {
            public int lbStyle;
            public int lbColor;
            public int lbHatch;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
            public string lfFaceName;
            public LOGFONT()
            {
            }

            public LOGFONT(IntNativeMethods.LOGFONT lf)
            {
                this.lfHeight = lf.lfHeight;
                this.lfWidth = lf.lfWidth;
                this.lfEscapement = lf.lfEscapement;
                this.lfOrientation = lf.lfOrientation;
                this.lfWeight = lf.lfWeight;
                this.lfItalic = lf.lfItalic;
                this.lfUnderline = lf.lfUnderline;
                this.lfStrikeOut = lf.lfStrikeOut;
                this.lfCharSet = lf.lfCharSet;
                this.lfOutPrecision = lf.lfOutPrecision;
                this.lfClipPrecision = lf.lfClipPrecision;
                this.lfQuality = lf.lfQuality;
                this.lfPitchAndFamily = lf.lfPitchAndFamily;
                this.lfFaceName = lf.lfFaceName;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
            public POINT()
            {
            }

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public Point ToPoint()
            {
                return new Point(this.x, this.y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(Rectangle r)
            {
                this.left = r.Left;
                this.top = r.Top;
                this.right = r.Right;
                this.bottom = r.Bottom;
            }

            public static IntNativeMethods.RECT FromXYWH(int x, int y, int width, int height)
            {
                return new IntNativeMethods.RECT(x, y, x + width, y + height);
            }

            public System.Drawing.Size Size
            {
                get
                {
                    return new System.Drawing.Size(this.right - this.left, this.bottom - this.top);
                }
            }
            public Rectangle ToRectangle()
            {
                return new Rectangle(this.left, this.top, this.right - this.left, this.bottom - this.top);
            }
        }

        public enum RegionFlags
        {
            ERROR,
            NULLREGION,
            SIMPLEREGION,
            COMPLEXREGION
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SIZE
        {
            public int cx;
            public int cy;
            public SIZE()
            {
            }

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }

            public Size ToSize()
            {
                return new Size(this.cx, this.cy);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TEXTMETRICA
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public byte tmFirstChar;
            public byte tmLastChar;
            public byte tmDefaultChar;
            public byte tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }
    }
}

