namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;

    [Serializable]
    public class WebProxy : IAutoWebProxy, IWebProxy, ISerializable
    {
        private ArrayList _BypassList;
        private bool _BypassOnLocal;
        private ICredentials _Credentials;
        private Uri _ProxyAddress;
        private Hashtable _ProxyHostAddresses;
        private Regex[] _RegExBypassList;
        private bool _UseRegistry;
        private bool m_EnableAutoproxy;
        private AutoWebProxyScriptEngine m_ScriptEngine;

        public WebProxy() : this((Uri) null, false, null, null)
        {
        }

        internal WebProxy(bool enableAutoproxy)
        {
            this.m_EnableAutoproxy = enableAutoproxy;
            this.UnsafeUpdateFromRegistry();
        }

        public WebProxy(string Address) : this(CreateProxyUri(Address), false, null, null)
        {
        }

        public WebProxy(Uri Address) : this(Address, false, null, null)
        {
        }

        protected WebProxy(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            bool boolean = false;
            try
            {
                boolean = serializationInfo.GetBoolean("_UseRegistry");
            }
            catch
            {
            }
            if (boolean)
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                this.UnsafeUpdateFromRegistry();
            }
            else
            {
                this._ProxyAddress = (Uri) serializationInfo.GetValue("_ProxyAddress", typeof(Uri));
                this._BypassOnLocal = serializationInfo.GetBoolean("_BypassOnLocal");
                this._BypassList = (ArrayList) serializationInfo.GetValue("_BypassList", typeof(ArrayList));
                try
                {
                    this.UseDefaultCredentials = serializationInfo.GetBoolean("_UseDefaultCredentials");
                }
                catch
                {
                }
            }
        }

        public WebProxy(string Address, bool BypassOnLocal) : this(CreateProxyUri(Address), BypassOnLocal, null, null)
        {
        }

        public WebProxy(string Host, int Port) : this(new Uri("http://" + Host + ":" + Port.ToString(CultureInfo.InvariantCulture)), false, null, null)
        {
        }

        public WebProxy(Uri Address, bool BypassOnLocal) : this(Address, BypassOnLocal, null, null)
        {
        }

        public WebProxy(string Address, bool BypassOnLocal, string[] BypassList) : this(CreateProxyUri(Address), BypassOnLocal, BypassList, null)
        {
        }

        public WebProxy(Uri Address, bool BypassOnLocal, string[] BypassList) : this(Address, BypassOnLocal, BypassList, null)
        {
        }

        public WebProxy(string Address, bool BypassOnLocal, string[] BypassList, ICredentials Credentials) : this(CreateProxyUri(Address), BypassOnLocal, BypassList, Credentials)
        {
        }

        public WebProxy(Uri Address, bool BypassOnLocal, string[] BypassList, ICredentials Credentials)
        {
            this._ProxyAddress = Address;
            this._BypassOnLocal = BypassOnLocal;
            if (BypassList != null)
            {
                this._BypassList = new ArrayList(BypassList);
                this.UpdateRegExList(true);
            }
            this._Credentials = Credentials;
            this.m_EnableAutoproxy = true;
        }

        internal void AbortGetProxiesAuto(ref int syncStatus)
        {
            if (this.ScriptEngine != null)
            {
                this.ScriptEngine.Abort(ref syncStatus);
            }
        }

        private static bool AreAllBypassed(IEnumerable<string> proxies, bool checkFirstOnly)
        {
            bool flag = true;
            foreach (string str in proxies)
            {
                flag = string.IsNullOrEmpty(str);
                if (checkFirstOnly || !flag)
                {
                    return flag;
                }
            }
            return flag;
        }

        internal void CheckForChanges()
        {
            if (this.ScriptEngine != null)
            {
                this.ScriptEngine.CheckForChanges();
            }
        }

        private static Uri CreateProxyUri(string address)
        {
            if (address == null)
            {
                return null;
            }
            if (address.IndexOf("://") == -1)
            {
                address = "http://" + address;
            }
            return new Uri(address);
        }

        internal void DeleteScriptEngine()
        {
            if (this.ScriptEngine != null)
            {
                this.ScriptEngine.Close();
                this.ScriptEngine = null;
            }
        }

        [Obsolete("This method has been deprecated. Please use the proxy selected for you by default. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static WebProxy GetDefaultProxy()
        {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            return new WebProxy(true);
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            serializationInfo.AddValue("_BypassOnLocal", this._BypassOnLocal);
            serializationInfo.AddValue("_ProxyAddress", this._ProxyAddress);
            serializationInfo.AddValue("_BypassList", this._BypassList);
            serializationInfo.AddValue("_UseDefaultCredentials", this.UseDefaultCredentials);
            if (this._UseRegistry)
            {
                serializationInfo.AddValue("_UseRegistry", true);
            }
        }

        internal Uri[] GetProxiesAuto(Uri destination, ref int syncStatus)
        {
            if (this.ScriptEngine == null)
            {
                return null;
            }
            IList<string> proxyList = null;
            if (!this.ScriptEngine.GetProxies(destination, out proxyList, ref syncStatus))
            {
                return null;
            }
            Uri[] uriArray = null;
            if (proxyList.Count == 0)
            {
                return new Uri[0];
            }
            if (AreAllBypassed(proxyList, false))
            {
                return new Uri[1];
            }
            uriArray = new Uri[proxyList.Count];
            for (int i = 0; i < proxyList.Count; i++)
            {
                uriArray[i] = ProxyUri(proxyList[i]);
            }
            return uriArray;
        }

        public Uri GetProxy(Uri destination)
        {
            Uri uri;
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (this.GetProxyAuto(destination, out uri))
            {
                return uri;
            }
            if (this.IsBypassedManual(destination))
            {
                return destination;
            }
            Hashtable hashtable = this._ProxyHostAddresses;
            Uri uri2 = (hashtable != null) ? (hashtable[destination.Scheme] as Uri) : this._ProxyAddress;
            if (uri2 == null)
            {
                return destination;
            }
            return uri2;
        }

        private bool GetProxyAuto(Uri destination, out Uri proxyUri)
        {
            proxyUri = null;
            if (this.ScriptEngine == null)
            {
                return false;
            }
            IList<string> proxyList = null;
            if (!this.ScriptEngine.GetProxies(destination, out proxyList))
            {
                return false;
            }
            if (proxyList.Count > 0)
            {
                if (AreAllBypassed(proxyList, true))
                {
                    proxyUri = destination;
                }
                else
                {
                    proxyUri = ProxyUri(proxyList[0]);
                }
            }
            return true;
        }

        internal Uri GetProxyAutoFailover(Uri destination)
        {
            if (this.IsBypassedManual(destination))
            {
                return null;
            }
            Uri uri = this._ProxyAddress;
            Hashtable hashtable = this._ProxyHostAddresses;
            if (hashtable != null)
            {
                uri = hashtable[destination.Scheme] as Uri;
            }
            return uri;
        }

        public bool IsBypassed(Uri host)
        {
            bool flag;
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (this.IsBypassedAuto(host, out flag))
            {
                return flag;
            }
            return this.IsBypassedManual(host);
        }

        private bool IsBypassedAuto(Uri destination, out bool isBypassed)
        {
            IList<string> list;
            isBypassed = true;
            if (this.ScriptEngine == null)
            {
                return false;
            }
            if (!this.ScriptEngine.GetProxies(destination, out list))
            {
                return false;
            }
            if (list.Count == 0)
            {
                isBypassed = false;
            }
            else
            {
                isBypassed = AreAllBypassed(list, true);
            }
            return true;
        }

        private bool IsBypassedManual(Uri host)
        {
            if (!host.IsLoopback && ((((this._ProxyAddress != null) || (this._ProxyHostAddresses != null)) && (!this._BypassOnLocal || !this.IsLocal(host))) && !this.IsMatchInBypassList(host)))
            {
                return this.IsLocalInProxyHash(host);
            }
            return true;
        }

        private bool IsLocal(Uri host)
        {
            string ipString = host.Host;
            int indexB = -1;
            bool flag = true;
            bool flag2 = false;
            for (int i = 0; i < ipString.Length; i++)
            {
                if (ipString[i] == '.')
                {
                    if (indexB != -1)
                    {
                        continue;
                    }
                    indexB = i;
                    if (flag)
                    {
                        continue;
                    }
                    break;
                }
                if (ipString[i] == ':')
                {
                    flag2 = true;
                    flag = false;
                    break;
                }
                if ((ipString[i] < '0') || (ipString[i] > '9'))
                {
                    flag = false;
                    if (indexB != -1)
                    {
                        break;
                    }
                }
            }
            if ((indexB == -1) && !flag2)
            {
                return true;
            }
            if (flag || flag2)
            {
                try
                {
                    IPAddress address = IPAddress.Parse(ipString);
                    return (IPAddress.IsLoopback(address) || NclUtilities.IsAddressLocal(address));
                }
                catch (FormatException)
                {
                }
            }
            string strA = "." + IPGlobalProperties.InternalGetIPGlobalProperties().DomainName;
            return (((strA != null) && (strA.Length == (ipString.Length - indexB))) && (string.Compare(strA, 0, ipString, indexB, strA.Length, StringComparison.OrdinalIgnoreCase) == 0));
        }

        private bool IsLocalInProxyHash(Uri host)
        {
            Hashtable hashtable = this._ProxyHostAddresses;
            if (hashtable != null)
            {
                Uri uri = (Uri) hashtable[host.Scheme];
                if (uri == null)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsMatchInBypassList(Uri input)
        {
            this.UpdateRegExList(false);
            if (this._RegExBypassList != null)
            {
                string str = input.Scheme + "://" + input.Host + (!input.IsDefaultPort ? (":" + input.Port) : "");
                for (int i = 0; i < this._BypassList.Count; i++)
                {
                    if (this._RegExBypassList[i].IsMatch(str))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Uri ProxyUri(string proxyName)
        {
            if ((proxyName != null) && (proxyName.Length != 0))
            {
                return new Uri("http://" + proxyName);
            }
            return null;
        }

        ProxyChain IAutoWebProxy.GetProxies(Uri destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            return new ProxyScriptChain(this, destination);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        internal void UnsafeUpdateFromRegistry()
        {
            this._UseRegistry = true;
            this.ScriptEngine = new AutoWebProxyScriptEngine(this, true);
            WebProxyData webProxyData = this.ScriptEngine.GetWebProxyData();
            this.Update(webProxyData);
        }

        internal void Update(WebProxyData webProxyData)
        {
            lock (this)
            {
                this._BypassOnLocal = webProxyData.bypassOnLocal;
                this._ProxyAddress = webProxyData.proxyAddress;
                this._ProxyHostAddresses = webProxyData.proxyHostAddresses;
                this._BypassList = webProxyData.bypassList;
                this.ScriptEngine.AutomaticallyDetectSettings = this.m_EnableAutoproxy && webProxyData.automaticallyDetectSettings;
                this.ScriptEngine.AutomaticConfigurationScript = this.m_EnableAutoproxy ? webProxyData.scriptLocation : null;
            }
        }

        private void UpdateRegExList(bool canThrow)
        {
            Regex[] regexArray = null;
            ArrayList list = this._BypassList;
            try
            {
                if ((list != null) && (list.Count > 0))
                {
                    regexArray = new Regex[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        regexArray[i] = new Regex((string) list[i], RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    }
                }
            }
            catch
            {
                if (canThrow)
                {
                    throw;
                }
                this._RegExBypassList = null;
                return;
            }
            this._RegExBypassList = regexArray;
        }

        public Uri Address
        {
            get
            {
                this.CheckForChanges();
                return this._ProxyAddress;
            }
            set
            {
                this._UseRegistry = false;
                this.DeleteScriptEngine();
                this._ProxyHostAddresses = null;
                this._ProxyAddress = value;
            }
        }

        internal bool AutoDetect
        {
            set
            {
                if (this.ScriptEngine == null)
                {
                    this.ScriptEngine = new AutoWebProxyScriptEngine(this, false);
                }
                this.ScriptEngine.AutomaticallyDetectSettings = value;
            }
        }

        public ArrayList BypassArrayList
        {
            get
            {
                this.CheckForChanges();
                if (this._BypassList == null)
                {
                    this._BypassList = new ArrayList();
                }
                return this._BypassList;
            }
        }

        public string[] BypassList
        {
            get
            {
                this.CheckForChanges();
                if (this._BypassList == null)
                {
                    this._BypassList = new ArrayList();
                }
                return (string[]) this._BypassList.ToArray(typeof(string));
            }
            set
            {
                this._UseRegistry = false;
                this.DeleteScriptEngine();
                this._BypassList = new ArrayList(value);
                this.UpdateRegExList(true);
            }
        }

        public bool BypassProxyOnLocal
        {
            get
            {
                this.CheckForChanges();
                return this._BypassOnLocal;
            }
            set
            {
                this._UseRegistry = false;
                this.DeleteScriptEngine();
                this._BypassOnLocal = value;
            }
        }

        public ICredentials Credentials
        {
            get
            {
                return this._Credentials;
            }
            set
            {
                this._Credentials = value;
            }
        }

        internal AutoWebProxyScriptEngine ScriptEngine
        {
            get
            {
                return this.m_ScriptEngine;
            }
            set
            {
                this.m_ScriptEngine = value;
            }
        }

        internal Uri ScriptLocation
        {
            set
            {
                if (this.ScriptEngine == null)
                {
                    this.ScriptEngine = new AutoWebProxyScriptEngine(this, false);
                }
                this.ScriptEngine.AutomaticConfigurationScript = value;
            }
        }

        public bool UseDefaultCredentials
        {
            get
            {
                return (this.Credentials is SystemNetworkCredential);
            }
            set
            {
                this._Credentials = value ? CredentialCache.DefaultCredentials : null;
            }
        }
    }
}

