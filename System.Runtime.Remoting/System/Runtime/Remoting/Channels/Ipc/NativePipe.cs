namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal static class NativePipe
    {
        private const string AdvApi32 = "advapi32.dll";
        public const uint CREATE_ALWAYS = 2;
        public const uint CREATE_NEW = 1;
        public const long ERROR_BROKEN_PIPE = 0x6dL;
        public const long ERROR_IO_PENDING = 0x3e5L;
        public const long ERROR_NO_DATA = 0xe8L;
        public const long ERROR_PIPE_BUSY = 0xe7L;
        public const long ERROR_PIPE_CONNECTED = 0x217L;
        public const long ERROR_PIPE_LISTENING = 0x218L;
        public const long ERROR_PIPE_NOT_CONNECTED = 0xe9L;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        public const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 0x80000;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        public const uint FILE_SHARE_READ = 1;
        public const uint FILE_SHARE_WRITE = 2;
        internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        internal const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const uint GENERIC_ALL = 0x10000000;
        public const uint GENERIC_EXECUTE = 0x20000000;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const int INVALID_HANDLE_VALUE = -1;
        private const string Kernel32 = "kernel32.dll";
        public const uint NMPWAIT_NOWAIT = 1;
        public const uint NMPWAIT_USE_DEFAULT_WAIT = 0;
        public const uint NMPWAIT_WAIT_FOREVER = uint.MaxValue;
        internal static readonly IntPtr NULL = IntPtr.Zero;
        public const uint OPEN_ALWAYS = 4;
        public const uint OPEN_EXISTING = 3;
        public const uint PIPE_ACCESS_DUPLEX = 3;
        public const uint PIPE_ACCESS_INBOUND = 1;
        public const uint PIPE_ACCESS_OUTBOUND = 2;
        public const uint PIPE_CLIENT_END = 0;
        public const uint PIPE_NOWAIT = 1;
        public const uint PIPE_READMODE_BYTE = 0;
        public const uint PIPE_READMODE_MESSAGE = 2;
        public const uint PIPE_SERVER_END = 1;
        public const uint PIPE_TYPE_BYTE = 0;
        public const uint PIPE_TYPE_MESSAGE = 4;
        public const uint PIPE_UNLIMITED_INSTANCES = 0xff;
        public const uint PIPE_WAIT = 0;
        public const uint SECURITY_ANONYMOUS = 0;
        public const uint SECURITY_DELEGATION = 0x30000;
        public const uint SECURITY_IDENTIFICATION = 0x10000;
        public const uint SECURITY_IMPERSONATION = 0x20000;
        public const uint SECURITY_SQOS_PRESENT = 0x100000;
        public const uint TRUNCATE_EXISTING = 5;

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool ConnectNamedPipe(PipeHandle hNamedPipe, Overlapped lpOverlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern PipeHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr attr, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern PipeHandle CreateNamedPipe(string lpName, uint dwOpenMode, uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, SECURITY_ATTRIBUTES pipeSecurityDescriptor);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);
        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool ImpersonateNamedPipeClient(PipeHandle hNamedPipe);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern unsafe bool ReadFile(PipeHandle hFile, byte* lpBuffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, IntPtr mustBeZero);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern unsafe bool ReadFile(PipeHandle hFile, byte* lpBuffer, int nNumberOfBytesToRead, IntPtr numBytesRead_mustBeZero, NativeOverlapped* lpOverlapped);
        [DllImport("advapi32.dll")]
        public static extern bool RevertToSelf();
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool WaitNamedPipe(string name, int timeout);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern unsafe bool WriteFile(PipeHandle hFile, byte* lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, IntPtr lpOverlapped);
    }
}

