namespace System.Security.Cryptography.X509Certificates
{
    using System;

    [Flags]
    public enum OpenFlags
    {
        IncludeArchived = 8,
        MaxAllowed = 2,
        OpenExistingOnly = 4,
        ReadOnly = 0,
        ReadWrite = 1
    }
}

