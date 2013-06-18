namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        public static readonly int ACM_OPEN;
        public const int ACM_OPENA = 0x464;
        public const int ACM_OPENW = 0x467;
        public const int ADVF_NODATA = 1;
        public const int ADVF_ONLYONCE = 4;
        public const int ADVF_PRIMEFIRST = 2;
        public const int ALTERNATE = 1;
        public const int ANSI_CHARSET = 0;
        public const int ARW_BOTTOMLEFT = 0;
        public const int ARW_BOTTOMRIGHT = 1;
        public const int ARW_DOWN = 4;
        public const int ARW_HIDE = 8;
        public const int ARW_LEFT = 0;
        public const int ARW_RIGHT = 0;
        public const int ARW_TOPLEFT = 2;
        public const int ARW_TOPRIGHT = 3;
        public const int ARW_UP = 4;
        public const int ATTR_CONVERTED = 2;
        public const int ATTR_FIXEDCONVERTED = 5;
        public const int ATTR_INPUT = 0;
        public const int ATTR_INPUT_ERROR = 4;
        public const int ATTR_TARGET_CONVERTED = 1;
        public const int ATTR_TARGET_NOTCONVERTED = 3;
        public const int AUTOAPPEND = 0x40000000;
        public const int AUTOAPPEND_OFF = -2147483648;
        public const int AUTOSUGGEST = 0x10000000;
        public const int AUTOSUGGEST_OFF = 0x20000000;
        public const int BCM_GETIDEALSIZE = 0x1601;
        public const int BDR_RAISED = 5;
        public const int BDR_RAISEDINNER = 4;
        public const int BDR_RAISEDOUTER = 1;
        public const int BDR_SUNKEN = 10;
        public const int BDR_SUNKENINNER = 8;
        public const int BDR_SUNKENOUTER = 2;
        public const int BF_ADJUST = 0x2000;
        public const int BF_BOTTOM = 8;
        public const int BF_FLAT = 0x4000;
        public const int BF_LEFT = 1;
        public const int BF_MIDDLE = 0x800;
        public const int BF_RIGHT = 4;
        public const int BF_TOP = 2;
        public const int BFFM_ENABLEOK = 0x465;
        public const int BFFM_INITIALIZED = 1;
        public const int BFFM_SELCHANGED = 2;
        public static readonly int BFFM_SETSELECTION;
        public const int BFFM_SETSELECTIONA = 0x466;
        public const int BFFM_SETSELECTIONW = 0x467;
        public const int BI_BITFIELDS = 3;
        public const int BI_RGB = 0;
        public const int BITMAPINFO_MAX_COLORSIZE = 0x100;
        public const int BITSPIXEL = 12;
        public const int BM_CLICK = 0xf5;
        public const int BM_SETCHECK = 0xf1;
        public const int BM_SETSTATE = 0xf3;
        public const int BN_CLICKED = 0;
        public const int BS_3STATE = 5;
        public const int BS_BOTTOM = 0x800;
        public const int BS_CENTER = 0x300;
        public const int BS_DEFPUSHBUTTON = 1;
        public const int BS_GROUPBOX = 7;
        public const int BS_LEFT = 0x100;
        public const int BS_MULTILINE = 0x2000;
        public const int BS_OWNERDRAW = 11;
        public const int BS_PATTERN = 3;
        public const int BS_PUSHBUTTON = 0;
        public const int BS_PUSHLIKE = 0x1000;
        public const int BS_RADIOBUTTON = 4;
        public const int BS_RIGHT = 0x200;
        public const int BS_RIGHTBUTTON = 0x20;
        public const int BS_SOLID = 0;
        public const int BS_TOP = 0x400;
        public const int BS_VCENTER = 0xc00;
        public const int CB_ADDSTRING = 0x143;
        public const int CB_DELETESTRING = 0x144;
        public const int CB_ERR = -1;
        public const int CB_FINDSTRING = 0x14c;
        public const int CB_FINDSTRINGEXACT = 0x158;
        public const int CB_GETCURSEL = 0x147;
        public const int CB_GETDROPPEDSTATE = 0x157;
        public const int CB_GETDROPPEDWIDTH = 0x15f;
        public const int CB_GETEDITSEL = 320;
        public const int CB_GETITEMDATA = 0x150;
        public const int CB_GETITEMHEIGHT = 340;
        public const int CB_GETLBTEXT = 0x148;
        public const int CB_GETLBTEXTLEN = 0x149;
        public const int CB_INSERTSTRING = 330;
        public const int CB_LIMITTEXT = 0x141;
        public const int CB_RESETCONTENT = 0x14b;
        public const int CB_SETCURSEL = 0x14e;
        public const int CB_SETDROPPEDWIDTH = 0x160;
        public const int CB_SETEDITSEL = 0x142;
        public const int CB_SETITEMHEIGHT = 0x153;
        public const int CB_SHOWDROPDOWN = 0x14f;
        public static readonly int CBEM_GETITEM;
        public const int CBEM_GETITEMA = 0x404;
        public const int CBEM_GETITEMW = 0x40d;
        public static readonly int CBEM_INSERTITEM;
        public const int CBEM_INSERTITEMA = 0x401;
        public const int CBEM_INSERTITEMW = 0x40b;
        public static readonly int CBEM_SETITEM;
        public const int CBEM_SETITEMA = 0x405;
        public const int CBEM_SETITEMW = 0x40c;
        public static readonly int CBEN_ENDEDIT;
        public const int CBEN_ENDEDITA = -805;
        public const int CBEN_ENDEDITW = -806;
        public const int CBN_CLOSEUP = 8;
        public const int CBN_DBLCLK = 2;
        public const int CBN_DROPDOWN = 7;
        public const int CBN_EDITCHANGE = 5;
        public const int CBN_EDITUPDATE = 6;
        public const int CBN_SELCHANGE = 1;
        public const int CBN_SELENDOK = 9;
        public const int CBS_AUTOHSCROLL = 0x40;
        public const int CBS_DROPDOWN = 2;
        public const int CBS_DROPDOWNLIST = 3;
        public const int CBS_HASSTRINGS = 0x200;
        public const int CBS_NOINTEGRALHEIGHT = 0x400;
        public const int CBS_OWNERDRAWFIXED = 0x10;
        public const int CBS_OWNERDRAWVARIABLE = 0x20;
        public const int CBS_SIMPLE = 1;
        public const int CC_ANYCOLOR = 0x100;
        public const int CC_ENABLEHOOK = 0x10;
        public const int CC_FULLOPEN = 2;
        public const int CC_PREVENTFULLOPEN = 4;
        public const int CC_RGBINIT = 1;
        public const int CC_SHOWHELP = 8;
        public const int CC_SOLIDCOLOR = 0x80;
        public const int CCM_GETVERSION = 0x2008;
        public const int CCM_SETVERSION = 0x2007;
        public const int CCS_NODIVIDER = 0x40;
        public const int CCS_NOPARENTALIGN = 8;
        public const int CCS_NORESIZE = 4;
        public const int CDDS_ITEM = 0x10000;
        public const int CDDS_ITEMPOSTPAINT = 0x10002;
        public const int CDDS_ITEMPREPAINT = 0x10001;
        public const int CDDS_POSTPAINT = 2;
        public const int CDDS_PREPAINT = 1;
        public const int CDDS_SUBITEM = 0x20000;
        public const int CDERR_DIALOGFAILURE = 0xffff;
        public const int CDERR_FINDRESFAILURE = 6;
        public const int CDERR_INITIALIZATION = 2;
        public const int CDERR_LOADRESFAILURE = 7;
        public const int CDERR_LOADSTRFAILURE = 5;
        public const int CDERR_LOCKRESFAILURE = 8;
        public const int CDERR_MEMALLOCFAILURE = 9;
        public const int CDERR_MEMLOCKFAILURE = 10;
        public const int CDERR_NOHINSTANCE = 4;
        public const int CDERR_NOHOOK = 11;
        public const int CDERR_NOTEMPLATE = 3;
        public const int CDERR_REGISTERMSGFAIL = 12;
        public const int CDERR_STRUCTSIZE = 1;
        public const int CDIS_CHECKED = 8;
        public const int CDIS_DEFAULT = 0x20;
        public const int CDIS_DISABLED = 4;
        public const int CDIS_FOCUS = 0x10;
        public const int CDIS_GRAYED = 2;
        public const int CDIS_HOT = 0x40;
        public const int CDIS_INDETERMINATE = 0x100;
        public const int CDIS_MARKED = 0x80;
        public const int CDIS_SELECTED = 1;
        public const int CDIS_SHOWKEYBOARDCUES = 0x200;
        public const int CDRF_DODEFAULT = 0;
        public const int CDRF_NEWFONT = 2;
        public const int CDRF_NOTIFYITEMDRAW = 0x20;
        public const int CDRF_NOTIFYPOSTPAINT = 0x10;
        public const int CDRF_NOTIFYSUBITEMDRAW = 0x20;
        public const int CDRF_SKIPDEFAULT = 4;
        public const int CF_APPLY = 0x200;
        public const int CF_BITMAP = 2;
        public const int CF_DIB = 8;
        public const int CF_DIF = 5;
        public const int CF_EFFECTS = 0x100;
        public const int CF_ENABLEHOOK = 8;
        public const int CF_ENHMETAFILE = 14;
        public const int CF_FIXEDPITCHONLY = 0x4000;
        public const int CF_FORCEFONTEXIST = 0x10000;
        public const int CF_HDROP = 15;
        public const int CF_INITTOLOGFONTSTRUCT = 0x40;
        public const int CF_LIMITSIZE = 0x2000;
        public const int CF_LOCALE = 0x10;
        public const int CF_METAFILEPICT = 3;
        public const int CF_NOSIMULATIONS = 0x1000;
        public const int CF_NOVECTORFONTS = 0x800;
        public const int CF_NOVERTFONTS = 0x1000000;
        public const int CF_OEMTEXT = 7;
        public const int CF_PALETTE = 9;
        public const int CF_PENDATA = 10;
        public const int CF_RIFF = 11;
        public const int CF_SCREENFONTS = 1;
        public const int CF_SCRIPTSONLY = 0x400;
        public const int CF_SELECTSCRIPT = 0x400000;
        public const int CF_SHOWHELP = 4;
        public const int CF_SYLK = 4;
        public const int CF_TEXT = 1;
        public const int CF_TIFF = 6;
        public const int CF_TTONLY = 0x40000;
        public const int CF_UNICODETEXT = 13;
        public const int CF_WAVE = 12;
        public const int CFERR_MAXLESSTHANMIN = 0x2002;
        public const int CFERR_NOFONTS = 0x2001;
        public const int CHILDID_SELF = 0;
        public const int CLR_DEFAULT = -16777216;
        public const int CLR_NONE = -1;
        public const int CLSCTX_INPROC_SERVER = 1;
        public const int CLSCTX_LOCAL_SERVER = 4;
        public const int cmb4 = 0x473;
        public const int COLOR_WINDOW = 5;
        public const int CONNECT_E_CANNOTCONNECT = -2147220990;
        public const int CONNECT_E_NOCONNECTION = -2147220992;
        public const int CP_WINANSI = 0x3ec;
        public const int CPS_CANCEL = 4;
        public const int CPS_COMPLETE = 1;
        public const int CS_DBLCLKS = 8;
        public const int CS_DROPSHADOW = 0x20000;
        public const int CS_SAVEBITS = 0x800;
        public const int CSC_NAVIGATEBACK = 2;
        public const int CSC_NAVIGATEFORWARD = 1;
        public const int CSIDL_APPDATA = 0x1a;
        public const int CSIDL_COMMON_APPDATA = 0x23;
        public const int CSIDL_COOKIES = 0x21;
        public const int CSIDL_DESKTOP = 0;
        public const int CSIDL_DESKTOPDIRECTORY = 0x10;
        public const int CSIDL_FAVORITES = 6;
        public const int CSIDL_HISTORY = 0x22;
        public const int CSIDL_INTERNET = 1;
        public const int CSIDL_INTERNET_CACHE = 0x20;
        public const int CSIDL_LOCAL_APPDATA = 0x1c;
        public const int CSIDL_PERSONAL = 5;
        public const int CSIDL_PROGRAM_FILES = 0x26;
        public const int CSIDL_PROGRAM_FILES_COMMON = 0x2b;
        public const int CSIDL_PROGRAMS = 2;
        public const int CSIDL_RECENT = 8;
        public const int CSIDL_SENDTO = 9;
        public const int CSIDL_STARTMENU = 11;
        public const int CSIDL_STARTUP = 7;
        public const int CSIDL_SYSTEM = 0x25;
        public const int CSIDL_TEMPLATES = 0x15;
        public const int CTRLINFO_EATS_ESCAPE = 2;
        public const int CTRLINFO_EATS_RETURN = 1;
        public const int CW_USEDEFAULT = -2147483648;
        public const int CWP_SKIPINVISIBLE = 1;
        public const int DCX_CACHE = 2;
        public const int DCX_INTERSECTRGN = 0x80;
        public const int DCX_LOCKWINDOWUPDATE = 0x400;
        public const int DCX_WINDOW = 1;
        public const int DEFAULT_CHARSET = 1;
        public const int DEFAULT_GUI_FONT = 0x11;
        public const int DESKTOP_SWITCHDESKTOP = 0x100;
        public const int DFC_BUTTON = 4;
        public const int DFC_CAPTION = 1;
        public const int DFC_MENU = 2;
        public const int DFC_SCROLL = 3;
        public const int DFCS_BUTTON3STATE = 8;
        public const int DFCS_BUTTONCHECK = 0;
        public const int DFCS_BUTTONPUSH = 0x10;
        public const int DFCS_BUTTONRADIO = 4;
        public const int DFCS_CAPTIONCLOSE = 0;
        public const int DFCS_CAPTIONHELP = 4;
        public const int DFCS_CAPTIONMAX = 2;
        public const int DFCS_CAPTIONMIN = 1;
        public const int DFCS_CAPTIONRESTORE = 3;
        public const int DFCS_CHECKED = 0x400;
        public const int DFCS_FLAT = 0x4000;
        public const int DFCS_INACTIVE = 0x100;
        public const int DFCS_MENUARROW = 0;
        public const int DFCS_MENUBULLET = 2;
        public const int DFCS_MENUCHECK = 1;
        public const int DFCS_PUSHED = 0x200;
        public const int DFCS_SCROLLCOMBOBOX = 5;
        public const int DFCS_SCROLLDOWN = 1;
        public const int DFCS_SCROLLLEFT = 2;
        public const int DFCS_SCROLLRIGHT = 3;
        public const int DFCS_SCROLLUP = 0;
        public const int DI_NORMAL = 3;
        public const int DIB_RGB_COLORS = 0;
        public const int DISP_E_EXCEPTION = -2147352567;
        public const int DISP_E_MEMBERNOTFOUND = -2147352573;
        public const int DISP_E_PARAMNOTFOUND = -2147352572;
        public const int DISPATCH_METHOD = 1;
        public const int DISPATCH_PROPERTYGET = 2;
        public const int DISPATCH_PROPERTYPUT = 4;
        public const int DISPID_PROPERTYPUT = -3;
        public const int DISPID_UNKNOWN = -1;
        public const int DLGC_HASSETSEL = 8;
        public const int DLGC_WANTALLKEYS = 4;
        public const int DLGC_WANTARROWS = 1;
        public const int DLGC_WANTCHARS = 0x80;
        public const int DLGC_WANTMESSAGE = 4;
        public const int DLGC_WANTTAB = 2;
        public const int DM_DISPLAYORIENTATION = 0x80;
        public const int DRAGDROP_E_ALREADYREGISTERED = -2147221247;
        public const int DRAGDROP_E_NOTREGISTERED = -2147221248;
        public const int DTM_GETMONTHCAL = 0x1008;
        public const int DTM_GETSYSTEMTIME = 0x1001;
        public static readonly int DTM_SETFORMAT;
        public const int DTM_SETFORMATA = 0x1005;
        public const int DTM_SETFORMATW = 0x1032;
        public const int DTM_SETMCCOLOR = 0x1006;
        public const int DTM_SETMCFONT = 0x1009;
        public const int DTM_SETRANGE = 0x1004;
        public const int DTM_SETSYSTEMTIME = 0x1002;
        public const int DTN_CLOSEUP = -753;
        public const int DTN_DATETIMECHANGE = -759;
        public const int DTN_DROPDOWN = -754;
        public static readonly int DTN_FORMAT;
        public const int DTN_FORMATA = -756;
        public static readonly int DTN_FORMATQUERY;
        public const int DTN_FORMATQUERYA = -755;
        public const int DTN_FORMATQUERYW = -742;
        public const int DTN_FORMATW = -743;
        public static readonly int DTN_USERSTRING;
        public const int DTN_USERSTRINGA = -758;
        public const int DTN_USERSTRINGW = -745;
        public static readonly int DTN_WMKEYDOWN;
        public const int DTN_WMKEYDOWNA = -757;
        public const int DTN_WMKEYDOWNW = -744;
        public const int DTS_LONGDATEFORMAT = 4;
        public const int DTS_RIGHTALIGN = 0x20;
        public const int DTS_SHOWNONE = 2;
        public const int DTS_TIMEFORMAT = 9;
        public const int DTS_UPDOWN = 1;
        public const int DUPLICATE = 6;
        public const int DUPLICATE_SAME_ACCESS = 2;
        public const int DV_E_DVASPECT = -2147221397;
        public const int DVASPECT_CONTENT = 1;
        public const int DVASPECT_OPAQUE = 0x10;
        public const int DVASPECT_TRANSPARENT = 0x20;
        public const int E_ABORT = -2147467260;
        public const int E_FAIL = -2147467259;
        public const int E_INVALIDARG = -2147024809;
        public const int E_NOINTERFACE = -2147467262;
        public const int E_NOTIMPL = -2147467263;
        public const int E_OUTOFMEMORY = -2147024882;
        public const int E_UNEXPECTED = -2147418113;
        public const int EC_LEFTMARGIN = 1;
        public const int EC_RIGHTMARGIN = 2;
        public const int EDGE_BUMP = 9;
        public const int EDGE_ETCHED = 6;
        public const int EDGE_RAISED = 5;
        public const int EDGE_SUNKEN = 10;
        public const int EM_CANUNDO = 0xc6;
        public const int EM_CHARFROMPOS = 0xd7;
        public const int EM_EMPTYUNDOBUFFER = 0xcd;
        public const int EM_GETFIRSTVISIBLELINE = 0xce;
        public const int EM_GETLINE = 0xc4;
        public const int EM_GETLINECOUNT = 0xba;
        public const int EM_GETMODIFY = 0xb8;
        public const int EM_GETPASSWORDCHAR = 210;
        public const int EM_GETSEL = 0xb0;
        public const int EM_LIMITTEXT = 0xc5;
        public const int EM_LINEFROMCHAR = 0xc9;
        public const int EM_LINEINDEX = 0xbb;
        public const int EM_POSFROMCHAR = 0xd6;
        public const int EM_REPLACESEL = 0xc2;
        public const int EM_SCROLL = 0xb5;
        public const int EM_SCROLLCARET = 0xb7;
        public const int EM_SETMARGINS = 0xd3;
        public const int EM_SETMODIFY = 0xb9;
        public const int EM_SETPASSWORDCHAR = 0xcc;
        public const int EM_SETREADONLY = 0xcf;
        public const int EM_SETSEL = 0xb1;
        public const int EM_UNDO = 0xc7;
        public static readonly int EMR_POLYTEXTOUT;
        public const int EMR_POLYTEXTOUTA = 0x60;
        public const int EMR_POLYTEXTOUTW = 0x61;
        public const int EN_ALIGN_LTR_EC = 0x700;
        public const int EN_ALIGN_RTL_EC = 0x701;
        public const int EN_CHANGE = 0x300;
        public const int EN_HSCROLL = 0x601;
        public const int EN_UPDATE = 0x400;
        public const int EN_VSCROLL = 0x602;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_CLASS_ALREADY_EXISTS = 0x582;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ES_AUTOHSCROLL = 0x80;
        public const int ES_AUTOVSCROLL = 0x40;
        public const int ES_CENTER = 1;
        public const int ES_LEFT = 0;
        public const int ES_LOWERCASE = 0x10;
        public const int ES_MULTILINE = 4;
        public const int ES_NOHIDESEL = 0x100;
        public const int ES_PASSWORD = 0x20;
        public const int ES_READONLY = 0x800;
        public const int ES_RIGHT = 2;
        public const int ES_UPPERCASE = 8;
        public const int ESB_DISABLE_BOTH = 3;
        public const int ESB_ENABLE_BOTH = 0;
        public const int ETO_CLIPPED = 4;
        public const int ETO_OPAQUE = 2;
        public const int FADF_BSTR = 0x100;
        public const int FADF_DISPATCH = 0x400;
        public const int FADF_UNKNOWN = 0x200;
        public const int FADF_VARIANT = 0x800;
        public const int FALT = 0x10;
        public const int FILE_MAP_COPY = 1;
        public const int FILE_MAP_READ = 4;
        public const int FILE_MAP_WRITE = 2;
        public const int FNERR_BUFFERTOOSMALL = 0x3003;
        public const int FNERR_INVALIDFILENAME = 0x3002;
        public const int FNERR_SUBCLASSFAILURE = 0x3001;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FRERR_BUFFERLENGTHZERO = 0x4001;
        public const int FSHIFT = 4;
        public const int FVIRTKEY = 1;
        public const int FW_BOLD = 700;
        public const int FW_DONTCARE = 0;
        public const int FW_NORMAL = 400;
        public const int GA_PARENT = 1;
        public const int GA_ROOT = 2;
        public const int GCL_WNDPROC = -24;
        public const int GCS_COMPATTR = 0x10;
        public const int GCS_COMPSTR = 8;
        public const int GCS_RESULTSTR = 0x800;
        public const int GDI_ERROR = -1;
        public const int GDT_NONE = 1;
        public const int GDT_VALID = 0;
        public const int GDTR_MAX = 2;
        public const int GDTR_MIN = 1;
        public const int GHND = 0x42;
        public const int GM_ADVANCED = 2;
        public const int GM_COMPATIBLE = 1;
        public const int GMEM_DDESHARE = 0x2000;
        public const int GMEM_DISCARDABLE = 0x100;
        public const int GMEM_FIXED = 0;
        public const int GMEM_INVALID_HANDLE = 0x8000;
        public const int GMEM_LOWER = 0x1000;
        public const int GMEM_MODIFY = 0x80;
        public const int GMEM_MOVEABLE = 2;
        public const int GMEM_NOCOMPACT = 0x10;
        public const int GMEM_NODISCARD = 0x20;
        public const int GMEM_NOT_BANKED = 0x1000;
        public const int GMEM_NOTIFY = 0x4000;
        public const int GMEM_SHARE = 0x2000;
        public const int GMEM_VALID_FLAGS = 0x7f72;
        public const int GMEM_ZEROINIT = 0x40;
        public const int GMR_DAYSTATE = 1;
        public const int GMR_VISIBLE = 0;
        public const int GPTR = 0x40;
        public const int GW_CHILD = 5;
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GWL_EXSTYLE = -20;
        public const int GWL_HWNDPARENT = -8;
        public const int GWL_ID = -12;
        public const int GWL_STYLE = -16;
        public const int GWL_WNDPROC = -4;
        public const int HBMMENU_CALLBACK = -1;
        public const int HBMMENU_MBAR_CLOSE = 5;
        public const int HBMMENU_MBAR_CLOSE_D = 6;
        public const int HBMMENU_MBAR_MINIMIZE = 3;
        public const int HBMMENU_MBAR_MINIMIZE_D = 7;
        public const int HBMMENU_MBAR_RESTORE = 2;
        public const int HBMMENU_POPUP_CLOSE = 8;
        public const int HBMMENU_POPUP_MAXIMIZE = 10;
        public const int HBMMENU_POPUP_MINIMIZE = 11;
        public const int HBMMENU_POPUP_RESTORE = 9;
        public const int HBMMENU_SYSTEM = 1;
        public const int HC_ACTION = 0;
        public const int HC_GETNEXT = 1;
        public const int HC_SKIP = 2;
        public const int HCF_HIGHCONTRASTON = 1;
        public const int HDI_ORDER = 0x80;
        public const int HDI_WIDTH = 1;
        public static readonly int HDM_GETITEM;
        public const int HDM_GETITEMA = 0x1203;
        public const int HDM_GETITEMCOUNT = 0x1200;
        public const int HDM_GETITEMW = 0x120b;
        public static readonly int HDM_INSERTITEM;
        public const int HDM_INSERTITEMA = 0x1201;
        public const int HDM_INSERTITEMW = 0x120a;
        public const int HDM_LAYOUT = 0x1205;
        public static readonly int HDM_SETITEM;
        public const int HDM_SETITEMA = 0x1204;
        public const int HDM_SETITEMW = 0x120c;
        public const int HDN_BEGINTDRAG = -310;
        public static readonly int HDN_BEGINTRACK;
        public const int HDN_BEGINTRACKA = -306;
        public const int HDN_BEGINTRACKW = -326;
        public static readonly int HDN_DIVIDERDBLCLICK;
        public const int HDN_DIVIDERDBLCLICKA = -305;
        public const int HDN_DIVIDERDBLCLICKW = -325;
        public const int HDN_ENDDRAG = -311;
        public static readonly int HDN_ENDTRACK;
        public const int HDN_ENDTRACKA = -307;
        public const int HDN_ENDTRACKW = -327;
        public static readonly int HDN_GETDISPINFO;
        public const int HDN_GETDISPINFOA = -309;
        public const int HDN_GETDISPINFOW = -329;
        public static readonly int HDN_ITEMCHANGED;
        public const int HDN_ITEMCHANGEDA = -301;
        public const int HDN_ITEMCHANGEDW = -321;
        public static readonly int HDN_ITEMCHANGING;
        public const int HDN_ITEMCHANGINGA = -300;
        public const int HDN_ITEMCHANGINGW = -320;
        public static readonly int HDN_ITEMCLICK;
        public const int HDN_ITEMCLICKA = -302;
        public const int HDN_ITEMCLICKW = -322;
        public static readonly int HDN_ITEMDBLCLICK;
        public const int HDN_ITEMDBLCLICKA = -303;
        public const int HDN_ITEMDBLCLICKW = -323;
        public static readonly int HDN_TRACK;
        public const int HDN_TRACKA = -308;
        public const int HDN_TRACKW = -328;
        public const int HDS_FULLDRAG = 0x80;
        public const int HELPINFO_WINDOW = 1;
        public const int HH_FTS_DEFAULT_PROXIMITY = -1;
        public const int HICF_ACCELERATOR = 4;
        public const int HICF_ARROWKEYS = 2;
        public const int HICF_DUPACCEL = 8;
        public const int HICF_ENTERING = 0x10;
        public const int HICF_LEAVING = 0x20;
        public const int HICF_LMOUSE = 0x80;
        public const int HICF_MOUSE = 1;
        public const int HICF_OTHER = 0;
        public const int HICF_RESELECT = 0x40;
        public const int HICF_TOGGLEDROPDOWN = 0x100;
        public const int HLP_FILE = 1;
        public const int HLP_KEYWORD = 2;
        public const int HLP_NAVIGATOR = 3;
        public const int HLP_OBJECT = 4;
        public const int HOLLOW_BRUSH = 5;
        public const int HTBORDER = 0x12;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 0x10;
        public const int HTBOTTOMRIGHT = 0x11;
        public const int HTCLIENT = 1;
        public const int HTLEFT = 10;
        public const int HTNOWHERE = 0;
        public const int HTTRANSPARENT = -1;
        public static HandleRef HWND_BOTTOM = new HandleRef(null, (IntPtr) 1);
        public static HandleRef HWND_MESSAGE = new HandleRef(null, new IntPtr(-3));
        public static HandleRef HWND_NOTOPMOST = new HandleRef(null, new IntPtr(-2));
        public static HandleRef HWND_TOP = new HandleRef(null, IntPtr.Zero);
        public static HandleRef HWND_TOPMOST = new HandleRef(null, new IntPtr(-1));
        public const int ICC_BAR_CLASSES = 4;
        public const int ICC_DATE_CLASSES = 0x100;
        public const int ICC_LISTVIEW_CLASSES = 1;
        public const int ICC_PROGRESS_CLASS = 0x20;
        public const int ICC_TAB_CLASSES = 8;
        public const int ICC_TREEVIEW_CLASSES = 2;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL = 0;
        public const int IDC_APPSTARTING = 0x7f8a;
        public const int IDC_ARROW = 0x7f00;
        public const int IDC_CROSS = 0x7f03;
        public const int IDC_HELP = 0x7f8b;
        public const int IDC_IBEAM = 0x7f01;
        public const int IDC_NO = 0x7f88;
        public const int IDC_SIZEALL = 0x7f86;
        public const int IDC_SIZENESW = 0x7f83;
        public const int IDC_SIZENS = 0x7f85;
        public const int IDC_SIZENWSE = 0x7f82;
        public const int IDC_SIZEWE = 0x7f84;
        public const int IDC_UPARROW = 0x7f04;
        public const int IDC_WAIT = 0x7f02;
        public const int IDM_PAGESETUP = 0x7d4;
        public const int IDM_PRINT = 0x1b;
        public const int IDM_PRINTPREVIEW = 0x7d3;
        public const int IDM_PROPERTIES = 0x1c;
        public const int IDM_SAVEAS = 0x47;
        public const int ILC_COLOR = 0;
        public const int ILC_COLOR16 = 0x10;
        public const int ILC_COLOR24 = 0x18;
        public const int ILC_COLOR32 = 0x20;
        public const int ILC_COLOR4 = 4;
        public const int ILC_COLOR8 = 8;
        public const int ILC_MASK = 1;
        public const int ILC_MIRROR = 0x2000;
        public const int ILD_MASK = 0x10;
        public const int ILD_NORMAL = 0;
        public const int ILD_ROP = 0x40;
        public const int ILD_TRANSPARENT = 1;
        public const int ILP_DOWNLEVEL = 1;
        public const int ILP_NORMAL = 0;
        public const int ILS_ALPHA = 8;
        public const int ILS_GLOW = 1;
        public const int ILS_NORMAL = 0;
        public const int ILS_SATURATE = 4;
        public const int ILS_SHADOW = 2;
        public const int IMAGE_CURSOR = 2;
        public const int IMAGE_ICON = 1;
        public const int IME_CMODE_FULLSHAPE = 8;
        public const int IME_CMODE_KATAKANA = 2;
        public const int IME_CMODE_NATIVE = 1;
        public const int IMN_OPENSTATUSWINDOW = 2;
        public const int IMN_SETCONVERSIONMODE = 6;
        public const int IMN_SETOPENSTATUS = 8;
        public const int INET_E_DEFAULT_ACTION = -2146697199;
        public const int INPLACE_E_NOTOOLSPACE = -2147221087;
        public const int INPUT_KEYBOARD = 1;
        public static IntPtr InvalidIntPtr = ((IntPtr) (-1));
        public const int KEYEVENTF_EXTENDEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 2;
        public const int KEYEVENTF_UNICODE = 4;
        public const int LANG_NEUTRAL = 0;
        public static readonly int LANG_USER_DEFAULT = MAKELANGID(0, 1);
        public const int LB_ADDSTRING = 0x180;
        public const int LB_DELETESTRING = 0x182;
        public const int LB_ERR = -1;
        public const int LB_ERRSPACE = -2;
        public const int LB_FINDSTRING = 0x18f;
        public const int LB_FINDSTRINGEXACT = 0x1a2;
        public const int LB_GETCARETINDEX = 0x19f;
        public const int LB_GETCURSEL = 0x188;
        public const int LB_GETITEMHEIGHT = 0x1a1;
        public const int LB_GETITEMRECT = 0x198;
        public const int LB_GETSEL = 0x187;
        public const int LB_GETSELCOUNT = 400;
        public const int LB_GETSELITEMS = 0x191;
        public const int LB_GETTEXT = 0x189;
        public const int LB_GETTEXTLEN = 0x18a;
        public const int LB_GETTOPINDEX = 0x18e;
        public const int LB_INSERTSTRING = 0x181;
        public const int LB_ITEMFROMPOINT = 0x1a9;
        public const int LB_RESETCONTENT = 0x184;
        public const int LB_SETCOLUMNWIDTH = 0x195;
        public const int LB_SETCURSEL = 390;
        public const int LB_SETHORIZONTALEXTENT = 0x194;
        public const int LB_SETITEMHEIGHT = 0x1a0;
        public const int LB_SETLOCALE = 0x1a5;
        public const int LB_SETSEL = 0x185;
        public const int LB_SETTABSTOPS = 0x192;
        public const int LB_SETTOPINDEX = 0x197;
        public const int LBN_DBLCLK = 2;
        public const int LBN_SELCHANGE = 1;
        public const int LBS_DISABLENOSCROLL = 0x1000;
        public const int LBS_EXTENDEDSEL = 0x800;
        public const int LBS_HASSTRINGS = 0x40;
        public const int LBS_MULTICOLUMN = 0x200;
        public const int LBS_MULTIPLESEL = 8;
        public const int LBS_NOINTEGRALHEIGHT = 0x100;
        public const int LBS_NOSEL = 0x4000;
        public const int LBS_NOTIFY = 1;
        public const int LBS_OWNERDRAWFIXED = 0x10;
        public const int LBS_OWNERDRAWVARIABLE = 0x20;
        public const int LBS_USETABSTOPS = 0x80;
        public const int LBS_WANTKEYBOARDINPUT = 0x400;
        public const int LOCALE_IFIRSTDAYOFWEEK = 0x100c;
        public const int LOCALE_IMEASURE = 13;
        public static readonly int LOCALE_USER_DEFAULT = MAKELCID(LANG_USER_DEFAULT);
        public const int LOCK_EXCLUSIVE = 2;
        public const int LOCK_ONLYONCE = 4;
        public const int LOCK_WRITE = 1;
        public const int LOGPIXELSX = 0x58;
        public const int LOGPIXELSY = 90;
        public static IntPtr LPSTR_TEXTCALLBACK = ((IntPtr) (-1));
        public const int LV_VIEW_TILE = 4;
        public const int LVA_ALIGNLEFT = 1;
        public const int LVA_ALIGNTOP = 2;
        public const int LVA_DEFAULT = 0;
        public const int LVA_SNAPTOGRID = 5;
        public const int LVBKIF_SOURCE_NONE = 0;
        public const int LVBKIF_SOURCE_URL = 2;
        public const int LVBKIF_STYLE_NORMAL = 0;
        public const int LVBKIF_STYLE_TILE = 0x10;
        public const int LVCDI_GROUP = 1;
        public const int LVCDI_ITEM = 0;
        public const int LVCF_FMT = 1;
        public const int LVCF_IMAGE = 0x10;
        public const int LVCF_ORDER = 0x20;
        public const int LVCF_SUBITEM = 8;
        public const int LVCF_TEXT = 4;
        public const int LVCF_WIDTH = 2;
        public const int LVCFMT_IMAGE = 0x800;
        public const int LVFI_NEARESTXY = 0x40;
        public const int LVFI_PARAM = 1;
        public const int LVFI_PARTIAL = 8;
        public const int LVFI_STRING = 2;
        public const int LVGA_FOOTER_CENTER = 0x10;
        public const int LVGA_FOOTER_LEFT = 8;
        public const int LVGA_FOOTER_RIGHT = 0x20;
        public const int LVGA_HEADER_CENTER = 2;
        public const int LVGA_HEADER_LEFT = 1;
        public const int LVGA_HEADER_RIGHT = 4;
        public const int LVGF_ALIGN = 8;
        public const int LVGF_FOOTER = 2;
        public const int LVGF_GROUPID = 0x10;
        public const int LVGF_HEADER = 1;
        public const int LVGF_NONE = 0;
        public const int LVGF_STATE = 4;
        public const int LVGS_COLLAPSED = 1;
        public const int LVGS_HIDDEN = 2;
        public const int LVGS_NORMAL = 0;
        public const int LVHT_ABOVE = 8;
        public const int LVHT_BELOW = 0x10;
        public const int LVHT_LEFT = 0x40;
        public const int LVHT_NOWHERE = 1;
        public const int LVHT_ONITEM = 14;
        public const int LVHT_ONITEMICON = 2;
        public const int LVHT_ONITEMLABEL = 4;
        public const int LVHT_ONITEMSTATEICON = 8;
        public const int LVHT_RIGHT = 0x20;
        public const int LVIF_COLUMNS = 0x200;
        public const int LVIF_GROUPID = 0x100;
        public const int LVIF_IMAGE = 2;
        public const int LVIF_INDENT = 0x10;
        public const int LVIF_PARAM = 4;
        public const int LVIF_STATE = 8;
        public const int LVIF_TEXT = 1;
        public const int LVIM_AFTER = 1;
        public const int LVIR_BOUNDS = 0;
        public const int LVIR_ICON = 1;
        public const int LVIR_LABEL = 2;
        public const int LVIR_SELECTBOUNDS = 3;
        public const int LVIS_CUT = 4;
        public const int LVIS_DROPHILITED = 8;
        public const int LVIS_FOCUSED = 1;
        public const int LVIS_OVERLAYMASK = 0xf00;
        public const int LVIS_SELECTED = 2;
        public const int LVIS_STATEIMAGEMASK = 0xf000;
        public const int LVM_ARRANGE = 0x1016;
        public const int LVM_DELETEALLITEMS = 0x1009;
        public const int LVM_DELETECOLUMN = 0x101c;
        public const int LVM_DELETEITEM = 0x1008;
        public static readonly int LVM_EDITLABEL;
        public const int LVM_EDITLABELA = 0x1017;
        public const int LVM_EDITLABELW = 0x1076;
        public const int LVM_ENABLEGROUPVIEW = 0x109d;
        public const int LVM_ENSUREVISIBLE = 0x1013;
        public static readonly int LVM_FINDITEM;
        public const int LVM_FINDITEMA = 0x100d;
        public const int LVM_FINDITEMW = 0x1053;
        public const int LVM_GETCALLBACKMASK = 0x100a;
        public static readonly int LVM_GETCOLUMN;
        public const int LVM_GETCOLUMNA = 0x1019;
        public const int LVM_GETCOLUMNORDERARRAY = 0x103b;
        public const int LVM_GETCOLUMNW = 0x105f;
        public const int LVM_GETCOLUMNWIDTH = 0x101d;
        public const int LVM_GETGROUPINFO = 0x1095;
        public const int LVM_GETHEADER = 0x101f;
        public const int LVM_GETHOTITEM = 0x103d;
        public const int LVM_GETINSERTMARK = 0x10a7;
        public const int LVM_GETINSERTMARKCOLOR = 0x10ab;
        public const int LVM_GETINSERTMARKRECT = 0x10a9;
        public static readonly int LVM_GETISEARCHSTRING;
        public const int LVM_GETISEARCHSTRINGA = 0x1034;
        public const int LVM_GETISEARCHSTRINGW = 0x1075;
        public static readonly int LVM_GETITEM;
        public const int LVM_GETITEMA = 0x1005;
        public const int LVM_GETITEMCOUNT = 0x1004;
        public const int LVM_GETITEMPOSITION = 0x1010;
        public const int LVM_GETITEMRECT = 0x100e;
        public const int LVM_GETITEMSTATE = 0x102c;
        public static readonly int LVM_GETITEMTEXT;
        public const int LVM_GETITEMTEXTA = 0x102d;
        public const int LVM_GETITEMTEXTW = 0x1073;
        public const int LVM_GETITEMW = 0x104b;
        public const int LVM_GETNEXTITEM = 0x100c;
        public const int LVM_GETSELECTEDCOUNT = 0x1032;
        public static readonly int LVM_GETSTRINGWIDTH;
        public const int LVM_GETSTRINGWIDTHA = 0x1011;
        public const int LVM_GETSTRINGWIDTHW = 0x1057;
        public const int LVM_GETSUBITEMRECT = 0x1038;
        public const int LVM_GETTILEVIEWINFO = 0x10a3;
        public const int LVM_GETTOPINDEX = 0x1027;
        public const int LVM_HASGROUP = 0x10a1;
        public const int LVM_HITTEST = 0x1012;
        public static readonly int LVM_INSERTCOLUMN;
        public const int LVM_INSERTCOLUMNA = 0x101b;
        public const int LVM_INSERTCOLUMNW = 0x1061;
        public const int LVM_INSERTGROUP = 0x1091;
        public static readonly int LVM_INSERTITEM;
        public const int LVM_INSERTITEMA = 0x1007;
        public const int LVM_INSERTITEMW = 0x104d;
        public const int LVM_INSERTMARKHITTEST = 0x10a8;
        public const int LVM_ISGROUPVIEWENABLED = 0x10af;
        public const int LVM_MOVEITEMTOGROUP = 0x109a;
        public const int LVM_REDRAWITEMS = 0x1015;
        public const int LVM_REMOVEALLGROUPS = 0x10a0;
        public const int LVM_REMOVEGROUP = 0x1096;
        public const int LVM_SCROLL = 0x1014;
        public const int LVM_SETBKCOLOR = 0x1001;
        public static readonly int LVM_SETBKIMAGE;
        public const int LVM_SETBKIMAGEA = 0x1044;
        public const int LVM_SETBKIMAGEW = 0x108a;
        public const int LVM_SETCALLBACKMASK = 0x100b;
        public static readonly int LVM_SETCOLUMN;
        public const int LVM_SETCOLUMNA = 0x101a;
        public const int LVM_SETCOLUMNORDERARRAY = 0x103a;
        public const int LVM_SETCOLUMNW = 0x1060;
        public const int LVM_SETCOLUMNWIDTH = 0x101e;
        public const int LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036;
        public const int LVM_SETGROUPINFO = 0x1093;
        public const int LVM_SETIMAGELIST = 0x1003;
        public const int LVM_SETINFOTIP = 0x10ad;
        public const int LVM_SETINSERTMARK = 0x10a6;
        public const int LVM_SETINSERTMARKCOLOR = 0x10aa;
        public static readonly int LVM_SETITEM;
        public const int LVM_SETITEMA = 0x1006;
        public const int LVM_SETITEMCOUNT = 0x102f;
        public const int LVM_SETITEMPOSITION = 0x100f;
        public const int LVM_SETITEMPOSITION32 = 0x1031;
        public const int LVM_SETITEMSTATE = 0x102b;
        public static readonly int LVM_SETITEMTEXT;
        public const int LVM_SETITEMTEXTA = 0x102e;
        public const int LVM_SETITEMTEXTW = 0x1074;
        public const int LVM_SETITEMW = 0x104c;
        public const int LVM_SETSELECTIONMARK = 0x1043;
        public const int LVM_SETTEXTBKCOLOR = 0x1026;
        public const int LVM_SETTEXTCOLOR = 0x1024;
        public const int LVM_SETTILEVIEWINFO = 0x10a2;
        public const int LVM_SETTOOLTIPS = 0x104a;
        public const int LVM_SETVIEW = 0x108e;
        public const int LVM_SORTITEMS = 0x1030;
        public const int LVM_SUBITEMHITTEST = 0x1039;
        public const int LVM_UPDATE = 0x102a;
        public const int LVN_BEGINDRAG = -109;
        public static readonly int LVN_BEGINLABELEDIT;
        public const int LVN_BEGINLABELEDITA = -105;
        public const int LVN_BEGINLABELEDITW = -175;
        public const int LVN_BEGINRDRAG = -111;
        public const int LVN_COLUMNCLICK = -108;
        public static readonly int LVN_ENDLABELEDIT;
        public const int LVN_ENDLABELEDITA = -106;
        public const int LVN_ENDLABELEDITW = -176;
        public static readonly int LVN_GETDISPINFO;
        public const int LVN_GETDISPINFOA = -150;
        public const int LVN_GETDISPINFOW = -177;
        public static readonly int LVN_GETINFOTIP;
        public const int LVN_GETINFOTIPA = -157;
        public const int LVN_GETINFOTIPW = -158;
        public const int LVN_ITEMACTIVATE = -114;
        public const int LVN_ITEMCHANGED = -101;
        public const int LVN_ITEMCHANGING = -100;
        public const int LVN_KEYDOWN = -155;
        public const int LVN_ODCACHEHINT = -113;
        public static readonly int LVN_ODFINDITEM;
        public const int LVN_ODFINDITEMA = -152;
        public const int LVN_ODFINDITEMW = -179;
        public const int LVN_ODSTATECHANGED = -115;
        public static readonly int LVN_SETDISPINFO;
        public const int LVN_SETDISPINFOA = -151;
        public const int LVN_SETDISPINFOW = -178;
        public const int LVNI_FOCUSED = 1;
        public const int LVNI_SELECTED = 2;
        public const int LVS_ALIGNLEFT = 0x800;
        public const int LVS_ALIGNTOP = 0;
        public const int LVS_AUTOARRANGE = 0x100;
        public const int LVS_EDITLABELS = 0x200;
        public const int LVS_EX_CHECKBOXES = 4;
        public const int LVS_EX_DOUBLEBUFFER = 0x10000;
        public const int LVS_EX_FULLROWSELECT = 0x20;
        public const int LVS_EX_GRIDLINES = 1;
        public const int LVS_EX_HEADERDRAGDROP = 0x10;
        public const int LVS_EX_INFOTIP = 0x400;
        public const int LVS_EX_ONECLICKACTIVATE = 0x40;
        public const int LVS_EX_TRACKSELECT = 8;
        public const int LVS_EX_TWOCLICKACTIVATE = 0x80;
        public const int LVS_EX_UNDERLINEHOT = 0x800;
        public const int LVS_ICON = 0;
        public const int LVS_LIST = 3;
        public const int LVS_NOCOLUMNHEADER = 0x4000;
        public const int LVS_NOLABELWRAP = 0x80;
        public const int LVS_NOSCROLL = 0x2000;
        public const int LVS_NOSORTHEADER = 0x8000;
        public const int LVS_OWNERDATA = 0x1000;
        public const int LVS_REPORT = 1;
        public const int LVS_SHAREIMAGELISTS = 0x40;
        public const int LVS_SHOWSELALWAYS = 8;
        public const int LVS_SINGLESEL = 4;
        public const int LVS_SMALLICON = 2;
        public const int LVS_SORTASCENDING = 0x10;
        public const int LVS_SORTDESCENDING = 0x20;
        public const int LVSCW_AUTOSIZE = -1;
        public const int LVSCW_AUTOSIZE_USEHEADER = -2;
        public const int LVSIL_NORMAL = 0;
        public const int LVSIL_SMALL = 1;
        public const int LVSIL_STATE = 2;
        public const int LVTVIF_FIXEDSIZE = 3;
        public const int LVTVIM_COLUMNS = 2;
        public const int LVTVIM_TILESIZE = 1;
        public const int LWA_ALPHA = 2;
        public const int LWA_COLORKEY = 1;
        public const int MA_ACTIVATE = 1;
        public const int MA_ACTIVATEANDEAT = 2;
        public const int MA_NOACTIVATE = 3;
        public const int MA_NOACTIVATEANDEAT = 4;
        public const int MAX_PATH = 260;
        public const int MB_OK = 0;
        public const int MCHT_CALENDAR = 0x20000;
        public const int MCHT_CALENDARBK = 0x20000;
        public const int MCHT_CALENDARDATE = 0x20001;
        public const int MCHT_CALENDARDATENEXT = 0x1020001;
        public const int MCHT_CALENDARDATEPREV = 0x2020001;
        public const int MCHT_CALENDARDAY = 0x20002;
        public const int MCHT_CALENDARWEEKNUM = 0x20003;
        public const int MCHT_TITLE = 0x10000;
        public const int MCHT_TITLEBK = 0x10000;
        public const int MCHT_TITLEBTNNEXT = 0x1010003;
        public const int MCHT_TITLEBTNPREV = 0x2010003;
        public const int MCHT_TITLEMONTH = 0x10001;
        public const int MCHT_TITLEYEAR = 0x10002;
        public const int MCHT_TODAYLINK = 0x30000;
        public const int MCM_GETMAXTODAYWIDTH = 0x1015;
        public const int MCM_GETMINREQRECT = 0x1009;
        public const int MCM_GETMONTHRANGE = 0x1007;
        public const int MCM_GETTODAY = 0x100d;
        public const int MCM_HITTEST = 0x100e;
        public const int MCM_SETCOLOR = 0x100a;
        public const int MCM_SETFIRSTDAYOFWEEK = 0x100f;
        public const int MCM_SETMAXSELCOUNT = 0x1004;
        public const int MCM_SETMONTHDELTA = 0x1014;
        public const int MCM_SETRANGE = 0x1012;
        public const int MCM_SETSELRANGE = 0x1006;
        public const int MCM_SETTODAY = 0x100c;
        public const int MCN_GETDAYSTATE = -747;
        public const int MCN_SELCHANGE = -749;
        public const int MCN_SELECT = -746;
        public const int MCS_DAYSTATE = 1;
        public const int MCS_MULTISELECT = 2;
        public const int MCS_NOTODAY = 0x10;
        public const int MCS_NOTODAYCIRCLE = 8;
        public const int MCS_WEEKNUMBERS = 4;
        public const int MCSC_MONTHBK = 4;
        public const int MCSC_TEXT = 1;
        public const int MCSC_TITLEBK = 2;
        public const int MCSC_TITLETEXT = 3;
        public const int MCSC_TRAILINGTEXT = 5;
        public const int MDIS_ALLCHILDSTYLES = 1;
        public const int MDITILE_HORIZONTAL = 1;
        public const int MDITILE_SKIPDISABLED = 2;
        public const int MDITILE_VERTICAL = 0;
        public const int MEMBERID_NIL = -1;
        public const int MF_BYCOMMAND = 0;
        public const int MF_BYPOSITION = 0x400;
        public const int MF_ENABLED = 0;
        public const int MF_GRAYED = 1;
        public const int MF_POPUP = 0x10;
        public const int MF_SYSMENU = 0x2000;
        public const int MFS_DISABLED = 3;
        public const int MFT_MENUBREAK = 0x40;
        public const int MFT_RIGHTJUSTIFY = 0x4000;
        public const int MFT_RIGHTORDER = 0x2000;
        public const int MFT_SEPARATOR = 0x800;
        public const int MIIM_BITMAP = 0x80;
        public const int MIIM_DATA = 0x20;
        public const int MIIM_FTYPE = 0x100;
        public const int MIIM_ID = 2;
        public const int MIIM_STATE = 1;
        public const int MIIM_STRING = 0x40;
        public const int MIIM_SUBMENU = 4;
        public const int MIIM_TYPE = 0x10;
        public const int MK_CONTROL = 8;
        public const int MK_LBUTTON = 1;
        public const int MK_MBUTTON = 0x10;
        public const int MK_RBUTTON = 2;
        public const int MK_SHIFT = 4;
        public const int MM_ANISOTROPIC = 8;
        public const int MM_HIMETRIC = 3;
        public const int MM_TEXT = 1;
        public const int MNC_EXECUTE = 2;
        public const int MNC_SELECT = 3;
        public const string MOUSEZ_CLASSNAME = "MouseZ";
        public const string MOUSEZ_TITLE = "Magellan MSWHEEL";
        public const int MSAA_MENU_SIG = -1441927155;
        public const string MSH_MOUSEWHEEL = "MSWHEEL_ROLLMSG";
        public const string MSH_SCROLL_LINES = "MSH_SCROLL_LINES_MSG";
        public const int MWMO_INPUTAVAILABLE = 4;
        public const int MWT_IDENTITY = 1;
        public const int NFR_ANSI = 1;
        public const int NFR_UNICODE = 2;
        public const int NI_COMPOSITIONSTR = 0x15;
        public const int NIF_ICON = 2;
        public const int NIF_INFO = 0x10;
        public const int NIF_MESSAGE = 1;
        public const int NIF_TIP = 4;
        public const int NIIF_ERROR = 3;
        public const int NIIF_INFO = 1;
        public const int NIIF_NONE = 0;
        public const int NIIF_WARNING = 2;
        public const int NIM_ADD = 0;
        public const int NIM_DELETE = 2;
        public const int NIM_MODIFY = 1;
        public const int NIM_SETVERSION = 4;
        public const int NIN_BALLOONHIDE = 0x403;
        public const int NIN_BALLOONSHOW = 0x402;
        public const int NIN_BALLOONTIMEOUT = 0x404;
        public const int NIN_BALLOONUSERCLICK = 0x405;
        public const int NM_CLICK = -2;
        public const int NM_CUSTOMDRAW = -12;
        public const int NM_DBLCLK = -3;
        public const int NM_RCLICK = -5;
        public const int NM_RDBLCLK = -6;
        public const int NM_RELEASEDCAPTURE = -16;
        public const int NONANTIALIASED_QUALITY = 3;
        public const int NOTSRCCOPY = 0x330008;
        public const int NULL_BRUSH = 5;
        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        public const int OBJ_BITMAP = 7;
        public const int OBJ_BRUSH = 2;
        public const int OBJ_DC = 3;
        public const int OBJ_ENHMETADC = 12;
        public const int OBJ_EXTPEN = 11;
        public const int OBJ_FONT = 6;
        public const int OBJ_MEMDC = 10;
        public const int OBJ_METADC = 4;
        public const int OBJ_METAFILE = 9;
        public const int OBJ_PAL = 5;
        public const int OBJ_PEN = 1;
        public const int OBJ_REGION = 8;
        public const int OBJID_CLIENT = -4;
        public const int OBJID_QUERYCLASSNAMEIDX = -12;
        public const int OBJID_WINDOW = 0;
        public const int ODS_CHECKED = 8;
        public const int ODS_COMBOBOXEDIT = 0x1000;
        public const int ODS_DEFAULT = 0x20;
        public const int ODS_DISABLED = 4;
        public const int ODS_FOCUS = 0x10;
        public const int ODS_GRAYED = 2;
        public const int ODS_HOTLIGHT = 0x40;
        public const int ODS_INACTIVE = 0x80;
        public const int ODS_NOACCEL = 0x100;
        public const int ODS_NOFOCUSRECT = 0x200;
        public const int ODS_SELECTED = 1;
        public const int OFN_ALLOWMULTISELECT = 0x200;
        public const int OFN_CREATEPROMPT = 0x2000;
        public const int OFN_ENABLEHOOK = 0x20;
        public const int OFN_ENABLESIZING = 0x800000;
        public const int OFN_EXPLORER = 0x80000;
        public const int OFN_FILEMUSTEXIST = 0x1000;
        public const int OFN_HIDEREADONLY = 4;
        public const int OFN_NOCHANGEDIR = 8;
        public const int OFN_NODEREFERENCELINKS = 0x100000;
        public const int OFN_NOVALIDATE = 0x100;
        public const int OFN_OVERWRITEPROMPT = 2;
        public const int OFN_PATHMUSTEXIST = 0x800;
        public const int OFN_READONLY = 1;
        public const int OFN_SHOWHELP = 0x10;
        public const int OFN_USESHELLITEM = 0x1000000;
        public const int OLE_E_INVALIDRECT = -2147221491;
        public const int OLE_E_NOCONNECTION = -2147221500;
        public const int OLE_E_PROMPTSAVECANCELLED = -2147221492;
        public const int OLECLOSE_PROMPTSAVE = 2;
        public const int OLECLOSE_SAVEIFDIRTY = 0;
        public const int OLEIVERB_DISCARDUNDOSTATE = -6;
        public const int OLEIVERB_HIDE = -3;
        public const int OLEIVERB_INPLACEACTIVATE = -5;
        public const int OLEIVERB_PRIMARY = 0;
        public const int OLEIVERB_PROPERTIES = -7;
        public const int OLEIVERB_SHOW = -1;
        public const int OLEIVERB_UIACTIVATE = -4;
        public const int OLEMISC_ACTIVATEWHENVISIBLE = 0x100;
        public const int OLEMISC_ACTSLIKEBUTTON = 0x1000;
        public const int OLEMISC_INSIDEOUT = 0x80;
        public const int OLEMISC_RECOMPOSEONRESIZE = 1;
        public const int OLEMISC_SETCLIENTSITEFIRST = 0x20000;
        public const int OPAQUE = 2;
        public const int OUT_DEFAULT_PRECIS = 0;
        public const int OUT_TT_ONLY_PRECIS = 7;
        public const int OUT_TT_PRECIS = 4;
        public const int PAGE_READONLY = 2;
        public const int PAGE_READWRITE = 4;
        public const int PAGE_WRITECOPY = 8;
        public const int PATCOPY = 0xf00021;
        public const int PATINVERT = 0x5a0049;
        public const int PBM_SETBARCOLOR = 0x409;
        public const int PBM_SETBKCOLOR = 0x2001;
        public const int PBM_SETMARQUEE = 0x40a;
        public const int PBM_SETPOS = 0x402;
        public const int PBM_SETRANGE = 0x401;
        public const int PBM_SETRANGE32 = 0x406;
        public const int PBM_SETSTEP = 0x404;
        public const int PBS_MARQUEE = 8;
        public const int PBS_SMOOTH = 1;
        public const int PD_ALLPAGES = 0;
        public const int PD_COLLATE = 0x10;
        public const int PD_CURRENTPAGE = 0x400000;
        public const int PD_DISABLEPRINTTOFILE = 0x80000;
        public const int PD_ENABLEPRINTHOOK = 0x1000;
        public const int PD_ENABLEPRINTTEMPLATE = 0x4000;
        public const int PD_ENABLEPRINTTEMPLATEHANDLE = 0x10000;
        public const int PD_ENABLESETUPHOOK = 0x2000;
        public const int PD_ENABLESETUPTEMPLATE = 0x8000;
        public const int PD_ENABLESETUPTEMPLATEHANDLE = 0x20000;
        public const int PD_EXCLUSIONFLAGS = 0x1000000;
        public const int PD_HIDEPRINTTOFILE = 0x100000;
        public const int PD_NOCURRENTPAGE = 0x800000;
        public const int PD_NONETWORKBUTTON = 0x200000;
        public const int PD_NOPAGENUMS = 8;
        public const int PD_NOSELECTION = 4;
        public const int PD_NOWARNING = 0x80;
        public const int PD_PAGENUMS = 2;
        public const int PD_PRINTSETUP = 0x40;
        public const int PD_PRINTTOFILE = 0x20;
        public const int PD_RESULT_APPLY = 2;
        public const int PD_RESULT_CANCEL = 0;
        public const int PD_RESULT_PRINT = 1;
        public const int PD_RETURNDC = 0x100;
        public const int PD_RETURNDEFAULT = 0x400;
        public const int PD_RETURNIC = 0x200;
        public const int PD_SELECTION = 1;
        public const int PD_SHOWHELP = 0x800;
        public const int PD_USEDEVMODECOPIES = 0x40000;
        public const int PD_USEDEVMODECOPIESANDCOLLATE = 0x40000;
        public const int PD_USELARGETEMPLATE = 0x10000000;
        public const int PDERR_CREATEICFAILURE = 0x100a;
        public const int PDERR_DEFAULTDIFFERENT = 0x100c;
        public const int PDERR_DNDMMISMATCH = 0x1009;
        public const int PDERR_GETDEVMODEFAIL = 0x1005;
        public const int PDERR_INITFAILURE = 0x1006;
        public const int PDERR_LOADDRVFAILURE = 0x1004;
        public const int PDERR_NODEFAULTPRN = 0x1008;
        public const int PDERR_NODEVICES = 0x1007;
        public const int PDERR_PARSEFAILURE = 0x1002;
        public const int PDERR_PRINTERNOTFOUND = 0x100b;
        public const int PDERR_RETDEFFAILURE = 0x1003;
        public const int PDERR_SETUPFAILURE = 0x1001;
        public const int PLANES = 14;
        public const int PM_NOREMOVE = 0;
        public const int PM_NOYIELD = 2;
        public const int PM_REMOVE = 1;
        public const int PRF_CHECKVISIBLE = 1;
        public const int PRF_CHILDREN = 0x10;
        public const int PRF_CLIENT = 4;
        public const int PRF_ERASEBKGND = 8;
        public const int PRF_NONCLIENT = 2;
        public const int PS_DOT = 2;
        public const int PS_ENDCAP_SQUARE = 0x100;
        public const int PS_GEOMETRIC = 0x10000;
        public const int PS_INSIDEFRAME = 6;
        public const int PS_NULL = 5;
        public const int PS_SOLID = 0;
        public const int PSD_DISABLEMARGINS = 0x10;
        public const int PSD_DISABLEORIENTATION = 0x100;
        public const int PSD_DISABLEPAPER = 0x200;
        public const int PSD_DISABLEPRINTER = 0x20;
        public const int PSD_ENABLEPAGESETUPHOOK = 0x2000;
        public const int PSD_INHUNDREDTHSOFMILLIMETERS = 8;
        public const int PSD_MARGINS = 2;
        public const int PSD_MINMARGINS = 1;
        public const int PSD_NONETWORKBUTTON = 0x200000;
        public const int PSD_SHOWHELP = 0x800;
        public static readonly int PSM_SETFINISHTEXT;
        public const int PSM_SETFINISHTEXTA = 0x473;
        public const int PSM_SETFINISHTEXTW = 0x479;
        public static readonly int PSM_SETTITLE;
        public const int PSM_SETTITLEA = 0x46f;
        public const int PSM_SETTITLEW = 0x478;
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
        public const int R2_BLACK = 1;
        public const int R2_COPYPEN = 13;
        public const int R2_MASKNOTPEN = 3;
        public const int R2_MASKPEN = 9;
        public const int R2_MASKPENNOT = 5;
        public const int R2_MERGENOTPEN = 12;
        public const int R2_MERGEPEN = 15;
        public const int R2_MERGEPENNOT = 14;
        public const int R2_NOP = 11;
        public const int R2_NOT = 6;
        public const int R2_NOTCOPYPEN = 4;
        public const int R2_NOTMASKPEN = 8;
        public const int R2_NOTMERGEPEN = 2;
        public const int R2_NOTXORPEN = 10;
        public const int R2_WHITE = 0x10;
        public const int R2_XORPEN = 7;
        public static readonly int RB_INSERTBAND;
        public const int RB_INSERTBANDA = 0x401;
        public const int RB_INSERTBANDW = 0x40a;
        public const int RDW_ALLCHILDREN = 0x80;
        public const int RDW_ERASE = 4;
        public const int RDW_ERASENOW = 0x200;
        public const int RDW_FRAME = 0x400;
        public const int RDW_INVALIDATE = 1;
        public const int RDW_UPDATENOW = 0x100;
        public const int RECO_DROP = 1;
        public const int RGN_AND = 1;
        public const int RGN_DIFF = 4;
        public const int RGN_XOR = 3;
        public const int RPC_E_CANTCALLOUT_ININPUTSYNCCALL = -2147417843;
        public const int RPC_E_CHANGED_MODE = -2147417850;
        public const int S_FALSE = 1;
        public const int S_OK = 0;
        public const int SB_BOTTOM = 7;
        public const int SB_CTL = 2;
        public const int SB_ENDSCROLL = 8;
        public const int SB_GETRECT = 0x40a;
        public static readonly int SB_GETTEXT;
        public const int SB_GETTEXTA = 0x402;
        public static readonly int SB_GETTEXTLENGTH;
        public const int SB_GETTEXTLENGTHA = 0x403;
        public const int SB_GETTEXTLENGTHW = 0x40c;
        public const int SB_GETTEXTW = 0x40d;
        public static readonly int SB_GETTIPTEXT;
        public const int SB_GETTIPTEXTA = 0x412;
        public const int SB_GETTIPTEXTW = 0x413;
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
        public const int SB_SETICON = 0x40f;
        public const int SB_SETPARTS = 0x404;
        public static readonly int SB_SETTEXT;
        public const int SB_SETTEXTA = 0x401;
        public const int SB_SETTEXTW = 0x40b;
        public static readonly int SB_SETTIPTEXT;
        public const int SB_SETTIPTEXTA = 0x410;
        public const int SB_SETTIPTEXTW = 0x411;
        public const int SB_SIMPLE = 0x409;
        public const int SB_THUMBPOSITION = 4;
        public const int SB_THUMBTRACK = 5;
        public const int SB_TOP = 6;
        public const int SB_VERT = 1;
        public const int SBARS_SIZEGRIP = 0x100;
        public const int SBS_HORZ = 0;
        public const int SBS_VERT = 1;
        public const int SBT_NOBORDERS = 0x100;
        public const int SBT_OWNERDRAW = 0x1000;
        public const int SBT_POPOUT = 0x200;
        public const int SBT_RTLREADING = 0x400;
        public const int SC_CLOSE = 0xf060;
        public const int SC_CONTEXTHELP = 0xf180;
        public const int SC_KEYMENU = 0xf100;
        public const int SC_MAXIMIZE = 0xf030;
        public const int SC_MINIMIZE = 0xf020;
        public const int SC_MOVE = 0xf010;
        public const int SC_RESTORE = 0xf120;
        public const int SC_SIZE = 0xf000;
        public const int SHGFI_ADDOVERLAYS = 0x20;
        public const int SHGFI_ATTR_SPECIFIED = 0x20000;
        public const int SHGFI_ATTRIBUTES = 0x800;
        public const int SHGFI_DISPLAYNAME = 0x200;
        public const int SHGFI_EXETYPE = 0x2000;
        public const int SHGFI_ICON = 0x100;
        public const int SHGFI_ICONLOCATION = 0x1000;
        public const int SHGFI_LARGEICON = 0;
        public const int SHGFI_LINKOVERLAY = 0x8000;
        public const int SHGFI_OPENICON = 2;
        public const int SHGFI_OVERLAYINDEX = 0x40;
        public const int SHGFI_PIDL = 8;
        public const int SHGFI_SELECTED = 0x10000;
        public const int SHGFI_SHELLICONSIZE = 4;
        public const int SHGFI_SMALLICON = 1;
        public const int SHGFI_SYSICONINDEX = 0x4000;
        public const int SHGFI_TYPENAME = 0x400;
        public const int SHGFI_USEFILEATTRIBUTES = 0x10;
        public const int SHGFP_TYPE_CURRENT = 0;
        public const int SIF_ALL = 0x17;
        public const int SIF_PAGE = 2;
        public const int SIF_POS = 4;
        public const int SIF_RANGE = 1;
        public const int SIF_TRACKPOS = 0x10;
        public const int SIZE_MAXIMIZED = 2;
        public const int SIZE_RESTORED = 0;
        public const int SM_ARRANGE = 0x38;
        public const int SM_CLEANBOOT = 0x43;
        public const int SM_CMONITORS = 80;
        public const int SM_CMOUSEBUTTONS = 0x2b;
        public const int SM_CXBORDER = 5;
        public const int SM_CXCURSOR = 13;
        public const int SM_CXDOUBLECLK = 0x24;
        public const int SM_CXDRAG = 0x44;
        public const int SM_CXEDGE = 0x2d;
        public const int SM_CXFIXEDFRAME = 7;
        public const int SM_CXFOCUSBORDER = 0x53;
        public const int SM_CXFRAME = 0x20;
        public const int SM_CXHSCROLL = 0x15;
        public const int SM_CXHTHUMB = 10;
        public const int SM_CXICON = 11;
        public const int SM_CXICONSPACING = 0x26;
        public const int SM_CXMAXIMIZED = 0x3d;
        public const int SM_CXMAXTRACK = 0x3b;
        public const int SM_CXMENUCHECK = 0x47;
        public const int SM_CXMENUSIZE = 0x36;
        public const int SM_CXMIN = 0x1c;
        public const int SM_CXMINIMIZED = 0x39;
        public const int SM_CXMINSPACING = 0x2f;
        public const int SM_CXMINTRACK = 0x22;
        public const int SM_CXSCREEN = 0;
        public const int SM_CXSIZE = 30;
        public const int SM_CXSIZEFRAME = 0x20;
        public const int SM_CXSMICON = 0x31;
        public const int SM_CXSMSIZE = 0x34;
        public const int SM_CXVIRTUALSCREEN = 0x4e;
        public const int SM_CXVSCROLL = 2;
        public const int SM_CYBORDER = 6;
        public const int SM_CYCAPTION = 4;
        public const int SM_CYCURSOR = 14;
        public const int SM_CYDOUBLECLK = 0x25;
        public const int SM_CYDRAG = 0x45;
        public const int SM_CYEDGE = 0x2e;
        public const int SM_CYFIXEDFRAME = 8;
        public const int SM_CYFOCUSBORDER = 0x54;
        public const int SM_CYFRAME = 0x21;
        public const int SM_CYHSCROLL = 3;
        public const int SM_CYICON = 12;
        public const int SM_CYICONSPACING = 0x27;
        public const int SM_CYKANJIWINDOW = 0x12;
        public const int SM_CYMAXIMIZED = 0x3e;
        public const int SM_CYMAXTRACK = 60;
        public const int SM_CYMENU = 15;
        public const int SM_CYMENUCHECK = 0x48;
        public const int SM_CYMENUSIZE = 0x37;
        public const int SM_CYMIN = 0x1d;
        public const int SM_CYMINIMIZED = 0x3a;
        public const int SM_CYMINSPACING = 0x30;
        public const int SM_CYMINTRACK = 0x23;
        public const int SM_CYSCREEN = 1;
        public const int SM_CYSIZE = 0x1f;
        public const int SM_CYSIZEFRAME = 0x21;
        public const int SM_CYSMCAPTION = 0x33;
        public const int SM_CYSMICON = 50;
        public const int SM_CYSMSIZE = 0x35;
        public const int SM_CYVIRTUALSCREEN = 0x4f;
        public const int SM_CYVSCROLL = 20;
        public const int SM_CYVTHUMB = 9;
        public const int SM_DBCSENABLED = 0x2a;
        public const int SM_DEBUG = 0x16;
        public const int SM_MENUDROPALIGNMENT = 40;
        public const int SM_MIDEASTENABLED = 0x4a;
        public const int SM_MOUSEPRESENT = 0x13;
        public const int SM_MOUSEWHEELPRESENT = 0x4b;
        public const int SM_NETWORK = 0x3f;
        public const int SM_PENWINDOWS = 0x29;
        public const int SM_REMOTESESSION = 0x1000;
        public const int SM_SAMEDISPLAYFORMAT = 0x51;
        public const int SM_SECURE = 0x2c;
        public const int SM_SHOWSOUNDS = 70;
        public const int SM_SWAPBUTTON = 0x17;
        public const int SM_XVIRTUALSCREEN = 0x4c;
        public const int SM_YVIRTUALSCREEN = 0x4d;
        public const int SORT_DEFAULT = 0;
        public const int SPI_GETACTIVEWINDOWTRACKING = 0x1000;
        public const int SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002;
        public const int SPI_GETANIMATION = 0x48;
        public const int SPI_GETBORDER = 5;
        public const int SPI_GETCARETWIDTH = 0x2006;
        public const int SPI_GETCOMBOBOXANIMATION = 0x1004;
        public const int SPI_GETDEFAULTINPUTLANG = 0x59;
        public const int SPI_GETDRAGFULLWINDOWS = 0x26;
        public const int SPI_GETDROPSHADOW = 0x1024;
        public const int SPI_GETFLATMENU = 0x1022;
        public const int SPI_GETFONTSMOOTHING = 0x4a;
        public const int SPI_GETFONTSMOOTHINGCONTRAST = 0x200c;
        public const int SPI_GETFONTSMOOTHINGTYPE = 0x200a;
        public const int SPI_GETGRADIENTCAPTIONS = 0x1008;
        public const int SPI_GETHIGHCONTRAST = 0x42;
        public const int SPI_GETHOTTRACKING = 0x100e;
        public const int SPI_GETICONTITLELOGFONT = 0x1f;
        public const int SPI_GETICONTITLEWRAP = 0x19;
        public const int SPI_GETKEYBOARDCUES = 0x100a;
        public const int SPI_GETKEYBOARDDELAY = 0x16;
        public const int SPI_GETKEYBOARDPREF = 0x44;
        public const int SPI_GETKEYBOARDSPEED = 10;
        public const int SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006;
        public const int SPI_GETMENUANIMATION = 0x1002;
        public const int SPI_GETMENUDROPALIGNMENT = 0x1b;
        public const int SPI_GETMENUFADE = 0x1012;
        public const int SPI_GETMENUSHOWDELAY = 0x6a;
        public const int SPI_GETMOUSEHOVERHEIGHT = 100;
        public const int SPI_GETMOUSEHOVERTIME = 0x66;
        public const int SPI_GETMOUSEHOVERWIDTH = 0x62;
        public const int SPI_GETMOUSESPEED = 0x70;
        public const int SPI_GETNONCLIENTMETRICS = 0x29;
        public const int SPI_GETSELECTIONFADE = 0x1014;
        public const int SPI_GETSNAPTODEFBUTTON = 0x5f;
        public const int SPI_GETTOOLTIPANIMATION = 0x1016;
        public const int SPI_GETUIEFFECTS = 0x103e;
        public const int SPI_GETWHEELSCROLLLINES = 0x68;
        public const int SPI_GETWORKAREA = 0x30;
        public const int SPI_ICONHORIZONTALSPACING = 13;
        public const int SPI_ICONVERTICALSPACING = 0x18;
        public const int SRCAND = 0x8800c6;
        public const int SRCCOPY = 0xcc0020;
        public const int SRCPAINT = 0xee0086;
        public const int SS_CENTER = 1;
        public const int SS_LEFT = 0;
        public const int SS_NOPREFIX = 0x80;
        public const int SS_OWNERDRAW = 13;
        public const int SS_RIGHT = 2;
        public const int SS_SUNKEN = 0x1000;
        public const int STAP_ALLOW_CONTROLS = 2;
        public const int STAP_ALLOW_NONCLIENT = 1;
        public const int STAP_ALLOW_WEBCONTENT = 4;
        public static int START_PAGE_GENERAL = -1;
        public const int STARTF_USESHOWWINDOW = 1;
        public const int STATFLAG_DEFAULT = 0;
        public const int STATFLAG_NONAME = 1;
        public const int STATFLAG_NOOPEN = 2;
        public const int STATUS_PENDING = 0x103;
        public const int stc4 = 0x443;
        public const int STG_E_ACCESSDENIED = -2147287035;
        public const int STG_E_DISKISWRITEPROTECTED = -2147287021;
        public const int STG_E_FILENOTFOUND = -2147287038;
        public const int STG_E_INSUFFICIENTMEMORY = -2147287032;
        public const int STG_E_INVALIDFUNCTION = -2147287039;
        public const int STG_E_INVALIDHANDLE = -2147287034;
        public const int STG_E_INVALIDPOINTER = -2147287031;
        public const int STG_E_LOCKVIOLATION = -2147287007;
        public const int STG_E_NOMOREFILES = -2147287022;
        public const int STG_E_PATHNOTFOUND = -2147287037;
        public const int STG_E_READFAULT = -2147287010;
        public const int STG_E_SEEKERROR = -2147287015;
        public const int STG_E_SHAREVIOLATION = -2147287008;
        public const int STG_E_TOOMANYOPENFILES = -2147287036;
        public const int STG_E_WRITEFAULT = -2147287011;
        public const int STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 4;
        public const int STGC_DEFAULT = 0;
        public const int STGC_ONLYIFCURRENT = 2;
        public const int STGC_OVERWRITE = 1;
        public const int STGM_CONVERT = 0x20000;
        public const int STGM_CREATE = 0x1000;
        public const int STGM_DELETEONRELEASE = 0x4000000;
        public const int STGM_READ = 0;
        public const int STGM_READWRITE = 2;
        public const int STGM_SHARE_EXCLUSIVE = 0x10;
        public const int STGM_TRANSACTED = 0x10000;
        public const int STGM_WRITE = 1;
        public const uint STILL_ACTIVE = 0x103;
        public const int STREAM_SEEK_CUR = 1;
        public const int STREAM_SEEK_END = 2;
        public const int STREAM_SEEK_SET = 0;
        public const int SUBLANG_DEFAULT = 1;
        public const int SW_ERASE = 4;
        public const int SW_HIDE = 0;
        public const int SW_INVALIDATE = 2;
        public const int SW_MAX = 10;
        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        public const int SW_NORMAL = 1;
        public const int SW_RESTORE = 9;
        public const int SW_SCROLLCHILDREN = 1;
        public const int SW_SHOW = 5;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SMOOTHSCROLL = 0x10;
        public const int SWP_DRAWFRAME = 0x20;
        public const int SWP_HIDEWINDOW = 0x80;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_NOMOVE = 2;
        public const int SWP_NOOWNERZORDER = 0x200;
        public const int SWP_NOSIZE = 1;
        public const int SWP_NOZORDER = 4;
        public const int SWP_SHOWWINDOW = 0x40;
        public const int TA_DEFAULT = 0;
        public static readonly int TB_ADDBUTTONS;
        public const int TB_ADDBUTTONSA = 0x414;
        public const int TB_ADDBUTTONSW = 0x444;
        public static readonly int TB_ADDSTRING;
        public const int TB_ADDSTRINGA = 0x41c;
        public const int TB_ADDSTRINGW = 0x44d;
        public const int TB_AUTOSIZE = 0x421;
        public const int TB_BOTTOM = 7;
        public const int TB_BUTTONSTRUCTSIZE = 0x41e;
        public const int TB_DELETEBUTTON = 0x416;
        public const int TB_ENABLEBUTTON = 0x401;
        public const int TB_ENDTRACK = 8;
        public const int TB_GETBUTTON = 0x417;
        public static readonly int TB_GETBUTTONINFO;
        public const int TB_GETBUTTONINFOA = 0x441;
        public const int TB_GETBUTTONINFOW = 0x43f;
        public const int TB_GETBUTTONSIZE = 0x43a;
        public static readonly int TB_GETBUTTONTEXT;
        public const int TB_GETBUTTONTEXTA = 0x42d;
        public const int TB_GETBUTTONTEXTW = 0x44b;
        public const int TB_GETRECT = 0x433;
        public const int TB_GETROWS = 0x428;
        public const int TB_GETTOOLTIPS = 0x423;
        public static readonly int TB_INSERTBUTTON;
        public const int TB_INSERTBUTTONA = 0x415;
        public const int TB_INSERTBUTTONW = 0x443;
        public const int TB_ISBUTTONCHECKED = 0x40a;
        public const int TB_ISBUTTONINDETERMINATE = 0x40d;
        public const int TB_LINEDOWN = 1;
        public const int TB_LINEUP = 0;
        public static readonly int TB_MAPACCELERATOR;
        public const int TB_MAPACCELERATORA = 0x44e;
        public const int TB_MAPACCELERATORW = 0x45a;
        public const int TB_PAGEDOWN = 3;
        public const int TB_PAGEUP = 2;
        public static readonly int TB_SAVERESTORE;
        public const int TB_SAVERESTOREA = 0x41a;
        public const int TB_SAVERESTOREW = 0x44c;
        public static readonly int TB_SETBUTTONINFO;
        public const int TB_SETBUTTONINFOA = 0x442;
        public const int TB_SETBUTTONINFOW = 0x440;
        public const int TB_SETBUTTONSIZE = 0x41f;
        public const int TB_SETEXTENDEDSTYLE = 0x454;
        public const int TB_SETIMAGELIST = 0x430;
        public const int TB_SETTOOLTIPS = 0x424;
        public const int TB_THUMBPOSITION = 4;
        public const int TB_THUMBTRACK = 5;
        public const int TB_TOP = 6;
        public const int TBIF_COMMAND = 0x20;
        public const int TBIF_IMAGE = 1;
        public const int TBIF_SIZE = 0x40;
        public const int TBIF_STATE = 4;
        public const int TBIF_STYLE = 8;
        public const int TBIF_TEXT = 2;
        public const int TBM_GETPOS = 0x400;
        public const int TBM_SETLINESIZE = 0x417;
        public const int TBM_SETPAGESIZE = 0x415;
        public const int TBM_SETPOS = 0x405;
        public const int TBM_SETRANGE = 0x406;
        public const int TBM_SETRANGEMAX = 0x408;
        public const int TBM_SETRANGEMIN = 0x407;
        public const int TBM_SETTIC = 0x404;
        public const int TBM_SETTICFREQ = 0x414;
        public const int TBN_DROPDOWN = -710;
        public static readonly int TBN_GETBUTTONINFO;
        public const int TBN_GETBUTTONINFOA = -700;
        public const int TBN_GETBUTTONINFOW = -720;
        public static readonly int TBN_GETDISPINFO;
        public const int TBN_GETDISPINFOA = -716;
        public const int TBN_GETDISPINFOW = -717;
        public static readonly int TBN_GETINFOTIP;
        public const int TBN_GETINFOTIPA = -718;
        public const int TBN_GETINFOTIPW = -719;
        public const int TBN_HOTITEMCHANGE = -713;
        public const int TBN_QUERYINSERT = -706;
        public const int TBS_AUTOTICKS = 1;
        public const int TBS_BOTH = 8;
        public const int TBS_BOTTOM = 0;
        public const int TBS_NOTICKS = 0x10;
        public const int TBS_TOP = 4;
        public const int TBS_VERT = 2;
        public const int TBSTATE_CHECKED = 1;
        public const int TBSTATE_ENABLED = 4;
        public const int TBSTATE_HIDDEN = 8;
        public const int TBSTATE_INDETERMINATE = 0x10;
        public const int TBSTYLE_BUTTON = 0;
        public const int TBSTYLE_CHECK = 2;
        public const int TBSTYLE_DROPDOWN = 8;
        public const int TBSTYLE_EX_DRAWDDARROWS = 1;
        public const int TBSTYLE_FLAT = 0x800;
        public const int TBSTYLE_LIST = 0x1000;
        public const int TBSTYLE_SEP = 1;
        public const int TBSTYLE_TOOLTIPS = 0x100;
        public const int TBSTYLE_WRAPPABLE = 0x200;
        public const int TCIF_IMAGE = 2;
        public const int TCIF_TEXT = 1;
        public const int TCM_ADJUSTRECT = 0x1328;
        public const int TCM_DELETEALLITEMS = 0x1309;
        public const int TCM_DELETEITEM = 0x1308;
        public const int TCM_GETCURSEL = 0x130b;
        public static readonly int TCM_GETITEM;
        public const int TCM_GETITEMA = 0x1305;
        public const int TCM_GETITEMRECT = 0x130a;
        public const int TCM_GETITEMW = 0x133c;
        public const int TCM_GETROWCOUNT = 0x132c;
        public const int TCM_GETTOOLTIPS = 0x132d;
        public static readonly int TCM_INSERTITEM;
        public const int TCM_INSERTITEMA = 0x1307;
        public const int TCM_INSERTITEMW = 0x133e;
        public const int TCM_SETCURSEL = 0x130c;
        public const int TCM_SETIMAGELIST = 0x1303;
        public static readonly int TCM_SETITEM;
        public const int TCM_SETITEMA = 0x1306;
        public const int TCM_SETITEMSIZE = 0x1329;
        public const int TCM_SETITEMW = 0x133d;
        public const int TCM_SETPADDING = 0x132b;
        public const int TCM_SETTOOLTIPS = 0x132e;
        public const int TCN_SELCHANGE = -551;
        public const int TCN_SELCHANGING = -552;
        public const int TCS_BOTTOM = 2;
        public const int TCS_BUTTONS = 0x100;
        public const int TCS_FIXEDWIDTH = 0x400;
        public const int TCS_FLATBUTTONS = 8;
        public const int TCS_HOTTRACK = 0x40;
        public const int TCS_MULTILINE = 0x200;
        public const int TCS_OWNERDRAWFIXED = 0x2000;
        public const int TCS_RAGGEDRIGHT = 0x800;
        public const int TCS_RIGHT = 2;
        public const int TCS_RIGHTJUSTIFY = 0;
        public const int TCS_TABS = 0;
        public const int TCS_TOOLTIPS = 0x4000;
        public const int TCS_VERTICAL = 0x80;
        public const int TME_HOVER = 1;
        public const int TME_LEAVE = 2;
        public const int TMPF_FIXED_PITCH = 1;
        public const string TOOLTIPS_CLASS = "tooltips_class32";
        public const int TPM_LEFTALIGN = 0;
        public const int TPM_LEFTBUTTON = 0;
        public const int TPM_RIGHTALIGN = 8;
        public const int TPM_RIGHTBUTTON = 2;
        public const int TPM_VERTICAL = 0x40;
        public const int TRANSPARENT = 1;
        public const int TTDT_AUTOMATIC = 0;
        public const int TTDT_AUTOPOP = 2;
        public const int TTDT_INITIAL = 3;
        public const int TTDT_RESHOW = 1;
        public const int TTF_ABSOLUTE = 0x80;
        public const int TTF_CENTERTIP = 2;
        public const int TTF_IDISHWND = 1;
        public const int TTF_RTLREADING = 4;
        public const int TTF_SUBCLASS = 0x10;
        public const int TTF_TRACK = 0x20;
        public const int TTF_TRANSPARENT = 0x100;
        public const int TTI_WARNING = 2;
        public const int TTM_ACTIVATE = 0x401;
        public static readonly int TTM_ADDTOOL;
        public const int TTM_ADDTOOLA = 0x404;
        public const int TTM_ADDTOOLW = 0x432;
        public const int TTM_ADJUSTRECT = 0x41f;
        public static readonly int TTM_DELTOOL;
        public const int TTM_DELTOOLA = 0x405;
        public const int TTM_DELTOOLW = 0x433;
        public static readonly int TTM_ENUMTOOLS;
        public const int TTM_ENUMTOOLSA = 0x40e;
        public const int TTM_ENUMTOOLSW = 0x43a;
        public static readonly int TTM_GETCURRENTTOOL;
        public const int TTM_GETCURRENTTOOLA = 0x40f;
        public const int TTM_GETCURRENTTOOLW = 0x43b;
        public const int TTM_GETDELAYTIME = 0x415;
        public static readonly int TTM_GETTEXT;
        public const int TTM_GETTEXTA = 0x40b;
        public const int TTM_GETTEXTW = 0x438;
        public const int TTM_GETTIPBKCOLOR = 0x416;
        public const int TTM_GETTIPTEXTCOLOR = 0x417;
        public static readonly int TTM_GETTOOLINFO;
        public const int TTM_GETTOOLINFOA = 0x408;
        public const int TTM_GETTOOLINFOW = 0x435;
        public static readonly int TTM_HITTEST;
        public const int TTM_HITTESTA = 0x40a;
        public const int TTM_HITTESTW = 0x437;
        public static readonly int TTM_NEWTOOLRECT;
        public const int TTM_NEWTOOLRECTA = 0x406;
        public const int TTM_NEWTOOLRECTW = 0x434;
        public const int TTM_POP = 0x41c;
        public const int TTM_RELAYEVENT = 0x407;
        public const int TTM_SETDELAYTIME = 0x403;
        public const int TTM_SETMAXTIPWIDTH = 0x418;
        public const int TTM_SETTIPBKCOLOR = 0x413;
        public const int TTM_SETTIPTEXTCOLOR = 0x414;
        public static readonly int TTM_SETTITLE;
        public const int TTM_SETTITLEA = 0x420;
        public const int TTM_SETTITLEW = 0x421;
        public static readonly int TTM_SETTOOLINFO;
        public const int TTM_SETTOOLINFOA = 0x409;
        public const int TTM_SETTOOLINFOW = 0x436;
        public const int TTM_TRACKACTIVATE = 0x411;
        public const int TTM_TRACKPOSITION = 0x412;
        public const int TTM_UPDATE = 0x41d;
        public static readonly int TTM_UPDATETIPTEXT;
        public const int TTM_UPDATETIPTEXTA = 0x40c;
        public const int TTM_UPDATETIPTEXTW = 0x439;
        public const int TTM_WINDOWFROMPOINT = 0x410;
        public static readonly int TTN_GETDISPINFO;
        public const int TTN_GETDISPINFOA = -520;
        public const int TTN_GETDISPINFOW = -530;
        public static readonly int TTN_NEEDTEXT;
        public const int TTN_NEEDTEXTA = -520;
        public const int TTN_NEEDTEXTW = -530;
        public const int TTN_POP = -522;
        public const int TTN_SHOW = -521;
        public const int TTS_ALWAYSTIP = 1;
        public const int TTS_BALLOON = 0x40;
        public const int TTS_NOANIMATE = 0x10;
        public const int TTS_NOFADE = 0x20;
        public const int TTS_NOPREFIX = 2;
        public const int TV_FIRST = 0x1100;
        public const int TVC_BYKEYBOARD = 2;
        public const int TVC_BYMOUSE = 1;
        public const int TVC_UNKNOWN = 0;
        public const int TVE_COLLAPSE = 1;
        public const int TVE_EXPAND = 2;
        public const int TVGN_CARET = 9;
        public const int TVGN_DROPHILITE = 8;
        public const int TVGN_FIRSTVISIBLE = 5;
        public const int TVGN_NEXT = 1;
        public const int TVGN_NEXTVISIBLE = 6;
        public const int TVGN_PREVIOUS = 2;
        public const int TVGN_PREVIOUSVISIBLE = 7;
        public const int TVHT_ABOVE = 0x100;
        public const int TVHT_BELOW = 0x200;
        public const int TVHT_NOWHERE = 1;
        public const int TVHT_ONITEM = 70;
        public const int TVHT_ONITEMBUTTON = 0x10;
        public const int TVHT_ONITEMICON = 2;
        public const int TVHT_ONITEMINDENT = 8;
        public const int TVHT_ONITEMLABEL = 4;
        public const int TVHT_ONITEMRIGHT = 0x20;
        public const int TVHT_ONITEMSTATEICON = 0x40;
        public const int TVHT_TOLEFT = 0x800;
        public const int TVHT_TORIGHT = 0x400;
        public const int TVI_FIRST = -65535;
        public const int TVI_ROOT = -65536;
        public const int TVIF_HANDLE = 0x10;
        public const int TVIF_IMAGE = 2;
        public const int TVIF_PARAM = 4;
        public const int TVIF_SELECTEDIMAGE = 0x20;
        public const int TVIF_STATE = 8;
        public const int TVIF_TEXT = 1;
        public const int TVIS_EXPANDED = 0x20;
        public const int TVIS_EXPANDEDONCE = 0x40;
        public const int TVIS_SELECTED = 2;
        public const int TVIS_STATEIMAGEMASK = 0xf000;
        public const int TVM_DELETEITEM = 0x1101;
        public static readonly int TVM_EDITLABEL;
        public const int TVM_EDITLABELA = 0x110e;
        public const int TVM_EDITLABELW = 0x1141;
        public const int TVM_ENDEDITLABELNOW = 0x1116;
        public const int TVM_ENSUREVISIBLE = 0x1114;
        public const int TVM_EXPAND = 0x1102;
        public const int TVM_GETEDITCONTROL = 0x110f;
        public const int TVM_GETINDENT = 0x1106;
        public static readonly int TVM_GETISEARCHSTRING;
        public const int TVM_GETISEARCHSTRINGA = 0x1117;
        public const int TVM_GETISEARCHSTRINGW = 0x1140;
        public static readonly int TVM_GETITEM;
        public const int TVM_GETITEMA = 0x110c;
        public const int TVM_GETITEMHEIGHT = 0x111c;
        public const int TVM_GETITEMRECT = 0x1104;
        public const int TVM_GETITEMW = 0x113e;
        public const int TVM_GETLINECOLOR = 0x1129;
        public const int TVM_GETNEXTITEM = 0x110a;
        public const int TVM_GETVISIBLECOUNT = 0x1110;
        public const int TVM_HITTEST = 0x1111;
        public static readonly int TVM_INSERTITEM;
        public const int TVM_INSERTITEMA = 0x1100;
        public const int TVM_INSERTITEMW = 0x1132;
        public const int TVM_SELECTITEM = 0x110b;
        public const int TVM_SETBKCOLOR = 0x111d;
        public const int TVM_SETIMAGELIST = 0x1109;
        public const int TVM_SETINDENT = 0x1107;
        public static readonly int TVM_SETITEM;
        public const int TVM_SETITEMA = 0x110d;
        public const int TVM_SETITEMHEIGHT = 0x111b;
        public const int TVM_SETITEMW = 0x113f;
        public const int TVM_SETLINECOLOR = 0x1128;
        public const int TVM_SETTEXTCOLOR = 0x111e;
        public const int TVM_SETTOOLTIPS = 0x1118;
        public const int TVM_SORTCHILDRENCB = 0x1115;
        public static readonly int TVN_BEGINDRAG;
        public const int TVN_BEGINDRAGA = -407;
        public const int TVN_BEGINDRAGW = -456;
        public static readonly int TVN_BEGINLABELEDIT;
        public const int TVN_BEGINLABELEDITA = -410;
        public const int TVN_BEGINLABELEDITW = -459;
        public static readonly int TVN_BEGINRDRAG;
        public const int TVN_BEGINRDRAGA = -408;
        public const int TVN_BEGINRDRAGW = -457;
        public static readonly int TVN_ENDLABELEDIT;
        public const int TVN_ENDLABELEDITA = -411;
        public const int TVN_ENDLABELEDITW = -460;
        public static readonly int TVN_GETDISPINFO;
        public const int TVN_GETDISPINFOA = -403;
        public const int TVN_GETDISPINFOW = -452;
        public const int TVN_GETINFOTIPA = -413;
        public const int TVN_GETINFOTIPW = -414;
        public static readonly int TVN_ITEMEXPANDED;
        public const int TVN_ITEMEXPANDEDA = -406;
        public const int TVN_ITEMEXPANDEDW = -455;
        public static readonly int TVN_ITEMEXPANDING;
        public const int TVN_ITEMEXPANDINGA = -405;
        public const int TVN_ITEMEXPANDINGW = -454;
        public static readonly int TVN_SELCHANGED;
        public const int TVN_SELCHANGEDA = -402;
        public const int TVN_SELCHANGEDW = -451;
        public static readonly int TVN_SELCHANGING;
        public const int TVN_SELCHANGINGA = -401;
        public const int TVN_SELCHANGINGW = -450;
        public static readonly int TVN_SETDISPINFO;
        public const int TVN_SETDISPINFOA = -404;
        public const int TVN_SETDISPINFOW = -453;
        public const int TVS_CHECKBOXES = 0x100;
        public const int TVS_EDITLABELS = 8;
        public const int TVS_FULLROWSELECT = 0x1000;
        public const int TVS_HASBUTTONS = 1;
        public const int TVS_HASLINES = 2;
        public const int TVS_INFOTIP = 0x800;
        public const int TVS_LINESATROOT = 4;
        public const int TVS_NONEVENHEIGHT = 0x4000;
        public const int TVS_NOTOOLTIPS = 0x80;
        public const int TVS_RTLREADING = 0x40;
        public const int TVS_SHOWSELALWAYS = 0x20;
        public const int TVS_TRACKSELECT = 0x200;
        public const int TVSIL_STATE = 2;
        public const int TYMED_NULL = 0;
        public const int UIS_CLEAR = 2;
        public const int UIS_INITIALIZE = 3;
        public const int UIS_SET = 1;
        public const int UISF_HIDEACCEL = 2;
        public const int UISF_HIDEFOCUS = 1;
        public const int UOI_FLAGS = 1;
        public const int USERCLASSTYPE_APPNAME = 3;
        public const int USERCLASSTYPE_FULL = 1;
        public const int USERCLASSTYPE_SHORT = 2;
        public const string uuid_IAccessible = "{618736E0-3C3D-11CF-810C-00AA00389B71}";
        public const string uuid_IEnumVariant = "{00020404-0000-0000-C000-000000000046}";
        public const int VIEW_E_DRAW = -2147221184;
        public const int VK_CAPITAL = 20;
        public const int VK_CONTROL = 0x11;
        public const int VK_DELETE = 0x2e;
        public const int VK_DOWN = 40;
        public const int VK_END = 0x23;
        public const int VK_ESCAPE = 0x1b;
        public const int VK_HOME = 0x24;
        public const int VK_INSERT = 0x2d;
        public const int VK_KANA = 0x15;
        public const int VK_LEFT = 0x25;
        public const int VK_MENU = 0x12;
        public const int VK_NEXT = 0x22;
        public const int VK_NUMLOCK = 0x90;
        public const int VK_PRIOR = 0x21;
        public const int VK_RIGHT = 0x27;
        public const int VK_SCROLL = 0x91;
        public const int VK_SHIFT = 0x10;
        public const int VK_TAB = 9;
        public const int VK_UP = 0x26;
        public const int WA_ACTIVE = 1;
        public const int WA_CLICKACTIVE = 2;
        public const int WA_INACTIVE = 0;
        public const string WC_DATETIMEPICK = "SysDateTimePick32";
        public const string WC_LISTVIEW = "SysListView32";
        public const string WC_MONTHCAL = "SysMonthCal32";
        public const string WC_PROGRESS = "msctls_progress32";
        public const string WC_STATUSBAR = "msctls_statusbar32";
        public const string WC_TABCONTROL = "SysTabControl32";
        public const string WC_TOOLBAR = "ToolbarWindow32";
        public const string WC_TRACKBAR = "msctls_trackbar32";
        public const string WC_TREEVIEW = "SysTreeView32";
        public const int WH_GETMESSAGE = 3;
        public const int WH_JOURNALPLAYBACK = 1;
        public const int WH_MOUSE = 7;
        public const int WHEEL_DELTA = 120;
        public const int WINDING = 2;
        public const int WM_ACTIVATE = 6;
        public const int WM_ACTIVATEAPP = 0x1c;
        public const int WM_AFXFIRST = 0x360;
        public const int WM_AFXLAST = 0x37f;
        public const int WM_APP = 0x8000;
        public const int WM_ASKCBFORMATNAME = 780;
        public const int WM_CANCELJOURNAL = 0x4b;
        public const int WM_CANCELMODE = 0x1f;
        public const int WM_CAPTURECHANGED = 0x215;
        public const int WM_CHANGECBCHAIN = 0x30d;
        public const int WM_CHANGEUISTATE = 0x127;
        public const int WM_CHAR = 0x102;
        public const int WM_CHARTOITEM = 0x2f;
        public const int WM_CHILDACTIVATE = 0x22;
        public const int WM_CHOOSEFONT_GETLOGFONT = 0x401;
        public const int WM_CLEAR = 0x303;
        public const int WM_CLOSE = 0x10;
        public const int WM_COMMAND = 0x111;
        public const int WM_COMMNOTIFY = 0x44;
        public const int WM_COMPACTING = 0x41;
        public const int WM_COMPAREITEM = 0x39;
        public const int WM_CONTEXTMENU = 0x7b;
        public const int WM_COPY = 0x301;
        public const int WM_COPYDATA = 0x4a;
        public const int WM_CREATE = 1;
        public const int WM_CTLCOLOR = 0x19;
        public const int WM_CTLCOLORBTN = 0x135;
        public const int WM_CTLCOLORDLG = 310;
        public const int WM_CTLCOLOREDIT = 0x133;
        public const int WM_CTLCOLORLISTBOX = 0x134;
        public const int WM_CTLCOLORMSGBOX = 0x132;
        public const int WM_CTLCOLORSCROLLBAR = 0x137;
        public const int WM_CTLCOLORSTATIC = 0x138;
        public const int WM_CUT = 0x300;
        public const int WM_DEADCHAR = 0x103;
        public const int WM_DELETEITEM = 0x2d;
        public const int WM_DESTROY = 2;
        public const int WM_DESTROYCLIPBOARD = 0x307;
        public const int WM_DEVICECHANGE = 0x219;
        public const int WM_DEVMODECHANGE = 0x1b;
        public const int WM_DISPLAYCHANGE = 0x7e;
        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_DRAWITEM = 0x2b;
        public const int WM_DROPFILES = 0x233;
        public const int WM_ENABLE = 10;
        public const int WM_ENDSESSION = 0x16;
        public const int WM_ENTERIDLE = 0x121;
        public const int WM_ENTERMENULOOP = 0x211;
        public const int WM_ENTERSIZEMOVE = 0x231;
        public const int WM_ERASEBKGND = 20;
        public const int WM_EXITMENULOOP = 530;
        public const int WM_EXITSIZEMOVE = 0x232;
        public const int WM_FONTCHANGE = 0x1d;
        public const int WM_GETDLGCODE = 0x87;
        public const int WM_GETFONT = 0x31;
        public const int WM_GETHOTKEY = 0x33;
        public const int WM_GETICON = 0x7f;
        public const int WM_GETMINMAXINFO = 0x24;
        public const int WM_GETOBJECT = 0x3d;
        public const int WM_GETTEXT = 13;
        public const int WM_GETTEXTLENGTH = 14;
        public const int WM_HANDHELDFIRST = 0x358;
        public const int WM_HANDHELDLAST = 0x35f;
        public const int WM_HELP = 0x53;
        public const int WM_HOTKEY = 0x312;
        public const int WM_HSCROLL = 0x114;
        public const int WM_HSCROLLCLIPBOARD = 0x30e;
        public const int WM_ICONERASEBKGND = 0x27;
        public const int WM_IME_CHAR = 0x286;
        public const int WM_IME_COMPOSITION = 0x10f;
        public const int WM_IME_COMPOSITIONFULL = 0x284;
        public const int WM_IME_CONTROL = 0x283;
        public const int WM_IME_ENDCOMPOSITION = 270;
        public const int WM_IME_KEYDOWN = 0x290;
        public const int WM_IME_KEYLAST = 0x10f;
        public const int WM_IME_KEYUP = 0x291;
        public const int WM_IME_NOTIFY = 0x282;
        public const int WM_IME_SELECT = 0x285;
        public const int WM_IME_SETCONTEXT = 0x281;
        public const int WM_IME_STARTCOMPOSITION = 0x10d;
        public const int WM_INITDIALOG = 0x110;
        public const int WM_INITMENU = 0x116;
        public const int WM_INITMENUPOPUP = 0x117;
        public const int WM_INPUTLANGCHANGE = 0x51;
        public const int WM_INPUTLANGCHANGEREQUEST = 80;
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
        public const int WM_MDIACTIVATE = 0x222;
        public const int WM_MDICASCADE = 0x227;
        public const int WM_MDICREATE = 0x220;
        public const int WM_MDIDESTROY = 0x221;
        public const int WM_MDIGETACTIVE = 0x229;
        public const int WM_MDIICONARRANGE = 0x228;
        public const int WM_MDIMAXIMIZE = 0x225;
        public const int WM_MDINEXT = 0x224;
        public const int WM_MDIREFRESHMENU = 0x234;
        public const int WM_MDIRESTORE = 0x223;
        public const int WM_MDISETMENU = 560;
        public const int WM_MDITILE = 550;
        public const int WM_MEASUREITEM = 0x2c;
        public const int WM_MENUCHAR = 0x120;
        public const int WM_MENUSELECT = 0x11f;
        public const int WM_MOUSEACTIVATE = 0x21;
        public const int WM_MOUSEFIRST = 0x200;
        public const int WM_MOUSEHOVER = 0x2a1;
        public const int WM_MOUSELAST = 0x20a;
        public const int WM_MOUSELEAVE = 0x2a3;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_MOUSEWHEEL = 0x20a;
        public const int WM_MOVE = 3;
        public const int WM_MOVING = 0x216;
        public const int WM_NCACTIVATE = 0x86;
        public const int WM_NCCALCSIZE = 0x83;
        public const int WM_NCCREATE = 0x81;
        public const int WM_NCDESTROY = 130;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_NCLBUTTONDBLCLK = 0xa3;
        public const int WM_NCLBUTTONDOWN = 0xa1;
        public const int WM_NCLBUTTONUP = 0xa2;
        public const int WM_NCMBUTTONDBLCLK = 0xa9;
        public const int WM_NCMBUTTONDOWN = 0xa7;
        public const int WM_NCMBUTTONUP = 0xa8;
        public const int WM_NCMOUSELEAVE = 0x2a2;
        public const int WM_NCMOUSEMOVE = 160;
        public const int WM_NCPAINT = 0x85;
        public const int WM_NCRBUTTONDBLCLK = 0xa6;
        public const int WM_NCRBUTTONDOWN = 0xa4;
        public const int WM_NCRBUTTONUP = 0xa5;
        public const int WM_NCXBUTTONDBLCLK = 0xad;
        public const int WM_NCXBUTTONDOWN = 0xab;
        public const int WM_NCXBUTTONUP = 0xac;
        public const int WM_NEXTDLGCTL = 40;
        public const int WM_NEXTMENU = 0x213;
        public const int WM_NOTIFY = 0x4e;
        public const int WM_NOTIFYFORMAT = 0x55;
        public const int WM_NULL = 0;
        public const int WM_PAINT = 15;
        public const int WM_PAINTCLIPBOARD = 0x309;
        public const int WM_PAINTICON = 0x26;
        public const int WM_PALETTECHANGED = 0x311;
        public const int WM_PALETTEISCHANGING = 0x310;
        public const int WM_PARENTNOTIFY = 0x210;
        public const int WM_PASTE = 770;
        public const int WM_PENWINFIRST = 0x380;
        public const int WM_PENWINLAST = 0x38f;
        public const int WM_POWER = 0x48;
        public const int WM_POWERBROADCAST = 0x218;
        public const int WM_PRINT = 0x317;
        public const int WM_PRINTCLIENT = 0x318;
        public const int WM_QUERYDRAGICON = 0x37;
        public const int WM_QUERYENDSESSION = 0x11;
        public const int WM_QUERYNEWPALETTE = 0x30f;
        public const int WM_QUERYOPEN = 0x13;
        public const int WM_QUERYUISTATE = 0x129;
        public const int WM_QUEUESYNC = 0x23;
        public const int WM_QUIT = 0x12;
        public const int WM_RBUTTONDBLCLK = 0x206;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_REFLECT = 0x2000;
        public const int WM_RENDERALLFORMATS = 0x306;
        public const int WM_RENDERFORMAT = 0x305;
        public const int WM_SETCURSOR = 0x20;
        public const int WM_SETFOCUS = 7;
        public const int WM_SETFONT = 0x30;
        public const int WM_SETHOTKEY = 50;
        public const int WM_SETICON = 0x80;
        public const int WM_SETREDRAW = 11;
        public const int WM_SETTEXT = 12;
        public const int WM_SETTINGCHANGE = 0x1a;
        public const int WM_SHOWWINDOW = 0x18;
        public const int WM_SIZE = 5;
        public const int WM_SIZECLIPBOARD = 0x30b;
        public const int WM_SIZING = 0x214;
        public const int WM_SPOOLERSTATUS = 0x2a;
        public const int WM_STYLECHANGED = 0x7d;
        public const int WM_STYLECHANGING = 0x7c;
        public const int WM_SYSCHAR = 0x106;
        public const int WM_SYSCOLORCHANGE = 0x15;
        public const int WM_SYSCOMMAND = 0x112;
        public const int WM_SYSDEADCHAR = 0x107;
        public const int WM_SYSKEYDOWN = 260;
        public const int WM_SYSKEYUP = 0x105;
        public const int WM_TCARD = 0x52;
        public const int WM_THEMECHANGED = 0x31a;
        public const int WM_TIMECHANGE = 30;
        public const int WM_TIMER = 0x113;
        public const int WM_UNDO = 0x304;
        public const int WM_UNINITMENUPOPUP = 0x125;
        public const int WM_UPDATEUISTATE = 0x128;
        public const int WM_USER = 0x400;
        public const int WM_USERCHANGED = 0x54;
        public const int WM_VKEYTOITEM = 0x2e;
        public const int WM_VSCROLL = 0x115;
        public const int WM_VSCROLLCLIPBOARD = 0x30a;
        public const int WM_WINDOWPOSCHANGED = 0x47;
        public const int WM_WINDOWPOSCHANGING = 70;
        public const int WM_WININICHANGE = 0x1a;
        public const int WM_XBUTTONDBLCLK = 0x20d;
        public const int WM_XBUTTONDOWN = 0x20b;
        public const int WM_XBUTTONUP = 0x20c;
        private static int wmMouseEnterMessage = -1;
        private static int wmUnSubclass = -1;
        public const int WPF_SETMINPOSITION = 1;
        public const int WS_BORDER = 0x800000;
        public const int WS_CAPTION = 0xc00000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_CLIPCHILDREN = 0x2000000;
        public const int WS_CLIPSIBLINGS = 0x4000000;
        public const int WS_DISABLED = 0x8000000;
        public const int WS_DLGFRAME = 0x400000;
        public const int WS_EX_APPWINDOW = 0x40000;
        public const int WS_EX_CLIENTEDGE = 0x200;
        public const int WS_EX_CONTEXTHELP = 0x400;
        public const int WS_EX_CONTROLPARENT = 0x10000;
        public const int WS_EX_DLGMODALFRAME = 1;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_LAYOUTRTL = 0x400000;
        public const int WS_EX_LEFT = 0;
        public const int WS_EX_LEFTSCROLLBAR = 0x4000;
        public const int WS_EX_MDICHILD = 0x40;
        public const int WS_EX_NOINHERITLAYOUT = 0x100000;
        public const int WS_EX_RIGHT = 0x1000;
        public const int WS_EX_RTLREADING = 0x2000;
        public const int WS_EX_STATICEDGE = 0x20000;
        public const int WS_EX_TOOLWINDOW = 0x80;
        public const int WS_EX_TOPMOST = 8;
        public const int WS_HSCROLL = 0x100000;
        public const int WS_MAXIMIZE = 0x1000000;
        public const int WS_MAXIMIZEBOX = 0x10000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_MINIMIZEBOX = 0x20000;
        public const int WS_OVERLAPPED = 0;
        public const int WS_POPUP = -2147483648;
        public const int WS_SYSMENU = 0x80000;
        public const int WS_TABSTOP = 0x10000;
        public const int WS_THICKFRAME = 0x40000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_VSCROLL = 0x200000;
        public const int WSF_VISIBLE = 1;
        public const int XBUTTON1 = 1;
        public const int XBUTTON2 = 2;

        static NativeMethods()
        {
            if (Marshal.SystemDefaultCharSize == 1)
            {
                BFFM_SETSELECTION = 0x466;
                CBEM_GETITEM = 0x404;
                CBEM_SETITEM = 0x405;
                CBEN_ENDEDIT = -805;
                CBEM_INSERTITEM = 0x401;
                LVM_GETITEMTEXT = 0x102d;
                LVM_SETITEMTEXT = 0x102e;
                ACM_OPEN = 0x464;
                DTM_SETFORMAT = 0x1005;
                DTN_USERSTRING = -758;
                DTN_WMKEYDOWN = -757;
                DTN_FORMAT = -756;
                DTN_FORMATQUERY = -755;
                EMR_POLYTEXTOUT = 0x60;
                HDM_INSERTITEM = 0x1201;
                HDM_GETITEM = 0x1203;
                HDM_SETITEM = 0x1204;
                HDN_ITEMCHANGING = -300;
                HDN_ITEMCHANGED = -301;
                HDN_ITEMCLICK = -302;
                HDN_ITEMDBLCLICK = -303;
                HDN_DIVIDERDBLCLICK = -305;
                HDN_BEGINTRACK = -306;
                HDN_ENDTRACK = -307;
                HDN_TRACK = -308;
                HDN_GETDISPINFO = -309;
                LVM_SETBKIMAGE = 0x1044;
                LVM_GETITEM = 0x1005;
                LVM_SETITEM = 0x1006;
                LVM_INSERTITEM = 0x1007;
                LVM_FINDITEM = 0x100d;
                LVM_GETSTRINGWIDTH = 0x1011;
                LVM_EDITLABEL = 0x1017;
                LVM_GETCOLUMN = 0x1019;
                LVM_SETCOLUMN = 0x101a;
                LVM_GETISEARCHSTRING = 0x1034;
                LVM_INSERTCOLUMN = 0x101b;
                LVN_BEGINLABELEDIT = -105;
                LVN_ENDLABELEDIT = -106;
                LVN_ODFINDITEM = -152;
                LVN_GETDISPINFO = -150;
                LVN_GETINFOTIP = -157;
                LVN_SETDISPINFO = -151;
                PSM_SETTITLE = 0x46f;
                PSM_SETFINISHTEXT = 0x473;
                RB_INSERTBAND = 0x401;
                SB_SETTEXT = 0x401;
                SB_GETTEXT = 0x402;
                SB_GETTEXTLENGTH = 0x403;
                SB_SETTIPTEXT = 0x410;
                SB_GETTIPTEXT = 0x412;
                TB_SAVERESTORE = 0x41a;
                TB_ADDSTRING = 0x41c;
                TB_GETBUTTONTEXT = 0x42d;
                TB_MAPACCELERATOR = 0x44e;
                TB_GETBUTTONINFO = 0x441;
                TB_SETBUTTONINFO = 0x442;
                TB_INSERTBUTTON = 0x415;
                TB_ADDBUTTONS = 0x414;
                TBN_GETBUTTONINFO = -700;
                TBN_GETINFOTIP = -718;
                TBN_GETDISPINFO = -716;
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
                TTN_GETDISPINFO = -520;
                TTN_NEEDTEXT = -520;
                TVM_INSERTITEM = 0x1100;
                TVM_GETITEM = 0x110c;
                TVM_SETITEM = 0x110d;
                TVM_EDITLABEL = 0x110e;
                TVM_GETISEARCHSTRING = 0x1117;
                TVN_SELCHANGING = -401;
                TVN_SELCHANGED = -402;
                TVN_GETDISPINFO = -403;
                TVN_SETDISPINFO = -404;
                TVN_ITEMEXPANDING = -405;
                TVN_ITEMEXPANDED = -406;
                TVN_BEGINDRAG = -407;
                TVN_BEGINRDRAG = -408;
                TVN_BEGINLABELEDIT = -410;
                TVN_ENDLABELEDIT = -411;
                TCM_GETITEM = 0x1305;
                TCM_SETITEM = 0x1306;
                TCM_INSERTITEM = 0x1307;
            }
            else
            {
                BFFM_SETSELECTION = 0x467;
                CBEM_GETITEM = 0x40d;
                CBEM_SETITEM = 0x40c;
                CBEN_ENDEDIT = -806;
                CBEM_INSERTITEM = 0x40b;
                LVM_GETITEMTEXT = 0x1073;
                LVM_SETITEMTEXT = 0x1074;
                ACM_OPEN = 0x467;
                DTM_SETFORMAT = 0x1032;
                DTN_USERSTRING = -745;
                DTN_WMKEYDOWN = -744;
                DTN_FORMAT = -743;
                DTN_FORMATQUERY = -742;
                EMR_POLYTEXTOUT = 0x61;
                HDM_INSERTITEM = 0x120a;
                HDM_GETITEM = 0x120b;
                HDM_SETITEM = 0x120c;
                HDN_ITEMCHANGING = -320;
                HDN_ITEMCHANGED = -321;
                HDN_ITEMCLICK = -322;
                HDN_ITEMDBLCLICK = -323;
                HDN_DIVIDERDBLCLICK = -325;
                HDN_BEGINTRACK = -326;
                HDN_ENDTRACK = -327;
                HDN_TRACK = -328;
                HDN_GETDISPINFO = -329;
                LVM_SETBKIMAGE = 0x108a;
                LVM_GETITEM = 0x104b;
                LVM_SETITEM = 0x104c;
                LVM_INSERTITEM = 0x104d;
                LVM_FINDITEM = 0x1053;
                LVM_GETSTRINGWIDTH = 0x1057;
                LVM_EDITLABEL = 0x1076;
                LVM_GETCOLUMN = 0x105f;
                LVM_SETCOLUMN = 0x1060;
                LVM_GETISEARCHSTRING = 0x1075;
                LVM_INSERTCOLUMN = 0x1061;
                LVN_BEGINLABELEDIT = -175;
                LVN_ENDLABELEDIT = -176;
                LVN_ODFINDITEM = -179;
                LVN_GETDISPINFO = -177;
                LVN_GETINFOTIP = -158;
                LVN_SETDISPINFO = -178;
                PSM_SETTITLE = 0x478;
                PSM_SETFINISHTEXT = 0x479;
                RB_INSERTBAND = 0x40a;
                SB_SETTEXT = 0x40b;
                SB_GETTEXT = 0x40d;
                SB_GETTEXTLENGTH = 0x40c;
                SB_SETTIPTEXT = 0x411;
                SB_GETTIPTEXT = 0x413;
                TB_SAVERESTORE = 0x44c;
                TB_ADDSTRING = 0x44d;
                TB_GETBUTTONTEXT = 0x44b;
                TB_MAPACCELERATOR = 0x45a;
                TB_GETBUTTONINFO = 0x43f;
                TB_SETBUTTONINFO = 0x440;
                TB_INSERTBUTTON = 0x443;
                TB_ADDBUTTONS = 0x444;
                TBN_GETBUTTONINFO = -720;
                TBN_GETINFOTIP = -719;
                TBN_GETDISPINFO = -717;
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
                TTN_GETDISPINFO = -530;
                TTN_NEEDTEXT = -530;
                TVM_INSERTITEM = 0x1132;
                TVM_GETITEM = 0x113e;
                TVM_SETITEM = 0x113f;
                TVM_EDITLABEL = 0x1141;
                TVM_GETISEARCHSTRING = 0x1140;
                TVN_SELCHANGING = -450;
                TVN_SELCHANGED = -451;
                TVN_GETDISPINFO = -452;
                TVN_SETDISPINFO = -453;
                TVN_ITEMEXPANDING = -454;
                TVN_ITEMEXPANDED = -455;
                TVN_BEGINDRAG = -456;
                TVN_BEGINRDRAG = -457;
                TVN_BEGINLABELEDIT = -459;
                TVN_ENDLABELEDIT = -460;
                TCM_GETITEM = 0x133c;
                TCM_SETITEM = 0x133d;
                TCM_INSERTITEM = 0x133e;
            }
        }

        public static bool Failed(int hr)
        {
            return (hr < 0);
        }

        internal static string GetLocalPath(string fileName)
        {
            Uri uri = new Uri(fileName);
            return (uri.LocalPath + uri.Fragment);
        }

        public static int MAKELANGID(int primary, int sub)
        {
            return ((((ushort) sub) << 10) | ((ushort) primary));
        }

        public static int MAKELCID(int lgid)
        {
            return MAKELCID(lgid, 0);
        }

        public static int MAKELCID(int lgid, int sort)
        {
            return ((0xffff & lgid) | ((15 & sort) << 0x10));
        }

        public static bool Succeeded(int hr)
        {
            return (hr >= 0);
        }

        public static int WM_MOUSEENTER
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (wmMouseEnterMessage == -1)
                {
                    wmMouseEnterMessage = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("WinFormsMouseEnter");
                }
                return wmMouseEnterMessage;
            }
        }

        public static int WM_UIUNSUBCLASS
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (wmUnSubclass == -1)
                {
                    wmUnSubclass = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("WinFormsUnSubclass");
                }
                return wmUnSubclass;
            }
        }

        [StructLayout(LayoutKind.Sequential), CLSCompliant(false)]
        public sealed class _POINTL
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ACCEL
        {
            public byte fVirt;
            public short key;
            public short cmd;
        }

        public class ActiveX
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
            public const int DISPID_AMBIENT_RIGHTTOLEFT = -732;
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

            private ActiveX()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public short bmPlanes;
            public short bmBitsPixel;
            public IntPtr bmBits = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAPINFO
        {
            public int bmiHeader_biSize = 40;
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
            public byte bmiColors_rgbBlue;
            public byte bmiColors_rgbGreen;
            public byte bmiColors_rgbRed;
            public byte bmiColors_rgbReserved;
            private BITMAPINFO()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO_FLAT
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
        public class BITMAPINFOHEADER
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
        public sealed class CA_STRUCT
        {
            public int cElems;
            public IntPtr pElems = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public class CHARFORMAT2A
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.CHARFORMAT2A));
            public int dwMask;
            public int dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] szFaceName = new byte[0x20];
            public short wWeight;
            public short sSpacing;
            public int crBackColor;
            public int lcid;
            public int dwReserved;
            public short sStyle;
            public short wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public class CHARFORMATA
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.CHARFORMATA));
            public int dwMask;
            public int dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public byte[] szFaceName = new byte[0x20];
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public class CHARFORMATW
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.CHARFORMATW));
            public int dwMask;
            public int dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            public byte[] szFaceName = new byte[0x40];
        }

        [StructLayout(LayoutKind.Sequential)]
        public class CHARRANGE
        {
            public int cpMin;
            public int cpMax;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class CHOOSECOLOR
        {
            public int lStructSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.CHOOSECOLOR));
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public int rgbResult;
            public IntPtr lpCustColors;
            public int Flags;
            public IntPtr lCustData = IntPtr.Zero;
            public System.Windows.Forms.NativeMethods.WndProc lpfnHook;
            public string lpTemplateName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto), CLSCompliant(false)]
        public class CHOOSEFONT
        {
            public int lStructSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.CHOOSEFONT));
            public IntPtr hwndOwner;
            public IntPtr hDC;
            public IntPtr lpLogFont;
            public int iPointSize;
            public int Flags;
            public int rgbColors;
            public IntPtr lCustData = IntPtr.Zero;
            public System.Windows.Forms.NativeMethods.WndProc lpfnHook;
            public string lpTemplateName;
            public IntPtr hInstance;
            public string lpszStyle;
            public short nFontType;
            public short ___MISSING_ALIGNMENT__;
            public int nSizeMin;
            public int nSizeMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class CLIENTCREATESTRUCT
        {
            public IntPtr hWindowMenu;
            public int idFirstChild;
            public CLIENTCREATESTRUCT(IntPtr hmenu, int idFirst)
            {
                this.hWindowMenu = hmenu;
                this.idFirstChild = idFirst;
            }
        }

        public sealed class CommonHandles
        {
            public static readonly int Accelerator = HandleCollector.RegisterType("Accelerator", 80, 50);
            public static readonly int CompatibleHDC = HandleCollector.RegisterType("ComptibleHDC", 50, 50);
            public static readonly int Cursor = HandleCollector.RegisterType("Cursor", 20, 500);
            public static readonly int EMF = HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);
            public static readonly int Find = HandleCollector.RegisterType("Find", 0, 0x3e8);
            public static readonly int GDI = HandleCollector.RegisterType("GDI", 50, 500);
            public static readonly int HDC = HandleCollector.RegisterType("HDC", 100, 2);
            public static readonly int Icon = HandleCollector.RegisterType("Icon", 20, 500);
            public static readonly int Kernel = HandleCollector.RegisterType("Kernel", 0, 0x3e8);
            public static readonly int Menu = HandleCollector.RegisterType("Menu", 30, 0x3e8);
            public static readonly int Window = HandleCollector.RegisterType("Window", 5, 0x3e8);
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

            public COMRECT(Rectangle r)
            {
                this.left = r.X;
                this.top = r.Y;
                this.right = r.Right;
                this.bottom = r.Bottom;
            }

            public COMRECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public static System.Windows.Forms.NativeMethods.COMRECT FromXYWH(int x, int y, int width, int height)
            {
                return new System.Windows.Forms.NativeMethods.COMRECT(x, y, x + width, y + height);
            }

            public override string ToString()
            {
                return string.Concat(new object[] { "Left = ", this.left, " Top ", this.top, " Right = ", this.right, " Bottom = ", this.bottom });
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class COPYDATASTRUCT
        {
            public int dwData;
            public int cbData;
            public IntPtr lpData = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DLLVERSIONINFO
        {
            internal uint cbSize;
            internal uint dwMajorVersion;
            internal uint dwMinorVersion;
            internal uint dwBuildNumber;
            internal uint dwPlatformID;
        }

        public enum DOCHOSTUIDBLCLICK
        {
            DEFAULT,
            SHOWPROPERTIES,
            SHOWCODE
        }

        public enum DOCHOSTUIFLAG
        {
            ACTIVATE_CLIENTHIT_ONLY = 0x200,
            DIALOG = 1,
            DISABLE_COOKIE = 0x400,
            DISABLE_HELP_MENU = 2,
            DISABLE_OFFSCREEN = 0x40,
            DISABLE_SCRIPT_INACTIVE = 0x10,
            DIV_BLOCKDEFAULT = 0x100,
            FLAT_SCROLLBAR = 0x80,
            NO3DBORDER = 4,
            NO3DOUTERBORDER = 0x200000,
            NOTHEME = 0x80000,
            OPENNEWWIN = 0x20,
            SCROLL_NO = 8,
            THEME = 0x40000
        }

        [StructLayout(LayoutKind.Sequential), ComVisible(true)]
        public class DOCHOSTUIINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.DOCHOSTUIINFO));
            [MarshalAs(UnmanagedType.I4)]
            public int dwFlags;
            [MarshalAs(UnmanagedType.I4)]
            public int dwDoubleClick;
            [MarshalAs(UnmanagedType.I4)]
            public int dwReserved1;
            [MarshalAs(UnmanagedType.I4)]
            public int dwReserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DRAWITEMSTRUCT
        {
            public int CtlType;
            public int CtlID;
            public int itemID;
            public int itemAction;
            public int itemState;
            public IntPtr hwndItem = IntPtr.Zero;
            public IntPtr hDC = IntPtr.Zero;
            public System.Windows.Forms.NativeMethods.RECT rcItem;
            public IntPtr itemData = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class EDITSTREAM
        {
            public IntPtr dwCookie = IntPtr.Zero;
            public int dwError;
            public System.Windows.Forms.NativeMethods.EditStreamCallback pfnCallback;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class EDITSTREAM64
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            public byte[] contents = new byte[20];
        }

        public delegate int EditStreamCallback(IntPtr dwCookie, IntPtr buf, int cb, out int transferred);

        [StructLayout(LayoutKind.Sequential)]
        public class ENDROPFILES
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public IntPtr hDrop = IntPtr.Zero;
            public int cp;
            public bool fProtected;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ENLINK
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public int msg;
            public IntPtr wParam = IntPtr.Zero;
            public IntPtr lParam = IntPtr.Zero;
            public System.Windows.Forms.NativeMethods.CHARRANGE charrange;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ENLINK64
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x38)]
            public byte[] contents = new byte[0x38];
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class ENPROTECTED
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public int msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public System.Windows.Forms.NativeMethods.CHARRANGE chrg;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ENPROTECTED64
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x38)]
            public byte[] contents = new byte[0x38];
        }

        public delegate bool EnumChildrenCallback(IntPtr hwnd, IntPtr lParam);

        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class EVENTMSG
        {
            public int message;
            public int paramL;
            public int paramH;
            public int time;
            public IntPtr hwnd;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class FINDTEXT
        {
            public System.Windows.Forms.NativeMethods.CHARRANGE chrg;
            public string lpstrText;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class FONTDESC
        {
            public int cbSizeOfStruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.FONTDESC));
            public string lpstrName;
            public long cySize;
            public short sWeight;
            public short sCharset;
            public bool fItalic;
            public bool fUnderline;
            public bool fStrikethrough;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class GETTEXTLENGTHEX
        {
            public uint flags;
            public uint codepage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class HDITEM2
        {
            public int mask;
            public int cxy;
            public IntPtr pszText_notUsed = IntPtr.Zero;
            public IntPtr hbm = IntPtr.Zero;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam = IntPtr.Zero;
            public int iImage;
            public int iOrder;
            public int type;
            public IntPtr pvFilter = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HDLAYOUT
        {
            public IntPtr prc;
            public IntPtr pwpos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class HELPINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.HELPINFO));
            public int iContextType;
            public int iCtrlId;
            public IntPtr hItemHandle = IntPtr.Zero;
            public IntPtr dwContextId = IntPtr.Zero;
            public System.Windows.Forms.NativeMethods.POINT MousePos;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class HH_AKLINK
        {
            internal int cbStruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.HH_AKLINK));
            internal bool fReserved;
            internal string pszKeywords;
            internal string pszUrl;
            internal string pszMsgText;
            internal string pszMsgTitle;
            internal string pszWindow;
            internal bool fIndexOnFail;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class HH_FTS_QUERY
        {
            internal int cbStruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.HH_FTS_QUERY));
            internal bool fUniCodeStrings;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszSearchQuery;
            internal int iProximity = -1;
            internal bool fStemmedSearch;
            internal bool fTitleOnly;
            internal bool fExecute = true;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszWindow;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class HH_POPUP
        {
            internal int cbStruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.HH_POPUP));
            internal IntPtr hinst = IntPtr.Zero;
            internal int idString;
            internal IntPtr pszText;
            internal System.Windows.Forms.NativeMethods.POINT pt;
            internal int clrForeground = -1;
            internal int clrBackground = -1;
            internal System.Windows.Forms.NativeMethods.RECT rcMargins = System.Windows.Forms.NativeMethods.RECT.FromXYWH(-1, -1, -1, -1);
            internal string pszFont;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct HIGHCONTRAST
        {
            public int cbSize;
            public int dwFlags;
            public string lpszDefaultScheme;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct HIGHCONTRAST_I
        {
            public int cbSize;
            public int dwFlags;
            public IntPtr lpszDefaultScheme;
        }

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [ComImport, Guid("4D07FC10-F931-11CE-B001-00AA006884E5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ICategorizeProperties
        {
            [PreserveSig]
            int MapPropertyToCategory(int dispID, ref int categoryID);
            [PreserveSig]
            int GetCategoryName(int propcat, [In, MarshalAs(UnmanagedType.U4)] int lcid, out string categoryName);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ICONINFO
        {
            public int fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask = IntPtr.Zero;
            public IntPtr hbmColor = IntPtr.Zero;
        }

        [ComVisible(true), Guid("626FC520-A41E-11CF-A731-00A0C9082637"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLDocument
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetScript();
        }

        [StructLayout(LayoutKind.Sequential)]
        public class IMAGEINFO
        {
            public IntPtr hbmImage = IntPtr.Zero;
            public IntPtr hbmMask = IntPtr.Zero;
            public int Unused1;
            public int Unused2;
            public int rcImage_left;
            public int rcImage_top;
            public int rcImage_right;
            public int rcImage_bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class IMAGELISTDRAWPARAMS
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.IMAGELISTDRAWPARAMS));
            public IntPtr himl = IntPtr.Zero;
            public int i;
            public IntPtr hdcDst = IntPtr.Zero;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;
            public int yBitmap;
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [ComImport, Guid("7494683C-37A0-11d2-A273-00C04F8EF4FF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IManagedPerPropertyBrowsing
        {
            [PreserveSig]
            int GetPropertyAttributes(int dispid, ref int pcAttributes, ref IntPtr pbstrAttrNames, ref IntPtr pvariantInitValues);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class INITCOMMONCONTROLSEX
        {
            public int dwSize = 8;
            public int dwICC;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public System.Windows.Forms.NativeMethods.INPUTUNION inputUnion;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            [FieldOffset(0)]
            public System.Windows.Forms.NativeMethods.HARDWAREINPUT hi;
            [FieldOffset(0)]
            public System.Windows.Forms.NativeMethods.KEYBDINPUT ki;
            [FieldOffset(0)]
            public System.Windows.Forms.NativeMethods.MOUSEINPUT mi;
        }

        [ComImport, ComVisible(true), Guid("B722BCCB-4E68-101B-A2BC-00AA00404770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), CLSCompliant(false)]
        public interface IOleCommandTarget
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryStatus(ref Guid pguidCmdGroup, int cCmds, [In, Out] System.Windows.Forms.NativeMethods.OLECMD prgCmds, [In, Out] IntPtr pCmdText);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Exec(ref Guid pguidCmdGroup, int nCmdID, int nCmdexecopt, [In, MarshalAs(UnmanagedType.LPArray)] object[] pvaIn, int pvaOut);
        }

        [ComImport, Guid("376BD3AA-3845-101B-84ED-08002B2EC713"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPerPropertyBrowsing
        {
            [PreserveSig]
            int GetDisplayString(int dispID, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstr);
            [PreserveSig]
            int MapPropertyToPage(int dispID, out Guid pGuid);
            [PreserveSig]
            int GetPredefinedStrings(int dispID, [Out] System.Windows.Forms.NativeMethods.CA_STRUCT pCaStringsOut, [Out] System.Windows.Forms.NativeMethods.CA_STRUCT pCaCookiesOut);
            [PreserveSig]
            int GetPredefinedValue(int dispID, [In, MarshalAs(UnmanagedType.U4)] int dwCookie, [Out] System.Windows.Forms.NativeMethods.VARIANT pVarOut);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B196B283-BAB4-101A-B69C-00AA00341D07")]
        public interface IProvideClassInfo
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.ITypeInfo GetClassInfo();
        }

        [ComImport, Guid("A7ABA9C1-8983-11cf-8F20-00805F2CD064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IProvideMultipleClassInfo
        {
            [PreserveSig]
            System.Windows.Forms.UnsafeNativeMethods.ITypeInfo GetClassInfo();
            [PreserveSig]
            int GetGUID(int dwGuidKind, [In, Out] ref Guid pGuid);
            [PreserveSig]
            int GetMultiTypeInfoCount([In, Out] ref int pcti);
            [PreserveSig]
            int GetInfoOfIndex(int iti, int dwFlags, [In, Out] ref System.Windows.Forms.UnsafeNativeMethods.ITypeInfo pTypeInfo, int pTIFlags, int pcdispidReserved, IntPtr piidPrimary, IntPtr piidSource);
        }

        [ComImport, Guid("33C0C1D8-33CF-11d3-BFF2-00C04F990235"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IProvidePropertyBuilder
        {
            [PreserveSig]
            int MapPropertyToBuilder(int dispid, [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] pdwCtlBldType, [In, Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrGuidBldr, [In, Out, MarshalAs(UnmanagedType.Bool)] ref bool builderAvailable);
            [PreserveSig]
            int ExecuteBuilder(int dispid, [In, MarshalAs(UnmanagedType.BStr)] string bstrGuidBldr, [In, MarshalAs(UnmanagedType.Interface)] object pdispApp, HandleRef hwndBldrOwner, [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pvarValue, [In, Out, MarshalAs(UnmanagedType.Bool)] ref bool actionCommitted);
        }

        [ComImport, Guid("B196B28B-BAB4-101A-B69C-00AA00341D07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ISpecifyPropertyPages
        {
            void GetPages([Out] System.Windows.Forms.NativeMethods.tagCAUUID pPages);
        }

        [ComImport, Guid("0FF510A3-5FA5-49F1-8CCC-190D71083F3E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IVsPerPropertyBrowsing
        {
            [PreserveSig]
            int HideProperty(int dispid, ref bool pfHide);
            [PreserveSig]
            int DisplayChildProperties(int dispid, ref bool pfDisplay);
            [PreserveSig]
            int GetLocalizedPropertyInfo(int dispid, int localeID, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrLocalizedName, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrLocalizeDescription);
            [PreserveSig]
            int HasDefaultValue(int dispid, ref bool fDefault);
            [PreserveSig]
            int IsPropertyReadOnly(int dispid, ref bool fReadOnly);
            [PreserveSig]
            int GetClassName([In, Out] ref string pbstrClassName);
            [PreserveSig]
            int CanResetPropertyValue(int dispid, [In, Out] ref bool pfCanReset);
            [PreserveSig]
            int ResetPropertyValue(int dispid);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        public delegate int ListViewCompareCallback(IntPtr lParam1, IntPtr lParam2, IntPtr lParamSort);

        [StructLayout(LayoutKind.Sequential)]
        public class LOGBRUSH
        {
            public int lbStyle;
            public int lbColor;
            public IntPtr lbHatch;
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

            public LOGFONT(System.Windows.Forms.NativeMethods.LOGFONT lf)
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
        public class LOGPEN
        {
            public int lopnStyle;
            public int lopnWidth_x;
            public int lopnWidth_y;
            public int lopnColor;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVBKIMAGE
        {
            public int ulFlags;
            public IntPtr hBmp = IntPtr.Zero;
            public string pszImage;
            public int cchImageMax;
            public int xOffset;
            public int yOffset;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVCOLUMN
        {
            public int mask;
            public int fmt;
            public int cx;
            public IntPtr pszText;
            public int cchTextMax;
            public int iSubItem;
            public int iImage;
            public int iOrder;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVCOLUMN_T
        {
            public int mask;
            public int fmt;
            public int cx;
            public string pszText;
            public int cchTextMax;
            public int iSubItem;
            public int iImage;
            public int iOrder;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVFINDINFO
        {
            public int flags;
            public string psz;
            public IntPtr lParam;
            public int ptX;
            public int ptY;
            public int vkDirection;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class LVGROUP
        {
            public uint cbSize = ((uint) Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.LVGROUP)));
            public uint mask;
            public IntPtr pszHeader;
            public int cchHeader;
            public IntPtr pszFooter = IntPtr.Zero;
            public int cchFooter;
            public int iGroupId;
            public uint stateMask;
            public uint state;
            public uint uAlign;
            public override string ToString()
            {
                return ("LVGROUP: header = " + this.pszHeader.ToString() + ", iGroupId = " + this.iGroupId.ToString(CultureInfo.InvariantCulture));
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVHITTESTINFO
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public int iItem;
            public int iSubItem;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVINSERTMARK
        {
            public uint cbSize = ((uint) Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.LVINSERTMARK)));
            public int dwFlags;
            public int iItem;
            public int dwReserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
            public void Reset()
            {
                this.pszText = null;
                this.mask = 0;
                this.iItem = 0;
                this.iSubItem = 0;
                this.stateMask = 0;
                this.state = 0;
                this.cchTextMax = 0;
                this.iImage = 0;
                this.lParam = IntPtr.Zero;
                this.iIndent = 0;
                this.iGroupId = 0;
                this.cColumns = 0;
                this.puColumns = IntPtr.Zero;
            }

            public override string ToString()
            {
                return ("LVITEM: pszText = " + this.pszText + ", iItem = " + this.iItem.ToString(CultureInfo.InvariantCulture) + ", iSubItem = " + this.iSubItem.ToString(CultureInfo.InvariantCulture) + ", state = " + this.state.ToString(CultureInfo.InvariantCulture) + ", iGroupId = " + this.iGroupId.ToString(CultureInfo.InvariantCulture) + ", cColumns = " + this.cColumns.ToString(CultureInfo.InvariantCulture));
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVITEM_NOTEXT
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVTILEVIEWINFO
        {
            public uint cbSize = ((uint) Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.LVTILEVIEWINFO)));
            public int dwMask;
            public int dwFlags;
            public System.Windows.Forms.NativeMethods.SIZE sizeTile;
            public int cLines;
            public System.Windows.Forms.NativeMethods.RECT rcLabelMargin;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class MCHITTESTINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MCHITTESTINFO));
            public int pt_x;
            public int pt_y;
            public int uHit;
            public short st_wYear;
            public short st_wMonth;
            public short st_wDayOfWeek;
            public short st_wDay;
            public short st_wHour;
            public short st_wMinute;
            public short st_wSecond;
            public short st_wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MEASUREITEMSTRUCT
        {
            public int CtlType;
            public int CtlID;
            public int itemID;
            public int itemWidth;
            public int itemHeight;
            public IntPtr itemData = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class MENUITEMINFO_T
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MENUITEMINFO_T));
            public int fMask;
            public int fType;
            public int fState;
            public int wID;
            public IntPtr hSubMenu;
            public IntPtr hbmpChecked;
            public IntPtr hbmpUnchecked;
            public IntPtr dwItemData;
            public string dwTypeData;
            public int cch;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class MENUITEMINFO_T_RW
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW));
            public int fMask;
            public int fType;
            public int fState;
            public int wID;
            public IntPtr hSubMenu = IntPtr.Zero;
            public IntPtr hbmpChecked = IntPtr.Zero;
            public IntPtr hbmpUnchecked = IntPtr.Zero;
            public IntPtr dwItemData = IntPtr.Zero;
            public IntPtr dwTypeData = IntPtr.Zero;
            public int cch;
            public IntPtr hbmpItem = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MINMAXINFO
        {
            public System.Windows.Forms.NativeMethods.POINT ptReserved;
            public System.Windows.Forms.NativeMethods.POINT ptMaxSize;
            public System.Windows.Forms.NativeMethods.POINT ptMaxPosition;
            public System.Windows.Forms.NativeMethods.POINT ptMinTrackSize;
            public System.Windows.Forms.NativeMethods.POINT ptMaxTrackSize;
        }

        public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=4)]
        public class MONITORINFO
        {
            internal int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MONITORINFO));
            internal System.Windows.Forms.NativeMethods.RECT rcMonitor = new System.Windows.Forms.NativeMethods.RECT();
            internal System.Windows.Forms.NativeMethods.RECT rcWork = new System.Windows.Forms.NativeMethods.RECT();
            internal int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=4)]
        public class MONITORINFOEX
        {
            internal int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MONITORINFOEX));
            internal System.Windows.Forms.NativeMethods.RECT rcMonitor = new System.Windows.Forms.NativeMethods.RECT();
            internal System.Windows.Forms.NativeMethods.RECT rcWork = new System.Windows.Forms.NativeMethods.RECT();
            internal int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            internal char[] szDevice = new char[0x20];
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MOUSEHOOKSTRUCT
        {
            public int pt_x;
            public int pt_y;
            public IntPtr hWnd = IntPtr.Zero;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct MSAAMENUINFO
        {
            public int dwMSAASignature;
            public int cchWText;
            public string pszWText;
            public MSAAMENUINFO(string text)
            {
                this.dwMSAASignature = -1441927155;
                this.cchWText = text.Length;
                this.pszWText = text;
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
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

        public class MSOCM
        {
            public const int msocadvfModal = 1;
            public const int msocadvfRecording = 8;
            public const int msocadvfRedrawOff = 2;
            public const int msocadvfWarningsOff = 4;
            public const int msoccontextAll = 0;
            public const int msoccontextMine = 1;
            public const int msoccontextOthers = 2;
            public const int msochostfExclusiveBorderSpace = 1;
            public const int msocrfExclusiveActivation = 0x80;
            public const int msocrfExclusiveBorderSpace = 0x40;
            public const int msocrfMaster = 0x200;
            public const int msocrfNeedAllActiveNotifs = 0x20;
            public const int msocrfNeedAllMacEvents = 0x100;
            public const int msocrfNeedIdleTime = 1;
            public const int msocrfNeedPeriodicIdleTime = 2;
            public const int msocrfNeedSpecActiveNotifs = 0x10;
            public const int msocrfPreTranslateAll = 8;
            public const int msocrfPreTranslateKeys = 4;
            public const int msocstateModal = 1;
            public const int msocstateRecording = 4;
            public const int msocstateRedrawOff = 2;
            public const int msocstateWarningsOff = 3;
            public const int msocWindowComponent = 2;
            public const int msocWindowDlgOwner = 3;
            public const int msocWindowFrameOwner = 1;
            public const int msocWindowFrameToplevel = 0;
            public const int msogacActive = 0;
            public const int msogacTracking = 1;
            public const int msogacTrackingOrActive = 2;
            public const int msoidlefAll = -1;
            public const int msoidlefNonPeriodic = 2;
            public const int msoidlefPeriodic = 1;
            public const int msoidlefPriority = 4;
            public const int msoloopDebug = 3;
            public const int msoloopDoEvents = 2;
            public const int msoloopDoEventsModal = -2;
            public const int msoloopFocusWait = 1;
            public const int msoloopMain = -1;
            public const int msoloopModalAlert = 5;
            public const int msoloopModalForm = 4;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public class MSOCRINFOSTRUCT
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT));
            public int uIdleTimeInterval;
            public int grfcrf;
            public int grfcadvf;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMCUSTOMDRAW
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmcd;
            public int dwDrawStage;
            public IntPtr hdc;
            public System.Windows.Forms.NativeMethods.RECT rc;
            public IntPtr dwItemSpec;
            public int uItemState;
            public IntPtr lItemlParam;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMDATETIMECHANGE
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public int dwFlags;
            public System.Windows.Forms.NativeMethods.SYSTEMTIME st;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMDAYSTATE
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public System.Windows.Forms.NativeMethods.SYSTEMTIME stStart;
            public int cDayState;
            public IntPtr prgDayState;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR
        {
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMHEADER
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public int iItem;
            public int iButton;
            public IntPtr pItem = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLISTVIEW
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public int iItem;
            public int iSubItem;
            public int uNewState;
            public int uOldState;
            public int uChanged;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMLVCACHEHINT
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public int iFrom;
            public int iTo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLVCUSTOMDRAW
        {
            public System.Windows.Forms.NativeMethods.NMCUSTOMDRAW nmcd;
            public int clrText;
            public int clrTextBk;
            public int iSubItem;
            public int dwItemType;
            public int clrFace;
            public int iIconEffect;
            public int iIconPhase;
            public int iPartId;
            public int iStateId;
            public System.Windows.Forms.NativeMethods.RECT rcText;
            public uint uAlign;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMLVDISPINFO
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public System.Windows.Forms.NativeMethods.LVITEM item;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMLVDISPINFO_NOTEXT
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public System.Windows.Forms.NativeMethods.LVITEM_NOTEXT item;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLVFINDITEM
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public int iStart;
            public System.Windows.Forms.NativeMethods.LVFINDINFO lvfi;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMLVGETINFOTIP
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public int flags;
            public IntPtr lpszText = IntPtr.Zero;
            public int cchTextMax;
            public int item;
            public int subItem;
            public IntPtr lParam = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMLVKEYDOWN
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public short wVKey;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMLVODSTATECHANGE
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public int iFrom;
            public int iTo;
            public int uNewState;
            public int uOldState;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMSELCHANGE
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public System.Windows.Forms.NativeMethods.SYSTEMTIME stSelStart;
            public System.Windows.Forms.NativeMethods.SYSTEMTIME stSelEnd;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMTBHOTITEM
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public int idOld;
            public int idNew;
            public int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMTOOLBAR
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public int iItem;
            public System.Windows.Forms.NativeMethods.TBBUTTON tbButton;
            public int cchText;
            public IntPtr pszText;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMTREEVIEW
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public int action;
            public System.Windows.Forms.NativeMethods.TV_ITEM itemOld;
            public System.Windows.Forms.NativeMethods.TV_ITEM itemNew;
            public int ptDrag_X;
            public int ptDrag_Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMTVCUSTOMDRAW
        {
            public System.Windows.Forms.NativeMethods.NMCUSTOMDRAW nmcd;
            public int clrText;
            public int clrTextBk;
            public int iLevel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMTVDISPINFO
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public System.Windows.Forms.NativeMethods.TV_ITEM item;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMTVGETINFOTIP
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public string pszText;
            public int cchTextMax;
            public IntPtr item;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NONCLIENTMETRICS
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.NONCLIENTMETRICS));
            public int iBorderWidth;
            public int iScrollWidth;
            public int iScrollHeight;
            public int iCaptionWidth;
            public int iCaptionHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Windows.Forms.NativeMethods.LOGFONT lfCaptionFont;
            public int iSmCaptionWidth;
            public int iSmCaptionHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Windows.Forms.NativeMethods.LOGFONT lfSmCaptionFont;
            public int iMenuWidth;
            public int iMenuHeight;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Windows.Forms.NativeMethods.LOGFONT lfMenuFont;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Windows.Forms.NativeMethods.LOGFONT lfStatusFont;
            [MarshalAs(UnmanagedType.Struct)]
            public System.Windows.Forms.NativeMethods.LOGFONT lfMessageFont;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NOTIFYICONDATA
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.NOTIFYICONDATA));
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x100)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x40)]
            public string szInfoTitle;
            public int dwInfoFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class OCPFIPARAMS
        {
            public int cbSizeOfStruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.OCPFIPARAMS));
            public IntPtr hwndOwner;
            public int x;
            public int y;
            public string lpszCaption;
            public int cObjects = 1;
            public IntPtr ppUnk;
            public int pageCount = 1;
            public IntPtr uuid;
            public int lcid = Application.CurrentCulture.LCID;
            public int dispidInitial;
        }

        public class Ole
        {
            public const int PICTYPE_BITMAP = 1;
            public const int PICTYPE_ENHMETAFILE = 4;
            public const int PICTYPE_ICON = 3;
            public const int PICTYPE_METAFILE = 2;
            public const int PICTYPE_NONE = 0;
            public const int PICTYPE_UNINITIALIZED = -1;
            public const int STATFLAG_DEFAULT = 0;
            public const int STATFLAG_NONAME = 1;
        }

        [StructLayout(LayoutKind.Sequential), CLSCompliant(false)]
        public class OLECMD
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cmdID;
            [MarshalAs(UnmanagedType.U4)]
            public int cmdf;
        }

        public enum OLECMDEXECOPT
        {
            OLECMDEXECOPT_DODEFAULT,
            OLECMDEXECOPT_PROMPTUSER,
            OLECMDEXECOPT_DONTPROMPTUSER,
            OLECMDEXECOPT_SHOWHELP
        }

        public enum OLECMDF
        {
            OLECMDF_DEFHIDEONCTXTMENU = 0x20,
            OLECMDF_ENABLED = 2,
            OLECMDF_INVISIBLE = 0x10,
            OLECMDF_LATCHED = 4,
            OLECMDF_NINCHED = 8,
            OLECMDF_SUPPORTED = 1
        }

        public enum OLECMDID
        {
            OLECMDID_PAGESETUP = 8,
            OLECMDID_PRINT = 6,
            OLECMDID_PRINTPREVIEW = 7,
            OLECMDID_PROPERTIES = 10,
            OLECMDID_SAVEAS = 4
        }

        public enum OLERENDER
        {
            OLERENDER_NONE,
            OLERENDER_DRAW,
            OLERENDER_FORMAT,
            OLERENDER_ASIS
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class OPENFILENAME_I
        {
            public int lStructSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.OPENFILENAME_I));
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public IntPtr lpstrCustomFilter = IntPtr.Zero;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public IntPtr lpstrFile;
            public int nMaxFile = 260;
            public IntPtr lpstrFileTitle = IntPtr.Zero;
            public int nMaxFileTitle = 260;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData = IntPtr.Zero;
            public System.Windows.Forms.NativeMethods.WndProc lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved = IntPtr.Zero;
            public int dwReserved;
            public int FlagsEx;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PAGESETUPDLG
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public int Flags;
            public int paperSizeX;
            public int paperSizeY;
            public int minMarginLeft;
            public int minMarginTop;
            public int minMarginRight;
            public int minMarginBottom;
            public int marginLeft;
            public int marginTop;
            public int marginRight;
            public int marginBottom;
            public IntPtr hInstance = IntPtr.Zero;
            public IntPtr lCustData = IntPtr.Zero;
            public System.Windows.Forms.NativeMethods.WndProc lpfnPageSetupHook;
            public System.Windows.Forms.NativeMethods.WndProc lpfnPagePaintHook;
            public string lpPageSetupTemplateName;
            public IntPtr hPageSetupTemplate = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public int rcPaint_left;
            public int rcPaint_top;
            public int rcPaint_right;
            public int rcPaint_bottom;
            public bool fRestore;
            public bool fIncUpdate;
            public int reserved1;
            public int reserved2;
            public int reserved3;
            public int reserved4;
            public int reserved5;
            public int reserved6;
            public int reserved7;
            public int reserved8;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PALETTEENTRY
        {
            public byte peRed;
            public byte peGreen;
            public byte peBlue;
            public byte peFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PARAFORMAT
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.PARAFORMAT));
            public int dwMask;
            public short wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public int[] rgxTabs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESC
        {
            internal int cbSizeOfStruct;
            public int picType;
            internal IntPtr union1;
            internal int union2;
            internal int union3;
            public static System.Windows.Forms.NativeMethods.PICTDESC CreateBitmapPICTDESC(IntPtr hbitmap, IntPtr hpal)
            {
                return new System.Windows.Forms.NativeMethods.PICTDESC { cbSizeOfStruct = 0x10, picType = 1, union1 = hbitmap, union2 = (int) (((long) hpal) & 0xffffffffL), union3 = (int) (((long) hpal) >> 0x20) };
            }

            public static System.Windows.Forms.NativeMethods.PICTDESC CreateIconPICTDESC(IntPtr hicon)
            {
                return new System.Windows.Forms.NativeMethods.PICTDESC { cbSizeOfStruct = 12, picType = 3, union1 = hicon };
            }

            public virtual IntPtr GetHandle()
            {
                return this.union1;
            }

            public virtual IntPtr GetHPal()
            {
                if (this.picType == 1)
                {
                    return (IntPtr) (((ulong) this.union2) | (this.union3 << 0x20));
                }
                return IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESCbmp
        {
            internal int cbSizeOfStruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.PICTDESCbmp));
            internal int picType = 1;
            internal IntPtr hbitmap = IntPtr.Zero;
            internal IntPtr hpalette = IntPtr.Zero;
            internal int unused;
            public PICTDESCbmp(Bitmap bitmap)
            {
                this.hbitmap = bitmap.GetHbitmap();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESCemf
        {
            internal int cbSizeOfStruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.PICTDESCemf));
            internal int picType = 4;
            internal IntPtr hemf = IntPtr.Zero;
            internal int unused1;
            internal int unused2;
            public PICTDESCemf(Metafile metafile)
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESCicon
        {
            internal int cbSizeOfStruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.PICTDESCicon));
            internal int picType = 3;
            internal IntPtr hicon = IntPtr.Zero;
            internal int unused1;
            internal int unused2;
            public PICTDESCicon(Icon icon)
            {
                this.hicon = System.Windows.Forms.SafeNativeMethods.CopyImage(new HandleRef(icon, icon.Handle), 1, icon.Size.Width, icon.Size.Height, 0);
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
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class POINTL
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTSTRUCT
        {
            public int x;
            public int y;
            public POINTSTRUCT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public interface PRINTDLG
        {
            int Flags { get; set; }

            IntPtr hDC { get; set; }

            IntPtr hDevMode { get; set; }

            IntPtr hDevNames { get; set; }

            IntPtr hInstance { get; set; }

            IntPtr hPrintTemplate { get; set; }

            IntPtr hSetupTemplate { get; set; }

            IntPtr hwndOwner { get; set; }

            IntPtr lCustData { get; set; }

            System.Windows.Forms.NativeMethods.WndProc lpfnPrintHook { get; set; }

            System.Windows.Forms.NativeMethods.WndProc lpfnSetupHook { get; set; }

            string lpPrintTemplateName { get; set; }

            string lpSetupTemplateName { get; set; }

            int lStructSize { get; set; }

            short nCopies { get; set; }

            short nFromPage { get; set; }

            short nMaxPage { get; set; }

            short nMinPage { get; set; }

            short nToPage { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        public class PRINTDLG_32 : System.Windows.Forms.NativeMethods.PRINTDLG
        {
            private int m_lStructSize;
            private IntPtr m_hwndOwner;
            private IntPtr m_hDevMode;
            private IntPtr m_hDevNames;
            private IntPtr m_hDC;
            private int m_Flags;
            private short m_nFromPage;
            private short m_nToPage;
            private short m_nMinPage;
            private short m_nMaxPage;
            private short m_nCopies;
            private IntPtr m_hInstance;
            private IntPtr m_lCustData;
            private System.Windows.Forms.NativeMethods.WndProc m_lpfnPrintHook;
            private System.Windows.Forms.NativeMethods.WndProc m_lpfnSetupHook;
            private string m_lpPrintTemplateName;
            private string m_lpSetupTemplateName;
            private IntPtr m_hPrintTemplate;
            private IntPtr m_hSetupTemplate;
            public int lStructSize
            {
                get
                {
                    return this.m_lStructSize;
                }
                set
                {
                    this.m_lStructSize = value;
                }
            }
            public IntPtr hwndOwner
            {
                get
                {
                    return this.m_hwndOwner;
                }
                set
                {
                    this.m_hwndOwner = value;
                }
            }
            public IntPtr hDevMode
            {
                get
                {
                    return this.m_hDevMode;
                }
                set
                {
                    this.m_hDevMode = value;
                }
            }
            public IntPtr hDevNames
            {
                get
                {
                    return this.m_hDevNames;
                }
                set
                {
                    this.m_hDevNames = value;
                }
            }
            public IntPtr hDC
            {
                get
                {
                    return this.m_hDC;
                }
                set
                {
                    this.m_hDC = value;
                }
            }
            public int Flags
            {
                get
                {
                    return this.m_Flags;
                }
                set
                {
                    this.m_Flags = value;
                }
            }
            public short nFromPage
            {
                get
                {
                    return this.m_nFromPage;
                }
                set
                {
                    this.m_nFromPage = value;
                }
            }
            public short nToPage
            {
                get
                {
                    return this.m_nToPage;
                }
                set
                {
                    this.m_nToPage = value;
                }
            }
            public short nMinPage
            {
                get
                {
                    return this.m_nMinPage;
                }
                set
                {
                    this.m_nMinPage = value;
                }
            }
            public short nMaxPage
            {
                get
                {
                    return this.m_nMaxPage;
                }
                set
                {
                    this.m_nMaxPage = value;
                }
            }
            public short nCopies
            {
                get
                {
                    return this.m_nCopies;
                }
                set
                {
                    this.m_nCopies = value;
                }
            }
            public IntPtr hInstance
            {
                get
                {
                    return this.m_hInstance;
                }
                set
                {
                    this.m_hInstance = value;
                }
            }
            public IntPtr lCustData
            {
                get
                {
                    return this.m_lCustData;
                }
                set
                {
                    this.m_lCustData = value;
                }
            }
            public System.Windows.Forms.NativeMethods.WndProc lpfnPrintHook
            {
                get
                {
                    return this.m_lpfnPrintHook;
                }
                set
                {
                    this.m_lpfnPrintHook = value;
                }
            }
            public System.Windows.Forms.NativeMethods.WndProc lpfnSetupHook
            {
                get
                {
                    return this.m_lpfnSetupHook;
                }
                set
                {
                    this.m_lpfnSetupHook = value;
                }
            }
            public string lpPrintTemplateName
            {
                get
                {
                    return this.m_lpPrintTemplateName;
                }
                set
                {
                    this.m_lpPrintTemplateName = value;
                }
            }
            public string lpSetupTemplateName
            {
                get
                {
                    return this.m_lpSetupTemplateName;
                }
                set
                {
                    this.m_lpSetupTemplateName = value;
                }
            }
            public IntPtr hPrintTemplate
            {
                get
                {
                    return this.m_hPrintTemplate;
                }
                set
                {
                    this.m_hPrintTemplate = value;
                }
            }
            public IntPtr hSetupTemplate
            {
                get
                {
                    return this.m_hSetupTemplate;
                }
                set
                {
                    this.m_hSetupTemplate = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class PRINTDLG_64 : System.Windows.Forms.NativeMethods.PRINTDLG
        {
            private int m_lStructSize;
            private IntPtr m_hwndOwner;
            private IntPtr m_hDevMode;
            private IntPtr m_hDevNames;
            private IntPtr m_hDC;
            private int m_Flags;
            private short m_nFromPage;
            private short m_nToPage;
            private short m_nMinPage;
            private short m_nMaxPage;
            private short m_nCopies;
            private IntPtr m_hInstance;
            private IntPtr m_lCustData;
            private System.Windows.Forms.NativeMethods.WndProc m_lpfnPrintHook;
            private System.Windows.Forms.NativeMethods.WndProc m_lpfnSetupHook;
            private string m_lpPrintTemplateName;
            private string m_lpSetupTemplateName;
            private IntPtr m_hPrintTemplate;
            private IntPtr m_hSetupTemplate;
            public int lStructSize
            {
                get
                {
                    return this.m_lStructSize;
                }
                set
                {
                    this.m_lStructSize = value;
                }
            }
            public IntPtr hwndOwner
            {
                get
                {
                    return this.m_hwndOwner;
                }
                set
                {
                    this.m_hwndOwner = value;
                }
            }
            public IntPtr hDevMode
            {
                get
                {
                    return this.m_hDevMode;
                }
                set
                {
                    this.m_hDevMode = value;
                }
            }
            public IntPtr hDevNames
            {
                get
                {
                    return this.m_hDevNames;
                }
                set
                {
                    this.m_hDevNames = value;
                }
            }
            public IntPtr hDC
            {
                get
                {
                    return this.m_hDC;
                }
                set
                {
                    this.m_hDC = value;
                }
            }
            public int Flags
            {
                get
                {
                    return this.m_Flags;
                }
                set
                {
                    this.m_Flags = value;
                }
            }
            public short nFromPage
            {
                get
                {
                    return this.m_nFromPage;
                }
                set
                {
                    this.m_nFromPage = value;
                }
            }
            public short nToPage
            {
                get
                {
                    return this.m_nToPage;
                }
                set
                {
                    this.m_nToPage = value;
                }
            }
            public short nMinPage
            {
                get
                {
                    return this.m_nMinPage;
                }
                set
                {
                    this.m_nMinPage = value;
                }
            }
            public short nMaxPage
            {
                get
                {
                    return this.m_nMaxPage;
                }
                set
                {
                    this.m_nMaxPage = value;
                }
            }
            public short nCopies
            {
                get
                {
                    return this.m_nCopies;
                }
                set
                {
                    this.m_nCopies = value;
                }
            }
            public IntPtr hInstance
            {
                get
                {
                    return this.m_hInstance;
                }
                set
                {
                    this.m_hInstance = value;
                }
            }
            public IntPtr lCustData
            {
                get
                {
                    return this.m_lCustData;
                }
                set
                {
                    this.m_lCustData = value;
                }
            }
            public System.Windows.Forms.NativeMethods.WndProc lpfnPrintHook
            {
                get
                {
                    return this.m_lpfnPrintHook;
                }
                set
                {
                    this.m_lpfnPrintHook = value;
                }
            }
            public System.Windows.Forms.NativeMethods.WndProc lpfnSetupHook
            {
                get
                {
                    return this.m_lpfnSetupHook;
                }
                set
                {
                    this.m_lpfnSetupHook = value;
                }
            }
            public string lpPrintTemplateName
            {
                get
                {
                    return this.m_lpPrintTemplateName;
                }
                set
                {
                    this.m_lpPrintTemplateName = value;
                }
            }
            public string lpSetupTemplateName
            {
                get
                {
                    return this.m_lpSetupTemplateName;
                }
                set
                {
                    this.m_lpSetupTemplateName = value;
                }
            }
            public IntPtr hPrintTemplate
            {
                get
                {
                    return this.m_hPrintTemplate;
                }
                set
                {
                    this.m_hPrintTemplate = value;
                }
            }
            public IntPtr hSetupTemplate
            {
                get
                {
                    return this.m_hSetupTemplate;
                }
                set
                {
                    this.m_hSetupTemplate = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class PRINTDLGEX
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;
            public int Flags;
            public int Flags2;
            public int ExclusionFlags;
            public int nPageRanges;
            public int nMaxPageRanges;
            public IntPtr pageRanges;
            public int nMinPage;
            public int nMaxPage;
            public int nCopies;
            public IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpPrintTemplateName;
            public System.Windows.Forms.NativeMethods.WndProc lpCallback;
            public int nPropertyPages;
            public IntPtr lphPropertyPages;
            public int nStartPage;
            public int dwResultAction;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        public class PRINTPAGERANGE
        {
            public int nFromPage;
            public int nToPage;
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

            public static System.Windows.Forms.NativeMethods.RECT FromXYWH(int x, int y, int width, int height)
            {
                return new System.Windows.Forms.NativeMethods.RECT(x, y, x + width, y + height);
            }

            public System.Drawing.Size Size
            {
                get
                {
                    return new System.Drawing.Size(this.right - this.left, this.bottom - this.top);
                }
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
        public class REPASTESPECIAL
        {
            public int dwAspect;
            public int dwParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class REQRESIZE
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public System.Windows.Forms.NativeMethods.RECT rc;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGNDATAHEADER
        {
            public int cbSizeOfStruct;
            public int iType;
            public int nCount;
            public int nRgnSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SCROLLINFO
        {
            public int cbSize;
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;
            public SCROLLINFO()
            {
                this.cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.SCROLLINFO));
            }

            public SCROLLINFO(int mask, int min, int max, int page, int pos)
            {
                this.cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.SCROLLINFO));
                this.fMask = mask;
                this.nMin = min;
                this.nMax = max;
                this.nPage = page;
                this.nPos = pos;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public class SELCHANGE
        {
            public System.Windows.Forms.NativeMethods.NMHDR nmhdr;
            public System.Windows.Forms.NativeMethods.CHARRANGE chrg;
            public int seltyp;
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
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class STARTUPINFO_I
        {
            public int cb;
            public IntPtr lpReserved = IntPtr.Zero;
            public IntPtr lpDesktop = IntPtr.Zero;
            public IntPtr lpTitle = IntPtr.Zero;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2 = IntPtr.Zero;
            public IntPtr hStdInput = IntPtr.Zero;
            public IntPtr hStdOutput = IntPtr.Zero;
            public IntPtr hStdError = IntPtr.Zero;
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
        public struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte Reserved1;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
            public override string ToString()
            {
                return ("[SYSTEMTIME: " + this.wDay.ToString(CultureInfo.InvariantCulture) + "/" + this.wMonth.ToString(CultureInfo.InvariantCulture) + "/" + this.wYear.ToString(CultureInfo.InvariantCulture) + " " + this.wHour.ToString(CultureInfo.InvariantCulture) + ":" + this.wMinute.ToString(CultureInfo.InvariantCulture) + ":" + this.wSecond.ToString(CultureInfo.InvariantCulture) + "]");
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class SYSTEMTIMEARRAY
        {
            public short wYear1;
            public short wMonth1;
            public short wDayOfWeek1;
            public short wDay1;
            public short wHour1;
            public short wMinute1;
            public short wSecond1;
            public short wMilliseconds1;
            public short wYear2;
            public short wMonth2;
            public short wDayOfWeek2;
            public short wDay2;
            public short wHour2;
            public short wMinute2;
            public short wSecond2;
            public short wMilliseconds2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagCAUUID
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cElems;
            public IntPtr pElems = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagCONTROLINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.tagCONTROLINFO));
            public IntPtr hAccel;
            [MarshalAs(UnmanagedType.U2)]
            public short cAccel;
            [MarshalAs(UnmanagedType.U4)]
            public int dwFlags;
        }

        public enum tagDESCKIND
        {
            DESCKIND_NONE,
            DESCKIND_FUNCDESC,
            DESCKIND_VARDESC,
            DESCKIND_TYPECOMP,
            DESCKIND_IMPLICITAPPOBJ,
            DESCKIND_MAX
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagDISPPARAMS
        {
            public IntPtr rgvarg;
            public IntPtr rgdispidNamedArgs;
            [MarshalAs(UnmanagedType.U4)]
            public int cArgs;
            [MarshalAs(UnmanagedType.U4)]
            public int cNamedArgs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagDVTARGETDEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int tdSize;
            [MarshalAs(UnmanagedType.U2)]
            public short tdDriverNameOffset;
            [MarshalAs(UnmanagedType.U2)]
            public short tdDeviceNameOffset;
            [MarshalAs(UnmanagedType.U2)]
            public short tdPortNameOffset;
            [MarshalAs(UnmanagedType.U2)]
            public short tdExtDevmodeOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagELEMDESC
        {
            public System.Windows.Forms.NativeMethods.tagTYPEDESC tdesc;
            public System.Windows.Forms.NativeMethods.tagPARAMDESC paramdesc;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class tagEXCEPINFO
        {
            [MarshalAs(UnmanagedType.U2)]
            public short wCode;
            [MarshalAs(UnmanagedType.U2)]
            public short wReserved;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrSource;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrDescription;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrHelpFile;
            [MarshalAs(UnmanagedType.U4)]
            public int dwHelpContext;
            public IntPtr pvReserved = IntPtr.Zero;
            public IntPtr pfnDeferredFillIn = IntPtr.Zero;
            [MarshalAs(UnmanagedType.U4)]
            public int scode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagFONTDESC
        {
            public int cbSizeofstruct = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.tagFONTDESC));
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpstrName;
            [MarshalAs(UnmanagedType.U8)]
            public long cySize;
            [MarshalAs(UnmanagedType.U2)]
            public short sWeight;
            [MarshalAs(UnmanagedType.U2)]
            public short sCharset;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fItalic;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fUnderline;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fStrikethrough;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagFUNCDESC
        {
            public int memid;
            public IntPtr lprgscode = IntPtr.Zero;
            public IntPtr lprgelemdescParam = IntPtr.Zero;
            public int funckind;
            public int invkind;
            public int callconv;
            [MarshalAs(UnmanagedType.I2)]
            public short cParams;
            [MarshalAs(UnmanagedType.I2)]
            public short cParamsOpt;
            [MarshalAs(UnmanagedType.I2)]
            public short oVft;
            [MarshalAs(UnmanagedType.I2)]
            public short cScodesi;
            public System.Windows.Forms.NativeMethods.value_tagELEMDESC elemdescFunc;
            [MarshalAs(UnmanagedType.U2)]
            public short wFuncFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tagIDLDESC
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwReserved;
            [MarshalAs(UnmanagedType.U2)]
            public short wIDLFlags;
        }

        public enum tagINVOKEKIND
        {
            INVOKE_FUNC = 1,
            INVOKE_PROPERTYGET = 2,
            INVOKE_PROPERTYPUT = 4,
            INVOKE_PROPERTYPUTREF = 8
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagLICINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbLicInfo = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.tagLICINFO));
            public int fRuntimeAvailable;
            public int fLicVerified;
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
            public bool fMDIApp;
            public IntPtr hwndFrame;
            public IntPtr hAccel;
            [MarshalAs(UnmanagedType.U4)]
            public int cAccelEntries;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagOleMenuGroupWidths
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
            public int[] widths = new int[6];
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagOLEVERB
        {
            public int lVerb;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszVerbName;
            [MarshalAs(UnmanagedType.U4)]
            public int fuFlags;
            [MarshalAs(UnmanagedType.U4)]
            public int grfAttribs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tagPARAMDESC
        {
            public IntPtr pparamdescex;
            [MarshalAs(UnmanagedType.U2)]
            public short wParamFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagPOINTF
        {
            [MarshalAs(UnmanagedType.R4)]
            public float x;
            [MarshalAs(UnmanagedType.R4)]
            public float y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagSIZE
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagSIZEL
        {
            public int cx;
            public int cy;
        }

        public enum tagSYSKIND
        {
            SYS_MAC = 2,
            SYS_WIN16 = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagTYPEATTR
        {
            public Guid guid;
            [MarshalAs(UnmanagedType.U4)]
            public int lcid;
            [MarshalAs(UnmanagedType.U4)]
            public int dwReserved;
            public int memidConstructor;
            public int memidDestructor;
            public IntPtr lpstrSchema = IntPtr.Zero;
            [MarshalAs(UnmanagedType.U4)]
            public int cbSizeInstance;
            public int typekind;
            [MarshalAs(UnmanagedType.U2)]
            public short cFuncs;
            [MarshalAs(UnmanagedType.U2)]
            public short cVars;
            [MarshalAs(UnmanagedType.U2)]
            public short cImplTypes;
            [MarshalAs(UnmanagedType.U2)]
            public short cbSizeVft;
            [MarshalAs(UnmanagedType.U2)]
            public short cbAlignment;
            [MarshalAs(UnmanagedType.U2)]
            public short wTypeFlags;
            [MarshalAs(UnmanagedType.U2)]
            public short wMajorVerNum;
            [MarshalAs(UnmanagedType.U2)]
            public short wMinorVerNum;
            [MarshalAs(UnmanagedType.U4)]
            public int tdescAlias_unionMember;
            [MarshalAs(UnmanagedType.U2)]
            public short tdescAlias_vt;
            [MarshalAs(UnmanagedType.U4)]
            public int idldescType_dwReserved;
            [MarshalAs(UnmanagedType.U2)]
            public short idldescType_wIDLFlags;
            public System.Windows.Forms.NativeMethods.tagTYPEDESC Get_tdescAlias()
            {
                return new System.Windows.Forms.NativeMethods.tagTYPEDESC { unionMember = (IntPtr) this.tdescAlias_unionMember, vt = this.tdescAlias_vt };
            }

            public System.Windows.Forms.NativeMethods.tagIDLDESC Get_idldescType()
            {
                return new System.Windows.Forms.NativeMethods.tagIDLDESC { dwReserved = this.idldescType_dwReserved, wIDLFlags = this.idldescType_wIDLFlags };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class tagTYPEDESC
        {
            public IntPtr unionMember;
            public short vt;
        }

        public enum tagTYPEKIND
        {
            TKIND_ENUM,
            TKIND_RECORD,
            TKIND_MODULE,
            TKIND_INTERFACE,
            TKIND_DISPATCH,
            TKIND_COCLASS,
            TKIND_ALIAS,
            TKIND_UNION,
            TKIND_MAX
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagVARDESC
        {
            public int memid;
            public IntPtr lpstrSchema = IntPtr.Zero;
            public IntPtr unionMember = IntPtr.Zero;
            public System.Windows.Forms.NativeMethods.value_tagELEMDESC elemdescVar;
            [MarshalAs(UnmanagedType.U2)]
            public short wVarFlags;
            public int varkind;
        }

        public enum tagVARFLAGS
        {
            VARFLAG_FBINDABLE = 4,
            VARFLAG_FDEFAULTBIND = 0x20,
            VARFLAG_FDEFAULTCOLLELEM = 0x100,
            VARFLAG_FDISPLAYBIND = 0x10,
            VARFLAG_FHIDDEN = 0x40,
            VARFLAG_FIMMEDIATEBIND = 0x1000,
            VARFLAG_FNONBROWSABLE = 0x400,
            VARFLAG_FREADONLY = 1,
            VARFLAG_FREPLACEABLE = 0x800,
            VARFLAG_FREQUESTEDIT = 8,
            VARFLAG_FSOURCE = 2,
            VARFLAG_FUIDEFAULT = 0x200
        }

        public enum tagVARKIND
        {
            VAR_PERINSTANCE,
            VAR_STATIC,
            VAR_CONST,
            VAR_DISPATCH
        }

        public enum tagVT
        {
            VT_ARRAY = 0x2000,
            VT_BLOB = 0x41,
            VT_BLOB_OBJECT = 70,
            VT_BOOL = 11,
            VT_BSTR = 8,
            VT_BSTR_BLOB = 0xfff,
            VT_BYREF = 0x4000,
            VT_CARRAY = 0x1c,
            VT_CF = 0x47,
            VT_CLSID = 0x48,
            VT_CY = 6,
            VT_DATE = 7,
            VT_DECIMAL = 14,
            VT_DISPATCH = 9,
            VT_EMPTY = 0,
            VT_ERROR = 10,
            VT_FILETIME = 0x40,
            VT_HRESULT = 0x19,
            VT_I1 = 0x10,
            VT_I2 = 2,
            VT_I4 = 3,
            VT_I8 = 20,
            VT_ILLEGAL = 0xffff,
            VT_ILLEGALMASKED = 0xfff,
            VT_INT = 0x16,
            VT_LPSTR = 30,
            VT_LPWSTR = 0x1f,
            VT_NULL = 1,
            VT_PTR = 0x1a,
            VT_R4 = 4,
            VT_R8 = 5,
            VT_RECORD = 0x24,
            VT_RESERVED = 0x8000,
            VT_SAFEARRAY = 0x1b,
            VT_STORAGE = 0x43,
            VT_STORED_OBJECT = 0x45,
            VT_STREAM = 0x42,
            VT_STREAMED_OBJECT = 0x44,
            VT_TYPEMASK = 0xfff,
            VT_UI1 = 0x11,
            VT_UI2 = 0x12,
            VT_UI4 = 0x13,
            VT_UI8 = 0x15,
            VT_UINT = 0x17,
            VT_UNKNOWN = 13,
            VT_USERDEFINED = 0x1d,
            VT_VARIANT = 12,
            VT_VECTOR = 0x1000,
            VT_VOID = 0x18
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TBBUTTON
        {
            public int iBitmap;
            public int idCommand;
            public byte fsState;
            public byte fsStyle;
            public byte bReserved0;
            public byte bReserved1;
            public IntPtr dwData;
            public IntPtr iString;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct TBBUTTONINFO
        {
            public int cbSize;
            public int dwMask;
            public int idCommand;
            public int iImage;
            public byte fsState;
            public byte fsStyle;
            public short cx;
            public IntPtr lParam;
            public IntPtr pszText;
            public int cchTest;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TCITEM_T
        {
            public int mask;
            public int dwState;
            public int dwStateMask;
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
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

        [StructLayout(LayoutKind.Sequential)]
        public class TEXTRANGE
        {
            public System.Windows.Forms.NativeMethods.CHARRANGE chrg;
            public IntPtr lpstrText;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TOOLINFO_T
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_T));
            public int uFlags;
            public IntPtr hwnd;
            public IntPtr uId;
            public System.Windows.Forms.NativeMethods.RECT rect;
            public IntPtr hinst = IntPtr.Zero;
            public string lpszText;
            public IntPtr lParam = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TOOLINFO_TOOLTIP
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP));
            public int uFlags;
            public IntPtr hwnd;
            public IntPtr uId;
            public System.Windows.Forms.NativeMethods.RECT rect;
            public IntPtr hinst = IntPtr.Zero;
            public IntPtr lpszText;
            public IntPtr lParam = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TOOLTIPTEXT
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public string lpszText;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            public string szText;
            public IntPtr hinst;
            public int uFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TOOLTIPTEXTA
        {
            public System.Windows.Forms.NativeMethods.NMHDR hdr;
            public string lpszText;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            public string szText;
            public IntPtr hinst;
            public int uFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TPMPARAMS
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TPMPARAMS));
            public int rcExclude_left;
            public int rcExclude_top;
            public int rcExclude_right;
            public int rcExclude_bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TRACKMOUSEEVENT
        {
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TRACKMOUSEEVENT));
            public int dwFlags;
            public IntPtr hwndTrack;
            public int dwHoverTime = 100;
        }

        public delegate int TreeViewCompareCallback(IntPtr lParam1, IntPtr lParam2, IntPtr lParamSort);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TV_HITTESTINFO
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public IntPtr hItem = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct TV_INSERTSTRUCT
        {
            public IntPtr hParent;
            public IntPtr hInsertAfter;
            public int item_mask;
            public IntPtr item_hItem;
            public int item_state;
            public int item_stateMask;
            public IntPtr item_pszText;
            public int item_cchTextMax;
            public int item_iImage;
            public int item_iSelectedImage;
            public int item_cChildren;
            public IntPtr item_lParam;
            public int item_iIntegral;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct TV_ITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct TVSORTCB
        {
            public IntPtr hParent;
            public System.Windows.Forms.NativeMethods.TreeViewCompareCallback lpfnCompare;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class USEROBJECTFLAGS
        {
            public int fInherit;
            public int fReserved;
            public int dwFlags;
        }

        public static class Util
        {
            private static int GetEmbeddedNullStringLengthAnsi(string s)
            {
                int index = s.IndexOf('\0');
                if (index > -1)
                {
                    string str = s.Substring(0, index);
                    string str2 = s.Substring(index + 1);
                    return ((GetPInvokeStringLength(str) + GetEmbeddedNullStringLengthAnsi(str2)) + 1);
                }
                return GetPInvokeStringLength(s);
            }

            public static int GetPInvokeStringLength(string s)
            {
                if (s == null)
                {
                    return 0;
                }
                if (Marshal.SystemDefaultCharSize == 2)
                {
                    return s.Length;
                }
                if (s.Length == 0)
                {
                    return 0;
                }
                if (s.IndexOf('\0') > -1)
                {
                    return GetEmbeddedNullStringLengthAnsi(s);
                }
                return lstrlen(s);
            }

            public static int HIWORD(int n)
            {
                return ((n >> 0x10) & 0xffff);
            }

            public static int HIWORD(IntPtr n)
            {
                return HIWORD((int) ((long) n));
            }

            public static int LOWORD(int n)
            {
                return (n & 0xffff);
            }

            public static int LOWORD(IntPtr n)
            {
                return LOWORD((int) ((long) n));
            }

            [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
            private static extern int lstrlen(string s);
            public static int MAKELONG(int low, int high)
            {
                return ((high << 0x10) | (low & 0xffff));
            }

            public static IntPtr MAKELPARAM(int low, int high)
            {
                return (IntPtr) ((high << 0x10) | (low & 0xffff));
            }

            public static int SignedHIWORD(int n)
            {
                return (short) ((n >> 0x10) & 0xffff);
            }

            public static int SignedHIWORD(IntPtr n)
            {
                return SignedHIWORD((int) ((long) n));
            }

            public static int SignedLOWORD(int n)
            {
                return (short) (n & 0xffff);
            }

            public static int SignedLOWORD(IntPtr n)
            {
                return SignedLOWORD((int) ((long) n));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct value_tagELEMDESC
        {
            public System.Windows.Forms.NativeMethods.tagTYPEDESC tdesc;
            public System.Windows.Forms.NativeMethods.tagPARAMDESC paramdesc;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class VARIANT
        {
            [MarshalAs(UnmanagedType.I2)]
            public short vt;
            [MarshalAs(UnmanagedType.I2)]
            public short reserved1;
            [MarshalAs(UnmanagedType.I2)]
            public short reserved2;
            [MarshalAs(UnmanagedType.I2)]
            public short reserved3;
            public IntPtr data1;
            public IntPtr data2;
            public bool Byref
            {
                get
                {
                    return (0 != (this.vt & 0x4000));
                }
            }
            public void Clear()
            {
                if (((this.vt == 13) || (this.vt == 9)) && (this.data1 != IntPtr.Zero))
                {
                    Marshal.Release(this.data1);
                }
                if ((this.vt == 8) && (this.data1 != IntPtr.Zero))
                {
                    SysFreeString(this.data1);
                }
                this.data1 = this.data2 = IntPtr.Zero;
                this.vt = 0;
            }

            ~VARIANT()
            {
                this.Clear();
            }

            public static System.Windows.Forms.NativeMethods.VARIANT FromObject(object var)
            {
                System.Windows.Forms.NativeMethods.VARIANT variant = new System.Windows.Forms.NativeMethods.VARIANT();
                if (var == null)
                {
                    variant.vt = 0;
                    return variant;
                }
                if (!Convert.IsDBNull(var))
                {
                    System.Type type = var.GetType();
                    if (type == typeof(bool))
                    {
                        variant.vt = 11;
                        return variant;
                    }
                    if (type == typeof(byte))
                    {
                        variant.vt = 0x11;
                        variant.data1 = (IntPtr) Convert.ToByte(var, CultureInfo.InvariantCulture);
                        return variant;
                    }
                    if (type == typeof(char))
                    {
                        variant.vt = 0x12;
                        variant.data1 = (IntPtr) Convert.ToChar(var, CultureInfo.InvariantCulture);
                        return variant;
                    }
                    if (type == typeof(string))
                    {
                        variant.vt = 8;
                        variant.data1 = SysAllocString(Convert.ToString(var, CultureInfo.InvariantCulture));
                        return variant;
                    }
                    if (type == typeof(short))
                    {
                        variant.vt = 2;
                        variant.data1 = (IntPtr) Convert.ToInt16(var, CultureInfo.InvariantCulture);
                        return variant;
                    }
                    if (type == typeof(int))
                    {
                        variant.vt = 3;
                        variant.data1 = (IntPtr) Convert.ToInt32(var, CultureInfo.InvariantCulture);
                        return variant;
                    }
                    if (type == typeof(long))
                    {
                        variant.vt = 20;
                        variant.SetLong(Convert.ToInt64(var, CultureInfo.InvariantCulture));
                        return variant;
                    }
                    if (type == typeof(decimal))
                    {
                        variant.vt = 6;
                        decimal d = (decimal) var;
                        variant.SetLong(decimal.ToInt64(d));
                        return variant;
                    }
                    if (type == typeof(decimal))
                    {
                        variant.vt = 14;
                        decimal num2 = Convert.ToDecimal(var, CultureInfo.InvariantCulture);
                        variant.SetLong(decimal.ToInt64(num2));
                        return variant;
                    }
                    if (type == typeof(double))
                    {
                        variant.vt = 5;
                        return variant;
                    }
                    if ((type == typeof(float)) || (type == typeof(float)))
                    {
                        variant.vt = 4;
                        return variant;
                    }
                    if (type == typeof(DateTime))
                    {
                        variant.vt = 7;
                        variant.SetLong(Convert.ToDateTime(var, CultureInfo.InvariantCulture).ToFileTime());
                        return variant;
                    }
                    if (type == typeof(sbyte))
                    {
                        variant.vt = 0x10;
                        variant.data1 = (IntPtr) Convert.ToSByte(var, CultureInfo.InvariantCulture);
                        return variant;
                    }
                    if (type == typeof(ushort))
                    {
                        variant.vt = 0x12;
                        variant.data1 = (IntPtr) Convert.ToUInt16(var, CultureInfo.InvariantCulture);
                        return variant;
                    }
                    if (type == typeof(uint))
                    {
                        variant.vt = 0x13;
                        variant.data1 = (IntPtr) Convert.ToUInt32(var, CultureInfo.InvariantCulture);
                        return variant;
                    }
                    if (type == typeof(ulong))
                    {
                        variant.vt = 0x15;
                        variant.SetLong((long) Convert.ToUInt64(var, CultureInfo.InvariantCulture));
                        return variant;
                    }
                    if (((type != typeof(object)) && (type != typeof(System.Windows.Forms.UnsafeNativeMethods.IDispatch))) && !type.IsCOMObject)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ConnPointUnhandledType", new object[] { type.Name }));
                    }
                    variant.vt = (type == typeof(System.Windows.Forms.UnsafeNativeMethods.IDispatch)) ? ((short) 9) : ((short) 13);
                    variant.data1 = Marshal.GetIUnknownForObject(var);
                }
                return variant;
            }

            [DllImport("oleaut32.dll", CharSet=CharSet.Auto)]
            private static extern IntPtr SysAllocString([In, MarshalAs(UnmanagedType.LPWStr)] string s);
            [DllImport("oleaut32.dll", CharSet=CharSet.Auto)]
            private static extern void SysFreeString(IntPtr pbstr);
            public void SetLong(long lVal)
            {
                this.data1 = (IntPtr) (((ulong) lVal) & 0xffffffffL);
                this.data2 = (IntPtr) (((ulong) (lVal >> 0x20)) & 0xffffffffL);
            }

            public IntPtr ToCoTaskMemPtr()
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(0x10);
                Marshal.WriteInt16(ptr, this.vt);
                Marshal.WriteInt16(ptr, 2, this.reserved1);
                Marshal.WriteInt16(ptr, 4, this.reserved2);
                Marshal.WriteInt16(ptr, 6, this.reserved3);
                Marshal.WriteInt32(ptr, 8, (int) this.data1);
                Marshal.WriteInt32(ptr, 12, (int) this.data2);
                return ptr;
            }

            public object ToObject()
            {
                long num;
                IntPtr refInt = this.data1;
                int num2 = this.vt & 0xfff;
                switch (num2)
                {
                    case 0:
                        return null;

                    case 1:
                        return Convert.DBNull;

                    case 2:
                        if (this.Byref)
                        {
                            refInt = (IntPtr) Marshal.ReadInt16(refInt);
                        }
                        return (short) (0xffff & ((short) ((int) refInt)));

                    case 3:
                    case 0x16:
                        if (this.Byref)
                        {
                            refInt = (IntPtr) Marshal.ReadInt32(refInt);
                        }
                        return (int) refInt;

                    case 0x10:
                        if (this.Byref)
                        {
                            refInt = (IntPtr) Marshal.ReadByte(refInt);
                        }
                        return (sbyte) (0xff & ((sbyte) ((int) refInt)));

                    case 0x11:
                        if (this.Byref)
                        {
                            refInt = (IntPtr) Marshal.ReadByte(refInt);
                        }
                        return (byte) (0xff & ((byte) ((int) refInt)));

                    case 0x12:
                        if (this.Byref)
                        {
                            refInt = (IntPtr) Marshal.ReadInt16(refInt);
                        }
                        return (ushort) (0xffff & ((ushort) ((int) refInt)));

                    case 0x13:
                    case 0x17:
                        if (this.Byref)
                        {
                            refInt = (IntPtr) Marshal.ReadInt32(refInt);
                        }
                        return (uint) ((int) refInt);

                    case 20:
                    case 0x15:
                        if (!this.Byref)
                        {
                            num = (long) ((ulong) ((((int) this.data1) & -1) | ((int) this.data2)));
                            break;
                        }
                        num = Marshal.ReadInt64(refInt);
                        break;

                    default:
                    {
                        if (this.Byref)
                        {
                            refInt = GetRefInt(refInt);
                        }
                        switch (num2)
                        {
                            case 4:
                            case 5:
                                throw new FormatException(System.Windows.Forms.SR.GetString("CannotConvertIntToFloat"));

                            case 6:
                                return new decimal((long) ((ulong) ((((int) this.data1) & -1) | ((int) this.data2))));

                            case 7:
                                throw new FormatException(System.Windows.Forms.SR.GetString("CannotConvertDoubleToDate"));

                            case 8:
                            case 0x1f:
                                return Marshal.PtrToStringUni(refInt);

                            case 9:
                            case 13:
                                return Marshal.GetObjectForIUnknown(refInt);

                            case 11:
                                return (refInt != IntPtr.Zero);

                            case 12:
                            {
                                System.Windows.Forms.NativeMethods.VARIANT variant = (System.Windows.Forms.NativeMethods.VARIANT) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(refInt, typeof(System.Windows.Forms.NativeMethods.VARIANT));
                                return variant.ToObject();
                            }
                            case 14:
                                return new decimal((long) ((ulong) ((((int) this.data1) & -1) | ((int) this.data2))));

                            case 0x19:
                                return refInt;

                            case 0x1d:
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("COM2UnhandledVT", new object[] { "VT_USERDEFINED" }));

                            case 30:
                                return Marshal.PtrToStringAnsi(refInt);

                            case 0x40:
                                num = (long) ((ulong) ((((int) this.data1) & -1) | ((int) this.data2)));
                                return new DateTime(num);

                            case 0x48:
                                return (Guid) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(refInt, typeof(Guid));
                        }
                        int vt = this.vt;
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("COM2UnhandledVT", new object[] { vt.ToString(CultureInfo.InvariantCulture) }));
                    }
                }
                if (this.vt == 20)
                {
                    return num;
                }
                return (ulong) num;
            }

            private static IntPtr GetRefInt(IntPtr value)
            {
                return Marshal.ReadIntPtr(value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public int ptMinPosition_x;
            public int ptMinPosition_y;
            public int ptMaxPosition_x;
            public int ptMaxPosition_y;
            public int rcNormalPosition_left;
            public int rcNormalPosition_top;
            public int rcNormalPosition_right;
            public int rcNormalPosition_bottom;
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

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class WNDCLASS_D
        {
            public int style;
            public System.Windows.Forms.NativeMethods.WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance = IntPtr.Zero;
            public IntPtr hIcon = IntPtr.Zero;
            public IntPtr hCursor = IntPtr.Zero;
            public IntPtr hbrBackground = IntPtr.Zero;
            public string lpszMenuName;
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class WNDCLASS_I
        {
            public int style;
            public IntPtr lpfnWndProc = IntPtr.Zero;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance = IntPtr.Zero;
            public IntPtr hIcon = IntPtr.Zero;
            public IntPtr hCursor = IntPtr.Zero;
            public IntPtr hbrBackground = IntPtr.Zero;
            public IntPtr lpszMenuName = IntPtr.Zero;
            public IntPtr lpszClassName = IntPtr.Zero;
        }

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}

