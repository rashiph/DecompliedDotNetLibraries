namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    internal class SamlAssertionDirectKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private string samlUri;

        public SamlAssertionDirectKeyIdentifierClause(string samlUri, byte[] derivationNonce, int derivationLength) : base(null, derivationNonce, derivationLength)
        {
            if (string.IsNullOrEmpty(samlUri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException("SamlUriCannotBeNullOrEmpty"));
            }
            this.samlUri = samlUri;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SamlAssertionDirectKeyIdentifierClause objB = keyIdentifierClause as SamlAssertionDirectKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && (objB.SamlUri == this.SamlUri)));
        }

        public string SamlUri
        {
            get
            {
                return this.samlUri;
            }
        }
    }
}

