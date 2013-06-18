namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    public delegate void IssuedSecurityTokenHandler(SecurityToken issuedToken, EndpointAddress tokenRequestor);
}

