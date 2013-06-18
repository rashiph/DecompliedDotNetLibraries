namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;

    internal class HttpChannelFactory : TransportChannelFactory<IRequestChannel>, IHttpTransportFactorySettings, ITransportFactorySettings, IDefaultCommunicationTimeouts
    {
        private bool allowCookies;
        private AuthenticationSchemes authenticationScheme;
        private SecurityCredentialsManager channelCredentials;
        private CookieContainer cookieContainer;
        private MruCache<Uri, Uri> credentialCacheUriPrefixCache;
        [SecurityCritical]
        private MruCache<string, string> credentialHashCache;
        private bool decompressionEnabled;
        [SecurityCritical]
        private System.Security.Cryptography.HashAlgorithm hashAlgorithm;
        private static bool httpWebRequestWebPermissionDenied = false;
        private bool keepAliveEnabled;
        private int maxBufferSize;
        private IWebProxy proxy;
        private WebProxyFactory proxyFactory;
        private static RequestCachePolicy requestCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
        private ISecurityCapabilities securityCapabilities;
        private System.IdentityModel.Selectors.SecurityTokenManager securityTokenManager;
        private System.ServiceModel.TransferMode transferMode;

        internal HttpChannelFactory(HttpTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context, HttpTransportDefaults.GetDefaultMessageEncoderFactory())
        {
            if (bindingElement.TransferMode == System.ServiceModel.TransferMode.Buffered)
            {
                if (bindingElement.MaxReceivedMessageSize > 0x7fffffffL)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("bindingElement.MaxReceivedMessageSize", System.ServiceModel.SR.GetString("MaxReceivedMessageSizeMustBeInIntegerRange")));
                }
                if (bindingElement.MaxBufferSize != bindingElement.MaxReceivedMessageSize)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement", System.ServiceModel.SR.GetString("MaxBufferSizeMustMatchMaxReceivedMessageSize"));
                }
            }
            else if (bindingElement.MaxBufferSize > bindingElement.MaxReceivedMessageSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement", System.ServiceModel.SR.GetString("MaxBufferSizeMustNotExceedMaxReceivedMessageSize"));
            }
            if (TransferModeHelper.IsRequestStreamed(bindingElement.TransferMode) && (bindingElement.AuthenticationScheme != AuthenticationSchemes.Anonymous))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement", System.ServiceModel.SR.GetString("HttpAuthDoesNotSupportRequestStreaming"));
            }
            this.allowCookies = bindingElement.AllowCookies;
            this.authenticationScheme = bindingElement.AuthenticationScheme;
            this.decompressionEnabled = bindingElement.DecompressionEnabled;
            this.keepAliveEnabled = bindingElement.KeepAliveEnabled;
            this.maxBufferSize = bindingElement.MaxBufferSize;
            this.transferMode = bindingElement.TransferMode;
            if (bindingElement.Proxy != null)
            {
                this.proxy = bindingElement.Proxy;
            }
            else if (bindingElement.ProxyAddress != null)
            {
                if (bindingElement.UseDefaultWebProxy)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UseDefaultWebProxyCantBeUsedWithExplicitProxyAddress")));
                }
                if (bindingElement.ProxyAuthenticationScheme == AuthenticationSchemes.Anonymous)
                {
                    this.proxy = new WebProxy(bindingElement.ProxyAddress, bindingElement.BypassProxyOnLocal);
                }
                else
                {
                    this.proxy = null;
                    this.proxyFactory = new WebProxyFactory(bindingElement.ProxyAddress, bindingElement.BypassProxyOnLocal, bindingElement.ProxyAuthenticationScheme);
                }
            }
            else if (!bindingElement.UseDefaultWebProxy)
            {
                this.proxy = new WebProxy();
            }
            this.channelCredentials = context.BindingParameters.Find<SecurityCredentialsManager>();
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private string AppendWindowsAuthenticationInfo(string inputString, NetworkCredential credential, AuthenticationLevel authenticationLevel, TokenImpersonationLevel impersonationLevel)
        {
            return System.ServiceModel.Security.SecurityUtils.AppendWindowsAuthenticationInfo(inputString, credential, authenticationLevel, impersonationLevel);
        }

        private void ApplyManualAddressing(ref EndpointAddress to, ref Uri via, Message message)
        {
            object obj2;
            if (base.ManualAddressing)
            {
                Uri uri = message.Headers.To;
                if (uri == null)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ManualAddressingRequiresAddressedMessages")), message);
                }
                to = new EndpointAddress(uri, new AddressHeader[0]);
                if (base.MessageVersion.Addressing == AddressingVersion.None)
                {
                    via = uri;
                }
            }
            if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out obj2))
            {
                HttpRequestMessageProperty property = (HttpRequestMessageProperty) obj2;
                if (!string.IsNullOrEmpty(property.QueryString))
                {
                    UriBuilder builder = new UriBuilder(via);
                    if (property.QueryString.StartsWith("?", StringComparison.Ordinal))
                    {
                        builder.Query = property.QueryString.Substring(1);
                    }
                    else
                    {
                        builder.Query = property.QueryString;
                    }
                    via = builder.Uri;
                }
            }
        }

        private SecurityTokenProviderContainer CreateAndOpenTokenProvider(TimeSpan timeout, AuthenticationSchemes authenticationScheme, EndpointAddress target, Uri via, ChannelParameterCollection channelParameters)
        {
            SecurityTokenProvider tokenProvider = null;
            switch (authenticationScheme)
            {
                case AuthenticationSchemes.Digest:
                    tokenProvider = TransportSecurityHelpers.GetDigestTokenProvider(this.SecurityTokenManager, target, via, this.Scheme, authenticationScheme, channelParameters);
                    break;

                case AuthenticationSchemes.Negotiate:
                case AuthenticationSchemes.Ntlm:
                    tokenProvider = TransportSecurityHelpers.GetSspiTokenProvider(this.SecurityTokenManager, target, via, this.Scheme, authenticationScheme, channelParameters);
                    break;

                case AuthenticationSchemes.Basic:
                    tokenProvider = TransportSecurityHelpers.GetUserNameTokenProvider(this.SecurityTokenManager, target, via, this.Scheme, authenticationScheme, channelParameters);
                    break;

                case AuthenticationSchemes.Anonymous:
                    break;

                default:
                    throw Fx.AssertAndThrow("CreateAndOpenTokenProvider: Invalid authentication scheme");
            }
            if (tokenProvider != null)
            {
                SecurityTokenProviderContainer container = new SecurityTokenProviderContainer(tokenProvider);
                container.Open(timeout);
                return container;
            }
            return null;
        }

        private void CreateAndOpenTokenProviders(EndpointAddress to, Uri via, ChannelParameterCollection channelParameters, TimeSpan timeout, out SecurityTokenProviderContainer tokenProvider, out SecurityTokenProviderContainer proxyTokenProvider)
        {
            if (!this.IsSecurityTokenManagerRequired())
            {
                tokenProvider = null;
                proxyTokenProvider = null;
            }
            else
            {
                this.CreateAndOpenTokenProvidersCore(to, via, channelParameters, timeout, out tokenProvider, out proxyTokenProvider);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CreateAndOpenTokenProvidersCore(EndpointAddress to, Uri via, ChannelParameterCollection channelParameters, TimeSpan timeout, out SecurityTokenProviderContainer tokenProvider, out SecurityTokenProviderContainer proxyTokenProvider)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            tokenProvider = this.CreateAndOpenTokenProvider(helper.RemainingTime(), this.AuthenticationScheme, to, via, channelParameters);
            if (this.proxyFactory != null)
            {
                proxyTokenProvider = this.CreateAndOpenTokenProvider(helper.RemainingTime(), this.proxyFactory.AuthenticationScheme, to, via, channelParameters);
            }
            else
            {
                proxyTokenProvider = null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private SecurityMessageProperty CreateMutuallyAuthenticatedReplySecurityProperty(HttpWebResponse response)
        {
            string principalName = AuthenticationManager.CustomTargetNameDictionary[response.ResponseUri.AbsoluteUri];
            if (principalName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("HttpSpnNotFound", new object[] { response.ResponseUri })));
            }
            ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = System.ServiceModel.Security.SecurityUtils.CreatePrincipalNameAuthorizationPolicies(principalName);
            return new SecurityMessageProperty { TransportToken = new SecurityTokenSpecification(null, tokenPolicies), ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies) };
        }

        internal virtual SecurityMessageProperty CreateReplySecurityProperty(HttpWebRequest request, HttpWebResponse response)
        {
            if (!response.IsMutuallyAuthenticated)
            {
                return null;
            }
            return this.CreateMutuallyAuthenticatedReplySecurityProperty(response);
        }

        internal Exception CreateToMustEqualViaException(Uri to, Uri via)
        {
            return new ArgumentException(System.ServiceModel.SR.GetString("HttpToMustEqualVia", new object[] { to, via }));
        }

        [SecuritySafeCritical]
        private string GetConnectionGroupName(HttpWebRequest httpWebRequest, NetworkCredential credential, AuthenticationLevel authenticationLevel, TokenImpersonationLevel impersonationLevel, SecurityTokenContainer clientCertificateToken)
        {
            if (this.credentialHashCache == null)
            {
                lock (base.ThisLock)
                {
                    if (this.credentialHashCache == null)
                    {
                        this.credentialHashCache = new MruCache<string, string>(5);
                    }
                }
            }
            string inputString = TransferModeHelper.IsRequestStreamed(this.TransferMode) ? "streamed" : string.Empty;
            if (AuthenticationSchemesHelper.IsWindowsAuth(this.AuthenticationScheme))
            {
                if (!httpWebRequestWebPermissionDenied)
                {
                    try
                    {
                        httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
                    }
                    catch (SecurityException exception)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        httpWebRequestWebPermissionDenied = true;
                    }
                }
                inputString = this.AppendWindowsAuthenticationInfo(inputString, credential, authenticationLevel, impersonationLevel);
            }
            inputString = this.OnGetConnectionGroupPrefix(httpWebRequest, clientCertificateToken) + inputString;
            string str3 = null;
            if (!string.IsNullOrEmpty(inputString))
            {
                lock (this.credentialHashCache)
                {
                    if (!this.credentialHashCache.TryGetValue(inputString, out str3))
                    {
                        byte[] bytes = new UTF8Encoding().GetBytes(inputString);
                        str3 = Convert.ToBase64String(this.HashAlgorithm.ComputeHash(bytes));
                        this.credentialHashCache.Add(inputString, str3);
                    }
                }
            }
            return str3;
        }

        private Uri GetCredentialCacheUriPrefix(Uri via)
        {
            Uri uri;
            if (this.credentialCacheUriPrefixCache == null)
            {
                lock (base.ThisLock)
                {
                    if (this.credentialCacheUriPrefixCache == null)
                    {
                        this.credentialCacheUriPrefixCache = new MruCache<Uri, Uri>(10);
                    }
                }
            }
            lock (this.credentialCacheUriPrefixCache)
            {
                if (!this.credentialCacheUriPrefixCache.TryGetValue(via, out uri))
                {
                    uri = new UriBuilder(via.Scheme, via.Host, via.Port).Uri;
                    this.credentialCacheUriPrefixCache.Add(via, uri);
                }
            }
            return uri;
        }

        internal override int GetMaxBufferSize()
        {
            return this.MaxBufferSize;
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) this.securityCapabilities;
            }
            return base.GetProperty<T>();
        }

        private HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, SecurityTokenProviderContainer tokenProvider, SecurityTokenProviderContainer proxyTokenProvider, SecurityTokenContainer clientCertificateToken, TimeSpan timeout)
        {
            TokenImpersonationLevel level;
            AuthenticationLevel level2;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            NetworkCredential credential = HttpChannelUtilities.GetCredential(this.authenticationScheme, tokenProvider, helper.RemainingTime(), out level, out level2);
            return this.GetWebRequest(to, via, credential, level, level2, proxyTokenProvider, clientCertificateToken, helper.RemainingTime());
        }

        private HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, NetworkCredential credential, TokenImpersonationLevel impersonationLevel, AuthenticationLevel authenticationLevel, SecurityTokenProviderContainer proxyTokenProvider, SecurityTokenContainer clientCertificateToken, TimeSpan timeout)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(via);
            httpWebRequest.Method = "POST";
            if (TransferModeHelper.IsRequestStreamed(this.TransferMode))
            {
                httpWebRequest.SendChunked = true;
                httpWebRequest.AllowWriteStreamBuffering = false;
            }
            else
            {
                httpWebRequest.AllowWriteStreamBuffering = true;
            }
            httpWebRequest.CachePolicy = requestCachePolicy;
            httpWebRequest.KeepAlive = this.keepAliveEnabled;
            if (this.decompressionEnabled)
            {
                httpWebRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }
            else
            {
                httpWebRequest.AutomaticDecompression = DecompressionMethods.None;
            }
            if (credential != null)
            {
                CredentialCache cache = new CredentialCache();
                cache.Add(this.GetCredentialCacheUriPrefix(via), AuthenticationSchemesHelper.ToString(this.authenticationScheme), credential);
                httpWebRequest.Credentials = cache;
            }
            httpWebRequest.AuthenticationLevel = authenticationLevel;
            httpWebRequest.ImpersonationLevel = impersonationLevel;
            string str = this.GetConnectionGroupName(httpWebRequest, credential, authenticationLevel, impersonationLevel, clientCertificateToken);
            X509CertificateEndpointIdentity identity = to.Identity as X509CertificateEndpointIdentity;
            if (identity != null)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", new object[] { str, identity.Certificates[0].Thumbprint });
            }
            if (!string.IsNullOrEmpty(str))
            {
                httpWebRequest.ConnectionGroupName = str;
            }
            if (this.AuthenticationScheme == AuthenticationSchemes.Basic)
            {
                httpWebRequest.PreAuthenticate = true;
            }
            if (this.proxy != null)
            {
                httpWebRequest.Proxy = this.proxy;
            }
            else if (this.proxyFactory != null)
            {
                httpWebRequest.Proxy = this.proxyFactory.CreateWebProxy(httpWebRequest, proxyTokenProvider, timeout);
            }
            if (this.AllowCookies)
            {
                httpWebRequest.CookieContainer = this.cookieContainer;
            }
            httpWebRequest.ServicePoint.UseNagleAlgorithm = false;
            return httpWebRequest;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeSecurityTokenManager()
        {
            if (this.channelCredentials == null)
            {
                this.channelCredentials = ClientCredentials.CreateDefaultCredentials();
            }
            this.securityTokenManager = this.channelCredentials.CreateSecurityTokenManager();
        }

        protected virtual bool IsSecurityTokenManagerRequired()
        {
            return ((this.AuthenticationScheme != AuthenticationSchemes.Anonymous) || ((this.proxyFactory != null) && (this.proxyFactory.AuthenticationScheme != AuthenticationSchemes.Anonymous)));
        }

        private bool MapIdentity(EndpointAddress target)
        {
            return (((target.Identity != null) && !(target.Identity is X509CertificateEndpointIdentity)) && AuthenticationSchemesHelper.IsWindowsAuth(this.AuthenticationScheme));
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpen(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IRequestChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via)
        {
            this.ValidateCreateChannelParameters(remoteAddress, via);
            return new HttpRequestChannel(this, remoteAddress, via, base.ManualAddressing);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected virtual string OnGetConnectionGroupPrefix(HttpWebRequest httpWebRequest, SecurityTokenContainer clientCertificateToken)
        {
            return string.Empty;
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            if (this.IsSecurityTokenManagerRequired())
            {
                this.InitializeSecurityTokenManager();
            }
            if (this.AllowCookies)
            {
                this.cookieContainer = new CookieContainer();
            }
            if (!httpWebRequestWebPermissionDenied && (HttpWebRequest.DefaultMaximumErrorResponseLength != -1))
            {
                int num;
                if (this.MaxBufferSize >= 0x7ffffbff)
                {
                    num = -1;
                }
                else
                {
                    num = this.MaxBufferSize / 0x400;
                    if ((num * 0x400) < this.MaxBufferSize)
                    {
                        num++;
                    }
                }
                if ((num == -1) || (num > HttpWebRequest.DefaultMaximumErrorResponseLength))
                {
                    try
                    {
                        HttpWebRequest.DefaultMaximumErrorResponseLength = num;
                    }
                    catch (SecurityException exception)
                    {
                        httpWebRequestWebPermissionDenied = true;
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                        }
                    }
                }
            }
        }

        internal static void TraceResponseReceived(HttpWebResponse response, Message message, object receiver)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                if ((response != null) && (response.ResponseUri != null))
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40009, System.ServiceModel.SR.GetString("TraceCodeHttpResponseReceived"), new StringTraceRecord("ResponseUri", response.ResponseUri.ToString()), receiver, null, message);
                }
                else
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40009, System.ServiceModel.SR.GetString("TraceCodeHttpResponseReceived"), receiver, message);
                }
            }
        }

        protected virtual void ValidateCreateChannelParameters(EndpointAddress remoteAddress, Uri via)
        {
            base.ValidateScheme(via);
            if ((base.MessageVersion.Addressing == AddressingVersion.None) && (remoteAddress.Uri != via))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateToMustEqualViaException(remoteAddress.Uri, via));
            }
        }

        public bool AllowCookies
        {
            get
            {
                return this.allowCookies;
            }
        }

        public AuthenticationSchemes AuthenticationScheme
        {
            get
            {
                return this.authenticationScheme;
            }
        }

        public bool DecompressionEnabled
        {
            get
            {
                return this.decompressionEnabled;
            }
        }

        private System.Security.Cryptography.HashAlgorithm HashAlgorithm
        {
            [SecurityCritical]
            get
            {
                if (this.hashAlgorithm == null)
                {
                    this.hashAlgorithm = CryptoHelper.CreateHashAlgorithm("http://www.w3.org/2000/09/xmldsig#sha1");
                }
                else
                {
                    this.hashAlgorithm.Initialize();
                }
                return this.hashAlgorithm;
            }
        }

        public virtual bool IsChannelBindingSupportEnabled
        {
            get
            {
                return false;
            }
        }

        public bool KeepAliveEnabled
        {
            get
            {
                return this.keepAliveEnabled;
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return this.maxBufferSize;
            }
        }

        public IWebProxy Proxy
        {
            get
            {
                return this.proxy;
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttp;
            }
        }

        public System.IdentityModel.Selectors.SecurityTokenManager SecurityTokenManager
        {
            get
            {
                return this.securityTokenManager;
            }
        }

        int IHttpTransportFactorySettings.MaxBufferSize
        {
            get
            {
                return this.MaxBufferSize;
            }
        }

        System.ServiceModel.TransferMode IHttpTransportFactorySettings.TransferMode
        {
            get
            {
                return this.TransferMode;
            }
        }

        public System.ServiceModel.TransferMode TransferMode
        {
            get
            {
                return this.transferMode;
            }
        }

        protected class HttpRequestChannel : RequestChannel
        {
            private ServiceModelActivity activity;
            private ChannelParameterCollection channelParameters;
            private bool cleanupIdentity;
            private HttpChannelFactory factory;
            private SecurityTokenProviderContainer proxyTokenProvider;
            private SecurityTokenProviderContainer tokenProvider;

            public HttpRequestChannel(HttpChannelFactory factory, EndpointAddress to, Uri via, bool manualAddressing) : base(factory, to, via, manualAddressing)
            {
                this.factory = factory;
            }

            private void AbortTokenProviders()
            {
                if (this.tokenProvider != null)
                {
                    this.tokenProvider.Abort();
                }
                if (this.proxyTokenProvider != null)
                {
                    this.proxyTokenProvider.Abort();
                }
            }

            public virtual IAsyncResult BeginGetWebRequest(EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return this.BeginGetWebRequest(to, via, null, ref timeoutHelper, callback, state);
            }

            protected IAsyncResult BeginGetWebRequest(EndpointAddress to, Uri via, SecurityTokenContainer clientCertificateToken, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return new GetWebRequestAsyncResult(this, to, via, clientCertificateToken, ref timeoutHelper, callback, state);
            }

            private void CloseTokenProviders(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (this.tokenProvider != null)
                {
                    this.tokenProvider.Close(helper.RemainingTime());
                }
                if (this.proxyTokenProvider != null)
                {
                    this.proxyTokenProvider.Close(helper.RemainingTime());
                }
            }

            private void CreateAndOpenTokenProviders(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (!base.ManualAddressing)
                {
                    this.Factory.CreateAndOpenTokenProviders(base.RemoteAddress, base.Via, this.channelParameters, helper.RemainingTime(), out this.tokenProvider, out this.proxyTokenProvider);
                }
            }

            protected override IAsyncRequest CreateAsyncRequest(Message message, AsyncCallback callback, object state)
            {
                if (DiagnosticUtility.ShouldUseActivity && (this.activity == null))
                {
                    this.activity = ServiceModelActivity.CreateActivity();
                    if (FxTrace.Trace != null)
                    {
                        FxTrace.Trace.TraceTransfer(this.activity.Id);
                    }
                    ServiceModelActivity.Start(this.activity, System.ServiceModel.SR.GetString("ActivityReceiveBytes", new object[] { base.RemoteAddress.Uri.ToString() }), ActivityType.ReceiveBytes);
                }
                return new HttpChannelAsyncRequest(this, callback, state);
            }

            protected override IRequest CreateRequest(Message message)
            {
                return new HttpChannelRequest(this, this.Factory);
            }

            public virtual HttpWebRequest EndGetWebRequest(IAsyncResult result)
            {
                return GetWebRequestAsyncResult.End(result);
            }

            public override T GetProperty<T>() where T: class
            {
                if (!(typeof(T) == typeof(ChannelParameterCollection)))
                {
                    return base.GetProperty<T>();
                }
                if (base.State == CommunicationState.Created)
                {
                    lock (base.ThisLock)
                    {
                        if (this.channelParameters == null)
                        {
                            this.channelParameters = new ChannelParameterCollection();
                        }
                    }
                }
                return (T) this.channelParameters;
            }

            public virtual HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper)
            {
                return this.GetWebRequest(to, via, null, ref timeoutHelper);
            }

            protected HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, SecurityTokenContainer clientCertificateToken, ref TimeoutHelper timeoutHelper)
            {
                SecurityTokenProviderContainer tokenProvider;
                SecurityTokenProviderContainer proxyTokenProvider;
                HttpWebRequest request;
                if (base.ManualAddressing)
                {
                    this.Factory.CreateAndOpenTokenProviders(to, via, this.channelParameters, timeoutHelper.RemainingTime(), out tokenProvider, out proxyTokenProvider);
                }
                else
                {
                    tokenProvider = this.tokenProvider;
                    proxyTokenProvider = this.proxyTokenProvider;
                }
                try
                {
                    request = this.Factory.GetWebRequest(to, via, tokenProvider, proxyTokenProvider, clientCertificateToken, timeoutHelper.RemainingTime());
                }
                finally
                {
                    if (base.ManualAddressing)
                    {
                        if (tokenProvider != null)
                        {
                            tokenProvider.Abort();
                        }
                        if (proxyTokenProvider != null)
                        {
                            proxyTokenProvider.Abort();
                        }
                    }
                }
                return request;
            }

            protected override void OnAbort()
            {
                this.PrepareClose(true);
                this.AbortTokenProviders();
                base.OnAbort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                IAsyncResult result = null;
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    this.PrepareClose(false);
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    this.CloseTokenProviders(helper.RemainingTime());
                    result = base.BeginWaitForPendingRequests(helper.RemainingTime(), callback, state);
                }
                ServiceModelActivity.Stop(this.activity);
                return result;
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.PrepareOpen();
                this.CreateAndOpenTokenProviders(new TimeoutHelper(timeout).RemainingTime());
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    this.PrepareClose(false);
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    this.CloseTokenProviders(helper.RemainingTime());
                    base.WaitForPendingRequests(helper.RemainingTime());
                }
                ServiceModelActivity.Stop(this.activity);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    base.EndWaitForPendingRequests(result);
                }
                ServiceModelActivity.Stop(this.activity);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.PrepareOpen();
                this.CreateAndOpenTokenProviders(timeout);
            }

            internal virtual void OnWebRequestCompleted(HttpWebRequest request)
            {
            }

            private void PrepareClose(bool aborting)
            {
                if (this.cleanupIdentity)
                {
                    lock (base.ThisLock)
                    {
                        if (this.cleanupIdentity)
                        {
                            this.cleanupIdentity = false;
                            HttpTransportSecurityHelpers.RemoveIdentityMapping(base.Via, base.RemoteAddress, !aborting);
                        }
                    }
                }
            }

            private void PrepareOpen()
            {
                if (this.Factory.MapIdentity(base.RemoteAddress))
                {
                    lock (base.ThisLock)
                    {
                        this.cleanupIdentity = HttpTransportSecurityHelpers.AddIdentityMapping(base.Via, base.RemoteAddress);
                    }
                }
            }

            public virtual bool WillGetWebRequestCompleteSynchronously()
            {
                return ((this.tokenProvider == null) && !this.Factory.ManualAddressing);
            }

            internal ServiceModelActivity Activity
            {
                get
                {
                    return this.activity;
                }
            }

            protected ChannelParameterCollection ChannelParameters
            {
                get
                {
                    return this.channelParameters;
                }
            }

            public HttpChannelFactory Factory
            {
                get
                {
                    return this.factory;
                }
            }

            private class GetWebRequestAsyncResult : AsyncResult
            {
                private SecurityTokenContainer clientCertificateToken;
                private HttpChannelFactory factory;
                private static AsyncCallback onGetSspiCredential;
                private static AsyncCallback onGetUserNameCredential;
                private SecurityTokenProviderContainer proxyTokenProvider;
                private HttpWebRequest request;
                private TimeoutHelper timeoutHelper;
                private EndpointAddress to;
                private SecurityTokenProviderContainer tokenProvider;
                private Uri via;

                public GetWebRequestAsyncResult(HttpChannelFactory.HttpRequestChannel channel, EndpointAddress to, Uri via, SecurityTokenContainer clientCertificateToken, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.to = to;
                    this.via = via;
                    this.clientCertificateToken = clientCertificateToken;
                    this.timeoutHelper = timeoutHelper;
                    this.factory = channel.Factory;
                    this.tokenProvider = channel.tokenProvider;
                    this.proxyTokenProvider = channel.proxyTokenProvider;
                    if (this.factory.ManualAddressing)
                    {
                        this.factory.CreateAndOpenTokenProviders(to, via, channel.channelParameters, timeoutHelper.RemainingTime(), out this.tokenProvider, out this.proxyTokenProvider);
                    }
                    bool flag = false;
                    IAsyncResult result = null;
                    if (this.factory.AuthenticationScheme == AuthenticationSchemes.Anonymous)
                    {
                        this.SetupWebRequest(AuthenticationLevel.None, TokenImpersonationLevel.None, null);
                        flag = true;
                    }
                    else if (this.factory.AuthenticationScheme == AuthenticationSchemes.Basic)
                    {
                        if (onGetUserNameCredential == null)
                        {
                            onGetUserNameCredential = Fx.ThunkCallback(new AsyncCallback(HttpChannelFactory.HttpRequestChannel.GetWebRequestAsyncResult.OnGetUserNameCredential));
                        }
                        result = TransportSecurityHelpers.BeginGetUserNameCredential(this.tokenProvider, timeoutHelper.RemainingTime(), onGetUserNameCredential, this);
                        if (result.CompletedSynchronously)
                        {
                            this.CompleteGetUserNameCredential(result);
                            flag = true;
                        }
                    }
                    else
                    {
                        if (onGetSspiCredential == null)
                        {
                            onGetSspiCredential = Fx.ThunkCallback(new AsyncCallback(HttpChannelFactory.HttpRequestChannel.GetWebRequestAsyncResult.OnGetSspiCredential));
                        }
                        result = TransportSecurityHelpers.BeginGetSspiCredential(this.tokenProvider, timeoutHelper.RemainingTime(), onGetSspiCredential, this);
                        if (result.CompletedSynchronously)
                        {
                            this.CompleteGetSspiCredential(result);
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        this.CloseTokenProvidersIfRequired();
                        base.Complete(true);
                    }
                }

                private void CloseTokenProvidersIfRequired()
                {
                    if (this.factory.ManualAddressing)
                    {
                        if (this.tokenProvider != null)
                        {
                            this.tokenProvider.Abort();
                        }
                        if (this.proxyTokenProvider != null)
                        {
                            this.proxyTokenProvider.Abort();
                        }
                    }
                }

                private void CompleteGetSspiCredential(IAsyncResult result)
                {
                    AuthenticationLevel level;
                    TokenImpersonationLevel level2;
                    NetworkCredential credential = TransportSecurityHelpers.EndGetSspiCredential(result, out level2, out level);
                    if (this.factory.AuthenticationScheme == AuthenticationSchemes.Digest)
                    {
                        HttpChannelUtilities.ValidateDigestCredential(ref credential, level2);
                    }
                    else if ((this.factory.AuthenticationScheme == AuthenticationSchemes.Ntlm) && (level == AuthenticationLevel.MutualAuthRequired))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CredentialDisallowsNtlm")));
                    }
                    this.SetupWebRequest(level, level2, credential);
                }

                private void CompleteGetUserNameCredential(IAsyncResult result)
                {
                    NetworkCredential credential = TransportSecurityHelpers.EndGetUserNameCredential(result);
                    this.SetupWebRequest(AuthenticationLevel.None, TokenImpersonationLevel.None, credential);
                }

                public static HttpWebRequest End(IAsyncResult result)
                {
                    return AsyncResult.End<HttpChannelFactory.HttpRequestChannel.GetWebRequestAsyncResult>(result).request;
                }

                private static void OnGetSspiCredential(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        HttpChannelFactory.HttpRequestChannel.GetWebRequestAsyncResult asyncState = (HttpChannelFactory.HttpRequestChannel.GetWebRequestAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.CompleteGetSspiCredential(result);
                            asyncState.CloseTokenProvidersIfRequired();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }

                private static void OnGetUserNameCredential(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        HttpChannelFactory.HttpRequestChannel.GetWebRequestAsyncResult asyncState = (HttpChannelFactory.HttpRequestChannel.GetWebRequestAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.CompleteGetUserNameCredential(result);
                            asyncState.CloseTokenProvidersIfRequired();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }

                private void SetupWebRequest(AuthenticationLevel authenticationLevel, TokenImpersonationLevel impersonationLevel, NetworkCredential credential)
                {
                    this.request = this.factory.GetWebRequest(this.to, this.via, credential, impersonationLevel, authenticationLevel, this.proxyTokenProvider, this.clientCertificateToken, this.timeoutHelper.RemainingTime());
                }
            }

            private class HttpChannelAsyncRequest : TraceAsyncResult, IAsyncRequest, IAsyncResult, IRequestBase
            {
                private HttpAbortReason abortReason;
                private HttpChannelFactory.HttpRequestChannel channel;
                private ChannelBinding channelBinding;
                private HttpChannelFactory factory;
                private HttpInput httpInput;
                private HttpOutput httpOutput;
                private Message message;
                private static AsyncCallback onGetResponse = Fx.ThunkCallback(new AsyncCallback(HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest.OnGetResponse));
                private static AsyncCallback onGetWebRequestCompleted;
                private static AsyncCallback onProcessIncomingMessage = Fx.ThunkCallback(new AsyncCallback(HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest.OnParseIncomingMessage));
                private static AsyncCallback onSend = Fx.ThunkCallback(new AsyncCallback(HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest.OnSend));
                private static Action<object> onSendTimeout;
                private Message replyMessage;
                private HttpWebRequest request;
                private Message requestMessage;
                private HttpWebResponse response;
                private object sendLock;
                private IOThreadTimer sendTimer;
                private TimeoutHelper timeoutHelper;
                private EndpointAddress to;
                private Uri via;

                public HttpChannelAsyncRequest(HttpChannelFactory.HttpRequestChannel channel, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.sendLock = new object();
                    this.channel = channel;
                    this.to = channel.RemoteAddress;
                    this.via = channel.Via;
                    this.factory = channel.Factory;
                }

                public void Abort(RequestChannel channel)
                {
                    this.Cleanup();
                    this.abortReason = HttpAbortReason.Aborted;
                }

                private void AbortSend()
                {
                    this.CancelSendTimer();
                    if (this.request != null)
                    {
                        this.channel.OnWebRequestCompleted(this.request);
                        this.abortReason = HttpAbortReason.TimedOut;
                        this.httpOutput.Abort(this.abortReason);
                    }
                }

                public void BeginSendRequest(Message message, TimeSpan timeout)
                {
                    this.message = this.requestMessage = message;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.factory.ApplyManualAddressing(ref this.to, ref this.via, this.requestMessage);
                    if (this.channel.WillGetWebRequestCompleteSynchronously())
                    {
                        this.SetWebRequest(this.channel.GetWebRequest(this.to, this.via, ref this.timeoutHelper));
                        if (this.SendWebRequest())
                        {
                            base.Complete(true);
                        }
                    }
                    else
                    {
                        if (onGetWebRequestCompleted == null)
                        {
                            onGetWebRequestCompleted = Fx.ThunkCallback(new AsyncCallback(HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest.OnGetWebRequestCompletedCallback));
                        }
                        IAsyncResult result = this.channel.BeginGetWebRequest(this.to, this.via, ref this.timeoutHelper, onGetWebRequestCompleted, this);
                        if (result.CompletedSynchronously)
                        {
                            if (TD.MessageSentByTransportIsEnabled())
                            {
                                TD.MessageSentByTransport(this.to.Uri.AbsoluteUri);
                            }
                            if (this.OnGetWebRequestCompleted(result))
                            {
                                base.Complete(true);
                            }
                        }
                    }
                }

                private void CancelSendTimer()
                {
                    lock (this.sendLock)
                    {
                        if (this.sendTimer != null)
                        {
                            this.sendTimer.Cancel();
                            this.sendTimer = null;
                        }
                    }
                }

                private void Cleanup()
                {
                    if (this.request != null)
                    {
                        HttpChannelUtilities.AbortRequest(this.request);
                        this.channel.OnWebRequestCompleted(this.request);
                    }
                    ChannelBinding channelBinding = this.channelBinding;
                    this.channelBinding = null;
                    if (channelBinding != null)
                    {
                        channelBinding.Dispose();
                    }
                }

                private bool CompleteGetResponse(IAsyncResult result)
                {
                    using (ServiceModelActivity.BoundOperation(this.channel.Activity))
                    {
                        HttpWebResponse response = null;
                        WebException responseException = null;
                        try
                        {
                            try
                            {
                                this.CancelSendTimer();
                                response = (HttpWebResponse) this.request.EndGetResponse(result);
                            }
                            catch (NullReferenceException exception2)
                            {
                                if (TransferModeHelper.IsRequestStreamed(this.factory.transferMode))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateNullReferenceResponseException(exception2));
                                }
                                throw;
                            }
                            if (DiagnosticUtility.ShouldTraceVerbose)
                            {
                                HttpChannelFactory.TraceResponseReceived(response, this.message, this);
                            }
                            if (TD.MessageReceivedByTransportIsEnabled())
                            {
                                TD.MessageReceivedByTransport(this.to.Uri.AbsoluteUri);
                            }
                        }
                        catch (WebException exception3)
                        {
                            responseException = exception3;
                            response = HttpChannelUtilities.ProcessGetResponseWebException(exception3, this.request, this.abortReason);
                        }
                        return this.ProcessResponse(response, responseException);
                    }
                }

                private void CompleteParseIncomingMessage(IAsyncResult result)
                {
                    Exception requestException = null;
                    this.replyMessage = this.httpInput.EndParseIncomingMessage(result, out requestException);
                    if (this.replyMessage != null)
                    {
                        HttpChannelUtilities.AddReplySecurityProperty(this.factory, this.request, this.response, this.replyMessage);
                    }
                }

                private bool CompleteSend(IAsyncResult result)
                {
                    bool flag2;
                    bool flag = false;
                    try
                    {
                        this.httpOutput.EndSend(result);
                        this.channelBinding = this.httpOutput.TakeChannelBinding();
                        this.httpOutput.Close();
                        flag = true;
                        if (TD.MessageSentByTransportIsEnabled())
                        {
                            TD.MessageSentByTransport(this.to.Uri.AbsoluteUri);
                        }
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.httpOutput.Abort(HttpAbortReason.Aborted);
                        }
                        if (!object.ReferenceEquals(this.message, this.requestMessage))
                        {
                            this.requestMessage.Close();
                        }
                    }
                    try
                    {
                        IAsyncResult result2;
                        try
                        {
                            result2 = this.request.BeginGetResponse(onGetResponse, this);
                        }
                        catch (NullReferenceException exception)
                        {
                            if (TransferModeHelper.IsRequestStreamed(this.factory.transferMode))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateNullReferenceResponseException(exception));
                            }
                            throw;
                        }
                        if (result2.CompletedSynchronously)
                        {
                            return this.CompleteGetResponse(result2);
                        }
                        flag2 = false;
                    }
                    catch (IOException exception2)
                    {
                        throw TraceUtility.ThrowHelperError(new CommunicationException(exception2.Message, exception2), this.requestMessage);
                    }
                    catch (WebException exception3)
                    {
                        throw TraceUtility.ThrowHelperError(new CommunicationException(exception3.Message, exception3), this.requestMessage);
                    }
                    catch (ObjectDisposedException exception4)
                    {
                        if (this.abortReason == HttpAbortReason.Aborted)
                        {
                            throw TraceUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("HttpRequestAborted", new object[] { this.to.Uri }), exception4), this.requestMessage);
                        }
                        throw TraceUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("HttpRequestTimedOut", new object[] { this.to.Uri, this.timeoutHelper.OriginalTimeout }), exception4), this.requestMessage);
                    }
                    return flag2;
                }

                public Message End()
                {
                    End(this);
                    return this.replyMessage;
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest>(result);
                }

                public void Fault(RequestChannel channel)
                {
                    this.Cleanup();
                }

                private static void OnGetResponse(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool flag;
                        HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest asyncState = (HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.CompleteGetResponse(result);
                        }
                        catch (WebException exception2)
                        {
                            flag = true;
                            exception = new CommunicationException(exception2.Message, exception2);
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception3;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private bool OnGetWebRequestCompleted(IAsyncResult result)
                {
                    this.SetWebRequest(this.channel.EndGetWebRequest(result));
                    return this.SendWebRequest();
                }

                private static void OnGetWebRequestCompletedCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool flag;
                        HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest asyncState = (HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.OnGetWebRequestCompleted(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception2;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private static void OnParseIncomingMessage(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest asyncState = (HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.CompleteParseIncomingMessage(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }

                private static void OnSend(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool flag;
                        HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest asyncState = (HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.CompleteSend(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception2;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private static void OnSendTimeout(object state)
                {
                    ((HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest) state).AbortSend();
                }

                private bool ProcessResponse(HttpWebResponse response, WebException responseException)
                {
                    this.httpInput = HttpChannelUtilities.ValidateRequestReplyResponse(this.request, response, this.factory, responseException, this.channelBinding);
                    this.channelBinding = null;
                    if (this.httpInput != null)
                    {
                        this.response = response;
                        IAsyncResult result = this.httpInput.BeginParseIncomingMessage(onProcessIncomingMessage, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.CompleteParseIncomingMessage(result);
                    }
                    else
                    {
                        this.replyMessage = null;
                    }
                    this.channel.OnWebRequestCompleted(this.request);
                    return true;
                }

                private bool SendWebRequest()
                {
                    bool flag3;
                    this.httpOutput = HttpOutput.CreateHttpOutput(this.request, this.factory, this.requestMessage, this.factory.IsChannelBindingSupportEnabled);
                    bool flag = false;
                    try
                    {
                        bool flag2 = false;
                        this.SetSendTimeout(this.timeoutHelper.RemainingTime());
                        IAsyncResult result = this.httpOutput.BeginSend(this.timeoutHelper.RemainingTime(), onSend, this);
                        flag = true;
                        if (result.CompletedSynchronously)
                        {
                            flag2 = this.CompleteSend(result);
                        }
                        flag3 = flag2;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.httpOutput.Abort(HttpAbortReason.Aborted);
                            if (!object.ReferenceEquals(this.message, this.requestMessage))
                            {
                                this.requestMessage.Close();
                            }
                        }
                    }
                    return flag3;
                }

                private void SetSendTimeout(TimeSpan timeout)
                {
                    HttpChannelUtilities.SetRequestTimeout(this.request, timeout);
                    if (timeout == TimeSpan.MaxValue)
                    {
                        this.CancelSendTimer();
                    }
                    else
                    {
                        this.SendTimer.Set(timeout);
                    }
                }

                private void SetWebRequest(HttpWebRequest webRequest)
                {
                    this.request = webRequest;
                    if (this.channel.State != CommunicationState.Opened)
                    {
                        this.Cleanup();
                        this.channel.ThrowIfDisposedOrNotOpen();
                    }
                }

                private IOThreadTimer SendTimer
                {
                    get
                    {
                        if (this.sendTimer == null)
                        {
                            if (onSendTimeout == null)
                            {
                                onSendTimeout = new Action<object>(HttpChannelFactory.HttpRequestChannel.HttpChannelAsyncRequest.OnSendTimeout);
                            }
                            this.sendTimer = new IOThreadTimer(onSendTimeout, this, false);
                        }
                        return this.sendTimer;
                    }
                }
            }

            private class HttpChannelRequest : IRequest, IRequestBase
            {
                private HttpAbortReason abortReason;
                private HttpChannelFactory.HttpRequestChannel channel;
                private ChannelBinding channelBinding;
                private HttpChannelFactory factory;
                private EndpointAddress to;
                private Uri via;
                private HttpWebRequest webRequest;

                public HttpChannelRequest(HttpChannelFactory.HttpRequestChannel channel, HttpChannelFactory factory)
                {
                    this.channel = channel;
                    this.to = channel.RemoteAddress;
                    this.via = channel.Via;
                    this.factory = factory;
                }

                public void Abort(RequestChannel channel)
                {
                    this.Cleanup();
                    this.abortReason = HttpAbortReason.Aborted;
                }

                private void Cleanup()
                {
                    if (this.webRequest != null)
                    {
                        HttpChannelUtilities.AbortRequest(this.webRequest);
                        this.channel.OnWebRequestCompleted(this.webRequest);
                    }
                    ChannelBinding channelBinding = this.channelBinding;
                    this.channelBinding = null;
                    if (channelBinding != null)
                    {
                        channelBinding.Dispose();
                    }
                }

                public void Fault(RequestChannel channel)
                {
                    this.Cleanup();
                }

                public void SendRequest(Message message, TimeSpan timeout)
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    this.factory.ApplyManualAddressing(ref this.to, ref this.via, message);
                    this.webRequest = this.channel.GetWebRequest(this.to, this.via, ref timeoutHelper);
                    Message message2 = message;
                    try
                    {
                        if (this.channel.State != CommunicationState.Opened)
                        {
                            this.Cleanup();
                            this.channel.ThrowIfDisposedOrNotOpen();
                        }
                        HttpChannelUtilities.SetRequestTimeout(this.webRequest, timeoutHelper.RemainingTime());
                        HttpOutput output = HttpOutput.CreateHttpOutput(this.webRequest, this.factory, message2, this.factory.IsChannelBindingSupportEnabled);
                        bool flag = false;
                        try
                        {
                            output.Send(timeoutHelper.RemainingTime());
                            this.channelBinding = output.TakeChannelBinding();
                            output.Close();
                            flag = true;
                            if (TD.MessageSentByTransportIsEnabled())
                            {
                                TD.MessageSentByTransport(this.to.Uri.AbsoluteUri);
                            }
                        }
                        finally
                        {
                            if (!flag)
                            {
                                output.Abort(HttpAbortReason.Aborted);
                            }
                        }
                    }
                    finally
                    {
                        if (!object.ReferenceEquals(message2, message))
                        {
                            message2.Close();
                        }
                    }
                }

                public Message WaitForReply(TimeSpan timeout)
                {
                    HttpWebResponse response = null;
                    WebException responseException = null;
                    try
                    {
                        try
                        {
                            response = (HttpWebResponse) this.webRequest.GetResponse();
                        }
                        catch (NullReferenceException exception2)
                        {
                            if (TransferModeHelper.IsRequestStreamed(this.factory.transferMode))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateNullReferenceResponseException(exception2));
                            }
                            throw;
                        }
                        if (DiagnosticUtility.ShouldTraceVerbose)
                        {
                            HttpChannelFactory.TraceResponseReceived(response, null, this);
                        }
                        if (TD.MessageReceivedByTransportIsEnabled())
                        {
                            TD.MessageReceivedByTransport((response.ResponseUri != null) ? response.ResponseUri.AbsoluteUri : string.Empty);
                        }
                    }
                    catch (WebException exception3)
                    {
                        responseException = exception3;
                        response = HttpChannelUtilities.ProcessGetResponseWebException(exception3, this.webRequest, this.abortReason);
                    }
                    HttpInput input = HttpChannelUtilities.ValidateRequestReplyResponse(this.webRequest, response, this.factory, responseException, this.channelBinding);
                    this.channelBinding = null;
                    Message replyMessage = null;
                    if (input != null)
                    {
                        Exception requestException = null;
                        replyMessage = input.ParseIncomingMessage(out requestException);
                        if (replyMessage != null)
                        {
                            HttpChannelUtilities.AddReplySecurityProperty(this.factory, this.webRequest, response, replyMessage);
                        }
                    }
                    this.channel.OnWebRequestCompleted(this.webRequest);
                    return replyMessage;
                }
            }
        }

        private class WebProxyFactory
        {
            private Uri address;
            private AuthenticationSchemes authenticationScheme;
            private bool bypassOnLocal;

            public WebProxyFactory(Uri address, bool bypassOnLocal, AuthenticationSchemes authenticationScheme)
            {
                this.address = address;
                this.bypassOnLocal = bypassOnLocal;
                this.authenticationScheme = authenticationScheme;
            }

            public IWebProxy CreateWebProxy(HttpWebRequest request, SecurityTokenProviderContainer tokenProvider, TimeSpan timeout)
            {
                WebProxy proxy = new WebProxy(this.address, this.bypassOnLocal);
                if (this.authenticationScheme != AuthenticationSchemes.Anonymous)
                {
                    TokenImpersonationLevel level;
                    AuthenticationLevel level2;
                    NetworkCredential cred = HttpChannelUtilities.GetCredential(this.authenticationScheme, tokenProvider, timeout, out level, out level2);
                    if (!TokenImpersonationLevelHelper.IsGreaterOrEqual(level, request.ImpersonationLevel))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ProxyImpersonationLevelMismatch", new object[] { level, request.ImpersonationLevel })));
                    }
                    if ((level2 == AuthenticationLevel.MutualAuthRequired) && (request.AuthenticationLevel != AuthenticationLevel.MutualAuthRequired))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ProxyAuthenticationLevelMismatch", new object[] { level2, request.AuthenticationLevel })));
                    }
                    CredentialCache cache = new CredentialCache();
                    cache.Add(this.address, AuthenticationSchemesHelper.ToString(this.authenticationScheme), cred);
                    proxy.Credentials = cache;
                }
                return proxy;
            }

            internal AuthenticationSchemes AuthenticationScheme
            {
                get
                {
                    return this.authenticationScheme;
                }
            }
        }
    }
}

