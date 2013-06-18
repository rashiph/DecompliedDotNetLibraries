namespace System.IdentityModel.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel;

    public class SamlAssertionKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private readonly string assertionId;
        private readonly string authorityKind;
        private readonly string binding;
        private readonly string location;
        private readonly string tokenTypeUri;
        private readonly string valueType;

        public SamlAssertionKeyIdentifierClause(string assertionId) : this(assertionId, null, 0)
        {
        }

        public SamlAssertionKeyIdentifierClause(string assertionId, byte[] derivationNonce, int derivationLength) : this(assertionId, derivationNonce, derivationLength, null, null, null, null, null)
        {
        }

        internal SamlAssertionKeyIdentifierClause(string assertionId, byte[] derivationNonce, int derivationLength, string valueType, string tokenTypeUri, string binding, string location, string authorityKind) : base(null, derivationNonce, derivationLength)
        {
            if (assertionId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertionId");
            }
            this.assertionId = assertionId;
            this.valueType = valueType;
            this.tokenTypeUri = tokenTypeUri;
            this.binding = binding;
            this.location = location;
            this.authorityKind = authorityKind;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SamlAssertionKeyIdentifierClause objB = keyIdentifierClause as SamlAssertionKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(this.assertionId)));
        }

        public bool Matches(string assertionId)
        {
            return (this.assertionId == assertionId);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SamlAssertionKeyIdentifierClause(AssertionId = '{0}')", new object[] { this.AssertionId });
        }

        public string AssertionId
        {
            get
            {
                return this.assertionId;
            }
        }

        internal string AuthorityKind
        {
            get
            {
                return this.authorityKind;
            }
        }

        internal string Binding
        {
            get
            {
                return this.binding;
            }
        }

        internal string Location
        {
            get
            {
                return this.location;
            }
        }

        internal string TokenTypeUri
        {
            get
            {
                return this.tokenTypeUri;
            }
        }

        internal string ValueType
        {
            get
            {
                return this.valueType;
            }
        }
    }
}

