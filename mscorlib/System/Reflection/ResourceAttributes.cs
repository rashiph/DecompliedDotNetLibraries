namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum ResourceAttributes
    {
        Private = 2,
        Public = 1
    }
}

