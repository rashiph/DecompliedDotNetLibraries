namespace System.Security.Util
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;

    [Serializable]
    internal sealed class URLString : SiteString
    {
        private const string m_defaultProtocol = "file";
        private DirectoryString m_directory;
        private string m_fullurl;
        [OptionalField(VersionAdded=3)]
        private bool m_isUncShare;
        private LocalSiteString m_localSite;
        [OptionalField(VersionAdded=2)]
        private bool m_parseDeferred;
        [OptionalField(VersionAdded=2)]
        private bool m_parsedOriginal;
        private int m_port;
        private string m_protocol;
        private SiteString m_siteString;
        [OptionalField(VersionAdded=2)]
        private string m_urlOriginal;
        [OptionalField(VersionAdded=2)]
        private string m_userpass;

        public URLString()
        {
            this.m_protocol = "";
            this.m_userpass = "";
            this.m_siteString = new SiteString();
            this.m_port = -1;
            this.m_localSite = null;
            this.m_directory = new DirectoryString();
            this.m_parseDeferred = false;
        }

        public URLString(string url) : this(url, false, false)
        {
        }

        public URLString(string url, bool parsed) : this(url, parsed, false)
        {
        }

        internal URLString(string url, bool parsed, bool doDeferredParsing)
        {
            this.m_port = -1;
            this.m_userpass = "";
            this.DoFastChecks(url);
            this.m_urlOriginal = url;
            this.m_parsedOriginal = parsed;
            this.m_parseDeferred = true;
            if (doDeferredParsing)
            {
                this.DoDeferredParse();
            }
        }

        public static bool CompareUrls(URLString url1, URLString url2)
        {
            if ((url1 == null) && (url2 == null))
            {
                return true;
            }
            if ((url1 == null) || (url2 == null))
            {
                return false;
            }
            url1.DoDeferredParse();
            url2.DoDeferredParse();
            URLString str = url1.SpecialNormalizeUrl();
            URLString str2 = url2.SpecialNormalizeUrl();
            if (string.Compare(str.m_protocol, str2.m_protocol, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            if (string.Compare(str.m_protocol, "file", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (!str.m_localSite.IsSubsetOf(str2.m_localSite) || !str2.m_localSite.IsSubsetOf(str.m_localSite))
                {
                    return false;
                }
            }
            else
            {
                if (string.Compare(str.m_userpass, str2.m_userpass, StringComparison.Ordinal) != 0)
                {
                    return false;
                }
                if (!str.m_siteString.IsSubsetOf(str2.m_siteString) || !str2.m_siteString.IsSubsetOf(str.m_siteString))
                {
                    return false;
                }
                if (url1.m_port != url2.m_port)
                {
                    return false;
                }
            }
            return (str.m_directory.IsSubsetOf(str2.m_directory) && str2.m_directory.IsSubsetOf(str.m_directory));
        }

        public override SiteString Copy()
        {
            return new URLString(this.m_urlOriginal, this.m_parsedOriginal);
        }

        [SecuritySafeCritical]
        private void DoDeferredParse()
        {
            if (this.m_parseDeferred)
            {
                this.ParseString(this.m_urlOriginal, this.m_parsedOriginal);
                this.m_parseDeferred = false;
            }
        }

        private void DoFastChecks(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            if (url.Length == 0)
            {
                throw new FormatException(Environment.GetResourceString("Format_StringZeroLength"));
            }
        }

        public override bool Equals(object o)
        {
            this.DoDeferredParse();
            return (((o != null) && (o is URLString)) && this.Equals((URLString) o));
        }

        public bool Equals(URLString url)
        {
            return CompareUrls(this, url);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetDeviceName(string driveLetter, StringHandleOnStack retDeviceName);
        public string GetDirectoryName()
        {
            this.DoDeferredParse();
            if (string.Compare(this.m_protocol, "file", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }
            string str = this.Directory.Replace('/', '\\');
            int length = 0;
            for (int i = str.Length; i > 0; i--)
            {
                if (str[i - 1] == '\\')
                {
                    length = i;
                    break;
                }
            }
            string str2 = this.Host.Replace('/', '\\');
            int index = str2.IndexOf('\\');
            if (index == -1)
            {
                if ((str2.Length != 2) || ((str2[1] != ':') && (str2[1] != '|')))
                {
                    str2 = @"\\" + str2;
                }
            }
            else if ((index > 2) || (((index == 2) && (str2[1] != ':')) && (str2[1] != '|')))
            {
                str2 = @"\\" + str2;
            }
            str2 = str2 + @"\";
            if (length > 0)
            {
                str2 = str2 + str.Substring(0, length);
            }
            return str2;
        }

        public string GetFileName()
        {
            this.DoDeferredParse();
            if (string.Compare(this.m_protocol, "file", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }
            string str = this.Directory.Replace('/', '\\');
            string str2 = this.Host.Replace('/', '\\');
            int index = str2.IndexOf('\\');
            if (index == -1)
            {
                if ((str2.Length != 2) || ((str2[1] != ':') && (str2[1] != '|')))
                {
                    str2 = @"\\" + str2;
                }
            }
            else if ((index != 2) || (((index == 2) && (str2[1] != ':')) && (str2[1] != '|')))
            {
                str2 = @"\\" + str2;
            }
            return (str2 + @"\" + str);
        }

        public override int GetHashCode()
        {
            this.DoDeferredParse();
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
            int caseInsensitiveHashCode = 0;
            if (this.m_protocol != null)
            {
                caseInsensitiveHashCode = textInfo.GetCaseInsensitiveHashCode(this.m_protocol);
            }
            if (this.m_localSite != null)
            {
                caseInsensitiveHashCode ^= this.m_localSite.GetHashCode();
            }
            else
            {
                caseInsensitiveHashCode ^= this.m_siteString.GetHashCode();
            }
            return (caseInsensitiveHashCode ^ this.m_directory.GetHashCode());
        }

        public override bool IsSubsetOf(SiteString site)
        {
            if (site == null)
            {
                return false;
            }
            URLString str = site as URLString;
            if (str == null)
            {
                return false;
            }
            this.DoDeferredParse();
            str.DoDeferredParse();
            URLString str2 = this.SpecialNormalizeUrl();
            URLString str3 = str.SpecialNormalizeUrl();
            if ((string.Compare(str2.m_protocol, str3.m_protocol, StringComparison.OrdinalIgnoreCase) != 0) || !str2.m_directory.IsSubsetOf(str3.m_directory))
            {
                return false;
            }
            if (str2.m_localSite != null)
            {
                return str2.m_localSite.IsSubsetOf(str3.m_localSite);
            }
            if (str2.m_port != str3.m_port)
            {
                return false;
            }
            return ((str3.m_siteString != null) && str2.m_siteString.IsSubsetOf(str3.m_siteString));
        }

        internal string NormalizeUrl()
        {
            StringBuilder builder = new StringBuilder();
            this.DoDeferredParse();
            if (string.Compare(this.m_protocol, "file", StringComparison.OrdinalIgnoreCase) == 0)
            {
                builder = builder.AppendFormat("FILE:///{0}/{1}", this.m_localSite.ToString(), this.m_directory.ToString());
            }
            else
            {
                builder = builder.AppendFormat("{0}://{1}{2}", this.m_protocol, this.m_userpass, this.m_siteString.ToString());
                if (this.m_port != -1)
                {
                    builder = builder.AppendFormat("{0}", this.m_port);
                }
                builder = builder.AppendFormat("/{0}", this.m_directory.ToString());
            }
            return builder.ToString().ToUpper(CultureInfo.InvariantCulture);
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext ctx)
        {
            if (this.m_urlOriginal == null)
            {
                this.m_parseDeferred = false;
                this.m_parsedOriginal = false;
                this.m_userpass = "";
                this.m_urlOriginal = this.m_fullurl;
                this.m_fullurl = null;
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_fullurl = null;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.DoDeferredParse();
                this.m_fullurl = this.m_urlOriginal;
            }
        }

        private void ParseFileURL(string url)
        {
            string str2;
            int num3;
            bool flag;
            string str = url;
            int index = str.IndexOf('/');
            if (((index != -1) && ((((index == 2) && (str[index - 1] != ':')) && (str[index - 1] != '|')) || (index != 2))) && (index != (str.Length - 1)))
            {
                int num2 = str.IndexOf('/', index + 1);
                if (num2 != -1)
                {
                    index = num2;
                }
                else
                {
                    index = -1;
                }
            }
            if (index == -1)
            {
                str2 = str;
            }
            else
            {
                str2 = str.Substring(0, index);
            }
            if (str2.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
            }
            if ((str2[0] == '\\') && (str2[1] == '\\'))
            {
                flag = true;
                num3 = 2;
            }
            else
            {
                num3 = 0;
                flag = false;
            }
            bool flag2 = true;
            while (num3 < str2.Length)
            {
                char ch = str2[num3];
                if ((((ch < 'A') || (ch > 'Z')) && ((ch < 'a') || (ch > 'z'))) && (((ch < '0') || (ch > '9')) && (((((ch != '-') && (ch != '/')) && ((ch != ':') && (ch != '|'))) && (((ch != '.') && (ch != '*')) && (ch != '$'))) && (!flag || (ch != ' ')))))
                {
                    flag2 = false;
                    break;
                }
                num3++;
            }
            if (flag2)
            {
                str2 = string.SmallCharToUpper(str2);
            }
            else
            {
                str2 = str2.ToUpper(CultureInfo.InvariantCulture);
            }
            this.m_localSite = new LocalSiteString(str2);
            if (index == -1)
            {
                if (str2[str2.Length - 1] == '*')
                {
                    this.m_directory = new DirectoryString("*", false);
                }
                else
                {
                    this.m_directory = new DirectoryString();
                }
            }
            else
            {
                string directory = str.Substring(index + 1);
                if (directory.Length == 0)
                {
                    this.m_directory = new DirectoryString();
                }
                else
                {
                    this.m_directory = new DirectoryString(directory, true);
                }
            }
            this.m_siteString = null;
        }

        private void ParseNonFileURL(string url)
        {
            string site = url;
            int index = site.IndexOf('/');
            if (index == -1)
            {
                this.m_localSite = null;
                this.m_siteString = new SiteString(site);
                this.m_directory = new DirectoryString();
            }
            else
            {
                string str2 = site.Substring(0, index);
                this.m_localSite = null;
                this.m_siteString = new SiteString(str2);
                string directory = site.Substring(index + 1);
                if (directory.Length == 0)
                {
                    this.m_directory = new DirectoryString();
                }
                else
                {
                    this.m_directory = new DirectoryString(directory, false);
                }
            }
        }

        private string ParsePort(string url)
        {
            string str = url;
            char[] anyOf = new char[] { ':', '/' };
            int startIndex = 0;
            int index = str.IndexOf('@');
            if ((index != -1) && (str.IndexOf('/', 0, index) == -1))
            {
                this.m_userpass = str.Substring(0, index);
                startIndex = index + 1;
            }
            int num3 = -1;
            int num4 = -1;
            int num5 = -1;
            num3 = url.IndexOf('[', startIndex);
            if (num3 != -1)
            {
                num4 = url.IndexOf(']', num3);
            }
            if (num4 != -1)
            {
                num5 = str.IndexOfAny(anyOf, num4);
            }
            else
            {
                num5 = str.IndexOfAny(anyOf, startIndex);
            }
            if ((num5 != -1) && (str[num5] == ':'))
            {
                if ((str[num5 + 1] < '0') || (str[num5 + 1] > '9'))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                }
                int num6 = str.IndexOf('/', startIndex);
                if (num6 == -1)
                {
                    this.m_port = int.Parse(str.Substring(num5 + 1), CultureInfo.InvariantCulture);
                    if (this.m_port < 0)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                    }
                    return str.Substring(startIndex, num5 - startIndex);
                }
                if (num6 <= num5)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                }
                this.m_port = int.Parse(str.Substring(num5 + 1, (num6 - num5) - 1), CultureInfo.InvariantCulture);
                return (str.Substring(startIndex, num5 - startIndex) + str.Substring(num6));
            }
            return str.Substring(startIndex);
        }

        private string ParseProtocol(string url)
        {
            int index = url.IndexOf(':');
            switch (index)
            {
                case 0:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));

                case -1:
                    this.m_protocol = "file";
                    return url;
            }
            if (url.Length <= (index + 1))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
            }
            if ((index == "file".Length) && (string.Compare(url, 0, "file", 0, index, StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.m_protocol = "file";
                string str = url.Substring(index + 1);
                this.m_isUncShare = true;
                return str;
            }
            if (url[index + 1] != '\\')
            {
                if (((url.Length <= (index + 2)) || (url[index + 1] != '/')) || (url[index + 2] != '/'))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                }
                this.m_protocol = url.Substring(0, index);
                for (int i = 0; i < this.m_protocol.Length; i++)
                {
                    char ch = this.m_protocol[i];
                    if ((((ch < 'a') || (ch > 'z')) && ((ch < 'A') || (ch > 'Z'))) && (((ch < '0') || (ch > '9')) && (((ch != '+') && (ch != '.')) && (ch != '-'))))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                    }
                }
                return url.Substring(index + 3);
            }
            this.m_protocol = "file";
            return url;
        }

        private void ParseString(string url, bool parsed)
        {
            if (!parsed)
            {
                url = this.UnescapeURL(url);
            }
            string str = this.ParseProtocol(url);
            bool isFileURL = string.Compare(this.m_protocol, "file", StringComparison.OrdinalIgnoreCase) == 0;
            str = this.PreProcessURL(str, isFileURL);
            if (isFileURL)
            {
                this.ParseFileURL(str);
            }
            else
            {
                str = this.ParsePort(str);
                this.ParseNonFileURL(str);
            }
        }

        internal static string PreProcessForExtendedPathRemoval(string url, bool isFileUrl)
        {
            bool isUncShare = false;
            return PreProcessForExtendedPathRemoval(url, isFileUrl, ref isUncShare);
        }

        private static string PreProcessForExtendedPathRemoval(string url, bool isFileUrl, ref bool isUncShare)
        {
            StringBuilder builder = new StringBuilder(url);
            int indexA = 0;
            int startIndex = 0;
            if (((url.Length - indexA) >= 4) && ((string.Compare(url, indexA, "//?/", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(url, indexA, "//./", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)))
            {
                builder.Remove(startIndex, 4);
                indexA += 4;
            }
            else
            {
                if (isFileUrl)
                {
                    while (url[indexA] == '/')
                    {
                        indexA++;
                        startIndex++;
                    }
                }
                if (((url.Length - indexA) >= 4) && (((string.Compare(url, indexA, @"\\?\", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(url, indexA, @"\\?/", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)) || ((string.Compare(url, indexA, @"\\.\", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(url, indexA, @"\\./", 0, 4, StringComparison.OrdinalIgnoreCase) == 0))))
                {
                    builder.Remove(startIndex, 4);
                    indexA += 4;
                }
            }
            if (isFileUrl)
            {
                int length = 0;
                bool flag = false;
                while ((length < builder.Length) && ((builder[length] == '/') || (builder[length] == '\\')))
                {
                    if (!flag && (builder[length] == '\\'))
                    {
                        flag = true;
                        if (((length + 1) < builder.Length) && (builder[length + 1] == '\\'))
                        {
                            isUncShare = true;
                        }
                    }
                    length++;
                }
                builder.Remove(0, length);
                builder.Replace('\\', '/');
            }
            if (builder.Length >= 260)
            {
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
            }
            return builder.ToString();
        }

        private string PreProcessURL(string url, bool isFileURL)
        {
            if (isFileURL)
            {
                url = PreProcessForExtendedPathRemoval(url, true, ref this.m_isUncShare);
                return url;
            }
            url = url.Replace('\\', '/');
            return url;
        }

        [SecuritySafeCritical]
        internal URLString SpecialNormalizeUrl()
        {
            this.DoDeferredParse();
            if (string.Compare(this.m_protocol, "file", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return this;
            }
            string driveLetter = this.m_localSite.ToString();
            if ((driveLetter.Length != 2) || ((driveLetter[1] != '|') && (driveLetter[1] != ':')))
            {
                return this;
            }
            string s = null;
            GetDeviceName(driveLetter, JitHelpers.GetStringHandleOnStack(ref s));
            if (s == null)
            {
                return this;
            }
            if (s.IndexOf("://", StringComparison.Ordinal) != -1)
            {
                URLString str3 = new URLString(s + "/" + this.m_directory.ToString());
                str3.DoDeferredParse();
                return str3;
            }
            URLString str4 = new URLString("file://" + s + "/" + this.m_directory.ToString());
            str4.DoDeferredParse();
            return str4;
        }

        public override string ToString()
        {
            return this.m_urlOriginal;
        }

        private string UnescapeURL(string url)
        {
            StringBuilder builder = new StringBuilder(url.Length);
            int startIndex = 0;
            int index = -1;
            int num4 = -1;
            index = url.IndexOf('[', startIndex);
            if (index != -1)
            {
                num4 = url.IndexOf(']', index);
            }
            while (true)
            {
                int num2 = url.IndexOf('%', startIndex);
                if (num2 == -1)
                {
                    return builder.Append(url, startIndex, url.Length - startIndex).ToString();
                }
                if ((num2 > index) && (num2 < num4))
                {
                    builder = builder.Append(url, startIndex, (num4 - startIndex) + 1);
                    startIndex = num4 + 1;
                }
                else
                {
                    if ((url.Length - num2) < 2)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                    }
                    if ((url[num2 + 1] == 'u') || (url[num2 + 1] == 'U'))
                    {
                        if ((url.Length - num2) < 6)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                        }
                        try
                        {
                            char ch = (char) ((((Hex.ConvertHexDigit(url[num2 + 2]) << 12) | (Hex.ConvertHexDigit(url[num2 + 3]) << 8)) | (Hex.ConvertHexDigit(url[num2 + 4]) << 4)) | Hex.ConvertHexDigit(url[num2 + 5]));
                            builder = builder.Append(url, startIndex, num2 - startIndex).Append(ch);
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                        }
                        startIndex = num2 + 6;
                    }
                    else
                    {
                        if ((url.Length - num2) < 3)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                        }
                        try
                        {
                            char ch2 = (char) ((Hex.ConvertHexDigit(url[num2 + 1]) << 4) | Hex.ConvertHexDigit(url[num2 + 2]));
                            builder = builder.Append(url, startIndex, num2 - startIndex).Append(ch2);
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                        }
                        startIndex = num2 + 3;
                    }
                }
            }
        }

        public string Directory
        {
            get
            {
                this.DoDeferredParse();
                return this.m_directory.ToString();
            }
        }

        public string Host
        {
            get
            {
                this.DoDeferredParse();
                if (this.m_siteString != null)
                {
                    return this.m_siteString.ToString();
                }
                return this.m_localSite.ToString();
            }
        }

        public bool IsRelativeFileUrl
        {
            get
            {
                this.DoDeferredParse();
                if (!string.Equals(this.m_protocol, "file", StringComparison.OrdinalIgnoreCase) || this.m_isUncShare)
                {
                    return false;
                }
                string str = (this.m_localSite != null) ? this.m_localSite.ToString() : null;
                if (str.EndsWith('*'))
                {
                    return false;
                }
                string str2 = (this.m_directory != null) ? this.m_directory.ToString() : null;
                if (((str != null) && (str.Length >= 2)) && str.EndsWith(':'))
                {
                    return string.IsNullOrEmpty(str2);
                }
                return true;
            }
        }

        public string Port
        {
            get
            {
                this.DoDeferredParse();
                if (this.m_port == -1)
                {
                    return null;
                }
                return this.m_port.ToString(CultureInfo.InvariantCulture);
            }
        }

        public string Scheme
        {
            get
            {
                this.DoDeferredParse();
                return this.m_protocol;
            }
        }
    }
}

