namespace System
{
    [Flags]
    public enum GenericUriParserOptions
    {
        AllowEmptyAuthority = 2,
        Default = 0,
        DontCompressPath = 0x80,
        DontConvertPathBackslashes = 0x40,
        DontUnescapePathDotsAndSlashes = 0x100,
        GenericAuthority = 1,
        Idn = 0x200,
        IriParsing = 0x400,
        NoFragment = 0x20,
        NoPort = 8,
        NoQuery = 0x10,
        NoUserInfo = 4
    }
}

