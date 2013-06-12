namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Diagnostics.Eventing;
    using System.Diagnostics.Eventing.Reader;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        internal const string ADVAPI32 = "advapi32.dll";
        internal const int DUPLICATE_SAME_ACCESS = 2;
        internal const int ERROR_ACCESS_DENIED = 5;
        internal const int ERROR_ALREADY_EXISTS = 0xb7;
        internal const int ERROR_ARITHMETIC_OVERFLOW = 0x216;
        internal const int ERROR_BAD_LENGTH = 0x18;
        internal const int ERROR_BAD_PATHNAME = 0xa1;
        internal const int ERROR_BROKEN_PIPE = 0x6d;
        internal const int ERROR_ENVVAR_NOT_FOUND = 0xcb;
        internal const int ERROR_FILE_EXISTS = 80;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_FILENAME_EXCED_RANGE = 0xce;
        internal const int ERROR_HANDLE_EOF = 0x26;
        internal const int ERROR_INVALID_DRIVE = 15;
        internal const int ERROR_INVALID_HANDLE = 6;
        internal const int ERROR_INVALID_NAME = 0x7b;
        internal const int ERROR_INVALID_PARAMETER = 0x57;
        internal const int ERROR_IO_PENDING = 0x3e5;
        internal const int ERROR_MORE_DATA = 0xea;
        internal const int ERROR_NO_DATA = 0xe8;
        internal const int ERROR_NO_MORE_FILES = 0x12;
        internal const int ERROR_NOT_ENOUGH_MEMORY = 8;
        internal const int ERROR_NOT_FOUND = 0x490;
        internal const int ERROR_NOT_READY = 0x15;
        internal const int ERROR_OPERATION_ABORTED = 0x3e3;
        internal const int ERROR_PATH_NOT_FOUND = 3;
        internal const int ERROR_PIPE_BUSY = 0xe7;
        internal const int ERROR_PIPE_CONNECTED = 0x217;
        internal const int ERROR_PIPE_LISTENING = 0x218;
        internal const int ERROR_PIPE_NOT_CONNECTED = 0xe9;
        internal const int ERROR_SHARING_VIOLATION = 0x20;
        internal const int ERROR_SUCCESS = 0;
        internal const int FILE_ATTRIBUTE_NORMAL = 0x80;
        internal const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x80000;
        internal const int FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const int FILE_MAP_COPY = 1;
        internal const int FILE_MAP_EXECUTE = 0x20;
        internal const int FILE_MAP_READ = 4;
        internal const int FILE_MAP_WRITE = 2;
        internal const int FILE_SHARE_READ = 1;
        internal const int FILE_SHARE_WRITE = 2;
        internal const int FILE_TYPE_CHAR = 2;
        internal const int FILE_TYPE_DISK = 1;
        internal const int FILE_TYPE_PIPE = 3;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        internal const int GENERIC_READ = -2147483648;
        internal const int GENERIC_WRITE = 0x40000000;
        internal const int INVALID_FILE_SIZE = -1;
        internal const string KERNEL32 = "kernel32.dll";
        internal const int MEM_COMMIT = 0x1000;
        internal const int MEM_RESERVE = 0x2000;
        internal static readonly IntPtr NULL = IntPtr.Zero;
        internal const int OPEN_EXISTING = 3;
        internal const int PAGE_EXECUTE_READ = 0x20;
        internal const int PAGE_EXECUTE_READWRITE = 0x40;
        internal const int PAGE_READONLY = 2;
        internal const int PAGE_READWRITE = 4;
        internal const int PAGE_WRITECOPY = 8;
        internal const int PIPE_ACCESS_DUPLEX = 3;
        internal const int PIPE_ACCESS_INBOUND = 1;
        internal const int PIPE_ACCESS_OUTBOUND = 2;
        internal const int PIPE_READMODE_BYTE = 0;
        internal const int PIPE_READMODE_MESSAGE = 2;
        internal const int PIPE_TYPE_BYTE = 0;
        internal const int PIPE_TYPE_MESSAGE = 4;
        internal const int PIPE_UNLIMITED_INSTANCES = 0xff;
        internal const int SECURITY_ANONYMOUS = 0;
        internal const int SECURITY_DELEGATION = 0x30000;
        internal const int SECURITY_IDENTIFICATION = 0x10000;
        internal const int SECURITY_IMPERSONATION = 0x20000;
        internal const int SECURITY_SQOS_PRESENT = 0x100000;
        internal const int SEM_FAILCRITICALERRORS = 1;
        internal const int STD_ERROR_HANDLE = -12;
        internal const int STD_INPUT_HANDLE = -10;
        internal const int STD_OUTPUT_HANDLE = -11;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool CloseHandle(IntPtr handle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe bool ConnectNamedPipe(SafePipeHandle handle, NativeOverlapped* overlapped);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool ConnectNamedPipe(SafePipeHandle handle, IntPtr overlapped);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeMemoryMappedFileHandle CreateFileMapping(SafeFileHandle hFile, SECURITY_ATTRIBUTES lpAttributes, int fProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafePipeHandle CreateNamedPipe(string pipeName, int openMode, int pipeMode, int maxInstances, int outBufferSize, int inBufferSize, int defaultTimeout, SECURITY_ATTRIBUTES securityAttributes);
        [DllImport("kernel32.dll", EntryPoint="CreateFile", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafePipeHandle CreateNamedPipeClient(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CreatePipe(out SafePipeHandle hReadPipe, out SafePipeHandle hWritePipe, SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool DisconnectNamedPipe(SafePipeHandle hNamedPipe);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, SafePipeHandle hSourceHandle, IntPtr hTargetProcessHandle, out SafePipeHandle lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint EventActivityIdControl([In] int ControlCode, [In, Out] ref Guid ActivityId);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int EventEnabled([In] long registrationHandle, [In] ref EventDescriptor eventDescriptor);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int EventProviderEnabled([In] long registrationHandle, [In] byte level, [In] long keywords);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventRegister([In] ref Guid providerId, [In] EtwEnableCallback enableCallback, [In] void* callbackContext, [In, Out] ref long registrationHandle);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int EventUnregister([In] long registrationHandle);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWrite([In] long registrationHandle, [In] ref EventDescriptor eventDescriptor, [In] uint userDataCount, [In] void* userData);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWrite([In] long registrationHandle, [In] EventDescriptor* eventDescriptor, [In] uint userDataCount, [In] void* userData);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWriteString([In] long registrationHandle, [In] byte level, [In] long keywords, [In] char* message);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWriteTransfer([In] long registrationHandle, [In] ref EventDescriptor eventDescriptor, [In] Guid* activityId, [In] Guid* relatedActivityId, [In] uint userDataCount, [In] void* userData);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtArchiveExportedLog(EventLogHandle session, [MarshalAs(UnmanagedType.LPWStr)] string logFilePath, int locale, int flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", SetLastError=true)]
        internal static extern bool EvtCancel(EventLogHandle handle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtClearLog(EventLogHandle session, [MarshalAs(UnmanagedType.LPWStr)] string channelPath, [MarshalAs(UnmanagedType.LPWStr)] string targetFilePath, int flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("wevtapi.dll")]
        internal static extern bool EvtClose(IntPtr handle);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtCreateBookmark([MarshalAs(UnmanagedType.LPWStr)] string bookmarkXml);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtCreateRenderContext(int valuePathsCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr)] string[] valuePaths, [MarshalAs(UnmanagedType.I4)] EvtRenderContextFlags flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtExportLog(EventLogHandle session, [MarshalAs(UnmanagedType.LPWStr)] string channelPath, [MarshalAs(UnmanagedType.LPWStr)] string query, [MarshalAs(UnmanagedType.LPWStr)] string targetFilePath, int flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtFormatMessage(EventLogHandle publisherMetadataHandle, EventLogHandle eventHandle, uint messageId, int valueCount, EvtStringVariant[] values, [MarshalAs(UnmanagedType.I4)] EvtFormatMessageFlags flags, int bufferSize, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, out int bufferUsed);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", EntryPoint="EvtFormatMessage", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtFormatMessageBuffer(EventLogHandle publisherMetadataHandle, EventLogHandle eventHandle, uint messageId, int valueCount, IntPtr values, [MarshalAs(UnmanagedType.I4)] EvtFormatMessageFlags flags, int bufferSize, IntPtr buffer, out int bufferUsed);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtGetChannelConfigProperty(EventLogHandle channelConfig, [MarshalAs(UnmanagedType.I4)] EvtChannelConfigPropertyId propertyId, int flags, int propertyValueBufferSize, IntPtr propertyValueBuffer, out int propertyValueBufferUsed);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtGetEventInfo(EventLogHandle eventHandle, [MarshalAs(UnmanagedType.I4)] EvtEventPropertyId propertyId, int bufferSize, IntPtr bufferPtr, out int bufferUsed);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtGetEventMetadataProperty(EventLogHandle eventMetadata, [MarshalAs(UnmanagedType.I4)] EvtEventMetadataPropertyId propertyId, int flags, int eventMetadataPropertyBufferSize, IntPtr eventMetadataPropertyBuffer, out int eventMetadataPropertyBufferUsed);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtGetLogInfo(EventLogHandle log, [MarshalAs(UnmanagedType.I4)] EvtLogPropertyId propertyId, int propertyValueBufferSize, IntPtr propertyValueBuffer, out int propertyValueBufferUsed);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtGetObjectArrayProperty(EventLogHandle objectArray, int propertyId, int arrayIndex, int flags, int propertyValueBufferSize, IntPtr propertyValueBuffer, out int propertyValueBufferUsed);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtGetObjectArraySize(EventLogHandle objectArray, out int objectArraySize);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtGetPublisherMetadataProperty(EventLogHandle publisherMetadataHandle, [MarshalAs(UnmanagedType.I4)] EvtPublisherMetadataPropertyId propertyId, int flags, int publisherMetadataPropertyBufferSize, IntPtr publisherMetadataPropertyBuffer, out int publisherMetadataPropertyBufferUsed);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtGetQueryInfo(EventLogHandle queryHandle, [MarshalAs(UnmanagedType.I4)] EvtQueryPropertyId propertyId, int bufferSize, IntPtr buffer, ref int bufferRequired);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", SetLastError=true)]
        internal static extern bool EvtNext(EventLogHandle queryHandle, int eventSize, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] events, int timeout, int flags, ref int returned);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtNextChannelPath(EventLogHandle channelEnum, int channelPathBufferSize, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder channelPathBuffer, out int channelPathBufferUsed);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtNextEventMetadata(EventLogHandle eventMetadataEnum, int flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtNextPublisherId(EventLogHandle publisherEnum, int publisherIdBufferSize, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder publisherIdBuffer, out int publisherIdBufferUsed);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtOpenChannelConfig(EventLogHandle session, [MarshalAs(UnmanagedType.LPWStr)] string channelPath, int flags);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtOpenChannelEnum(EventLogHandle session, int flags);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtOpenEventMetadataEnum(EventLogHandle publisherMetadata, int flags);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtOpenLog(EventLogHandle session, [MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.I4)] PathType flags);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtOpenPublisherEnum(EventLogHandle session, int flags);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtOpenPublisherMetadata(EventLogHandle session, [MarshalAs(UnmanagedType.LPWStr)] string publisherId, [MarshalAs(UnmanagedType.LPWStr)] string logFilePath, int locale, int flags);
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern EventLogHandle EvtOpenSession([MarshalAs(UnmanagedType.I4)] EvtLoginClass loginClass, ref EvtRpcLogin login, int timeout, int flags);
        [DllImport("wevtapi.dll", SetLastError=true)]
        internal static extern EventLogHandle EvtQuery(EventLogHandle session, [MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] string query, int flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", SetLastError=true)]
        internal static extern bool EvtRender(EventLogHandle context, EventLogHandle eventHandle, EvtRenderFlags flags, int buffSize, IntPtr buffer, out int buffUsed, out int propCount);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", SetLastError=true)]
        internal static extern bool EvtRender(EventLogHandle context, EventLogHandle eventHandle, EvtRenderFlags flags, int buffSize, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, out int buffUsed, out int propCount);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtSaveChannelConfig(EventLogHandle channelConfig, int flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtSeek(EventLogHandle resultSet, long position, EventLogHandle bookmark, int timeout, [MarshalAs(UnmanagedType.I4)] EvtSeekFlags flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtSetChannelConfigProperty(EventLogHandle channelConfig, [MarshalAs(UnmanagedType.I4)] EvtChannelConfigPropertyId propertyId, int flags, ref EvtVariant propertyValue);
        [DllImport("wevtapi.dll", SetLastError=true)]
        internal static extern EventLogHandle EvtSubscribe(EventLogHandle session, SafeWaitHandle signalEvent, [MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] string query, EventLogHandle bookmark, IntPtr context, IntPtr callback, int flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wevtapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool EvtUpdateBookmark(EventLogHandle bookmark, EventLogHandle eventHandle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool FlushFileBuffers(SafePipeHandle hNamedPipe);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe bool FlushViewOfFile(byte* lpBaseAddress, IntPtr dwNumberOfBytesToFlush);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int GetFileSize(SafeMemoryMappedFileHandle hFile, out int highSize);
        [DllImport("kernel32.dll")]
        internal static extern int GetFileType(SafeFileHandle handle);
        [DllImport("kernel32.dll")]
        internal static extern int GetFileType(SafePipeHandle handle);
        [SecurityCritical]
        internal static string GetMessage(int errorCode)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            if (FormatMessage(0x3200, NULL, errorCode, 0, lpBuffer, lpBuffer.Capacity, NULL) != 0)
            {
                return lpBuffer.ToString();
            }
            return ("UnknownError_Num " + errorCode);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetNamedPipeHandleState(SafePipeHandle hNamedPipe, out int lpState, IntPtr lpCurInstances, IntPtr lpMaxCollectionCount, IntPtr lpCollectDataTimeout, IntPtr lpUserName, int nMaxUserNameSize);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetNamedPipeHandleState(SafePipeHandle hNamedPipe, IntPtr lpState, out int lpCurInstances, IntPtr lpMaxCollectionCount, IntPtr lpCollectDataTimeout, IntPtr lpUserName, int nMaxUserNameSize);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetNamedPipeHandleState(SafePipeHandle hNamedPipe, IntPtr lpState, IntPtr lpCurInstances, IntPtr lpMaxCollectionCount, IntPtr lpCollectDataTimeout, StringBuilder lpUserName, int nMaxUserNameSize);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetNamedPipeInfo(SafePipeHandle hNamedPipe, out int lpFlags, IntPtr lpOutBufferSize, IntPtr lpInBufferSize, IntPtr lpMaxInstances);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetNamedPipeInfo(SafePipeHandle hNamedPipe, IntPtr lpFlags, out int lpOutBufferSize, IntPtr lpInBufferSize, IntPtr lpMaxInstances);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetNamedPipeInfo(SafePipeHandle hNamedPipe, IntPtr lpFlags, IntPtr lpOutBufferSize, out int lpInBufferSize, IntPtr lpMaxInstances);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool ImpersonateNamedPipeClient(SafePipeHandle hNamedPipe);
        [SecurityCritical, DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern Microsoft.Win32.SafeLibraryHandle LoadLibraryEx(string libFilename, IntPtr reserved, int flags);
        internal static int MakeHRFromErrorCode(int errorCode)
        {
            return (-2147024896 | errorCode);
        }

        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern SafeMemoryMappedViewHandle MapViewOfFile(SafeMemoryMappedFileHandle handle, int dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeMemoryMappedFileHandle OpenFileMapping(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe PerfCounterSetInstanceStruct* PerfCreateInstance([In] SafePerfProviderHandle hProvider, [In] ref Guid CounterSetGuid, [In] string szInstanceName, [In] uint dwInstance);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint PerfDeleteInstance([In] SafePerfProviderHandle hProvider, [In] PerfCounterSetInstanceStruct* InstanceBlock);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint PerfSetCounterRefValue([In] SafePerfProviderHandle hProvider, [In] PerfCounterSetInstanceStruct* pInstance, [In] uint CounterId, [In] void* lpAddr);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint PerfSetCounterSetInfo([In] SafePerfProviderHandle hProvider, [In, Out] PerfCounterSetInfoStruct* pTemplate, [In] uint dwTemplateSize);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint PerfStartProvider([In] ref Guid ProviderGuid, [In] PERFLIBREQUEST ControlCallback, out SafePerfProviderHandle phProvider);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint PerfStopProvider([In] IntPtr hProvider);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int ReadFile(SafePipeHandle handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead_mustBeZero, NativeOverlapped* overlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int ReadFile(SafePipeHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool RevertToSelf();
        [SecurityCritical]
        internal static SafeFileHandle SafeCreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            SafeFileHandle handle = CreateFile(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            if (!handle.IsInvalid && (GetFileType(handle) != 1))
            {
                handle.Dispose();
                throw new NotSupportedException(System.SR.GetString("NotSupported_IONonFileDevices"));
            }
            return handle;
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool SetEndOfFile(IntPtr hNamedPipe);
        [DllImport("kernel32.dll")]
        internal static extern int SetErrorMode(int newMode);
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
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern unsafe bool SetNamedPipeHandleState(SafePipeHandle hNamedPipe, int* lpMode, IntPtr lpMaxCollectionCount, IntPtr lpCollectDataTimeout);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr VirtualAlloc(SafeMemoryMappedViewHandle address, UIntPtr numBytes, int commitOrReserve, int pageProtectionMode);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr VirtualQuery(SafeMemoryMappedViewHandle address, ref MEMORY_BASIC_INFORMATION buffer, IntPtr sizeOfBuffer);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool WaitNamedPipe(string name, int timeout);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(SafePipeHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(SafePipeHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);

        internal unsafe delegate void EtwEnableCallback([In] ref Guid sourceId, [In] int isEnabled, [In] byte level, [In] long matchAnyKeywords, [In] long matchAllKeywords, [In] void* filterData, [In] void* callbackContext);

        internal enum EvtChannelConfigPropertyId
        {
            EvtChannelConfigEnabled,
            EvtChannelConfigIsolation,
            EvtChannelConfigType,
            EvtChannelConfigOwningPublisher,
            EvtChannelConfigClassicEventlog,
            EvtChannelConfigAccess,
            EvtChannelLoggingConfigRetention,
            EvtChannelLoggingConfigAutoBackup,
            EvtChannelLoggingConfigMaxSize,
            EvtChannelLoggingConfigLogFilePath,
            EvtChannelPublishingConfigLevel,
            EvtChannelPublishingConfigKeywords,
            EvtChannelPublishingConfigControlGuid,
            EvtChannelPublishingConfigBufferSize,
            EvtChannelPublishingConfigMinBuffers,
            EvtChannelPublishingConfigMaxBuffers,
            EvtChannelPublishingConfigLatency,
            EvtChannelPublishingConfigClockType,
            EvtChannelPublishingConfigSidType,
            EvtChannelPublisherList,
            EvtChannelConfigPropertyIdEND
        }

        internal enum EvtChannelReferenceFlags
        {
            EvtChannelReferenceImported = 1
        }

        internal enum EvtEventMetadataPropertyId
        {
            EventMetadataEventID,
            EventMetadataEventVersion,
            EventMetadataEventChannel,
            EventMetadataEventLevel,
            EventMetadataEventOpcode,
            EventMetadataEventTask,
            EventMetadataEventKeyword,
            EventMetadataEventMessageID,
            EventMetadataEventTemplate
        }

        internal enum EvtEventPropertyId
        {
            EvtEventQueryIDs,
            EvtEventPath
        }

        internal enum EvtExportLogFlags
        {
            EvtExportLogChannelPath = 1,
            EvtExportLogFilePath = 2,
            EvtExportLogTolerateQueryErrors = 0x1000
        }

        internal enum EvtFormatMessageFlags
        {
            EvtFormatMessageChannel = 6,
            EvtFormatMessageEvent = 1,
            EvtFormatMessageId = 8,
            EvtFormatMessageKeyword = 5,
            EvtFormatMessageLevel = 2,
            EvtFormatMessageOpcode = 4,
            EvtFormatMessageProvider = 7,
            EvtFormatMessageTask = 3,
            EvtFormatMessageXml = 9
        }

        internal enum EvtLoginClass
        {
            EvtRpcLogin = 1
        }

        internal enum EvtLogPropertyId
        {
            EvtLogCreationTime,
            EvtLogLastAccessTime,
            EvtLogLastWriteTime,
            EvtLogFileSize,
            EvtLogAttributes,
            EvtLogNumberOfLogRecords,
            EvtLogOldestRecordNumber,
            EvtLogFull
        }

        internal enum EvtMasks
        {
            EVT_VARIANT_TYPE_ARRAY = 0x80,
            EVT_VARIANT_TYPE_MASK = 0x7f
        }

        internal enum EvtPublisherMetadataPropertyId
        {
            EvtPublisherMetadataPublisherGuid,
            EvtPublisherMetadataResourceFilePath,
            EvtPublisherMetadataParameterFilePath,
            EvtPublisherMetadataMessageFilePath,
            EvtPublisherMetadataHelpLink,
            EvtPublisherMetadataPublisherMessageID,
            EvtPublisherMetadataChannelReferences,
            EvtPublisherMetadataChannelReferencePath,
            EvtPublisherMetadataChannelReferenceIndex,
            EvtPublisherMetadataChannelReferenceID,
            EvtPublisherMetadataChannelReferenceFlags,
            EvtPublisherMetadataChannelReferenceMessageID,
            EvtPublisherMetadataLevels,
            EvtPublisherMetadataLevelName,
            EvtPublisherMetadataLevelValue,
            EvtPublisherMetadataLevelMessageID,
            EvtPublisherMetadataTasks,
            EvtPublisherMetadataTaskName,
            EvtPublisherMetadataTaskEventGuid,
            EvtPublisherMetadataTaskValue,
            EvtPublisherMetadataTaskMessageID,
            EvtPublisherMetadataOpcodes,
            EvtPublisherMetadataOpcodeName,
            EvtPublisherMetadataOpcodeValue,
            EvtPublisherMetadataOpcodeMessageID,
            EvtPublisherMetadataKeywords,
            EvtPublisherMetadataKeywordName,
            EvtPublisherMetadataKeywordValue,
            EvtPublisherMetadataKeywordMessageID
        }

        [Flags]
        internal enum EvtQueryFlags
        {
            EvtQueryChannelPath = 1,
            EvtQueryFilePath = 2,
            EvtQueryForwardDirection = 0x100,
            EvtQueryReverseDirection = 0x200,
            EvtQueryTolerateQueryErrors = 0x1000
        }

        internal enum EvtQueryPropertyId
        {
            EvtQueryNames,
            EvtQueryStatuses
        }

        internal enum EvtRenderContextFlags
        {
            EvtRenderContextValues,
            EvtRenderContextSystem,
            EvtRenderContextUser
        }

        internal enum EvtRenderFlags
        {
            EvtRenderEventValues,
            EvtRenderEventXml,
            EvtRenderBookmark
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal struct EvtRpcLogin
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Server;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string User;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Domain;
            public CoTaskMemUnicodeSafeHandle Password;
            public int Flags;
        }

        [Flags]
        internal enum EvtSeekFlags
        {
            EvtSeekOriginMask = 7,
            EvtSeekRelativeToBookmark = 4,
            EvtSeekRelativeToCurrent = 3,
            EvtSeekRelativeToFirst = 1,
            EvtSeekRelativeToLast = 2,
            EvtSeekStrict = 0x10000
        }

        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Auto)]
        internal struct EvtStringVariant
        {
            [FieldOffset(8)]
            public uint Count;
            [MarshalAs(UnmanagedType.LPWStr), FieldOffset(0)]
            public string StringVal;
            [FieldOffset(12)]
            public uint Type;
        }

        [Flags]
        internal enum EvtSubscribeFlags
        {
            EvtSubscribeStartAfterBookmark = 3,
            EvtSubscribeStartAtOldestRecord = 2,
            EvtSubscribeStrict = 0x10000,
            EvtSubscribeToFutureEvents = 1,
            EvtSubscribeTolerateQueryErrors = 0x1000
        }

        internal enum EvtSystemPropertyId
        {
            EvtSystemProviderName,
            EvtSystemProviderGuid,
            EvtSystemEventID,
            EvtSystemQualifiers,
            EvtSystemLevel,
            EvtSystemTask,
            EvtSystemOpcode,
            EvtSystemKeywords,
            EvtSystemTimeCreated,
            EvtSystemEventRecordId,
            EvtSystemActivityID,
            EvtSystemRelatedActivityID,
            EvtSystemProcessID,
            EvtSystemThreadID,
            EvtSystemChannel,
            EvtSystemComputer,
            EvtSystemUserID,
            EvtSystemVersion,
            EvtSystemPropertyIdEND
        }

        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Auto)]
        internal struct EvtVariant
        {
            [FieldOffset(0)]
            public IntPtr AnsiString;
            [FieldOffset(0)]
            public IntPtr Binary;
            [FieldOffset(0)]
            public uint Bool;
            [FieldOffset(0)]
            public byte ByteVal;
            [FieldOffset(8)]
            public uint Count;
            [FieldOffset(0)]
            public double Double;
            [FieldOffset(0)]
            public ulong FileTime;
            [FieldOffset(0)]
            public IntPtr GuidReference;
            [FieldOffset(0)]
            public IntPtr Handle;
            [FieldOffset(0)]
            public int Integer;
            [FieldOffset(0)]
            public long Long;
            [FieldOffset(0)]
            public IntPtr Reference;
            [FieldOffset(0)]
            public byte SByte;
            [FieldOffset(0)]
            public short Short;
            [FieldOffset(0)]
            public IntPtr SidVal;
            [FieldOffset(0)]
            public IntPtr StringVal;
            [FieldOffset(0)]
            public IntPtr SystemTime;
            [FieldOffset(12)]
            public uint Type;
            [FieldOffset(0)]
            public byte UInt8;
            [FieldOffset(0)]
            public uint UInteger;
            [FieldOffset(0)]
            public ulong ULong;
            [FieldOffset(0)]
            public ushort UShort;
        }

        internal enum EvtVariantType
        {
            EvtVarTypeAnsiString = 2,
            EvtVarTypeBinary = 14,
            EvtVarTypeBoolean = 13,
            EvtVarTypeByte = 4,
            EvtVarTypeDouble = 12,
            EvtVarTypeEvtHandle = 0x20,
            EvtVarTypeEvtXml = 0x23,
            EvtVarTypeFileTime = 0x11,
            EvtVarTypeGuid = 15,
            EvtVarTypeHexInt32 = 20,
            EvtVarTypeHexInt64 = 0x15,
            EvtVarTypeInt16 = 5,
            EvtVarTypeInt32 = 7,
            EvtVarTypeInt64 = 9,
            EvtVarTypeNull = 0,
            EvtVarTypeSByte = 3,
            EvtVarTypeSid = 0x13,
            EvtVarTypeSingle = 11,
            EvtVarTypeSizeT = 0x10,
            EvtVarTypeString = 1,
            EvtVarTypeStringArray = 0x81,
            EvtVarTypeSysTime = 0x12,
            EvtVarTypeUInt16 = 6,
            EvtVarTypeUInt32 = 8,
            EvtVarTypeUInt32Array = 0x88,
            EvtVarTypeUInt64 = 10
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

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class MEMORYSTATUSEX
        {
            internal uint dwLength = ((uint) Marshal.SizeOf(typeof(Microsoft.Win32.UnsafeNativeMethods.MEMORYSTATUSEX)));
            internal uint dwMemoryLoad;
            internal ulong ullTotalPhys;
            internal ulong ullAvailPhys;
            internal ulong ullTotalPageFile;
            internal ulong ullAvailPageFile;
            internal ulong ullTotalVirtual;
            internal ulong ullAvailVirtual;
            internal ulong ullAvailExtendedVirtual;
            [SecurityCritical]
            internal MEMORYSTATUSEX()
            {
            }
        }

        internal enum NativeErrorCodes : uint
        {
            ERROR_EVT_MESSAGE_ID_NOT_FOUND = 0x3ab4,
            ERROR_EVT_MESSAGE_LOCALE_NOT_FOUND = 0x3ab9,
            ERROR_EVT_MESSAGE_NOT_FOUND = 0x3ab3,
            ERROR_EVT_UNRESOLVED_VALUE_INSERT = 0x3ab5,
            ERROR_INSUFFICIENT_BUFFER = 0x7a,
            ERROR_INVALID_PARAMETER = 0x57,
            ERROR_MUI_FILE_NOT_FOUND = 0x3afc,
            ERROR_NO_MORE_ITEMS = 0x103,
            ERROR_RESOURCE_LANG_NOT_FOUND = 0x717,
            ERROR_SUCCESS = 0
        }

        [StructLayout(LayoutKind.Explicit, Size=0x20)]
        internal struct PerfCounterInfoStruct
        {
            [FieldOffset(8)]
            internal long Attrib;
            [FieldOffset(0)]
            internal uint CounterId;
            [FieldOffset(4)]
            internal uint CounterType;
            [FieldOffset(20)]
            internal uint DetailLevel;
            [FieldOffset(0x1c)]
            internal uint Offset;
            [FieldOffset(0x18)]
            internal uint Scale;
            [FieldOffset(0x10)]
            internal uint Size;
        }

        [StructLayout(LayoutKind.Explicit, Size=40)]
        internal struct PerfCounterSetInfoStruct
        {
            [FieldOffset(0)]
            internal Guid CounterSetGuid;
            [FieldOffset(0x24)]
            internal uint InstanceType;
            [FieldOffset(0x20)]
            internal uint NumCounters;
            [FieldOffset(0x10)]
            internal Guid ProviderGuid;
        }

        [StructLayout(LayoutKind.Explicit, Size=0x20)]
        internal struct PerfCounterSetInstanceStruct
        {
            [FieldOffset(0)]
            internal Guid CounterSetGuid;
            [FieldOffset(0x10)]
            internal uint dwSize;
            [FieldOffset(20)]
            internal uint InstanceId;
            [FieldOffset(0x18)]
            internal uint InstanceNameOffset;
            [FieldOffset(0x1c)]
            internal uint InstanceNameSize;
        }

        internal unsafe delegate uint PERFLIBREQUEST([In] uint RequestCode, [In] void* Buffer, [In] uint BufferSize);

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength;
            internal unsafe byte* pSecurityDescriptor;
            internal int bInheritHandle;
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
    }
}

