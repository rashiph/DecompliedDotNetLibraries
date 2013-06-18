namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal class SupportingTokenAuthenticatorSpecification
    {
        private bool isTokenOptional;
        private System.ServiceModel.Security.SecurityTokenAttachmentMode tokenAttachmentMode;
        private SecurityTokenAuthenticator tokenAuthenticator;
        private SecurityTokenParameters tokenParameters;
        private SecurityTokenResolver tokenResolver;

        public SupportingTokenAuthenticatorSpecification(SecurityTokenAuthenticator tokenAuthenticator, SecurityTokenResolver securityTokenResolver, System.ServiceModel.Security.SecurityTokenAttachmentMode attachmentMode, SecurityTokenParameters tokenParameters) : this(tokenAuthenticator, securityTokenResolver, attachmentMode, tokenParameters, false)
        {
        }

        internal SupportingTokenAuthenticatorSpecification(SecurityTokenAuthenticator tokenAuthenticator, SecurityTokenResolver securityTokenResolver, System.ServiceModel.Security.SecurityTokenAttachmentMode attachmentMode, SecurityTokenParameters tokenParameters, bool isTokenOptional)
        {
            if (tokenAuthenticator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenAuthenticator");
            }
            SecurityTokenAttachmentModeHelper.Validate(attachmentMode);
            if (tokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenParameters");
            }
            this.tokenAuthenticator = tokenAuthenticator;
            this.tokenResolver = securityTokenResolver;
            this.tokenAttachmentMode = attachmentMode;
            this.tokenParameters = tokenParameters;
            this.isTokenOptional = isTokenOptional;
        }

        internal bool IsTokenOptional
        {
            get
            {
                return this.isTokenOptional;
            }
            set
            {
                this.isTokenOptional = value;
            }
        }

        public System.ServiceModel.Security.SecurityTokenAttachmentMode SecurityTokenAttachmentMode
        {
            get
            {
                return this.tokenAttachmentMode;
            }
        }

        public SecurityTokenAuthenticator TokenAuthenticator
        {
            get
            {
                return this.tokenAuthenticator;
            }
        }

        public SecurityTokenParameters TokenParameters
        {
            get
            {
                return this.tokenParameters;
            }
        }

        public SecurityTokenResolver TokenResolver
        {
            get
            {
                return this.tokenResolver;
            }
        }
    }
}

