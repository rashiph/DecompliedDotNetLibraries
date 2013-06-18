namespace System.IdentityModel.Policy
{
    using System;
    using System.IdentityModel.Claims;

    public interface IAuthorizationPolicy : IAuthorizationComponent
    {
        bool Evaluate(EvaluationContext evaluationContext, ref object state);

        ClaimSet Issuer { get; }
    }
}

