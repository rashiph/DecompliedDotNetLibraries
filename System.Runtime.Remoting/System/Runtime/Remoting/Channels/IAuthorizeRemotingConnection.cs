namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Net;
    using System.Security.Principal;

    public interface IAuthorizeRemotingConnection
    {
        bool IsConnectingEndPointAuthorized(EndPoint endPoint);
        bool IsConnectingIdentityAuthorized(IIdentity identity);
    }
}

