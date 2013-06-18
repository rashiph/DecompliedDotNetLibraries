namespace System.IdentityModel.Claims
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [DataContract(Namespace="http://schemas.xmlsoap.org/ws/2005/05/identity")]
    public class DefaultClaimSet : ClaimSet
    {
        [DataMember(Name="Claims")]
        private IList<Claim> claims;
        [DataMember(Name="Issuer")]
        private ClaimSet issuer;

        public DefaultClaimSet(params Claim[] claims)
        {
            this.Initialize(this, claims);
        }

        public DefaultClaimSet(IList<Claim> claims)
        {
            this.Initialize(this, claims);
        }

        public DefaultClaimSet(ClaimSet issuer, params Claim[] claims)
        {
            this.Initialize(issuer, claims);
        }

        public DefaultClaimSet(ClaimSet issuer, IList<Claim> claims)
        {
            this.Initialize(issuer, claims);
        }

        public override bool ContainsClaim(Claim claim)
        {
            if (claim == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }
            for (int i = 0; i < this.claims.Count; i++)
            {
                if (claim.Equals(this.claims[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<Claim> FindClaims(string claimType, string right)
        {
            bool iteratorVariable0 = claimType == null;
            bool iteratorVariable1 = right == null;
            for (int i = 0; i < this.claims.Count; i++)
            {
                Claim iteratorVariable3 = this.claims[i];
                if (((iteratorVariable3 != null) && (iteratorVariable0 || (claimType == iteratorVariable3.ClaimType))) && (iteratorVariable1 || (right == iteratorVariable3.Right)))
                {
                    yield return iteratorVariable3;
                }
            }
        }

        public override IEnumerator<Claim> GetEnumerator()
        {
            return this.claims.GetEnumerator();
        }

        protected void Initialize(ClaimSet issuer, IList<Claim> claims)
        {
            if (issuer == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuer");
            }
            if (claims == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claims");
            }
            this.issuer = issuer;
            this.claims = claims;
        }

        public override string ToString()
        {
            return System.IdentityModel.SecurityUtils.ClaimSetToString(this);
        }

        public override int Count
        {
            get
            {
                return this.claims.Count;
            }
        }

        public override ClaimSet Issuer
        {
            get
            {
                return this.issuer;
            }
        }

        public override Claim this[int index]
        {
            get
            {
                return this.claims[index];
            }
        }

    }
}

