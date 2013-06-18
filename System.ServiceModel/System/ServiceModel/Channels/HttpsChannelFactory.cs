namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;

    internal class HttpsChannelFactory : HttpChannelFactory
    {
        private IChannelBindingProvider channelBindingProvider;
        private bool requireClientCertificate;

        internal HttpsChannelFactory(HttpsTransportBindingElement httpsBindingElement, BindingContext context) : base(httpsBindingElement, context)
        {
            this.requireClientCertificate = httpsBindingElement.RequireClientCertificate;
            this.channelBindingProvider = new ChannelBindingProviderHelper();
        }

        private SecurityTokenProvider CreateAndOpenCertificateTokenProvider(EndpointAddress target, Uri via, ChannelParameterCollection channelParameters, TimeSpan timeout)
        {
            if (!this.RequireClientCertificate)
            {
                return null;
            }
            SecurityTokenProvider tokenProvider = TransportSecurityHelpers.GetCertificateTokenProvider(base.SecurityTokenManager, target, via, this.Scheme, channelParameters);
            System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(tokenProvider, timeout);
            return tokenProvider;
        }

        internal override SecurityMessageProperty CreateReplySecurityProperty(HttpWebRequest request, HttpWebResponse response)
        {
            X509Certificate certificate = request.ServicePoint.Certificate;
            if (certificate != null)
            {
                X509Certificate2 certificate2 = new X509Certificate2(certificate);
                SecurityToken token = new X509SecurityToken(certificate2, false);
                ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = System.ServiceModel.Security.SecurityUtils.NonValidatingX509Authenticator.ValidateToken(token);
                return new SecurityMessageProperty { TransportToken = new SecurityTokenSpecification(token, tokenPolicies), ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies) };
            }
            return base.CreateReplySecurityProperty(request, response);
        }

        private SecurityTokenContainer GetCertificateSecurityToken(SecurityTokenProvider certificateProvider, EndpointAddress to, Uri via, ChannelParameterCollection channelParameters, ref TimeoutHelper timeoutHelper)
        {
            SecurityToken token = null;
            SecurityTokenContainer container = null;
            SecurityTokenProvider provider;
            if (base.ManualAddressing && this.RequireClientCertificate)
            {
                provider = this.CreateAndOpenCertificateTokenProvider(to, via, channelParameters, timeoutHelper.RemainingTime());
            }
            else
            {
                provider = certificateProvider;
            }
            if (provider != null)
            {
                token = provider.GetToken(timeoutHelper.RemainingTime());
            }
            if (base.ManualAddressing && this.RequireClientCertificate)
            {
                System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(provider);
            }
            if (token != null)
            {
                container = new SecurityTokenContainer(token);
            }
            return container;
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IChannelBindingProvider))
            {
                return (T) this.channelBindingProvider;
            }
            return base.GetProperty<T>();
        }

        protected override bool IsSecurityTokenManagerRequired()
        {
            if (!this.requireClientCertificate)
            {
                return base.IsSecurityTokenManagerRequired();
            }
            return true;
        }

        protected override IRequestChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            this.ValidateCreateChannelParameters(address, via);
            return new HttpsRequestChannel(this, address, via, base.ManualAddressing);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            base.OnEndOpen(result);
            this.OnOpenCore();
        }

        protected override string OnGetConnectionGroupPrefix(HttpWebRequest httpWebRequest, SecurityTokenContainer clientCertificateToken)
        {
            StringBuilder builder = new StringBuilder();
            string str = "\0";
            if (this.RequireClientCertificate)
            {
                SetCertificate(httpWebRequest, clientCertificateToken);
                X509CertificateCollection clientCertificates = httpWebRequest.ClientCertificates;
                for (int i = 0; i < clientCertificates.Count; i++)
                {
                    builder.AppendFormat("{0}{1}", clientCertificates[i].GetCertHashString(), str);
                }
            }
            return builder.ToString();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            base.OnOpen(timeout);
            this.OnOpenCore();
        }

        private void OnOpenCore()
        {
            if (this.requireClientCertificate && (base.SecurityTokenManager == null))
            {
                throw Fx.AssertAndThrow("HttpsChannelFactory: SecurityTokenManager is null on open.");
            }
        }

        private static void SetCertificate(HttpWebRequest request, SecurityTokenContainer clientCertificateToken)
        {
            if (clientCertificateToken != null)
            {
                X509SecurityToken token = (X509SecurityToken) clientCertificateToken.Token;
                request.ClientCertificates.Add(token.Certificate);
            }
        }

        protected override void ValidateCreateChannelParameters(EndpointAddress remoteAddress, Uri via)
        {
            if (remoteAddress.Identity != null)
            {
                X509CertificateEndpointIdentity identity = remoteAddress.Identity as X509CertificateEndpointIdentity;
                if ((identity != null) && (identity.Certificates.Count > 1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("remoteAddress", System.ServiceModel.SR.GetString("HttpsIdentityMultipleCerts", new object[] { remoteAddress.Uri }));
                }
                EndpointIdentity identity2 = remoteAddress.Identity;
                bool flag = (((identity != null) || ClaimTypes.Spn.Equals(identity2.IdentityClaim.ClaimType)) || ClaimTypes.Upn.Equals(identity2.IdentityClaim.ClaimType)) || ClaimTypes.Dns.Equals(identity2.IdentityClaim.ClaimType);
                if (!AuthenticationSchemesHelper.IsWindowsAuth(base.AuthenticationScheme) && !flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("remoteAddress", System.ServiceModel.SR.GetString("HttpsExplicitIdentity"));
                }
            }
            base.ValidateCreateChannelParameters(remoteAddress, via);
        }

        public override bool IsChannelBindingSupportEnabled
        {
            get
            {
                return this.channelBindingProvider.IsChannelBindingSupportEnabled;
            }
        }

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttps;
            }
        }

        private class HttpsRequestChannel : HttpChannelFactory.HttpRequestChannel
        {
            private SecurityTokenProvider certificateProvider;
            private HttpsChannelFactory factory;

            public HttpsRequestChannel(HttpsChannelFactory factory, EndpointAddress to, Uri via, bool manualAddressing) : base(factory, to, via, manualAddressing)
            {
                this.factory = factory;
            }

            private void AbortTokenProvider()
            {
                if (this.certificateProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.certificateProvider);
                }
            }

            public IAsyncResult BeginBaseGetWebRequest(EndpointAddress to, Uri via, SecurityTokenContainer clientCertificateToken, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return base.BeginGetWebRequest(to, via, clientCertificateToken, ref timeoutHelper, callback, state);
            }

            public override IAsyncResult BeginGetWebRequest(EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return new GetWebRequestAsyncResult(this, to, via, ref timeoutHelper, callback, state);
            }

            private void CloseTokenProvider(TimeSpan timeout)
            {
                if (this.certificateProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.certificateProvider, timeout);
                }
            }

            private void CreateAndOpenTokenProvider(TimeSpan timeout)
            {
                if (!base.ManualAddressing && this.Factory.RequireClientCertificate)
                {
                    this.certificateProvider = this.Factory.CreateAndOpenCertificateTokenProvider(base.RemoteAddress, base.Via, base.ChannelParameters, timeout);
                }
            }

            public HttpWebRequest EndBaseGetWebRequest(IAsyncResult result)
            {
                return base.EndGetWebRequest(result);
            }

            public override HttpWebRequest EndGetWebRequest(IAsyncResult result)
            {
                return GetWebRequestAsyncResult.End(result);
            }

            public override HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper)
            {
                SecurityTokenContainer clientCertificateToken = this.Factory.GetCertificateSecurityToken(this.certificateProvider, to, via, base.ChannelParameters, ref timeoutHelper);
                HttpWebRequest request = base.GetWebRequest(to, via, clientCertificateToken, ref timeoutHelper);
                HttpTransportSecurityHelpers.AddServerCertMapping(request, to);
                return request;
            }

            protected override void OnAbort()
            {
                this.AbortTokenProvider();
                base.OnAbort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.CloseTokenProvider(helper.RemainingTime());
                return base.OnBeginClose(helper.RemainingTime(), callback, state);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.CreateAndOpenTokenProvider(helper.RemainingTime());
                return base.OnBeginOpen(helper.RemainingTime(), callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.CloseTokenProvider(helper.RemainingTime());
                base.OnClose(helper.RemainingTime());
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.CreateAndOpenTokenProvider(helper.RemainingTime());
                base.OnOpen(helper.RemainingTime());
            }

            internal override void OnWebRequestCompleted(HttpWebRequest request)
            {
                HttpTransportSecurityHelpers.RemoveServerCertMapping(request);
            }

            public override bool WillGetWebRequestCompleteSynchronously()
            {
                if (!base.WillGetWebRequestCompleteSynchronously())
                {
                    return false;
                }
                return ((this.certificateProvider == null) && !this.Factory.ManualAddressing);
            }

            public HttpsChannelFactory Factory
            {
                get
                {
                    return this.factory;
                }
            }

            private class GetWebRequestAsyncResult : AsyncResult
            {
                private SecurityTokenProvider certificateProvider;
                private HttpsChannelFactory factory;
                private HttpsChannelFactory.HttpsRequestChannel httpsChannel;
                private static AsyncCallback onGetBaseWebRequestCallback = Fx.ThunkCallback(new AsyncCallback(HttpsChannelFactory.HttpsRequestChannel.GetWebRequestAsyncResult.OnGetBaseWebRequestCallback));
                private static AsyncCallback onGetTokenCallback;
                private HttpWebRequest request;
                private TimeoutHelper timeoutHelper;
                private EndpointAddress to;
                private SecurityTokenContainer tokenContainer;
                private Uri via;

                public GetWebRequestAsyncResult(HttpsChannelFactory.HttpsRequestChannel httpsChannel, EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.httpsChannel = httpsChannel;
                    this.to = to;
                    this.via = via;
                    this.timeoutHelper = timeoutHelper;
                    this.factory = httpsChannel.Factory;
                    this.certificateProvider = httpsChannel.certificateProvider;
                    if (this.factory.ManualAddressing && this.factory.RequireClientCertificate)
                    {
                        this.certificateProvider = this.factory.CreateAndOpenCertificateTokenProvider(to, via, httpsChannel.ChannelParameters, timeoutHelper.RemainingTime());
                    }
                    if (this.GetToken() && this.GetWebRequest())
                    {
                        base.Complete(true);
                    }
                }

                private void CloseCertificateProviderIfRequired()
                {
                    if (this.factory.ManualAddressing && (this.certificateProvider != null))
                    {
                        System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.certificateProvider);
                    }
                }

                public static HttpWebRequest End(IAsyncResult result)
                {
                    return AsyncResult.End<HttpsChannelFactory.HttpsRequestChannel.GetWebRequestAsyncResult>(result).request;
                }

                private bool GetToken()
                {
                    if (this.certificateProvider != null)
                    {
                        if (onGetTokenCallback == null)
                        {
                            onGetTokenCallback = Fx.ThunkCallback(new AsyncCallback(HttpsChannelFactory.HttpsRequestChannel.GetWebRequestAsyncResult.OnGetTokenCallback));
                        }
                        IAsyncResult result = this.certificateProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), onGetTokenCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.OnGetToken(result);
                    }
                    return true;
                }

                private bool GetWebRequest()
                {
                    IAsyncResult result = this.httpsChannel.BeginBaseGetWebRequest(this.to, this.via, this.tokenContainer, ref this.timeoutHelper, onGetBaseWebRequestCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.request = this.httpsChannel.EndBaseGetWebRequest(result);
                    HttpTransportSecurityHelpers.AddServerCertMapping(this.request, this.to);
                    return true;
                }

                private static void OnGetBaseWebRequestCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        HttpsChannelFactory.HttpsRequestChannel.GetWebRequestAsyncResult asyncState = (HttpsChannelFactory.HttpsRequestChannel.GetWebRequestAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.request = asyncState.httpsChannel.EndBaseGetWebRequest(result);
                            HttpTransportSecurityHelpers.AddServerCertMapping(asyncState.request, asyncState.to);
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

                private void OnGetToken(IAsyncResult result)
                {
                    SecurityToken token = this.certificateProvider.EndGetToken(result);
                    if (token != null)
                    {
                        this.tokenContainer = new SecurityTokenContainer(token);
                    }
                    this.CloseCertificateProviderIfRequired();
                }

                private static void OnGetTokenCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool webRequest;
                        HttpsChannelFactory.HttpsRequestChannel.GetWebRequestAsyncResult asyncState = (HttpsChannelFactory.HttpsRequestChannel.GetWebRequestAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.OnGetToken(result);
                            webRequest = asyncState.GetWebRequest();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            webRequest = true;
                            exception = exception2;
                        }
                        if (webRequest)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }
            }
        }
    }
}

