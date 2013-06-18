namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Text;
    using System.Web.Configuration;

    public sealed class HttpCookie
    {
        private bool _added;
        private bool _changed;
        private string _domain;
        private bool _expirationSet;
        private DateTime _expires;
        private bool _httpOnly;
        private HttpValueCollection _multiValue;
        private string _name;
        private string _path;
        private bool _secure;
        private string _stringValue;

        internal HttpCookie()
        {
            this._path = "/";
            this._changed = true;
        }

        public HttpCookie(string name)
        {
            this._path = "/";
            this._name = name;
            this.SetDefaultsFromConfig();
            this._changed = true;
        }

        public HttpCookie(string name, string value)
        {
            this._path = "/";
            this._name = name;
            this._stringValue = value;
            this.SetDefaultsFromConfig();
            this._changed = true;
        }

        internal HttpResponseHeader GetSetCookieHeader(HttpContext context)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(this._name))
            {
                builder.Append(this._name);
                builder.Append('=');
            }
            if (this._multiValue != null)
            {
                builder.Append(this._multiValue.ToString(false));
            }
            else if (this._stringValue != null)
            {
                builder.Append(this._stringValue);
            }
            if (!string.IsNullOrEmpty(this._domain))
            {
                builder.Append("; domain=");
                builder.Append(this._domain);
            }
            if (this._expirationSet && (this._expires != DateTime.MinValue))
            {
                builder.Append("; expires=");
                builder.Append(HttpUtility.FormatHttpCookieDateTime(this._expires));
            }
            if (!string.IsNullOrEmpty(this._path))
            {
                builder.Append("; path=");
                builder.Append(this._path);
            }
            if (this._secure)
            {
                builder.Append("; secure");
            }
            if (this._httpOnly && this.SupportsHttpOnly(context))
            {
                builder.Append("; HttpOnly");
            }
            return new HttpResponseHeader(0x1b, builder.ToString());
        }

        private void SetDefaultsFromConfig()
        {
            HttpCookiesSection httpCookies = RuntimeConfig.GetConfig().HttpCookies;
            this._secure = httpCookies.RequireSSL;
            this._httpOnly = httpCookies.HttpOnlyCookies;
            if ((httpCookies.Domain != null) && (httpCookies.Domain.Length > 0))
            {
                this._domain = httpCookies.Domain;
            }
        }

        private bool SupportsHttpOnly(HttpContext context)
        {
            if ((context == null) || (context.Request == null))
            {
                return false;
            }
            HttpBrowserCapabilities browser = context.Request.Browser;
            if (browser == null)
            {
                return false;
            }
            if (!(browser.Type != "IE5"))
            {
                return (browser.Platform != "MacPPC");
            }
            return true;
        }

        internal bool Added
        {
            get
            {
                return this._added;
            }
            set
            {
                this._added = value;
            }
        }

        internal bool Changed
        {
            get
            {
                return this._changed;
            }
            set
            {
                this._changed = value;
            }
        }

        public string Domain
        {
            get
            {
                return this._domain;
            }
            set
            {
                this._domain = value;
                this._changed = true;
            }
        }

        public DateTime Expires
        {
            get
            {
                if (!this._expirationSet)
                {
                    return DateTime.MinValue;
                }
                return this._expires;
            }
            set
            {
                this._expires = value;
                this._expirationSet = true;
                this._changed = true;
            }
        }

        public bool HasKeys
        {
            get
            {
                return this.Values.HasKeys();
            }
        }

        public bool HttpOnly
        {
            get
            {
                return this._httpOnly;
            }
            set
            {
                this._httpOnly = value;
                this._changed = true;
            }
        }

        public string this[string key]
        {
            get
            {
                return this.Values[key];
            }
            set
            {
                this.Values[key] = value;
                this._changed = true;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                this._changed = true;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
                this._changed = true;
            }
        }

        public bool Secure
        {
            get
            {
                return this._secure;
            }
            set
            {
                this._secure = value;
                this._changed = true;
            }
        }

        public string Value
        {
            get
            {
                if (this._multiValue != null)
                {
                    return this._multiValue.ToString(false);
                }
                return this._stringValue;
            }
            set
            {
                if (this._multiValue != null)
                {
                    this._multiValue.Reset();
                    this._multiValue.Add(null, value);
                }
                else
                {
                    this._stringValue = value;
                }
                this._changed = true;
            }
        }

        public NameValueCollection Values
        {
            get
            {
                if (this._multiValue == null)
                {
                    this._multiValue = new HttpValueCollection();
                    if (this._stringValue != null)
                    {
                        if ((this._stringValue.IndexOf('&') >= 0) || (this._stringValue.IndexOf('=') >= 0))
                        {
                            this._multiValue.FillFromString(this._stringValue);
                        }
                        else
                        {
                            this._multiValue.Add(null, this._stringValue);
                        }
                        this._stringValue = null;
                    }
                }
                this._changed = true;
                return this._multiValue;
            }
        }
    }
}

