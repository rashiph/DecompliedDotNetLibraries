namespace System.Net
{
    using System;

    [Flags]
    public enum NetworkAccess
    {
        Accept = 0x80,
        Connect = 0x40
    }
}

