namespace System.Management
{
    using System;

    internal enum tag_WBEM_COMPARISON_FLAG
    {
        WBEM_COMPARISON_INCLUDE_ALL = 0,
        WBEM_FLAG_IGNORE_CASE = 0x10,
        WBEM_FLAG_IGNORE_CLASS = 8,
        WBEM_FLAG_IGNORE_DEFAULT_VALUES = 4,
        WBEM_FLAG_IGNORE_FLAVOR = 0x20,
        WBEM_FLAG_IGNORE_OBJECT_SOURCE = 2,
        WBEM_FLAG_IGNORE_QUALIFIERS = 1
    }
}

