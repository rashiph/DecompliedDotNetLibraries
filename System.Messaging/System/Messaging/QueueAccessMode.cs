namespace System.Messaging
{
    using System;

    public enum QueueAccessMode
    {
        Peek = 0x20,
        PeekAndAdmin = 160,
        Receive = 1,
        ReceiveAndAdmin = 0x81,
        Send = 2,
        SendAndReceive = 3
    }
}

