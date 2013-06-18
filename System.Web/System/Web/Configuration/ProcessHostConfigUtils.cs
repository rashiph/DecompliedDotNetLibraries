namespace System.Web.Configuration
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    internal static class ProcessHostConfigUtils
    {
        private static NativeConfigWrapper _configWrapper;
        internal const string DEFAULT_SITE_ID_STRING = "1";
        internal const uint DEFAULT_SITE_ID_UINT = 1;
        private static string s_defaultSiteName;
        private static volatile int s_InitedExternalConfig;

        static ProcessHostConfigUtils()
        {
            HttpRuntime.ForceStaticInit();
        }

        internal static string GetSiteNameFromId(uint siteId)
        {
            if ((siteId == 1) && (s_defaultSiteName != null))
            {
                return s_defaultSiteName;
            }
            IntPtr zero = IntPtr.Zero;
            int cchSiteName = 0;
            string str = null;
            try
            {
                str = ((UnsafeIISMethods.MgdGetSiteNameFromId(IntPtr.Zero, siteId, out zero, out cchSiteName) == 0) && (zero != IntPtr.Zero)) ? StringUtil.StringFromWCharPtr(zero, cchSiteName) : string.Empty;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(zero);
                }
            }
            if (siteId == 1)
            {
                s_defaultSiteName = str;
            }
            return str;
        }

        internal static void InitStandaloneConfig()
        {
            if ((!HostingEnvironment.IsUnderIISProcess && !ServerConfig.UseMetabase) && (s_InitedExternalConfig == 0))
            {
                try
                {
                    _configWrapper = new NativeConfigWrapper();
                }
                finally
                {
                    s_InitedExternalConfig = 1;
                }
            }
        }

        internal static string MapPathActual(string siteName, VirtualPath path)
        {
            string str = null;
            IntPtr zero = IntPtr.Zero;
            int cchPath = 0;
            try
            {
                if (UnsafeIISMethods.MgdMapPathDirect(IntPtr.Zero, siteName, path.VirtualPathString, out zero, out cchPath) < 0)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Cannot_map_path", new object[] { path.VirtualPathString }));
                }
                str = (zero != IntPtr.Zero) ? StringUtil.StringFromWCharPtr(zero, cchPath) : null;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(zero);
                }
            }
            return str;
        }

        private class NativeConfigWrapper : CriticalFinalizerObject
        {
            internal NativeConfigWrapper()
            {
                int num = UnsafeIISMethods.MgdInitNativeConfig();
                if (num < 0)
                {
                    ProcessHostConfigUtils.s_InitedExternalConfig = 0;
                    throw new InvalidOperationException(System.Web.SR.GetString("Cant_Init_Native_Config", new object[] { num.ToString("X8", CultureInfo.InvariantCulture) }));
                }
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            ~NativeConfigWrapper()
            {
                UnsafeIISMethods.MgdTerminateNativeConfig();
            }
        }
    }
}

