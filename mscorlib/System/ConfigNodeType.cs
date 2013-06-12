namespace System
{
    [Serializable]
    internal enum ConfigNodeType
    {
        ATTDef = 0x18,
        AttlistDecl = 9,
        ATTPresence = 0x1a,
        Attribute = 2,
        ATTType = 0x19,
        CData = 14,
        Comment = 0x10,
        DocType = 5,
        DTDAttribute = 6,
        DTDSubset = 0x1b,
        Element = 1,
        ElementDecl = 8,
        EntityDecl = 7,
        EntityRef = 0x11,
        Group = 11,
        IgnoreSect = 15,
        IncludeSect = 12,
        LastNodeType = 0x1c,
        Model = 0x17,
        Name = 0x13,
        NMToken = 20,
        Notation = 10,
        PCData = 13,
        Peref = 0x16,
        Pi = 3,
        String = 0x15,
        Whitespace = 0x12,
        XmlDecl = 4
    }
}

