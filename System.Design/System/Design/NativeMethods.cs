namespace System.Design
{
    using System;
    using System.Drawing;
    using System.Internal;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal static class NativeMethods
    {
        public const int BM_SETIMAGE = 0xf7;
        public const int BS_ICON = 0x40;
        public const int CHILDID_SELF = 0;
        public const int CS_DBLCLKS = 8;
        public const int CS_DROPSHADOW = 0x20000;
        public const int CWP_SKIPINVISIBLE = 1;
        public const int DLGC_WANTALLKEYS = 4;
        public const int DT_CALCRECT = 0x400;
        public const int E_FAIL = -2147467259;
        public const int E_INVALIDARG = -2147024809;
        public const int E_NOINTERFACE = -2147467262;
        public const int E_NOTIMPL = -2147467263;
        public const int GW_CHILD = 5;
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_MAX = 5;
        public const int GW_OWNER = 4;
        public const int GWL_EXSTYLE = -20;
        public const int GWL_HWNDPARENT = -8;
        public const int GWL_STYLE = -16;
        public const int HC_ACTION = 0;
        public const int HDM_CREATEDRAGIMAGE = 0x1210;
        public const int HDM_GETIMAGELIST = 0x1209;
        public const int HDM_GETITEMRECT = 0x1207;
        public const int HDM_GETORDERARRAY = 0x1211;
        public const int HDM_HITTEST = 0x1206;
        public const int HDM_ORDERTOINDEX = 0x120f;
        public const int HDM_SETHOTDIVIDER = 0x1213;
        public const int HDM_SETIMAGELIST = 0x1208;
        public const int HDM_SETORDERARRAY = 0x1212;
        public const int HDN_BEGINDRAG = -310;
        public const int HDN_BEGINTRACKA = -306;
        public const int HDN_BEGINTRACKW = -326;
        public const int HDN_DIVIDERDBLCLICKA = -305;
        public const int HDN_DIVIDERDBLCLICKW = -325;
        public const int HDN_ENDDRAG = -311;
        public static readonly int HDN_ENDTRACK = ((Marshal.SystemDefaultCharSize == 1) ? -307 : -327);
        public const int HDN_ENDTRACKA = -307;
        public const int HDN_ENDTRACKW = -327;
        public const int HDN_GETDISPINFOA = -309;
        public const int HDN_GETDISPINFOW = -329;
        public const int HDN_ITEMCHANGEDA = -301;
        public const int HDN_ITEMCHANGEDW = -321;
        public const int HDN_ITEMCHANGINGA = -300;
        public const int HDN_ITEMCHANGINGW = -320;
        public const int HDN_ITEMCLICKA = -302;
        public const int HDN_ITEMCLICKW = -322;
        public const int HDN_ITEMDBLCLICKA = -303;
        public const int HDN_ITEMDBLCLICKW = -323;
        public const int HDN_TRACKA = -308;
        public const int HDN_TRACKW = -328;
        public const int HHT_ABOVE = 0x100;
        public const int HHT_BELOW = 0x200;
        public const int HHT_ONDIVIDER = 4;
        public const int HHT_ONDIVOPEN = 8;
        public const int HHT_ONHEADER = 2;
        public const int HHT_TOLEFT = 0x800;
        public const int HHT_TORIGHT = 0x400;
        public const int HIST_BACK = 0;
        public const int HOLLOW_BRUSH = 5;
        public const int HTCAPTION = 2;
        public const int HTCLIENT = 1;
        public const int HTERROR = -2;
        public const int HTGROWBOX = 4;
        public const int HTHSCROLL = 6;
        public const int HTNOWHERE = 0;
        public const int HTSIZE = 4;
        public const int HTSYSMENU = 3;
        public const int HTTRANSPARENT = -1;
        public const int HTVSCROLL = 7;
        public const int HWND_BOTTOM = 1;
        public const int HWND_NOTOPMOST = -2;
        public const int HWND_TOP = 0;
        public const int HWND_TOPMOST = -1;
        public const int IMAGE_ICON = 1;
        public static IntPtr InvalidIntPtr = ((IntPtr) (-1));
        public const int LB_ADDFILE = 0x196;
        public const int LB_ADDSTRING = 0x180;
        public const int LB_DELETESTRING = 0x182;
        public const int LB_DIR = 0x18d;
        public const int LB_FINDSTRING = 0x18f;
        public const int LB_FINDSTRINGEXACT = 0x1a2;
        public const int LB_GETANCHORINDEX = 0x19d;
        public const int LB_GETCARETINDEX = 0x19f;
        public const int LB_GETCOUNT = 0x18b;
        public const int LB_GETCURSEL = 0x188;
        public const int LB_GETHORIZONTALEXTENT = 0x193;
        public const int LB_GETITEMDATA = 0x199;
        public const int LB_GETITEMHEIGHT = 0x1a1;
        public const int LB_GETITEMRECT = 0x198;
        public const int LB_GETLOCALE = 0x1a6;
        public const int LB_GETSEL = 0x187;
        public const int LB_GETSELCOUNT = 400;
        public const int LB_GETSELITEMS = 0x191;
        public const int LB_GETTEXT = 0x189;
        public const int LB_GETTEXTLEN = 0x18a;
        public const int LB_GETTOPINDEX = 0x18e;
        public const int LB_INITSTORAGE = 0x1a8;
        public const int LB_INSERTSTRING = 0x181;
        public const int LB_ITEMFROMPOINT = 0x1a9;
        public const int LB_MSGMAX = 0x1b0;
        public const int LB_RESETCONTENT = 0x184;
        public const int LB_SELECTSTRING = 0x18c;
        public const int LB_SELITEMRANGE = 0x19b;
        public const int LB_SELITEMRANGEEX = 0x183;
        public const int LB_SETANCHORINDEX = 0x19c;
        public const int LB_SETCARETINDEX = 0x19e;
        public const int LB_SETCOLUMNWIDTH = 0x195;
        public const int LB_SETCOUNT = 0x1a7;
        public const int LB_SETCURSEL = 390;
        public const int LB_SETHORIZONTALEXTENT = 0x194;
        public const int LB_SETITEMDATA = 410;
        public const int LB_SETITEMHEIGHT = 0x1a0;
        public const int LB_SETLOCALE = 0x1a5;
        public const int LB_SETSEL = 0x185;
        public const int LB_SETTABSTOPS = 0x192;
        public const int LB_SETTOPINDEX = 0x197;
        public const int LVM_CREATEDRAGIMAGE = 0x1021;
        public const int LVM_GETCOUNTPERPAGE = 0x1028;
        public const int LVM_GETEXTENDEDLISTVIEWSTYLE = 0x1037;
        public const int LVM_GETHEADER = 0x101f;
        public const int LVM_GETISEARCHSTRINGA = 0x1034;
        public const int LVM_GETISEARCHSTRINGW = 0x1075;
        public const int LVM_GETITEMSPACING = 0x1033;
        public const int LVM_GETITEMSTATE = 0x102c;
        public const int LVM_GETITEMTEXTA = 0x102d;
        public const int LVM_GETITEMTEXTW = 0x1073;
        public const int LVM_GETORIGIN = 0x1029;
        public const int LVM_GETSELECTEDCOUNT = 0x1032;
        public const int LVM_GETTEXTBKCOLOR = 0x1025;
        public const int LVM_GETTEXTCOLOR = 0x1023;
        public const int LVM_GETTOPINDEX = 0x1027;
        public const int LVM_GETVIEWRECT = 0x1022;
        public const int LVM_SETCOLUMNWIDTH = 0x101e;
        public const int LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036;
        public const int LVM_SETICONSPACING = 0x1035;
        public const int LVM_SETITEMCOUNT = 0x102f;
        public const int LVM_SETITEMPOSITION32 = 0x1031;
        public const int LVM_SETITEMSTATE = 0x102b;
        public const int LVM_SETITEMTEXTA = 0x102e;
        public const int LVM_SETITEMTEXTW = 0x1074;
        public const int LVM_SETTEXTBKCOLOR = 0x1026;
        public const int LVM_SETTEXTCOLOR = 0x1024;
        public const int LVM_SORTITEMS = 0x1030;
        public const int LVM_UPDATE = 0x102a;
        public const int LVS_EX_DOUBLEBUFFER = 0x10000;
        public const int LVS_EX_GRIDLINES = 1;
        public const int LVSICF_NOINVALIDATEALL = 1;
        public const int LVSICF_NOSCROLL = 2;
        public const int MK_CONTROL = 8;
        public const int MK_LBUTTON = 1;
        public const int MK_MBUTTON = 0x10;
        public const int MK_RBUTTON = 2;
        public const int MK_SHIFT = 4;
        public const int MK_XBUTTON1 = 0x20;
        public const int MK_XBUTTON2 = 0x40;
        public const int MWMO_INPUTAVAILABLE = 4;
        public const int NM_CLICK = -2;
        public const int NOTSRCCOPY = 0x330008;
        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        public const int OBJID_CLIENT = -4;
        public const int OBJID_WINDOW = 0;
        public const int OLECLOSE_NOSAVE = 1;
        public const int OLECLOSE_PROMPTSAVE = 2;
        public const int OLECLOSE_SAVEIFDIRTY = 0;
        public const int OLEIVERB_DISCARDUNDOSTATE = -6;
        public const int OLEIVERB_HIDE = -3;
        public const int OLEIVERB_INPLACEACTIVATE = -5;
        public const int OLEIVERB_OPEN = -2;
        public const int OLEIVERB_PRIMARY = 0;
        public const int OLEIVERB_PROPERTIES = -7;
        public const int OLEIVERB_SHOW = -1;
        public const int OLEIVERB_UIACTIVATE = -4;
        public const int PM_NOREMOVE = 0;
        public const int PM_REMOVE = 1;
        public const int PRF_CHILDREN = 0x10;
        public const int PRF_CLIENT = 4;
        public const int PRF_ERASEBKGND = 8;
        public const int PRF_NONCLIENT = 2;
        public static int PS_SOLID = 0;
        public const int QS_ALLEVENTS = 0xbf;
        public const int QS_ALLINPUT = 0xff;
        public const int QS_ALLPOSTMESSAGE = 0x100;
        public const int QS_HOTKEY = 0x80;
        public const int QS_INPUT = 7;
        public const int QS_KEY = 1;
        public const int QS_MOUSE = 6;
        public const int QS_MOUSEBUTTON = 4;
        public const int QS_MOUSEMOVE = 2;
        public const int QS_PAINT = 0x20;
        public const int QS_POSTMESSAGE = 8;
        public const int QS_SENDMESSAGE = 0x40;
        public const int QS_TIMER = 0x10;
        public const int RDW_FRAME = 0x400;
        public const int RECO_DROP = 1;
        public const int RECO_PASTE = 0;
        public const int S_FALSE = 1;
        public const int S_OK = 0;
        public const int SB_BOTH = 3;
        public const int SB_BOTTOM = 7;
        public const int SB_CTL = 2;
        public const int SB_ENDSCROLL = 8;
        public const int SB_HORZ = 0;
        public const int SB_LEFT = 6;
        public const int SB_LINEDOWN = 1;
        public const int SB_LINELEFT = 0;
        public const int SB_LINERIGHT = 1;
        public const int SB_LINEUP = 0;
        public const int SB_PAGEDOWN = 3;
        public const int SB_PAGELEFT = 2;
        public const int SB_PAGERIGHT = 3;
        public const int SB_PAGEUP = 2;
        public const int SB_RIGHT = 7;
        public const int SB_THUMBPOSITION = 4;
        public const int SB_THUMBTRACK = 5;
        public const int SB_TOP = 6;
        public const int SB_VERT = 1;
        public const int SPI_GETNONCLIENTMETRICS = 0x29;
        public const int SRCCOPY = 0xcc0020;
        public const int STGM_CONVERT = 0x20000;
        public const int STGM_CREATE = 0x1000;
        public const int STGM_DELETEONRELEASE = 0x4000000;
        public const int STGM_READ = 0;
        public const int STGM_READWRITE = 2;
        public const int STGM_SHARE_EXCLUSIVE = 0x10;
        public const int STGM_TRANSACTED = 0x10000;
        public const int STGM_WRITE = 1;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_DRAWFRAME = 0x20;
        public const int SWP_FRAMECHANGED = 0x20;
        public const int SWP_HIDEWINDOW = 0x80;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_NOCOPYBITS = 0x100;
        public const int SWP_NOMOVE = 2;
        public const int SWP_NOOWNERZORDER = 0x200;
        public const int SWP_NOREDRAW = 8;
        public const int SWP_NOREPOSITION = 0x200;
        public const int SWP_NOSENDCHANGING = 0x400;
        public const int SWP_NOSIZE = 1;
        public const int SWP_NOZORDER = 4;
        public const int SWP_SHOWWINDOW = 0x40;
        public const int TCM_HITTEST = 0x130d;
        public static int TME_HOVER = 1;
        public const int TV_FIRST = 0x1100;
        public const int TVGN_ROOT = 0;
        public const int TVHT_ABOVE = 0x100;
        public const int TVHT_BELOW = 0x200;
        public const int TVHT_ONITEMBUTTON = 0x10;
        public const int TVHT_ONITEMICON = 2;
        public const int TVHT_ONITEMINDENT = 8;
        public const int TVHT_ONITEMLABEL = 4;
        public const int TVHT_ONITEMRIGHT = 0x20;
        public const int TVHT_ONITEMSTATEICON = 0x40;
        public const int TVHT_TOLEFT = 0x800;
        public const int TVHT_TORIGHT = 0x400;
        public const int TVM_GETCOUNT = 0x1105;
        public const int TVM_GETEXTENDEDSTYLE = 0x112d;
        public const int TVM_GETIMAGELIST = 0x1108;
        public const int TVM_GETINDENT = 0x1106;
        public const int TVM_GETITEMRECT = 0x1104;
        public const int TVM_GETNEXTITEM = 0x110a;
        public const int TVM_HITTEST = 0x1111;
        public const int TVM_SETEXTENDEDSTYLE = 0x112c;
        public const int TVM_SETIMAGELIST = 0x1109;
        public const int TVM_SETINDENT = 0x1107;
        public const int TVS_EX_DOUBLEBUFFER = 4;
        public const int TVS_EX_FADEINOUTEXPANDOS = 0x40;
        public const int TVSIL_NORMAL = 0;
        public const int TVSIL_STATE = 2;
        public const string uuid_IAccessible = "{618736E0-3C3D-11CF-810C-00AA00389B71}";
        public const string uuid_IEnumVariant = "{00020404-0000-0000-C000-000000000046}";
        public const int VK_PROCESSKEY = 0xe5;
        public const int WA_ACTIVE = 1;
        public const int WA_INACTIVE = 0;
        public const int WH_MOUSE = 7;
        public const int WM_ACTIVATE = 6;
        public const int WM_CANCELMODE = 0x1f;
        public const int WM_CAPTURECHANGED = 0x215;
        public const int WM_CHAR = 0x102;
        public const int WM_CLOSE = 0x10;
        public const int WM_CONTEXTMENU = 0x7b;
        public const int WM_CREATE = 1;
        public const int WM_DEADCHAR = 0x103;
        public const int WM_DESTROY = 2;
        public const int WM_GETDLGCODE = 0x87;
        public const int WM_GETOBJECT = 0x3d;
        public const int WM_HSCROLL = 0x114;
        public const int WM_IME_COMPOSITION = 0x10f;
        public const int WM_IME_ENDCOMPOSITION = 270;
        public const int WM_IME_STARTCOMPOSITION = 0x10d;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYFIRST = 0x100;
        public const int WM_KEYLAST = 0x108;
        public const int WM_KEYUP = 0x101;
        public const int WM_KILLFOCUS = 8;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_MBUTTONDBLCLK = 0x209;
        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_MBUTTONUP = 520;
        public const int WM_MOUSEACTIVATE = 0x21;
        public static readonly int WM_MOUSEENTER = Util.RegisterWindowMessage("WinFormsMouseEnter");
        public const int WM_MOUSEFIRST = 0x200;
        public const int WM_MOUSEHOVER = 0x2a1;
        public const int WM_MOUSELAST = 0x20a;
        public const int WM_MOUSELEAVE = 0x2a3;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_MOUSEWHEEL = 0x20a;
        public const int WM_NCACTIVATE = 0x86;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_NCLBUTTONDBLCLK = 0xa3;
        public const int WM_NCLBUTTONDOWN = 0xa1;
        public const int WM_NCLBUTTONUP = 0xa2;
        public const int WM_NCMBUTTONDBLCLK = 0xa9;
        public const int WM_NCMBUTTONDOWN = 0xa7;
        public const int WM_NCMBUTTONUP = 0xa8;
        public const int WM_NCMOUSEHOVER = 0x2a0;
        public const int WM_NCMOUSELEAVE = 0x2a2;
        public const int WM_NCMOUSEMOVE = 160;
        public const int WM_NCPAINT = 0x85;
        public const int WM_NCRBUTTONDBLCLK = 0xa6;
        public const int WM_NCRBUTTONDOWN = 0xa4;
        public const int WM_NCRBUTTONUP = 0xa5;
        public const int WM_NCXBUTTONDBLCLK = 0xad;
        public const int WM_NCXBUTTONDOWN = 0xab;
        public const int WM_NCXBUTTONUP = 0xac;
        public const int WM_NOTIFY = 0x4e;
        public const int WM_PAINT = 15;
        public const int WM_PARENTNOTIFY = 0x210;
        public const int WM_PRINT = 0x317;
        public const int WM_PRINTCLIENT = 0x318;
        public const int WM_RBUTTONDBLCLK = 0x206;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_REFLECT = 0x2000;
        public const int WM_SETCURSOR = 0x20;
        public const int WM_SETFOCUS = 7;
        public const int WM_SETREDRAW = 11;
        public const int WM_SHOWWINDOW = 0x18;
        public const int WM_SIZE = 5;
        public const int WM_STYLECHANGED = 0x7d;
        public const int WM_SYSCHAR = 0x106;
        public const int WM_SYSDEADCHAR = 0x107;
        public const int WM_SYSKEYDOWN = 260;
        public const int WM_SYSKEYUP = 0x105;
        public const int WM_TIMER = 0x113;
        public const int WM_USER = 0x400;
        public const int WM_VSCROLL = 0x115;
        public const int WM_WINDOWPOSCHANGED = 0x47;
        public const int WM_WINDOWPOSCHANGING = 70;
        public const int WS_BORDER = 0x800000;
        public const int WS_CLIPCHILDREN = 0x2000000;
        public const int WS_CLIPSIBLINGS = 0x4000000;
        public const int WS_DISABLED = 0x8000000;
        public const int WS_EX_LAYOUTRTL = 0x400000;
        public const int WS_EX_STATICEDGE = 0x20000;
        public const int WS_EX_TOOLWINDOW = 0x80;
        public const int WS_POPUP = -2147483648;

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr ChildWindowFromPointEx(IntPtr hwndParent, int x, int y, int uFlags);
        public static IntPtr CreateRectRgn(int x1, int y1, int x2, int y2)
        {
            return System.Internal.HandleCollector.Add(IntCreateRectRgn(x1, y1, x2, y2), CommonHandles.GDI);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public static bool DeleteObject(IntPtr hObject)
        {
            System.Internal.HandleCollector.Remove(hObject, CommonHandles.GDI);
            return IntDeleteObject(hObject);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int DispatchMessage([In] ref MSG msg);
        [DllImport("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ExternalDeleteObject(HandleRef hObject);
        public static bool Failed(int hr)
        {
            return (hr < 0);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetClientRect(IntPtr hWnd, [In, Out] COMRECT rect);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetCursor();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetCursorPos([In, Out] POINT pt);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetFocus();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern short GetKeyState(int keyCode);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetUpdateRect(IntPtr hwnd, [In, Out] ref RECT rc, bool fErase);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetUpdateRgn(IntPtr hwnd, IntPtr hrgn, bool fErase);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetWindowRect(IntPtr hWnd, [In, Out] ref RECT rect);
        [DllImport("gdi32.dll", EntryPoint="CreateRectRgn", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntCreateRectRgn(int x1, int y1, int x2, int y2);
        [DllImport("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern bool IntDeleteObject(IntPtr hObject);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern ITypeLib LoadRegTypeLib(ref Guid clsid, short majorVersion, short minorVersion, int lcid);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern ITypeLib LoadTypeLib([In, MarshalAs(UnmanagedType.LPWStr)] string typelib);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref RECT rect, int cPoints);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] POINT pt, int cPoints);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int MultiByteToWideChar(int CodePage, int dwFlags, byte[] lpMultiByteStr, int cchMultiByte, char[] lpWideCharStr, int cchWideChar);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool PeekMessage([In, Out] ref MSG msg, IntPtr hwnd, int msgMin, int msgMax, int remove);
        [return: MarshalAs(UnmanagedType.BStr)]
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern string QueryPathOfRegTypeLib(ref Guid guid, short majorVersion, short minorVersion, int lcid);
        [DllImport("ole32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int RevokeDragDrop(IntPtr hwnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [In, Out] TV_HITTESTINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [In, Out] HDHITTESTINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);
        public static bool Succeeded(int hr)
        {
            return (hr >= 0);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool TranslateMessage([In, Out] ref MSG msg);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ValidateRect(IntPtr hwnd, IntPtr prect);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr WindowFromPoint(int x, int y);

        internal class ActiveX
        {
            public const int ALIGN_BOTTOM = 2;
            public const int ALIGN_LEFT = 3;
            public const int ALIGN_MAX = 4;
            public const int ALIGN_MIN = 0;
            public const int ALIGN_NO_CHANGE = 0;
            public const int ALIGN_RIGHT = 4;
            public const int ALIGN_TOP = 1;
            public const int DISPID_ABOUTBOX = -552;
            public const int DISPID_AMBIENT_APPEARANCE = -716;
            public const int DISPID_AMBIENT_AUTOCLIP = -715;
            public const int DISPID_AMBIENT_BACKCOLOR = -701;
            public const int DISPID_AMBIENT_DISPLAYASDEFAULT = -713;
            public const int DISPID_AMBIENT_DISPLAYNAME = -702;
            public const int DISPID_AMBIENT_FONT = -703;
            public const int DISPID_AMBIENT_FORECOLOR = -704;
            public const int DISPID_AMBIENT_LOCALEID = -705;
            public const int DISPID_AMBIENT_MESSAGEREFLECT = -706;
            public const int DISPID_AMBIENT_PALETTE = -726;
            public const int DISPID_AMBIENT_SCALEUNITS = -707;
            public const int DISPID_AMBIENT_SHOWGRABHANDLES = -711;
            public const int DISPID_AMBIENT_SHOWHATCHING = -712;
            public const int DISPID_AMBIENT_SUPPORTSMNEMONICS = -714;
            public const int DISPID_AMBIENT_TEXTALIGN = -708;
            public const int DISPID_AMBIENT_TRANSFERPRIORITY = -728;
            public const int DISPID_AMBIENT_UIDEAD = -710;
            public const int DISPID_AMBIENT_USERMODE = -709;
            public const int DISPID_APPEARANCE = -520;
            public const int DISPID_AUTOSIZE = -500;
            public const int DISPID_BACKCOLOR = -501;
            public const int DISPID_BACKSTYLE = -502;
            public const int DISPID_BORDERCOLOR = -503;
            public const int DISPID_BORDERSTYLE = -504;
            public const int DISPID_BORDERVISIBLE = -519;
            public const int DISPID_BORDERWIDTH = -505;
            public const int DISPID_CAPTION = -518;
            public const int DISPID_CLICK = -600;
            public const int DISPID_DBLCLICK = -601;
            public const int DISPID_Delete = -801;
            public const int DISPID_DOCLICK = -551;
            public const int DISPID_DRAWMODE = -507;
            public const int DISPID_DRAWSTYLE = -508;
            public const int DISPID_DRAWWIDTH = -509;
            public const int DISPID_ENABLED = -514;
            public const int DISPID_ERROREVENT = -608;
            public const int DISPID_FILLCOLOR = -510;
            public const int DISPID_FILLSTYLE = -511;
            public const int DISPID_FONT = -512;
            public const int DISPID_FORECOLOR = -513;
            public const int DISPID_HWND = -515;
            public const int DISPID_KEYDOWN = -602;
            public const int DISPID_KEYPRESS = -603;
            public const int DISPID_KEYUP = -604;
            public const int DISPID_MOUSEDOWN = -605;
            public const int DISPID_MOUSEICON = -522;
            public const int DISPID_MOUSEMOVE = -606;
            public const int DISPID_MOUSEPOINTER = -521;
            public const int DISPID_MOUSEUP = -607;
            public const int DISPID_Name = -800;
            public const int DISPID_Object = -802;
            public const int DISPID_Parent = -803;
            public const int DISPID_PICTURE = -523;
            public const int DISPID_READYSTATE = -525;
            public const int DISPID_READYSTATECHANGE = -609;
            public const int DISPID_REFRESH = -550;
            public const int DISPID_RIGHTTOLEFT = -611;
            public const int DISPID_TABSTOP = -516;
            public const int DISPID_TEXT = -517;
            public const int DISPID_UNKNOWN = -1;
            public const int DISPID_VALID = -524;
            public const int DISPID_VALUE = 0;
            public const int DVASPECT_CONTENT = 1;
            public const int DVASPECT_DOCPRINT = 8;
            public const int DVASPECT_ICON = 4;
            public const int DVASPECT_THUMBNAIL = 2;
            public const int GC_WCH_ALL = 4;
            public const int GC_WCH_CONTAINED = 3;
            public const int GC_WCH_CONTAINER = 2;
            public const int GC_WCH_FONLYNEXT = 0x10000000;
            public const int GC_WCH_FONLYPREV = 0x20000000;
            public const int GC_WCH_FREVERSEDIR = 0x8000000;
            public const int GC_WCH_FSELECTED = 0x40000000;
            public const int GC_WCH_SIBLING = 1;
            public static Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
            public const int OCM__BASE = 0x2000;
            public const int OLECONTF_EMBEDDINGS = 1;
            public const int OLECONTF_LINKS = 2;
            public const int OLECONTF_ONLYIFRUNNING = 0x10;
            public const int OLECONTF_ONLYUSER = 8;
            public const int OLECONTF_OTHERS = 4;
            public const int OLEMISC_ACTIVATEWHENVISIBLE = 0x100;
            public const int OLEMISC_ACTSLIKEBUTTON = 0x1000;
            public const int OLEMISC_ACTSLIKELABEL = 0x2000;
            public const int OLEMISC_ALIGNABLE = 0x8000;
            public const int OLEMISC_ALWAYSRUN = 0x800;
            public const int OLEMISC_CANLINKBYOLE1 = 0x20;
            public const int OLEMISC_CANTLINKINSIDE = 0x10;
            public const int OLEMISC_IGNOREACTIVATEWHENVISIBLE = 0x80000;
            public const int OLEMISC_IMEMODE = 0x40000;
            public const int OLEMISC_INSERTNOTREPLACE = 4;
            public const int OLEMISC_INSIDEOUT = 0x80;
            public const int OLEMISC_INVISIBLEATRUNTIME = 0x400;
            public const int OLEMISC_ISLINKOBJECT = 0x40;
            public const int OLEMISC_NOUIACTIVATE = 0x4000;
            public const int OLEMISC_ONLYICONIC = 2;
            public const int OLEMISC_RECOMPOSEONRESIZE = 1;
            public const int OLEMISC_RENDERINGISDEVICEINDEPENDENT = 0x200;
            public const int OLEMISC_SETCLIENTSITEFIRST = 0x20000;
            public const int OLEMISC_SIMPLEFRAME = 0x10000;
            public const int OLEMISC_STATIC = 8;
            public const int OLEMISC_SUPPORTSMULTILEVELUNDO = 0x200000;
            public const int OLEMISC_WANTSTOMENUMERGE = 0x100000;
            public const int OLEVERBATTRIB_NEVERDIRTIES = 1;
            public const int OLEVERBATTRIB_ONCONTAINERMENU = 2;
            public const int PROPCAT_Appearance = -5;
            public const int PROPCAT_Behavior = -6;
            public const int PROPCAT_Data = -7;
            public const int PROPCAT_DDE = -11;
            public const int PROPCAT_Font = -3;
            public const int PROPCAT_List = -8;
            public const int PROPCAT_Misc = -2;
            public const int PROPCAT_Nil = -1;
            public const int PROPCAT_Position = -4;
            public const int PROPCAT_Scale = -10;
            public const int PROPCAT_Text = -9;
            public const int QACONTAINER_AUTOCLIP = 0x20;
            public const int QACONTAINER_DISPLAYASDEFAULT = 8;
            public const int QACONTAINER_MESSAGEREFLECT = 0x40;
            public const int QACONTAINER_SHOWGRABHANDLES = 2;
            public const int QACONTAINER_SHOWHATCHING = 1;
            public const int QACONTAINER_SUPPORTSMNEMONICS = 0x80;
            public const int QACONTAINER_UIDEAD = 0x10;
            public const int QACONTAINER_USERMODE = 4;
            public const int XFORMCOORDS_CONTAINERTOHIMETRIC = 8;
            public const int XFORMCOORDS_HIMETRICTOCONTAINER = 4;
            public const int XFORMCOORDS_POSITION = 1;
            public const int XFORMCOORDS_SIZE = 2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class CHARRANGE
        {
            public int cpMin;
            public int cpMax;
        }

        public sealed class CommonHandles
        {
            public static readonly int Accelerator = System.Internal.HandleCollector.RegisterType("Accelerator", 80, 50);
            public static readonly int Cursor = System.Internal.HandleCollector.RegisterType("Cursor", 20, 500);
            public static readonly int EMF = System.Internal.HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);
            public static readonly int Find = System.Internal.HandleCollector.RegisterType("Find", 0, 0x3e8);
            public static readonly int GDI = System.Internal.HandleCollector.RegisterType("GDI", 90, 50);
            public static readonly int HDC = System.Internal.HandleCollector.RegisterType("HDC", 100, 2);
            public static readonly int Icon = System.Internal.HandleCollector.RegisterType("Icon", 20, 500);
            public static readonly int Kernel = System.Internal.HandleCollector.RegisterType("Kernel", 0, 0x3e8);
            public static readonly int Menu = System.Internal.HandleCollector.RegisterType("Menu", 30, 0x3e8);
            public static readonly int Window = System.Internal.HandleCollector.RegisterType("Window", 5, 0x3e8);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class COMRECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public COMRECT()
            {
            }

            public COMRECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
        }

        public class DOCHOSTUIDBLCLICK
        {
            public const int DEFAULT = 0;
            public const int SHOWCODE = 2;
            public const int SHOWPROPERTIES = 1;
        }

        public class DOCHOSTUIFLAG
        {
            public const int ACTIVATE_CLIENTHIT_ONLY = 0x200;
            public const int DIALOG = 1;
            public const int DISABLE_COOKIE = 0x400;
            public const int DISABLE_HELP_MENU = 2;
            public const int DISABLE_OFFSCREEN = 0x40;
            public const int DISABLE_SCRIPT_INACTIVE = 0x10;
            public const int DIV_BLOCKDEFAULT = 0x100;
            public const int FLAT_SCROLLBAR = 0x80;
            public const int NO3DBORDER = 4;
            public const int OPENNEWWIN = 0x20;
            public const int SCROLL_NO = 8;
        }

        [StructLayout(LayoutKind.Sequential), ComVisible(true)]
        public class DOCHOSTUIINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.I4)]
            public int dwFlags;
            [MarshalAs(UnmanagedType.I4)]
            public int dwDoubleClick;
            [MarshalAs(UnmanagedType.I4)]
            public int dwReserved1;
            [MarshalAs(UnmanagedType.I4)]
            public int dwReserved2;
        }

        public delegate bool EnumChildrenCallback(IntPtr hwnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class FORMATETC
        {
            [MarshalAs(UnmanagedType.I4)]
            public int cfFormat;
            [MarshalAs(UnmanagedType.I4)]
            public IntPtr ptd = IntPtr.Zero;
            [MarshalAs(UnmanagedType.I4)]
            public int dwAspect;
            [MarshalAs(UnmanagedType.I4)]
            public int lindex;
            [MarshalAs(UnmanagedType.I4)]
            public int tymed;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class HDHITTESTINFO
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public int iItem;
        }

        [ComImport, Guid("25336920-03F9-11CF-8FD0-00AA00686F13"), ComVisible(true)]
        public class HTMLDocument
        {
        }

        [ComImport, Guid("0000010F-0000-0000-C000-000000000046"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAdviseSink
        {
            void OnDataChange([In] System.Design.NativeMethods.FORMATETC pFormatetc, [In] System.Design.NativeMethods.STGMEDIUM pStgmed);
            void OnViewChange([In, MarshalAs(UnmanagedType.U4)] int dwAspect, [In, MarshalAs(UnmanagedType.I4)] int lindex);
            void OnRename([In, MarshalAs(UnmanagedType.Interface)] object pmk);
            void OnSave();
            void OnClose();
        }

        [ComImport, Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
        public interface IDocHostUIHandler
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowContextMenu([In, MarshalAs(UnmanagedType.U4)] int dwID, [In] System.Design.NativeMethods.POINT pt, [In, MarshalAs(UnmanagedType.Interface)] object pcmdtReserved, [In, MarshalAs(UnmanagedType.Interface)] object pdispReserved);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetHostInfo([In, Out] System.Design.NativeMethods.DOCHOSTUIINFO info);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowUI([In, MarshalAs(UnmanagedType.I4)] int dwID, [In] System.Design.NativeMethods.IOleInPlaceActiveObject activeObject, [In] System.Design.NativeMethods.IOleCommandTarget commandTarget, [In] System.Design.NativeMethods.IOleInPlaceFrame frame, [In] System.Design.NativeMethods.IOleInPlaceUIWindow doc);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int HideUI();
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int UpdateUI();
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int EnableModeless([In, MarshalAs(UnmanagedType.Bool)] bool fEnable);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnDocWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnFrameWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ResizeBorder([In] System.Design.NativeMethods.COMRECT rect, [In] System.Design.NativeMethods.IOleInPlaceUIWindow doc, bool fFrameWindow);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateAccelerator([In] ref System.Design.NativeMethods.MSG msg, [In] ref Guid group, [In, MarshalAs(UnmanagedType.I4)] int nCmdID);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetOptionKeyPath([Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrKey, [In, MarshalAs(UnmanagedType.U4)] int dw);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetDropTarget([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IOleDropTarget pDropTarget, [MarshalAs(UnmanagedType.Interface)] out System.Design.NativeMethods.IOleDropTarget ppDropTarget);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetExternal([MarshalAs(UnmanagedType.Interface)] out object ppDispatch);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateUrl([In, MarshalAs(UnmanagedType.U4)] int dwTranslate, [In, MarshalAs(UnmanagedType.LPWStr)] string strURLIn, [MarshalAs(UnmanagedType.LPWStr)] out string pstrURLOut);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int FilterDataObject(IDataObject pDO, out IDataObject ppDORet);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000103-0000-0000-C000-000000000046")]
        public interface IEnumFORMATETC
        {
            [PreserveSig]
            int Next([In, MarshalAs(UnmanagedType.U4)] int celt, [Out] System.Design.NativeMethods.FORMATETC rgelt, [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] pceltFetched);
            [PreserveSig]
            int Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
            [PreserveSig]
            int Reset();
            [PreserveSig]
            int Clone([Out, MarshalAs(UnmanagedType.LPArray)] System.Design.NativeMethods.IEnumFORMATETC[] ppenum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000104-0000-0000-C000-000000000046")]
        public interface IEnumOLEVERB
        {
            [PreserveSig]
            int Next([MarshalAs(UnmanagedType.U4)] int celt, [In, Out] System.Design.NativeMethods.tagOLEVERB rgelt, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pceltFetched);
            [PreserveSig]
            int Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
            void Reset();
            void Clone(out System.Design.NativeMethods.IEnumOLEVERB ppenum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000105-0000-0000-C000-000000000046")]
        public interface IEnumSTATDATA
        {
            void Next([In, MarshalAs(UnmanagedType.U4)] int celt, [Out] System.Design.NativeMethods.STATDATA rgelt, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pceltFetched);
            void Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
            void Reset();
            void Clone([Out, MarshalAs(UnmanagedType.LPArray)] System.Design.NativeMethods.IEnumSTATDATA[] ppenum);
        }

        [ComImport, Guid("3050F1D8-98B5-11CF-BB82-00AA00BDCE0B"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IHTMLBodyElement
        {
            void SetBackground([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackground();
            void SetBgProperties([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBgProperties();
            void SetLeftMargin([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLeftMargin();
            void SetTopMargin([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetTopMargin();
            void SetRightMargin([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetRightMargin();
            void SetBottomMargin([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBottomMargin();
            void SetNoWrap([In, MarshalAs(UnmanagedType.Bool)] bool p);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetNoWrap();
            void SetBgColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBgColor();
            void SetText([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetText();
            void SetLink([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLink();
            void SetVLink([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetVLink();
            void SetALink([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetALink();
            void SetOnload([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnload();
            void SetOnunload([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnunload();
            void SetScroll([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetScroll();
            void SetOnselect([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnselect();
            void SetOnbeforeunload([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnbeforeunload();
            [return: MarshalAs(UnmanagedType.Interface)]
            object CreateTextRange();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), Guid("3050F3DB-98B5-11CF-BB82-00AA00BDCE0B")]
        public interface IHTMLCurrentStyle
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPosition();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetStyleFloat();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetColor();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBackgroundColor();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontFamily();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontStyle();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontObject();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetFontWeight();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetFontSize();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundImage();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBackgroundPositionX();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBackgroundPositionY();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundRepeat();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderLeftColor();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderTopColor();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderRightColor();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderBottomColor();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderTopStyle();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderRightStyle();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderBottomStyle();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderLeftStyle();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderTopWidth();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderRightWidth();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderBottomWidth();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderLeftWidth();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLeft();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetTop();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetWidth();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetHeight();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPaddingLeft();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPaddingTop();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPaddingRight();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPaddingBottom();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextAlign();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextDecoration();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDisplay();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetVisibility();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetZIndex();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLetterSpacing();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLineHeight();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetTextIndent();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetVerticalAlign();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundAttachment();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetMarginTop();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetMarginRight();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetMarginBottom();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetMarginLeft();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetClear();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyleType();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStylePosition();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyleImage();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetClipTop();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetClipRight();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetClipBottom();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetClipLeft();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetOverflow();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPageBreakBefore();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPageBreakAfter();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCursor();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTableLayout();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderCollapse();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDirection();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBehavior();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, MarshalAs(UnmanagedType.I4)] int lFlags);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetUnicodeBidi();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetRight();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBottom();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("626FC520-A41E-11CF-A731-00A0C9082637"), ComVisible(true)]
        public interface IHTMLDocument
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetScript();
        }

        [ComImport, Guid("332C4425-26CB-11D0-B483-00C04FD90119"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IHTMLDocument2
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetScript();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetAll();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement GetBody();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement GetActiveElement();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetImages();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetApplets();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetLinks();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetForms();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetAnchors();
            void SetTitle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTitle();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetScripts();
            void SetDesignMode([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDesignMode();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetSelection();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetReadyState();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetFrames();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetEmbeds();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetPlugins();
            void SetAlinkColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetAlinkColor();
            void SetBgColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBgColor();
            void SetFgColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetFgColor();
            void SetLinkColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLinkColor();
            void SetVlinkColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetVlinkColor();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetReferrer();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetLocation();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetLastModified();
            void SetURL([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetURL();
            void SetDomain([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDomain();
            void SetCookie([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCookie();
            void SetExpando([In, MarshalAs(UnmanagedType.Bool)] bool p);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetExpando();
            void SetCharset([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCharset();
            void SetDefaultCharset([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDefaultCharset();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetMimeType();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFileSize();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFileCreatedDate();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFileModifiedDate();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFileUpdatedDate();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetSecurity();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetProtocol();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetNameProp();
            void DummyWrite([In, MarshalAs(UnmanagedType.I4)] int psarray);
            void DummyWriteln([In, MarshalAs(UnmanagedType.I4)] int psarray);
            [return: MarshalAs(UnmanagedType.Interface)]
            object Open([In, MarshalAs(UnmanagedType.BStr)] string URL, [In, MarshalAs(UnmanagedType.Struct)] object name, [In, MarshalAs(UnmanagedType.Struct)] object features, [In, MarshalAs(UnmanagedType.Struct)] object replace);
            void Close();
            void Clear();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool QueryCommandSupported([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool QueryCommandEnabled([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool QueryCommandState([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool QueryCommandIndeterm([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
            [return: MarshalAs(UnmanagedType.BStr)]
            string QueryCommandText([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
            [return: MarshalAs(UnmanagedType.Struct)]
            object QueryCommandValue([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool ExecCommand([In, MarshalAs(UnmanagedType.BStr)] string cmdID, [In, MarshalAs(UnmanagedType.Bool)] bool showUI, [In, MarshalAs(UnmanagedType.Struct)] object value);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool ExecCommandShowHelp([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement CreateElement([In, MarshalAs(UnmanagedType.BStr)] string eTag);
            void SetOnhelp([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnhelp();
            void SetOnclick([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnclick();
            void SetOndblclick([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndblclick();
            void SetOnkeyup([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnkeyup();
            void SetOnkeydown([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnkeydown();
            void SetOnkeypress([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnkeypress();
            void SetOnmouseup([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmouseup();
            void SetOnmousedown([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmousedown();
            void SetOnmousemove([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmousemove();
            void SetOnmouseout([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmouseout();
            void SetOnmouseover([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmouseover();
            void SetOnreadystatechange([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnreadystatechange();
            void SetOnafterupdate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnafterupdate();
            void SetOnrowexit([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnrowexit();
            void SetOnrowenter([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnrowenter();
            void SetOndragstart([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndragstart();
            void SetOnselectstart([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnselectstart();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement ElementFromPoint([In, MarshalAs(UnmanagedType.I4)] int x, [In, MarshalAs(UnmanagedType.I4)] int y);
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetParentWindow();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetStyleSheets();
            void SetOnbeforeupdate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnbeforeupdate();
            void SetOnerrorupdate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnerrorupdate();
            [return: MarshalAs(UnmanagedType.BStr)]
            string toString();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLStyleSheet CreateStyleSheet([In, MarshalAs(UnmanagedType.BStr)] string bstrHref, [In, MarshalAs(UnmanagedType.I4)] int lIndex);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), Guid("3050F5DA-98B5-11CF-BB82-00AA00BDCE0B")]
        public interface IHTMLDOMNode
        {
            [return: MarshalAs(UnmanagedType.I4)]
            int GetNodeType();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode GetParentNode();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool HasChildNodes();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetChildNodes();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetAttributes();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode InsertBefore([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLDOMNode newChild, [In, MarshalAs(UnmanagedType.Struct)] object refChild);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode RemoveChild([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLDOMNode oldChild);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode ReplaceChild([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLDOMNode newChild, [In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLDOMNode oldChild);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode CloneNode([In, MarshalAs(UnmanagedType.Bool)] bool fDeep);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode RemoveNode([In, MarshalAs(UnmanagedType.Bool)] bool fDeep);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode SwapNode([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLDOMNode otherNode);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode ReplaceNode([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLDOMNode replacement);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode AppendChild([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLDOMNode newChild);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetNodeName();
            void SetNodeValue([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetNodeValue();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode GetFirstChild();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode GetLastChild();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode GetPreviousSibling();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDOMNode GetNextSibling();
        }

        [ComImport, Guid("3050F1FF-98B5-11CF-BB82-00AA00BDCE0B"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IHTMLElement
        {
            void SetAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, MarshalAs(UnmanagedType.Struct)] object AttributeValue, [In, MarshalAs(UnmanagedType.I4)] int lFlags);
            void GetAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, MarshalAs(UnmanagedType.I4)] int lFlags, [Out, MarshalAs(UnmanagedType.LPArray)] object[] pvars);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool RemoveAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, MarshalAs(UnmanagedType.I4)] int lFlags);
            void SetClassName([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetClassName();
            void SetId([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetId();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTagName();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement GetParentElement();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLStyle GetStyle();
            void SetOnhelp([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnhelp();
            void SetOnclick([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnclick();
            void SetOndblclick([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndblclick();
            void SetOnkeydown([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnkeydown();
            void SetOnkeyup([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnkeyup();
            void SetOnkeypress([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnkeypress();
            void SetOnmouseout([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmouseout();
            void SetOnmouseover([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmouseover();
            void SetOnmousemove([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmousemove();
            void SetOnmousedown([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmousedown();
            void SetOnmouseup([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmouseup();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLDocument2 GetDocument();
            void SetTitle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTitle();
            void SetLanguage([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetLanguage();
            void SetOnselectstart([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnselectstart();
            void ScrollIntoView([In, MarshalAs(UnmanagedType.Struct)] object varargStart);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool Contains([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLElement pChild);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetSourceIndex();
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetRecordNumber();
            void SetLang([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetLang();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetOffsetLeft();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetOffsetTop();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetOffsetWidth();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetOffsetHeight();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement GetOffsetParent();
            void SetInnerHTML([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetInnerHTML();
            void SetInnerText([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetInnerText();
            void SetOuterHTML([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetOuterHTML();
            void SetOuterText([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetOuterText();
            void InsertAdjacentHTML([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.BStr)] string html);
            void InsertAdjacentText([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.BStr)] string text);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement GetParentTextEdit();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetIsTextEdit();
            void Click();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetFilters();
            void SetOndragstart([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndragstart();
            [return: MarshalAs(UnmanagedType.BStr)]
            string toString();
            void SetOnbeforeupdate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnbeforeupdate();
            void SetOnafterupdate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnafterupdate();
            void SetOnerrorupdate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnerrorupdate();
            void SetOnrowexit([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnrowexit();
            void SetOnrowenter([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnrowenter();
            void SetOndatasetchanged([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndatasetchanged();
            void SetOndataavailable([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndataavailable();
            void SetOndatasetcomplete([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndatasetcomplete();
            void SetOnfilterchange([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnfilterchange();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetChildren();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetAll();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("3050F434-98B5-11CF-BB82-00AA00BDCE0B")]
        public interface IHTMLElement2
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetScopeName();
            void SetCapture([In, MarshalAs(UnmanagedType.Bool)] bool containerCapture);
            void ReleaseCapture();
            void SetOnlosecapture([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnlosecapture();
            [return: MarshalAs(UnmanagedType.BStr)]
            string ComponentFromPoint([In, MarshalAs(UnmanagedType.I4)] int x, [In, MarshalAs(UnmanagedType.I4)] int y);
            void DoScroll([In, MarshalAs(UnmanagedType.Struct)] object component);
            void SetOnscroll([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnscroll();
            void SetOndrag([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndrag();
            void SetOndragend([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndragend();
            void SetOndragenter([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndragenter();
            void SetOndragover([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndragover();
            void SetOndragleave([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndragleave();
            void SetOndrop([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOndrop();
            void SetOnbeforecut([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnbeforecut();
            void SetOncut([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOncut();
            void SetOnbeforecopy([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnbeforecopy();
            void SetOncopy([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOncopy();
            void SetOnbeforepaste([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnbeforepaste();
            void SetOnpaste([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnpaste();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLCurrentStyle GetCurrentStyle();
            void SetOnpropertychange([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnpropertychange();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLRectCollection GetClientRects();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLRect GetBoundingClientRect();
            void SetExpression([In, MarshalAs(UnmanagedType.BStr)] string propname, [In, MarshalAs(UnmanagedType.BStr)] string expression, [In, MarshalAs(UnmanagedType.BStr)] string language);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetExpression([In, MarshalAs(UnmanagedType.BStr)] object propname);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool RemoveExpression([In, MarshalAs(UnmanagedType.BStr)] string propname);
            void SetTabIndex([In, MarshalAs(UnmanagedType.I2)] short p);
            [return: MarshalAs(UnmanagedType.I2)]
            short GetTabIndex();
            void Focus();
            void SetAccessKey([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetAccessKey();
            void SetOnblur([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnblur();
            void SetOnfocus([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnfocus();
            void SetOnresize([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnresize();
            void Blur();
            void AddFilter([In, MarshalAs(UnmanagedType.Interface)] object pUnk);
            void RemoveFilter([In, MarshalAs(UnmanagedType.Interface)] object pUnk);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetClientHeight();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetClientWidth();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetClientTop();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetClientLeft();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool AttachEvent([In, MarshalAs(UnmanagedType.BStr)] string ev, [In, MarshalAs(UnmanagedType.Interface)] object pdisp);
            void DetachEvent([In, MarshalAs(UnmanagedType.BStr)] string ev, [In, MarshalAs(UnmanagedType.Interface)] object pdisp);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetReadyState();
            void SetOnreadystatechange([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnreadystatechange();
            void SetOnrowsdelete([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnrowsdelete();
            void SetOnrowsinserted([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnrowsinserted();
            void SetOncellchange([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOncellchange();
            void SetDir([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDir();
            [return: MarshalAs(UnmanagedType.Interface)]
            object CreateControlRange();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetScrollHeight();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetScrollWidth();
            void SetScrollTop([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetScrollTop();
            void SetScrollLeft([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetScrollLeft();
            void ClearAttributes();
            void MergeAttributes([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLElement mergeThis);
            void SetOncontextmenu([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOncontextmenu();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement InsertAdjacentElement([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLElement insertedElement);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement ApplyElement([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLElement apply, [In, MarshalAs(UnmanagedType.BStr)] string where);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetAdjacentText([In, MarshalAs(UnmanagedType.BStr)] string where);
            [return: MarshalAs(UnmanagedType.BStr)]
            string ReplaceAdjacentText([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.BStr)] string newText);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetCanHaveChildren();
            [return: MarshalAs(UnmanagedType.I4)]
            int AddBehavior([In, MarshalAs(UnmanagedType.BStr)] string bstrUrl, [In] ref object pvarFactory);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool RemoveBehavior([In, MarshalAs(UnmanagedType.I4)] int cookie);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLStyle GetRuntimeStyle();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetBehaviorUrns();
            void SetTagUrn([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTagUrn();
            void SetOnbeforeeditfocus([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnbeforeeditfocus();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetReadyStateValue();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElementCollection GetElementsByTagName([In, MarshalAs(UnmanagedType.BStr)] string v);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLStyle GetBaseStyle();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLCurrentStyle GetBaseCurrentStyle();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLStyle GetBaseRuntimeStyle();
            void SetOnmousehover([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnmousehover();
            void SetOnkeydownpreview([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnkeydownpreview();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetBehavior([In, MarshalAs(UnmanagedType.BStr)] string bstrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrUrn);
        }

        [ComImport, Guid("3050F673-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
        public interface IHTMLElement3
        {
            void MergeAttributes([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IHTMLElement mergeThis, [In, MarshalAs(UnmanagedType.Struct)] object pvarFlags);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetIsMultiLine();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetCanHaveHTML();
            void SetOnLayoutComplete([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnLayoutComplete();
            void SetOnPage([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnPage();
            void SetInflateBlock([In, MarshalAs(UnmanagedType.Bool)] bool inflate);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetInflateBlock();
            void SetOnBeforeDeactivate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnBeforeDeactivate();
            void SetActive();
            void SetContentEditable([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetContentEditable();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetIsContentEditable();
            void SetHideFocus([In, MarshalAs(UnmanagedType.Bool)] bool v);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetHideFocus();
            void SetDisabled([In, MarshalAs(UnmanagedType.Bool)] bool v);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetDisabled();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetIsDisabled();
            void SetOnMove([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnMove();
            void SetOnControlSelect([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnControlSelect();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool FireEvent([In, MarshalAs(UnmanagedType.BStr)] string eventName, [In, MarshalAs(UnmanagedType.Struct)] object eventObject);
            void SetOnResizeStart([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnResizeStart();
            void SetOnResizeEnd([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnResizeEnd();
            void SetOnMoveStart([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnMoveStart();
            void SetOnMoveEnd([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnMoveEnd();
            void SetOnMouseEnter([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnMouseEnter();
            void SetOnMouseLeave([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnMouseLeave();
            void SetOnActivate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnActivate();
            void SetOnDeactivate([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetOnDeactivate();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool DragDrop();
            [return: MarshalAs(UnmanagedType.I4)]
            int GetGlyphMode();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("3050F21F-98B5-11CF-BB82-00AA00BDCE0B"), ComVisible(true)]
        public interface IHTMLElementCollection
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            string toString();
            void SetLength([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetLength();
            [return: MarshalAs(UnmanagedType.Interface)]
            object Get_newEnum();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement Item([In, MarshalAs(UnmanagedType.Struct)] object name, [In, MarshalAs(UnmanagedType.Struct)] object index);
            [return: MarshalAs(UnmanagedType.Interface)]
            object Tags([In, MarshalAs(UnmanagedType.Struct)] object tagName);
        }

        [ComImport, Guid("3050F4A3-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
        public interface IHTMLRect
        {
            void SetLeft([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetLeft();
            void SetTop([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetTop();
            void SetRight([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetRight();
            void SetBottom([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetBottom();
        }

        [ComImport, ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("3050F4A4-98B5-11CF-BB82-00AA00BDCE0B")]
        public interface IHTMLRectCollection
        {
            [return: MarshalAs(UnmanagedType.I4)]
            int GetLength();
            [return: MarshalAs(UnmanagedType.Interface)]
            object Get_newEnum();
            [return: MarshalAs(UnmanagedType.Struct)]
            object Item([In] ref object pvarIndex);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), Guid("3050F25E-98B5-11CF-BB82-00AA00BDCE0B")]
        public interface IHTMLStyle
        {
            void SetFontFamily([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontFamily();
            void SetFontStyle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontStyle();
            void SetFontObject([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontObject();
            void SetFontWeight([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontWeight();
            void SetFontSize([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetFontSize();
            void SetFont([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFont();
            void SetColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetColor();
            void SetBackground([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackground();
            void SetBackgroundColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBackgroundColor();
            void SetBackgroundImage([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundImage();
            void SetBackgroundRepeat([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundRepeat();
            void SetBackgroundAttachment([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundAttachment();
            void SetBackgroundPosition([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundPosition();
            void SetBackgroundPositionX([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBackgroundPositionX();
            void SetBackgroundPositionY([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBackgroundPositionY();
            void SetWordSpacing([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetWordSpacing();
            void SetLetterSpacing([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLetterSpacing();
            void SetTextDecoration([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextDecoration();
            void SetTextDecorationNone([In, MarshalAs(UnmanagedType.Bool)] bool p);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationNone();
            void SetTextDecorationUnderline([In, MarshalAs(UnmanagedType.Bool)] bool p);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationUnderline();
            void SetTextDecorationOverline([In, MarshalAs(UnmanagedType.Bool)] bool p);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationOverline();
            void SetTextDecorationLineThrough([In, MarshalAs(UnmanagedType.Bool)] bool p);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationLineThrough();
            void SetTextDecorationBlink([In, MarshalAs(UnmanagedType.Bool)] bool p);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationBlink();
            void SetVerticalAlign([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetVerticalAlign();
            void SetTextTransform([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextTransform();
            void SetTextAlign([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextAlign();
            void SetTextIndent([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetTextIndent();
            void SetLineHeight([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLineHeight();
            void SetMarginTop([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetMarginTop();
            void SetMarginRight([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetMarginRight();
            void SetMarginBottom([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetMarginBottom();
            void SetMarginLeft([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetMarginLeft();
            void SetMargin([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetMargin();
            void SetPaddingTop([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPaddingTop();
            void SetPaddingRight([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPaddingRight();
            void SetPaddingBottom([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPaddingBottom();
            void SetPaddingLeft([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPaddingLeft();
            void SetPadding([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPadding();
            void SetBorder([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorder();
            void SetBorderTop([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderTop();
            void SetBorderRight([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderRight();
            void SetBorderBottom([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderBottom();
            void SetBorderLeft([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderLeft();
            void SetBorderColor([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderColor();
            void SetBorderTopColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderTopColor();
            void SetBorderRightColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderRightColor();
            void SetBorderBottomColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderBottomColor();
            void SetBorderLeftColor([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderLeftColor();
            void SetBorderWidth([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderWidth();
            void SetBorderTopWidth([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderTopWidth();
            void SetBorderRightWidth([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderRightWidth();
            void SetBorderBottomWidth([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderBottomWidth();
            void SetBorderLeftWidth([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetBorderLeftWidth();
            void SetBorderStyle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderStyle();
            void SetBorderTopStyle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderTopStyle();
            void SetBorderRightStyle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderRightStyle();
            void SetBorderBottomStyle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderBottomStyle();
            void SetBorderLeftStyle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderLeftStyle();
            void SetWidth([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetWidth();
            void SetHeight([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetHeight();
            void SetStyleFloat([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetStyleFloat();
            void SetClear([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetClear();
            void SetDisplay([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDisplay();
            void SetVisibility([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetVisibility();
            void SetListStyleType([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyleType();
            void SetListStylePosition([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStylePosition();
            void SetListStyleImage([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyleImage();
            void SetListStyle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyle();
            void SetWhiteSpace([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetWhiteSpace();
            void SetTop([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetTop();
            void SetLeft([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetLeft();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPosition();
            void SetZIndex([In, MarshalAs(UnmanagedType.Struct)] object p);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetZIndex();
            void SetOverflow([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetOverflow();
            void SetPageBreakBefore([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPageBreakBefore();
            void SetPageBreakAfter([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPageBreakAfter();
            void SetCssText([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCssText();
            void SetPixelTop([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetPixelTop();
            void SetPixelLeft([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetPixelLeft();
            void SetPixelWidth([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetPixelWidth();
            void SetPixelHeight([In, MarshalAs(UnmanagedType.I4)] int p);
            [return: MarshalAs(UnmanagedType.I4)]
            int GetPixelHeight();
            void SetPosTop([In, MarshalAs(UnmanagedType.R4)] float p);
            [return: MarshalAs(UnmanagedType.R4)]
            float GetPosTop();
            void SetPosLeft([In, MarshalAs(UnmanagedType.R4)] float p);
            [return: MarshalAs(UnmanagedType.R4)]
            float GetPosLeft();
            void SetPosWidth([In, MarshalAs(UnmanagedType.R4)] float p);
            [return: MarshalAs(UnmanagedType.R4)]
            float GetPosWidth();
            void SetPosHeight([In, MarshalAs(UnmanagedType.R4)] float p);
            [return: MarshalAs(UnmanagedType.R4)]
            float GetPosHeight();
            void SetCursor([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCursor();
            void SetClip([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetClip();
            void SetFilter([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFilter();
            void SetAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, MarshalAs(UnmanagedType.Struct)] object AttributeValue, [In, MarshalAs(UnmanagedType.I4)] int lFlags);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, MarshalAs(UnmanagedType.I4)] int lFlags);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool RemoveAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, MarshalAs(UnmanagedType.I4)] int lFlags);
        }

        [ComImport, ComVisible(true), Guid("3050F2E3-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IHTMLStyleSheet
        {
            void SetTitle([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTitle();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLStyleSheet GetParentStyleSheet();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IHTMLElement GetOwningElement();
            void SetDisabled([In, MarshalAs(UnmanagedType.Bool)] bool p);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetDisabled();
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetReadOnly();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetImports();
            void SetHref([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetHref();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetStyleSheetType();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetId();
            [return: MarshalAs(UnmanagedType.I4)]
            int AddImport([In, MarshalAs(UnmanagedType.BStr)] string bstrURL, [In, MarshalAs(UnmanagedType.I4)] int lIndex);
            [return: MarshalAs(UnmanagedType.I4)]
            int AddRule([In, MarshalAs(UnmanagedType.BStr)] string bstrSelector, [In, MarshalAs(UnmanagedType.BStr)] string bstrStyle, [In, MarshalAs(UnmanagedType.I4)] int lIndex);
            void RemoveImport([In, MarshalAs(UnmanagedType.I4)] int lIndex);
            void RemoveRule([In, MarshalAs(UnmanagedType.I4)] int lIndex);
            void SetMedia([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetMedia();
            void SetCssText([In, MarshalAs(UnmanagedType.BStr)] string p);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCssText();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetRules();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000118-0000-0000-C000-000000000046"), ComVisible(true)]
        public interface IOleClientSite
        {
            void SaveObject();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetMoniker([In, MarshalAs(UnmanagedType.U4)] int dwAssign, [In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker);
            [PreserveSig]
            int GetContainer(out System.Design.NativeMethods.IOleContainer ppContainer);
            void ShowObject();
            void OnShowWindow([In, MarshalAs(UnmanagedType.I4)] int fShow);
            void RequestNewObjectLayout();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B722BCCB-4E68-101B-A2BC-00AA00404770"), ComVisible(true)]
        public interface IOleCommandTarget
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryStatus(ref Guid pguidCmdGroup, int cCmds, [In, Out] System.Design.NativeMethods.OLECMD prgCmds, [In, Out] string pCmdText);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Exec(ref Guid pguidCmdGroup, int nCmdID, int nCmdexecopt, [In, MarshalAs(UnmanagedType.LPArray)] object[] pvaIn, IntPtr pvaOut);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000011B-0000-0000-C000-000000000046"), ComVisible(true)]
        public interface IOleContainer
        {
            void ParseDisplayName([In, MarshalAs(UnmanagedType.Interface)] object pbc, [In, MarshalAs(UnmanagedType.BStr)] string pszDisplayName, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pchEaten, [Out, MarshalAs(UnmanagedType.LPArray)] object[] ppmkOut);
            void EnumObjects([In, MarshalAs(UnmanagedType.U4)] int grfFlags, [MarshalAs(UnmanagedType.Interface)] out object ppenum);
            void LockContainer([In, MarshalAs(UnmanagedType.I4)] int fLock);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("B722BCC7-4E68-101B-A2BC-00AA00404770")]
        public interface IOleDocumentSite
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ActivateMe([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IOleDocumentView pViewToActivate);
        }

        [ComImport, Guid("B722BCC6-4E68-101B-A2BC-00AA00404770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
        public interface IOleDocumentView
        {
            void SetInPlaceSite([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IOleInPlaceSite pIPSite);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IOleInPlaceSite GetInPlaceSite();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetDocument();
            void SetRect([In] System.Design.NativeMethods.COMRECT prcView);
            void GetRect([Out] System.Design.NativeMethods.COMRECT prcView);
            void SetRectComplex([In] System.Design.NativeMethods.COMRECT prcView, [In] System.Design.NativeMethods.COMRECT prcHScroll, [In] System.Design.NativeMethods.COMRECT prcVScroll, [In] System.Design.NativeMethods.COMRECT prcSizeBox);
            void Show([In, MarshalAs(UnmanagedType.I4)] int fShow);
            void UIActivate([In, MarshalAs(UnmanagedType.I4)] int fUIActivate);
            void Open();
            void CloseView([In, MarshalAs(UnmanagedType.U4)] int dwReserved);
            void SaveViewState([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IStream pstm);
            void ApplyViewState([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IStream pstm);
            void Clone([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IOleInPlaceSite pIPSiteNew, [Out, MarshalAs(UnmanagedType.LPArray)] System.Design.NativeMethods.IOleDocumentView[] ppViewNew);
        }

        [ComImport, Guid("00000122-0000-0000-C000-000000000046"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleDropTarget
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleDragEnter(IDataObject pDataObj, [In, MarshalAs(UnmanagedType.U4)] int grfKeyState, [In, MarshalAs(UnmanagedType.U8)] long pt, [In, Out, MarshalAs(UnmanagedType.I4)] ref int pdwEffect);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleDragOver([In, MarshalAs(UnmanagedType.U4)] int grfKeyState, [In, MarshalAs(UnmanagedType.U8)] long pt, [In, Out, MarshalAs(UnmanagedType.I4)] ref int pdwEffect);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleDragLeave();
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleDrop(IDataObject pDataObj, [In, MarshalAs(UnmanagedType.U4)] int grfKeyState, [In, MarshalAs(UnmanagedType.U8)] long pt, [In, Out, MarshalAs(UnmanagedType.I4)] ref int pdwEffect);
        }

        [ComImport, ComVisible(true), Guid("00000117-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceActiveObject
        {
            int GetWindow(out IntPtr hwnd);
            void ContextSensitiveHelp([In, MarshalAs(UnmanagedType.I4)] int fEnterMode);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateAccelerator([In] ref System.Design.NativeMethods.MSG lpmsg);
            void OnFrameWindowActivate([In, MarshalAs(UnmanagedType.I4)] int fActivate);
            void OnDocWindowActivate([In, MarshalAs(UnmanagedType.I4)] int fActivate);
            void ResizeBorder([In] System.Design.NativeMethods.COMRECT prcBorder, [In] System.Design.NativeMethods.IOleInPlaceUIWindow pUIWindow, [In, MarshalAs(UnmanagedType.I4)] int fFrameWindow);
            void EnableModeless([In, MarshalAs(UnmanagedType.I4)] int fEnable);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000116-0000-0000-C000-000000000046"), ComVisible(true)]
        public interface IOleInPlaceFrame
        {
            IntPtr GetWindow();
            void ContextSensitiveHelp([In, MarshalAs(UnmanagedType.I4)] int fEnterMode);
            void GetBorder([Out] System.Design.NativeMethods.COMRECT lprectBorder);
            void RequestBorderSpace([In] System.Design.NativeMethods.COMRECT pborderwidths);
            void SetBorderSpace([In] System.Design.NativeMethods.COMRECT pborderwidths);
            void SetActiveObject([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IOleInPlaceActiveObject pActiveObject, [In, MarshalAs(UnmanagedType.LPWStr)] string pszObjName);
            void InsertMenus([In] IntPtr hmenuShared, [In, Out] object lpMenuWidths);
            void SetMenu([In] IntPtr hmenuShared, [In] IntPtr holemenu, [In] IntPtr hwndActiveObject);
            void RemoveMenus([In] IntPtr hmenuShared);
            void SetStatusText([In, MarshalAs(UnmanagedType.BStr)] string pszStatusText);
            void EnableModeless([In, MarshalAs(UnmanagedType.I4)] int fEnable);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateAccelerator([In] ref System.Design.NativeMethods.MSG lpmsg, [In, MarshalAs(UnmanagedType.U2)] short wID);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("00000119-0000-0000-C000-000000000046")]
        public interface IOleInPlaceSite
        {
            IntPtr GetWindow();
            void ContextSensitiveHelp([In, MarshalAs(UnmanagedType.I4)] int fEnterMode);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int CanInPlaceActivate();
            void OnInPlaceActivate();
            void OnUIActivate();
            void GetWindowContext(out System.Design.NativeMethods.IOleInPlaceFrame ppFrame, out System.Design.NativeMethods.IOleInPlaceUIWindow ppDoc, [Out] System.Design.NativeMethods.COMRECT lprcPosRect, [Out] System.Design.NativeMethods.COMRECT lprcClipRect, [In, Out] System.Design.NativeMethods.tagOIFI lpFrameInfo);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Scroll([In, MarshalAs(UnmanagedType.U4)] System.Design.NativeMethods.tagSIZE scrollExtant);
            void OnUIDeactivate([In, MarshalAs(UnmanagedType.I4)] int fUndoable);
            void OnInPlaceDeactivate();
            void DiscardUndoState();
            void DeactivateAndUndo();
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnPosRectChange([In] System.Design.NativeMethods.COMRECT lprcPosRect);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("00000115-0000-0000-C000-000000000046")]
        public interface IOleInPlaceUIWindow
        {
            IntPtr GetWindow();
            void ContextSensitiveHelp([In, MarshalAs(UnmanagedType.I4)] int fEnterMode);
            void GetBorder([Out] System.Design.NativeMethods.COMRECT lprectBorder);
            void RequestBorderSpace([In] System.Design.NativeMethods.COMRECT pborderwidths);
            void SetBorderSpace([In] System.Design.NativeMethods.COMRECT pborderwidths);
            void SetActiveObject([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IOleInPlaceActiveObject pActiveObject, [In, MarshalAs(UnmanagedType.LPWStr)] string pszObjName);
        }

        [ComImport, Guid("00000112-0000-0000-C000-000000000046"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleObject
        {
            [PreserveSig]
            int SetClientSite([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IOleClientSite pClientSite);
            [PreserveSig]
            int GetClientSite(out System.Design.NativeMethods.IOleClientSite site);
            [PreserveSig]
            int SetHostNames([In, MarshalAs(UnmanagedType.LPWStr)] string szContainerApp, [In, MarshalAs(UnmanagedType.LPWStr)] string szContainerObj);
            [PreserveSig]
            int Close([In, MarshalAs(UnmanagedType.I4)] int dwSaveOption);
            [PreserveSig]
            int SetMoniker([In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker, [In, MarshalAs(UnmanagedType.Interface)] object pmk);
            [PreserveSig]
            int GetMoniker([In, MarshalAs(UnmanagedType.U4)] int dwAssign, [In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker, out object moniker);
            [PreserveSig]
            int InitFromData(IDataObject pDataObject, [In, MarshalAs(UnmanagedType.I4)] int fCreation, [In, MarshalAs(UnmanagedType.U4)] int dwReserved);
            [PreserveSig]
            int GetClipboardData([In, MarshalAs(UnmanagedType.U4)] int dwReserved, out IDataObject data);
            [PreserveSig]
            int DoVerb([In, MarshalAs(UnmanagedType.I4)] int iVerb, [In] IntPtr lpmsg, [In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IOleClientSite pActiveSite, [In, MarshalAs(UnmanagedType.I4)] int lindex, [In] IntPtr hwndParent, [In] System.Design.NativeMethods.COMRECT lprcPosRect);
            [PreserveSig]
            int EnumVerbs(out System.Design.NativeMethods.IEnumOLEVERB e);
            [PreserveSig]
            int OleUpdate();
            [PreserveSig]
            int IsUpToDate();
            [PreserveSig]
            int GetUserClassID([In, Out] ref Guid pClsid);
            [PreserveSig]
            int GetUserType([In, MarshalAs(UnmanagedType.U4)] int dwFormOfType, [MarshalAs(UnmanagedType.LPWStr)] out string userType);
            [PreserveSig]
            int SetExtent([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, [In] System.Design.NativeMethods.tagSIZEL pSizel);
            [PreserveSig]
            int GetExtent([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, [Out] System.Design.NativeMethods.tagSIZEL pSizel);
            [PreserveSig]
            int Advise([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IAdviseSink pAdvSink, out int cookie);
            [PreserveSig]
            int Unadvise([In, MarshalAs(UnmanagedType.U4)] int dwConnection);
            [PreserveSig]
            int EnumAdvise(out object e);
            [PreserveSig]
            int GetMiscStatus([In, MarshalAs(UnmanagedType.U4)] int dwAspect, out int misc);
            [PreserveSig]
            int SetColorScheme([In] System.Design.NativeMethods.tagLOGPALETTE pLogpal);
        }

        [ComImport, Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistStreamInit
        {
            void GetClassID([In, Out] ref Guid pClassID);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int IsDirty();
            void Load([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IStream pstm);
            void Save([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IStream pstm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
            void GetSizeMax([Out, MarshalAs(UnmanagedType.LPArray)] long pcbSize);
            void InitNew();
        }

        [ComImport, ComVisible(true), Guid("0000000C-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IStream
        {
            [return: MarshalAs(UnmanagedType.I4)]
            int Read([In] IntPtr buf, [In, MarshalAs(UnmanagedType.I4)] int len);
            [return: MarshalAs(UnmanagedType.I4)]
            int Write([In] IntPtr buf, [In, MarshalAs(UnmanagedType.I4)] int len);
            [return: MarshalAs(UnmanagedType.I8)]
            long Seek([In, MarshalAs(UnmanagedType.I8)] long dlibMove, [In, MarshalAs(UnmanagedType.I4)] int dwOrigin);
            void SetSize([In, MarshalAs(UnmanagedType.I8)] long libNewSize);
            [return: MarshalAs(UnmanagedType.I8)]
            long CopyTo([In, MarshalAs(UnmanagedType.Interface)] System.Design.NativeMethods.IStream pstm, [In, MarshalAs(UnmanagedType.I8)] long cb, [Out, MarshalAs(UnmanagedType.LPArray)] long[] pcbRead);
            void Commit([In, MarshalAs(UnmanagedType.I4)] int grfCommitFlags);
            void Revert();
            void LockRegion([In, MarshalAs(UnmanagedType.I8)] long libOffset, [In, MarshalAs(UnmanagedType.I8)] long cb, [In, MarshalAs(UnmanagedType.I4)] int dwLockType);
            void UnlockRegion([In, MarshalAs(UnmanagedType.I8)] long libOffset, [In, MarshalAs(UnmanagedType.I8)] long cb, [In, MarshalAs(UnmanagedType.I4)] int dwLockType);
            void Stat([In] IntPtr pStatstg, [In, MarshalAs(UnmanagedType.I4)] int grfStatFlag);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Design.NativeMethods.IStream Clone();
        }

        public delegate int ListViewCompareCallback(IntPtr lParam1, IntPtr lParam2, IntPtr lParamSort);

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

            public LOGFONT(System.Design.NativeMethods.LOGFONT lf)
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

            public override string ToString()
            {
                return string.Concat(new object[] { 
                    "lfHeight=", this.lfHeight, ", lfWidth=", this.lfWidth, ", lfEscapement=", this.lfEscapement, ", lfOrientation=", this.lfOrientation, ", lfWeight=", this.lfWeight, ", lfItalic=", this.lfItalic, ", lfUnderline=", this.lfUnderline, ", lfStrikeOut=", this.lfStrikeOut, 
                    ", lfCharSet=", this.lfCharSet, ", lfOutPrecision=", this.lfOutPrecision, ", lfClipPrecision=", this.lfClipPrecision, ", lfQuality=", this.lfQuality, ", lfPitchAndFamily=", this.lfPitchAndFamily, ", lfFaceName=", this.lfFaceName
                 });
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEHOOKSTRUCT
        {
            public int pt_x;
            public int pt_y;
            public IntPtr hWnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public int pt_x;
            public int pt_y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMHDR
        {
            public int hwndFrom;
            public int idFrom;
            public int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMHEADER
        {
            public int hwndFrom;
            public int idFrom;
            public int code;
            public int iItem;
            public int iButton;
            public int pItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMTREEVIEW
        {
            public System.Design.NativeMethods.NMHDR nmhdr;
            public int action;
            public System.Design.NativeMethods.TV_ITEM itemOld;
            public System.Design.NativeMethods.TV_ITEM itemNew;
            public System.Design.NativeMethods.POINT ptDrag;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NONCLIENTMETRICS
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Design.NativeMethods.NONCLIENTMETRICS));
            public int iBorderWidth;
            public int iScrollWidth;
            public int iScrollHeight;
            public int iCaptionWidth;
            public int iCaptionHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Design.NativeMethods.LOGFONT lfCaptionFont;
            public int iSmCaptionWidth;
            public int iSmCaptionHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Design.NativeMethods.LOGFONT lfSmCaptionFont;
            public int iMenuWidth;
            public int iMenuHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Design.NativeMethods.LOGFONT lfMenuFont;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Design.NativeMethods.LOGFONT lfStatusFont;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Design.NativeMethods.LOGFONT lfMessageFont;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class OLECMD
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cmdID;
            [MarshalAs(UnmanagedType.U4)]
            public int cmdf;
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
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class STATDATA
        {
            [MarshalAs(UnmanagedType.U4)]
            public int advf;
            [MarshalAs(UnmanagedType.U4)]
            public int dwConnection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class STATSTG
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwcsName;
            public int type;
            [MarshalAs(UnmanagedType.I8)]
            public long cbSize;
            [MarshalAs(UnmanagedType.I8)]
            public long mtime;
            [MarshalAs(UnmanagedType.I8)]
            public long ctime;
            [MarshalAs(UnmanagedType.I8)]
            public long atime;
            [MarshalAs(UnmanagedType.I4)]
            public int grfMode;
            [MarshalAs(UnmanagedType.I4)]
            public int grfLocksSupported;
            public int clsid_data1;
            [MarshalAs(UnmanagedType.I2)]
            public short clsid_data2;
            [MarshalAs(UnmanagedType.I2)]
            public short clsid_data3;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b1;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b2;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b3;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b4;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b5;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b6;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b7;
            [MarshalAs(UnmanagedType.I4)]
            public int grfStateBits;
            [MarshalAs(UnmanagedType.I4)]
            public int reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class STGMEDIUM
        {
            [MarshalAs(UnmanagedType.I4)]
            public int tymed;
            public IntPtr unionmember = IntPtr.Zero;
            public IntPtr pUnkForRelease = IntPtr.Zero;
        }

        [ComVisible(false)]
        public enum StructFormat
        {
            Ansi = 1,
            Auto = 3,
            Unicode = 2
        }

        [Flags]
        public enum TabControlHitTest
        {
            TCHT_NOWHERE = 1,
            TCHT_ONITEMICON = 2,
            TCHT_ONITEMLABEL = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagLOGPALETTE
        {
            [MarshalAs(UnmanagedType.U2)]
            public short palVersion;
            [MarshalAs(UnmanagedType.U2)]
            public short palNumEntries;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagOIFI
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.I4)]
            public int fMDIApp;
            public IntPtr hwndFrame;
            public IntPtr hAccel;
            [MarshalAs(UnmanagedType.U4)]
            public int cAccelEntries;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagOLEVERB
        {
            [MarshalAs(UnmanagedType.I4)]
            public int lVerb;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszVerbName;
            [MarshalAs(UnmanagedType.U4)]
            public int fuFlags;
            [MarshalAs(UnmanagedType.U4)]
            public int grfAttribs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagSIZE
        {
            [MarshalAs(UnmanagedType.I4)]
            public int cx;
            [MarshalAs(UnmanagedType.I4)]
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential), ComVisible(true)]
        public sealed class tagSIZEL
        {
            [MarshalAs(UnmanagedType.I4)]
            public int cx;
            [MarshalAs(UnmanagedType.I4)]
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TCHITTESTINFO
        {
            public Point pt;
            public System.Design.NativeMethods.TabControlHitTest flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TEXTMETRIC
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

        public delegate void TimerProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class TRACKMOUSEEVENT
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Design.NativeMethods.TRACKMOUSEEVENT));
            public int dwFlags;
            public IntPtr hwndTrack;
            public int dwHoverTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        public class TV_HITTESTINFO
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public int hItem;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        public class TV_ITEM
        {
            public int mask;
            public int hItem;
            public int state;
            public int stateMask;
            public int pszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public int lParam;
        }

        internal class Util
        {
            public static int HIWORD(int n)
            {
                return ((n >> 0x10) & 0xffff);
            }

            public static int LOWORD(int n)
            {
                return (n & 0xffff);
            }

            public static int MAKELONG(int low, int high)
            {
                return ((high << 0x10) | (low & 0xffff));
            }

            public static int MAKELPARAM(int low, int high)
            {
                return ((high << 0x10) | (low & 0xffff));
            }

            [DllImport("user32.dll", CharSet=CharSet.Auto)]
            internal static extern int RegisterWindowMessage(string msg);
            public static int SignedHIWORD(int n)
            {
                int num = (short) ((n >> 0x10) & 0xffff);
                num = num << 0x10;
                return (num >> 0x10);
            }

            public static int SignedHIWORD(IntPtr n)
            {
                return SignedHIWORD((int) ((long) n));
            }

            public static int SignedLOWORD(int n)
            {
                int num = (short) (n & 0xffff);
                num = num << 0x10;
                return (num >> 0x10);
            }

            public static int SignedLOWORD(IntPtr n)
            {
                return SignedLOWORD((int) ((long) n));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}

