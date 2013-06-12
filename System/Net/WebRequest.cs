namespace System.Net
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net.Cache;
    using System.Net.Configuration;
    using System.Net.Security;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    [Serializable]
    public abstract class WebRequest : MarshalByRefObject, ISerializable
    {
        internal const int DefaultTimeout = 0x186a0;
        private System.Net.Security.AuthenticationLevel m_AuthenticationLevel;
        private RequestCacheBinding m_CacheBinding;
        private RequestCachePolicy m_CachePolicy;
        private RequestCacheProtocol m_CacheProtocol;
        private TokenImpersonationLevel m_ImpersonationLevel;
        private static TimerThread.Queue s_DefaultTimerQueue = TimerThread.CreateQueue(0x186a0);
        private static IWebProxy s_DefaultWebProxy;
        private static bool s_DefaultWebProxyInitialized;
        private static object s_InternalSyncObject;
        private static ArrayList s_PrefixList;

        protected WebRequest()
        {
            this.m_ImpersonationLevel = TokenImpersonationLevel.Delegation;
            this.m_AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
        }

        protected WebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }

        public virtual void Abort()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public static WebRequest Create(string requestUriString)
        {
            if (requestUriString == null)
            {
                throw new ArgumentNullException("requestUriString");
            }
            return Create(new Uri(requestUriString), false);
        }

        public static WebRequest Create(Uri requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri");
            }
            return Create(requestUri, false);
        }

        private static WebRequest Create(Uri requestUri, bool useUriBase)
        {
            string absoluteUri;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, "WebRequest", "Create", requestUri.ToString());
            }
            WebRequestPrefixElement element = null;
            bool flag = false;
            if (!useUriBase)
            {
                absoluteUri = requestUri.AbsoluteUri;
            }
            else
            {
                absoluteUri = requestUri.Scheme + ':';
            }
            int length = absoluteUri.Length;
            ArrayList prefixList = PrefixList;
            for (int i = 0; i < prefixList.Count; i++)
            {
                element = (WebRequestPrefixElement) prefixList[i];
                if ((length >= element.Prefix.Length) && (string.Compare(element.Prefix, 0, absoluteUri, 0, element.Prefix.Length, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    flag = true;
                    break;
                }
            }
            WebRequest retObject = null;
            if (flag)
            {
                retObject = element.Creator.Create(requestUri);
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, "WebRequest", "Create", retObject);
                }
                return retObject;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, "WebRequest", "Create", (string) null);
            }
            throw new NotSupportedException(SR.GetString("net_unknown_prefix"));
        }

        public static WebRequest CreateDefault(Uri requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri");
            }
            return Create(requestUri, true);
        }

        public static HttpWebRequest CreateHttp(string requestUriString)
        {
            if (requestUriString == null)
            {
                throw new ArgumentNullException("requestUriString");
            }
            return CreateHttp(new Uri(requestUriString));
        }

        public static HttpWebRequest CreateHttp(Uri requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri");
            }
            if ((requestUri.Scheme != Uri.UriSchemeHttp) && (requestUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new NotSupportedException(SR.GetString("net_unknown_prefix"));
            }
            return (HttpWebRequest) CreateDefault(requestUri);
        }

        public virtual Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public virtual WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        internal virtual ContextAwareResult GetConnectingContext()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }

        internal virtual ContextAwareResult GetReadingContext()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public virtual Stream GetRequestStream()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public virtual WebResponse GetResponse()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public static IWebProxy GetSystemWebProxy()
        {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            return InternalGetSystemWebProxy();
        }

        internal virtual ContextAwareResult GetWritingContext()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        internal static IWebProxy InternalGetSystemWebProxy()
        {
            return new WebProxyWrapperOpaque(new WebProxy(true));
        }

        private void InternalSetCachePolicy(RequestCachePolicy policy)
        {
            if ((((this.m_CacheBinding != null) && (this.m_CacheBinding.Cache != null)) && ((this.m_CacheBinding.Validator != null) && (this.CacheProtocol == null))) && ((policy != null) && (policy.Level != RequestCacheLevel.BypassCache)))
            {
                this.CacheProtocol = new RequestCacheProtocol(this.m_CacheBinding.Cache, this.m_CacheBinding.Validator.CreateValidator());
            }
            this.m_CachePolicy = policy;
        }

        public static bool RegisterPrefix(string prefix, IWebRequestCreate creator)
        {
            bool flag = false;
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            if (creator == null)
            {
                throw new ArgumentNullException("creator");
            }
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            lock (InternalSyncObject)
            {
                Uri uri;
                ArrayList list = (ArrayList) PrefixList.Clone();
                if (Uri.TryCreate(prefix, UriKind.Absolute, out uri))
                {
                    string absoluteUri = uri.AbsoluteUri;
                    if (!prefix.EndsWith("/", StringComparison.Ordinal) && uri.GetComponents(UriComponents.Fragment | UriComponents.PathAndQuery, UriFormat.UriEscaped).Equals("/"))
                    {
                        absoluteUri = absoluteUri.Substring(0, absoluteUri.Length - 1);
                    }
                    prefix = absoluteUri;
                }
                int index = 0;
                while (index < list.Count)
                {
                    WebRequestPrefixElement element = (WebRequestPrefixElement) list[index];
                    if (prefix.Length > element.Prefix.Length)
                    {
                        break;
                    }
                    if ((prefix.Length == element.Prefix.Length) && (string.Compare(element.Prefix, prefix, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        flag = true;
                        break;
                    }
                    index++;
                }
                if (!flag)
                {
                    list.Insert(index, new WebRequestPrefixElement(prefix, creator));
                    PrefixList = list;
                }
            }
            return !flag;
        }

        internal virtual void RequestCallback(object obj)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        internal void SetupCacheProtocol(Uri uri)
        {
            this.m_CacheBinding = RequestCacheManager.GetBinding(uri.Scheme);
            this.InternalSetCachePolicy(this.m_CacheBinding.Policy);
            if (this.m_CachePolicy == null)
            {
                this.InternalSetCachePolicy(DefaultCachePolicy);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        public System.Net.Security.AuthenticationLevel AuthenticationLevel
        {
            get
            {
                return this.m_AuthenticationLevel;
            }
            set
            {
                this.m_AuthenticationLevel = value;
            }
        }

        public virtual RequestCachePolicy CachePolicy
        {
            get
            {
                return this.m_CachePolicy;
            }
            set
            {
                this.InternalSetCachePolicy(value);
            }
        }

        internal RequestCacheProtocol CacheProtocol
        {
            get
            {
                return this.m_CacheProtocol;
            }
            set
            {
                this.m_CacheProtocol = value;
            }
        }

        public virtual string ConnectionGroupName
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual long ContentLength
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual string ContentType
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual ICredentials Credentials
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public static RequestCachePolicy DefaultCachePolicy
        {
            get
            {
                return RequestCacheManager.GetBinding(string.Empty).Policy;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                RequestCacheBinding binding = RequestCacheManager.GetBinding(string.Empty);
                RequestCacheManager.SetBinding(string.Empty, new RequestCacheBinding(binding.Cache, binding.Validator, value));
            }
        }

        internal static TimerThread.Queue DefaultTimerQueue
        {
            get
            {
                return s_DefaultTimerQueue;
            }
        }

        public static IWebProxy DefaultWebProxy
        {
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return InternalDefaultWebProxy;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                InternalDefaultWebProxy = value;
            }
        }

        public virtual WebHeaderCollection Headers
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public TokenImpersonationLevel ImpersonationLevel
        {
            get
            {
                return this.m_ImpersonationLevel;
            }
            set
            {
                this.m_ImpersonationLevel = value;
            }
        }

        internal static IWebProxy InternalDefaultWebProxy
        {
            get
            {
                if (!s_DefaultWebProxyInitialized)
                {
                    lock (InternalSyncObject)
                    {
                        if (!s_DefaultWebProxyInitialized)
                        {
                            DefaultProxySectionInternal section = DefaultProxySectionInternal.GetSection();
                            if (section != null)
                            {
                                s_DefaultWebProxy = section.WebProxy;
                            }
                            s_DefaultWebProxyInitialized = true;
                        }
                    }
                }
                return s_DefaultWebProxy;
            }
            set
            {
                if (!s_DefaultWebProxyInitialized)
                {
                    lock (InternalSyncObject)
                    {
                        s_DefaultWebProxy = value;
                        s_DefaultWebProxyInitialized = true;
                        return;
                    }
                }
                s_DefaultWebProxy = value;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        public virtual string Method
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual bool PreAuthenticate
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        internal static ArrayList PrefixList
        {
            get
            {
                if (s_PrefixList == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (s_PrefixList == null)
                        {
                            s_PrefixList = WebRequestModulesSectionInternal.GetSection().WebRequestModules;
                        }
                    }
                }
                return s_PrefixList;
            }
            set
            {
                s_PrefixList = value;
            }
        }

        public virtual IWebProxy Proxy
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual Uri RequestUri
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual int Timeout
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        public virtual bool UseDefaultCredentials
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        internal class WebProxyWrapper : WebRequest.WebProxyWrapperOpaque
        {
            internal WebProxyWrapper(System.Net.WebProxy webProxy) : base(webProxy)
            {
            }

            internal System.Net.WebProxy WebProxy
            {
                get
                {
                    return base.webProxy;
                }
            }
        }

        internal class WebProxyWrapperOpaque : IAutoWebProxy, IWebProxy
        {
            protected readonly WebProxy webProxy;

            internal WebProxyWrapperOpaque(WebProxy webProxy)
            {
                this.webProxy = webProxy;
            }

            public ProxyChain GetProxies(Uri destination)
            {
                return ((IAutoWebProxy) this.webProxy).GetProxies(destination);
            }

            public Uri GetProxy(Uri destination)
            {
                return this.webProxy.GetProxy(destination);
            }

            public bool IsBypassed(Uri host)
            {
                return this.webProxy.IsBypassed(host);
            }

            public ICredentials Credentials
            {
                get
                {
                    return this.webProxy.Credentials;
                }
                set
                {
                    this.webProxy.Credentials = value;
                }
            }
        }
    }
}

