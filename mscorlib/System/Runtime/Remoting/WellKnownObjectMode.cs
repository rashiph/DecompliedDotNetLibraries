namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum WellKnownObjectMode
    {
        SingleCall = 2,
        Singleton = 1
    }
}

