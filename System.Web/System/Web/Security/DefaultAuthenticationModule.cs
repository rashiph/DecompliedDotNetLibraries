namespace System.Web.Security
{
    using System;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;

    public sealed class DefaultAuthenticationModule : IHttpModule
    {
        private DefaultAuthenticationEventHandler _eventHandler;

        public event DefaultAuthenticationEventHandler Authenticate
        {
            add
            {
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Method_Not_Supported_By_Iis_Integrated_Mode", new object[] { "DefaultAuthentication.Authenticate" }));
                }
                this._eventHandler = (DefaultAuthenticationEventHandler) Delegate.Combine(this._eventHandler, value);
            }
            remove
            {
                this._eventHandler = (DefaultAuthenticationEventHandler) Delegate.Remove(this._eventHandler, value);
            }
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        internal static DefaultAuthenticationModule CreateDefaultAuthenticationModuleWithAssert()
        {
            return new DefaultAuthenticationModule();
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication app)
        {
            if (HttpRuntime.UseIntegratedPipeline)
            {
                app.PostAuthenticateRequest += new EventHandler(this.OnEnter);
            }
            else
            {
                app.DefaultAuthentication += new EventHandler(this.OnEnter);
            }
        }

        private void OnAuthenticate(DefaultAuthenticationEventArgs e)
        {
            if (this._eventHandler != null)
            {
                this._eventHandler(this, e);
            }
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal=true)]
        private void OnEnter(object source, EventArgs eventArgs)
        {
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            if (context.Response.StatusCode > 200)
            {
                if (context.Response.StatusCode == 0x191)
                {
                    this.WriteErrorMessage(context);
                }
                application.CompleteRequest();
            }
            else
            {
                if (context.User == null)
                {
                    this.OnAuthenticate(new DefaultAuthenticationEventArgs(context));
                    if (context.Response.StatusCode > 200)
                    {
                        if (context.Response.StatusCode == 0x191)
                        {
                            this.WriteErrorMessage(context);
                        }
                        application.CompleteRequest();
                        return;
                    }
                }
                if (context.User == null)
                {
                    context.SetPrincipalNoDemand(new GenericPrincipal(new GenericIdentity(string.Empty, string.Empty), new string[0]), false);
                }
                Thread.CurrentPrincipal = context.User;
            }
        }

        private void WriteErrorMessage(HttpContext context)
        {
            context.Response.Write(AuthFailedErrorFormatter.GetErrorText());
            context.Response.GenerateResponseHeadersForHandler();
        }
    }
}

