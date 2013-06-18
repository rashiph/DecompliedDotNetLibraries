namespace System.Web.Hosting
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    internal class ISAPIApplicationHost : MarshalByRefObject, IApplicationHost
    {
        private string _appId;
        private IProcessHostSupportFunctions _functions;
        private string _iisVersion;
        private string _physicalPath;
        private string _siteID;
        private string _siteName;
        private VirtualPath _virtualPath;
        private const string DEFAULT_APPID_PREFIX = "/LM/W3SVC/1/ROOT";
        private const string DEFAULT_SITEID = "1";
        private const string LMW3SVC_PREFIX = "/LM/W3SVC/";
        private const int MAX_PATH = 260;

        internal ISAPIApplicationHost(string appIdOrVirtualPath, string physicalPath, bool validatePhysicalPath) : this(appIdOrVirtualPath, physicalPath, validatePhysicalPath, null, null)
        {
        }

        internal ISAPIApplicationHost(string appIdOrVirtualPath, string physicalPath, bool validatePhysicalPath, IProcessHostSupportFunctions functions, string iisVersion = null)
        {
            this._iisVersion = iisVersion;
            this._functions = functions;
            if (this._functions == null)
            {
                ProcessHost defaultHost = ProcessHost.DefaultHost;
                if (defaultHost != null)
                {
                    this._functions = defaultHost.SupportFunctions;
                    if (this._functions != null)
                    {
                        HostingEnvironment.SupportFunctions = this._functions;
                    }
                }
            }
            IServerConfig defaultDomainInstance = ServerConfig.GetDefaultDomainInstance(this._iisVersion);
            if (StringUtil.StringStartsWithIgnoreCase(appIdOrVirtualPath, "/LM/W3SVC/"))
            {
                this._appId = appIdOrVirtualPath;
                this._virtualPath = VirtualPath.Create(ExtractVPathFromAppId(this._appId));
                this._siteID = ExtractSiteIdFromAppId(this._appId);
                this._siteName = defaultDomainInstance.GetSiteNameFromSiteID(this._siteID);
            }
            else
            {
                this._virtualPath = VirtualPath.Create(appIdOrVirtualPath);
                this._appId = GetDefaultAppIdFromVPath(this._virtualPath.VirtualPathString);
                this._siteID = "1";
                this._siteName = defaultDomainInstance.GetSiteNameFromSiteID(this._siteID);
            }
            if (physicalPath == null)
            {
                this._physicalPath = defaultDomainInstance.MapPath(this, this._virtualPath);
            }
            else
            {
                this._physicalPath = physicalPath;
            }
            if (validatePhysicalPath && !Directory.Exists(this._physicalPath))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_IIS_app", new object[] { appIdOrVirtualPath }));
            }
        }

        private static string ExtractSiteIdFromAppId(string id)
        {
            int length = "/LM/W3SVC/".Length;
            int index = id.IndexOf('/', length);
            if (index <= 0)
            {
                return "1";
            }
            return id.Substring(length, index - length);
        }

        private static string ExtractVPathFromAppId(string id)
        {
            int startIndex = 0;
            for (int i = 1; i < 5; i++)
            {
                startIndex = id.IndexOf('/', startIndex + 1);
                if (startIndex < 0)
                {
                    break;
                }
            }
            if (startIndex < 0)
            {
                return "/";
            }
            return id.Substring(startIndex);
        }

        private static string GetDefaultAppIdFromVPath(string virtualPath)
        {
            if ((virtualPath.Length == 1) && (virtualPath[0] == '/'))
            {
                return "/LM/W3SVC/1/ROOT";
            }
            return ("/LM/W3SVC/1/ROOT" + virtualPath);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal string ResolveRootWebConfigPath()
        {
            string rootWebConfigFilename = null;
            if (this._functions != null)
            {
                rootWebConfigFilename = this._functions.GetRootWebConfigFilename();
            }
            return rootWebConfigFilename;
        }

        IConfigMapPathFactory IApplicationHost.GetConfigMapPathFactory()
        {
            return new ISAPIConfigMapPathFactory();
        }

        IntPtr IApplicationHost.GetConfigToken()
        {
            string str;
            string str2;
            if (this._functions != null)
            {
                return this._functions.GetConfigToken(this._appId);
            }
            IntPtr zero = IntPtr.Zero;
            if (ServerConfig.GetDefaultDomainInstance(this._iisVersion).GetUncUser(this, this._virtualPath, out str, out str2))
            {
                try
                {
                    string str3;
                    zero = IdentitySection.CreateUserToken(str, str2, out str3);
                }
                catch
                {
                }
            }
            return zero;
        }

        string IApplicationHost.GetPhysicalPath()
        {
            return this._physicalPath;
        }

        string IApplicationHost.GetSiteID()
        {
            return this._siteID;
        }

        string IApplicationHost.GetSiteName()
        {
            return this._siteName;
        }

        string IApplicationHost.GetVirtualPath()
        {
            return this._virtualPath.VirtualPathString;
        }

        void IApplicationHost.MessageReceived()
        {
        }

        internal string AppId
        {
            get
            {
                return this._appId;
            }
        }

        internal IProcessHostSupportFunctions SupportFunctions
        {
            get
            {
                return this._functions;
            }
        }
    }
}

