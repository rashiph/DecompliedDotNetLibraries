namespace System.Web.Security
{
    using System;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    public sealed class WindowsAuthenticationModule : IHttpModule
    {
        private static WindowsIdentity _anonymousIdentity;
        private static WindowsPrincipal _anonymousPrincipal;
        private static bool _fAuthChecked;
        private static bool _fAuthRequired;

        public event WindowsAuthenticationEventHandler Authenticate;

        public void Dispose()
        {
        }

        public void Init(HttpApplication app)
        {
            app.AuthenticateRequest += new EventHandler(this.OnEnter);
        }

        private void OnAuthenticate(WindowsAuthenticationEventArgs e)
        {
            if (this._eventHandler != null)
            {
                this._eventHandler(this, e);
            }
            if (e.Context.User == null)
            {
                if (e.User != null)
                {
                    e.Context.User = e.User;
                }
                else if (e.Identity == _anonymousIdentity)
                {
                    e.Context.SetPrincipalNoDemand(_anonymousPrincipal, false);
                }
                else
                {
                    e.Context.SetPrincipalNoDemand(new WindowsPrincipal(e.Identity), false);
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true, ControlPrincipal=true)]
        private void OnEnter(object source, EventArgs eventArgs)
        {
            if (IsEnabled)
            {
                HttpApplication application = (HttpApplication) source;
                HttpContext context = application.Context;
                WindowsIdentity identity = null;
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    WindowsPrincipal user = context.User as WindowsPrincipal;
                    if (user != null)
                    {
                        identity = user.Identity as WindowsIdentity;
                        context.SetPrincipalNoDemand(null, false);
                    }
                }
                else
                {
                    string serverVariable = context.WorkerRequest.GetServerVariable("LOGON_USER");
                    string str2 = context.WorkerRequest.GetServerVariable("AUTH_TYPE");
                    if (serverVariable == null)
                    {
                        serverVariable = string.Empty;
                    }
                    if (str2 == null)
                    {
                        str2 = string.Empty;
                    }
                    if ((serverVariable.Length == 0) && ((str2.Length == 0) || StringUtil.EqualsIgnoreCase(str2, "basic")))
                    {
                        identity = _anonymousIdentity;
                    }
                    else
                    {
                        identity = new WindowsIdentity(context.WorkerRequest.GetUserToken(), str2, WindowsAccountType.Normal, true);
                    }
                }
                if (identity != null)
                {
                    this.OnAuthenticate(new WindowsAuthenticationEventArgs(identity, context));
                }
            }
        }

        internal static IPrincipal AnonymousPrincipal
        {
            get
            {
                return _anonymousPrincipal;
            }
        }

        internal static bool IsEnabled
        {
            get
            {
                if (!_fAuthChecked)
                {
                    _fAuthRequired = AuthenticationConfig.Mode == AuthenticationMode.Windows;
                    if (_fAuthRequired)
                    {
                        _anonymousIdentity = WindowsIdentity.GetAnonymous();
                        _anonymousPrincipal = new WindowsPrincipal(_anonymousIdentity);
                    }
                    _fAuthChecked = true;
                }
                return _fAuthRequired;
            }
        }
    }
}

