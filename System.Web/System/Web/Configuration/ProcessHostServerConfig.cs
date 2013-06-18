namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    internal sealed class ProcessHostServerConfig : IServerConfig, IServerConfig2
    {
        private string _siteNameForCurrentApplication;
        private static object s_initLock = new object();
        private static ProcessHostServerConfig s_instance;

        static ProcessHostServerConfig()
        {
            HttpRuntime.ForceStaticInit();
        }

        private ProcessHostServerConfig()
        {
            if (HostingEnvironment.SupportFunctions == null)
            {
                ProcessHostConfigUtils.InitStandaloneConfig();
            }
            else
            {
                IProcessHostSupportFunctions supportFunctions = HostingEnvironment.SupportFunctions;
                if (supportFunctions != null)
                {
                    IntPtr nativeConfigurationSystem = supportFunctions.GetNativeConfigurationSystem();
                    if (IntPtr.Zero != nativeConfigurationSystem)
                    {
                        UnsafeIISMethods.MgdSetNativeConfiguration(nativeConfigurationSystem);
                    }
                }
            }
            this._siteNameForCurrentApplication = HostingEnvironment.SiteNameNoDemand;
            if (this._siteNameForCurrentApplication == null)
            {
                this._siteNameForCurrentApplication = ProcessHostConfigUtils.GetSiteNameFromId(1);
            }
        }

        internal static IServerConfig GetInstance()
        {
            if (s_instance == null)
            {
                lock (s_initLock)
                {
                    if (s_instance == null)
                    {
                        s_instance = new ProcessHostServerConfig();
                    }
                }
            }
            return s_instance;
        }

        string IServerConfig.GetSiteNameFromSiteID(string siteID)
        {
            uint num;
            if (!uint.TryParse(siteID, out num))
            {
                return string.Empty;
            }
            return ProcessHostConfigUtils.GetSiteNameFromId(num);
        }

        bool IServerConfig.GetUncUser(IApplicationHost appHost, VirtualPath path, out string username, out string password)
        {
            bool flag = false;
            username = null;
            password = null;
            IntPtr zero = IntPtr.Zero;
            int cchUserName = 0;
            IntPtr bstrPassword = IntPtr.Zero;
            int cchPassword = 0;
            try
            {
                if (UnsafeIISMethods.MgdGetVrPathCreds(IntPtr.Zero, appHost.GetSiteName(), path.VirtualPathString, out zero, out cchUserName, out bstrPassword, out cchPassword) == 0)
                {
                    username = (cchUserName > 0) ? StringUtil.StringFromWCharPtr(zero, cchUserName) : null;
                    password = (cchPassword > 0) ? StringUtil.StringFromWCharPtr(bstrPassword, cchPassword) : null;
                    flag = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(zero);
                }
                if (bstrPassword != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstrPassword);
                }
            }
            return flag;
        }

        string[] IServerConfig.GetVirtualSubdirs(VirtualPath path, bool inApp)
        {
            if (!inApp)
            {
                throw new NotSupportedException();
            }
            string virtualPathString = path.VirtualPathString;
            string[] strArray = null;
            int num = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr bstrPath = IntPtr.Zero;
            int cchPath = 0;
            try
            {
                int count = 0;
                int num4 = UnsafeIISMethods.MgdGetAppCollection(IntPtr.Zero, this._siteNameForCurrentApplication, virtualPathString, out bstrPath, out cchPath, out zero, out count);
                if ((num4 < 0) || (bstrPath == IntPtr.Zero))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Cant_Enumerate_NativeDirs", new object[] { num4 }));
                }
                string str2 = StringUtil.StringFromWCharPtr(bstrPath, cchPath);
                Marshal.FreeBSTR(bstrPath);
                bstrPath = IntPtr.Zero;
                cchPath = 0;
                strArray = new string[count];
                int length = virtualPathString.Length;
                if (virtualPathString[length - 1] == '/')
                {
                    length--;
                }
                int startIndex = str2.Length;
                string str3 = (length > startIndex) ? virtualPathString.Substring(startIndex, length - startIndex) : string.Empty;
                for (uint i = 0; i < count; i++)
                {
                    num4 = UnsafeIISMethods.MgdGetNextVPath(zero, i, out bstrPath, out cchPath);
                    if ((num4 < 0) || (bstrPath == IntPtr.Zero))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Cant_Enumerate_NativeDirs", new object[] { num4 }));
                    }
                    string str4 = (cchPath > 1) ? StringUtil.StringFromWCharPtr(bstrPath, cchPath) : null;
                    Marshal.FreeBSTR(bstrPath);
                    bstrPath = IntPtr.Zero;
                    cchPath = 0;
                    if ((str4 != null) && (str4.Length > str3.Length))
                    {
                        if (str3.Length == 0)
                        {
                            if (str4.IndexOf('/', 1) == -1)
                            {
                                strArray[num++] = str4.Substring(1);
                            }
                        }
                        else if (StringUtil.EqualsIgnoreCase(str3, 0, str4, 0, str3.Length))
                        {
                            int index = str4.IndexOf('/', 1 + str3.Length);
                            if (index > -1)
                            {
                                strArray[num++] = str4.Substring(str3.Length + 1, index - str3.Length);
                            }
                            else
                            {
                                strArray[num++] = str4.Substring(str3.Length + 1);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                    zero = IntPtr.Zero;
                }
                if (bstrPath != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstrPath);
                    bstrPath = IntPtr.Zero;
                }
            }
            string[] strArray2 = null;
            if (num > 0)
            {
                strArray2 = new string[num];
                for (int j = 0; j < strArray2.Length; j++)
                {
                    strArray2[j] = strArray[j];
                }
            }
            return strArray2;
        }

        long IServerConfig.GetW3WPMemoryLimitInKB()
        {
            long limit = 0L;
            if (UnsafeIISMethods.MgdGetMemoryLimitKB(out limit) < 0)
            {
                return 0L;
            }
            return limit;
        }

        string IServerConfig.MapPath(IApplicationHost appHost, VirtualPath path)
        {
            string siteName = (appHost == null) ? this._siteNameForCurrentApplication : appHost.GetSiteName();
            string physicalPath = ProcessHostConfigUtils.MapPathActual(siteName, path);
            if (FileUtil.IsSuspiciousPhysicalPath(physicalPath))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Cannot_map_path", new object[] { path.VirtualPathString }));
            }
            return physicalPath;
        }

        bool IServerConfig2.IsWithinApp(string virtualPath)
        {
            return UnsafeIISMethods.MgdIsWithinApp(IntPtr.Zero, this._siteNameForCurrentApplication, HttpRuntime.AppDomainAppVirtualPathString, virtualPath);
        }
    }
}

