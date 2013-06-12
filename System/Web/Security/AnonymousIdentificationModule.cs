namespace System.Web.Security
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;

    public sealed class AnonymousIdentificationModule : IHttpModule
    {
        private const int MAX_ENCODED_COOKIE_STRING = 0x200;
        private const int MAX_ID_LENGTH = 0x80;
        private static HttpCookieMode s_CookieMode = HttpCookieMode.UseDeviceProfile;
        private static string s_CookieName = ".ASPXANONYMOUS";
        private static string s_CookiePath = "/";
        private static int s_CookieTimeout = 0x186a0;
        private static string s_Domain = null;
        private static bool s_Enabled = false;
        private static bool s_Initialized = false;
        private static object s_InitLock = new object();
        private static byte[] s_Modifier = null;
        private static CookieProtection s_Protection = CookieProtection.None;
        private static bool s_RequireSSL = false;
        private static bool s_SlidingExpiration = true;

        public event AnonymousIdentificationEventHandler Creating;

        public static void ClearAnonymousIdentifier()
        {
            if (!s_Initialized)
            {
                Initialize();
            }
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                if (!s_Enabled || !current.Request.IsAuthenticated)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("Anonymous_ClearAnonymousIdentifierNotSupported"));
                }
                bool flag = false;
                if (current.CookielessHelper.GetCookieValue('A') != null)
                {
                    current.CookielessHelper.SetCookieValue('A', null);
                    flag = true;
                }
                if (!CookielessHelperClass.UseCookieless(current, false, s_CookieMode) || current.Request.Browser.Cookies)
                {
                    string str = string.Empty;
                    if (current.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                    {
                        str = "NoCookie";
                    }
                    HttpCookie cookie = new HttpCookie(s_CookieName, str) {
                        HttpOnly = true,
                        Path = s_CookiePath,
                        Secure = s_RequireSSL
                    };
                    if (s_Domain != null)
                    {
                        cookie.Domain = s_Domain;
                    }
                    cookie.Expires = new DateTime(0x7cf, 10, 12);
                    current.Response.Cookies.RemoveCookie(s_CookieName);
                    current.Response.Cookies.Add(cookie);
                }
                if (flag)
                {
                    current.Response.Redirect(current.Request.RawUrl, false);
                }
            }
        }

        public void Dispose()
        {
        }

        private static AnonymousIdData GetDecodedValue(string data)
        {
            if (((data != null) && (data.Length >= 1)) && (data.Length <= 0x200))
            {
                try
                {
                    byte[] buffer = CookieProtectionHelper.Decode(s_Protection, data);
                    if ((buffer == null) || (buffer.Length < 13))
                    {
                        return null;
                    }
                    DateTime dt = DateTime.FromFileTimeUtc(BitConverter.ToInt64(buffer, 0));
                    if (dt < DateTime.UtcNow)
                    {
                        return null;
                    }
                    int count = BitConverter.ToInt32(buffer, 8);
                    if ((count < 0) || (count > (buffer.Length - 12)))
                    {
                        return null;
                    }
                    string id = Encoding.UTF8.GetString(buffer, 12, count);
                    if (id.Length > 0x80)
                    {
                        return null;
                    }
                    return new AnonymousIdData(id, dt);
                }
                catch
                {
                }
            }
            return null;
        }

        private static string GetEncodedValue(AnonymousIdData data)
        {
            if (data == null)
            {
                return null;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(data.AnonymousId);
            byte[] src = BitConverter.GetBytes(bytes.Length);
            byte[] buffer3 = BitConverter.GetBytes(data.ExpireDate.ToFileTimeUtc());
            byte[] dst = new byte[12 + bytes.Length];
            Buffer.BlockCopy(buffer3, 0, dst, 0, 8);
            Buffer.BlockCopy(src, 0, dst, 8, 4);
            Buffer.BlockCopy(bytes, 0, dst, 12, bytes.Length);
            return CookieProtectionHelper.Encode(s_Protection, dst, dst.Length);
        }

        public void Init(HttpApplication app)
        {
            if (!s_Initialized)
            {
                Initialize();
            }
            if (s_Enabled)
            {
                app.PostAuthenticateRequest += new EventHandler(this.OnEnter);
            }
        }

        private static void Initialize()
        {
            if (!s_Initialized)
            {
                lock (s_InitLock)
                {
                    if (!s_Initialized)
                    {
                        AnonymousIdentificationSection anonymousIdentification = RuntimeConfig.GetAppConfig().AnonymousIdentification;
                        s_Enabled = anonymousIdentification.Enabled;
                        s_CookieName = anonymousIdentification.CookieName;
                        s_CookiePath = anonymousIdentification.CookiePath;
                        s_CookieTimeout = (int) anonymousIdentification.CookieTimeout.TotalMinutes;
                        s_RequireSSL = anonymousIdentification.CookieRequireSSL;
                        s_SlidingExpiration = anonymousIdentification.CookieSlidingExpiration;
                        s_Protection = anonymousIdentification.CookieProtection;
                        s_CookieMode = anonymousIdentification.Cookieless;
                        s_Domain = anonymousIdentification.Domain;
                        s_Modifier = Encoding.UTF8.GetBytes("AnonymousIdentification");
                        if (s_CookieTimeout < 1)
                        {
                            s_CookieTimeout = 1;
                        }
                        if (s_CookieTimeout > 0x100a40)
                        {
                            s_CookieTimeout = 0x100a40;
                        }
                        s_Initialized = true;
                    }
                }
            }
        }

        private void OnEnter(object source, EventArgs eventArgs)
        {
            if (!s_Initialized)
            {
                Initialize();
            }
            if (s_Enabled)
            {
                bool flag2;
                HttpCookie cookie = null;
                bool flag = false;
                AnonymousIdData decodedValue = null;
                string cookieValue = null;
                bool isAuthenticated = false;
                HttpApplication application = (HttpApplication) source;
                HttpContext context = application.Context;
                isAuthenticated = context.Request.IsAuthenticated;
                if (isAuthenticated)
                {
                    flag2 = CookielessHelperClass.UseCookieless(context, false, s_CookieMode);
                }
                else
                {
                    flag2 = CookielessHelperClass.UseCookieless(context, true, s_CookieMode);
                }
                if ((s_RequireSSL && !context.Request.IsSecureConnection) && !flag2)
                {
                    if (context.Request.Cookies[s_CookieName] != null)
                    {
                        cookie = new HttpCookie(s_CookieName, string.Empty) {
                            HttpOnly = true,
                            Path = s_CookiePath,
                            Secure = s_RequireSSL
                        };
                        if (s_Domain != null)
                        {
                            cookie.Domain = s_Domain;
                        }
                        cookie.Expires = new DateTime(0x7cf, 10, 12);
                        if (context.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                        {
                            cookie.Value = "NoCookie";
                        }
                        context.Response.Cookies.Add(cookie);
                    }
                }
                else
                {
                    if (!flag2)
                    {
                        cookie = context.Request.Cookies[s_CookieName];
                        if (cookie != null)
                        {
                            cookieValue = cookie.Value;
                            cookie.Path = s_CookiePath;
                            if (s_Domain != null)
                            {
                                cookie.Domain = s_Domain;
                            }
                        }
                    }
                    else
                    {
                        cookieValue = context.CookielessHelper.GetCookieValue('A');
                    }
                    decodedValue = GetDecodedValue(cookieValue);
                    if ((decodedValue != null) && (decodedValue.AnonymousId != null))
                    {
                        context.Request._AnonymousId = decodedValue.AnonymousId;
                    }
                    if (!isAuthenticated)
                    {
                        if (context.Request._AnonymousId == null)
                        {
                            if (this._CreateNewIdEventHandler != null)
                            {
                                AnonymousIdentificationEventArgs e = new AnonymousIdentificationEventArgs(context);
                                this._CreateNewIdEventHandler(this, e);
                                context.Request._AnonymousId = e.AnonymousID;
                            }
                            if (string.IsNullOrEmpty(context.Request._AnonymousId))
                            {
                                context.Request._AnonymousId = Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
                            }
                            else if (context.Request._AnonymousId.Length > 0x80)
                            {
                                throw new HttpException(System.Web.SR.GetString("Anonymous_id_too_long"));
                            }
                            if ((s_RequireSSL && !context.Request.IsSecureConnection) && !flag2)
                            {
                                return;
                            }
                            flag = true;
                        }
                        DateTime utcNow = DateTime.UtcNow;
                        if (!flag && s_SlidingExpiration)
                        {
                            if ((decodedValue == null) || (decodedValue.ExpireDate < utcNow))
                            {
                                flag = true;
                            }
                            else
                            {
                                TimeSpan span = (TimeSpan) (decodedValue.ExpireDate - utcNow);
                                if (span.TotalSeconds < ((s_CookieTimeout * 60) / 2))
                                {
                                    flag = true;
                                }
                            }
                        }
                        if (flag)
                        {
                            DateTime dt = utcNow.AddMinutes((double) s_CookieTimeout);
                            cookieValue = GetEncodedValue(new AnonymousIdData(context.Request.AnonymousID, dt));
                            if (cookieValue.Length > 0x200)
                            {
                                throw new HttpException(System.Web.SR.GetString("Anonymous_id_too_long_2"));
                            }
                            if (!flag2)
                            {
                                cookie = new HttpCookie(s_CookieName, cookieValue) {
                                    HttpOnly = true,
                                    Expires = dt,
                                    Path = s_CookiePath,
                                    Secure = s_RequireSSL
                                };
                                if (s_Domain != null)
                                {
                                    cookie.Domain = s_Domain;
                                }
                                context.Response.Cookies.Add(cookie);
                            }
                            else
                            {
                                context.CookielessHelper.SetCookieValue('A', cookieValue);
                                context.Response.Redirect(context.Request.RawUrl);
                            }
                        }
                    }
                }
            }
        }

        public static bool Enabled
        {
            get
            {
                if (!s_Initialized)
                {
                    Initialize();
                }
                return s_Enabled;
            }
        }
    }
}

