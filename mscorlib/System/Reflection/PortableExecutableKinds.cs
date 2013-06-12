namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum PortableExecutableKinds
    {
        ILOnly = 1,
        NotAPortableExecutableImage = 0,
        PE32Plus = 4,
        Required32Bit = 2,
        Unmanaged32Bit = 8
    }
}

