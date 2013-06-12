namespace System
{
    using System.Collections;
    using System.Globalization;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public abstract class UriParser
    {
        private const int c_InitialTableSize = 0x19;
        private const int c_MaxCapacity = 0x200;
        private const UriSyntaxFlags c_UpdatableFlags = UriSyntaxFlags.UnEscapeDotsAndSlashes;
        private const UriSyntaxFlags FileSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser FileUri;
        private const UriSyntaxFlags FtpSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser FtpUri;
        private const UriSyntaxFlags GopherSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser GopherUri;
        internal static UriParser HttpsUri;
        private const UriSyntaxFlags HttpSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser HttpUri = new BuiltInUriParser("http", 80, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
        private const UriSyntaxFlags LdapSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser LdapUri;
        private UriSyntaxFlags m_Flags;
        private int m_Port;
        private string m_Scheme;
        private static readonly Hashtable m_Table = new Hashtable(0x19);
        private static Hashtable m_TempTable = new Hashtable(0x19);
        private volatile UriSyntaxFlags m_UpdatableFlags;
        private volatile bool m_UpdatableFlagsUsed;
        private const UriSyntaxFlags MailtoSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.MailToLikeUri | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo);
        internal static UriParser MailToUri;
        private const UriSyntaxFlags NetPipeSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser NetPipeUri;
        private const UriSyntaxFlags NetTcpSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser NetTcpUri;
        private const UriSyntaxFlags NewsSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath);
        internal static UriParser NewsUri;
        private const UriSyntaxFlags NntpSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser NntpUri;
        internal const int NoDefaultPort = -1;
        private const UriSyntaxFlags SchemeOnlyFlags = UriSyntaxFlags.MayHavePath;
        private const UriSyntaxFlags TelnetSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser TelnetUri;
        private const UriSyntaxFlags UnknownV1SyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.V1_UnknownUri | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.OptionalAuthority);
        internal static UriParser UuidUri;
        private const UriSyntaxFlags VsmacrosSyntaxFlags = (UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MustHaveAuthority);
        internal static UriParser VsMacrosUri;

        static UriParser()
        {
            m_Table[HttpUri.SchemeName] = HttpUri;
            HttpsUri = new BuiltInUriParser("https", 0x1bb, HttpUri.m_Flags);
            m_Table[HttpsUri.SchemeName] = HttpsUri;
            FtpUri = new BuiltInUriParser("ftp", 0x15, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
            m_Table[FtpUri.SchemeName] = FtpUri;
            FileUri = new BuiltInUriParser("file", -1, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MustHaveAuthority);
            m_Table[FileUri.SchemeName] = FileUri;
            GopherUri = new BuiltInUriParser("gopher", 70, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
            m_Table[GopherUri.SchemeName] = GopherUri;
            NntpUri = new BuiltInUriParser("nntp", 0x77, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
            m_Table[NntpUri.SchemeName] = NntpUri;
            NewsUri = new BuiltInUriParser("news", -1, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath);
            m_Table[NewsUri.SchemeName] = NewsUri;
            MailToUri = new BuiltInUriParser("mailto", 0x19, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.MailToLikeUri | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo);
            m_Table[MailToUri.SchemeName] = MailToUri;
            UuidUri = new BuiltInUriParser("uuid", -1, NewsUri.m_Flags);
            m_Table[UuidUri.SchemeName] = UuidUri;
            TelnetUri = new BuiltInUriParser("telnet", 0x17, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
            m_Table[TelnetUri.SchemeName] = TelnetUri;
            LdapUri = new BuiltInUriParser("ldap", 0x185, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MustHaveAuthority);
            m_Table[LdapUri.SchemeName] = LdapUri;
            NetTcpUri = new BuiltInUriParser("net.tcp", 0x328, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MustHaveAuthority);
            m_Table[NetTcpUri.SchemeName] = NetTcpUri;
            NetPipeUri = new BuiltInUriParser("net.pipe", -1, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MustHaveAuthority);
            m_Table[NetPipeUri.SchemeName] = NetPipeUri;
            VsMacrosUri = new BuiltInUriParser("vsmacros", -1, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MustHaveAuthority);
            m_Table[VsMacrosUri.SchemeName] = VsMacrosUri;
        }

        protected UriParser() : this(UriSyntaxFlags.MayHavePath)
        {
        }

        internal UriParser(UriSyntaxFlags flags)
        {
            this.m_Flags = flags;
            this.m_Scheme = string.Empty;
        }

        private static bool CheckSchemeName(string schemeName)
        {
            if (((schemeName == null) || (schemeName.Length == 0)) || !IsAsciiLetter(schemeName[0]))
            {
                return false;
            }
            for (int i = schemeName.Length - 1; i > 0; i--)
            {
                if ((!IsAsciiLetterOrDigit(schemeName[i]) && (schemeName[i] != '+')) && ((schemeName[i] != '-') && (schemeName[i] != '.')))
                {
                    return false;
                }
            }
            return true;
        }

        internal void CheckSetIsSimpleFlag()
        {
            Type type = base.GetType();
            if ((((type == typeof(GenericUriParser)) || (type == typeof(HttpStyleUriParser))) || ((type == typeof(FtpStyleUriParser)) || (type == typeof(FileStyleUriParser)))) || (((type == typeof(NewsStyleUriParser)) || (type == typeof(GopherStyleUriParser))) || (((type == typeof(NetPipeStyleUriParser)) || (type == typeof(NetTcpStyleUriParser))) || (type == typeof(LdapStyleUriParser)))))
            {
                this.m_Flags |= UriSyntaxFlags.SimpleUserSyntax;
            }
        }

        private static void FetchSyntax(UriParser syntax, string lwrCaseSchemeName, int defaultPort)
        {
            if (syntax.SchemeName.Length != 0)
            {
                throw new InvalidOperationException(SR.GetString("net_uri_NeedFreshParser", new object[] { syntax.SchemeName }));
            }
            lock (m_Table)
            {
                syntax.m_Flags &= ~UriSyntaxFlags.V1_UnknownUri;
                UriParser parser = (UriParser) m_Table[lwrCaseSchemeName];
                if (parser != null)
                {
                    throw new InvalidOperationException(SR.GetString("net_uri_AlreadyRegistered", new object[] { parser.SchemeName }));
                }
                parser = (UriParser) m_TempTable[syntax.SchemeName];
                if (parser != null)
                {
                    lwrCaseSchemeName = parser.m_Scheme;
                    m_TempTable.Remove(lwrCaseSchemeName);
                }
                syntax.OnRegister(lwrCaseSchemeName, defaultPort);
                syntax.m_Scheme = lwrCaseSchemeName;
                syntax.CheckSetIsSimpleFlag();
                syntax.m_Port = defaultPort;
                m_Table[syntax.SchemeName] = syntax;
            }
        }

        internal static UriParser FindOrFetchAsUnknownV1Syntax(string lwrCaseScheme)
        {
            UriParser parser = (UriParser) m_Table[lwrCaseScheme];
            if (parser != null)
            {
                return parser;
            }
            parser = (UriParser) m_TempTable[lwrCaseScheme];
            if (parser != null)
            {
                return parser;
            }
            lock (m_Table)
            {
                if (m_TempTable.Count >= 0x200)
                {
                    m_TempTable = new Hashtable(0x19);
                }
                parser = new BuiltInUriParser(lwrCaseScheme, -1, UriSyntaxFlags.AllowIriParsing | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.V1_UnknownUri | UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.OptionalAuthority);
                m_TempTable[lwrCaseScheme] = parser;
                return parser;
            }
        }

        protected virtual string GetComponents(Uri uri, UriComponents components, UriFormat format)
        {
            if (((components & UriComponents.SerializationInfoString) != 0) && (components != UriComponents.SerializationInfoString))
            {
                throw new ArgumentOutOfRangeException("UriComponents.SerializationInfoString");
            }
            if ((format & ~UriFormat.SafeUnescaped) != ((UriFormat) 0))
            {
                throw new ArgumentOutOfRangeException("format");
            }
            if (uri.UserDrivenParsing)
            {
                throw new InvalidOperationException(SR.GetString("net_uri_UserDrivenParsing", new object[] { base.GetType().FullName }));
            }
            if (!uri.IsAbsoluteUri)
            {
                throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
            }
            return uri.GetComponentsHelper(components, format);
        }

        internal static UriParser GetSyntax(string lwrCaseScheme)
        {
            object obj2 = m_Table[lwrCaseScheme];
            if (obj2 == null)
            {
                obj2 = m_TempTable[lwrCaseScheme];
            }
            return (UriParser) obj2;
        }

        internal bool InFact(UriSyntaxFlags flags)
        {
            return !this.IsFullMatch(flags, UriSyntaxFlags.None);
        }

        protected virtual void InitializeAndValidate(Uri uri, out UriFormatException parsingError)
        {
            parsingError = uri.ParseMinimal();
        }

        internal string InternalGetComponents(Uri thisUri, UriComponents uriComponents, UriFormat uriFormat)
        {
            return this.GetComponents(thisUri, uriComponents, uriFormat);
        }

        internal bool InternalIsBaseOf(Uri thisBaseUri, Uri uriLink)
        {
            return this.IsBaseOf(thisBaseUri, uriLink);
        }

        internal bool InternalIsWellFormedOriginalString(Uri thisUri)
        {
            return this.IsWellFormedOriginalString(thisUri);
        }

        internal UriParser InternalOnNewUri()
        {
            UriParser parser = this.OnNewUri();
            if (this != parser)
            {
                parser.m_Scheme = this.m_Scheme;
                parser.m_Port = this.m_Port;
                parser.m_Flags = this.m_Flags;
            }
            return parser;
        }

        internal string InternalResolve(Uri thisBaseUri, Uri uriLink, out UriFormatException parsingError)
        {
            return this.Resolve(thisBaseUri, uriLink, out parsingError);
        }

        internal void InternalValidate(Uri thisUri, out UriFormatException parsingError)
        {
            this.InitializeAndValidate(thisUri, out parsingError);
        }

        internal bool IsAllSet(UriSyntaxFlags flags)
        {
            return this.IsFullMatch(flags, flags);
        }

        private static bool IsAsciiLetter(char character)
        {
            return (((character >= 'a') && (character <= 'z')) || ((character >= 'A') && (character <= 'Z')));
        }

        private static bool IsAsciiLetterOrDigit(char character)
        {
            return (IsAsciiLetter(character) || ((character >= '0') && (character <= '9')));
        }

        protected virtual bool IsBaseOf(Uri baseUri, Uri relativeUri)
        {
            return baseUri.IsBaseOfHelper(relativeUri);
        }

        private bool IsFullMatch(UriSyntaxFlags flags, UriSyntaxFlags expected)
        {
            UriSyntaxFlags flags2;
            if (((flags & UriSyntaxFlags.UnEscapeDotsAndSlashes) == UriSyntaxFlags.None) || !this.m_UpdatableFlagsUsed)
            {
                flags2 = this.m_Flags;
            }
            else
            {
                flags2 = (this.m_Flags & ~UriSyntaxFlags.UnEscapeDotsAndSlashes) | ((UriSyntaxFlags) this.m_UpdatableFlags);
            }
            return ((flags2 & flags) == expected);
        }

        public static bool IsKnownScheme(string schemeName)
        {
            if (schemeName == null)
            {
                throw new ArgumentNullException("schemeName");
            }
            if (!CheckSchemeName(schemeName))
            {
                throw new ArgumentOutOfRangeException("schemeName");
            }
            UriParser syntax = GetSyntax(schemeName.ToLower(CultureInfo.InvariantCulture));
            return ((syntax != null) && syntax.NotAny(UriSyntaxFlags.V1_UnknownUri));
        }

        protected virtual bool IsWellFormedOriginalString(Uri uri)
        {
            return uri.InternalIsWellFormedOriginalString();
        }

        internal bool NotAny(UriSyntaxFlags flags)
        {
            return this.IsFullMatch(flags, UriSyntaxFlags.None);
        }

        protected virtual UriParser OnNewUri()
        {
            return this;
        }

        protected virtual void OnRegister(string schemeName, int defaultPort)
        {
        }

        public static void Register(UriParser uriParser, string schemeName, int defaultPort)
        {
            ExceptionHelper.InfrastructurePermission.Demand();
            if (uriParser == null)
            {
                throw new ArgumentNullException("uriParser");
            }
            if (schemeName == null)
            {
                throw new ArgumentNullException("schemeName");
            }
            if (schemeName.Length == 1)
            {
                throw new ArgumentOutOfRangeException("uriParser.SchemeName");
            }
            if (!CheckSchemeName(schemeName))
            {
                throw new ArgumentOutOfRangeException("schemeName");
            }
            if (((defaultPort >= 0xffff) || (defaultPort < 0)) && (defaultPort != -1))
            {
                throw new ArgumentOutOfRangeException("defaultPort");
            }
            schemeName = schemeName.ToLower(CultureInfo.InvariantCulture);
            FetchSyntax(uriParser, schemeName, defaultPort);
        }

        protected virtual string Resolve(Uri baseUri, Uri relativeUri, out UriFormatException parsingError)
        {
            if (baseUri.UserDrivenParsing)
            {
                throw new InvalidOperationException(SR.GetString("net_uri_UserDrivenParsing", new object[] { base.GetType().FullName }));
            }
            if (!baseUri.IsAbsoluteUri)
            {
                throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
            }
            string newUriString = null;
            bool userEscaped = false;
            Uri uri = Uri.ResolveHelper(baseUri, relativeUri, ref newUriString, ref userEscaped, out parsingError);
            if (parsingError != null)
            {
                return null;
            }
            if (uri != null)
            {
                return uri.OriginalString;
            }
            return newUriString;
        }

        internal void SetUpdatableFlags(UriSyntaxFlags flags)
        {
            this.m_UpdatableFlags = flags;
            this.m_UpdatableFlagsUsed = true;
        }

        internal int DefaultPort
        {
            get
            {
                return this.m_Port;
            }
        }

        internal UriSyntaxFlags Flags
        {
            get
            {
                return this.m_Flags;
            }
        }

        internal bool IsSimple
        {
            get
            {
                return this.InFact(UriSyntaxFlags.SimpleUserSyntax);
            }
        }

        internal string SchemeName
        {
            get
            {
                return this.m_Scheme;
            }
        }

        private class BuiltInUriParser : UriParser
        {
            internal BuiltInUriParser(string lwrCaseScheme, int defaultPort, UriSyntaxFlags syntaxFlags) : base((syntaxFlags | UriSyntaxFlags.SimpleUserSyntax) | UriSyntaxFlags.BuiltInSyntax)
            {
                base.m_Scheme = lwrCaseScheme;
                base.m_Port = defaultPort;
            }
        }
    }
}

