namespace System.Net
{
    using System;

    internal enum NetworkingPerfCounterName
    {
        SocketConnectionsEstablished,
        SocketBytesReceived,
        SocketBytesSent,
        SocketDatagramsReceived,
        SocketDatagramsSent,
        HttpWebRequestCreated,
        HttpWebRequestAvgLifeTime,
        HttpWebRequestAvgLifeTimeBase,
        HttpWebRequestQueued,
        HttpWebRequestAvgQueueTime,
        HttpWebRequestAvgQueueTimeBase,
        HttpWebRequestAborted,
        HttpWebRequestFailed
    }
}

