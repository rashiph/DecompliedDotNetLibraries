namespace System.Web.Handlers
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class AssemblyResourceLoader : IHttpHandler
    {
        internal static string _applicationRootPath;
        private static IDictionary _assemblyInfoCache = Hashtable.Synchronized(new Hashtable());
        private static bool _handlerExistenceChecked;
        private static bool _handlerExists;
        private static bool _smartNavPageChecked;
        private static VirtualPath _smartNavPageLocation;
        private static bool _smartNavScriptChecked;
        private static VirtualPath _smartNavScriptLocation;
        private static IDictionary _typeAssemblyCache = Hashtable.Synchronized(new Hashtable());
        private static IDictionary _urlCache = Hashtable.Synchronized(new Hashtable());
        private static bool _webFormsScriptChecked;
        private static VirtualPath _webFormsScriptLocation;
        private static IDictionary _webResourceCache = Hashtable.Synchronized(new Hashtable());
        private const string _webResourceUrl = "WebResource.axd";
        private static bool _webUIValidationScriptChecked;
        private static VirtualPath _webUIValidationScriptLocation;
        private static readonly Regex webResourceRegex = new WebResourceRegex();

        private static int CreateWebResourceUrlCacheKey(Assembly assembly, string resourceName, bool htmlEncoded, bool forSubstitution, bool enableCdn, bool debuggingEnabled, bool secureConnection)
        {
            return HashCodeCombiner.CombineHashCodes(HashCodeCombiner.CombineHashCodes(assembly.GetHashCode(), resourceName.GetHashCode(), htmlEncoded.GetHashCode(), forSubstitution.GetHashCode(), enableCdn.GetHashCode()), debuggingEnabled.GetHashCode(), secureConnection.GetHashCode());
        }

        private static void EnsureHandlerExistenceChecked()
        {
            if (!_handlerExistenceChecked)
            {
                HttpContext current = HttpContext.Current;
                IIS7WorkerRequest request = (current != null) ? (current.WorkerRequest as IIS7WorkerRequest) : null;
                string path = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, "WebResource.axd");
                if (request != null)
                {
                    string str2 = request.MapHandlerAndGetHandlerTypeString("GET", path, false);
                    if (!string.IsNullOrEmpty(str2))
                    {
                        _handlerExists = typeof(AssemblyResourceLoader) == BuildManager.GetType(str2, true, false);
                    }
                }
                else
                {
                    HttpHandlerAction action = RuntimeConfig.GetConfig(VirtualPath.Create(path)).HttpHandlers.FindMapping("GET", VirtualPath.Create("WebResource.axd"));
                    _handlerExists = (action != null) && (action.TypeInternal == typeof(AssemblyResourceLoader));
                }
                _handlerExistenceChecked = true;
            }
        }

        private static WebResourceAttribute FindWebResourceAttribute(Assembly assembly, string resourceName)
        {
            object[] customAttributes = assembly.GetCustomAttributes(false);
            for (int i = 0; i < customAttributes.Length; i++)
            {
                WebResourceAttribute attribute = customAttributes[i] as WebResourceAttribute;
                if ((attribute != null) && string.Equals(attribute.WebResource, resourceName, StringComparison.Ordinal))
                {
                    return attribute;
                }
            }
            return null;
        }

        internal static string FormatCdnUrl(Assembly assembly, string cdnPath)
        {
            AssemblyName name = new AssemblyName(assembly.FullName);
            return string.Format(CultureInfo.InvariantCulture, cdnPath, new object[] { HttpUtility.UrlEncode(name.Name), HttpUtility.UrlEncode(name.Version.ToString(4)), HttpUtility.UrlEncode(AssemblyUtil.GetAssemblyFileVersion(assembly)) });
        }

        private static string FormatWebResourceUrl(string assemblyName, string resourceName, long assemblyDate, bool htmlEncoded)
        {
            string str = Page.EncryptString(assemblyName + "|" + resourceName);
            if (htmlEncoded)
            {
                return string.Format(CultureInfo.InvariantCulture, "WebResource.axd?d={0}&amp;t={1}", new object[] { str, assemblyDate });
            }
            return string.Format(CultureInfo.InvariantCulture, "WebResource.axd?d={0}&t={1}", new object[] { str, assemblyDate });
        }

        internal static Assembly GetAssemblyFromType(Type type)
        {
            Assembly assembly = (Assembly) _typeAssemblyCache[type];
            if (assembly == null)
            {
                assembly = type.Assembly;
                _typeAssemblyCache[type] = assembly;
            }
            return assembly;
        }

        private static Pair GetAssemblyInfo(Assembly assembly)
        {
            Pair assemblyInfoWithAssertInternal = _assemblyInfoCache[assembly] as Pair;
            if (assemblyInfoWithAssertInternal == null)
            {
                assemblyInfoWithAssertInternal = GetAssemblyInfoWithAssertInternal(assembly);
                _assemblyInfoCache[assembly] = assemblyInfoWithAssertInternal;
            }
            return assemblyInfoWithAssertInternal;
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private static Pair GetAssemblyInfoWithAssertInternal(Assembly assembly)
        {
            AssemblyName x = assembly.GetName();
            return new Pair(x, File.GetLastWriteTime(new Uri(x.CodeBase).LocalPath).Ticks);
        }

        private static string GetCdnPath(string resourceName, Assembly assembly, bool secureConnection)
        {
            string str = null;
            WebResourceAttribute attribute = FindWebResourceAttribute(assembly, resourceName);
            if (attribute != null)
            {
                str = secureConnection ? attribute.CdnPathSecureConnection : attribute.CdnPath;
                if (!string.IsNullOrEmpty(str))
                {
                    str = FormatCdnUrl(assembly, str);
                }
            }
            return str;
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private static VirtualPath GetDiskResourcePath(string resourceName)
        {
            VirtualPath path2 = Util.GetScriptLocation().SimpleCombine(resourceName);
            if (File.Exists(path2.MapPath()))
            {
                return path2;
            }
            return null;
        }

        internal static string GetWebResourceUrl(Type type, string resourceName)
        {
            return GetWebResourceUrl(type, resourceName, false, null);
        }

        internal static string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded)
        {
            return GetWebResourceUrl(type, resourceName, htmlEncoded, null);
        }

        internal static string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded, IScriptManager scriptManager)
        {
            Assembly assemblyFromType = GetAssemblyFromType(type);
            if (assemblyFromType == typeof(AssemblyResourceLoader).Assembly)
            {
                if (string.Equals(resourceName, "WebForms.js", StringComparison.Ordinal))
                {
                    if (!_webFormsScriptChecked)
                    {
                        _webFormsScriptLocation = GetDiskResourcePath(resourceName);
                        _webFormsScriptChecked = true;
                    }
                    if (_webFormsScriptLocation != null)
                    {
                        return _webFormsScriptLocation.VirtualPathString;
                    }
                }
                else if (string.Equals(resourceName, "WebUIValidation.js", StringComparison.Ordinal))
                {
                    if (!_webUIValidationScriptChecked)
                    {
                        _webUIValidationScriptLocation = GetDiskResourcePath(resourceName);
                        _webUIValidationScriptChecked = true;
                    }
                    if (_webUIValidationScriptLocation != null)
                    {
                        return _webUIValidationScriptLocation.VirtualPathString;
                    }
                }
                else if (string.Equals(resourceName, "SmartNav.htm", StringComparison.Ordinal))
                {
                    if (!_smartNavPageChecked)
                    {
                        _smartNavPageLocation = GetDiskResourcePath(resourceName);
                        _smartNavPageChecked = true;
                    }
                    if (_smartNavPageLocation != null)
                    {
                        return _smartNavPageLocation.VirtualPathString;
                    }
                }
                else if (string.Equals(resourceName, "SmartNav.js", StringComparison.Ordinal))
                {
                    if (!_smartNavScriptChecked)
                    {
                        _smartNavScriptLocation = GetDiskResourcePath(resourceName);
                        _smartNavScriptChecked = true;
                    }
                    if (_smartNavScriptLocation != null)
                    {
                        return _smartNavScriptLocation.VirtualPathString;
                    }
                }
            }
            return GetWebResourceUrlInternal(assemblyFromType, resourceName, htmlEncoded, false, scriptManager);
        }

        internal static string GetWebResourceUrlInternal(Assembly assembly, string resourceName, bool htmlEncoded, bool forSubstitution, IScriptManager scriptManager)
        {
            bool isSecureConnection;
            EnsureHandlerExistenceChecked();
            if (!_handlerExists)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("AssemblyResourceLoader_HandlerNotRegistered"));
            }
            Assembly resourceAssembly = assembly;
            string str = resourceName;
            bool enableCdn = false;
            bool debuggingEnabled = false;
            if (scriptManager != null)
            {
                enableCdn = scriptManager.EnableCdn;
                debuggingEnabled = scriptManager.IsDebuggingEnabled;
                isSecureConnection = scriptManager.IsSecureConnection;
            }
            else
            {
                isSecureConnection = ((HttpContext.Current != null) && (HttpContext.Current.Request != null)) && HttpContext.Current.Request.IsSecureConnection;
            }
            int num = CreateWebResourceUrlCacheKey(assembly, resourceName, htmlEncoded, forSubstitution, enableCdn, debuggingEnabled, isSecureConnection);
            string s = (string) _urlCache[num];
            if (s == null)
            {
                IScriptResourceDefinition definition = null;
                if (ClientScriptManager._scriptResourceMapping != null)
                {
                    definition = ClientScriptManager._scriptResourceMapping.GetDefinition(resourceName, assembly);
                    if (definition != null)
                    {
                        if (!string.IsNullOrEmpty(definition.ResourceName))
                        {
                            str = definition.ResourceName;
                        }
                        if (definition.ResourceAssembly != null)
                        {
                            resourceAssembly = definition.ResourceAssembly;
                        }
                    }
                }
                string debugPath = null;
                if (definition != null)
                {
                    if (enableCdn)
                    {
                        if (debuggingEnabled)
                        {
                            debugPath = isSecureConnection ? definition.CdnDebugPathSecureConnection : definition.CdnDebugPath;
                            if (string.IsNullOrEmpty(debugPath))
                            {
                                debugPath = definition.DebugPath;
                                if (string.IsNullOrEmpty(debugPath))
                                {
                                    if (!isSecureConnection || string.IsNullOrEmpty(definition.CdnDebugPath))
                                    {
                                        debugPath = GetCdnPath(str, resourceAssembly, isSecureConnection);
                                    }
                                    if (string.IsNullOrEmpty(debugPath))
                                    {
                                        debugPath = definition.Path;
                                    }
                                }
                            }
                        }
                        else
                        {
                            debugPath = isSecureConnection ? definition.CdnPathSecureConnection : definition.CdnPath;
                            if (string.IsNullOrEmpty(debugPath))
                            {
                                if (!isSecureConnection || string.IsNullOrEmpty(definition.CdnPath))
                                {
                                    debugPath = GetCdnPath(str, resourceAssembly, isSecureConnection);
                                }
                                if (string.IsNullOrEmpty(debugPath))
                                {
                                    debugPath = definition.Path;
                                }
                            }
                        }
                    }
                    else if (debuggingEnabled)
                    {
                        debugPath = definition.DebugPath;
                        if (string.IsNullOrEmpty(debugPath))
                        {
                            debugPath = definition.Path;
                        }
                    }
                    else
                    {
                        debugPath = definition.Path;
                    }
                }
                else if (enableCdn)
                {
                    debugPath = GetCdnPath(str, resourceAssembly, isSecureConnection);
                }
                if (!string.IsNullOrEmpty(debugPath))
                {
                    if (UrlPath.IsAppRelativePath(debugPath))
                    {
                        if (_applicationRootPath == null)
                        {
                            s = VirtualPathUtility.ToAbsolute(debugPath);
                        }
                        else
                        {
                            s = VirtualPathUtility.ToAbsolute(debugPath, _applicationRootPath);
                        }
                    }
                    else
                    {
                        s = debugPath;
                    }
                    if (htmlEncoded)
                    {
                        s = HttpUtility.HtmlEncode(s);
                    }
                }
                else
                {
                    string str4;
                    Pair assemblyInfo = GetAssemblyInfo(resourceAssembly);
                    AssemblyName first = (AssemblyName) assemblyInfo.First;
                    long second = (long) assemblyInfo.Second;
                    string str5 = first.Version.ToString();
                    if (resourceAssembly.GlobalAssemblyCache)
                    {
                        if (resourceAssembly == HttpContext.SystemWebAssembly)
                        {
                            str4 = "s";
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append('f');
                            builder.Append(first.Name);
                            builder.Append(',');
                            builder.Append(str5);
                            builder.Append(',');
                            if (first.CultureInfo != null)
                            {
                                builder.Append(first.CultureInfo.ToString());
                            }
                            builder.Append(',');
                            byte[] publicKeyToken = first.GetPublicKeyToken();
                            for (int i = 0; i < publicKeyToken.Length; i++)
                            {
                                builder.Append(publicKeyToken[i].ToString("x2", CultureInfo.InvariantCulture));
                            }
                            str4 = builder.ToString();
                        }
                    }
                    else
                    {
                        str4 = "p" + first.Name;
                    }
                    s = FormatWebResourceUrl(str4, str, second, htmlEncoded);
                    if (!forSubstitution && (HttpRuntime.AppDomainAppVirtualPathString != null))
                    {
                        s = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, s);
                    }
                }
                _urlCache[num] = s;
            }
            return s;
        }

        internal static bool IsValidWebResourceRequest(HttpContext context)
        {
            EnsureHandlerExistenceChecked();
            if (!_handlerExists)
            {
                return false;
            }
            string b = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, "WebResource.axd");
            return string.Equals(context.Request.Path, b, StringComparison.OrdinalIgnoreCase);
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            context.Response.Clear();
            Stream manifestResourceStream = null;
            try
            {
                string str = context.Request.QueryString["d"];
                if (string.IsNullOrEmpty(str))
                {
                    throw new HttpException(0x194, System.Web.SR.GetString("AssemblyResourceLoader_InvalidRequest"));
                }
                string str2 = Page.DecryptString(str);
                int index = str2.IndexOf('|');
                string str3 = str2.Substring(0, index);
                if (string.IsNullOrEmpty(str3))
                {
                    throw new HttpException(0x194, System.Web.SR.GetString("AssemblyResourceLoader_AssemblyNotFound", new object[] { str3 }));
                }
                string webResource = str2.Substring(index + 1);
                if (string.IsNullOrEmpty(webResource))
                {
                    throw new HttpException(0x194, System.Web.SR.GetString("AssemblyResourceLoader_ResourceNotFound", new object[] { webResource }));
                }
                char ch = str3[0];
                str3 = str3.Substring(1);
                Assembly assembly = null;
                switch (ch)
                {
                    case 's':
                        assembly = typeof(AssemblyResourceLoader).Assembly;
                        break;

                    case 'p':
                        assembly = Assembly.Load(str3);
                        break;

                    case 'f':
                    {
                        string[] strArray = str3.Split(new char[] { ',' });
                        if (strArray.Length != 4)
                        {
                            throw new HttpException(0x194, System.Web.SR.GetString("AssemblyResourceLoader_InvalidRequest"));
                        }
                        AssemblyName assemblyRef = new AssemblyName {
                            Name = strArray[0],
                            Version = new Version(strArray[1])
                        };
                        string name = strArray[2];
                        if (name.Length > 0)
                        {
                            assemblyRef.CultureInfo = new CultureInfo(name);
                        }
                        else
                        {
                            assemblyRef.CultureInfo = CultureInfo.InvariantCulture;
                        }
                        string str6 = strArray[3];
                        byte[] publicKeyToken = new byte[str6.Length / 2];
                        for (int i = 0; i < publicKeyToken.Length; i++)
                        {
                            publicKeyToken[i] = byte.Parse(str6.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        assemblyRef.SetPublicKeyToken(publicKeyToken);
                        assembly = Assembly.Load(assemblyRef);
                        break;
                    }
                    default:
                        throw new HttpException(0x194, System.Web.SR.GetString("AssemblyResourceLoader_InvalidRequest"));
                }
                if (assembly == null)
                {
                    throw new HttpException(0x194, System.Web.SR.GetString("AssemblyResourceLoader_InvalidRequest"));
                }
                bool third = false;
                bool first = false;
                string second = string.Empty;
                int num3 = HashCodeCombiner.CombineHashCodes(assembly.GetHashCode(), webResource.GetHashCode());
                Triplet triplet = (Triplet) _webResourceCache[num3];
                if (triplet != null)
                {
                    first = (bool) triplet.First;
                    second = (string) triplet.Second;
                    third = (bool) triplet.Third;
                }
                else
                {
                    WebResourceAttribute attribute = FindWebResourceAttribute(assembly, webResource);
                    if (attribute != null)
                    {
                        webResource = attribute.WebResource;
                        first = true;
                        second = attribute.ContentType;
                        third = attribute.PerformSubstitution;
                    }
                    try
                    {
                        if (first)
                        {
                            first = false;
                            manifestResourceStream = assembly.GetManifestResourceStream(webResource);
                            first = manifestResourceStream != null;
                        }
                    }
                    finally
                    {
                        Triplet triplet2 = new Triplet {
                            First = first,
                            Second = second,
                            Third = third
                        };
                        _webResourceCache[num3] = triplet2;
                    }
                }
                if (first)
                {
                    HttpCachePolicy cache = context.Response.Cache;
                    cache.SetCacheability(HttpCacheability.Public);
                    cache.VaryByParams["d"] = true;
                    cache.SetOmitVaryStar(true);
                    cache.SetExpires(DateTime.Now + TimeSpan.FromDays(365.0));
                    cache.SetValidUntilExpires(true);
                    Pair assemblyInfo = GetAssemblyInfo(assembly);
                    cache.SetLastModified(new DateTime((long) assemblyInfo.Second));
                    StreamReader reader = null;
                    try
                    {
                        if (manifestResourceStream == null)
                        {
                            manifestResourceStream = assembly.GetManifestResourceStream(webResource);
                        }
                        if (manifestResourceStream != null)
                        {
                            context.Response.ContentType = second;
                            if (third)
                            {
                                reader = new StreamReader(manifestResourceStream, true);
                                string input = reader.ReadToEnd();
                                MatchCollection matchs = webResourceRegex.Matches(input);
                                int startIndex = 0;
                                StringBuilder builder = new StringBuilder();
                                foreach (Match match in matchs)
                                {
                                    builder.Append(input.Substring(startIndex, match.Index - startIndex));
                                    Group group = match.Groups["resourceName"];
                                    if (group != null)
                                    {
                                        string a = group.ToString();
                                        if (a.Length > 0)
                                        {
                                            if (string.Equals(a, webResource, StringComparison.Ordinal))
                                            {
                                                throw new HttpException(0x194, System.Web.SR.GetString("AssemblyResourceLoader_NoCircularReferences", new object[] { webResource }));
                                            }
                                            builder.Append(GetWebResourceUrlInternal(assembly, a, false, true, null));
                                        }
                                    }
                                    startIndex = match.Index + match.Length;
                                }
                                builder.Append(input.Substring(startIndex, input.Length - startIndex));
                                StreamWriter writer = new StreamWriter(context.Response.OutputStream, reader.CurrentEncoding);
                                writer.Write(builder.ToString());
                                writer.Flush();
                            }
                            else
                            {
                                byte[] buffer = new byte[0x400];
                                Stream outputStream = context.Response.OutputStream;
                                int count = 1;
                                while (count > 0)
                                {
                                    count = manifestResourceStream.Read(buffer, 0, 0x400);
                                    outputStream.Write(buffer, 0, count);
                                }
                                outputStream.Flush();
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                        if (manifestResourceStream != null)
                        {
                            manifestResourceStream.Close();
                        }
                    }
                }
            }
            catch
            {
                manifestResourceStream = null;
            }
            if (manifestResourceStream == null)
            {
                throw new HttpException(0x194, System.Web.SR.GetString("AssemblyResourceLoader_InvalidRequest"));
            }
            context.Response.IgnoreFurtherWrites();
        }

        private static bool DebugMode
        {
            get
            {
                return HttpContext.Current.IsDebuggingEnabled;
            }
        }

        bool IHttpHandler.IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}

