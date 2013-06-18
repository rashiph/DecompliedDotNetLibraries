namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;

    public class KerberosReceiverSecurityToken : WindowsSecurityToken
    {
        private ChannelBinding channelBinding;
        private ExtendedProtectionPolicy extendedProtectionPolicy;
        private string id;
        private bool isAuthenticated;
        private byte[] request;
        private ReadOnlyCollection<System.IdentityModel.Tokens.SecurityKey> securityKeys;
        private SymmetricSecurityKey symmetricSecurityKey;
        private string valueTypeUri;

        public KerberosReceiverSecurityToken(byte[] request) : this(request, SecurityUniqueId.Create().Value)
        {
        }

        public KerberosReceiverSecurityToken(byte[] request, string id) : this(request, id, true, null)
        {
        }

        public KerberosReceiverSecurityToken(byte[] request, string id, string valueTypeUri) : this(request, id, true, valueTypeUri)
        {
        }

        internal KerberosReceiverSecurityToken(byte[] request, string id, bool doAuthenticate, string valueTypeUri) : this(request, id, doAuthenticate, valueTypeUri, null, null)
        {
        }

        internal KerberosReceiverSecurityToken(byte[] request, string id, bool doAuthenticate, string valueTypeUri, ChannelBinding channelBinding, ExtendedProtectionPolicy extendedProtectionPolicy)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("request"));
            }
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("id"));
            }
            this.id = id;
            this.request = request;
            this.valueTypeUri = valueTypeUri;
            this.channelBinding = channelBinding;
            this.extendedProtectionPolicy = extendedProtectionPolicy;
            if (doAuthenticate)
            {
                this.Initialize(null, channelBinding, extendedProtectionPolicy);
            }
        }

        public override bool CanCreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            return ((typeof(T) == typeof(KerberosTicketHashKeyIdentifierClause)) || base.CanCreateKeyIdentifierClause<T>());
        }

        public override T CreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            if (typeof(T) == typeof(KerberosTicketHashKeyIdentifierClause))
            {
                return (new KerberosTicketHashKeyIdentifierClause(CryptoHelper.ComputeHash(this.request), false, null, 0) as T);
            }
            return base.CreateKeyIdentifierClause<T>();
        }

        public byte[] GetRequest()
        {
            return System.IdentityModel.SecurityUtils.CloneBuffer(this.request);
        }

        internal void Initialize(SafeFreeCredentials credentialsHandle, ChannelBinding channelBinding, ExtendedProtectionPolicy extendedProtectionPolicy)
        {
            if (!this.isAuthenticated)
            {
                bool flag = false;
                SafeDeleteContext refContext = null;
                SafeCloseHandle token = null;
                byte[] request = this.request;
                try
                {
                    if (credentialsHandle == null)
                    {
                        credentialsHandle = SspiWrapper.AcquireDefaultCredential("Kerberos", CredentialUse.Inbound, new string[0]);
                        flag = true;
                    }
                    SspiContextFlags inFlags = SspiContextFlags.AllocateMemory | SspiContextFlags.Confidentiality | SspiContextFlags.SequenceDetect | SspiContextFlags.ReplayDetect;
                    ExtendedProtectionPolicyHelper helper = new ExtendedProtectionPolicyHelper(channelBinding, extendedProtectionPolicy);
                    if (((helper.PolicyEnforcement == PolicyEnforcement.Always) && (helper.ChannelBinding == null)) && (helper.ProtectionScenario != ProtectionScenario.TrustedProxy))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SecurityChannelBindingMissing")));
                    }
                    if (helper.PolicyEnforcement == PolicyEnforcement.WhenSupported)
                    {
                        inFlags |= SspiContextFlags.ChannelBindingAllowMissingBindings;
                    }
                    if (helper.ProtectionScenario == ProtectionScenario.TrustedProxy)
                    {
                        inFlags |= SspiContextFlags.ChannelBindingProxyBindings;
                    }
                    SspiContextFlags zero = SspiContextFlags.Zero;
                    SecurityBuffer outputBuffer = new SecurityBuffer(0, BufferType.Token);
                    List<SecurityBuffer> list = new List<SecurityBuffer>(2) {
                        new SecurityBuffer(request, 2)
                    };
                    if (helper.ShouldAddChannelBindingToASC())
                    {
                        list.Add(new SecurityBuffer(helper.ChannelBinding));
                    }
                    SecurityBuffer[] inputBuffers = null;
                    if (list.Count > 0)
                    {
                        inputBuffers = list.ToArray();
                    }
                    int error = SspiWrapper.AcceptSecurityContext(credentialsHandle, ref refContext, inFlags, Endianness.Native, inputBuffers, outputBuffer, ref zero);
                    switch (error)
                    {
                        case 0:
                            break;

                        case 0x90312:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("KerberosMultilegsNotSupported"), new Win32Exception(error)));

                        case -2146893056:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("KerberosApReqInvalidOrOutOfMemory"), new Win32Exception(error)));

                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("FailAcceptSecurityContext"), new Win32Exception(error)));
                    }
                    LifeSpan span = (LifeSpan) SspiWrapper.QueryContextAttributes(refContext, ContextAttribute.Lifespan);
                    DateTime effectiveTimeUtc = span.EffectiveTimeUtc;
                    DateTime expiryTimeUtc = span.ExpiryTimeUtc;
                    SecuritySessionKeyClass class2 = (SecuritySessionKeyClass) SspiWrapper.QueryContextAttributes(refContext, ContextAttribute.SessionKey);
                    this.symmetricSecurityKey = new InMemorySymmetricSecurityKey(class2.SessionKey);
                    error = SspiWrapper.QuerySecurityContextToken(refContext, out token);
                    if (error != 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }
                    System.Security.Principal.WindowsIdentity windowsIdentity = new System.Security.Principal.WindowsIdentity(token.DangerousGetHandle(), "Kerberos");
                    base.Initialize(this.id, "Kerberos", effectiveTimeUtc, expiryTimeUtc, windowsIdentity, false);
                    this.isAuthenticated = true;
                }
                finally
                {
                    if (token != null)
                    {
                        token.Close();
                    }
                    if (refContext != null)
                    {
                        refContext.Close();
                    }
                    if (flag && (credentialsHandle != null))
                    {
                        credentialsHandle.Close();
                    }
                }
            }
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            KerberosTicketHashKeyIdentifierClause clause = keyIdentifierClause as KerberosTicketHashKeyIdentifierClause;
            if (clause != null)
            {
                return clause.Matches(CryptoHelper.ComputeHash(this.request));
            }
            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        public SymmetricSecurityKey SecurityKey
        {
            get
            {
                if (!this.isAuthenticated)
                {
                    this.Initialize(null, this.channelBinding, this.extendedProtectionPolicy);
                }
                return this.symmetricSecurityKey;
            }
        }

        public override ReadOnlyCollection<System.IdentityModel.Tokens.SecurityKey> SecurityKeys
        {
            get
            {
                if (this.securityKeys == null)
                {
                    this.securityKeys = new List<System.IdentityModel.Tokens.SecurityKey>(1) { this.SecurityKey }.AsReadOnly();
                }
                return this.securityKeys;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                if (!this.isAuthenticated)
                {
                    this.Initialize(null, this.channelBinding, this.extendedProtectionPolicy);
                }
                return base.ValidFrom;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                if (!this.isAuthenticated)
                {
                    this.Initialize(null, this.channelBinding, this.extendedProtectionPolicy);
                }
                return base.ValidTo;
            }
        }

        public string ValueTypeUri
        {
            get
            {
                return this.valueTypeUri;
            }
        }

        public override System.Security.Principal.WindowsIdentity WindowsIdentity
        {
            get
            {
                base.ThrowIfDisposed();
                if (!this.isAuthenticated)
                {
                    this.Initialize(null, this.channelBinding, this.extendedProtectionPolicy);
                }
                return base.WindowsIdentity;
            }
        }
    }
}

