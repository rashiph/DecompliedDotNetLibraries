namespace System.Web.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;

    public sealed class RoleManagerModule : IHttpModule
    {
        private RoleManagerEventHandler _eventHandler;
        private const int MAX_COOKIE_LENGTH = 0x1000;

        public event RoleManagerEventHandler GetRoles
        {
            add
            {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
                this._eventHandler = (RoleManagerEventHandler) Delegate.Combine(this._eventHandler, value);
            }
            remove
            {
                this._eventHandler = (RoleManagerEventHandler) Delegate.Remove(this._eventHandler, value);
            }
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal=true)]
        private RolePrincipal CreateRolePrincipalWithAssert(IIdentity identity, string encryptedTicket = null)
        {
            if (encryptedTicket == null)
            {
                return new RolePrincipal(identity);
            }
            return new RolePrincipal(identity, encryptedTicket);
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication app)
        {
            if (Roles.Enabled)
            {
                app.PostAuthenticateRequest += new EventHandler(this.OnEnter);
                app.EndRequest += new EventHandler(this.OnLeave);
            }
        }

        private void OnEnter(object source, EventArgs eventArgs)
        {
            if (!Roles.Enabled)
            {
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    ((HttpApplication) source).Context.DisableNotifications(RequestNotification.EndRequest, 0);
                }
            }
            else
            {
                HttpApplication application = (HttpApplication) source;
                HttpContext context = application.Context;
                if (this._eventHandler != null)
                {
                    RoleManagerEventArgs e = new RoleManagerEventArgs(context);
                    this._eventHandler(this, e);
                    if (e.RolesPopulated)
                    {
                        return;
                    }
                }
                if (Roles.CacheRolesInCookie)
                {
                    if (context.User.Identity.IsAuthenticated && (!Roles.CookieRequireSSL || context.Request.IsSecureConnection))
                    {
                        try
                        {
                            HttpCookie cookie = context.Request.Cookies[Roles.CookieName];
                            if (cookie != null)
                            {
                                string encryptedTicket = cookie.Value;
                                if ((encryptedTicket != null) && (encryptedTicket.Length > 0x1000))
                                {
                                    Roles.DeleteCookie();
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(Roles.CookiePath) && (Roles.CookiePath != "/"))
                                    {
                                        cookie.Path = Roles.CookiePath;
                                    }
                                    cookie.Domain = Roles.Domain;
                                    context.SetPrincipalNoDemand(this.CreateRolePrincipalWithAssert(context.User.Identity, encryptedTicket));
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        if (context.Request.Cookies[Roles.CookieName] != null)
                        {
                            Roles.DeleteCookie();
                        }
                        if (HttpRuntime.UseIntegratedPipeline)
                        {
                            context.DisableNotifications(RequestNotification.EndRequest, 0);
                        }
                    }
                }
                if (!(context.User is RolePrincipal))
                {
                    context.SetPrincipalNoDemand(this.CreateRolePrincipalWithAssert(context.User.Identity, null));
                }
                HttpApplication.SetCurrentPrincipalWithAssert(context.User);
            }
        }

        private void OnLeave(object source, EventArgs eventArgs)
        {
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            if (((Roles.Enabled && Roles.CacheRolesInCookie) && !context.Response.HeadersWritten) && (((context.User != null) && (context.User is RolePrincipal)) && context.User.Identity.IsAuthenticated))
            {
                if (Roles.CookieRequireSSL && !context.Request.IsSecureConnection)
                {
                    if (context.Request.Cookies[Roles.CookieName] != null)
                    {
                        Roles.DeleteCookie();
                    }
                }
                else
                {
                    RolePrincipal user = (RolePrincipal) context.User;
                    if (user.CachedListChanged && context.Request.Browser.Cookies)
                    {
                        string str = user.ToEncryptedTicket();
                        if (string.IsNullOrEmpty(str) || (str.Length > 0x1000))
                        {
                            Roles.DeleteCookie();
                        }
                        else
                        {
                            HttpCookie cookie = new HttpCookie(Roles.CookieName, str) {
                                HttpOnly = true,
                                Path = Roles.CookiePath,
                                Domain = Roles.Domain
                            };
                            if (Roles.CreatePersistentCookie)
                            {
                                cookie.Expires = user.ExpireDate;
                            }
                            cookie.Secure = Roles.CookieRequireSSL;
                            context.Response.Cookies.Add(cookie);
                        }
                    }
                }
            }
        }
    }
}

