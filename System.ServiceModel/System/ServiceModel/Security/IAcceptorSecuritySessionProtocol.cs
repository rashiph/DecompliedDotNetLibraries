namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Xml;

    internal interface IAcceptorSecuritySessionProtocol
    {
        SecurityToken GetOutgoingSessionToken();
        void SetOutgoingSessionToken(SecurityToken token);
        void SetSessionTokenAuthenticator(UniqueId sessionId, SecurityTokenAuthenticator sessionTokenAuthenticator, SecurityTokenResolver sessionTokenResolver);

        bool ReturnCorrelationState { get; set; }
    }
}

