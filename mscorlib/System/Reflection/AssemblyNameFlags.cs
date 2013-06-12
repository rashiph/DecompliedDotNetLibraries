namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum AssemblyNameFlags
    {
        EnableJITcompileOptimizer = 0x4000,
        EnableJITcompileTracking = 0x8000,
        None = 0,
        PublicKey = 1,
        Retargetable = 0x100
    }
}

