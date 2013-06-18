namespace System.Management
{
    using System;

    internal enum tag_WBEM_CHANGE_FLAG_TYPE
    {
        WBEM_FLAG_ADVISORY = 0x10000,
        WBEM_FLAG_CREATE_ONLY = 2,
        WBEM_FLAG_CREATE_OR_UPDATE = 0,
        WBEM_FLAG_UPDATE_COMPATIBLE = 0,
        WBEM_FLAG_UPDATE_FORCE_MODE = 0x40,
        WBEM_FLAG_UPDATE_ONLY = 1,
        WBEM_FLAG_UPDATE_SAFE_MODE = 0x20,
        WBEM_MASK_UPDATE_MODE = 0x60
    }
}

