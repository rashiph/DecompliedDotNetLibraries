namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum ImageFileMachine
    {
        AMD64 = 0x8664,
        I386 = 0x14c,
        IA64 = 0x200
    }
}

