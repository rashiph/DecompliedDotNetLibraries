namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;

    internal interface IInitiatorSecuritySessionProtocol
    {
        List<SecurityToken> GetIncomingSessionTokens();
        SecurityToken GetOutgoingSessionToken();
        void SetIdentityCheckAuthenticator(SecurityTokenAuthenticator tokenAuthenticator);
        void SetIncomingSessionTokens(List<SecurityToken> tokens);
        void SetOutgoingSessionToken(SecurityToken token);

        bool ReturnCorrelationState { get; set; }
    }
}

