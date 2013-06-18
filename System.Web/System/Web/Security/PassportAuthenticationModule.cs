namespace System.Web.Security
{
    using System;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Handlers;

    [Obsolete("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
    public sealed class PassportAuthenticationModule : IHttpModule
    {
        private static bool _fAuthChecked;
        private static bool _fAuthRequired;
        private static string _LoginUrl;

        public event PassportAuthenticationEventHandler Authenticate;

        public void Dispose()
        {
        }

        public void Init(HttpApplication app)
        {
            app.AuthenticateRequest += new EventHandler(this.OnEnter);
            app.EndRequest += new EventHandler(this.OnLeave);
        }

        private void OnAuthenticate(PassportAuthenticationEventArgs e)
        {
            if (this._eventHandler != null)
            {
                this._eventHandler(this, e);
                if ((e.Context.User == null) && (e.User != null))
                {
                    InternalSecurityPermissions.ControlPrincipal.Demand();
                    e.Context.User = e.User;
                }
            }
            if (e.Context.User == null)
            {
                InternalSecurityPermissions.ControlPrincipal.Demand();
                e.Context.User = new PassportPrincipal(e.Identity, new string[0]);
            }
        }

        private void OnEnter(object source, EventArgs eventArgs)
        {
            if (!_fAuthChecked || _fAuthRequired)
            {
                HttpApplication application = (HttpApplication) source;
                HttpContext context = application.Context;
                if (!_fAuthChecked)
                {
                    AuthenticationSection authentication = RuntimeConfig.GetAppConfig().Authentication;
                    _fAuthRequired = AuthenticationConfig.Mode == AuthenticationMode.Passport;
                    _LoginUrl = authentication.Passport.RedirectUrl;
                    _fAuthChecked = true;
                }
                if (_fAuthRequired)
                {
                    PassportIdentity identity = new PassportIdentity();
                    this.OnAuthenticate(new PassportAuthenticationEventArgs(identity, context));
                    context.SetSkipAuthorizationNoDemand(AuthenticationConfig.AccessingLoginPage(context, _LoginUrl), false);
                    if (!context.SkipAuthorization)
                    {
                        context.SkipAuthorization = AssemblyResourceLoader.IsValidWebResourceRequest(context);
                    }
                }
            }
        }

        private void OnLeave(object source, EventArgs eventArgs)
        {
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            if ((_fAuthChecked && _fAuthRequired) && (((context.User != null) && (context.User.Identity != null)) && (context.User.Identity is PassportIdentity)))
            {
                PassportIdentity identity = (PassportIdentity) context.User.Identity;
                if ((context.Response.StatusCode == 0x191) && !identity.WWWAuthHeaderSet)
                {
                    if (((_LoginUrl == null) || (_LoginUrl.Length < 1)) || (string.Compare(_LoginUrl, "internal", StringComparison.Ordinal) == 0))
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = 200;
                        if (!ErrorFormatter.RequiresAdaptiveErrorReporting(context))
                        {
                            string str = context.Request.Url.ToString();
                            int index = str.IndexOf('?');
                            if (index >= 0)
                            {
                                str = str.Substring(0, index);
                            }
                            string str2 = identity.LogoTag2(HttpUtility.UrlEncode(str, context.Request.ContentEncoding));
                            string s = System.Web.SR.GetString("PassportAuthFailed", new object[] { str2 });
                            context.Response.Write(s);
                        }
                        else
                        {
                            ErrorFormatter formatter = new PassportAuthFailedErrorFormatter();
                            context.Response.Write(formatter.GetAdaptiveErrorMessage(context, true));
                        }
                    }
                    else
                    {
                        string str7;
                        string completeLoginUrl = AuthenticationConfig.GetCompleteLoginUrl(context, _LoginUrl);
                        if ((completeLoginUrl == null) || (completeLoginUrl.Length <= 0))
                        {
                            throw new HttpException(System.Web.SR.GetString("Invalid_Passport_Redirect_URL"));
                        }
                        string str5 = context.Request.Url.ToString();
                        if (completeLoginUrl.IndexOf('?') >= 0)
                        {
                            str7 = "&";
                        }
                        else
                        {
                            str7 = "?";
                        }
                        string url = completeLoginUrl + str7 + "ReturnUrl=" + HttpUtility.UrlEncode(str5, context.Request.ContentEncoding);
                        int num2 = str5.IndexOf('?');
                        if ((num2 >= 0) && (num2 < (str5.Length - 1)))
                        {
                            url = url + "&" + str5.Substring(num2 + 1);
                        }
                        context.Response.Redirect(url, false);
                    }
                }
            }
        }
    }
}

