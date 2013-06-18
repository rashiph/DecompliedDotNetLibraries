namespace System.ServiceModel.ComIntegration
{
    using System;

    [Flags]
    internal enum CLSCTX
    {
        ACTIVATE_32_BIT_SERVER = 0x40000,
        ACTIVATE_64_BIT_SERVER = 0x80000,
        ALL = 0x17,
        DISABLE_AAA = 0x8000,
        ENABLE_AAA = 0x10000,
        ENABLE_CODE_DOWNLOAD = 0x2000,
        FROM_DEFAULT_CONTEXT = 0x20000,
        INPROC = 3,
        INPROC_HANDLER = 2,
        INPROC_HANDLER16 = 0x20,
        INPROC_SERVER = 1,
        INPROC_SERVER16 = 8,
        LOCAL_SERVER = 4,
        NO_CODE_DOWNLOAD = 0x400,
        NO_CUSTOM_MARSHAL = 0x1000,
        NO_FAILURE_LOG = 0x4000,
        REMOTE_SERVER = 0x10,
        RESERVED1 = 0x40,
        RESERVED2 = 0x80,
        RESERVED3 = 0x100,
        RESERVED4 = 0x200,
        RESERVED5 = 0x800,
        SERVER = 0x15
    }
}

