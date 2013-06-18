namespace System.ServiceModel.Diagnostics
{
    using System;

    [Flags]
    internal enum MessageLoggingSource
    {
        All = 0x7fffffff,
        LastChance = 0x800,
        Malformed = 0x400,
        None = 0,
        ServiceLevel = 0x3f0,
        ServiceLevelProxy = 0x1a0,
        ServiceLevelReceive = 0x150,
        ServiceLevelReceiveDatagram = 0x10,
        ServiceLevelReceiveReply = 0x100,
        ServiceLevelReceiveRequest = 0x40,
        ServiceLevelSend = 0x2a0,
        ServiceLevelSendDatagram = 0x20,
        ServiceLevelSendReply = 0x200,
        ServiceLevelSendRequest = 0x80,
        ServiceLevelService = 0x250,
        Transport = 6,
        TransportReceive = 2,
        TransportSend = 4
    }
}

