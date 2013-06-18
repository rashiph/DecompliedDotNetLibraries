namespace System.Workflow.Interop
{
    using System;
    using System.Drawing.Drawing2D;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        internal const int BS_SOLID = 0;
        internal const int GM_ADVANCED = 2;
        internal const int HDF_BITMAP = 0x2000;
        internal const int HDF_BITMAP_ON_RIGHT = 0x1000;
        internal const int HDF_CENTER = 2;
        internal const int HDF_IMAGE = 0x800;
        internal const int HDF_JUSTIFYMASK = 3;
        internal const int HDF_LEFT = 0;
        internal const int HDF_OWNERDRAW = 0x8000;
        internal const int HDF_RIGHT = 1;
        internal const int HDF_RTLREADING = 4;
        internal const int HDF_SORTDOWN = 0x200;
        internal const int HDF_SORTUP = 0x400;
        internal const int HDF_STRING = 0x4000;
        internal const int HDI_BITMAP = 0x10;
        internal const int HDI_DI_SETITEM = 0x40;
        internal const int HDI_FILTER = 0x100;
        internal const int HDI_FORMAT = 4;
        internal const int HDI_HEIGHT = 1;
        internal const int HDI_IMAGE = 0x20;
        internal const int HDI_LPARAM = 8;
        internal const int HDI_ORDER = 0x80;
        internal const int HDI_TEXT = 2;
        internal const int HDI_WIDTH = 1;
        internal const int HDM_GETITEM = 0x120b;
        internal const int HDM_SETITEM = 0x120c;
        internal const int HOLLOW_BRUSH = 5;
        internal const int HORZRES = 8;
        internal const int LARGE_ICON = 1;
        internal const int LOGPIXELSX = 0x58;
        internal const int LOGPIXELSY = 90;
        internal const int LVM_GETHEADER = 0x101f;
        internal const int OBJ_BRUSH = 2;
        internal const int OBJ_EXTPEN = 11;
        internal const int OBJ_PEN = 1;
        internal const int PHYSICALHEIGHT = 0x6f;
        internal const int PHYSICALOFFSETX = 0x70;
        internal const int PHYSICALOFFSETY = 0x71;
        internal const int PHYSICALWIDTH = 110;
        internal const int PS_COSMETIC = 0;
        internal const int PS_SOLID = 0;
        internal const int PS_USERSTYLE = 7;
        internal const int SMALL_ICON = 0;
        internal const int SWP_NOACTIVATE = 0x10;
        internal const int SWP_NOSIZE = 1;
        internal const int SWP_NOZORDER = 4;
        internal const int TTDT_AUTOMATIC = 0;
        internal const int TTDT_AUTOPOP = 2;
        internal const int TTDT_INITIAL = 3;
        internal const int TTDT_RESHOW = 1;
        internal const int TTF_ABSOLUTE = 0x80;
        internal const int TTF_CENTERTIP = 2;
        internal const int TTF_DI_SETITEM = 0x8000;
        internal const int TTF_IDISHWND = 1;
        internal const int TTF_PARSELINKS = 0x1000;
        internal const int TTF_RTLREADING = 4;
        internal const int TTF_SUBCLASS = 0x10;
        internal const int TTF_TRACK = 0x20;
        internal const int TTF_TRANSPARENT = 0x100;
        internal const int TTI_ERROR = 3;
        internal const int TTI_INFO = 1;
        internal const int TTI_NONE = 0;
        internal const int TTI_WARNING = 2;
        internal static readonly int TTM_ACTIVATE = 0x401;
        internal static readonly int TTM_ADDTOOL;
        private const int TTM_ADDTOOLA = 0x404;
        private const int TTM_ADDTOOLW = 0x432;
        internal static readonly int TTM_ADJUSTRECT = 0x41f;
        internal static readonly int TTM_DELTOOL;
        private const int TTM_DELTOOLA = 0x405;
        private const int TTM_DELTOOLW = 0x433;
        internal static readonly int TTM_ENUMTOOLS;
        private const int TTM_ENUMTOOLSA = 0x40e;
        private const int TTM_ENUMTOOLSW = 0x43a;
        internal static readonly int TTM_GETCURRENTTOOL;
        private const int TTM_GETCURRENTTOOLA = 0x40f;
        private const int TTM_GETCURRENTTOOLW = 0x43b;
        internal static readonly int TTM_GETDELAYTIME = 0x415;
        internal static readonly int TTM_GETTEXT;
        private const int TTM_GETTEXTA = 0x40b;
        private const int TTM_GETTEXTW = 0x438;
        internal static readonly int TTM_GETTOOLINFO;
        private const int TTM_GETTOOLINFOA = 0x408;
        private const int TTM_GETTOOLINFOW = 0x435;
        internal static readonly int TTM_HITTEST;
        private const int TTM_HITTESTA = 0x40a;
        private const int TTM_HITTESTW = 0x437;
        internal static readonly int TTM_NEWTOOLRECT;
        private const int TTM_NEWTOOLRECTA = 0x406;
        private const int TTM_NEWTOOLRECTW = 0x434;
        internal static readonly int TTM_POP = 0x41c;
        internal static readonly int TTM_RELAYEVENT = 0x407;
        internal static readonly int TTM_SETDELAYTIME = 0x403;
        internal static readonly int TTM_SETMAXTIPWIDTH = 0x418;
        internal static readonly int TTM_SETTITLE;
        private const int TTM_SETTITLEA = 0x420;
        private const int TTM_SETTITLEW = 0x421;
        internal static readonly int TTM_SETTOOLINFO;
        private const int TTM_SETTOOLINFOA = 0x409;
        private const int TTM_SETTOOLINFOW = 0x436;
        internal static readonly int TTM_TRACKACTIVATE = 0x411;
        internal static readonly int TTM_TRACKPOSITION = 0x412;
        internal static readonly int TTM_UPDATE = 0x41d;
        internal static readonly int TTM_UPDATETIPTEXT;
        private const int TTM_UPDATETIPTEXTA = 0x40c;
        private const int TTM_UPDATETIPTEXTW = 0x439;
        internal static readonly int TTM_WINDOWFROMPOINT = 0x410;
        internal static readonly int TTN_GETDISPINFO;
        private const int TTN_GETDISPINFOA = -520;
        private const int TTN_GETDISPINFOW = -530;
        internal static readonly int TTN_NEEDTEXT;
        private const int TTN_NEEDTEXTA = -520;
        private const int TTN_NEEDTEXTW = -530;
        internal static readonly int TTN_POP = -522;
        internal static readonly int TTN_SHOW = -521;
        internal const int TTS_ALWAYSTIP = 1;
        internal const int TTS_BALLOON = 0x40;
        internal const int TTS_CLOSE = 0x80;
        internal const int TTS_NOANIMATE = 0x10;
        internal const int TTS_NOFADE = 0x20;
        internal const int TTS_NOPREFIX = 2;
        internal const int VERTRES = 10;
        internal const int WM_KEYDOWN = 0x100;
        internal const int WM_KEYUP = 0x101;
        internal const int WM_NOTIFY = 0x4e;
        internal const int WM_SETFONT = 0x30;
        internal const int WM_SETICON = 0x80;
        internal const int WM_SETREDRAW = 11;
        internal const int WM_SYSKEYDOWN = 260;
        internal const int WM_SYSKEYUP = 0x105;
        internal const int WS_EX_DLGMODALFRAME = 1;
        internal const int WS_EX_TOPMOST = 8;
        internal const int WS_POPUP = -2147483648;

        static NativeMethods()
        {
            if (Marshal.SystemDefaultCharSize == 1)
            {
                TTN_GETDISPINFO = -520;
                TTN_NEEDTEXT = -520;
                TTM_ADDTOOL = 0x404;
                TTM_SETTITLE = 0x420;
                TTM_DELTOOL = 0x405;
                TTM_NEWTOOLRECT = 0x406;
                TTM_GETTOOLINFO = 0x408;
                TTM_SETTOOLINFO = 0x409;
                TTM_HITTEST = 0x40a;
                TTM_GETTEXT = 0x40b;
                TTM_UPDATETIPTEXT = 0x40c;
                TTM_ENUMTOOLS = 0x40e;
                TTM_GETCURRENTTOOL = 0x40f;
            }
            else
            {
                TTN_GETDISPINFO = -530;
                TTN_NEEDTEXT = -530;
                TTM_ADDTOOL = 0x432;
                TTM_SETTITLE = 0x421;
                TTM_DELTOOL = 0x433;
                TTM_NEWTOOLRECT = 0x434;
                TTM_GETTOOLINFO = 0x435;
                TTM_SETTOOLINFO = 0x436;
                TTM_HITTEST = 0x437;
                TTM_GETTEXT = 0x438;
                TTM_UPDATETIPTEXT = 0x439;
                TTM_ENUMTOOLS = 0x43a;
                TTM_GETCURRENTTOOL = 0x43b;
            }
        }

        [DllImport("gdi32", CharSet=CharSet.Auto)]
        internal static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int DeleteObject(HandleRef hObject);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr ExtCreatePen(int style, int nWidth, LOGBRUSH logbrush, int styleArrayLength, int[] styleArray);
        internal static bool Failed(int hr)
        {
            return (hr < 0);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr GetCurrentObject(HandleRef hDC, uint uObjectType);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);
        internal static bool Header_GetItem(IntPtr hWndHeader, int index, [In, Out] HDITEM hdi)
        {
            if (!(SendMessage(hWndHeader, 0x120b, new IntPtr(index), hdi) != IntPtr.Zero))
            {
                return false;
            }
            return true;
        }

        internal static bool Header_SetItem(IntPtr hWndHeader, int index, [In, Out] HDITEM hdi)
        {
            if (!(SendMessage(hWndHeader, 0x120c, new IntPtr(index), hdi) != IntPtr.Zero))
            {
                return false;
            }
            return true;
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool LineTo(HandleRef hdc, int x, int y);
        internal static IntPtr ListView_GetHeader(IntPtr hWndLV)
        {
            return SendMessage(hWndLV, 0x101f, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool MoveToEx(HandleRef hdc, int x, int y, POINT pt);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr SelectObject(HandleRef hdc, HandleRef obj);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, [In, Out] HDITEM lParam);
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref RECT rc);
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TOOLINFO ti);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetGraphicsMode(HandleRef hdc, int iMode);
        [DllImport("user32.dll")]
        internal static extern int SetWindowPos(IntPtr hWnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, int flags);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetWorldTransform(HandleRef hdc, XFORM xform);
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static int ThrowOnFailure(int hr)
        {
            return ThrowOnFailure(hr, null);
        }

        internal static int ThrowOnFailure(int hr, params int[] expectedHRFailure)
        {
            if (Failed(hr) && ((expectedHRFailure == null) || (Array.IndexOf<int>(expectedHRFailure, hr) < 0)))
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            return hr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto), ComVisible(false)]
        internal class HDITEM
        {
            public int mask;
            public int cxy;
            public IntPtr pszText = IntPtr.Zero;
            public IntPtr hbm = IntPtr.Zero;
            public int cchTextMax;
            public int fmt;
            public int lParam;
            public int image;
            public int order;
            public int type;
            public IntPtr filter = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class LOGBRUSH
        {
            public int lbStyle;
            public int lbColor;
            public long lbHatch;
            public LOGBRUSH(int style, int color, int hatch)
            {
                this.lbStyle = style;
                this.lbColor = color;
                this.lbHatch = hatch;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        internal class NMHDR
        {
            public IntPtr hwndFrom = IntPtr.Zero;
            public int idFrom = 0;
            public int code = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        internal struct TOOLINFO
        {
            public int size;
            public int flags;
            public IntPtr hwnd;
            public IntPtr id;
            public System.Workflow.Interop.NativeMethods.RECT rect;
            public IntPtr hinst;
            public IntPtr text;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class XFORM
        {
            public float eM11;
            public float eM12;
            public float eM21;
            public float eM22;
            public float eDx;
            public float eDy;
            public XFORM()
            {
                this.eM11 = 1f;
                this.eM22 = 1f;
            }

            public XFORM(Matrix transform)
            {
                this.eM11 = 1f;
                this.eM22 = 1f;
                this.eM11 = transform.Elements[0];
                this.eM12 = transform.Elements[1];
                this.eM21 = transform.Elements[2];
                this.eM22 = transform.Elements[3];
                this.eDx = transform.Elements[4];
                this.eDy = transform.Elements[5];
            }
        }
    }
}

