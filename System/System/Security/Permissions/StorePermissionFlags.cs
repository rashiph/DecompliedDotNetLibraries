namespace System.Security.Permissions
{
    using System;

    [Serializable, Flags]
    public enum StorePermissionFlags
    {
        AddToStore = 0x20,
        AllFlags = 0xf7,
        CreateStore = 1,
        DeleteStore = 2,
        EnumerateCertificates = 0x80,
        EnumerateStores = 4,
        NoFlags = 0,
        OpenStore = 0x10,
        RemoveFromStore = 0x40
    }
}

