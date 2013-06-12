namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum X509KeyStorageFlags
    {
        DefaultKeySet = 0,
        Exportable = 4,
        MachineKeySet = 2,
        PersistKeySet = 0x10,
        UserKeySet = 1,
        UserProtected = 8
    }
}

