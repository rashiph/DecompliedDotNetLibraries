namespace System.Xml
{
    using System;

    internal enum ElementProperties : uint
    {
        BLOCK_WS = 0x40,
        BOOL_PARENT = 2,
        DEFAULT = 0,
        EMPTY = 8,
        HAS_NS = 0x80,
        HEAD = 0x20,
        NAME_PARENT = 4,
        NO_ENTITIES = 0x10,
        URI_PARENT = 1
    }
}

