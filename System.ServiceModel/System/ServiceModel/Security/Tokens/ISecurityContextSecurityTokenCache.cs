namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;

    public interface ISecurityContextSecurityTokenCache
    {
        void AddContext(SecurityContextSecurityToken token);
        void ClearContexts();
        Collection<SecurityContextSecurityToken> GetAllContexts(UniqueId contextId);
        SecurityContextSecurityToken GetContext(UniqueId contextId, UniqueId generation);
        void RemoveAllContexts(UniqueId contextId);
        void RemoveContext(UniqueId contextId, UniqueId generation);
        bool TryAddContext(SecurityContextSecurityToken token);
        void UpdateContextCachingTime(SecurityContextSecurityToken context, DateTime expirationTime);
    }
}

