namespace System.Management
{
    using System;

    internal enum tag_WBEM_SECURITY_FLAGS
    {
        WBEM_ENABLE = 1,
        WBEM_FULL_WRITE_REP = 4,
        WBEM_METHOD_EXECUTE = 2,
        WBEM_PARTIAL_WRITE_REP = 8,
        WBEM_REMOTE_ACCESS = 0x20,
        WBEM_RIGHT_PUBLISH = 1,
        WBEM_RIGHT_SUBSCRIBE = 1,
        WBEM_WRITE_PROVIDER = 0x10
    }
}

