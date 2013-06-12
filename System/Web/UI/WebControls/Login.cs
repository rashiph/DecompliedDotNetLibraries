namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Security;
    using System.Web.UI;

    [DefaultEvent("Authenticate"), Designer("System.Web.UI.Design.WebControls.LoginDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false)]
    public class Login : CompositeControl, IBorderPaddingControl, IRenderOuterTableControl
    {
        private TableItemStyle _checkBoxStyle;
        private bool _convertingToTemplate;
        private const string _createUserLinkID = "CreateUserLink";
        private const string _failureParameterName = "loginfailure";
        private const string _failureTextID = "FailureText";
        private TableItemStyle _failureTextStyle;
        private const string _helpLinkID = "HelpLink";
        private TableItemStyle _hyperLinkStyle;
        private const string _imageButtonID = "LoginImageButton";
        private TableItemStyle _instructionTextStyle;
        private TableItemStyle _labelStyle;
        private const string _linkButtonID = "LoginLinkButton";
        private Style _loginButtonStyle;
        private ITemplate _loginTemplate;
        private string _password;
        private const string _passwordID = "Password";
        private const string _passwordRecoveryLinkID = "PasswordRecoveryLink";
        private const string _passwordRequiredID = "PasswordRequired";
        private const string _pushButtonID = "LoginButton";
        private const string _rememberMeID = "RememberMe";
        private bool _renderDesignerRegion;
        private const ValidatorDisplay _requiredFieldValidatorDisplay = ValidatorDisplay.Static;
        private LoginContainer _templateContainer;
        private Style _textBoxStyle;
        private TableItemStyle _titleTextStyle;
        private const string _userNameID = "UserName";
        private const string _userNameRequiredID = "UserNameRequired";
        private Style _validatorTextStyle;
        private const int _viewStateArrayLength = 10;
        private static readonly object EventAuthenticate = new object();
        private static readonly object EventLoggedIn = new object();
        private static readonly object EventLoggingIn = new object();
        private static readonly object EventLoginError = new object();
        public static readonly string LoginButtonCommandName = "Login";

        [WebCategory("Action"), WebSysDescription("Login_Authenticate")]
        public event AuthenticateEventHandler Authenticate
        {
            add
            {
                base.Events.AddHandler(EventAuthenticate, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAuthenticate, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("Login_LoggedIn")]
        public event EventHandler LoggedIn
        {
            add
            {
                base.Events.AddHandler(EventLoggedIn, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLoggedIn, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("Login_LoggingIn")]
        public event LoginCancelEventHandler LoggingIn
        {
            add
            {
                base.Events.AddHandler(EventLoggingIn, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLoggingIn, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("Login_LoginError")]
        public event EventHandler LoginError
        {
            add
            {
                base.Events.AddHandler(EventLoginError, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLoginError, value);
            }
        }

        private void AttemptLogin()
        {
            if ((this.Page == null) || this.Page.IsValid)
            {
                LoginCancelEventArgs e = new LoginCancelEventArgs();
                this.OnLoggingIn(e);
                if (!e.Cancel)
                {
                    AuthenticateEventArgs args2 = new AuthenticateEventArgs();
                    this.OnAuthenticate(args2);
                    if (args2.Authenticated)
                    {
                        FormsAuthentication.SetAuthCookie(this.UserNameInternal, this.RememberMeSet);
                        this.OnLoggedIn(EventArgs.Empty);
                        this.Page.Response.Redirect(this.GetRedirectUrl(), false);
                    }
                    else
                    {
                        this.OnLoginError(EventArgs.Empty);
                        if (this.FailureAction == LoginFailureAction.RedirectToLoginPage)
                        {
                            FormsAuthentication.RedirectToLoginPage("loginfailure=1");
                        }
                        ITextControl failureTextLabel = (ITextControl) this.TemplateContainer.FailureTextLabel;
                        if (failureTextLabel != null)
                        {
                            failureTextLabel.Text = this.FailureText;
                        }
                    }
                }
            }
        }

        private void AuthenticateUsingMembershipProvider(AuthenticateEventArgs e)
        {
            e.Authenticated = LoginUtil.GetProvider(this.MembershipProvider).ValidateUser(this.UserNameInternal, this.PasswordInternal);
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            this._templateContainer = new LoginContainer(this);
            this._templateContainer.RenderDesignerRegion = this._renderDesignerRegion;
            ITemplate layoutTemplate = this.LayoutTemplate;
            if (layoutTemplate == null)
            {
                this._templateContainer.EnableViewState = false;
                this._templateContainer.EnableTheming = false;
                layoutTemplate = new LoginTemplate(this);
            }
            layoutTemplate.InstantiateIn(this._templateContainer);
            this._templateContainer.Visible = true;
            this.Controls.Add(this._templateContainer);
            this.SetEditableChildProperties();
            IEditableTextControl userNameTextBox = this._templateContainer.UserNameTextBox as IEditableTextControl;
            if (userNameTextBox != null)
            {
                userNameTextBox.TextChanged += new EventHandler(this.UserNameTextChanged);
            }
            IEditableTextControl passwordTextBox = this._templateContainer.PasswordTextBox as IEditableTextControl;
            if (passwordTextBox != null)
            {
                passwordTextBox.TextChanged += new EventHandler(this.PasswordTextChanged);
            }
            ICheckBoxControl rememberMeCheckBox = (ICheckBoxControl) this._templateContainer.RememberMeCheckBox;
            if (rememberMeCheckBox != null)
            {
                rememberMeCheckBox.CheckedChanged += new EventHandler(this.RememberMeCheckedChanged);
            }
        }

        private string GetRedirectUrl()
        {
            if (this.OnLoginPage())
            {
                string returnUrl = FormsAuthentication.GetReturnUrl(false);
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return returnUrl;
                }
                string str2 = this.DestinationPageUrl;
                if (!string.IsNullOrEmpty(str2))
                {
                    return base.ResolveClientUrl(str2);
                }
                return FormsAuthentication.DefaultUrl;
            }
            string destinationPageUrl = this.DestinationPageUrl;
            if (!string.IsNullOrEmpty(destinationPageUrl))
            {
                return base.ResolveClientUrl(destinationPageUrl);
            }
            if ((this.Page.Form != null) && string.Equals(this.Page.Form.Method, "get", StringComparison.OrdinalIgnoreCase))
            {
                return this.Page.Request.ClientFilePath.VirtualPathString;
            }
            return this.Page.Request.RawUrl;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadViewState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 10)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.LoginButtonStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.LabelStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.TextBoxStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.HyperLinkStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.InstructionTextStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.TitleTextStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.CheckBoxStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.FailureTextStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.ValidatorTextStyle).LoadViewState(objArray[9]);
                }
            }
        }

        protected virtual void OnAuthenticate(AuthenticateEventArgs e)
        {
            AuthenticateEventHandler handler = (AuthenticateEventHandler) base.Events[EventAuthenticate];
            if (handler != null)
            {
                handler(this, e);
            }
            else
            {
                this.AuthenticateUsingMembershipProvider(e);
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            bool flag = false;
            if (e is CommandEventArgs)
            {
                CommandEventArgs args = (CommandEventArgs) e;
                if (string.Equals(args.CommandName, LoginButtonCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    this.AttemptLogin();
                    flag = true;
                }
            }
            return flag;
        }

        protected virtual void OnLoggedIn(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventLoggedIn];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLoggingIn(LoginCancelEventArgs e)
        {
            LoginCancelEventHandler handler = (LoginCancelEventHandler) base.Events[EventLoggingIn];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLoginError(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventLoginError];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool OnLoginPage()
        {
            return AuthenticationConfig.AccessingLoginPage(this.Context, FormsAuthentication.LoginUrl);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.SetEditableChildProperties();
            this.TemplateContainer.Visible = (this.VisibleWhenLoggedIn || !this.Page.Request.IsAuthenticated) || this.OnLoginPage();
        }

        private void PasswordTextChanged(object source, EventArgs e)
        {
            this._password = ((ITextControl) source).Text;
        }

        private bool RedirectedFromFailedLogin()
        {
            return ((!base.DesignMode && (this.Page != null)) && (!this.Page.IsPostBack && (this.Page.Request.QueryString["loginfailure"] != null)));
        }

        private void RememberMeCheckedChanged(object source, EventArgs e)
        {
            this.RememberMeSet = ((ICheckBoxControl) source).Checked;
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            if (base.DesignMode)
            {
                base.ChildControlsCreated = false;
                this.EnsureChildControls();
            }
            if (this.TemplateContainer.Visible)
            {
                this.SetChildProperties();
                this.RenderContents(writer);
            }
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._loginButtonStyle != null) ? ((IStateManager) this._loginButtonStyle).SaveViewState() : null, (this._labelStyle != null) ? ((IStateManager) this._labelStyle).SaveViewState() : null, (this._textBoxStyle != null) ? ((IStateManager) this._textBoxStyle).SaveViewState() : null, (this._hyperLinkStyle != null) ? ((IStateManager) this._hyperLinkStyle).SaveViewState() : null, (this._instructionTextStyle != null) ? ((IStateManager) this._instructionTextStyle).SaveViewState() : null, (this._titleTextStyle != null) ? ((IStateManager) this._titleTextStyle).SaveViewState() : null, (this._checkBoxStyle != null) ? ((IStateManager) this._checkBoxStyle).SaveViewState() : null, (this._failureTextStyle != null) ? ((IStateManager) this._failureTextStyle).SaveViewState() : null, (this._validatorTextStyle != null) ? ((IStateManager) this._validatorTextStyle).SaveViewState() : null };
            for (int i = 0; i < 10; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        internal void SetChildProperties()
        {
            this.SetCommonChildProperties();
            if (this.LayoutTemplate == null)
            {
                this.SetDefaultTemplateChildProperties();
            }
        }

        private void SetCommonChildProperties()
        {
            LoginContainer templateContainer = this.TemplateContainer;
            Util.CopyBaseAttributesToInnerControl(this, templateContainer);
            templateContainer.ApplyStyle(base.ControlStyle);
            ITextControl failureTextLabel = (ITextControl) templateContainer.FailureTextLabel;
            string failureText = this.FailureText;
            if (((failureTextLabel != null) && (failureText.Length > 0)) && this.RedirectedFromFailedLogin())
            {
                failureTextLabel.Text = failureText;
            }
        }

        private void SetDefaultTemplateChildProperties()
        {
            LoginContainer templateContainer = this.TemplateContainer;
            templateContainer.BorderTable.CellPadding = this.BorderPadding;
            templateContainer.BorderTable.CellSpacing = 0;
            Literal title = templateContainer.Title;
            string titleText = this.TitleText;
            if (titleText.Length > 0)
            {
                title.Text = titleText;
                if (this._titleTextStyle != null)
                {
                    LoginUtil.SetTableCellStyle(title, this.TitleTextStyle);
                }
                LoginUtil.SetTableCellVisible(title, true);
            }
            else
            {
                LoginUtil.SetTableCellVisible(title, false);
            }
            Literal instruction = templateContainer.Instruction;
            string instructionText = this.InstructionText;
            if (instructionText.Length > 0)
            {
                instruction.Text = instructionText;
                if (this._instructionTextStyle != null)
                {
                    LoginUtil.SetTableCellStyle(instruction, this.InstructionTextStyle);
                }
                LoginUtil.SetTableCellVisible(instruction, true);
            }
            else
            {
                LoginUtil.SetTableCellVisible(instruction, false);
            }
            Control userNameLabel = templateContainer.UserNameLabel;
            string userNameLabelText = this.UserNameLabelText;
            if (userNameLabelText.Length > 0)
            {
                ((ITextControl) userNameLabel).Text = userNameLabelText;
                if (this._labelStyle != null)
                {
                    LoginUtil.SetTableCellStyle(userNameLabel, this.LabelStyle);
                }
                userNameLabel.Visible = true;
            }
            else
            {
                userNameLabel.Visible = false;
            }
            WebControl userNameTextBox = (WebControl) templateContainer.UserNameTextBox;
            if (this._textBoxStyle != null)
            {
                userNameTextBox.ApplyStyle(this.TextBoxStyle);
            }
            userNameTextBox.TabIndex = this.TabIndex;
            userNameTextBox.AccessKey = this.AccessKey;
            bool flag = true;
            RequiredFieldValidator userNameRequired = templateContainer.UserNameRequired;
            userNameRequired.ErrorMessage = this.UserNameRequiredErrorMessage;
            userNameRequired.ToolTip = this.UserNameRequiredErrorMessage;
            userNameRequired.Enabled = flag;
            userNameRequired.Visible = flag;
            if (this._validatorTextStyle != null)
            {
                userNameRequired.ApplyStyle(this._validatorTextStyle);
            }
            Control passwordLabel = templateContainer.PasswordLabel;
            string passwordLabelText = this.PasswordLabelText;
            if (passwordLabelText.Length > 0)
            {
                ((ITextControl) passwordLabel).Text = passwordLabelText;
                if (this._labelStyle != null)
                {
                    LoginUtil.SetTableCellStyle(passwordLabel, this.LabelStyle);
                }
                passwordLabel.Visible = true;
            }
            else
            {
                passwordLabel.Visible = false;
            }
            WebControl passwordTextBox = (WebControl) templateContainer.PasswordTextBox;
            if (this._textBoxStyle != null)
            {
                passwordTextBox.ApplyStyle(this.TextBoxStyle);
            }
            passwordTextBox.TabIndex = this.TabIndex;
            RequiredFieldValidator passwordRequired = templateContainer.PasswordRequired;
            passwordRequired.ErrorMessage = this.PasswordRequiredErrorMessage;
            passwordRequired.ToolTip = this.PasswordRequiredErrorMessage;
            passwordRequired.Enabled = flag;
            passwordRequired.Visible = flag;
            if (this._validatorTextStyle != null)
            {
                passwordRequired.ApplyStyle(this._validatorTextStyle);
            }
            CheckBox rememberMeCheckBox = (CheckBox) templateContainer.RememberMeCheckBox;
            if (this.DisplayRememberMe)
            {
                rememberMeCheckBox.Text = this.RememberMeText;
                if (this._checkBoxStyle != null)
                {
                    LoginUtil.SetTableCellStyle(rememberMeCheckBox, this.CheckBoxStyle);
                }
                LoginUtil.SetTableCellVisible(rememberMeCheckBox, true);
            }
            else
            {
                LoginUtil.SetTableCellVisible(rememberMeCheckBox, false);
            }
            rememberMeCheckBox.TabIndex = this.TabIndex;
            LinkButton linkButton = templateContainer.LinkButton;
            ImageButton imageButton = templateContainer.ImageButton;
            Button pushButton = templateContainer.PushButton;
            WebControl control5 = null;
            switch (this.LoginButtonType)
            {
                case ButtonType.Button:
                    pushButton.Text = this.LoginButtonText;
                    control5 = pushButton;
                    break;

                case ButtonType.Image:
                    imageButton.ImageUrl = this.LoginButtonImageUrl;
                    imageButton.AlternateText = this.LoginButtonText;
                    control5 = imageButton;
                    break;

                case ButtonType.Link:
                    linkButton.Text = this.LoginButtonText;
                    control5 = linkButton;
                    break;
            }
            linkButton.Visible = false;
            imageButton.Visible = false;
            pushButton.Visible = false;
            control5.Visible = true;
            control5.TabIndex = this.TabIndex;
            if (this._loginButtonStyle != null)
            {
                control5.ApplyStyle(this.LoginButtonStyle);
            }
            Image createUserIcon = templateContainer.CreateUserIcon;
            HyperLink createUserLink = templateContainer.CreateUserLink;
            LiteralControl createUserLinkSeparator = templateContainer.CreateUserLinkSeparator;
            HyperLink passwordRecoveryLink = templateContainer.PasswordRecoveryLink;
            Image passwordRecoveryIcon = templateContainer.PasswordRecoveryIcon;
            HyperLink helpPageLink = templateContainer.HelpPageLink;
            Image helpPageIcon = templateContainer.HelpPageIcon;
            LiteralControl passwordRecoveryLinkSeparator = templateContainer.PasswordRecoveryLinkSeparator;
            string createUserText = this.CreateUserText;
            string createUserIconUrl = this.CreateUserIconUrl;
            string passwordRecoveryText = this.PasswordRecoveryText;
            string passwordRecoveryIconUrl = this.PasswordRecoveryIconUrl;
            string helpPageText = this.HelpPageText;
            string helpPageIconUrl = this.HelpPageIconUrl;
            bool flag2 = createUserText.Length > 0;
            bool flag3 = passwordRecoveryText.Length > 0;
            bool flag4 = helpPageText.Length > 0;
            bool flag5 = helpPageIconUrl.Length > 0;
            bool flag6 = createUserIconUrl.Length > 0;
            bool flag7 = passwordRecoveryIconUrl.Length > 0;
            bool flag8 = flag4 || flag5;
            bool flag9 = flag2 || flag6;
            bool flag10 = flag3 || flag7;
            helpPageLink.Visible = flag4;
            passwordRecoveryLinkSeparator.Visible = flag8 && (flag10 || flag9);
            if (flag4)
            {
                helpPageLink.Text = helpPageText;
                helpPageLink.NavigateUrl = this.HelpPageUrl;
                helpPageLink.TabIndex = this.TabIndex;
            }
            helpPageIcon.Visible = flag5;
            if (flag5)
            {
                helpPageIcon.ImageUrl = helpPageIconUrl;
                helpPageIcon.AlternateText = this.HelpPageText;
            }
            createUserLink.Visible = flag2;
            createUserLinkSeparator.Visible = flag9 && flag10;
            if (flag2)
            {
                createUserLink.Text = createUserText;
                createUserLink.NavigateUrl = this.CreateUserUrl;
                createUserLink.TabIndex = this.TabIndex;
            }
            createUserIcon.Visible = flag6;
            if (flag6)
            {
                createUserIcon.ImageUrl = createUserIconUrl;
                createUserIcon.AlternateText = this.CreateUserText;
            }
            passwordRecoveryLink.Visible = flag3;
            if (flag3)
            {
                passwordRecoveryLink.Text = passwordRecoveryText;
                passwordRecoveryLink.NavigateUrl = this.PasswordRecoveryUrl;
                passwordRecoveryLink.TabIndex = this.TabIndex;
            }
            passwordRecoveryIcon.Visible = flag7;
            if (flag7)
            {
                passwordRecoveryIcon.ImageUrl = passwordRecoveryIconUrl;
                passwordRecoveryIcon.AlternateText = this.PasswordRecoveryText;
            }
            if ((flag9 || flag10) || flag8)
            {
                if (this._hyperLinkStyle != null)
                {
                    TableItemStyle style = new TableItemStyle();
                    style.CopyFrom(this.HyperLinkStyle);
                    style.Font.Reset();
                    LoginUtil.SetTableCellStyle(createUserLink, style);
                    createUserLink.Font.CopyFrom(this.HyperLinkStyle.Font);
                    createUserLink.ForeColor = this.HyperLinkStyle.ForeColor;
                    passwordRecoveryLink.Font.CopyFrom(this.HyperLinkStyle.Font);
                    passwordRecoveryLink.ForeColor = this.HyperLinkStyle.ForeColor;
                    helpPageLink.Font.CopyFrom(this.HyperLinkStyle.Font);
                    helpPageLink.ForeColor = this.HyperLinkStyle.ForeColor;
                }
                LoginUtil.SetTableCellVisible(helpPageLink, true);
            }
            else
            {
                LoginUtil.SetTableCellVisible(helpPageLink, false);
            }
            Control failureTextLabel = templateContainer.FailureTextLabel;
            if (((ITextControl) failureTextLabel).Text.Length > 0)
            {
                LoginUtil.SetTableCellStyle(failureTextLabel, this.FailureTextStyle);
                LoginUtil.SetTableCellVisible(failureTextLabel, true);
            }
            else
            {
                LoginUtil.SetTableCellVisible(failureTextLabel, false);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override void SetDesignModeState(IDictionary data)
        {
            if (data != null)
            {
                object obj2 = data["ConvertToTemplate"];
                if (obj2 != null)
                {
                    this._convertingToTemplate = (bool) obj2;
                }
                obj2 = data["RegionEditing"];
                if (obj2 != null)
                {
                    this._renderDesignerRegion = (bool) obj2;
                }
            }
        }

        private void SetEditableChildProperties()
        {
            LoginContainer templateContainer = this.TemplateContainer;
            string userNameInternal = this.UserNameInternal;
            if (!string.IsNullOrEmpty(userNameInternal))
            {
                ITextControl userNameTextBox = (ITextControl) templateContainer.UserNameTextBox;
                if (userNameTextBox != null)
                {
                    userNameTextBox.Text = userNameInternal;
                }
            }
            ICheckBoxControl rememberMeCheckBox = (ICheckBoxControl) templateContainer.RememberMeCheckBox;
            if (rememberMeCheckBox != null)
            {
                if (this.LayoutTemplate == null)
                {
                    LoginUtil.SetTableCellVisible(templateContainer.RememberMeCheckBox, this.DisplayRememberMe);
                }
                rememberMeCheckBox.Checked = this.RememberMeSet;
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._loginButtonStyle != null)
            {
                ((IStateManager) this._loginButtonStyle).TrackViewState();
            }
            if (this._labelStyle != null)
            {
                ((IStateManager) this._labelStyle).TrackViewState();
            }
            if (this._textBoxStyle != null)
            {
                ((IStateManager) this._textBoxStyle).TrackViewState();
            }
            if (this._hyperLinkStyle != null)
            {
                ((IStateManager) this._hyperLinkStyle).TrackViewState();
            }
            if (this._instructionTextStyle != null)
            {
                ((IStateManager) this._instructionTextStyle).TrackViewState();
            }
            if (this._titleTextStyle != null)
            {
                ((IStateManager) this._titleTextStyle).TrackViewState();
            }
            if (this._checkBoxStyle != null)
            {
                ((IStateManager) this._checkBoxStyle).TrackViewState();
            }
            if (this._failureTextStyle != null)
            {
                ((IStateManager) this._failureTextStyle).TrackViewState();
            }
            if (this._validatorTextStyle != null)
            {
                ((IStateManager) this._validatorTextStyle).TrackViewState();
            }
        }

        private void UserNameTextChanged(object source, EventArgs e)
        {
            this.UserName = ((ITextControl) source).Text;
        }

        [WebSysDescription("Login_BorderPadding"), WebCategory("Appearance"), DefaultValue(1)]
        public virtual int BorderPadding
        {
            get
            {
                object obj2 = this.ViewState["BorderPadding"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 1;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("Login_InvalidBorderPadding"));
                }
                this.ViewState["BorderPadding"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), DefaultValue((string) null), WebSysDescription("Login_CheckBoxStyle"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle CheckBoxStyle
        {
            get
            {
                if (this._checkBoxStyle == null)
                {
                    this._checkBoxStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._checkBoxStyle).TrackViewState();
                    }
                }
                return this._checkBoxStyle;
            }
        }

        private bool ConvertingToTemplate
        {
            get
            {
                return (base.DesignMode && this._convertingToTemplate);
            }
        }

        [UrlProperty, WebCategory("Links"), DefaultValue(""), WebSysDescription("Login_CreateUserIconUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string CreateUserIconUrl
        {
            get
            {
                object obj2 = this.ViewState["CreateUserIconUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CreateUserIconUrl"] = value;
            }
        }

        [WebSysDescription("ChangePassword_CreateUserText"), Localizable(true), WebCategory("Links"), DefaultValue("")]
        public virtual string CreateUserText
        {
            get
            {
                object obj2 = this.ViewState["CreateUserText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CreateUserText"] = value;
            }
        }

        [WebSysDescription("Login_CreateUserUrl"), UrlProperty, WebCategory("Links"), DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string CreateUserUrl
        {
            get
            {
                object obj2 = this.ViewState["CreateUserUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CreateUserUrl"] = value;
            }
        }

        [Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Behavior"), DefaultValue(""), WebSysDescription("Login_DestinationPageUrl"), Themeable(false), UrlProperty]
        public virtual string DestinationPageUrl
        {
            get
            {
                object obj2 = this.ViewState["DestinationPageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DestinationPageUrl"] = value;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("Login_DisplayRememberMe"), DefaultValue(true), Themeable(false)]
        public virtual bool DisplayRememberMe
        {
            get
            {
                object obj2 = this.ViewState["DisplayRememberMe"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["DisplayRememberMe"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Behavior"), Themeable(false), WebSysDescription("Login_FailureAction")]
        public virtual LoginFailureAction FailureAction
        {
            get
            {
                object obj2 = this.ViewState["FailureAction"];
                if (obj2 != null)
                {
                    return (LoginFailureAction) obj2;
                }
                return LoginFailureAction.Refresh;
            }
            set
            {
                if ((value < LoginFailureAction.Refresh) || (value > LoginFailureAction.RedirectToLoginPage))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["FailureAction"] = value;
            }
        }

        [WebSysDefaultValue("Login_DefaultFailureText"), Localizable(true), WebCategory("Appearance"), WebSysDescription("Login_FailureText")]
        public virtual string FailureText
        {
            get
            {
                object obj2 = this.ViewState["FailureText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Login_DefaultFailureText");
            }
            set
            {
                this.ViewState["FailureText"] = value;
            }
        }

        [WebCategory("Styles"), WebSysDescription("WebControl_FailureTextStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FailureTextStyle
        {
            get
            {
                if (this._failureTextStyle == null)
                {
                    this._failureTextStyle = new ErrorTableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._failureTextStyle).TrackViewState();
                    }
                }
                return this._failureTextStyle;
            }
        }

        [DefaultValue(""), UrlProperty, WebCategory("Links"), WebSysDescription("Login_HelpPageIconUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string HelpPageIconUrl
        {
            get
            {
                object obj2 = this.ViewState["HelpPageIconUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HelpPageIconUrl"] = value;
            }
        }

        [WebSysDescription("ChangePassword_HelpPageText"), Localizable(true), WebCategory("Links"), DefaultValue("")]
        public virtual string HelpPageText
        {
            get
            {
                object obj2 = this.ViewState["HelpPageText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HelpPageText"] = value;
            }
        }

        [WebSysDescription("LoginControls_HelpPageUrl"), UrlProperty, WebCategory("Links"), DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string HelpPageUrl
        {
            get
            {
                object obj2 = this.ViewState["HelpPageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HelpPageUrl"] = value;
            }
        }

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_HyperLinkStyle"), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public TableItemStyle HyperLinkStyle
        {
            get
            {
                if (this._hyperLinkStyle == null)
                {
                    this._hyperLinkStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._hyperLinkStyle).TrackViewState();
                    }
                }
                return this._hyperLinkStyle;
            }
        }

        [WebCategory("Appearance"), Localizable(true), DefaultValue(""), WebSysDescription("WebControl_InstructionText")]
        public virtual string InstructionText
        {
            get
            {
                object obj2 = this.ViewState["InstructionText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["InstructionText"] = value;
            }
        }

        [WebCategory("Styles"), WebSysDescription("WebControl_InstructionTextStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle InstructionTextStyle
        {
            get
            {
                if (this._instructionTextStyle == null)
                {
                    this._instructionTextStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._instructionTextStyle).TrackViewState();
                    }
                }
                return this._instructionTextStyle;
            }
        }

        [NotifyParentProperty(true), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("LoginControls_LabelStyle")]
        public TableItemStyle LabelStyle
        {
            get
            {
                if (this._labelStyle == null)
                {
                    this._labelStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._labelStyle).TrackViewState();
                    }
                }
                return this._labelStyle;
            }
        }

        [Browsable(false), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(Login))]
        public virtual ITemplate LayoutTemplate
        {
            get
            {
                return this._loginTemplate;
            }
            set
            {
                this._loginTemplate = value;
                base.ChildControlsCreated = false;
            }
        }

        [DefaultValue(""), UrlProperty, WebCategory("Appearance"), WebSysDescription("Login_LoginButtonImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string LoginButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["LoginButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["LoginButtonImageUrl"] = value;
            }
        }

        [NotifyParentProperty(true), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Login_LoginButtonStyle")]
        public Style LoginButtonStyle
        {
            get
            {
                if (this._loginButtonStyle == null)
                {
                    this._loginButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._loginButtonStyle).TrackViewState();
                    }
                }
                return this._loginButtonStyle;
            }
        }

        [Localizable(true), WebSysDefaultValue("Login_DefaultLoginButtonText"), WebCategory("Appearance"), WebSysDescription("Login_LoginButtonText")]
        public virtual string LoginButtonText
        {
            get
            {
                object obj2 = this.ViewState["LoginButtonText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Login_DefaultLoginButtonText");
            }
            set
            {
                this.ViewState["LoginButtonText"] = value;
            }
        }

        [WebSysDescription("Login_LoginButtonType"), WebCategory("Appearance"), DefaultValue(0)]
        public virtual ButtonType LoginButtonType
        {
            get
            {
                object obj2 = this.ViewState["LoginButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Button;
            }
            set
            {
                if ((value < ButtonType.Button) || (value > ButtonType.Link))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["LoginButtonType"] = value;
            }
        }

        [WebCategory("Data"), Themeable(false), WebSysDescription("MembershipProvider_Name"), DefaultValue("")]
        public virtual string MembershipProvider
        {
            get
            {
                object obj2 = this.ViewState["MembershipProvider"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["MembershipProvider"] = value;
            }
        }

        [WebSysDescription("Login_Orientation"), WebCategory("Layout"), DefaultValue(1)]
        public virtual System.Web.UI.WebControls.Orientation Orientation
        {
            get
            {
                object obj2 = this.ViewState["Orientation"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.Orientation) obj2;
                }
                return System.Web.UI.WebControls.Orientation.Vertical;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.Orientation.Horizontal) || (value > System.Web.UI.WebControls.Orientation.Vertical))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["Orientation"] = value;
                base.ChildControlsCreated = false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual string Password
        {
            get
            {
                if (this._password != null)
                {
                    return this._password;
                }
                return string.Empty;
            }
        }

        private string PasswordInternal
        {
            get
            {
                string password = this.Password;
                if (string.IsNullOrEmpty(password) && (this._templateContainer != null))
                {
                    ITextControl passwordTextBox = (ITextControl) this._templateContainer.PasswordTextBox;
                    if ((passwordTextBox != null) && (passwordTextBox.Text != null))
                    {
                        return passwordTextBox.Text;
                    }
                }
                return password;
            }
        }

        [WebSysDefaultValue("LoginControls_DefaultPasswordLabelText"), WebSysDescription("LoginControls_PasswordLabelText"), Localizable(true), WebCategory("Appearance")]
        public virtual string PasswordLabelText
        {
            get
            {
                object obj2 = this.ViewState["PasswordLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("LoginControls_DefaultPasswordLabelText");
            }
            set
            {
                this.ViewState["PasswordLabelText"] = value;
            }
        }

        [WebCategory("Links"), DefaultValue(""), WebSysDescription("Login_PasswordRecoveryIconUrl"), UrlProperty, Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string PasswordRecoveryIconUrl
        {
            get
            {
                object obj2 = this.ViewState["PasswordRecoveryIconUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["PasswordRecoveryIconUrl"] = value;
            }
        }

        [WebSysDescription("ChangePassword_PasswordRecoveryText"), DefaultValue(""), Localizable(true), WebCategory("Links")]
        public virtual string PasswordRecoveryText
        {
            get
            {
                object obj2 = this.ViewState["PasswordRecoveryText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["PasswordRecoveryText"] = value;
            }
        }

        [DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("Login_PasswordRecoveryUrl"), WebCategory("Links")]
        public virtual string PasswordRecoveryUrl
        {
            get
            {
                object obj2 = this.ViewState["PasswordRecoveryUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["PasswordRecoveryUrl"] = value;
            }
        }

        [WebCategory("Validation"), Localizable(true), WebSysDescription("Login_PasswordRequiredErrorMessage"), WebSysDefaultValue("Login_DefaultPasswordRequiredErrorMessage")]
        public virtual string PasswordRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["PasswordRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Login_DefaultPasswordRequiredErrorMessage");
            }
            set
            {
                this.ViewState["PasswordRequiredErrorMessage"] = value;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("Login_RememberMeSet"), DefaultValue(false), Themeable(false)]
        public virtual bool RememberMeSet
        {
            get
            {
                object obj2 = this.ViewState["RememberMeSet"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["RememberMeSet"] = value;
            }
        }

        [WebSysDefaultValue("Login_DefaultRememberMeText"), WebCategory("Appearance"), Localizable(true), WebSysDescription("Login_RememberMeText")]
        public virtual string RememberMeText
        {
            get
            {
                object obj2 = this.ViewState["RememberMeText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Login_DefaultRememberMeText");
            }
            set
            {
                this.ViewState["RememberMeText"] = value;
            }
        }

        [WebSysDescription("LoginControls_RenderOuterTable"), WebCategory("Layout"), DefaultValue(true)]
        public virtual bool RenderOuterTable
        {
            get
            {
                object obj2 = this.ViewState["RenderOuterTable"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["RenderOuterTable"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private LoginContainer TemplateContainer
        {
            get
            {
                this.EnsureChildControls();
                return this._templateContainer;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("LoginControls_TextBoxStyle"), WebCategory("Styles"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style TextBoxStyle
        {
            get
            {
                if (this._textBoxStyle == null)
                {
                    this._textBoxStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._textBoxStyle).TrackViewState();
                    }
                }
                return this._textBoxStyle;
            }
        }

        [WebCategory("Layout"), WebSysDescription("LoginControls_TextLayout"), DefaultValue(0)]
        public virtual LoginTextLayout TextLayout
        {
            get
            {
                object obj2 = this.ViewState["TextLayout"];
                if (obj2 != null)
                {
                    return (LoginTextLayout) obj2;
                }
                return LoginTextLayout.TextOnLeft;
            }
            set
            {
                if ((value < LoginTextLayout.TextOnLeft) || (value > LoginTextLayout.TextOnTop))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["TextLayout"] = value;
                base.ChildControlsCreated = false;
            }
        }

        [Localizable(true), WebSysDescription("LoginControls_TitleText"), WebCategory("Appearance"), WebSysDefaultValue("Login_DefaultTitleText")]
        public virtual string TitleText
        {
            get
            {
                object obj2 = this.ViewState["TitleText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Login_DefaultTitleText");
            }
            set
            {
                this.ViewState["TitleText"] = value;
            }
        }

        [NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("LoginControls_TitleTextStyle"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null)]
        public TableItemStyle TitleTextStyle
        {
            get
            {
                if (this._titleTextStyle == null)
                {
                    this._titleTextStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._titleTextStyle).TrackViewState();
                    }
                }
                return this._titleTextStyle;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("UserName_InitialValue"), DefaultValue("")]
        public virtual string UserName
        {
            get
            {
                object obj2 = this.ViewState["UserName"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["UserName"] = value;
            }
        }

        private string UserNameInternal
        {
            get
            {
                string userName = this.UserName;
                if (string.IsNullOrEmpty(userName) && (this._templateContainer != null))
                {
                    ITextControl userNameTextBox = (ITextControl) this._templateContainer.UserNameTextBox;
                    if ((userNameTextBox != null) && (userNameTextBox.Text != null))
                    {
                        return userNameTextBox.Text;
                    }
                }
                return userName;
            }
        }

        [WebSysDescription("LoginControls_UserNameLabelText"), WebSysDefaultValue("Login_DefaultUserNameLabelText"), Localizable(true), WebCategory("Appearance")]
        public virtual string UserNameLabelText
        {
            get
            {
                object obj2 = this.ViewState["UserNameLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Login_DefaultUserNameLabelText");
            }
            set
            {
                this.ViewState["UserNameLabelText"] = value;
            }
        }

        [WebSysDescription("ChangePassword_UserNameRequiredErrorMessage"), Localizable(true), WebCategory("Validation"), WebSysDefaultValue("Login_DefaultUserNameRequiredErrorMessage")]
        public virtual string UserNameRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["UserNameRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Login_DefaultUserNameRequiredErrorMessage");
            }
            set
            {
                this.ViewState["UserNameRequiredErrorMessage"] = value;
            }
        }

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Login_ValidatorTextStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebCategory("Styles")]
        public Style ValidatorTextStyle
        {
            get
            {
                if (this._validatorTextStyle == null)
                {
                    this._validatorTextStyle = new ErrorStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._validatorTextStyle).TrackViewState();
                    }
                }
                return this._validatorTextStyle;
            }
        }

        [DefaultValue(true), WebCategory("Behavior"), Themeable(false), WebSysDescription("Login_VisibleWhenLoggedIn")]
        public virtual bool VisibleWhenLoggedIn
        {
            get
            {
                object obj2 = this.ViewState["VisibleWhenLoggedIn"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["VisibleWhenLoggedIn"] = value;
            }
        }

        internal sealed class LoginContainer : LoginUtil.GenericContainer<Login>
        {
            private Image _createUserIcon;
            private HyperLink _createUserLink;
            private LiteralControl _createUserLinkSeparator;
            private Control _failureTextLabel;
            private Image _helpPageIcon;
            private HyperLink _helpPageLink;
            private System.Web.UI.WebControls.ImageButton _imageButton;
            private Literal _instruction;
            private System.Web.UI.WebControls.LinkButton _linkButton;
            private LabelLiteral _passwordLabel;
            private Image _passwordRecoveryIcon;
            private HyperLink _passwordRecoveryLink;
            private LiteralControl _passwordRecoveryLinkSeparator;
            private RequiredFieldValidator _passwordRequired;
            private Control _passwordTextBox;
            private Button _pushButton;
            private Control _rememberMeCheckBox;
            private Literal _title;
            private LabelLiteral _userNameLabel;
            private RequiredFieldValidator _userNameRequired;
            private Control _userNameTextBox;

            public LoginContainer(Login owner) : base(owner)
            {
            }

            protected override bool ConvertingToTemplate
            {
                get
                {
                    return base.Owner.ConvertingToTemplate;
                }
            }

            internal Image CreateUserIcon
            {
                get
                {
                    return this._createUserIcon;
                }
                set
                {
                    this._createUserIcon = value;
                }
            }

            internal HyperLink CreateUserLink
            {
                get
                {
                    return this._createUserLink;
                }
                set
                {
                    this._createUserLink = value;
                }
            }

            internal LiteralControl CreateUserLinkSeparator
            {
                get
                {
                    return this._createUserLinkSeparator;
                }
                set
                {
                    this._createUserLinkSeparator = value;
                }
            }

            internal Control FailureTextLabel
            {
                get
                {
                    if (this._failureTextLabel != null)
                    {
                        return this._failureTextLabel;
                    }
                    return base.FindOptionalControl<ITextControl>("FailureText");
                }
                set
                {
                    this._failureTextLabel = value;
                }
            }

            internal Image HelpPageIcon
            {
                get
                {
                    return this._helpPageIcon;
                }
                set
                {
                    this._helpPageIcon = value;
                }
            }

            internal HyperLink HelpPageLink
            {
                get
                {
                    return this._helpPageLink;
                }
                set
                {
                    this._helpPageLink = value;
                }
            }

            internal System.Web.UI.WebControls.ImageButton ImageButton
            {
                get
                {
                    return this._imageButton;
                }
                set
                {
                    this._imageButton = value;
                }
            }

            internal Literal Instruction
            {
                get
                {
                    return this._instruction;
                }
                set
                {
                    this._instruction = value;
                }
            }

            internal System.Web.UI.WebControls.LinkButton LinkButton
            {
                get
                {
                    return this._linkButton;
                }
                set
                {
                    this._linkButton = value;
                }
            }

            internal LabelLiteral PasswordLabel
            {
                get
                {
                    return this._passwordLabel;
                }
                set
                {
                    this._passwordLabel = value;
                }
            }

            internal Image PasswordRecoveryIcon
            {
                get
                {
                    return this._passwordRecoveryIcon;
                }
                set
                {
                    this._passwordRecoveryIcon = value;
                }
            }

            internal HyperLink PasswordRecoveryLink
            {
                get
                {
                    return this._passwordRecoveryLink;
                }
                set
                {
                    this._passwordRecoveryLink = value;
                }
            }

            internal LiteralControl PasswordRecoveryLinkSeparator
            {
                get
                {
                    return this._passwordRecoveryLinkSeparator;
                }
                set
                {
                    this._passwordRecoveryLinkSeparator = value;
                }
            }

            internal RequiredFieldValidator PasswordRequired
            {
                get
                {
                    return this._passwordRequired;
                }
                set
                {
                    this._passwordRequired = value;
                }
            }

            internal Control PasswordTextBox
            {
                get
                {
                    if (this._passwordTextBox != null)
                    {
                        return this._passwordTextBox;
                    }
                    return base.FindRequiredControl<IEditableTextControl>("Password", "Login_NoPasswordTextBox");
                }
                set
                {
                    this._passwordTextBox = value;
                }
            }

            internal Button PushButton
            {
                get
                {
                    return this._pushButton;
                }
                set
                {
                    this._pushButton = value;
                }
            }

            internal Control RememberMeCheckBox
            {
                get
                {
                    if (this._rememberMeCheckBox != null)
                    {
                        return this._rememberMeCheckBox;
                    }
                    return base.FindOptionalControl<ICheckBoxControl>("RememberMe");
                }
                set
                {
                    this._rememberMeCheckBox = value;
                }
            }

            internal Literal Title
            {
                get
                {
                    return this._title;
                }
                set
                {
                    this._title = value;
                }
            }

            internal LabelLiteral UserNameLabel
            {
                get
                {
                    return this._userNameLabel;
                }
                set
                {
                    this._userNameLabel = value;
                }
            }

            internal RequiredFieldValidator UserNameRequired
            {
                get
                {
                    return this._userNameRequired;
                }
                set
                {
                    this._userNameRequired = value;
                }
            }

            internal Control UserNameTextBox
            {
                get
                {
                    if (this._userNameTextBox != null)
                    {
                        return this._userNameTextBox;
                    }
                    return base.FindRequiredControl<IEditableTextControl>("UserName", "Login_NoUserNameTextBox");
                }
                set
                {
                    this._userNameTextBox = value;
                }
            }
        }

        private sealed class LoginTemplate : ITemplate
        {
            private Login _owner;

            public LoginTemplate(Login owner)
            {
                this._owner = owner;
            }

            private void CreateControls(Login.LoginContainer loginContainer)
            {
                string uniqueID = this._owner.UniqueID;
                Literal literal = new Literal();
                loginContainer.Title = literal;
                Literal literal2 = new Literal();
                loginContainer.Instruction = literal2;
                TextBox forControl = new TextBox {
                    ID = "UserName"
                };
                loginContainer.UserNameTextBox = forControl;
                LabelLiteral literal3 = new LabelLiteral(forControl);
                loginContainer.UserNameLabel = literal3;
                bool flag = true;
                RequiredFieldValidator validator = new RequiredFieldValidator {
                    ID = "UserNameRequired",
                    ValidationGroup = uniqueID,
                    ControlToValidate = forControl.ID,
                    Display = ValidatorDisplay.Static,
                    Text = System.Web.SR.GetString("LoginControls_DefaultRequiredFieldValidatorText"),
                    Enabled = flag,
                    Visible = flag
                };
                loginContainer.UserNameRequired = validator;
                TextBox box2 = new TextBox {
                    ID = "Password",
                    TextMode = TextBoxMode.Password
                };
                loginContainer.PasswordTextBox = box2;
                LabelLiteral literal4 = new LabelLiteral(box2);
                loginContainer.PasswordLabel = literal4;
                RequiredFieldValidator validator2 = new RequiredFieldValidator {
                    ID = "PasswordRequired",
                    ValidationGroup = uniqueID,
                    ControlToValidate = box2.ID,
                    Display = ValidatorDisplay.Static,
                    Text = System.Web.SR.GetString("LoginControls_DefaultRequiredFieldValidatorText"),
                    Enabled = flag,
                    Visible = flag
                };
                loginContainer.PasswordRequired = validator2;
                CheckBox box3 = new CheckBox {
                    ID = "RememberMe"
                };
                loginContainer.RememberMeCheckBox = box3;
                LinkButton button = new LinkButton {
                    ID = "LoginLinkButton",
                    ValidationGroup = uniqueID,
                    CommandName = Login.LoginButtonCommandName
                };
                loginContainer.LinkButton = button;
                ImageButton button2 = new ImageButton {
                    ID = "LoginImageButton",
                    ValidationGroup = uniqueID,
                    CommandName = Login.LoginButtonCommandName
                };
                loginContainer.ImageButton = button2;
                Button button3 = new Button {
                    ID = "LoginButton",
                    ValidationGroup = uniqueID,
                    CommandName = Login.LoginButtonCommandName
                };
                loginContainer.PushButton = button3;
                HyperLink link = new HyperLink();
                loginContainer.PasswordRecoveryLink = link;
                LiteralControl control = new LiteralControl();
                link.ID = "PasswordRecoveryLink";
                loginContainer.PasswordRecoveryLinkSeparator = control;
                HyperLink link2 = new HyperLink();
                loginContainer.CreateUserLink = link2;
                link2.ID = "CreateUserLink";
                LiteralControl control2 = new LiteralControl();
                loginContainer.CreateUserLinkSeparator = control2;
                HyperLink link3 = new HyperLink {
                    ID = "HelpLink"
                };
                loginContainer.HelpPageLink = link3;
                Literal literal5 = new Literal {
                    ID = "FailureText"
                };
                loginContainer.FailureTextLabel = literal5;
                loginContainer.PasswordRecoveryIcon = new Image();
                loginContainer.HelpPageIcon = new Image();
                loginContainer.CreateUserIcon = new Image();
            }

            private void LayoutControls(Login.LoginContainer loginContainer)
            {
                Orientation orientation = this._owner.Orientation;
                LoginTextLayout textLayout = this._owner.TextLayout;
                if ((orientation == Orientation.Vertical) && (textLayout == LoginTextLayout.TextOnLeft))
                {
                    this.LayoutVerticalTextOnLeft(loginContainer);
                }
                else if ((orientation == Orientation.Vertical) && (textLayout == LoginTextLayout.TextOnTop))
                {
                    this.LayoutVerticalTextOnTop(loginContainer);
                }
                else if ((orientation == Orientation.Horizontal) && (textLayout == LoginTextLayout.TextOnLeft))
                {
                    this.LayoutHorizontalTextOnLeft(loginContainer);
                }
                else
                {
                    this.LayoutHorizontalTextOnTop(loginContainer);
                }
            }

            private void LayoutHorizontalTextOnLeft(Login.LoginContainer loginContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    ColumnSpan = 6,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 6,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                if (this._owner.ConvertingToTemplate)
                {
                    loginContainer.UserNameLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(loginContainer.UserNameLabel);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(loginContainer.UserNameTextBox);
                cell.Controls.Add(loginContainer.UserNameRequired);
                row.Cells.Add(cell);
                cell = new TableCell();
                if (this._owner.ConvertingToTemplate)
                {
                    loginContainer.PasswordLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(loginContainer.PasswordLabel);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(loginContainer.PasswordTextBox);
                cell.Controls.Add(loginContainer.PasswordRequired);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(loginContainer.RememberMeCheckBox);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(loginContainer.LinkButton);
                cell.Controls.Add(loginContainer.ImageButton);
                cell.Controls.Add(loginContainer.PushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 6
                };
                cell.Controls.Add(loginContainer.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 6
                };
                cell.Controls.Add(loginContainer.CreateUserIcon);
                cell.Controls.Add(loginContainer.CreateUserLink);
                loginContainer.CreateUserLinkSeparator.Text = " ";
                cell.Controls.Add(loginContainer.CreateUserLinkSeparator);
                cell.Controls.Add(loginContainer.PasswordRecoveryIcon);
                cell.Controls.Add(loginContainer.PasswordRecoveryLink);
                loginContainer.PasswordRecoveryLinkSeparator.Text = " ";
                cell.Controls.Add(loginContainer.PasswordRecoveryLinkSeparator);
                cell.Controls.Add(loginContainer.HelpPageIcon);
                cell.Controls.Add(loginContainer.HelpPageLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                loginContainer.LayoutTable = child;
                loginContainer.BorderTable = table2;
                loginContainer.Controls.Add(table2);
            }

            private void LayoutHorizontalTextOnTop(Login.LoginContainer loginContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    ColumnSpan = 4,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 4,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                if (this._owner.ConvertingToTemplate)
                {
                    loginContainer.UserNameLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(loginContainer.UserNameLabel);
                row.Cells.Add(cell);
                cell = new TableCell();
                if (this._owner.ConvertingToTemplate)
                {
                    loginContainer.PasswordLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(loginContainer.PasswordLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(loginContainer.UserNameTextBox);
                cell.Controls.Add(loginContainer.UserNameRequired);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(loginContainer.PasswordTextBox);
                cell.Controls.Add(loginContainer.PasswordRequired);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(loginContainer.RememberMeCheckBox);
                row.Cells.Add(cell);
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(loginContainer.LinkButton);
                cell.Controls.Add(loginContainer.ImageButton);
                cell.Controls.Add(loginContainer.PushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 4
                };
                cell.Controls.Add(loginContainer.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 4
                };
                cell.Controls.Add(loginContainer.CreateUserIcon);
                cell.Controls.Add(loginContainer.CreateUserLink);
                loginContainer.CreateUserLinkSeparator.Text = " ";
                cell.Controls.Add(loginContainer.CreateUserLinkSeparator);
                cell.Controls.Add(loginContainer.PasswordRecoveryIcon);
                cell.Controls.Add(loginContainer.PasswordRecoveryLink);
                loginContainer.PasswordRecoveryLinkSeparator.Text = " ";
                cell.Controls.Add(loginContainer.PasswordRecoveryLinkSeparator);
                cell.Controls.Add(loginContainer.HelpPageIcon);
                cell.Controls.Add(loginContainer.HelpPageLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                loginContainer.LayoutTable = child;
                loginContainer.BorderTable = table2;
                loginContainer.Controls.Add(table2);
            }

            private void LayoutVerticalTextOnLeft(Login.LoginContainer loginContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                if (this._owner.ConvertingToTemplate)
                {
                    loginContainer.UserNameLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(loginContainer.UserNameLabel);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(loginContainer.UserNameTextBox);
                cell.Controls.Add(loginContainer.UserNameRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                if (this._owner.ConvertingToTemplate)
                {
                    loginContainer.PasswordLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(loginContainer.PasswordLabel);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(loginContainer.PasswordTextBox);
                cell.Controls.Add(loginContainer.PasswordRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2
                };
                cell.Controls.Add(loginContainer.RememberMeCheckBox);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(loginContainer.LinkButton);
                cell.Controls.Add(loginContainer.ImageButton);
                cell.Controls.Add(loginContainer.PushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2
                };
                cell.Controls.Add(loginContainer.CreateUserIcon);
                cell.Controls.Add(loginContainer.CreateUserLink);
                cell.Controls.Add(loginContainer.CreateUserLinkSeparator);
                cell.Controls.Add(loginContainer.PasswordRecoveryIcon);
                cell.Controls.Add(loginContainer.PasswordRecoveryLink);
                loginContainer.PasswordRecoveryLinkSeparator.Text = "<br />";
                loginContainer.CreateUserLinkSeparator.Text = "<br />";
                cell.Controls.Add(loginContainer.PasswordRecoveryLinkSeparator);
                cell.Controls.Add(loginContainer.HelpPageIcon);
                cell.Controls.Add(loginContainer.HelpPageLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                loginContainer.LayoutTable = child;
                loginContainer.BorderTable = table2;
                loginContainer.Controls.Add(table2);
            }

            private void LayoutVerticalTextOnTop(Login.LoginContainer loginContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                if (this._owner.ConvertingToTemplate)
                {
                    loginContainer.UserNameLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(loginContainer.UserNameLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(loginContainer.UserNameTextBox);
                cell.Controls.Add(loginContainer.UserNameRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                if (this._owner.ConvertingToTemplate)
                {
                    loginContainer.PasswordLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(loginContainer.PasswordLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(loginContainer.PasswordTextBox);
                cell.Controls.Add(loginContainer.PasswordRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(loginContainer.RememberMeCheckBox);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(loginContainer.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(loginContainer.LinkButton);
                cell.Controls.Add(loginContainer.ImageButton);
                cell.Controls.Add(loginContainer.PushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(loginContainer.CreateUserIcon);
                cell.Controls.Add(loginContainer.CreateUserLink);
                loginContainer.CreateUserLinkSeparator.Text = "<br />";
                cell.Controls.Add(loginContainer.CreateUserLinkSeparator);
                cell.Controls.Add(loginContainer.PasswordRecoveryIcon);
                cell.Controls.Add(loginContainer.PasswordRecoveryLink);
                loginContainer.PasswordRecoveryLinkSeparator.Text = "<br />";
                cell.Controls.Add(loginContainer.PasswordRecoveryLinkSeparator);
                cell.Controls.Add(loginContainer.HelpPageIcon);
                cell.Controls.Add(loginContainer.HelpPageLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                loginContainer.LayoutTable = child;
                loginContainer.BorderTable = table2;
                loginContainer.Controls.Add(table2);
            }

            void ITemplate.InstantiateIn(Control container)
            {
                Login.LoginContainer loginContainer = (Login.LoginContainer) container;
                this.CreateControls(loginContainer);
                this.LayoutControls(loginContainer);
            }
        }
    }
}

