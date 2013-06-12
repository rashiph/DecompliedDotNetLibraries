namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true)]
    public enum CallingConvention
    {
        Cdecl = 2,
        FastCall = 5,
        StdCall = 3,
        ThisCall = 4,
        Winapi = 1
    }
}

