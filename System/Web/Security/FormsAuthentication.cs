namespace System.Web.Security
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Util;

    public sealed class FormsAuthentication
    {
        private static string _CookieDomain = null;
        private static HttpCookieMode _CookieMode;
        private static string _DefaultUrl;
        private static bool _EnableCrossAppRedirects;
        private static string _FormsCookiePath;
        private static string _FormsName;
        private static bool _Initialized;
        private static object _lockObject = new object();
        private static string _LoginUrl;
        private static FormsProtectionEnum _Protection;
        private static bool _RequireSSL;
        private static bool _SlidingExpiration;
        private static System.Web.Configuration.TicketCompatibilityMode _TicketCompatibilityMode;
        private static int _Timeout;
        private const string CONFIG_DEFAULT_COOKIE = ".ASPXAUTH";
        private const int MAX_TICKET_LENGTH = 0x1000;
        internal const string RETURN_URL = "ReturnUrl";

        public static bool Authenticate(string name, string password)
        {
            bool flag = InternalAuthenticate(name, password);
            if (flag)
            {
                PerfCounters.IncrementCounter(AppPerfCounter.FORMS_AUTH_SUCCESS);
                WebBaseEvent.RaiseSystemEvent(null, 0xfa1, name);
                return flag;
            }
            PerfCounters.IncrementCounter(AppPerfCounter.FORMS_AUTH_FAIL);
            WebBaseEvent.RaiseSystemEvent(null, 0xfa5, name);
            return flag;
        }

        public static FormsAuthenticationTicket Decrypt(string encryptedTicket)
        {
            if (string.IsNullOrEmpty(encryptedTicket) || (encryptedTicket.Length > 0x1000))
            {
                throw new ArgumentException(System.Web.SR.GetString("InvalidArgumentValue", new object[] { "encryptedTicket" }));
            }
            Initialize();
            byte[] buf = null;
            if ((encryptedTicket.Length % 2) == 0)
            {
                try
                {
                    buf = MachineKeySection.HexStringToByteArray(encryptedTicket);
                }
                catch
                {
                }
            }
            if (buf == null)
            {
                buf = HttpServerUtility.UrlTokenDecode(encryptedTicket);
            }
            if ((buf == null) || (buf.Length < 1))
            {
                throw new ArgumentException(System.Web.SR.GetString("InvalidArgumentValue", new object[] { "encryptedTicket" }));
            }
            if ((_Protection == FormsProtectionEnum.All) || (_Protection == FormsProtectionEnum.Encryption))
            {
                buf = MachineKeySection.EncryptOrDecryptData(false, buf, null, 0, buf.Length, false, false, IVType.Random);
                if (buf == null)
                {
                    return null;
                }
            }
            int length = buf.Length;
            if ((_Protection == FormsProtectionEnum.All) || (_Protection == FormsProtectionEnum.Validation))
            {
                if (!MachineKeySection.VerifyHashedData(buf))
                {
                    return null;
                }
                length -= MachineKeySection.HashSize;
            }
            if (!AppSettings.UseLegacyFormsAuthenticationTicketCompatibility)
            {
                return FormsAuthenticationTicketSerializer.Deserialize(buf, length);
            }
            int capacity = (length > 0x1000) ? 0x1000 : length;
            StringBuilder szName = new StringBuilder(capacity);
            StringBuilder szData = new StringBuilder(capacity);
            StringBuilder szPath = new StringBuilder(capacity);
            byte[] pBytes = new byte[4];
            long[] pDates = new long[2];
            if (System.Web.UnsafeNativeMethods.CookieAuthParseTicket(buf, length, szName, capacity, szData, capacity, szPath, capacity, pBytes, pDates) != 0)
            {
                return null;
            }
            DateTime issueDate = DateTime.FromFileTime(pDates[0]);
            return new FormsAuthenticationTicket(pBytes[0], szName.ToString(), issueDate, DateTime.FromFileTime(pDates[1]), pBytes[1] != 0, szData.ToString(), szPath.ToString());
        }

        public static void EnableFormsAuthentication(NameValueCollection configurationData)
        {
            BuildManager.ThrowIfPreAppStartNotRunning();
            configurationData = configurationData ?? new NameValueCollection();
            AuthenticationConfig.Mode = AuthenticationMode.Forms;
            Initialize();
            string str = configurationData["defaultUrl"];
            if (!string.IsNullOrEmpty(str))
            {
                _DefaultUrl = str;
            }
            string str2 = configurationData["loginUrl"];
            if (!string.IsNullOrEmpty(str2))
            {
                _LoginUrl = str2;
            }
        }

        public static string Encrypt(FormsAuthenticationTicket ticket)
        {
            return Encrypt(ticket, true);
        }

        private static string Encrypt(FormsAuthenticationTicket ticket, bool hexEncodedTicket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }
            Initialize();
            byte[] buf = MakeTicketIntoBinaryBlob(ticket);
            if (buf == null)
            {
                return null;
            }
            if ((_Protection == FormsProtectionEnum.All) || (_Protection == FormsProtectionEnum.Validation))
            {
                byte[] src = MachineKeySection.HashData(buf, null, 0, buf.Length);
                if (src == null)
                {
                    return null;
                }
                byte[] dst = new byte[src.Length + buf.Length];
                Buffer.BlockCopy(buf, 0, dst, 0, buf.Length);
                Buffer.BlockCopy(src, 0, dst, buf.Length, src.Length);
                buf = dst;
            }
            if ((_Protection == FormsProtectionEnum.All) || (_Protection == FormsProtectionEnum.Encryption))
            {
                buf = MachineKeySection.EncryptOrDecryptData(true, buf, null, 0, buf.Length, false, false, IVType.Random);
            }
            if (!hexEncodedTicket)
            {
                return HttpServerUtility.UrlTokenEncode(buf);
            }
            return MachineKeySection.ByteArrayToHexString(buf, 0);
        }

        public static HttpCookie GetAuthCookie(string userName, bool createPersistentCookie)
        {
            Initialize();
            return GetAuthCookie(userName, createPersistentCookie, FormsCookiePath);
        }

        public static HttpCookie GetAuthCookie(string userName, bool createPersistentCookie, string strCookiePath)
        {
            return GetAuthCookie(userName, createPersistentCookie, strCookiePath, true);
        }

        private static HttpCookie GetAuthCookie(string userName, bool createPersistentCookie, string strCookiePath, bool hexEncodedTicket)
        {
            Initialize();
            if (userName == null)
            {
                userName = string.Empty;
            }
            if ((strCookiePath == null) || (strCookiePath.Length < 1))
            {
                strCookiePath = FormsCookiePath;
            }
            DateTime utcNow = DateTime.UtcNow;
            DateTime expirationUtc = utcNow.AddMinutes((double) _Timeout);
            FormsAuthenticationTicket ticket = FormsAuthenticationTicket.FromUtc(2, userName, utcNow, expirationUtc, createPersistentCookie, string.Empty, strCookiePath);
            string str = Encrypt(ticket, hexEncodedTicket);
            if ((str == null) || (str.Length < 1))
            {
                throw new HttpException(System.Web.SR.GetString("Unable_to_encrypt_cookie_ticket"));
            }
            HttpCookie cookie = new HttpCookie(FormsCookieName, str) {
                HttpOnly = true,
                Path = strCookiePath,
                Secure = _RequireSSL
            };
            if (_CookieDomain != null)
            {
                cookie.Domain = _CookieDomain;
            }
            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }
            return cookie;
        }

        internal static string GetLoginPage(string extraQueryString)
        {
            return GetLoginPage(extraQueryString, false);
        }

        internal static string GetLoginPage(string extraQueryString, bool reuseReturnUrl)
        {
            HttpContext current = HttpContext.Current;
            string loginUrl = LoginUrl;
            if (loginUrl.IndexOf('?') >= 0)
            {
                loginUrl = RemoveQueryStringVariableFromUrl(loginUrl, "ReturnUrl");
            }
            int index = loginUrl.IndexOf('?');
            if (index < 0)
            {
                loginUrl = loginUrl + "?";
            }
            else if (index < (loginUrl.Length - 1))
            {
                loginUrl = loginUrl + "&";
            }
            string str2 = null;
            if (reuseReturnUrl)
            {
                str2 = HttpUtility.UrlEncode(GetReturnUrl(false), current.Request.QueryStringEncoding);
            }
            if (str2 == null)
            {
                str2 = HttpUtility.UrlEncode(current.Request.RawUrl, current.Request.ContentEncoding);
            }
            loginUrl = loginUrl + "ReturnUrl=" + str2;
            if (!string.IsNullOrEmpty(extraQueryString))
            {
                loginUrl = loginUrl + "&" + extraQueryString;
            }
            return loginUrl;
        }

        public static string GetRedirectUrl(string userName, bool createPersistentCookie)
        {
            if (userName == null)
            {
                return null;
            }
            return GetReturnUrl(true);
        }

        internal static string GetReturnUrl(bool useDefaultIfAbsent)
        {
            Initialize();
            HttpContext current = HttpContext.Current;
            string str = current.Request.QueryString["ReturnUrl"];
            if (str == null)
            {
                str = current.Request.Form["ReturnUrl"];
                if ((!string.IsNullOrEmpty(str) && !str.Contains("/")) && str.Contains("%"))
                {
                    str = HttpUtility.UrlDecode(str);
                }
            }
            if ((!string.IsNullOrEmpty(str) && !EnableCrossAppRedirects) && !UrlPath.IsPathOnSameServer(str, current.Request.Url))
            {
                str = null;
            }
            if (!string.IsNullOrEmpty(str) && CrossSiteScriptingValidation.IsDangerousUrl(str))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_redirect_return_url"));
            }
            if ((str == null) && useDefaultIfAbsent)
            {
                return DefaultUrl;
            }
            return str;
        }

        public static string HashPasswordForStoringInConfigFile(string password, string passwordFormat)
        {
            HashAlgorithm algorithm;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            if (passwordFormat == null)
            {
                throw new ArgumentNullException("passwordFormat");
            }
            if (StringUtil.EqualsIgnoreCase(passwordFormat, "sha1"))
            {
                algorithm = SHA1.Create();
            }
            else
            {
                if (!StringUtil.EqualsIgnoreCase(passwordFormat, "md5"))
                {
                    throw new ArgumentException(System.Web.SR.GetString("InvalidArgumentValue", new object[] { "passwordFormat" }));
                }
                algorithm = MD5.Create();
            }
            return MachineKeySection.ByteArrayToHexString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(password)), 0);
        }

        public static void Initialize()
        {
            if (!_Initialized)
            {
                lock (_lockObject)
                {
                    if (!_Initialized)
                    {
                        AuthenticationSection authentication = RuntimeConfig.GetAppConfig().Authentication;
                        authentication.ValidateAuthenticationMode();
                        _FormsName = authentication.Forms.Name;
                        _RequireSSL = authentication.Forms.RequireSSL;
                        _SlidingExpiration = authentication.Forms.SlidingExpiration;
                        if (_FormsName == null)
                        {
                            _FormsName = ".ASPXAUTH";
                        }
                        _Protection = authentication.Forms.Protection;
                        _Timeout = (int) authentication.Forms.Timeout.TotalMinutes;
                        _FormsCookiePath = authentication.Forms.Path;
                        _LoginUrl = authentication.Forms.LoginUrl;
                        if (_LoginUrl == null)
                        {
                            _LoginUrl = "login.aspx";
                        }
                        _DefaultUrl = authentication.Forms.DefaultUrl;
                        if (_DefaultUrl == null)
                        {
                            _DefaultUrl = "default.aspx";
                        }
                        _CookieMode = authentication.Forms.Cookieless;
                        _CookieDomain = authentication.Forms.Domain;
                        _EnableCrossAppRedirects = authentication.Forms.EnableCrossAppRedirects;
                        _TicketCompatibilityMode = authentication.Forms.TicketCompatibilityMode;
                        _Initialized = true;
                    }
                }
            }
        }

        private static bool InternalAuthenticate(string name, string password)
        {
            AuthenticationSection authentication;
            string str;
            string str2;
            if ((name != null) && (password != null))
            {
                Initialize();
                authentication = RuntimeConfig.GetAppConfig().Authentication;
                authentication.ValidateAuthenticationMode();
                FormsAuthenticationUserCollection users = authentication.Forms.Credentials.Users;
                if (users == null)
                {
                    return false;
                }
                FormsAuthenticationUser user = users[name.ToLower(CultureInfo.InvariantCulture)];
                if (user == null)
                {
                    return false;
                }
                str = user.Password;
                if (str == null)
                {
                    return false;
                }
                switch (authentication.Forms.Credentials.PasswordFormat)
                {
                    case FormsAuthPasswordFormat.Clear:
                        str2 = password;
                        goto Label_00A3;

                    case FormsAuthPasswordFormat.SHA1:
                        str2 = HashPasswordForStoringInConfigFile(password, "sha1");
                        goto Label_00A3;

                    case FormsAuthPasswordFormat.MD5:
                        str2 = HashPasswordForStoringInConfigFile(password, "md5");
                        goto Label_00A3;
                }
            }
            return false;
        Label_00A3:
            return (string.Compare(str2, str, (authentication.Forms.Credentials.PasswordFormat != FormsAuthPasswordFormat.Clear) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0);
        }

        private static bool IsPathWithinAppRoot(HttpContext context, string path)
        {
            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri))
            {
                return HttpRuntime.IsPathWithinAppRoot(path);
            }
            if (!uri.IsLoopback && !string.Equals(context.Request.Url.Host, uri.Host, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return HttpRuntime.IsPathWithinAppRoot(uri.AbsolutePath);
        }

        private static byte[] MakeTicketIntoBinaryBlob(FormsAuthenticationTicket ticket)
        {
            if (((ticket.Name == null) || (ticket.UserData == null)) || (ticket.CookiePath == null))
            {
                return null;
            }
            if (!AppSettings.UseLegacyFormsAuthenticationTicketCompatibility)
            {
                return FormsAuthenticationTicketSerializer.Serialize(ticket);
            }
            byte[] dst = new byte[0x1000];
            byte[] pBytes = new byte[4];
            long[] pDates = new long[2];
            if (((_Protection != FormsProtectionEnum.All) && (_Protection != FormsProtectionEnum.Encryption)) || (MachineKeySection.CompatMode == MachineKeyCompatibilityMode.Framework20SP1))
            {
                byte[] data = new byte[8];
                new RNGCryptoServiceProvider().GetBytes(data);
                Buffer.BlockCopy(data, 0, dst, 0, 8);
            }
            pBytes[0] = (byte) ticket.Version;
            pBytes[1] = ticket.IsPersistent ? ((byte) 1) : ((byte) 0);
            pDates[0] = ticket.IssueDate.ToFileTime();
            pDates[1] = ticket.Expiration.ToFileTime();
            int count = System.Web.UnsafeNativeMethods.CookieAuthConstructTicket(dst, dst.Length, ticket.Name, ticket.UserData, ticket.CookiePath, pBytes, pDates);
            if (count < 0)
            {
                return null;
            }
            byte[] buffer4 = new byte[count];
            Buffer.BlockCopy(dst, 0, buffer4, 0, count);
            return buffer4;
        }

        public static void RedirectFromLoginPage(string userName, bool createPersistentCookie)
        {
            Initialize();
            RedirectFromLoginPage(userName, createPersistentCookie, FormsCookiePath);
        }

        public static void RedirectFromLoginPage(string userName, bool createPersistentCookie, string strCookiePath)
        {
            Initialize();
            if (userName != null)
            {
                HttpContext current = HttpContext.Current;
                string returnUrl = GetReturnUrl(true);
                if (CookiesSupported || IsPathWithinAppRoot(current, returnUrl))
                {
                    SetAuthCookie(userName, createPersistentCookie, strCookiePath);
                    returnUrl = RemoveQueryStringVariableFromUrl(returnUrl, FormsCookieName);
                    if (!CookiesSupported)
                    {
                        int index = returnUrl.IndexOf("://", StringComparison.Ordinal);
                        if (index > 0)
                        {
                            index = returnUrl.IndexOf('/', index + 3);
                            if (index > 0)
                            {
                                returnUrl = returnUrl.Substring(index);
                            }
                        }
                    }
                }
                else
                {
                    if (!EnableCrossAppRedirects)
                    {
                        throw new HttpException(System.Web.SR.GetString("Can_not_issue_cookie_or_redirect"));
                    }
                    HttpCookie cookie = GetAuthCookie(userName, createPersistentCookie, strCookiePath);
                    returnUrl = RemoveQueryStringVariableFromUrl(returnUrl, cookie.Name);
                    if (returnUrl.IndexOf('?') > 0)
                    {
                        string str2 = returnUrl;
                        returnUrl = str2 + "&" + cookie.Name + "=" + cookie.Value;
                    }
                    else
                    {
                        string str3 = returnUrl;
                        returnUrl = str3 + "?" + cookie.Name + "=" + cookie.Value;
                    }
                }
                current.Response.Redirect(returnUrl, false);
            }
        }

        public static void RedirectToLoginPage()
        {
            RedirectToLoginPage(null);
        }

        public static void RedirectToLoginPage(string extraQueryString)
        {
            HttpContext current = HttpContext.Current;
            string loginPage = GetLoginPage(extraQueryString);
            current.Response.Redirect(loginPage, false);
        }

        private static void RemoveQSVar(ref string strUrl, int posQ, string token, string sep, int lenAtStartToLeave)
        {
            for (int i = strUrl.LastIndexOf(token, StringComparison.Ordinal); i >= posQ; i = strUrl.LastIndexOf(token, StringComparison.Ordinal))
            {
                int startIndex = strUrl.IndexOf(sep, i + token.Length, StringComparison.Ordinal) + sep.Length;
                if ((startIndex < sep.Length) || (startIndex >= strUrl.Length))
                {
                    strUrl = strUrl.Substring(0, i);
                }
                else
                {
                    strUrl = strUrl.Substring(0, i + lenAtStartToLeave) + strUrl.Substring(startIndex);
                }
            }
        }

        internal static string RemoveQueryStringVariableFromUrl(string strUrl, string QSVar)
        {
            int index = strUrl.IndexOf('?');
            if (index >= 0)
            {
                string sep = "&";
                string str2 = "?";
                string token = sep + QSVar + "=";
                RemoveQSVar(ref strUrl, index, token, sep, sep.Length);
                token = str2 + QSVar + "=";
                RemoveQSVar(ref strUrl, index, token, sep, str2.Length);
                sep = HttpUtility.UrlEncode("&");
                str2 = HttpUtility.UrlEncode("?");
                token = sep + HttpUtility.UrlEncode(QSVar + "=");
                RemoveQSVar(ref strUrl, index, token, sep, sep.Length);
                token = str2 + HttpUtility.UrlEncode(QSVar + "=");
                RemoveQSVar(ref strUrl, index, token, sep, str2.Length);
            }
            return strUrl;
        }

        public static FormsAuthenticationTicket RenewTicketIfOld(FormsAuthenticationTicket tOld)
        {
            if (tOld == null)
            {
                return null;
            }
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan span = (TimeSpan) (utcNow - tOld.IssueDateUtc);
            TimeSpan span2 = (TimeSpan) (tOld.ExpirationUtc - utcNow);
            if (span2 > span)
            {
                return tOld;
            }
            TimeSpan span3 = (TimeSpan) (tOld.ExpirationUtc - tOld.IssueDateUtc);
            DateTime expirationUtc = utcNow + span3;
            return FormsAuthenticationTicket.FromUtc(tOld.Version, tOld.Name, utcNow, expirationUtc, tOld.IsPersistent, tOld.UserData, tOld.CookiePath);
        }

        public static void SetAuthCookie(string userName, bool createPersistentCookie)
        {
            Initialize();
            SetAuthCookie(userName, createPersistentCookie, FormsCookiePath);
        }

        public static void SetAuthCookie(string userName, bool createPersistentCookie, string strCookiePath)
        {
            Initialize();
            HttpContext current = HttpContext.Current;
            if (!current.Request.IsSecureConnection && RequireSSL)
            {
                throw new HttpException(System.Web.SR.GetString("Connection_not_secure_creating_secure_cookie"));
            }
            bool flag = CookielessHelperClass.UseCookieless(current, false, CookieMode);
            HttpCookie cookie = GetAuthCookie(userName, createPersistentCookie, flag ? "/" : strCookiePath, !flag);
            if (!flag)
            {
                HttpContext.Current.Response.Cookies.Add(cookie);
                current.CookielessHelper.SetCookieValue('F', null);
            }
            else
            {
                current.CookielessHelper.SetCookieValue('F', cookie.Value);
            }
        }

        public static void SignOut()
        {
            Initialize();
            HttpContext current = HttpContext.Current;
            bool flag = current.CookielessHelper.DoesCookieValueExistInOriginal('F');
            current.CookielessHelper.SetCookieValue('F', null);
            if (!CookielessHelperClass.UseCookieless(current, false, CookieMode) || current.Request.Browser.Cookies)
            {
                string str = string.Empty;
                if (current.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                {
                    str = "NoCookie";
                }
                HttpCookie cookie = new HttpCookie(FormsCookieName, str) {
                    HttpOnly = true,
                    Path = _FormsCookiePath,
                    Expires = new DateTime(0x7cf, 10, 12),
                    Secure = _RequireSSL
                };
                if (_CookieDomain != null)
                {
                    cookie.Domain = _CookieDomain;
                }
                current.Response.Cookies.RemoveCookie(FormsCookieName);
                current.Response.Cookies.Add(cookie);
            }
            if (flag)
            {
                current.Response.Redirect(GetLoginPage(null), false);
            }
        }

        public static string CookieDomain
        {
            get
            {
                Initialize();
                return _CookieDomain;
            }
        }

        public static HttpCookieMode CookieMode
        {
            get
            {
                Initialize();
                return _CookieMode;
            }
        }

        public static bool CookiesSupported
        {
            get
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    return !CookielessHelperClass.UseCookieless(current, false, CookieMode);
                }
                return true;
            }
        }

        public static string DefaultUrl
        {
            get
            {
                Initialize();
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    return AuthenticationConfig.GetCompleteLoginUrl(current, _DefaultUrl);
                }
                if ((_DefaultUrl.Length != 0) && ((_DefaultUrl[0] == '/') || (_DefaultUrl.IndexOf("//", StringComparison.Ordinal) >= 0)))
                {
                    return _DefaultUrl;
                }
                return ("/" + _DefaultUrl);
            }
        }

        public static bool EnableCrossAppRedirects
        {
            get
            {
                Initialize();
                return _EnableCrossAppRedirects;
            }
        }

        public static string FormsCookieName
        {
            get
            {
                Initialize();
                return _FormsName;
            }
        }

        public static string FormsCookiePath
        {
            get
            {
                Initialize();
                return _FormsCookiePath;
            }
        }

        public static bool IsEnabled
        {
            get
            {
                return (AuthenticationConfig.Mode == AuthenticationMode.Forms);
            }
        }

        public static string LoginUrl
        {
            get
            {
                Initialize();
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    return AuthenticationConfig.GetCompleteLoginUrl(current, _LoginUrl);
                }
                if ((_LoginUrl.Length != 0) && ((_LoginUrl[0] == '/') || (_LoginUrl.IndexOf("//", StringComparison.Ordinal) >= 0)))
                {
                    return _LoginUrl;
                }
                return ("/" + _LoginUrl);
            }
        }

        public static bool RequireSSL
        {
            get
            {
                Initialize();
                return _RequireSSL;
            }
        }

        public static bool SlidingExpiration
        {
            get
            {
                Initialize();
                return _SlidingExpiration;
            }
        }

        public static System.Web.Configuration.TicketCompatibilityMode TicketCompatibilityMode
        {
            get
            {
                Initialize();
                return _TicketCompatibilityMode;
            }
        }

        public static TimeSpan Timeout
        {
            get
            {
                Initialize();
                return new TimeSpan(0, _Timeout, 0);
            }
        }
    }
}

