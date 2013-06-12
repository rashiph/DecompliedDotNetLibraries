namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class Cookie
    {
        internal const string CommentAttributeName = "Comment";
        internal const string CommentUrlAttributeName = "CommentURL";
        internal const string DiscardAttributeName = "Discard";
        internal const string DomainAttributeName = "Domain";
        internal const string EqualsLiteral = "=";
        internal const string ExpiresAttributeName = "Expires";
        internal const string HttpOnlyAttributeName = "HttpOnly";
        internal bool IsQuotedDomain;
        internal bool IsQuotedVersion;
        private string m_comment;
        private Uri m_commentUri;
        private CookieVariant m_cookieVariant;
        private bool m_discard;
        private string m_domain;
        private bool m_domain_implicit;
        private string m_domainKey;
        private DateTime m_expires;
        [OptionalField]
        private bool m_httpOnly;
        private string m_name;
        private string m_path;
        private bool m_path_implicit;
        private string m_port;
        private bool m_port_implicit;
        private int[] m_port_list;
        private bool m_secure;
        private DateTime m_timeStamp;
        private string m_value;
        private int m_version;
        internal const string MaxAgeAttributeName = "Max-Age";
        internal const int MaxSupportedVersion = 1;
        internal const string PathAttributeName = "Path";
        internal const string PortAttributeName = "Port";
        internal static readonly char[] PortSplitDelimiters = new char[] { ' ', ',', '"' };
        internal const string QuotesLiteral = "\"";
        internal static readonly char[] Reserved2Name = new char[] { ' ', '\t', '\r', '\n', '=', ';', ',' };
        internal static readonly char[] Reserved2Value = new char[] { ';', ',' };
        internal const string SecureAttributeName = "Secure";
        internal const string SeparatorLiteral = "; ";
        internal const string SpecialAttributeLiteral = "$";
        private static System.Net.Comparer staticComparer = new System.Net.Comparer();
        internal const string VersionAttributeName = "Version";

        public Cookie()
        {
            this.m_comment = string.Empty;
            this.m_cookieVariant = CookieVariant.Plain;
            this.m_domain = string.Empty;
            this.m_domain_implicit = true;
            this.m_expires = DateTime.MinValue;
            this.m_name = string.Empty;
            this.m_path = string.Empty;
            this.m_path_implicit = true;
            this.m_port = string.Empty;
            this.m_port_implicit = true;
            this.m_timeStamp = DateTime.Now;
            this.m_value = string.Empty;
            this.m_domainKey = string.Empty;
        }

        public Cookie(string name, string value)
        {
            this.m_comment = string.Empty;
            this.m_cookieVariant = CookieVariant.Plain;
            this.m_domain = string.Empty;
            this.m_domain_implicit = true;
            this.m_expires = DateTime.MinValue;
            this.m_name = string.Empty;
            this.m_path = string.Empty;
            this.m_path_implicit = true;
            this.m_port = string.Empty;
            this.m_port_implicit = true;
            this.m_timeStamp = DateTime.Now;
            this.m_value = string.Empty;
            this.m_domainKey = string.Empty;
            this.Name = name;
            this.m_value = value;
        }

        public Cookie(string name, string value, string path) : this(name, value)
        {
            this.Path = path;
        }

        public Cookie(string name, string value, string path, string domain) : this(name, value, path)
        {
            this.Domain = domain;
        }

        internal Cookie Clone()
        {
            Cookie cookie = new Cookie(this.m_name, this.m_value);
            if (!this.m_port_implicit)
            {
                cookie.Port = this.m_port;
            }
            if (!this.m_path_implicit)
            {
                cookie.Path = this.m_path;
            }
            cookie.Domain = this.m_domain;
            cookie.DomainImplicit = this.m_domain_implicit;
            cookie.m_timeStamp = this.m_timeStamp;
            cookie.Comment = this.m_comment;
            cookie.CommentUri = this.m_commentUri;
            cookie.HttpOnly = this.m_httpOnly;
            cookie.Discard = this.m_discard;
            cookie.Expires = this.m_expires;
            cookie.Version = this.m_version;
            cookie.Secure = this.m_secure;
            cookie.m_cookieVariant = this.m_cookieVariant;
            return cookie;
        }

        private static bool DomainCharsTest(string name)
        {
            if ((name == null) || (name.Length == 0))
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                char ch = name[i];
                if ((((ch < '0') || (ch > '9')) && (((ch != '.') && (ch != '-')) && ((ch < 'a') || (ch > 'z')))) && (((ch < 'A') || (ch > 'Z')) && (ch != '_')))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object comparand)
        {
            if (!(comparand is Cookie))
            {
                return false;
            }
            Cookie cookie = (Cookie) comparand;
            return ((((string.Compare(this.Name, cookie.Name, StringComparison.OrdinalIgnoreCase) == 0) && (string.Compare(this.Value, cookie.Value, StringComparison.Ordinal) == 0)) && ((string.Compare(this.Path, cookie.Path, StringComparison.Ordinal) == 0) && (string.Compare(this.Domain, cookie.Domain, StringComparison.OrdinalIgnoreCase) == 0))) && (this.Version == cookie.Version));
        }

        internal static IComparer GetComparer()
        {
            return staticComparer;
        }

        public override int GetHashCode()
        {
            return string.Concat(new object[] { this.Name, "=", this.Value, ";", this.Path, "; ", this.Domain, "; ", this.Version }).GetHashCode();
        }

        internal bool InternalSetName(string value)
        {
            if ((ValidationHelper.IsBlankString(value) || (value[0] == '$')) || (value.IndexOfAny(Reserved2Name) != -1))
            {
                this.m_name = string.Empty;
                return false;
            }
            this.m_name = value;
            return true;
        }

        private static bool IsDomainEqualToHost(string domain, string host)
        {
            return (((host.Length + 1) == domain.Length) && (string.Compare(host, 0, domain, 1, host.Length, StringComparison.OrdinalIgnoreCase) == 0));
        }

        internal string ToServerString()
        {
            string str = this.Name + "=" + this.Value;
            if ((this.m_comment != null) && (this.m_comment.Length > 0))
            {
                str = str + "; Comment=" + this.m_comment;
            }
            if (this.m_commentUri != null)
            {
                str = str + "; CommentURL=\"" + this.m_commentUri.ToString() + "\"";
            }
            if (this.m_discard)
            {
                str = str + "; Discard";
            }
            if ((!this.m_domain_implicit && (this.m_domain != null)) && (this.m_domain.Length > 0))
            {
                str = str + "; Domain=" + this.m_domain;
            }
            if (this.Expires != DateTime.MinValue)
            {
                TimeSpan span = (TimeSpan) (this.Expires.ToLocalTime() - DateTime.Now);
                int totalSeconds = (int) span.TotalSeconds;
                if (totalSeconds < 0)
                {
                    totalSeconds = 0;
                }
                str = str + "; Max-Age=" + totalSeconds.ToString(NumberFormatInfo.InvariantInfo);
            }
            if ((!this.m_path_implicit && (this.m_path != null)) && (this.m_path.Length > 0))
            {
                str = str + "; Path=" + this.m_path;
            }
            if ((!this.Plain && !this.m_port_implicit) && ((this.m_port != null) && (this.m_port.Length > 0)))
            {
                str = str + "; Port=" + this.m_port;
            }
            if (this.m_version > 0)
            {
                str = str + "; Version=" + this.m_version.ToString(NumberFormatInfo.InvariantInfo);
            }
            if (!(str == "="))
            {
                return str;
            }
            return null;
        }

        public override string ToString()
        {
            string str = this._Domain;
            string str2 = this._Path;
            string str3 = this._Port;
            string str4 = this._Version;
            string str5 = ((str4.Length == 0) ? string.Empty : (str4 + "; ")) + this.Name + "=" + this.Value + ((str2.Length == 0) ? string.Empty : ("; " + str2)) + ((str.Length == 0) ? string.Empty : ("; " + str)) + ((str3.Length == 0) ? string.Empty : ("; " + str3));
            if (str5 == "=")
            {
                return string.Empty;
            }
            return str5;
        }

        internal bool VerifySetDefaults(CookieVariant variant, Uri uri, bool isLocalDomain, string localDomain, bool set_default, bool isThrow)
        {
            string host = uri.Host;
            int port = uri.Port;
            string absolutePath = uri.AbsolutePath;
            bool flag = true;
            if (set_default)
            {
                if (this.Version == 0)
                {
                    variant = CookieVariant.Plain;
                }
                else if ((this.Version == 1) && (variant == CookieVariant.Unknown))
                {
                    variant = CookieVariant.Rfc2109;
                }
                this.m_cookieVariant = variant;
            }
            if (((this.m_name == null) || (this.m_name.Length == 0)) || ((this.m_name[0] == '$') || (this.m_name.IndexOfAny(Reserved2Name) != -1)))
            {
                if (isThrow)
                {
                    throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Name", (this.m_name == null) ? "<null>" : this.m_name }));
                }
                return false;
            }
            if ((this.m_value == null) || ((((this.m_value.Length <= 2) || (this.m_value[0] != '"')) || (this.m_value[this.m_value.Length - 1] != '"')) && (this.m_value.IndexOfAny(Reserved2Value) != -1)))
            {
                if (isThrow)
                {
                    throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Value", (this.m_value == null) ? "<null>" : this.m_value }));
                }
                return false;
            }
            if (((this.Comment != null) && (((this.Comment.Length <= 2) || (this.Comment[0] != '"')) || (this.Comment[this.Comment.Length - 1] != '"'))) && (this.Comment.IndexOfAny(Reserved2Value) != -1))
            {
                if (isThrow)
                {
                    throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Comment", this.Comment }));
                }
                return false;
            }
            if (((this.Path != null) && (((this.Path.Length <= 2) || (this.Path[0] != '"')) || (this.Path[this.Path.Length - 1] != '"'))) && (this.Path.IndexOfAny(Reserved2Value) != -1))
            {
                if (isThrow)
                {
                    throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Path", this.Path }));
                }
                return false;
            }
            if (set_default && this.m_domain_implicit)
            {
                this.m_domain = host;
            }
            else
            {
                if (!this.m_domain_implicit)
                {
                    string domain = this.m_domain;
                    if (!DomainCharsTest(domain))
                    {
                        if (isThrow)
                        {
                            throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Domain", (domain == null) ? "<null>" : domain }));
                        }
                        return false;
                    }
                    if (domain[0] != '.')
                    {
                        if ((variant != CookieVariant.Rfc2965) && (variant != CookieVariant.Plain))
                        {
                            if (isThrow)
                            {
                                throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Domain", this.m_domain }));
                            }
                            return false;
                        }
                        domain = '.' + domain;
                    }
                    int index = host.IndexOf('.');
                    if (isLocalDomain && (string.Compare(localDomain, domain, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        flag = true;
                    }
                    else if (domain.IndexOf('.', 1, domain.Length - 2) == -1)
                    {
                        if (!IsDomainEqualToHost(domain, host))
                        {
                            flag = false;
                        }
                    }
                    else if (variant == CookieVariant.Plain)
                    {
                        if (!IsDomainEqualToHost(domain, host) && ((host.Length <= domain.Length) || (string.Compare(host, host.Length - domain.Length, domain, 0, domain.Length, StringComparison.OrdinalIgnoreCase) != 0)))
                        {
                            flag = false;
                        }
                    }
                    else if ((((index == -1) || (domain.Length != (host.Length - index))) || (string.Compare(host, index, domain, 0, domain.Length, StringComparison.OrdinalIgnoreCase) != 0)) && !IsDomainEqualToHost(domain, host))
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        this.m_domainKey = domain.ToLower(CultureInfo.InvariantCulture);
                    }
                }
                else if (string.Compare(host, this.m_domain, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    flag = false;
                }
                if (!flag)
                {
                    if (isThrow)
                    {
                        throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Domain", this.m_domain }));
                    }
                    return false;
                }
            }
            if (!set_default || !this.m_path_implicit)
            {
                if (!absolutePath.StartsWith(CookieParser.CheckQuoted(this.m_path)))
                {
                    if (isThrow)
                    {
                        throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Path", this.m_path }));
                    }
                    return false;
                }
            }
            else
            {
                switch (this.m_cookieVariant)
                {
                    case CookieVariant.Plain:
                        this.m_path = absolutePath;
                        goto Label_04F0;

                    case CookieVariant.Rfc2109:
                        this.m_path = absolutePath.Substring(0, absolutePath.LastIndexOf('/'));
                        goto Label_04F0;
                }
                this.m_path = absolutePath.Substring(0, absolutePath.LastIndexOf('/') + 1);
            }
        Label_04F0:
            if ((set_default && !this.m_port_implicit) && (this.m_port.Length == 0))
            {
                this.m_port_list = new int[] { port };
            }
            if (!this.m_port_implicit)
            {
                flag = false;
                foreach (int num3 in this.m_port_list)
                {
                    if (num3 == port)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    if (isThrow)
                    {
                        throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Port", this.m_port }));
                    }
                    return false;
                }
            }
            return true;
        }

        private string _Domain
        {
            get
            {
                if ((!this.Plain && !this.m_domain_implicit) && (this.m_domain.Length != 0))
                {
                    return ("$Domain=" + (this.IsQuotedDomain ? "\"" : string.Empty) + this.m_domain + (this.IsQuotedDomain ? "\"" : string.Empty));
                }
                return string.Empty;
            }
        }

        private string _Path
        {
            get
            {
                if ((!this.Plain && !this.m_path_implicit) && (this.m_path.Length != 0))
                {
                    return ("$Path=" + this.m_path);
                }
                return string.Empty;
            }
        }

        private string _Port
        {
            get
            {
                if (!this.m_port_implicit)
                {
                    return ("$Port" + ((this.m_port.Length == 0) ? string.Empty : ("=" + this.m_port)));
                }
                return string.Empty;
            }
        }

        private string _Version
        {
            get
            {
                if (this.Version != 0)
                {
                    return ("$Version=" + (this.IsQuotedVersion ? "\"" : string.Empty) + this.m_version.ToString(NumberFormatInfo.InvariantInfo) + (this.IsQuotedVersion ? "\"" : string.Empty));
                }
                return string.Empty;
            }
        }

        public string Comment
        {
            get
            {
                return this.m_comment;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                this.m_comment = value;
            }
        }

        public Uri CommentUri
        {
            get
            {
                return this.m_commentUri;
            }
            set
            {
                this.m_commentUri = value;
            }
        }

        public bool Discard
        {
            get
            {
                return this.m_discard;
            }
            set
            {
                this.m_discard = value;
            }
        }

        public string Domain
        {
            get
            {
                return this.m_domain;
            }
            set
            {
                this.m_domain = (value == null) ? string.Empty : value;
                this.m_domain_implicit = false;
                this.m_domainKey = string.Empty;
            }
        }

        internal bool DomainImplicit
        {
            get
            {
                return this.m_domain_implicit;
            }
            set
            {
                this.m_domain_implicit = value;
            }
        }

        internal string DomainKey
        {
            get
            {
                if (!this.m_domain_implicit)
                {
                    return this.m_domainKey;
                }
                return this.Domain;
            }
        }

        public bool Expired
        {
            get
            {
                return ((this.m_expires != DateTime.MinValue) && (this.m_expires.ToLocalTime() <= DateTime.Now));
            }
            set
            {
                if (value)
                {
                    this.m_expires = DateTime.Now;
                }
            }
        }

        public DateTime Expires
        {
            get
            {
                return this.m_expires;
            }
            set
            {
                this.m_expires = value;
            }
        }

        public bool HttpOnly
        {
            get
            {
                return this.m_httpOnly;
            }
            set
            {
                this.m_httpOnly = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                if (ValidationHelper.IsBlankString(value) || !this.InternalSetName(value))
                {
                    throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Name", (value == null) ? "<null>" : value }));
                }
            }
        }

        public string Path
        {
            get
            {
                return this.m_path;
            }
            set
            {
                this.m_path = (value == null) ? string.Empty : value;
                this.m_path_implicit = false;
            }
        }

        internal bool Plain
        {
            get
            {
                return (this.Variant == CookieVariant.Plain);
            }
        }

        public string Port
        {
            get
            {
                return this.m_port;
            }
            set
            {
                this.m_port_implicit = false;
                if ((value == null) || (value.Length == 0))
                {
                    this.m_port = string.Empty;
                }
                else
                {
                    if ((value[0] != '"') || (value[value.Length - 1] != '"'))
                    {
                        throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Port", value }));
                    }
                    string[] strArray = value.Split(PortSplitDelimiters);
                    List<int> list = new List<int>();
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        if (strArray[i] != string.Empty)
                        {
                            int num;
                            if (!int.TryParse(strArray[i], out num))
                            {
                                throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Port", value }));
                            }
                            if ((num < 0) || (num > 0xffff))
                            {
                                throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Port", value }));
                            }
                            list.Add(num);
                        }
                    }
                    this.m_port_list = list.ToArray();
                    this.m_port = value;
                    this.m_version = 1;
                    this.m_cookieVariant = CookieVariant.Rfc2965;
                }
            }
        }

        internal int[] PortList
        {
            get
            {
                return this.m_port_list;
            }
        }

        public bool Secure
        {
            get
            {
                return this.m_secure;
            }
            set
            {
                this.m_secure = value;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return this.m_timeStamp;
            }
        }

        public string Value
        {
            get
            {
                return this.m_value;
            }
            set
            {
                this.m_value = (value == null) ? string.Empty : value;
            }
        }

        internal CookieVariant Variant
        {
            get
            {
                return this.m_cookieVariant;
            }
            set
            {
                this.m_cookieVariant = value;
            }
        }

        public int Version
        {
            get
            {
                return this.m_version;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_version = value;
                if ((value > 0) && (this.m_cookieVariant < CookieVariant.Rfc2109))
                {
                    this.m_cookieVariant = CookieVariant.Rfc2109;
                }
            }
        }
    }
}

