namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum ThreadPriority
    {
        Lowest,
        BelowNormal,
        Normal,
        AboveNormal,
        Highest
    }
}

