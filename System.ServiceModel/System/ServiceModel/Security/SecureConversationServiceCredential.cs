namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    public sealed class SecureConversationServiceCredential
    {
        private static readonly System.ServiceModel.Security.SecurityStateEncoder defaultSecurityStateEncoder = new DataProtectionSecurityStateEncoder();
        private bool isReadOnly;
        private Collection<Type> securityContextClaimTypes;
        private System.ServiceModel.Security.SecurityStateEncoder securityStateEncoder;

        internal SecureConversationServiceCredential()
        {
            this.securityStateEncoder = defaultSecurityStateEncoder;
            this.securityContextClaimTypes = new Collection<Type>();
            SamlAssertion.AddSamlClaimTypes(this.securityContextClaimTypes);
        }

        internal SecureConversationServiceCredential(SecureConversationServiceCredential other)
        {
            this.securityStateEncoder = other.securityStateEncoder;
            this.securityContextClaimTypes = new Collection<Type>();
            for (int i = 0; i < other.securityContextClaimTypes.Count; i++)
            {
                this.securityContextClaimTypes.Add(other.securityContextClaimTypes[i]);
            }
            this.isReadOnly = other.isReadOnly;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public Collection<Type> SecurityContextClaimTypes
        {
            get
            {
                return this.securityContextClaimTypes;
            }
        }

        public System.ServiceModel.Security.SecurityStateEncoder SecurityStateEncoder
        {
            get
            {
                return this.securityStateEncoder;
            }
            set
            {
                this.ThrowIfImmutable();
                this.securityStateEncoder = value;
            }
        }
    }
}

