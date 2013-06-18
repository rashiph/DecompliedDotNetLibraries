namespace System.Web.Configuration
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    internal sealed class NativeConfig : CriticalFinalizerObject, IDisposable
    {
        private IntPtr _nativeConfig;

        private NativeConfig()
        {
        }

        internal NativeConfig(string version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            int hresult = 0;
            using (new IISVersionHelper(version))
            {
                hresult = UnsafeIISMethods.MgdCreateNativeConfigSystem(out this._nativeConfig);
            }
            Misc.ThrowIfFailedHr(hresult);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this._nativeConfig != IntPtr.Zero)
            {
                IntPtr pConfigSystem = Interlocked.Exchange(ref this._nativeConfig, IntPtr.Zero);
                if (pConfigSystem != IntPtr.Zero)
                {
                    Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdReleaseNativeConfigSystem(pConfigSystem));
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        ~NativeConfig()
        {
            this.Dispose(false);
        }

        internal string GetSiteNameFromId(uint siteID)
        {
            IntPtr zero = IntPtr.Zero;
            int cchSiteName = 0;
            string str = null;
            try
            {
                str = ((UnsafeIISMethods.MgdGetSiteNameFromId(this._nativeConfig, siteID, out zero, out cchSiteName) == 0) && (zero != IntPtr.Zero)) ? StringUtil.StringFromWCharPtr(zero, cchSiteName) : string.Empty;
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

        internal string MapPathDirect(string siteName, VirtualPath path)
        {
            string str = null;
            IntPtr zero = IntPtr.Zero;
            int cchPath = 0;
            try
            {
                if (UnsafeIISMethods.MgdMapPathDirect(this._nativeConfig, siteName, path.VirtualPathString, out zero, out cchPath) < 0)
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

        internal int MgdGetAppCollection(string siteName, string virtualPath, out IntPtr pBstr, out int cBstr, out IntPtr pAppCollection, out int count)
        {
            return UnsafeIISMethods.MgdGetAppCollection(this._nativeConfig, siteName, virtualPath, out pBstr, out cBstr, out pAppCollection, out count);
        }

        internal int MgdGetAppPathForPath(uint siteId, string virtualPath, out IntPtr bstrPath, out int cchPath)
        {
            return UnsafeIISMethods.MgdGetAppPathForPath(this._nativeConfig, siteId, virtualPath, out bstrPath, out cchPath);
        }

        internal int MgdGetVrPathCreds(string siteName, string virtualPath, out IntPtr bstrUserName, out int cchUserName, out IntPtr bstrPassword, out int cchPassword)
        {
            return UnsafeIISMethods.MgdGetVrPathCreds(this._nativeConfig, siteName, virtualPath, out bstrUserName, out cchUserName, out bstrPassword, out cchPassword);
        }

        internal bool MgdIsWithinApp(string siteName, string appPath, string virtualPath)
        {
            return UnsafeIISMethods.MgdIsWithinApp(this._nativeConfig, siteName, appPath, virtualPath);
        }

        internal uint MgdResolveSiteName(string siteName)
        {
            return UnsafeIISMethods.MgdResolveSiteName(this._nativeConfig, siteName);
        }
    }
}

