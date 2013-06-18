namespace Microsoft.Build.Utilities
{
    using System;

    public enum ExecutableType
    {
        Native32Bit,
        Native64Bit,
        ManagedIL,
        Managed32Bit,
        Managed64Bit,
        SameAsCurrentProcess
    }
}

