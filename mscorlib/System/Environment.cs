namespace System
{
    using Microsoft.Win32;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [ComVisible(true)]
    public static class Environment
    {
        private static bool isUserNonInteractive;
        private static OperatingSystem m_os;
        private static OSName m_osname;
        private static ResourceHelper m_resHelper;
        private const int MaxEnvVariableValueLength = 0x7fff;
        private const int MaxMachineNameLength = 0x100;
        private const int MaxSystemEnvVariableLength = 0x400;
        private const int MaxUserEnvVariableLength = 0xff;
        private static IntPtr processWinStation;
        private static bool s_CheckedOSType;
        private static volatile bool s_CheckedOSW2k3;
        private static object s_InternalSyncObject;
        private static bool s_IsW2k3;
        private static bool s_IsWindowsVista;

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void _Exit(int exitCode);
        private static void CheckEnvironmentVariableName(string variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("variable");
            }
            if (variable.Length == 0)
            {
                throw new ArgumentException(GetResourceString("Argument_StringZeroLength"), "variable");
            }
            if (variable[0] == '\0')
            {
                throw new ArgumentException(GetResourceString("Argument_StringFirstCharIsZero"), "variable");
            }
            if (variable.Length >= 0x7fff)
            {
                throw new ArgumentException(GetResourceString("Argument_LongEnvVarValue"));
            }
            if (variable.IndexOf('=') != -1)
            {
                throw new ArgumentException(GetResourceString("Argument_IllegalEnvVarName"));
            }
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void Exit(int exitCode)
        {
            _Exit(exitCode);
        }

        [SecuritySafeCritical]
        public static string ExpandEnvironmentVariables(string name)
        {
            int num2;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                return name;
            }
            bool flag = CodeAccessSecurityEngine.QuickCheckForAllDemands();
            string[] strArray = name.Split(new char[] { '%' });
            StringBuilder builder = flag ? null : new StringBuilder();
            int capacity = 100;
            StringBuilder lpDst = new StringBuilder(capacity);
            bool flag2 = false;
            for (int i = 1; i < (strArray.Length - 1); i++)
            {
                if ((strArray[i].Length == 0) || flag2)
                {
                    flag2 = false;
                }
                else
                {
                    lpDst.Length = 0;
                    string lpSrc = "%" + strArray[i] + "%";
                    num2 = Win32Native.ExpandEnvironmentStrings(lpSrc, lpDst, capacity);
                    if (num2 == 0)
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    while (num2 > capacity)
                    {
                        capacity = num2;
                        lpDst.Capacity = capacity;
                        lpDst.Length = 0;
                        num2 = Win32Native.ExpandEnvironmentStrings(lpSrc, lpDst, capacity);
                        if (num2 == 0)
                        {
                            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                        }
                    }
                    if (!flag && (lpDst.ToString() != lpSrc))
                    {
                        builder.Append(strArray[i]);
                        builder.Append(';');
                    }
                }
            }
            if (!flag)
            {
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, builder.ToString()).Demand();
            }
            lpDst.Length = 0;
            num2 = Win32Native.ExpandEnvironmentStrings(name, lpDst, capacity);
            if (num2 == 0)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            while (num2 > capacity)
            {
                capacity = num2;
                lpDst.Capacity = capacity;
                lpDst.Length = 0;
                num2 = Win32Native.ExpandEnvironmentStrings(name, lpDst, capacity);
                if (num2 == 0)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
            }
            return lpDst.ToString();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void FailFast(string message);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void FailFast(string message, Exception exception);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetCommandLine(StringHandleOnStack retString);
        [SecuritySafeCritical]
        public static string[] GetCommandLineArgs()
        {
            new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Demand();
            return GetCommandLineArgsNative();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern string[] GetCommandLineArgsNative();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool GetCompatibilityFlag(CompatibilityFlag flag);
        [SecurityCritical]
        private static unsafe char[] GetEnvironmentCharArray()
        {
            char[] chArray = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                char* pSrc = null;
                try
                {
                    pSrc = Win32Native.GetEnvironmentStrings();
                    if (pSrc == null)
                    {
                        throw new OutOfMemoryException();
                    }
                    char* chPtr2 = pSrc;
                    while ((chPtr2[0] != '\0') || (chPtr2[1] != '\0'))
                    {
                        chPtr2++;
                    }
                    int len = (int) (((long) ((chPtr2 - pSrc) / 2)) + 1L);
                    chArray = new char[len];
                    try
                    {
                        fixed (char* chRef = chArray)
                        {
                            Buffer.memcpy(pSrc, 0, chRef, 0, len);
                        }
                    }
                    finally
                    {
                        chRef = null;
                    }
                }
                finally
                {
                    if (pSrc != null)
                    {
                        Win32Native.FreeEnvironmentStrings(pSrc);
                    }
                }
            }
            return chArray;
        }

        [SecuritySafeCritical]
        public static string GetEnvironmentVariable(string variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("variable");
            }
            new EnvironmentPermission(EnvironmentPermissionAccess.Read, variable).Demand();
            StringBuilder lpValue = new StringBuilder(0x80);
            int num = Win32Native.GetEnvironmentVariable(variable, lpValue, lpValue.Capacity);
            if ((num != 0) || (Marshal.GetLastWin32Error() != 0xcb))
            {
                while (num > lpValue.Capacity)
                {
                    lpValue.Capacity = num;
                    lpValue.Length = 0;
                    num = Win32Native.GetEnvironmentVariable(variable, lpValue, lpValue.Capacity);
                }
                return lpValue.ToString();
            }
            return null;
        }

        [SecuritySafeCritical]
        public static string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("variable");
            }
            if (target == EnvironmentVariableTarget.Process)
            {
                return GetEnvironmentVariable(variable);
            }
            new EnvironmentPermission(PermissionState.Unrestricted).Demand();
            if (target == EnvironmentVariableTarget.Machine)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Session Manager\Environment", false))
                {
                    if (key == null)
                    {
                        return null;
                    }
                    return (key.GetValue(variable) as string);
                }
            }
            if (target == EnvironmentVariableTarget.User)
            {
                using (RegistryKey key2 = Registry.CurrentUser.OpenSubKey("Environment", false))
                {
                    if (key2 == null)
                    {
                        return null;
                    }
                    return (key2.GetValue(variable) as string);
                }
            }
            throw new ArgumentException(GetResourceString("Arg_EnumIllegalVal", new object[] { (int) target }));
        }

        [SecuritySafeCritical]
        public static IDictionary GetEnvironmentVariables()
        {
            bool flag = CodeAccessSecurityEngine.QuickCheckForAllDemands();
            char[] environmentCharArray = GetEnvironmentCharArray();
            Hashtable hashtable = new Hashtable(20);
            StringBuilder builder = flag ? null : new StringBuilder();
            bool flag2 = true;
            for (int i = 0; i < environmentCharArray.Length; i++)
            {
                int startIndex = i;
                while ((environmentCharArray[i] != '=') && (environmentCharArray[i] != '\0'))
                {
                    i++;
                }
                if (environmentCharArray[i] != '\0')
                {
                    if ((i - startIndex) == 0)
                    {
                        while (environmentCharArray[i] != '\0')
                        {
                            i++;
                        }
                    }
                    else
                    {
                        string str = new string(environmentCharArray, startIndex, i - startIndex);
                        i++;
                        int num3 = i;
                        while (environmentCharArray[i] != '\0')
                        {
                            i++;
                        }
                        string str2 = new string(environmentCharArray, num3, i - num3);
                        hashtable[str] = str2;
                        if (!flag)
                        {
                            if (flag2)
                            {
                                flag2 = false;
                            }
                            else
                            {
                                builder.Append(';');
                            }
                            builder.Append(str);
                        }
                    }
                }
            }
            if (!flag)
            {
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, builder.ToString()).Demand();
            }
            return hashtable;
        }

        [SecuritySafeCritical]
        public static IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target)
        {
            if (target == EnvironmentVariableTarget.Process)
            {
                return GetEnvironmentVariables();
            }
            new EnvironmentPermission(PermissionState.Unrestricted).Demand();
            if (target == EnvironmentVariableTarget.Machine)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Session Manager\Environment", false))
                {
                    return GetRegistryKeyNameValuePairs(key);
                }
            }
            if (target == EnvironmentVariableTarget.User)
            {
                using (RegistryKey key2 = Registry.CurrentUser.OpenSubKey("Environment", false))
                {
                    return GetRegistryKeyNameValuePairs(key2);
                }
            }
            throw new ArgumentException(GetResourceString("Arg_EnumIllegalVal", new object[] { (int) target }));
        }

        [SecuritySafeCritical]
        public static string GetFolderPath(SpecialFolder folder)
        {
            return GetFolderPath(folder, SpecialFolderOption.None);
        }

        [SecuritySafeCritical]
        public static string GetFolderPath(SpecialFolder folder, SpecialFolderOption option)
        {
            if (!Enum.IsDefined(typeof(SpecialFolder), folder))
            {
                throw new ArgumentException(GetResourceString("Arg_EnumIllegalVal", new object[] { (int) folder }));
            }
            if (!Enum.IsDefined(typeof(SpecialFolderOption), option))
            {
                throw new ArgumentException(GetResourceString("Arg_EnumIllegalVal", new object[] { (int) option }));
            }
            if (option == SpecialFolderOption.Create)
            {
                new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.Write }.Demand();
            }
            StringBuilder lpszPath = new StringBuilder(260);
            int num = Win32Native.SHGetFolderPath(IntPtr.Zero, (int) (folder | ((SpecialFolder) ((int) option))), IntPtr.Zero, 0, lpszPath);
            if (num < 0)
            {
                switch (num)
                {
                    case -2146233031:
                        throw new PlatformNotSupportedException();
                }
            }
            string path = lpszPath.ToString();
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
            return path;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool GetIsCLRHosted();
        [SecuritySafeCritical]
        public static string[] GetLogicalDrives()
        {
            new EnvironmentPermission(PermissionState.Unrestricted).Demand();
            int logicalDrives = Win32Native.GetLogicalDrives();
            if (logicalDrives == 0)
            {
                __Error.WinIOError();
            }
            uint num2 = (uint) logicalDrives;
            int num3 = 0;
            while (num2 != 0)
            {
                if ((num2 & 1) != 0)
                {
                    num3++;
                }
                num2 = num2 >> 1;
            }
            string[] strArray = new string[num3];
            char[] chArray = new char[] { 'A', ':', '\\' };
            num2 = (uint) logicalDrives;
            num3 = 0;
            while (num2 != 0)
            {
                if ((num2 & 1) != 0)
                {
                    strArray[num3++] = new string(chArray);
                }
                num2 = num2 >> 1;
                chArray[0] = (char) (chArray[0] + '\x0001');
            }
            return strArray;
        }

        internal static IDictionary GetRegistryKeyNameValuePairs(RegistryKey registryKey)
        {
            Hashtable hashtable = new Hashtable(20);
            if (registryKey != null)
            {
                foreach (string str in registryKey.GetValueNames())
                {
                    string str2 = registryKey.GetValue(str, "").ToString();
                    hashtable.Add(str, str2);
                }
            }
            return hashtable;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string GetResourceFromDefault(string key);
        [SecuritySafeCritical]
        internal static string GetResourceString(string key)
        {
            return GetResourceFromDefault(key);
        }

        [SecuritySafeCritical]
        internal static string GetResourceString(string key, params object[] values)
        {
            string resourceFromDefault = GetResourceFromDefault(key);
            return string.Format(CultureInfo.CurrentCulture, resourceFromDefault, values);
        }

        internal static string GetResourceStringLocal(string key)
        {
            if (m_resHelper == null)
            {
                InitResourceHelper();
            }
            return m_resHelper.GetResourceString(key);
        }

        [SecuritySafeCritical]
        internal static string GetRuntimeResourceString(string key)
        {
            return GetResourceFromDefault(key);
        }

        [SecuritySafeCritical]
        internal static string GetRuntimeResourceString(string key, params object[] values)
        {
            string resourceFromDefault = GetResourceFromDefault(key);
            return string.Format(CultureInfo.CurrentCulture, resourceFromDefault, values);
        }

        internal static string GetStackTrace(Exception e, bool needFileInfo)
        {
            System.Diagnostics.StackTrace trace;
            if (e == null)
            {
                trace = new System.Diagnostics.StackTrace(needFileInfo);
            }
            else
            {
                trace = new System.Diagnostics.StackTrace(e, needFileInfo);
            }
            return trace.ToString(System.Diagnostics.StackTrace.TraceFormat.Normal);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool GetVersion(Win32Native.OSVERSIONINFO osVer);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool GetVersionEx(Win32Native.OSVERSIONINFOEX osVer);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern long GetWorkingSet();
        [SecuritySafeCritical]
        private static void InitResourceHelper()
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(InternalSyncObject, ref lockTaken);
                if (m_resHelper == null)
                {
                    ResourceHelper helper = new ResourceHelper("mscorlib");
                    Thread.MemoryBarrier();
                    m_resHelper = helper;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(InternalSyncObject);
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string nativeGetEnvironmentVariable(string variable);
        [SecuritySafeCritical]
        public static void SetEnvironmentVariable(string variable, string value)
        {
            CheckEnvironmentVariableName(variable);
            new EnvironmentPermission(PermissionState.Unrestricted).Demand();
            if (string.IsNullOrEmpty(value) || (value[0] == '\0'))
            {
                value = null;
            }
            else if (value.Length >= 0x7fff)
            {
                throw new ArgumentException(GetResourceString("Argument_LongEnvVarValue"));
            }
            if (!Win32Native.SetEnvironmentVariable(variable, value))
            {
                int errorCode = Marshal.GetLastWin32Error();
                switch (errorCode)
                {
                    case 0xcb:
                        return;

                    case 0xce:
                        throw new ArgumentException(GetResourceString("Argument_LongEnvVarValue"));
                }
                throw new ArgumentException(Win32Native.GetMessage(errorCode));
            }
        }

        [SecuritySafeCritical]
        public static void SetEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target)
        {
            if (target == EnvironmentVariableTarget.Process)
            {
                SetEnvironmentVariable(variable, value);
                return;
            }
            CheckEnvironmentVariableName(variable);
            if (variable.Length >= 0x400)
            {
                throw new ArgumentException(GetResourceString("Argument_LongEnvVarName"));
            }
            new EnvironmentPermission(PermissionState.Unrestricted).Demand();
            if (string.IsNullOrEmpty(value) || (value[0] == '\0'))
            {
                value = null;
            }
            if (target == EnvironmentVariableTarget.Machine)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Session Manager\Environment", true))
                {
                    if (key != null)
                    {
                        if (value == null)
                        {
                            key.DeleteValue(variable, false);
                        }
                        else
                        {
                            key.SetValue(variable, value);
                        }
                    }
                    goto Label_0100;
                }
            }
            if (target == EnvironmentVariableTarget.User)
            {
                if (variable.Length >= 0xff)
                {
                    throw new ArgumentException(GetResourceString("Argument_LongEnvVarValue"));
                }
                using (RegistryKey key2 = Registry.CurrentUser.OpenSubKey("Environment", true))
                {
                    if (key2 != null)
                    {
                        if (value == null)
                        {
                            key2.DeleteValue(variable, false);
                        }
                        else
                        {
                            key2.SetValue(variable, value);
                        }
                    }
                    goto Label_0100;
                }
            }
            throw new ArgumentException(GetResourceString("Arg_EnumIllegalVal", new object[] { (int) target }));
        Label_0100:
            bool flag1 = Win32Native.SendMessageTimeout(new IntPtr(0xffff), 0x1a, IntPtr.Zero, "Environment", 0, 0x3e8, IntPtr.Zero) == IntPtr.Zero;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void TriggerCodeContractFailure(ContractFailureKind failureKind, string message, string condition, string exceptionAsString);

        public static string CommandLine
        {
            [SecuritySafeCritical]
            get
            {
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Demand();
                string s = null;
                GetCommandLine(JitHelpers.GetStringHandleOnStack(ref s));
                return s;
            }
        }

        public static string CurrentDirectory
        {
            get
            {
                return Directory.GetCurrentDirectory();
            }
            set
            {
                Directory.SetCurrentDirectory(value);
            }
        }

        public static int ExitCode { [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical] get; [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical] set; }

        public static bool HasShutdownStarted { [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical] get; }

        private static object InternalSyncObject
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal static string InternalWindowsDirectory
        {
            [SecurityCritical]
            get
            {
                StringBuilder sb = new StringBuilder(260);
                if (Win32Native.GetWindowsDirectory(sb, 260) == 0)
                {
                    __Error.WinIOError();
                }
                return sb.ToString();
            }
        }

        public static bool Is64BitOperatingSystem
        {
            [SecuritySafeCritical]
            get
            {
                bool flag;
                return ((Win32Native.DoesWin32MethodExist("kernel32.dll", "IsWow64Process") && Win32Native.IsWow64Process(Win32Native.GetCurrentProcess(), out flag)) && flag);
            }
        }

        public static bool Is64BitProcess
        {
            get
            {
                return false;
            }
        }

        internal static bool IsCLRHosted
        {
            [SecuritySafeCritical]
            get
            {
                return GetIsCLRHosted();
            }
        }

        internal static bool IsW2k3
        {
            get
            {
                if (!s_CheckedOSW2k3)
                {
                    OperatingSystem oSVersion = OSVersion;
                    s_IsW2k3 = ((oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version.Major == 5)) && (oSVersion.Version.Minor == 2);
                    s_CheckedOSW2k3 = true;
                }
                return s_IsW2k3;
            }
        }

        internal static bool IsWindowsVista
        {
            get
            {
                if (!s_CheckedOSType)
                {
                    OperatingSystem oSVersion = OSVersion;
                    s_IsWindowsVista = (oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version.Major >= 6);
                    s_CheckedOSType = true;
                }
                return s_IsWindowsVista;
            }
        }

        public static string MachineName
        {
            [SecuritySafeCritical]
            get
            {
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, "COMPUTERNAME").Demand();
                StringBuilder nameBuffer = new StringBuilder(0x100);
                int bufferSize = 0x100;
                if (Win32Native.GetComputerName(nameBuffer, ref bufferSize) == 0)
                {
                    throw new InvalidOperationException(GetResourceString("InvalidOperation_ComputerName"));
                }
                return nameBuffer.ToString();
            }
        }

        public static string NewLine
        {
            get
            {
                return "\r\n";
            }
        }

        internal static OSName OSInfo
        {
            [SecuritySafeCritical]
            get
            {
                if (m_osname == OSName.Invalid)
                {
                    lock (InternalSyncObject)
                    {
                        if (m_osname == OSName.Invalid)
                        {
                            Win32Native.OSVERSIONINFO osVer = new Win32Native.OSVERSIONINFO();
                            if (!GetVersion(osVer))
                            {
                                throw new InvalidOperationException(GetResourceString("InvalidOperation_GetVersion"));
                            }
                            switch (osVer.PlatformId)
                            {
                                case 1:
                                    m_osname = OSName.Unknown;
                                    goto Label_0112;

                                case 2:
                                    switch (osVer.MajorVersion)
                                    {
                                        case 4:
                                            goto Label_008F;
                                    }
                                    goto Label_0097;

                                case 11:
                                    if (osVer.MajorVersion != 10)
                                    {
                                        goto Label_00F4;
                                    }
                                    switch (osVer.MinorVersion)
                                    {
                                        case 4:
                                            m_osname = OSName.Tiger;
                                            goto Label_0112;

                                        case 5:
                                            m_osname = OSName.Leopard;
                                            goto Label_0112;
                                    }
                                    m_osname = OSName.MacOSX;
                                    goto Label_0112;

                                default:
                                    goto Label_0100;
                            }
                            m_osname = OSName.Win2k;
                        }
                        goto Label_0112;
                    Label_008F:
                        m_osname = OSName.Unknown;
                        goto Label_0112;
                    Label_0097:
                        m_osname = OSName.WinNT;
                        goto Label_0112;
                    Label_00F4:
                        m_osname = OSName.MacOSX;
                        goto Label_0112;
                    Label_0100:
                        m_osname = OSName.Unknown;
                    }
                }
            Label_0112:
                return m_osname;
            }
        }

        public static OperatingSystem OSVersion
        {
            [SecuritySafeCritical]
            get
            {
                if (m_os == null)
                {
                    PlatformID unix;
                    bool flag;
                    Win32Native.OSVERSIONINFO osVer = new Win32Native.OSVERSIONINFO();
                    if (!GetVersion(osVer))
                    {
                        throw new InvalidOperationException(GetResourceString("InvalidOperation_GetVersion"));
                    }
                    switch (osVer.PlatformId)
                    {
                        case 10:
                            unix = PlatformID.Unix;
                            flag = false;
                            break;

                        case 11:
                            unix = PlatformID.MacOSX;
                            flag = false;
                            break;

                        case 2:
                            unix = PlatformID.Win32NT;
                            flag = true;
                            break;

                        default:
                            throw new InvalidOperationException(GetResourceString("InvalidOperation_InvalidPlatformID"));
                    }
                    Win32Native.OSVERSIONINFOEX osversioninfoex = new Win32Native.OSVERSIONINFOEX();
                    if (flag && !GetVersionEx(osversioninfoex))
                    {
                        throw new InvalidOperationException(GetResourceString("InvalidOperation_GetVersion"));
                    }
                    System.Version version = new System.Version(osVer.MajorVersion, osVer.MinorVersion, osVer.BuildNumber, (osversioninfoex.ServicePackMajor << 0x10) | osversioninfoex.ServicePackMinor);
                    m_os = new OperatingSystem(unix, version, osVer.CSDVersion);
                }
                return m_os;
            }
        }

        public static int ProcessorCount
        {
            [SecuritySafeCritical]
            get
            {
                Win32Native.SYSTEM_INFO lpSystemInfo = new Win32Native.SYSTEM_INFO();
                Win32Native.GetSystemInfo(ref lpSystemInfo);
                return lpSystemInfo.dwNumberOfProcessors;
            }
        }

        internal static bool RunningOnWinNT
        {
            get
            {
                return (OSVersion.Platform == PlatformID.Win32NT);
            }
        }

        public static string StackTrace
        {
            [SecuritySafeCritical]
            get
            {
                new EnvironmentPermission(PermissionState.Unrestricted).Demand();
                return GetStackTrace(null, true);
            }
        }

        public static string SystemDirectory
        {
            [SecuritySafeCritical]
            get
            {
                StringBuilder sb = new StringBuilder(260);
                if (Win32Native.GetSystemDirectory(sb, 260) == 0)
                {
                    __Error.WinIOError();
                }
                string path = sb.ToString();
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
                return path;
            }
        }

        public static int SystemPageSize
        {
            [SecuritySafeCritical]
            get
            {
                new EnvironmentPermission(PermissionState.Unrestricted).Demand();
                Win32Native.SYSTEM_INFO lpSystemInfo = new Win32Native.SYSTEM_INFO();
                Win32Native.GetSystemInfo(ref lpSystemInfo);
                return lpSystemInfo.dwPageSize;
            }
        }

        public static int TickCount { [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical] get; }

        public static string UserDomainName
        {
            [SecuritySafeCritical]
            get
            {
                int num3;
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, "UserDomain").Demand();
                byte[] sid = new byte[0x400];
                int length = sid.Length;
                StringBuilder domainName = new StringBuilder(0x400);
                int capacity = domainName.Capacity;
                if (Win32Native.GetUserNameEx(2, domainName, ref capacity) == 1)
                {
                    string str = domainName.ToString();
                    int index = str.IndexOf('\\');
                    if (index != -1)
                    {
                        return str.Substring(0, index);
                    }
                }
                capacity = domainName.Capacity;
                if (!Win32Native.LookupAccountName(null, UserName, sid, ref length, domainName, ref capacity, out num3))
                {
                    throw new InvalidOperationException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
                }
                return domainName.ToString();
            }
        }

        public static bool UserInteractive
        {
            [SecuritySafeCritical]
            get
            {
                if ((OSInfo & OSName.WinNT) == OSName.WinNT)
                {
                    IntPtr processWindowStation = Win32Native.GetProcessWindowStation();
                    if ((processWindowStation != IntPtr.Zero) && (processWinStation != processWindowStation))
                    {
                        int lpnLengthNeeded = 0;
                        Win32Native.USEROBJECTFLAGS pvBuffer = new Win32Native.USEROBJECTFLAGS();
                        if (Win32Native.GetUserObjectInformation(processWindowStation, 1, pvBuffer, Marshal.SizeOf(pvBuffer), ref lpnLengthNeeded) && ((pvBuffer.dwFlags & 1) == 0))
                        {
                            isUserNonInteractive = true;
                        }
                        processWinStation = processWindowStation;
                    }
                }
                return !isUserNonInteractive;
            }
        }

        public static string UserName
        {
            [SecuritySafeCritical]
            get
            {
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, "UserName").Demand();
                StringBuilder lpBuffer = new StringBuilder(0x100);
                int capacity = lpBuffer.Capacity;
                Win32Native.GetUserName(lpBuffer, ref capacity);
                return lpBuffer.ToString();
            }
        }

        public static System.Version Version
        {
            get
            {
                return new System.Version("4.0.30319.296");
            }
        }

        public static long WorkingSet
        {
            [SecuritySafeCritical]
            get
            {
                new EnvironmentPermission(PermissionState.Unrestricted).Demand();
                return GetWorkingSet();
            }
        }

        [Serializable]
        internal enum OSName
        {
            Invalid = 0,
            Leopard = 0x102,
            MacOSX = 0x100,
            Nt4 = 0x81,
            Tiger = 0x101,
            Unknown = 1,
            Win2k = 130,
            WinNT = 0x80
        }

        internal sealed class ResourceHelper
        {
            private Stack currentlyLoading;
            private string m_name;
            internal bool resourceManagerInited;
            private ResourceManager SystemResMgr;

            internal ResourceHelper(string name)
            {
                this.m_name = name;
            }

            [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal string GetResourceString(string key)
            {
                if ((key != null) && (key.Length != 0))
                {
                    return this.GetResourceString(key, null);
                }
                return "[Resource lookup failed - null or empty resource name]";
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
            internal string GetResourceString(string key, CultureInfo culture)
            {
                if ((key == null) || (key.Length == 0))
                {
                    return "[Resource lookup failed - null or empty resource name]";
                }
                GetResourceStringUserData userData = new GetResourceStringUserData(this, key, culture);
                RuntimeHelpers.TryCode code = new RuntimeHelpers.TryCode(this.GetResourceStringCode);
                RuntimeHelpers.CleanupCode backoutCode = new RuntimeHelpers.CleanupCode(this.GetResourceStringBackoutCode);
                RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(code, backoutCode, userData);
                return userData.m_retVal;
            }

            [PrePrepareMethod]
            private void GetResourceStringBackoutCode(object userDataIn, bool exceptionThrown)
            {
                GetResourceStringUserData data = (GetResourceStringUserData) userDataIn;
                Environment.ResourceHelper resourceHelper = data.m_resourceHelper;
                if (exceptionThrown && data.m_lockWasTaken)
                {
                    resourceHelper.SystemResMgr = null;
                    resourceHelper.currentlyLoading = null;
                }
                if (data.m_lockWasTaken)
                {
                    Monitor.Exit(resourceHelper);
                }
            }

            [SecuritySafeCritical]
            private void GetResourceStringCode(object userDataIn)
            {
                GetResourceStringUserData data = (GetResourceStringUserData) userDataIn;
                Environment.ResourceHelper resourceHelper = data.m_resourceHelper;
                string key = data.m_key;
                CultureInfo culture = data.m_culture;
                Monitor.Enter(resourceHelper, ref data.m_lockWasTaken);
                if (((resourceHelper.currentlyLoading != null) && (resourceHelper.currentlyLoading.Count > 0)) && resourceHelper.currentlyLoading.Contains(key))
                {
                    try
                    {
                        new StackTrace(true).ToString(StackTrace.TraceFormat.NoResourceLookup);
                    }
                    catch (StackOverflowException)
                    {
                    }
                    catch (NullReferenceException)
                    {
                    }
                    catch (OutOfMemoryException)
                    {
                    }
                    data.m_retVal = "[Resource lookup failed - infinite recursion or critical failure detected.]";
                }
                else
                {
                    if (resourceHelper.currentlyLoading == null)
                    {
                        resourceHelper.currentlyLoading = new Stack(4);
                    }
                    if (!resourceHelper.resourceManagerInited)
                    {
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                        }
                        finally
                        {
                            RuntimeHelpers.RunClassConstructor(typeof(ResourceManager).TypeHandle);
                            RuntimeHelpers.RunClassConstructor(typeof(ResourceReader).TypeHandle);
                            RuntimeHelpers.RunClassConstructor(typeof(RuntimeResourceSet).TypeHandle);
                            RuntimeHelpers.RunClassConstructor(typeof(BinaryReader).TypeHandle);
                            resourceHelper.resourceManagerInited = true;
                        }
                    }
                    resourceHelper.currentlyLoading.Push(key);
                    if (resourceHelper.SystemResMgr == null)
                    {
                        resourceHelper.SystemResMgr = new ResourceManager(this.m_name, typeof(object).Assembly);
                    }
                    string str2 = resourceHelper.SystemResMgr.GetString(key, null);
                    resourceHelper.currentlyLoading.Pop();
                    data.m_retVal = str2;
                }
            }

            internal class GetResourceStringUserData
            {
                public CultureInfo m_culture;
                public string m_key;
                public bool m_lockWasTaken;
                public Environment.ResourceHelper m_resourceHelper;
                public string m_retVal;

                public GetResourceStringUserData(Environment.ResourceHelper resourceHelper, string key, CultureInfo culture)
                {
                    this.m_resourceHelper = resourceHelper;
                    this.m_key = key;
                    this.m_culture = culture;
                }
            }
        }

        [ComVisible(true)]
        public enum SpecialFolder
        {
            AdminTools = 0x30,
            ApplicationData = 0x1a,
            CDBurning = 0x3b,
            CommonAdminTools = 0x2f,
            CommonApplicationData = 0x23,
            CommonDesktopDirectory = 0x19,
            CommonDocuments = 0x2e,
            CommonMusic = 0x35,
            CommonOemLinks = 0x3a,
            CommonPictures = 0x36,
            CommonProgramFiles = 0x2b,
            CommonProgramFilesX86 = 0x2c,
            CommonPrograms = 0x17,
            CommonStartMenu = 0x16,
            CommonStartup = 0x18,
            CommonTemplates = 0x2d,
            CommonVideos = 0x37,
            Cookies = 0x21,
            Desktop = 0,
            DesktopDirectory = 0x10,
            Favorites = 6,
            Fonts = 20,
            History = 0x22,
            InternetCache = 0x20,
            LocalApplicationData = 0x1c,
            LocalizedResources = 0x39,
            MyComputer = 0x11,
            MyDocuments = 5,
            MyMusic = 13,
            MyPictures = 0x27,
            MyVideos = 14,
            NetworkShortcuts = 0x13,
            Personal = 5,
            PrinterShortcuts = 0x1b,
            ProgramFiles = 0x26,
            ProgramFilesX86 = 0x2a,
            Programs = 2,
            Recent = 8,
            Resources = 0x38,
            SendTo = 9,
            StartMenu = 11,
            Startup = 7,
            System = 0x25,
            SystemX86 = 0x29,
            Templates = 0x15,
            UserProfile = 40,
            Windows = 0x24
        }

        public enum SpecialFolderOption
        {
            Create = 0x8000,
            DoNotVerify = 0x4000,
            None = 0
        }
    }
}

