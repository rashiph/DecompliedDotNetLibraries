namespace System.ServiceModel.Security.Tokens
{
    using System;

    public interface IIssuanceSecurityTokenAuthenticator
    {
        System.ServiceModel.Security.Tokens.IssuedSecurityTokenHandler IssuedSecurityTokenHandler { get; set; }

        System.ServiceModel.Security.Tokens.RenewedSecurityTokenHandler RenewedSecurityTokenHandler { get; set; }
    }
}

