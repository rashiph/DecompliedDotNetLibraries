namespace System
{
    [Serializable]
    internal enum ConfigNodeSubType
    {
        Any = 0x33,
        AtCData = 0x25,
        AtEntities = 0x2a,
        AtEntity = 0x29,
        AtFixed = 0x30,
        AtId = 0x26,
        AtIdref = 0x27,
        AtIdrefs = 40,
        AtImplied = 0x2f,
        AtNmToken = 0x2b,
        AtNmTokens = 0x2c,
        AtNotation = 0x2d,
        AtRequired = 0x2e,
        Choice = 0x36,
        Empty = 50,
        Encoding = 0x1d,
        LastSubNodeType = 0x3a,
        Mixed = 0x34,
        NData = 0x24,
        NS = 0x1f,
        PentityDecl = 0x31,
        Plus = 0x38,
        Public = 0x23,
        Questionmark = 0x39,
        Sequence = 0x35,
        Standalone = 30,
        Star = 0x37,
        System = 0x22,
        Version = 0x1c,
        XMLLang = 0x21,
        XMLSpace = 0x20
    }
}

