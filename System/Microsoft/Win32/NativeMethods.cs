namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal static class NativeMethods
    {
        internal const int ALL_EVENTS = 0x1fb;
        public const int BACKWARDS_READ = 8;
        internal const int CE_BREAK = 0x10;
        internal const int CE_FRAME = 8;
        internal const int CE_OVERRUN = 2;
        internal const int CE_PARITY = 4;
        internal const int CE_RXOVER = 1;
        internal const int CE_TXFULL = 0x100;
        internal const int CLRDTR = 6;
        internal const int CLRRTS = 4;
        public const int COLOR_WINDOW = 5;
        public const int CREATE_ALWAYS = 2;
        public const int CREATE_NO_WINDOW = 0x8000000;
        public const int CREATE_SUSPENDED = 4;
        public const int CREATE_UNICODE_ENVIRONMENT = 0x400;
        public const int CTRL_BREAK_EVENT = 1;
        public const int CTRL_C_EVENT = 0;
        public const int CTRL_CLOSE_EVENT = 2;
        public const int CTRL_LOGOFF_EVENT = 5;
        public const int CTRL_SHUTDOWN_EVENT = 6;
        public const int DEFAULT_GUI_FONT = 0x11;
        internal const byte DEFAULTXOFFCHAR = 0x13;
        internal const byte DEFAULTXONCHAR = 0x11;
        internal const int DTR_CONTROL_DISABLE = 0;
        internal const int DTR_CONTROL_ENABLE = 1;
        internal const int DTR_CONTROL_HANDSHAKE = 2;
        public const int DUPLICATE_CLOSE_SOURCE = 1;
        public const int DUPLICATE_SAME_ACCESS = 2;
        public const int DWORD_SIZE = 4;
        public const int E_ABORT = -2147467260;
        public const int E_NOTIMPL = -2147467263;
        public const int ENDSESSION_LOGOFF = -2147483648;
        internal const byte EOFCHAR = 0x1a;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_ALREADY_EXISTS = 0xb7;
        public const int ERROR_BAD_EXE_FORMAT = 0xc1;
        public const int ERROR_BROKEN_PIPE = 0x6d;
        public const int ERROR_BUSY = 170;
        public const int ERROR_CANCELLED = 0x4c7;
        public const int ERROR_CLASS_ALREADY_EXISTS = 0x582;
        public const int ERROR_COUNTER_TIMEOUT = 0x461;
        public const int ERROR_DDE_FAIL = 0x484;
        public const int ERROR_DLL_NOT_FOUND = 0x485;
        public const int ERROR_EVENTLOG_FILE_CHANGED = 0x5df;
        public const int ERROR_EXE_MACHINE_TYPE_MISMATCH = 0xd8;
        public const int ERROR_FILE_EXISTS = 80;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_FILENAME_EXCED_RANGE = 0xce;
        public const int ERROR_HANDLE_EOF = 0x26;
        public const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_INVALID_NAME = 0x7b;
        public const int ERROR_INVALID_PARAMETER = 0x57;
        public const int ERROR_IO_INCOMPLETE = 0x3e4;
        public const int ERROR_IO_PENDING = 0x3e5;
        public const int ERROR_LOCK_FAILED = 0xa7;
        public const int ERROR_MORE_DATA = 0xea;
        public const int ERROR_NO_ASSOCIATION = 0x483;
        public const int ERROR_NO_DATA = 0xe8;
        public const int ERROR_NONE_MAPPED = 0x534;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;
        public const int ERROR_NOT_READY = 0x15;
        public const int ERROR_OPERATION_ABORTED = 0x3e3;
        public const int ERROR_PARTIAL_COPY = 0x12b;
        public const int ERROR_PATH_NOT_FOUND = 3;
        public const int ERROR_PROC_NOT_FOUND = 0x7f;
        public const int ERROR_SHARING_VIOLATION = 0x20;
        public const int ERROR_SUCCESS = 0;
        internal const int EV_BREAK = 0x40;
        internal const int EV_CTS = 8;
        internal const int EV_DSR = 0x10;
        internal const int EV_ERR = 0x80;
        internal const int EV_RING = 0x100;
        internal const int EV_RLSD = 0x20;
        internal const int EV_RXCHAR = 1;
        internal const int EV_RXFLAG = 2;
        internal const int EVENPARITY = 2;
        internal const int FABORTONOERROR = 14;
        internal const int FBINARY = 0;
        internal const int FDSRSENSITIVITY = 6;
        internal const int FDTRCONTROL = 4;
        internal const int FDUMMY2 = 15;
        internal const int FERRORCHAR = 10;
        public const int FILE_ATTRIBUTE_NORMAL = 0x80;
        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int FILE_MAP_READ = 4;
        public const int FILE_MAP_WRITE = 2;
        public const int FILE_SHARE_DELETE = 4;
        public const int FILE_SHARE_READ = 1;
        public const int FILE_SHARE_WRITE = 2;
        internal const int FINX = 9;
        internal const int FNULL = 11;
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xff;
        public const int FORWARDS_READ = 4;
        internal const int FOUTX = 8;
        internal const int FOUTXCTSFLOW = 2;
        internal const int FOUTXDSRFLOW = 3;
        internal const int FPARITY = 1;
        internal const int FRTSCONTROL = 12;
        internal const int FTXCONTINUEONXOFF = 7;
        public const int GCL_WNDPROC = -24;
        public const int GENERIC_ALL = 0x10000000;
        public const int GENERIC_EXECUTE = 0x20000000;
        public const int GENERIC_READ = -2147483648;
        public const int GENERIC_WRITE = 0x40000000;
        public const int GHND = 0x42;
        public const int GMEM_DDESHARE = 0x2000;
        public const int GMEM_DISCARDABLE = 0x100;
        public const int GMEM_DISCARDED = 0x4000;
        public const int GMEM_FIXED = 0;
        public const int GMEM_INVALID_HANDLE = 0x8000;
        public const int GMEM_LOCKCOUNT = 0xff;
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
        public const int GPTR = 0x40;
        public const int GW_OWNER = 4;
        public const int GWL_STYLE = -16;
        public const int GWL_WNDPROC = -4;
        public static readonly IntPtr HKEY_LOCAL_MACHINE = ((IntPtr) (-2147483646));
        public const int HKEY_PERFORMANCE_DATA = -2147483644;
        public const int IMPERSONATION_LEVEL_SecurityAnonymous = 0;
        public const int IMPERSONATION_LEVEL_SecurityDelegation = 3;
        public const int IMPERSONATION_LEVEL_SecurityIdentification = 1;
        public const int IMPERSONATION_LEVEL_SecurityImpersonation = 2;
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public const int KEY_ENUMERATE_SUB_KEYS = 8;
        public const int KEY_NOTIFY = 0x10;
        public const int KEY_QUERY_VALUE = 1;
        public const int KEY_READ = 0x20019;
        public const int LARGE_INTEGER_SIZE = 8;
        public const int LOAD_LIBRARY_AS_DATAFILE = 2;
        public const int LOAD_WITH_ALTERED_SEARCH_PATH = 8;
        public const int LOGON32_LOGON_BATCH = 4;
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;
        internal const int MARKPARITY = 3;
        public const int MAX_PATH = 260;
        internal const int MAXDWORD = -1;
        public const int MOVEFILE_REPLACE_EXISTING = 1;
        internal const int MS_CTS_ON = 0x10;
        internal const int MS_DSR_ON = 0x20;
        internal const int MS_RING_ON = 0x40;
        internal const int MS_RLSD_ON = 0x80;
        public const int MWMO_INPUTAVAILABLE = 4;
        internal const int NOPARITY = 0;
        public const int NOTIFY_FOR_THIS_SESSION = 0;
        public const int NtPerfCounterSizeDword = 0;
        public const int NtPerfCounterSizeLarge = 0x100;
        public const int NtQueryProcessBasicInfo = 0;
        public const int NtQuerySystemProcessInformation = 5;
        public static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        internal const int ODDPARITY = 1;
        internal const byte ONE5STOPBITS = 1;
        internal const byte ONESTOPBIT = 0;
        public const int PAGE_READWRITE = 4;
        public const int PBT_APMBATTERYLOW = 9;
        public const int PBT_APMOEMEVENT = 11;
        public const int PBT_APMPOWERSTATUSCHANGE = 10;
        public const int PBT_APMQUERYSTANDBY = 1;
        public const int PBT_APMQUERYSTANDBYFAILED = 3;
        public const int PBT_APMQUERYSUSPEND = 0;
        public const int PBT_APMQUERYSUSPENDFAILED = 2;
        public const int PBT_APMRESUMECRITICAL = 6;
        public const int PBT_APMRESUMESTANDBY = 8;
        public const int PBT_APMRESUMESUSPEND = 7;
        public const int PBT_APMSTANDBY = 5;
        public const int PBT_APMSUSPEND = 4;
        public const int PDH_CALC_NEGATIVE_DENOMINATOR = -2147481642;
        public const int PDH_CALC_NEGATIVE_VALUE = -2147481640;
        public const uint PDH_FMT_DOUBLE = 0x200;
        public const uint PDH_FMT_NOCAP100 = 0x8000;
        public const uint PDH_FMT_NOSCALE = 0x1000;
        public const int PDH_NO_DATA = -2147481643;
        public const int PERF_100NSEC_MULTI_TIMER = 0x22510500;
        public const int PERF_100NSEC_MULTI_TIMER_INV = 0x23510500;
        public const int PERF_100NSEC_TIMER = 0x20510500;
        public const int PERF_100NSEC_TIMER_INV = 0x21510500;
        public const int PERF_AVERAGE_BASE = 0x40030402;
        public const int PERF_AVERAGE_BULK = 0x40020500;
        public const int PERF_AVERAGE_TIMER = 0x30020400;
        public const int PERF_COUNTER_100NS_QUEUELEN_TYPE = 0x550500;
        public const int PERF_COUNTER_BASE = 0x30000;
        public const int PERF_COUNTER_BULK_COUNT = 0x10410500;
        public const int PERF_COUNTER_COUNTER = 0x10410400;
        public const int PERF_COUNTER_DELTA = 0x400400;
        public const int PERF_COUNTER_ELAPSED = 0x40000;
        public const int PERF_COUNTER_FRACTION = 0x20000;
        public const int PERF_COUNTER_HISTOGRAM = 0x60000;
        public const int PERF_COUNTER_LARGE_DELTA = 0x400500;
        public const int PERF_COUNTER_LARGE_QUEUELEN_TYPE = 0x450500;
        public const int PERF_COUNTER_LARGE_RAWCOUNT = 0x10100;
        public const int PERF_COUNTER_LARGE_RAWCOUNT_HEX = 0x100;
        public const int PERF_COUNTER_MULTI_BASE = 0x42030500;
        public const int PERF_COUNTER_MULTI_TIMER = 0x22410500;
        public const int PERF_COUNTER_MULTI_TIMER_INV = 0x23410500;
        public const int PERF_COUNTER_NODATA = 0x40000200;
        public const int PERF_COUNTER_OBJ_TIME_QUEUELEN_TYPE = 0x650500;
        public const int PERF_COUNTER_PRECISION = 0x70000;
        public const int PERF_COUNTER_QUEUELEN = 0x50000;
        public const int PERF_COUNTER_QUEUELEN_TYPE = 0x450400;
        public const int PERF_COUNTER_RATE = 0x10000;
        public const int PERF_COUNTER_RAWCOUNT = 0x10000;
        public const int PERF_COUNTER_RAWCOUNT_HEX = 0;
        public const int PERF_COUNTER_TEXT = 0xb00;
        public const int PERF_COUNTER_TIMER = 0x20410500;
        public const int PERF_COUNTER_TIMER_INV = 0x21410500;
        public const int PERF_COUNTER_VALUE = 0;
        public const int PERF_DELTA_BASE = 0x800000;
        public const int PERF_DELTA_COUNTER = 0x400000;
        public const int PERF_DETAIL_ADVANCED = 200;
        public const int PERF_DETAIL_EXPERT = 300;
        public const int PERF_DETAIL_NOVICE = 100;
        public const int PERF_DETAIL_WIZARD = 400;
        public const int PERF_DISPLAY_NO_SUFFIX = 0;
        public const int PERF_DISPLAY_NOSHOW = 0x40000000;
        public const int PERF_DISPLAY_PER_SEC = 0x10000000;
        public const int PERF_DISPLAY_PERCENT = 0x20000000;
        public const int PERF_DISPLAY_SECONDS = 0x30000000;
        public const int PERF_ELAPSED_TIME = 0x30240500;
        public const int PERF_INVERSE_COUNTER = 0x1000000;
        public const int PERF_LARGE_RAW_BASE = 0x40030500;
        public const int PERF_LARGE_RAW_FRACTION = 0x20020500;
        public const int PERF_MULTI_COUNTER = 0x2000000;
        public const int PERF_NO_INSTANCES = -1;
        public const int PERF_NO_UNIQUE_ID = -1;
        public const int PERF_NUMBER_DEC_1000 = 0x20000;
        public const int PERF_NUMBER_DECIMAL = 0x10000;
        public const int PERF_NUMBER_HEX = 0;
        public const int PERF_OBJ_TIME_TIME = 0x20610500;
        public const int PERF_OBJ_TIME_TIMER = 0x20610500;
        public const int PERF_OBJECT_TIMER = 0x200000;
        public const int PERF_PRECISION_100NS_TIMER = 0x20570500;
        public const int PERF_PRECISION_OBJECT_TIMER = 0x20670500;
        public const int PERF_PRECISION_SYSTEM_TIMER = 0x20470500;
        public const int PERF_RAW_BASE = 0x40030403;
        public const int PERF_RAW_FRACTION = 0x20020400;
        public const int PERF_SAMPLE_BASE = 0x40030401;
        public const int PERF_SAMPLE_COUNTER = 0x410400;
        public const int PERF_SAMPLE_FRACTION = 0x20c20400;
        public const int PERF_SIZE_DWORD = 0;
        public const int PERF_SIZE_LARGE = 0x100;
        public const int PERF_SIZE_VARIABLE_LEN = 0x300;
        public const int PERF_SIZE_ZERO = 0x200;
        public const int PERF_TEXT_ASCII = 0x10000;
        public const int PERF_TEXT_UNICODE = 0;
        public const int PERF_TIMER_100NS = 0x100000;
        public const int PERF_TIMER_TICK = 0;
        public const int PERF_TYPE_COUNTER = 0x400;
        public const int PERF_TYPE_NUMBER = 0;
        public const int PERF_TYPE_TEXT = 0x800;
        public const int PERF_TYPE_ZERO = 0xc00;
        public const int PIPE_ACCESS_DUPLEX = 3;
        public const int PIPE_ACCESS_INBOUND = 1;
        public const int PIPE_ACCESS_OUTBOUND = 2;
        public const int PIPE_NOWAIT = 1;
        public const int PIPE_READMODE_BYTE = 0;
        public const int PIPE_READMODE_MESSAGE = 2;
        public const int PIPE_SINGLE_INSTANCES = 1;
        public const int PIPE_TYPE_BYTE = 0;
        public const int PIPE_TYPE_MESSAGE = 4;
        public const int PIPE_UNLIMITED_INSTANCES = 0xff;
        public const int PIPE_WAIT = 0;
        public const int PM_REMOVE = 1;
        public const int PROCESS_ALL_ACCESS = 0x1f0fff;
        public const int PROCESS_CREATE_PROCESS = 0x80;
        public const int PROCESS_CREATE_THREAD = 2;
        public const int PROCESS_DUP_HANDLE = 0x40;
        public const int PROCESS_QUERY_INFORMATION = 0x400;
        public const int PROCESS_SET_INFORMATION = 0x200;
        public const int PROCESS_SET_QUOTA = 0x100;
        public const int PROCESS_SET_SESSIONID = 4;
        public const int PROCESS_TERMINATE = 1;
        public const int PROCESS_VM_OPERATION = 8;
        public const int PROCESS_VM_READ = 0x10;
        public const int PROCESS_VM_WRITE = 0x20;
        internal const int PURGE_RXABORT = 2;
        internal const int PURGE_RXCLEAR = 8;
        internal const int PURGE_TXABORT = 1;
        internal const int PURGE_TXCLEAR = 4;
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
        public const int READ_CONTROL = 0x20000;
        public const int REG_BINARY = 3;
        public const int REG_MULTI_SZ = 7;
        public const int RPC_S_CALL_FAILED = 0x6be;
        public const int RPC_S_SERVER_UNAVAILABLE = 0x6ba;
        internal const int RTS_CONTROL_DISABLE = 0;
        internal const int RTS_CONTROL_ENABLE = 1;
        internal const int RTS_CONTROL_HANDSHAKE = 2;
        internal const int RTS_CONTROL_TOGGLE = 3;
        public const int S_OK = 0;
        internal const int SDDL_REVISION_1 = 1;
        public const int SE_ERR_ACCESSDENIED = 5;
        public const int SE_ERR_ASSOCINCOMPLETE = 0x1b;
        public const int SE_ERR_DDEBUSY = 30;
        public const int SE_ERR_DDEFAIL = 0x1d;
        public const int SE_ERR_DDETIMEOUT = 0x1c;
        public const int SE_ERR_DLLNOTFOUND = 0x20;
        public const int SE_ERR_FNF = 2;
        public const int SE_ERR_NOASSOC = 0x1f;
        public const int SE_ERR_OOM = 8;
        public const int SE_ERR_PNF = 3;
        public const int SE_ERR_SHARE = 0x1a;
        public const int SE_PRIVILEGE_ENABLED = 2;
        public const int SECURITY_DESCRIPTOR_REVISION = 1;
        public const int SEE_MASK_ASYNCOK = 0x100000;
        public const int SEE_MASK_CLASSKEY = 3;
        public const int SEE_MASK_CLASSNAME = 1;
        public const int SEE_MASK_CONNECTNETDRV = 0x80;
        public const int SEE_MASK_DOENVSUBST = 0x200;
        public const int SEE_MASK_FLAG_DDEWAIT = 0x100;
        public const int SEE_MASK_FLAG_NO_UI = 0x400;
        public const int SEE_MASK_HOTKEY = 0x20;
        public const int SEE_MASK_ICON = 0x10;
        public const int SEE_MASK_IDLIST = 4;
        public const int SEE_MASK_INVOKEIDLIST = 12;
        public const int SEE_MASK_NO_CONSOLE = 0x8000;
        public const int SEE_MASK_NOCLOSEPROCESS = 0x40;
        public const int SEE_MASK_UNICODE = 0x4000;
        public const int SEEK_READ = 2;
        internal const int SETDTR = 5;
        internal const int SETRTS = 3;
        public const int SHGFI_TYPENAME = 0x400;
        public const int SHGFI_USEFILEATTRIBUTES = 0x10;
        public const int SM_CYSCREEN = 1;
        public const int SMTO_ABORTIFHUNG = 2;
        internal const int SPACEPARITY = 4;
        public const int SPI_GETACCESSTIMEOUT = 60;
        public const int SPI_GETACTIVEWINDOWTRACKING = 0x1000;
        public const int SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002;
        public const int SPI_GETACTIVEWNDTRKZORDER = 0x100c;
        public const int SPI_GETANIMATION = 0x48;
        public const int SPI_GETBEEP = 1;
        public const int SPI_GETBORDER = 5;
        public const int SPI_GETCARETWIDTH = 0x2006;
        public const int SPI_GETCOMBOBOXANIMATION = 0x1004;
        public const int SPI_GETCURSORSHADOW = 0x101a;
        public const int SPI_GETDEFAULTINPUTLANG = 0x59;
        public const int SPI_GETDESKWALLPAPER = 0x73;
        public const int SPI_GETDRAGFULLWINDOWS = 0x26;
        public const int SPI_GETFASTTASKSWITCH = 0x23;
        public const int SPI_GETFILTERKEYS = 50;
        public const int SPI_GETFONTSMOOTHING = 0x4a;
        public const int SPI_GETFOREGROUNDFLASHCOUNT = 0x2004;
        public const int SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        public const int SPI_GETGRADIENTCAPTIONS = 0x1008;
        public const int SPI_GETGRIDGRANULARITY = 0x12;
        public const int SPI_GETHIGHCONTRAST = 0x42;
        public const int SPI_GETHOTTRACKING = 0x100e;
        public const int SPI_GETICONMETRICS = 0x2d;
        public const int SPI_GETICONTITLELOGFONT = 0x1f;
        public const int SPI_GETICONTITLEWRAP = 0x19;
        public const int SPI_GETKEYBOARDCUES = 0x100a;
        public const int SPI_GETKEYBOARDDELAY = 0x16;
        public const int SPI_GETKEYBOARDPREF = 0x44;
        public const int SPI_GETKEYBOARDSPEED = 10;
        public const int SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006;
        public const int SPI_GETLOWPOWERACTIVE = 0x53;
        public const int SPI_GETLOWPOWERTIMEOUT = 0x4f;
        public const int SPI_GETMENUANIMATION = 0x1002;
        public const int SPI_GETMENUDROPALIGNMENT = 0x1b;
        public const int SPI_GETMENUFADE = 0x1012;
        public const int SPI_GETMENUSHOWDELAY = 0x6a;
        public const int SPI_GETMENUUNDERLINES = 0x100a;
        public const int SPI_GETMINIMIZEDMETRICS = 0x2b;
        public const int SPI_GETMOUSE = 3;
        public const int SPI_GETMOUSEHOVERHEIGHT = 100;
        public const int SPI_GETMOUSEHOVERTIME = 0x66;
        public const int SPI_GETMOUSEHOVERWIDTH = 0x62;
        public const int SPI_GETMOUSEKEYS = 0x36;
        public const int SPI_GETMOUSESPEED = 0x70;
        public const int SPI_GETMOUSETRAILS = 0x5e;
        public const int SPI_GETNONCLIENTMETRICS = 0x29;
        public const int SPI_GETPOWEROFFACTIVE = 0x54;
        public const int SPI_GETPOWEROFFTIMEOUT = 80;
        public const int SPI_GETSCREENREADER = 70;
        public const int SPI_GETSCREENSAVEACTIVE = 0x10;
        public const int SPI_GETSCREENSAVERRUNNING = 0x72;
        public const int SPI_GETSCREENSAVETIMEOUT = 14;
        public const int SPI_GETSELECTIONFADE = 0x1014;
        public const int SPI_GETSERIALKEYS = 0x3e;
        public const int SPI_GETSHOWIMEUI = 110;
        public const int SPI_GETSHOWSOUNDS = 0x38;
        public const int SPI_GETSNAPTODEFBUTTON = 0x5f;
        public const int SPI_GETSOUNDSENTRY = 0x40;
        public const int SPI_GETSTICKYKEYS = 0x3a;
        public const int SPI_GETTOGGLEKEYS = 0x34;
        public const int SPI_GETTOOLTIPANIMATION = 0x1016;
        public const int SPI_GETTOOLTIPFADE = 0x1018;
        public const int SPI_GETUIEFFECTS = 0x103e;
        public const int SPI_GETWHEELSCROLLLINES = 0x68;
        public const int SPI_GETWINDOWSEXTENSION = 0x5c;
        public const int SPI_GETWORKAREA = 0x30;
        public const int SPI_ICONHORIZONTALSPACING = 13;
        public const int SPI_ICONVERTICALSPACING = 0x18;
        public const int SPI_LANGDRIVER = 12;
        public const int SPI_SCREENSAVERRUNNING = 0x61;
        public const int SPI_SETACCESSTIMEOUT = 0x3d;
        public const int SPI_SETACTIVEWINDOWTRACKING = 0x1001;
        public const int SPI_SETACTIVEWNDTRKTIMEOUT = 0x2003;
        public const int SPI_SETACTIVEWNDTRKZORDER = 0x100d;
        public const int SPI_SETANIMATION = 0x49;
        public const int SPI_SETBEEP = 2;
        public const int SPI_SETBORDER = 6;
        public const int SPI_SETCARETWIDTH = 0x2007;
        public const int SPI_SETCOMBOBOXANIMATION = 0x1005;
        public const int SPI_SETCURSORS = 0x57;
        public const int SPI_SETCURSORSHADOW = 0x101b;
        public const int SPI_SETDEFAULTINPUTLANG = 90;
        public const int SPI_SETDESKPATTERN = 0x15;
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int SPI_SETDOUBLECLICKTIME = 0x20;
        public const int SPI_SETDOUBLECLKHEIGHT = 30;
        public const int SPI_SETDOUBLECLKWIDTH = 0x1d;
        public const int SPI_SETDRAGFULLWINDOWS = 0x25;
        public const int SPI_SETDRAGHEIGHT = 0x4d;
        public const int SPI_SETDRAGWIDTH = 0x4c;
        public const int SPI_SETFASTTASKSWITCH = 0x24;
        public const int SPI_SETFILTERKEYS = 0x33;
        public const int SPI_SETFONTSMOOTHING = 0x4b;
        public const int SPI_SETFOREGROUNDFLASHCOUNT = 0x2005;
        public const int SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        public const int SPI_SETGRADIENTCAPTIONS = 0x1009;
        public const int SPI_SETGRIDGRANULARITY = 0x13;
        public const int SPI_SETHANDHELD = 0x4e;
        public const int SPI_SETHIGHCONTRAST = 0x43;
        public const int SPI_SETHOTTRACKING = 0x100f;
        public const int SPI_SETICONMETRICS = 0x2e;
        public const int SPI_SETICONS = 0x58;
        public const int SPI_SETICONTITLELOGFONT = 0x22;
        public const int SPI_SETICONTITLEWRAP = 0x1a;
        public const int SPI_SETKEYBOARDCUES = 0x100b;
        public const int SPI_SETKEYBOARDDELAY = 0x17;
        public const int SPI_SETKEYBOARDPREF = 0x45;
        public const int SPI_SETKEYBOARDSPEED = 11;
        public const int SPI_SETLANGTOGGLE = 0x5b;
        public const int SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007;
        public const int SPI_SETLOWPOWERACTIVE = 0x55;
        public const int SPI_SETLOWPOWERTIMEOUT = 0x51;
        public const int SPI_SETMENUANIMATION = 0x1003;
        public const int SPI_SETMENUDROPALIGNMENT = 0x1c;
        public const int SPI_SETMENUFADE = 0x1013;
        public const int SPI_SETMENUSHOWDELAY = 0x6b;
        public const int SPI_SETMENUUNDERLINES = 0x100b;
        public const int SPI_SETMINIMIZEDMETRICS = 0x2c;
        public const int SPI_SETMOUSE = 4;
        public const int SPI_SETMOUSEBUTTONSWAP = 0x21;
        public const int SPI_SETMOUSEHOVERHEIGHT = 0x65;
        public const int SPI_SETMOUSEHOVERTIME = 0x67;
        public const int SPI_SETMOUSEHOVERWIDTH = 0x63;
        public const int SPI_SETMOUSEKEYS = 0x37;
        public const int SPI_SETMOUSESPEED = 0x71;
        public const int SPI_SETMOUSETRAILS = 0x5d;
        public const int SPI_SETNONCLIENTMETRICS = 0x2a;
        public const int SPI_SETPENWINDOWS = 0x31;
        public const int SPI_SETPOWEROFFACTIVE = 0x56;
        public const int SPI_SETPOWEROFFTIMEOUT = 0x52;
        public const int SPI_SETSCREENREADER = 0x47;
        public const int SPI_SETSCREENSAVEACTIVE = 0x11;
        public const int SPI_SETSCREENSAVERRUNNING = 0x61;
        public const int SPI_SETSCREENSAVETIMEOUT = 15;
        public const int SPI_SETSELECTIONFADE = 0x1015;
        public const int SPI_SETSERIALKEYS = 0x3f;
        public const int SPI_SETSHOWIMEUI = 0x6f;
        public const int SPI_SETSHOWSOUNDS = 0x39;
        public const int SPI_SETSNAPTODEFBUTTON = 0x60;
        public const int SPI_SETSOUNDSENTRY = 0x41;
        public const int SPI_SETSTICKYKEYS = 0x3b;
        public const int SPI_SETTOGGLEKEYS = 0x35;
        public const int SPI_SETTOOLTIPANIMATION = 0x1017;
        public const int SPI_SETTOOLTIPFADE = 0x1019;
        public const int SPI_SETUIEFFECTS = 0x103f;
        public const int SPI_SETWHEELSCROLLLINES = 0x69;
        public const int SPI_SETWORKAREA = 0x2f;
        public const int STANDARD_RIGHTS_READ = 0x20000;
        public const int STANDARD_RIGHTS_REQUIRED = 0xf0000;
        public const int STARTF_USESHOWWINDOW = 1;
        public const int STARTF_USESTDHANDLES = 0x100;
        public const uint STATUS_INFO_LENGTH_MISMATCH = 0xc0000004;
        public const int STD_ERROR_HANDLE = -12;
        public const int STD_INPUT_HANDLE = -10;
        public const int STD_OUTPUT_HANDLE = -11;
        public const int STILL_ACTIVE = 0x103;
        public const int SW_HIDE = 0;
        public const int SW_MAX = 10;
        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        public const int SW_NORMAL = 1;
        public const int SW_RESTORE = 9;
        public const int SW_SHOW = 5;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOWNORMAL = 1;
        public const int SYNCHRONIZE = 0x100000;
        public const int TH32CS_INHERIT = -2147483648;
        public const int TH32CS_SNAPHEAPLIST = 1;
        public const int TH32CS_SNAPMODULE = 8;
        public const int TH32CS_SNAPPROCESS = 2;
        public const int TH32CS_SNAPTHREAD = 4;
        public const int THREAD_DIRECT_IMPERSONATION = 0x200;
        public const int THREAD_GET_CONTEXT = 8;
        public const int THREAD_IMPERSONATE = 0x100;
        public const int THREAD_QUERY_INFORMATION = 0x40;
        public const int THREAD_SET_CONTEXT = 0x10;
        public const int THREAD_SET_INFORMATION = 0x20;
        public const int THREAD_SET_THREAD_TOKEN = 0x80;
        public const int THREAD_SUSPEND_RESUME = 2;
        public const int THREAD_TERMINATE = 1;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        public const int TOKEN_ALL_ACCESS = 0xf01ff;
        public const int TOKEN_EXECUTE = 0x20000;
        public const int TOKEN_IMPERSONATE = 4;
        public const int TOKEN_QUERY = 8;
        public const int TOKEN_READ = 0x20008;
        public const int TOKEN_TYPE_TokenImpersonation = 2;
        public const int TOKEN_TYPE_TokenPrimary = 1;
        internal const byte TWOSTOPBITS = 2;
        public const int UIS_CLEAR = 2;
        public const int UIS_SET = 1;
        public const int UISF_HIDEACCEL = 2;
        public const int UISF_HIDEFOCUS = 1;
        public const int UOI_FLAGS = 1;
        public const int UOI_NAME = 2;
        public const int UOI_TYPE = 3;
        public const int UOI_USER_SID = 4;
        public const int USERCLASSTYPE_FULL = 1;
        public const int VER_PLATFORM_WIN32_NT = 2;
        public const int VFT_APP = 1;
        public const int VFT_DLL = 2;
        public const int VFT_DRV = 3;
        public const int VFT_FONT = 4;
        public const int VFT_STATIC_LIB = 7;
        public const int VFT_UNKNOWN = 0;
        public const int VFT_VXD = 5;
        public const int VFT2_DRV_COMM = 10;
        public const int VFT2_DRV_DISPLAY = 4;
        public const int VFT2_DRV_INPUTMETHOD = 11;
        public const int VFT2_DRV_INSTALLABLE = 8;
        public const int VFT2_DRV_KEYBOARD = 2;
        public const int VFT2_DRV_LANGUAGE = 3;
        public const int VFT2_DRV_MOUSE = 5;
        public const int VFT2_DRV_NETWORK = 6;
        public const int VFT2_DRV_PRINTER = 1;
        public const int VFT2_DRV_SOUND = 9;
        public const int VFT2_DRV_SYSTEM = 7;
        public const int VFT2_FONT_RASTER = 1;
        public const int VFT2_FONT_TRUETYPE = 3;
        public const int VFT2_FONT_VECTOR = 2;
        public const int VFT2_UNKNOWN = 0;
        public const int VS_FF_DEBUG = 1;
        public const int VS_FF_INFOINFERRED = 0x10;
        public const int VS_FF_PATCHED = 4;
        public const int VS_FF_PRERELEASE = 2;
        public const int VS_FF_PRIVATEBUILD = 8;
        public const int VS_FF_SPECIALBUILD = 0x20;
        public const int VS_FFI_FILEFLAGSMASK = 0x3f;
        public const int VS_FFI_SIGNATURE = -17890115;
        public const int VS_FFI_STRUCVERSION = 0x10000;
        public const int VS_FILE_INFO = 0x10;
        public const int VS_USER_DEFINED = 100;
        public const int VS_VERSION_INFO = 1;
        public const int WAIT_ABANDONED = 0x80;
        public const int WAIT_ABANDONED_0 = 0x80;
        public const int WAIT_FAILED = -1;
        public const int WAIT_OBJECT_0 = 0;
        public const int WAIT_TIMEOUT = 0x102;
        public const int WHITENESS = 0xff0062;
        public const int WM_CLOSE = 0x10;
        public const int WM_COMPACTING = 0x41;
        public const int WM_CREATETIMER = 0x401;
        public const int WM_DISPLAYCHANGE = 0x7e;
        public const int WM_ENDSESSION = 0x16;
        public const int WM_FONTCHANGE = 0x1d;
        public const int WM_KILLTIMER = 0x402;
        public const int WM_NULL = 0;
        public const int WM_PALETTECHANGED = 0x311;
        public const int WM_POWERBROADCAST = 0x218;
        public const int WM_QUERYENDSESSION = 0x11;
        public const int WM_QUIT = 0x12;
        public const int WM_REFLECT = 0x2000;
        public const int WM_SETTINGCHANGE = 0x1a;
        public const int WM_SYSCOLORCHANGE = 0x15;
        public const int WM_THEMECHANGED = 0x31a;
        public const int WM_TIMECHANGE = 30;
        public const int WM_TIMER = 0x113;
        public const int WM_USER = 0x400;
        public const int WM_WTSSESSION_CHANGE = 0x2b1;
        public const int WS_DISABLED = 0x8000000;
        public const int WS_POPUP = -2147483648;
        public const int WS_VISIBLE = 0x10000000;
        public const int WSF_VISIBLE = 1;
        public const int WTS_CONSOLE_CONNECT = 1;
        public const int WTS_CONSOLE_DISCONNECT = 2;
        public const int WTS_REMOTE_CONNECT = 3;
        public const int WTS_REMOTE_DISCONNECT = 4;
        public const int WTS_SESSION_LOCK = 7;
        public const int WTS_SESSION_LOGOFF = 6;
        public const int WTS_SESSION_LOGON = 5;
        public const int WTS_SESSION_REMOTE_CONTROL = 9;
        public const int WTS_SESSION_UNLOCK = 8;

        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool AdjustTokenPrivileges(HandleRef TokenHandle, bool DisableAllPrivileges, TokenPrivileges NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, SafeFileHandle hTemplateFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern Microsoft.Win32.SafeHandles.SafeFileMappingHandle CreateFileMapping(IntPtr hFile, SECURITY_ATTRIBUTES lpFileMappingAttributes, int flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool CreateProcess([MarshalAs(UnmanagedType.LPTStr)] string lpApplicationName, StringBuilder lpCommandLine, SECURITY_ATTRIBUTES lpProcessAttributes, SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, STARTUPINFO lpStartupInfo, Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation);
        [SuppressUnmanagedCodeSecurity, DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool CreateProcessAsUser(SafeHandle hToken, string lpApplicationName, string lpCommandLine, SECURITY_ATTRIBUTES lpProcessAttributes, SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, HandleRef lpEnvironment, string lpCurrentDirectory, STARTUPINFO lpStartupInfo, Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern bool CreateProcessWithLogonW(string userName, string domain, IntPtr password, LogonFlags logonFlags, [MarshalAs(UnmanagedType.LPTStr)] string appName, StringBuilder cmdLine, int creationFlags, IntPtr environmentBlock, [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, STARTUPINFO lpStartupInfo, Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr CreateToolhelp32Snapshot(int flags, int processId);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        public static extern bool DuplicateHandle(HandleRef hSourceProcessHandle, SafeHandle hSourceHandle, HandleRef hTargetProcess, out SafeFileHandle targetHandle, int dwDesiredAccess, bool bInheritHandle, int dwOptions);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        public static extern bool DuplicateHandle(HandleRef hSourceProcessHandle, SafeHandle hSourceHandle, HandleRef hTargetProcess, out SafeWaitHandle targetHandle, int dwDesiredAccess, bool bInheritHandle, int dwOptions);
        [DllImport("psapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool EnumProcesses(int[] processIds, int size, out int needed);
        [DllImport("psapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool EnumProcessModules(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, IntPtr modules, int size, ref int needed);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        public static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int GetCurrentProcessId();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetExitCodeProcess(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, out int exitCode);
        internal static string GetLocalPath(string fileName)
        {
            Uri uri = new Uri(fileName);
            return (uri.LocalPath + uri.Fragment);
        }

        [DllImport("psapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetModuleBaseName(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, HandleRef moduleHandle, StringBuilder baseName, int size);
        [DllImport("psapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetModuleFileNameEx(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, HandleRef moduleHandle, StringBuilder baseName, int size);
        [DllImport("psapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetModuleFileNameEx(HandleRef processHandle, HandleRef moduleHandle, StringBuilder baseName, int size);
        [DllImport("psapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetModuleInformation(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, HandleRef moduleHandle, NtModuleInfo ntModuleInfo, int size);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetPriorityClass(Microsoft.Win32.SafeHandles.SafeProcessHandle handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetProcessAffinityMask(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, out IntPtr processMask, out IntPtr systemMask);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetProcessPriorityBoost(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, out bool disabled);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetProcessTimes(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, out long creation, out long exit, out long kernel, out long user);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetProcessWorkingSetSize(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, out IntPtr min, out IntPtr max);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        public static extern IntPtr GetStdHandle(int whichHandle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetThreadPriority(Microsoft.Win32.SafeHandles.SafeThreadHandle handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetThreadPriorityBoost(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, out bool disabled);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetThreadTimes(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, out long creation, out long exit, out long kernel, out long user);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowLong(HandleRef hWnd, int nIndex);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool IsWindowVisible(HandleRef hWnd);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool LookupPrivilegeValue([MarshalAs(UnmanagedType.LPTStr)] string lpSystemName, [MarshalAs(UnmanagedType.LPTStr)] string lpName, out LUID lpLuid);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool Module32First(HandleRef handle, IntPtr entry);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool Module32Next(HandleRef handle, IntPtr entry);
        [DllImport("ntdll.dll", CharSet=CharSet.Auto)]
        public static extern int NtQueryInformationProcess(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, int query, NtProcessBasicInfo info, int size, int[] returnedSize);
        [DllImport("ntdll.dll", CharSet=CharSet.Auto)]
        public static extern int NtQuerySystemInformation(int query, IntPtr dataPtr, int size, out int returnedSize);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern Microsoft.Win32.SafeHandles.SafeFileMappingHandle OpenFileMapping(int dwDesiredAccess, bool bInheritHandle, string lpName);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcess(int access, bool inherit, int processId);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool OpenProcessToken(HandleRef ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern Microsoft.Win32.SafeHandles.SafeThreadHandle OpenThread(int access, bool inherit, int threadId);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int PostMessage(HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool Process32First(HandleRef handle, IntPtr entry);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool Process32Next(HandleRef handle, IntPtr entry);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam, int flags, int timeout, out IntPtr pdwResult);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetPriorityClass(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, int priorityClass);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetProcessAffinityMask(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, IntPtr mask);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetProcessPriorityBoost(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, bool disabled);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetProcessWorkingSetSize(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, IntPtr min, IntPtr max);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr SetThreadAffinityMask(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, HandleRef mask);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int SetThreadIdealProcessor(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, int processor);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetThreadPriority(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, int priority);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetThreadPriorityBoost(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, bool disabled);
        [DllImport("shell32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool ShellExecuteEx(ShellExecuteInfo info);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool TerminateProcess(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, int exitCode);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool Thread32First(HandleRef handle, WinThreadEntry entry);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool Thread32Next(HandleRef handle, WinThreadEntry entry);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr VirtualQuery(SafeFileMapViewHandle address, ref MEMORY_BASIC_INFORMATION buffer, IntPtr sizeOfBuffer);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int WaitForInputIdle(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, int milliseconds);

        public delegate int ConHndlr(int signalType);

        internal delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [Flags]
        internal enum LogonFlags
        {
            LOGON_NETCREDENTIALS_ONLY = 2,
            LOGON_WITH_PROFILE = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MEMORY_BASIC_INFORMATION
        {
            internal IntPtr BaseAddress;
            internal IntPtr AllocationBase;
            internal uint AllocationProtect;
            internal UIntPtr RegionSize;
            internal uint State;
            internal uint Protect;
            internal uint Type;
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
        internal class NtModuleInfo
        {
            public IntPtr BaseOfDll = IntPtr.Zero;
            public int SizeOfImage;
            public IntPtr EntryPoint = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class NtProcessBasicInfo
        {
            public int ExitStatus;
            public IntPtr PebBaseAddress = IntPtr.Zero;
            public IntPtr AffinityMask = IntPtr.Zero;
            public int BasePriority;
            public IntPtr UniqueProcessId = IntPtr.Zero;
            public IntPtr InheritedFromUniqueProcessId = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PDH_FMT_COUNTERVALUE
        {
            public int CStatus;
            public double data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PDH_RAW_COUNTER
        {
            public int CStatus;
            public long TimeStamp;
            public long FirstValue;
            public long SecondValue;
            public int MultiCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_COUNTER_BLOCK
        {
            public int ByteLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_COUNTER_DEFINITION
        {
            public int ByteLength;
            public int CounterNameTitleIndex;
            public int CounterNameTitlePtr;
            public int CounterHelpTitleIndex;
            public int CounterHelpTitlePtr;
            public int DefaultScale;
            public int DetailLevel;
            public int CounterType;
            public int CounterSize;
            public int CounterOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_DATA_BLOCK
        {
            public int Signature1;
            public int Signature2;
            public int LittleEndian;
            public int Version;
            public int Revision;
            public int TotalByteLength;
            public int HeaderLength;
            public int NumObjectTypes;
            public int DefaultObject;
            public Microsoft.Win32.NativeMethods.SYSTEMTIME SystemTime;
            public int pad1;
            public long PerfTime;
            public long PerfFreq;
            public long PerfTime100nSec;
            public int SystemNameLength;
            public int SystemNameOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_INSTANCE_DEFINITION
        {
            public int ByteLength;
            public int ParentObjectTitleIndex;
            public int ParentObjectInstance;
            public int UniqueID;
            public int NameOffset;
            public int NameLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_OBJECT_TYPE
        {
            public int TotalByteLength;
            public int DefinitionLength;
            public int HeaderLength;
            public int ObjectNameTitleIndex;
            public int ObjectNameTitlePtr;
            public int ObjectHelpTitleIndex;
            public int ObjectHelpTitlePtr;
            public int DetailLevel;
            public int NumCounters;
            public int DefaultCounter;
            public int NumInstances;
            public int CodePage;
            public long PerfTime;
            public long PerfFreq;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            public int nLength = 12;
            public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class ShellExecuteInfo
        {
            public int cbSize;
            public int fMask;
            public IntPtr hwnd = IntPtr.Zero;
            public IntPtr lpVerb = IntPtr.Zero;
            public IntPtr lpFile = IntPtr.Zero;
            public IntPtr lpParameters = IntPtr.Zero;
            public IntPtr lpDirectory = IntPtr.Zero;
            public int nShow;
            public IntPtr hInstApp = IntPtr.Zero;
            public IntPtr lpIDList = IntPtr.Zero;
            public IntPtr lpClass = IntPtr.Zero;
            public IntPtr hkeyClass = IntPtr.Zero;
            public int dwHotKey;
            public IntPtr hIcon = IntPtr.Zero;
            public IntPtr hProcess = IntPtr.Zero;
            public ShellExecuteInfo()
            {
                this.cbSize = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class STARTUPINFO
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
            public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);
            public STARTUPINFO()
            {
                this.cb = Marshal.SizeOf(this);
            }

            public void Dispose()
            {
                if ((this.hStdInput != null) && !this.hStdInput.IsInvalid)
                {
                    this.hStdInput.Close();
                    this.hStdInput = null;
                }
                if ((this.hStdOutput != null) && !this.hStdOutput.IsInvalid)
                {
                    this.hStdOutput.Close();
                    this.hStdOutput = null;
                }
                if ((this.hStdError != null) && !this.hStdError.IsInvalid)
                {
                    this.hStdError.Close();
                    this.hStdError = null;
                }
            }
        }

        public enum StructFormat
        {
            Ansi = 1,
            Auto = 3,
            Unicode = 2
        }

        public enum StructFormatEnum
        {
            Ansi = 1,
            Auto = 3,
            Unicode = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SYSTEMTIME
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
                return ("[SYSTEMTIME: " + this.wDay.ToString(CultureInfo.CurrentCulture) + "/" + this.wMonth.ToString(CultureInfo.CurrentCulture) + "/" + this.wYear.ToString(CultureInfo.CurrentCulture) + " " + this.wHour.ToString(CultureInfo.CurrentCulture) + ":" + this.wMinute.ToString(CultureInfo.CurrentCulture) + ":" + this.wSecond.ToString(CultureInfo.CurrentCulture) + "]");
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class TEXTMETRIC
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
        internal class TokenPrivileges
        {
            public int PrivilegeCount = 1;
            public Microsoft.Win32.NativeMethods.LUID Luid;
            public int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class USEROBJECTFLAGS
        {
            public int fInherit;
            public int fReserved;
            public int dwFlags;
        }

        internal static class Util
        {
            public static int HIWORD(int n)
            {
                return ((n >> 0x10) & 0xffff);
            }

            public static int LOWORD(int n)
            {
                return (n & 0xffff);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class VS_FIXEDFILEINFO
        {
            public int dwSignature;
            public int dwStructVersion;
            public int dwFileVersionMS;
            public int dwFileVersionLS;
            public int dwProductVersionMS;
            public int dwProductVersionLS;
            public int dwFileFlagsMask;
            public int dwFileFlags;
            public int dwFileOS;
            public int dwFileType;
            public int dwFileSubtype;
            public int dwFileDateMS;
            public int dwFileDateLS;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class WinModuleEntry
        {
            public const int sizeofModuleName = 0x100;
            public const int sizeofFileName = 260;
            public int dwSize;
            public int th32ModuleID;
            public int th32ProcessID;
            public int GlblcntUsage;
            public int ProccntUsage;
            public IntPtr modBaseAddr = IntPtr.Zero;
            public int modBaseSize;
            public IntPtr hModule = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class WinProcessEntry
        {
            public const int sizeofFileName = 260;
            public int dwSize;
            public int cntUsage;
            public int th32ProcessID;
            public IntPtr th32DefaultHeapID = IntPtr.Zero;
            public int th32ModuleID;
            public int cntThreads;
            public int th32ParentProcessID;
            public int pcPriClassBase;
            public int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class WinThreadEntry
        {
            public int dwSize;
            public int cntUsage;
            public int th32ThreadID;
            public int th32OwnerProcessID;
            public int tpBasePri;
            public int tpDeltaPri;
            public int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class WNDCLASS
        {
            public int style;
            public Microsoft.Win32.NativeMethods.WndProc lpfnWndProc;
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
        internal class WNDCLASS_I
        {
            public int style;
            public IntPtr lpfnWndProc;
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

