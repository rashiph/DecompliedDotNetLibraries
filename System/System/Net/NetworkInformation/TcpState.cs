namespace System.Net.NetworkInformation
{
    using System;

    public enum TcpState
    {
        Unknown,
        Closed,
        Listen,
        SynSent,
        SynReceived,
        Established,
        FinWait1,
        FinWait2,
        CloseWait,
        Closing,
        LastAck,
        TimeWait,
        DeleteTcb
    }
}

