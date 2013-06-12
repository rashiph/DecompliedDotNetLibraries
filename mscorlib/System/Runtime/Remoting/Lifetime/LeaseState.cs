namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum LeaseState
    {
        Null,
        Initial,
        Active,
        Renewing,
        Expired
    }
}

