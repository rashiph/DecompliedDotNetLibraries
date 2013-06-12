namespace System.Diagnostics
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    internal static class SharedUtils
    {
        private static int environment = 0;
        internal const int NonNtEnvironment = 3;
        internal const int NtEnvironment = 2;
        private static object s_InternalSyncObject;
        internal const int UnknownEnvironment = 0;
        internal const int W2kEnvironment = 1;

        internal static void CheckEnvironment()
        {
            if (CurrentEnvironment == 3)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
        }

        internal static void CheckNtEnvironment()
        {
            if (CurrentEnvironment == 2)
            {
                throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
            }
        }

        internal static Win32Exception CreateSafeWin32Exception()
        {
            return CreateSafeWin32Exception(0);
        }

        internal static Win32Exception CreateSafeWin32Exception(int error)
        {
            Win32Exception exception = null;
            new SecurityPermission(PermissionState.Unrestricted).Assert();
            try
            {
                if (error == 0)
                {
                    return new Win32Exception();
                }
                exception = new Win32Exception(error);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return exception;
        }

        internal static void EnterMutex(string name, ref Mutex mutex)
        {
            string mutexName = null;
            if (CurrentEnvironment == 1)
            {
                mutexName = @"Global\" + name;
            }
            else
            {
                mutexName = name;
            }
            EnterMutexWithoutGlobal(mutexName, ref mutex);
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal=true)]
        internal static void EnterMutexWithoutGlobal(string mutexName, ref Mutex mutex)
        {
            bool flag;
            MutexSecurity mutexSecurity = new MutexSecurity();
            SecurityIdentifier identity = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            mutexSecurity.AddAccessRule(new MutexAccessRule(identity, MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow));
            Mutex mutexIn = new Mutex(false, mutexName, out flag, mutexSecurity);
            SafeWaitForMutex(mutexIn, ref mutex);
        }

        private static int GetLargestBuildNumberFromKey(RegistryKey rootKey)
        {
            int num = -1;
            string[] valueNames = rootKey.GetValueNames();
            for (int i = 0; i < valueNames.Length; i++)
            {
                int num3;
                if (int.TryParse(valueNames[i], out num3))
                {
                    num = (num > num3) ? num : num3;
                }
            }
            return num;
        }

        internal static string GetLatestBuildDllDirectory(string machineName)
        {
            string str = "";
            RegistryKey key = null;
            RegistryKey key2 = null;
            new RegistryPermission(PermissionState.Unrestricted).Assert();
            try
            {
                if (machineName.Equals("."))
                {
                    return GetLocalBuildDirectory();
                }
                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machineName);
                if (key == null)
                {
                    throw new InvalidOperationException(SR.GetString("RegKeyMissingShort", new object[] { "HKEY_LOCAL_MACHINE", machineName }));
                }
                key2 = key.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework");
                if (key2 == null)
                {
                    return str;
                }
                string str2 = (string) key2.GetValue("InstallRoot");
                if ((str2 == null) || !(str2 != string.Empty))
                {
                    return str;
                }
                string name = string.Concat(new object[] { "v", Environment.Version.Major, ".", Environment.Version.Minor });
                RegistryKey key3 = key2.OpenSubKey("policy");
                string str4 = null;
                if (key3 == null)
                {
                    return str;
                }
                try
                {
                    RegistryKey rootKey = key3.OpenSubKey(name);
                    if (rootKey != null)
                    {
                        try
                        {
                            str4 = name + "." + GetLargestBuildNumberFromKey(rootKey);
                            goto Label_02A4;
                        }
                        finally
                        {
                            rootKey.Close();
                        }
                    }
                    string[] subKeyNames = key3.GetSubKeyNames();
                    int[] numArray = new int[] { -1, -1, -1 };
                    for (int i = 0; i < subKeyNames.Length; i++)
                    {
                        string str5 = subKeyNames[i];
                        if (((str5.Length > 1) && (str5[0] == 'v')) && str5.Contains("."))
                        {
                            int[] numArray2 = new int[] { -1, -1, -1 };
                            string[] strArray2 = str5.Substring(1).Split(new char[] { '.' });
                            if (((strArray2.Length == 2) && int.TryParse(strArray2[0], out numArray2[0])) && int.TryParse(strArray2[1], out numArray2[1]))
                            {
                                RegistryKey key5 = key3.OpenSubKey(str5);
                                if (key5 != null)
                                {
                                    try
                                    {
                                        numArray2[2] = GetLargestBuildNumberFromKey(key5);
                                        if ((numArray2[0] > numArray[0]) || ((numArray2[0] == numArray[0]) && (numArray2[1] > numArray[1])))
                                        {
                                            numArray = numArray2;
                                        }
                                    }
                                    finally
                                    {
                                        key5.Close();
                                    }
                                }
                            }
                        }
                    }
                    str4 = string.Concat(new object[] { "v", numArray[0], ".", numArray[1], ".", numArray[2] });
                }
                finally
                {
                    key3.Close();
                }
            Label_02A4:
                if ((str4 == null) || !(str4 != string.Empty))
                {
                    return str;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append(str2);
                if (!str2.EndsWith(@"\", StringComparison.Ordinal))
                {
                    builder.Append(@"\");
                }
                builder.Append(str4);
                return builder.ToString();
            }
            catch
            {
            }
            finally
            {
                if (key2 != null)
                {
                    key2.Close();
                }
                if (key != null)
                {
                    key.Close();
                }
                CodeAccessPermission.RevertAssert();
            }
            return str;
        }

        private static string GetLocalBuildDirectory()
        {
            return RuntimeEnvironment.GetRuntimeDirectory();
        }

        private static bool SafeWaitForMutex(Mutex mutexIn, ref Mutex mutexOut)
        {
            while (SafeWaitForMutexOnce(mutexIn, ref mutexOut))
            {
                if (mutexOut != null)
                {
                    return true;
                }
                Thread.Sleep(0);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool SafeWaitForMutexOnce(Mutex mutexIn, ref Mutex mutexOut)
        {
            bool flag;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                Thread.BeginCriticalRegion();
                Thread.BeginThreadAffinity();
                switch (WaitForSingleObjectDontCallThis(mutexIn.SafeWaitHandle, 500))
                {
                    case 0:
                    case 0x80:
                        mutexOut = mutexIn;
                        flag = true;
                        break;

                    case 0x102:
                        flag = true;
                        break;

                    default:
                        flag = false;
                        break;
                }
                if (mutexOut == null)
                {
                    Thread.EndThreadAffinity();
                    Thread.EndCriticalRegion();
                }
            }
            return flag;
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", EntryPoint="WaitForSingleObject", SetLastError=true, ExactSpelling=true)]
        private static extern int WaitForSingleObjectDontCallThis(SafeWaitHandle handle, int timeout);

        internal static int CurrentEnvironment
        {
            get
            {
                if (environment == 0)
                {
                    lock (InternalSyncObject)
                    {
                        if (environment == 0)
                        {
                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                if (Environment.OSVersion.Version.Major >= 5)
                                {
                                    environment = 1;
                                }
                                else
                                {
                                    environment = 2;
                                }
                            }
                            else
                            {
                                environment = 3;
                            }
                        }
                    }
                }
                return environment;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }
    }
}

