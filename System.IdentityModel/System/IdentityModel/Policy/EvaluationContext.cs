namespace System.IdentityModel.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;

    public abstract class EvaluationContext
    {
        protected EvaluationContext()
        {
        }

        public abstract void AddClaimSet(IAuthorizationPolicy policy, ClaimSet claimSet);
        public abstract void RecordExpirationTime(DateTime expirationTime);

        public abstract ReadOnlyCollection<ClaimSet> ClaimSets { get; }

        public abstract int Generation { get; }

        public abstract IDictionary<string, object> Properties { get; }
    }
}

