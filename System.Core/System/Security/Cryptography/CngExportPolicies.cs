namespace System.Security.Cryptography
{
    using System;

    [Flags]
    public enum CngExportPolicies
    {
        AllowArchiving = 4,
        AllowExport = 1,
        AllowPlaintextArchiving = 8,
        AllowPlaintextExport = 2,
        None = 0
    }
}

