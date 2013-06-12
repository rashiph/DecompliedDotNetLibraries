namespace System.Security.Util
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Policy;

    internal static class Config
    {
        private static string m_machineConfig;
        private static string m_userConfig;

        [SecurityCritical]
        internal static void AddCacheEntry(ConfigId id, int numKey, byte[] key, byte[] data)
        {
            AddCacheEntry(id, numKey, key, key.Length, data, data.Length);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddCacheEntry(ConfigId id, int numKey, [In] byte[] key, int keyLength, byte[] data, int dataLength);
        [SecurityCritical]
        internal static bool GetCacheEntry(ConfigId id, int numKey, byte[] key, out byte[] data)
        {
            byte[] o = null;
            bool flag = GetCacheEntry(id, numKey, key, key.Length, JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
            data = o;
            return flag;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool GetCacheEntry(ConfigId id, int numKey, [In] byte[] key, int keyLength, ObjectHandleOnStack retData);
        [SecurityCritical]
        private static void GetFileLocales()
        {
            if (m_machineConfig == null)
            {
                string s = null;
                GetMachineDirectory(JitHelpers.GetStringHandleOnStack(ref s));
                m_machineConfig = s;
            }
            if (m_userConfig == null)
            {
                string str2 = null;
                GetUserDirectory(JitHelpers.GetStringHandleOnStack(ref str2));
                m_userConfig = str2;
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetMachineDirectory(StringHandleOnStack retDirectory);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetUserDirectory(StringHandleOnStack retDirectory);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool RecoverData(ConfigId id);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void ResetCacheData(ConfigId id);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int SaveDataByte(string path, [In] byte[] data, int length);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void SetQuickCache(ConfigId id, QuickCacheEntryType quickCacheFlags);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool WriteToEventLog(string message);

        internal static string MachineDirectory
        {
            [SecurityCritical]
            get
            {
                GetFileLocales();
                return m_machineConfig;
            }
        }

        internal static string UserDirectory
        {
            [SecurityCritical]
            get
            {
                GetFileLocales();
                return m_userConfig;
            }
        }
    }
}

