namespace System.ServiceModel.Channels
{
    using System;

    internal interface ISessionPreambleHandler
    {
        void HandleServerSessionPreamble(ServerSessionPreambleConnectionReader serverSessionPreambleReader, ConnectionDemuxer connectionDemuxer);
    }
}

