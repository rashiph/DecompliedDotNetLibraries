namespace System.Runtime.Versioning
{
    using System;

    [Flags]
    internal enum SxSRequirements
    {
        AppDomainID = 1,
        AssemblyName = 8,
        CLRInstanceID = 4,
        None = 0,
        ProcessID = 2,
        TypeName = 0x10
    }
}

