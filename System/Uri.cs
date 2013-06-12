namespace System
{
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [Serializable, TypeConverter(typeof(UriTypeConverter))]
    public class Uri : ISerializable
    {
        private static readonly char[] _WSchars = new char[] { ' ', '\n', '\r', '\t' };
        private const char c_DummyChar = '￿';
        private const short c_EncodedCharsPerByte = 3;
        private const char c_EOL = '￾';
        private const int c_Max16BitUtf8SequenceLength = 12;
        private const short c_MaxAsciiCharsReallocate = 40;
        private const short c_MaxUnicodeCharsReallocate = 40;
        private const int c_MaxUriBufferSize = 0xfff0;
        private const int c_MaxUriSchemeName = 0x400;
        private const short c_MaxUTF_8BytesPerUnicodeChar = 4;
        internal static readonly char[] HexLowerChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        private static readonly char[] HexUpperChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private string m_DnsSafeHost;
        private Flags m_Flags;
        private UriInfo m_Info;
        private bool m_iriParsing;
        private string m_originalUnicodeString;
        private string m_String;
        private UriParser m_Syntax;
        private static volatile bool s_ConfigInitialized;
        private static volatile bool s_ConfigInitializing;
        private static volatile UriIdnScope s_IdnScope = 0;
        private static object s_initLock;
        private static object s_IntranetLock = new object();
        private static volatile bool s_IriParsing = false;
        private static IInternetSecurityManager s_ManagerRef = null;
        public static readonly string SchemeDelimiter = "://";
        public static readonly string UriSchemeFile = UriParser.FileUri.SchemeName;
        public static readonly string UriSchemeFtp = UriParser.FtpUri.SchemeName;
        public static readonly string UriSchemeGopher = UriParser.GopherUri.SchemeName;
        public static readonly string UriSchemeHttp = UriParser.HttpUri.SchemeName;
        public static readonly string UriSchemeHttps = UriParser.HttpsUri.SchemeName;
        public static readonly string UriSchemeMailto = UriParser.MailToUri.SchemeName;
        public static readonly string UriSchemeNetPipe = UriParser.NetPipeUri.SchemeName;
        public static readonly string UriSchemeNetTcp = UriParser.NetTcpUri.SchemeName;
        public static readonly string UriSchemeNews = UriParser.NewsUri.SchemeName;
        public static readonly string UriSchemeNntp = UriParser.NntpUri.SchemeName;
        private const UriFormat V1ToStringUnescape = ((UriFormat) 0x7fff);

        public Uri(string uriString)
        {
            if (uriString == null)
            {
                throw new ArgumentNullException("uriString");
            }
            this.CreateThis(uriString, false, UriKind.Absolute);
        }

        protected Uri(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            string uri = serializationInfo.GetString("AbsoluteUri");
            if (uri.Length != 0)
            {
                this.CreateThis(uri, false, UriKind.Absolute);
            }
            else
            {
                uri = serializationInfo.GetString("RelativeUri");
                if (uri == null)
                {
                    throw new ArgumentNullException("uriString");
                }
                this.CreateThis(uri, false, UriKind.Relative);
            }
        }

        [Obsolete("The constructor has been deprecated. Please use new Uri(string). The dontEscape parameter is deprecated and is always false. http://go.microsoft.com/fwlink/?linkid=14202")]
        public Uri(string uriString, bool dontEscape)
        {
            if (uriString == null)
            {
                throw new ArgumentNullException("uriString");
            }
            this.CreateThis(uriString, dontEscape, UriKind.Absolute);
        }

        public Uri(string uriString, UriKind uriKind)
        {
            if (uriString == null)
            {
                throw new ArgumentNullException("uriString");
            }
            this.CreateThis(uriString, false, uriKind);
        }

        public Uri(Uri baseUri, string relativeUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
            if (!baseUri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("baseUri");
            }
            this.CreateUri(baseUri, relativeUri, false);
        }

        public Uri(Uri baseUri, Uri relativeUri)
        {
            UriFormatException exception;
            bool flag;
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
            if (!baseUri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("baseUri");
            }
            this.CreateThisFromUri(relativeUri);
            string newUriString = null;
            if (baseUri.Syntax.IsSimple)
            {
                flag = this.InFact(Flags.HostNotParsed | Flags.UserEscaped);
                relativeUri = ResolveHelper(baseUri, this, ref newUriString, ref flag, out exception);
                if (exception != null)
                {
                    throw exception;
                }
                if (relativeUri != null)
                {
                    if (relativeUri != this)
                    {
                        this.CreateThisFromUri(relativeUri);
                    }
                    return;
                }
            }
            else
            {
                flag = false;
                newUriString = baseUri.Syntax.InternalResolve(baseUri, this, out exception);
                if (exception != null)
                {
                    throw exception;
                }
            }
            this.m_Flags = Flags.HostNotParsed;
            this.m_Info = null;
            this.m_Syntax = null;
            this.CreateThis(newUriString, flag, UriKind.Absolute);
        }

        [Obsolete("The constructor has been deprecated. Please new Uri(Uri, string). The dontEscape parameter is deprecated and is always false. http://go.microsoft.com/fwlink/?linkid=14202")]
        public Uri(Uri baseUri, string relativeUri, bool dontEscape)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
            if (!baseUri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("baseUri");
            }
            this.CreateUri(baseUri, relativeUri, dontEscape);
        }

        private Uri(Flags flags, UriParser uriParser, string uri)
        {
            this.m_Flags = flags;
            this.m_Syntax = uriParser;
            this.m_String = uri;
        }

        private bool AllowIdnStatic(UriParser syntax, Flags flags)
        {
            if ((syntax == null) || ((syntax.Flags & UriSyntaxFlags.AllowIdn) == UriSyntaxFlags.None))
            {
                return false;
            }
            return ((s_IdnScope == 2) || ((s_IdnScope == 1) && StaticNotAny(flags, Flags.HostNotParsed | Flags.IntranetUri)));
        }

        internal static int CalculateCaseInsensitiveHashCode(string text)
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(text);
        }

        [Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void Canonicalize()
        {
        }

        private unsafe ushort CheckAuthorityHelper(char* pString, ushort idx, ushort length, ref ParsingError err, ref Flags flags, UriParser syntax, ref string newHost)
        {
            char ch;
            int end = length;
            int num2 = idx;
            ushort index = idx;
            newHost = null;
            bool justNormalized = false;
            bool iriParsing = s_IriParsing && IriParsingStatic(syntax);
            bool hasUnicode = (flags & Flags.HasUnicode) != Flags.HostNotParsed;
            bool flag4 = (flags & (Flags.HostNotParsed | Flags.HostUnicodeNormalized)) == Flags.HostNotParsed;
            UriSyntaxFlags flags2 = syntax.Flags;
            if ((hasUnicode && iriParsing) && flag4)
            {
                newHost = this.m_originalUnicodeString.Substring(0, num2);
            }
            if ((((idx == length) || ((ch = pString[idx]) == '/')) || ((ch == '\\') && StaticIsFile(syntax))) || ((ch == '#') || (ch == '?')))
            {
                if (syntax.InFact(UriSyntaxFlags.AllowEmptyHost))
                {
                    flags &= ~(Flags.HostNotParsed | Flags.UncPath);
                    if (StaticInFact(flags, Flags.HostNotParsed | Flags.ImplicitFile))
                    {
                        err = ParsingError.BadHostName;
                    }
                    else
                    {
                        flags |= Flags.BasicHostType;
                    }
                }
                else
                {
                    err = ParsingError.BadHostName;
                }
                if ((hasUnicode && iriParsing) && flag4)
                {
                    flags |= Flags.HostNotParsed | Flags.HostUnicodeNormalized;
                }
                return idx;
            }
            string userInfoString = null;
            if ((flags2 & UriSyntaxFlags.MayHaveUserInfo) != UriSyntaxFlags.None)
            {
                while (index < end)
                {
                    if (((index == (end - 1)) || (pString[index] == '?')) || (((pString[index] == '#') || (pString[index] == '\\')) || (pString[index] == '/')))
                    {
                        index = idx;
                        break;
                    }
                    if (pString[index] == '@')
                    {
                        flags |= Flags.HasUserInfo;
                        if (iriParsing || (s_IdnScope != null))
                        {
                            if ((iriParsing && hasUnicode) && flag4)
                            {
                                userInfoString = this.EscapeUnescapeIri(pString, num2, index + 1, UriComponents.UserInfo);
                                try
                                {
                                    userInfoString = userInfoString.Normalize(NormalizationForm.FormC);
                                }
                                catch (ArgumentException)
                                {
                                    err = ParsingError.BadFormat;
                                    return idx;
                                }
                                newHost = newHost + userInfoString;
                            }
                            else
                            {
                                userInfoString = new string(pString, num2, (index - num2) + 1);
                            }
                        }
                        index = (ushort) (index + 1);
                        ch = pString[index];
                        break;
                    }
                    index = (ushort) (index + 1);
                }
            }
            bool notCanonical = (flags2 & UriSyntaxFlags.SimpleUserSyntax) == UriSyntaxFlags.None;
            if (((ch == '[') && syntax.InFact(UriSyntaxFlags.AllowIPv6Host)) && IPv6AddressHelper.IsValid(pString, index + 1, ref end))
            {
                flags |= Flags.HostNotParsed | Flags.IPv6HostType;
                if (!s_ConfigInitialized)
                {
                    InitializeUriConfig();
                    this.m_iriParsing = s_IriParsing && IriParsingStatic(syntax);
                }
                if ((hasUnicode && iriParsing) && flag4)
                {
                    newHost = newHost + new string(pString, index, end - index);
                    flags |= Flags.HostNotParsed | Flags.HostUnicodeNormalized;
                    justNormalized = true;
                }
            }
            else if (((ch <= '9') && (ch >= '0')) && (syntax.InFact(UriSyntaxFlags.AllowIPv4Host) && IPv4AddressHelper.IsValid(pString, index, ref end, false, StaticNotAny(flags, Flags.HostNotParsed | Flags.ImplicitFile))))
            {
                flags |= Flags.HostNotParsed | Flags.IPv4HostType;
                if ((hasUnicode && iriParsing) && flag4)
                {
                    newHost = newHost + new string(pString, index, end - index);
                    flags |= Flags.HostNotParsed | Flags.HostUnicodeNormalized;
                    justNormalized = true;
                }
            }
            else if ((((flags2 & UriSyntaxFlags.AllowDnsHost) != UriSyntaxFlags.None) && !iriParsing) && DomainNameHelper.IsValid(pString, index, ref end, ref notCanonical, StaticNotAny(flags, Flags.HostNotParsed | Flags.ImplicitFile)))
            {
                flags |= Flags.DnsHostType;
                if (!notCanonical)
                {
                    flags |= Flags.CanonicalDnsHost;
                }
                if (s_IdnScope != null)
                {
                    if ((s_IdnScope == 1) && this.IsIntranet(new string(pString, 0, end)))
                    {
                        flags |= Flags.HostNotParsed | Flags.IntranetUri;
                    }
                    if (this.AllowIdnStatic(syntax, flags))
                    {
                        bool allAscii = true;
                        bool atLeastOneValidIdn = false;
                        string str2 = DomainNameHelper.UnicodeEquivalent(pString, index, end, ref allAscii, ref atLeastOneValidIdn);
                        if (atLeastOneValidIdn)
                        {
                            if (StaticNotAny(flags, Flags.HasUnicode))
                            {
                                this.m_originalUnicodeString = this.m_String;
                            }
                            flags |= Flags.HostNotParsed | Flags.IdnHost;
                            newHost = this.m_originalUnicodeString.Substring(0, num2) + userInfoString + str2;
                            flags |= Flags.CanonicalDnsHost;
                            this.m_DnsSafeHost = new string(pString, index, end - index);
                            justNormalized = true;
                        }
                        flags |= Flags.HostNotParsed | Flags.HostUnicodeNormalized;
                    }
                }
            }
            else if ((((iriParsing || (s_IdnScope != null)) && ((flags2 & UriSyntaxFlags.AllowDnsHost) != UriSyntaxFlags.None)) && ((iriParsing && flag4) || this.AllowIdnStatic(syntax, flags))) && DomainNameHelper.IsValidByIri(pString, index, ref end, ref notCanonical, StaticNotAny(flags, Flags.HostNotParsed | Flags.ImplicitFile)))
            {
                this.CheckAuthorityHelperHandleDnsIri(pString, index, end, num2, iriParsing, hasUnicode, syntax, userInfoString, ref flags, ref justNormalized, ref newHost, ref err);
            }
            else if ((((s_IdnScope == null) && !s_IriParsing) && (((flags2 & UriSyntaxFlags.AllowUncHost) != UriSyntaxFlags.None) && UncNameHelper.IsValid(pString, index, ref end, StaticNotAny(flags, Flags.HostNotParsed | Flags.ImplicitFile)))) && ((end - index) <= 0x100))
            {
                flags |= Flags.HostNotParsed | Flags.UncHostType;
            }
            if (((end < length) && (pString[end] == '\\')) && (((flags & (Flags.BasicHostType | Flags.IPv4HostType)) != Flags.HostNotParsed) && !StaticIsFile(syntax)))
            {
                if (syntax.InFact(UriSyntaxFlags.V1_UnknownUri))
                {
                    err = ParsingError.BadHostName;
                    flags |= Flags.BasicHostType | Flags.IPv4HostType;
                    return (ushort) end;
                }
                flags &= ~(Flags.BasicHostType | Flags.IPv4HostType);
            }
            else if ((end < length) && (pString[end] == ':'))
            {
                if (!syntax.InFact(UriSyntaxFlags.MayHavePort))
                {
                    flags &= ~(Flags.BasicHostType | Flags.IPv4HostType);
                }
                else
                {
                    int num4 = 0;
                    int startIndex = end;
                    idx = (ushort) (end + 1);
                    while (idx < length)
                    {
                        ushort num6 = pString[idx] - '0';
                        if ((num6 >= 0) && (num6 <= 9))
                        {
                            num4 = (num4 * 10) + num6;
                            if (num4 > 0xffff)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (((num6 == 0xffff) || (num6 == 15)) || (num6 == 0xfff3))
                            {
                                break;
                            }
                            if (syntax.InFact(UriSyntaxFlags.AllowAnyOtherHost) && syntax.NotAny(UriSyntaxFlags.V1_UnknownUri))
                            {
                                flags &= ~(Flags.BasicHostType | Flags.IPv4HostType);
                                break;
                            }
                            err = ParsingError.BadPort;
                            return idx;
                        }
                        idx = (ushort) (idx + 1);
                    }
                    if (num4 > 0xffff)
                    {
                        if (!syntax.InFact(UriSyntaxFlags.AllowAnyOtherHost))
                        {
                            err = ParsingError.BadPort;
                            return idx;
                        }
                        flags &= ~(Flags.BasicHostType | Flags.IPv4HostType);
                    }
                    if ((iriParsing && hasUnicode) && justNormalized)
                    {
                        newHost = newHost + new string(pString, startIndex, idx - startIndex);
                    }
                }
            }
            if ((flags & (Flags.BasicHostType | Flags.IPv4HostType)) == Flags.HostNotParsed)
            {
                flags &= ~Flags.HasUserInfo;
                if (!syntax.InFact(UriSyntaxFlags.AllowAnyOtherHost))
                {
                    if (!syntax.InFact(UriSyntaxFlags.V1_UnknownUri))
                    {
                        if (syntax.InFact(UriSyntaxFlags.MustHaveAuthority))
                        {
                            err = ParsingError.BadHostName;
                            flags |= Flags.BasicHostType | Flags.IPv4HostType;
                            return idx;
                        }
                    }
                    else
                    {
                        bool flag8 = false;
                        int num7 = idx;
                        for (end = idx; end < length; end++)
                        {
                            if (flag8 && (((pString[end] == '/') || (pString[end] == '?')) || (pString[end] == '#')))
                            {
                                break;
                            }
                            if ((end < (idx + 2)) && (pString[end] == '.'))
                            {
                                flag8 = true;
                            }
                            else
                            {
                                err = ParsingError.BadHostName;
                                flags |= Flags.BasicHostType | Flags.IPv4HostType;
                                return idx;
                            }
                        }
                        flags |= Flags.BasicHostType;
                        if ((iriParsing && hasUnicode) && StaticNotAny(flags, Flags.HostNotParsed | Flags.HostUnicodeNormalized))
                        {
                            string str3 = new string(pString, num7, num7 - end);
                            try
                            {
                                newHost = newHost + str3.Normalize(NormalizationForm.FormC);
                            }
                            catch (ArgumentException)
                            {
                                err = ParsingError.BadFormat;
                                return idx;
                            }
                            flags |= Flags.HostNotParsed | Flags.HostUnicodeNormalized;
                        }
                    }
                }
                else
                {
                    flags |= Flags.BasicHostType;
                    end = idx;
                    while (end < length)
                    {
                        if (((pString[end] == '/') || (pString[end] == '?')) || (pString[end] == '#'))
                        {
                            break;
                        }
                        end++;
                    }
                    this.CheckAuthorityHelperHandleAnyHostIri(pString, num2, end, iriParsing, hasUnicode, syntax, ref flags, ref newHost, ref err);
                }
            }
            return (ushort) end;
        }

        private unsafe void CheckAuthorityHelperHandleAnyHostIri(char* pString, int startInput, int end, bool iriParsing, bool hasUnicode, UriParser syntax, ref Flags flags, ref string newHost, ref ParsingError err)
        {
            if (StaticNotAny(flags, Flags.HostNotParsed | Flags.HostUnicodeNormalized) && (this.AllowIdnStatic(syntax, flags) || (iriParsing && hasUnicode)))
            {
                string str = new string(pString, startInput, end - startInput);
                if (this.AllowIdnStatic(syntax, flags))
                {
                    bool allAscii = true;
                    bool atLeastOneValidIdn = false;
                    string str2 = DomainNameHelper.UnicodeEquivalent(pString, startInput, end, ref allAscii, ref atLeastOneValidIdn);
                    if (((allAscii && atLeastOneValidIdn) || !allAscii) && (!iriParsing || !hasUnicode))
                    {
                        this.m_originalUnicodeString = this.m_String;
                        newHost = this.m_originalUnicodeString.Substring(0, startInput);
                        flags |= Flags.HasUnicode;
                    }
                    if (atLeastOneValidIdn || !allAscii)
                    {
                        newHost = newHost + str2;
                        string bidiStrippedHost = null;
                        this.m_DnsSafeHost = DomainNameHelper.IdnEquivalent(pString, startInput, end, ref allAscii, ref bidiStrippedHost);
                        if (atLeastOneValidIdn)
                        {
                            flags |= Flags.HostNotParsed | Flags.IdnHost;
                        }
                        if (!allAscii)
                        {
                            flags |= Flags.HostNotParsed | Flags.UnicodeHost;
                        }
                    }
                    else if (iriParsing && hasUnicode)
                    {
                        newHost = newHost + str;
                    }
                }
                else
                {
                    try
                    {
                        newHost = newHost + str.Normalize(NormalizationForm.FormC);
                    }
                    catch (ArgumentException)
                    {
                        err = ParsingError.BadHostName;
                    }
                }
                flags |= Flags.HostNotParsed | Flags.HostUnicodeNormalized;
            }
        }

        private unsafe void CheckAuthorityHelperHandleDnsIri(char* pString, ushort start, int end, int startInput, bool iriParsing, bool hasUnicode, UriParser syntax, string userInfoString, ref Flags flags, ref bool justNormalized, ref string newHost, ref ParsingError err)
        {
            flags |= Flags.DnsHostType;
            if ((s_IdnScope == 1) && this.IsIntranet(new string(pString, 0, end)))
            {
                flags |= Flags.HostNotParsed | Flags.IntranetUri;
            }
            if (this.AllowIdnStatic(syntax, flags))
            {
                bool allAscii = true;
                bool atLeastOneValidIdn = false;
                string idnHost = DomainNameHelper.IdnEquivalent(pString, start, end, ref allAscii, ref atLeastOneValidIdn);
                string str2 = DomainNameHelper.UnicodeEquivalent(idnHost, pString, start, end);
                if (!allAscii)
                {
                    flags |= Flags.HostNotParsed | Flags.UnicodeHost;
                }
                if (atLeastOneValidIdn)
                {
                    flags |= Flags.HostNotParsed | Flags.IdnHost;
                }
                if ((allAscii && atLeastOneValidIdn) && StaticNotAny(flags, Flags.HasUnicode))
                {
                    this.m_originalUnicodeString = this.m_String;
                    newHost = this.m_originalUnicodeString.Substring(0, startInput) + (StaticInFact(flags, Flags.HasUserInfo) ? userInfoString : null);
                    justNormalized = true;
                }
                else if (!iriParsing && (StaticInFact(flags, Flags.HostNotParsed | Flags.UnicodeHost) || StaticInFact(flags, Flags.HostNotParsed | Flags.IdnHost)))
                {
                    this.m_originalUnicodeString = this.m_String;
                    newHost = this.m_originalUnicodeString.Substring(0, startInput) + (StaticInFact(flags, Flags.HasUserInfo) ? userInfoString : null);
                    justNormalized = true;
                }
                if (!allAscii || atLeastOneValidIdn)
                {
                    this.m_DnsSafeHost = idnHost;
                    newHost = newHost + str2;
                    justNormalized = true;
                }
                else if ((allAscii && !atLeastOneValidIdn) && (iriParsing && hasUnicode))
                {
                    newHost = newHost + str2;
                    justNormalized = true;
                }
            }
            else if (hasUnicode)
            {
                string str3 = StripBidiControlCharacter(pString, start, end - start);
                try
                {
                    newHost = newHost + ((str3 != null) ? str3.Normalize(NormalizationForm.FormC) : null);
                }
                catch (ArgumentException)
                {
                    err = ParsingError.BadHostName;
                }
                justNormalized = true;
            }
            flags |= Flags.HostNotParsed | Flags.HostUnicodeNormalized;
        }

        private unsafe Check CheckCanonical(char* str, ref ushort idx, ushort end, char delim)
        {
            Check none = Check.None;
            bool flag = false;
            bool flag2 = false;
            char c = 0xffff;
            ushort index = idx;
            while (index < end)
            {
                c = str[index];
                if ((c <= '\x001f') || ((c >= '\x007f') && (c <= '\x009f')))
                {
                    flag = true;
                    flag2 = true;
                    none |= Check.ReservedFound;
                }
                else if ((c > 'z') && (c != '~'))
                {
                    if (this.m_iriParsing)
                    {
                        bool flag3 = false;
                        none |= Check.FoundNonAscii;
                        if (char.IsHighSurrogate(c))
                        {
                            if ((index + 1) < end)
                            {
                                bool surrogatePair = false;
                                flag3 = CheckIriUnicodeRange(c, str[index + 1], ref surrogatePair, true);
                            }
                        }
                        else
                        {
                            flag3 = CheckIriUnicodeRange(c, true);
                        }
                        if (!flag3)
                        {
                            none |= Check.NotIriCanonical;
                        }
                    }
                    if (!flag)
                    {
                        flag = true;
                    }
                }
                else
                {
                    if ((c == delim) || (((delim == '?') && (c == '#')) && ((this.m_Syntax != null) && this.m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment))))
                    {
                        break;
                    }
                    if (c == '?')
                    {
                        if (this.IsImplicitFile || (((this.m_Syntax != null) && !this.m_Syntax.InFact(UriSyntaxFlags.MayHaveQuery)) && (delim != 0xfffe)))
                        {
                            none |= Check.ReservedFound;
                            flag2 = true;
                            flag = true;
                        }
                    }
                    else if (c == '#')
                    {
                        flag = true;
                        if (this.IsImplicitFile || ((this.m_Syntax != null) && !this.m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment)))
                        {
                            none |= Check.ReservedFound;
                            flag2 = true;
                        }
                    }
                    else if ((c == '/') || (c == '\\'))
                    {
                        if (((none & Check.BackslashInPath) == Check.None) && (c == '\\'))
                        {
                            none |= Check.BackslashInPath;
                        }
                        if ((((none & Check.DotSlashAttn) == Check.None) && ((index + 1) != end)) && ((str[index + 1] == '/') || (str[index + 1] == '\\')))
                        {
                            none |= Check.DotSlashAttn;
                        }
                    }
                    else if (c == '.')
                    {
                        if ((((none & Check.DotSlashAttn) == Check.None) && ((index + 1) == end)) || (((str[index + 1] == '.') || (str[index + 1] == '/')) || (((str[index + 1] == '\\') || (str[index + 1] == '?')) || (str[index + 1] == '#'))))
                        {
                            none |= Check.DotSlashAttn;
                        }
                    }
                    else if (!flag && ((((c <= '"') && (c != '!')) || ((c >= '[') && (c <= '^'))) || (((c == '>') || (c == '<')) || (c == '`'))))
                    {
                        flag = true;
                    }
                    else if (c == '%')
                    {
                        if (!flag2)
                        {
                            flag2 = true;
                        }
                        if (((index + 2) < end) && ((c = EscapedAscii(str[index + 1], str[index + 2])) != 0xffff))
                        {
                            if (((c == '.') || (c == '/')) || (c == '\\'))
                            {
                                none |= Check.DotSlashEscaped;
                            }
                            index = (ushort) (index + 2);
                        }
                        else if (!flag)
                        {
                            flag = true;
                        }
                    }
                }
                index = (ushort) (index + 1);
            }
            if (flag2)
            {
                if (!flag)
                {
                    none |= Check.EscapedCanonical;
                }
            }
            else
            {
                none |= Check.DisplayCanonical;
                if (!flag)
                {
                    none |= Check.EscapedCanonical;
                }
            }
            idx = index;
            return none;
        }

        private static bool CheckForColonInFirstPathSegment(string uriString)
        {
            char[] anyOf = new char[] { ':', '\\', '/', '?', '#' };
            int num = uriString.IndexOfAny(anyOf);
            return ((num >= 0) && (uriString[num] == ':'));
        }

        private unsafe bool CheckForConfigLoad(string data)
        {
            bool flag = false;
            int length = data.Length;
            fixed (char* str = ((char*) data))
            {
                char* chPtr = str;
                for (int i = 0; i < length; i++)
                {
                    if (((chPtr[i] > '\x007f') || (chPtr[i] == '%')) || ((((chPtr[i] == 'x') && ((i + 3) < length)) && ((chPtr[i + 1] == 'n') && (chPtr[i + 2] == '-'))) && (chPtr[i + 3] == '-')))
                    {
                        flag = true;
                        goto Label_0077;
                    }
                }
            }
        Label_0077:;
            return flag;
        }

        private unsafe bool CheckForUnicode(string data)
        {
            bool flag = false;
            char[] dest = new char[data.Length];
            int length = 0;
            string str = new string(UnescapeString(data, 0, data.Length, dest, ref length, 0xffff, 0xffff, 0xffff, UnescapeMode.UnescapeAll | UnescapeMode.Unescape, null, false, false), 0, length);
            int num2 = str.Length;
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                for (int i = 0; i < num2; i++)
                {
                    if (chPtr[i] > '\x007f')
                    {
                        flag = true;
                        goto Label_0079;
                    }
                }
            }
        Label_0079:;
            return flag;
        }

        public static unsafe UriHostNameType CheckHostName(string name)
        {
            if (((name != null) && (name.Length != 0)) && (name.Length <= 0x7fff))
            {
                int length = name.Length;
                fixed (char* str = ((char*) name))
                {
                    char* chPtr = str;
                    if (((name[0] == '[') && (name[name.Length - 1] == ']')) && (IPv6AddressHelper.IsValid(chPtr, 1, ref length) && (length == name.Length)))
                    {
                        return UriHostNameType.IPv6;
                    }
                    length = name.Length;
                    if (IPv4AddressHelper.IsValid(chPtr, 0, ref length, false, false) && (length == name.Length))
                    {
                        return UriHostNameType.IPv4;
                    }
                    length = name.Length;
                    bool notCanonical = false;
                    if (DomainNameHelper.IsValid(chPtr, 0, ref length, ref notCanonical, false) && (length == name.Length))
                    {
                        return UriHostNameType.Dns;
                    }
                }
                length = name.Length + 2;
                name = "[" + name + "]";
                fixed (char* str2 = ((char*) name))
                {
                    char* chPtr2 = str2;
                    if (IPv6AddressHelper.IsValid(chPtr2, 1, ref length) && (length == name.Length))
                    {
                        return UriHostNameType.IPv6;
                    }
                }
            }
            return UriHostNameType.Unknown;
        }

        internal static bool CheckIriUnicodeRange(char unicode, bool isQuery)
        {
            if ((((unicode < '\x00a0') || (unicode > 0xd7ff)) && ((unicode < 0xf900) || (unicode > 0xfdcf))) && (((unicode < 0xfdf0) || (unicode > 0xffef)) && ((!isQuery || (unicode < 0xe000)) || (unicode > 0xf8ff))))
            {
                return false;
            }
            return true;
        }

        internal static bool CheckIriUnicodeRange(char highSurr, char lowSurr, ref bool surrogatePair, bool isQuery)
        {
            bool flag = false;
            surrogatePair = false;
            if (!CheckIriUnicodeRange(highSurr, isQuery))
            {
                if (!char.IsHighSurrogate(highSurr) || !char.IsSurrogatePair(highSurr, lowSurr))
                {
                    return flag;
                }
                surrogatePair = true;
                char[] chArray = new char[] { highSurr, lowSurr };
                string str = new string(chArray);
                if ((((((str.CompareTo("\ud800\udc00") < 0) || (str.CompareTo("\ud83f\udffd") > 0)) && ((str.CompareTo("\ud840\udc00") < 0) || (str.CompareTo("\ud87f\udffd") > 0))) && (((str.CompareTo("\ud880\udc00") < 0) || (str.CompareTo("\ud8bf\udffd") > 0)) && ((str.CompareTo("\ud8c0\udc00") < 0) || (str.CompareTo("\ud8ff\udffd") > 0)))) && ((((str.CompareTo("\ud900\udc00") < 0) || (str.CompareTo("\ud93f\udffd") > 0)) && ((str.CompareTo("\ud940\udc00") < 0) || (str.CompareTo("\ud97f\udffd") > 0))) && (((str.CompareTo("\ud980\udc00") < 0) || (str.CompareTo("\ud9bf\udffd") > 0)) && ((str.CompareTo("\ud9c0\udc00") < 0) || (str.CompareTo("\ud9ff\udffd") > 0))))) && (((((str.CompareTo("\uda00\udc00") < 0) || (str.CompareTo("\uda3f\udffd") > 0)) && ((str.CompareTo("\uda40\udc00") < 0) || (str.CompareTo("\uda7f\udffd") > 0))) && (((str.CompareTo("\uda80\udc00") < 0) || (str.CompareTo("\udabf\udffd") > 0)) && ((str.CompareTo("\udac0\udc00") < 0) || (str.CompareTo("\udaff\udffd") > 0)))) && (((str.CompareTo("\udb00\udc00") < 0) || (str.CompareTo("\udb3f\udffd") > 0)) && ((str.CompareTo("\udb40\udc00") < 0) || (str.CompareTo("\udb7f\udffd") > 0)))))
                {
                    if (!isQuery)
                    {
                        return flag;
                    }
                    if (((str.CompareTo("\udb80\udc00") < 0) || (str.CompareTo("\udbbf\udffd") > 0)) && ((str.CompareTo("\udbc0\udc00") < 0) || (str.CompareTo("\udbff\udffd") > 0)))
                    {
                        return flag;
                    }
                }
            }
            return true;
        }

        internal static bool CheckIriUnicodeRange(string uri, int offset, ref bool surrogatePair, bool isQuery)
        {
            char ch = 0xffff;
            return CheckIriUnicodeRange(uri[offset], ((offset + 1) < uri.Length) ? uri[offset + 1] : ch, ref surrogatePair, isQuery);
        }

        internal bool CheckIsReserved(char ch)
        {
            char[] chArray = new char[] { ':', '/', '?', '#', '[', ']', '@' };
            for (int i = 0; i < chArray.Length; i++)
            {
                if (chArray[i] == ch)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool CheckIsReserved(char ch, UriComponents component)
        {
            if ((((component != UriComponents.Scheme) || (component != UriComponents.UserInfo)) || ((component != UriComponents.Host) || (component != UriComponents.Port))) || (((component != UriComponents.Path) || (component != UriComponents.Query)) || (component != UriComponents.Fragment)))
            {
                if (component != 0)
                {
                    return false;
                }
                return this.CheckIsReserved(ch);
            }
            switch (component)
            {
                case UriComponents.Query:
                    if (((ch != '#') && (ch != '[')) && (ch != ']'))
                    {
                        break;
                    }
                    return true;

                case UriComponents.Fragment:
                    if (((ch != '#') && (ch != '[')) && (ch != ']'))
                    {
                        break;
                    }
                    return true;

                case UriComponents.UserInfo:
                    if ((((ch != '/') && (ch != '?')) && ((ch != '#') && (ch != '['))) && ((ch != ']') && (ch != '@')))
                    {
                        break;
                    }
                    return true;

                case UriComponents.Host:
                    if ((((ch != ':') && (ch != '/')) && ((ch != '?') && (ch != '#'))) && (((ch != '[') && (ch != ']')) && (ch != '@')))
                    {
                        break;
                    }
                    return true;

                case UriComponents.Path:
                    if (((ch == '/') || (ch == '?')) || (((ch == '#') || (ch == '[')) || (ch == ']')))
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        private static unsafe bool CheckKnownSchemes(long* lptr, ushort nChars, ref UriParser syntax)
        {
            switch ((lptr[0] | 0x20002000200020L))
            {
                case 0x64006900750075L:
                    if (nChars != 4)
                    {
                        break;
                    }
                    syntax = UriParser.UuidUri;
                    return true;

                case 0x65006c00690066L:
                    if (nChars != 4)
                    {
                        break;
                    }
                    syntax = UriParser.FileUri;
                    return true;

                case 0x680070006f0067L:
                    if ((nChars != 6) || ((*(((int*) (lptr + 1))) | 0x200020) != 0x720065))
                    {
                        break;
                    }
                    syntax = UriParser.GopherUri;
                    return true;

                case 0x2e00740065006eL:
                    if ((nChars == 8) && ((lptr[1] | 0x20002000200020L) == 0x65007000690070L))
                    {
                        syntax = UriParser.NetPipeUri;
                        return true;
                    }
                    if ((nChars != 7) || ((lptr[1] | 0x20002000200020L) != 0x3a007000630074L))
                    {
                        break;
                    }
                    syntax = UriParser.NetTcpUri;
                    return true;

                case 0x3a007000740066L:
                    if (nChars != 3)
                    {
                        break;
                    }
                    syntax = UriParser.FtpUri;
                    return true;

                case 0x6c00690061006dL:
                    if ((nChars != 6) || ((*(((int*) (lptr + 1))) | 0x200020) != 0x6f0074))
                    {
                        break;
                    }
                    syntax = UriParser.MailToUri;
                    return true;

                case 0x6e006c00650074L:
                    if ((nChars != 6) || ((*(((int*) (lptr + 1))) | 0x200020) != 0x740065))
                    {
                        break;
                    }
                    syntax = UriParser.TelnetUri;
                    return true;

                case 0x7000610064006cL:
                    if (nChars == 4)
                    {
                        syntax = UriParser.LdapUri;
                        return true;
                    }
                    break;

                case 0x700074006e006eL:
                    if (nChars != 4)
                    {
                        break;
                    }
                    syntax = UriParser.NntpUri;
                    return true;

                case 0x70007400740068L:
                    if (nChars == 4)
                    {
                        syntax = UriParser.HttpUri;
                        return true;
                    }
                    if ((nChars != 5) || ((*(((ushort*) (lptr + 1))) | 0x20) != 0x73))
                    {
                        break;
                    }
                    syntax = UriParser.HttpsUri;
                    return true;

                case 0x7300770065006eL:
                    if (nChars != 4)
                    {
                        break;
                    }
                    syntax = UriParser.NewsUri;
                    return true;
            }
            return false;
        }

        public static bool CheckSchemeName(string schemeName)
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

        private static unsafe ParsingError CheckSchemeSyntax(char* ptr, ushort length, ref UriParser syntax)
        {
            char ch = ptr[0];
            if ((ch < 'a') || (ch > 'z'))
            {
                if ((ch < 'A') || (ch > 'Z'))
                {
                    return ParsingError.BadScheme;
                }
                ptr[0] = (char) (ch | ' ');
            }
            for (ushort i = 1; i < length; i = (ushort) (i + 1))
            {
                char ch2 = ptr[i];
                if ((ch2 < 'a') || (ch2 > 'z'))
                {
                    if ((ch2 >= 'A') && (ch2 <= 'Z'))
                    {
                        ptr[i] = (char) (ch2 | ' ');
                    }
                    else if (((ch2 < '0') || (ch2 > '9')) && (((ch2 != '+') && (ch2 != '-')) && (ch2 != '.')))
                    {
                        return ParsingError.BadScheme;
                    }
                }
            }
            string lwrCaseScheme = new string(ptr, 0, length);
            syntax = UriParser.FindOrFetchAsUnknownV1Syntax(lwrCaseScheme);
            return ParsingError.None;
        }

        [Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void CheckSecurity()
        {
            bool flag1 = this.Scheme == "telnet";
        }

        private static string CombineUri(Uri basePart, string relativePart, UriFormat uriFormat)
        {
            string parts;
            int length;
            char[] chArray;
            char ch = relativePart[0];
            if ((basePart.IsDosPath && ((ch == '/') || (ch == '\\'))) && ((relativePart.Length == 1) || ((relativePart[1] != '/') && (relativePart[1] != '\\'))))
            {
                int index = basePart.OriginalString.IndexOf(':');
                if (!basePart.IsImplicitFile)
                {
                    index = basePart.OriginalString.IndexOf(':', index + 1);
                }
                return (basePart.OriginalString.Substring(0, index + 1) + relativePart);
            }
            if (!StaticIsFile(basePart.Syntax) || ((ch != '\\') && (ch != '/')))
            {
                bool flag = basePart.Syntax.InFact(UriSyntaxFlags.ConvertPathSlashes);
                parts = null;
                if ((ch == '/') || ((ch == '\\') && flag))
                {
                    if ((relativePart.Length >= 2) && (relativePart[1] == '/'))
                    {
                        return (basePart.Scheme + ':' + relativePart);
                    }
                    if (basePart.HostType == (Flags.HostNotParsed | Flags.IPv6HostType))
                    {
                        parts = string.Concat(new object[] { basePart.GetParts(UriComponents.UserInfo | UriComponents.Scheme, uriFormat), '[', basePart.DnsSafeHost, ']', basePart.GetParts(UriComponents.KeepDelimiter | UriComponents.Port, uriFormat) });
                    }
                    else
                    {
                        parts = basePart.GetParts(UriComponents.SchemeAndServer | UriComponents.UserInfo, uriFormat);
                    }
                    if (flag && (ch == '\\'))
                    {
                        relativePart = '/' + relativePart.Substring(1);
                    }
                    return (parts + relativePart);
                }
                parts = basePart.GetParts(UriComponents.KeepDelimiter | UriComponents.Path, basePart.IsImplicitFile ? UriFormat.Unescaped : uriFormat);
                length = parts.Length;
                chArray = new char[length + relativePart.Length];
                if (length > 0)
                {
                    parts.CopyTo(0, chArray, 0, length);
                    while (length > 0)
                    {
                        if (chArray[--length] == '/')
                        {
                            length++;
                            break;
                        }
                    }
                }
            }
            else
            {
                if ((relativePart.Length >= 2) && ((relativePart[1] == '\\') || (relativePart[1] == '/')))
                {
                    if (!basePart.IsImplicitFile)
                    {
                        return ("file:" + relativePart);
                    }
                    return relativePart;
                }
                if (!basePart.IsUnc)
                {
                    return ("file://" + relativePart);
                }
                string str = basePart.GetParts(UriComponents.KeepDelimiter | UriComponents.Path, UriFormat.Unescaped);
                for (int i = 1; i < str.Length; i++)
                {
                    if (str[i] == '/')
                    {
                        str = str.Substring(0, i);
                        break;
                    }
                }
                if (basePart.IsImplicitFile)
                {
                    return (@"\\" + basePart.GetParts(UriComponents.Host, UriFormat.Unescaped) + str + relativePart);
                }
                return ("file://" + basePart.GetParts(UriComponents.Host, uriFormat) + str + relativePart);
            }
            relativePart.CopyTo(0, chArray, length, relativePart.Length);
            ch = basePart.Syntax.InFact(UriSyntaxFlags.MayHaveQuery) ? '?' : ((char) 0xffff);
            char ch2 = (!basePart.IsImplicitFile && basePart.Syntax.InFact(UriSyntaxFlags.MayHaveFragment)) ? '#' : ((char) 0xffff);
            string str3 = string.Empty;
            if ((ch == 0xffff) && (ch2 == 0xffff))
            {
                length += relativePart.Length;
            }
            else
            {
                int startIndex = 0;
                while (startIndex < relativePart.Length)
                {
                    if ((chArray[length + startIndex] == ch) || (chArray[length + startIndex] == ch2))
                    {
                        break;
                    }
                    startIndex++;
                }
                if (startIndex == 0)
                {
                    str3 = relativePart;
                }
                else if (startIndex < relativePart.Length)
                {
                    str3 = relativePart.Substring(startIndex);
                }
                length += startIndex;
            }
            if (basePart.HostType == (Flags.HostNotParsed | Flags.IPv6HostType))
            {
                if (basePart.IsImplicitFile)
                {
                    parts = @"\\[" + basePart.DnsSafeHost + ']';
                }
                else
                {
                    parts = string.Concat(new object[] { basePart.GetParts(UriComponents.UserInfo | UriComponents.Scheme, uriFormat), '[', basePart.DnsSafeHost, ']', basePart.GetParts(UriComponents.KeepDelimiter | UriComponents.Port, uriFormat) });
                }
            }
            else if (basePart.IsImplicitFile)
            {
                if (basePart.IsDosPath)
                {
                    return (new string(Compress(chArray, 3, ref length, basePart.Syntax), 1, length - 1) + str3);
                }
                parts = @"\\" + basePart.GetParts(UriComponents.Host, UriFormat.Unescaped);
            }
            else
            {
                parts = basePart.GetParts(UriComponents.SchemeAndServer | UriComponents.UserInfo, uriFormat);
            }
            chArray = Compress(chArray, basePart.SecuredPathIndex, ref length, basePart.Syntax);
            return (parts + new string(chArray, 0, length) + str3);
        }

        public static int Compare(Uri uri1, Uri uri2, UriComponents partsToCompare, UriFormat compareFormat, StringComparison comparisonType)
        {
            if (uri1 == null)
            {
                if (uri2 == null)
                {
                    return 0;
                }
                return -1;
            }
            if (uri2 == null)
            {
                return 1;
            }
            if (uri1.IsAbsoluteUri && uri2.IsAbsoluteUri)
            {
                return string.Compare(uri1.GetParts(partsToCompare, compareFormat), uri2.GetParts(partsToCompare, compareFormat), comparisonType);
            }
            if (uri1.IsAbsoluteUri)
            {
                return 1;
            }
            if (!uri2.IsAbsoluteUri)
            {
                return string.Compare(uri1.OriginalString, uri2.OriginalString, comparisonType);
            }
            return -1;
        }

        private static char[] Compress(char[] dest, ushort start, ref int destLength, UriParser syntax)
        {
            ushort num = 0;
            ushort num2 = 0;
            ushort num3 = 0;
            ushort num4 = 0;
            ushort index = (ushort) (((ushort) destLength) - 1);
            start = (ushort) (start - 1);
            while (index != start)
            {
                char ch = dest[index];
                if ((ch == '\\') && syntax.InFact(UriSyntaxFlags.ConvertPathSlashes))
                {
                    dest[index] = ch = '/';
                }
                if (ch == '/')
                {
                    num = (ushort) (num + 1);
                }
                else
                {
                    if (num > 1)
                    {
                        num2 = (ushort) (index + 1);
                    }
                    num = 0;
                }
                if (ch == '.')
                {
                    num3 = (ushort) (num3 + 1);
                    goto Label_017F;
                }
                if (num3 == 0)
                {
                    goto Label_0148;
                }
                bool flag = syntax.NotAny(UriSyntaxFlags.CanonicalizeAsFilePath) && (((num3 > 2) || (ch != '/')) || (index == start));
                if (!flag && (ch == '/'))
                {
                    if ((num2 != ((index + num3) + 1)) && ((num2 != 0) || (((index + num3) + 1) != destLength)))
                    {
                        goto Label_0146;
                    }
                    num2 = (ushort) (((index + 1) + num3) + ((num2 == 0) ? 0 : 1));
                    Buffer.BlockCopy(dest, num2 << 1, dest, (index + 1) << 1, (destLength - num2) << 1);
                    destLength -= (num2 - index) - 1;
                    num2 = index;
                    if (num3 == 2)
                    {
                        num4 = (ushort) (num4 + 1);
                    }
                    num3 = 0;
                    goto Label_017F;
                }
                if ((!flag && (num4 == 0)) && ((num2 == ((index + num3) + 1)) || ((num2 == 0) && (((index + num3) + 1) == destLength))))
                {
                    num3 = (ushort) ((index + 1) + num3);
                    Buffer.BlockCopy(dest, num3 << 1, dest, (index + 1) << 1, (destLength - num3) << 1);
                    destLength -= (num3 - index) - 1;
                    num2 = 0;
                    num3 = 0;
                    goto Label_017F;
                }
            Label_0146:
                num3 = 0;
            Label_0148:
                if (ch == '/')
                {
                    if (num4 != 0)
                    {
                        num4 = (ushort) (num4 - 1);
                        num2 = (ushort) (num2 + 1);
                        Buffer.BlockCopy(dest, num2 << 1, dest, (index + 1) << 1, (destLength - num2) << 1);
                        destLength -= (num2 - index) - 1;
                    }
                    num2 = index;
                }
            Label_017F:
                index = (ushort) (index - 1);
            }
            start = (ushort) (start + 1);
            if (((((ushort) destLength) > start) && syntax.InFact(UriSyntaxFlags.CanonicalizeAsFilePath)) && (num <= 1))
            {
                if ((num4 != 0) && (dest[start] != '/'))
                {
                    num2 = (ushort) (num2 + 1);
                    Buffer.BlockCopy(dest, num2 << 1, dest, start << 1, (destLength - num2) << 1);
                    destLength -= num2;
                    return dest;
                }
                if ((num3 != 0) && ((num2 == (num3 + 1)) || ((num2 == 0) && ((num3 + 1) == destLength))))
                {
                    num3 = (ushort) (num3 + ((num2 == 0) ? 0 : 1));
                    Buffer.BlockCopy(dest, num3 << 1, dest, start << 1, (destLength - num3) << 1);
                    destLength -= num3;
                }
            }
            return dest;
        }

        internal static Uri CreateHelper(string uriString, bool dontEscape, UriKind uriKind, ref UriFormatException e)
        {
            if ((uriKind < UriKind.RelativeOrAbsolute) || (uriKind > UriKind.Relative))
            {
                throw new ArgumentException(System.SR.GetString("net_uri_InvalidUriKind", new object[] { uriKind }));
            }
            UriParser syntax = null;
            Flags hostNotParsed = Flags.HostNotParsed;
            ParsingError err = ParseScheme(uriString, ref hostNotParsed, ref syntax);
            if (dontEscape)
            {
                hostNotParsed |= Flags.HostNotParsed | Flags.UserEscaped;
            }
            if (err != ParsingError.None)
            {
                if ((uriKind != UriKind.Absolute) && (err <= ParsingError.EmptyUriString))
                {
                    return new Uri(hostNotParsed & (Flags.HostNotParsed | Flags.UserEscaped), null, uriString);
                }
                return null;
            }
            Uri uri = new Uri(hostNotParsed, syntax, uriString);
            try
            {
                uri.InitializeUri(err, uriKind, out e);
                if (e == null)
                {
                    return uri;
                }
                return null;
            }
            catch (UriFormatException exception)
            {
                e = exception;
                return null;
            }
        }

        private unsafe void CreateHostString()
        {
            if (!this.m_Syntax.IsSimple)
            {
                lock (this.m_Info)
                {
                    if (this.NotAny(Flags.ErrorOrParsingRecursion))
                    {
                        this.m_Flags |= Flags.ErrorOrParsingRecursion;
                        this.GetHostViaCustomSyntax();
                        this.m_Flags &= ~Flags.ErrorOrParsingRecursion;
                        return;
                    }
                }
            }
            Flags flags = this.m_Flags;
            string input = CreateHostStringHelper(this.m_String, this.m_Info.Offset.Host, this.m_Info.Offset.Path, ref flags, ref this.m_Info.ScopeId);
            if (input.Length != 0)
            {
                if (this.HostType == Flags.BasicHostType)
                {
                    Check check;
                    ushort idx = 0;
                    fixed (char* str2 = ((char*) input))
                    {
                        char* str = str2;
                        check = this.CheckCanonical(str, ref idx, (ushort) input.Length, 0xffff);
                    }
                    if (((check & Check.DisplayCanonical) == Check.None) && (this.NotAny(Flags.HostNotParsed | Flags.ImplicitFile) || ((check & Check.ReservedFound) != Check.None)))
                    {
                        flags |= Flags.HostNotCanonical;
                    }
                    if (this.InFact(Flags.HostNotParsed | Flags.ImplicitFile) && ((check & (Check.ReservedFound | Check.EscapedCanonical)) != Check.None))
                    {
                        check &= ~Check.EscapedCanonical;
                    }
                    if ((check & (Check.BackslashInPath | Check.EscapedCanonical)) != Check.EscapedCanonical)
                    {
                        flags |= Flags.E_HostNotCanonical;
                        if (this.NotAny(Flags.HostNotParsed | Flags.UserEscaped))
                        {
                            int destPos = 0;
                            char[] chArray = EscapeString(input, 0, input.Length, null, ref destPos, true, '?', '#', this.IsImplicitFile ? ((char) 0xffff) : '%');
                            if (chArray != null)
                            {
                                input = new string(chArray, 0, destPos);
                            }
                        }
                    }
                }
                else if (this.NotAny(Flags.CanonicalDnsHost))
                {
                    if (this.m_Info.ScopeId != null)
                    {
                        flags |= Flags.E_HostNotCanonical | Flags.HostNotCanonical;
                    }
                    else
                    {
                        for (ushort i = 0; i < input.Length; i = (ushort) (i + 1))
                        {
                            if (((this.m_Info.Offset.Host + i) >= this.m_Info.Offset.End) || (input[i] != this.m_String[this.m_Info.Offset.Host + i]))
                            {
                                flags |= Flags.E_HostNotCanonical | Flags.HostNotCanonical;
                                break;
                            }
                        }
                    }
                }
            }
            this.m_Info.Host = input;
            lock (this.m_Info)
            {
                this.m_Flags |= flags;
            }
        }

        private static string CreateHostStringHelper(string str, ushort idx, ushort end, ref Flags flags, ref string scopeId)
        {
            string str2;
            bool isLoopback = false;
            Flags flags2 = ((Flags) flags) & (Flags.BasicHostType | Flags.IPv4HostType);
            if (flags2 <= Flags.DnsHostType)
            {
                switch (flags2)
                {
                    case (Flags.HostNotParsed | Flags.IPv6HostType):
                        str2 = IPv6AddressHelper.ParseCanonicalName(str, idx, ref isLoopback, ref scopeId);
                        goto Label_00C4;

                    case (Flags.HostNotParsed | Flags.IPv4HostType):
                        str2 = IPv4AddressHelper.ParseCanonicalName(str, idx, end, ref isLoopback);
                        goto Label_00C4;

                    case Flags.DnsHostType:
                        str2 = DomainNameHelper.ParseCanonicalName(str, idx, end, ref isLoopback);
                        goto Label_00C4;
                }
            }
            else
            {
                switch (flags2)
                {
                    case (Flags.HostNotParsed | Flags.UncHostType):
                        str2 = UncNameHelper.ParseCanonicalName(str, idx, end, ref isLoopback);
                        goto Label_00C4;

                    case Flags.BasicHostType:
                        if (StaticInFact(flags, Flags.DosPath))
                        {
                            str2 = string.Empty;
                        }
                        else
                        {
                            str2 = str.Substring(idx, end - idx);
                        }
                        if (str2.Length == 0)
                        {
                            isLoopback = true;
                        }
                        goto Label_00C4;

                    case (Flags.BasicHostType | Flags.IPv4HostType):
                        str2 = string.Empty;
                        goto Label_00C4;
                }
            }
            throw GetException(ParsingError.BadHostName);
        Label_00C4:
            if (isLoopback)
            {
                flags |= Flags.HostNotParsed | Flags.LoopbackHost;
            }
            return str2;
        }

        private void CreateThis(string uri, bool dontEscape, UriKind uriKind)
        {
            UriFormatException exception;
            if ((uriKind < UriKind.RelativeOrAbsolute) || (uriKind > UriKind.Relative))
            {
                throw new ArgumentException(System.SR.GetString("net_uri_InvalidUriKind", new object[] { uriKind }));
            }
            this.m_String = (uri == null) ? string.Empty : uri;
            if (dontEscape)
            {
                this.m_Flags |= Flags.HostNotParsed | Flags.UserEscaped;
            }
            ParsingError err = ParseScheme(this.m_String, ref this.m_Flags, ref this.m_Syntax);
            this.InitializeUri(err, uriKind, out exception);
            if (exception != null)
            {
                throw exception;
            }
        }

        private void CreateThisFromUri(Uri otherUri)
        {
            this.m_Info = null;
            this.m_Flags = otherUri.m_Flags;
            if (this.InFact(Flags.HostNotParsed | Flags.MinimalUriInfoSet))
            {
                this.m_Flags &= ~(Flags.AllUriInfoSet | Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.MinimalUriInfoSet | Flags.ShouldBeCompressed);
                int path = otherUri.m_Info.Offset.Path;
                if (this.InFact(Flags.HostNotParsed | Flags.NotDefaultPort))
                {
                    while ((otherUri.m_String[path] != ':') && (path > otherUri.m_Info.Offset.Host))
                    {
                        path--;
                    }
                    if (otherUri.m_String[path] != ':')
                    {
                        path = otherUri.m_Info.Offset.Path;
                    }
                }
                this.m_Flags |= (Flags) path;
            }
            this.m_Syntax = otherUri.m_Syntax;
            this.m_String = otherUri.m_String;
            this.m_iriParsing = otherUri.m_iriParsing;
            if (otherUri.OriginalStringSwitched)
            {
                this.m_originalUnicodeString = otherUri.m_originalUnicodeString;
            }
            if (otherUri.AllowIdn && (otherUri.InFact(Flags.HostNotParsed | Flags.IdnHost) || otherUri.InFact(Flags.HostNotParsed | Flags.UnicodeHost)))
            {
                this.m_DnsSafeHost = otherUri.m_DnsSafeHost;
            }
        }

        private void CreateUri(Uri baseUri, string relativeUri, bool dontEscape)
        {
            UriFormatException exception;
            this.CreateThis(relativeUri, dontEscape, UriKind.RelativeOrAbsolute);
            if (baseUri.Syntax.IsSimple)
            {
                Uri otherUri = ResolveHelper(baseUri, this, ref relativeUri, ref dontEscape, out exception);
                if (exception != null)
                {
                    throw exception;
                }
                if (otherUri != null)
                {
                    if (otherUri != this)
                    {
                        this.CreateThisFromUri(otherUri);
                    }
                    return;
                }
            }
            else
            {
                dontEscape = false;
                relativeUri = baseUri.Syntax.InternalResolve(baseUri, this, out exception);
                if (exception != null)
                {
                    throw exception;
                }
            }
            this.m_Flags = Flags.HostNotParsed;
            this.m_Info = null;
            this.m_Syntax = null;
            this.CreateThis(relativeUri, dontEscape, UriKind.Absolute);
        }

        private unsafe void CreateUriInfo(Flags cF)
        {
            ushort length;
            UriInfo info = new UriInfo();
            info.Offset.End = (ushort) this.m_String.Length;
            if (this.UserDrivenParsing)
            {
                goto Label_041E;
            }
            bool flag = false;
            if ((cF & (Flags.HostNotParsed | Flags.ImplicitFile)) != Flags.HostNotParsed)
            {
                length = 0;
                while (IsLWS(this.m_String[length]))
                {
                    length = (ushort) (length + 1);
                    info.Offset.Scheme = (ushort) (info.Offset.Scheme + 1);
                }
                if (StaticInFact(cF, Flags.HostNotParsed | Flags.UncPath))
                {
                    for (length = (ushort) (length + 2); (length < ((ushort) (cF & (Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed)))) && ((this.m_String[length] == '/') || (this.m_String[length] == '\\')); length = (ushort) (length + 1))
                    {
                    }
                }
                goto Label_016D;
            }
            length = (ushort) this.m_Syntax.SchemeName.Length;
        Label_00E2:
            length = (ushort) (length + 1);
            if (this.m_String[length] != ':')
            {
                info.Offset.Scheme = (ushort) (info.Offset.Scheme + 1);
                goto Label_00E2;
            }
            if ((cF & Flags.AuthorityFound) != Flags.HostNotParsed)
            {
                if ((this.m_String[length] == '\\') || (this.m_String[length + 1] == '\\'))
                {
                    flag = true;
                }
                length = (ushort) (length + 2);
                if ((cF & (Flags.DosPath | Flags.UncPath)) != Flags.HostNotParsed)
                {
                    while ((length < ((ushort) (cF & (Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed)))) && ((this.m_String[length] == '/') || (this.m_String[length] == '\\')))
                    {
                        flag = true;
                        length = (ushort) (length + 1);
                    }
                }
            }
        Label_016D:
            if (this.m_Syntax.DefaultPort != -1)
            {
                info.Offset.PortValue = (ushort) this.m_Syntax.DefaultPort;
            }
            if (((cF & (Flags.BasicHostType | Flags.IPv4HostType)) == (Flags.BasicHostType | Flags.IPv4HostType)) || StaticInFact(cF, Flags.DosPath))
            {
                info.Offset.User = (ushort) (cF & (Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed));
                info.Offset.Host = info.Offset.User;
                info.Offset.Path = info.Offset.User;
                cF &= ~(Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed);
                if (flag)
                {
                    cF |= Flags.HostNotParsed | Flags.SchemeNotCanonical;
                }
            }
            else
            {
                info.Offset.User = length;
                if (this.HostType == Flags.BasicHostType)
                {
                    info.Offset.Host = length;
                    info.Offset.Path = (ushort) (cF & (Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed));
                    cF &= ~(Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed);
                }
                else
                {
                    if ((cF & Flags.HasUserInfo) != Flags.HostNotParsed)
                    {
                        while (this.m_String[length] != '@')
                        {
                            length = (ushort) (length + 1);
                        }
                        length = (ushort) (length + 1);
                        info.Offset.Host = length;
                    }
                    else
                    {
                        info.Offset.Host = length;
                    }
                    length = (ushort) (cF & (Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed));
                    cF &= ~(Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed);
                    if (flag)
                    {
                        cF |= Flags.HostNotParsed | Flags.SchemeNotCanonical;
                    }
                    info.Offset.Path = length;
                    bool flag2 = false;
                    bool flag3 = (cF & (Flags.HostNotParsed | Flags.UseOrigUncdStrOffset)) != Flags.HostNotParsed;
                    cF &= ~(Flags.HostNotParsed | Flags.UseOrigUncdStrOffset);
                    if (flag3)
                    {
                        info.Offset.End = (ushort) this.m_originalUnicodeString.Length;
                    }
                    if (length < info.Offset.End)
                    {
                        fixed (char* str = (flag3 ? ((char*) this.m_originalUnicodeString) : ((char*) this.m_String)))
                        {
                            char* chPtr = str;
                            if (chPtr[length] != ':')
                            {
                                goto Label_041E;
                            }
                            int num2 = 0;
                            if ((length = (ushort) (length + 1)) < info.Offset.End)
                            {
                                num2 = chPtr[length] - '0';
                                switch (num2)
                                {
                                    case 0xffff:
                                    case 15:
                                    case 0xfff3:
                                        goto Label_03D8;
                                }
                                flag2 = true;
                                if (num2 == 0)
                                {
                                    cF |= Flags.E_PortNotCanonical | Flags.PortNotCanonical;
                                }
                                length = (ushort) (length + 1);
                                while (length < info.Offset.End)
                                {
                                    ushort num3 = chPtr[length] - '0';
                                    switch (num3)
                                    {
                                        case 0xffff:
                                        case 15:
                                        case 0xfff3:
                                            goto Label_03D8;
                                    }
                                    num2 = (num2 * 10) + num3;
                                    length = (ushort) (length + 1);
                                }
                            }
                        Label_03D8:
                            if (flag2 && (info.Offset.PortValue != ((ushort) num2)))
                            {
                                info.Offset.PortValue = (ushort) num2;
                                cF |= Flags.HostNotParsed | Flags.NotDefaultPort;
                            }
                            else
                            {
                                cF |= Flags.E_PortNotCanonical | Flags.PortNotCanonical;
                            }
                            info.Offset.Path = length;
                        }
                    }
                }
            }
        Label_041E:
            cF |= Flags.HostNotParsed | Flags.MinimalUriInfoSet;
            info.DnsSafeHost = this.m_DnsSafeHost;
            lock (this.m_String)
            {
                if ((this.m_Flags & (Flags.HostNotParsed | Flags.MinimalUriInfoSet)) == Flags.HostNotParsed)
                {
                    this.m_Info = info;
                    this.m_Flags = (this.m_Flags & ~(Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed)) | cF;
                }
            }
        }

        private static unsafe char[] EnsureDestinationSize(char* pStr, char[] dest, int currentInputPos, short charsToAdd, short minReallocateChars, ref int destPos, int prevInputPos)
        {
            if ((dest == null) || (dest.Length < ((destPos + (currentInputPos - prevInputPos)) + charsToAdd)))
            {
                char[] dst = new char[(destPos + (currentInputPos - prevInputPos)) + minReallocateChars];
                if ((dest != null) && (destPos != 0))
                {
                    Buffer.BlockCopy(dest, 0, dst, 0, destPos << 1);
                }
                dest = dst;
            }
            while (prevInputPos != currentInputPos)
            {
                dest[destPos++] = pStr[prevInputPos++];
            }
            return dest;
        }

        private void EnsureHostString(bool allowDnsOptimization)
        {
            this.EnsureUriInfo();
            if ((this.m_Info.Host == null) && (!allowDnsOptimization || !this.InFact(Flags.CanonicalDnsHost)))
            {
                this.CreateHostString();
            }
        }

        private void EnsureParseRemaining()
        {
            if ((this.m_Flags & Flags.AllUriInfoSet) == Flags.HostNotParsed)
            {
                this.ParseRemaining();
            }
        }

        private UriInfo EnsureUriInfo()
        {
            Flags cF = this.m_Flags;
            if ((this.m_Flags & (Flags.HostNotParsed | Flags.MinimalUriInfoSet)) == Flags.HostNotParsed)
            {
                this.CreateUriInfo(cF);
            }
            return this.m_Info;
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override unsafe bool Equals(object comparand)
        {
            if (comparand == null)
            {
                return false;
            }
            if (this == comparand)
            {
                return true;
            }
            Uri result = comparand as Uri;
            if (result == null)
            {
                string uriString = comparand as string;
                if (uriString == null)
                {
                    return false;
                }
                if (!TryCreate(uriString, UriKind.RelativeOrAbsolute, out result))
                {
                    return false;
                }
            }
            if (this.m_String == result.m_String)
            {
                return true;
            }
            if (this.IsAbsoluteUri != result.IsAbsoluteUri)
            {
                return false;
            }
            if (this.IsNotAbsoluteUri)
            {
                return this.OriginalString.Equals(result.OriginalString);
            }
            if (this.NotAny(Flags.AllUriInfoSet) || result.NotAny(Flags.AllUriInfoSet))
            {
                if (!this.IsUncOrDosPath)
                {
                    if (this.m_String.Length == result.m_String.Length)
                    {
                        fixed (char* str5 = ((char*) this.m_String))
                        {
                            char* chPtr = str5;
                            fixed (char* str6 = ((char*) result.m_String))
                            {
                                char* chPtr2 = str6;
                                int index = this.m_String.Length - 1;
                                while (index >= 0)
                                {
                                    if (chPtr[index] != chPtr2[index])
                                    {
                                        break;
                                    }
                                    index--;
                                }
                                if (index == -1)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                else if (string.Compare(this.m_String, result.m_String, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            this.EnsureUriInfo();
            result.EnsureUriInfo();
            if ((!this.UserDrivenParsing && !result.UserDrivenParsing) && (this.Syntax.IsSimple && result.Syntax.IsSimple))
            {
                if (!this.InFact(Flags.CanonicalDnsHost) || !result.InFact(Flags.CanonicalDnsHost))
                {
                    this.EnsureHostString(false);
                    result.EnsureHostString(false);
                    if (!this.m_Info.Host.Equals(result.m_Info.Host))
                    {
                        return false;
                    }
                }
                else
                {
                    ushort host = this.m_Info.Offset.Host;
                    ushort path = this.m_Info.Offset.Path;
                    ushort num4 = result.m_Info.Offset.Host;
                    ushort num5 = result.m_Info.Offset.Path;
                    string str2 = result.m_String;
                    if ((path - host) > (num5 - num4))
                    {
                        path = (ushort) ((host + num5) - num4);
                    }
                    while (host < path)
                    {
                        if (this.m_String[host] != str2[num4])
                        {
                            return false;
                        }
                        if (str2[num4] == ':')
                        {
                            break;
                        }
                        host = (ushort) (host + 1);
                        num4 = (ushort) (num4 + 1);
                    }
                    if (((host < this.m_Info.Offset.Path) && (this.m_String[host] != ':')) || ((num4 < num5) && (str2[num4] != ':')))
                    {
                        return false;
                    }
                }
                if (this.Port != result.Port)
                {
                    return false;
                }
            }
            UriInfo info = this.m_Info;
            UriInfo info2 = result.m_Info;
            if (info.MoreInfo == null)
            {
                info.MoreInfo = new MoreInfo();
            }
            if (info2.MoreInfo == null)
            {
                info2.MoreInfo = new MoreInfo();
            }
            string remoteUrl = info.MoreInfo.RemoteUrl;
            if (remoteUrl == null)
            {
                remoteUrl = this.GetParts(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
                info.MoreInfo.RemoteUrl = remoteUrl;
            }
            string parts = info2.MoreInfo.RemoteUrl;
            if (parts == null)
            {
                parts = result.GetParts(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
                info2.MoreInfo.RemoteUrl = parts;
            }
            if (this.IsUncOrDosPath)
            {
                return (string.Compare(info.MoreInfo.RemoteUrl, info2.MoreInfo.RemoteUrl, this.IsUncOrDosPath ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0);
            }
            if (remoteUrl.Length != parts.Length)
            {
                return false;
            }
            fixed (char* str7 = ((char*) remoteUrl))
            {
                char* chPtr3 = str7;
                fixed (char* str8 = ((char*) parts))
                {
                    char* chPtr4 = str8;
                    char* chPtr5 = chPtr3 + remoteUrl.Length;
                    char* chPtr6 = chPtr4 + remoteUrl.Length;
                    while (chPtr5 != chPtr3)
                    {
                        if (*(--chPtr5) != *(--chPtr6))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        [Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void Escape()
        {
        }

        private static void EscapeAsciiChar(char ch, char[] to, ref int pos)
        {
            to[pos++] = '%';
            to[pos++] = HexUpperChars[(ch & 240) >> 4];
            to[pos++] = HexUpperChars[ch & '\x000f'];
        }

        private static char EscapedAscii(char digit, char next)
        {
            if ((((digit < '0') || (digit > '9')) && ((digit < 'A') || (digit > 'F'))) && ((digit < 'a') || (digit > 'f')))
            {
                return 0xffff;
            }
            int num = (digit <= '9') ? (digit - '0') : (((digit <= 'F') ? (digit - 'A') : (digit - 'a')) + 10);
            if ((((next < '0') || (next > '9')) && ((next < 'A') || (next > 'F'))) && ((next < 'a') || (next > 'f')))
            {
                return 0xffff;
            }
            return (char) ((num << 4) + ((next <= '9') ? (next - '0') : (((next <= 'F') ? (next - 'A') : (next - 'a')) + 10)));
        }

        public static string EscapeDataString(string stringToEscape)
        {
            if (stringToEscape == null)
            {
                throw new ArgumentNullException("stringToUnescape");
            }
            if (stringToEscape.Length == 0)
            {
                return string.Empty;
            }
            int destPos = 0;
            char[] chArray = EscapeString(stringToEscape, 0, stringToEscape.Length, null, ref destPos, false, 0xffff, 0xffff, 0xffff);
            if (chArray == null)
            {
                return stringToEscape;
            }
            return new string(chArray, 0, destPos);
        }

        [Obsolete("The method has been deprecated. Please use GetComponents() or static EscapeUriString() to escape a Uri component or a string. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected static string EscapeString(string str)
        {
            if (str == null)
            {
                return string.Empty;
            }
            int destPos = 0;
            char[] chArray = EscapeString(str, 0, str.Length, null, ref destPos, true, '?', '#', '%');
            if (chArray == null)
            {
                return str;
            }
            return new string(chArray, 0, destPos);
        }

        private static unsafe char[] EscapeString(string input, int start, int end, char[] dest, ref int destPos, bool isUriString, char force1, char force2, char rsvd)
        {
            if ((end - start) >= 0xfff0)
            {
                throw GetException(ParsingError.SizeLimit);
            }
            int index = start;
            int prevInputPos = start;
            byte* bytes = stackalloc byte[160];
            fixed (char* str = ((char*) input))
            {
                char* pStr = str;
                while (index < end)
                {
                    char ch = pStr[index];
                    if (ch > '\x007f')
                    {
                        short num3 = (short) Math.Min(end - index, 0x27);
                        short charCount = 1;
                        while ((charCount < num3) && (pStr[index + charCount] > '\x007f'))
                        {
                            charCount = (short) (charCount + 1);
                        }
                        if ((pStr[(index + charCount) - 1] >= 0xd800) && (pStr[(index + charCount) - 1] <= 0xdbff))
                        {
                            if ((charCount == 1) || (charCount == (end - index)))
                            {
                                throw new UriFormatException(System.SR.GetString("net_uri_BadString"));
                            }
                            charCount = (short) (charCount + 1);
                        }
                        dest = EnsureDestinationSize(pStr, dest, index, (short) ((charCount * 4) * 3), 480, ref destPos, prevInputPos);
                        short num5 = (short) Encoding.UTF8.GetBytes(pStr + index, charCount, bytes, 160);
                        if (num5 == 0)
                        {
                            throw new UriFormatException(System.SR.GetString("net_uri_BadString"));
                        }
                        index += charCount - 1;
                        for (charCount = 0; charCount < num5; charCount = (short) (charCount + 1))
                        {
                            EscapeAsciiChar(*((char*) (bytes + charCount)), dest, ref destPos);
                        }
                        prevInputPos = index + 1;
                    }
                    else if ((ch == '%') && (rsvd == '%'))
                    {
                        dest = EnsureDestinationSize(pStr, dest, index, 3, 120, ref destPos, prevInputPos);
                        if (((index + 2) < end) && (EscapedAscii(pStr[index + 1], pStr[index + 2]) != 0xffff))
                        {
                            dest[destPos++] = '%';
                            dest[destPos++] = pStr[index + 1];
                            dest[destPos++] = pStr[index + 2];
                            index += 2;
                        }
                        else
                        {
                            EscapeAsciiChar('%', dest, ref destPos);
                        }
                        prevInputPos = index + 1;
                    }
                    else if ((ch == force1) || (ch == force2))
                    {
                        dest = EnsureDestinationSize(pStr, dest, index, 3, 120, ref destPos, prevInputPos);
                        EscapeAsciiChar(ch, dest, ref destPos);
                        prevInputPos = index + 1;
                    }
                    else if ((ch != rsvd) && (isUriString ? IsNotReservedNotUnreservedNotHash(ch) : IsNotUnreserved(ch)))
                    {
                        dest = EnsureDestinationSize(pStr, dest, index, 3, 120, ref destPos, prevInputPos);
                        EscapeAsciiChar(ch, dest, ref destPos);
                        prevInputPos = index + 1;
                    }
                    index++;
                }
                if ((prevInputPos != index) && ((prevInputPos != start) || (dest != null)))
                {
                    dest = EnsureDestinationSize(pStr, dest, index, 0, 0, ref destPos, prevInputPos);
                }
            }
            return dest;
        }

        internal unsafe string EscapeUnescapeIri(string input, int start, int end, UriComponents component)
        {
            fixed (char* str2 = ((char*) input))
            {
                char* pInput = str2;
                return this.EscapeUnescapeIri(pInput, start, end, component);
            }
        }

        internal unsafe string EscapeUnescapeIri(char* pInput, int start, int end, UriComponents component)
        {
            char[] chArray = new char[end - start];
            byte[] bytes = null;
            GCHandle handle = GCHandle.Alloc(chArray, GCHandleType.Pinned);
            char* pDest = (char*) handle.AddrOfPinnedObject();
            byte num = 0;
            int index = start;
            int destOffset = 0;
            bool flag = false;
            bool surrogatePair = false;
            bool flag3 = false;
            while (index < end)
            {
                byte[] buffer3;
                flag = false;
                surrogatePair = false;
                flag3 = false;
                char ch = pInput[index];
                if (ch == '%')
                {
                    if ((index + 2) < end)
                    {
                        ch = EscapedAscii(pInput[index + 1], pInput[index + 2]);
                        if (((ch == 0xffff) || (ch == '%')) || (this.CheckIsReserved(ch, component) || IsNotSafeForUnescape(ch)))
                        {
                            pDest[destOffset++] = pInput[index++];
                            pDest[destOffset++] = pInput[index++];
                            pDest[destOffset++] = pInput[index];
                            goto Label_0400;
                        }
                        if (ch <= '\x007f')
                        {
                            pDest[destOffset++] = ch;
                            index += 2;
                            goto Label_0400;
                        }
                        int num4 = index;
                        int byteCount = 1;
                        if (bytes == null)
                        {
                            bytes = new byte[end - index];
                        }
                        bytes[0] = (byte) ch;
                        index += 3;
                        while (index < end)
                        {
                            if (((ch = pInput[index]) != '%') || ((index + 2) >= end))
                            {
                                break;
                            }
                            ch = EscapedAscii(pInput[index + 1], pInput[index + 2]);
                            if ((ch == 0xffff) || (ch < '\x0080'))
                            {
                                break;
                            }
                            bytes[byteCount++] = (byte) ch;
                            index += 3;
                        }
                        index--;
                        Encoding encoding = Encoding.GetEncoding("utf-8", new EncoderReplacementFallback(""), new DecoderReplacementFallback(""));
                        char[] chars = new char[bytes.Length];
                        int charCount = encoding.GetChars(bytes, 0, byteCount, chars, 0);
                        if (charCount != 0)
                        {
                            MatchUTF8Sequence(pDest, chArray, ref destOffset, chars, charCount, bytes, component == UriComponents.Query, true);
                        }
                        else
                        {
                            for (int j = num4; j <= index; j++)
                            {
                                pDest[destOffset++] = pInput[j];
                            }
                        }
                    }
                    else
                    {
                        pDest[destOffset++] = pInput[index];
                    }
                }
                else if (ch > '\x007f')
                {
                    if (char.IsHighSurrogate(ch) && ((index + 1) < end))
                    {
                        char lowSurr = pInput[index + 1];
                        flag = !CheckIriUnicodeRange(ch, lowSurr, ref surrogatePair, component == UriComponents.Query);
                        if (!flag)
                        {
                            pDest[destOffset++] = pInput[index++];
                            pDest[destOffset++] = pInput[index];
                        }
                        else
                        {
                            flag3 = true;
                        }
                    }
                    else if (CheckIriUnicodeRange(ch, component == UriComponents.Query))
                    {
                        if (!IsBidiControlCharacter(ch))
                        {
                            pDest[destOffset++] = pInput[index];
                        }
                    }
                    else
                    {
                        flag = true;
                        flag3 = true;
                    }
                }
                else
                {
                    pDest[destOffset++] = pInput[index];
                }
                if (!flag)
                {
                    goto Label_0400;
                }
                if (num == 0)
                {
                    char[] chArray4;
                    num = 30;
                    char[] chArray3 = new char[chArray.Length + (num * 3)];
                    if (((chArray4 = chArray3) == null) || (chArray4.Length == 0))
                    {
                        chRef = null;
                        goto Label_0328;
                    }
                    fixed (char* chRef = chArray4)
                    {
                        int num8;
                    Label_0328:
                        num8 = 0;
                        while (num8 < destOffset)
                        {
                            chRef[num8] = pDest[num8];
                            num8++;
                        }
                    }
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                    chArray = chArray3;
                    handle = GCHandle.Alloc(chArray, GCHandleType.Pinned);
                    pDest = (char*) handle.AddrOfPinnedObject();
                }
                else if (flag3)
                {
                    if (surrogatePair)
                    {
                        num = (byte) (num - 4);
                    }
                    else
                    {
                        num = (byte) (num - 3);
                    }
                }
                else
                {
                    num = (byte) (num - 1);
                }
                byte[] buffer2 = new byte[4];
                if (((buffer3 = buffer2) == null) || (buffer3.Length == 0))
                {
                    numRef = null;
                }
                else
                {
                    numRef = buffer3;
                }
                int num9 = Encoding.UTF8.GetBytes(pInput + index, surrogatePair ? 2 : 1, numRef, 4);
                for (int i = 0; i < num9; i++)
                {
                    EscapeAsciiChar((char) buffer2[i], chArray, ref destOffset);
                }
                fixed (byte* numRef = null)
                {
                Label_0400:
                    index++;
                }
            }
            if (handle.IsAllocated)
            {
                handle.Free();
            }
            return new string(chArray, 0, destOffset);
        }

        public static string EscapeUriString(string stringToEscape)
        {
            if (stringToEscape == null)
            {
                throw new ArgumentNullException("stringToUnescape");
            }
            if (stringToEscape.Length == 0)
            {
                return string.Empty;
            }
            int destPos = 0;
            char[] chArray = EscapeString(stringToEscape, 0, stringToEscape.Length, null, ref destPos, true, 0xffff, 0xffff, 0xffff);
            if (chArray == null)
            {
                return stringToEscape;
            }
            return new string(chArray, 0, destPos);
        }

        private unsafe void FindEndOfComponent(string input, ref ushort idx, ushort end, char delim)
        {
            fixed (char* str = ((char*) input))
            {
                char* chPtr = str;
                this.FindEndOfComponent(chPtr, ref idx, end, delim);
            }
        }

        private unsafe void FindEndOfComponent(char* str, ref ushort idx, ushort end, char delim)
        {
            char ch = 0xffff;
            ushort index = idx;
            while (index < end)
            {
                ch = str[index];
                if ((ch == delim) || (((delim == '?') && (ch == '#')) && ((this.m_Syntax != null) && this.m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment))))
                {
                    break;
                }
                index = (ushort) (index + 1);
            }
            idx = index;
        }

        public static int FromHex(char digit)
        {
            if ((((digit < '0') || (digit > '9')) && ((digit < 'A') || (digit > 'F'))) && ((digit < 'a') || (digit > 'f')))
            {
                throw new ArgumentException("digit");
            }
            if (digit > '9')
            {
                return (((digit <= 'F') ? (digit - 'A') : (digit - 'a')) + 10);
            }
            return (digit - '0');
        }

        private unsafe char[] GetCanonicalPath(char[] dest, ref int pos, UriFormat formatAs)
        {
            char[] chArray2;
            if (this.InFact(Flags.FirstSlashAbsent))
            {
                dest[pos++] = '/';
            }
            if (this.m_Info.Offset.Path == this.m_Info.Offset.Query)
            {
                return dest;
            }
            int destinationIndex = pos;
            int securedPathIndex = this.SecuredPathIndex;
            if (formatAs != UriFormat.UriEscaped)
            {
                goto Label_0272;
            }
            if (!this.InFact(Flags.HostNotParsed | Flags.ShouldBeCompressed))
            {
                goto Label_013F;
            }
            this.m_String.CopyTo(this.m_Info.Offset.Path, dest, destinationIndex, this.m_Info.Offset.Query - this.m_Info.Offset.Path);
            destinationIndex += this.m_Info.Offset.Query - this.m_Info.Offset.Path;
            if ((!this.m_Syntax.InFact(UriSyntaxFlags.UnEscapeDotsAndSlashes) || !this.InFact(Flags.HostNotParsed | Flags.PathNotCanonical)) || this.IsImplicitFile)
            {
                goto Label_0352;
            }
            if (((chArray2 = dest) == null) || (chArray2.Length == 0))
            {
                chRef = null;
            }
            else
            {
                chRef = chArray2;
            }
            UnescapeOnly(chRef, pos, ref destinationIndex, '.', '/', this.m_Syntax.InFact(UriSyntaxFlags.ConvertPathSlashes) ? '\\' : ((char) 0xffff));
            fixed (char* chRef = null)
            {
                char[] chArray3;
                goto Label_0352;
            Label_013F:
                if (this.InFact(Flags.E_PathNotCanonical) && this.NotAny(Flags.HostNotParsed | Flags.UserEscaped))
                {
                    string input = this.m_String;
                    if ((securedPathIndex != 0) && (input[(securedPathIndex + this.m_Info.Offset.Path) - 1] == '|'))
                    {
                        input = input.Remove((securedPathIndex + this.m_Info.Offset.Path) - 1, 1).Insert((securedPathIndex + this.m_Info.Offset.Path) - 1, ":");
                    }
                    dest = EscapeString(input, this.m_Info.Offset.Path, this.m_Info.Offset.Query, dest, ref destinationIndex, true, '?', '#', this.IsImplicitFile ? ((char) 0xffff) : '%');
                }
                else
                {
                    this.m_String.CopyTo(this.m_Info.Offset.Path, dest, destinationIndex, this.m_Info.Offset.Query - this.m_Info.Offset.Path);
                    destinationIndex += this.m_Info.Offset.Query - this.m_Info.Offset.Path;
                }
                goto Label_0352;
            Label_0272:
                this.m_String.CopyTo(this.m_Info.Offset.Path, dest, destinationIndex, this.m_Info.Offset.Query - this.m_Info.Offset.Path);
                destinationIndex += this.m_Info.Offset.Query - this.m_Info.Offset.Path;
                if ((!this.InFact(Flags.HostNotParsed | Flags.ShouldBeCompressed) || !this.m_Syntax.InFact(UriSyntaxFlags.UnEscapeDotsAndSlashes)) || (!this.InFact(Flags.HostNotParsed | Flags.PathNotCanonical) || this.IsImplicitFile))
                {
                    goto Label_0352;
                }
                if (((chArray3 = dest) == null) || (chArray3.Length == 0))
                {
                    chRef2 = null;
                }
                else
                {
                    chRef2 = chArray3;
                }
                UnescapeOnly(chRef2, pos, ref destinationIndex, '.', '/', this.m_Syntax.InFact(UriSyntaxFlags.ConvertPathSlashes) ? '\\' : ((char) 0xffff));
                fixed (char* chRef2 = null)
                {
                    UnescapeMode mode;
                    char[] chArray;
                Label_0352:
                    if ((securedPathIndex != 0) && (dest[(securedPathIndex + pos) - 1] == '|'))
                    {
                        dest[(securedPathIndex + pos) - 1] = ':';
                    }
                    if (this.InFact(Flags.HostNotParsed | Flags.ShouldBeCompressed))
                    {
                        dest = Compress(dest, (ushort) (pos + securedPathIndex), ref destinationIndex, this.m_Syntax);
                        if (dest[pos] == '\\')
                        {
                            dest[pos] = '/';
                        }
                        if (((formatAs == UriFormat.UriEscaped) && this.NotAny(Flags.HostNotParsed | Flags.UserEscaped)) && this.InFact(Flags.E_PathNotCanonical))
                        {
                            string str2 = new string(dest, pos, destinationIndex - pos);
                            dest = EscapeString(str2, 0, destinationIndex - pos, dest, ref pos, true, '?', '#', this.IsImplicitFile ? ((char) 0xffff) : '%');
                            destinationIndex = pos;
                        }
                    }
                    else if (this.m_Syntax.InFact(UriSyntaxFlags.ConvertPathSlashes) && this.InFact(Flags.BackslashInPath))
                    {
                        for (int i = pos; i < destinationIndex; i++)
                        {
                            if (dest[i] == '\\')
                            {
                                dest[i] = '/';
                            }
                        }
                    }
                    if ((formatAs == UriFormat.UriEscaped) || !this.InFact(Flags.HostNotParsed | Flags.PathNotCanonical))
                    {
                        goto Label_052A;
                    }
                    if (!this.InFact(Flags.HostNotParsed | Flags.PathNotCanonical))
                    {
                        goto Label_04CB;
                    }
                    UriFormat format = formatAs;
                    if (format != UriFormat.Unescaped)
                    {
                        if (format != ((UriFormat) 0x7fff))
                        {
                            goto Label_04A6;
                        }
                        mode = (this.InFact(Flags.HostNotParsed | Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape) | UnescapeMode.V1ToStringFlag;
                        if (this.IsImplicitFile)
                        {
                            mode &= ~UnescapeMode.Unescape;
                        }
                    }
                    else
                    {
                        mode = this.IsImplicitFile ? UnescapeMode.CopyOnly : (UnescapeMode.UnescapeAll | UnescapeMode.Unescape);
                    }
                    goto Label_04CE;
                Label_04A6:
                    mode = this.InFact(Flags.HostNotParsed | Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape;
                    if (this.IsImplicitFile)
                    {
                        mode &= ~UnescapeMode.Unescape;
                    }
                    goto Label_04CE;
                Label_04CB:
                    mode = UnescapeMode.CopyOnly;
                Label_04CE:
                    chArray = new char[dest.Length];
                    Buffer.BlockCopy(dest, 0, chArray, 0, destinationIndex << 1);
                    fixed (char* chRef3 = chArray)
                    {
                        dest = UnescapeString(chRef3, pos, destinationIndex, dest, ref pos, '?', '#', 0xffff, mode, this.m_Syntax, false, false);
                    }
                    return dest;
                Label_052A:
                    pos = destinationIndex;
                    return dest;
                }
            }
        }

        private static unsafe ParsingError GetCombinedString(Uri baseUri, string relativeStr, bool dontEscape, ref string result)
        {
            for (int i = 0; i < relativeStr.Length; i++)
            {
                if (((relativeStr[i] == '/') || (relativeStr[i] == '\\')) || ((relativeStr[i] == '?') || (relativeStr[i] == '#')))
                {
                    break;
                }
                if (relativeStr[i] == ':')
                {
                    if (i >= 2)
                    {
                        string str = relativeStr.Substring(0, i);
                        fixed (char* str2 = ((char*) str))
                        {
                            char* ptr = str2;
                            UriParser syntax = null;
                            if (CheckSchemeSyntax(ptr, (ushort) str.Length, ref syntax) == ParsingError.None)
                            {
                                if (baseUri.Syntax == syntax)
                                {
                                    if ((i + 1) < relativeStr.Length)
                                    {
                                        relativeStr = relativeStr.Substring(i + 1);
                                    }
                                    else
                                    {
                                        relativeStr = string.Empty;
                                    }
                                }
                                else
                                {
                                    result = relativeStr;
                                    return ParsingError.None;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            if (relativeStr.Length == 0)
            {
                result = baseUri.OriginalString;
                return ParsingError.None;
            }
            result = CombineUri(baseUri, relativeStr, dontEscape ? UriFormat.UriEscaped : UriFormat.SafeUnescaped);
            return ParsingError.None;
        }

        public string GetComponents(UriComponents components, UriFormat format)
        {
            if (((components & UriComponents.SerializationInfoString) != 0) && (components != UriComponents.SerializationInfoString))
            {
                throw new ArgumentOutOfRangeException("UriComponents.SerializationInfoString");
            }
            if ((format & ~UriFormat.SafeUnescaped) != ((UriFormat) 0))
            {
                throw new ArgumentOutOfRangeException("format");
            }
            if (this.IsNotAbsoluteUri)
            {
                if (components != UriComponents.SerializationInfoString)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                return this.GetRelativeSerializationString(format);
            }
            if (this.Syntax.IsSimple)
            {
                return this.GetComponentsHelper(components, format);
            }
            return this.Syntax.InternalGetComponents(this, components, format);
        }

        internal string GetComponentsHelper(UriComponents uriComponents, UriFormat uriFormat)
        {
            if (uriComponents == UriComponents.Scheme)
            {
                return this.m_Syntax.SchemeName;
            }
            if ((uriComponents & UriComponents.SerializationInfoString) != 0)
            {
                uriComponents |= UriComponents.AbsoluteUri;
            }
            this.EnsureParseRemaining();
            if ((uriComponents & UriComponents.Host) != 0)
            {
                this.EnsureHostString(true);
            }
            if ((uriComponents == UriComponents.Port) || (uriComponents == UriComponents.StrongPort))
            {
                if (((this.m_Flags & (Flags.HostNotParsed | Flags.NotDefaultPort)) == Flags.HostNotParsed) && ((uriComponents != UriComponents.StrongPort) || (this.m_Syntax.DefaultPort == -1)))
                {
                    return string.Empty;
                }
                return this.m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
            }
            if ((uriComponents & UriComponents.StrongPort) != 0)
            {
                uriComponents |= UriComponents.Port;
            }
            if ((uriComponents == UriComponents.Host) && ((uriFormat == UriFormat.UriEscaped) || ((this.m_Flags & (Flags.E_HostNotCanonical | Flags.HostNotCanonical)) == Flags.HostNotParsed)))
            {
                this.EnsureHostString(false);
                return this.m_Info.Host;
            }
            switch (uriFormat)
            {
                case UriFormat.UriEscaped:
                    return this.GetEscapedParts(uriComponents);

                case UriFormat.Unescaped:
                case UriFormat.SafeUnescaped:
                case ((UriFormat) 0x7fff):
                    return this.GetUnescapedParts(uriComponents, uriFormat);
            }
            throw new ArgumentOutOfRangeException("uriFormat");
        }

        private string GetEscapedParts(UriComponents uriParts)
        {
            ushort nonCanonical = (ushort) ((((ushort) this.m_Flags) & 0x3f80) >> 6);
            if (this.InFact(Flags.HostNotParsed | Flags.SchemeNotCanonical))
            {
                nonCanonical = (ushort) (nonCanonical | 1);
            }
            if ((uriParts & UriComponents.Path) != 0)
            {
                if (this.InFact(Flags.BackslashInPath | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed))
                {
                    nonCanonical = (ushort) (nonCanonical | 0x10);
                }
                else if (this.IsDosPath && (this.m_String[(this.m_Info.Offset.Path + this.SecuredPathIndex) - 1] == '|'))
                {
                    nonCanonical = (ushort) (nonCanonical | 0x10);
                }
            }
            if ((((ushort) uriParts) & nonCanonical) == 0)
            {
                string uriPartsFromUserString = this.GetUriPartsFromUserString(uriParts);
                if (uriPartsFromUserString != null)
                {
                    return uriPartsFromUserString;
                }
            }
            return this.ReCreateParts(uriParts, nonCanonical, UriFormat.UriEscaped);
        }

        private static UriFormatException GetException(ParsingError err)
        {
            switch (err)
            {
                case ParsingError.None:
                    return null;

                case ParsingError.BadFormat:
                    return ExceptionHelper.BadFormatException;

                case ParsingError.BadScheme:
                    return ExceptionHelper.BadSchemeException;

                case ParsingError.BadAuthority:
                    return ExceptionHelper.BadAuthorityException;

                case ParsingError.EmptyUriString:
                    return ExceptionHelper.EmptyUriException;

                case ParsingError.SchemeLimit:
                    return ExceptionHelper.SchemeLimitException;

                case ParsingError.SizeLimit:
                    return ExceptionHelper.SizeLimitException;

                case ParsingError.MustRootedPath:
                    return ExceptionHelper.MustRootedPathException;

                case ParsingError.BadHostName:
                    return ExceptionHelper.BadHostNameException;

                case ParsingError.NonEmptyHost:
                    return ExceptionHelper.BadFormatException;

                case ParsingError.BadPort:
                    return ExceptionHelper.BadPortException;

                case ParsingError.BadAuthorityTerminator:
                    return ExceptionHelper.BadAuthorityTerminatorException;

                case ParsingError.CannotCreateRelative:
                    return ExceptionHelper.CannotCreateRelativeException;
            }
            return ExceptionHelper.BadFormatException;
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override int GetHashCode()
        {
            if (this.IsNotAbsoluteUri)
            {
                return CalculateCaseInsensitiveHashCode(this.OriginalString);
            }
            UriInfo info = this.EnsureUriInfo();
            if (info.MoreInfo == null)
            {
                info.MoreInfo = new MoreInfo();
            }
            int hash = info.MoreInfo.Hash;
            if (hash == 0)
            {
                string remoteUrl = info.MoreInfo.RemoteUrl;
                if (remoteUrl == null)
                {
                    remoteUrl = this.GetParts(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
                }
                hash = CalculateCaseInsensitiveHashCode(remoteUrl);
                if (hash == 0)
                {
                    hash = 0x1000000;
                }
                info.MoreInfo.Hash = hash;
            }
            return hash;
        }

        private unsafe void GetHostViaCustomSyntax()
        {
            if (this.m_Info.Host == null)
            {
                string str = this.m_Syntax.InternalGetComponents(this, UriComponents.Host, UriFormat.UriEscaped);
                if (this.m_Info.Host == null)
                {
                    if (str.Length >= 0xfff0)
                    {
                        throw GetException(ParsingError.SizeLimit);
                    }
                    ParsingError none = ParsingError.None;
                    Flags flags = this.m_Flags & ~(Flags.BasicHostType | Flags.IPv4HostType);
                    fixed (char* str4 = ((char*) str))
                    {
                        char* pString = str4;
                        string newHost = null;
                        if (this.CheckAuthorityHelper(pString, 0, (ushort) str.Length, ref none, ref flags, this.m_Syntax, ref newHost) != ((ushort) str.Length))
                        {
                            flags &= ~(Flags.BasicHostType | Flags.IPv4HostType);
                            flags |= Flags.BasicHostType | Flags.IPv4HostType;
                        }
                    }
                    if ((none != ParsingError.None) || ((flags & (Flags.BasicHostType | Flags.IPv4HostType)) == (Flags.BasicHostType | Flags.IPv4HostType)))
                    {
                        this.m_Flags = (this.m_Flags & ~(Flags.BasicHostType | Flags.IPv4HostType)) | Flags.BasicHostType;
                    }
                    else
                    {
                        str = CreateHostStringHelper(str, 0, (ushort) str.Length, ref flags, ref this.m_Info.ScopeId);
                        for (ushort i = 0; i < str.Length; i = (ushort) (i + 1))
                        {
                            if (((this.m_Info.Offset.Host + i) >= this.m_Info.Offset.End) || (str[i] != this.m_String[this.m_Info.Offset.Host + i]))
                            {
                                this.m_Flags |= Flags.E_HostNotCanonical | Flags.HostNotCanonical;
                                break;
                            }
                        }
                        this.m_Flags = (this.m_Flags & ~(Flags.BasicHostType | Flags.IPv4HostType)) | (flags & (Flags.BasicHostType | Flags.IPv4HostType));
                    }
                }
                string str3 = this.m_Syntax.InternalGetComponents(this, UriComponents.StrongPort, UriFormat.UriEscaped);
                int num2 = 0;
                if ((str3 == null) || (str3.Length == 0))
                {
                    this.m_Flags &= ~(Flags.HostNotParsed | Flags.NotDefaultPort);
                    this.m_Flags |= Flags.E_PortNotCanonical | Flags.PortNotCanonical;
                    this.m_Info.Offset.PortValue = 0;
                }
                else
                {
                    for (int j = 0; j < str3.Length; j++)
                    {
                        int num4 = str3[j] - '0';
                        if (((num4 < 0) || (num4 > 9)) || ((num2 = (num2 * 10) + num4) > 0xffff))
                        {
                            throw new UriFormatException(System.SR.GetString("net_uri_PortOutOfRange", new object[] { this.m_Syntax.GetType().FullName, str3 }));
                        }
                    }
                    if (num2 != this.m_Info.Offset.PortValue)
                    {
                        if (num2 == this.m_Syntax.DefaultPort)
                        {
                            this.m_Flags &= ~(Flags.HostNotParsed | Flags.NotDefaultPort);
                        }
                        else
                        {
                            this.m_Flags |= Flags.HostNotParsed | Flags.NotDefaultPort;
                        }
                        this.m_Flags |= Flags.E_PortNotCanonical | Flags.PortNotCanonical;
                        this.m_Info.Offset.PortValue = (ushort) num2;
                    }
                }
                this.m_Info.Host = str;
            }
        }

        public string GetLeftPart(UriPartial part)
        {
            if (this.IsNotAbsoluteUri)
            {
                throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
            }
            this.EnsureUriInfo();
            switch (part)
            {
                case UriPartial.Scheme:
                    return this.GetParts(UriComponents.KeepDelimiter | UriComponents.Scheme, UriFormat.UriEscaped);

                case UriPartial.Authority:
                    if (!this.NotAny(Flags.AuthorityFound) && !this.IsDosPath)
                    {
                        return this.GetParts(UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.UriEscaped);
                    }
                    return string.Empty;

                case UriPartial.Path:
                    return this.GetParts(UriComponents.Path | UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.UriEscaped);

                case UriPartial.Query:
                    return this.GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.UriEscaped);
            }
            throw new ArgumentException("part");
        }

        private string GetLocalPath()
        {
            int path;
            this.EnsureParseRemaining();
            if (!this.IsUncOrDosPath)
            {
                return this.GetUnescapedParts(UriComponents.KeepDelimiter | UriComponents.Path, UriFormat.Unescaped);
            }
            this.EnsureHostString(false);
            if (this.NotAny(Flags.HostNotCanonical | Flags.PathNotCanonical | Flags.ShouldBeCompressed))
            {
                path = this.IsUncPath ? (this.m_Info.Offset.Host - 2) : this.m_Info.Offset.Path;
                string str = ((this.IsImplicitFile && (this.m_Info.Offset.Host == (this.IsDosPath ? 0 : 2))) && (this.m_Info.Offset.Query == this.m_Info.Offset.End)) ? this.m_String : ((this.IsDosPath && ((this.m_String[path] == '/') || (this.m_String[path] == '\\'))) ? this.m_String.Substring(path + 1, (this.m_Info.Offset.Query - path) - 1) : this.m_String.Substring(path, this.m_Info.Offset.Query - path));
                if (this.IsDosPath && (str[1] == '|'))
                {
                    str = str.Remove(1, 1).Insert(1, ":");
                }
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '/')
                    {
                        return str.Replace('/', '\\');
                    }
                }
                return str;
            }
            int destPosition = 0;
            path = this.m_Info.Offset.Path;
            string host = this.m_Info.Host;
            char[] dest = new char[((host.Length + 3) + this.m_Info.Offset.Fragment) - this.m_Info.Offset.Path];
            if (this.IsUncPath)
            {
                dest[0] = '\\';
                dest[1] = '\\';
                destPosition = 2;
                UnescapeString(host, 0, host.Length, dest, ref destPosition, 0xffff, 0xffff, 0xffff, UnescapeMode.CopyOnly, this.m_Syntax, false, false);
            }
            else if ((this.m_String[path] == '/') || (this.m_String[path] == '\\'))
            {
                path++;
            }
            ushort num4 = (ushort) destPosition;
            UnescapeMode unescapeMode = (this.InFact(Flags.HostNotParsed | Flags.PathNotCanonical) && !this.IsImplicitFile) ? (UnescapeMode.UnescapeAll | UnescapeMode.Unescape) : UnescapeMode.CopyOnly;
            UnescapeString(this.m_String, path, this.m_Info.Offset.Query, dest, ref destPosition, 0xffff, 0xffff, 0xffff, unescapeMode, this.m_Syntax, true, false);
            if (dest[1] == '|')
            {
                dest[1] = ':';
            }
            if (this.InFact(Flags.HostNotParsed | Flags.ShouldBeCompressed))
            {
                dest = Compress(dest, this.IsDosPath ? ((ushort) (num4 + 2)) : num4, ref destPosition, this.m_Syntax);
            }
            for (ushort i = 0; i < ((ushort) destPosition); i = (ushort) (i + 1))
            {
                if (dest[i] == '/')
                {
                    dest[i] = '\\';
                }
            }
            return new string(dest, 0, destPosition);
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        protected void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            if (this.IsAbsoluteUri)
            {
                serializationInfo.AddValue("AbsoluteUri", this.GetParts(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
            }
            else
            {
                serializationInfo.AddValue("AbsoluteUri", string.Empty);
                serializationInfo.AddValue("RelativeUri", this.GetParts(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
            }
        }

        internal string GetParts(UriComponents uriParts, UriFormat formatAs)
        {
            return this.GetComponents(uriParts, formatAs);
        }

        private string GetRelativeSerializationString(UriFormat format)
        {
            if (format == UriFormat.UriEscaped)
            {
                if (this.m_String.Length == 0)
                {
                    return string.Empty;
                }
                int destPos = 0;
                char[] chArray = EscapeString(this.m_String, 0, this.m_String.Length, null, ref destPos, true, 0xffff, 0xffff, '%');
                if (chArray == null)
                {
                    return this.m_String;
                }
                return new string(chArray, 0, destPos);
            }
            if (format == UriFormat.Unescaped)
            {
                return UnescapeDataString(this.m_String);
            }
            if (format != UriFormat.SafeUnescaped)
            {
                throw new ArgumentOutOfRangeException("format");
            }
            if (this.m_String.Length == 0)
            {
                return string.Empty;
            }
            char[] dest = new char[this.m_String.Length];
            int length = 0;
            return new string(UnescapeString(this.m_String, 0, this.m_String.Length, dest, ref length, 0xffff, 0xffff, 0xffff, UnescapeMode.EscapeUnescape, null, false, true), 0, length);
        }

        private string GetUnescapedParts(UriComponents uriParts, UriFormat formatAs)
        {
            ushort nonCanonical = (ushort) (((ushort) this.m_Flags) & 0x7f);
            if ((uriParts & UriComponents.Path) != 0)
            {
                if ((this.m_Flags & (Flags.BackslashInPath | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed)) != Flags.HostNotParsed)
                {
                    nonCanonical = (ushort) (nonCanonical | 0x10);
                }
                else if (this.IsDosPath && (this.m_String[(this.m_Info.Offset.Path + this.SecuredPathIndex) - 1] == '|'))
                {
                    nonCanonical = (ushort) (nonCanonical | 0x10);
                }
            }
            if ((((ushort) uriParts) & nonCanonical) == 0)
            {
                string uriPartsFromUserString = this.GetUriPartsFromUserString(uriParts);
                if (uriPartsFromUserString != null)
                {
                    return uriPartsFromUserString;
                }
            }
            return this.ReCreateParts(uriParts, nonCanonical, formatAs);
        }

        private string GetUriPartsFromUserString(UriComponents uriParts)
        {
            ushort query;
            switch ((uriParts & ~UriComponents.KeepDelimiter))
            {
                case UriComponents.Query:
                    if (uriParts == UriComponents.Query)
                    {
                        query = (ushort) (this.m_Info.Offset.Query + 1);
                    }
                    else
                    {
                        query = this.m_Info.Offset.Query;
                    }
                    if (query >= this.m_Info.Offset.Fragment)
                    {
                        return string.Empty;
                    }
                    return this.m_String.Substring(query, this.m_Info.Offset.Fragment - query);

                case UriComponents.PathAndQuery:
                    return this.m_String.Substring(this.m_Info.Offset.Path, this.m_Info.Offset.Fragment - this.m_Info.Offset.Path);

                case UriComponents.Scheme:
                    if (uriParts == UriComponents.Scheme)
                    {
                        return this.m_Syntax.SchemeName;
                    }
                    return this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.User - this.m_Info.Offset.Scheme);

                case UriComponents.UserInfo:
                    if (!this.NotAny(Flags.HasUserInfo))
                    {
                        if (uriParts == UriComponents.UserInfo)
                        {
                            query = (ushort) (this.m_Info.Offset.Host - 1);
                        }
                        else
                        {
                            query = this.m_Info.Offset.Host;
                        }
                        if (this.m_Info.Offset.User >= query)
                        {
                            return string.Empty;
                        }
                        return this.m_String.Substring(this.m_Info.Offset.User, query - this.m_Info.Offset.User);
                    }
                    return string.Empty;

                case UriComponents.Host:
                {
                    ushort path = this.m_Info.Offset.Path;
                    if (this.InFact(Flags.HostNotParsed | Flags.NotDefaultPort | Flags.PortNotCanonical))
                    {
                        while (this.m_String[path = (ushort) (path - 1)] != ':')
                        {
                        }
                    }
                    if ((path - this.m_Info.Offset.Host) != 0)
                    {
                        return this.m_String.Substring(this.m_Info.Offset.Host, path - this.m_Info.Offset.Host);
                    }
                    return string.Empty;
                }
                case UriComponents.SchemeAndServer:
                    if (this.InFact(Flags.HasUserInfo))
                    {
                        return (this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.User - this.m_Info.Offset.Scheme) + this.m_String.Substring(this.m_Info.Offset.Host, this.m_Info.Offset.Path - this.m_Info.Offset.Host));
                    }
                    return this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.Path - this.m_Info.Offset.Scheme);

                case (UriComponents.Port | UriComponents.Host | UriComponents.UserInfo):
                    goto Label_06B6;

                case (UriComponents.SchemeAndServer | UriComponents.UserInfo):
                    return this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.Path - this.m_Info.Offset.Scheme);

                case UriComponents.Path:
                    if (((uriParts != UriComponents.Path) || !this.InFact(Flags.AuthorityFound)) || ((this.m_Info.Offset.End <= this.m_Info.Offset.Path) || (this.m_String[this.m_Info.Offset.Path] != '/')))
                    {
                        query = this.m_Info.Offset.Path;
                        break;
                    }
                    query = (ushort) (this.m_Info.Offset.Path + 1);
                    break;

                case UriComponents.HttpRequestUrl:
                    if (!this.InFact(Flags.HasUserInfo))
                    {
                        if ((this.m_Info.Offset.Scheme == 0) && (this.m_Info.Offset.Fragment == this.m_String.Length))
                        {
                            return this.m_String;
                        }
                        return this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.Fragment - this.m_Info.Offset.Scheme);
                    }
                    return (this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.User - this.m_Info.Offset.Scheme) + this.m_String.Substring(this.m_Info.Offset.Host, this.m_Info.Offset.Fragment - this.m_Info.Offset.Host));

                case (UriComponents.HttpRequestUrl | UriComponents.UserInfo):
                    if ((this.m_Info.Offset.Scheme != 0) || (this.m_Info.Offset.Fragment != this.m_String.Length))
                    {
                        return this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.Fragment - this.m_Info.Offset.Scheme);
                    }
                    return this.m_String;

                case UriComponents.Fragment:
                    if (uriParts != UriComponents.Fragment)
                    {
                        query = this.m_Info.Offset.Fragment;
                    }
                    else
                    {
                        query = (ushort) (this.m_Info.Offset.Fragment + 1);
                    }
                    if (query >= this.m_Info.Offset.End)
                    {
                        return string.Empty;
                    }
                    return this.m_String.Substring(query, this.m_Info.Offset.End - query);

                case (UriComponents.Fragment | UriComponents.PathAndQuery):
                    return this.m_String.Substring(this.m_Info.Offset.Path, this.m_Info.Offset.End - this.m_Info.Offset.Path);

                case (UriComponents.Fragment | UriComponents.HttpRequestUrl):
                    if (!this.InFact(Flags.HasUserInfo))
                    {
                        if ((this.m_Info.Offset.Scheme == 0) && (this.m_Info.Offset.End == this.m_String.Length))
                        {
                            return this.m_String;
                        }
                        return this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.End - this.m_Info.Offset.Scheme);
                    }
                    return (this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.User - this.m_Info.Offset.Scheme) + this.m_String.Substring(this.m_Info.Offset.Host, this.m_Info.Offset.End - this.m_Info.Offset.Host));

                case UriComponents.AbsoluteUri:
                    if ((this.m_Info.Offset.Scheme != 0) || (this.m_Info.Offset.End != this.m_String.Length))
                    {
                        return this.m_String.Substring(this.m_Info.Offset.Scheme, this.m_Info.Offset.End - this.m_Info.Offset.Scheme);
                    }
                    return this.m_String;

                case UriComponents.HostAndPort:
                    if (!this.InFact(Flags.HasUserInfo))
                    {
                        goto Label_071C;
                    }
                    if (!this.InFact(Flags.HostNotParsed | Flags.NotDefaultPort) && (this.m_Syntax.DefaultPort != -1))
                    {
                        return (this.m_String.Substring(this.m_Info.Offset.Host, this.m_Info.Offset.Path - this.m_Info.Offset.Host) + ':' + this.m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture));
                    }
                    return this.m_String.Substring(this.m_Info.Offset.Host, this.m_Info.Offset.Path - this.m_Info.Offset.Host);

                case UriComponents.StrongAuthority:
                    goto Label_071C;

                default:
                    return null;
            }
            if (query >= this.m_Info.Offset.Query)
            {
                return string.Empty;
            }
            return this.m_String.Substring(query, this.m_Info.Offset.Query - query);
        Label_06B6:
            if ((this.m_Info.Offset.Path - this.m_Info.Offset.User) != 0)
            {
                return this.m_String.Substring(this.m_Info.Offset.User, this.m_Info.Offset.Path - this.m_Info.Offset.User);
            }
            return string.Empty;
        Label_071C:
            if (this.InFact(Flags.HostNotParsed | Flags.NotDefaultPort) || (this.m_Syntax.DefaultPort == -1))
            {
                goto Label_06B6;
            }
            return (this.m_String.Substring(this.m_Info.Offset.User, this.m_Info.Offset.Path - this.m_Info.Offset.User) + ':' + this.m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture));
        }

        public static string HexEscape(char character)
        {
            if (character > '\x00ff')
            {
                throw new ArgumentOutOfRangeException("character");
            }
            char[] to = new char[3];
            int pos = 0;
            EscapeAsciiChar(character, to, ref pos);
            return new string(to);
        }

        public static char HexUnescape(string pattern, ref int index)
        {
            if ((index < 0) || (index >= pattern.Length))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((pattern[index] == '%') && ((pattern.Length - index) >= 3))
            {
                char ch = EscapedAscii(pattern[index + 1], pattern[index + 2]);
                if (ch != 0xffff)
                {
                    index += 3;
                    return ch;
                }
            }
            return pattern[index++];
        }

        private bool InFact(Flags flags)
        {
            return ((this.m_Flags & flags) != Flags.HostNotParsed);
        }

        private void InitializeUri(ParsingError err, UriKind uriKind, out UriFormatException e)
        {
            if (err == ParsingError.None)
            {
                if (this.IsImplicitFile)
                {
                    if ((this.NotAny(Flags.DosPath) && (uriKind != UriKind.Absolute)) && ((uriKind == UriKind.Relative) || ((this.m_String.Length >= 2) && ((this.m_String[0] != '\\') || (this.m_String[1] != '\\')))))
                    {
                        this.m_Syntax = null;
                        this.m_Flags &= Flags.HostNotParsed | Flags.UserEscaped;
                        e = null;
                        return;
                    }
                    if ((uriKind == UriKind.Relative) && this.InFact(Flags.DosPath))
                    {
                        this.m_Syntax = null;
                        this.m_Flags &= Flags.HostNotParsed | Flags.UserEscaped;
                        e = null;
                        return;
                    }
                }
            }
            else if (err > ParsingError.EmptyUriString)
            {
                this.m_String = null;
                e = GetException(err);
                return;
            }
            bool flag = false;
            if (!s_ConfigInitialized && this.CheckForConfigLoad(this.m_String))
            {
                InitializeUriConfig();
            }
            this.m_iriParsing = s_IriParsing && ((this.m_Syntax == null) || this.m_Syntax.InFact(UriSyntaxFlags.AllowIriParsing));
            if (this.m_iriParsing && this.CheckForUnicode(this.m_String))
            {
                this.m_Flags |= Flags.HasUnicode;
                flag = true;
                this.m_originalUnicodeString = this.m_String;
            }
            if (this.m_Syntax != null)
            {
                if (!this.m_Syntax.IsSimple)
                {
                    this.m_Syntax = this.m_Syntax.InternalOnNewUri();
                    this.m_Flags |= Flags.HostNotParsed | Flags.UserDrivenParsing;
                    this.m_Syntax.InternalValidate(this, out e);
                    if (e == null)
                    {
                        if ((err != ParsingError.None) || this.InFact(Flags.ErrorOrParsingRecursion))
                        {
                            this.SetUserDrivenParsing();
                        }
                        else if (uriKind == UriKind.Relative)
                        {
                            e = GetException(ParsingError.CannotCreateRelative);
                        }
                        if (this.m_iriParsing && flag)
                        {
                            this.EnsureParseRemaining();
                        }
                    }
                    else if (((uriKind != UriKind.Absolute) && (err != ParsingError.None)) && (err <= ParsingError.EmptyUriString))
                    {
                        this.m_Syntax = null;
                        e = null;
                        this.m_Flags &= Flags.HostNotParsed | Flags.UserEscaped;
                    }
                }
                else
                {
                    if ((err = this.PrivateParseMinimal()) != ParsingError.None)
                    {
                        if ((uriKind != UriKind.Absolute) && (err <= ParsingError.EmptyUriString))
                        {
                            this.m_Syntax = null;
                            e = null;
                            this.m_Flags &= Flags.HostNotParsed | Flags.UserEscaped;
                        }
                        else
                        {
                            e = GetException(err);
                        }
                    }
                    else if (uriKind == UriKind.Relative)
                    {
                        e = GetException(ParsingError.CannotCreateRelative);
                    }
                    else
                    {
                        e = null;
                    }
                    if (this.m_iriParsing && flag)
                    {
                        this.EnsureParseRemaining();
                    }
                }
            }
            else if (((err != ParsingError.None) && (uriKind != UriKind.Absolute)) && (err <= ParsingError.EmptyUriString))
            {
                e = null;
                this.m_Flags &= Flags.HasUnicode | Flags.UserEscaped;
                if (this.m_iriParsing && flag)
                {
                    this.m_String = this.EscapeUnescapeIri(this.m_originalUnicodeString, 0, this.m_originalUnicodeString.Length, 0);
                    try
                    {
                        this.m_String = this.m_String.Normalize(NormalizationForm.FormC);
                    }
                    catch (ArgumentException)
                    {
                        e = GetException(ParsingError.BadFormat);
                    }
                }
            }
            else
            {
                this.m_String = null;
                e = GetException(err);
            }
        }

        private static void InitializeUriConfig()
        {
            if (!s_ConfigInitialized)
            {
                lock (InitializeLock)
                {
                    if (!s_ConfigInitialized && !s_ConfigInitializing)
                    {
                        s_ConfigInitializing = true;
                        UriSectionInternal section = UriSectionInternal.GetSection();
                        if (section != null)
                        {
                            s_IdnScope = section.IdnScope;
                            s_IriParsing = section.IriParsing;
                            SetEscapedDotSlashSettings(section, "http");
                            SetEscapedDotSlashSettings(section, "https");
                        }
                        s_ConfigInitialized = true;
                        s_ConfigInitializing = false;
                    }
                }
            }
        }

        internal static string InternalEscapeString(string rawString)
        {
            if (rawString == null)
            {
                return string.Empty;
            }
            int destPos = 0;
            char[] chArray = EscapeString(rawString, 0, rawString.Length, null, ref destPos, true, '?', '#', '%');
            if (chArray == null)
            {
                return rawString;
            }
            return new string(chArray, 0, destPos);
        }

        internal unsafe bool InternalIsWellFormedOriginalString()
        {
            if (this.UserDrivenParsing)
            {
                throw new InvalidOperationException(System.SR.GetString("net_uri_UserDrivenParsing", new object[] { base.GetType().FullName }));
            }
            fixed (char* str = ((char*) this.m_String))
            {
                char* chPtr = str;
                ushort idx = 0;
                if (!this.IsAbsoluteUri)
                {
                    return ((this.CheckCanonical(chPtr, ref idx, (ushort) this.m_String.Length, 0xfffe) & (Check.BackslashInPath | Check.EscapedCanonical)) == Check.EscapedCanonical);
                }
                if (this.IsImplicitFile)
                {
                    return false;
                }
                this.EnsureParseRemaining();
                Flags flags = this.m_Flags & (Flags.E_CannotDisplayCanonical | Flags.FragmentIriCanonical | Flags.PathIriCanonical | Flags.QueryIriCanonical | Flags.UserIriCanonical);
                if ((((flags & Flags.E_CannotDisplayCanonical) & (Flags.E_FragmentNotCanonical | Flags.E_PathNotCanonical | Flags.E_QueryNotCanonical | Flags.E_UserNotCanonical)) != Flags.HostNotParsed) && (!this.m_iriParsing || (((this.m_iriParsing && (((flags & Flags.E_UserNotCanonical) == Flags.HostNotParsed) || ((flags & (Flags.HostNotParsed | Flags.UserIriCanonical)) == Flags.HostNotParsed))) && (((flags & Flags.E_PathNotCanonical) == Flags.HostNotParsed) || ((flags & (Flags.HostNotParsed | Flags.PathIriCanonical)) == Flags.HostNotParsed))) && ((((flags & Flags.E_QueryNotCanonical) == Flags.HostNotParsed) || ((flags & (Flags.HostNotParsed | Flags.QueryIriCanonical)) == Flags.HostNotParsed)) && (((flags & Flags.E_FragmentNotCanonical) == Flags.HostNotParsed) || ((flags & Flags.FragmentIriCanonical) == Flags.HostNotParsed))))))
                {
                    return false;
                }
                if (this.InFact(Flags.AuthorityFound))
                {
                    idx = (ushort) ((this.m_Info.Offset.Scheme + this.m_Syntax.SchemeName.Length) + 2);
                    if (((idx >= this.m_Info.Offset.User) || (this.m_String[idx - 1] == '\\')) || (this.m_String[idx] == '\\'))
                    {
                        return false;
                    }
                    if (this.InFact(Flags.DosPath | Flags.UncPath))
                    {
                        while (((idx = (ushort) (idx + 1)) < this.m_Info.Offset.User) && ((this.m_String[idx] == '/') || (this.m_String[idx] == '\\')))
                        {
                            return false;
                        }
                    }
                }
                if (this.InFact(Flags.FirstSlashAbsent) && (this.m_Info.Offset.Query > this.m_Info.Offset.Path))
                {
                    return false;
                }
                if (this.InFact(Flags.BackslashInPath))
                {
                    return false;
                }
                if (this.IsDosPath && (this.m_String[(this.m_Info.Offset.Path + this.SecuredPathIndex) - 1] == '|'))
                {
                    return false;
                }
                if ((this.m_Flags & Flags.CanonicalDnsHost) == Flags.HostNotParsed)
                {
                    idx = this.m_Info.Offset.User;
                    if (!this.m_iriParsing || (this.HostType != (Flags.HostNotParsed | Flags.IPv6HostType)))
                    {
                        Check check = this.CheckCanonical(chPtr, ref idx, this.m_Info.Offset.Path, '/');
                        if (((check & (Check.ReservedFound | Check.BackslashInPath | Check.EscapedCanonical)) != Check.EscapedCanonical) && (!this.m_iriParsing || (this.m_iriParsing && ((check & (Check.NotIriCanonical | Check.FoundNonAscii | Check.DisplayCanonical)) != (Check.FoundNonAscii | Check.DisplayCanonical)))))
                        {
                            return false;
                        }
                    }
                }
                if ((this.m_Flags & (Flags.AuthorityFound | Flags.SchemeNotCanonical)) == (Flags.AuthorityFound | Flags.SchemeNotCanonical))
                {
                    idx = (ushort) this.m_Syntax.SchemeName.Length;
                    do
                    {
                        idx = (ushort) (idx + 1);
                    }
                    while (chPtr[idx] != ':');
                    if ((((idx + 1) >= this.m_String.Length) || (chPtr[idx] != '/')) || (chPtr[idx + 1] != '/'))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IriParsingStatic(UriParser syntax)
        {
            if (!s_IriParsing)
            {
                return false;
            }
            return (((syntax != null) && syntax.InFact(UriSyntaxFlags.AllowIriParsing)) || (syntax == null));
        }

        private static bool IsAsciiLetter(char character)
        {
            return (((character >= 'a') && (character <= 'z')) || ((character >= 'A') && (character <= 'Z')));
        }

        private static bool IsAsciiLetterOrDigit(char character)
        {
            return (IsAsciiLetter(character) || ((character >= '0') && (character <= '9')));
        }

        [Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual bool IsBadFileSystemCharacter(char character)
        {
            if (((((character >= ' ') && (character != ';')) && ((character != '/') && (character != '?'))) && (((character != ':') && (character != '&')) && ((character != '=') && (character != ',')))) && ((((character != '*') && (character != '<')) && ((character != '>') && (character != '"'))) && ((character != '|') && (character != '\\'))))
            {
                return (character == '^');
            }
            return true;
        }

        public bool IsBaseOf(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (!this.IsAbsoluteUri)
            {
                return false;
            }
            if (this.Syntax.IsSimple)
            {
                return this.IsBaseOfHelper(uri);
            }
            return this.Syntax.InternalIsBaseOf(this, uri);
        }

        internal unsafe bool IsBaseOfHelper(Uri uriLink)
        {
            if (!this.IsAbsoluteUri || this.UserDrivenParsing)
            {
                return false;
            }
            if (!uriLink.IsAbsoluteUri)
            {
                UriFormatException exception;
                string newUriString = null;
                bool userEscaped = false;
                uriLink = ResolveHelper(this, uriLink, ref newUriString, ref userEscaped, out exception);
                if (exception != null)
                {
                    return false;
                }
                if (uriLink == null)
                {
                    uriLink = CreateHelper(newUriString, userEscaped, UriKind.Absolute, ref exception);
                }
                if (exception != null)
                {
                    return false;
                }
            }
            if (this.Syntax.SchemeName != uriLink.Syntax.SchemeName)
            {
                return false;
            }
            string parts = this.GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.SafeUnescaped);
            string str3 = uriLink.GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.SafeUnescaped);
            fixed (char* str4 = ((char*) parts))
            {
                char* pMe = str4;
                fixed (char* str5 = ((char*) str3))
                {
                    char* pShe = str5;
                    return TestForSubPath(pMe, (ushort) parts.Length, pShe, (ushort) str3.Length, this.IsUncOrDosPath || uriLink.IsUncOrDosPath);
                }
            }
        }

        internal static bool IsBidiControlCharacter(char ch)
        {
            if ((((ch != '‎') && (ch != '‏')) && ((ch != '‪') && (ch != '‫'))) && ((ch != '‬') && (ch != '‭')))
            {
                return (ch == '‮');
            }
            return true;
        }

        [Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected static bool IsExcludedCharacter(char character)
        {
            if (((((character > ' ') && (character < '\x007f')) && ((character != '<') && (character != '>'))) && (((character != '#') && (character != '%')) && ((character != '"') && (character != '{')))) && ((((character != '}') && (character != '|')) && ((character != '\\') && (character != '^'))) && ((character != '[') && (character != ']'))))
            {
                return (character == '`');
            }
            return true;
        }

        internal static bool IsGenDelim(char ch)
        {
            if ((((ch != ':') && (ch != '/')) && ((ch != '?') && (ch != '#'))) && ((ch != '[') && (ch != ']')))
            {
                return (ch == '@');
            }
            return true;
        }

        public static bool IsHexDigit(char character)
        {
            if (((character < '0') || (character > '9')) && ((character < 'A') || (character > 'F')))
            {
                return ((character >= 'a') && (character <= 'f'));
            }
            return true;
        }

        public static bool IsHexEncoding(string pattern, int index)
        {
            if ((pattern.Length - index) < 3)
            {
                return false;
            }
            return ((pattern[index] == '%') && (EscapedAscii(pattern[index + 1], pattern[index + 1]) != 0xffff));
        }

        private bool IsIntranet(string schemeHost)
        {
            bool flag = false;
            int pdwZone = -1;
            int num2 = -2147467259;
            if (this.m_Syntax.SchemeName.Length > 0x20)
            {
                return false;
            }
            if (s_ManagerRef == null)
            {
                lock (s_IntranetLock)
                {
                    if (s_ManagerRef == null)
                    {
                        s_ManagerRef = (IInternetSecurityManager) new InternetSecurityManager();
                    }
                }
            }
            try
            {
                s_ManagerRef.MapUrlToZone(schemeHost.TrimStart(_WSchars), out pdwZone, 0);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == num2)
                {
                    flag = true;
                }
            }
            if (pdwZone != 1)
            {
                if (((pdwZone != 2) && (pdwZone != 4)) && !flag)
                {
                    return false;
                }
                for (int i = 0; i < schemeHost.Length; i++)
                {
                    if (schemeHost[i] == '.')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsLWS(char ch)
        {
            if (ch > ' ')
            {
                return false;
            }
            if (((ch != ' ') && (ch != '\n')) && (ch != '\r'))
            {
                return (ch == '\t');
            }
            return true;
        }

        private static bool IsNotReservedNotUnreservedNotHash(char c)
        {
            if ((c <= 'z') || (c == '~'))
            {
                if (((c > 'Z') && (c < 'a')) && (c != '_'))
                {
                    return true;
                }
                if (c < '!')
                {
                    return true;
                }
                if (((c != '>') && (c != '<')) && (((c != '%') && (c != '"')) && (c != '`')))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsNotSafeForUnescape(char ch)
        {
            if (((ch > '\x001f') && ((ch < '\x007f') || (ch > '\x009f'))) && (((((ch < ';') || (ch > '@')) || ((ch | '\x0002') == 0x3e)) && ((ch < '#') || (ch > '&'))) && (((ch != '+') && (ch != ',')) && ((ch != '/') && (ch != '\\')))))
            {
                return false;
            }
            return true;
        }

        private static bool IsNotUnreserved(char c)
        {
            if ((c <= 'z') || (c == '~'))
            {
                if (((c > '9') && (c < 'A')) || (((c > 'Z') && (c < 'a')) && (c != '_')))
                {
                    return true;
                }
                if (((c >= '\'') || (c == '!')) && (((c != '+') && (c != ',')) && (c != '/')))
                {
                    return false;
                }
            }
            return true;
        }

        [Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual bool IsReservedCharacter(char character)
        {
            if ((((character != ';') && (character != '/')) && ((character != ':') && (character != '@'))) && (((character != '&') && (character != '=')) && ((character != '+') && (character != '$'))))
            {
                return (character == ',');
            }
            return true;
        }

        public bool IsWellFormedOriginalString()
        {
            if (!this.IsNotAbsoluteUri && !this.Syntax.IsSimple)
            {
                return this.Syntax.InternalIsWellFormedOriginalString(this);
            }
            return this.InternalIsWellFormedOriginalString();
        }

        public static bool IsWellFormedUriString(string uriString, UriKind uriKind)
        {
            Uri uri;
            if (!TryCreate(uriString, uriKind, out uri))
            {
                return false;
            }
            return uri.IsWellFormedOriginalString();
        }

        [Obsolete("The method has been deprecated. Please use MakeRelativeUri(Uri uri). http://go.microsoft.com/fwlink/?linkid=14202")]
        public string MakeRelative(Uri toUri)
        {
            if (toUri == null)
            {
                throw new ArgumentNullException("toUri");
            }
            if (this.IsNotAbsoluteUri || toUri.IsNotAbsoluteUri)
            {
                throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
            }
            if (((this.Scheme == toUri.Scheme) && (this.Host == toUri.Host)) && (this.Port == toUri.Port))
            {
                return PathDifference(this.AbsolutePath, toUri.AbsolutePath, !this.IsUncOrDosPath);
            }
            return toUri.ToString();
        }

        public Uri MakeRelativeUri(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (this.IsNotAbsoluteUri || uri.IsNotAbsoluteUri)
            {
                throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
            }
            if ((!(this.Scheme == uri.Scheme) || !(this.Host == uri.Host)) || (this.Port != uri.Port))
            {
                return uri;
            }
            string absolutePath = uri.AbsolutePath;
            string uriString = PathDifference(this.AbsolutePath, absolutePath, !this.IsUncOrDosPath);
            if (CheckForColonInFirstPathSegment(uriString) && (!uri.IsDosPath || !absolutePath.Equals(uriString, StringComparison.Ordinal)))
            {
                uriString = "./" + uriString;
            }
            return new Uri(uriString + uri.GetParts(UriComponents.Fragment | UriComponents.Query, UriFormat.UriEscaped), UriKind.Relative);
        }

        private static unsafe void MatchUTF8Sequence(char* pDest, char[] dest, ref int destOffset, char[] unescapedChars, int charCount, byte[] bytes, bool isQuery, bool iriParsing)
        {
            char[] chArray;
            int index = 0;
            if (((chArray = unescapedChars) == null) || (chArray.Length == 0))
            {
                chRef = null;
                goto Label_001C;
            }
            fixed (char* chRef = chArray)
            {
                int num2;
            Label_001C:
                num2 = 0;
                while (num2 < charCount)
                {
                    bool flag = char.IsHighSurrogate(chRef[num2]);
                    byte[] buffer = Encoding.UTF8.GetBytes(unescapedChars, num2, flag ? 2 : 1);
                    int length = buffer.Length;
                    bool flag2 = false;
                    if (iriParsing)
                    {
                        if (!flag)
                        {
                            flag2 = CheckIriUnicodeRange(unescapedChars[num2], isQuery);
                        }
                        else
                        {
                            bool surrogatePair = false;
                            flag2 = CheckIriUnicodeRange(unescapedChars[num2], unescapedChars[num2 + 1], ref surrogatePair, isQuery);
                        }
                    }
                Label_008B:
                    while (bytes[index] != buffer[0])
                    {
                        EscapeAsciiChar((char) bytes[index++], dest, ref destOffset);
                    }
                    bool flag4 = true;
                    int num4 = 0;
                    while (num4 < length)
                    {
                        if (bytes[index + num4] != buffer[num4])
                        {
                            flag4 = false;
                            break;
                        }
                        num4++;
                    }
                    if (flag4)
                    {
                        index += length;
                        if (iriParsing)
                        {
                            if (!flag2)
                            {
                                for (int i = 0; i < buffer.Length; i++)
                                {
                                    EscapeAsciiChar((char) buffer[i], dest, ref destOffset);
                                }
                            }
                            else if (!IsBidiControlCharacter(chRef[num2]))
                            {
                                pDest[destOffset++] = chRef[num2];
                            }
                            if (flag)
                            {
                                pDest[destOffset++] = chRef[num2 + 1];
                            }
                        }
                        else
                        {
                            pDest[destOffset++] = chRef[num2];
                            if (flag)
                            {
                                pDest[destOffset++] = chRef[num2 + 1];
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < num4; j++)
                        {
                            EscapeAsciiChar((char) bytes[index++], dest, ref destOffset);
                        }
                        goto Label_008B;
                    }
                    if (flag)
                    {
                        num2++;
                    }
                    num2++;
                }
            }
        }

        private bool NotAny(Flags flags)
        {
            return ((this.m_Flags & flags) == Flags.HostNotParsed);
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public static bool operator ==(Uri uri1, Uri uri2)
        {
            return ((uri1 == uri2) || (((uri1 != null) && (uri2 != null)) && uri2.Equals(uri1)));
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public static bool operator !=(Uri uri1, Uri uri2)
        {
            if (uri1 == uri2)
            {
                return false;
            }
            if ((uri1 != null) && (uri2 != null))
            {
                return !uri2.Equals(uri1);
            }
            return true;
        }

        [Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void Parse()
        {
        }

        internal UriFormatException ParseMinimal()
        {
            ParsingError err = this.PrivateParseMinimal();
            if (err == ParsingError.None)
            {
                return null;
            }
            this.m_Flags |= Flags.ErrorOrParsingRecursion;
            return GetException(err);
        }

        private unsafe void ParseRemaining()
        {
            this.EnsureUriInfo();
            Flags hostNotParsed = Flags.HostNotParsed;
            if (!this.UserDrivenParsing)
            {
                bool flag = (this.m_iriParsing && ((this.m_Flags & Flags.HasUnicode) != Flags.HostNotParsed)) && ((this.m_Flags & (Flags.HostNotParsed | Flags.RestUnicodeNormalized)) == Flags.HostNotParsed);
                ushort scheme = this.m_Info.Offset.Scheme;
                ushort length = (ushort) this.m_String.Length;
                Check none = Check.None;
                UriSyntaxFlags flags = this.m_Syntax.Flags;
                fixed (char* str4 = ((char*) this.m_String))
                {
                    char* chPtr = str4;
                    if ((length > scheme) && IsLWS(chPtr[length - 1]))
                    {
                        length = (ushort) (length - 1);
                        while ((length != scheme) && IsLWS(chPtr[length = (ushort) (length - 1)]))
                        {
                        }
                        length = (ushort) (length + 1);
                    }
                    if (this.IsImplicitFile)
                    {
                        hostNotParsed |= Flags.HostNotParsed | Flags.SchemeNotCanonical;
                    }
                    else
                    {
                        ushort num4 = 0;
                        ushort num5 = (ushort) this.m_Syntax.SchemeName.Length;
                        while (num4 < num5)
                        {
                            if (this.m_Syntax.SchemeName[num4] != chPtr[scheme + num4])
                            {
                                hostNotParsed |= Flags.HostNotParsed | Flags.SchemeNotCanonical;
                            }
                            num4 = (ushort) (num4 + 1);
                        }
                        if (((this.m_Flags & Flags.AuthorityFound) != Flags.HostNotParsed) && (((((scheme + num4) + 3) >= length) || (chPtr[(scheme + num4) + 1] != '/')) || (chPtr[(scheme + num4) + 2] != '/')))
                        {
                            hostNotParsed |= Flags.HostNotParsed | Flags.SchemeNotCanonical;
                        }
                    }
                    if ((this.m_Flags & Flags.HasUserInfo) != Flags.HostNotParsed)
                    {
                        scheme = this.m_Info.Offset.User;
                        none = this.CheckCanonical(chPtr, ref scheme, this.m_Info.Offset.Host, '@');
                        if ((none & Check.DisplayCanonical) == Check.None)
                        {
                            hostNotParsed |= Flags.HostNotParsed | Flags.UserNotCanonical;
                        }
                        if ((none & (Check.BackslashInPath | Check.EscapedCanonical)) != Check.EscapedCanonical)
                        {
                            hostNotParsed |= Flags.E_UserNotCanonical;
                        }
                        if (this.m_iriParsing && ((none & (Check.NotIriCanonical | Check.BackslashInPath | Check.FoundNonAscii | Check.DisplayCanonical | Check.EscapedCanonical)) == (Check.FoundNonAscii | Check.DisplayCanonical)))
                        {
                            hostNotParsed |= Flags.HostNotParsed | Flags.UserIriCanonical;
                        }
                    }
                }
                scheme = this.m_Info.Offset.Path;
                ushort path = this.m_Info.Offset.Path;
                if (flag)
                {
                    if (this.IsDosPath)
                    {
                        if (this.IsImplicitFile)
                        {
                            this.m_String = string.Empty;
                        }
                        else
                        {
                            this.m_String = this.m_Syntax.SchemeName + SchemeDelimiter;
                        }
                    }
                    this.m_Info.Offset.Path = (ushort) this.m_String.Length;
                    scheme = this.m_Info.Offset.Path;
                    ushort start = path;
                    if (this.IsImplicitFile || ((flags & (UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery)) == UriSyntaxFlags.None))
                    {
                        this.FindEndOfComponent(this.m_originalUnicodeString, ref path, (ushort) this.m_originalUnicodeString.Length, 0xffff);
                    }
                    else
                    {
                        this.FindEndOfComponent(this.m_originalUnicodeString, ref path, (ushort) this.m_originalUnicodeString.Length, this.m_Syntax.InFact(UriSyntaxFlags.MayHaveQuery) ? '?' : (this.m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment) ? '#' : ((char) 0xfffe)));
                    }
                    string str = this.EscapeUnescapeIri(this.m_originalUnicodeString, start, path, UriComponents.Path);
                    try
                    {
                        this.m_String = this.m_String + str.Normalize(NormalizationForm.FormC);
                    }
                    catch (ArgumentException)
                    {
                        throw GetException(ParsingError.BadFormat);
                    }
                    length = (ushort) this.m_String.Length;
                }
                fixed (char* str5 = ((char*) this.m_String))
                {
                    char* chPtr2 = str5;
                    if (this.IsImplicitFile || ((flags & (UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.MayHaveQuery)) == UriSyntaxFlags.None))
                    {
                        none = this.CheckCanonical(chPtr2, ref scheme, length, 0xffff);
                    }
                    else
                    {
                        none = this.CheckCanonical(chPtr2, ref scheme, length, ((flags & UriSyntaxFlags.MayHaveQuery) != UriSyntaxFlags.None) ? '?' : (this.m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment) ? '#' : ((char) 0xfffe)));
                    }
                    if ((((this.m_Flags & Flags.AuthorityFound) != Flags.HostNotParsed) && ((flags & UriSyntaxFlags.PathIsRooted) != UriSyntaxFlags.None)) && ((this.m_Info.Offset.Path == length) || ((chPtr2[this.m_Info.Offset.Path] != '/') && (chPtr2[this.m_Info.Offset.Path] != '\\'))))
                    {
                        hostNotParsed |= Flags.FirstSlashAbsent;
                    }
                }
                bool flag2 = false;
                if (this.IsDosPath || (((this.m_Flags & Flags.AuthorityFound) != Flags.HostNotParsed) && (((flags & (UriSyntaxFlags.CompressPath | UriSyntaxFlags.ConvertPathSlashes)) != UriSyntaxFlags.None) || this.m_Syntax.InFact(UriSyntaxFlags.UnEscapeDotsAndSlashes))))
                {
                    if (((none & Check.DotSlashEscaped) != Check.None) && this.m_Syntax.InFact(UriSyntaxFlags.UnEscapeDotsAndSlashes))
                    {
                        hostNotParsed |= Flags.E_PathNotCanonical | Flags.PathNotCanonical;
                        flag2 = true;
                    }
                    if (((flags & UriSyntaxFlags.ConvertPathSlashes) != UriSyntaxFlags.None) && ((none & Check.BackslashInPath) != Check.None))
                    {
                        hostNotParsed |= Flags.E_PathNotCanonical | Flags.PathNotCanonical;
                        flag2 = true;
                    }
                    if (((flags & UriSyntaxFlags.CompressPath) != UriSyntaxFlags.None) && (((hostNotParsed & Flags.E_PathNotCanonical) != Flags.HostNotParsed) || ((none & Check.DotSlashAttn) != Check.None)))
                    {
                        hostNotParsed |= Flags.HostNotParsed | Flags.ShouldBeCompressed;
                    }
                    if ((none & Check.BackslashInPath) != Check.None)
                    {
                        hostNotParsed |= Flags.BackslashInPath;
                    }
                }
                else if ((none & Check.BackslashInPath) != Check.None)
                {
                    hostNotParsed |= Flags.E_PathNotCanonical;
                    flag2 = true;
                }
                if (((none & Check.DisplayCanonical) == Check.None) && ((((this.m_Flags & (Flags.HostNotParsed | Flags.ImplicitFile)) == Flags.HostNotParsed) || ((this.m_Flags & (Flags.HostNotParsed | Flags.UserEscaped)) != Flags.HostNotParsed)) || ((none & Check.ReservedFound) != Check.None)))
                {
                    hostNotParsed |= Flags.HostNotParsed | Flags.PathNotCanonical;
                    flag2 = true;
                }
                if (((this.m_Flags & (Flags.HostNotParsed | Flags.ImplicitFile)) != Flags.HostNotParsed) && ((none & (Check.ReservedFound | Check.EscapedCanonical)) != Check.None))
                {
                    none &= ~Check.EscapedCanonical;
                }
                if ((none & Check.EscapedCanonical) == Check.None)
                {
                    hostNotParsed |= Flags.E_PathNotCanonical;
                }
                if (this.m_iriParsing && (!flag2 & ((none & (Check.NotIriCanonical | Check.FoundNonAscii | Check.DisplayCanonical | Check.EscapedCanonical)) == (Check.FoundNonAscii | Check.DisplayCanonical))))
                {
                    hostNotParsed |= Flags.HostNotParsed | Flags.PathIriCanonical;
                }
                if (flag)
                {
                    ushort num7 = path;
                    if ((path < this.m_originalUnicodeString.Length) && (this.m_originalUnicodeString[path] == '?'))
                    {
                        path = (ushort) (path + 1);
                        this.FindEndOfComponent(this.m_originalUnicodeString, ref path, (ushort) this.m_originalUnicodeString.Length, ((flags & UriSyntaxFlags.MayHaveFragment) != UriSyntaxFlags.None) ? '#' : ((char) 0xfffe));
                        string str2 = this.EscapeUnescapeIri(this.m_originalUnicodeString, num7, path, UriComponents.Query);
                        try
                        {
                            this.m_String = this.m_String + str2.Normalize(NormalizationForm.FormC);
                        }
                        catch (ArgumentException)
                        {
                            throw GetException(ParsingError.BadFormat);
                        }
                        length = (ushort) this.m_String.Length;
                    }
                }
                this.m_Info.Offset.Query = scheme;
                fixed (char* str6 = ((char*) this.m_String))
                {
                    char* chPtr3 = str6;
                    if ((scheme < length) && (chPtr3[scheme] == '?'))
                    {
                        scheme = (ushort) (scheme + 1);
                        none = this.CheckCanonical(chPtr3, ref scheme, length, ((flags & UriSyntaxFlags.MayHaveFragment) != UriSyntaxFlags.None) ? '#' : ((char) 0xfffe));
                        if ((none & Check.DisplayCanonical) == Check.None)
                        {
                            hostNotParsed |= Flags.HostNotParsed | Flags.QueryNotCanonical;
                        }
                        if ((none & (Check.BackslashInPath | Check.EscapedCanonical)) != Check.EscapedCanonical)
                        {
                            hostNotParsed |= Flags.E_QueryNotCanonical;
                        }
                        if (this.m_iriParsing && ((none & (Check.NotIriCanonical | Check.BackslashInPath | Check.FoundNonAscii | Check.DisplayCanonical | Check.EscapedCanonical)) == (Check.FoundNonAscii | Check.DisplayCanonical)))
                        {
                            hostNotParsed |= Flags.HostNotParsed | Flags.QueryIriCanonical;
                        }
                    }
                }
                if (flag)
                {
                    ushort num8 = path;
                    if ((path < this.m_originalUnicodeString.Length) && (this.m_originalUnicodeString[path] == '#'))
                    {
                        path = (ushort) (path + 1);
                        this.FindEndOfComponent(this.m_originalUnicodeString, ref path, (ushort) this.m_originalUnicodeString.Length, 0xfffe);
                        string str3 = this.EscapeUnescapeIri(this.m_originalUnicodeString, num8, path, UriComponents.Fragment);
                        try
                        {
                            this.m_String = this.m_String + str3.Normalize(NormalizationForm.FormC);
                        }
                        catch (ArgumentException)
                        {
                            throw GetException(ParsingError.BadFormat);
                        }
                        length = (ushort) this.m_String.Length;
                    }
                }
                this.m_Info.Offset.Fragment = scheme;
                fixed (char* str7 = ((char*) this.m_String))
                {
                    char* chPtr4 = str7;
                    if ((scheme < length) && (chPtr4[scheme] == '#'))
                    {
                        scheme = (ushort) (scheme + 1);
                        none = this.CheckCanonical(chPtr4, ref scheme, length, 0xfffe);
                        if ((none & Check.DisplayCanonical) == Check.None)
                        {
                            hostNotParsed |= Flags.FragmentNotCanonical;
                        }
                        if ((none & (Check.BackslashInPath | Check.EscapedCanonical)) != Check.EscapedCanonical)
                        {
                            hostNotParsed |= Flags.E_FragmentNotCanonical;
                        }
                        if (this.m_iriParsing && ((none & (Check.NotIriCanonical | Check.BackslashInPath | Check.FoundNonAscii | Check.DisplayCanonical | Check.EscapedCanonical)) == (Check.FoundNonAscii | Check.DisplayCanonical)))
                        {
                            hostNotParsed |= Flags.FragmentIriCanonical;
                        }
                    }
                }
                this.m_Info.Offset.End = scheme;
            }
            hostNotParsed |= Flags.AllUriInfoSet;
            lock (this.m_Info)
            {
                this.m_Flags |= hostNotParsed;
            }
            this.m_Flags |= Flags.HostNotParsed | Flags.RestUnicodeNormalized;
        }

        private static unsafe ParsingError ParseScheme(string uriString, ref Flags flags, ref UriParser syntax)
        {
            int length = uriString.Length;
            if (length == 0)
            {
                return ParsingError.EmptyUriString;
            }
            if (length >= 0xfff0)
            {
                return ParsingError.SizeLimit;
            }
            fixed (char* str = ((char*) uriString))
            {
                char* chPtr = str;
                ParsingError none = ParsingError.None;
                ushort num2 = ParseSchemeCheckImplicitFile(chPtr, (ushort) length, ref none, ref flags, ref syntax);
                if (none != ParsingError.None)
                {
                    return none;
                }
                flags |= (Flags) num2;
            }
            return ParsingError.None;
        }

        private static unsafe ushort ParseSchemeCheckImplicitFile(char* uriString, ushort length, ref ParsingError err, ref Flags flags, ref UriParser syntax)
        {
            ushort index = 0;
            while ((index < length) && IsLWS(uriString[index]))
            {
                index = (ushort) (index + 1);
            }
            ushort num2 = index;
            while ((num2 < length) && (uriString[num2] != ':'))
            {
                num2 = (ushort) (num2 + 1);
            }
            if (((IntPtr.Size != 4) || (num2 == length)) || ((num2 < (index + 3)) || !CheckKnownSchemes((long*) (uriString + index), (ushort) (num2 - index), ref syntax)))
            {
                char ch;
                if (((index + 2) >= length) || (num2 == index))
                {
                    err = ParsingError.BadFormat;
                    return 0;
                }
                if (((ch = uriString[index + 1]) == ':') || (ch == '|'))
                {
                    if (IsAsciiLetter(uriString[index]))
                    {
                        if (((ch = uriString[index + 2]) == '\\') || (ch == '/'))
                        {
                            flags |= Flags.AuthorityFound | Flags.DosPath | Flags.ImplicitFile;
                            syntax = UriParser.FileUri;
                            return index;
                        }
                        err = ParsingError.MustRootedPath;
                        return 0;
                    }
                    if (ch == ':')
                    {
                        err = ParsingError.BadScheme;
                    }
                    else
                    {
                        err = ParsingError.BadFormat;
                    }
                    return 0;
                }
                if (((ch = uriString[index]) == '/') || (ch == '\\'))
                {
                    if (((ch = uriString[index + 1]) == '\\') || (ch == '/'))
                    {
                        flags |= Flags.AuthorityFound | Flags.ImplicitFile | Flags.UncPath;
                        syntax = UriParser.FileUri;
                        index = (ushort) (index + 2);
                        while ((index < length) && (((ch = uriString[index]) == '/') || (ch == '\\')))
                        {
                            index = (ushort) (index + 1);
                        }
                        return index;
                    }
                    err = ParsingError.BadFormat;
                    return 0;
                }
                if (num2 == length)
                {
                    err = ParsingError.BadFormat;
                    return 0;
                }
                if ((num2 - index) > 0x400)
                {
                    err = ParsingError.SchemeLimit;
                    return 0;
                }
                char* ptr = (char*) stackalloc byte[(((IntPtr) (num2 - index)) * 2)];
                length = 0;
                while (index < num2)
                {
                    length = (ushort) (length + 1);
                    ptr[length] = uriString[index];
                    index = (ushort) (index + 1);
                }
                err = CheckSchemeSyntax(ptr, length, ref syntax);
                if (err != ParsingError.None)
                {
                    return 0;
                }
            }
            return (ushort) (num2 + 1);
        }

        private static string PathDifference(string path1, string path2, bool compareCase)
        {
            int num2 = -1;
            int num = 0;
            while ((num < path1.Length) && (num < path2.Length))
            {
                if ((path1[num] != path2[num]) && (compareCase || (char.ToLower(path1[num], CultureInfo.InvariantCulture) != char.ToLower(path2[num], CultureInfo.InvariantCulture))))
                {
                    break;
                }
                if (path1[num] == '/')
                {
                    num2 = num;
                }
                num++;
            }
            if (num == 0)
            {
                return path2;
            }
            if ((num == path1.Length) && (num == path2.Length))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            while (num < path1.Length)
            {
                if (path1[num] == '/')
                {
                    builder.Append("../");
                }
                num++;
            }
            if ((builder.Length == 0) && ((path2.Length - 1) == num2))
            {
                return "./";
            }
            return (builder.ToString() + path2.Substring(num2 + 1));
        }

        private unsafe ParsingError PrivateParseMinimal()
        {
            ushort index = (ushort) (this.m_Flags & (Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed));
            ushort length = (ushort) this.m_String.Length;
            string newHost = null;
            this.m_Flags &= ~(Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed | Flags.UserDrivenParsing);
            fixed (char* str2 = (((this.m_iriParsing && ((this.m_Flags & Flags.HasUnicode) != Flags.HostNotParsed)) && ((this.m_Flags & (Flags.HostNotParsed | Flags.HostUnicodeNormalized)) == Flags.HostNotParsed)) ? ((char*) this.m_originalUnicodeString) : ((char*) this.m_String)))
            {
                char* pString = str2;
                if ((length > index) && IsLWS(pString[length - 1]))
                {
                    length = (ushort) (length - 1);
                    while ((length != index) && IsLWS(pString[length = (ushort) (length - 1)]))
                    {
                    }
                    length = (ushort) (length + 1);
                }
                if ((this.m_Syntax.IsAllSet(UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.AllowEmptyHost) && this.NotAny(Flags.HostNotParsed | Flags.ImplicitFile)) && ((index + 1) < length))
                {
                    char ch;
                    ushort num3 = index;
                    while (num3 < length)
                    {
                        if (((ch = pString[num3]) != '\\') && (ch != '/'))
                        {
                            break;
                        }
                        num3 = (ushort) (num3 + 1);
                    }
                    if (this.m_Syntax.InFact(UriSyntaxFlags.FileLikeUri) || ((num3 - index) <= 3))
                    {
                        if ((num3 - index) >= 2)
                        {
                            this.m_Flags |= Flags.AuthorityFound;
                        }
                        if ((((num3 + 1) < length) && (((ch = pString[num3 + 1]) == ':') || (ch == '|'))) && IsAsciiLetter(pString[num3]))
                        {
                            if (((num3 + 2) >= length) || (((ch = pString[num3 + 2]) != '\\') && (ch != '/')))
                            {
                                if (this.m_Syntax.InFact(UriSyntaxFlags.FileLikeUri))
                                {
                                    return ParsingError.MustRootedPath;
                                }
                            }
                            else
                            {
                                this.m_Flags |= Flags.DosPath;
                                if (this.m_Syntax.InFact(UriSyntaxFlags.MustHaveAuthority))
                                {
                                    this.m_Flags |= Flags.AuthorityFound;
                                }
                                if ((num3 != index) && ((num3 - index) != 2))
                                {
                                    index = (ushort) (num3 - 1);
                                }
                                else
                                {
                                    index = num3;
                                }
                            }
                        }
                        else if (((this.m_Syntax.InFact(UriSyntaxFlags.FileLikeUri) && ((num3 - index) >= 2)) && (((num3 - index) != 3) && (num3 < length))) && ((pString[num3] != '?') && (pString[num3] != '#')))
                        {
                            this.m_Flags |= Flags.HostNotParsed | Flags.UncPath;
                            index = num3;
                        }
                    }
                }
                if ((this.m_Flags & (Flags.DosPath | Flags.UncPath)) == Flags.HostNotParsed)
                {
                    if ((index + 2) <= length)
                    {
                        char ch2 = pString[index];
                        char ch3 = pString[index + 1];
                        if (!this.m_Syntax.InFact(UriSyntaxFlags.MustHaveAuthority))
                        {
                            if (!this.m_Syntax.InFact(UriSyntaxFlags.OptionalAuthority) || (!this.InFact(Flags.AuthorityFound) && ((ch2 != '/') || (ch3 != '/'))))
                            {
                                if (this.m_Syntax.NotAny(UriSyntaxFlags.MailToLikeUri))
                                {
                                    this.m_Flags |= ((Flags) index) | (Flags.BasicHostType | Flags.IPv4HostType);
                                    return ParsingError.None;
                                }
                            }
                            else
                            {
                                this.m_Flags |= Flags.AuthorityFound;
                                index = (ushort) (index + 2);
                            }
                        }
                        else
                        {
                            if (((ch2 != '/') && (ch2 != '\\')) || ((ch3 != '/') && (ch3 != '\\')))
                            {
                                return ParsingError.BadAuthority;
                            }
                            this.m_Flags |= Flags.AuthorityFound;
                            index = (ushort) (index + 2);
                        }
                    }
                    else
                    {
                        if (this.m_Syntax.InFact(UriSyntaxFlags.MustHaveAuthority))
                        {
                            return ParsingError.BadAuthority;
                        }
                        if (this.m_Syntax.NotAny(UriSyntaxFlags.MailToLikeUri))
                        {
                            this.m_Flags |= ((Flags) index) | (Flags.BasicHostType | Flags.IPv4HostType);
                            return ParsingError.None;
                        }
                    }
                }
                if (this.InFact(Flags.DosPath))
                {
                    this.m_Flags |= ((this.m_Flags & Flags.AuthorityFound) != Flags.HostNotParsed) ? Flags.BasicHostType : (Flags.BasicHostType | Flags.IPv4HostType);
                    this.m_Flags |= (Flags) index;
                    return ParsingError.None;
                }
                ParsingError none = ParsingError.None;
                index = this.CheckAuthorityHelper(pString, index, length, ref none, ref this.m_Flags, this.m_Syntax, ref newHost);
                if (none != ParsingError.None)
                {
                    return none;
                }
                if (((index < length) && (pString[index] == '\\')) && (this.NotAny(Flags.HostNotParsed | Flags.ImplicitFile) && this.m_Syntax.NotAny(UriSyntaxFlags.AllowDOSPath)))
                {
                    return ParsingError.BadAuthorityTerminator;
                }
                this.m_Flags |= (Flags) index;
            }
            if ((s_IdnScope != null) || this.m_iriParsing)
            {
                this.PrivateParseMinimalIri(newHost, index);
            }
            return ParsingError.None;
        }

        private void PrivateParseMinimalIri(string newHost, ushort idx)
        {
            if (newHost != null)
            {
                this.m_String = newHost;
            }
            if (((!this.m_iriParsing && this.AllowIdn) && (((this.m_Flags & (Flags.HostNotParsed | Flags.IdnHost)) != Flags.HostNotParsed) || ((this.m_Flags & (Flags.HostNotParsed | Flags.UnicodeHost)) != Flags.HostNotParsed))) || ((this.m_iriParsing && ((this.m_Flags & Flags.HasUnicode) == Flags.HostNotParsed)) && (this.AllowIdn && ((this.m_Flags & (Flags.HostNotParsed | Flags.IdnHost)) != Flags.HostNotParsed))))
            {
                this.m_Flags &= ~(Flags.BackslashInPath | Flags.CannotDisplayCanonical | Flags.E_CannotDisplayCanonical | Flags.FirstSlashAbsent | Flags.ShouldBeCompressed);
                this.m_Flags |= (Flags) this.m_String.Length;
                this.m_String = this.m_String + this.m_originalUnicodeString.Substring(idx, this.m_originalUnicodeString.Length - idx);
            }
            if (this.m_iriParsing && ((this.m_Flags & Flags.HasUnicode) != Flags.HostNotParsed))
            {
                this.m_Flags |= Flags.HostNotParsed | Flags.UseOrigUncdStrOffset;
            }
        }

        private string ReCreateParts(UriComponents parts, ushort nonCanonical, UriFormat formatAs)
        {
            ushort num3;
            this.EnsureHostString(false);
            string input = ((parts & UriComponents.Host) == 0) ? string.Empty : this.m_Info.Host;
            int destinationIndex = (this.m_Info.Offset.End - this.m_Info.Offset.User) * ((formatAs == UriFormat.UriEscaped) ? 12 : 1);
            char[] destination = new char[(((input.Length + destinationIndex) + this.m_Syntax.SchemeName.Length) + 3) + 1];
            destinationIndex = 0;
            if ((parts & UriComponents.Scheme) != 0)
            {
                this.m_Syntax.SchemeName.CopyTo(0, destination, destinationIndex, this.m_Syntax.SchemeName.Length);
                destinationIndex += this.m_Syntax.SchemeName.Length;
                if (parts != UriComponents.Scheme)
                {
                    destination[destinationIndex++] = ':';
                    if (this.InFact(Flags.AuthorityFound))
                    {
                        destination[destinationIndex++] = '/';
                        destination[destinationIndex++] = '/';
                    }
                }
            }
            if (((parts & UriComponents.UserInfo) != 0) && this.InFact(Flags.HasUserInfo))
            {
                if ((nonCanonical & 2) == 0)
                {
                    UnescapeString(this.m_String, this.m_Info.Offset.User, this.m_Info.Offset.Host, destination, ref destinationIndex, 0xffff, 0xffff, 0xffff, UnescapeMode.CopyOnly, this.m_Syntax, false, false);
                }
                else
                {
                    switch (formatAs)
                    {
                        case UriFormat.UriEscaped:
                            if (!this.NotAny(Flags.HostNotParsed | Flags.UserEscaped))
                            {
                                this.InFact(Flags.E_UserNotCanonical);
                                this.m_String.CopyTo(this.m_Info.Offset.User, destination, destinationIndex, this.m_Info.Offset.Host - this.m_Info.Offset.User);
                                destinationIndex += this.m_Info.Offset.Host - this.m_Info.Offset.User;
                                break;
                            }
                            destination = EscapeString(this.m_String, this.m_Info.Offset.User, this.m_Info.Offset.Host, destination, ref destinationIndex, true, '?', '#', '%');
                            break;

                        case UriFormat.Unescaped:
                            destination = UnescapeString(this.m_String, this.m_Info.Offset.User, this.m_Info.Offset.Host, destination, ref destinationIndex, 0xffff, 0xffff, 0xffff, UnescapeMode.UnescapeAll | UnescapeMode.Unescape, this.m_Syntax, false, false);
                            break;

                        case UriFormat.SafeUnescaped:
                            destination = UnescapeString(this.m_String, this.m_Info.Offset.User, this.m_Info.Offset.Host - 1, destination, ref destinationIndex, '@', '/', '\\', this.InFact(Flags.HostNotParsed | Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape, this.m_Syntax, false, false);
                            destination[destinationIndex++] = '@';
                            break;

                        default:
                            destination = UnescapeString(this.m_String, this.m_Info.Offset.User, this.m_Info.Offset.Host, destination, ref destinationIndex, 0xffff, 0xffff, 0xffff, UnescapeMode.CopyOnly, this.m_Syntax, false, false);
                            break;
                    }
                }
                if (parts == UriComponents.UserInfo)
                {
                    destinationIndex--;
                }
            }
            if (((parts & UriComponents.Host) != 0) && (input.Length != 0))
            {
                UnescapeMode copyOnly;
                if (((formatAs != UriFormat.UriEscaped) && (this.HostType == Flags.BasicHostType)) && ((nonCanonical & 4) != 0))
                {
                    copyOnly = (formatAs == UriFormat.Unescaped) ? (UnescapeMode.UnescapeAll | UnescapeMode.Unescape) : (this.InFact(Flags.HostNotParsed | Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape);
                }
                else
                {
                    copyOnly = UnescapeMode.CopyOnly;
                }
                destination = UnescapeString(input, 0, input.Length, destination, ref destinationIndex, '/', '?', '#', copyOnly, this.m_Syntax, false, false);
                if ((((parts & UriComponents.SerializationInfoString) != 0) && (this.HostType == (Flags.HostNotParsed | Flags.IPv6HostType))) && (this.m_Info.ScopeId != null))
                {
                    this.m_Info.ScopeId.CopyTo(0, destination, destinationIndex - 1, this.m_Info.ScopeId.Length);
                    destinationIndex += this.m_Info.ScopeId.Length;
                    destination[destinationIndex - 1] = ']';
                }
            }
            if ((parts & UriComponents.Port) != 0)
            {
                if ((nonCanonical & 8) == 0)
                {
                    if (this.InFact(Flags.HostNotParsed | Flags.NotDefaultPort))
                    {
                        ushort path = this.m_Info.Offset.Path;
                        while (this.m_String[path = (ushort) (path - 1)] != ':')
                        {
                        }
                        this.m_String.CopyTo(path, destination, destinationIndex, this.m_Info.Offset.Path - path);
                        destinationIndex += this.m_Info.Offset.Path - path;
                    }
                    else if (((parts & UriComponents.StrongPort) != 0) && (this.m_Syntax.DefaultPort != -1))
                    {
                        destination[destinationIndex++] = ':';
                        input = this.m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
                        input.CopyTo(0, destination, destinationIndex, input.Length);
                        destinationIndex += input.Length;
                    }
                }
                else if (this.InFact(Flags.HostNotParsed | Flags.NotDefaultPort) || (((parts & UriComponents.StrongPort) != 0) && (this.m_Syntax.DefaultPort != -1)))
                {
                    destination[destinationIndex++] = ':';
                    input = this.m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
                    input.CopyTo(0, destination, destinationIndex, input.Length);
                    destinationIndex += input.Length;
                }
            }
            if ((parts & UriComponents.Path) != 0)
            {
                destination = this.GetCanonicalPath(destination, ref destinationIndex, formatAs);
                if (parts == UriComponents.Path)
                {
                    if ((this.InFact(Flags.AuthorityFound) && (destinationIndex != 0)) && (destination[0] == '/'))
                    {
                        num3 = 1;
                        destinationIndex--;
                    }
                    else
                    {
                        num3 = 0;
                    }
                    if (destinationIndex != 0)
                    {
                        return new string(destination, num3, destinationIndex);
                    }
                    return string.Empty;
                }
            }
            if (((parts & UriComponents.Query) != 0) && (this.m_Info.Offset.Query < this.m_Info.Offset.Fragment))
            {
                num3 = (ushort) (this.m_Info.Offset.Query + 1);
                if (parts != UriComponents.Query)
                {
                    destination[destinationIndex++] = '?';
                }
                if ((nonCanonical & 0x20) == 0)
                {
                    UnescapeString(this.m_String, num3, this.m_Info.Offset.Fragment, destination, ref destinationIndex, 0xffff, 0xffff, 0xffff, UnescapeMode.CopyOnly, this.m_Syntax, true, false);
                }
                else
                {
                    switch (formatAs)
                    {
                        case UriFormat.UriEscaped:
                            if (!this.NotAny(Flags.HostNotParsed | Flags.UserEscaped))
                            {
                                UnescapeString(this.m_String, num3, this.m_Info.Offset.Fragment, destination, ref destinationIndex, 0xffff, 0xffff, 0xffff, UnescapeMode.CopyOnly, this.m_Syntax, true, false);
                                break;
                            }
                            destination = EscapeString(this.m_String, num3, this.m_Info.Offset.Fragment, destination, ref destinationIndex, true, '#', 0xffff, '%');
                            break;

                        case UriFormat.Unescaped:
                            destination = UnescapeString(this.m_String, num3, this.m_Info.Offset.Fragment, destination, ref destinationIndex, '#', 0xffff, 0xffff, UnescapeMode.UnescapeAll | UnescapeMode.Unescape, this.m_Syntax, true, false);
                            break;

                        case ((UriFormat) 0x7fff):
                            destination = UnescapeString(this.m_String, num3, this.m_Info.Offset.Fragment, destination, ref destinationIndex, '#', 0xffff, 0xffff, (this.InFact(Flags.HostNotParsed | Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape) | UnescapeMode.V1ToStringFlag, this.m_Syntax, true, false);
                            break;

                        default:
                            destination = UnescapeString(this.m_String, num3, this.m_Info.Offset.Fragment, destination, ref destinationIndex, '#', 0xffff, 0xffff, this.InFact(Flags.HostNotParsed | Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape, this.m_Syntax, true, false);
                            break;
                    }
                }
            }
            if (((parts & UriComponents.Fragment) != 0) && (this.m_Info.Offset.Fragment < this.m_Info.Offset.End))
            {
                num3 = (ushort) (this.m_Info.Offset.Fragment + 1);
                if (parts != UriComponents.Fragment)
                {
                    destination[destinationIndex++] = '#';
                }
                if ((nonCanonical & 0x40) == 0)
                {
                    UnescapeString(this.m_String, num3, this.m_Info.Offset.End, destination, ref destinationIndex, 0xffff, 0xffff, 0xffff, UnescapeMode.CopyOnly, this.m_Syntax, false, false);
                }
                else
                {
                    switch (formatAs)
                    {
                        case UriFormat.UriEscaped:
                            if (!this.NotAny(Flags.HostNotParsed | Flags.UserEscaped))
                            {
                                UnescapeString(this.m_String, num3, this.m_Info.Offset.End, destination, ref destinationIndex, 0xffff, 0xffff, 0xffff, UnescapeMode.CopyOnly, this.m_Syntax, false, false);
                                break;
                            }
                            destination = EscapeString(this.m_String, num3, this.m_Info.Offset.End, destination, ref destinationIndex, true, '#', 0xffff, '%');
                            break;

                        case UriFormat.Unescaped:
                            destination = UnescapeString(this.m_String, num3, this.m_Info.Offset.End, destination, ref destinationIndex, '#', 0xffff, 0xffff, UnescapeMode.UnescapeAll | UnescapeMode.Unescape, this.m_Syntax, false, false);
                            break;

                        case ((UriFormat) 0x7fff):
                            destination = UnescapeString(this.m_String, num3, this.m_Info.Offset.End, destination, ref destinationIndex, '#', 0xffff, 0xffff, (this.InFact(Flags.HostNotParsed | Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape) | UnescapeMode.V1ToStringFlag, this.m_Syntax, false, false);
                            break;

                        default:
                            destination = UnescapeString(this.m_String, num3, this.m_Info.Offset.End, destination, ref destinationIndex, '#', 0xffff, 0xffff, this.InFact(Flags.HostNotParsed | Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape, this.m_Syntax, false, false);
                            break;
                    }
                }
            }
            return new string(destination, 0, destinationIndex);
        }

        internal static Uri ResolveHelper(Uri baseUri, Uri relativeUri, ref string newUriString, ref bool userEscaped, out UriFormatException e)
        {
            e = null;
            string relativeStr = string.Empty;
            if (relativeUri != null)
            {
                if (relativeUri.IsAbsoluteUri)
                {
                    return relativeUri;
                }
                relativeStr = relativeUri.OriginalString;
                userEscaped = relativeUri.UserEscaped;
            }
            else
            {
                relativeStr = string.Empty;
            }
            if ((relativeStr.Length > 0) && (IsLWS(relativeStr[0]) || IsLWS(relativeStr[relativeStr.Length - 1])))
            {
                relativeStr = relativeStr.Trim(_WSchars);
            }
            if (relativeStr.Length == 0)
            {
                newUriString = baseUri.GetParts(UriComponents.AbsoluteUri, baseUri.UserEscaped ? UriFormat.UriEscaped : UriFormat.SafeUnescaped);
                return null;
            }
            if (((relativeStr[0] == '#') && !baseUri.IsImplicitFile) && baseUri.Syntax.InFact(UriSyntaxFlags.MayHaveFragment))
            {
                newUriString = baseUri.GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.UriEscaped) + relativeStr;
                return null;
            }
            if (((relativeStr[0] == '?') && !baseUri.IsImplicitFile) && baseUri.Syntax.InFact(UriSyntaxFlags.MayHaveQuery))
            {
                newUriString = baseUri.GetParts(UriComponents.Path | UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.UriEscaped) + relativeStr;
                return null;
            }
            if (((relativeStr.Length >= 3) && ((relativeStr[1] == ':') || (relativeStr[1] == '|'))) && (IsAsciiLetter(relativeStr[0]) && ((relativeStr[2] == '\\') || (relativeStr[2] == '/'))))
            {
                if (baseUri.IsImplicitFile)
                {
                    newUriString = relativeStr;
                    return null;
                }
                if (baseUri.Syntax.InFact(UriSyntaxFlags.AllowDOSPath))
                {
                    string str2;
                    if (baseUri.InFact(Flags.AuthorityFound))
                    {
                        str2 = baseUri.Syntax.InFact(UriSyntaxFlags.PathIsRooted) ? ":///" : "://";
                    }
                    else
                    {
                        str2 = baseUri.Syntax.InFact(UriSyntaxFlags.PathIsRooted) ? ":/" : ":";
                    }
                    newUriString = baseUri.Scheme + str2 + relativeStr;
                    return null;
                }
            }
            ParsingError err = GetCombinedString(baseUri, relativeStr, userEscaped, ref newUriString);
            if (err != ParsingError.None)
            {
                e = GetException(err);
                return null;
            }
            if (newUriString == baseUri.m_String)
            {
                return baseUri;
            }
            return null;
        }

        private static void SetEscapedDotSlashSettings(UriSectionInternal uriSection, string scheme)
        {
            SchemeSettingInternal schemeSetting = uriSection.GetSchemeSetting(scheme);
            if ((schemeSetting != null) && (schemeSetting.Options == GenericUriParserOptions.DontUnescapePathDotsAndSlashes))
            {
                UriParser.GetSyntax(scheme).SetUpdatableFlags(UriSyntaxFlags.None);
            }
        }

        private void SetUserDrivenParsing()
        {
            this.m_Flags = (Flags.HostNotParsed | Flags.UserDrivenParsing) | (this.m_Flags & (Flags.HostNotParsed | Flags.UserEscaped));
        }

        private static bool StaticInFact(Flags allFlags, Flags checkFlags)
        {
            return ((allFlags & checkFlags) != Flags.HostNotParsed);
        }

        private static bool StaticIsFile(UriParser syntax)
        {
            return syntax.InFact(UriSyntaxFlags.FileLikeUri);
        }

        private static bool StaticNotAny(Flags allFlags, Flags checkFlags)
        {
            return ((allFlags & checkFlags) == Flags.HostNotParsed);
        }

        internal static unsafe string StripBidiControlCharacter(char* strToClean, int start, int length)
        {
            if (length <= 0)
            {
                return "";
            }
            char[] chArray = new char[length];
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                char ch = strToClean[start + i];
                if (((ch < '‎') || (ch > '‮')) || !IsBidiControlCharacter(ch))
                {
                    chArray[num++] = ch;
                }
            }
            return new string(chArray, 0, num);
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        private static unsafe bool TestForSubPath(char* pMe, ushort meLength, char* pShe, ushort sheLength, bool ignoreCase)
        {
            char ch;
            ushort index = 0;
            bool flag = true;
            while ((index < meLength) && (index < sheLength))
            {
                ch = pMe[index];
                char c = pShe[index];
                if ((ch == '?') || (ch == '#'))
                {
                    return true;
                }
                if (ch == '/')
                {
                    if (c != '/')
                    {
                        return false;
                    }
                    if (!flag)
                    {
                        return false;
                    }
                    flag = true;
                }
                else
                {
                    switch (c)
                    {
                        case '?':
                        case '#':
                            goto Label_0096;
                    }
                    if (!ignoreCase)
                    {
                        if (ch != c)
                        {
                            flag = false;
                        }
                    }
                    else if (char.ToLower(ch, CultureInfo.InvariantCulture) != char.ToLower(c, CultureInfo.InvariantCulture))
                    {
                        flag = false;
                    }
                }
                index = (ushort) (index + 1);
            }
        Label_0096:
            while (index < meLength)
            {
                if (((ch = pMe[index]) == '?') || (ch == '#'))
                {
                    return true;
                }
                if (ch == '/')
                {
                    return false;
                }
                index = (ushort) (index + 1);
            }
            return true;
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override string ToString()
        {
            if (this.m_Syntax == null)
            {
                if (this.m_iriParsing && this.InFact(Flags.HasUnicode))
                {
                    return this.m_String;
                }
                return this.OriginalString;
            }
            this.EnsureUriInfo();
            if (this.m_Info.String == null)
            {
                if (this.Syntax.IsSimple)
                {
                    this.m_Info.String = this.GetComponentsHelper(UriComponents.AbsoluteUri, (UriFormat) 0x7fff);
                }
                else
                {
                    this.m_Info.String = this.GetParts(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
                }
            }
            return this.m_Info.String;
        }

        public static bool TryCreate(string uriString, UriKind uriKind, out Uri result)
        {
            if (uriString == null)
            {
                result = null;
                return false;
            }
            UriFormatException e = null;
            result = CreateHelper(uriString, false, uriKind, ref e);
            return ((e == null) && (result != null));
        }

        public static bool TryCreate(Uri baseUri, string relativeUri, out Uri result)
        {
            Uri uri;
            if (TryCreate(relativeUri, UriKind.RelativeOrAbsolute, out uri))
            {
                if (!uri.IsAbsoluteUri)
                {
                    return TryCreate(baseUri, uri, out result);
                }
                result = uri;
                return true;
            }
            result = null;
            return false;
        }

        public static bool TryCreate(Uri baseUri, Uri relativeUri, out Uri result)
        {
            UriFormatException exception;
            bool userEscaped;
            result = null;
            if ((baseUri == null) || (relativeUri == null))
            {
                return false;
            }
            if (baseUri.IsNotAbsoluteUri)
            {
                return false;
            }
            string newUriString = null;
            if (baseUri.Syntax.IsSimple)
            {
                userEscaped = relativeUri.UserEscaped;
                result = ResolveHelper(baseUri, relativeUri, ref newUriString, ref userEscaped, out exception);
            }
            else
            {
                userEscaped = false;
                newUriString = baseUri.Syntax.InternalResolve(baseUri, relativeUri, out exception);
            }
            if (exception != null)
            {
                return false;
            }
            if (result == null)
            {
                result = CreateHelper(newUriString, userEscaped, UriKind.Absolute, ref exception);
            }
            return (((exception == null) && (result != null)) && result.IsAbsoluteUri);
        }

        [Obsolete("The method has been deprecated. Please use GetComponents() or static UnescapeDataString() to unescape a Uri component or a string. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual string Unescape(string path)
        {
            char[] dest = new char[path.Length];
            int length = 0;
            return new string(UnescapeString(path, 0, path.Length, dest, ref length, 0xffff, 0xffff, 0xffff, UnescapeMode.UnescapeAll | UnescapeMode.Unescape, null, false, true), 0, length);
        }

        public static unsafe string UnescapeDataString(string stringToUnescape)
        {
            if (stringToUnescape == null)
            {
                throw new ArgumentNullException("stringToUnescape");
            }
            if (stringToUnescape.Length == 0)
            {
                return string.Empty;
            }
            fixed (char* str2 = ((char*) stringToUnescape))
            {
                char* chPtr = str2;
                int index = 0;
                while (index < stringToUnescape.Length)
                {
                    if (chPtr[index] == '%')
                    {
                        break;
                    }
                    index++;
                }
                if (index == stringToUnescape.Length)
                {
                    return stringToUnescape;
                }
                index = 0;
                char[] dest = new char[stringToUnescape.Length];
                return new string(UnescapeString(stringToUnescape, 0, stringToUnescape.Length, dest, ref index, 0xffff, 0xffff, 0xffff, UnescapeMode.UnescapeAllOrThrow | UnescapeMode.Unescape, null, false, true), 0, index);
            }
        }

        private static unsafe void UnescapeOnly(char* pch, int start, ref int end, char ch1, char ch2, char ch3)
        {
            if ((end - start) < 3)
            {
                return;
            }
            char* chPtr = (pch + end) - 2;
            pch += start;
            char* chPtr2 = null;
        Label_001E:
            if (pch < chPtr)
            {
                pch++;
                if (pch[0] != '%')
                {
                    goto Label_001E;
                }
                pch++;
                pch++;
                char ch = EscapedAscii(pch[0], pch[0]);
                if (((ch != ch1) && (ch != ch2)) && (ch != ch3))
                {
                    goto Label_001E;
                }
                chPtr2 = pch - 2;
                *(chPtr2 - 1) = ch;
                while (pch < chPtr)
                {
                    chPtr2++;
                    pch++;
                    if ((chPtr2[0] = pch[0]) == '%')
                    {
                        char ch5;
                        char ch6;
                        chPtr2++;
                        pch++;
                        chPtr2[0] = ch5 = pch[0];
                        chPtr2++;
                        pch++;
                        chPtr2[0] = ch6 = pch[0];
                        ch = EscapedAscii(ch5, ch6);
                        if (((ch == ch1) || (ch == ch2)) || (ch == ch3))
                        {
                            chPtr2 -= 2;
                            *(chPtr2 - 1) = ch;
                        }
                    }
                }
            }
            chPtr += 2;
            if (chPtr2 != null)
            {
                if (pch == chPtr)
                {
                    end -= (int) ((long) ((pch - chPtr2) / 2));
                }
                else
                {
                    chPtr2++;
                    pch++;
                    chPtr2[0] = pch[0];
                    if (pch == chPtr)
                    {
                        end -= (int) ((long) ((pch - chPtr2) / 2));
                    }
                    else
                    {
                        chPtr2++;
                        pch++;
                        chPtr2[0] = pch[0];
                        end -= (int) ((long) ((pch - chPtr2) / 2));
                    }
                }
            }
        }

        private static unsafe char[] UnescapeString(string input, int start, int end, char[] dest, ref int destPosition, char rsvd1, char rsvd2, char rsvd3, UnescapeMode unescapeMode, UriParser syntax, bool isQuery, bool readOnlyConfig)
        {
            fixed (char* str = ((char*) input))
            {
                char* pStr = str;
                return UnescapeString(pStr, start, end, dest, ref destPosition, rsvd1, rsvd2, rsvd3, unescapeMode, syntax, isQuery, readOnlyConfig);
            }
        }

        private static unsafe char[] UnescapeString(char* pStr, int start, int end, char[] dest, ref int destPosition, char rsvd1, char rsvd2, char rsvd3, UnescapeMode unescapeMode, UriParser syntax, bool isQuery, bool readOnlyConfig)
        {
            byte[] bytes = null;
            byte num = 0;
            bool flag = false;
            int index = start;
            bool iriParsing = (s_IriParsing && (readOnlyConfig || (!readOnlyConfig && IriParsingStatic(syntax)))) && ((unescapeMode & UnescapeMode.EscapeUnescape) == UnescapeMode.EscapeUnescape);
        Label_002E:;
            try
            {
                fixed (char* chRef = dest)
                {
                    char ch;
                    if ((unescapeMode & UnescapeMode.EscapeUnescape) == UnescapeMode.CopyOnly)
                    {
                        while (start < end)
                        {
                            chRef[destPosition++] = pStr[start++];
                        }
                        return dest;
                    }
                Label_007F:
                    ch = '\0';
                    while (index < end)
                    {
                        ch = pStr[index];
                        if (ch == '%')
                        {
                            if ((unescapeMode & UnescapeMode.Unescape) == UnescapeMode.CopyOnly)
                            {
                                flag = true;
                            }
                            else
                            {
                                if ((index + 2) < end)
                                {
                                    ch = EscapedAscii(pStr[index + 1], pStr[index + 2]);
                                    if (unescapeMode >= UnescapeMode.UnescapeAll)
                                    {
                                        if (ch == 0xffff)
                                        {
                                            if (unescapeMode >= UnescapeMode.UnescapeAllOrThrow)
                                            {
                                                throw new UriFormatException(System.SR.GetString("net_uri_BadString"));
                                            }
                                            goto Label_01E8;
                                        }
                                        break;
                                    }
                                    if (ch == 0xffff)
                                    {
                                        if ((unescapeMode & UnescapeMode.Escape) == UnescapeMode.CopyOnly)
                                        {
                                            goto Label_01E8;
                                        }
                                        flag = true;
                                        break;
                                    }
                                    if (ch == '%')
                                    {
                                        index += 2;
                                    }
                                    else if (((ch == rsvd1) || (ch == rsvd2)) || (ch == rsvd3))
                                    {
                                        index += 2;
                                    }
                                    else if (((unescapeMode & UnescapeMode.V1ToStringFlag) == UnescapeMode.CopyOnly) && IsNotSafeForUnescape(ch))
                                    {
                                        index += 2;
                                    }
                                    else
                                    {
                                        if (!iriParsing || (((ch > '\x009f') || !IsNotSafeForUnescape(ch)) && ((ch <= '\x009f') || CheckIriUnicodeRange(ch, isQuery))))
                                        {
                                            break;
                                        }
                                        index += 2;
                                    }
                                    goto Label_01E8;
                                }
                                if (unescapeMode >= UnescapeMode.UnescapeAll)
                                {
                                    if (unescapeMode >= UnescapeMode.UnescapeAllOrThrow)
                                    {
                                        throw new UriFormatException(System.SR.GetString("net_uri_BadString"));
                                    }
                                    goto Label_01E8;
                                }
                                flag = true;
                            }
                            break;
                        }
                        if (((unescapeMode & (UnescapeMode.UnescapeAll | UnescapeMode.Unescape)) != (UnescapeMode.UnescapeAll | UnescapeMode.Unescape)) && ((unescapeMode & UnescapeMode.Escape) != UnescapeMode.CopyOnly))
                        {
                            if (((ch == rsvd1) || (ch == rsvd2)) || (ch == rsvd3))
                            {
                                flag = true;
                                break;
                            }
                            if (((unescapeMode & UnescapeMode.V1ToStringFlag) == UnescapeMode.CopyOnly) && ((ch <= '\x001f') || ((ch >= '\x007f') && (ch <= '\x009f'))))
                            {
                                flag = true;
                                break;
                            }
                        }
                    Label_01E8:
                        index++;
                    }
                    while (start < index)
                    {
                        chRef[destPosition++] = pStr[start++];
                    }
                    if (index != end)
                    {
                        if (flag)
                        {
                            if (num == 0)
                            {
                                num = 30;
                                char[] chArray = new char[dest.Length + (num * 3)];
                                fixed (char* chRef2 = chArray)
                                {
                                    for (int i = 0; i < destPosition; i++)
                                    {
                                        chRef2[i] = chRef[i];
                                    }
                                }
                                dest = chArray;
                                goto Label_002E;
                            }
                            num = (byte) (num - 1);
                            EscapeAsciiChar(pStr[index], dest, ref destPosition);
                            flag = false;
                            start = ++index;
                            goto Label_007F;
                        }
                        if (ch <= '\x007f')
                        {
                            dest[destPosition++] = ch;
                            index += 3;
                            start = index;
                            goto Label_007F;
                        }
                        int byteCount = 1;
                        if (bytes == null)
                        {
                            bytes = new byte[end - index];
                        }
                        bytes[0] = (byte) ch;
                        index += 3;
                        while (index < end)
                        {
                            if (((ch = pStr[index]) != '%') || ((index + 2) >= end))
                            {
                                break;
                            }
                            ch = EscapedAscii(pStr[index + 1], pStr[index + 2]);
                            if ((ch == 0xffff) || (ch < '\x0080'))
                            {
                                break;
                            }
                            bytes[byteCount++] = (byte) ch;
                            index += 3;
                        }
                        Encoding encoding = Encoding.GetEncoding("utf-8", new EncoderReplacementFallback(""), new DecoderReplacementFallback(""));
                        char[] chars = new char[bytes.Length];
                        int charCount = encoding.GetChars(bytes, 0, byteCount, chars, 0);
                        if (charCount != 0)
                        {
                            start = index;
                            MatchUTF8Sequence(chRef, dest, ref destPosition, chars, charCount, bytes, isQuery, iriParsing);
                        }
                        else
                        {
                            if (unescapeMode >= UnescapeMode.UnescapeAllOrThrow)
                            {
                                throw new UriFormatException(System.SR.GetString("net_uri_BadString"));
                            }
                            index = start + 3;
                            start = index;
                            dest[destPosition++] = (char) bytes[0];
                        }
                    }
                    if (index != end)
                    {
                        goto Label_007F;
                    }
                    return dest;
                }
            }
            finally
            {
                chRef = null;
            }
            return dest;
        }

        public string AbsolutePath
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                string privateAbsolutePath = this.PrivateAbsolutePath;
                if (this.IsDosPath && (privateAbsolutePath[0] == '/'))
                {
                    privateAbsolutePath = privateAbsolutePath.Substring(1);
                }
                return privateAbsolutePath;
            }
        }

        public string AbsoluteUri
        {
            get
            {
                if (this.m_Syntax == null)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                UriInfo info = this.EnsureUriInfo();
                if (info.MoreInfo == null)
                {
                    info.MoreInfo = new MoreInfo();
                }
                string absoluteUri = info.MoreInfo.AbsoluteUri;
                if (absoluteUri == null)
                {
                    absoluteUri = this.GetParts(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
                    info.MoreInfo.AbsoluteUri = absoluteUri;
                }
                return absoluteUri;
            }
        }

        private bool AllowIdn
        {
            get
            {
                if ((this.m_Syntax == null) || ((this.m_Syntax.Flags & UriSyntaxFlags.AllowIdn) == UriSyntaxFlags.None))
                {
                    return false;
                }
                return ((s_IdnScope == 2) || ((s_IdnScope == 1) && this.NotAny(Flags.HostNotParsed | Flags.IntranetUri)));
            }
        }

        public string Authority
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                return this.GetParts(UriComponents.Port | UriComponents.Host, UriFormat.UriEscaped);
            }
        }

        public string DnsSafeHost
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                if (this.AllowIdn && (((this.m_Flags & (Flags.HostNotParsed | Flags.IdnHost)) != Flags.HostNotParsed) || ((this.m_Flags & (Flags.HostNotParsed | Flags.UnicodeHost)) != Flags.HostNotParsed)))
                {
                    this.EnsureUriInfo();
                    return this.m_Info.DnsSafeHost;
                }
                this.EnsureHostString(false);
                if (!string.IsNullOrEmpty(this.m_Info.DnsSafeHost))
                {
                    return this.m_Info.DnsSafeHost;
                }
                if (this.m_Info.Host.Length == 0)
                {
                    return string.Empty;
                }
                string host = this.m_Info.Host;
                if (this.HostType == (Flags.HostNotParsed | Flags.IPv6HostType))
                {
                    host = host.Substring(1, host.Length - 2);
                    if (this.m_Info.ScopeId != null)
                    {
                        host = host + this.m_Info.ScopeId;
                    }
                }
                else if (this.HostType == Flags.BasicHostType)
                {
                    if (this.InFact(Flags.E_HostNotCanonical | Flags.HostNotCanonical))
                    {
                        char[] dest = new char[host.Length];
                        int destPosition = 0;
                        UnescapeString(host, 0, host.Length, dest, ref destPosition, 0xffff, 0xffff, 0xffff, UnescapeMode.UnescapeAll | UnescapeMode.Unescape, this.m_Syntax, false, false);
                        host = new string(dest, 0, destPosition);
                    }
                    fixed (char* str2 = ((char*) host))
                    {
                        char* name = str2;
                        int length = host.Length;
                        bool notCanonical = false;
                        if (!DomainNameHelper.IsValidByIri(name, 0, ref length, ref notCanonical, this.IsImplicitFile))
                        {
                            throw new InvalidOperationException(System.SR.GetString("net_uri_GenericAuthorityNotDnsSafe", new object[] { host }));
                        }
                    }
                }
                this.m_Info.DnsSafeHost = host;
                return host;
            }
        }

        public string Fragment
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                UriInfo info = this.EnsureUriInfo();
                if (info.MoreInfo == null)
                {
                    info.MoreInfo = new MoreInfo();
                }
                string fragment = info.MoreInfo.Fragment;
                if (fragment == null)
                {
                    fragment = this.GetParts(UriComponents.KeepDelimiter | UriComponents.Fragment, UriFormat.UriEscaped);
                    info.MoreInfo.Fragment = fragment;
                }
                return fragment;
            }
        }

        internal bool HasAuthority
        {
            get
            {
                return this.InFact(Flags.AuthorityFound);
            }
        }

        public string Host
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                return this.GetParts(UriComponents.Host, UriFormat.UriEscaped);
            }
        }

        public UriHostNameType HostNameType
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                if (this.m_Syntax.IsSimple)
                {
                    this.EnsureUriInfo();
                }
                else
                {
                    this.EnsureHostString(false);
                }
                Flags hostType = this.HostType;
                if (hostType <= Flags.DnsHostType)
                {
                    switch (hostType)
                    {
                        case (Flags.HostNotParsed | Flags.IPv6HostType):
                            return UriHostNameType.IPv6;

                        case (Flags.HostNotParsed | Flags.IPv4HostType):
                            return UriHostNameType.IPv4;

                        case Flags.DnsHostType:
                            return UriHostNameType.Dns;
                    }
                }
                else
                {
                    switch (hostType)
                    {
                        case (Flags.HostNotParsed | Flags.UncHostType):
                            return UriHostNameType.Basic;

                        case Flags.BasicHostType:
                            return UriHostNameType.Basic;

                        case (Flags.BasicHostType | Flags.IPv4HostType):
                            return UriHostNameType.Unknown;
                    }
                }
                return UriHostNameType.Unknown;
            }
        }

        private Flags HostType
        {
            get
            {
                return (this.m_Flags & (Flags.BasicHostType | Flags.IPv4HostType));
            }
        }

        private static object InitializeLock
        {
            get
            {
                if (s_initLock == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_initLock, obj2, null);
                }
                return s_initLock;
            }
        }

        public bool IsAbsoluteUri
        {
            get
            {
                return (this.m_Syntax != null);
            }
        }

        public bool IsDefaultPort
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                if (this.m_Syntax.IsSimple)
                {
                    this.EnsureUriInfo();
                }
                else
                {
                    this.EnsureHostString(false);
                }
                return this.NotAny(Flags.HostNotParsed | Flags.NotDefaultPort);
            }
        }

        private bool IsDosPath
        {
            get
            {
                return ((this.m_Flags & Flags.DosPath) != Flags.HostNotParsed);
            }
        }

        public bool IsFile
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                return (this.m_Syntax.SchemeName == UriSchemeFile);
            }
        }

        private bool IsImplicitFile
        {
            get
            {
                return ((this.m_Flags & (Flags.HostNotParsed | Flags.ImplicitFile)) != Flags.HostNotParsed);
            }
        }

        public bool IsLoopback
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                this.EnsureHostString(false);
                return this.InFact(Flags.HostNotParsed | Flags.LoopbackHost);
            }
        }

        private bool IsNotAbsoluteUri
        {
            get
            {
                return (this.m_Syntax == null);
            }
        }

        public bool IsUnc
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                return this.IsUncPath;
            }
        }

        private bool IsUncOrDosPath
        {
            get
            {
                return ((this.m_Flags & (Flags.DosPath | Flags.UncPath)) != Flags.HostNotParsed);
            }
        }

        private bool IsUncPath
        {
            get
            {
                return ((this.m_Flags & (Flags.HostNotParsed | Flags.UncPath)) != Flags.HostNotParsed);
            }
        }

        public string LocalPath
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                return this.GetLocalPath();
            }
        }

        public string OriginalString
        {
            get
            {
                if (!this.OriginalStringSwitched)
                {
                    return this.m_String;
                }
                return this.m_originalUnicodeString;
            }
        }

        private bool OriginalStringSwitched
        {
            get
            {
                if (!this.m_iriParsing || !this.InFact(Flags.HasUnicode))
                {
                    if (!this.AllowIdn)
                    {
                        return false;
                    }
                    if (!this.InFact(Flags.HostNotParsed | Flags.IdnHost))
                    {
                        return this.InFact(Flags.HostNotParsed | Flags.UnicodeHost);
                    }
                }
                return true;
            }
        }

        public string PathAndQuery
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                string parts = this.GetParts(UriComponents.PathAndQuery, UriFormat.UriEscaped);
                if (this.IsDosPath && (parts[0] == '/'))
                {
                    parts = parts.Substring(1);
                }
                return parts;
            }
        }

        public int Port
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                if (this.m_Syntax.IsSimple)
                {
                    this.EnsureUriInfo();
                }
                else
                {
                    this.EnsureHostString(false);
                }
                if (this.InFact(Flags.HostNotParsed | Flags.NotDefaultPort))
                {
                    return this.m_Info.Offset.PortValue;
                }
                return this.m_Syntax.DefaultPort;
            }
        }

        private string PrivateAbsolutePath
        {
            get
            {
                UriInfo info = this.EnsureUriInfo();
                if (info.MoreInfo == null)
                {
                    info.MoreInfo = new MoreInfo();
                }
                string path = info.MoreInfo.Path;
                if (path == null)
                {
                    path = this.GetParts(UriComponents.KeepDelimiter | UriComponents.Path, UriFormat.UriEscaped);
                    info.MoreInfo.Path = path;
                }
                return path;
            }
        }

        public string Query
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                UriInfo info = this.EnsureUriInfo();
                if (info.MoreInfo == null)
                {
                    info.MoreInfo = new MoreInfo();
                }
                string query = info.MoreInfo.Query;
                if (query == null)
                {
                    query = this.GetParts(UriComponents.KeepDelimiter | UriComponents.Query, UriFormat.UriEscaped);
                    info.MoreInfo.Query = query;
                }
                return query;
            }
        }

        public string Scheme
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                return this.m_Syntax.SchemeName;
            }
        }

        private ushort SecuredPathIndex
        {
            get
            {
                if (this.IsDosPath)
                {
                    char ch = this.m_String[this.m_Info.Offset.Path];
                    return (((ch == '/') || (ch == '\\')) ? ((ushort) 3) : ((ushort) 2));
                }
                return 0;
            }
        }

        public string[] Segments
        {
            get
            {
                int index;
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                string[] strArray = null;
                if (strArray != null)
                {
                    return strArray;
                }
                string privateAbsolutePath = this.PrivateAbsolutePath;
                if (privateAbsolutePath.Length == 0)
                {
                    return new string[0];
                }
                ArrayList list = new ArrayList();
                for (int i = 0; i < privateAbsolutePath.Length; i = index + 1)
                {
                    index = privateAbsolutePath.IndexOf('/', i);
                    if (index == -1)
                    {
                        index = privateAbsolutePath.Length - 1;
                    }
                    list.Add(privateAbsolutePath.Substring(i, (index - i) + 1));
                }
                return (string[]) list.ToArray(typeof(string));
            }
        }

        private UriParser Syntax
        {
            get
            {
                return this.m_Syntax;
            }
        }

        internal bool UserDrivenParsing
        {
            get
            {
                return ((this.m_Flags & (Flags.HostNotParsed | Flags.UserDrivenParsing)) != Flags.HostNotParsed);
            }
        }

        public bool UserEscaped
        {
            get
            {
                return this.InFact(Flags.HostNotParsed | Flags.UserEscaped);
            }
        }

        public string UserInfo
        {
            get
            {
                if (this.IsNotAbsoluteUri)
                {
                    throw new InvalidOperationException(System.SR.GetString("net_uri_NotAbsolute"));
                }
                return this.GetParts(UriComponents.UserInfo, UriFormat.UriEscaped);
            }
        }

        [Flags]
        private enum Check
        {
            BackslashInPath = 0x10,
            DisplayCanonical = 2,
            DotSlashAttn = 4,
            DotSlashEscaped = 0x80,
            EscapedCanonical = 1,
            FoundNonAscii = 8,
            None = 0,
            NotIriCanonical = 0x40,
            ReservedFound = 0x20
        }

        [Flags]
        private enum Flags : ulong
        {
            AllUriInfoSet = 0x80000000L,
            AuthorityFound = 0x100000L,
            BackslashInPath = 0x8000L,
            BasicHostType = 0x50000L,
            CannotDisplayCanonical = 0x7fL,
            CanonicalDnsHost = 0x2000000L,
            DnsHostType = 0x30000L,
            DosPath = 0x8000000L,
            E_CannotDisplayCanonical = 0x1f80L,
            E_FragmentNotCanonical = 0x1000L,
            E_HostNotCanonical = 0x100L,
            E_PathNotCanonical = 0x400L,
            E_PortNotCanonical = 0x200L,
            E_QueryNotCanonical = 0x800L,
            E_UserNotCanonical = 0x80L,
            ErrorOrParsingRecursion = 0x4000000L,
            FirstSlashAbsent = 0x4000L,
            FragmentIriCanonical = 0x40000000000L,
            FragmentNotCanonical = 0x40L,
            HasUnicode = 0x200000000L,
            HasUserInfo = 0x200000L,
            HostNotCanonical = 4L,
            HostNotParsed = 0L,
            HostTypeMask = 0x70000L,
            HostUnicodeNormalized = 0x400000000L,
            IdnHost = 0x100000000L,
            ImplicitFile = 0x20000000L,
            IndexMask = 0xffffL,
            IntranetUri = 0x2000000000L,
            IPv4HostType = 0x20000L,
            IPv6HostType = 0x10000L,
            IriCanonical = 0x78000000000L,
            LoopbackHost = 0x400000L,
            MinimalUriInfoSet = 0x40000000L,
            NotDefaultPort = 0x800000L,
            PathIriCanonical = 0x10000000000L,
            PathNotCanonical = 0x10L,
            PortNotCanonical = 8L,
            QueryIriCanonical = 0x20000000000L,
            QueryNotCanonical = 0x20L,
            RestUnicodeNormalized = 0x800000000L,
            SchemeNotCanonical = 1L,
            ShouldBeCompressed = 0x2000L,
            UncHostType = 0x40000L,
            UncPath = 0x10000000L,
            UnicodeHost = 0x1000000000L,
            UnknownHostType = 0x70000L,
            UnusedHostType = 0x60000L,
            UseOrigUncdStrOffset = 0x4000000000L,
            UserDrivenParsing = 0x1000000L,
            UserEscaped = 0x80000L,
            UserIriCanonical = 0x8000000000L,
            UserNotCanonical = 2L,
            Zero = 0L
        }

        private class MoreInfo
        {
            public string AbsoluteUri;
            public string Fragment;
            public int Hash;
            public string Path;
            public string Query;
            public string RemoteUrl;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        private struct Offset
        {
            public ushort Scheme;
            public ushort User;
            public ushort Host;
            public ushort PortValue;
            public ushort Path;
            public ushort Query;
            public ushort Fragment;
            public ushort End;
        }

        private enum ParsingError
        {
            BadAuthority = 3,
            BadAuthorityTerminator = 11,
            BadFormat = 1,
            BadHostName = 8,
            BadPort = 10,
            BadScheme = 2,
            CannotCreateRelative = 12,
            EmptyUriString = 4,
            LastRelativeUriOkErrIndex = 4,
            MustRootedPath = 7,
            None = 0,
            NonEmptyHost = 9,
            SchemeLimit = 5,
            SizeLimit = 6
        }

        [Flags]
        private enum UnescapeMode
        {
            CopyOnly = 0,
            Escape = 1,
            EscapeUnescape = 3,
            Unescape = 2,
            UnescapeAll = 8,
            UnescapeAllOrThrow = 0x18,
            V1ToStringFlag = 4
        }

        private class UriInfo
        {
            public string DnsSafeHost;
            public string Host;
            public System.Uri.MoreInfo MoreInfo;
            public System.Uri.Offset Offset;
            public string ScopeId;
            public string String;
        }
    }
}

