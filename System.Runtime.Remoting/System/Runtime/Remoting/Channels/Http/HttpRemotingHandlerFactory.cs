namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Web;
    using System.Web.UI;

    public class HttpRemotingHandlerFactory : IHttpHandlerFactory
    {
        internal object _webServicesFactory;
        internal static object s_configLock = new object();
        internal static Hashtable s_registeredDynamicTypeTable = Hashtable.Synchronized(new Hashtable());
        internal static Type s_webServicesFactoryType = null;

        private void ConfigureAppName(HttpRequest httpRequest)
        {
            if (RemotingConfiguration.ApplicationName == null)
            {
                lock (s_configLock)
                {
                    if (RemotingConfiguration.ApplicationName == null)
                    {
                        RemotingConfiguration.ApplicationName = httpRequest.ApplicationPath;
                    }
                }
            }
        }

        private void DumpRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
        }

        public IHttpHandler GetHandler(HttpContext context, string verb, string url, string filePath)
        {
            this.DumpRequest(context);
            HttpRequest httpRequest = context.Request;
            this.ConfigureAppName(httpRequest);
            string str = httpRequest.QueryString[null];
            bool flag = string.Compare(httpRequest.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) == 0;
            bool flag2 = System.IO.File.Exists(httpRequest.PhysicalPath);
            if ((flag && flag2) && (str == null))
            {
                return this.WebServicesFactory.GetHandler(context, verb, url, filePath);
            }
            if (flag2)
            {
                Type compiledType = WebServiceParser.GetCompiledType(url, context);
                string machineAndAppName = Dns.GetHostName() + httpRequest.ApplicationPath;
                string[] strArray = httpRequest.PhysicalPath.Split(new char[] { '\\' });
                string uri = strArray[strArray.Length - 1];
                Type type2 = (Type) s_registeredDynamicTypeTable[uri];
                if (type2 != compiledType)
                {
                    RegistrationHelper.RegisterType(machineAndAppName, compiledType, uri);
                    s_registeredDynamicTypeTable[uri] = compiledType;
                }
            }
            return new HttpRemotingHandler();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            if (this._webServicesFactory != null)
            {
                ((IHttpHandlerFactory) this._webServicesFactory).ReleaseHandler(handler);
                this._webServicesFactory = null;
            }
        }

        private IHttpHandlerFactory WebServicesFactory
        {
            get
            {
                if (this._webServicesFactory == null)
                {
                    lock (this)
                    {
                        if (this._webServicesFactory == null)
                        {
                            this._webServicesFactory = Activator.CreateInstance(WebServicesFactoryType);
                        }
                    }
                }
                return (IHttpHandlerFactory) this._webServicesFactory;
            }
        }

        private static Type WebServicesFactoryType
        {
            get
            {
                if (s_webServicesFactoryType == null)
                {
                    Assembly assembly = Assembly.Load("System.Web.Services, Version=4.0.0.0, Culture=neutral, PublicKeyToken= b03f5f7f11d50a3a");
                    if (assembly == null)
                    {
                        throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_AssemblyLoadFailed"), new object[] { "System.Web.Services" }));
                    }
                    s_webServicesFactoryType = assembly.GetType("System.Web.Services.Protocols.WebServiceHandlerFactory");
                }
                return s_webServicesFactoryType;
            }
        }
    }
}

