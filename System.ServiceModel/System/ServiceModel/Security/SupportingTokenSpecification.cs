namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security.Tokens;

    public class SupportingTokenSpecification : SecurityTokenSpecification
    {
        private System.ServiceModel.Security.SecurityTokenAttachmentMode tokenAttachmentMode;
        private System.ServiceModel.Security.Tokens.SecurityTokenParameters tokenParameters;

        public SupportingTokenSpecification(SecurityToken token, ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies, System.ServiceModel.Security.SecurityTokenAttachmentMode attachmentMode) : this(token, tokenPolicies, attachmentMode, null)
        {
        }

        public SupportingTokenSpecification(SecurityToken token, ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies, System.ServiceModel.Security.SecurityTokenAttachmentMode attachmentMode, System.ServiceModel.Security.Tokens.SecurityTokenParameters tokenParameters) : base(token, tokenPolicies)
        {
            SecurityTokenAttachmentModeHelper.Validate(attachmentMode);
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

        internal System.ServiceModel.Security.Tokens.SecurityTokenParameters SecurityTokenParameters
        {
            get
            {
                return this.tokenParameters;
            }
        }
    }
}

