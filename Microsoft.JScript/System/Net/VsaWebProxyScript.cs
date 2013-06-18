namespace System.Net
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;

    internal class VsaWebProxyScript : MarshalByRefObject, IWebProxyScript
    {
        private VsaEngine engine;
        private static readonly Zone IntranetZone = new Zone(SecurityZone.Intranet);
        private const string NetScriptSource_bindings = "var ProxyConfig = { bindings:{} };\r\n";
        private const string NetScriptSource_v4 = "import System.Security;\r\n[assembly:System.Security.SecurityTransparent()]\r\nfunction isPlainHostName(hostName: String): Boolean { return __om.isPlainHostName(hostName); }\r\nfunction dnsDomainIs(host: String, domain: String): Boolean { return __om.dnsDomainIs(host, domain); }\r\nfunction localHostOrDomainIs(host: String, hostdom: String): Boolean { return __om.localHostOrDomainIs(host, hostdom); }\r\nfunction isResolvable(host: String): Boolean { return __om.isResolvable(host); }\r\nfunction isInNet(host: String, pattern: String, mask: String): Boolean { return __om.isInNet(host, pattern, mask); }\r\nfunction dnsResolve(host: String): String { return __om.dnsResolve(host); }\r\nfunction myIpAddress(): String { return __om.myIpAddress(); }\r\nfunction dnsDomainLevels(host: String): int { return __om.dnsDomainLevels(host); }\r\nfunction shExpMatch(str: String, pattern: String): Boolean { return __om.shExpMatch(str, pattern); }\r\nfunction weekdayRange(wd1: String, wd2: String, gmt: String): Boolean { return __om.weekdayRange(wd1, wd2, gmt); }\r\nfunction dateRange(day1, month1, year1, day2, month2, year2, gmt): Boolean { return true; }\r\nfunction timeRange(hour1, min1, sec1, hour2, min2, sec2, gmt): Boolean { return true; }\r\n";
        private const string NetScriptSource_v4Class = "class __WebProxyScript { function ExecuteFindProxyForURL(url, host): String { return String(FindProxyForURL(url, host)); } }\r\n";
        private const string NetScriptSource_v6ExtClass = "function getClientVersion(): String { return __om.getClientVersion(); }\r\nfunction sortIpAddressList(IPAddressList:String): String { return __om.sortIpAddressList(IPAddressList); }\r\nfunction isInNetEx(ipAddress:String, ipPrefix:String): Boolean { return __om.isInNetEx(ipAddress, ipPrefix); }\r\nfunction myIpAddressEx(): String { return __om.myIpAddressEx(); }\r\nfunction dnsResolveEx(host:String): String { return __om.dnsResolveEx(host); }\r\nfunction isResolvableEx(host:String): Boolean { return __om.isResolvableEx(host); }\r\nvar __RefereceOfFindProxyForURLEx = this[\"FindProxyForURLEx\"];\r\nvar bFindProxyForURLExFound : Boolean = __RefereceOfFindProxyForURLEx != null && typeof(__RefereceOfFindProxyForURLEx) == \"function\";\r\nclass __WebProxyScript { \t\r\n\t\t\t     \tfunction ExecuteFindProxyForURL(url, host): String { \r\n\t\t         \t\tif(bFindProxyForURLExFound) {\r\n\t\t         \t\t\treturn String(FindProxyForURLEx(url, host)); \r\n\t\t         \t\t}\r\n\t\t         \t\telse {\r\n\t\t\t         \t\treturn String(FindProxyForURL(url, host)); \r\n\t\t         \t\t}\r\n\t\t\t       \t} \r\n\t\t\t       }\r\n\t\t\t         ";
        private const string RootNamespace = "__WebProxyScript";
        private object scriptInstance;
        private BaseVsaStartup startupInstance;

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        private static object CallMethod(object targetObject, string name, object[] args)
        {
            if ((targetObject == null) || (name == null))
            {
                return null;
            }
            Type type = targetObject.GetType();
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                types[i] = args[i].GetType();
            }
            return type.GetMethod(name, BindingFlags.CreateInstance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, types, null).Invoke(targetObject, args);
        }

        public void Close()
        {
            if (this.startupInstance != null)
            {
                try
                {
                    this.startupInstance.Shutdown();
                    this.startupInstance = null;
                }
                catch (Exception exception)
                {
                    throw new JSVsaException(JSVsaError.EngineCannotReset, exception.ToString(), exception);
                }
            }
            this.engine.Close();
        }

        [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
        private bool CompileScript(Uri engineScriptLocation, string scriptBody, Type helperType, out byte[] pe, out byte[] pdb)
        {
            pe = null;
            pdb = null;
            try
            {
                this.engine = new VsaEngine();
                this.engine.RootMoniker = "pac-" + engineScriptLocation.ToString();
                this.engine.Site = new VsaEngineSite(helperType);
                this.engine.InitNew();
                this.engine.RootNamespace = "__WebProxyScript";
                this.engine.SetOption("print", false);
                this.engine.SetOption("fast", false);
                this.engine.SetOption("autoref", false);
                string name = typeof(SecurityTransparentAttribute).Assembly.GetName().Name + ".dll";
                IJSVsaReferenceItem item = this.engine.Items.CreateItem(name, JSVsaItemType.Reference, JSVsaItemFlag.None) as IJSVsaReferenceItem;
                item.AssemblyName = name;
                StringBuilder builder = new StringBuilder();
                builder.Append("import System.Security;\r\n[assembly:System.Security.SecurityTransparent()]\r\nfunction isPlainHostName(hostName: String): Boolean { return __om.isPlainHostName(hostName); }\r\nfunction dnsDomainIs(host: String, domain: String): Boolean { return __om.dnsDomainIs(host, domain); }\r\nfunction localHostOrDomainIs(host: String, hostdom: String): Boolean { return __om.localHostOrDomainIs(host, hostdom); }\r\nfunction isResolvable(host: String): Boolean { return __om.isResolvable(host); }\r\nfunction isInNet(host: String, pattern: String, mask: String): Boolean { return __om.isInNet(host, pattern, mask); }\r\nfunction dnsResolve(host: String): String { return __om.dnsResolve(host); }\r\nfunction myIpAddress(): String { return __om.myIpAddress(); }\r\nfunction dnsDomainLevels(host: String): int { return __om.dnsDomainLevels(host); }\r\nfunction shExpMatch(str: String, pattern: String): Boolean { return __om.shExpMatch(str, pattern); }\r\nfunction weekdayRange(wd1: String, wd2: String, gmt: String): Boolean { return __om.weekdayRange(wd1, wd2, gmt); }\r\nfunction dateRange(day1, month1, year1, day2, month2, year2, gmt): Boolean { return true; }\r\nfunction timeRange(hour1, min1, sec1, hour2, min2, sec2, gmt): Boolean { return true; }\r\n");
                if (Socket.OSSupportsIPv6)
                {
                    builder.Append("function getClientVersion(): String { return __om.getClientVersion(); }\r\nfunction sortIpAddressList(IPAddressList:String): String { return __om.sortIpAddressList(IPAddressList); }\r\nfunction isInNetEx(ipAddress:String, ipPrefix:String): Boolean { return __om.isInNetEx(ipAddress, ipPrefix); }\r\nfunction myIpAddressEx(): String { return __om.myIpAddressEx(); }\r\nfunction dnsResolveEx(host:String): String { return __om.dnsResolveEx(host); }\r\nfunction isResolvableEx(host:String): Boolean { return __om.isResolvableEx(host); }\r\nvar __RefereceOfFindProxyForURLEx = this[\"FindProxyForURLEx\"];\r\nvar bFindProxyForURLExFound : Boolean = __RefereceOfFindProxyForURLEx != null && typeof(__RefereceOfFindProxyForURLEx) == \"function\";\r\nclass __WebProxyScript { \t\r\n\t\t\t     \tfunction ExecuteFindProxyForURL(url, host): String { \r\n\t\t         \t\tif(bFindProxyForURLExFound) {\r\n\t\t         \t\t\treturn String(FindProxyForURLEx(url, host)); \r\n\t\t         \t\t}\r\n\t\t         \t\telse {\r\n\t\t\t         \t\treturn String(FindProxyForURL(url, host)); \r\n\t\t         \t\t}\r\n\t\t\t       \t} \r\n\t\t\t       }\r\n\t\t\t         ");
                }
                else
                {
                    builder.Append("class __WebProxyScript { function ExecuteFindProxyForURL(url, host): String { return String(FindProxyForURL(url, host)); } }\r\n");
                }
                builder.Append("var ProxyConfig = { bindings:{} };\r\n");
                builder.Append("//@position(file=\"" + engineScriptLocation.ToString() + "\",line = 1)\n");
                builder.Append(scriptBody);
                IJSVsaCodeItem item2 = this.engine.Items.CreateItem("SourceText", JSVsaItemType.Code, JSVsaItemFlag.None) as IJSVsaCodeItem;
                item2.SourceText = builder.ToString();
                this.engine.Items.CreateItem("__om", JSVsaItemType.AppGlobal, JSVsaItemFlag.None);
                if (this.engine.Compile())
                {
                    this.engine.SaveCompiledState(out pe, out pdb);
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public bool Load(Uri engineScriptLocation, string scriptBody, Type helperType)
        {
            byte[] buffer;
            byte[] buffer2;
            if (!this.CompileScript(engineScriptLocation, scriptBody, helperType, out buffer, out buffer2))
            {
                return false;
            }
            return this.LoadAssembly(buffer, buffer2);
        }

        private bool LoadAssembly(byte[] pe, byte[] pdb)
        {
            try
            {
                Assembly assembly = Assembly.Load(pe, pdb, SecurityContextSource.CurrentAppDomain);
                Type type = assembly.GetType(this.engine.RootNamespace + "._Startup");
                this.startupInstance = (BaseVsaStartup) Activator.CreateInstance(type);
                this.startupInstance.SetSite(this.engine.Site);
                this.startupInstance.Startup();
                Type type2 = assembly.GetType(this.engine.RootNamespace + ".__WebProxyScript");
                this.scriptInstance = Activator.CreateInstance(type2);
                CallMethod(this.scriptInstance, "SetEngine", new object[] { this.engine });
            }
            catch
            {
                return false;
            }
            return true;
        }

        public string Run(string url, string host)
        {
            return (CallMethod(this.scriptInstance, "ExecuteFindProxyForURL", new object[] { url, host }) as string);
        }

        private class VsaEngineSite : IJSVsaSite
        {
            private readonly Type m_GlobalType;

            internal VsaEngineSite(Type globalType)
            {
                this.m_GlobalType = globalType;
            }

            public void GetCompiledState(out byte[] pe, out byte[] debugInfo)
            {
                pe = null;
                debugInfo = null;
            }

            public object GetEventSourceInstance(string itemName, string eventSourceName)
            {
                return null;
            }

            [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
            public object GetGlobalInstance(string name)
            {
                if (name != "__om")
                {
                    throw new JSVsaException(JSVsaError.GlobalInstanceInvalid);
                }
                return Activator.CreateInstance(this.m_GlobalType, true);
            }

            public void Notify(string notify, object info)
            {
            }

            public bool OnCompilerError(IJSVsaError error)
            {
                return (error.Severity != 0);
            }
        }
    }
}

