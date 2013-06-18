namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.Net;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class SpnegoTokenProvider : SspiNegotiationTokenProvider
    {
        private TokenImpersonationLevel allowedImpersonationLevel;
        private bool allowNtlm;
        private bool authenticateServer;
        private ICredentials clientCredential;
        private System.IdentityModel.SafeFreeCredentials credentialsHandle;
        private System.ServiceModel.Security.IdentityVerifier identityVerifier;
        private bool interactiveNegoExLogonEnabled;
        private bool ownCredentialsHandle;

        public SpnegoTokenProvider(System.IdentityModel.SafeFreeCredentials credentialsHandle) : this(credentialsHandle, null)
        {
        }

        public SpnegoTokenProvider(System.IdentityModel.SafeFreeCredentials credentialsHandle, SecurityBindingElement securityBindingElement) : base(securityBindingElement)
        {
            this.allowedImpersonationLevel = TokenImpersonationLevel.Identification;
            this.identityVerifier = System.ServiceModel.Security.IdentityVerifier.CreateDefault();
            this.allowNtlm = true;
            this.authenticateServer = true;
            this.interactiveNegoExLogonEnabled = true;
            this.credentialsHandle = credentialsHandle;
        }

        protected override IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<SspiNegotiationTokenProviderState>(this.CreateNegotiationState(target, via, timeout), callback, state);
        }

        protected override SspiNegotiationTokenProviderState CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout)
        {
            string spnFromIdentity;
            string str2;
            base.EnsureEndpointAddressDoesNotRequireEncryption(target);
            EndpointIdentity identity = null;
            if (this.identityVerifier == null)
            {
                identity = target.Identity;
            }
            else
            {
                this.identityVerifier.TryGetIdentity(target, out identity);
            }
            if (this.AuthenticateServer || !this.AllowNtlm)
            {
                spnFromIdentity = System.ServiceModel.Security.SecurityUtils.GetSpnFromIdentity(identity, target);
            }
            else
            {
                Claim identityClaim = identity.IdentityClaim;
                if ((identityClaim != null) && ((identityClaim.ClaimType == ClaimTypes.Spn) || (identityClaim.ClaimType == ClaimTypes.Upn)))
                {
                    spnFromIdentity = identityClaim.Resource.ToString();
                }
                else
                {
                    spnFromIdentity = "host/" + target.Uri.DnsSafeHost;
                }
            }
            if (!this.allowNtlm && !System.ServiceModel.Security.SecurityUtils.IsOsGreaterThanXP())
            {
                str2 = "Kerberos";
            }
            else
            {
                str2 = "Negotiate";
            }
            return new SspiNegotiationTokenProviderState(new WindowsSspiNegotiation(str2, this.credentialsHandle, this.AllowedImpersonationLevel, spnFromIdentity, true, this.InteractiveNegoExLogonEnabled, this.allowNtlm));
        }

        protected override bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via)
        {
            return true;
        }

        protected override SspiNegotiationTokenProviderState EndCreateNegotiationState(IAsyncResult result)
        {
            return CompletedAsyncResult<SspiNegotiationTokenProviderState>.End(result);
        }

        private void FreeCredentialsHandle()
        {
            if (this.credentialsHandle != null)
            {
                if (this.ownCredentialsHandle)
                {
                    this.credentialsHandle.Close();
                }
                this.credentialsHandle = null;
            }
        }

        public override void OnAbort()
        {
            base.OnAbort();
            this.FreeCredentialsHandle();
        }

        public override void OnClose(TimeSpan timeout)
        {
            base.OnClose(timeout);
            this.FreeCredentialsHandle();
        }

        public override void OnOpening()
        {
            bool flag = System.ServiceModel.Security.SecurityUtils.IsOsGreaterThanXP();
            base.OnOpening();
            if (this.credentialsHandle == null)
            {
                string str;
                if (!this.allowNtlm && !flag)
                {
                    str = "Kerberos";
                }
                else
                {
                    str = "Negotiate";
                }
                NetworkCredential credential = null;
                if (this.clientCredential != null)
                {
                    credential = this.clientCredential.GetCredential(base.TargetAddress.Uri, str);
                }
                if (!this.allowNtlm && flag)
                {
                    this.credentialsHandle = System.ServiceModel.Security.SecurityUtils.GetCredentialsHandle(str, credential, false, new string[] { "!NTLM" });
                }
                else
                {
                    this.credentialsHandle = System.ServiceModel.Security.SecurityUtils.GetCredentialsHandle(str, credential, false, new string[0]);
                }
                this.ownCredentialsHandle = true;
            }
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation)
        {
            WindowsSspiNegotiation windowsNegotiation = (WindowsSspiNegotiation) sspiNegotiation;
            if (!windowsNegotiation.IsValidContext)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidSspiNegotiation")));
            }
            if (this.AuthenticateServer && !windowsNegotiation.IsMutualAuthFlag)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("CannotAuthenticateServer")));
            }
            SecurityTraceRecordHelper.TraceClientSpnego(windowsNegotiation);
            return System.ServiceModel.Security.SecurityUtils.CreatePrincipalNameAuthorizationPolicies(windowsNegotiation.ServicePrincipalName);
        }

        public TokenImpersonationLevel AllowedImpersonationLevel
        {
            get
            {
                return this.allowedImpersonationLevel;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                TokenImpersonationLevelHelper.Validate(value);
                if (value == TokenImpersonationLevel.None)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", string.Format(CultureInfo.InvariantCulture, System.ServiceModel.SR.GetString("SpnegoImpersonationLevelCannotBeSetToNone"), new object[0])));
                }
                this.allowedImpersonationLevel = value;
            }
        }

        public bool AllowNtlm
        {
            get
            {
                return this.allowNtlm;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.allowNtlm = value;
            }
        }

        public bool AuthenticateServer
        {
            get
            {
                return this.authenticateServer;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.authenticateServer = value;
            }
        }

        public ICredentials ClientCredential
        {
            get
            {
                return this.clientCredential;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.clientCredential = value;
            }
        }

        public System.ServiceModel.Security.IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.identityVerifier = value;
            }
        }

        public bool InteractiveNegoExLogonEnabled
        {
            get
            {
                return this.interactiveNegoExLogonEnabled;
            }
            set
            {
                this.interactiveNegoExLogonEnabled = value;
            }
        }

        public override XmlDictionaryString NegotiationValueType
        {
            get
            {
                return System.ServiceModel.XD.TrustApr2004Dictionary.SpnegoValueTypeUri;
            }
        }
    }
}

