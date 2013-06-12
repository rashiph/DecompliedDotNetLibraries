namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [ComVisible(true)]
    public class RuntimeEnvironment
    {
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void _GetSystemVersion(StringHandleOnStack retVer);
        public static bool FromGlobalAccessCache(Assembly a)
        {
            return a.GlobalAssemblyCache;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string GetDeveloperPath();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string GetHostBindingFile();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string GetModuleFileName();
        [SecuritySafeCritical]
        public static string GetRuntimeDirectory()
        {
            string runtimeDirectoryImpl = GetRuntimeDirectoryImpl();
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, runtimeDirectoryImpl).Demand();
            return runtimeDirectoryImpl;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string GetRuntimeDirectoryImpl();
        [SecurityCritical, ComVisible(false)]
        public static IntPtr GetRuntimeInterfaceAsIntPtr(Guid clsid, Guid riid)
        {
            return GetRuntimeInterfaceImpl(clsid, riid);
        }

        [SecurityCritical, ComVisible(false)]
        public static object GetRuntimeInterfaceAsObject(Guid clsid, Guid riid)
        {
            object objectForIUnknown;
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = GetRuntimeInterfaceImpl(clsid, riid);
                objectForIUnknown = Marshal.GetObjectForIUnknown(zero);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                }
            }
            return objectForIUnknown;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern IntPtr GetRuntimeInterfaceImpl([In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static string GetSystemVersion()
        {
            string s = null;
            _GetSystemVersion(JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        public static string SystemConfigurationFile
        {
            [SecuritySafeCritical]
            get
            {
                StringBuilder builder = new StringBuilder(260);
                builder.Append(GetRuntimeDirectory());
                builder.Append(AppDomainSetup.RuntimeConfigurationFile);
                string path = builder.ToString();
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
                return path;
            }
        }
    }
}

