namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    internal class WindowsStreamSecurityUpgradeProvider : StreamSecurityUpgradeProvider
    {
        private bool extractGroupsForWindowsAccounts;
        private EndpointIdentity identity;
        private System.ServiceModel.Security.IdentityVerifier identityVerifier;
        private bool isClient;
        private Uri listenUri;
        private System.Net.Security.ProtectionLevel protectionLevel;
        private string scheme;
        private SecurityTokenManager securityTokenManager;
        private NetworkCredential serverCredential;

        public WindowsStreamSecurityUpgradeProvider(WindowsStreamSecurityBindingElement bindingElement, BindingContext context, bool isClient) : base(context.Binding)
        {
            this.extractGroupsForWindowsAccounts = true;
            this.protectionLevel = bindingElement.ProtectionLevel;
            this.scheme = context.Binding.Scheme;
            this.isClient = isClient;
            this.listenUri = TransportSecurityHelpers.GetListenUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
            SecurityCredentialsManager manager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (manager == null)
            {
                if (isClient)
                {
                    manager = ClientCredentials.CreateDefaultCredentials();
                }
                else
                {
                    manager = ServiceCredentials.CreateDefaultCredentials();
                }
            }
            this.securityTokenManager = manager.CreateSecurityTokenManager();
        }

        public override StreamUpgradeAcceptor CreateUpgradeAcceptor()
        {
            base.ThrowIfDisposedOrNotOpen();
            return new WindowsStreamSecurityUpgradeAcceptor(this);
        }

        public override StreamUpgradeInitiator CreateUpgradeInitiator(EndpointAddress remoteAddress, Uri via)
        {
            base.ThrowIfDisposedOrNotOpen();
            return new WindowsStreamSecurityUpgradeInitiator(this, remoteAddress, via);
        }

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpen(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            if (!this.isClient)
            {
                SecurityTokenRequirement sspiTokenRequirement = TransportSecurityHelpers.CreateSspiTokenRequirement(this.Scheme, this.listenUri);
                this.serverCredential = TransportSecurityHelpers.GetSspiCredential(this.securityTokenManager, sspiTokenRequirement, timeout, out this.extractGroupsForWindowsAccounts);
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            if (this.identityVerifier == null)
            {
                this.identityVerifier = System.ServiceModel.Security.IdentityVerifier.CreateDefault();
            }
            if (this.serverCredential == null)
            {
                this.serverCredential = CredentialCache.DefaultNetworkCredentials;
            }
        }

        internal bool ExtractGroupsForWindowsAccounts
        {
            get
            {
                return this.extractGroupsForWindowsAccounts;
            }
        }

        public override EndpointIdentity Identity
        {
            get
            {
                if ((this.serverCredential != null) && (this.identity == null))
                {
                    lock (base.ThisLock)
                    {
                        if (this.identity == null)
                        {
                            this.identity = System.ServiceModel.Security.SecurityUtils.CreateWindowsIdentity(this.serverCredential);
                        }
                    }
                }
                return this.identity;
            }
        }

        internal System.ServiceModel.Security.IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
        }

        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
        }

        public string Scheme
        {
            get
            {
                return this.scheme;
            }
        }

        private NetworkCredential ServerCredential
        {
            get
            {
                return this.serverCredential;
            }
        }

        private class WindowsStreamSecurityUpgradeAcceptor : StreamSecurityUpgradeAcceptorBase
        {
            private SecurityMessageProperty clientSecurity;
            private WindowsStreamSecurityUpgradeProvider parent;

            public WindowsStreamSecurityUpgradeAcceptor(WindowsStreamSecurityUpgradeProvider parent) : base("application/negotiate")
            {
                this.parent = parent;
                this.clientSecurity = new SecurityMessageProperty();
            }

            private SecurityMessageProperty CreateClientSecurity(NegotiateStream negotiateStream, bool extractGroupsForWindowsAccounts)
            {
                WindowsIdentity remoteIdentity = (WindowsIdentity) negotiateStream.RemoteIdentity;
                System.ServiceModel.Security.SecurityUtils.ValidateAnonymityConstraint(remoteIdentity, false);
                WindowsSecurityTokenAuthenticator authenticator = new WindowsSecurityTokenAuthenticator(extractGroupsForWindowsAccounts);
                SecurityToken token = new WindowsSecurityToken(remoteIdentity, SecurityUniqueId.Create().Value, remoteIdentity.AuthenticationType);
                ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = authenticator.ValidateToken(token);
                this.clientSecurity = new SecurityMessageProperty();
                this.clientSecurity.TransportToken = new SecurityTokenSpecification(token, tokenPolicies);
                this.clientSecurity.ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies);
                return this.clientSecurity;
            }

            public override SecurityMessageProperty GetRemoteSecurity()
            {
                if (this.clientSecurity.TransportToken != null)
                {
                    return this.clientSecurity;
                }
                return base.GetRemoteSecurity();
            }

            protected override Stream OnAcceptUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
            {
                NegotiateStream negotiateStream = new NegotiateStream(stream);
                try
                {
                    negotiateStream.AuthenticateAsServer(this.parent.ServerCredential, this.parent.ProtectionLevel, TokenImpersonationLevel.Identification);
                }
                catch (AuthenticationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message, exception));
                }
                catch (IOException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NegotiationFailedIO", new object[] { exception2.Message }), exception2));
                }
                remoteSecurity = this.CreateClientSecurity(negotiateStream, this.parent.ExtractGroupsForWindowsAccounts);
                return negotiateStream;
            }

            protected override IAsyncResult OnBeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state)
            {
                AcceptUpgradeAsyncResult result = new AcceptUpgradeAsyncResult(this, callback, state);
                result.Begin(stream);
                return result;
            }

            protected override Stream OnEndAcceptUpgrade(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
            {
                return StreamSecurityUpgradeAcceptorAsyncResult.End(result, out remoteSecurity);
            }

            private class AcceptUpgradeAsyncResult : StreamSecurityUpgradeAcceptorAsyncResult
            {
                private WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeAcceptor acceptor;
                private NegotiateStream negotiateStream;

                public AcceptUpgradeAsyncResult(WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeAcceptor acceptor, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.acceptor = acceptor;
                }

                protected override IAsyncResult OnBegin(Stream stream, AsyncCallback callback)
                {
                    this.negotiateStream = new NegotiateStream(stream);
                    return this.negotiateStream.BeginAuthenticateAsServer(this.acceptor.parent.ServerCredential, this.acceptor.parent.ProtectionLevel, TokenImpersonationLevel.Identification, callback, this);
                }

                protected override Stream OnCompleteAuthenticateAsServer(IAsyncResult result)
                {
                    this.negotiateStream.EndAuthenticateAsServer(result);
                    return this.negotiateStream;
                }

                protected override SecurityMessageProperty ValidateCreateSecurity()
                {
                    return this.acceptor.CreateClientSecurity(this.negotiateStream, this.acceptor.parent.ExtractGroupsForWindowsAccounts);
                }
            }
        }

        private class WindowsStreamSecurityUpgradeInitiator : StreamSecurityUpgradeInitiatorBase
        {
            private bool allowNtlm;
            private SspiSecurityTokenProvider clientTokenProvider;
            private NetworkCredential credential;
            private IdentityVerifier identityVerifier;
            private TokenImpersonationLevel impersonationLevel;
            private WindowsStreamSecurityUpgradeProvider parent;

            public WindowsStreamSecurityUpgradeInitiator(WindowsStreamSecurityUpgradeProvider parent, EndpointAddress remoteAddress, Uri via) : base("application/negotiate", remoteAddress, via)
            {
                this.parent = parent;
                this.clientTokenProvider = TransportSecurityHelpers.GetSspiTokenProvider(parent.securityTokenManager, remoteAddress, via, parent.Scheme, out this.identityVerifier);
            }

            private IAsyncResult BaseBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.BeginClose(timeout, callback, state);
            }

            private IAsyncResult BaseBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.BeginOpen(timeout, callback, state);
            }

            private void BaseEndClose(IAsyncResult result)
            {
                base.EndClose(result);
            }

            private void BaseEndOpen(IAsyncResult result)
            {
                base.EndOpen(result);
            }

            internal override IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseAsyncResult(this, timeout, callback, state);
            }

            internal override IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new OpenAsyncResult(this, timeout, callback, state);
            }

            internal override void Close(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.Close(helper.RemainingTime());
                System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.clientTokenProvider, helper.RemainingTime());
            }

            private static SecurityMessageProperty CreateServerSecurity(NegotiateStream negotiateStream)
            {
                GenericIdentity remoteIdentity = (GenericIdentity) negotiateStream.RemoteIdentity;
                string name = remoteIdentity.Name;
                if ((name != null) && (name.Length > 0))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = System.ServiceModel.Security.SecurityUtils.CreatePrincipalNameAuthorizationPolicies(name);
                    return new SecurityMessageProperty { TransportToken = new SecurityTokenSpecification(null, tokenPolicies), ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies) };
                }
                return null;
            }

            internal override void EndClose(IAsyncResult result)
            {
                CloseAsyncResult.End(result);
            }

            internal override void EndOpen(IAsyncResult result)
            {
                OpenAsyncResult.End(result);
            }

            private void InitiateUpgradePrepare(Stream stream, out NegotiateStream negotiateStream, out string targetName, out EndpointIdentity identity)
            {
                negotiateStream = new NegotiateStream(stream);
                targetName = string.Empty;
                identity = null;
                if (this.parent.IdentityVerifier.TryGetIdentity(base.RemoteAddress, base.Via, out identity))
                {
                    targetName = System.ServiceModel.Security.SecurityUtils.GetSpnFromIdentity(identity, base.RemoteAddress);
                }
                else
                {
                    targetName = System.ServiceModel.Security.SecurityUtils.GetSpnFromTarget(base.RemoteAddress);
                }
            }

            protected override IAsyncResult OnBeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state)
            {
                InitiateUpgradeAsyncResult result = new InitiateUpgradeAsyncResult(this, callback, state);
                result.Begin(stream);
                return result;
            }

            protected override Stream OnEndInitiateUpgrade(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
            {
                return StreamSecurityUpgradeInitiatorAsyncResult.End(result, out remoteSecurity);
            }

            protected override Stream OnInitiateUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
            {
                NegotiateStream stream2;
                string str;
                EndpointIdentity identity;
                this.InitiateUpgradePrepare(stream, out stream2, out str, out identity);
                try
                {
                    stream2.AuthenticateAsClient(this.credential, str, this.parent.ProtectionLevel, this.impersonationLevel);
                }
                catch (AuthenticationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message, exception));
                }
                catch (IOException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NegotiationFailedIO", new object[] { exception2.Message }), exception2));
                }
                remoteSecurity = CreateServerSecurity(stream2);
                this.ValidateMutualAuth(identity, stream2, remoteSecurity, this.allowNtlm);
                return stream2;
            }

            internal override void Open(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.Open(helper.RemainingTime());
                System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.clientTokenProvider, helper.RemainingTime());
                this.credential = TransportSecurityHelpers.GetSspiCredential(this.clientTokenProvider, helper.RemainingTime(), out this.impersonationLevel, out this.allowNtlm);
            }

            private void ValidateMutualAuth(EndpointIdentity expectedIdentity, NegotiateStream negotiateStream, SecurityMessageProperty remoteSecurity, bool allowNtlm)
            {
                if (negotiateStream.IsMutuallyAuthenticated)
                {
                    if ((expectedIdentity != null) && !this.parent.IdentityVerifier.CheckAccess(expectedIdentity, remoteSecurity.ServiceSecurityContext.AuthorizationContext))
                    {
                        string identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(remoteSecurity.ServiceSecurityContext.AuthorizationContext);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("RemoteIdentityFailedVerification", new object[] { identityNamesFromContext })));
                    }
                }
                else if (!allowNtlm)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("StreamMutualAuthNotSatisfied")));
                }
            }

            private class CloseAsyncResult : AsyncResult
            {
                private AsyncCallback onBaseClose;
                private AsyncCallback onCloseTokenProvider;
                private WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator parent;
                private TimeoutHelper timeoutHelper;

                public CloseAsyncResult(WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.parent = parent;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.onBaseClose = Fx.ThunkCallback(new AsyncCallback(this.OnBaseClose));
                    this.onCloseTokenProvider = Fx.ThunkCallback(new AsyncCallback(this.OnCloseTokenProvider));
                    IAsyncResult result = parent.BaseBeginClose(this.timeoutHelper.RemainingTime(), this.onBaseClose, this);
                    if (result.CompletedSynchronously && this.HandleBaseCloseComplete(result))
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator.CloseAsyncResult>(result);
                }

                private bool HandleBaseCloseComplete(IAsyncResult result)
                {
                    this.parent.BaseEndClose(result);
                    IAsyncResult result2 = System.ServiceModel.Security.SecurityUtils.BeginCloseTokenProviderIfRequired(this.parent.clientTokenProvider, this.timeoutHelper.RemainingTime(), this.onCloseTokenProvider, this);
                    if (!result2.CompletedSynchronously)
                    {
                        return false;
                    }
                    System.ServiceModel.Security.SecurityUtils.EndCloseTokenProviderIfRequired(result2);
                    return true;
                }

                private void OnBaseClose(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        bool flag = false;
                        try
                        {
                            flag = this.HandleBaseCloseComplete(result);
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
                            base.Complete(false, exception);
                        }
                    }
                }

                private void OnCloseTokenProvider(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        try
                        {
                            System.ServiceModel.Security.SecurityUtils.EndCloseTokenProviderIfRequired(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        base.Complete(false, exception);
                    }
                }
            }

            private class InitiateUpgradeAsyncResult : StreamSecurityUpgradeInitiatorAsyncResult
            {
                private EndpointIdentity expectedIdentity;
                private WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator initiator;
                private NegotiateStream negotiateStream;

                public InitiateUpgradeAsyncResult(WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator initiator, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.initiator = initiator;
                }

                protected override IAsyncResult OnBeginAuthenticateAsClient(Stream stream, AsyncCallback callback)
                {
                    string str;
                    this.initiator.InitiateUpgradePrepare(stream, out this.negotiateStream, out str, out this.expectedIdentity);
                    return this.negotiateStream.BeginAuthenticateAsClient(this.initiator.credential, str, this.initiator.parent.ProtectionLevel, this.initiator.impersonationLevel, callback, this);
                }

                protected override Stream OnCompleteAuthenticateAsClient(IAsyncResult result)
                {
                    this.negotiateStream.EndAuthenticateAsClient(result);
                    return this.negotiateStream;
                }

                protected override SecurityMessageProperty ValidateCreateSecurity()
                {
                    SecurityMessageProperty remoteSecurity = WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator.CreateServerSecurity(this.negotiateStream);
                    this.initiator.ValidateMutualAuth(this.expectedIdentity, this.negotiateStream, remoteSecurity, this.initiator.allowNtlm);
                    return remoteSecurity;
                }
            }

            private class OpenAsyncResult : AsyncResult
            {
                private AsyncCallback onBaseOpen;
                private AsyncCallback onGetSspiCredential;
                private AsyncCallback onOpenTokenProvider;
                private WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator parent;
                private TimeoutHelper timeoutHelper;

                public OpenAsyncResult(WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.parent = parent;
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    this.onBaseOpen = Fx.ThunkCallback(new AsyncCallback(this.OnBaseOpen));
                    this.onGetSspiCredential = Fx.ThunkCallback(new AsyncCallback(this.OnGetSspiCredential));
                    this.onOpenTokenProvider = Fx.ThunkCallback(new AsyncCallback(this.OnOpenTokenProvider));
                    IAsyncResult result = parent.BaseBeginOpen(helper.RemainingTime(), this.onBaseOpen, this);
                    if (result.CompletedSynchronously && this.HandleBaseOpenComplete(result))
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<WindowsStreamSecurityUpgradeProvider.WindowsStreamSecurityUpgradeInitiator.OpenAsyncResult>(result);
                }

                private bool HandleBaseOpenComplete(IAsyncResult result)
                {
                    this.parent.BaseEndOpen(result);
                    IAsyncResult result2 = System.ServiceModel.Security.SecurityUtils.BeginOpenTokenProviderIfRequired(this.parent.clientTokenProvider, this.timeoutHelper.RemainingTime(), this.onOpenTokenProvider, this);
                    if (!result2.CompletedSynchronously)
                    {
                        return false;
                    }
                    return this.HandleOpenTokenProviderComplete(result2);
                }

                private bool HandleGetSspiCredentialComplete(IAsyncResult result)
                {
                    this.parent.credential = TransportSecurityHelpers.EndGetSspiCredential(result, out this.parent.impersonationLevel, out this.parent.allowNtlm);
                    return true;
                }

                private bool HandleOpenTokenProviderComplete(IAsyncResult result)
                {
                    System.ServiceModel.Security.SecurityUtils.EndOpenTokenProviderIfRequired(result);
                    IAsyncResult result2 = TransportSecurityHelpers.BeginGetSspiCredential(this.parent.clientTokenProvider, this.timeoutHelper.RemainingTime(), this.onGetSspiCredential, this);
                    if (!result2.CompletedSynchronously)
                    {
                        return false;
                    }
                    return this.HandleGetSspiCredentialComplete(result2);
                }

                private void OnBaseOpen(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        bool flag = false;
                        try
                        {
                            flag = this.HandleBaseOpenComplete(result);
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
                            base.Complete(false, exception);
                        }
                    }
                }

                private void OnGetSspiCredential(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        bool flag = false;
                        try
                        {
                            flag = this.HandleGetSspiCredentialComplete(result);
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
                            base.Complete(false, exception);
                        }
                    }
                }

                private void OnOpenTokenProvider(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        bool flag = false;
                        try
                        {
                            flag = this.HandleOpenTokenProviderComplete(result);
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
                            base.Complete(false, exception);
                        }
                    }
                }
            }
        }
    }
}

