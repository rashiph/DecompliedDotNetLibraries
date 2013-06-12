namespace System.Deployment.Internal.Isolation
{
    using System;

    [Flags]
    internal enum STORE_ASSEMBLY_STATUS_FLAGS
    {
        STORE_ASSEMBLY_STATUS_MANIFEST_ONLY = 1,
        STORE_ASSEMBLY_STATUS_PARTIAL_INSTALL = 4,
        STORE_ASSEMBLY_STATUS_PAYLOAD_RESIDENT = 2
    }
}

