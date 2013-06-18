namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;

    public class SamlSecurityToken : SecurityToken
    {
        private SamlAssertion assertion;

        protected SamlSecurityToken()
        {
        }

        public SamlSecurityToken(SamlAssertion assertion)
        {
            this.Initialize(assertion);
        }

        public override bool CanCreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            return (typeof(T) == typeof(SamlAssertionKeyIdentifierClause));
        }

        public override T CreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            if (typeof(T) != typeof(SamlAssertionKeyIdentifierClause))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("UnableToCreateTokenReference")));
            }
            return (new SamlAssertionKeyIdentifierClause(this.Id) as T);
        }

        protected void Initialize(SamlAssertion assertion)
        {
            if (assertion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }
            this.assertion = assertion;
            this.assertion.MakeReadOnly();
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SamlAssertionKeyIdentifierClause clause = keyIdentifierClause as SamlAssertionKeyIdentifierClause;
            return ((clause != null) && clause.Matches(this.Id));
        }

        public SamlAssertion Assertion
        {
            get
            {
                return this.assertion;
            }
        }

        public override string Id
        {
            get
            {
                return this.assertion.AssertionId;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return this.assertion.SecurityKeys;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                if (this.assertion.Conditions != null)
                {
                    return this.assertion.Conditions.NotBefore;
                }
                return System.IdentityModel.SecurityUtils.MinUtcDateTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                if (this.assertion.Conditions != null)
                {
                    return this.assertion.Conditions.NotOnOrAfter;
                }
                return System.IdentityModel.SecurityUtils.MaxUtcDateTime;
            }
        }
    }
}

