namespace System.Web.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Util;

    public sealed class FileAuthorizationModule : IHttpModule
    {
        private static bool s_Enabled;
        private static bool s_EnabledDetermined;

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public static bool CheckFileAccessForUser(string virtualPath, IntPtr token, string verb)
        {
            bool flag;
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            if (token == IntPtr.Zero)
            {
                throw new ArgumentNullException("token");
            }
            if (verb == null)
            {
                throw new ArgumentNullException("verb");
            }
            VirtualPath path = VirtualPath.Create(virtualPath);
            if (!path.IsWithinAppRoot)
            {
                throw new ArgumentException(System.Web.SR.GetString("Virtual_path_outside_application_not_supported"), "virtualPath");
            }
            if (!s_EnabledDetermined)
            {
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    s_Enabled = true;
                }
                else
                {
                    HttpModulesSection httpModules = RuntimeConfig.GetConfig().HttpModules;
                    int count = httpModules.Modules.Count;
                    for (int i = 0; i < count; i++)
                    {
                        HttpModuleAction action = httpModules.Modules[i];
                        if (Type.GetType(action.Type, false) == typeof(FileAuthorizationModule))
                        {
                            s_Enabled = true;
                            break;
                        }
                    }
                }
                s_EnabledDetermined = true;
            }
            if (!s_Enabled)
            {
                return true;
            }
            FileSecurityDescriptorWrapper fileSecurityDescriptorWrapper = GetFileSecurityDescriptorWrapper(path.MapPath(), out flag);
            int iAccess = 3;
            if (((verb == "GET") || (verb == "POST")) || ((verb == "HEAD") || (verb == "OPTIONS")))
            {
                iAccess = 1;
            }
            bool flag2 = fileSecurityDescriptorWrapper.IsAccessAllowed(token, iAccess);
            if (flag)
            {
                fileSecurityDescriptorWrapper.FreeSecurityDescriptor();
            }
            return flag2;
        }

        public void Dispose()
        {
        }

        private static FileSecurityDescriptorWrapper GetFileSecurityDescriptorWrapper(string fileName, out bool freeDescriptor)
        {
            if (CachedPathData.DoNotCacheUrlMetadata)
            {
                freeDescriptor = true;
                return new FileSecurityDescriptorWrapper(fileName);
            }
            freeDescriptor = false;
            string key = "h" + fileName;
            FileSecurityDescriptorWrapper wrapper = HttpRuntime.CacheInternal.Get(key) as FileSecurityDescriptorWrapper;
            if (wrapper == null)
            {
                wrapper = new FileSecurityDescriptorWrapper(fileName);
                string cacheDependencyPath = wrapper.GetCacheDependencyPath();
                if (cacheDependencyPath == null)
                {
                    return wrapper;
                }
                try
                {
                    CacheDependency dependencies = new CacheDependency(0, cacheDependencyPath);
                    TimeSpan urlMetadataSlidingExpiration = CachedPathData.UrlMetadataSlidingExpiration;
                    HttpRuntime.CacheInternal.UtcInsert(key, wrapper, dependencies, Cache.NoAbsoluteExpiration, urlMetadataSlidingExpiration, CacheItemPriority.Normal, new CacheItemRemovedCallback(wrapper.OnCacheItemRemoved));
                }
                catch (Exception)
                {
                    freeDescriptor = true;
                }
            }
            return wrapper;
        }

        public void Init(HttpApplication app)
        {
            app.AuthorizeRequest += new EventHandler(this.OnEnter);
        }

        private static bool IsUserAllowedToFile(HttpContext context, string fileName)
        {
            bool flag2;
            bool flag3;
            if (!IsWindowsIdentity(context))
            {
                return true;
            }
            if (fileName == null)
            {
                fileName = context.Request.PhysicalPathInternal;
            }
            bool flag = (context.User == null) || !context.User.Identity.IsAuthenticated;
            CachedPathData configurationPathData = null;
            int iAccess = 3;
            HttpVerb httpVerb = context.Request.HttpVerb;
            if (((httpVerb == HttpVerb.GET) || (httpVerb == HttpVerb.POST)) || ((httpVerb == HttpVerb.HEAD) || (context.Request.HttpMethod == "OPTIONS")))
            {
                iAccess = 1;
                if (!CachedPathData.DoNotCacheUrlMetadata)
                {
                    configurationPathData = context.GetConfigurationPathData();
                    if (!StringUtil.EqualsIgnoreCase(fileName, configurationPathData.PhysicalPath))
                    {
                        configurationPathData = null;
                    }
                    else
                    {
                        if (configurationPathData.AnonymousAccessAllowed)
                        {
                            return true;
                        }
                        if (configurationPathData.AnonymousAccessChecked && flag)
                        {
                            return configurationPathData.AnonymousAccessAllowed;
                        }
                    }
                }
            }
            FileSecurityDescriptorWrapper fileSecurityDescriptorWrapper = GetFileSecurityDescriptorWrapper(fileName, out flag2);
            if (iAccess == 1)
            {
                if (fileSecurityDescriptorWrapper._AnonymousAccessChecked && flag)
                {
                    flag3 = fileSecurityDescriptorWrapper._AnonymousAccess;
                }
                else
                {
                    flag3 = fileSecurityDescriptorWrapper.IsAccessAllowed(context.WorkerRequest.GetUserToken(), iAccess);
                }
                if (!fileSecurityDescriptorWrapper._AnonymousAccessChecked && flag)
                {
                    fileSecurityDescriptorWrapper._AnonymousAccess = flag3;
                    fileSecurityDescriptorWrapper._AnonymousAccessChecked = true;
                }
                if (((configurationPathData != null) && configurationPathData.Exists) && fileSecurityDescriptorWrapper._AnonymousAccessChecked)
                {
                    configurationPathData.AnonymousAccessAllowed = fileSecurityDescriptorWrapper._AnonymousAccess;
                    configurationPathData.AnonymousAccessChecked = true;
                }
            }
            else
            {
                flag3 = fileSecurityDescriptorWrapper.IsAccessAllowed(context.WorkerRequest.GetUserToken(), iAccess);
            }
            if (flag2)
            {
                fileSecurityDescriptorWrapper.FreeSecurityDescriptor();
            }
            if (flag3)
            {
                WebBaseEvent.RaiseSystemEvent(null, 0xfa4);
                return flag3;
            }
            if (!flag)
            {
                WebBaseEvent.RaiseSystemEvent(null, 0xfa8);
            }
            return flag3;
        }

        internal static bool IsUserAllowedToPath(HttpContext context, VirtualPath virtualPath)
        {
            return IsUserAllowedToFile(context, virtualPath.MapPath());
        }

        internal static bool IsWindowsIdentity(HttpContext context)
        {
            return (((context.User != null) && (context.User.Identity != null)) && (context.User.Identity is WindowsIdentity));
        }

        private void OnEnter(object source, EventArgs eventArgs)
        {
            if (!HttpRuntime.IsOnUNCShareInternal)
            {
                HttpApplication application = (HttpApplication) source;
                HttpContext context = application.Context;
                if (!IsUserAllowedToFile(context, null))
                {
                    context.Response.StatusCode = 0x191;
                    this.WriteErrorMessage(context);
                    application.CompleteRequest();
                }
            }
        }

        internal static bool RequestRequiresAuthorization(HttpContext context)
        {
            if (!IsWindowsIdentity(context))
            {
                return false;
            }
            string key = "h" + context.Request.PhysicalPathInternal;
            object obj2 = HttpRuntime.CacheInternal.Get(key);
            if ((obj2 != null) && (obj2 is FileSecurityDescriptorWrapper))
            {
                FileSecurityDescriptorWrapper wrapper = (FileSecurityDescriptorWrapper) obj2;
                if (wrapper._AnonymousAccessChecked && wrapper._AnonymousAccess)
                {
                    return false;
                }
            }
            return true;
        }

        private void WriteErrorMessage(HttpContext context)
        {
            if (!context.IsCustomErrorEnabled)
            {
                context.Response.Write(new FileAccessFailedErrorFormatter(context.Request.PhysicalPathInternal).GetErrorMessage(context, false));
            }
            else
            {
                context.Response.Write(new FileAccessFailedErrorFormatter(null).GetErrorMessage(context, true));
            }
            context.Response.GenerateResponseHeadersForHandler();
        }
    }
}

