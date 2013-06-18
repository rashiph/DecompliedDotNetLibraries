namespace System.Messaging
{
    using System;

    [Serializable, Flags]
    public enum MessageQueuePermissionAccess
    {
        Administer = 0x3e,
        Browse = 2,
        None = 0,
        Peek = 10,
        Receive = 0x1a,
        Send = 6
    }
}

