namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.ServiceModel.Diagnostics;

    public class RsaSecurityToken : SecurityToken
    {
        private DateTime effectiveTime;
        private string id;
        private CspKeyContainerInfo keyContainerInfo;
        private RSA rsa;
        private GCHandle rsaHandle;
        private ReadOnlyCollection<SecurityKey> rsaKey;

        public RsaSecurityToken(RSA rsa) : this(rsa, SecurityUniqueId.Create().Value)
        {
        }

        public RsaSecurityToken(RSA rsa, string id)
        {
            if (rsa == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");
            }
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }
            this.rsa = rsa;
            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
            GC.SuppressFinalize(this);
        }

        private RsaSecurityToken(RSACryptoServiceProvider rsa, bool ownsRsa)
        {
            if (rsa == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");
            }
            this.rsa = rsa;
            this.id = SecurityUniqueId.Create().Value;
            this.effectiveTime = DateTime.UtcNow;
            if (ownsRsa)
            {
                this.keyContainerInfo = rsa.CspKeyContainerInfo;
                rsa.PersistKeyInCsp = true;
                this.rsaHandle = GCHandle.Alloc(rsa);
            }
            else
            {
                GC.SuppressFinalize(this);
            }
        }

        public override bool CanCreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            return (typeof(T) == typeof(RsaKeyIdentifierClause));
        }

        public override T CreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            if (typeof(T) != typeof(RsaKeyIdentifierClause))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("TokenDoesNotSupportKeyIdentifierClauseCreation", new object[] { base.GetType().Name, typeof(T).Name })));
            }
            return (T) new RsaKeyIdentifierClause(this.rsa);
        }

        internal static RsaSecurityToken CreateSafeRsaSecurityToken(int keySize)
        {
            RsaSecurityToken token;
            RSACryptoServiceProvider rsa = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                try
                {
                }
                finally
                {
                    rsa = new RSACryptoServiceProvider(keySize);
                }
                token = new RsaSecurityToken(rsa, true);
                rsa = null;
            }
            finally
            {
                if (rsa != null)
                {
                    rsa.Dispose();
                }
            }
            return token;
        }

        internal void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.rsaHandle.IsAllocated)
            {
                try
                {
                    System.IdentityModel.SafeProvHandle handle;
                    string keyContainerName = this.keyContainerInfo.KeyContainerName;
                    string providerName = this.keyContainerInfo.ProviderName;
                    uint providerType = (uint) this.keyContainerInfo.ProviderType;
                    this.rsa.Dispose();
                    if (!System.IdentityModel.NativeMethods.CryptAcquireContextW(out handle, keyContainerName, providerName, providerType, 0x10))
                    {
                        int error = Marshal.GetLastWin32Error();
                        try
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("FailedToDeleteKeyContainerFile"), new Win32Exception(error)));
                        }
                        catch (InvalidOperationException exception)
                        {
                            if (DiagnosticUtility.ShouldTraceWarning)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                            }
                        }
                    }
                    Utility.CloseInvalidOutSafeHandle(handle);
                }
                finally
                {
                    this.rsaHandle.Free();
                }
            }
        }

        ~RsaSecurityToken()
        {
            this.Dispose(false);
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            RsaKeyIdentifierClause clause = keyIdentifierClause as RsaKeyIdentifierClause;
            return ((clause != null) && clause.Matches(this.rsa));
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public RSA Rsa
        {
            get
            {
                return this.rsa;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (this.rsaKey == null)
                {
                    this.rsaKey = new List<SecurityKey>(1) { new RsaSecurityKey(this.rsa) }.AsReadOnly();
                }
                return this.rsaKey;
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
                return System.IdentityModel.SecurityUtils.MaxUtcDateTime;
            }
        }
    }
}

