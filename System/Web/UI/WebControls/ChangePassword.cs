namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Security;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.ChangePasswordDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false), DefaultEvent("ChangedPassword")]
    public class ChangePassword : CompositeControl, IBorderPaddingControl, INamingContainer, IRenderOuterTableControl
    {
        private Style _cancelButtonStyle;
        private const string _cancelImageButtonID = "CancelImageButton";
        private const string _cancelLinkButtonID = "CancelLinkButton";
        private const string _cancelPushButtonID = "CancelPushButton";
        private Style _changePasswordButtonStyle;
        private ChangePasswordContainer _changePasswordContainer;
        private const string _changePasswordImageButtonID = "ChangePasswordImageButton";
        private const string _changePasswordLinkButtonID = "ChangePasswordLinkButton";
        private const string _changePasswordPushButtonID = "ChangePasswordPushButton";
        private ITemplate _changePasswordTemplate;
        private const string _changePasswordViewContainerID = "ChangePasswordContainerID";
        private const ValidatorDisplay _compareFieldValidatorDisplay = ValidatorDisplay.Dynamic;
        private string _confirmNewPassword;
        private const string _confirmNewPasswordID = "ConfirmNewPassword";
        private const string _confirmNewPasswordRequiredID = "ConfirmNewPasswordRequired";
        private Style _continueButtonStyle;
        private const string _continueImageButtonID = "ContinueImageButton";
        private const string _continueLinkButtonID = "ContinueLinkButton";
        private const string _continuePushButtonID = "ContinuePushButton";
        private bool _convertingToTemplate;
        private const string _createUserLinkID = "CreateUserLink";
        private const string _currentPasswordID = "CurrentPassword";
        private const string _currentPasswordRequiredID = "CurrentPasswordRequired";
        private View _currentView;
        private const string _editProfileLinkID = "EditProfileLink";
        private const string _editProfileSuccessLinkID = "EditProfileLinkSuccess";
        private const string _failureTextID = "FailureText";
        private TableItemStyle _failureTextStyle;
        private const string _helpLinkID = "HelpLink";
        private TableItemStyle _hyperLinkStyle;
        private TableItemStyle _instructionTextStyle;
        private TableItemStyle _labelStyle;
        private System.Web.UI.WebControls.MailDefinition _mailDefinition;
        private string _newPassword;
        private const string _newPasswordCompareID = "NewPasswordCompare";
        private const string _newPasswordID = "NewPassword";
        private const string _newPasswordRegExpID = "NewPasswordRegExp";
        private const string _newPasswordRequiredID = "NewPasswordRequired";
        private string _password;
        private TableItemStyle _passwordHintStyle;
        private Control _passwordHintTableRow;
        private const string _passwordRecoveryLinkID = "PasswordRecoveryLink";
        private const string _passwordReplacementKey = @"<%\s*Password\s*%>";
        private const ValidatorDisplay _regexpFieldValidatorDisplay = ValidatorDisplay.Dynamic;
        private bool _renderDesignerRegion;
        private const ValidatorDisplay _requiredFieldValidatorDisplay = ValidatorDisplay.Static;
        private SuccessContainer _successContainer;
        private ITemplate _successTemplate;
        private TableItemStyle _successTextStyle;
        private const string _successViewContainerID = "SuccessContainerID";
        private Style _textBoxStyle;
        private TableItemStyle _titleTextStyle;
        private string _userName;
        private const string _userNameID = "UserName";
        private const string _userNameReplacementKey = @"<%\s*UserName\s*%>";
        private const string _userNameRequiredID = "UserNameRequired";
        private Control _userNameTableRow;
        private Control _validatorRow;
        private Style _validatorTextStyle;
        private const int _viewStateArrayLength = 14;
        public static readonly string CancelButtonCommandName = "Cancel";
        public static readonly string ChangePasswordButtonCommandName = "ChangePassword";
        public static readonly string ContinueButtonCommandName = "Continue";
        private static readonly object EventCancelButtonClick = new object();
        private static readonly object EventChangedPassword = new object();
        private static readonly object EventChangePasswordError = new object();
        private static readonly object EventChangingPassword = new object();
        private static readonly object EventContinueButtonClick = new object();
        private static readonly object EventSendingMail = new object();
        private static readonly object EventSendMailError = new object();

        [WebSysDescription("ChangePassword_CancelButtonClick"), WebCategory("Action")]
        public event EventHandler CancelButtonClick
        {
            add
            {
                base.Events.AddHandler(EventCancelButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCancelButtonClick, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("ChangePassword_ChangedPassword")]
        public event EventHandler ChangedPassword
        {
            add
            {
                base.Events.AddHandler(EventChangedPassword, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventChangedPassword, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("ChangePassword_ChangePasswordError")]
        public event EventHandler ChangePasswordError
        {
            add
            {
                base.Events.AddHandler(EventChangePasswordError, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventChangePasswordError, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("ChangePassword_ChangingPassword")]
        public event LoginCancelEventHandler ChangingPassword
        {
            add
            {
                base.Events.AddHandler(EventChangingPassword, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventChangingPassword, value);
            }
        }

        [WebSysDescription("ChangePassword_ContinueButtonClick"), WebCategory("Action")]
        public event EventHandler ContinueButtonClick
        {
            add
            {
                base.Events.AddHandler(EventContinueButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventContinueButtonClick, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("ChangePassword_SendingMail")]
        public event MailMessageEventHandler SendingMail
        {
            add
            {
                base.Events.AddHandler(EventSendingMail, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSendingMail, value);
            }
        }

        [WebSysDescription("ChangePassword_SendMailError"), WebCategory("Action")]
        public event SendMailErrorEventHandler SendMailError
        {
            add
            {
                base.Events.AddHandler(EventSendMailError, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSendMailError, value);
            }
        }

        private void AttemptChangePassword()
        {
            if ((this.Page == null) || this.Page.IsValid)
            {
                LoginCancelEventArgs e = new LoginCancelEventArgs();
                this.OnChangingPassword(e);
                if (!e.Cancel)
                {
                    System.Web.Security.MembershipProvider provider = LoginUtil.GetProvider(this.MembershipProvider);
                    MembershipUser user = provider.GetUser(this.UserNameInternal, false, false);
                    string newPasswordInternal = this.NewPasswordInternal;
                    if ((user != null) && user.ChangePassword(this.CurrentPasswordInternal, newPasswordInternal, false))
                    {
                        if (user.IsApproved && !user.IsLockedOut)
                        {
                            FormsAuthentication.SetAuthCookie(this.UserNameInternal, false);
                        }
                        this.OnChangedPassword(EventArgs.Empty);
                        this.PerformSuccessAction(user.Email, user.UserName, newPasswordInternal);
                    }
                    else
                    {
                        this.OnChangePasswordError(EventArgs.Empty);
                        string changePasswordFailureText = this.ChangePasswordFailureText;
                        if (!string.IsNullOrEmpty(changePasswordFailureText))
                        {
                            changePasswordFailureText = string.Format(CultureInfo.CurrentCulture, changePasswordFailureText, new object[] { provider.MinRequiredPasswordLength, provider.MinRequiredNonAlphanumericCharacters });
                        }
                        this.SetFailureTextLabel(this._changePasswordContainer, changePasswordFailureText);
                    }
                }
            }
        }

        private void ConfirmNewPasswordTextChanged(object source, EventArgs e)
        {
            this._confirmNewPassword = ((ITextControl) source).Text;
        }

        private void CreateChangePasswordViewControls()
        {
            this._changePasswordContainer = new ChangePasswordContainer(this);
            this._changePasswordContainer.ID = "ChangePasswordContainerID";
            this._changePasswordContainer.RenderDesignerRegion = this._renderDesignerRegion;
            ITemplate changePasswordTemplate = this.ChangePasswordTemplate;
            if (changePasswordTemplate == null)
            {
                this._changePasswordContainer.EnableViewState = false;
                this._changePasswordContainer.EnableTheming = false;
                changePasswordTemplate = new DefaultChangePasswordTemplate(this);
            }
            changePasswordTemplate.InstantiateIn(this._changePasswordContainer);
            this.Controls.Add(this._changePasswordContainer);
            IEditableTextControl userNameTextBox = this._changePasswordContainer.UserNameTextBox as IEditableTextControl;
            if (userNameTextBox != null)
            {
                userNameTextBox.TextChanged += new EventHandler(this.UserNameTextChanged);
            }
            IEditableTextControl currentPasswordTextBox = this._changePasswordContainer.CurrentPasswordTextBox as IEditableTextControl;
            if (currentPasswordTextBox != null)
            {
                currentPasswordTextBox.TextChanged += new EventHandler(this.PasswordTextChanged);
            }
            IEditableTextControl newPasswordTextBox = this._changePasswordContainer.NewPasswordTextBox as IEditableTextControl;
            if (newPasswordTextBox != null)
            {
                newPasswordTextBox.TextChanged += new EventHandler(this.NewPasswordTextChanged);
            }
            IEditableTextControl confirmNewPasswordTextBox = this._changePasswordContainer.ConfirmNewPasswordTextBox as IEditableTextControl;
            if (confirmNewPasswordTextBox != null)
            {
                confirmNewPasswordTextBox.TextChanged += new EventHandler(this.ConfirmNewPasswordTextChanged);
            }
            this.SetEditableChildProperties();
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            this.CreateChangePasswordViewControls();
            this.CreateSuccessViewControls();
            this.UpdateValidators();
        }

        private void CreateSuccessViewControls()
        {
            ITemplate successTemplate = null;
            this._successContainer = new SuccessContainer(this);
            this._successContainer.ID = "SuccessContainerID";
            this._successContainer.RenderDesignerRegion = this._renderDesignerRegion;
            if (this.SuccessTemplate != null)
            {
                successTemplate = this.SuccessTemplate;
            }
            else
            {
                successTemplate = new DefaultSuccessTemplate(this);
                this._successContainer.EnableViewState = false;
                this._successContainer.EnableTheming = false;
            }
            successTemplate.InstantiateIn(this._successContainer);
            this.Controls.Add(this._successContainer);
        }

        protected internal override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                Triplet triplet = (Triplet) savedState;
                if (triplet.First != null)
                {
                    base.LoadControlState(triplet.First);
                }
                if (triplet.Second != null)
                {
                    this._currentView = (View) ((int) triplet.Second);
                }
                if (triplet.Third != null)
                {
                    this._userName = (string) triplet.Third;
                }
            }
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
                if (objArray.Length != 14)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.ChangePasswordButtonStyle).LoadViewState(objArray[1]);
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
                    ((IStateManager) this.PasswordHintStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.FailureTextStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.MailDefinition).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    ((IStateManager) this.CancelButtonStyle).LoadViewState(objArray[10]);
                }
                if (objArray[11] != null)
                {
                    ((IStateManager) this.ContinueButtonStyle).LoadViewState(objArray[11]);
                }
                if (objArray[12] != null)
                {
                    ((IStateManager) this.SuccessTextStyle).LoadViewState(objArray[12]);
                }
                if (objArray[13] != null)
                {
                    ((IStateManager) this.ValidatorTextStyle).LoadViewState(objArray[13]);
                }
            }
            this.UpdateValidators();
        }

        private void NewPasswordTextChanged(object source, EventArgs e)
        {
            this._newPassword = ((ITextControl) source).Text;
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            bool flag = false;
            if (e is CommandEventArgs)
            {
                CommandEventArgs args = (CommandEventArgs) e;
                if (args.CommandName.Equals(ChangePasswordButtonCommandName, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.AttemptChangePassword();
                    return true;
                }
                if (args.CommandName.Equals(CancelButtonCommandName, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.OnCancelButtonClick(args);
                    return true;
                }
                if (args.CommandName.Equals(ContinueButtonCommandName, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.OnContinueButtonClick(args);
                    flag = true;
                }
            }
            return flag;
        }

        protected virtual void OnCancelButtonClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventCancelButtonClick];
            if (handler != null)
            {
                handler(this, e);
            }
            string cancelDestinationPageUrl = this.CancelDestinationPageUrl;
            if (!string.IsNullOrEmpty(cancelDestinationPageUrl))
            {
                this.Page.Response.Redirect(base.ResolveClientUrl(cancelDestinationPageUrl), false);
            }
        }

        protected virtual void OnChangedPassword(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventChangedPassword];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnChangePasswordError(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventChangePasswordError];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnChangingPassword(LoginCancelEventArgs e)
        {
            LoginCancelEventHandler handler = (LoginCancelEventHandler) base.Events[EventChangingPassword];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnContinueButtonClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventContinueButtonClick];
            if (handler != null)
            {
                handler(this, e);
            }
            string continueDestinationPageUrl = this.ContinueDestinationPageUrl;
            if (!string.IsNullOrEmpty(continueDestinationPageUrl))
            {
                this.Page.Response.Redirect(base.ResolveClientUrl(continueDestinationPageUrl), false);
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            if (!base.DesignMode)
            {
                string userName = LoginUtil.GetUserName(this);
                if (!string.IsNullOrEmpty(userName))
                {
                    this.UserName = userName;
                }
            }
            base.OnInit(e);
            this.Page.RegisterRequiresControlState(this);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.CurrentView == View.ChangePassword)
            {
                this.SetEditableChildProperties();
            }
        }

        protected virtual void OnSendingMail(MailMessageEventArgs e)
        {
            MailMessageEventHandler handler = (MailMessageEventHandler) base.Events[EventSendingMail];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSendMailError(SendMailErrorEventArgs e)
        {
            SendMailErrorEventHandler handler = (SendMailErrorEventHandler) base.Events[EventSendMailError];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void PasswordTextChanged(object source, EventArgs e)
        {
            this._password = ((ITextControl) source).Text;
        }

        private void PerformSuccessAction(string email, string userName, string newPassword)
        {
            if ((this._mailDefinition != null) && !string.IsNullOrEmpty(email))
            {
                LoginUtil.SendPasswordMail(email, userName, newPassword, this.MailDefinition, null, null, new LoginUtil.OnSendingMailDelegate(this.OnSendingMail), new LoginUtil.OnSendMailErrorDelegate(this.OnSendMailError), this);
            }
            string successPageUrl = this.SuccessPageUrl;
            if (!string.IsNullOrEmpty(successPageUrl))
            {
                this.Page.Response.Redirect(base.ResolveClientUrl(successPageUrl), false);
            }
            else
            {
                this.CurrentView = View.Success;
            }
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
            }
            this.EnsureChildControls();
            this.SetChildProperties();
            this.RenderContents(writer);
        }

        protected internal override object SaveControlState()
        {
            object x = base.SaveControlState();
            object y = null;
            object z = null;
            y = (int) this._currentView;
            if ((this._userName != null) && (this._currentView != View.Success))
            {
                z = this._userName;
            }
            return new Triplet(x, y, z);
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._changePasswordButtonStyle != null) ? ((IStateManager) this._changePasswordButtonStyle).SaveViewState() : null, (this._labelStyle != null) ? ((IStateManager) this._labelStyle).SaveViewState() : null, (this._textBoxStyle != null) ? ((IStateManager) this._textBoxStyle).SaveViewState() : null, (this._hyperLinkStyle != null) ? ((IStateManager) this._hyperLinkStyle).SaveViewState() : null, (this._instructionTextStyle != null) ? ((IStateManager) this._instructionTextStyle).SaveViewState() : null, (this._titleTextStyle != null) ? ((IStateManager) this._titleTextStyle).SaveViewState() : null, (this._passwordHintStyle != null) ? ((IStateManager) this._passwordHintStyle).SaveViewState() : null, (this._failureTextStyle != null) ? ((IStateManager) this._failureTextStyle).SaveViewState() : null, (this._mailDefinition != null) ? ((IStateManager) this._mailDefinition).SaveViewState() : null, (this._cancelButtonStyle != null) ? ((IStateManager) this._cancelButtonStyle).SaveViewState() : null, (this._continueButtonStyle != null) ? ((IStateManager) this._continueButtonStyle).SaveViewState() : null, (this._successTextStyle != null) ? ((IStateManager) this._successTextStyle).SaveViewState() : null, (this._validatorTextStyle != null) ? ((IStateManager) this._validatorTextStyle).SaveViewState() : null };
            for (int i = 0; i < 14; i++)
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
            switch (this.CurrentView)
            {
                case View.ChangePassword:
                    this.SetCommonChangePasswordViewProperties();
                    if (this.ChangePasswordTemplate != null)
                    {
                        break;
                    }
                    this.SetDefaultChangePasswordViewProperties();
                    return;

                case View.Success:
                    this.SetCommonSuccessViewProperties();
                    if (this.SuccessTemplate == null)
                    {
                        this.SetDefaultSuccessViewProperties();
                    }
                    break;

                default:
                    return;
            }
        }

        private void SetCommonChangePasswordViewProperties()
        {
            Util.CopyBaseAttributesToInnerControl(this, this._changePasswordContainer);
            this._changePasswordContainer.ApplyStyle(base.ControlStyle);
            this._successContainer.Visible = false;
        }

        private void SetCommonSuccessViewProperties()
        {
            Util.CopyBaseAttributesToInnerControl(this, this._successContainer);
            this._successContainer.ApplyStyle(base.ControlStyle);
            this._changePasswordContainer.Visible = false;
        }

        private void SetDefaultChangePasswordViewProperties()
        {
            ChangePasswordContainer container = this._changePasswordContainer;
            container.BorderTable.CellPadding = this.BorderPadding;
            container.BorderTable.CellSpacing = 0;
            LoginUtil.ApplyStyleToLiteral(container.Title, this.ChangePasswordTitleText, this.TitleTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(container.Instruction, this.InstructionText, this.InstructionTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(container.UserNameLabel, this.UserNameLabelText, this.LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(container.CurrentPasswordLabel, this.PasswordLabelText, this.LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(container.NewPasswordLabel, this.NewPasswordLabelText, this.LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(container.ConfirmNewPasswordLabel, this.ConfirmNewPasswordLabelText, this.LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(container.PasswordHintLabel, this.PasswordHintText, this.PasswordHintStyle, false);
            if (this._textBoxStyle != null)
            {
                if (this.DisplayUserName)
                {
                    ((WebControl) container.UserNameTextBox).ApplyStyle(this.TextBoxStyle);
                }
                ((WebControl) container.CurrentPasswordTextBox).ApplyStyle(this.TextBoxStyle);
                ((WebControl) container.NewPasswordTextBox).ApplyStyle(this.TextBoxStyle);
                ((WebControl) container.ConfirmNewPasswordTextBox).ApplyStyle(this.TextBoxStyle);
            }
            this._passwordHintTableRow.Visible = !string.IsNullOrEmpty(this.PasswordHintText);
            this._userNameTableRow.Visible = this.DisplayUserName;
            if (this.DisplayUserName)
            {
                ((WebControl) container.UserNameTextBox).TabIndex = this.TabIndex;
                ((WebControl) container.UserNameTextBox).AccessKey = this.AccessKey;
            }
            else
            {
                ((WebControl) container.CurrentPasswordTextBox).AccessKey = this.AccessKey;
            }
            ((WebControl) container.CurrentPasswordTextBox).TabIndex = this.TabIndex;
            ((WebControl) container.NewPasswordTextBox).TabIndex = this.TabIndex;
            ((WebControl) container.ConfirmNewPasswordTextBox).TabIndex = this.TabIndex;
            bool flag = true;
            this.ValidatorRow.Visible = flag;
            RequiredFieldValidator userNameRequired = container.UserNameRequired;
            userNameRequired.ErrorMessage = this.UserNameRequiredErrorMessage;
            userNameRequired.ToolTip = this.UserNameRequiredErrorMessage;
            userNameRequired.Enabled = flag;
            userNameRequired.Visible = flag;
            if (this._validatorTextStyle != null)
            {
                userNameRequired.ApplyStyle(this._validatorTextStyle);
            }
            RequiredFieldValidator passwordRequired = container.PasswordRequired;
            passwordRequired.ErrorMessage = this.PasswordRequiredErrorMessage;
            passwordRequired.ToolTip = this.PasswordRequiredErrorMessage;
            passwordRequired.Enabled = flag;
            passwordRequired.Visible = flag;
            RequiredFieldValidator newPasswordRequired = container.NewPasswordRequired;
            newPasswordRequired.ErrorMessage = this.NewPasswordRequiredErrorMessage;
            newPasswordRequired.ToolTip = this.NewPasswordRequiredErrorMessage;
            newPasswordRequired.Enabled = flag;
            newPasswordRequired.Visible = flag;
            RequiredFieldValidator confirmNewPasswordRequired = container.ConfirmNewPasswordRequired;
            confirmNewPasswordRequired.ErrorMessage = this.ConfirmPasswordRequiredErrorMessage;
            confirmNewPasswordRequired.ToolTip = this.ConfirmPasswordRequiredErrorMessage;
            confirmNewPasswordRequired.Enabled = flag;
            confirmNewPasswordRequired.Visible = flag;
            CompareValidator newPasswordCompareValidator = container.NewPasswordCompareValidator;
            newPasswordCompareValidator.ErrorMessage = this.ConfirmPasswordCompareErrorMessage;
            newPasswordCompareValidator.Enabled = flag;
            newPasswordCompareValidator.Visible = flag;
            if (this._validatorTextStyle != null)
            {
                passwordRequired.ApplyStyle(this._validatorTextStyle);
                newPasswordRequired.ApplyStyle(this._validatorTextStyle);
                confirmNewPasswordRequired.ApplyStyle(this._validatorTextStyle);
                newPasswordCompareValidator.ApplyStyle(this._validatorTextStyle);
            }
            RegularExpressionValidator regExpValidator = container.RegExpValidator;
            regExpValidator.ErrorMessage = this.NewPasswordRegularExpressionErrorMessage;
            regExpValidator.Enabled = flag;
            regExpValidator.Visible = flag;
            if (this._validatorTextStyle != null)
            {
                regExpValidator.ApplyStyle(this._validatorTextStyle);
            }
            LinkButton changePasswordLinkButton = container.ChangePasswordLinkButton;
            LinkButton cancelLinkButton = container.CancelLinkButton;
            ImageButton changePasswordImageButton = container.ChangePasswordImageButton;
            ImageButton cancelImageButton = container.CancelImageButton;
            Button changePasswordPushButton = container.ChangePasswordPushButton;
            Button cancelPushButton = container.CancelPushButton;
            WebControl control = null;
            WebControl control2 = null;
            switch (this.CancelButtonType)
            {
                case ButtonType.Button:
                    cancelPushButton.Text = this.CancelButtonText;
                    control2 = cancelPushButton;
                    break;

                case ButtonType.Image:
                    cancelImageButton.ImageUrl = this.CancelButtonImageUrl;
                    cancelImageButton.AlternateText = this.CancelButtonText;
                    control2 = cancelImageButton;
                    break;

                case ButtonType.Link:
                    cancelLinkButton.Text = this.CancelButtonText;
                    control2 = cancelLinkButton;
                    break;
            }
            switch (this.ChangePasswordButtonType)
            {
                case ButtonType.Button:
                    changePasswordPushButton.Text = this.ChangePasswordButtonText;
                    control = changePasswordPushButton;
                    break;

                case ButtonType.Image:
                    changePasswordImageButton.ImageUrl = this.ChangePasswordButtonImageUrl;
                    changePasswordImageButton.AlternateText = this.ChangePasswordButtonText;
                    control = changePasswordImageButton;
                    break;

                case ButtonType.Link:
                    changePasswordLinkButton.Text = this.ChangePasswordButtonText;
                    control = changePasswordLinkButton;
                    break;
            }
            changePasswordLinkButton.Visible = false;
            changePasswordImageButton.Visible = false;
            changePasswordPushButton.Visible = false;
            cancelLinkButton.Visible = false;
            cancelImageButton.Visible = false;
            cancelPushButton.Visible = false;
            control.Visible = true;
            control2.Visible = true;
            control2.TabIndex = this.TabIndex;
            control.TabIndex = this.TabIndex;
            if (this.CancelButtonStyle != null)
            {
                control2.ApplyStyle(this.CancelButtonStyle);
            }
            if (this.ChangePasswordButtonStyle != null)
            {
                control.ApplyStyle(this.ChangePasswordButtonStyle);
            }
            Image createUserIcon = container.CreateUserIcon;
            HyperLink createUserLink = container.CreateUserLink;
            LiteralControl createUserLinkSeparator = container.CreateUserLinkSeparator;
            HyperLink passwordRecoveryLink = container.PasswordRecoveryLink;
            Image passwordRecoveryIcon = container.PasswordRecoveryIcon;
            HyperLink helpPageLink = container.HelpPageLink;
            Image helpPageIcon = container.HelpPageIcon;
            LiteralControl helpPageLinkSeparator = container.HelpPageLinkSeparator;
            LiteralControl editProfileLinkSeparator = container.EditProfileLinkSeparator;
            HyperLink editProfileLink = container.EditProfileLink;
            Image editProfileIcon = container.EditProfileIcon;
            string createUserText = this.CreateUserText;
            string createUserIconUrl = this.CreateUserIconUrl;
            string passwordRecoveryText = this.PasswordRecoveryText;
            string passwordRecoveryIconUrl = this.PasswordRecoveryIconUrl;
            string helpPageText = this.HelpPageText;
            string helpPageIconUrl = this.HelpPageIconUrl;
            string editProfileText = this.EditProfileText;
            string editProfileIconUrl = this.EditProfileIconUrl;
            bool flag2 = createUserText.Length > 0;
            bool flag3 = passwordRecoveryText.Length > 0;
            bool flag4 = helpPageText.Length > 0;
            bool flag5 = helpPageIconUrl.Length > 0;
            bool flag6 = createUserIconUrl.Length > 0;
            bool flag7 = passwordRecoveryIconUrl.Length > 0;
            bool flag8 = flag4 || flag5;
            bool flag9 = flag2 || flag6;
            bool flag10 = flag3 || flag7;
            bool flag11 = editProfileText.Length > 0;
            bool flag12 = editProfileIconUrl.Length > 0;
            bool flag13 = flag11 || flag12;
            helpPageLink.Visible = flag4;
            helpPageLinkSeparator.Visible = flag8 && ((flag10 || flag9) || flag13);
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
            createUserLinkSeparator.Visible = flag9 && (flag10 || flag13);
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
            editProfileLinkSeparator.Visible = flag10 && flag13;
            editProfileLink.Visible = flag11;
            editProfileIcon.Visible = flag12;
            if (flag11)
            {
                editProfileLink.Text = editProfileText;
                editProfileLink.NavigateUrl = this.EditProfileUrl;
                editProfileLink.TabIndex = this.TabIndex;
            }
            if (flag12)
            {
                editProfileIcon.ImageUrl = editProfileIconUrl;
                editProfileIcon.AlternateText = this.EditProfileText;
            }
            if ((flag9 || flag10) || (flag8 || flag13))
            {
                if (this._hyperLinkStyle != null)
                {
                    TableItemStyle style = new TableItemStyle();
                    style.CopyFrom(this._hyperLinkStyle);
                    style.Font.Reset();
                    LoginUtil.SetTableCellStyle(createUserLink, style);
                    createUserLink.Font.CopyFrom(this._hyperLinkStyle.Font);
                    createUserLink.ForeColor = this._hyperLinkStyle.ForeColor;
                    passwordRecoveryLink.Font.CopyFrom(this._hyperLinkStyle.Font);
                    passwordRecoveryLink.ForeColor = this._hyperLinkStyle.ForeColor;
                    helpPageLink.Font.CopyFrom(this._hyperLinkStyle.Font);
                    helpPageLink.ForeColor = this._hyperLinkStyle.ForeColor;
                    editProfileLink.Font.CopyFrom(this._hyperLinkStyle.Font);
                    editProfileLink.ForeColor = this._hyperLinkStyle.ForeColor;
                }
                LoginUtil.SetTableCellVisible(helpPageLink, true);
            }
            else
            {
                LoginUtil.SetTableCellVisible(helpPageLink, false);
            }
            Control failureTextLabel = container.FailureTextLabel;
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

        internal void SetDefaultSuccessViewProperties()
        {
            SuccessContainer container = this._successContainer;
            LinkButton continueLinkButton = container.ContinueLinkButton;
            ImageButton continueImageButton = container.ContinueImageButton;
            Button continuePushButton = container.ContinuePushButton;
            container.BorderTable.CellPadding = this.BorderPadding;
            container.BorderTable.CellSpacing = 0;
            WebControl control = null;
            switch (this.ContinueButtonType)
            {
                case ButtonType.Button:
                    continuePushButton.Text = this.ContinueButtonText;
                    control = continuePushButton;
                    break;

                case ButtonType.Image:
                    continueImageButton.ImageUrl = this.ContinueButtonImageUrl;
                    continueImageButton.AlternateText = this.ContinueButtonText;
                    control = continueImageButton;
                    break;

                case ButtonType.Link:
                    continueLinkButton.Text = this.ContinueButtonText;
                    control = continueLinkButton;
                    break;
            }
            continueLinkButton.Visible = false;
            continueImageButton.Visible = false;
            continuePushButton.Visible = false;
            control.Visible = true;
            control.TabIndex = this.TabIndex;
            control.AccessKey = this.AccessKey;
            if (this.ContinueButtonStyle != null)
            {
                control.ApplyStyle(this.ContinueButtonStyle);
            }
            LoginUtil.ApplyStyleToLiteral(container.Title, this.SuccessTitleText, this._titleTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(container.SuccessTextLabel, this.SuccessText, this._successTextStyle, true);
            string editProfileText = this.EditProfileText;
            string editProfileIconUrl = this.EditProfileIconUrl;
            bool flag = editProfileText.Length > 0;
            bool flag2 = editProfileIconUrl.Length > 0;
            HyperLink editProfileLink = container.EditProfileLink;
            Image editProfileIcon = container.EditProfileIcon;
            editProfileIcon.Visible = flag2;
            editProfileLink.Visible = flag;
            if (flag)
            {
                editProfileLink.Text = editProfileText;
                editProfileLink.NavigateUrl = this.EditProfileUrl;
                editProfileLink.TabIndex = this.TabIndex;
                if (this._hyperLinkStyle != null)
                {
                    Style style = new TableItemStyle();
                    style.CopyFrom(this._hyperLinkStyle);
                    style.Font.Reset();
                    LoginUtil.SetTableCellStyle(editProfileLink, style);
                    editProfileLink.Font.CopyFrom(this._hyperLinkStyle.Font);
                    editProfileLink.ForeColor = this._hyperLinkStyle.ForeColor;
                }
            }
            if (flag2)
            {
                editProfileIcon.ImageUrl = editProfileIconUrl;
                editProfileIcon.AlternateText = this.EditProfileText;
            }
            LoginUtil.SetTableCellVisible(editProfileLink, flag || flag2);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override void SetDesignModeState(IDictionary data)
        {
            if (data != null)
            {
                object obj2 = data["CurrentView"];
                if (obj2 != null)
                {
                    this.CurrentView = (View) obj2;
                }
                obj2 = data["ConvertToTemplate"];
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
            if ((this.UserNameInternal.Length > 0) && this.DisplayUserName)
            {
                ITextControl userNameTextBox = (ITextControl) this._changePasswordContainer.UserNameTextBox;
                if (userNameTextBox != null)
                {
                    userNameTextBox.Text = this.UserNameInternal;
                }
            }
        }

        private void SetFailureTextLabel(ChangePasswordContainer container, string failureText)
        {
            ITextControl failureTextLabel = (ITextControl) container.FailureTextLabel;
            if (failureTextLabel != null)
            {
                failureTextLabel.Text = failureText;
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._changePasswordButtonStyle != null)
            {
                ((IStateManager) this._changePasswordButtonStyle).TrackViewState();
            }
            if (this._labelStyle != null)
            {
                ((IStateManager) this._labelStyle).TrackViewState();
            }
            if (this._textBoxStyle != null)
            {
                ((IStateManager) this._textBoxStyle).TrackViewState();
            }
            if (this._successTextStyle != null)
            {
                ((IStateManager) this._successTextStyle).TrackViewState();
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
            if (this._passwordHintStyle != null)
            {
                ((IStateManager) this._passwordHintStyle).TrackViewState();
            }
            if (this._failureTextStyle != null)
            {
                ((IStateManager) this._failureTextStyle).TrackViewState();
            }
            if (this._mailDefinition != null)
            {
                ((IStateManager) this._mailDefinition).TrackViewState();
            }
            if (this._cancelButtonStyle != null)
            {
                ((IStateManager) this._cancelButtonStyle).TrackViewState();
            }
            if (this._continueButtonStyle != null)
            {
                ((IStateManager) this._continueButtonStyle).TrackViewState();
            }
            if (this._validatorTextStyle != null)
            {
                ((IStateManager) this._validatorTextStyle).TrackViewState();
            }
        }

        private void UpdateValidators()
        {
            if (!base.DesignMode)
            {
                ChangePasswordContainer container = this._changePasswordContainer;
                if (container != null)
                {
                    bool displayUserName = this.DisplayUserName;
                    RequiredFieldValidator userNameRequired = container.UserNameRequired;
                    if (userNameRequired != null)
                    {
                        userNameRequired.Enabled = displayUserName;
                        userNameRequired.Visible = displayUserName;
                    }
                    bool regExpEnabled = this.RegExpEnabled;
                    RegularExpressionValidator regExpValidator = container.RegExpValidator;
                    if (regExpValidator != null)
                    {
                        regExpValidator.Enabled = regExpEnabled;
                        regExpValidator.Visible = regExpEnabled;
                    }
                }
            }
        }

        private void UserNameTextChanged(object source, EventArgs e)
        {
            string text = ((ITextControl) source).Text;
            if (!string.IsNullOrEmpty(text))
            {
                this.UserName = text;
            }
        }

        [WebSysDescription("Login_BorderPadding"), DefaultValue(1), WebCategory("Appearance")]
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
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("ChangePassword_InvalidBorderPadding"));
                }
                this.ViewState["BorderPadding"] = value;
            }
        }

        [UrlProperty, WebCategory("Appearance"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), WebSysDescription("ChangePassword_CancelButtonImageUrl")]
        public virtual string CancelButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["CancelButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CancelButtonImageUrl"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("ChangePassword_CancelButtonStyle")]
        public Style CancelButtonStyle
        {
            get
            {
                if (this._cancelButtonStyle == null)
                {
                    this._cancelButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._cancelButtonStyle).TrackViewState();
                    }
                }
                return this._cancelButtonStyle;
            }
        }

        [WebSysDefaultValue("ChangePassword_DefaultCancelButtonText"), Localizable(true), WebSysDescription("ChangePassword_CancelButtonText"), WebCategory("Appearance")]
        public virtual string CancelButtonText
        {
            get
            {
                object obj2 = this.ViewState["CancelButtonText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultCancelButtonText");
            }
            set
            {
                this.ViewState["CancelButtonText"] = value;
            }
        }

        [WebSysDescription("ChangePassword_CancelButtonType"), WebCategory("Appearance"), DefaultValue(0)]
        public virtual ButtonType CancelButtonType
        {
            get
            {
                object obj2 = this.ViewState["CancelButtonType"];
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
                this.ViewState["CancelButtonType"] = value;
            }
        }

        [Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, DefaultValue(""), WebSysDescription("ChangePassword_CancelDestinationPageUrl"), WebCategory("Behavior"), Themeable(false)]
        public virtual string CancelDestinationPageUrl
        {
            get
            {
                object obj2 = this.ViewState["CancelDestinationPageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CancelDestinationPageUrl"] = value;
            }
        }

        [WebCategory("Appearance"), DefaultValue(""), WebSysDescription("ChangePassword_ChangePasswordButtonImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
        public virtual string ChangePasswordButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["ChangePasswordButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ChangePasswordButtonImageUrl"] = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("ChangePassword_ChangePasswordButtonStyle"), NotifyParentProperty(true), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Style ChangePasswordButtonStyle
        {
            get
            {
                if (this._changePasswordButtonStyle == null)
                {
                    this._changePasswordButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._changePasswordButtonStyle).TrackViewState();
                    }
                }
                return this._changePasswordButtonStyle;
            }
        }

        [WebSysDefaultValue("ChangePassword_DefaultChangePasswordButtonText"), WebSysDescription("ChangePassword_ChangePasswordButtonText"), WebCategory("Appearance"), Localizable(true)]
        public virtual string ChangePasswordButtonText
        {
            get
            {
                object obj2 = this.ViewState["ChangePasswordButtonText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultChangePasswordButtonText");
            }
            set
            {
                this.ViewState["ChangePasswordButtonText"] = value;
            }
        }

        [DefaultValue(0), WebSysDescription("ChangePassword_ChangePasswordButtonType"), WebCategory("Appearance")]
        public virtual ButtonType ChangePasswordButtonType
        {
            get
            {
                object obj2 = this.ViewState["ChangePasswordButtonType"];
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
                this.ViewState["ChangePasswordButtonType"] = value;
            }
        }

        [Localizable(true), WebSysDescription("ChangePassword_ChangePasswordFailureText"), WebCategory("Appearance"), WebSysDefaultValue("ChangePassword_DefaultChangePasswordFailureText")]
        public virtual string ChangePasswordFailureText
        {
            get
            {
                object obj2 = this.ViewState["ChangePasswordFailureText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultChangePasswordFailureText");
            }
            set
            {
                this.ViewState["ChangePasswordFailureText"] = value;
            }
        }

        [Browsable(false), TemplateContainer(typeof(ChangePassword)), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate ChangePasswordTemplate
        {
            get
            {
                return this._changePasswordTemplate;
            }
            set
            {
                this._changePasswordTemplate = value;
                base.ChildControlsCreated = false;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control ChangePasswordTemplateContainer
        {
            get
            {
                this.EnsureChildControls();
                return this._changePasswordContainer;
            }
        }

        [WebCategory("Appearance"), Localizable(true), WebSysDefaultValue("ChangePassword_DefaultChangePasswordTitleText"), WebSysDescription("LoginControls_TitleText")]
        public virtual string ChangePasswordTitleText
        {
            get
            {
                object obj2 = this.ViewState["ChangePasswordTitleText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultChangePasswordTitleText");
            }
            set
            {
                this.ViewState["ChangePasswordTitleText"] = value;
            }
        }

        [Filterable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Themeable(false), Browsable(false)]
        public virtual string ConfirmNewPassword
        {
            get
            {
                if (this._confirmNewPassword != null)
                {
                    return this._confirmNewPassword;
                }
                return string.Empty;
            }
        }

        [WebCategory("Appearance"), WebSysDefaultValue("ChangePassword_DefaultConfirmNewPasswordLabelText"), WebSysDescription("ChangePassword_ConfirmNewPasswordLabelText"), Localizable(true)]
        public virtual string ConfirmNewPasswordLabelText
        {
            get
            {
                object obj2 = this.ViewState["ConfirmNewPasswordLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultConfirmNewPasswordLabelText");
            }
            set
            {
                this.ViewState["ConfirmNewPasswordLabelText"] = value;
            }
        }

        [WebCategory("Validation"), WebSysDescription("ChangePassword_ConfirmPasswordCompareErrorMessage"), Localizable(true), WebSysDefaultValue("ChangePassword_DefaultConfirmPasswordCompareErrorMessage")]
        public virtual string ConfirmPasswordCompareErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["ConfirmPasswordCompareErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultConfirmPasswordCompareErrorMessage");
            }
            set
            {
                this.ViewState["ConfirmPasswordCompareErrorMessage"] = value;
            }
        }

        [WebSysDescription("LoginControls_ConfirmPasswordRequiredErrorMessage"), Localizable(true), WebCategory("Validation"), WebSysDefaultValue("ChangePassword_DefaultConfirmPasswordRequiredErrorMessage")]
        public virtual string ConfirmPasswordRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["ConfirmPasswordRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultConfirmPasswordRequiredErrorMessage");
            }
            set
            {
                this.ViewState["ConfirmPasswordRequiredErrorMessage"] = value;
            }
        }

        [DefaultValue(""), WebCategory("Appearance"), UrlProperty, WebSysDescription("ChangePassword_ContinueButtonImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string ContinueButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["ContinueButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ContinueButtonImageUrl"] = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("ChangePassword_ContinueButtonStyle")]
        public Style ContinueButtonStyle
        {
            get
            {
                if (this._continueButtonStyle == null)
                {
                    this._continueButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._continueButtonStyle).TrackViewState();
                    }
                }
                return this._continueButtonStyle;
            }
        }

        [WebSysDescription("ChangePassword_ContinueButtonText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("ChangePassword_DefaultContinueButtonText")]
        public virtual string ContinueButtonText
        {
            get
            {
                object obj2 = this.ViewState["ContinueButtonText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultContinueButtonText");
            }
            set
            {
                this.ViewState["ContinueButtonText"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Appearance"), WebSysDescription("ChangePassword_ContinueButtonType")]
        public virtual ButtonType ContinueButtonType
        {
            get
            {
                object obj2 = this.ViewState["ContinueButtonType"];
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
                this.ViewState["ContinueButtonType"] = value;
            }
        }

        [Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Behavior"), Themeable(false), UrlProperty, WebSysDescription("LoginControls_ContinueDestinationPageUrl"), DefaultValue("")]
        public virtual string ContinueDestinationPageUrl
        {
            get
            {
                object obj2 = this.ViewState["ContinueDestinationPageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ContinueDestinationPageUrl"] = value;
            }
        }

        private bool ConvertingToTemplate
        {
            get
            {
                return (base.DesignMode && this._convertingToTemplate);
            }
        }

        [UrlProperty, Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Links"), DefaultValue(""), WebSysDescription("ChangePassword_CreateUserIconUrl")]
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

        [Localizable(true), WebCategory("Links"), WebSysDescription("ChangePassword_CreateUserText"), DefaultValue("")]
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

        [UrlProperty, WebCategory("Links"), DefaultValue(""), WebSysDescription("ChangePassword_CreateUserUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
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

        [Themeable(false), Filterable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual string CurrentPassword
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

        private string CurrentPasswordInternal
        {
            get
            {
                string currentPassword = this.CurrentPassword;
                if (string.IsNullOrEmpty(currentPassword) && (this._changePasswordContainer != null))
                {
                    ITextControl currentPasswordTextBox = (ITextControl) this._changePasswordContainer.CurrentPasswordTextBox;
                    if (currentPasswordTextBox != null)
                    {
                        return currentPasswordTextBox.Text;
                    }
                }
                return currentPassword;
            }
        }

        internal View CurrentView
        {
            get
            {
                return this._currentView;
            }
            set
            {
                if ((value < View.ChangePassword) || (value > View.Success))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != this.CurrentView)
                {
                    this._currentView = value;
                }
            }
        }

        [DefaultValue(false), WebCategory("Behavior"), WebSysDescription("ChangePassword_DisplayUserName")]
        public virtual bool DisplayUserName
        {
            get
            {
                object obj2 = this.ViewState["DisplayUserName"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                if (this.DisplayUserName != value)
                {
                    this.ViewState["DisplayUserName"] = value;
                    this.UpdateValidators();
                }
            }
        }

        [DefaultValue(""), UrlProperty, WebCategory("Links"), WebSysDescription("LoginControls_EditProfileIconUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string EditProfileIconUrl
        {
            get
            {
                object obj2 = this.ViewState["EditProfileIconUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["EditProfileIconUrl"] = value;
            }
        }

        [WebCategory("Links"), Localizable(true), WebSysDescription("ChangePassword_EditProfileText"), DefaultValue("")]
        public virtual string EditProfileText
        {
            get
            {
                object obj2 = this.ViewState["EditProfileText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["EditProfileText"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("ChangePassword_EditProfileUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Links")]
        public virtual string EditProfileUrl
        {
            get
            {
                object obj2 = this.ViewState["EditProfileUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["EditProfileUrl"] = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("WebControl_FailureTextStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
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

        [WebCategory("Links"), DefaultValue(""), WebSysDescription("LoginControls_HelpPageIconUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
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

        [Localizable(true), WebCategory("Links"), DefaultValue(""), WebSysDescription("ChangePassword_HelpPageText")]
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

        [WebCategory("Links"), DefaultValue(""), WebSysDescription("LoginControls_HelpPageUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
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

        [NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_HyperLinkStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles")]
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

        [Localizable(true), WebSysDescription("WebControl_InstructionText"), DefaultValue(""), WebCategory("Appearance")]
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

        [NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("LoginControls_LabelStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Behavior"), Themeable(false), WebSysDescription("ChangePassword_MailDefinition"), PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true)]
        public System.Web.UI.WebControls.MailDefinition MailDefinition
        {
            get
            {
                if (this._mailDefinition == null)
                {
                    this._mailDefinition = new System.Web.UI.WebControls.MailDefinition();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._mailDefinition).TrackViewState();
                    }
                }
                return this._mailDefinition;
            }
        }

        [DefaultValue(""), WebCategory("Data"), WebSysDescription("MembershipProvider_Name"), Themeable(false)]
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

        [Themeable(false), Filterable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual string NewPassword
        {
            get
            {
                if (this._newPassword != null)
                {
                    return this._newPassword;
                }
                return string.Empty;
            }
        }

        private string NewPasswordInternal
        {
            get
            {
                string newPassword = this.NewPassword;
                if (string.IsNullOrEmpty(newPassword) && (this._changePasswordContainer != null))
                {
                    ITextControl newPasswordTextBox = (ITextControl) this._changePasswordContainer.NewPasswordTextBox;
                    if (newPasswordTextBox != null)
                    {
                        return newPasswordTextBox.Text;
                    }
                }
                return newPassword;
            }
        }

        [WebSysDescription("ChangePassword_NewPasswordLabelText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("ChangePassword_DefaultNewPasswordLabelText")]
        public virtual string NewPasswordLabelText
        {
            get
            {
                object obj2 = this.ViewState["NewPasswordLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultNewPasswordLabelText");
            }
            set
            {
                this.ViewState["NewPasswordLabelText"] = value;
            }
        }

        [WebSysDescription("ChangePassword_NewPasswordRegularExpression"), WebSysDefaultValue(""), WebCategory("Validation")]
        public virtual string NewPasswordRegularExpression
        {
            get
            {
                object obj2 = this.ViewState["NewPasswordRegularExpression"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (this.NewPasswordRegularExpression != value)
                {
                    this.ViewState["NewPasswordRegularExpression"] = value;
                    this.UpdateValidators();
                }
            }
        }

        [WebCategory("Validation"), WebSysDescription("ChangePassword_NewPasswordRegularExpressionErrorMessage"), WebSysDefaultValue("Password_InvalidPasswordErrorMessage")]
        public virtual string NewPasswordRegularExpressionErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["NewPasswordRegularExpressionErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Password_InvalidPasswordErrorMessage");
            }
            set
            {
                this.ViewState["NewPasswordRegularExpressionErrorMessage"] = value;
            }
        }

        [WebSysDefaultValue("ChangePassword_DefaultNewPasswordRequiredErrorMessage"), Localizable(true), WebSysDescription("ChangePassword_NewPasswordRequiredErrorMessage"), WebCategory("Validation")]
        public virtual string NewPasswordRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["NewPasswordRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultNewPasswordRequiredErrorMessage");
            }
            set
            {
                this.ViewState["NewPasswordRequiredErrorMessage"] = value;
            }
        }

        [WebCategory("Styles"), WebSysDescription("ChangePassword_PasswordHintStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle PasswordHintStyle
        {
            get
            {
                if (this._passwordHintStyle == null)
                {
                    this._passwordHintStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._passwordHintStyle).TrackViewState();
                    }
                }
                return this._passwordHintStyle;
            }
        }

        [WebCategory("Appearance"), Localizable(true), WebSysDescription("ChangePassword_PasswordHintText"), DefaultValue("")]
        public virtual string PasswordHintText
        {
            get
            {
                object obj2 = this.ViewState["PasswordHintText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["PasswordHintText"] = value;
            }
        }

        [WebSysDefaultValue("LoginControls_DefaultPasswordLabelText"), WebSysDescription("LoginControls_PasswordLabelText"), WebCategory("Appearance"), Localizable(true)]
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

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), WebSysDescription("ChangePassword_PasswordRecoveryIconUrl"), WebCategory("Links"), UrlProperty]
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

        [WebSysDescription("ChangePassword_PasswordRecoveryText"), WebCategory("Links"), DefaultValue(""), Localizable(true)]
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

        [WebCategory("Links"), DefaultValue(""), WebSysDescription("ChangePassword_PasswordRecoveryUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
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

        [WebCategory("Validation"), WebSysDescription("ChangePassword_PasswordRequiredErrorMessage"), Localizable(true), WebSysDefaultValue("ChangePassword_DefaultPasswordRequiredErrorMessage")]
        public virtual string PasswordRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["PasswordRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultPasswordRequiredErrorMessage");
            }
            set
            {
                this.ViewState["PasswordRequiredErrorMessage"] = value;
            }
        }

        private bool RegExpEnabled
        {
            get
            {
                return (this.NewPasswordRegularExpression.Length > 0);
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

        [WebCategory("Behavior"), UrlProperty, DefaultValue(""), WebSysDescription("LoginControls_SuccessPageUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Themeable(false)]
        public virtual string SuccessPageUrl
        {
            get
            {
                object obj2 = this.ViewState["SuccessPageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["SuccessPageUrl"] = value;
            }
        }

        [Browsable(false), TemplateContainer(typeof(ChangePassword)), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate SuccessTemplate
        {
            get
            {
                return this._successTemplate;
            }
            set
            {
                this._successTemplate = value;
                base.ChildControlsCreated = false;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control SuccessTemplateContainer
        {
            get
            {
                this.EnsureChildControls();
                return this._successContainer;
            }
        }

        [WebSysDescription("ChangePassword_SuccessText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("ChangePassword_DefaultSuccessText")]
        public virtual string SuccessText
        {
            get
            {
                object obj2 = this.ViewState["SuccessText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultSuccessText");
            }
            set
            {
                this.ViewState["SuccessText"] = value;
            }
        }

        [DefaultValue((string) null), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("ChangePassword_SuccessTextStyle")]
        public TableItemStyle SuccessTextStyle
        {
            get
            {
                if (this._successTextStyle == null)
                {
                    this._successTextStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._successTextStyle).TrackViewState();
                    }
                }
                return this._successTextStyle;
            }
        }

        [Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("ChangePassword_DefaultSuccessTitleText"), WebSysDescription("ChangePassword_SuccessTitleText")]
        public virtual string SuccessTitleText
        {
            get
            {
                object obj2 = this.ViewState["SuccessTitleText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultSuccessTitleText");
            }
            set
            {
                this.ViewState["SuccessTitleText"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        [WebSysDescription("LoginControls_TextBoxStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("LoginControls_TitleTextStyle")]
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

        [DefaultValue(""), WebCategory("Appearance"), WebSysDescription("UserName_InitialValue")]
        public virtual string UserName
        {
            get
            {
                if (this._userName != null)
                {
                    return this._userName;
                }
                return string.Empty;
            }
            set
            {
                this._userName = value;
            }
        }

        private string UserNameInternal
        {
            get
            {
                string userName = this.UserName;
                if ((string.IsNullOrEmpty(userName) && (this._changePasswordContainer != null)) && this.DisplayUserName)
                {
                    ITextControl userNameTextBox = (ITextControl) this._changePasswordContainer.UserNameTextBox;
                    if (userNameTextBox != null)
                    {
                        return userNameTextBox.Text;
                    }
                }
                return userName;
            }
        }

        [WebCategory("Appearance"), Localizable(true), WebSysDescription("LoginControls_UserNameLabelText"), WebSysDefaultValue("ChangePassword_DefaultUserNameLabelText")]
        public virtual string UserNameLabelText
        {
            get
            {
                object obj2 = this.ViewState["UserNameLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultUserNameLabelText");
            }
            set
            {
                this.ViewState["UserNameLabelText"] = value;
            }
        }

        [Localizable(true), WebSysDescription("ChangePassword_UserNameRequiredErrorMessage"), WebCategory("Validation"), WebSysDefaultValue("ChangePassword_DefaultUserNameRequiredErrorMessage")]
        public virtual string UserNameRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["UserNameRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("ChangePassword_DefaultUserNameRequiredErrorMessage");
            }
            set
            {
                this.ViewState["UserNameRequiredErrorMessage"] = value;
            }
        }

        internal Control ValidatorRow
        {
            get
            {
                return this._validatorRow;
            }
            set
            {
                this._validatorRow = value;
            }
        }

        [NotifyParentProperty(true), WebSysDescription("ChangePassword_ValidatorTextStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
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

        internal sealed class ChangePasswordContainer : LoginUtil.GenericContainer<ChangePassword>, INonBindingContainer, INamingContainer
        {
            private ImageButton _cancelImageButton;
            private LinkButton _cancelLinkButton;
            private Button _cancelPushButton;
            private ImageButton _changePasswordImageButton;
            private LinkButton _changePasswordLinkButton;
            private Button _changePasswordPushButton;
            private LabelLiteral _confirmNewPasswordLabel;
            private RequiredFieldValidator _confirmNewPasswordRequired;
            private Control _confirmNewPasswordTextBox;
            private Image _createUserIcon;
            private HyperLink _createUserLink;
            private LiteralControl _createUserLinkSeparator;
            private LabelLiteral _currentPasswordLabel;
            private Control _currentPasswordTextBox;
            private Image _editProfileIcon;
            private HyperLink _editProfileLink;
            private LiteralControl _editProfileLinkSeparator;
            private Control _failureTextLabel;
            private Image _helpPageIcon;
            private HyperLink _helpPageLink;
            private LiteralControl _helpPageLinkSeparator;
            private Literal _instruction;
            private CompareValidator _newPasswordCompareValidator;
            private LabelLiteral _newPasswordLabel;
            private RequiredFieldValidator _newPasswordRequired;
            private Control _newPasswordTextBox;
            private Literal _passwordHintLabel;
            private Image _passwordRecoveryIcon;
            private HyperLink _passwordRecoveryLink;
            private RequiredFieldValidator _passwordRequired;
            private RegularExpressionValidator _regExpValidator;
            private Literal _title;
            private LabelLiteral _userNameLabel;
            private RequiredFieldValidator _userNameRequired;
            private Control _userNameTextBox;

            public ChangePasswordContainer(ChangePassword owner) : base(owner)
            {
            }

            internal ImageButton CancelImageButton
            {
                get
                {
                    return this._cancelImageButton;
                }
                set
                {
                    this._cancelImageButton = value;
                }
            }

            internal LinkButton CancelLinkButton
            {
                get
                {
                    return this._cancelLinkButton;
                }
                set
                {
                    this._cancelLinkButton = value;
                }
            }

            internal Button CancelPushButton
            {
                get
                {
                    return this._cancelPushButton;
                }
                set
                {
                    this._cancelPushButton = value;
                }
            }

            internal ImageButton ChangePasswordImageButton
            {
                get
                {
                    return this._changePasswordImageButton;
                }
                set
                {
                    this._changePasswordImageButton = value;
                }
            }

            internal LinkButton ChangePasswordLinkButton
            {
                get
                {
                    return this._changePasswordLinkButton;
                }
                set
                {
                    this._changePasswordLinkButton = value;
                }
            }

            internal Button ChangePasswordPushButton
            {
                get
                {
                    return this._changePasswordPushButton;
                }
                set
                {
                    this._changePasswordPushButton = value;
                }
            }

            internal LabelLiteral ConfirmNewPasswordLabel
            {
                get
                {
                    return this._confirmNewPasswordLabel;
                }
                set
                {
                    this._confirmNewPasswordLabel = value;
                }
            }

            internal RequiredFieldValidator ConfirmNewPasswordRequired
            {
                get
                {
                    return this._confirmNewPasswordRequired;
                }
                set
                {
                    this._confirmNewPasswordRequired = value;
                }
            }

            internal Control ConfirmNewPasswordTextBox
            {
                get
                {
                    if (this._confirmNewPasswordTextBox != null)
                    {
                        return this._confirmNewPasswordTextBox;
                    }
                    return base.FindOptionalControl<IEditableTextControl>("ConfirmNewPassword");
                }
                set
                {
                    this._confirmNewPasswordTextBox = value;
                }
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

            internal LabelLiteral CurrentPasswordLabel
            {
                get
                {
                    return this._currentPasswordLabel;
                }
                set
                {
                    this._currentPasswordLabel = value;
                }
            }

            internal Control CurrentPasswordTextBox
            {
                get
                {
                    if (this._currentPasswordTextBox != null)
                    {
                        return this._currentPasswordTextBox;
                    }
                    return base.FindRequiredControl<IEditableTextControl>("CurrentPassword", "ChangePassword_NoCurrentPasswordTextBox");
                }
                set
                {
                    this._currentPasswordTextBox = value;
                }
            }

            internal Image EditProfileIcon
            {
                get
                {
                    return this._editProfileIcon;
                }
                set
                {
                    this._editProfileIcon = value;
                }
            }

            internal HyperLink EditProfileLink
            {
                get
                {
                    return this._editProfileLink;
                }
                set
                {
                    this._editProfileLink = value;
                }
            }

            internal LiteralControl EditProfileLinkSeparator
            {
                get
                {
                    return this._editProfileLinkSeparator;
                }
                set
                {
                    this._editProfileLinkSeparator = value;
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

            internal LiteralControl HelpPageLinkSeparator
            {
                get
                {
                    return this._helpPageLinkSeparator;
                }
                set
                {
                    this._helpPageLinkSeparator = value;
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

            internal CompareValidator NewPasswordCompareValidator
            {
                get
                {
                    return this._newPasswordCompareValidator;
                }
                set
                {
                    this._newPasswordCompareValidator = value;
                }
            }

            internal LabelLiteral NewPasswordLabel
            {
                get
                {
                    return this._newPasswordLabel;
                }
                set
                {
                    this._newPasswordLabel = value;
                }
            }

            internal RequiredFieldValidator NewPasswordRequired
            {
                get
                {
                    return this._newPasswordRequired;
                }
                set
                {
                    this._newPasswordRequired = value;
                }
            }

            internal Control NewPasswordTextBox
            {
                get
                {
                    if (this._newPasswordTextBox != null)
                    {
                        return this._newPasswordTextBox;
                    }
                    return base.FindRequiredControl<IEditableTextControl>("NewPassword", "ChangePassword_NoNewPasswordTextBox");
                }
                set
                {
                    this._newPasswordTextBox = value;
                }
            }

            internal Literal PasswordHintLabel
            {
                get
                {
                    return this._passwordHintLabel;
                }
                set
                {
                    this._passwordHintLabel = value;
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

            internal RegularExpressionValidator RegExpValidator
            {
                get
                {
                    return this._regExpValidator;
                }
                set
                {
                    this._regExpValidator = value;
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
                    if (base.Owner.DisplayUserName)
                    {
                        return base.FindRequiredControl<IEditableTextControl>("UserName", "ChangePassword_NoUserNameTextBox");
                    }
                    base.VerifyControlNotPresent<IEditableTextControl>("UserName", "ChangePassword_UserNameTextBoxNotAllowed");
                    return null;
                }
                set
                {
                    this._userNameTextBox = value;
                }
            }
        }

        private sealed class DefaultChangePasswordTemplate : ITemplate
        {
            private ChangePassword _owner;

            public DefaultChangePasswordTemplate(ChangePassword owner)
            {
                this._owner = owner;
            }

            private void CreateControls(ChangePassword.ChangePasswordContainer container)
            {
                string uniqueID = this._owner.UniqueID;
                container.Title = new Literal();
                container.Instruction = new Literal();
                container.PasswordHintLabel = new Literal();
                TextBox forControl = new TextBox {
                    ID = "UserName"
                };
                container.UserNameTextBox = forControl;
                container.UserNameLabel = new LabelLiteral(forControl);
                bool enableValidation = this._owner.CurrentView == ChangePassword.View.ChangePassword;
                container.UserNameRequired = this.CreateRequiredFieldValidator("UserNameRequired", forControl, uniqueID, enableValidation);
                TextBox box2 = new TextBox {
                    ID = "CurrentPassword",
                    TextMode = TextBoxMode.Password
                };
                container.CurrentPasswordTextBox = box2;
                container.CurrentPasswordLabel = new LabelLiteral(box2);
                container.PasswordRequired = this.CreateRequiredFieldValidator("CurrentPasswordRequired", box2, uniqueID, enableValidation);
                TextBox box3 = new TextBox {
                    ID = "NewPassword",
                    TextMode = TextBoxMode.Password
                };
                container.NewPasswordTextBox = box3;
                container.NewPasswordLabel = new LabelLiteral(box3);
                container.NewPasswordRequired = this.CreateRequiredFieldValidator("NewPasswordRequired", box3, uniqueID, enableValidation);
                TextBox box4 = new TextBox {
                    ID = "ConfirmNewPassword",
                    TextMode = TextBoxMode.Password
                };
                container.ConfirmNewPasswordTextBox = box4;
                container.ConfirmNewPasswordLabel = new LabelLiteral(box4);
                container.ConfirmNewPasswordRequired = this.CreateRequiredFieldValidator("ConfirmNewPasswordRequired", box4, uniqueID, enableValidation);
                CompareValidator validator = new CompareValidator {
                    ID = "NewPasswordCompare",
                    ValidationGroup = uniqueID,
                    ControlToValidate = "ConfirmNewPassword",
                    ControlToCompare = "NewPassword",
                    Operator = ValidationCompareOperator.Equal,
                    ErrorMessage = this._owner.ConfirmPasswordCompareErrorMessage,
                    Display = ValidatorDisplay.Dynamic,
                    Enabled = enableValidation,
                    Visible = enableValidation
                };
                container.NewPasswordCompareValidator = validator;
                RegularExpressionValidator validator2 = new RegularExpressionValidator {
                    ID = "NewPasswordRegExp",
                    ValidationGroup = uniqueID,
                    ControlToValidate = "NewPassword",
                    ErrorMessage = this._owner.NewPasswordRegularExpressionErrorMessage,
                    ValidationExpression = this._owner.NewPasswordRegularExpression,
                    Display = ValidatorDisplay.Dynamic,
                    Enabled = enableValidation,
                    Visible = enableValidation
                };
                container.RegExpValidator = validator2;
                LinkButton button = new LinkButton {
                    ID = "ChangePasswordLinkButton",
                    ValidationGroup = uniqueID,
                    CommandName = ChangePassword.ChangePasswordButtonCommandName
                };
                container.ChangePasswordLinkButton = button;
                button = new LinkButton {
                    ID = "CancelLinkButton",
                    CausesValidation = false,
                    CommandName = ChangePassword.CancelButtonCommandName
                };
                container.CancelLinkButton = button;
                ImageButton button2 = new ImageButton {
                    ID = "ChangePasswordImageButton",
                    ValidationGroup = uniqueID,
                    CommandName = ChangePassword.ChangePasswordButtonCommandName
                };
                container.ChangePasswordImageButton = button2;
                button2 = new ImageButton {
                    ID = "CancelImageButton",
                    CommandName = ChangePassword.CancelButtonCommandName,
                    CausesValidation = false
                };
                container.CancelImageButton = button2;
                Button button3 = new Button {
                    ID = "ChangePasswordPushButton",
                    ValidationGroup = uniqueID,
                    CommandName = ChangePassword.ChangePasswordButtonCommandName
                };
                container.ChangePasswordPushButton = button3;
                button3 = new Button {
                    ID = "CancelPushButton",
                    CommandName = ChangePassword.CancelButtonCommandName,
                    CausesValidation = false
                };
                container.CancelPushButton = button3;
                container.PasswordRecoveryIcon = new Image();
                container.PasswordRecoveryLink = new HyperLink();
                container.PasswordRecoveryLink.ID = "PasswordRecoveryLink";
                container.CreateUserIcon = new Image();
                container.CreateUserLink = new HyperLink();
                container.CreateUserLink.ID = "CreateUserLink";
                container.CreateUserLinkSeparator = new LiteralControl();
                container.HelpPageIcon = new Image();
                container.HelpPageLink = new HyperLink();
                container.HelpPageLink.ID = "HelpLink";
                container.HelpPageLinkSeparator = new LiteralControl();
                container.EditProfileLink = new HyperLink();
                container.EditProfileLink.ID = "EditProfileLink";
                container.EditProfileIcon = new Image();
                container.EditProfileLinkSeparator = new LiteralControl();
                Literal literal = new Literal {
                    ID = "FailureText"
                };
                container.FailureTextLabel = literal;
            }

            private RequiredFieldValidator CreateRequiredFieldValidator(string id, TextBox textBox, string validationGroup, bool enableValidation)
            {
                return new RequiredFieldValidator { ID = id, ValidationGroup = validationGroup, ControlToValidate = textBox.ID, Display = ValidatorDisplay.Static, Text = System.Web.SR.GetString("LoginControls_DefaultRequiredFieldValidatorText"), Enabled = enableValidation, Visible = enableValidation };
            }

            private void LayoutControls(ChangePassword.ChangePasswordContainer container)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(container.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(container.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                if (this._owner.ConvertingToTemplate)
                {
                    container.UserNameLabel.RenderAsLabel = true;
                }
                cell.Controls.Add(container.UserNameLabel);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(container.UserNameTextBox);
                cell.Controls.Add(container.UserNameRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                this._owner._userNameTableRow = row;
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(container.CurrentPasswordLabel);
                if (this._owner.ConvertingToTemplate)
                {
                    container.CurrentPasswordLabel.RenderAsLabel = true;
                }
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(container.CurrentPasswordTextBox);
                cell.Controls.Add(container.PasswordRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(container.NewPasswordLabel);
                if (this._owner.ConvertingToTemplate)
                {
                    container.NewPasswordLabel.RenderAsLabel = true;
                }
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(container.NewPasswordTextBox);
                cell.Controls.Add(container.NewPasswordRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(container.PasswordHintLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                this._owner._passwordHintTableRow = row;
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(container.ConfirmNewPasswordLabel);
                if (this._owner.ConvertingToTemplate)
                {
                    container.ConfirmNewPasswordLabel.RenderAsLabel = true;
                }
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(container.ConfirmNewPasswordTextBox);
                cell.Controls.Add(container.ConfirmNewPasswordRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center,
                    ColumnSpan = 2
                };
                cell.Controls.Add(container.NewPasswordCompareValidator);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                if (this._owner.RegExpEnabled)
                {
                    row = new LoginUtil.DisappearingTableRow();
                    cell = new TableCell {
                        HorizontalAlign = HorizontalAlign.Center,
                        ColumnSpan = 2
                    };
                    cell.Controls.Add(container.RegExpValidator);
                    row.Cells.Add(cell);
                    child.Rows.Add(row);
                }
                this._owner.ValidatorRow = row;
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center,
                    ColumnSpan = 2
                };
                cell.Controls.Add(container.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(container.ChangePasswordLinkButton);
                cell.Controls.Add(container.ChangePasswordImageButton);
                cell.Controls.Add(container.ChangePasswordPushButton);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(container.CancelLinkButton);
                cell.Controls.Add(container.CancelImageButton);
                cell.Controls.Add(container.CancelPushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2
                };
                cell.Controls.Add(container.HelpPageIcon);
                cell.Controls.Add(container.HelpPageLink);
                cell.Controls.Add(container.HelpPageLinkSeparator);
                cell.Controls.Add(container.CreateUserIcon);
                cell.Controls.Add(container.CreateUserLink);
                container.HelpPageLinkSeparator.Text = "<br />";
                container.CreateUserLinkSeparator.Text = "<br />";
                container.EditProfileLinkSeparator.Text = "<br />";
                cell.Controls.Add(container.CreateUserLinkSeparator);
                cell.Controls.Add(container.PasswordRecoveryIcon);
                cell.Controls.Add(container.PasswordRecoveryLink);
                cell.Controls.Add(container.EditProfileLinkSeparator);
                cell.Controls.Add(container.EditProfileIcon);
                cell.Controls.Add(container.EditProfileLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                container.LayoutTable = child;
                container.BorderTable = table2;
                container.Controls.Add(table2);
            }

            void ITemplate.InstantiateIn(Control container)
            {
                ChangePassword.ChangePasswordContainer container2 = (ChangePassword.ChangePasswordContainer) container;
                this.CreateControls(container2);
                this.LayoutControls(container2);
            }
        }

        private sealed class DefaultSuccessTemplate : ITemplate
        {
            private ChangePassword _owner;

            public DefaultSuccessTemplate(ChangePassword owner)
            {
                this._owner = owner;
            }

            private void CreateControls(ChangePassword.SuccessContainer successContainer)
            {
                successContainer.Title = new Literal();
                successContainer.SuccessTextLabel = new Literal();
                successContainer.EditProfileLink = new HyperLink();
                successContainer.EditProfileLink.ID = "EditProfileLinkSuccess";
                successContainer.EditProfileIcon = new Image();
                LinkButton button = new LinkButton {
                    ID = "ContinueLinkButton",
                    CommandName = ChangePassword.ContinueButtonCommandName,
                    CausesValidation = false
                };
                successContainer.ContinueLinkButton = button;
                ImageButton button2 = new ImageButton {
                    ID = "ContinueImageButton",
                    CommandName = ChangePassword.ContinueButtonCommandName,
                    CausesValidation = false
                };
                successContainer.ContinueImageButton = button2;
                Button button3 = new Button {
                    ID = "ContinuePushButton",
                    CommandName = ChangePassword.ContinueButtonCommandName,
                    CausesValidation = false
                };
                successContainer.ContinuePushButton = button3;
            }

            private void LayoutControls(ChangePassword.SuccessContainer successContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(successContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(successContainer.SuccessTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(successContainer.ContinuePushButton);
                cell.Controls.Add(successContainer.ContinueLinkButton);
                cell.Controls.Add(successContainer.ContinueImageButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2
                };
                cell.Controls.Add(successContainer.EditProfileIcon);
                cell.Controls.Add(successContainer.EditProfileLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                successContainer.LayoutTable = child;
                successContainer.BorderTable = table2;
                successContainer.Controls.Add(table2);
            }

            void ITemplate.InstantiateIn(Control container)
            {
                ChangePassword.SuccessContainer successContainer = (ChangePassword.SuccessContainer) container;
                this.CreateControls(successContainer);
                this.LayoutControls(successContainer);
            }
        }

        internal sealed class SuccessContainer : LoginUtil.GenericContainer<ChangePassword>, INonBindingContainer, INamingContainer
        {
            private ImageButton _continueImageButton;
            private LinkButton _continueLinkButton;
            private Button _continuePushButton;
            private Image _editProfileIcon;
            private HyperLink _editProfileLink;
            private Literal _successTextLabel;
            private Literal _title;

            public SuccessContainer(ChangePassword owner) : base(owner)
            {
            }

            internal ImageButton ContinueImageButton
            {
                get
                {
                    return this._continueImageButton;
                }
                set
                {
                    this._continueImageButton = value;
                }
            }

            internal LinkButton ContinueLinkButton
            {
                get
                {
                    return this._continueLinkButton;
                }
                set
                {
                    this._continueLinkButton = value;
                }
            }

            internal Button ContinuePushButton
            {
                get
                {
                    return this._continuePushButton;
                }
                set
                {
                    this._continuePushButton = value;
                }
            }

            protected override bool ConvertingToTemplate
            {
                get
                {
                    return base.Owner.ConvertingToTemplate;
                }
            }

            internal Image EditProfileIcon
            {
                get
                {
                    return this._editProfileIcon;
                }
                set
                {
                    this._editProfileIcon = value;
                }
            }

            internal HyperLink EditProfileLink
            {
                get
                {
                    return this._editProfileLink;
                }
                set
                {
                    this._editProfileLink = value;
                }
            }

            public Literal SuccessTextLabel
            {
                get
                {
                    return this._successTextLabel;
                }
                set
                {
                    this._successTextLabel = value;
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
        }

        internal enum View
        {
            ChangePassword,
            Success
        }
    }
}

