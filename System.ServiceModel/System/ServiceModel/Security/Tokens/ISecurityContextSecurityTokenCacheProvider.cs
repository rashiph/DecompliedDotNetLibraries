namespace System.ServiceModel.Security.Tokens
{
    internal interface ISecurityContextSecurityTokenCacheProvider
    {
        ISecurityContextSecurityTokenCache TokenCache { get; }
    }
}

