namespace Microsoft.Build.Shared
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal static class NativeMethodsShared
    {
        internal const uint E_ABORT = 0x80004004;
        internal const uint ERROR_FILE_NOT_FOUND = 0x80070002;
        internal const uint ERROR_INSUFFICIENT_BUFFER = 0x8007007a;
        internal const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        internal const int FILE_ATTRIBUTE_READONLY = 1;
        internal const int FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
        internal const uint FILE_TYPE_CHAR = 2;
        internal const uint FUSION_E_PRIVATE_ASM_DISALLOWED = 0x80131044;
        internal const uint INFINITE = uint.MaxValue;
        private const string kernel32Dll = "kernel32.dll";
        internal static int MAX_PATH = 260;
        private const string mscoreeDLL = "mscoree.dll";
        internal static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        internal static IntPtr NullIntPtr = new IntPtr(0);
        internal const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        internal const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        internal const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
        internal const uint RPC_S_CALLPENDING = 0x80010115;
        internal const uint RUNTIME_INFO_DONT_SHOW_ERROR_DIALOG = 0x40;
        internal const uint S_FALSE = 1;
        internal const uint S_OK = 0;
        internal const uint STARTUP_LOADER_SAFEMODE = 0x10;
        internal const int STD_OUTPUT_HANDLE = -11;
        internal const uint WAIT_ABANDONED_0 = 0x80;
        internal const uint WAIT_OBJECT_0 = 0;
        internal const uint WAIT_TIMEOUT = 0x102;

        [DllImport("ole32.dll")]
        public static extern int CoWaitForMultipleHandles(COWAIT_FLAGS dwFlags, int dwTimeout, int cHandles, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] pHandles, out int pdwIndex);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, SecurityAttributes lpPipeAttributes, int nSize);
        internal static string FindOnPath(string filename)
        {
            StringBuilder buffer = new StringBuilder(MAX_PATH + 1);
            string str = null;
            for (int i = 0; i < 2; i++)
            {
                uint num2 = SearchPath(null, filename, null, buffer.Capacity, buffer, null);
                if (num2 > buffer.Capacity)
                {
                    ErrorUtilities.VerifyThrow(i == 0, "We should not have to resize the buffer twice.");
                    buffer.Capacity = (int) num2;
                }
                else
                {
                    if (num2 > 0)
                    {
                        str = buffer.ToString();
                    }
                    return str;
                }
            }
            return str;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool FreeLibrary([In] IntPtr module);
        internal static List<Tuple<int, SafeProcessHandle>> GetChildProcessIds(int parentProcessId, DateTime parentStartTime)
        {
            List<Tuple<int, SafeProcessHandle>> list = new List<Tuple<int, SafeProcessHandle>>();
            foreach (Process process in Process.GetProcesses())
            {
                using (process)
                {
                    SafeProcessHandle handle = OpenProcess(eDesiredAccess.PROCESS_QUERY_INFORMATION, false, process.Id);
                    if (!handle.IsInvalid)
                    {
                        bool flag = false;
                        try
                        {
                            if (process.StartTime > parentStartTime)
                            {
                                int num = GetParentProcessId(process.Id);
                                if ((num != 0) && (parentProcessId == num))
                                {
                                    list.Add(new Tuple<int, SafeProcessHandle>(process.Id, handle));
                                    flag = true;
                                }
                            }
                        }
                        finally
                        {
                            if (!flag)
                            {
                                handle.Dispose();
                            }
                        }
                    }
                }
            }
            return list;
        }

        internal static string GetCurrentDirectory()
        {
            StringBuilder lpBuffer = new StringBuilder(MAX_PATH);
            if (GetCurrentDirectory(MAX_PATH, lpBuffer) > 0)
            {
                return lpBuffer.ToString();
            }
            return null;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int GetCurrentDirectory(int nBufferLength, StringBuilder lpBuffer);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);
        [DllImport("kernel32.dll")]
        internal static extern uint GetFileType(IntPtr hFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int GetFullPathName(string target, int bufferLength, StringBuilder buffer, IntPtr mustBeZero);
        internal static DateTime GetLastWriteFileUtcTime(string fullPath)
        {
            DateTime minValue = DateTime.MinValue;
            WIN32_FILE_ATTRIBUTE_DATA lpFileInformation = new WIN32_FILE_ATTRIBUTE_DATA();
            if (GetFileAttributesEx(fullPath, 0, ref lpFileInformation))
            {
                long fileTime = (lpFileInformation.ftLastWriteTimeHigh << 0x20) | lpFileInformation.ftLastWriteTimeLow;
                minValue = DateTime.FromFileTimeUtc(fileTime);
            }
            return minValue;
        }

        internal static string GetLongFilePath(string path)
        {
            if (path != null)
            {
                int capacity = GetLongPathName(path, null, 0);
                if (capacity > 0)
                {
                    StringBuilder fullpath = new StringBuilder(capacity);
                    GetLongPathName(path, fullpath, capacity);
                    path = fullpath.ToString();
                }
            }
            return path;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetLongPathName([In] string path, [In, Out] StringBuilder fullpath, [In] int length);
        internal static MemoryStatus GetMemoryStatus()
        {
            MemoryStatus lpBuffer = new MemoryStatus();
            if (!GlobalMemoryStatusEx(lpBuffer))
            {
                return null;
            }
            return lpBuffer;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);
        [DllImport("kernel32.dll")]
        internal static extern int GetOEMCP();
        internal static int GetParentProcessId(int processId)
        {
            int inheritedFromUniqueProcessId = 0;
            SafeProcessHandle hProcess = OpenProcess(eDesiredAccess.PROCESS_QUERY_INFORMATION, false, processId);
            if (!hProcess.IsInvalid)
            {
                try
                {
                    PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                    int pSize = 0;
                    if (-1 != NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, pbi.Size, ref pSize))
                    {
                        inheritedFromUniqueProcessId = pbi.InheritedFromUniqueProcessId;
                    }
                }
                finally
                {
                    hProcess.Dispose();
                }
            }
            return inheritedFromUniqueProcessId;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        internal static extern IntPtr GetProcAddress(IntPtr module, string procName);
        [DllImport("mscoree.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern uint GetRequestedRuntimeInfo(string pExe, string pwszVersion, string pConfigurationFile, uint startupFlags, uint runtimeInfoFlags, StringBuilder pDirectory, int dwDirectory, out uint dwDirectoryLength, StringBuilder pVersion, int cchBuffer, out uint dwlength);
        internal static string GetShortFilePath(string path)
        {
            if (path != null)
            {
                int capacity = GetShortPathName(path, null, 0);
                if (capacity > 0)
                {
                    StringBuilder fullpath = new StringBuilder(capacity);
                    GetShortPathName(path, fullpath, capacity);
                    path = fullpath.ToString();
                }
            }
            return path;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetShortPathName(string path, [In, Out] StringBuilder fullpath, [In] int length);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatus lpBuffer);
        public static bool HResultFailed(int hr)
        {
            return (hr < 0);
        }

        public static bool HResultSucceeded(int hr)
        {
            return (hr >= 0);
        }

        private static bool IsExpectedKillException(Exception e)
        {
            return (((e is Win32Exception) || (e is NotSupportedException)) || (e is InvalidOperationException));
        }

        internal static void KillTree(int processIdTokill)
        {
            using (Process process = Process.GetProcessById(processIdTokill))
            {
                DateTime startTime = process.StartTime;
                SafeProcessHandle handle = OpenProcess(eDesiredAccess.PROCESS_QUERY_INFORMATION, false, processIdTokill);
                if (!handle.IsInvalid)
                {
                    try
                    {
                        process.Kill();
                        List<Tuple<int, SafeProcessHandle>> childProcessIds = GetChildProcessIds(processIdTokill, startTime);
                        try
                        {
                            foreach (Tuple<int, SafeProcessHandle> tuple in childProcessIds)
                            {
                                KillTree(tuple.Item1);
                            }
                        }
                        finally
                        {
                            foreach (Tuple<int, SafeProcessHandle> tuple2 in childProcessIds)
                            {
                                tuple2.Item2.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        handle.Dispose();
                    }
                }
            }
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr LoadLibrary(string fileName);
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static bool MsgWaitOne(this WaitHandle handle)
        {
            return handle.MsgWaitOne(-1);
        }

        internal static bool MsgWaitOne(this WaitHandle handle, int timeout)
        {
            int num;
            IntPtr[] pHandles = new IntPtr[] { handle.SafeWaitHandle.DangerousGetHandle() };
            int num2 = CoWaitForMultipleHandles(COWAIT_FLAGS.COWAIT_NONE, timeout, 1, pHandles, out num);
            ErrorUtilities.VerifyThrow((num2 == 0) || ((num2 == -2147417835) && (timeout != -1)), "Received {0} from CoWaitForMultipleHandles, but expected 0 (S_OK)", num2);
            return (num2 == 0);
        }

        internal static bool MsgWaitOne(this WaitHandle handle, TimeSpan timeout)
        {
            return handle.MsgWaitOne(((int) timeout.TotalMilliseconds));
        }

        [DllImport("NTDLL.DLL")]
        private static extern int NtQueryInformationProcess(SafeProcessHandle hProcess, PROCESSINFOCLASS pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, ref int pSize);
        [DllImport("kernel32.dll")]
        private static extern SafeProcessHandle OpenProcess(eDesiredAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern uint SearchPath(string path, string fileName, string extension, int numBufferChars, StringBuilder buffer, int[] filePart);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool SetCurrentDirectory(string path);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        public static extern int WaitForMultipleObjects(uint handle, IntPtr[] handles, bool waitAll, uint milliseconds);

        [Flags]
        public enum COWAIT_FLAGS
        {
            COWAIT_NONE,
            COWAIT_WAITALL,
            COWAIT_ALERTABLE
        }

        private enum eDesiredAccess
        {
            DELETE = 0x10000,
            PROCESS_ALL_ACCESS = 0x100fff,
            PROCESS_CREATE_PROCESS = 0x80,
            PROCESS_CREATE_THREAD = 2,
            PROCESS_DUP_HANDLE = 0x40,
            PROCESS_QUERY_INFORMATION = 0x400,
            PROCESS_SET_INFORMATION = 0x200,
            PROCESS_SET_QUOTA = 0x100,
            PROCESS_SET_SESSIONID = 4,
            PROCESS_TERMINATE = 1,
            PROCESS_VM_OPERATION = 8,
            PROCESS_VM_READ = 0x10,
            PROCESS_VM_WRITE = 0x20,
            READ_CONTROL = 0x20000,
            STANDARD_RIGHTS_ALL = 0x1f0000,
            SYNCHRONIZE = 0x100000,
            WRITE_DAC = 0x40000,
            WRITE_OWNER = 0x80000
        }

        internal static class InprocTracking
        {
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void EndTrackingContext();
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void ResumeTracking();
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void SetThreadCount(int threadCount);
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void StartTrackingContext([In, MarshalAs(UnmanagedType.LPWStr)] string intermediateDirectory, [In, MarshalAs(UnmanagedType.LPWStr)] string taskName);
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void StartTrackingContextWithRoot([In, MarshalAs(UnmanagedType.LPWStr)] string intermediateDirectory, [In, MarshalAs(UnmanagedType.LPWStr)] string taskName, [In, MarshalAs(UnmanagedType.LPWStr)] string rootMarker);
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void StopTrackingAndCleanup();
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void SuspendTracking();
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void WriteAllTLogs([In, MarshalAs(UnmanagedType.LPWStr)] string intermediateDirectory, [In, MarshalAs(UnmanagedType.LPWStr)] string tlogRootName);
            [DllImport("FileTracker.dll", PreserveSig=false)]
            internal static extern void WriteContextTLogs([In, MarshalAs(UnmanagedType.LPWStr)] string intermediateDirectory, [In, MarshalAs(UnmanagedType.LPWStr)] string tlogRootName);
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class MemoryStatus
        {
            private uint Length = ((uint) Marshal.SizeOf(typeof(NativeMethodsShared.MemoryStatus)));
            public uint MemoryLoad;
            public ulong TotalPhysical;
            public ulong AvailablePhysical;
            public ulong TotalPageFile;
            public ulong AvailablePageFile;
            public ulong TotalVirtual;
            public ulong AvailableVirtual;
            public ulong AvailableExtendedVirtual;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public int ExitStatus;
            public int PebBaseAddress;
            public int AffinityMask;
            public int BasePriority;
            public int UniqueProcessId;
            public int InheritedFromUniqueProcessId;
            public int Size
            {
                get
                {
                    return 0x18;
                }
            }
        }

        private enum PROCESSINFOCLASS
        {
            ProcessBasicInformation,
            ProcessQuotaLimits,
            ProcessIoCounters,
            ProcessVmCounters,
            ProcessTimes,
            ProcessBasePriority,
            ProcessRaisePriority,
            ProcessDebugPort,
            ProcessExceptionPort,
            ProcessAccessToken,
            ProcessLdtInformation,
            ProcessLdtSize,
            ProcessDefaultHardErrorMode,
            ProcessIoPortHandlers,
            ProcessPooledUsageAndLimits,
            ProcessWorkingSetWatch,
            ProcessUserModeIOPL,
            ProcessEnableAlignmentFaultFixup,
            ProcessPriorityClass,
            ProcessWx86Information,
            ProcessHandleCount,
            ProcessAffinityMask,
            ProcessPriorityBoost,
            MaxProcessInfoClass
        }

        internal class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeProcessHandle() : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            private static extern bool CloseHandle(IntPtr hObject);
            protected override bool ReleaseHandle()
            {
                return CloseHandle(base.handle);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SecurityAttributes
        {
            private uint nLength = ((uint) Marshal.SizeOf(typeof(NativeMethodsShared.SecurityAttributes)));
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            internal ushort wProcessorArchitecture;
            internal ushort wReserved;
            internal uint dwPageSize;
            internal IntPtr lpMinimumApplicationAddress;
            internal IntPtr lpMaximumApplicationAddress;
            internal IntPtr dwActiveProcessorMask;
            internal uint dwNumberOfProcessors;
            internal uint dwProcessorType;
            internal uint dwAllocationGranularity;
            internal ushort wProcessorLevel;
            internal ushort wProcessorRevision;
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

