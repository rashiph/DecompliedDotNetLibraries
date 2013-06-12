namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum StreamingContextStates
    {
        All = 0xff,
        Clone = 0x40,
        CrossAppDomain = 0x80,
        CrossMachine = 2,
        CrossProcess = 1,
        File = 4,
        Other = 0x20,
        Persistence = 8,
        Remoting = 0x10
    }
}

