namespace System.IO.IsolatedStorage
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum IsolatedStorageScope
    {
        Application = 0x20,
        Assembly = 4,
        Domain = 2,
        Machine = 0x10,
        None = 0,
        Roaming = 8,
        User = 1
    }
}

