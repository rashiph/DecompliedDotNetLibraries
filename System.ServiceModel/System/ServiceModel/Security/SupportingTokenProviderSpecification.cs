namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal class SupportingTokenProviderSpecification
    {
        private System.ServiceModel.Security.SecurityTokenAttachmentMode tokenAttachmentMode;
        private SecurityTokenParameters tokenParameters;
        private SecurityTokenProvider tokenProvider;

        public SupportingTokenProviderSpecification(SecurityTokenProvider tokenProvider, System.ServiceModel.Security.SecurityTokenAttachmentMode attachmentMode, SecurityTokenParameters tokenParameters)
        {
            if (tokenProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenProvider");
            }
            SecurityTokenAttachmentModeHelper.Validate(attachmentMode);
            if (tokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenParameters");
            }
            this.tokenProvider = tokenProvider;
            this.tokenAttachmentMode = attachmentMode;
            this.tokenParameters = tokenParameters;
        }

        public System.ServiceModel.Security.SecurityTokenAttachmentMode SecurityTokenAttachmentMode
        {
            get
            {
                return this.tokenAttachmentMode;
            }
        }

        public SecurityTokenParameters TokenParameters
        {
            get
            {
                return this.tokenParameters;
            }
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

