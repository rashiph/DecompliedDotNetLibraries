namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;

    public delegate void RenewedSecurityTokenHandler(SecurityToken newSecurityToken, SecurityToken oldSecurityToken);
}

