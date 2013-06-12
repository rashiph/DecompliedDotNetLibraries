namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity, SecurityCritical]
    internal static class Win32Native
    {
        internal const string ADVAPI32 = "advapi32.dll";
        internal const uint ANONYMOUS_LOGON_LUID = 0x3e6;
        internal const string CRYPT32 = "crypt32.dll";
        internal const uint CRYPTPROTECTMEMORY_BLOCK_SIZE = 0x10;
        internal const uint CRYPTPROTECTMEMORY_CROSS_PROCESS = 1;
        internal const uint CRYPTPROTECTMEMORY_SAME_LOGON = 2;
        internal const uint CRYPTPROTECTMEMORY_SAME_PROCESS = 0;
        internal const int CSIDL_ADMINTOOLS = 0x30;
        internal const int CSIDL_APPDATA = 0x1a;
        internal const int CSIDL_CDBURN_AREA = 0x3b;
        internal const int CSIDL_COMMON_ADMINTOOLS = 0x2f;
        internal const int CSIDL_COMMON_APPDATA = 0x23;
        internal const int CSIDL_COMMON_DESKTOPDIRECTORY = 0x19;
        internal const int CSIDL_COMMON_DOCUMENTS = 0x2e;
        internal const int CSIDL_COMMON_MUSIC = 0x35;
        internal const int CSIDL_COMMON_OEM_LINKS = 0x3a;
        internal const int CSIDL_COMMON_PICTURES = 0x36;
        internal const int CSIDL_COMMON_PROGRAMS = 0x17;
        internal const int CSIDL_COMMON_STARTMENU = 0x16;
        internal const int CSIDL_COMMON_STARTUP = 0x18;
        internal const int CSIDL_COMMON_TEMPLATES = 0x2d;
        internal const int CSIDL_COMMON_VIDEO = 0x37;
        internal const int CSIDL_COOKIES = 0x21;
        internal const int CSIDL_DESKTOP = 0;
        internal const int CSIDL_DESKTOPDIRECTORY = 0x10;
        internal const int CSIDL_DRIVES = 0x11;
        internal const int CSIDL_FAVORITES = 6;
        internal const int CSIDL_FLAG_CREATE = 0x8000;
        internal const int CSIDL_FLAG_DONT_VERIFY = 0x4000;
        internal const int CSIDL_FONTS = 20;
        internal const int CSIDL_HISTORY = 0x22;
        internal const int CSIDL_INTERNET_CACHE = 0x20;
        internal const int CSIDL_LOCAL_APPDATA = 0x1c;
        internal const int CSIDL_MYMUSIC = 13;
        internal const int CSIDL_MYPICTURES = 0x27;
        internal const int CSIDL_MYVIDEO = 14;
        internal const int CSIDL_NETHOOD = 0x13;
        internal const int CSIDL_PERSONAL = 5;
        internal const int CSIDL_PRINTHOOD = 0x1b;
        internal const int CSIDL_PROFILE = 40;
        internal const int CSIDL_PROGRAM_FILES = 0x26;
        internal const int CSIDL_PROGRAM_FILES_COMMON = 0x2b;
        internal const int CSIDL_PROGRAM_FILES_COMMONX86 = 0x2c;
        internal const int CSIDL_PROGRAM_FILESX86 = 0x2a;
        internal const int CSIDL_PROGRAMS = 2;
        internal const int CSIDL_RECENT = 8;
        internal const int CSIDL_RESOURCES = 0x38;
        internal const int CSIDL_RESOURCES_LOCALIZED = 0x39;
        internal const int CSIDL_SENDTO = 9;
        internal const int CSIDL_STARTMENU = 11;
        internal const int CSIDL_STARTUP = 7;
        internal const int CSIDL_SYSTEM = 0x25;
        internal const int CSIDL_SYSTEMX86 = 0x29;
        internal const int CSIDL_TEMPLATES = 0x15;
        internal const int CSIDL_WINDOWS = 0x24;
        internal const int CTRL_BREAK_EVENT = 1;
        internal const int CTRL_C_EVENT = 0;
        internal const int CTRL_CLOSE_EVENT = 2;
        internal const int CTRL_LOGOFF_EVENT = 5;
        internal const int CTRL_SHUTDOWN_EVENT = 6;
        internal const uint DUPLICATE_CLOSE_SOURCE = 1;
        internal const uint DUPLICATE_SAME_ACCESS = 2;
        internal const uint DUPLICATE_SAME_ATTRIBUTES = 4;
        internal const int ENABLE_ECHO_INPUT = 4;
        internal const int ENABLE_LINE_INPUT = 2;
        internal const int ENABLE_PROCESSED_INPUT = 1;
        internal const int ERROR_ACCESS_DENIED = 5;
        internal const int ERROR_ALREADY_EXISTS = 0xb7;
        internal const int ERROR_BAD_IMPERSONATION_LEVEL = 0x542;
        internal const int ERROR_BAD_LENGTH = 0x18;
        internal const int ERROR_BAD_PATHNAME = 0xa1;
        internal const int ERROR_CALL_NOT_IMPLEMENTED = 120;
        internal const int ERROR_CANT_OPEN_ANONYMOUS = 0x543;
        internal const int ERROR_DIRECTORY = 0x10b;
        internal const int ERROR_DLL_INIT_FAILED = 0x45a;
        internal const int ERROR_ENVVAR_NOT_FOUND = 0xcb;
        internal const int ERROR_FILE_EXISTS = 80;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_FILENAME_EXCED_RANGE = 0xce;
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        internal const int ERROR_INVALID_ACL = 0x538;
        internal const int ERROR_INVALID_DATA = 13;
        internal const int ERROR_INVALID_DRIVE = 15;
        internal const int ERROR_INVALID_FUNCTION = 1;
        internal const int ERROR_INVALID_HANDLE = 6;
        internal const int ERROR_INVALID_NAME = 0x7b;
        internal const int ERROR_INVALID_OWNER = 0x51b;
        internal const int ERROR_INVALID_PARAMETER = 0x57;
        internal const int ERROR_INVALID_PRIMARY_GROUP = 0x51c;
        internal const int ERROR_INVALID_SECURITY_DESCR = 0x53a;
        internal const int ERROR_INVALID_SID = 0x539;
        internal const int ERROR_MORE_DATA = 0xea;
        internal const int ERROR_NO_DATA = 0xe8;
        internal const int ERROR_NO_MORE_FILES = 0x12;
        internal const int ERROR_NO_SECURITY_ON_OBJECT = 0x546;
        internal const int ERROR_NO_SUCH_PRIVILEGE = 0x521;
        internal const int ERROR_NO_TOKEN = 0x3f0;
        internal const int ERROR_NON_ACCOUNT_SID = 0x4e9;
        internal const int ERROR_NONE_MAPPED = 0x534;
        internal const int ERROR_NOT_ALL_ASSIGNED = 0x514;
        internal const int ERROR_NOT_ENOUGH_MEMORY = 8;
        internal const int ERROR_NOT_READY = 0x15;
        internal const int ERROR_NOT_SUPPORTED = 50;
        internal const int ERROR_OPERATION_ABORTED = 0x3e3;
        internal const int ERROR_PATH_NOT_FOUND = 3;
        internal const int ERROR_PIPE_NOT_CONNECTED = 0xe9;
        internal const int ERROR_PRIVILEGE_NOT_HELD = 0x522;
        internal const int ERROR_SHARING_VIOLATION = 0x20;
        internal const int ERROR_SUCCESS = 0;
        internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 0x6fd;
        internal const int ERROR_UNKNOWN_REVISION = 0x519;
        internal const int EVENT_MODIFY_STATE = 2;
        internal const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        internal const int FILE_ATTRIBUTE_READONLY = 1;
        internal const int FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
        internal const int FILE_TYPE_CHAR = 2;
        internal const int FILE_TYPE_DISK = 1;
        internal const int FILE_TYPE_PIPE = 3;
        internal const int FIND_ENDSWITH = 0x200000;
        internal const int FIND_FROMEND = 0x800000;
        internal const int FIND_FROMSTART = 0x400000;
        internal const int FIND_STARTSWITH = 0x100000;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        internal const int HWND_BROADCAST = 0xffff;
        internal const int INVALID_FILE_SIZE = -1;
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        internal const int IO_REPARSE_TAG_MOUNT_POINT = -1610612733;
        internal const string KERNEL32 = "kernel32.dll";
        internal const int KEY_CREATE_LINK = 0x20;
        internal const int KEY_CREATE_SUB_KEY = 4;
        internal const int KEY_ENUMERATE_SUB_KEYS = 8;
        internal const short KEY_EVENT = 1;
        internal const int KEY_NOTIFY = 0x10;
        internal const int KEY_QUERY_VALUE = 1;
        internal const int KEY_READ = 0x20019;
        internal const int KEY_SET_VALUE = 2;
        internal const int KEY_WOW64_32KEY = 0x200;
        internal const int KEY_WOW64_64KEY = 0x100;
        internal const int KEY_WRITE = 0x20006;
        internal const int LCID_SUPPORTED = 2;
        internal const int LMEM_FIXED = 0;
        internal const int LMEM_ZEROINIT = 0x40;
        internal const int LOAD_LIBRARY_AS_DATAFILE = 2;
        internal const int LOAD_STRING_MAX_LENGTH = 500;
        internal const int LPTR = 0x40;
        internal const string LSTRLENA = "lstrlenA";
        internal const string LSTRLENW = "lstrlenW";
        internal const int MAX_PATH = 260;
        internal const int MEM_COMMIT = 0x1000;
        internal const int MEM_FREE = 0x10000;
        internal const int MEM_RELEASE = 0x8000;
        internal const int MEM_RESERVE = 0x2000;
        internal const string MICROSOFT_KERBEROS_NAME = "Kerberos";
        internal const string MOVEMEMORY = "RtlMoveMemory";
        internal const string MSCORWKS = "clr.dll";
        internal const int MUI_ALL_LANGUAGES = 0x40;
        internal const int MUI_INSTALLED_LANGUAGES = 0x20;
        internal const int MUI_LANG_NEUTRAL_PE_FILE = 0x100;
        internal const int MUI_LANGUAGE_ID = 4;
        internal const int MUI_LANGUAGE_NAME = 8;
        internal const int MUI_NON_LANG_NEUTRAL_FILE = 0x200;
        internal const int MUI_PREFERRED_UI_LANGUAGES = 0x10;
        internal const int MUTEX_ALL_ACCESS = 0x1f0001;
        internal const int MUTEX_MODIFY_STATE = 1;
        internal const int NameSamCompatible = 2;
        internal static readonly IntPtr NULL = IntPtr.Zero;
        internal const string OLE32 = "ole32.dll";
        internal const string OLEAUT32 = "oleaut32.dll";
        internal const int PAGE_READWRITE = 4;
        internal const int READ_CONTROL = 0x20000;
        internal const int REG_BINARY = 3;
        internal const int REG_DWORD = 4;
        internal const int REG_DWORD_BIG_ENDIAN = 5;
        internal const int REG_DWORD_LITTLE_ENDIAN = 4;
        internal const int REG_EXPAND_SZ = 2;
        internal const int REG_FULL_RESOURCE_DESCRIPTOR = 9;
        internal const int REG_LINK = 6;
        internal const int REG_MULTI_SZ = 7;
        internal const int REG_NONE = 0;
        internal const int REG_OPTION_BACKUP_RESTORE = 4;
        internal const int REG_OPTION_CREATE_LINK = 2;
        internal const int REG_OPTION_NON_VOLATILE = 0;
        internal const int REG_OPTION_VOLATILE = 1;
        internal const int REG_QWORD = 11;
        internal const int REG_RESOURCE_LIST = 8;
        internal const int REG_RESOURCE_REQUIREMENTS_LIST = 10;
        internal const int REG_SZ = 1;
        internal const int REPLACEFILE_IGNORE_MERGE_ERRORS = 2;
        internal const int REPLACEFILE_WRITE_THROUGH = 1;
        internal const uint SE_GROUP_ENABLED = 4;
        internal const uint SE_GROUP_ENABLED_BY_DEFAULT = 2;
        internal const uint SE_GROUP_LOGON_ID = 0xc0000000;
        internal const uint SE_GROUP_MANDATORY = 1;
        internal const uint SE_GROUP_OWNER = 8;
        internal const uint SE_GROUP_RESOURCE = 0x20000000;
        internal const uint SE_GROUP_USE_FOR_DENY_ONLY = 0x10;
        internal const uint SE_PRIVILEGE_DISABLED = 0;
        internal const uint SE_PRIVILEGE_ENABLED = 2;
        internal const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1;
        internal const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
        internal const string SECUR32 = "secur32.dll";
        internal const int SECURITY_ANONYMOUS = 0;
        internal const int SECURITY_ANONYMOUS_LOGON_RID = 7;
        internal const int SECURITY_AUTHENTICATED_USER_RID = 11;
        internal const int SECURITY_BUILTIN_DOMAIN_RID = 0x20;
        internal const int SECURITY_LOCAL_SYSTEM_RID = 0x12;
        internal const int SECURITY_SQOS_PRESENT = 0x100000;
        internal const int SEM_FAILCRITICALERRORS = 1;
        internal const int SEMAPHORE_MODIFY_STATE = 2;
        internal const string SHFOLDER = "shfolder.dll";
        internal const int SHGFP_TYPE_CURRENT = 0;
        internal const string SHIM = "mscoree.dll";
        internal const int STANDARD_RIGHTS_READ = 0x20000;
        internal const int STANDARD_RIGHTS_WRITE = 0x20000;
        internal const uint STATUS_ACCESS_DENIED = 0xc0000022;
        internal const int STATUS_ACCOUNT_RESTRICTION = -1073741714;
        internal const uint STATUS_INSUFFICIENT_RESOURCES = 0xc000009a;
        internal const uint STATUS_NO_MEMORY = 0xc0000017;
        internal const uint STATUS_NONE_MAPPED = 0xc0000073;
        internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xc0000034;
        internal const uint STATUS_SOME_NOT_MAPPED = 0x107;
        internal const uint STATUS_SUCCESS = 0;
        internal const int STD_ERROR_HANDLE = -12;
        internal const int STD_INPUT_HANDLE = -10;
        internal const int STD_OUTPUT_HANDLE = -11;
        internal const int SYNCHRONIZE = 0x100000;
        internal const int TIME_ZONE_ID_DAYLIGHT = 2;
        internal const int TIME_ZONE_ID_INVALID = -1;
        internal const int TIME_ZONE_ID_STANDARD = 1;
        internal const int TIME_ZONE_ID_UNKNOWN = 0;
        internal const int UOI_FLAGS = 1;
        internal const string USER32 = "user32.dll";
        internal const int VER_PLATFORM_MACOSX = 11;
        internal const int VER_PLATFORM_UNIX = 10;
        internal const int VER_PLATFORM_WIN32_NT = 2;
        internal const int VER_PLATFORM_WIN32_WINDOWS = 1;
        internal const int VER_PLATFORM_WIN32s = 0;
        internal const int VER_PLATFORM_WINCE = 3;
        internal const int WM_SETTINGCHANGE = 0x1a;
        internal const int WSF_VISIBLE = 1;
        internal const string ZEROMEMORY = "RtlZeroMemory";

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool AdjustTokenPrivileges([In] SafeTokenHandle TokenHandle, [In] bool DisableAllPrivileges, [In] ref TOKEN_PRIVILEGE NewState, [In] uint BufferLength, [In, Out] ref TOKEN_PRIVILEGE PreviousState, [In, Out] ref uint ReturnLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool AllocateLocallyUniqueId([In, Out] ref LUID Luid);
        [DllImport("bcrypt.dll")]
        internal static extern uint BCryptGetFipsAlgorithmMode([MarshalAs(UnmanagedType.U1)] out bool pfEnabled);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool Beep(int frequency, int duration);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CheckTokenMembership([In] SafeTokenHandle TokenHandle, [In] byte[] SidToCheck, [In, Out] ref bool IsMember);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool CloseHandle(IntPtr handle);
        [DllImport("ole32.dll")]
        internal static extern int CoCreateGuid(out Guid guid);
        [DllImport("advapi32.dll", EntryPoint="ConvertSecurityDescriptorToStringSecurityDescriptorW", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern int ConvertSdToStringSd(byte[] securityDescriptor, uint requestedRevision, uint securityInformation, out IntPtr resultString, ref uint resultStringLength);
        [DllImport("advapi32.dll", EntryPoint="ConvertStringSecurityDescriptorToSecurityDescriptorW", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern int ConvertStringSdToSd(string stringSd, uint stringSdRevision, out IntPtr resultSd, ref uint resultSdLength);
        [DllImport("advapi32.dll", EntryPoint="ConvertStringSidToSidW", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern int ConvertStringSidToSid(string stringSid, out IntPtr ByteArray);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CopyFile(string src, string dst, bool failIfExists);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Ansi, ExactSpelling=true)]
        internal static extern void CopyMemoryAnsi(IntPtr pdst, [In] StringBuilder psrc, IntPtr sizetcb);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Ansi, ExactSpelling=true)]
        internal static extern void CopyMemoryAnsi(StringBuilder pdst, IntPtr psrc, IntPtr sizetcb);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern void CopyMemoryUni(IntPtr pdst, string psrc, IntPtr sizetcb);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern void CopyMemoryUni(StringBuilder pdst, IntPtr psrc, IntPtr sizetcb);
        [ForceTokenStabilization, DllImport("ole32.dll")]
        internal static extern IntPtr CoTaskMemAlloc(int cb);
        [ForceTokenStabilization, DllImport("ole32.dll")]
        internal static extern void CoTaskMemFree(IntPtr ptr);
        [DllImport("ole32.dll")]
        internal static extern IntPtr CoTaskMemRealloc(IntPtr pv, int cb);
        [DllImport("clr.dll", CharSet=CharSet.Auto)]
        internal static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum, IApplicationContext pAppCtx, IAssemblyName pName, uint dwFlags, IntPtr pvReserved);
        [DllImport("clr.dll", CharSet=CharSet.Unicode)]
        internal static extern int CreateAssemblyNameObject(out IAssemblyName ppEnum, string szAssemblyName, uint dwFlags, IntPtr pvReserved);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CreateDirectory(string path, SECURITY_ATTRIBUTES lpSecurityAttributes);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeWaitHandle CreateEvent(SECURITY_ATTRIBUTES lpSecurityAttributes, bool isManualReset, bool initialState, string name);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeFileMappingHandle CreateFileMapping(SafeFileHandle hFile, IntPtr lpAttributes, uint fProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeWaitHandle CreateMutex(SECURITY_ATTRIBUTES lpSecurityAttributes, bool initialOwner, string name);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeWaitHandle CreateSemaphore(SECURITY_ATTRIBUTES lpSecurityAttributes, int initialCount, int maximumCount, string name);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern int CreateWellKnownSid(int sidType, byte[] domainSid, [Out] byte[] resultSid, ref uint resultSidLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DecryptFile(string path, int reservedMustBeZero);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DeleteFile(string path);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DeleteVolumeMountPoint(string mountPoint);
        [SecurityCritical]
        internal static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DuplicateHandle([In] IntPtr hSourceProcessHandle, [In] SafeTokenHandle hSourceHandle, [In] IntPtr hTargetProcessHandle, [In, Out] ref SafeTokenHandle lpTargetHandle, [In] uint dwDesiredAccess, [In] bool bInheritHandle, [In] uint dwOptions);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DuplicateHandle([In] IntPtr hSourceProcessHandle, [In] IntPtr hSourceHandle, [In] IntPtr hTargetProcessHandle, [In, Out] ref SafeTokenHandle lpTargetHandle, [In] uint dwDesiredAccess, [In] bool bInheritHandle, [In] uint dwOptions);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DuplicateTokenEx([In] SafeTokenHandle ExistingTokenHandle, [In] TokenAccessLevels DesiredAccess, [In] IntPtr TokenAttributes, [In] SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, [In] System.Security.Principal.TokenType TokenType, [In, Out] ref SafeTokenHandle DuplicateTokenHandle);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DuplicateTokenEx([In] SafeTokenHandle hExistingToken, [In] uint dwDesiredAccess, [In] IntPtr lpTokenAttributes, [In] uint ImpersonationLevel, [In] uint TokenType, [In, Out] ref SafeTokenHandle phNewToken);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EncryptFile(string path);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int ExpandEnvironmentStrings(string lpSrc, StringBuilder lpDst, int nSize);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool FillConsoleOutputAttribute(IntPtr hConsoleOutput, short wColorAttribute, int numCells, COORD startCoord, out int pNumBytesWritten);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool FillConsoleOutputCharacter(IntPtr hConsoleOutput, char character, int nLength, COORD dwWriteCoord, out int pNumCharsWritten);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll")]
        internal static extern bool FindClose(IntPtr handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeFindHandle FindFirstFile(string fileName, [In, Out] WIN32_FIND_DATA data);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool FindNextFile(SafeFindHandle hndFindFile, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATA lpFindFileData);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool FlushFileBuffers(SafeFileHandle hFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern unsafe bool FreeEnvironmentStrings(char* pStrings);
        [DllImport("kernel32.dll")]
        internal static extern int GetACP();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int GetCalendarInfo(int Locale, int Calendar, int CalType, StringBuilder lpCalData, int cchData, IntPtr lpValue);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int GetComputerName(StringBuilder nameBuffer, ref int bufferSize);
        [DllImport("kernel32.dll")]
        internal static extern uint GetConsoleCP();
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool GetConsoleCursorInfo(IntPtr hConsoleOutput, out CONSOLE_CURSOR_INFO cci);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);
        [DllImport("kernel32.dll")]
        internal static extern uint GetConsoleOutputCP();
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetConsoleTitle(StringBuilder sb, int capacity);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetCurrentDirectory(int nBufferLength, StringBuilder lpBuffer);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern uint GetCurrentProcessId();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetDiskFreeSpaceEx(string drive, out long freeBytesForUser, out long totalBytes, out long freeBytes);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetDriveType(string drive);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern unsafe char* GetEnvironmentStrings();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetEnvironmentVariable(string lpName, StringBuilder lpValue, int size);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int GetFileSize(SafeFileHandle hFile, out int highSize);
        [DllImport("kernel32.dll")]
        internal static extern int GetFileType(SafeFileHandle handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern unsafe int GetFullPathName(char* path, int numBufferChars, char* buffer, IntPtr mustBeZero);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetFullPathName(string path, int numBufferChars, StringBuilder buffer, IntPtr mustBeZero);
        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int virtualKeyCode);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern COORD GetLargestConsoleWindowSize(IntPtr hConsoleOutput);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int GetLogicalDrives();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern unsafe int GetLongPathName(char* path, char* longPathBuffer, int bufferLength);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetLongPathName(string path, StringBuilder longPathBuffer, int bufferLength);
        internal static string GetMessage(int errorCode)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            if (FormatMessage(0x3200, NULL, errorCode, 0, lpBuffer, lpBuffer.Capacity, NULL) != 0)
            {
                return lpBuffer.ToString();
            }
            return Environment.GetRuntimeResourceString("UnknownError_Num", new object[] { errorCode });
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string methodName);
        [DllImport("user32.dll", ExactSpelling=true)]
        internal static extern IntPtr GetProcessWindowStation();
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint GetSecurityDescriptorLength(IntPtr byteArray);
        [DllImport("advapi32.dll", EntryPoint="GetSecurityInfo", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint GetSecurityInfoByHandle(SafeHandle handle, uint objectType, uint securityInformation, out IntPtr sidOwner, out IntPtr sidGroup, out IntPtr dacl, out IntPtr sacl, out IntPtr securityDescriptor);
        [DllImport("advapi32.dll", EntryPoint="GetNamedSecurityInfoW", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint GetSecurityInfoByName(string name, uint objectType, uint securityInformation, out IntPtr sidOwner, out IntPtr sidGroup, out IntPtr dacl, out IntPtr sacl, out IntPtr securityDescriptor);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetSystemDirectory(StringBuilder sb, int length);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern uint GetTempFileName(string tmpPath, string prefix, uint uniqueIdOrZero, StringBuilder tmpFileName);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern uint GetTempPath(int bufferLen, StringBuilder buffer);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetTokenInformation([In] SafeTokenHandle TokenHandle, [In] uint TokenInformationClass, [In] SafeLocalAllocHandle TokenInformation, [In] uint TokenInformationLength, out uint ReturnLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetTokenInformation([In] IntPtr TokenHandle, [In] uint TokenInformationClass, [In] SafeLocalAllocHandle TokenInformation, [In] uint TokenInformationLength, out uint ReturnLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern bool GetUserName(StringBuilder lpBuffer, ref int nSize);
        [DllImport("secur32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern byte GetUserNameEx(int format, StringBuilder domainName, ref int domainNameLen);
        [DllImport("user32.dll", SetLastError=true)]
        internal static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, [MarshalAs(UnmanagedType.LPStruct)] USEROBJECTFLAGS pvBuffer, int nLength, ref int lpnLengthNeeded);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetVolumeInformation(string drive, StringBuilder volumeName, int volumeNameBufLen, out int volSerialNumber, out int maxFileNameLen, out int fileSystemFlags, StringBuilder fileSystemName, int fileSystemNameBufLen);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern int GetWindowsAccountDomainSid(byte[] sid, [Out] byte[] resultSid, ref uint resultSidLength);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetWindowsDirectory(StringBuilder sb, int length);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX buffer);
        [DllImport("advapi32.dll", EntryPoint="EqualDomainSid", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern int IsEqualDomainSid(byte[] sid1, byte[] sid2, out bool result);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern int IsWellKnownSid(byte[] sid, int type);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool IsWow64Process([In] IntPtr hSourceProcessHandle, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeLocalAllocHandle LocalAlloc([In] int uFlags, [In] IntPtr sizetdwBytes);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", EntryPoint="LocalAlloc")]
        internal static extern IntPtr LocalAlloc_NoSafeHandle(int uFlags, IntPtr sizetdwBytes);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr LocalFree(IntPtr handle);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr LocalReAlloc(IntPtr handle, IntPtr sizetcbBytes, int uFlags);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool LockFile(SafeFileHandle handle, int offsetLow, int offsetHigh, int countLow, int countHigh);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool LookupAccountName(string machineName, string accountName, byte[] sid, ref int sidLen, StringBuilder domainName, ref int domainNameLen, out int peUse);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("advapi32.dll", EntryPoint="LookupPrivilegeValueW", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool LookupPrivilegeValue([In] string lpSystemName, [In] string lpName, [In, Out] ref LUID Luid);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true)]
        internal static extern int LsaClose(IntPtr handle);
        [DllImport("secur32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int LsaConnectUntrusted([In, Out] ref SafeLsaLogonProcessHandle LsaHandle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll", SetLastError=true)]
        internal static extern int LsaDeregisterLogonProcess(IntPtr handle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true)]
        internal static extern int LsaFreeMemory(IntPtr handle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll", SetLastError=true)]
        internal static extern int LsaFreeReturnBuffer(IntPtr handle);
        [DllImport("secur32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int LsaGetLogonSessionData([In] ref LUID LogonId, [In, Out] ref SafeLsaReturnBufferHandle ppLogonSessionData);
        [DllImport("secur32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int LsaLogonUser([In] SafeLsaLogonProcessHandle LsaHandle, [In] ref UNICODE_INTPTR_STRING OriginName, [In] uint LogonType, [In] uint AuthenticationPackage, [In] IntPtr AuthenticationInformation, [In] uint AuthenticationInformationLength, [In] IntPtr LocalGroups, [In] ref TOKEN_SOURCE SourceContext, [In, Out] ref SafeLsaReturnBufferHandle ProfileBuffer, [In, Out] ref uint ProfileBufferLength, [In, Out] ref LUID LogonId, [In, Out] ref SafeTokenHandle Token, [In, Out] ref QUOTA_LIMITS Quotas, [In, Out] ref int SubStatus);
        [DllImport("secur32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int LsaLookupAuthenticationPackage([In] SafeLsaLogonProcessHandle LsaHandle, [In] ref UNICODE_INTPTR_STRING PackageName, [In, Out] ref uint AuthenticationPackage);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint LsaLookupNames(SafeLsaPolicyHandle handle, int count, UNICODE_STRING[] names, ref SafeLsaMemoryHandle referencedDomains, ref SafeLsaMemoryHandle sids);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint LsaLookupNames2(SafeLsaPolicyHandle handle, int flags, int count, UNICODE_STRING[] names, ref SafeLsaMemoryHandle referencedDomains, ref SafeLsaMemoryHandle sids);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint LsaLookupSids(SafeLsaPolicyHandle handle, int count, IntPtr[] sids, ref SafeLsaMemoryHandle referencedDomains, ref SafeLsaMemoryHandle names);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int LsaNtStatusToWinError([In] int status);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint LsaOpenPolicy(string systemName, ref LSA_OBJECT_ATTRIBUTES attributes, int accessMask, out SafeLsaPolicyHandle handle);
        [DllImport("secur32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int LsaRegisterLogonProcess([In] ref UNICODE_INTPTR_STRING LogonProcessName, [In, Out] ref SafeLsaLogonProcessHandle LsaHandle, [In, Out] ref IntPtr SecurityMode);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
        internal static extern int lstrlenA(IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int lstrlenW(IntPtr ptr);
        internal static int MakeHRFromErrorCode(int errorCode)
        {
            return (-2147024896 | errorCode);
        }

        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern IntPtr MapViewOfFile(SafeFileMappingHandle handle, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumerOfBytesToMap);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool MoveFile(string src, string dst);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeWaitHandle OpenEvent(int desiredAccess, bool inheritHandle, string name);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeWaitHandle OpenMutex(int desiredAccess, bool inheritHandle, string name);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool OpenProcessToken([In] IntPtr ProcessToken, [In] TokenAccessLevels DesiredAccess, out SafeTokenHandle TokenHandle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool PeekConsoleInput(IntPtr hConsoleInput, out InputRecord buffer, int numInputRecords_UseOne, out int numEventsRead);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        internal static extern bool QueryPerformanceCounter(out long value);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        internal static extern bool QueryPerformanceFrequency(out long value);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool ReadConsoleInput(IntPtr hConsoleInput, out InputRecord buffer, int numInputRecords_UseOne, out int numEventsRead);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe bool ReadConsoleOutput(IntPtr hConsoleOutput, CHAR_INFO* pBuffer, COORD bufferSize, COORD bufferCoord, ref SMALL_RECT readRegion);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead_mustBeZero, NativeOverlapped* overlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegConnectRegistry(string machineName, SafeRegistryHandle key, out SafeRegistryHandle result);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegCreateKeyEx(SafeRegistryHandle hKey, string lpSubKey, int Reserved, string lpClass, int dwOptions, int samDesired, SECURITY_ATTRIBUTES lpSecurityAttributes, out SafeRegistryHandle hkResult, out int lpdwDisposition);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegDeleteKey(SafeRegistryHandle hKey, string lpSubKey);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegDeleteKeyEx(SafeRegistryHandle hKey, string lpSubKey, int samDesired, int Reserved);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegDeleteValue(SafeRegistryHandle hKey, string lpValueName);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegEnumKeyEx(SafeRegistryHandle hKey, int dwIndex, StringBuilder lpName, out int lpcbName, int[] lpReserved, StringBuilder lpClass, int[] lpcbClass, long[] lpftLastWriteTime);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegEnumValue(SafeRegistryHandle hKey, int dwIndex, StringBuilder lpValueName, ref int lpcbValueName, IntPtr lpReserved_MustBeZero, int[] lpType, byte[] lpData, int[] lpcbData);
        [DllImport("advapi32.dll")]
        internal static extern int RegFlushKey(SafeRegistryHandle hKey);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegOpenKeyEx(SafeRegistryHandle hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryInfoKey(SafeRegistryHandle hKey, StringBuilder lpClass, int[] lpcbClass, IntPtr lpReserved_MustBeZero, ref int lpcSubKeys, int[] lpcbMaxSubKeyLen, int[] lpcbMaxClassLen, ref int lpcValues, int[] lpcbMaxValueNameLen, int[] lpcbMaxValueLen, int[] lpcbSecurityDescriptor, int[] lpftLastWriteTime);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] byte[] lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, ref int lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, ref long lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] char[] lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, StringBuilder lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, byte[] lpData, int cbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, ref int lpData, int cbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, ref long lpData, int cbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, string lpData, int cbData);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool ReleaseMutex(SafeWaitHandle handle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool ReleaseSemaphore(SafeWaitHandle handle, int releaseCount, out int previousCount);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool RemoveDirectory(string path);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool ReplaceFile(string replacedFileName, string replacementFileName, string backupFileName, int dwReplaceFlags, IntPtr lpExclude, IntPtr lpReserved);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool ResetEvent(SafeWaitHandle handle);
        [SecurityCritical]
        internal static SafeFileHandle SafeCreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            SafeFileHandle handle = CreateFile(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            if (!handle.IsInvalid && (GetFileType(handle) != 1))
            {
                handle.Dispose();
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_FileStreamOnNonFiles"));
            }
            return handle;
        }

        [DllImport("user32.dll", SetLastError=true)]
        internal static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, IntPtr lpdwResult);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetConsoleCP(uint codePage);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerRoutine handler, bool addOrRemove);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetConsoleCursorInfo(IntPtr hConsoleOutput, ref CONSOLE_CURSOR_INFO cci);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD cursorPosition);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetConsoleOutputCP(uint codePage);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, COORD size);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, short attributes);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetConsoleTitle(string title);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe bool SetConsoleWindowInfo(IntPtr hConsoleOutput, bool absolute, SMALL_RECT* consoleWindow);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetCurrentDirectory(string path);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetEndOfFile(SafeFileHandle hFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetEnvironmentVariable(string lpName, string lpValue);
        [DllImport("kernel32.dll")]
        internal static extern int SetErrorMode(int newMode);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetEvent(SafeWaitHandle handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetFileAttributes(string name, int attr);
        [SecurityCritical]
        internal static unsafe long SetFilePointer(SafeFileHandle handle, long offset, SeekOrigin origin, out int hr)
        {
            hr = 0;
            int lo = (int) offset;
            int hi = (int) (offset >> 0x20);
            lo = SetFilePointerWin32(handle, lo, &hi, (int) origin);
            if ((lo == -1) && ((hr = Marshal.GetLastWin32Error()) != 0))
            {
                return -1L;
            }
            return (long) ((((ulong) hi) << 0x20) | ((ulong) lo));
        }

        [DllImport("kernel32.dll", EntryPoint="SetFilePointer", SetLastError=true)]
        private static extern unsafe int SetFilePointerWin32(SafeFileHandle handle, int lo, int* hi, int origin);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe bool SetFileTime(SafeFileHandle hFile, FILE_TIME* creationTime, FILE_TIME* lastAccessTime, FILE_TIME* lastWriteTime);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern void SetLastError(int errorCode);
        [DllImport("advapi32.dll", EntryPoint="SetSecurityInfo", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint SetSecurityInfoByHandle(SafeHandle handle, uint objectType, uint securityInformation, byte[] owner, byte[] group, byte[] dacl, byte[] sacl);
        [DllImport("advapi32.dll", EntryPoint="SetNamedSecurityInfoW", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern uint SetSecurityInfoByName(string name, uint objectType, uint securityInformation, byte[] owner, byte[] group, byte[] dacl, byte[] sacl);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetVolumeLabel(string driveLetter, string volumeName);
        [DllImport("shfolder.dll", CharSet=CharSet.Auto)]
        internal static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("oleaut32.dll")]
        internal static extern IntPtr SysAllocStringByteLen(byte[] str, uint len);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("oleaut32.dll", CharSet=CharSet.Unicode)]
        internal static extern IntPtr SysAllocStringLen(string src, int len);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oleaut32.dll")]
        internal static extern void SysFreeString(IntPtr bstr);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oleaut32.dll")]
        internal static extern uint SysStringByteLen(IntPtr bstr);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oleaut32.dll")]
        internal static extern int SysStringLen(IntPtr bstr);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oleaut32.dll")]
        internal static extern int SysStringLen(SafeBSTRHandle bstr);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int SystemFunction040([In, Out] SafeBSTRHandle pDataIn, [In] uint cbDataIn, [In] uint dwFlags);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int SystemFunction041([In, Out] SafeBSTRHandle pDataIn, [In] uint cbDataIn, [In] uint dwFlags);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool UnlockFile(SafeFileHandle handle, int offsetLow, int offsetHigh, int countLow, int countHigh);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
        [SecurityCritical]
        internal static SafeFileHandle UnsafeCreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            return CreateFile(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe void* VirtualAlloc(void* address, UIntPtr numBytes, int commitOrReserve, int pageProtectionMode);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe bool VirtualFree(void* address, UIntPtr numBytes, int pageFreeMode);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe IntPtr VirtualQuery(void* address, ref MEMORY_BASIC_INFORMATION buffer, IntPtr sizeOfBuffer);
        [DllImport("kernel32.dll")]
        internal static extern unsafe int WideCharToMultiByte(uint cp, uint flags, char* pwzSource, int cchSource, byte* pbDestBuffer, int cbDestBuffer, IntPtr null1, IntPtr null2);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe bool WriteConsoleOutput(IntPtr hConsoleOutput, CHAR_INFO* buffer, COORD bufferSize, COORD bufferCoord, ref SMALL_RECT writeRegion);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", EntryPoint="RtlZeroMemory", SetLastError=true)]
        internal static extern void ZeroMemory(IntPtr handle, uint length);

        [StructLayout(LayoutKind.Sequential)]
        internal struct CHAR_INFO
        {
            private ushort charData;
            private short attributes;
        }

        [Serializable, Flags]
        internal enum Color : short
        {
            BackgroundBlue = 0x10,
            BackgroundGreen = 0x20,
            BackgroundIntensity = 0x80,
            BackgroundMask = 240,
            BackgroundRed = 0x40,
            BackgroundYellow = 0x60,
            Black = 0,
            ColorMask = 0xff,
            ForegroundBlue = 1,
            ForegroundGreen = 2,
            ForegroundIntensity = 8,
            ForegroundMask = 15,
            ForegroundRed = 4,
            ForegroundYellow = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_CURSOR_INFO
        {
            internal int dwSize;
            internal bool bVisible;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_SCREEN_BUFFER_INFO
        {
            internal Win32Native.COORD dwSize;
            internal Win32Native.COORD dwCursorPosition;
            internal short wAttributes;
            internal Win32Native.SMALL_RECT srWindow;
            internal Win32Native.COORD dwMaximumWindowSize;
        }

        internal delegate bool ConsoleCtrlHandlerRoutine(int controlType);

        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct DynamicTimeZoneInformation
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
            public string StandardName;
            public Win32Native.SystemTime StandardDate;
            [MarshalAs(UnmanagedType.I4)]
            public int StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
            public string DaylightName;
            public Win32Native.SystemTime DaylightDate;
            [MarshalAs(UnmanagedType.I4)]
            public int DaylightBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            public string TimeZoneKeyName;
            [MarshalAs(UnmanagedType.Bool)]
            public bool DynamicDaylightTimeDisabled;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_TIME
        {
            internal uint ftTimeLow;
            internal uint ftTimeHigh;
            public FILE_TIME(long fileTime)
            {
                this.ftTimeLow = (uint) fileTime;
                this.ftTimeHigh = (uint) (fileTime >> 0x20);
            }

            public long ToTicks()
            {
                return (long) ((this.ftTimeHigh << 0x20) + this.ftTimeLow);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal struct InputRecord
        {
            internal short eventType;
            internal Win32Native.KeyEventRecord keyEvent;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct KERB_S4U_LOGON
        {
            internal uint MessageType;
            internal uint Flags;
            internal Win32Native.UNICODE_INTPTR_STRING ClientUpn;
            internal Win32Native.UNICODE_INTPTR_STRING ClientRealm;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal struct KeyEventRecord
        {
            internal bool keyDown;
            internal short repeatCount;
            internal short virtualKeyCode;
            internal short virtualScanCode;
            internal char uChar;
            internal int controlKeyState;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_OBJECT_ATTRIBUTES
        {
            internal int Length;
            internal IntPtr RootDirectory;
            internal IntPtr ObjectName;
            internal int Attributes;
            internal IntPtr SecurityDescriptor;
            internal IntPtr SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_REFERENCED_DOMAIN_LIST
        {
            internal int Entries;
            internal IntPtr Domains;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRANSLATED_NAME
        {
            internal int Use;
            internal Win32Native.UNICODE_INTPTR_STRING Name;
            internal int DomainIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRANSLATED_SID
        {
            internal int Use;
            internal uint Rid;
            internal int DomainIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRANSLATED_SID2
        {
            internal int Use;
            internal IntPtr Sid;
            internal int DomainIndex;
            private uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRUST_INFORMATION
        {
            internal Win32Native.UNICODE_INTPTR_STRING Name;
            internal IntPtr Sid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct LUID
        {
            internal uint LowPart;
            internal uint HighPart;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct LUID_AND_ATTRIBUTES
        {
            internal Win32Native.LUID Luid;
            internal uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MEMORY_BASIC_INFORMATION
        {
            internal unsafe void* BaseAddress;
            internal unsafe void* AllocationBase;
            internal uint AllocationProtect;
            internal UIntPtr RegionSize;
            internal uint State;
            internal uint Protect;
            internal uint Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class MEMORYSTATUSEX
        {
            internal int length;
            internal int memoryLoad;
            internal ulong totalPhys;
            internal ulong availPhys;
            internal ulong totalPageFile;
            internal ulong availPageFile;
            internal ulong totalVirtual;
            internal ulong availVirtual;
            internal ulong availExtendedVirtual;
            internal MEMORYSTATUSEX()
            {
                this.length = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class OSVERSIONINFO
        {
            internal int OSVersionInfoSize;
            internal int MajorVersion;
            internal int MinorVersion;
            internal int BuildNumber;
            internal int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            internal string CSDVersion;
            internal OSVERSIONINFO()
            {
                this.OSVersionInfoSize = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class OSVERSIONINFOEX
        {
            internal int OSVersionInfoSize;
            internal int MajorVersion;
            internal int MinorVersion;
            internal int BuildNumber;
            internal int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            internal string CSDVersion;
            internal ushort ServicePackMajor;
            internal ushort ServicePackMinor;
            internal short SuiteMask;
            internal byte ProductType;
            internal byte Reserved;
            public OSVERSIONINFOEX()
            {
                this.OSVersionInfoSize = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct QUOTA_LIMITS
        {
            internal IntPtr PagedPoolLimit;
            internal IntPtr NonPagedPoolLimit;
            internal IntPtr MinimumWorkingSetSize;
            internal IntPtr MaximumWorkingSetSize;
            internal IntPtr PagefileLimit;
            internal IntPtr TimeLimit;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RegistryTimeZoneInformation
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Bias;
            [MarshalAs(UnmanagedType.I4)]
            public int StandardBias;
            [MarshalAs(UnmanagedType.I4)]
            public int DaylightBias;
            public Win32Native.SystemTime StandardDate;
            public Win32Native.SystemTime DaylightDate;
            public RegistryTimeZoneInformation(Win32Native.TimeZoneInformation tzi)
            {
                this.Bias = tzi.Bias;
                this.StandardDate = tzi.StandardDate;
                this.StandardBias = tzi.StandardBias;
                this.DaylightDate = tzi.DaylightDate;
                this.DaylightBias = tzi.DaylightBias;
            }

            public RegistryTimeZoneInformation(byte[] bytes)
            {
                if ((bytes == null) || (bytes.Length != 0x2c))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidREG_TZI_FORMAT"), "bytes");
                }
                this.Bias = BitConverter.ToInt32(bytes, 0);
                this.StandardBias = BitConverter.ToInt32(bytes, 4);
                this.DaylightBias = BitConverter.ToInt32(bytes, 8);
                this.StandardDate.Year = BitConverter.ToInt16(bytes, 12);
                this.StandardDate.Month = BitConverter.ToInt16(bytes, 14);
                this.StandardDate.DayOfWeek = BitConverter.ToInt16(bytes, 0x10);
                this.StandardDate.Day = BitConverter.ToInt16(bytes, 0x12);
                this.StandardDate.Hour = BitConverter.ToInt16(bytes, 20);
                this.StandardDate.Minute = BitConverter.ToInt16(bytes, 0x16);
                this.StandardDate.Second = BitConverter.ToInt16(bytes, 0x18);
                this.StandardDate.Milliseconds = BitConverter.ToInt16(bytes, 0x1a);
                this.DaylightDate.Year = BitConverter.ToInt16(bytes, 0x1c);
                this.DaylightDate.Month = BitConverter.ToInt16(bytes, 30);
                this.DaylightDate.DayOfWeek = BitConverter.ToInt16(bytes, 0x20);
                this.DaylightDate.Day = BitConverter.ToInt16(bytes, 0x22);
                this.DaylightDate.Hour = BitConverter.ToInt16(bytes, 0x24);
                this.DaylightDate.Minute = BitConverter.ToInt16(bytes, 0x26);
                this.DaylightDate.Second = BitConverter.ToInt16(bytes, 40);
                this.DaylightDate.Milliseconds = BitConverter.ToInt16(bytes, 0x2a);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength;
            internal unsafe byte* pSecurityDescriptor = null;
            internal int bInheritHandle;
        }

        internal enum SECURITY_IMPERSONATION_LEVEL
        {
            Anonymous,
            Identification,
            Impersonation,
            Delegation
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct SECURITY_LOGON_SESSION_DATA
        {
            internal uint Size;
            internal Win32Native.LUID LogonId;
            internal Win32Native.UNICODE_INTPTR_STRING UserName;
            internal Win32Native.UNICODE_INTPTR_STRING LogonDomain;
            internal Win32Native.UNICODE_INTPTR_STRING AuthenticationPackage;
            internal uint LogonType;
            internal uint Session;
            internal IntPtr Sid;
            internal long LogonTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct SID_AND_ATTRIBUTES
        {
            internal IntPtr Sid;
            internal uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            internal short Left;
            internal short Top;
            internal short Right;
            internal short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            internal int dwOemId;
            internal int dwPageSize;
            internal IntPtr lpMinimumApplicationAddress;
            internal IntPtr lpMaximumApplicationAddress;
            internal IntPtr dwActiveProcessorMask;
            internal int dwNumberOfProcessors;
            internal int dwProcessorType;
            internal int dwAllocationGranularity;
            internal short wProcessorLevel;
            internal short wProcessorRevision;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SystemTime
        {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;
            [MarshalAs(UnmanagedType.U2)]
            public short Month;
            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)]
            public short Day;
            [MarshalAs(UnmanagedType.U2)]
            public short Hour;
            [MarshalAs(UnmanagedType.U2)]
            public short Minute;
            [MarshalAs(UnmanagedType.U2)]
            public short Second;
            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TimeZoneInformation
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
            public string StandardName;
            public Win32Native.SystemTime StandardDate;
            [MarshalAs(UnmanagedType.I4)]
            public int StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
            public string DaylightName;
            public Win32Native.SystemTime DaylightDate;
            [MarshalAs(UnmanagedType.I4)]
            public int DaylightBias;
            public TimeZoneInformation(Win32Native.DynamicTimeZoneInformation dtzi)
            {
                this.Bias = dtzi.Bias;
                this.StandardName = dtzi.StandardName;
                this.StandardDate = dtzi.StandardDate;
                this.StandardBias = dtzi.StandardBias;
                this.DaylightName = dtzi.DaylightName;
                this.DaylightDate = dtzi.DaylightDate;
                this.DaylightBias = dtzi.DaylightBias;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_GROUPS
        {
            internal uint GroupCount;
            internal Win32Native.SID_AND_ATTRIBUTES Groups;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_PRIVILEGE
        {
            internal uint PrivilegeCount;
            internal Win32Native.LUID_AND_ATTRIBUTES Privilege;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_SOURCE
        {
            private const int TOKEN_SOURCE_LENGTH = 8;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            internal char[] Name;
            internal Win32Native.LUID SourceIdentifier;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_STATISTICS
        {
            internal Win32Native.LUID TokenId;
            internal Win32Native.LUID AuthenticationId;
            internal long ExpirationTime;
            internal uint TokenType;
            internal uint ImpersonationLevel;
            internal uint DynamicCharged;
            internal uint DynamicAvailable;
            internal uint GroupCount;
            internal uint PrivilegeCount;
            internal Win32Native.LUID ModifiedId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct TOKEN_USER
        {
            internal Win32Native.SID_AND_ATTRIBUTES User;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct UNICODE_INTPTR_STRING
        {
            internal ushort Length;
            internal ushort MaxLength;
            internal IntPtr Buffer;
            [SecurityCritical]
            internal UNICODE_INTPTR_STRING(int stringBytes, SafeLocalAllocHandle buffer)
            {
                this.Length = (ushort) stringBytes;
                this.MaxLength = (ushort) buffer.ByteLength;
                this.Buffer = buffer.DangerousGetHandle();
            }

            internal UNICODE_INTPTR_STRING(int stringBytes, IntPtr buffer)
            {
                this.Length = (ushort) stringBytes;
                this.MaxLength = (ushort) stringBytes;
                this.Buffer = buffer;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct UNICODE_STRING
        {
            internal ushort Length;
            internal ushort MaximumLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class USEROBJECTFLAGS
        {
            internal int fInherit;
            internal int fReserved;
            internal int dwFlags;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_FILE_ATTRIBUTE_DATA
        {
            internal int fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal int fileSizeHigh;
            internal int fileSizeLow;
            [SecurityCritical]
            internal void PopulateFrom(Win32Native.WIN32_FIND_DATA findData)
            {
                this.fileAttributes = findData.dwFileAttributes;
                this.ftCreationTimeLow = findData.ftCreationTime_dwLowDateTime;
                this.ftCreationTimeHigh = findData.ftCreationTime_dwHighDateTime;
                this.ftLastAccessTimeLow = findData.ftLastAccessTime_dwLowDateTime;
                this.ftLastAccessTimeHigh = findData.ftLastAccessTime_dwHighDateTime;
                this.ftLastWriteTimeLow = findData.ftLastWriteTime_dwLowDateTime;
                this.ftLastWriteTimeHigh = findData.ftLastWriteTime_dwHighDateTime;
                this.fileSizeHigh = findData.nFileSizeHigh;
                this.fileSizeLow = findData.nFileSizeLow;
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto), BestFitMapping(false)]
        internal class WIN32_FIND_DATA
        {
            internal int dwFileAttributes;
            internal uint ftCreationTime_dwLowDateTime;
            internal uint ftCreationTime_dwHighDateTime;
            internal uint ftLastAccessTime_dwLowDateTime;
            internal uint ftLastAccessTime_dwHighDateTime;
            internal uint ftLastWriteTime_dwLowDateTime;
            internal uint ftLastWriteTime_dwHighDateTime;
            internal int nFileSizeHigh;
            internal int nFileSizeLow;
            internal int dwReserved0;
            internal int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            internal string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
            internal string cAlternateFileName;
        }
    }
}

