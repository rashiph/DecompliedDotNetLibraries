namespace System.Web.Services.Protocols
{
    using System;
    using System.Web.SessionState;

    internal class SyncSessionHandler : SyncSessionlessHandler, IRequiresSessionState
    {
        internal SyncSessionHandler(ServerProtocol protocol) : base(protocol)
        {
        }
    }
}

