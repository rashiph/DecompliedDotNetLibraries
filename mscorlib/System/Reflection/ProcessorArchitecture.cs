namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum ProcessorArchitecture
    {
        None,
        MSIL,
        X86,
        IA64,
        Amd64
    }
}

