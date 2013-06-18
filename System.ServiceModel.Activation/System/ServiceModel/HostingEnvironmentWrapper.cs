namespace System.ServiceModel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Hosting;

    internal static class HostingEnvironmentWrapper
    {
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static void DecrementBusyCount()
        {
            HostingEnvironment.DecrementBusyCount();
        }

        [SecuritySafeCritical]
        public static VirtualFile GetServiceFile(string normalizedVirtualPath)
        {
            IDisposable disposable = null;
            VirtualFile file;
            try
            {
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        disposable = UnsafeImpersonate();
                    }
                    file = HostingEnvironment.VirtualPathProvider.GetFile(normalizedVirtualPath);
                }
                finally
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }
            return file;
        }

        public static IDisposable Impersonate()
        {
            return HostingEnvironment.Impersonate();
        }

        public static IDisposable Impersonate(IntPtr token)
        {
            return HostingEnvironment.Impersonate(token);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static void IncrementBusyCount()
        {
            HostingEnvironment.IncrementBusyCount();
        }

        public static string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }

        [SecuritySafeCritical]
        public static bool ServiceFileExists(string normalizedVirtualPath)
        {
            IDisposable disposable = null;
            bool flag;
            try
            {
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        disposable = UnsafeImpersonate();
                    }
                    flag = HostingEnvironment.VirtualPathProvider.FileExists(normalizedVirtualPath);
                }
                finally
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }
            return flag;
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, ControlPrincipal=true)]
        public static IDisposable UnsafeImpersonate()
        {
            return HostingEnvironment.Impersonate();
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        public static IDisposable UnsafeImpersonate(IntPtr token)
        {
            return HostingEnvironment.Impersonate(token);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        public static void UnsafeRegisterObject(IRegisteredObject target)
        {
            HostingEnvironment.RegisterObject(target);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        public static void UnsafeUnregisterObject(IRegisteredObject target)
        {
            HostingEnvironment.UnregisterObject(target);
        }

        public static string ApplicationVirtualPath
        {
            get
            {
                return HostingEnvironment.ApplicationVirtualPath;
            }
        }

        public static bool IsHosted
        {
            get
            {
                return HostingEnvironment.IsHosted;
            }
        }

        public static string UnsafeApplicationID
        {
            [SecurityCritical, AspNetHostingPermission(SecurityAction.Assert, Level=AspNetHostingPermissionLevel.High)]
            get
            {
                return HostingEnvironment.ApplicationID;
            }
        }

        public static System.Web.Hosting.VirtualPathProvider VirtualPathProvider
        {
            [SecuritySafeCritical]
            get
            {
                return HostingEnvironment.VirtualPathProvider;
            }
        }
    }
}

