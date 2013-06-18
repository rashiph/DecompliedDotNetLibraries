namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class HttpChannelListener : TransportChannelListener, IHttpTransportFactorySettings, ITransportFactorySettings, IDefaultCommunicationTimeouts, IChannelListener<IReplyChannel>, IChannelListener, ICommunicationObject
    {
        private ReplyChannelAcceptor acceptor;
        private HttpAnonymousUriPrefixMatcher anonymousUriPrefixMatcher;
        private AuthenticationSchemes authenticationScheme;
        private SecurityCredentialsManager credentialProvider;
        private System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy extendedProtectionPolicy;
        private bool extractGroupsForWindowsAccounts;
        private EndpointIdentity identity;
        private bool keepAliveEnabled;
        private int maxBufferSize;
        private string method;
        private string realm;
        private ISecurityCapabilities securityCapabilities;
        private System.ServiceModel.TransferMode transferMode;
        private static UriPrefixTable<ITransportManagerRegistration> transportManagerTable = new UriPrefixTable<ITransportManagerRegistration>(true);
        private bool unsafeConnectionNtlmAuthentication;
        private SecurityTokenAuthenticator userNameTokenAuthenticator;
        private bool usingDefaultSpnList;
        private SecurityTokenAuthenticator windowsTokenAuthenticator;

        public HttpChannelListener(HttpTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context, HttpTransportDefaults.GetDefaultMessageEncoderFactory(), bindingElement.HostNameComparisonMode)
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
            if ((bindingElement.AuthenticationScheme == AuthenticationSchemes.Basic) && (bindingElement.ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Always))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("ExtendedProtectionPolicyBasicAuthNotSupported")));
            }
            this.authenticationScheme = bindingElement.AuthenticationScheme;
            this.keepAliveEnabled = bindingElement.KeepAliveEnabled;
            base.InheritBaseAddressSettings = bindingElement.InheritBaseAddressSettings;
            this.maxBufferSize = bindingElement.MaxBufferSize;
            this.method = bindingElement.Method;
            this.realm = bindingElement.Realm;
            this.transferMode = bindingElement.TransferMode;
            this.unsafeConnectionNtlmAuthentication = bindingElement.UnsafeConnectionNtlmAuthentication;
            this.credentialProvider = context.BindingParameters.Find<SecurityCredentialsManager>();
            this.acceptor = new TransportReplyChannelAcceptor(this);
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
            this.extendedProtectionPolicy = GetPolicyWithDefaultSpnCollection(bindingElement.ExtendedProtectionPolicy, this.authenticationScheme, base.HostNameComparisonModeInternal, base.Uri, out this.usingDefaultSpnList);
            if (bindingElement.AnonymousUriPrefixMatcher != null)
            {
                this.anonymousUriPrefixMatcher = new HttpAnonymousUriPrefixMatcher(bindingElement.AnonymousUriPrefixMatcher);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AbortUserNameTokenAuthenticator()
        {
            System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.userNameTokenAuthenticator);
        }

        public IReplyChannel AcceptChannel()
        {
            return this.AcceptChannel(this.DefaultReceiveTimeout);
        }

        public IReplyChannel AcceptChannel(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            return this.acceptor.AcceptChannel(timeout);
        }

        private static void AddSpn(Dictionary<string, string> list, string value)
        {
            string key = value.ToLowerInvariant();
            if (!list.ContainsKey(key))
            {
                list.Add(key, value);
            }
        }

        internal override void ApplyHostedContext(string virtualPath, bool isMetadataListener)
        {
            base.ApplyHostedContext(virtualPath, isMetadataListener);
            AspNetEnvironment.Current.ValidateHttpSettings(virtualPath, isMetadataListener, this.usingDefaultSpnList, ref this.authenticationScheme, ref this.extendedProtectionPolicy, ref this.realm);
        }

        public IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return this.BeginAcceptChannel(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            return this.acceptor.BeginAcceptChannel(timeout, callback, state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CloseUserNameTokenAuthenticator(TimeSpan timeout)
        {
            System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.userNameTokenAuthenticator, timeout);
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return new SharedHttpTransportManager(listenUri, this);
        }

        public IReplyChannel EndAcceptChannel(IAsyncResult result)
        {
            base.ThrowPending();
            return this.acceptor.EndAcceptChannel(result);
        }

        private string GetAuthType(HttpListenerContext listenerContext)
        {
            string authenticationType = null;
            IPrincipal user = listenerContext.User;
            if ((user != null) && (user.Identity != null))
            {
                authenticationType = user.Identity.AuthenticationType;
            }
            return authenticationType;
        }

        private string GetAuthType(IHttpAuthenticationContext authenticationContext)
        {
            string authenticationType = null;
            if (authenticationContext.LogonUserIdentity != null)
            {
                authenticationType = authenticationContext.LogonUserIdentity.AuthenticationType;
            }
            return authenticationType;
        }

        private static ServiceNameCollection GetDefaultSpnList(System.ServiceModel.HostNameComparisonMode hostNameComparisonMode, Uri listenUri)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            string hostName = null;
            string dnsSafeHost = listenUri.DnsSafeHost;
            switch (hostNameComparisonMode)
            {
                case System.ServiceModel.HostNameComparisonMode.StrongWildcard:
                case System.ServiceModel.HostNameComparisonMode.WeakWildcard:
                    hostName = Dns.GetHostEntry(string.Empty).HostName;
                    AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HOST/{0}", new object[] { hostName }));
                    AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HTTP/{0}", new object[] { hostName }));
                    break;

                case System.ServiceModel.HostNameComparisonMode.Exact:
                {
                    UriHostNameType hostNameType = listenUri.HostNameType;
                    if ((hostNameType != UriHostNameType.IPv4) && (hostNameType != UriHostNameType.IPv6))
                    {
                        if (listenUri.DnsSafeHost.Contains("."))
                        {
                            AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HOST/{0}", new object[] { dnsSafeHost }));
                            AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HTTP/{0}", new object[] { dnsSafeHost }));
                        }
                        else
                        {
                            hostName = Dns.GetHostEntry(string.Empty).HostName;
                            AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HOST/{0}", new object[] { dnsSafeHost }));
                            AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HTTP/{0}", new object[] { dnsSafeHost }));
                            AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HOST/{0}", new object[] { hostName }));
                            AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HTTP/{0}", new object[] { hostName }));
                        }
                        break;
                    }
                    hostName = Dns.GetHostEntry(string.Empty).HostName;
                    AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HOST/{0}", new object[] { hostName }));
                    AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HTTP/{0}", new object[] { hostName }));
                    break;
                }
            }
            AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HOST/{0}", new object[] { "localhost" }));
            AddSpn(list, string.Format(CultureInfo.InvariantCulture, "HTTP/{0}", new object[] { "localhost" }));
            return new ServiceNameCollection(list.Values);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T GetIdentityModelProperty<T>()
        {
            if (typeof(T) == typeof(EndpointIdentity))
            {
                if ((this.identity == null) && AuthenticationSchemesHelper.IsWindowsAuth(this.authenticationScheme))
                {
                    this.identity = System.ServiceModel.Security.SecurityUtils.CreateWindowsIdentity();
                }
                return (T) this.identity;
            }
            if ((typeof(T) == typeof(ILogonTokenCacheManager)) && (this.userNameTokenAuthenticator != null))
            {
                ILogonTokenCacheManager userNameTokenAuthenticator = this.userNameTokenAuthenticator as ILogonTokenCacheManager;
                if (userNameTokenAuthenticator != null)
                {
                    return (T) userNameTokenAuthenticator;
                }
            }
            return default(T);
        }

        internal override int GetMaxBufferSize()
        {
            return this.MaxBufferSize;
        }

        private static System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy GetPolicyWithDefaultSpnCollection(System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy policy, AuthenticationSchemes authenticationScheme, System.ServiceModel.HostNameComparisonMode hostNameComparisonMode, Uri listenUri, out bool usingDefaultSpnList)
        {
            if ((((policy.PolicyEnforcement != PolicyEnforcement.Never) && (policy.CustomServiceNames == null)) && ((policy.CustomChannelBinding == null) && (authenticationScheme != AuthenticationSchemes.Anonymous))) && string.Equals(listenUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                usingDefaultSpnList = true;
                return new System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy(policy.PolicyEnforcement, policy.ProtectionScenario, GetDefaultSpnList(hostNameComparisonMode, listenUri));
            }
            usingDefaultSpnList = false;
            return policy;
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(EndpointIdentity))
            {
                return (T) this.identity;
            }
            if (typeof(T) == typeof(ILogonTokenCacheManager))
            {
                object identityModelProperty = this.GetIdentityModelProperty<T>();
                if (identityModelProperty != null)
                {
                    return (T) identityModelProperty;
                }
            }
            else
            {
                if (typeof(T) == typeof(ISecurityCapabilities))
                {
                    return (T) this.securityCapabilities;
                }
                if (typeof(T) == typeof(System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy))
                {
                    return (T) this.extendedProtectionPolicy;
                }
            }
            return base.GetProperty<T>();
        }

        internal bool HttpContextReceived(HttpRequestContext context, Action callback)
        {
            bool flag = false;
            bool flag2 = false;
            try
            {
                if (!context.ProcessAuthentication())
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40027, System.ServiceModel.SR.GetString("TraceCodeHttpAuthFailed"), this);
                    }
                    flag2 = true;
                    return false;
                }
                try
                {
                    context.CreateMessage();
                }
                catch (ProtocolException exception)
                {
                    HttpStatusCode badRequest = HttpStatusCode.BadRequest;
                    string statusDescription = string.Empty;
                    if (exception.Data.Contains("System.ServiceModel.Channels.HttpInput.HttpStatusCode"))
                    {
                        badRequest = (HttpStatusCode) exception.Data["System.ServiceModel.Channels.HttpInput.HttpStatusCode"];
                        exception.Data.Remove("System.ServiceModel.Channels.HttpInput.HttpStatusCode");
                    }
                    if (exception.Data.Contains("System.ServiceModel.Channels.HttpInput.HttpStatusDescription"))
                    {
                        statusDescription = (string) exception.Data["System.ServiceModel.Channels.HttpInput.HttpStatusDescription"];
                        exception.Data.Remove("System.ServiceModel.Channels.HttpInput.HttpStatusDescription");
                    }
                    context.SendResponseAndClose(badRequest, statusDescription);
                    throw;
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    try
                    {
                        context.SendResponseAndClose(HttpStatusCode.BadRequest);
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        if (DiagnosticUtility.ShouldTraceError)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Error);
                        }
                    }
                    throw;
                }
                flag = true;
                this.acceptor.Enqueue(context, callback);
                flag2 = true;
            }
            catch (CommunicationException exception4)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                }
            }
            catch (XmlException exception5)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                }
            }
            catch (IOException exception6)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception6, TraceEventType.Information);
                }
            }
            catch (TimeoutException exception7)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception7, TraceEventType.Information);
                }
            }
            catch (Exception exception8)
            {
                if (Fx.IsFatal(exception8))
                {
                    throw;
                }
                if (!System.ServiceModel.Dispatcher.ExceptionHandler.HandleTransportExceptionHelper(exception8))
                {
                    throw;
                }
            }
            finally
            {
                if (!flag2)
                {
                    context.Abort();
                }
            }
            return flag;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeSecurityTokenAuthenticator()
        {
            ServiceCredentials credentialProvider = this.credentialProvider as ServiceCredentials;
            if (credentialProvider != null)
            {
                this.extractGroupsForWindowsAccounts = (this.AuthenticationScheme == AuthenticationSchemes.Basic) ? credentialProvider.UserNameAuthentication.IncludeWindowsGroups : credentialProvider.WindowsAuthentication.IncludeWindowsGroups;
                if (credentialProvider.UserNameAuthentication.UserNamePasswordValidationMode == UserNamePasswordValidationMode.Custom)
                {
                    this.userNameTokenAuthenticator = new CustomUserNameSecurityTokenAuthenticator(credentialProvider.UserNameAuthentication.GetUserNamePasswordValidator());
                }
                else if (credentialProvider.UserNameAuthentication.CacheLogonTokens)
                {
                    this.userNameTokenAuthenticator = new WindowsUserNameCachingSecurityTokenAuthenticator(this.extractGroupsForWindowsAccounts, credentialProvider.UserNameAuthentication.MaxCachedLogonTokens, credentialProvider.UserNameAuthentication.CachedLogonTokenLifetime);
                }
                else
                {
                    this.userNameTokenAuthenticator = new WindowsUserNameSecurityTokenAuthenticator(this.extractGroupsForWindowsAccounts);
                }
            }
            else
            {
                this.extractGroupsForWindowsAccounts = true;
                this.userNameTokenAuthenticator = new WindowsUserNameSecurityTokenAuthenticator(this.extractGroupsForWindowsAccounts);
            }
            this.windowsTokenAuthenticator = new WindowsSecurityTokenAuthenticator(this.extractGroupsForWindowsAccounts);
        }

        private bool IsAuthSchemeValid(string authType)
        {
            return AuthenticationSchemesHelper.DoesAuthTypeMatch(this.authenticationScheme, authType);
        }

        protected override void OnAbort()
        {
            if (this.IsAuthenticationRequired)
            {
                this.AbortUserNameTokenAuthenticator();
            }
            this.acceptor.Abort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ICommunicationObject[] objArray;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            ICommunicationObject userNameTokenAuthenticator = this.userNameTokenAuthenticator as ICommunicationObject;
            if (userNameTokenAuthenticator == null)
            {
                if (this.IsAuthenticationRequired)
                {
                    this.CloseUserNameTokenAuthenticator(helper.RemainingTime());
                }
                objArray = new ICommunicationObject[] { this.acceptor };
            }
            else
            {
                objArray = new ICommunicationObject[] { this.acceptor, userNameTokenAuthenticator };
            }
            return new ChainedCloseAsyncResult(helper.RemainingTime(), callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), objArray);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen), new ICommunicationObject[] { this.acceptor });
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.acceptor.BeginWaitForChannel(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.acceptor.Close(helper.RemainingTime());
            if (this.IsAuthenticationRequired)
            {
                this.CloseUserNameTokenAuthenticator(helper.RemainingTime());
            }
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.acceptor.EndWaitForChannel(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            this.acceptor.Open(helper.RemainingTime());
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            if (this.IsAuthenticationRequired)
            {
                this.InitializeSecurityTokenAuthenticator();
                this.identity = this.GetIdentityModelProperty<EndpointIdentity>();
            }
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.acceptor.WaitForChannel(timeout);
        }

        private SecurityMessageProperty ProcessAuthentication(HttpListenerBasicIdentity identity)
        {
            SecurityToken token = new UserNameSecurityToken(identity.Name, identity.Password);
            ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = this.userNameTokenAuthenticator.ValidateToken(token);
            return new SecurityMessageProperty { TransportToken = new SecurityTokenSpecification(token, tokenPolicies), ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies) };
        }

        public virtual SecurityMessageProperty ProcessAuthentication(HttpListenerContext listenerContext)
        {
            if (this.IsAuthenticationRequired)
            {
                return this.ProcessRequiredAuthentication(listenerContext);
            }
            return null;
        }

        public virtual SecurityMessageProperty ProcessAuthentication(IHttpAuthenticationContext authenticationContext)
        {
            SecurityMessageProperty property;
            if (!this.IsAuthenticationRequired)
            {
                return null;
            }
            try
            {
                property = this.ProcessAuthentication(authenticationContext.LogonUserIdentity, this.GetAuthType(authenticationContext));
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (AuditLevel.Failure == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                {
                    this.WriteAuditEvent(AuditLevel.Failure, (authenticationContext.LogonUserIdentity != null) ? authenticationContext.LogonUserIdentity.Name : string.Empty, exception);
                }
                throw;
            }
            if (AuditLevel.Success == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
            {
                this.WriteAuditEvent(AuditLevel.Success, (authenticationContext.LogonUserIdentity != null) ? authenticationContext.LogonUserIdentity.Name : string.Empty, null);
            }
            return property;
        }

        private SecurityMessageProperty ProcessAuthentication(WindowsIdentity identity, string authenticationType)
        {
            System.ServiceModel.Security.SecurityUtils.ValidateAnonymityConstraint(identity, false);
            SecurityToken token = new WindowsSecurityToken(identity, SecurityUniqueId.Create().Value, authenticationType);
            ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = this.windowsTokenAuthenticator.ValidateToken(token);
            return new SecurityMessageProperty { TransportToken = new SecurityTokenSpecification(token, tokenPolicies), ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies) };
        }

        private SecurityMessageProperty ProcessRequiredAuthentication(HttpListenerContext listenerContext)
        {
            SecurityMessageProperty property;
            HttpListenerBasicIdentity identity = null;
            WindowsIdentity identity2 = null;
            try
            {
                if (this.AuthenticationScheme == AuthenticationSchemes.Basic)
                {
                    identity = listenerContext.User.Identity as HttpListenerBasicIdentity;
                    property = this.ProcessAuthentication(identity);
                }
                else
                {
                    identity2 = listenerContext.User.Identity as WindowsIdentity;
                    property = this.ProcessAuthentication(identity2, this.GetAuthType(listenerContext));
                }
            }
            catch (Exception exception)
            {
                if (!Fx.IsFatal(exception) && (AuditLevel.Failure == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure)))
                {
                    this.WriteAuditEvent(AuditLevel.Failure, (identity != null) ? identity.Name : ((identity2 != null) ? identity2.Name : string.Empty), exception);
                }
                throw;
            }
            if (AuditLevel.Success == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
            {
                this.WriteAuditEvent(AuditLevel.Success, (identity != null) ? identity.Name : ((identity2 != null) ? identity2.Name : string.Empty), null);
            }
            return property;
        }

        protected override bool TryGetTransportManagerRegistration(System.ServiceModel.HostNameComparisonMode hostNameComparisonMode, out ITransportManagerRegistration registration)
        {
            if (this.TransportManagerTable.TryLookupUri(this.Uri, hostNameComparisonMode, out registration))
            {
                HttpTransportManager manager = registration as HttpTransportManager;
                if ((manager != null) && manager.IsHosted)
                {
                    return true;
                }
                if (registration.ListenUri.Segments.Length >= base.BaseUri.Segments.Length)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual HttpStatusCode ValidateAuthentication(HttpListenerContext listenerContext)
        {
            HttpStatusCode oK = HttpStatusCode.OK;
            if (this.IsAuthenticationRequired)
            {
                string authType = this.GetAuthType(listenerContext);
                oK = this.ValidateAuthentication(authType);
            }
            return oK;
        }

        public virtual HttpStatusCode ValidateAuthentication(IHttpAuthenticationContext authenticationContext)
        {
            HttpStatusCode oK = HttpStatusCode.OK;
            if (this.IsAuthenticationRequired)
            {
                string authType = this.GetAuthType(authenticationContext);
                oK = this.ValidateAuthentication(authType);
            }
            if (((oK == HttpStatusCode.OK) && (this.ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Always)) && !authenticationContext.IISSupportsExtendedProtection)
            {
                Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PlatformNotSupportedException(System.ServiceModel.SR.GetString("ExtendedProtectionNotSupported")));
                this.WriteAuditEvent(AuditLevel.Failure, string.Empty, exception);
                oK = HttpStatusCode.Unauthorized;
            }
            return oK;
        }

        private HttpStatusCode ValidateAuthentication(string authType)
        {
            if (this.IsAuthSchemeValid(authType))
            {
                return HttpStatusCode.OK;
            }
            if (AuditLevel.Failure == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
            {
                string message = System.ServiceModel.SR.GetString("HttpAuthenticationFailed", new object[] { this.AuthenticationScheme, HttpStatusCode.Unauthorized });
                Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(message));
                this.WriteAuditEvent(AuditLevel.Failure, string.Empty, exception);
            }
            return HttpStatusCode.Unauthorized;
        }

        protected void WriteAuditEvent(AuditLevel auditLevel, string primaryIdentity, Exception exception)
        {
            try
            {
                if (auditLevel == AuditLevel.Success)
                {
                    SecurityAuditHelper.WriteTransportAuthenticationSuccessEvent(base.AuditBehavior.AuditLogLocation, base.AuditBehavior.SuppressAuditFailure, null, this.Uri, primaryIdentity);
                }
                else
                {
                    SecurityAuditHelper.WriteTransportAuthenticationFailureEvent(base.AuditBehavior.AuditLogLocation, base.AuditBehavior.SuppressAuditFailure, null, this.Uri, primaryIdentity, exception);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2) || (auditLevel == AuditLevel.Success))
                {
                    throw;
                }
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
            }
        }

        internal HttpAnonymousUriPrefixMatcher AnonymousUriPrefixMatcher
        {
            get
            {
                return this.anonymousUriPrefixMatcher;
            }
        }

        public AuthenticationSchemes AuthenticationScheme
        {
            get
            {
                return this.authenticationScheme;
            }
        }

        public System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
        }

        public bool ExtractGroupsForWindowsAccounts
        {
            get
            {
                return this.extractGroupsForWindowsAccounts;
            }
        }

        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return base.HostNameComparisonModeInternal;
            }
        }

        private bool IsAuthenticationRequired
        {
            get
            {
                return (this.AuthenticationScheme != AuthenticationSchemes.Anonymous);
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

        public string Method
        {
            get
            {
                return this.method;
            }
        }

        public string Realm
        {
            get
            {
                return this.realm;
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttp;
            }
        }

        internal static UriPrefixTable<ITransportManagerRegistration> StaticTransportManagerTable
        {
            get
            {
                return transportManagerTable;
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

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        public bool UnsafeConnectionNtlmAuthentication
        {
            get
            {
                return this.unsafeConnectionNtlmAuthentication;
            }
        }

        internal interface IHttpAuthenticationContext
        {
            TraceRecord CreateTraceRecord();
            X509Certificate2 GetClientCertificate(out bool isValidCertificate);

            bool IISSupportsExtendedProtection { get; }

            WindowsIdentity LogonUserIdentity { get; }
        }
    }
}

