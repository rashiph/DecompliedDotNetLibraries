namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Security;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.CreateUserWizardDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false), ToolboxData("<{0}:CreateUserWizard runat=\"server\"> <WizardSteps> <asp:CreateUserWizardStep runat=\"server\"/> <asp:CompleteWizardStep runat=\"server\"/> </WizardSteps> </{0}:CreateUserWizard>"), DefaultEvent("CreatedUser")]
    public class CreateUserWizard : Wizard
    {
        private string _answer;
        private const string _answerID = "Answer";
        private const string _answerRequiredID = "AnswerRequired";
        private TableRow _answerRow;
        private const ValidatorDisplay _compareFieldValidatorDisplay = ValidatorDisplay.Dynamic;
        private CompleteWizardStep _completeStep;
        private CompleteStepContainer _completeStepContainer;
        private const string _completeStepContainerID = "CompleteStepContainer";
        private TableItemStyle _completeSuccessTextStyle;
        private string _confirmPassword;
        private const string _confirmPasswordID = "ConfirmPassword";
        private const string _confirmPasswordRequiredID = "ConfirmPasswordRequired";
        private TableRow _confirmPasswordTableRow;
        private const string _continueButtonID = "ContinueButton";
        private Style _continueButtonStyle;
        private bool _convertingToTemplate;
        private Style _createUserButtonStyle;
        private const string _createUserNavigationTemplateName = "CreateUserNavigationTemplate";
        private CreateUserWizardStep _createUserStep;
        private CreateUserStepContainer _createUserStepContainer;
        private const string _createUserStepContainerID = "CreateUserStepContainer";
        private DefaultCreateUserNavigationTemplate _defaultCreateUserNavigationTemplate;
        private const bool _displaySideBarDefaultValue = false;
        private const string _editProfileLinkID = "EditProfileLink";
        private const string _emailID = "Email";
        private const string _emailRegExpID = "EmailRegExp";
        private TableRow _emailRegExpRow;
        private const string _emailRequiredID = "EmailRequired";
        private TableRow _emailRow;
        private const string _errorMessageID = "ErrorMessage";
        private TableItemStyle _errorMessageStyle;
        private bool _failure;
        private const string _helpLinkID = "HelpLink";
        private TableItemStyle _hyperLinkStyle;
        private TableItemStyle _instructionTextStyle;
        private TableItemStyle _labelStyle;
        private System.Web.UI.WebControls.MailDefinition _mailDefinition;
        private string _password;
        private const string _passwordCompareID = "PasswordCompare";
        private TableRow _passwordCompareRow;
        private TableItemStyle _passwordHintStyle;
        private TableRow _passwordHintTableRow;
        private const string _passwordID = "Password";
        private const string _passwordRegExpID = "PasswordRegExp";
        private TableRow _passwordRegExpRow;
        private const string _passwordReplacementKey = @"<%\s*Password\s*%>";
        private const string _passwordRequiredID = "PasswordRequired";
        private TableRow _passwordTableRow;
        private const string _questionID = "Question";
        private const string _questionRequiredID = "QuestionRequired";
        private TableRow _questionRow;
        private const ValidatorDisplay _regexpFieldValidatorDisplay = ValidatorDisplay.Dynamic;
        private const ValidatorDisplay _requiredFieldValidatorDisplay = ValidatorDisplay.Static;
        private const string _sideBarLabelID = "SideBarLabel";
        private Style _textBoxStyle;
        private TableItemStyle _titleTextStyle;
        private string _unknownErrorMessage;
        private const string _userNameID = "UserName";
        private const string _userNameReplacementKey = @"<%\s*UserName\s*%>";
        private const string _userNameRequiredID = "UserNameRequired";
        private string _validationGroup;
        private Style _validatorTextStyle;
        private const int _viewStateArrayLength = 13;
        public static readonly string ContinueButtonCommandName = "Continue";
        private static readonly object EventButtonContinueClick = new object();
        private static readonly object EventCreatedUser = new object();
        private static readonly object EventCreateUserError = new object();
        private static readonly object EventCreatingUser = new object();
        private static readonly object EventSendingMail = new object();
        private static readonly object EventSendMailError = new object();

        [WebCategory("Action"), WebSysDescription("CreateUserWizard_ContinueButtonClick")]
        public event EventHandler ContinueButtonClick
        {
            add
            {
                base.Events.AddHandler(EventButtonContinueClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventButtonContinueClick, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("CreateUserWizard_CreatedUser")]
        public event EventHandler CreatedUser
        {
            add
            {
                base.Events.AddHandler(EventCreatedUser, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCreatedUser, value);
            }
        }

        [WebSysDescription("CreateUserWizard_CreateUserError"), WebCategory("Action")]
        public event CreateUserErrorEventHandler CreateUserError
        {
            add
            {
                base.Events.AddHandler(EventCreateUserError, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCreateUserError, value);
            }
        }

        [WebSysDescription("CreateUserWizard_CreatingUser"), WebCategory("Action")]
        public event LoginCancelEventHandler CreatingUser
        {
            add
            {
                base.Events.AddHandler(EventCreatingUser, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCreatingUser, value);
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

        [WebCategory("Action"), WebSysDescription("CreateUserWizard_SendMailError")]
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

        public CreateUserWizard() : base(false)
        {
        }

        private void AnswerTextChanged(object source, EventArgs e)
        {
            this.Answer = ((ITextControl) source).Text;
        }

        private void ApplyCommonCreateUserValues()
        {
            if (!string.IsNullOrEmpty(this.UserNameInternal))
            {
                ITextControl userNameTextBox = (ITextControl) this._createUserStepContainer.UserNameTextBox;
                if (userNameTextBox != null)
                {
                    userNameTextBox.Text = this.UserNameInternal;
                }
            }
            if (!string.IsNullOrEmpty(this.EmailInternal))
            {
                ITextControl emailTextBox = (ITextControl) this._createUserStepContainer.EmailTextBox;
                if (emailTextBox != null)
                {
                    emailTextBox.Text = this.EmailInternal;
                }
            }
            if (!string.IsNullOrEmpty(this.QuestionInternal))
            {
                ITextControl questionTextBox = (ITextControl) this._createUserStepContainer.QuestionTextBox;
                if (questionTextBox != null)
                {
                    questionTextBox.Text = this.QuestionInternal;
                }
            }
            if (!string.IsNullOrEmpty(this.AnswerInternal))
            {
                ITextControl answerTextBox = (ITextControl) this._createUserStepContainer.AnswerTextBox;
                if (answerTextBox != null)
                {
                    answerTextBox.Text = this.AnswerInternal;
                }
            }
        }

        private void ApplyCompleteValues()
        {
            LoginUtil.ApplyStyleToLiteral(this._completeStepContainer.SuccessTextLabel, this.CompleteSuccessText, this._completeSuccessTextStyle, true);
            switch (this.ContinueButtonType)
            {
                case ButtonType.Button:
                    this._completeStepContainer.ContinueLinkButton.Visible = false;
                    this._completeStepContainer.ContinueImageButton.Visible = false;
                    this._completeStepContainer.ContinuePushButton.Text = this.ContinueButtonText;
                    this._completeStepContainer.ContinuePushButton.ValidationGroup = this.ValidationGroup;
                    this._completeStepContainer.ContinuePushButton.TabIndex = this.TabIndex;
                    this._completeStepContainer.ContinuePushButton.AccessKey = this.AccessKey;
                    break;

                case ButtonType.Image:
                    this._completeStepContainer.ContinueLinkButton.Visible = false;
                    this._completeStepContainer.ContinuePushButton.Visible = false;
                    this._completeStepContainer.ContinueImageButton.ImageUrl = this.ContinueButtonImageUrl;
                    this._completeStepContainer.ContinueImageButton.AlternateText = this.ContinueButtonText;
                    this._completeStepContainer.ContinueImageButton.ValidationGroup = this.ValidationGroup;
                    this._completeStepContainer.ContinueImageButton.TabIndex = this.TabIndex;
                    this._completeStepContainer.ContinueImageButton.AccessKey = this.AccessKey;
                    break;

                case ButtonType.Link:
                    this._completeStepContainer.ContinuePushButton.Visible = false;
                    this._completeStepContainer.ContinueImageButton.Visible = false;
                    this._completeStepContainer.ContinueLinkButton.Text = this.ContinueButtonText;
                    this._completeStepContainer.ContinueLinkButton.ValidationGroup = this.ValidationGroup;
                    this._completeStepContainer.ContinueLinkButton.TabIndex = this.TabIndex;
                    this._completeStepContainer.ContinueLinkButton.AccessKey = this.AccessKey;
                    break;
            }
            if (!base.NavigationButtonStyle.IsEmpty)
            {
                this._completeStepContainer.ContinuePushButton.ApplyStyle(base.NavigationButtonStyle);
                this._completeStepContainer.ContinueImageButton.ApplyStyle(base.NavigationButtonStyle);
                this._completeStepContainer.ContinueLinkButton.ApplyStyle(base.NavigationButtonStyle);
            }
            if (this._continueButtonStyle != null)
            {
                this._completeStepContainer.ContinuePushButton.ApplyStyle(this._continueButtonStyle);
                this._completeStepContainer.ContinueImageButton.ApplyStyle(this._continueButtonStyle);
                this._completeStepContainer.ContinueLinkButton.ApplyStyle(this._continueButtonStyle);
            }
            LoginUtil.ApplyStyleToLiteral(this._completeStepContainer.Title, this.CompleteStep.Title, this._titleTextStyle, true);
            string editProfileText = this.EditProfileText;
            bool flag = editProfileText.Length > 0;
            HyperLink editProfileLink = this._completeStepContainer.EditProfileLink;
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
            string editProfileIconUrl = this.EditProfileIconUrl;
            bool flag2 = editProfileIconUrl.Length > 0;
            Image editProfileIcon = this._completeStepContainer.EditProfileIcon;
            editProfileIcon.Visible = flag2;
            if (flag2)
            {
                editProfileIcon.ImageUrl = editProfileIconUrl;
                editProfileIcon.AlternateText = this.EditProfileText;
            }
            LoginUtil.SetTableCellVisible(editProfileLink, flag || flag2);
            Table layoutTable = ((CompleteStepContainer) this.CompleteStep.ContentTemplateContainer).LayoutTable;
            layoutTable.Height = this.Height;
            layoutTable.Width = this.Width;
        }

        private void ApplyDefaultCreateUserValues()
        {
            this._createUserStepContainer.UserNameLabel.Text = this.UserNameLabelText;
            WebControl userNameTextBox = (WebControl) this._createUserStepContainer.UserNameTextBox;
            userNameTextBox.TabIndex = this.TabIndex;
            userNameTextBox.AccessKey = this.AccessKey;
            this._createUserStepContainer.PasswordLabel.Text = this.PasswordLabelText;
            WebControl passwordTextBox = (WebControl) this._createUserStepContainer.PasswordTextBox;
            passwordTextBox.TabIndex = this.TabIndex;
            this._createUserStepContainer.ConfirmPasswordLabel.Text = this.ConfirmPasswordLabelText;
            WebControl confirmPasswordTextBox = (WebControl) this._createUserStepContainer.ConfirmPasswordTextBox;
            confirmPasswordTextBox.TabIndex = this.TabIndex;
            if (this._textBoxStyle != null)
            {
                userNameTextBox.ApplyStyle(this._textBoxStyle);
                passwordTextBox.ApplyStyle(this._textBoxStyle);
                confirmPasswordTextBox.ApplyStyle(this._textBoxStyle);
            }
            LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.Title, this.CreateUserStep.Title, this.TitleTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.InstructionLabel, this.InstructionText, this.InstructionTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.UserNameLabel, this.UserNameLabelText, this.LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.PasswordLabel, this.PasswordLabelText, this.LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.ConfirmPasswordLabel, this.ConfirmPasswordLabelText, this.LabelStyle, false);
            if (!string.IsNullOrEmpty(this.PasswordHintText) && !this.AutoGeneratePassword)
            {
                LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.PasswordHintLabel, this.PasswordHintText, this.PasswordHintStyle, false);
            }
            else
            {
                this._passwordHintTableRow.Visible = false;
            }
            bool flag = true;
            WebControl emailTextBox = null;
            if (this.RequireEmail)
            {
                LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.EmailLabel, this.EmailLabelText, this.LabelStyle, false);
                emailTextBox = (WebControl) this._createUserStepContainer.EmailTextBox;
                ((ITextControl) emailTextBox).Text = this.Email;
                RequiredFieldValidator emailRequired = this._createUserStepContainer.EmailRequired;
                emailRequired.ToolTip = this.EmailRequiredErrorMessage;
                emailRequired.ErrorMessage = this.EmailRequiredErrorMessage;
                emailRequired.Enabled = flag;
                emailRequired.Visible = flag;
                if (this._validatorTextStyle != null)
                {
                    emailRequired.ApplyStyle(this._validatorTextStyle);
                }
                emailTextBox.TabIndex = this.TabIndex;
                if (this._textBoxStyle != null)
                {
                    emailTextBox.ApplyStyle(this._textBoxStyle);
                }
            }
            else
            {
                this._emailRow.Visible = false;
            }
            WebControl questionTextBox = null;
            WebControl answerTextBox = null;
            RequiredFieldValidator questionRequired = this._createUserStepContainer.QuestionRequired;
            RequiredFieldValidator answerRequired = this._createUserStepContainer.AnswerRequired;
            bool flag2 = flag && this.QuestionAndAnswerRequired;
            questionRequired.Enabled = flag2;
            questionRequired.Visible = flag2;
            answerRequired.Enabled = flag2;
            answerRequired.Visible = flag2;
            if (this.QuestionAndAnswerRequired)
            {
                LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.QuestionLabel, this.QuestionLabelText, this.LabelStyle, false);
                questionTextBox = (WebControl) this._createUserStepContainer.QuestionTextBox;
                ((ITextControl) questionTextBox).Text = this.Question;
                questionTextBox.TabIndex = this.TabIndex;
                LoginUtil.ApplyStyleToLiteral(this._createUserStepContainer.AnswerLabel, this.AnswerLabelText, this.LabelStyle, false);
                answerTextBox = (WebControl) this._createUserStepContainer.AnswerTextBox;
                ((ITextControl) answerTextBox).Text = this.Answer;
                answerTextBox.TabIndex = this.TabIndex;
                if (this._textBoxStyle != null)
                {
                    questionTextBox.ApplyStyle(this._textBoxStyle);
                    answerTextBox.ApplyStyle(this._textBoxStyle);
                }
                questionRequired.ToolTip = this.QuestionRequiredErrorMessage;
                questionRequired.ErrorMessage = this.QuestionRequiredErrorMessage;
                answerRequired.ToolTip = this.AnswerRequiredErrorMessage;
                answerRequired.ErrorMessage = this.AnswerRequiredErrorMessage;
                if (this._validatorTextStyle != null)
                {
                    questionRequired.ApplyStyle(this._validatorTextStyle);
                    answerRequired.ApplyStyle(this._validatorTextStyle);
                }
            }
            else
            {
                this._questionRow.Visible = false;
                this._answerRow.Visible = false;
            }
            if (this._defaultCreateUserNavigationTemplate != null)
            {
                ((Wizard.BaseNavigationTemplateContainer) this.CreateUserStep.CustomNavigationTemplateContainer).NextButton = this._defaultCreateUserNavigationTemplate.CreateUserButton;
                ((Wizard.BaseNavigationTemplateContainer) this.CreateUserStep.CustomNavigationTemplateContainer).CancelButton = this._defaultCreateUserNavigationTemplate.CancelButton;
            }
            RequiredFieldValidator passwordRequired = this._createUserStepContainer.PasswordRequired;
            RequiredFieldValidator confirmPasswordRequired = this._createUserStepContainer.ConfirmPasswordRequired;
            CompareValidator passwordCompareValidator = this._createUserStepContainer.PasswordCompareValidator;
            RegularExpressionValidator passwordRegExpValidator = this._createUserStepContainer.PasswordRegExpValidator;
            bool flag3 = flag && !this.AutoGeneratePassword;
            passwordRequired.Enabled = flag3;
            passwordRequired.Visible = flag3;
            confirmPasswordRequired.Enabled = flag3;
            confirmPasswordRequired.Visible = flag3;
            passwordCompareValidator.Enabled = flag3;
            passwordCompareValidator.Visible = flag3;
            bool flag4 = flag3 && (this.PasswordRegularExpression.Length > 0);
            passwordRegExpValidator.Enabled = flag4;
            passwordRegExpValidator.Visible = flag4;
            if (!flag)
            {
                this._passwordRegExpRow.Visible = false;
                this._passwordCompareRow.Visible = false;
                this._emailRegExpRow.Visible = false;
            }
            if (this.AutoGeneratePassword)
            {
                this._passwordTableRow.Visible = false;
                this._confirmPasswordTableRow.Visible = false;
                this._passwordRegExpRow.Visible = false;
                this._passwordCompareRow.Visible = false;
            }
            else
            {
                passwordRequired.ErrorMessage = this.PasswordRequiredErrorMessage;
                passwordRequired.ToolTip = this.PasswordRequiredErrorMessage;
                confirmPasswordRequired.ErrorMessage = this.ConfirmPasswordRequiredErrorMessage;
                confirmPasswordRequired.ToolTip = this.ConfirmPasswordRequiredErrorMessage;
                passwordCompareValidator.ErrorMessage = this.ConfirmPasswordCompareErrorMessage;
                if (this._validatorTextStyle != null)
                {
                    passwordRequired.ApplyStyle(this._validatorTextStyle);
                    confirmPasswordRequired.ApplyStyle(this._validatorTextStyle);
                    passwordCompareValidator.ApplyStyle(this._validatorTextStyle);
                }
                if (flag4)
                {
                    passwordRegExpValidator.ValidationExpression = this.PasswordRegularExpression;
                    passwordRegExpValidator.ErrorMessage = this.PasswordRegularExpressionErrorMessage;
                    if (this._validatorTextStyle != null)
                    {
                        passwordRegExpValidator.ApplyStyle(this._validatorTextStyle);
                    }
                }
                else
                {
                    this._passwordRegExpRow.Visible = false;
                }
            }
            RequiredFieldValidator userNameRequired = this._createUserStepContainer.UserNameRequired;
            userNameRequired.ErrorMessage = this.UserNameRequiredErrorMessage;
            userNameRequired.ToolTip = this.UserNameRequiredErrorMessage;
            userNameRequired.Enabled = flag;
            userNameRequired.Visible = flag;
            if (this._validatorTextStyle != null)
            {
                userNameRequired.ApplyStyle(this._validatorTextStyle);
            }
            bool flag5 = (flag && (this.EmailRegularExpression.Length > 0)) && this.RequireEmail;
            RegularExpressionValidator emailRegExpValidator = this._createUserStepContainer.EmailRegExpValidator;
            emailRegExpValidator.Enabled = flag5;
            emailRegExpValidator.Visible = flag5;
            if ((this.EmailRegularExpression.Length > 0) && this.RequireEmail)
            {
                emailRegExpValidator.ValidationExpression = this.EmailRegularExpression;
                emailRegExpValidator.ErrorMessage = this.EmailRegularExpressionErrorMessage;
                if (this._validatorTextStyle != null)
                {
                    emailRegExpValidator.ApplyStyle(this._validatorTextStyle);
                }
            }
            else
            {
                this._emailRegExpRow.Visible = false;
            }
            string helpPageText = this.HelpPageText;
            bool flag6 = helpPageText.Length > 0;
            HyperLink helpPageLink = this._createUserStepContainer.HelpPageLink;
            Image helpPageIcon = this._createUserStepContainer.HelpPageIcon;
            helpPageLink.Visible = flag6;
            if (flag6)
            {
                helpPageLink.Text = helpPageText;
                helpPageLink.NavigateUrl = this.HelpPageUrl;
                helpPageLink.TabIndex = this.TabIndex;
            }
            string helpPageIconUrl = this.HelpPageIconUrl;
            bool flag7 = helpPageIconUrl.Length > 0;
            helpPageIcon.Visible = flag7;
            if (flag7)
            {
                helpPageIcon.ImageUrl = helpPageIconUrl;
                helpPageIcon.AlternateText = helpPageText;
            }
            LoginUtil.SetTableCellVisible(helpPageLink, flag6 || flag7);
            if ((this._hyperLinkStyle != null) && (flag6 || flag7))
            {
                TableItemStyle style = new TableItemStyle();
                style.CopyFrom(this._hyperLinkStyle);
                style.Font.Reset();
                LoginUtil.SetTableCellStyle(helpPageLink, style);
                helpPageLink.Font.CopyFrom(this._hyperLinkStyle.Font);
                helpPageLink.ForeColor = this._hyperLinkStyle.ForeColor;
            }
            Control errorMessageLabel = this._createUserStepContainer.ErrorMessageLabel;
            if (errorMessageLabel != null)
            {
                if (this._failure && !string.IsNullOrEmpty(this._unknownErrorMessage))
                {
                    ((ITextControl) errorMessageLabel).Text = this._unknownErrorMessage;
                    LoginUtil.SetTableCellStyle(errorMessageLabel, this.ErrorMessageStyle);
                    LoginUtil.SetTableCellVisible(errorMessageLabel, true);
                }
                else
                {
                    LoginUtil.SetTableCellVisible(errorMessageLabel, false);
                }
            }
        }

        private bool AttemptCreateUser()
        {
            if ((this.Page == null) || this.Page.IsValid)
            {
                MembershipCreateStatus status;
                LoginCancelEventArgs e = new LoginCancelEventArgs();
                this.OnCreatingUser(e);
                if (e.Cancel)
                {
                    return false;
                }
                System.Web.Security.MembershipProvider provider = LoginUtil.GetProvider(this.MembershipProvider);
                if (this.AutoGeneratePassword)
                {
                    int length = Math.Max(10, Membership.MinRequiredPasswordLength);
                    this._password = Membership.GeneratePassword(length, Membership.MinRequiredNonAlphanumericCharacters);
                }
                provider.CreateUser(this.UserNameInternal, this.PasswordInternal, this.EmailInternal, this.QuestionInternal, this.AnswerInternal, !this.DisableCreatedUser, null, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    this.OnCreatedUser(EventArgs.Empty);
                    if ((this._mailDefinition != null) && !string.IsNullOrEmpty(this.EmailInternal))
                    {
                        LoginUtil.SendPasswordMail(this.EmailInternal, this.UserNameInternal, this.PasswordInternal, this.MailDefinition, null, null, new LoginUtil.OnSendingMailDelegate(this.OnSendingMail), new LoginUtil.OnSendMailErrorDelegate(this.OnSendMailError), this);
                    }
                    this.CreateUserStep.AllowReturnInternal = false;
                    if (this.LoginCreatedUser)
                    {
                        this.AttemptLogin();
                    }
                    return true;
                }
                this.OnCreateUserError(new CreateUserErrorEventArgs(status));
                switch (status)
                {
                    case MembershipCreateStatus.InvalidPassword:
                    {
                        string invalidPasswordErrorMessage = this.InvalidPasswordErrorMessage;
                        if (!string.IsNullOrEmpty(invalidPasswordErrorMessage))
                        {
                            invalidPasswordErrorMessage = string.Format(CultureInfo.InvariantCulture, invalidPasswordErrorMessage, new object[] { provider.MinRequiredPasswordLength, provider.MinRequiredNonAlphanumericCharacters });
                        }
                        this._unknownErrorMessage = invalidPasswordErrorMessage;
                        break;
                    }
                    case MembershipCreateStatus.InvalidQuestion:
                        this._unknownErrorMessage = this.InvalidQuestionErrorMessage;
                        break;

                    case MembershipCreateStatus.InvalidAnswer:
                        this._unknownErrorMessage = this.InvalidAnswerErrorMessage;
                        break;

                    case MembershipCreateStatus.InvalidEmail:
                        this._unknownErrorMessage = this.InvalidEmailErrorMessage;
                        break;

                    case MembershipCreateStatus.DuplicateUserName:
                        this._unknownErrorMessage = this.DuplicateUserNameErrorMessage;
                        break;

                    case MembershipCreateStatus.DuplicateEmail:
                        this._unknownErrorMessage = this.DuplicateEmailErrorMessage;
                        break;

                    default:
                        this._unknownErrorMessage = this.UnknownErrorMessage;
                        break;
                }
            }
            return false;
        }

        private void AttemptLogin()
        {
            if (LoginUtil.GetProvider(this.MembershipProvider).ValidateUser(this.UserName, this.Password))
            {
                FormsAuthentication.SetAuthCookie(this.UserNameInternal, false);
            }
        }

        private void ConfirmPasswordTextChanged(object source, EventArgs e)
        {
            if (!this.AutoGeneratePassword)
            {
                this._confirmPassword = ((ITextControl) source).Text;
            }
        }

        protected internal override void CreateChildControls()
        {
            this._createUserStep = null;
            this._completeStep = null;
            base.CreateChildControls();
            this.UpdateValidators();
        }

        internal override void CreateCustomNavigationTemplates()
        {
            for (int i = 0; i < this.WizardSteps.Count; i++)
            {
                TemplatedWizardStep step = this.WizardSteps[i] as TemplatedWizardStep;
                if (step != null)
                {
                    string customContainerID = Wizard.GetCustomContainerID(i);
                    Wizard.BaseNavigationTemplateContainer container = base.CreateBaseNavigationTemplateContainer(customContainerID);
                    if (step.CustomNavigationTemplate != null)
                    {
                        step.CustomNavigationTemplate.InstantiateIn(container);
                        step.CustomNavigationTemplateContainer = container;
                        container.SetEnableTheming();
                    }
                    else if (step == this.CreateUserStep)
                    {
                        ITemplate template = new DefaultCreateUserNavigationTemplate(this);
                        template.InstantiateIn(container);
                        step.CustomNavigationTemplateContainer = container;
                        container.RegisterButtonCommandEvents();
                    }
                    base.CustomNavigationContainers[step] = container;
                }
            }
        }

        internal override ITemplate CreateDefaultDataListItemTemplate()
        {
            return new DataListItemTemplate();
        }

        internal override ITemplate CreateDefaultSideBarTemplate()
        {
            return new DefaultSideBarTemplate();
        }

        private static TableRow CreateDoubleSpannedColumnRow(params Control[] cellControls)
        {
            return CreateDoubleSpannedColumnRow(null, cellControls);
        }

        private static TableRow CreateDoubleSpannedColumnRow(HorizontalAlign? cellHorizontalAlignment, params Control[] cellControls)
        {
            TableRow row = CreateTableRow();
            TableCell cell = CreateTableCell();
            cell.ColumnSpan = 2;
            if (cellHorizontalAlignment.HasValue)
            {
                cell.HorizontalAlign = cellHorizontalAlignment.Value;
            }
            foreach (Control control in cellControls)
            {
                cell.Controls.Add(control);
            }
            row.Cells.Add(cell);
            return row;
        }

        private static LabelLiteral CreateLabelLiteral(Control control)
        {
            LabelLiteral literal = new LabelLiteral(control);
            literal.PreventAutoID();
            return literal;
        }

        internal override Wizard.LayoutTemplateWizardRendering CreateLayoutTemplateRendering()
        {
            return new LayoutTemplateWizardRendering(this);
        }

        private static Literal CreateLiteral()
        {
            Literal literal = new Literal();
            literal.PreventAutoID();
            return literal;
        }

        private static RequiredFieldValidator CreateRequiredFieldValidator(string id, string validationGroup, Control targetTextBox, bool enableValidation)
        {
            return new RequiredFieldValidator { ID = id, ControlToValidate = targetTextBox.ID, ValidationGroup = validationGroup, Display = ValidatorDisplay.Static, Text = System.Web.SR.GetString("LoginControls_DefaultRequiredFieldValidatorText"), Enabled = enableValidation, Visible = enableValidation };
        }

        private static Table CreateTable()
        {
            Table table = new Table {
                Width = Unit.Percentage(100.0),
                Height = Unit.Percentage(100.0)
            };
            table.PreventAutoID();
            return table;
        }

        private static TableCell CreateTableCell()
        {
            TableCell cell = new TableCell();
            cell.PreventAutoID();
            return cell;
        }

        internal override Wizard.TableWizardRendering CreateTableRendering()
        {
            return new TableWizardRendering(this);
        }

        private static TableRow CreateTableRow()
        {
            TableRow row = new LoginUtil.DisappearingTableRow();
            row.PreventAutoID();
            return row;
        }

        private static TableRow CreateTwoColumnRow(Control leftCellControl, params Control[] rightCellControls)
        {
            TableRow row = CreateTableRow();
            TableCell cell = CreateTableCell();
            cell.HorizontalAlign = HorizontalAlign.Right;
            cell.Controls.Add(leftCellControl);
            row.Cells.Add(cell);
            TableCell cell2 = CreateTableCell();
            foreach (Control control in rightCellControls)
            {
                cell2.Controls.Add(control);
            }
            row.Cells.Add(cell2);
            return row;
        }

        internal override void DataListItemDataBound(object sender, WizardSideBarListControlItemEventArgs e)
        {
            WizardSideBarListControlItem item = e.Item;
            if (((item.ItemType == ListItemType.Item) || (item.ItemType == ListItemType.AlternatingItem)) || ((item.ItemType == ListItemType.SelectedItem) || (item.ItemType == ListItemType.EditItem)))
            {
                if (item.FindControl(Wizard.SideBarButtonID) is IButtonControl)
                {
                    base.DataListItemDataBound(sender, e);
                }
                else
                {
                    Label label = item.FindControl("SideBarLabel") as Label;
                    if (label == null)
                    {
                        if (!base.DesignMode)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("CreateUserWizard_SideBar_Label_Not_Found", new object[] { Wizard.DataListID, "SideBarLabel" }));
                        }
                    }
                    else
                    {
                        label.MergeStyle(base.SideBarButtonStyle);
                        WizardStepBase dataItem = item.DataItem as WizardStepBase;
                        if (dataItem != null)
                        {
                            base.RegisterSideBarDataListForRender();
                            if (dataItem.Title.Length > 0)
                            {
                                label.Text = dataItem.Title;
                            }
                            else
                            {
                                label.Text = dataItem.ID;
                            }
                        }
                    }
                }
            }
        }

        private void EmailTextChanged(object source, EventArgs e)
        {
            this.Email = ((ITextControl) source).Text;
        }

        private void EnsureCreateUserSteps()
        {
            bool flag = false;
            bool flag2 = false;
            foreach (WizardStepBase base2 in this.WizardSteps)
            {
                CreateUserWizardStep step = base2 as CreateUserWizardStep;
                if (step != null)
                {
                    if (flag)
                    {
                        throw new HttpException(System.Web.SR.GetString("CreateUserWizard_DuplicateCreateUserWizardStep"));
                    }
                    flag = true;
                    this._createUserStep = step;
                }
                else
                {
                    CompleteWizardStep step2 = base2 as CompleteWizardStep;
                    if (step2 != null)
                    {
                        if (flag2)
                        {
                            throw new HttpException(System.Web.SR.GetString("CreateUserWizard_DuplicateCompleteWizardStep"));
                        }
                        flag2 = true;
                        this._completeStep = step2;
                    }
                }
            }
            if (!flag)
            {
                this._createUserStep = new CreateUserWizardStep();
                this._createUserStep.ApplyStyleSheetSkin(this.Page);
                this.WizardSteps.AddAt(0, this._createUserStep);
                this._createUserStep.Active = true;
            }
            if (!flag2)
            {
                this._completeStep = new CompleteWizardStep();
                this._completeStep.ApplyStyleSheetSkin(this.Page);
                this.WizardSteps.Add(this._completeStep);
            }
            if (this.ActiveStepIndex == -1)
            {
                this.ActiveStepIndex = 0;
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override IDictionary GetDesignModeState()
        {
            IDictionary designModeState = base.GetDesignModeState();
            WizardStepBase activeStep = base.ActiveStep;
            if ((activeStep != null) && (activeStep == this.CreateUserStep))
            {
                designModeState["CustomNavigationControls"] = base.CustomNavigationContainers[base.ActiveStep].Controls;
            }
            Control errorMessageLabel = this._createUserStepContainer.ErrorMessageLabel;
            if (errorMessageLabel != null)
            {
                LoginUtil.SetTableCellVisible(errorMessageLabel, true);
            }
            return designModeState;
        }

        internal override void InstantiateStepContentTemplates()
        {
            bool useInnerTable = this.LayoutTemplate == null;
            foreach (WizardStepBase base2 in this.WizardSteps)
            {
                if (base2 == this.CreateUserStep)
                {
                    base2.Controls.Clear();
                    this._createUserStepContainer = new CreateUserStepContainer(this, useInnerTable);
                    this._createUserStepContainer.ID = "CreateUserStepContainer";
                    ITemplate contentTemplate = this.CreateUserStep.ContentTemplate;
                    if (contentTemplate == null)
                    {
                        contentTemplate = new DefaultCreateUserContentTemplate(this);
                    }
                    else
                    {
                        this._createUserStepContainer.SetEnableTheming();
                    }
                    contentTemplate.InstantiateIn(this._createUserStepContainer.Container);
                    this.CreateUserStep.ContentTemplateContainer = this._createUserStepContainer;
                    base2.Controls.Add(this._createUserStepContainer);
                }
                else if (base2 == this.CompleteStep)
                {
                    base2.Controls.Clear();
                    this._completeStepContainer = new CompleteStepContainer(this, useInnerTable);
                    this._completeStepContainer.ID = "CompleteStepContainer";
                    ITemplate template2 = this.CompleteStep.ContentTemplate;
                    if (template2 == null)
                    {
                        template2 = new DefaultCompleteStepContentTemplate(this._completeStepContainer);
                    }
                    else
                    {
                        this._completeStepContainer.SetEnableTheming();
                    }
                    template2.InstantiateIn(this._completeStepContainer.Container);
                    this.CompleteStep.ContentTemplateContainer = this._completeStepContainer;
                    base2.Controls.Add(this._completeStepContainer);
                }
                else
                {
                    TemplatedWizardStep step = base2 as TemplatedWizardStep;
                    if (step != null)
                    {
                        base.InstantiateStepContentTemplate(step);
                    }
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
                if (objArray.Length != 13)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.CreateUserButtonStyle).LoadViewState(objArray[1]);
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
                    ((IStateManager) this.ErrorMessageStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.PasswordHintStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.MailDefinition).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    ((IStateManager) this.ContinueButtonStyle).LoadViewState(objArray[10]);
                }
                if (objArray[11] != null)
                {
                    ((IStateManager) this.CompleteSuccessTextStyle).LoadViewState(objArray[11]);
                }
                if (objArray[12] != null)
                {
                    ((IStateManager) this.ValidatorTextStyle).LoadViewState(objArray[12]);
                }
            }
            this.UpdateValidators();
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            CommandEventArgs args = e as CommandEventArgs;
            if ((args != null) && args.CommandName.Equals(ContinueButtonCommandName, StringComparison.CurrentCultureIgnoreCase))
            {
                this.OnContinueButtonClick(EventArgs.Empty);
                return true;
            }
            return base.OnBubbleEvent(source, e);
        }

        protected virtual void OnContinueButtonClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventButtonContinueClick];
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

        protected virtual void OnCreatedUser(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventCreatedUser];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCreateUserError(CreateUserErrorEventArgs e)
        {
            CreateUserErrorEventHandler handler = (CreateUserErrorEventHandler) base.Events[EventCreateUserError];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCreatingUser(LoginCancelEventArgs e)
        {
            LoginCancelEventHandler handler = (LoginCancelEventHandler) base.Events[EventCreatingUser];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnNextButtonClick(WizardNavigationEventArgs e)
        {
            if (this.WizardSteps[e.CurrentStepIndex] == this._createUserStep)
            {
                e.Cancel = (this.Page != null) && !this.Page.IsValid;
                if (!e.Cancel)
                {
                    this._failure = !this.AttemptCreateUser();
                    if (this._failure)
                    {
                        e.Cancel = true;
                        ITextControl errorMessageLabel = (ITextControl) this._createUserStepContainer.ErrorMessageLabel;
                        if ((errorMessageLabel != null) && !string.IsNullOrEmpty(this._unknownErrorMessage))
                        {
                            errorMessageLabel.Text = this._unknownErrorMessage;
                            Control control2 = errorMessageLabel as Control;
                            if (control2 != null)
                            {
                                control2.Visible = true;
                            }
                        }
                    }
                }
            }
            base.OnNextButtonClick(e);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            this.EnsureCreateUserSteps();
            base.OnPreRender(e);
            string membershipProvider = this.MembershipProvider;
            if (!string.IsNullOrEmpty(membershipProvider) && (Membership.Providers[membershipProvider] == null))
            {
                throw new HttpException(System.Web.SR.GetString("WebControl_CantFindProvider"));
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
            if (!this.AutoGeneratePassword)
            {
                this._password = ((ITextControl) source).Text;
            }
        }

        private void QuestionTextChanged(object source, EventArgs e)
        {
            this.Question = ((ITextControl) source).Text;
        }

        private void RegisterEvents()
        {
            RegisterTextChangedEvent(this._createUserStepContainer.UserNameTextBox, new Action<object, EventArgs>(this.UserNameTextChanged));
            RegisterTextChangedEvent(this._createUserStepContainer.EmailTextBox, new Action<object, EventArgs>(this.EmailTextChanged));
            RegisterTextChangedEvent(this._createUserStepContainer.QuestionTextBox, new Action<object, EventArgs>(this.QuestionTextChanged));
            RegisterTextChangedEvent(this._createUserStepContainer.AnswerTextBox, new Action<object, EventArgs>(this.AnswerTextChanged));
            RegisterTextChangedEvent(this._createUserStepContainer.PasswordTextBox, new Action<object, EventArgs>(this.PasswordTextChanged));
            RegisterTextChangedEvent(this._createUserStepContainer.ConfirmPasswordTextBox, new Action<object, EventArgs>(this.ConfirmPasswordTextChanged));
        }

        private static void RegisterTextChangedEvent(Control control, Action<object, EventArgs> textChangedHandler)
        {
            IEditableTextControl control2 = control as IEditableTextControl;
            if (control2 != null)
            {
                control2.TextChanged += new EventHandler(textChangedHandler.Invoke);
            }
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._createUserButtonStyle != null) ? ((IStateManager) this._createUserButtonStyle).SaveViewState() : null, (this._labelStyle != null) ? ((IStateManager) this._labelStyle).SaveViewState() : null, (this._textBoxStyle != null) ? ((IStateManager) this._textBoxStyle).SaveViewState() : null, (this._hyperLinkStyle != null) ? ((IStateManager) this._hyperLinkStyle).SaveViewState() : null, (this._instructionTextStyle != null) ? ((IStateManager) this._instructionTextStyle).SaveViewState() : null, (this._titleTextStyle != null) ? ((IStateManager) this._titleTextStyle).SaveViewState() : null, (this._errorMessageStyle != null) ? ((IStateManager) this._errorMessageStyle).SaveViewState() : null, (this._passwordHintStyle != null) ? ((IStateManager) this._passwordHintStyle).SaveViewState() : null, (this._mailDefinition != null) ? ((IStateManager) this._mailDefinition).SaveViewState() : null, (this._continueButtonStyle != null) ? ((IStateManager) this._continueButtonStyle).SaveViewState() : null, (this._completeSuccessTextStyle != null) ? ((IStateManager) this._completeSuccessTextStyle).SaveViewState() : null, (this._validatorTextStyle != null) ? ((IStateManager) this._validatorTextStyle).SaveViewState() : null };
            for (int i = 0; i < 13; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        private void SetChildProperties()
        {
            this.ApplyCommonCreateUserValues();
            if (this.DefaultCreateUserStep)
            {
                this.ApplyDefaultCreateUserValues();
            }
            if (this.DefaultCompleteStep)
            {
                this.ApplyCompleteValues();
            }
            Control errorMessageLabel = this._createUserStepContainer.ErrorMessageLabel;
            if (errorMessageLabel != null)
            {
                if (this._failure && !string.IsNullOrEmpty(this._unknownErrorMessage))
                {
                    ((ITextControl) errorMessageLabel).Text = this._unknownErrorMessage;
                    errorMessageLabel.Visible = true;
                }
                else
                {
                    errorMessageLabel.Visible = false;
                }
            }
        }

        private void SetDefaultCreateUserNavigationTemplateProperties()
        {
            WebControl createUserButton = (WebControl) this._defaultCreateUserNavigationTemplate.CreateUserButton;
            WebControl previousButton = (WebControl) this._defaultCreateUserNavigationTemplate.PreviousButton;
            WebControl cancelButton = (WebControl) this._defaultCreateUserNavigationTemplate.CancelButton;
            this._defaultCreateUserNavigationTemplate.ApplyLayoutStyleToInnerCells(base.NavigationStyle);
            IButtonControl control4 = (IButtonControl) createUserButton;
            control4.CausesValidation = true;
            control4.Text = this.CreateUserButtonText;
            control4.ValidationGroup = this.ValidationGroup;
            IButtonControl control5 = (IButtonControl) previousButton;
            control5.CausesValidation = false;
            control5.Text = this.StepPreviousButtonText;
            ((IButtonControl) cancelButton).Text = this.CancelButtonText;
            if (this._createUserButtonStyle != null)
            {
                createUserButton.ApplyStyle(this._createUserButtonStyle);
            }
            createUserButton.ControlStyle.MergeWith(base.NavigationButtonStyle);
            createUserButton.TabIndex = this.TabIndex;
            createUserButton.Visible = true;
            ImageButton button = createUserButton as ImageButton;
            if (button != null)
            {
                button.ImageUrl = this.CreateUserButtonImageUrl;
                button.AlternateText = this.CreateUserButtonText;
            }
            previousButton.ApplyStyle(base.StepPreviousButtonStyle);
            previousButton.ControlStyle.MergeWith(base.NavigationButtonStyle);
            previousButton.TabIndex = this.TabIndex;
            int previousStepIndex = base.GetPreviousStepIndex(false);
            if ((previousStepIndex != -1) && this.WizardSteps[previousStepIndex].AllowReturn)
            {
                previousButton.Visible = true;
            }
            else
            {
                previousButton.Parent.Visible = false;
            }
            ImageButton button2 = previousButton as ImageButton;
            if (button2 != null)
            {
                button2.AlternateText = this.StepPreviousButtonText;
                button2.ImageUrl = this.StepPreviousButtonImageUrl;
            }
            if (this.DisplayCancelButton)
            {
                cancelButton.ApplyStyle(base.CancelButtonStyle);
                cancelButton.ControlStyle.MergeWith(base.NavigationButtonStyle);
                cancelButton.TabIndex = this.TabIndex;
                cancelButton.Visible = true;
                ImageButton button3 = cancelButton as ImageButton;
                if (button3 != null)
                {
                    button3.ImageUrl = this.CancelButtonImageUrl;
                    button3.AlternateText = this.CancelButtonText;
                }
            }
            else
            {
                cancelButton.Parent.Visible = false;
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
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._createUserButtonStyle != null)
            {
                ((IStateManager) this._createUserButtonStyle).TrackViewState();
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
            if (this._errorMessageStyle != null)
            {
                ((IStateManager) this._errorMessageStyle).TrackViewState();
            }
            if (this._passwordHintStyle != null)
            {
                ((IStateManager) this._passwordHintStyle).TrackViewState();
            }
            if (this._mailDefinition != null)
            {
                ((IStateManager) this._mailDefinition).TrackViewState();
            }
            if (this._continueButtonStyle != null)
            {
                ((IStateManager) this._continueButtonStyle).TrackViewState();
            }
            if (this._completeSuccessTextStyle != null)
            {
                ((IStateManager) this._completeSuccessTextStyle).TrackViewState();
            }
            if (this._validatorTextStyle != null)
            {
                ((IStateManager) this._validatorTextStyle).TrackViewState();
            }
        }

        private void UpdateValidators()
        {
            if (!base.DesignMode && (this.DefaultCreateUserStep && (this._createUserStepContainer != null)))
            {
                if (this.AutoGeneratePassword)
                {
                    BaseValidator confirmPasswordRequired = this._createUserStepContainer.ConfirmPasswordRequired;
                    if (confirmPasswordRequired != null)
                    {
                        this.Page.Validators.Remove(confirmPasswordRequired);
                        confirmPasswordRequired.Enabled = false;
                    }
                    BaseValidator passwordRequired = this._createUserStepContainer.PasswordRequired;
                    if (passwordRequired != null)
                    {
                        this.Page.Validators.Remove(passwordRequired);
                        passwordRequired.Enabled = false;
                    }
                    BaseValidator passwordRegExpValidator = this._createUserStepContainer.PasswordRegExpValidator;
                    if (passwordRegExpValidator != null)
                    {
                        this.Page.Validators.Remove(passwordRegExpValidator);
                        passwordRegExpValidator.Enabled = false;
                    }
                }
                else if (this.PasswordRegularExpression.Length <= 0)
                {
                    BaseValidator validator = this._createUserStepContainer.PasswordRegExpValidator;
                    if (validator != null)
                    {
                        if (this.Page != null)
                        {
                            this.Page.Validators.Remove(validator);
                        }
                        validator.Enabled = false;
                    }
                }
                if (!this.RequireEmail)
                {
                    BaseValidator emailRequired = this._createUserStepContainer.EmailRequired;
                    if (emailRequired != null)
                    {
                        if (this.Page != null)
                        {
                            this.Page.Validators.Remove(emailRequired);
                        }
                        emailRequired.Enabled = false;
                    }
                    BaseValidator emailRegExpValidator = this._createUserStepContainer.EmailRegExpValidator;
                    if (emailRegExpValidator != null)
                    {
                        if (this.Page != null)
                        {
                            this.Page.Validators.Remove(emailRegExpValidator);
                        }
                        emailRegExpValidator.Enabled = false;
                    }
                }
                else if (this.EmailRegularExpression.Length <= 0)
                {
                    BaseValidator validator7 = this._createUserStepContainer.EmailRegExpValidator;
                    if (validator7 != null)
                    {
                        if (this.Page != null)
                        {
                            this.Page.Validators.Remove(validator7);
                        }
                        validator7.Enabled = false;
                    }
                }
                if (!this.QuestionAndAnswerRequired)
                {
                    BaseValidator questionRequired = this._createUserStepContainer.QuestionRequired;
                    if (questionRequired != null)
                    {
                        if (this.Page != null)
                        {
                            this.Page.Validators.Remove(questionRequired);
                        }
                        questionRequired.Enabled = false;
                    }
                    BaseValidator answerRequired = this._createUserStepContainer.AnswerRequired;
                    if (answerRequired != null)
                    {
                        if (this.Page != null)
                        {
                            this.Page.Validators.Remove(answerRequired);
                        }
                        answerRequired.Enabled = false;
                    }
                }
            }
        }

        private void UserNameTextChanged(object source, EventArgs e)
        {
            this.UserName = ((ITextControl) source).Text;
        }

        [DefaultValue(0)]
        public override int ActiveStepIndex
        {
            get
            {
                return base.ActiveStepIndex;
            }
            set
            {
                base.ActiveStepIndex = value;
            }
        }

        [DefaultValue(""), Localizable(true), Themeable(false), WebCategory("Appearance"), WebSysDescription("CreateUserWizard_Answer")]
        public virtual string Answer
        {
            get
            {
                if (this._answer != null)
                {
                    return this._answer;
                }
                return string.Empty;
            }
            set
            {
                this._answer = value;
            }
        }

        private string AnswerInternal
        {
            get
            {
                string answer = this.Answer;
                if (string.IsNullOrEmpty(this.Answer) && (this._createUserStepContainer != null))
                {
                    ITextControl answerTextBox = (ITextControl) this._createUserStepContainer.AnswerTextBox;
                    if (answerTextBox != null)
                    {
                        answer = answerTextBox.Text;
                    }
                }
                if (string.IsNullOrEmpty(answer))
                {
                    answer = null;
                }
                return answer;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("CreateUserWizard_AnswerLabelText"), Localizable(true), WebSysDefaultValue("CreateUserWizard_DefaultAnswerLabelText")]
        public virtual string AnswerLabelText
        {
            get
            {
                object obj2 = this.ViewState["AnswerLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultAnswerLabelText");
            }
            set
            {
                this.ViewState["AnswerLabelText"] = value;
            }
        }

        [WebCategory("Validation"), Localizable(true), WebSysDescription("LoginControls_AnswerRequiredErrorMessage"), WebSysDefaultValue("CreateUserWizard_DefaultAnswerRequiredErrorMessage")]
        public virtual string AnswerRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["AnswerRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultAnswerRequiredErrorMessage");
            }
            set
            {
                this.ViewState["AnswerRequiredErrorMessage"] = value;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("CreateUserWizard_AutoGeneratePassword"), DefaultValue(false), Themeable(false)]
        public virtual bool AutoGeneratePassword
        {
            get
            {
                object obj2 = this.ViewState["AutoGeneratePassword"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                if (this.AutoGeneratePassword != value)
                {
                    this.ViewState["AutoGeneratePassword"] = value;
                    base.RequiresControlsRecreation();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("CreateUserWizard_CompleteStep"), Browsable(false), WebCategory("Appearance")]
        public CompleteWizardStep CompleteStep
        {
            get
            {
                this.EnsureChildControls();
                return this._completeStep;
            }
        }

        [WebSysDescription("CreateUserWizard_CompleteSuccessText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("CreateUserWizard_DefaultCompleteSuccessText")]
        public virtual string CompleteSuccessText
        {
            get
            {
                object obj2 = this.ViewState["CompleteSuccessText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultCompleteSuccessText");
            }
            set
            {
                this.ViewState["CompleteSuccessText"] = value;
            }
        }

        [DefaultValue((string) null), WebCategory("Styles"), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("CreateUserWizard_CompleteSuccessTextStyle")]
        public TableItemStyle CompleteSuccessTextStyle
        {
            get
            {
                if (this._completeSuccessTextStyle == null)
                {
                    this._completeSuccessTextStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._completeSuccessTextStyle).TrackViewState();
                    }
                }
                return this._completeSuccessTextStyle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual string ConfirmPassword
        {
            get
            {
                if (this._confirmPassword != null)
                {
                    return this._confirmPassword;
                }
                return string.Empty;
            }
        }

        [WebSysDescription("ChangePassword_ConfirmPasswordCompareErrorMessage"), WebSysDefaultValue("CreateUserWizard_DefaultConfirmPasswordCompareErrorMessage"), Localizable(true), WebCategory("Validation")]
        public virtual string ConfirmPasswordCompareErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["ConfirmPasswordCompareErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultConfirmPasswordCompareErrorMessage");
            }
            set
            {
                this.ViewState["ConfirmPasswordCompareErrorMessage"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_ConfirmPasswordLabelText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("CreateUserWizard_DefaultConfirmPasswordLabelText")]
        public virtual string ConfirmPasswordLabelText
        {
            get
            {
                object obj2 = this.ViewState["ConfirmPasswordLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultConfirmPasswordLabelText");
            }
            set
            {
                this.ViewState["ConfirmPasswordLabelText"] = value;
            }
        }

        [WebSysDescription("LoginControls_ConfirmPasswordRequiredErrorMessage"), WebSysDefaultValue("CreateUserWizard_DefaultConfirmPasswordRequiredErrorMessage"), Localizable(true), WebCategory("Validation")]
        public virtual string ConfirmPasswordRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["ConfirmPasswordRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultConfirmPasswordRequiredErrorMessage");
            }
            set
            {
                this.ViewState["ConfirmPasswordRequiredErrorMessage"] = value;
            }
        }

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Appearance"), DefaultValue(""), WebSysDescription("ChangePassword_ContinueButtonImageUrl")]
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

        [WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("CreateUserWizard_ContinueButtonStyle"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [Localizable(true), WebSysDefaultValue("CreateUserWizard_DefaultContinueButtonText"), WebSysDescription("CreateUserWizard_ContinueButtonText"), WebCategory("Appearance")]
        public virtual string ContinueButtonText
        {
            get
            {
                object obj2 = this.ViewState["ContinueButtonText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultContinueButtonText");
            }
            set
            {
                this.ViewState["ContinueButtonText"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_ContinueButtonType"), WebCategory("Appearance"), DefaultValue(0)]
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
                if (value != this.ContinueButtonType)
                {
                    this.ViewState["ContinueButtonType"] = value;
                }
            }
        }

        [DefaultValue(""), WebCategory("Behavior"), Themeable(false), WebSysDescription("LoginControls_ContinueDestinationPageUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
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

        [WebCategory("Appearance"), UrlProperty, DefaultValue(""), WebSysDescription("CreateUserWizard_CreateUserButtonImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string CreateUserButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["CreateUserButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CreateUserButtonImageUrl"] = value;
            }
        }

        [NotifyParentProperty(true), WebSysDescription("CreateUserWizard_CreateUserButtonStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style CreateUserButtonStyle
        {
            get
            {
                if (this._createUserButtonStyle == null)
                {
                    this._createUserButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._createUserButtonStyle).TrackViewState();
                    }
                }
                return this._createUserButtonStyle;
            }
        }

        [Localizable(true), WebSysDescription("CreateUserWizard_CreateUserButtonText"), WebSysDefaultValue("CreateUserWizard_DefaultCreateUserButtonText"), WebCategory("Appearance")]
        public virtual string CreateUserButtonText
        {
            get
            {
                object obj2 = this.ViewState["CreateUserButtonText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultCreateUserButtonText");
            }
            set
            {
                this.ViewState["CreateUserButtonText"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_CreateUserButtonType"), WebCategory("Appearance"), DefaultValue(0)]
        public virtual ButtonType CreateUserButtonType
        {
            get
            {
                object obj2 = this.ViewState["CreateUserButtonType"];
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
                if (value != this.CreateUserButtonType)
                {
                    this.ViewState["CreateUserButtonType"] = value;
                }
            }
        }

        [WebSysDescription("CreateUserWizard_CreateUserStep"), WebCategory("Appearance"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CreateUserWizardStep CreateUserStep
        {
            get
            {
                this.EnsureChildControls();
                return this._createUserStep;
            }
        }

        private bool DefaultCompleteStep
        {
            get
            {
                CompleteWizardStep completeStep = this.CompleteStep;
                return ((completeStep != null) && (completeStep.ContentTemplate == null));
            }
        }

        private bool DefaultCreateUserStep
        {
            get
            {
                CreateUserWizardStep createUserStep = this.CreateUserStep;
                return ((createUserStep != null) && (createUserStep.ContentTemplate == null));
            }
        }

        [WebSysDescription("CreateUserWizard_DisableCreatedUser"), DefaultValue(false), Themeable(false), WebCategory("Behavior")]
        public virtual bool DisableCreatedUser
        {
            get
            {
                object obj2 = this.ViewState["DisableCreatedUser"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["DisableCreatedUser"] = value;
            }
        }

        [DefaultValue(false)]
        public override bool DisplaySideBar
        {
            get
            {
                return base.DisplaySideBar;
            }
            set
            {
                base.DisplaySideBar = value;
            }
        }

        [WebSysDescription("CreateUserWizard_DuplicateEmailErrorMessage"), WebCategory("Appearance"), WebSysDefaultValue("CreateUserWizard_DefaultDuplicateEmailErrorMessage"), Localizable(true)]
        public virtual string DuplicateEmailErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["DuplicateEmailErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultDuplicateEmailErrorMessage");
            }
            set
            {
                this.ViewState["DuplicateEmailErrorMessage"] = value;
            }
        }

        [WebCategory("Appearance"), Localizable(true), WebSysDefaultValue("CreateUserWizard_DefaultDuplicateUserNameErrorMessage"), WebSysDescription("CreateUserWizard_DuplicateUserNameErrorMessage")]
        public virtual string DuplicateUserNameErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["DuplicateUserNameErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultDuplicateUserNameErrorMessage");
            }
            set
            {
                this.ViewState["DuplicateUserNameErrorMessage"] = value;
            }
        }

        [WebSysDescription("LoginControls_EditProfileIconUrl"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Links")]
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

        [WebSysDescription("CreateUserWizard_EditProfileText"), DefaultValue(""), WebCategory("Links"), Localizable(true)]
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

        [DefaultValue(""), WebCategory("Links"), WebSysDescription("CreateUserWizard_EditProfileUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
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

        [WebSysDescription("CreateUserWizard_Email"), DefaultValue(""), WebCategory("Appearance")]
        public virtual string Email
        {
            get
            {
                object obj2 = this.ViewState["Email"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Email"] = value;
            }
        }

        private string EmailInternal
        {
            get
            {
                string email = this.Email;
                if (string.IsNullOrEmpty(email) && (this._createUserStepContainer != null))
                {
                    ITextControl emailTextBox = (ITextControl) this._createUserStepContainer.EmailTextBox;
                    if (emailTextBox != null)
                    {
                        return emailTextBox.Text;
                    }
                }
                return email;
            }
        }

        [WebSysDescription("CreateUserWizard_EmailLabelText"), WebCategory("Appearance"), Localizable(true), WebSysDefaultValue("CreateUserWizard_DefaultEmailLabelText")]
        public virtual string EmailLabelText
        {
            get
            {
                object obj2 = this.ViewState["EmailLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultEmailLabelText");
            }
            set
            {
                this.ViewState["EmailLabelText"] = value;
            }
        }

        [WebCategory("Validation"), WebSysDefaultValue(""), WebSysDescription("CreateUserWizard_EmailRegularExpression")]
        public virtual string EmailRegularExpression
        {
            get
            {
                object obj2 = this.ViewState["EmailRegularExpression"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["EmailRegularExpression"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_EmailRegularExpressionErrorMessage"), WebCategory("Validation"), WebSysDefaultValue("CreateUserWizard_DefaultEmailRegularExpressionErrorMessage")]
        public virtual string EmailRegularExpressionErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["EmailRegularExpressionErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultEmailRegularExpressionErrorMessage");
            }
            set
            {
                this.ViewState["EmailRegularExpressionErrorMessage"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_EmailRequiredErrorMessage"), Localizable(true), WebCategory("Validation"), WebSysDefaultValue("CreateUserWizard_DefaultEmailRequiredErrorMessage")]
        public virtual string EmailRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["EmailRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultEmailRequiredErrorMessage");
            }
            set
            {
                this.ViewState["EmailRequiredErrorMessage"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_ErrorMessageStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ErrorMessageStyle
        {
            get
            {
                if (this._errorMessageStyle == null)
                {
                    this._errorMessageStyle = new ErrorTableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._errorMessageStyle).TrackViewState();
                    }
                }
                return this._errorMessageStyle;
            }
        }

        [WebSysDescription("LoginControls_HelpPageIconUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Links"), DefaultValue(""), UrlProperty]
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

        [WebSysDescription("LoginControls_HelpPageUrl"), WebCategory("Links"), DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
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

        [DefaultValue((string) null), WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("WebControl_HyperLinkStyle")]
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

        [WebSysDescription("WebControl_InstructionText"), Localizable(true), WebCategory("Appearance"), DefaultValue("")]
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

        [PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_InstructionTextStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
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

        [Localizable(true), WebSysDescription("CreateUserWizard_InvalidAnswerErrorMessage"), WebCategory("Appearance"), WebSysDefaultValue("CreateUserWizard_DefaultInvalidAnswerErrorMessage")]
        public virtual string InvalidAnswerErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["InvalidAnswerErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultInvalidAnswerErrorMessage");
            }
            set
            {
                this.ViewState["InvalidAnswerErrorMessage"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_InvalidEmailErrorMessage"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("CreateUserWizard_DefaultInvalidEmailErrorMessage")]
        public virtual string InvalidEmailErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["InvalidEmailErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultInvalidEmailErrorMessage");
            }
            set
            {
                this.ViewState["InvalidEmailErrorMessage"] = value;
            }
        }

        [WebSysDefaultValue("CreateUserWizard_DefaultInvalidPasswordErrorMessage"), WebSysDescription("CreateUserWizard_InvalidPasswordErrorMessage"), Localizable(true), WebCategory("Appearance")]
        public virtual string InvalidPasswordErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["InvalidPasswordErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultInvalidPasswordErrorMessage");
            }
            set
            {
                this.ViewState["InvalidPasswordErrorMessage"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_InvalidQuestionErrorMessage"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("CreateUserWizard_DefaultInvalidQuestionErrorMessage")]
        public virtual string InvalidQuestionErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["InvalidQuestionErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultInvalidQuestionErrorMessage");
            }
            set
            {
                this.ViewState["InvalidQuestionErrorMessage"] = value;
            }
        }

        [DefaultValue((string) null), WebSysDescription("LoginControls_LabelStyle"), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
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

        [DefaultValue(true), WebSysDescription("CreateUserWizard_LoginCreatedUser"), WebCategory("Behavior"), Themeable(false)]
        public virtual bool LoginCreatedUser
        {
            get
            {
                object obj2 = this.ViewState["LoginCreatedUser"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["LoginCreatedUser"] = value;
            }
        }

        [WebCategory("Behavior"), NotifyParentProperty(true), WebSysDescription("CreateUserWizard_MailDefinition"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), Themeable(false)]
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

        [Themeable(false), DefaultValue(""), WebSysDescription("MembershipProvider_Name"), WebCategory("Data")]
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
                if (this.MembershipProvider != value)
                {
                    this.ViewState["MembershipProvider"] = value;
                    base.RequiresControlsRecreation();
                }
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

        [NotifyParentProperty(true), WebSysDescription("CreateUserWizard_PasswordHintStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebCategory("Appearance"), WebSysDefaultValue(""), Localizable(true), WebSysDescription("ChangePassword_PasswordHintText")]
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

        private string PasswordInternal
        {
            get
            {
                string password = this.Password;
                if ((string.IsNullOrEmpty(password) && !this.AutoGeneratePassword) && (this._createUserStepContainer != null))
                {
                    ITextControl passwordTextBox = (ITextControl) this._createUserStepContainer.PasswordTextBox;
                    if (passwordTextBox != null)
                    {
                        return passwordTextBox.Text;
                    }
                }
                return password;
            }
        }

        [Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("LoginControls_DefaultPasswordLabelText"), WebSysDescription("LoginControls_PasswordLabelText")]
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

        [WebSysDescription("CreateUserWizard_PasswordRegularExpression"), WebCategory("Validation"), WebSysDefaultValue("")]
        public virtual string PasswordRegularExpression
        {
            get
            {
                object obj2 = this.ViewState["PasswordRegularExpression"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["PasswordRegularExpression"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_PasswordRegularExpressionErrorMessage"), WebSysDefaultValue("Password_InvalidPasswordErrorMessage"), WebCategory("Validation")]
        public virtual string PasswordRegularExpressionErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["PasswordRegularExpressionErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("Password_InvalidPasswordErrorMessage");
            }
            set
            {
                this.ViewState["PasswordRegularExpressionErrorMessage"] = value;
            }
        }

        [WebCategory("Validation"), Localizable(true), WebSysDefaultValue("CreateUserWizard_DefaultPasswordRequiredErrorMessage"), WebSysDescription("CreateUserWizard_PasswordRequiredErrorMessage")]
        public virtual string PasswordRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["PasswordRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultPasswordRequiredErrorMessage");
            }
            set
            {
                this.ViewState["PasswordRequiredErrorMessage"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_Question"), DefaultValue(""), Localizable(true), Themeable(false), WebCategory("Appearance")]
        public virtual string Question
        {
            get
            {
                object obj2 = this.ViewState["Question"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Question"] = value;
            }
        }

        [DefaultValue(true), WebCategory("Validation"), WebSysDescription("CreateUserWizard_QuestionAndAnswerRequired")]
        protected internal bool QuestionAndAnswerRequired
        {
            get
            {
                if (!base.DesignMode)
                {
                    return LoginUtil.GetProvider(this.MembershipProvider).RequiresQuestionAndAnswer;
                }
                if ((this.CreateUserStep != null) && (this.CreateUserStep.ContentTemplate != null))
                {
                    return false;
                }
                return true;
            }
        }

        private string QuestionInternal
        {
            get
            {
                string question = this.Question;
                if (string.IsNullOrEmpty(question) && (this._createUserStepContainer != null))
                {
                    ITextControl questionTextBox = (ITextControl) this._createUserStepContainer.QuestionTextBox;
                    if (questionTextBox != null)
                    {
                        question = questionTextBox.Text;
                    }
                }
                if (string.IsNullOrEmpty(question))
                {
                    question = null;
                }
                return question;
            }
        }

        [WebSysDefaultValue("CreateUserWizard_DefaultQuestionLabelText"), WebSysDescription("CreateUserWizard_QuestionLabelText"), Localizable(true), WebCategory("Appearance")]
        public virtual string QuestionLabelText
        {
            get
            {
                object obj2 = this.ViewState["QuestionLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultQuestionLabelText");
            }
            set
            {
                this.ViewState["QuestionLabelText"] = value;
            }
        }

        [Localizable(true), WebCategory("Validation"), WebSysDefaultValue("CreateUserWizard_DefaultQuestionRequiredErrorMessage"), WebSysDescription("CreateUserWizard_QuestionRequiredErrorMessage")]
        public virtual string QuestionRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["QuestionRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultQuestionRequiredErrorMessage");
            }
            set
            {
                this.ViewState["QuestionRequiredErrorMessage"] = value;
            }
        }

        [WebSysDescription("CreateUserWizard_RequireEmail"), DefaultValue(true), Themeable(false), WebCategory("Behavior")]
        public virtual bool RequireEmail
        {
            get
            {
                object obj2 = this.ViewState["RequireEmail"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                if (this.RequireEmail != value)
                {
                    this.ViewState["RequireEmail"] = value;
                }
            }
        }

        internal override bool ShowCustomNavigationTemplate
        {
            get
            {
                return (base.ShowCustomNavigationTemplate || (base.ActiveStep == this.CreateUserStep));
            }
        }

        [DefaultValue("")]
        public override string SkipLinkText
        {
            get
            {
                string skipLinkTextInternal = base.SkipLinkTextInternal;
                if (skipLinkTextInternal != null)
                {
                    return skipLinkTextInternal;
                }
                return string.Empty;
            }
            set
            {
                base.SkipLinkText = value;
            }
        }

        [NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("LoginControls_TextBoxStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("LoginControls_TitleTextStyle"), WebCategory("Styles"), NotifyParentProperty(true)]
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

        [WebSysDescription("CreateUserWizard_UnknownErrorMessage"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("CreateUserWizard_DefaultUnknownErrorMessage")]
        public virtual string UnknownErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["UnknownErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultUnknownErrorMessage");
            }
            set
            {
                this.ViewState["UnknownErrorMessage"] = value;
            }
        }

        [WebSysDescription("UserName_InitialValue"), WebCategory("Appearance"), DefaultValue("")]
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
                if (string.IsNullOrEmpty(userName) && (this._createUserStepContainer != null))
                {
                    ITextControl userNameTextBox = (ITextControl) this._createUserStepContainer.UserNameTextBox;
                    if (userNameTextBox != null)
                    {
                        return userNameTextBox.Text;
                    }
                }
                return userName;
            }
        }

        [WebSysDescription("LoginControls_UserNameLabelText"), WebCategory("Appearance"), WebSysDefaultValue("CreateUserWizard_DefaultUserNameLabelText"), Localizable(true)]
        public virtual string UserNameLabelText
        {
            get
            {
                object obj2 = this.ViewState["UserNameLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultUserNameLabelText");
            }
            set
            {
                this.ViewState["UserNameLabelText"] = value;
            }
        }

        [Localizable(true), WebCategory("Validation"), WebSysDefaultValue("CreateUserWizard_DefaultUserNameRequiredErrorMessage"), WebSysDescription("ChangePassword_UserNameRequiredErrorMessage")]
        public virtual string UserNameRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["UserNameRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CreateUserWizard_DefaultUserNameRequiredErrorMessage");
            }
            set
            {
                this.ViewState["UserNameRequiredErrorMessage"] = value;
            }
        }

        private string ValidationGroup
        {
            get
            {
                if (this._validationGroup == null)
                {
                    base.EnsureID();
                    this._validationGroup = this.ID;
                }
                return this._validationGroup;
            }
        }

        [WebCategory("Styles"), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("CreateUserWizard_ValidatorTextStyle")]
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

        [Editor("System.Web.UI.Design.WebControls.CreateUserWizardStepCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public override WizardStepCollection WizardSteps
        {
            get
            {
                return base.WizardSteps;
            }
        }

        private sealed class CompleteStepContainer : Wizard.BaseContentTemplateContainer
        {
            internal CompleteStepContainer(CreateUserWizard wizard, bool useInnerTable) : base(wizard, useInnerTable)
            {
            }

            internal ImageButton ContinueImageButton { get; set; }

            internal LinkButton ContinueLinkButton { get; set; }

            internal Button ContinuePushButton { get; set; }

            internal Image EditProfileIcon { get; set; }

            internal HyperLink EditProfileLink { get; set; }

            internal Table LayoutTable { get; set; }

            internal Literal SuccessTextLabel { get; set; }

            internal Literal Title { get; set; }
        }

        private sealed class CreateUserStepContainer : Wizard.BaseContentTemplateContainer
        {
            private Control _answerTextBox;
            private Control _confirmPasswordTextBox;
            private CreateUserWizard _createUserWizard;
            private Control _emailTextBox;
            private Control _passwordTextBox;
            private Control _questionTextBox;
            private Control _unknownErrorMessageLabel;
            private Control _userNameTextBox;

            internal CreateUserStepContainer(CreateUserWizard wizard, bool useInnerTable) : base(wizard, useInnerTable)
            {
                this._createUserWizard = wizard;
            }

            internal LabelLiteral AnswerLabel { get; set; }

            internal RequiredFieldValidator AnswerRequired { get; set; }

            internal Control AnswerTextBox
            {
                get
                {
                    if (this._answerTextBox != null)
                    {
                        return this._answerTextBox;
                    }
                    Control control = this.FindControl("Answer");
                    if (control is IEditableTextControl)
                    {
                        return control;
                    }
                    if (!this._createUserWizard.DesignMode && this._createUserWizard.QuestionAndAnswerRequired)
                    {
                        throw new HttpException(System.Web.SR.GetString("CreateUserWizard_NoAnswerTextBox", new object[] { this._createUserWizard.ID, "Answer" }));
                    }
                    return null;
                }
                set
                {
                    this._answerTextBox = value;
                }
            }

            internal LabelLiteral ConfirmPasswordLabel { get; set; }

            internal RequiredFieldValidator ConfirmPasswordRequired { get; set; }

            internal Control ConfirmPasswordTextBox
            {
                get
                {
                    if (this._confirmPasswordTextBox != null)
                    {
                        return this._confirmPasswordTextBox;
                    }
                    Control control = this.FindControl("ConfirmPassword");
                    if (control is IEditableTextControl)
                    {
                        return control;
                    }
                    return null;
                }
                set
                {
                    this._confirmPasswordTextBox = value;
                }
            }

            internal LabelLiteral EmailLabel { get; set; }

            internal RegularExpressionValidator EmailRegExpValidator { get; set; }

            internal RequiredFieldValidator EmailRequired { get; set; }

            internal Control EmailTextBox
            {
                get
                {
                    if (this._emailTextBox != null)
                    {
                        return this._emailTextBox;
                    }
                    Control control = this.FindControl("Email");
                    if (control is IEditableTextControl)
                    {
                        return control;
                    }
                    if (!this._createUserWizard.DesignMode && this._createUserWizard.RequireEmail)
                    {
                        throw new HttpException(System.Web.SR.GetString("CreateUserWizard_NoEmailTextBox", new object[] { this._createUserWizard.ID, "Email" }));
                    }
                    return null;
                }
                set
                {
                    this._emailTextBox = value;
                }
            }

            internal Control ErrorMessageLabel
            {
                get
                {
                    if (this._unknownErrorMessageLabel != null)
                    {
                        return this._unknownErrorMessageLabel;
                    }
                    Control control = this.FindControl("ErrorMessage");
                    if (control is ITextControl)
                    {
                        return control;
                    }
                    return null;
                }
                set
                {
                    this._unknownErrorMessageLabel = value;
                }
            }

            internal Image HelpPageIcon { get; set; }

            internal HyperLink HelpPageLink { get; set; }

            internal Literal InstructionLabel { get; set; }

            internal CompareValidator PasswordCompareValidator { get; set; }

            internal Literal PasswordHintLabel { get; set; }

            internal LabelLiteral PasswordLabel { get; set; }

            internal RegularExpressionValidator PasswordRegExpValidator { get; set; }

            internal RequiredFieldValidator PasswordRequired { get; set; }

            internal Control PasswordTextBox
            {
                get
                {
                    if (this._passwordTextBox != null)
                    {
                        return this._passwordTextBox;
                    }
                    Control control = this.FindControl("Password");
                    if (control is IEditableTextControl)
                    {
                        return control;
                    }
                    if (!this._createUserWizard.DesignMode && !this._createUserWizard.AutoGeneratePassword)
                    {
                        throw new HttpException(System.Web.SR.GetString("CreateUserWizard_NoPasswordTextBox", new object[] { this._createUserWizard.ID, "Password" }));
                    }
                    return null;
                }
                set
                {
                    this._passwordTextBox = value;
                }
            }

            internal LabelLiteral QuestionLabel { get; set; }

            internal RequiredFieldValidator QuestionRequired { get; set; }

            internal Control QuestionTextBox
            {
                get
                {
                    if (this._questionTextBox != null)
                    {
                        return this._questionTextBox;
                    }
                    Control control = this.FindControl("Question");
                    if (control is IEditableTextControl)
                    {
                        return control;
                    }
                    if (!this._createUserWizard.DesignMode && this._createUserWizard.QuestionAndAnswerRequired)
                    {
                        throw new HttpException(System.Web.SR.GetString("CreateUserWizard_NoQuestionTextBox", new object[] { this._createUserWizard.ID, "Question" }));
                    }
                    return null;
                }
                set
                {
                    this._questionTextBox = value;
                }
            }

            internal Literal Title { get; set; }

            internal LabelLiteral UserNameLabel { get; set; }

            internal RequiredFieldValidator UserNameRequired { get; set; }

            internal Control UserNameTextBox
            {
                get
                {
                    if (this._userNameTextBox != null)
                    {
                        return this._userNameTextBox;
                    }
                    Control control = this.FindControl("UserName");
                    if (control is IEditableTextControl)
                    {
                        return control;
                    }
                    if (!this._createUserWizard.DesignMode)
                    {
                        throw new HttpException(System.Web.SR.GetString("CreateUserWizard_NoUserNameTextBox", new object[] { this._createUserWizard.ID, "UserName" }));
                    }
                    return null;
                }
                set
                {
                    this._userNameTextBox = value;
                }
            }
        }

        private sealed class DataListItemTemplate : ITemplate
        {
            public void InstantiateIn(Control container)
            {
                Label child = new Label();
                child.PreventAutoID();
                child.ID = "SideBarLabel";
                container.Controls.Add(child);
            }
        }

        private sealed class DefaultCompleteStepContentTemplate : ITemplate
        {
            private CreateUserWizard.CompleteStepContainer _completeContainer;

            public DefaultCompleteStepContentTemplate(CreateUserWizard.CompleteStepContainer container)
            {
                this._completeContainer = container;
            }

            private static void AddContinueRow(Table table, CreateUserWizard.CompleteStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(3, new Control[] { container.ContinuePushButton, container.ContinueLinkButton, container.ContinueImageButton });
                table.Rows.Add(row);
            }

            private static void AddEditRow(Table table, CreateUserWizard.CompleteStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(new Control[] { container.EditProfileIcon, container.EditProfileLink });
                table.Rows.Add(row);
            }

            private static void AddSuccessTextRow(Table table, CreateUserWizard.CompleteStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateTableRow();
                TableCell cell = CreateUserWizard.CreateTableCell();
                cell.Controls.Add(container.SuccessTextLabel);
                row.Cells.Add(cell);
                table.Rows.Add(row);
            }

            private static void AddTitleRow(Table table, CreateUserWizard.CompleteStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(2, new Control[] { container.Title });
                table.Rows.Add(row);
            }

            private static void ConstructControls(CreateUserWizard.CompleteStepContainer container)
            {
                container.Title = CreateUserWizard.CreateLiteral();
                container.SuccessTextLabel = CreateUserWizard.CreateLiteral();
                HyperLink link = new HyperLink {
                    ID = "EditProfileLink"
                };
                container.EditProfileLink = link;
                container.EditProfileIcon = new Image();
                container.EditProfileIcon.PreventAutoID();
                LinkButton button = new LinkButton {
                    ID = "ContinueButtonLinkButton",
                    CommandName = CreateUserWizard.ContinueButtonCommandName,
                    CausesValidation = false
                };
                container.ContinueLinkButton = button;
                Button button2 = new Button {
                    ID = "ContinueButtonButton",
                    CommandName = CreateUserWizard.ContinueButtonCommandName,
                    CausesValidation = false
                };
                container.ContinuePushButton = button2;
                ImageButton button3 = new ImageButton {
                    ID = "ContinueButtonImageButton",
                    CommandName = CreateUserWizard.ContinueButtonCommandName,
                    CausesValidation = false
                };
                container.ContinueImageButton = button3;
            }

            private static void LayoutControls(CreateUserWizard.CompleteStepContainer container)
            {
                Table table = CreateUserWizard.CreateTable();
                table.EnableViewState = false;
                AddTitleRow(table, container);
                AddSuccessTextRow(table, container);
                AddContinueRow(table, container);
                AddEditRow(table, container);
                container.LayoutTable = table;
                container.AddChildControl(table);
            }

            void ITemplate.InstantiateIn(Control container)
            {
                ConstructControls(this._completeContainer);
                LayoutControls(this._completeContainer);
            }
        }

        private sealed class DefaultCreateUserContentTemplate : ITemplate
        {
            private CreateUserWizard _wizard;

            internal DefaultCreateUserContentTemplate(CreateUserWizard wizard)
            {
                this._wizard = wizard;
            }

            private void AddAnswerRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                if (this._wizard.ConvertingToTemplate)
                {
                    container.AnswerLabel.RenderAsLabel = true;
                }
                TableRow row = CreateUserWizard.CreateTwoColumnRow(container.AnswerLabel, new Control[] { container.AnswerTextBox, container.AnswerRequired });
                this._wizard._answerRow = row;
                table.Rows.Add(row);
            }

            private void AddConfirmPasswordRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                if (this._wizard.ConvertingToTemplate)
                {
                    container.ConfirmPasswordLabel.RenderAsLabel = true;
                }
                List<Control> list = new List<Control> {
                    container.ConfirmPasswordTextBox
                };
                if (!this._wizard.AutoGeneratePassword)
                {
                    list.Add(container.ConfirmPasswordRequired);
                }
                TableRow row = CreateUserWizard.CreateTwoColumnRow(container.ConfirmPasswordLabel, list.ToArray());
                this._wizard._confirmPasswordTableRow = row;
                table.Rows.Add(row);
            }

            private void AddEmailRegexValidatorRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(2, new Control[] { container.EmailRegExpValidator });
                this._wizard._emailRegExpRow = row;
                table.Rows.Add(row);
            }

            private void AddEmailRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                if (this._wizard.ConvertingToTemplate)
                {
                    container.EmailLabel.RenderAsLabel = true;
                }
                TableRow row = CreateUserWizard.CreateTwoColumnRow(container.EmailLabel, new Control[] { container.EmailTextBox, container.EmailRequired });
                this._wizard._emailRow = row;
                table.Rows.Add(row);
            }

            private static void AddErrorMessageRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(2, new Control[] { container.ErrorMessageLabel });
                table.Rows.Add(row);
            }

            private static void AddHelpPageLinkRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(new Control[] { container.HelpPageIcon, container.HelpPageLink });
                table.Rows.Add(row);
            }

            private static void AddInstructionRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(2, new Control[] { container.InstructionLabel });
                row.PreventAutoID();
                table.Rows.Add(row);
            }

            private void AddPasswordCompareValidatorRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(2, new Control[] { container.PasswordCompareValidator });
                this._wizard._passwordCompareRow = row;
                table.Rows.Add(row);
            }

            private void AddPasswordHintRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateTableRow();
                TableCell cell = CreateUserWizard.CreateTableCell();
                row.Cells.Add(cell);
                TableCell cell2 = CreateUserWizard.CreateTableCell();
                cell2.Controls.Add(container.PasswordHintLabel);
                row.Cells.Add(cell2);
                this._wizard._passwordHintTableRow = row;
                table.Rows.Add(row);
            }

            private void AddPasswordRegexValidatorRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(2, new Control[] { container.PasswordRegExpValidator });
                this._wizard._passwordRegExpRow = row;
                table.Rows.Add(row);
            }

            private void AddPasswordRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                if (this._wizard.ConvertingToTemplate)
                {
                    container.PasswordLabel.RenderAsLabel = true;
                }
                List<Control> list = new List<Control> {
                    container.PasswordTextBox
                };
                if (!this._wizard.AutoGeneratePassword)
                {
                    list.Add(container.PasswordRequired);
                }
                TableRow row = CreateUserWizard.CreateTwoColumnRow(container.PasswordLabel, list.ToArray());
                this._wizard._passwordTableRow = row;
                table.Rows.Add(row);
            }

            private void AddQuestionRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                if (this._wizard.ConvertingToTemplate)
                {
                    container.QuestionLabel.RenderAsLabel = true;
                }
                TableRow row = CreateUserWizard.CreateTwoColumnRow(container.QuestionLabel, new Control[] { container.QuestionTextBox, container.QuestionRequired });
                this._wizard._questionRow = row;
                table.Rows.Add(row);
            }

            private static void AddTitleRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                TableRow row = CreateUserWizard.CreateDoubleSpannedColumnRow(2, new Control[] { container.Title });
                table.Rows.Add(row);
            }

            private void AddUserNameRow(Table table, CreateUserWizard.CreateUserStepContainer container)
            {
                if (this._wizard.ConvertingToTemplate)
                {
                    container.UserNameLabel.RenderAsLabel = true;
                }
                TableRow row = CreateUserWizard.CreateTwoColumnRow(container.UserNameLabel, new Control[] { container.UserNameTextBox, container.UserNameRequired });
                table.Rows.Add(row);
            }

            private void ConstructControls(CreateUserWizard.CreateUserStepContainer container)
            {
                string validationGroup = this._wizard.ValidationGroup;
                container.Title = CreateUserWizard.CreateLiteral();
                container.InstructionLabel = CreateUserWizard.CreateLiteral();
                container.PasswordHintLabel = CreateUserWizard.CreateLiteral();
                TextBox box = new TextBox {
                    ID = "UserName"
                };
                container.UserNameTextBox = box;
                TextBox box2 = new TextBox {
                    ID = "Password",
                    TextMode = TextBoxMode.Password
                };
                container.PasswordTextBox = box2;
                TextBox box3 = new TextBox {
                    ID = "ConfirmPassword",
                    TextMode = TextBoxMode.Password
                };
                container.ConfirmPasswordTextBox = box3;
                bool enableValidation = true;
                container.UserNameRequired = CreateUserWizard.CreateRequiredFieldValidator("UserNameRequired", validationGroup, container.UserNameTextBox, enableValidation);
                container.UserNameLabel = CreateUserWizard.CreateLabelLiteral(container.UserNameTextBox);
                container.PasswordLabel = CreateUserWizard.CreateLabelLiteral(container.PasswordTextBox);
                container.ConfirmPasswordLabel = CreateUserWizard.CreateLabelLiteral(container.ConfirmPasswordTextBox);
                Image image = new Image();
                image.PreventAutoID();
                container.HelpPageIcon = image;
                HyperLink link = new HyperLink {
                    ID = "HelpLink"
                };
                container.HelpPageLink = link;
                Literal literal = new Literal {
                    ID = "ErrorMessage"
                };
                container.ErrorMessageLabel = literal;
                TextBox box4 = new TextBox {
                    ID = "Email"
                };
                container.EmailTextBox = box4;
                container.EmailRequired = CreateUserWizard.CreateRequiredFieldValidator("EmailRequired", validationGroup, container.EmailTextBox, enableValidation);
                container.EmailLabel = CreateUserWizard.CreateLabelLiteral(container.EmailTextBox);
                RegularExpressionValidator validator = new RegularExpressionValidator {
                    ID = "EmailRegExp",
                    ControlToValidate = "Email",
                    ErrorMessage = this._wizard.EmailRegularExpressionErrorMessage,
                    ValidationExpression = this._wizard.EmailRegularExpression,
                    ValidationGroup = validationGroup,
                    Display = ValidatorDisplay.Dynamic,
                    Enabled = enableValidation,
                    Visible = enableValidation
                };
                container.EmailRegExpValidator = validator;
                container.PasswordRequired = CreateUserWizard.CreateRequiredFieldValidator("PasswordRequired", validationGroup, container.PasswordTextBox, enableValidation);
                container.ConfirmPasswordRequired = CreateUserWizard.CreateRequiredFieldValidator("ConfirmPasswordRequired", validationGroup, container.ConfirmPasswordTextBox, enableValidation);
                RegularExpressionValidator validator2 = new RegularExpressionValidator {
                    ID = "PasswordRegExp",
                    ControlToValidate = "Password",
                    ErrorMessage = this._wizard.PasswordRegularExpressionErrorMessage,
                    ValidationExpression = this._wizard.PasswordRegularExpression,
                    ValidationGroup = validationGroup,
                    Display = ValidatorDisplay.Dynamic,
                    Enabled = enableValidation,
                    Visible = enableValidation
                };
                container.PasswordRegExpValidator = validator2;
                CompareValidator validator3 = new CompareValidator {
                    ID = "PasswordCompare",
                    ControlToValidate = "ConfirmPassword",
                    ControlToCompare = "Password",
                    Operator = ValidationCompareOperator.Equal,
                    ErrorMessage = this._wizard.ConfirmPasswordCompareErrorMessage,
                    ValidationGroup = validationGroup,
                    Display = ValidatorDisplay.Dynamic,
                    Enabled = enableValidation,
                    Visible = enableValidation
                };
                container.PasswordCompareValidator = validator3;
                TextBox box5 = new TextBox {
                    ID = "Question"
                };
                container.QuestionTextBox = box5;
                TextBox box6 = new TextBox {
                    ID = "Answer"
                };
                container.AnswerTextBox = box6;
                container.QuestionRequired = CreateUserWizard.CreateRequiredFieldValidator("QuestionRequired", validationGroup, container.QuestionTextBox, enableValidation);
                container.AnswerRequired = CreateUserWizard.CreateRequiredFieldValidator("AnswerRequired", validationGroup, container.AnswerTextBox, enableValidation);
                container.QuestionLabel = CreateUserWizard.CreateLabelLiteral(container.QuestionTextBox);
                container.AnswerLabel = CreateUserWizard.CreateLabelLiteral(container.AnswerTextBox);
            }

            private void LayoutControls(CreateUserWizard.CreateUserStepContainer container)
            {
                Table table = CreateUserWizard.CreateTable();
                table.EnableViewState = false;
                AddTitleRow(table, container);
                AddInstructionRow(table, container);
                this.AddUserNameRow(table, container);
                this.AddPasswordRow(table, container);
                this.AddPasswordHintRow(table, container);
                this.AddConfirmPasswordRow(table, container);
                this.AddEmailRow(table, container);
                this.AddQuestionRow(table, container);
                this.AddAnswerRow(table, container);
                this.AddPasswordCompareValidatorRow(table, container);
                this.AddPasswordRegexValidatorRow(table, container);
                this.AddEmailRegexValidatorRow(table, container);
                AddErrorMessageRow(table, container);
                AddHelpPageLinkRow(table, container);
                container.AddChildControl(table);
            }

            void ITemplate.InstantiateIn(Control container)
            {
                CreateUserWizard.CreateUserStepContainer container2 = this._wizard._createUserStepContainer;
                this.ConstructControls(container2);
                this.LayoutControls(container2);
            }
        }

        private sealed class DefaultCreateUserNavigationTemplate : ITemplate
        {
            private IButtonControl[][] _buttons;
            private TableCell[] _innerCells;
            private TableRow _row;
            private CreateUserWizard _wizard;

            internal DefaultCreateUserNavigationTemplate(CreateUserWizard wizard)
            {
                this._wizard = wizard;
            }

            internal void ApplyLayoutStyleToInnerCells(TableItemStyle tableItemStyle)
            {
                for (int i = 0; i < this._innerCells.Length; i++)
                {
                    if (tableItemStyle.IsSet(0x10000))
                    {
                        this._innerCells[i].HorizontalAlign = tableItemStyle.HorizontalAlign;
                    }
                    if (tableItemStyle.IsSet(0x20000))
                    {
                        this._innerCells[i].VerticalAlign = tableItemStyle.VerticalAlign;
                    }
                }
            }

            private TableCell CreateButtonControl(IButtonControl[] buttons, string validationGroup, string id, bool causesValidation, string commandName)
            {
                LinkButton child = new LinkButton {
                    CausesValidation = causesValidation,
                    ID = id + "LinkButton",
                    Visible = false,
                    CommandName = commandName,
                    ValidationGroup = validationGroup
                };
                buttons[0] = child;
                ImageButton button2 = new ImageButton {
                    CausesValidation = causesValidation,
                    ID = id + "ImageButton",
                    Visible = !this._wizard.DesignMode,
                    CommandName = commandName,
                    ValidationGroup = validationGroup
                };
                button2.PreRender += new EventHandler(this.OnPreRender);
                buttons[1] = button2;
                Button button3 = new Button {
                    CausesValidation = causesValidation,
                    ID = id + "Button",
                    Visible = false,
                    CommandName = commandName,
                    ValidationGroup = validationGroup
                };
                buttons[2] = button3;
                TableCell cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                this._row.Cells.Add(cell);
                cell.Controls.Add(child);
                cell.Controls.Add(button2);
                cell.Controls.Add(button3);
                return cell;
            }

            private IButtonControl GetButtonBasedOnType(int pos, ButtonType type)
            {
                switch (type)
                {
                    case ButtonType.Button:
                        return this._buttons[pos][2];

                    case ButtonType.Image:
                        return this._buttons[pos][1];

                    case ButtonType.Link:
                        return this._buttons[pos][0];
                }
                return null;
            }

            private void OnPreRender(object source, EventArgs e)
            {
                ((ImageButton) source).Visible = false;
            }

            void ITemplate.InstantiateIn(Control container)
            {
                this._wizard._defaultCreateUserNavigationTemplate = this;
                container.EnableViewState = false;
                Table child = CreateUserWizard.CreateTable();
                child.CellSpacing = 5;
                child.CellPadding = 5;
                container.Controls.Add(child);
                TableRow row = new TableRow();
                this._row = row;
                row.PreventAutoID();
                row.HorizontalAlign = HorizontalAlign.Right;
                child.Rows.Add(row);
                this._buttons = new IButtonControl[][] { new IButtonControl[3], new IButtonControl[3], new IButtonControl[3] };
                this._innerCells = new TableCell[] { this.CreateButtonControl(this._buttons[0], this._wizard.ValidationGroup, Wizard.StepPreviousButtonID, false, Wizard.MovePreviousCommandName), this.CreateButtonControl(this._buttons[1], this._wizard.ValidationGroup, Wizard.StepNextButtonID, true, Wizard.MoveNextCommandName), this.CreateButtonControl(this._buttons[2], this._wizard.ValidationGroup, Wizard.CancelButtonID, false, Wizard.CancelCommandName) };
            }

            internal IButtonControl CancelButton
            {
                get
                {
                    return this.GetButtonBasedOnType(2, this._wizard.CancelButtonType);
                }
            }

            internal IButtonControl CreateUserButton
            {
                get
                {
                    return this.GetButtonBasedOnType(1, this._wizard.CreateUserButtonType);
                }
            }

            internal IButtonControl PreviousButton
            {
                get
                {
                    return this.GetButtonBasedOnType(0, this._wizard.StepPreviousButtonType);
                }
            }
        }

        private sealed class DefaultSideBarTemplate : ITemplate
        {
            public void InstantiateIn(Control container)
            {
                DataList child = new DataList {
                    ID = Wizard.DataListID
                };
                container.Controls.Add(child);
                child.SelectedItemStyle.Font.Bold = true;
                child.ItemTemplate = new CreateUserWizard.DataListItemTemplate();
            }
        }

        private class LayoutTemplateWizardRendering : Wizard.LayoutTemplateWizardRendering
        {
            public LayoutTemplateWizardRendering(CreateUserWizard owner) : base(owner)
            {
                this.Owner = owner;
            }

            public override void ApplyControlProperties()
            {
                this.Owner.SetChildProperties();
                if (this.Owner.CreateUserStep.CustomNavigationTemplate == null)
                {
                    this.Owner.SetDefaultCreateUserNavigationTemplateProperties();
                }
                base.ApplyControlProperties();
            }

            public override void CreateControlHierarchy()
            {
                this.Owner.EnsureCreateUserSteps();
                base.CreateControlHierarchy();
                this.Owner.InstantiateStepContentTemplates();
                this.Owner.RegisterEvents();
                this.Owner.ApplyCommonCreateUserValues();
            }

            private CreateUserWizard Owner { get; set; }
        }

        private class TableWizardRendering : Wizard.TableWizardRendering
        {
            public TableWizardRendering(CreateUserWizard wizard) : base(wizard)
            {
                this.Owner = wizard;
            }

            public override void ApplyControlProperties()
            {
                this.Owner.SetChildProperties();
                if (this.Owner.CreateUserStep.CustomNavigationTemplate == null)
                {
                    this.Owner.SetDefaultCreateUserNavigationTemplateProperties();
                }
                base.ApplyControlProperties();
            }

            public override void CreateControlHierarchy()
            {
                this.Owner.EnsureCreateUserSteps();
                base.CreateControlHierarchy();
                this.Owner.RegisterEvents();
                this.Owner.ApplyCommonCreateUserValues();
            }

            private CreateUserWizard Owner { get; set; }
        }
    }
}

