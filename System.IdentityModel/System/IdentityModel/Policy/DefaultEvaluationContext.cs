namespace System.IdentityModel.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;

    internal class DefaultEvaluationContext : EvaluationContext
    {
        private List<ClaimSet> claimSets;
        private DateTime expirationTime = System.IdentityModel.SecurityUtils.MaxUtcDateTime;
        private int generation = 0;
        private Dictionary<string, object> properties = new Dictionary<string, object>();
        private ReadOnlyCollection<ClaimSet> readOnlyClaimSets;

        public override void AddClaimSet(IAuthorizationPolicy policy, ClaimSet claimSet)
        {
            if (claimSet == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimSet");
            }
            if (this.claimSets == null)
            {
                this.claimSets = new List<ClaimSet>();
            }
            this.claimSets.Add(claimSet);
            this.generation++;
        }

        public override void RecordExpirationTime(DateTime expirationTime)
        {
            if (this.expirationTime > expirationTime)
            {
                this.expirationTime = expirationTime;
            }
        }

        public override ReadOnlyCollection<ClaimSet> ClaimSets
        {
            get
            {
                if (this.claimSets == null)
                {
                    return EmptyReadOnlyCollection<ClaimSet>.Instance;
                }
                if (this.readOnlyClaimSets == null)
                {
                    this.readOnlyClaimSets = new ReadOnlyCollection<ClaimSet>(this.claimSets);
                }
                return this.readOnlyClaimSets;
            }
        }

        public DateTime ExpirationTime
        {
            get
            {
                return this.expirationTime;
            }
        }

        public override int Generation
        {
            get
            {
                return this.generation;
            }
        }

        public override IDictionary<string, object> Properties
        {
            get
            {
                return this.properties;
            }
        }
    }
}

