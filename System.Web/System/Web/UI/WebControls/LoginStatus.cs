namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Security;
    using System.Web.UI;

    [DefaultEvent("LoggingOut"), Designer("System.Web.UI.Design.WebControls.LoginStatusDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false)]
    public class LoginStatus : CompositeControl
    {
        private bool _loggedIn;
        private ImageButton _logInImageButton;
        private LinkButton _logInLinkButton;
        private ImageButton _logOutImageButton;
        private LinkButton _logOutLinkButton;
        private static readonly object EventLoggedOut = new object();
        private static readonly object EventLoggingOut = new object();

        [WebSysDescription("LoginStatus_LoggedOut"), WebCategory("Action")]
        public event EventHandler LoggedOut
        {
            add
            {
                base.Events.AddHandler(EventLoggedOut, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLoggedOut, value);
            }
        }

        [WebSysDescription("LoginStatus_LoggingOut"), WebCategory("Action")]
        public event LoginCancelEventHandler LoggingOut
        {
            add
            {
                base.Events.AddHandler(EventLoggingOut, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLoggingOut, value);
            }
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            this._logInLinkButton = new LinkButton();
            this._logInImageButton = new ImageButton();
            this._logOutLinkButton = new LinkButton();
            this._logOutImageButton = new ImageButton();
            this._logInLinkButton.EnableViewState = false;
            this._logInImageButton.EnableViewState = false;
            this._logOutLinkButton.EnableViewState = false;
            this._logOutImageButton.EnableViewState = false;
            this._logInLinkButton.EnableTheming = false;
            this._logInImageButton.EnableTheming = false;
            this._logInLinkButton.CausesValidation = false;
            this._logInImageButton.CausesValidation = false;
            this._logOutLinkButton.EnableTheming = false;
            this._logOutImageButton.EnableTheming = false;
            this._logOutLinkButton.CausesValidation = false;
            this._logOutImageButton.CausesValidation = false;
            CommandEventHandler handler = new CommandEventHandler(this.LogoutClicked);
            this._logOutLinkButton.Command += handler;
            this._logOutImageButton.Command += handler;
            handler = new CommandEventHandler(this.LoginClicked);
            this._logInLinkButton.Command += handler;
            this._logInImageButton.Command += handler;
            this.Controls.Add(this._logOutLinkButton);
            this.Controls.Add(this._logOutImageButton);
            this.Controls.Add(this._logInLinkButton);
            this.Controls.Add(this._logInImageButton);
        }

        private void LoginClicked(object Source, CommandEventArgs e)
        {
            this.Page.Response.Redirect(base.ResolveClientUrl(this.NavigateUrl), false);
        }

        private void LogoutClicked(object Source, CommandEventArgs e)
        {
            LoginCancelEventArgs args = new LoginCancelEventArgs();
            this.OnLoggingOut(args);
            if (!args.Cancel)
            {
                FormsAuthentication.SignOut();
                this.Page.Response.Clear();
                this.Page.Response.StatusCode = 200;
                this.OnLoggedOut(EventArgs.Empty);
                switch (this.LogoutAction)
                {
                    case System.Web.UI.WebControls.LogoutAction.Refresh:
                        if ((this.Page.Form == null) || !string.Equals(this.Page.Form.Method, "get", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Page.Response.Redirect(this.Page.Request.RawUrl, false);
                            return;
                        }
                        this.Page.Response.Redirect(this.Page.Request.ClientFilePath.VirtualPathString, false);
                        return;

                    case System.Web.UI.WebControls.LogoutAction.Redirect:
                    {
                        string logoutPageUrl = this.LogoutPageUrl;
                        if (string.IsNullOrEmpty(logoutPageUrl))
                        {
                            logoutPageUrl = FormsAuthentication.LoginUrl;
                        }
                        else
                        {
                            logoutPageUrl = base.ResolveClientUrl(logoutPageUrl);
                        }
                        this.Page.Response.Redirect(logoutPageUrl, false);
                        return;
                    }
                    case System.Web.UI.WebControls.LogoutAction.RedirectToLoginPage:
                        this.Page.Response.Redirect(FormsAuthentication.LoginUrl, false);
                        return;
                }
            }
        }

        protected virtual void OnLoggedOut(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventLoggedOut];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLoggingOut(LoginCancelEventArgs e)
        {
            LoginCancelEventHandler handler = (LoginCancelEventHandler) base.Events[EventLoggingOut];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.LoggedIn = this.Page.Request.IsAuthenticated;
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            this.SetChildProperties();
            if ((this.ID != null) && (this.ID.Length != 0))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            }
            base.RenderContents(writer);
        }

        private void SetChildProperties()
        {
            this.EnsureChildControls();
            this._logInLinkButton.Visible = false;
            this._logInImageButton.Visible = false;
            this._logOutLinkButton.Visible = false;
            this._logOutImageButton.Visible = false;
            WebControl control = null;
            if (this.LoggedIn)
            {
                string logoutImageUrl = this.LogoutImageUrl;
                if (logoutImageUrl.Length > 0)
                {
                    this._logOutImageButton.AlternateText = this.LogoutText;
                    this._logOutImageButton.ImageUrl = logoutImageUrl;
                    control = this._logOutImageButton;
                }
                else
                {
                    this._logOutLinkButton.Text = this.LogoutText;
                    control = this._logOutLinkButton;
                }
            }
            else
            {
                string loginImageUrl = this.LoginImageUrl;
                if (loginImageUrl.Length > 0)
                {
                    this._logInImageButton.AlternateText = this.LoginText;
                    this._logInImageButton.ImageUrl = loginImageUrl;
                    control = this._logInImageButton;
                }
                else
                {
                    this._logInLinkButton.Text = this.LoginText;
                    control = this._logInLinkButton;
                }
            }
            control.CopyBaseAttributes(this);
            control.ApplyStyle(base.ControlStyle);
            control.Visible = true;
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override void SetDesignModeState(IDictionary data)
        {
            if (data != null)
            {
                object obj2 = data["LoggedIn"];
                if (obj2 != null)
                {
                    this.LoggedIn = (bool) obj2;
                }
            }
        }

        private bool LoggedIn
        {
            get
            {
                return this._loggedIn;
            }
            set
            {
                this._loggedIn = value;
            }
        }

        [UrlProperty, WebSysDescription("LoginStatus_LoginImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Appearance"), DefaultValue("")]
        public virtual string LoginImageUrl
        {
            get
            {
                object obj2 = this.ViewState["LoginImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["LoginImageUrl"] = value;
            }
        }

        [WebSysDefaultValue("LoginStatus_DefaultLoginText"), WebCategory("Appearance"), Localizable(true), WebSysDescription("LoginStatus_LoginText")]
        public virtual string LoginText
        {
            get
            {
                object obj2 = this.ViewState["LoginText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("LoginStatus_DefaultLoginText");
            }
            set
            {
                this.ViewState["LoginText"] = value;
            }
        }

        [WebSysDescription("LoginStatus_LogoutAction"), Themeable(false), WebCategory("Behavior"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.LogoutAction LogoutAction
        {
            get
            {
                object obj2 = this.ViewState["LogoutAction"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.LogoutAction) obj2;
                }
                return System.Web.UI.WebControls.LogoutAction.Refresh;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.LogoutAction.Refresh) || (value > System.Web.UI.WebControls.LogoutAction.RedirectToLoginPage))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["LogoutAction"] = value;
            }
        }

        [DefaultValue(""), UrlProperty, WebCategory("Appearance"), WebSysDescription("LoginStatus_LogoutImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string LogoutImageUrl
        {
            get
            {
                object obj2 = this.ViewState["LogoutImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["LogoutImageUrl"] = value;
            }
        }

        [WebSysDescription("LoginStatus_LogoutPageUrl"), WebCategory("Behavior"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Themeable(false), UrlProperty, DefaultValue("")]
        public virtual string LogoutPageUrl
        {
            get
            {
                object obj2 = this.ViewState["LogoutPageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["LogoutPageUrl"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("LoginStatus_LogoutText"), Localizable(true), WebSysDefaultValue("LoginStatus_DefaultLogoutText")]
        public virtual string LogoutText
        {
            get
            {
                object obj2 = this.ViewState["LogoutText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("LoginStatus_DefaultLogoutText");
            }
            set
            {
                this.ViewState["LogoutText"] = value;
            }
        }

        private string NavigateUrl
        {
            get
            {
                if (!base.DesignMode)
                {
                    return FormsAuthentication.GetLoginPage(null, true);
                }
                return "url";
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.A;
            }
        }
    }
}

