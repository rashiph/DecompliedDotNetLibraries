namespace System.Drawing
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Internal;
    using System.Drawing.Text;
    using System.Internal;
    using System.IO;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal class SafeNativeMethods
    {
        public const int ALTERNATE = 1;
        public const int ANSI_CHARSET = 0;
        public const int BITSPIXEL = 12;
        public const int BLACKNESS = 0x42;
        public const int BS_SOLID = 0;
        public const int CAPTUREBLT = 0x40000000;
        public const int CHECKJPEGFORMAT = 0x1017;
        public const int CHECKPNGFORMAT = 0x1018;
        public const int CLIP_DEFAULT_PRECIS = 0;
        public const int DC_BINADJUST = 0x13;
        public const int DC_BINNAMES = 12;
        public const int DC_BINS = 6;
        public const int DC_COLLATE = 0x16;
        public const int DC_COPIES = 0x12;
        public const int DC_DATATYPE_PRODUCED = 0x15;
        public const int DC_DRIVER = 11;
        public const int DC_DUPLEX = 7;
        public const int DC_EMF_COMPLIANT = 20;
        public const int DC_ENUMRESOLUTIONS = 13;
        public const int DC_EXTRA = 9;
        public const int DC_FIELDS = 1;
        public const int DC_FILEDEPENDENCIES = 14;
        public const int DC_MAXEXTENT = 5;
        public const int DC_MINEXTENT = 4;
        public const int DC_ORIENTATION = 0x11;
        public const int DC_PAPERNAMES = 0x10;
        public const int DC_PAPERS = 2;
        public const int DC_PAPERSIZE = 3;
        public const int DC_SIZE = 8;
        public const int DC_TRUETYPE = 15;
        public const int DC_VERSION = 10;
        public const int DCBA_FACEDOWNCENTER = 0x101;
        public const int DCBA_FACEDOWNLEFT = 0x102;
        public const int DCBA_FACEDOWNNONE = 0x100;
        public const int DCBA_FACEDOWNRIGHT = 0x103;
        public const int DCBA_FACEUPCENTER = 1;
        public const int DCBA_FACEUPLEFT = 2;
        public const int DCBA_FACEUPNONE = 0;
        public const int DCBA_FACEUPRIGHT = 3;
        public const int DCTT_BITMAP = 1;
        public const int DCTT_DOWNLOAD = 2;
        public const int DCTT_DOWNLOAD_OUTLINE = 8;
        public const int DCTT_SUBDEV = 4;
        public const int DEFAULT_CHARSET = 1;
        public const int DEFAULT_QUALITY = 0;
        public const int DI_COMPAT = 4;
        public const int DI_DEFAULTSIZE = 8;
        public const int DI_IMAGE = 2;
        public const int DI_MASK = 1;
        public const int DI_NORMAL = 3;
        public const int DM_BITSPERPEL = 0x40000;
        public const int DM_COLLATE = 0x8000;
        public const int DM_COLOR = 0x800;
        public const int DM_COPIES = 0x100;
        public const int DM_COPY = 2;
        public const int DM_DEFAULTSOURCE = 0x200;
        public const int DM_DISPLAYFLAGS = 0x200000;
        public const int DM_DISPLAYFREQUENCY = 0x400000;
        public const int DM_DITHERTYPE = 0x10000000;
        public const int DM_DUPLEX = 0x1000;
        public const int DM_FORMNAME = 0x10000;
        public const int DM_ICCMANUFACTURER = 0x20000000;
        public const int DM_ICCMODEL = 0x40000000;
        public const int DM_ICMINTENT = 0x4000000;
        public const int DM_ICMMETHOD = 0x2000000;
        public const int DM_IN_BUFFER = 8;
        public const int DM_IN_PROMPT = 4;
        public const int DM_LOGPIXELS = 0x20000;
        public const int DM_MEDIATYPE = 0x8000000;
        public const int DM_MODIFY = 8;
        public const int DM_ORIENTATION = 1;
        public const int DM_OUT_BUFFER = 2;
        public const int DM_OUT_DEFAULT = 1;
        public const int DM_PANNINGHEIGHT = 0x1000000;
        public const int DM_PANNINGWIDTH = 0x800000;
        public const int DM_PAPERLENGTH = 4;
        public const int DM_PAPERSIZE = 2;
        public const int DM_PAPERWIDTH = 8;
        public const int DM_PELSHEIGHT = 0x100000;
        public const int DM_PELSWIDTH = 0x80000;
        public const int DM_PRINTQUALITY = 0x400;
        public const int DM_PROMPT = 4;
        public const int DM_SCALE = 0x10;
        public const int DM_SPECVERSION = 0x401;
        public const int DM_TTOPTION = 0x4000;
        public const int DM_UPDATE = 1;
        public const int DM_YRESOLUTION = 0x2000;
        public const int DMBIN_AUTO = 7;
        public const int DMBIN_CASSETTE = 14;
        public const int DMBIN_ENVELOPE = 5;
        public const int DMBIN_ENVMANUAL = 6;
        public const int DMBIN_FORMSOURCE = 15;
        public const int DMBIN_LARGECAPACITY = 11;
        public const int DMBIN_LARGEFMT = 10;
        public const int DMBIN_LAST = 15;
        public const int DMBIN_LOWER = 2;
        public const int DMBIN_MANUAL = 4;
        public const int DMBIN_MIDDLE = 3;
        public const int DMBIN_ONLYONE = 1;
        public const int DMBIN_SMALLFMT = 9;
        public const int DMBIN_TRACTOR = 8;
        public const int DMBIN_UPPER = 1;
        public const int DMBIN_USER = 0x100;
        public const int DMCOLLATE_FALSE = 0;
        public const int DMCOLLATE_TRUE = 1;
        public const int DMCOLOR_COLOR = 2;
        public const int DMCOLOR_MONOCHROME = 1;
        public const int DMDISPLAYFLAGS_TEXTMODE = 4;
        public const int DMDITHER_COARSE = 2;
        public const int DMDITHER_FINE = 3;
        public const int DMDITHER_GRAYSCALE = 5;
        public const int DMDITHER_LINEART = 4;
        public const int DMDITHER_NONE = 1;
        public const int DMDITHER_USER = 0x100;
        public const int DMDUP_HORIZONTAL = 3;
        public const int DMDUP_SIMPLEX = 1;
        public const int DMDUP_VERTICAL = 2;
        public const int DMICM_COLORMETRIC = 3;
        public const int DMICM_CONTRAST = 2;
        public const int DMICM_SATURATE = 1;
        public const int DMICM_USER = 0x100;
        public const int DMICMMETHOD_DEVICE = 4;
        public const int DMICMMETHOD_DRIVER = 3;
        public const int DMICMMETHOD_NONE = 1;
        public const int DMICMMETHOD_SYSTEM = 2;
        public const int DMICMMETHOD_USER = 0x100;
        public const int DMMEDIA_GLOSSY = 3;
        public const int DMMEDIA_STANDARD = 1;
        public const int DMMEDIA_TRANSPARENCY = 2;
        public const int DMMEDIA_USER = 0x100;
        public const int DMORIENT_LANDSCAPE = 2;
        public const int DMORIENT_PORTRAIT = 1;
        public const int DMPAPER_10X11 = 0x2d;
        public const int DMPAPER_10X14 = 0x10;
        public const int DMPAPER_11X17 = 0x11;
        public const int DMPAPER_12X11 = 90;
        public const int DMPAPER_15X11 = 0x2e;
        public const int DMPAPER_9X11 = 0x2c;
        public const int DMPAPER_A_PLUS = 0x39;
        public const int DMPAPER_A2 = 0x42;
        public const int DMPAPER_A3 = 8;
        public const int DMPAPER_A3_EXTRA = 0x3f;
        public const int DMPAPER_A3_EXTRA_TRANSVERSE = 0x44;
        public const int DMPAPER_A3_ROTATED = 0x4c;
        public const int DMPAPER_A3_TRANSVERSE = 0x43;
        public const int DMPAPER_A4 = 9;
        public const int DMPAPER_A4_EXTRA = 0x35;
        public const int DMPAPER_A4_PLUS = 60;
        public const int DMPAPER_A4_ROTATED = 0x4d;
        public const int DMPAPER_A4_TRANSVERSE = 0x37;
        public const int DMPAPER_A4SMALL = 10;
        public const int DMPAPER_A5 = 11;
        public const int DMPAPER_A5_EXTRA = 0x40;
        public const int DMPAPER_A5_ROTATED = 0x4e;
        public const int DMPAPER_A5_TRANSVERSE = 0x3d;
        public const int DMPAPER_A6 = 70;
        public const int DMPAPER_A6_ROTATED = 0x53;
        public const int DMPAPER_B_PLUS = 0x3a;
        public const int DMPAPER_B4 = 12;
        public const int DMPAPER_B4_JIS_ROTATED = 0x4f;
        public const int DMPAPER_B5 = 13;
        public const int DMPAPER_B5_EXTRA = 0x41;
        public const int DMPAPER_B5_JIS_ROTATED = 80;
        public const int DMPAPER_B5_TRANSVERSE = 0x3e;
        public const int DMPAPER_B6_JIS = 0x58;
        public const int DMPAPER_B6_JIS_ROTATED = 0x59;
        public const int DMPAPER_CSHEET = 0x18;
        public const int DMPAPER_DBL_JAPANESE_POSTCARD = 0x45;
        public const int DMPAPER_DBL_JAPANESE_POSTCARD_ROTATED = 0x52;
        public const int DMPAPER_DSHEET = 0x19;
        public const int DMPAPER_ENV_10 = 20;
        public const int DMPAPER_ENV_11 = 0x15;
        public const int DMPAPER_ENV_12 = 0x16;
        public const int DMPAPER_ENV_14 = 0x17;
        public const int DMPAPER_ENV_9 = 0x13;
        public const int DMPAPER_ENV_B4 = 0x21;
        public const int DMPAPER_ENV_B5 = 0x22;
        public const int DMPAPER_ENV_B6 = 0x23;
        public const int DMPAPER_ENV_C3 = 0x1d;
        public const int DMPAPER_ENV_C4 = 30;
        public const int DMPAPER_ENV_C5 = 0x1c;
        public const int DMPAPER_ENV_C6 = 0x1f;
        public const int DMPAPER_ENV_C65 = 0x20;
        public const int DMPAPER_ENV_DL = 0x1b;
        public const int DMPAPER_ENV_INVITE = 0x2f;
        public const int DMPAPER_ENV_ITALY = 0x24;
        public const int DMPAPER_ENV_MONARCH = 0x25;
        public const int DMPAPER_ENV_PERSONAL = 0x26;
        public const int DMPAPER_ESHEET = 0x1a;
        public const int DMPAPER_EXECUTIVE = 7;
        public const int DMPAPER_FANFOLD_LGL_GERMAN = 0x29;
        public const int DMPAPER_FANFOLD_STD_GERMAN = 40;
        public const int DMPAPER_FANFOLD_US = 0x27;
        public const int DMPAPER_FOLIO = 14;
        public const int DMPAPER_ISO_B4 = 0x2a;
        public const int DMPAPER_JAPANESE_POSTCARD = 0x2b;
        public const int DMPAPER_JAPANESE_POSTCARD_ROTATED = 0x51;
        public const int DMPAPER_JENV_CHOU3 = 0x49;
        public const int DMPAPER_JENV_CHOU3_ROTATED = 0x56;
        public const int DMPAPER_JENV_CHOU4 = 0x4a;
        public const int DMPAPER_JENV_CHOU4_ROTATED = 0x57;
        public const int DMPAPER_JENV_KAKU2 = 0x47;
        public const int DMPAPER_JENV_KAKU2_ROTATED = 0x54;
        public const int DMPAPER_JENV_KAKU3 = 0x48;
        public const int DMPAPER_JENV_KAKU3_ROTATED = 0x55;
        public const int DMPAPER_JENV_YOU4 = 0x5b;
        public const int DMPAPER_JENV_YOU4_ROTATED = 0x5c;
        public const int DMPAPER_LAST = 0x76;
        public const int DMPAPER_LEDGER = 4;
        public const int DMPAPER_LEGAL = 5;
        public const int DMPAPER_LEGAL_EXTRA = 0x33;
        public const int DMPAPER_LETTER = 1;
        public const int DMPAPER_LETTER_EXTRA = 50;
        public const int DMPAPER_LETTER_EXTRA_TRANSVERSE = 0x38;
        public const int DMPAPER_LETTER_PLUS = 0x3b;
        public const int DMPAPER_LETTER_ROTATED = 0x4b;
        public const int DMPAPER_LETTER_TRANSVERSE = 0x36;
        public const int DMPAPER_LETTERSMALL = 2;
        public const int DMPAPER_NOTE = 0x12;
        public const int DMPAPER_P16K = 0x5d;
        public const int DMPAPER_P16K_ROTATED = 0x6a;
        public const int DMPAPER_P32K = 0x5e;
        public const int DMPAPER_P32K_ROTATED = 0x6b;
        public const int DMPAPER_P32KBIG = 0x5f;
        public const int DMPAPER_P32KBIG_ROTATED = 0x6c;
        public const int DMPAPER_PENV_1 = 0x60;
        public const int DMPAPER_PENV_1_ROTATED = 0x6d;
        public const int DMPAPER_PENV_10 = 0x69;
        public const int DMPAPER_PENV_10_ROTATED = 0x76;
        public const int DMPAPER_PENV_2 = 0x61;
        public const int DMPAPER_PENV_2_ROTATED = 110;
        public const int DMPAPER_PENV_3 = 0x62;
        public const int DMPAPER_PENV_3_ROTATED = 0x6f;
        public const int DMPAPER_PENV_4 = 0x63;
        public const int DMPAPER_PENV_4_ROTATED = 0x70;
        public const int DMPAPER_PENV_5 = 100;
        public const int DMPAPER_PENV_5_ROTATED = 0x71;
        public const int DMPAPER_PENV_6 = 0x65;
        public const int DMPAPER_PENV_6_ROTATED = 0x72;
        public const int DMPAPER_PENV_7 = 0x66;
        public const int DMPAPER_PENV_7_ROTATED = 0x73;
        public const int DMPAPER_PENV_8 = 0x67;
        public const int DMPAPER_PENV_8_ROTATED = 0x74;
        public const int DMPAPER_PENV_9 = 0x68;
        public const int DMPAPER_PENV_9_ROTATED = 0x75;
        public const int DMPAPER_QUARTO = 15;
        public const int DMPAPER_RESERVED_48 = 0x30;
        public const int DMPAPER_RESERVED_49 = 0x31;
        public const int DMPAPER_STATEMENT = 6;
        public const int DMPAPER_TABLOID = 3;
        public const int DMPAPER_TABLOID_EXTRA = 0x34;
        public const int DMPAPER_USER = 0x100;
        public const int DMRES_DRAFT = -1;
        public const int DMRES_HIGH = -4;
        public const int DMRES_LOW = -2;
        public const int DMRES_MEDIUM = -3;
        public const int DMTT_BITMAP = 1;
        public const int DMTT_DOWNLOAD = 2;
        public const int DMTT_DOWNLOAD_OUTLINE = 4;
        public const int DMTT_SUBDEV = 3;
        public const int DSTINVERT = 0x550009;
        public const int DT_CHARSTREAM = 4;
        public const int DT_DISPFILE = 6;
        public const int DT_METAFILE = 5;
        public const int DT_PLOTTER = 0;
        public const int DT_RASCAMERA = 3;
        public const int DT_RASDISPLAY = 1;
        public const int DT_RASPRINTER = 2;
        public const int E_ABORT = -2147467260;
        public const int E_ACCESSDENIED = -2147024891;
        public const int E_FAIL = -2147467259;
        public const int E_HANDLE = -2147024890;
        public const int E_INVALIDARG = -2147024809;
        public const int E_NOINTERFACE = -2147467262;
        public const int E_NOTIMPL = -2147467263;
        public const int E_OUTOFMEMORY = -2147024882;
        public const int E_POINTER = -2147467261;
        public const int E_UNEXPECTED = -2147418113;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_CANCELLED = 0x4c7;
        public const int ERROR_PROC_NOT_FOUND = 0x7f;
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_DEFAULT = 0x1200;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FW_BOLD = 700;
        public const int FW_DONTCARE = 0;
        public const int FW_NORMAL = 400;
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
        public const int HOLLOW_BRUSH = 5;
        public const int HORZRES = 8;
        public const int IDC_APPSTARTING = 0x7f8a;
        public const int IDC_ARROW = 0x7f00;
        public const int IDC_CROSS = 0x7f03;
        public const int IDC_HELP = 0x7f8b;
        public const int IDC_IBEAM = 0x7f01;
        public const int IDC_ICON = 0x7f81;
        public const int IDC_NO = 0x7f88;
        public const int IDC_SIZE = 0x7f80;
        public const int IDC_SIZEALL = 0x7f86;
        public const int IDC_SIZENESW = 0x7f83;
        public const int IDC_SIZENS = 0x7f85;
        public const int IDC_SIZENWSE = 0x7f82;
        public const int IDC_SIZEWE = 0x7f84;
        public const int IDC_UPARROW = 0x7f04;
        public const int IDC_WAIT = 0x7f02;
        public const int IDI_APPLICATION = 0x7f00;
        public const int IDI_ASTERISK = 0x7f04;
        public const int IDI_ERROR = 0x7f01;
        public const int IDI_EXCLAMATION = 0x7f03;
        public const int IDI_HAND = 0x7f01;
        public const int IDI_INFORMATION = 0x7f04;
        public const int IDI_QUESTION = 0x7f02;
        public const int IDI_WARNING = 0x7f03;
        public const int IDI_WINLOGO = 0x7f05;
        public const int IMAGE_BITMAP = 0;
        public const int IMAGE_CURSOR = 2;
        public const int IMAGE_ENHMETAFILE = 3;
        public const int IMAGE_ICON = 1;
        public static IntPtr InvalidIntPtr = ((IntPtr) (-1));
        public const int LOGPIXELSX = 0x58;
        public const int LOGPIXELSY = 90;
        public const int MERGECOPY = 0xc000ca;
        public const int MERGEPAINT = 0xbb0226;
        public const int MM_TEXT = 1;
        public const int MWT_IDENTITY = 1;
        public const int NOMIRRORBITMAP = -2147483648;
        public const int NOTSRCCOPY = 0x330008;
        public const int NOTSRCERASE = 0x1100a6;
        public const int OBJ_FONT = 6;
        public const int OUT_DEFAULT_PRECIS = 0;
        public const int OUT_TT_ONLY_PRECIS = 7;
        public const int OUT_TT_PRECIS = 4;
        public const int PATCOPY = 0xf00021;
        public const int PATINVERT = 0x5a0049;
        public const int PATPAINT = 0xfb0a09;
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
        public const int PD_HIDEPRINTTOFILE = 0x100000;
        public const int PD_NOCURRENTPAGE = 0x800000;
        public const int PD_NONETWORKBUTTON = 0x200000;
        public const int PD_NOPAGENUMS = 8;
        public const int PD_NOSELECTION = 4;
        public const int PD_NOWARNING = 0x80;
        public const int PD_PAGENUMS = 2;
        public const int PD_PRINTSETUP = 0x40;
        public const int PD_PRINTTOFILE = 0x20;
        public const int PD_RETURNDC = 0x100;
        public const int PD_RETURNDEFAULT = 0x400;
        public const int PD_RETURNIC = 0x200;
        public const int PD_SELECTION = 1;
        public const int PD_SHOWHELP = 0x800;
        public const int PD_USEDEVMODECOPIES = 0x40000;
        public const int PD_USEDEVMODECOPIESANDCOLLATE = 0x40000;
        public const int PHYSICALHEIGHT = 0x6f;
        public const int PHYSICALOFFSETX = 0x70;
        public const int PHYSICALOFFSETY = 0x71;
        public const int PHYSICALWIDTH = 110;
        public const int PLANES = 14;
        public const int PM_NOREMOVE = 0;
        public const int PM_NOYIELD = 2;
        public const int PM_REMOVE = 1;
        public const int PRINTER_ENUM_CONNECTIONS = 4;
        public const int PRINTER_ENUM_CONTAINER = 0x8000;
        public const int PRINTER_ENUM_DEFAULT = 1;
        public const int PRINTER_ENUM_EXPAND = 0x4000;
        public const int PRINTER_ENUM_FAVORITE = 4;
        public const int PRINTER_ENUM_ICON1 = 0x10000;
        public const int PRINTER_ENUM_ICON2 = 0x20000;
        public const int PRINTER_ENUM_ICON3 = 0x40000;
        public const int PRINTER_ENUM_ICON4 = 0x80000;
        public const int PRINTER_ENUM_ICON5 = 0x100000;
        public const int PRINTER_ENUM_ICON6 = 0x200000;
        public const int PRINTER_ENUM_ICON7 = 0x400000;
        public const int PRINTER_ENUM_ICON8 = 0x800000;
        public const int PRINTER_ENUM_ICONMASK = 0xff0000;
        public const int PRINTER_ENUM_LOCAL = 2;
        public const int PRINTER_ENUM_NAME = 8;
        public const int PRINTER_ENUM_NETWORK = 0x40;
        public const int PRINTER_ENUM_REMOTE = 0x10;
        public const int PRINTER_ENUM_SHARED = 0x20;
        public const int PS_ALTERNATE = 8;
        public const int PS_COSMETIC = 0;
        public const int PS_DASH = 1;
        public const int PS_DASHDOT = 3;
        public const int PS_DASHDOTDOT = 4;
        public const int PS_DOT = 2;
        public const int PS_ENDCAP_FLAT = 0x200;
        public const int PS_ENDCAP_MASK = 0xf00;
        public const int PS_ENDCAP_ROUND = 0;
        public const int PS_ENDCAP_SQUARE = 0x100;
        public const int PS_GEOMETRIC = 0x10000;
        public const int PS_INSIDEFRAME = 6;
        public const int PS_JOIN_BEVEL = 0x1000;
        public const int PS_JOIN_MASK = 0xf000;
        public const int PS_JOIN_MITER = 0x2000;
        public const int PS_JOIN_ROUND = 0;
        public const int PS_NULL = 5;
        public const int PS_SOLID = 0;
        public const int PS_STYLE_MASK = 15;
        public const int PS_TYPE_MASK = 0xf0000;
        public const int PS_USERSTYLE = 7;
        public const int QUERYESCSUPPORT = 8;
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
        public const int RASTERCAPS = 0x26;
        public const int RC_PALETTE = 0x100;
        public const int SIZEPALETTE = 0x68;
        public const int SM_ARRANGE = 0x38;
        public const int SM_CLEANBOOT = 0x43;
        public const int SM_CMETRICS = 0x53;
        public const int SM_CMONITORS = 80;
        public const int SM_CMOUSEBUTTONS = 0x2b;
        public const int SM_CXBORDER = 5;
        public const int SM_CXCURSOR = 13;
        public const int SM_CXDLGFRAME = 7;
        public const int SM_CXDOUBLECLK = 0x24;
        public const int SM_CXDRAG = 0x44;
        public const int SM_CXEDGE = 0x2d;
        public const int SM_CXFIXEDFRAME = 7;
        public const int SM_CXFRAME = 0x20;
        public const int SM_CXFULLSCREEN = 0x10;
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
        public const int SM_CYDLGFRAME = 8;
        public const int SM_CYDOUBLECLK = 0x25;
        public const int SM_CYDRAG = 0x45;
        public const int SM_CYEDGE = 0x2e;
        public const int SM_CYFIXEDFRAME = 8;
        public const int SM_CYFRAME = 0x21;
        public const int SM_CYFULLSCREEN = 0x11;
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
        public const int SM_RESERVED1 = 0x18;
        public const int SM_RESERVED2 = 0x19;
        public const int SM_RESERVED3 = 0x1a;
        public const int SM_RESERVED4 = 0x1b;
        public const int SM_SAMEDISPLAYFORMAT = 0x51;
        public const int SM_SECURE = 0x2c;
        public const int SM_SHOWSOUNDS = 70;
        public const int SM_SLOWMACHINE = 0x49;
        public const int SM_SWAPBUTTON = 0x17;
        public const int SM_XVIRTUALSCREEN = 0x4c;
        public const int SM_YVIRTUALSCREEN = 0x4d;
        public const int SRCAND = 0x8800c6;
        public const int SRCCOPY = 0xcc0020;
        public const int SRCERASE = 0x440328;
        public const int SRCINVERT = 0x660046;
        public const int SRCPAINT = 0xee0086;
        public const int SYSPAL_STATIC = 1;
        public const int TA_DEFAULT = 0;
        public const int TECHNOLOGY = 2;
        public const int UOI_FLAGS = 1;
        public const int VERTRES = 10;
        public const int WHITENESS = 0xff0062;
        public const int WINDING = 2;
        public const int WSF_VISIBLE = 1;

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int AbortDoc(HandleRef hDC);
        public static int AddFontFile(string fileName)
        {
            if (Marshal.SystemDefaultCharSize == 1)
            {
                return 0;
            }
            return AddFontResourceEx(fileName, 0x10, IntPtr.Zero);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int AddFontResourceEx(string lpszFilename, int fl, IntPtr pdv);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int BitBlt(HandleRef hDC, int x, int y, int nWidth, int nHeight, HandleRef hSrcDC, int xSrc, int ySrc, int dwRop);
        public static IntPtr CopyImage(HandleRef hImage, int uType, int cxDesired, int cyDesired, int fuFlags)
        {
            int icon;
            if (uType == 1)
            {
                icon = CommonHandles.Icon;
            }
            else
            {
                icon = CommonHandles.GDI;
            }
            return System.Internal.HandleCollector.Add(IntCopyImage(hImage, uType, cxDesired, cyDesired, fuFlags), icon);
        }

        public static IntPtr CreateBitmap(int width, int height, int planes, int bpp, IntPtr bitmapData)
        {
            return System.Internal.HandleCollector.Add(IntCreateBitmap(width, height, planes, bpp, bitmapData), CommonHandles.GDI);
        }

        public static IntPtr CreateCompatibleBitmap(HandleRef hDC, int width, int height)
        {
            return System.Internal.HandleCollector.Add(IntCreateCompatibleBitmap(hDC, width, height), CommonHandles.GDI);
        }

        public static IntPtr CreateDIBSection(HandleRef hdc, ref System.Drawing.NativeMethods.BITMAPINFO_FLAT bmi, int iUsage, ref IntPtr ppvBits, IntPtr hSection, int dwOffset)
        {
            return System.Internal.HandleCollector.Add(IntCreateDIBSection(hdc, ref bmi, iUsage, ref ppvBits, hSection, dwOffset), CommonHandles.GDI);
        }

        public static unsafe IntPtr CreateIconFromResourceEx(byte* pbIconBits, int cbIconBits, bool fIcon, int dwVersion, int csDesired, int cyDesired, int flags)
        {
            return System.Internal.HandleCollector.Add(IntCreateIconFromResourceEx(pbIconBits, cbIconBits, fIcon, dwVersion, csDesired, cyDesired, flags), CommonHandles.Icon);
        }

        public static IntPtr CreateRectRgn(int x1, int y1, int x2, int y2)
        {
            return System.Internal.HandleCollector.Add(IntCreateRectRgn(x1, y1, x2, y2), CommonHandles.GDI);
        }

        public static int DeleteObject(HandleRef hObject)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hObject, CommonHandles.GDI);
            return IntDeleteObject(hObject);
        }

        public static bool DestroyIcon(HandleRef hIcon)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hIcon, CommonHandles.Icon);
            return IntDestroyIcon(hIcon);
        }

        [DllImport("winspool.drv", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int DeviceCapabilities(string pDevice, string pPort, short fwCapabilities, IntPtr pOutput, IntPtr pDevMode);
        [DllImport("winspool.drv", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int DocumentProperties(HandleRef hwnd, HandleRef hPrinter, string pDeviceName, IntPtr pDevModeOutput, IntPtr pDevModeInput, int fMode);
        [DllImport("winspool.drv", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int DocumentProperties(HandleRef hwnd, HandleRef hPrinter, string pDeviceName, IntPtr pDevModeOutput, HandleRef pDevModeInput, int fMode);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool DrawIconEx(HandleRef hDC, int x, int y, HandleRef hIcon, int width, int height, int iStepIfAniCursor, HandleRef hBrushFlickerFree, int diFlags);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int EndDoc(HandleRef hDC);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int EndPage(HandleRef hDC);
        [DllImport("winspool.drv", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int EnumPrinters(int flags, string name, int level, IntPtr pPrinterEnum, int cbBuf, out int pcbNeeded, out int pcReturned);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int ExtEscape(HandleRef hDC, int nEscape, int cbInput, ref int inData, int cbOutput, out int outData);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int ExtEscape(HandleRef hDC, int nEscape, int cbInput, byte[] inData, int cbOutput, out int outData);
        public static IntPtr ExtractAssociatedIcon(HandleRef hInst, StringBuilder iconPath, ref int index)
        {
            return System.Internal.HandleCollector.Add(IntExtractAssociatedIcon(hInst, iconPath, ref index), CommonHandles.Icon);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetClipRgn(HandleRef hDC, HandleRef hRgn);
        [DllImport("gdi32.dll")]
        public static extern int GetDIBits(HandleRef hdc, HandleRef hbm, int arg1, int arg2, IntPtr arg3, ref System.Drawing.NativeMethods.BITMAPINFO_FLAT bmi, int arg5);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool GetIconInfo(HandleRef hIcon, [In, Out] ICONINFO info);
        public static int GetObject(HandleRef hObject, LOGFONT lp)
        {
            return GetObject(hObject, Marshal.SizeOf(typeof(LOGFONT)), lp);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetObject(HandleRef hObject, int nSize, [In, Out] BITMAP bm);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetObject(HandleRef hObject, int nSize, [In, Out] LOGFONT lf);
        [DllImport("gdi32.dll")]
        public static extern uint GetPaletteEntries(HandleRef hpal, int iStartIndex, int nEntries, byte[] lppe);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetSysColor(int nIndex);
        public static IntPtr GlobalAlloc(int uFlags, uint dwBytes)
        {
            return IntGlobalAlloc(uFlags, new UIntPtr(dwBytes));
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr GlobalFree(HandleRef handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr GlobalLock(HandleRef handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool GlobalUnlock(HandleRef handle);
        [DllImport("user32.dll", EntryPoint="CopyImage", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCopyImage(HandleRef hImage, int uType, int cxDesired, int cyDesired, int fuFlags);
        [DllImport("gdi32.dll", EntryPoint="CreateBitmap", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntCreateBitmap(int width, int height, int planes, int bpp, IntPtr bitmapData);
        [DllImport("gdi32.dll", EntryPoint="CreateCompatibleBitmap", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntCreateCompatibleBitmap(HandleRef hDC, int width, int height);
        [DllImport("gdi32.dll", EntryPoint="CreateDIBSection", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntCreateDIBSection(HandleRef hdc, ref System.Drawing.NativeMethods.BITMAPINFO_FLAT bmi, int iUsage, ref IntPtr ppvBits, IntPtr hSection, int dwOffset);
        [DllImport("user32.dll", EntryPoint="CreateIconFromResourceEx", SetLastError=true)]
        private static extern unsafe IntPtr IntCreateIconFromResourceEx(byte* pbIconBits, int cbIconBits, bool fIcon, int dwVersion, int csDesired, int cyDesired, int flags);
        [DllImport("gdi32.dll", EntryPoint="CreateRectRgn", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateRectRgn(int x1, int y1, int x2, int y2);
        [DllImport("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern int IntDeleteObject(HandleRef hObject);
        [DllImport("user32.dll", EntryPoint="DestroyIcon", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern bool IntDestroyIcon(HandleRef hIcon);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntersectClipRect(HandleRef hDC, int x1, int y1, int x2, int y2);
        [DllImport("shell32.dll", EntryPoint="ExtractAssociatedIcon", CharSet=CharSet.Auto)]
        public static extern IntPtr IntExtractAssociatedIcon(HandleRef hInst, StringBuilder iconPath, ref int index);
        [DllImport("kernel32.dll", EntryPoint="GlobalAlloc", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntGlobalAlloc(int uFlags, UIntPtr dwBytes);
        [DllImport("user32.dll", EntryPoint="LoadIcon", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr IntLoadIcon(HandleRef hInst, IntPtr iconId);
        public static IntPtr LoadIcon(HandleRef hInst, int iconId)
        {
            return IntLoadIcon(hInst, new IntPtr(iconId));
        }

        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern IPicture OleCreatePictureIndirect(PICTDESC pictdesc, [In] ref Guid refiid, bool fOwn);
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool PrintDlg([In, Out] PRINTDLG lppd);
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool PrintDlg([In, Out] PRINTDLGX86 lppd);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr ResetDC(HandleRef hDC, HandleRef lpDevMode);
        internal static void RestoreClipRgn(IntPtr hDC, IntPtr hRgn)
        {
            try
            {
                SelectClipRgn(new HandleRef(null, hDC), new HandleRef(null, hRgn));
            }
            finally
            {
                if (hRgn != IntPtr.Zero)
                {
                    DeleteObject(new HandleRef(null, hRgn));
                }
            }
        }

        internal static IntPtr SaveClipRgn(IntPtr hDC)
        {
            IntPtr handle = CreateRectRgn(0, 0, 0, 0);
            IntPtr zero = IntPtr.Zero;
            try
            {
                if (GetClipRgn(new HandleRef(null, hDC), new HandleRef(null, handle)) > 0)
                {
                    zero = handle;
                    handle = IntPtr.Zero;
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    DeleteObject(new HandleRef(null, handle));
                }
            }
            return zero;
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SelectClipRgn(HandleRef hDC, HandleRef hRgn);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr SelectObject(HandleRef hdc, HandleRef obj);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int StartDoc(HandleRef hDC, DOCINFO lpDocInfo);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int StartPage(HandleRef hDC);
        [DllImport("kernel32.dll")]
        internal static extern void ZeroMemory(IntPtr destination, UIntPtr length);

        public enum BackgroundMode
        {
            OPAQUE = 2,
            TRANSPARENT = 1
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

        public sealed class CommonHandles
        {
            public static readonly int Accelerator = System.Internal.HandleCollector.RegisterType("Accelerator", 80, 50);
            public static readonly int Cursor = System.Internal.HandleCollector.RegisterType("Cursor", 20, 500);
            public static readonly int EMF = System.Internal.HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);
            public static readonly int Find = System.Internal.HandleCollector.RegisterType("Find", 0, 0x3e8);
            public static readonly int GDI = System.Internal.HandleCollector.RegisterType("GDI", 50, 500);
            public static readonly int HDC = System.Internal.HandleCollector.RegisterType("HDC", 100, 2);
            public static readonly int Icon = System.Internal.HandleCollector.RegisterType("Icon", 20, 500);
            public static readonly int Kernel = System.Internal.HandleCollector.RegisterType("Kernel", 0, 0x3e8);
            public static readonly int Menu = System.Internal.HandleCollector.RegisterType("Menu", 30, 0x3e8);
            public static readonly int Window = System.Internal.HandleCollector.RegisterType("Window", 5, 0x3e8);
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
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
            public int dmICCManufacturer;
            public int dmICCModel;
            public int dmPanningWidth;
            public int dmPanningHeight;
            public override string ToString()
            {
                return string.Concat(new object[] { 
                    "[DEVMODE: dmDeviceName=", this.dmDeviceName, ", dmSpecVersion=", this.dmSpecVersion, ", dmDriverVersion=", this.dmDriverVersion, ", dmSize=", this.dmSize, ", dmDriverExtra=", this.dmDriverExtra, ", dmFields=", this.dmFields, ", dmOrientation=", this.dmOrientation, ", dmPaperSize=", this.dmPaperSize, 
                    ", dmPaperLength=", this.dmPaperLength, ", dmPaperWidth=", this.dmPaperWidth, ", dmScale=", this.dmScale, ", dmCopies=", this.dmCopies, ", dmDefaultSource=", this.dmDefaultSource, ", dmPrintQuality=", this.dmPrintQuality, ", dmColor=", this.dmColor, ", dmDuplex=", this.dmDuplex, 
                    ", dmYResolution=", this.dmYResolution, ", dmTTOption=", this.dmTTOption, ", dmCollate=", this.dmCollate, ", dmFormName=", this.dmFormName, ", dmLogPixels=", this.dmLogPixels, ", dmBitsPerPel=", this.dmBitsPerPel, ", dmPelsWidth=", this.dmPelsWidth, ", dmPelsHeight=", this.dmPelsHeight, 
                    ", dmDisplayFlags=", this.dmDisplayFlags, ", dmDisplayFrequency=", this.dmDisplayFrequency, ", dmICMMethod=", this.dmICMMethod, ", dmICMIntent=", this.dmICMIntent, ", dmMediaType=", this.dmMediaType, ", dmDitherType=", this.dmDitherType, ", dmICCManufacturer=", this.dmICCManufacturer, ", dmICCModel=", this.dmICCModel, 
                    ", dmPanningWidth=", this.dmPanningWidth, ", dmPanningHeight=", this.dmPanningHeight, "]"
                 });
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class DOCINFO
        {
            public int cbSize = 20;
            public string lpszDocName;
            public string lpszOutput;
            public string lpszDatatype;
            public int fwType;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ENHMETAHEADER
        {
            public int iType;
            public int nSize = 40;
            public int rclBounds_left;
            public int rclBounds_top;
            public int rclBounds_right;
            public int rclBounds_bottom;
            public int rclFrame_left;
            public int rclFrame_top;
            public int rclFrame_right;
            public int rclFrame_bottom;
            public int dSignature;
            public int nVersion;
            public int nBytes;
            public int nRecords;
            public short nHandles;
            public short sReserved;
            public int nDescription;
            public int offDescription;
            public int nPalEntries;
            public int szlDevice_cx;
            public int szlDevice_cy;
            public int szlMillimeters_cx;
            public int szlMillimeters_cy;
            public int cbPixelFormat;
            public int offPixelFormat;
            public int bOpenGL;
        }

        [SuppressUnmanagedCodeSecurity]
        internal class Gdip
        {
            internal const int Aborted = 9;
            internal const int AccessDenied = 12;
            private static string atomName = null;
            internal const int FileNotFound = 10;
            internal const int FontFamilyNotFound = 14;
            internal const int FontStyleNotFound = 15;
            private static readonly BooleanSwitch GdiPlusIgnoreAtom = new BooleanSwitch("GdiPlusIgnoreAtom", "Ignores the use of global atoms for startup/shutdown");
            private static readonly TraceSwitch GdiPlusInitialization = new TraceSwitch("GdiPlusInitialization", "Tracks GDI+ initialization and teardown");
            internal const int GdiplusNotInitialized = 0x12;
            internal const int GenericError = 1;
            private static ushort hAtom = 0;
            private static IntPtr initToken;
            internal const int InsufficientBuffer = 5;
            internal const int InvalidParameter = 2;
            internal const int NotImplemented = 6;
            internal const int NotTrueTypeFont = 0x10;
            internal const int ObjectBusy = 4;
            internal const int Ok = 0;
            internal const int OutOfMemory = 3;
            internal const int PropertyNotFound = 0x13;
            internal const int PropertyNotSupported = 20;
            private const string ThreadDataSlotName = "system.drawing.threaddata";
            internal const int UnknownImageFormat = 13;
            internal const int UnsupportedGdiplusVersion = 0x11;
            internal const int ValueOverflow = 11;
            internal const int Win32Error = 7;
            internal const int WrongState = 8;

            static Gdip()
            {
                Initialize();
            }

            [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            private static extern ushort AddAtom(string lpString);
            internal static Point[] ConvertGPPOINTArray(IntPtr memory, int count)
            {
                if (memory == IntPtr.Zero)
                {
                    throw new ArgumentNullException("memory");
                }
                Point[] pointArray = new Point[count];
                GPPOINT gppoint = new GPPOINT();
                int num2 = Marshal.SizeOf(gppoint.GetType());
                for (int i = 0; i < count; i++)
                {
                    gppoint = (GPPOINT) UnsafeNativeMethods.PtrToStructure((IntPtr) (((long) memory) + (i * num2)), gppoint.GetType());
                    pointArray[i] = new Point(gppoint.X, gppoint.Y);
                }
                return pointArray;
            }

            internal static PointF[] ConvertGPPOINTFArrayF(IntPtr memory, int count)
            {
                if (memory == IntPtr.Zero)
                {
                    throw new ArgumentNullException("memory");
                }
                PointF[] tfArray = new PointF[count];
                GPPOINTF gppointf = new GPPOINTF();
                int num2 = Marshal.SizeOf(gppointf.GetType());
                for (int i = 0; i < count; i++)
                {
                    gppointf = (GPPOINTF) UnsafeNativeMethods.PtrToStructure((IntPtr) (((long) memory) + (i * num2)), gppointf.GetType());
                    tfArray[i] = new PointF(gppointf.X, gppointf.Y);
                }
                return tfArray;
            }

            internal static IntPtr ConvertPointToMemory(Point[] points)
            {
                if (points == null)
                {
                    throw new ArgumentNullException("points");
                }
                int num2 = Marshal.SizeOf(typeof(GPPOINT));
                int length = points.Length;
                IntPtr ptr = Marshal.AllocHGlobal((int) (length * num2));
                for (int i = 0; i < length; i++)
                {
                    Marshal.StructureToPtr(new GPPOINT(points[i]), (IntPtr) (((long) ptr) + (i * num2)), false);
                }
                return ptr;
            }

            internal static IntPtr ConvertPointToMemory(PointF[] points)
            {
                if (points == null)
                {
                    throw new ArgumentNullException("points");
                }
                int num2 = Marshal.SizeOf(typeof(GPPOINTF));
                int length = points.Length;
                IntPtr ptr = Marshal.AllocHGlobal((int) (length * num2));
                for (int i = 0; i < length; i++)
                {
                    Marshal.StructureToPtr(new GPPOINTF(points[i]), (IntPtr) (((long) ptr) + (i * num2)), false);
                }
                return ptr;
            }

            internal static IntPtr ConvertRectangleToMemory(Rectangle[] rect)
            {
                if (rect == null)
                {
                    throw new ArgumentNullException("rect");
                }
                int num2 = Marshal.SizeOf(typeof(GPRECT));
                int length = rect.Length;
                IntPtr ptr = Marshal.AllocHGlobal((int) (length * num2));
                for (int i = 0; i < length; i++)
                {
                    Marshal.StructureToPtr(new GPRECT(rect[i]), (IntPtr) (((long) ptr) + (i * num2)), false);
                }
                return ptr;
            }

            internal static IntPtr ConvertRectangleToMemory(RectangleF[] rect)
            {
                if (rect == null)
                {
                    throw new ArgumentNullException("rect");
                }
                int num2 = Marshal.SizeOf(typeof(GPRECTF));
                int length = rect.Length;
                IntPtr ptr = Marshal.AllocHGlobal((int) (length * num2));
                for (int i = 0; i < length; i++)
                {
                    Marshal.StructureToPtr(new GPRECTF(rect[i]), (IntPtr) (((long) ptr) + (i * num2)), false);
                }
                return ptr;
            }

            [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            private static extern ushort DeleteAtom(ushort hAtom);
            private static void DestroyAtom()
            {
                if (hAtom != 0)
                {
                    DeleteAtom(hAtom);
                }
            }

            internal static void DummyFunction()
            {
            }

            private static bool EnsureAtomInitialized()
            {
                if (FindAtom(AtomName) != 0)
                {
                    return true;
                }
                hAtom = AddAtom(AtomName);
                return false;
            }

            [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            private static extern ushort FindAtom(string lpString);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathArc(HandleRef path, float x, float y, float width, float height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathArcI(HandleRef path, int x, int y, int width, int height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathBezier(HandleRef path, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathBezierI(HandleRef path, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathBeziers(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathBeziersI(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathClosedCurve(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathClosedCurve2(HandleRef path, HandleRef memorypts, int count, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathClosedCurve2I(HandleRef path, HandleRef memorypts, int count, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathClosedCurveI(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathCurve(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathCurve2(HandleRef path, HandleRef memorypts, int count, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathCurve2I(HandleRef path, HandleRef memorypts, int count, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathCurve3(HandleRef path, HandleRef memorypts, int count, int offset, int numberOfSegments, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathCurve3I(HandleRef path, HandleRef memorypts, int count, int offset, int numberOfSegments, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathCurveI(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathEllipse(HandleRef path, float x, float y, float width, float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathEllipseI(HandleRef path, int x, int y, int width, int height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathLine(HandleRef path, float x1, float y1, float x2, float y2);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathLine2(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathLine2I(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathLineI(HandleRef path, int x1, int y1, int x2, int y2);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathPath(HandleRef path, HandleRef addingPath, bool connect);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathPie(HandleRef path, float x, float y, float width, float height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathPieI(HandleRef path, int x, int y, int width, int height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathPolygon(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathPolygonI(HandleRef path, HandleRef memorypts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathRectangle(HandleRef path, float x, float y, float width, float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathRectangleI(HandleRef path, int x, int y, int width, int height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathRectangles(HandleRef path, HandleRef rects, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathRectanglesI(HandleRef path, HandleRef rects, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathString(HandleRef path, string s, int length, HandleRef fontFamily, int style, float emSize, ref GPRECTF layoutRect, HandleRef format);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipAddPathStringI(HandleRef path, string s, int length, HandleRef fontFamily, int style, float emSize, ref GPRECT layoutRect, HandleRef format);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipBeginContainer(HandleRef graphics, ref GPRECTF dstRect, ref GPRECTF srcRect, int unit, out int state);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipBeginContainer2(HandleRef graphics, out int state);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipBeginContainerI(HandleRef graphics, ref GPRECT dstRect, ref GPRECT srcRect, int unit, out int state);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipBitmapGetPixel(HandleRef bitmap, int x, int y, out int argb);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipBitmapLockBits(HandleRef bitmap, ref GPRECT rect, ImageLockMode flags, PixelFormat format, [In, Out] BitmapData lockedBitmapData);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipBitmapSetPixel(HandleRef bitmap, int x, int y, int argb);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipBitmapSetResolution(HandleRef bitmap, float dpix, float dpiy);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipBitmapUnlockBits(HandleRef bitmap, BitmapData lockedBitmapData);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipClearPathMarkers(HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneBitmapArea(float x, float y, float width, float height, int format, HandleRef srcbitmap, out IntPtr dstbitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneBitmapAreaI(int x, int y, int width, int height, int format, HandleRef srcbitmap, out IntPtr dstbitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneBrush(HandleRef brush, out IntPtr clonebrush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneCustomLineCap(HandleRef customCap, out IntPtr clonedCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneFont(HandleRef font, out IntPtr cloneFont);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneFontFamily(HandleRef fontfamily, out IntPtr clonefontfamily);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneImage(HandleRef image, out IntPtr cloneimage);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneImageAttributes(HandleRef imageattr, out IntPtr cloneImageattr);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneMatrix(HandleRef matrix, out IntPtr cloneMatrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipClonePath(HandleRef path, out IntPtr clonepath);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipClonePen(HandleRef pen, out IntPtr clonepen);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneRegion(HandleRef region, out IntPtr cloneregion);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCloneStringFormat(HandleRef format, out IntPtr newFormat);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipClosePathFigure(HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipClosePathFigures(HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCombineRegionPath(HandleRef region, HandleRef path, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCombineRegionRect(HandleRef region, ref GPRECTF gprectf, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCombineRegionRectI(HandleRef region, ref GPRECT gprect, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCombineRegionRegion(HandleRef region, HandleRef region2, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipComment(HandleRef graphics, int sizeData, byte[] data);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateAdjustableArrowCap(float height, float width, bool isFilled, out IntPtr adjustableArrowCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromFile(string filename, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromFileICM(string filename, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromGraphics(int width, int height, HandleRef graphics, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromHBITMAP(HandleRef hbitmap, HandleRef hpalette, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromHICON(HandleRef hicon, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromResource(HandleRef hresource, HandleRef name, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromScan0(int width, int height, int stride, int format, HandleRef scan0, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromStream(UnsafeNativeMethods.IStream stream, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateBitmapFromStreamICM(UnsafeNativeMethods.IStream stream, out IntPtr bitmap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateCustomLineCap(HandleRef fillpath, HandleRef strokepath, LineCap baseCap, float baseInset, out IntPtr customCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateFont(HandleRef fontFamily, float emSize, FontStyle style, GraphicsUnit unit, out IntPtr font);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateFontFamilyFromName(string name, HandleRef fontCollection, out IntPtr FontFamily);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateFontFromDC(HandleRef hdc, ref IntPtr font);
            [DllImport("gdiplus.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateFontFromLogfontA(HandleRef hdc, [In, Out, MarshalAs(UnmanagedType.AsAny)] object lf, out IntPtr font);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateFontFromLogfontW(HandleRef hdc, [In, Out, MarshalAs(UnmanagedType.AsAny)] object lf, out IntPtr font);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateFromHDC(HandleRef hdc, out IntPtr graphics);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateFromHDC2(HandleRef hdc, HandleRef hdevice, out IntPtr graphics);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateFromHWND(HandleRef hwnd, out IntPtr graphics);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern IntPtr GdipCreateHalftonePalette();
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateHatchBrush(int hatchstyle, int forecol, int backcol, out IntPtr brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateHBITMAPFromBitmap(HandleRef nativeBitmap, out IntPtr hbitmap, int argbBackground);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateHICONFromBitmap(HandleRef nativeBitmap, out IntPtr hicon);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateImageAttributes(out IntPtr imageattr);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateLineBrush(GPPOINTF point1, GPPOINTF point2, int color1, int color2, int wrapMode, out IntPtr lineGradient);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateLineBrushFromRect(ref GPRECTF rect, int color1, int color2, int lineGradientMode, int wrapMode, out IntPtr lineGradient);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateLineBrushFromRectI(ref GPRECT rect, int color1, int color2, int lineGradientMode, int wrapMode, out IntPtr lineGradient);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateLineBrushFromRectWithAngle(ref GPRECTF rect, int color1, int color2, float angle, bool isAngleScaleable, int wrapMode, out IntPtr lineGradient);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateLineBrushFromRectWithAngleI(ref GPRECT rect, int color1, int color2, float angle, bool isAngleScaleable, int wrapMode, out IntPtr lineGradient);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateLineBrushI(GPPOINT point1, GPPOINT point2, int color1, int color2, int wrapMode, out IntPtr lineGradient);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateMatrix(out IntPtr matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateMatrix2(float m11, float m12, float m21, float m22, float dx, float dy, out IntPtr matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateMatrix3(ref GPRECTF rect, HandleRef dstplg, out IntPtr matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateMatrix3I(ref GPRECT rect, HandleRef dstplg, out IntPtr matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateMetafileFromEmf(HandleRef hEnhMetafile, bool deleteEmf, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateMetafileFromFile(string file, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateMetafileFromStream(UnsafeNativeMethods.IStream stream, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateMetafileFromWmf(HandleRef hMetafile, WmfPlaceableFileHeader wmfplacealbeHeader, bool deleteWmf, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePath(int brushMode, out IntPtr path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePath2(HandleRef points, HandleRef types, int count, int brushMode, out IntPtr path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePath2I(HandleRef points, HandleRef types, int count, int brushMode, out IntPtr path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePathGradient(HandleRef points, int count, int wrapMode, out IntPtr brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePathGradientFromPath(HandleRef path, out IntPtr brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePathGradientI(HandleRef points, int count, int wrapMode, out IntPtr brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePathIter(out IntPtr pathIter, HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePen1(int argb, float width, int unit, out IntPtr pen);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreatePen2(HandleRef brush, float width, int unit, out IntPtr pen);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateRegion(out IntPtr region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateRegionHrgn(HandleRef hRgn, out IntPtr region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateRegionPath(HandleRef path, out IntPtr region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateRegionRect(ref GPRECTF gprectf, out IntPtr region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateRegionRectI(ref GPRECT gprect, out IntPtr region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateRegionRgnData(byte[] rgndata, int size, out IntPtr region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateSolidFill(int color, out IntPtr brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateStringFormat(StringFormatFlags options, int language, out IntPtr format);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateTexture(HandleRef bitmap, int wrapmode, out IntPtr texture);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateTexture2(HandleRef bitmap, int wrapmode, float x, float y, float width, float height, out IntPtr texture);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateTexture2I(HandleRef bitmap, int wrapmode, int x, int y, int width, int height, out IntPtr texture);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateTextureIA(HandleRef bitmap, HandleRef imageAttrib, float x, float y, float width, float height, out IntPtr texture);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipCreateTextureIAI(HandleRef bitmap, HandleRef imageAttrib, int x, int y, int width, int height, out IntPtr texture);
            internal static int GdipDeleteBrush(HandleRef brush)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeleteBrush(brush);
            }

            internal static int GdipDeleteCustomLineCap(HandleRef customCap)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeleteCustomLineCap(customCap);
            }

            internal static int GdipDeleteFont(HandleRef font)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeleteFont(font);
            }

            internal static int GdipDeleteFontFamily(HandleRef fontFamily)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeleteFontFamily(fontFamily);
            }

            internal static int GdipDeleteGraphics(HandleRef graphics)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeleteGraphics(graphics);
            }

            internal static int GdipDeleteMatrix(HandleRef matrix)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeleteMatrix(matrix);
            }

            internal static int GdipDeletePath(HandleRef path)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeletePath(path);
            }

            internal static int GdipDeletePathIter(HandleRef pathIter)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeletePathIter(pathIter);
            }

            internal static int GdipDeletePen(HandleRef pen)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeletePen(pen);
            }

            internal static int GdipDeletePrivateFontCollection(out IntPtr fontCollection)
            {
                if (IsShutdown)
                {
                    fontCollection = IntPtr.Zero;
                    return 0;
                }
                return IntGdipDeletePrivateFontCollection(out fontCollection);
            }

            internal static int GdipDeleteRegion(HandleRef region)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeleteRegion(region);
            }

            internal static int GdipDeleteStringFormat(HandleRef format)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDeleteStringFormat(format);
            }

            internal static int GdipDisposeImage(HandleRef image)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDisposeImage(image);
            }

            internal static int GdipDisposeImageAttributes(HandleRef imageattr)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipDisposeImageAttributes(imageattr);
            }

            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawArc(HandleRef graphics, HandleRef pen, float x, float y, float width, float height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawArcI(HandleRef graphics, HandleRef pen, int x, int y, int width, int height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawBezier(HandleRef graphics, HandleRef pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawBeziers(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawBeziersI(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawClosedCurve(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawClosedCurve2(HandleRef graphics, HandleRef pen, HandleRef points, int count, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawClosedCurve2I(HandleRef graphics, HandleRef pen, HandleRef points, int count, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawClosedCurveI(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawCurve(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawCurve2(HandleRef graphics, HandleRef pen, HandleRef points, int count, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawCurve2I(HandleRef graphics, HandleRef pen, HandleRef points, int count, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawCurve3(HandleRef graphics, HandleRef pen, HandleRef points, int count, int offset, int numberOfSegments, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawCurve3I(HandleRef graphics, HandleRef pen, HandleRef points, int count, int offset, int numberOfSegments, float tension);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawCurveI(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawEllipse(HandleRef graphics, HandleRef pen, float x, float y, float width, float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawEllipseI(HandleRef graphics, HandleRef pen, int x, int y, int width, int height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImage(HandleRef graphics, HandleRef image, float x, float y);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImageI(HandleRef graphics, HandleRef image, int x, int y);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImagePointRect(HandleRef graphics, HandleRef image, float x, float y, float srcx, float srcy, float srcwidth, float srcheight, int srcunit);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImagePointRectI(HandleRef graphics, HandleRef image, int x, int y, int srcx, int srcy, int srcwidth, int srcheight, int srcunit);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImagePoints(HandleRef graphics, HandleRef image, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImagePointsI(HandleRef graphics, HandleRef image, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImagePointsRect(HandleRef graphics, HandleRef image, HandleRef points, int count, float srcx, float srcy, float srcwidth, float srcheight, int srcunit, HandleRef imageAttributes, Graphics.DrawImageAbort callback, HandleRef callbackdata);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImagePointsRectI(HandleRef graphics, HandleRef image, HandleRef points, int count, int srcx, int srcy, int srcwidth, int srcheight, int srcunit, HandleRef imageAttributes, Graphics.DrawImageAbort callback, HandleRef callbackdata);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImageRect(HandleRef graphics, HandleRef image, float x, float y, float width, float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImageRectI(HandleRef graphics, HandleRef image, int x, int y, int width, int height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImageRectRect(HandleRef graphics, HandleRef image, float dstx, float dsty, float dstwidth, float dstheight, float srcx, float srcy, float srcwidth, float srcheight, int srcunit, HandleRef imageAttributes, Graphics.DrawImageAbort callback, HandleRef callbackdata);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawImageRectRectI(HandleRef graphics, HandleRef image, int dstx, int dsty, int dstwidth, int dstheight, int srcx, int srcy, int srcwidth, int srcheight, int srcunit, HandleRef imageAttributes, Graphics.DrawImageAbort callback, HandleRef callbackdata);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawLine(HandleRef graphics, HandleRef pen, float x1, float y1, float x2, float y2);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawLineI(HandleRef graphics, HandleRef pen, int x1, int y1, int x2, int y2);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawLines(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawLinesI(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawPath(HandleRef graphics, HandleRef pen, HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawPie(HandleRef graphics, HandleRef pen, float x, float y, float width, float height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawPieI(HandleRef graphics, HandleRef pen, int x, int y, int width, int height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawPolygon(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawPolygonI(HandleRef graphics, HandleRef pen, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawRectangle(HandleRef graphics, HandleRef pen, float x, float y, float width, float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawRectangleI(HandleRef graphics, HandleRef pen, int x, int y, int width, int height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawRectangles(HandleRef graphics, HandleRef pen, HandleRef rects, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawRectanglesI(HandleRef graphics, HandleRef pen, HandleRef rects, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipDrawString(HandleRef graphics, string textString, int length, HandleRef font, ref GPRECTF layoutRect, HandleRef stringFormat, HandleRef brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEndContainer(HandleRef graphics, int state);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileDestPoint(HandleRef graphics, HandleRef metafile, GPPOINTF destPoint, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileDestPointI(HandleRef graphics, HandleRef metafile, GPPOINT destPoint, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileDestPoints(HandleRef graphics, HandleRef metafile, IntPtr destPoints, int count, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileDestPointsI(HandleRef graphics, HandleRef metafile, IntPtr destPoints, int count, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileDestRect(HandleRef graphics, HandleRef metafile, ref GPRECTF destRect, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileDestRectI(HandleRef graphics, HandleRef metafile, ref GPRECT destRect, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileSrcRectDestPoint(HandleRef graphics, HandleRef metafile, GPPOINTF destPoint, ref GPRECTF srcRect, int pageUnit, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileSrcRectDestPointI(HandleRef graphics, HandleRef metafile, GPPOINT destPoint, ref GPRECT srcRect, int pageUnit, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileSrcRectDestPoints(HandleRef graphics, HandleRef metafile, IntPtr destPoints, int count, ref GPRECTF srcRect, int pageUnit, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileSrcRectDestPointsI(HandleRef graphics, HandleRef metafile, IntPtr destPoints, int count, ref GPRECT srcRect, int pageUnit, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileSrcRectDestRect(HandleRef graphics, HandleRef metafile, ref GPRECTF destRect, ref GPRECTF srcRect, int pageUnit, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipEnumerateMetafileSrcRectDestRectI(HandleRef graphics, HandleRef metafile, ref GPRECT destRect, ref GPRECT srcRect, int pageUnit, Graphics.EnumerateMetafileProc callback, HandleRef callbackdata, HandleRef imageattributes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillClosedCurve(HandleRef graphics, HandleRef brush, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillClosedCurve2(HandleRef graphics, HandleRef brush, HandleRef points, int count, float tension, int mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillClosedCurve2I(HandleRef graphics, HandleRef brush, HandleRef points, int count, float tension, int mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillClosedCurveI(HandleRef graphics, HandleRef brush, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillEllipse(HandleRef graphics, HandleRef brush, float x, float y, float width, float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillEllipseI(HandleRef graphics, HandleRef brush, int x, int y, int width, int height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillPath(HandleRef graphics, HandleRef brush, HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillPie(HandleRef graphics, HandleRef brush, float x, float y, float width, float height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillPieI(HandleRef graphics, HandleRef brush, int x, int y, int width, int height, float startAngle, float sweepAngle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillPolygon(HandleRef graphics, HandleRef brush, HandleRef points, int count, int brushMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillPolygonI(HandleRef graphics, HandleRef brush, HandleRef points, int count, int brushMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillRectangle(HandleRef graphics, HandleRef brush, float x, float y, float width, float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillRectangleI(HandleRef graphics, HandleRef brush, int x, int y, int width, int height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillRectangles(HandleRef graphics, HandleRef brush, HandleRef rects, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillRectanglesI(HandleRef graphics, HandleRef brush, HandleRef rects, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFillRegion(HandleRef graphics, HandleRef brush, HandleRef region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFlattenPath(HandleRef path, HandleRef matrixfloat, float flatness);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipFlush(HandleRef graphics, FlushIntention intention);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetAdjustableArrowCapFillState(HandleRef adjustableArrowCap, out bool fillState);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetAdjustableArrowCapHeight(HandleRef adjustableArrowCap, out float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetAdjustableArrowCapMiddleInset(HandleRef adjustableArrowCap, out float middleInset);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetAdjustableArrowCapWidth(HandleRef adjustableArrowCap, out float width);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetAllPropertyItems(HandleRef image, int totalSize, int count, IntPtr buffer);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCellAscent(HandleRef family, FontStyle style, out int CellAscent);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCellDescent(HandleRef family, FontStyle style, out int CellDescent);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetClip(HandleRef graphics, HandleRef region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetClipBounds(HandleRef graphics, ref GPRECTF rect);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCompositingMode(HandleRef graphics, out int compositeMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCompositingQuality(HandleRef graphics, out CompositingQuality quality);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCustomLineCapBaseCap(HandleRef customCap, out LineCap baseCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCustomLineCapBaseInset(HandleRef customCap, out float inset);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCustomLineCapStrokeCaps(HandleRef customCap, out LineCap startCap, out LineCap endCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCustomLineCapStrokeJoin(HandleRef customCap, out LineJoin lineJoin);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCustomLineCapType(HandleRef customCap, out CustomLineCapType capType);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetCustomLineCapWidthScale(HandleRef customCap, out float widthScale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetDC(HandleRef graphics, out IntPtr hdc);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetDpiX(HandleRef graphics, float[] dpi);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetDpiY(HandleRef graphics, float[] dpi);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetEmHeight(HandleRef family, FontStyle style, out int EmHeight);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetEncoderParameterList(HandleRef image, ref Guid clsid, int size, IntPtr buffer);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetEncoderParameterListSize(HandleRef image, ref Guid clsid, out int size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFamily(HandleRef font, out IntPtr family);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFamilyName(HandleRef family, StringBuilder name, int language);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFontCollectionFamilyCount(HandleRef fontCollection, out int numFound);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFontCollectionFamilyList(HandleRef fontCollection, int numSought, IntPtr[] gpfamilies, out int numFound);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFontHeight(HandleRef font, HandleRef graphics, out float size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFontHeightGivenDPI(HandleRef font, float dpi, out float size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFontSize(HandleRef font, out float size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFontStyle(HandleRef font, out FontStyle style);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetFontUnit(HandleRef font, out GraphicsUnit unit);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetGenericFontFamilyMonospace(out IntPtr fontfamily);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetGenericFontFamilySansSerif(out IntPtr fontfamily);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetGenericFontFamilySerif(out IntPtr fontfamily);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetHatchBackgroundColor(HandleRef brush, out int backcol);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetHatchForegroundColor(HandleRef brush, out int forecol);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetHatchStyle(HandleRef brush, out int hatchstyle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetHemfFromMetafile(HandleRef metafile, out IntPtr hEnhMetafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageAttributesAdjustedPalette(HandleRef imageattr, HandleRef palette, ColorAdjustType type);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageBounds(HandleRef image, ref GPRECTF gprectf, out GraphicsUnit unit);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageDecoders(int numDecoders, int size, IntPtr decoders);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageDecodersSize(out int numDecoders, out int size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageDimension(HandleRef image, out float width, out float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageEncoders(int numEncoders, int size, IntPtr encoders);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageEncodersSize(out int numEncoders, out int size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageFlags(HandleRef image, out int flags);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageGraphicsContext(HandleRef image, out IntPtr graphics);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageHeight(HandleRef image, out int height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageHorizontalResolution(HandleRef image, out float horzRes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImagePalette(HandleRef image, IntPtr palette, int size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImagePaletteSize(HandleRef image, out int size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImagePixelFormat(HandleRef image, out int format);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageRawFormat(HandleRef image, ref Guid format);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageThumbnail(HandleRef image, int thumbWidth, int thumbHeight, out IntPtr thumbImage, Image.GetThumbnailImageAbort callback, IntPtr callbackdata);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageType(HandleRef image, out int type);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageVerticalResolution(HandleRef image, out float vertRes);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetImageWidth(HandleRef image, out int width);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetInterpolationMode(HandleRef graphics, out int mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLineBlend(HandleRef brush, IntPtr blend, IntPtr positions, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLineBlendCount(HandleRef brush, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLineColors(HandleRef brush, int[] colors);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLineGammaCorrection(HandleRef brush, out bool useGammaCorrection);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLinePresetBlend(HandleRef brush, IntPtr blend, IntPtr positions, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLinePresetBlendCount(HandleRef brush, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLineRect(HandleRef brush, ref GPRECTF gprectf);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLineSpacing(HandleRef family, FontStyle style, out int LineSpaceing);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLineTransform(HandleRef brush, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLineWrapMode(HandleRef brush, out int wrapMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLogFontA(HandleRef font, HandleRef graphics, [In, Out, MarshalAs(UnmanagedType.AsAny)] object lf);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetLogFontW(HandleRef font, HandleRef graphics, [In, Out, MarshalAs(UnmanagedType.AsAny)] object lf);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetMatrixElements(HandleRef matrix, IntPtr m);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetMetafileHeaderFromEmf(HandleRef hEnhMetafile, [In, Out] MetafileHeaderEmf metafileHeaderEmf);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetMetafileHeaderFromFile(string filename, IntPtr header);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetMetafileHeaderFromMetafile(HandleRef metafile, IntPtr header);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetMetafileHeaderFromStream(UnsafeNativeMethods.IStream stream, IntPtr header);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetMetafileHeaderFromWmf(HandleRef hMetafile, WmfPlaceableFileHeader wmfplaceable, [In, Out] MetafileHeaderWmf metafileHeaderWmf);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetNearestColor(HandleRef graphics, ref int color);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPageScale(HandleRef graphics, float[] scale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPageUnit(HandleRef graphics, out int unit);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathData(HandleRef path, IntPtr pathData);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathFillMode(HandleRef path, out int fillmode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientBlend(HandleRef brush, IntPtr blend, IntPtr positions, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientBlendCount(HandleRef brush, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientCenterColor(HandleRef brush, out int color);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientCenterPoint(HandleRef brush, GPPOINTF point);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientFocusScales(HandleRef brush, float[] xScale, float[] yScale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientPointCount(HandleRef brush, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientPresetBlend(HandleRef brush, IntPtr blend, IntPtr positions, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientPresetBlendCount(HandleRef brush, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientRect(HandleRef brush, ref GPRECTF gprectf);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientSurroundColorCount(HandleRef brush, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientSurroundColorsWithCount(HandleRef brush, int[] color, ref int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientTransform(HandleRef brush, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathGradientWrapMode(HandleRef brush, out int wrapmode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathLastPoint(HandleRef path, GPPOINTF lastPoint);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathPoints(HandleRef path, HandleRef points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathTypes(HandleRef path, byte[] types, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPathWorldBounds(HandleRef path, ref GPRECTF gprectf, HandleRef matrix, HandleRef pen);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenBrushFill(HandleRef pen, out IntPtr brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenColor(HandleRef pen, out int argb);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenCompoundArray(HandleRef pen, float[] array, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenCompoundCount(HandleRef pen, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenCustomEndCap(HandleRef pen, out IntPtr customCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenCustomStartCap(HandleRef pen, out IntPtr customCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenDashArray(HandleRef pen, IntPtr memorydash, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenDashCap197819(HandleRef pen, out int dashCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenDashCount(HandleRef pen, out int dashcount);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenDashOffset(HandleRef pen, float[] dashoffset);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenDashStyle(HandleRef pen, out int dashstyle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenEndCap(HandleRef pen, out int endCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenFillType(HandleRef pen, out int pentype);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenLineJoin(HandleRef pen, out int lineJoin);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenMiterLimit(HandleRef pen, float[] miterLimit);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenMode(HandleRef pen, out PenAlignment penAlign);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenStartCap(HandleRef pen, out int startCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenTransform(HandleRef pen, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPenWidth(HandleRef pen, float[] width);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPixelOffsetMode(HandleRef graphics, out PixelOffsetMode pixelOffsetMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPointCount(HandleRef path, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPropertyCount(HandleRef image, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPropertyIdList(HandleRef image, int count, int[] list);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPropertyItem(HandleRef image, int propid, int size, IntPtr buffer);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPropertyItemSize(HandleRef image, int propid, out int size);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetPropertySize(HandleRef image, out int totalSize, ref int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetRegionBounds(HandleRef region, HandleRef graphics, ref GPRECTF gprectf);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetRegionData(HandleRef region, byte[] regionData, int bufferSize, out int sizeFilled);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetRegionDataSize(HandleRef region, out int bufferSize);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetRegionHRgn(HandleRef region, HandleRef graphics, out IntPtr hrgn);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetRegionScans(HandleRef region, IntPtr rects, out int count, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetRegionScansCount(HandleRef region, out int count, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetRenderingOrigin(HandleRef graphics, out int x, out int y);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetSmoothingMode(HandleRef graphics, out SmoothingMode smoothingMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetSolidFillColor(HandleRef brush, out int color);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatAlign(HandleRef format, out StringAlignment align);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatDigitSubstitution(HandleRef format, out int langID, out StringDigitSubstitute sds);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatFlags(HandleRef format, out StringFormatFlags result);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatHotkeyPrefix(HandleRef format, out HotkeyPrefix hotkeyPrefix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatLineAlign(HandleRef format, out StringAlignment align);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatMeasurableCharacterRangeCount(HandleRef format, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatTabStopCount(HandleRef format, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatTabStops(HandleRef format, int count, out float firstTabOffset, [In, Out] float[] tabStops);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetStringFormatTrimming(HandleRef format, out StringTrimming trimming);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetTextContrast(HandleRef graphics, out int textContrast);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetTextRenderingHint(HandleRef graphics, out TextRenderingHint textRenderingHint);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetTextureImage(HandleRef brush, out IntPtr image);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetTextureTransform(HandleRef brush, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetTextureWrapMode(HandleRef brush, out int wrapMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetVisibleClipBounds(HandleRef graphics, ref GPRECTF rect);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGetWorldTransform(HandleRef graphics, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipGraphicsClear(HandleRef graphics, int argb);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipImageForceValidation(HandleRef image);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipImageGetFrameCount(HandleRef image, ref Guid dimensionID, int[] count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipImageGetFrameDimensionsCount(HandleRef image, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipImageGetFrameDimensionsList(HandleRef image, IntPtr buffer, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipImageRotateFlip(HandleRef image, int rotateFlipType);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipImageSelectActiveFrame(HandleRef image, ref Guid dimensionID, int frameIndex);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipInvertMatrix(HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsClipEmpty(HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsEmptyRegion(HandleRef region, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsEqualRegion(HandleRef region, HandleRef region2, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsInfiniteRegion(HandleRef region, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsMatrixEqual(HandleRef matrix, HandleRef matrix2, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsMatrixIdentity(HandleRef matrix, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsMatrixInvertible(HandleRef matrix, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsOutlineVisiblePathPoint(HandleRef path, float x, float y, HandleRef pen, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsOutlineVisiblePathPointI(HandleRef path, int x, int y, HandleRef pen, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsStyleAvailable(HandleRef family, FontStyle style, out int isStyleAvailable);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisibleClipEmpty(HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisiblePathPoint(HandleRef path, float x, float y, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisiblePathPointI(HandleRef path, int x, int y, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisiblePoint(HandleRef graphics, float x, float y, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisiblePointI(HandleRef graphics, int x, int y, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisibleRect(HandleRef graphics, float x, float y, float width, float height, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisibleRectI(HandleRef graphics, int x, int y, int width, int height, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisibleRegionPoint(HandleRef region, float X, float Y, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisibleRegionPointI(HandleRef region, int X, int Y, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisibleRegionRect(HandleRef region, float X, float Y, float width, float height, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipIsVisibleRegionRectI(HandleRef region, int X, int Y, int width, int height, HandleRef graphics, out int boolean);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipLoadImageFromFile(string filename, out IntPtr image);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipLoadImageFromFileICM(string filename, out IntPtr image);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipLoadImageFromStream(UnsafeNativeMethods.IStream stream, out IntPtr image);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipLoadImageFromStreamICM(UnsafeNativeMethods.IStream stream, out IntPtr image);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern void GdiplusShutdown(HandleRef token);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int GdiplusStartup(out IntPtr token, ref StartupInput input, out StartupOutput output);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipMeasureCharacterRanges(HandleRef graphics, string textString, int length, HandleRef font, ref GPRECTF layoutRect, HandleRef stringFormat, int characterCount, [In, Out] IntPtr[] region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipMeasureString(HandleRef graphics, string textString, int length, HandleRef font, ref GPRECTF layoutRect, HandleRef stringFormat, [In, Out] ref GPRECTF boundingBox, out int codepointsFitted, out int linesFilled);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipMultiplyLineTransform(HandleRef brush, HandleRef matrix, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipMultiplyMatrix(HandleRef matrix, HandleRef matrix2, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipMultiplyPathGradientTransform(HandleRef brush, HandleRef matrix, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipMultiplyPenTransform(HandleRef brush, HandleRef matrix, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipMultiplyTextureTransform(HandleRef brush, HandleRef matrix, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipMultiplyWorldTransform(HandleRef graphics, HandleRef matrix, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipNewInstalledFontCollection(out IntPtr fontCollection);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipNewPrivateFontCollection(out IntPtr fontCollection);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterCopyData(HandleRef pathIter, out int resultCount, IntPtr memoryPts, [In, Out] byte[] types, int startIndex, int endIndex);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterEnumerate(HandleRef pathIter, out int resultCount, IntPtr memoryPts, [In, Out] byte[] types, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterGetCount(HandleRef pathIter, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterGetSubpathCount(HandleRef pathIter, out int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterHasCurve(HandleRef pathIter, out bool hasCurve);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterNextMarker(HandleRef pathIter, out int resultCount, out int startIndex, out int endIndex);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterNextMarkerPath(HandleRef pathIter, out int resultCount, HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterNextPathType(HandleRef pathIter, out int resultCount, out byte pathType, out int startIndex, out int endIndex);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterNextSubpath(HandleRef pathIter, out int resultCount, out int startIndex, out int endIndex, out bool isClosed);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterNextSubpathPath(HandleRef pathIter, out int resultCount, HandleRef path, out bool isClosed);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPathIterRewind(HandleRef pathIter);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPlayMetafileRecord(HandleRef graphics, EmfPlusRecordType recordType, int flags, int dataSize, byte[] data);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPrivateAddFontFile(HandleRef fontCollection, string filename);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipPrivateAddMemoryFont(HandleRef fontCollection, HandleRef memory, int length);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafile(HandleRef referenceHdc, int emfType, ref GPRECTF frameRect, int frameUnit, string description, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafile(HandleRef referenceHdc, int emfType, HandleRef pframeRect, int frameUnit, string description, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafileFileName(string fileName, HandleRef referenceHdc, int emfType, ref GPRECTF frameRect, int frameUnit, string description, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafileFileName(string fileName, HandleRef referenceHdc, int emfType, HandleRef pframeRect, int frameUnit, string description, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafileFileNameI(string fileName, HandleRef referenceHdc, int emfType, ref GPRECT frameRect, int frameUnit, string description, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafileI(HandleRef referenceHdc, int emfType, ref GPRECT frameRect, int frameUnit, string description, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafileStream(UnsafeNativeMethods.IStream stream, HandleRef referenceHdc, int emfType, ref GPRECTF frameRect, int frameUnit, string description, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafileStream(UnsafeNativeMethods.IStream stream, HandleRef referenceHdc, int emfType, HandleRef pframeRect, int frameUnit, string description, out IntPtr metafile);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRecordMetafileStreamI(UnsafeNativeMethods.IStream stream, HandleRef referenceHdc, int emfType, ref GPRECT frameRect, int frameUnit, string description, out IntPtr metafile);
            internal static int GdipReleaseDC(HandleRef graphics, HandleRef hdc)
            {
                if (IsShutdown)
                {
                    return 0;
                }
                return IntGdipReleaseDC(graphics, hdc);
            }

            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRemovePropertyItem(HandleRef image, int propid);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipResetClip(HandleRef graphics);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipResetLineTransform(HandleRef brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipResetPath(HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipResetPathGradientTransform(HandleRef brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipResetPenTransform(HandleRef brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipResetTextureTransform(HandleRef brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipResetWorldTransform(HandleRef graphics);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRestoreGraphics(HandleRef graphics, int state);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipReversePath(HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRotateLineTransform(HandleRef brush, float angle, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRotateMatrix(HandleRef matrix, float angle, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRotatePathGradientTransform(HandleRef brush, float angle, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRotatePenTransform(HandleRef brush, float angle, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRotateTextureTransform(HandleRef brush, float angle, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipRotateWorldTransform(HandleRef graphics, float angle, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSaveAdd(HandleRef image, HandleRef encoderParams);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSaveAddImage(HandleRef image, HandleRef newImage, HandleRef encoderParams);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSaveGraphics(HandleRef graphics, out int state);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSaveImageToFile(HandleRef image, string filename, ref Guid classId, HandleRef encoderParams);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSaveImageToStream(HandleRef image, UnsafeNativeMethods.IStream stream, ref Guid classId, HandleRef encoderParams);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipScaleLineTransform(HandleRef brush, float sx, float sy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipScaleMatrix(HandleRef matrix, float scaleX, float scaleY, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipScalePathGradientTransform(HandleRef brush, float sx, float sy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipScalePenTransform(HandleRef brush, float sx, float sy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipScaleTextureTransform(HandleRef brush, float sx, float sy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipScaleWorldTransform(HandleRef graphics, float sx, float sy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetAdjustableArrowCapFillState(HandleRef adjustableArrowCap, bool fillState);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetAdjustableArrowCapHeight(HandleRef adjustableArrowCap, float height);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetAdjustableArrowCapMiddleInset(HandleRef adjustableArrowCap, float middleInset);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetAdjustableArrowCapWidth(HandleRef adjustableArrowCap, float width);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetClipGraphics(HandleRef graphics, HandleRef srcgraphics, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetClipPath(HandleRef graphics, HandleRef path, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetClipRect(HandleRef graphics, float x, float y, float width, float height, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetClipRectI(HandleRef graphics, int x, int y, int width, int height, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetClipRegion(HandleRef graphics, HandleRef region, CombineMode mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetCompositingMode(HandleRef graphics, int compositeMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetCompositingQuality(HandleRef graphics, CompositingQuality quality);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetCustomLineCapBaseCap(HandleRef customCap, LineCap baseCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetCustomLineCapBaseInset(HandleRef customCap, float inset);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetCustomLineCapStrokeCaps(HandleRef customCap, LineCap startCap, LineCap endCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetCustomLineCapStrokeJoin(HandleRef customCap, LineJoin lineJoin);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetCustomLineCapWidthScale(HandleRef customCap, float widthScale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetEmpty(HandleRef region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesColorKeys(HandleRef imageattr, ColorAdjustType type, bool enableFlag, int colorLow, int colorHigh);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesColorMatrix(HandleRef imageattr, ColorAdjustType type, bool enableFlag, ColorMatrix colorMatrix, ColorMatrix grayMatrix, ColorMatrixFlag flags);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesGamma(HandleRef imageattr, ColorAdjustType type, bool enableFlag, float gamma);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesNoOp(HandleRef imageattr, ColorAdjustType type, bool enableFlag);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesOutputChannel(HandleRef imageattr, ColorAdjustType type, bool enableFlag, ColorChannelFlag flags);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesOutputChannelColorProfile(HandleRef imageattr, ColorAdjustType type, bool enableFlag, string colorProfileFilename);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesRemapTable(HandleRef imageattr, ColorAdjustType type, bool enableFlag, int mapSize, HandleRef map);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesThreshold(HandleRef imageattr, ColorAdjustType type, bool enableFlag, float threshold);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImageAttributesWrapMode(HandleRef imageattr, int wrapmode, int argb, bool clamp);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetImagePalette(HandleRef image, IntPtr palette);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetInfinite(HandleRef region);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetInterpolationMode(HandleRef graphics, int mode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetLineBlend(HandleRef brush, HandleRef blend, HandleRef positions, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetLineColors(HandleRef brush, int color1, int color2);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetLineGammaCorrection(HandleRef brush, bool useGammaCorrection);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetLineLinearBlend(HandleRef brush, float focus, float scale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetLinePresetBlend(HandleRef brush, HandleRef blend, HandleRef positions, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetLineSigmaBlend(HandleRef brush, float focus, float scale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetLineTransform(HandleRef brush, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetLineWrapMode(HandleRef brush, int wrapMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetMatrixElements(HandleRef matrix, float m11, float m12, float m21, float m22, float dx, float dy);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPageScale(HandleRef graphics, float scale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPageUnit(HandleRef graphics, int unit);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathFillMode(HandleRef path, int fillmode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientBlend(HandleRef brush, HandleRef blend, HandleRef positions, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientCenterColor(HandleRef brush, int color);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientCenterPoint(HandleRef brush, GPPOINTF point);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientFocusScales(HandleRef brush, float xScale, float yScale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientLinearBlend(HandleRef brush, float focus, float scale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientPresetBlend(HandleRef brush, HandleRef blend, HandleRef positions, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientSigmaBlend(HandleRef brush, float focus, float scale);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientSurroundColorsWithCount(HandleRef brush, int[] argb, ref int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientTransform(HandleRef brush, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathGradientWrapMode(HandleRef brush, int wrapmode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPathMarker(HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenBrushFill(HandleRef pen, HandleRef brush);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenColor(HandleRef pen, int argb);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenCompoundArray(HandleRef pen, float[] array, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenCustomEndCap(HandleRef pen, HandleRef customCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenCustomStartCap(HandleRef pen, HandleRef customCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenDashArray(HandleRef pen, HandleRef memorydash, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenDashCap197819(HandleRef pen, int dashCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenDashOffset(HandleRef pen, float dashoffset);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenDashStyle(HandleRef pen, int dashstyle);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenEndCap(HandleRef pen, int endCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenLineCap197819(HandleRef pen, int startCap, int endCap, int dashCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenLineJoin(HandleRef pen, int lineJoin);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenMiterLimit(HandleRef pen, float miterLimit);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenMode(HandleRef pen, PenAlignment penAlign);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenStartCap(HandleRef pen, int startCap);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenTransform(HandleRef pen, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPenWidth(HandleRef pen, float width);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPixelOffsetMode(HandleRef graphics, PixelOffsetMode pixelOffsetMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetPropertyItem(HandleRef image, PropertyItemInternal propitem);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetRenderingOrigin(HandleRef graphics, int x, int y);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetSmoothingMode(HandleRef graphics, SmoothingMode smoothingMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetSolidFillColor(HandleRef brush, int color);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetStringFormatAlign(HandleRef format, StringAlignment align);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetStringFormatDigitSubstitution(HandleRef format, int langID, StringDigitSubstitute sds);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetStringFormatFlags(HandleRef format, StringFormatFlags options);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetStringFormatHotkeyPrefix(HandleRef format, HotkeyPrefix hotkeyPrefix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetStringFormatLineAlign(HandleRef format, StringAlignment align);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetStringFormatMeasurableCharacterRanges(HandleRef format, int rangeCount, [In, Out] CharacterRange[] range);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetStringFormatTabStops(HandleRef format, float firstTabOffset, int count, float[] tabStops);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetStringFormatTrimming(HandleRef format, StringTrimming trimming);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetTextContrast(HandleRef graphics, int textContrast);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetTextRenderingHint(HandleRef graphics, TextRenderingHint textRenderingHint);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetTextureTransform(HandleRef brush, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetTextureWrapMode(HandleRef brush, int wrapMode);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipSetWorldTransform(HandleRef graphics, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipShearMatrix(HandleRef matrix, float shearX, float shearY, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipStartPathFigure(HandleRef path);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipStringFormatGetGenericDefault(out IntPtr format);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipStringFormatGetGenericTypographic(out IntPtr format);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTransformMatrixPoints(HandleRef matrix, HandleRef pts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTransformMatrixPointsI(HandleRef matrix, HandleRef pts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTransformPath(HandleRef path, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTransformPoints(HandleRef graphics, int destSpace, int srcSpace, IntPtr points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTransformPointsI(HandleRef graphics, int destSpace, int srcSpace, IntPtr points, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTransformRegion(HandleRef region, HandleRef matrix);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslateClip(HandleRef graphics, float dx, float dy);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslateLineTransform(HandleRef brush, float dx, float dy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslateMatrix(HandleRef matrix, float offsetX, float offsetY, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslatePathGradientTransform(HandleRef brush, float dx, float dy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslatePenTransform(HandleRef brush, float dx, float dy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslateRegion(HandleRef region, float dx, float dy);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslateRegionI(HandleRef region, int dx, int dy);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslateTextureTransform(HandleRef brush, float dx, float dy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipTranslateWorldTransform(HandleRef graphics, float dx, float dy, MatrixOrder order);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipVectorTransformMatrixPoints(HandleRef matrix, HandleRef pts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipVectorTransformMatrixPointsI(HandleRef matrix, HandleRef pts, int count);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipWarpPath(HandleRef path, HandleRef matrix, HandleRef points, int count, float srcX, float srcY, float srcWidth, float srcHeight, WarpMode warpMode, float flatness);
            [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern int GdipWidenPath(HandleRef path, HandleRef pen, HandleRef matrix, float flatness);
            [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            private static extern int GetCurrentProcessId();
            private static void Initialize()
            {
                if (!EnsureAtomInitialized())
                {
                    StartupOutput output;
                    StartupInput input = StartupInput.GetDefault();
                    int status = GdiplusStartup(out initToken, ref input, out output);
                    if (status != 0)
                    {
                        throw StatusException(status);
                    }
                }
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(SafeNativeMethods.Gdip.OnProcessExit);
            }

            [DllImport("gdiplus.dll", EntryPoint="GdipDeleteBrush", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeleteBrush(HandleRef brush);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeleteCustomLineCap", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeleteCustomLineCap(HandleRef customCap);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeleteFont", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeleteFont(HandleRef font);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeleteFontFamily", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeleteFontFamily(HandleRef fontFamily);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeleteGraphics", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeleteGraphics(HandleRef graphics);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeleteMatrix", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeleteMatrix(HandleRef matrix);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeletePath", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeletePath(HandleRef path);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeletePathIter", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeletePathIter(HandleRef pathIter);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeletePen", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeletePen(HandleRef Pen);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeletePrivateFontCollection", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeletePrivateFontCollection(out IntPtr fontCollection);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeleteRegion", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeleteRegion(HandleRef region);
            [DllImport("gdiplus.dll", EntryPoint="GdipDeleteStringFormat", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDeleteStringFormat(HandleRef format);
            [DllImport("gdiplus.dll", EntryPoint="GdipDisposeImage", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDisposeImage(HandleRef image);
            [DllImport("gdiplus.dll", EntryPoint="GdipDisposeImageAttributes", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipDisposeImageAttributes(HandleRef imageattr);
            [DllImport("gdiplus.dll", EntryPoint="GdipReleaseDC", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern int IntGdipReleaseDC(HandleRef graphics, HandleRef hdc);
            [PrePrepareMethod]
            private static void OnProcessExit(object sender, EventArgs e)
            {
                Shutdown();
            }

            private static void Shutdown()
            {
                DestroyAtom();
                if (!IsShutdown)
                {
                    Thread.SetData(Thread.GetNamedDataSlot("system.drawing.threaddata"), null);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    if (initToken != IntPtr.Zero)
                    {
                        GdiplusShutdown(new HandleRef(null, initToken));
                    }
                }
            }

            internal static Exception StatusException(int status)
            {
                switch (status)
                {
                    case 1:
                        return new ExternalException(System.Drawing.SR.GetString("GdiplusGenericError"), -2147467259);

                    case 2:
                        return new ArgumentException(System.Drawing.SR.GetString("GdiplusInvalidParameter"));

                    case 3:
                        return new OutOfMemoryException(System.Drawing.SR.GetString("GdiplusOutOfMemory"));

                    case 4:
                        return new InvalidOperationException(System.Drawing.SR.GetString("GdiplusObjectBusy"));

                    case 5:
                        return new OutOfMemoryException(System.Drawing.SR.GetString("GdiplusInsufficientBuffer"));

                    case 6:
                        return new NotImplementedException(System.Drawing.SR.GetString("GdiplusNotImplemented"));

                    case 7:
                        return new ExternalException(System.Drawing.SR.GetString("GdiplusGenericError"), -2147467259);

                    case 8:
                        return new InvalidOperationException(System.Drawing.SR.GetString("GdiplusWrongState"));

                    case 9:
                        return new ExternalException(System.Drawing.SR.GetString("GdiplusAborted"), -2147467260);

                    case 10:
                        return new FileNotFoundException(System.Drawing.SR.GetString("GdiplusFileNotFound"));

                    case 11:
                        return new OverflowException(System.Drawing.SR.GetString("GdiplusOverflow"));

                    case 12:
                        return new ExternalException(System.Drawing.SR.GetString("GdiplusAccessDenied"), -2147024891);

                    case 13:
                        return new ArgumentException(System.Drawing.SR.GetString("GdiplusUnknownImageFormat"));

                    case 14:
                        return new ArgumentException(System.Drawing.SR.GetString("GdiplusFontFamilyNotFound", new object[] { "?" }));

                    case 15:
                        return new ArgumentException(System.Drawing.SR.GetString("GdiplusFontStyleNotFound", new object[] { "?", "?" }));

                    case 0x10:
                        return new ArgumentException(System.Drawing.SR.GetString("GdiplusNotTrueTypeFont_NoName"));

                    case 0x11:
                        return new ExternalException(System.Drawing.SR.GetString("GdiplusUnsupportedGdiplusVersion"), -2147467259);

                    case 0x12:
                        return new ExternalException(System.Drawing.SR.GetString("GdiplusNotInitialized"), -2147467259);

                    case 0x13:
                        return new ArgumentException(System.Drawing.SR.GetString("GdiplusPropertyNotFoundError"));

                    case 20:
                        return new ArgumentException(System.Drawing.SR.GetString("GdiplusPropertyNotSupportedError"));
                }
                return new ExternalException(System.Drawing.SR.GetString("GdiplusUnknown"), -2147418113);
            }

            private static string AtomName
            {
                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get
                {
                    if (atomName == null)
                    {
                        atomName = VersioningHelper.MakeVersionSafeName("GDI+Atom", ResourceScope.Machine, ResourceScope.AppDomain);
                    }
                    return atomName;
                }
            }

            private static bool IsShutdown
            {
                get
                {
                    return (FindAtom(AtomName) == 0);
                }
            }

            internal static IDictionary ThreadData
            {
                get
                {
                    LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot("system.drawing.threaddata");
                    IDictionary data = (IDictionary) Thread.GetData(namedDataSlot);
                    if (data == null)
                    {
                        data = new Hashtable();
                        Thread.SetData(namedDataSlot, data);
                    }
                    return data;
                }
            }

            private enum DebugEventLevel
            {
                Fatal,
                Warning
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct StartupInput
            {
                public int GdiplusVersion;
                public IntPtr DebugEventCallback;
                public bool SuppressBackgroundThread;
                public bool SuppressExternalCodecs;
                public static SafeNativeMethods.Gdip.StartupInput GetDefault()
                {
                    return new SafeNativeMethods.Gdip.StartupInput { GdiplusVersion = 1, SuppressBackgroundThread = false, SuppressExternalCodecs = false };
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct StartupOutput
            {
                public IntPtr hook;
                public IntPtr unhook;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack=2)]
        public struct ICONDIR
        {
            public short idReserved;
            public short idType;
            public short idCount;
            public SafeNativeMethods.ICONDIRENTRY idEntries;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONDIRENTRY
        {
            public byte bWidth;
            public byte bHeight;
            public byte bColorCount;
            public byte bReserved;
            public short wPlanes;
            public short wBitCount;
            public int dwBytesInRes;
            public int dwImageOffset;
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

        [ComImport, Guid("7BF80980-BF32-101A-8BBB-00AA00300CAB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPicture
        {
            [SuppressUnmanagedCodeSecurity]
            IntPtr GetHandle();
            [SuppressUnmanagedCodeSecurity]
            IntPtr GetHPal();
            [return: MarshalAs(UnmanagedType.I2)]
            [SuppressUnmanagedCodeSecurity]
            short GetPictureType();
            [SuppressUnmanagedCodeSecurity]
            int GetWidth();
            [SuppressUnmanagedCodeSecurity]
            int GetHeight();
            [SuppressUnmanagedCodeSecurity]
            void Render();
            [SuppressUnmanagedCodeSecurity]
            void SetHPal([In] IntPtr phpal);
            [SuppressUnmanagedCodeSecurity]
            IntPtr GetCurDC();
            [SuppressUnmanagedCodeSecurity]
            void SelectPicture([In] IntPtr hdcIn, [Out, MarshalAs(UnmanagedType.LPArray)] int[] phdcOut, [Out, MarshalAs(UnmanagedType.LPArray)] int[] phbmpOut);
            [return: MarshalAs(UnmanagedType.Bool)]
            [SuppressUnmanagedCodeSecurity]
            bool GetKeepOriginalFormat();
            [SuppressUnmanagedCodeSecurity]
            void SetKeepOriginalFormat([In, MarshalAs(UnmanagedType.Bool)] bool pfkeep);
            [SuppressUnmanagedCodeSecurity]
            void PictureChanged();
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int SaveAsFile([In, MarshalAs(UnmanagedType.Interface)] UnsafeNativeMethods.IStream pstm, [In] int fSaveMemCopy, out int pcbSize);
            [SuppressUnmanagedCodeSecurity]
            int GetAttributes();
            [SuppressUnmanagedCodeSecurity]
            void SetHdc([In] IntPtr hdc);
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

            public LOGFONT(SafeNativeMethods.LOGFONT lf)
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
        public struct OBJECTHEADER
        {
            public short signature;
            public short headersize;
            public short objectType;
            public short nameLen;
            public short classLen;
            public short nameOffset;
            public short classOffset;
            public short width;
            public short height;
            public IntPtr pInfo;
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

        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESC
        {
            internal int cbSizeOfStruct;
            public int picType;
            internal IntPtr union1;
            internal int union2;
            internal int union3;
            public static SafeNativeMethods.PICTDESC CreateIconPICTDESC(IntPtr hicon)
            {
                return new SafeNativeMethods.PICTDESC { cbSizeOfStruct = 12, picType = 3, union1 = hicon };
            }

            public virtual IntPtr GetHandle()
            {
                return this.union1;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class PRINTDLG
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;
            public int Flags;
            public short nFromPage;
            public short nToPage;
            public short nMinPage;
            public short nMaxPage;
            public short nCopies;
            public IntPtr hInstance;
            public IntPtr lCustData;
            public IntPtr lpfnPrintHook;
            public IntPtr lpfnSetupHook;
            public string lpPrintTemplateName;
            public string lpSetupTemplateName;
            public IntPtr hPrintTemplate;
            public IntPtr hSetupTemplate;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        public class PRINTDLGX86
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;
            public int Flags;
            public short nFromPage;
            public short nToPage;
            public short nMinPage;
            public short nMaxPage;
            public short nCopies;
            public IntPtr hInstance;
            public IntPtr lCustData;
            public IntPtr lpfnPrintHook;
            public IntPtr lpfnSetupHook;
            public string lpPrintTemplateName;
            public string lpSetupTemplateName;
            public IntPtr hPrintTemplate;
            public IntPtr hSetupTemplate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public class StreamConsts
        {
            public const int LOCK_EXCLUSIVE = 2;
            public const int LOCK_ONLYONCE = 4;
            public const int LOCK_WRITE = 1;
            public const int STATFLAG_DEFAULT = 0;
            public const int STATFLAG_NONAME = 1;
            public const int STATFLAG_NOOPEN = 2;
            public const int STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 4;
            public const int STGC_DEFAULT = 0;
            public const int STGC_ONLYIFCURRENT = 2;
            public const int STGC_OVERWRITE = 1;
            public const int STREAM_SEEK_CUR = 1;
            public const int STREAM_SEEK_END = 2;
            public const int STREAM_SEEK_SET = 0;
        }

        public enum StructFormat
        {
            Ansi = 1,
            Auto = 3,
            Unicode = 2
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

        internal enum Win32SystemColors
        {
            ActiveBorder = 10,
            ActiveCaption = 2,
            ActiveCaptionText = 9,
            AppWorkspace = 12,
            ButtonFace = 15,
            ButtonHighlight = 20,
            ButtonShadow = 0x10,
            Control = 15,
            ControlDark = 0x10,
            ControlDarkDark = 0x15,
            ControlLight = 0x16,
            ControlLightLight = 20,
            ControlText = 0x12,
            Desktop = 1,
            GradientActiveCaption = 0x1b,
            GradientInactiveCaption = 0x1c,
            GrayText = 0x11,
            Highlight = 13,
            HighlightText = 14,
            HotTrack = 0x1a,
            InactiveBorder = 11,
            InactiveCaption = 3,
            InactiveCaptionText = 0x13,
            Info = 0x18,
            InfoText = 0x17,
            Menu = 4,
            MenuBar = 30,
            MenuHighlight = 0x1d,
            MenuText = 7,
            ScrollBar = 0,
            Window = 5,
            WindowFrame = 6,
            WindowText = 8
        }
    }
}

