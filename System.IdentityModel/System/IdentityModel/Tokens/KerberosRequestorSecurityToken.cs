namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;

    public class KerberosRequestorSecurityToken : SecurityToken
    {
        private byte[] apreq;
        private DateTime effectiveTime;
        private DateTime expirationTime;
        private string id;
        private ReadOnlyCollection<System.IdentityModel.Tokens.SecurityKey> securityKeys;
        private readonly string servicePrincipalName;
        private SymmetricSecurityKey symmetricSecurityKey;

        public KerberosRequestorSecurityToken(string servicePrincipalName) : this(servicePrincipalName, TokenImpersonationLevel.Impersonation, null, SecurityUniqueId.Create().Value, null)
        {
        }

        public KerberosRequestorSecurityToken(string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential, string id) : this(servicePrincipalName, tokenImpersonationLevel, networkCredential, id, null, null)
        {
        }

        internal KerberosRequestorSecurityToken(string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential, string id, ChannelBinding channelBinding) : this(servicePrincipalName, tokenImpersonationLevel, networkCredential, id, null, channelBinding)
        {
        }

        internal KerberosRequestorSecurityToken(string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential, string id, System.IdentityModel.SafeFreeCredentials credentialsHandle, ChannelBinding channelBinding)
        {
            if (servicePrincipalName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("servicePrincipalName");
            }
            if ((tokenImpersonationLevel != TokenImpersonationLevel.Identification) && (tokenImpersonationLevel != TokenImpersonationLevel.Impersonation))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenImpersonationLevel", System.IdentityModel.SR.GetString("ImpersonationLevelNotSupported", new object[] { tokenImpersonationLevel })));
            }
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }
            this.servicePrincipalName = servicePrincipalName;
            if (((networkCredential != null) && (networkCredential != CredentialCache.DefaultNetworkCredentials)) && string.IsNullOrEmpty(networkCredential.UserName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("ProvidedNetworkCredentialsForKerberosHasInvalidUserName"));
            }
            this.id = id;
            try
            {
                this.Initialize(tokenImpersonationLevel, networkCredential, credentialsHandle, channelBinding);
            }
            catch (Win32Exception exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("UnableToCreateKerberosCredentials"), exception));
            }
            catch (SecurityTokenException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("UnableToCreateKerberosCredentials"), exception2));
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
                return (new KerberosTicketHashKeyIdentifierClause(CryptoHelper.ComputeHash(this.apreq), false, null, 0) as T);
            }
            return base.CreateKeyIdentifierClause<T>();
        }

        public byte[] GetRequest()
        {
            return System.IdentityModel.SecurityUtils.CloneBuffer(this.apreq);
        }

        private void Initialize(TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential, System.IdentityModel.SafeFreeCredentials credentialsHandle, ChannelBinding channelBinding)
        {
            bool flag = false;
            System.IdentityModel.SafeDeleteContext context = null;
            try
            {
                if (credentialsHandle == null)
                {
                    if ((networkCredential == null) || (networkCredential == CredentialCache.DefaultNetworkCredentials))
                    {
                        credentialsHandle = SspiWrapper.AcquireDefaultCredential("Kerberos", System.IdentityModel.CredentialUse.Outbound, new string[0]);
                    }
                    else
                    {
                        AuthIdentityEx authdata = new AuthIdentityEx(networkCredential.UserName, networkCredential.Password, networkCredential.Domain, new string[0]);
                        credentialsHandle = SspiWrapper.AcquireCredentialsHandle("Kerberos", System.IdentityModel.CredentialUse.Outbound, ref authdata);
                    }
                    flag = true;
                }
                SspiContextFlags inFlags = SspiContextFlags.AllocateMemory | SspiContextFlags.Confidentiality | SspiContextFlags.SequenceDetect | SspiContextFlags.ReplayDetect;
                if (tokenImpersonationLevel == TokenImpersonationLevel.Identification)
                {
                    inFlags |= SspiContextFlags.InitIdentify;
                }
                SspiContextFlags zero = SspiContextFlags.Zero;
                System.IdentityModel.SecurityBuffer inputBuffer = null;
                if (channelBinding != null)
                {
                    inputBuffer = new System.IdentityModel.SecurityBuffer(channelBinding);
                }
                System.IdentityModel.SecurityBuffer outputBuffer = new System.IdentityModel.SecurityBuffer(0, System.IdentityModel.BufferType.Token);
                int error = SspiWrapper.InitializeSecurityContext(credentialsHandle, ref context, this.servicePrincipalName, inFlags, System.IdentityModel.Endianness.Native, inputBuffer, outputBuffer, ref zero);
                switch (error)
                {
                    case 0:
                        break;

                    case 0x90312:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("KerberosMultilegsNotSupported"), new Win32Exception(error)));

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("FailInitializeSecurityContext"), new Win32Exception(error)));
                }
                this.apreq = outputBuffer.token;
                LifeSpan span = (LifeSpan) SspiWrapper.QueryContextAttributes(context, System.IdentityModel.ContextAttribute.Lifespan);
                this.effectiveTime = span.EffectiveTimeUtc;
                this.expirationTime = span.ExpiryTimeUtc;
                SecuritySessionKeyClass class2 = (SecuritySessionKeyClass) SspiWrapper.QueryContextAttributes(context, System.IdentityModel.ContextAttribute.SessionKey);
                this.symmetricSecurityKey = new InMemorySymmetricSecurityKey(class2.SessionKey);
            }
            finally
            {
                if (context != null)
                {
                    context.Close();
                }
                if (flag && (credentialsHandle != null))
                {
                    credentialsHandle.Close();
                }
            }
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            KerberosTicketHashKeyIdentifierClause clause = keyIdentifierClause as KerberosTicketHashKeyIdentifierClause;
            if (clause != null)
            {
                return clause.Matches(CryptoHelper.ComputeHash(this.apreq));
            }
            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public SymmetricSecurityKey SecurityKey
        {
            get
            {
                return this.symmetricSecurityKey;
            }
        }

        public override ReadOnlyCollection<System.IdentityModel.Tokens.SecurityKey> SecurityKeys
        {
            get
            {
                if (this.securityKeys == null)
                {
                    this.securityKeys = new List<System.IdentityModel.Tokens.SecurityKey>(1) { this.symmetricSecurityKey }.AsReadOnly();
                }
                return this.securityKeys;
            }
        }

        public string ServicePrincipalName
        {
            get
            {
                return this.servicePrincipalName;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                return this.effectiveTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                return this.expirationTime;
            }
        }
    }
}

