namespace System.Net.Sockets
{
    using System;

    public enum SocketAsyncOperation
    {
        None,
        Accept,
        Connect,
        Disconnect,
        Receive,
        ReceiveFrom,
        ReceiveMessageFrom,
        Send,
        SendPackets,
        SendTo
    }
}

