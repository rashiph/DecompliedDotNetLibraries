namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [ComVisible(false)]
    internal sealed class NativeMethods
    {
        private NativeMethods()
        {
        }

        [SecurityCritical, DllImport("user32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern int AttachThreadInput(int idAttach, int idAttachTo, int fAttach);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical, DllImport("kernel32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern int CloseHandle(IntPtr hObject);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string StringSecurityDescriptor, uint StringSDRevision, ref IntPtr SecurityDescriptor, IntPtr SecurityDescriptorSize);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto)]
        internal static extern int CreateProcess(string lpApplicationName, string lpCommandLine, NativeTypes.SECURITY_ATTRIBUTES lpProcessAttributes, NativeTypes.SECURITY_ATTRIBUTES lpThreadAttributes, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, NativeTypes.STARTUPINFO lpStartupInfo, NativeTypes.PROCESS_INFORMATION lpProcessInformation);
        [SecurityCritical, DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr FindWindow([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpClassName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpWindowName);
        [SecurityCritical, DllImport("user32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern IntPtr GetDesktopWindow();
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto)]
        internal static extern void GetStartupInfo([In, Out] NativeTypes.STARTUPINFO lpStartupInfo);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto)]
        internal static extern int GetVolumeInformation([MarshalAs(UnmanagedType.LPTStr)] string lpRootPathName, StringBuilder lpVolumeNameBuffer, int nVolumeNameSize, ref int lpVolumeSerialNumber, ref int lpMaximumComponentLength, ref int lpFileSystemFlags, IntPtr lpFileSystemNameBuffer, int nFileSystemNameSize);
        [SecurityCritical, DllImport("user32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern IntPtr GetWindow(IntPtr hwnd, int wFlag);
        [SecurityCritical, DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);
        [SecurityCritical, DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern void GlobalMemoryStatus(ref MEMORYSTATUS lpBuffer);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);
        [SecurityCritical, DllImport("user32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern IntPtr SetFocus(IntPtr hwnd);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, DllImport("user32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern bool SetForegroundWindow(IntPtr hwnd);
        [SecurityCritical, DllImport("shell32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        [SecurityCritical]
        internal static int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp)
        {
            if (IntPtr.Size == 4)
            {
                return SHFileOperation32(ref lpFileOp);
            }
            SHFILEOPSTRUCT64 shfileopstruct = new SHFILEOPSTRUCT64 {
                hwnd = lpFileOp.hwnd,
                wFunc = lpFileOp.wFunc,
                pFrom = lpFileOp.pFrom,
                pTo = lpFileOp.pTo,
                fFlags = lpFileOp.fFlags,
                fAnyOperationsAborted = lpFileOp.fAnyOperationsAborted,
                hNameMappings = lpFileOp.hNameMappings,
                lpszProgressTitle = lpFileOp.lpszProgressTitle
            };
            int num2 = SHFileOperation64(ref shfileopstruct);
            lpFileOp.fAnyOperationsAborted = shfileopstruct.fAnyOperationsAborted;
            return num2;
        }

        [SecurityCritical, DllImport("shell32.dll", EntryPoint="SHFileOperation", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern int SHFileOperation32(ref SHFILEOPSTRUCT lpFileOp);
        [SecurityCritical, DllImport("shell32.dll", EntryPoint="SHFileOperation", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern int SHFileOperation64(ref SHFILEOPSTRUCT64 lpFileOp);
        [SecurityCritical, DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int WaitForInputIdle(NativeTypes.LateInitSafeHandleZeroOrMinusOneIsInvalid Process, int Milliseconds);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern int WaitForSingleObject(NativeTypes.LateInitSafeHandleZeroOrMinusOneIsInvalid hHandle, int dwMilliseconds);

        [StructLayout(LayoutKind.Sequential)]
        internal struct MEMORYSTATUS
        {
            internal uint dwLength;
            internal uint dwMemoryLoad;
            internal uint dwTotalPhys;
            internal uint dwAvailPhys;
            internal uint dwTotalPageFile;
            internal uint dwAvailPageFile;
            internal uint dwTotalVirtual;
            internal uint dwAvailVirtual;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MEMORYSTATUSEX
        {
            internal uint dwLength;
            internal uint dwMemoryLoad;
            internal ulong ullTotalPhys;
            internal ulong ullAvailPhys;
            internal ulong ullTotalPageFile;
            internal ulong ullAvailPageFile;
            internal ulong ullTotalVirtual;
            internal ulong ullAvailVirtual;
            internal ulong ullAvailExtendedVirtual;
            internal void Init()
            {
                this.dwLength = (uint) Marshal.SizeOf(typeof(Microsoft.VisualBasic.CompilerServices.NativeMethods.MEMORYSTATUSEX));
            }
        }

        internal enum SHChangeEventParameterFlags : uint
        {
            SHCNF_DWORD = 3
        }

        internal enum SHChangeEventTypes : uint
        {
            SHCNE_ALLEVENTS = 0x7fffffff,
            SHCNE_DISKEVENTS = 0x2381f
        }

        [Flags]
        internal enum ShFileOperationFlags : ushort
        {
            FOF_ALLOWUNDO = 0x40,
            FOF_CONFIRMMOUSE = 2,
            FOF_FILESONLY = 0x80,
            FOF_MULTIDESTFILES = 1,
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,
            FOF_NOCONFIRMATION = 0x10,
            FOF_NOCONFIRMMKDIR = 0x200,
            FOF_NOCOPYSECURITYATTRIBS = 0x800,
            FOF_NOERRORUI = 0x400,
            FOF_NORECURSEREPARSE = 0x8000,
            FOF_NORECURSION = 0x1000,
            FOF_RENAMEONCOLLISION = 8,
            FOF_SILENT = 4,
            FOF_SIMPLEPROGRESS = 0x100,
            FOF_WANTMAPPINGHANDLE = 0x20,
            FOF_WANTNUKEWARNING = 0x4000
        }

        internal enum SHFileOperationType : uint
        {
            FO_COPY = 2,
            FO_DELETE = 3,
            FO_MOVE = 1,
            FO_RENAME = 4
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
        internal struct SHFILEOPSTRUCT
        {
            internal IntPtr hwnd;
            internal uint wFunc;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string pFrom;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string pTo;
            internal ushort fFlags;
            internal bool fAnyOperationsAborted;
            internal IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string lpszProgressTitle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        private struct SHFILEOPSTRUCT64
        {
            internal IntPtr hwnd;
            internal uint wFunc;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string pFrom;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string pTo;
            internal ushort fFlags;
            internal bool fAnyOperationsAborted;
            internal IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string lpszProgressTitle;
        }
    }
}

