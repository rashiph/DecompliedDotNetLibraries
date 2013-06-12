namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum SecurityZone
    {
        Internet = 3,
        Intranet = 1,
        MyComputer = 0,
        NoZone = -1,
        Trusted = 2,
        Untrusted = 4
    }
}

