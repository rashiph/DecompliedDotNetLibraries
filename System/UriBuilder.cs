namespace System
{
    using System.Globalization;
    using System.Net;
    using System.Text;
    using System.Threading;

    public class UriBuilder
    {
        private bool m_changed;
        private string m_fragment;
        private string m_host;
        private string m_password;
        private string m_path;
        private int m_port;
        private string m_query;
        private string m_scheme;
        private string m_schemeDelimiter;
        private System.Uri m_uri;
        private string m_username;

        public UriBuilder()
        {
            this.m_changed = true;
            this.m_fragment = string.Empty;
            this.m_host = "localhost";
            this.m_password = string.Empty;
            this.m_path = "/";
            this.m_port = -1;
            this.m_query = string.Empty;
            this.m_scheme = "http";
            this.m_schemeDelimiter = System.Uri.SchemeDelimiter;
            this.m_username = string.Empty;
        }

        public UriBuilder(string uri)
        {
            this.m_changed = true;
            this.m_fragment = string.Empty;
            this.m_host = "localhost";
            this.m_password = string.Empty;
            this.m_path = "/";
            this.m_port = -1;
            this.m_query = string.Empty;
            this.m_scheme = "http";
            this.m_schemeDelimiter = System.Uri.SchemeDelimiter;
            this.m_username = string.Empty;
            System.Uri uri2 = new System.Uri(uri, UriKind.RelativeOrAbsolute);
            if (uri2.IsAbsoluteUri)
            {
                this.Init(uri2);
            }
            else
            {
                uri = System.Uri.UriSchemeHttp + System.Uri.SchemeDelimiter + uri;
                this.Init(new System.Uri(uri));
            }
        }

        public UriBuilder(System.Uri uri)
        {
            this.m_changed = true;
            this.m_fragment = string.Empty;
            this.m_host = "localhost";
            this.m_password = string.Empty;
            this.m_path = "/";
            this.m_port = -1;
            this.m_query = string.Empty;
            this.m_scheme = "http";
            this.m_schemeDelimiter = System.Uri.SchemeDelimiter;
            this.m_username = string.Empty;
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            this.Init(uri);
        }

        public UriBuilder(string schemeName, string hostName)
        {
            this.m_changed = true;
            this.m_fragment = string.Empty;
            this.m_host = "localhost";
            this.m_password = string.Empty;
            this.m_path = "/";
            this.m_port = -1;
            this.m_query = string.Empty;
            this.m_scheme = "http";
            this.m_schemeDelimiter = System.Uri.SchemeDelimiter;
            this.m_username = string.Empty;
            this.Scheme = schemeName;
            this.Host = hostName;
        }

        public UriBuilder(string scheme, string host, int portNumber) : this(scheme, host)
        {
            this.Port = portNumber;
        }

        public UriBuilder(string scheme, string host, int port, string pathValue) : this(scheme, host, port)
        {
            this.Path = pathValue;
        }

        public UriBuilder(string scheme, string host, int port, string path, string extraValue) : this(scheme, host, port, path)
        {
            try
            {
                this.Extra = extraValue;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new ArgumentException("extraValue");
            }
        }

        private string ConvertSlashes(string path)
        {
            StringBuilder builder = new StringBuilder(path.Length);
            for (int i = 0; i < path.Length; i++)
            {
                char ch = path[i];
                if (ch == '\\')
                {
                    ch = '/';
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public override bool Equals(object rparam)
        {
            if (rparam == null)
            {
                return false;
            }
            return this.Uri.Equals(rparam.ToString());
        }

        public override int GetHashCode()
        {
            return this.Uri.GetHashCode();
        }

        private void Init(System.Uri uri)
        {
            this.m_fragment = uri.Fragment;
            this.m_query = uri.Query;
            this.m_host = uri.Host;
            this.m_path = uri.AbsolutePath;
            this.m_port = uri.Port;
            this.m_scheme = uri.Scheme;
            this.m_schemeDelimiter = uri.HasAuthority ? System.Uri.SchemeDelimiter : ":";
            string userInfo = uri.UserInfo;
            if (!ValidationHelper.IsBlankString(userInfo))
            {
                int index = userInfo.IndexOf(':');
                if (index != -1)
                {
                    this.m_password = userInfo.Substring(index + 1);
                    this.m_username = userInfo.Substring(0, index);
                }
                else
                {
                    this.m_username = userInfo;
                }
            }
            this.SetFieldsFromUri(uri);
        }

        private void SetFieldsFromUri(System.Uri uri)
        {
            this.m_fragment = uri.Fragment;
            this.m_query = uri.Query;
            this.m_host = uri.Host;
            this.m_path = uri.AbsolutePath;
            this.m_port = uri.Port;
            this.m_scheme = uri.Scheme;
            this.m_schemeDelimiter = uri.HasAuthority ? System.Uri.SchemeDelimiter : ":";
            string userInfo = uri.UserInfo;
            if (userInfo.Length > 0)
            {
                int index = userInfo.IndexOf(':');
                if (index != -1)
                {
                    this.m_password = userInfo.Substring(index + 1);
                    this.m_username = userInfo.Substring(0, index);
                }
                else
                {
                    this.m_username = userInfo;
                }
            }
        }

        public override string ToString()
        {
            if ((this.m_username.Length == 0) && (this.m_password.Length > 0))
            {
                throw new UriFormatException(SR.GetString("net_uri_BadUserPassword"));
            }
            if (this.m_scheme.Length != 0)
            {
                UriParser syntax = UriParser.GetSyntax(this.m_scheme);
                if (syntax != null)
                {
                    this.m_schemeDelimiter = (syntax.InFact(UriSyntaxFlags.MustHaveAuthority) || (((this.m_host.Length != 0) && syntax.NotAny(UriSyntaxFlags.MailToLikeUri)) && syntax.InFact(UriSyntaxFlags.OptionalAuthority))) ? System.Uri.SchemeDelimiter : ":";
                }
                else
                {
                    this.m_schemeDelimiter = (this.m_host.Length != 0) ? System.Uri.SchemeDelimiter : ":";
                }
            }
            string str = (this.m_scheme.Length != 0) ? (this.m_scheme + this.m_schemeDelimiter) : string.Empty;
            string[] strArray = new string[] { str, this.m_username, (this.m_password.Length > 0) ? (":" + this.m_password) : string.Empty, (this.m_username.Length > 0) ? "@" : string.Empty, this.m_host, ((this.m_port != -1) && (this.m_host.Length > 0)) ? (":" + this.m_port) : string.Empty, (((this.m_host.Length > 0) && (this.m_path.Length != 0)) && (this.m_path[0] != '/')) ? "/" : string.Empty, this.m_path, this.m_query, this.m_fragment };
            return string.Concat(strArray);
        }

        private string Extra
        {
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (value.Length > 0)
                {
                    if (value[0] == '#')
                    {
                        this.Fragment = value.Substring(1);
                    }
                    else
                    {
                        if (value[0] != '?')
                        {
                            throw new ArgumentException("value");
                        }
                        int index = value.IndexOf('#');
                        if (index == -1)
                        {
                            index = value.Length;
                        }
                        else
                        {
                            this.Fragment = value.Substring(index + 1);
                        }
                        this.Query = value.Substring(1, index - 1);
                    }
                }
                else
                {
                    this.Fragment = string.Empty;
                    this.Query = string.Empty;
                }
            }
        }

        public string Fragment
        {
            get
            {
                return this.m_fragment;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (value.Length > 0)
                {
                    value = '#' + value;
                }
                this.m_fragment = value;
                this.m_changed = true;
            }
        }

        public string Host
        {
            get
            {
                return this.m_host;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                this.m_host = value;
                if ((this.m_host.IndexOf(':') >= 0) && (this.m_host[0] != '['))
                {
                    this.m_host = "[" + this.m_host + "]";
                }
                this.m_changed = true;
            }
        }

        public string Password
        {
            get
            {
                return this.m_password;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                this.m_password = value;
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
                if ((value == null) || (value.Length == 0))
                {
                    value = "/";
                }
                this.m_path = System.Uri.InternalEscapeString(this.ConvertSlashes(value));
                this.m_changed = true;
            }
        }

        public int Port
        {
            get
            {
                return this.m_port;
            }
            set
            {
                if ((value < -1) || (value > 0xffff))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_port = value;
                this.m_changed = true;
            }
        }

        public string Query
        {
            get
            {
                return this.m_query;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (value.Length > 0)
                {
                    value = '?' + value;
                }
                this.m_query = value;
                this.m_changed = true;
            }
        }

        public string Scheme
        {
            get
            {
                return this.m_scheme;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                int index = value.IndexOf(':');
                if (index != -1)
                {
                    value = value.Substring(0, index);
                }
                if (value.Length != 0)
                {
                    if (!System.Uri.CheckSchemeName(value))
                    {
                        throw new ArgumentException("value");
                    }
                    value = value.ToLower(CultureInfo.InvariantCulture);
                }
                this.m_scheme = value;
                this.m_changed = true;
            }
        }

        public System.Uri Uri
        {
            get
            {
                if (this.m_changed)
                {
                    this.m_uri = new System.Uri(this.ToString());
                    this.SetFieldsFromUri(this.m_uri);
                    this.m_changed = false;
                }
                return this.m_uri;
            }
        }

        public string UserName
        {
            get
            {
                return this.m_username;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                this.m_username = value;
            }
        }
    }
}

