namespace System.IdentityModel.Claims
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Principal;

    [DataContract(Namespace="http://schemas.xmlsoap.org/ws/2005/05/identity")]
    public abstract class ClaimSet : IEnumerable<Claim>, IEnumerable
    {
        private static ClaimSet anonymous;
        private static ClaimSet system;
        private static ClaimSet windows;

        protected ClaimSet()
        {
        }

        public virtual bool ContainsClaim(Claim claim)
        {
            if (claim == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }
            IEnumerable<Claim> enumerable = this.FindClaims(claim.ClaimType, claim.Right);
            if (enumerable != null)
            {
                foreach (Claim claim2 in enumerable)
                {
                    if (claim.Equals(claim2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual bool ContainsClaim(Claim claim, IEqualityComparer<Claim> comparer)
        {
            if (claim == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }
            if (comparer == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("comparer");
            }
            IEnumerable<Claim> enumerable = this.FindClaims(null, null);
            if (enumerable != null)
            {
                foreach (Claim claim2 in enumerable)
                {
                    if (comparer.Equals(claim, claim2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public abstract IEnumerable<Claim> FindClaims(string claimType, string right);
        public abstract IEnumerator<Claim> GetEnumerator();
        internal static bool SupportedRight(string right)
        {
            if ((right != null) && !Rights.Identity.Equals(right))
            {
                return Rights.PossessProperty.Equals(right);
            }
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal static ClaimSet Anonymous
        {
            get
            {
                if (anonymous == null)
                {
                    anonymous = new DefaultClaimSet(new Claim[0]);
                }
                return anonymous;
            }
        }

        public abstract int Count { get; }

        public abstract ClaimSet Issuer { get; }

        public abstract Claim this[int index] { get; }

        public static ClaimSet System
        {
            get
            {
                if (system == null)
                {
                    system = new DefaultClaimSet(new List<Claim>(2) { Claim.System, new Claim(ClaimTypes.System, "System", Rights.PossessProperty) });
                }
                return system;
            }
        }

        public static ClaimSet Windows
        {
            get
            {
                if (windows == null)
                {
                    List<Claim> claims = new List<Claim>(2);
                    SecurityIdentifier resource = new SecurityIdentifier(WellKnownSidType.NTAuthoritySid, null);
                    claims.Add(new Claim(ClaimTypes.Sid, resource, Rights.Identity));
                    claims.Add(Claim.CreateWindowsSidClaim(resource));
                    windows = new DefaultClaimSet(claims);
                }
                return windows;
            }
        }
    }
}

