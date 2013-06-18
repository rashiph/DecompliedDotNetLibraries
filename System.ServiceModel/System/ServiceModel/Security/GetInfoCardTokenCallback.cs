namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;

    internal delegate SecurityToken GetInfoCardTokenCallback(bool requiresInfoCard, CardSpacePolicyElement[] chain, SecurityTokenSerializer tokenSerializer);
}

