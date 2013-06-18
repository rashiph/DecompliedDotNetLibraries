namespace System.ServiceModel.Security.Tokens
{
    using System;

    public interface ILogonTokenCacheManager
    {
        void FlushLogonTokenCache();
        bool RemoveCachedLogonToken(string username);
    }
}

