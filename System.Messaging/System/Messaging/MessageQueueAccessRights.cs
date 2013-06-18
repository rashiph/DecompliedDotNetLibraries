namespace System.Messaging
{
    using System;

    [Flags]
    public enum MessageQueueAccessRights
    {
        ChangeQueuePermissions = 0x40000,
        DeleteJournalMessage = 8,
        DeleteMessage = 1,
        DeleteQueue = 0x10000,
        FullControl = 0xf003f,
        GenericRead = 0x2002b,
        GenericWrite = 0x20024,
        GetQueuePermissions = 0x20000,
        GetQueueProperties = 0x20,
        PeekMessage = 2,
        ReceiveJournalMessage = 10,
        ReceiveMessage = 3,
        SetQueueProperties = 0x10,
        TakeQueueOwnership = 0x80000,
        WriteMessage = 4
    }
}

