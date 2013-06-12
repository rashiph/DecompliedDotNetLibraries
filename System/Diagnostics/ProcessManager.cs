namespace System.Diagnostics
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class ProcessManager
    {
        static ProcessManager()
        {
            Microsoft.Win32.NativeMethods.LUID lpLuid = new Microsoft.Win32.NativeMethods.LUID();
            if (Microsoft.Win32.NativeMethods.LookupPrivilegeValue(null, "SeDebugPrivilege", out lpLuid))
            {
                IntPtr zero = IntPtr.Zero;
                try
                {
                    if (Microsoft.Win32.NativeMethods.OpenProcessToken(new HandleRef(null, Microsoft.Win32.NativeMethods.GetCurrentProcess()), 0x20, out zero))
                    {
                        Microsoft.Win32.NativeMethods.TokenPrivileges newState = new Microsoft.Win32.NativeMethods.TokenPrivileges {
                            PrivilegeCount = 1,
                            Luid = lpLuid,
                            Attributes = 2
                        };
                        Microsoft.Win32.NativeMethods.AdjustTokenPrivileges(new HandleRef(null, zero), false, newState, 0, IntPtr.Zero, IntPtr.Zero);
                    }
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Microsoft.Win32.SafeNativeMethods.CloseHandle(new HandleRef(null, zero));
                    }
                }
            }
        }

        public static IntPtr GetMainWindowHandle(int processId)
        {
            MainWindowFinder finder = new MainWindowFinder();
            return finder.FindMainWindow(processId);
        }

        public static ModuleInfo[] GetModuleInfos(int processId)
        {
            if (IsNt)
            {
                return NtProcessManager.GetModuleInfos(processId);
            }
            return WinProcessManager.GetModuleInfos(processId);
        }

        public static int GetProcessIdFromHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle)
        {
            if (!IsNt)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
            return NtProcessManager.GetProcessIdFromHandle(processHandle);
        }

        public static int[] GetProcessIds()
        {
            if (IsNt)
            {
                return NtProcessManager.GetProcessIds();
            }
            return WinProcessManager.GetProcessIds();
        }

        public static int[] GetProcessIds(string machineName)
        {
            if (!IsRemoteMachine(machineName))
            {
                return GetProcessIds();
            }
            if (!IsNt)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequiredForRemote"));
            }
            return NtProcessManager.GetProcessIds(machineName, true);
        }

        public static ProcessInfo[] GetProcessInfos(string machineName)
        {
            bool isRemoteMachine = IsRemoteMachine(machineName);
            if (IsNt)
            {
                if (!isRemoteMachine && (Environment.OSVersion.Version.Major >= 5))
                {
                    return NtProcessInfoHelper.GetProcessInfos();
                }
                return NtProcessManager.GetProcessInfos(machineName, isRemoteMachine);
            }
            if (isRemoteMachine)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequiredForRemote"));
            }
            return WinProcessManager.GetProcessInfos();
        }

        public static bool IsProcessRunning(int processId)
        {
            return IsProcessRunning(processId, GetProcessIds());
        }

        public static bool IsProcessRunning(int processId, string machineName)
        {
            return IsProcessRunning(processId, GetProcessIds(machineName));
        }

        private static bool IsProcessRunning(int processId, int[] processIds)
        {
            for (int i = 0; i < processIds.Length; i++)
            {
                if (processIds[i] == processId)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsRemoteMachine(string machineName)
        {
            string str;
            if (machineName == null)
            {
                throw new ArgumentNullException("machineName");
            }
            if (machineName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            if (machineName.StartsWith(@"\", StringComparison.Ordinal))
            {
                str = machineName.Substring(2);
            }
            else
            {
                str = machineName;
            }
            if (str.Equals("."))
            {
                return false;
            }
            StringBuilder lpBuffer = new StringBuilder(0x100);
            Microsoft.Win32.SafeNativeMethods.GetComputerName(lpBuffer, new int[] { lpBuffer.Capacity });
            if (string.Compare(lpBuffer.ToString(), str, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            return true;
        }

        public static Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcess(int processId, int access, bool throwIfExited)
        {
            Microsoft.Win32.SafeHandles.SafeProcessHandle handle = Microsoft.Win32.NativeMethods.OpenProcess(access, false, processId);
            int error = Marshal.GetLastWin32Error();
            if (!handle.IsInvalid)
            {
                return handle;
            }
            if (processId == 0)
            {
                throw new Win32Exception(5);
            }
            if (IsProcessRunning(processId))
            {
                throw new Win32Exception(error);
            }
            if (throwIfExited)
            {
                throw new InvalidOperationException(SR.GetString("ProcessHasExited", new object[] { processId.ToString(CultureInfo.CurrentCulture) }));
            }
            return Microsoft.Win32.SafeHandles.SafeProcessHandle.InvalidHandle;
        }

        public static Microsoft.Win32.SafeHandles.SafeThreadHandle OpenThread(int threadId, int access)
        {
            Microsoft.Win32.SafeHandles.SafeThreadHandle handle2;
            try
            {
                Microsoft.Win32.SafeHandles.SafeThreadHandle handle = Microsoft.Win32.NativeMethods.OpenThread(access, false, threadId);
                int error = Marshal.GetLastWin32Error();
                if (handle.IsInvalid)
                {
                    if (error == 0x57)
                    {
                        throw new InvalidOperationException(SR.GetString("ThreadExited", new object[] { threadId.ToString(CultureInfo.CurrentCulture) }));
                    }
                    throw new Win32Exception(error);
                }
                handle2 = handle;
            }
            catch (EntryPointNotFoundException exception)
            {
                throw new PlatformNotSupportedException(SR.GetString("Win2000Required"), exception);
            }
            return handle2;
        }

        public static bool IsNt
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32NT);
            }
        }

        public static bool IsOSOlderThanXP
        {
            get
            {
                return ((Environment.OSVersion.Version.Major < 5) || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 0)));
            }
        }
    }
}

