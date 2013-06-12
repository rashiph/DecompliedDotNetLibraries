namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal static class UnsafeNativeMethods
    {
        public const int FILE_ACTION_ADDED = 1;
        public const int FILE_ACTION_MODIFIED = 3;
        public const int FILE_ACTION_REMOVED = 2;
        public const int FILE_ACTION_RENAMED_NEW_NAME = 5;
        public const int FILE_ACTION_RENAMED_OLD_NAME = 4;
        public const int FILE_ADD_FILE = 2;
        public const int FILE_ADD_SUBDIRECTORY = 4;
        public const int FILE_APPEND_DATA = 4;
        public const int FILE_ATTRIBUTE_ARCHIVE = 0x20;
        public const int FILE_ATTRIBUTE_COMPRESSED = 0x800;
        public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        public const int FILE_ATTRIBUTE_HIDDEN = 2;
        public const int FILE_ATTRIBUTE_NORMAL = 0x80;
        public const int FILE_ATTRIBUTE_OFFLINE = 0x1000;
        public const int FILE_ATTRIBUTE_READONLY = 1;
        public const int FILE_ATTRIBUTE_SYSTEM = 4;
        public const int FILE_ATTRIBUTE_TEMPORARY = 0x100;
        public const int FILE_CASE_PRESERVED_NAMES = 2;
        public const int FILE_CASE_SENSITIVE_SEARCH = 1;
        public const int FILE_CREATE_PIPE_INSTANCE = 4;
        public const int FILE_DELETE_CHILD = 0x40;
        public const int FILE_EXECUTE = 0x20;
        public const int FILE_FILE_COMPRESSION = 0x10;
        public const int FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
        public const int FILE_FLAG_DELETE_ON_CLOSE = 0x4000000;
        public const int FILE_FLAG_NO_BUFFERING = 0x20000000;
        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int FILE_FLAG_POSIX_SEMANTICS = 0x1000000;
        public const int FILE_FLAG_RANDOM_ACCESS = 0x10000000;
        public const int FILE_FLAG_SEQUENTIAL_SCAN = 0x8000000;
        public const int FILE_FLAG_WRITE_THROUGH = -2147483648;
        public const int FILE_LIST_DIRECTORY = 1;
        public const int FILE_NOTIFY_CHANGE_ATTRIBUTES = 4;
        public const int FILE_NOTIFY_CHANGE_CREATION = 0x40;
        public const int FILE_NOTIFY_CHANGE_DIR_NAME = 2;
        public const int FILE_NOTIFY_CHANGE_FILE_NAME = 1;
        public const int FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x20;
        public const int FILE_NOTIFY_CHANGE_LAST_WRITE = 0x10;
        public const int FILE_NOTIFY_CHANGE_SECURITY = 0x100;
        public const int FILE_NOTIFY_CHANGE_SIZE = 8;
        public const int FILE_PERSISTENT_ACLS = 8;
        public const int FILE_READ_ATTRIBUTES = 0x80;
        public const int FILE_READ_DATA = 1;
        public const int FILE_READ_EA = 8;
        public const int FILE_SHARE_DELETE = 4;
        public const int FILE_SHARE_READ = 1;
        public const int FILE_SHARE_WRITE = 2;
        public const int FILE_TRAVERSE = 0x20;
        public const int FILE_TYPE_CHAR = 2;
        public const int FILE_TYPE_DISK = 1;
        public const int FILE_TYPE_PIPE = 3;
        public const int FILE_TYPE_REMOTE = 0x8000;
        public const int FILE_TYPE_UNKNOWN = 0;
        public const int FILE_UNICODE_ON_DISK = 4;
        public const int FILE_VOLUME_IS_COMPRESSED = 0x8000;
        public const int FILE_WRITE_ATTRIBUTES = 0x100;
        public const int FILE_WRITE_DATA = 2;
        public const int FILE_WRITE_EA = 0x10;
        public const int GetFileExInfoStandard = 0;
        public const int OPEN_ALWAYS = 4;
        public const int OPEN_EXISTING = 3;

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool ClearCommBreak(SafeFileHandle hFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool ClearCommError(SafeFileHandle hFile, ref int lpErrors, ref COMSTAT lpStat);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool ClearCommError(SafeFileHandle hFile, ref int lpErrors, IntPtr lpStat);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool ClearEventLog(SafeHandle hEventLog, HandleRef lpctstrBackupFileName);
        [DllImport("ole32.dll")]
        internal static extern int CoGetMarshalSizeMax(out int pulSize, ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags);
        [DllImport("ole32.dll")]
        internal static extern int CoGetStandardMarshal(ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags, out IntPtr ppMarshal);
        [DllImport("ole32.dll")]
        internal static extern int CoMarshalInterface([MarshalAs(UnmanagedType.Interface)] object pStm, ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr securityAttrs, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr CreateWindowEx(int exStyle, string lpszClassName, string lpszWindowName, int style, int x, int y, int width, int height, HandleRef hWndParent, HandleRef hMenu, HandleRef hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool DestroyWindow(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int DispatchMessage([In] ref Microsoft.Win32.NativeMethods.MSG msg);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool EscapeCommFunction(SafeFileHandle hFile, int dwFunc);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool FlushFileBuffers(SafeFileHandle hFile);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetClassInfo(HandleRef hInst, string lpszClass, [In, Out] Microsoft.Win32.NativeMethods.WNDCLASS_I wc);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetCommModemStatus(SafeFileHandle hFile, ref int lpModemStat);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetCommProperties(SafeFileHandle hFile, ref COMMPROP lpCommProp);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetCommState(SafeFileHandle hFile, ref DCB lpDCB);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int GetFileType(SafeFileHandle hFile);
        [DllImport("version.dll", CharSet=CharSet.Auto)]
        public static extern bool GetFileVersionInfo(string lptstrFilename, int dwHandle, int dwLen, HandleRef lpData);
        [DllImport("version.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetFileVersionInfoSize(string lptstrFilename, out int handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string modName);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool GetNumberOfEventLogRecords(SafeHandle hEventLog, out int count);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool GetOldestEventLogRecord(SafeHandle hEventLog, out int number);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern unsafe bool GetOverlappedResult(SafeFileHandle hFile, NativeOverlapped* lpOverlapped, ref int lpNumberOfBytesTransferred, bool bWait);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(HandleRef hModule, string lpProcName);
        [DllImport("user32.dll", ExactSpelling=true)]
        public static extern IntPtr GetProcessWindowStation();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetStdHandle(int type);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool GetUserObjectInformation(HandleRef hObj, int nIndex, [MarshalAs(UnmanagedType.LPStruct)] Microsoft.Win32.NativeMethods.USEROBJECTFLAGS pvBuffer, int nLength, ref int lpnLengthNeeded);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsWindow(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool KillTimer(HandleRef hwnd, HandleRef idEvent);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int LookupAccountSid(string systemName, byte[] pSid, StringBuilder szUserName, ref int userNameSize, StringBuilder szDomainName, ref int domainNameSize, ref int eUse);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int MsgWaitForMultipleObjectsEx(int nCount, IntPtr pHandles, int dwMilliseconds, int dwWakeMask, int dwFlags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool NotifyChangeEventLog(SafeHandle hEventLog, SafeWaitHandle hEvent);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool PeekMessage([In, Out] ref Microsoft.Win32.NativeMethods.MSG msg, HandleRef hwnd, int msgMin, int msgMax, int remove);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool PostMessage(HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool PurgeComm(SafeFileHandle hFile, uint dwFlags);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern unsafe bool ReadDirectoryChangesW(SafeFileHandle hDirectory, HandleRef lpBuffer, int nBufferLength, int bWatchSubtree, int dwNotifyFilter, out int lpBytesReturned, NativeOverlapped* overlappedPointer, HandleRef lpCompletionRoutine);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool ReadEventLog(SafeHandle hEventLog, int dwReadFlags, int dwRecordOffset, byte[] buffer, int numberOfBytesToRead, out int bytesRead, out int minNumOfBytesNeeded);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead, NativeOverlapped* overlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr overlapped);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern short RegisterClass(Microsoft.Win32.NativeMethods.WNDCLASS wc);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool ReportEvent(SafeHandle hEventLog, short type, ushort category, uint eventID, byte[] userSID, short numStrings, int dataLen, HandleRef strings, byte[] rawData);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public static IntPtr SetClassLong(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetClassLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint="SetClassLong", CharSet=CharSet.Auto)]
        public static extern IntPtr SetClassLongPtr32(HandleRef hwnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", EntryPoint="SetClassLongPtr", CharSet=CharSet.Auto)]
        public static extern IntPtr SetClassLongPtr64(HandleRef hwnd, int nIndex, IntPtr dwNewLong);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetCommBreak(SafeFileHandle hFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetCommMask(SafeFileHandle hFile, int dwEvtMask);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetCommState(SafeFileHandle hFile, ref DCB lpDCB);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetCommTimeouts(SafeFileHandle hFile, ref COMMTIMEOUTS lpCommTimeouts);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern bool SetConsoleCtrlHandler(Microsoft.Win32.NativeMethods.ConHndlr handler, int add);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SetTimer(HandleRef hWnd, HandleRef nIDEvent, int uElapse, HandleRef lpTimerProc);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetupComm(SafeFileHandle hFile, int dwInQueue, int dwOutQueue);
        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, HandleRef dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint="SetWindowLong", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, HandleRef dwNewLong);
        [DllImport("user32.dll", EntryPoint="SetWindowLongPtr", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, HandleRef dwNewLong);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool TranslateMessage([In, Out] ref Microsoft.Win32.NativeMethods.MSG msg);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern short UnregisterClass(string lpClassName, HandleRef hInstance);
        [DllImport("version.dll", CharSet=CharSet.Auto)]
        public static extern int VerLanguageName(int langID, StringBuilder lpBuffer, int nSize);
        [DllImport("version.dll", CharSet=CharSet.Auto)]
        public static extern bool VerQueryValue(HandleRef pBlock, string lpSubBlock, [In, Out] ref IntPtr lplpBuffer, out int len);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe bool WaitCommEvent(SafeFileHandle hFile, int* lpEvtMask, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr lpOverlapped);
        [DllImport("wtsapi32.dll", CharSet=CharSet.Auto)]
        public static extern bool WTSRegisterSessionNotification(HandleRef hWnd, int dwFlags);
        [DllImport("wtsapi32.dll", CharSet=CharSet.Auto)]
        public static extern bool WTSUnRegisterSessionNotification(HandleRef hWnd);

        [StructLayout(LayoutKind.Sequential)]
        internal struct COMMPROP
        {
            public ushort wPacketLength;
            public ushort wPacketVersion;
            public int dwServiceMask;
            public int dwReserved1;
            public int dwMaxTxQueue;
            public int dwMaxRxQueue;
            public int dwMaxBaud;
            public int dwProvSubType;
            public int dwProvCapabilities;
            public int dwSettableParams;
            public int dwSettableBaud;
            public ushort wSettableData;
            public ushort wSettableStopParity;
            public int dwCurrentTxQueue;
            public int dwCurrentRxQueue;
            public int dwProvSpec1;
            public int dwProvSpec2;
            public char wcProvChar;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COMMTIMEOUTS
        {
            public int ReadIntervalTimeout;
            public int ReadTotalTimeoutMultiplier;
            public int ReadTotalTimeoutConstant;
            public int WriteTotalTimeoutMultiplier;
            public int WriteTotalTimeoutConstant;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COMSTAT
        {
            public uint Flags;
            public uint cbInQue;
            public uint cbOutQue;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DCB
        {
            public uint DCBlength;
            public uint BaudRate;
            public uint Flags;
            public ushort wReserved;
            public ushort XonLim;
            public ushort XoffLim;
            public byte ByteSize;
            public byte Parity;
            public byte StopBits;
            public byte XonChar;
            public byte XoffChar;
            public byte ErrorChar;
            public byte EofChar;
            public byte EvtChar;
            public ushort wReserved1;
        }

        [ComImport, Guid("00000003-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMarshal
        {
            [PreserveSig]
            int GetUnmarshalClass(ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags, out Guid pCid);
            [PreserveSig]
            int GetMarshalSizeMax(ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags, out int pSize);
            [PreserveSig]
            int MarshalInterface([MarshalAs(UnmanagedType.Interface)] object pStm, ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags);
            [PreserveSig]
            int UnmarshalInterface([MarshalAs(UnmanagedType.Interface)] object pStm, ref Guid riid, out IntPtr ppv);
            [PreserveSig]
            int ReleaseMarshalData([MarshalAs(UnmanagedType.Interface)] object pStm);
            [PreserveSig]
            int DisconnectObject(int dwReserved);
        }

        [ComImport, Guid("00000017-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IStdMarshal
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32_FILE_ATTRIBUTE_DATA
        {
            internal int fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint fileSizeHigh;
            internal uint fileSizeLow;
        }
    }
}

