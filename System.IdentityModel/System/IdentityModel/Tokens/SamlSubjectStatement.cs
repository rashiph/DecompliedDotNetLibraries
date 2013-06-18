namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;

    public abstract class SamlSubjectStatement : SamlStatement
    {
        private bool isReadOnly;
        private IAuthorizationPolicy policy;
        private System.IdentityModel.Tokens.SamlSubject subject;

        protected SamlSubjectStatement()
        {
        }

        protected SamlSubjectStatement(System.IdentityModel.Tokens.SamlSubject samlSubject)
        {
            if (samlSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSubject"));
            }
            this.subject = samlSubject;
        }

        protected abstract void AddClaimsToList(IList<Claim> claims);
        public override IAuthorizationPolicy CreatePolicy(ClaimSet issuer, SamlSecurityTokenAuthenticator samlAuthenticator)
        {
            if (issuer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuer");
            }
            if (this.policy == null)
            {
                List<ClaimSet> list = new List<ClaimSet>();
                ClaimSet item = this.subject.ExtractSubjectKeyClaimSet(samlAuthenticator);
                if (item != null)
                {
                    list.Add(item);
                }
                List<Claim> claims = new List<Claim>();
                ReadOnlyCollection<Claim> onlys = this.subject.ExtractClaims();
                for (int i = 0; i < onlys.Count; i++)
                {
                    claims.Add(onlys[i]);
                }
                this.AddClaimsToList(claims);
                list.Add(new DefaultClaimSet(issuer, claims));
                this.policy = new UnconditionalPolicy(this.subject.Identity, list.AsReadOnly(), System.IdentityModel.SecurityUtils.MaxUtcDateTime);
            }
            return this.policy;
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.subject.MakeReadOnly();
                this.isReadOnly = true;
            }
        }

        protected void SetSubject(System.IdentityModel.Tokens.SamlSubject samlSubject)
        {
            if (samlSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSubject"));
            }
            this.subject = samlSubject;
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public System.IdentityModel.Tokens.SamlSubject SamlSubject
        {
            get
            {
                return this.subject;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.subject = value;
            }
        }
    }
}

