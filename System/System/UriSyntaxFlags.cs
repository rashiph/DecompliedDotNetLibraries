namespace System
{
    [Flags]
    internal enum UriSyntaxFlags
    {
        AllowAnInternetHost = 0xe00,
        AllowAnyOtherHost = 0x1000,
        AllowDnsHost = 0x200,
        AllowDOSPath = 0x100000,
        AllowEmptyHost = 0x80,
        AllowIdn = 0x4000000,
        AllowIPv4Host = 0x400,
        AllowIPv6Host = 0x800,
        AllowIriParsing = 0x10000000,
        AllowUncHost = 0x100,
        BuiltInSyntax = 0x40000,
        CanonicalizeAsFilePath = 0x1000000,
        CompressPath = 0x800000,
        ConvertPathSlashes = 0x400000,
        FileLikeUri = 0x2000,
        MailToLikeUri = 0x4000,
        MayHaveFragment = 0x40,
        MayHavePath = 0x10,
        MayHavePort = 8,
        MayHaveQuery = 0x20,
        MayHaveUserInfo = 4,
        MustHaveAuthority = 1,
        None = 0,
        OptionalAuthority = 2,
        ParserSchemeOnly = 0x80000,
        PathIsRooted = 0x200000,
        SimpleUserSyntax = 0x20000,
        UnEscapeDotsAndSlashes = 0x2000000,
        V1_UnknownUri = 0x10000
    }
}

