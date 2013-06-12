namespace System.Web.SessionState
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Security;
    using System.Web.Util;

    public class SessionIDManager : ISessionIDManager
    {
        private bool _isInherited;
        private RandomNumberGenerator _randgen;
        internal const string ASP_SESSIONID_MANAGER_INITIALIZEREQUEST_CALLED_KEY = "AspSessionIDManagerInitializeRequestCalled";
        internal const string COOKIELESS_BOOL_SESSION_KEY = "AspCookielessBoolSession";
        internal const string COOKIELESS_SESSION_KEY = "AspCookielessSession";
        private const int COOKIELESS_SESSION_LENGTH = 0x1a;
        internal const HttpCookieMode COOKIEMODE_DEFAULT = HttpCookieMode.UseCookies;
        private static string s_appPath;
        private static SessionStateSection s_config;
        private static int s_iSessionId;
        private static ReadWriteSpinLock s_lock;
        internal const string SESSION_COOKIE_DEFAULT = "ASP.NET_SessionId";
        internal const int SESSION_ID_LENGTH_LIMIT = 80;

        internal static bool CheckIdLength(string id, bool throwOnFail)
        {
            bool flag = true;
            if (id.Length <= 80)
            {
                return flag;
            }
            if (throwOnFail)
            {
                object[] args = new object[] { 80.ToString(CultureInfo.InvariantCulture), id };
                throw new HttpException(System.Web.SR.GetString("Session_id_too_long", args));
            }
            return false;
        }

        private void CheckInitializeRequestCalled(HttpContext context)
        {
            if (context.Items["AspSessionIDManagerInitializeRequestCalled"] == null)
            {
                throw new HttpException(System.Web.SR.GetString("SessionIDManager_InitializeRequest_not_called"));
            }
        }

        private static HttpCookie CreateSessionCookie(string id)
        {
            return new HttpCookie(Config.CookieName, id) { Path = "/", HttpOnly = true };
        }

        public virtual string CreateSessionID(HttpContext context)
        {
            return SessionId.Create(ref this._randgen);
        }

        public virtual string Decode(string id)
        {
            if (this._isInherited)
            {
                return HttpUtility.UrlDecode(id);
            }
            return id.ToLower(CultureInfo.InvariantCulture);
        }

        public virtual string Encode(string id)
        {
            if (this._isInherited)
            {
                return HttpUtility.UrlEncode(id);
            }
            return id;
        }

        internal void GetCookielessSessionID(HttpContext context, bool allowRedirect, out bool cookieless)
        {
            HttpRequest request = context.Request;
            cookieless = CookielessHelperClass.UseCookieless(context, allowRedirect, Config.Cookieless);
            context.Items["AspCookielessBoolSession"] = (bool) cookieless;
            if (cookieless)
            {
                string cookieValue = context.CookielessHelper.GetCookieValue('S');
                if (cookieValue == null)
                {
                    cookieValue = string.Empty;
                }
                cookieValue = this.Decode(cookieValue);
                if (this.ValidateInternal(cookieValue, false))
                {
                    context.Items.Add("AspCookielessSession", cookieValue);
                }
            }
        }

        public string GetSessionID(HttpContext context)
        {
            string id = null;
            this.CheckInitializeRequestCalled(context);
            if (this.UseCookieless(context))
            {
                return (string) context.Items["AspCookielessSession"];
            }
            HttpCookie cookie = context.Request.Cookies[Config.CookieName];
            if ((cookie != null) && (cookie.Value != null))
            {
                id = this.Decode(cookie.Value);
                if ((id != null) && !this.ValidateInternal(id, false))
                {
                    id = null;
                }
            }
            return id;
        }

        public void Initialize()
        {
            if (s_config == null)
            {
                s_lock.AcquireWriterLock();
                try
                {
                    if (s_config == null)
                    {
                        this.OneTimeInit();
                    }
                }
                finally
                {
                    s_lock.ReleaseWriterLock();
                }
            }
            this._isInherited = !(base.GetType() == typeof(SessionIDManager));
        }

        public bool InitializeRequest(HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue)
        {
            bool flag;
            if (context.Items["AspSessionIDManagerInitializeRequestCalled"] != null)
            {
                supportSessionIDReissue = this.UseCookieless(context);
                return false;
            }
            context.Items["AspSessionIDManagerInitializeRequestCalled"] = true;
            if (Config.Cookieless == HttpCookieMode.UseCookies)
            {
                supportSessionIDReissue = false;
                return false;
            }
            this.GetCookielessSessionID(context, !suppressAutoDetectRedirect, out flag);
            supportSessionIDReissue = flag;
            return context.Response.IsRequestBeingRedirected;
        }

        private void OneTimeInit()
        {
            SessionStateSection sessionState = RuntimeConfig.GetAppConfig().SessionState;
            s_appPath = HostingEnvironment.ApplicationVirtualPathObject.VirtualPathString;
            s_iSessionId = s_appPath.Length;
            s_config = sessionState;
        }

        public void RemoveSessionID(HttpContext context)
        {
            context.Response.Cookies.RemoveCookie(Config.CookieName);
        }

        public void SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded)
        {
            redirected = false;
            cookieAdded = false;
            this.CheckInitializeRequestCalled(context);
            if (!context.Response.IsBuffered())
            {
                throw new HttpException(System.Web.SR.GetString("Cant_save_session_id_because_response_was_flushed"));
            }
            if (!this.ValidateInternal(id, true))
            {
                throw new HttpException(System.Web.SR.GetString("Cant_save_session_id_because_id_is_invalid", new object[] { id }));
            }
            string str = this.Encode(id);
            if (!this.UseCookieless(context))
            {
                HttpCookie cookie = CreateSessionCookie(str);
                context.Response.Cookies.Add(cookie);
                cookieAdded = true;
            }
            else
            {
                context.CookielessHelper.SetCookieValue('S', str);
                HttpRequest request = context.Request;
                string path = request.Path;
                string queryStringText = request.QueryStringText;
                if (!string.IsNullOrEmpty(queryStringText))
                {
                    path = path + "?" + queryStringText;
                }
                context.Response.Redirect(path, false);
                context.ApplicationInstance.CompleteRequest();
                redirected = true;
            }
        }

        internal bool UseCookieless(HttpContext context)
        {
            if (Config.Cookieless == HttpCookieMode.UseCookies)
            {
                return false;
            }
            object obj2 = context.Items["AspCookielessBoolSession"];
            return (bool) obj2;
        }

        public virtual bool Validate(string id)
        {
            return SessionId.IsLegit(id);
        }

        private bool ValidateInternal(string id, bool throwOnIdCheck)
        {
            return (CheckIdLength(id, throwOnIdCheck) && this.Validate(id));
        }

        private static SessionStateSection Config
        {
            get
            {
                if (s_config == null)
                {
                    throw new HttpException(System.Web.SR.GetString("SessionIDManager_uninit"));
                }
                return s_config;
            }
        }

        public static int SessionIDMaxLength
        {
            get
            {
                return 80;
            }
        }
    }
}

