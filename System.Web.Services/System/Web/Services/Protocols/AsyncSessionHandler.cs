namespace System.Web.Services.Protocols
{
    using System;
    using System.Web.SessionState;

    internal class AsyncSessionHandler : AsyncSessionlessHandler, IRequiresSessionState
    {
        internal AsyncSessionHandler(ServerProtocol protocol) : base(protocol)
        {
        }
    }
}

