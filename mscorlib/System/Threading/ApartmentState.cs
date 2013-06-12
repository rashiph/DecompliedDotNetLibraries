namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum ApartmentState
    {
        STA,
        MTA,
        Unknown
    }
}

