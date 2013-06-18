namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    internal class RelAssertionDirectKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private string assertionId;

        public RelAssertionDirectKeyIdentifierClause(string assertionId, byte[] derivationNonce, int derivationLength) : base(null, derivationNonce, derivationLength)
        {
            if (string.IsNullOrEmpty(assertionId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException("AssertionIdCannotBeNullOrEmpty"));
            }
            this.assertionId = assertionId;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            RelAssertionDirectKeyIdentifierClause objB = keyIdentifierClause as RelAssertionDirectKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && (objB.AssertionId == this.AssertionId)));
        }

        public string AssertionId
        {
            get
            {
                return this.assertionId;
            }
        }
    }
}

