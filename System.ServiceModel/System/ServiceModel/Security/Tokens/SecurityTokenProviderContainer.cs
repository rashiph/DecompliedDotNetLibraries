namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class SecurityTokenProviderContainer
    {
        private SecurityTokenProvider tokenProvider;

        public SecurityTokenProviderContainer(SecurityTokenProvider tokenProvider)
        {
            if (tokenProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenProvider");
            }
            this.tokenProvider = tokenProvider;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Abort()
        {
            System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.tokenProvider);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Close(TimeSpan timeout)
        {
            System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.tokenProvider, timeout);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public X509Certificate2 GetCertificate(TimeSpan timeout)
        {
            X509SecurityToken token = this.tokenProvider.GetToken(timeout) as X509SecurityToken;
            if (token != null)
            {
                return token.Certificate;
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Open(TimeSpan timeout)
        {
            System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.tokenProvider, timeout);
        }

        public SecurityTokenProvider TokenProvider
        {
            get
            {
                return this.tokenProvider;
            }
        }
    }
}

