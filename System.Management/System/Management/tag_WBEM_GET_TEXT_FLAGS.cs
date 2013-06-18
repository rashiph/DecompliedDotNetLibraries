namespace System.Management
{
    using System;

    internal enum tag_WBEM_GET_TEXT_FLAGS
    {
        WBEMPATH_COMPRESSED = 1,
        WBEMPATH_GET_NAMESPACE_ONLY = 0x10,
        WBEMPATH_GET_ORIGINAL = 0x20,
        WBEMPATH_GET_RELATIVE_ONLY = 2,
        WBEMPATH_GET_SERVER_AND_NAMESPACE_ONLY = 8,
        WBEMPATH_GET_SERVER_TOO = 4
    }
}

