namespace System
{
    public class GenericUriParser : UriParser
    {
        private const UriSyntaxFlags DefaultGenericUriParserFlags = (UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);

        public GenericUriParser(GenericUriParserOptions options) : base(MapGenericParserOptions(options))
        {
        }

        private static UriSyntaxFlags MapGenericParserOptions(GenericUriParserOptions options)
        {
            UriSyntaxFlags flags = UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority;
            if ((options & GenericUriParserOptions.GenericAuthority) != GenericUriParserOptions.Default)
            {
                flags &= ~(UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo);
                flags |= UriSyntaxFlags.AllowAnyOtherHost;
            }
            if ((options & GenericUriParserOptions.AllowEmptyAuthority) != GenericUriParserOptions.Default)
            {
                flags |= UriSyntaxFlags.AllowEmptyHost;
            }
            if ((options & GenericUriParserOptions.NoUserInfo) != GenericUriParserOptions.Default)
            {
                flags &= ~UriSyntaxFlags.MayHaveUserInfo;
            }
            if ((options & GenericUriParserOptions.NoPort) != GenericUriParserOptions.Default)
            {
                flags &= ~UriSyntaxFlags.MayHavePort;
            }
            if ((options & GenericUriParserOptions.NoQuery) != GenericUriParserOptions.Default)
            {
                flags &= ~UriSyntaxFlags.MayHaveQuery;
            }
            if ((options & GenericUriParserOptions.NoFragment) != GenericUriParserOptions.Default)
            {
                flags &= ~UriSyntaxFlags.MayHaveFragment;
            }
            if ((options & GenericUriParserOptions.DontConvertPathBackslashes) != GenericUriParserOptions.Default)
            {
                flags &= ~UriSyntaxFlags.ConvertPathSlashes;
            }
            if ((options & GenericUriParserOptions.DontCompressPath) != GenericUriParserOptions.Default)
            {
                flags &= ~(UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath);
            }
            if ((options & GenericUriParserOptions.DontUnescapePathDotsAndSlashes) != GenericUriParserOptions.Default)
            {
                flags &= ~UriSyntaxFlags.UnEscapeDotsAndSlashes;
            }
            if ((options & GenericUriParserOptions.Idn) != GenericUriParserOptions.Default)
            {
                flags |= UriSyntaxFlags.AllowIdn;
            }
            if ((options & GenericUriParserOptions.IriParsing) != GenericUriParserOptions.Default)
            {
                flags |= UriSyntaxFlags.AllowIriParsing;
            }
            return flags;
        }
    }
}

