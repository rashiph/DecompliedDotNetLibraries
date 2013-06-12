namespace System.Configuration.Assemblies
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum AssemblyHashAlgorithm
    {
        MD5 = 0x8003,
        None = 0,
        SHA1 = 0x8004
    }
}

