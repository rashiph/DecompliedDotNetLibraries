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

    [DefaultEvent("SendingMail"), Designer("System.Web.UI.Design.WebControls.PasswordRecoveryDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false)]
    public class PasswordRecovery : CompositeControl, IBorderPaddingControl, IRenderOuterTableControl
    {
        private string _answer;
        private const string _answerID = "Answer";
        private const string _answerRequiredID = "AnswerRequired";
        private bool _convertingToTemplate;
        private View _currentView;
        private const string _failureTextID = "FailureText";
        private TableItemStyle _failureTextStyle;
        private const string _helpLinkID = "HelpLink";
        private TableItemStyle _hyperLinkStyle;
        private const string _imageButtonID = "SubmitImageButton";
        private TableItemStyle _instructionTextStyle;
        private TableItemStyle _labelStyle;
        private const string _linkButtonID = "SubmitLinkButton";
        private System.Web.UI.WebControls.MailDefinition _mailDefinition;
        private const string _passwordReplacementKey = @"<%\s*Password\s*%>";
        private const string _pushButtonID = "SubmitButton";
        private string _question;
        private QuestionContainer _questionContainer;
        private const string _questionContainerID = "QuestionContainerID";
        private const string _questionID = "Question";
        private ITemplate _questionTemplate;
        private bool _renderDesignerRegion;
        private const ValidatorDisplay _requiredFieldValidatorDisplay = ValidatorDisplay.Static;
        private Style _submitButtonStyle;
        private SuccessContainer _successContainer;
        private const string _successContainerID = "SuccessContainerID";
        private ITemplate _successTemplate;
        private TableItemStyle _successTextStyle;
        private Style _textBoxStyle;
        private TableItemStyle _titleTextStyle;
        private string _userName;
        private UserNameContainer _userNameContainer;
        private const string _userNameContainerID = "UserNameContainerID";
        private const string _userNameID = "UserName";
        private const string _userNameReplacementKey = @"<%\s*UserName\s*%>";
        private const string _userNameRequiredID = "UserNameRequired";
        private ITemplate _userNameTemplate;
        private Style _validatorTextStyle;
        private const int _viewStateArrayLength = 11;
        private static readonly object EventAnswerLookupError = new object();
        private static readonly object EventSendingMail = new object();
        private static readonly object EventSendMailError = new object();
        private static readonly object EventUserLookupError = new object();
        private static readonly object EventVerifyingAnswer = new object();
        private static readonly object EventVerifyingUser = new object();
        public static readonly string SubmitButtonCommandName = "Submit";

        [WebSysDescription("PasswordRecovery_AnswerLookupError"), WebCategory("Action")]
        public event EventHandler AnswerLookupError
        {
            add
            {
                base.Events.AddHandler(EventAnswerLookupError, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAnswerLookupError, value);
            }
        }

        [WebSysDescription("PasswordRecovery_SendingMail"), WebCategory("Action")]
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

        [WebSysDescription("CreateUserWizard_SendMailError"), WebCategory("Action")]
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

        [WebSysDescription("PasswordRecovery_UserLookupError"), WebCategory("Action")]
        public event EventHandler UserLookupError
        {
            add
            {
                base.Events.AddHandler(EventUserLookupError, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventUserLookupError, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("PasswordRecovery_VerifyingAnswer")]
        public event LoginCancelEventHandler VerifyingAnswer
        {
            add
            {
                base.Events.AddHandler(EventVerifyingAnswer, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventVerifyingAnswer, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("PasswordRecovery_VerifyingUser")]
        public event LoginCancelEventHandler VerifyingUser
        {
            add
            {
                base.Events.AddHandler(EventVerifyingUser, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventVerifyingUser, value);
            }
        }

        private void AnswerTextChanged(object source, EventArgs e)
        {
            this._answer = ((ITextControl) source).Text;
        }

        private void AttemptSendPassword()
        {
            if ((this.Page == null) || this.Page.IsValid)
            {
                if (this.CurrentView == View.UserName)
                {
                    this.AttemptSendPasswordUserNameView();
                }
                else if (this.CurrentView == View.Question)
                {
                    this.AttemptSendPasswordQuestionView();
                }
            }
        }

        private void AttemptSendPasswordQuestionView()
        {
            System.Web.Security.MembershipProvider provider = LoginUtil.GetProvider(this.MembershipProvider);
            MembershipUser user = provider.GetUser(this.UserNameInternal, false, false);
            if (user != null)
            {
                if (user.IsLockedOut)
                {
                    this.SetFailureTextLabel(this._questionContainer, this.GeneralFailureText);
                }
                else
                {
                    this.Question = user.PasswordQuestion;
                    if (string.IsNullOrEmpty(this.Question))
                    {
                        this.SetFailureTextLabel(this._questionContainer, this.GeneralFailureText);
                    }
                    else
                    {
                        LoginCancelEventArgs e = new LoginCancelEventArgs();
                        this.OnVerifyingAnswer(e);
                        if (!e.Cancel)
                        {
                            string answerInternal = this.AnswerInternal;
                            string password = null;
                            string email = user.Email;
                            if (string.IsNullOrEmpty(email))
                            {
                                this.SetFailureTextLabel(this._questionContainer, this.GeneralFailureText);
                            }
                            else
                            {
                                if (provider.EnablePasswordRetrieval)
                                {
                                    password = user.GetPassword(answerInternal, false);
                                }
                                else
                                {
                                    if (!provider.EnablePasswordReset)
                                    {
                                        throw new HttpException(System.Web.SR.GetString("PasswordRecovery_RecoveryNotSupported"));
                                    }
                                    password = user.ResetPassword(answerInternal, false);
                                }
                                if (password != null)
                                {
                                    LoginUtil.SendPasswordMail(email, user.UserName, password, this.MailDefinition, System.Web.SR.GetString("PasswordRecovery_DefaultSubject"), System.Web.SR.GetString("PasswordRecovery_DefaultBody"), new LoginUtil.OnSendingMailDelegate(this.OnSendingMail), new LoginUtil.OnSendMailErrorDelegate(this.OnSendMailError), this);
                                    this.PerformSuccessAction();
                                }
                                else
                                {
                                    this.OnAnswerLookupError(EventArgs.Empty);
                                    this.SetFailureTextLabel(this._questionContainer, this.QuestionFailureText);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                this.SetFailureTextLabel(this._questionContainer, this.GeneralFailureText);
            }
        }

        private void AttemptSendPasswordUserNameView()
        {
            LoginCancelEventArgs e = new LoginCancelEventArgs();
            this.OnVerifyingUser(e);
            if (!e.Cancel)
            {
                System.Web.Security.MembershipProvider provider = LoginUtil.GetProvider(this.MembershipProvider);
                MembershipUser user = provider.GetUser(this.UserNameInternal, false, false);
                if (user != null)
                {
                    if (user.IsLockedOut)
                    {
                        this.SetFailureTextLabel(this._userNameContainer, this.UserNameFailureText);
                    }
                    else if (provider.RequiresQuestionAndAnswer)
                    {
                        this.Question = user.PasswordQuestion;
                        if (string.IsNullOrEmpty(this.Question))
                        {
                            this.SetFailureTextLabel(this._userNameContainer, this.GeneralFailureText);
                        }
                        else
                        {
                            this.CurrentView = View.Question;
                        }
                    }
                    else
                    {
                        string password = null;
                        string email = user.Email;
                        if (string.IsNullOrEmpty(email))
                        {
                            this.SetFailureTextLabel(this._userNameContainer, this.GeneralFailureText);
                        }
                        else
                        {
                            if (provider.EnablePasswordRetrieval)
                            {
                                password = user.GetPassword(false);
                            }
                            else
                            {
                                if (!provider.EnablePasswordReset)
                                {
                                    throw new HttpException(System.Web.SR.GetString("PasswordRecovery_RecoveryNotSupported"));
                                }
                                password = user.ResetPassword(false);
                            }
                            if (password != null)
                            {
                                LoginUtil.SendPasswordMail(email, user.UserName, password, this.MailDefinition, System.Web.SR.GetString("PasswordRecovery_DefaultSubject"), System.Web.SR.GetString("PasswordRecovery_DefaultBody"), new LoginUtil.OnSendingMailDelegate(this.OnSendingMail), new LoginUtil.OnSendMailErrorDelegate(this.OnSendMailError), this);
                                this.PerformSuccessAction();
                            }
                            else
                            {
                                this.SetFailureTextLabel(this._userNameContainer, this.GeneralFailureText);
                            }
                        }
                    }
                }
                else
                {
                    this.OnUserLookupError(EventArgs.Empty);
                    this.SetFailureTextLabel(this._userNameContainer, this.UserNameFailureText);
                }
            }
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            this.CreateUserView();
            this.CreateQuestionView();
            this.CreateSuccessView();
        }

        private void CreateQuestionView()
        {
            ITemplate questionTemplate = null;
            this._questionContainer = new QuestionContainer(this);
            this._questionContainer.ID = "QuestionContainerID";
            this._questionContainer.RenderDesignerRegion = this._renderDesignerRegion;
            if (this.QuestionTemplate != null)
            {
                questionTemplate = this.QuestionTemplate;
            }
            else
            {
                questionTemplate = new DefaultQuestionTemplate(this);
                this._questionContainer.EnableViewState = false;
                this._questionContainer.EnableTheming = false;
            }
            questionTemplate.InstantiateIn(this._questionContainer);
            this.Controls.Add(this._questionContainer);
            IEditableTextControl answerTextBox = this._questionContainer.AnswerTextBox as IEditableTextControl;
            if (answerTextBox != null)
            {
                answerTextBox.TextChanged += new EventHandler(this.AnswerTextChanged);
            }
        }

        private void CreateSuccessView()
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

        private void CreateUserView()
        {
            ITemplate userNameTemplate = null;
            this._userNameContainer = new UserNameContainer(this);
            this._userNameContainer.ID = "UserNameContainerID";
            this._userNameContainer.RenderDesignerRegion = this._renderDesignerRegion;
            if (this.UserNameTemplate != null)
            {
                userNameTemplate = this.UserNameTemplate;
            }
            else
            {
                userNameTemplate = new DefaultUserNameTemplate(this);
                this._userNameContainer.EnableViewState = false;
                this._userNameContainer.EnableTheming = false;
            }
            userNameTemplate.InstantiateIn(this._userNameContainer);
            this.Controls.Add(this._userNameContainer);
            this.SetUserNameEditableChildProperties();
            IEditableTextControl userNameTextBox = this._userNameContainer.UserNameTextBox as IEditableTextControl;
            if (userNameTextBox != null)
            {
                userNameTextBox.TextChanged += new EventHandler(this.UserNameTextChanged);
            }
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
                    this.CurrentView = (View) ((int) triplet.Second);
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
                if (objArray.Length != 11)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.SubmitButtonStyle).LoadViewState(objArray[1]);
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
                    ((IStateManager) this.FailureTextStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.SuccessTextStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.MailDefinition).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    ((IStateManager) this.ValidatorTextStyle).LoadViewState(objArray[10]);
                }
            }
        }

        protected virtual void OnAnswerLookupError(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventAnswerLookupError];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            bool flag = false;
            if (e is CommandEventArgs)
            {
                CommandEventArgs args = (CommandEventArgs) e;
                if (args.CommandName.Equals(SubmitButtonCommandName, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.AttemptSendPassword();
                    flag = true;
                }
            }
            return flag;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page.RegisterRequiresControlState(this);
            this.Page.LoadComplete += new EventHandler(this.OnPageLoadComplete);
        }

        private void OnPageLoadComplete(object sender, EventArgs e)
        {
            if ((this.CurrentView == View.Question) && string.IsNullOrEmpty(this.Question))
            {
                MembershipUser user = LoginUtil.GetProvider(this.MembershipProvider).GetUser(this.UserNameInternal, false, false);
                if (user != null)
                {
                    this.Question = user.PasswordQuestion;
                    if (string.IsNullOrEmpty(this.Question))
                    {
                        this.SetFailureTextLabel(this._questionContainer, this.GeneralFailureText);
                    }
                }
                else
                {
                    this.SetFailureTextLabel(this._questionContainer, this.GeneralFailureText);
                }
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this._userNameContainer.Visible = false;
            this._questionContainer.Visible = false;
            this._successContainer.Visible = false;
            switch (this.CurrentView)
            {
                case View.UserName:
                    this._userNameContainer.Visible = true;
                    this.SetUserNameEditableChildProperties();
                    return;

                case View.Question:
                    this._questionContainer.Visible = true;
                    return;

                case View.Success:
                    this._successContainer.Visible = true;
                    return;
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

        protected virtual void OnUserLookupError(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventUserLookupError];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVerifyingAnswer(LoginCancelEventArgs e)
        {
            LoginCancelEventHandler handler = (LoginCancelEventHandler) base.Events[EventVerifyingAnswer];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVerifyingUser(LoginCancelEventArgs e)
        {
            LoginCancelEventHandler handler = (LoginCancelEventHandler) base.Events[EventVerifyingUser];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void PerformSuccessAction()
        {
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
            this.EnsureChildControls();
            if (base.DesignMode)
            {
                this._userNameContainer.Visible = false;
                this._questionContainer.Visible = false;
                this._successContainer.Visible = false;
                switch (this.CurrentView)
                {
                    case View.UserName:
                        this._userNameContainer.Visible = true;
                        break;

                    case View.Question:
                        this._questionContainer.Visible = true;
                        break;

                    case View.Success:
                        this._successContainer.Visible = true;
                        break;
                }
            }
            switch (this.CurrentView)
            {
                case View.UserName:
                    this.SetUserNameChildProperties();
                    break;

                case View.Question:
                    this.SetQuestionChildProperties();
                    break;

                case View.Success:
                    this.SetSuccessChildProperties();
                    break;
            }
            this.RenderContents(writer);
        }

        protected internal override object SaveControlState()
        {
            object x = base.SaveControlState();
            if (((x == null) && (this._currentView == View.UserName)) && (this._userName == null))
            {
                return null;
            }
            object y = null;
            object z = null;
            if (this._currentView != View.UserName)
            {
                y = (int) this._currentView;
            }
            if ((this._userName != null) && (this._currentView != View.Success))
            {
                z = this._userName;
            }
            return new Triplet(x, y, z);
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._submitButtonStyle != null) ? ((IStateManager) this._submitButtonStyle).SaveViewState() : null, (this._labelStyle != null) ? ((IStateManager) this._labelStyle).SaveViewState() : null, (this._textBoxStyle != null) ? ((IStateManager) this._textBoxStyle).SaveViewState() : null, (this._hyperLinkStyle != null) ? ((IStateManager) this._hyperLinkStyle).SaveViewState() : null, (this._instructionTextStyle != null) ? ((IStateManager) this._instructionTextStyle).SaveViewState() : null, (this._titleTextStyle != null) ? ((IStateManager) this._titleTextStyle).SaveViewState() : null, (this._failureTextStyle != null) ? ((IStateManager) this._failureTextStyle).SaveViewState() : null, (this._successTextStyle != null) ? ((IStateManager) this._successTextStyle).SaveViewState() : null, (this._mailDefinition != null) ? ((IStateManager) this._mailDefinition).SaveViewState() : null, (this._validatorTextStyle != null) ? ((IStateManager) this._validatorTextStyle).SaveViewState() : null };
            for (int i = 0; i < 11; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
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

        private void SetFailureTextLabel(QuestionContainer container, string failureText)
        {
            ITextControl failureTextLabel = (ITextControl) container.FailureTextLabel;
            if (failureTextLabel != null)
            {
                failureTextLabel.Text = failureText;
            }
        }

        private void SetFailureTextLabel(UserNameContainer container, string failureText)
        {
            ITextControl failureTextLabel = (ITextControl) container.FailureTextLabel;
            if (failureTextLabel != null)
            {
                failureTextLabel.Text = failureText;
            }
        }

        internal void SetQuestionChildProperties()
        {
            this.SetQuestionCommonChildProperties();
            if (this.QuestionTemplate == null)
            {
                this.SetQuestionDefaultChildProperties();
            }
        }

        private void SetQuestionCommonChildProperties()
        {
            QuestionContainer child = this._questionContainer;
            Util.CopyBaseAttributesToInnerControl(this, child);
            child.ApplyStyle(base.ControlStyle);
            ITextControl userName = (ITextControl) child.UserName;
            if (userName != null)
            {
                userName.Text = HttpUtility.HtmlEncode(this.UserNameInternal);
            }
            ITextControl question = (ITextControl) child.Question;
            if (question != null)
            {
                question.Text = HttpUtility.HtmlEncode(this.Question);
            }
            ITextControl answerTextBox = (ITextControl) child.AnswerTextBox;
            if (answerTextBox != null)
            {
                answerTextBox.Text = string.Empty;
            }
        }

        private void SetQuestionDefaultChildProperties()
        {
            QuestionContainer container = this._questionContainer;
            container.BorderTable.CellPadding = this.BorderPadding;
            container.BorderTable.CellSpacing = 0;
            Literal title = container.Title;
            string questionTitleText = this.QuestionTitleText;
            if (questionTitleText.Length > 0)
            {
                title.Text = questionTitleText;
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
            Literal instruction = container.Instruction;
            string questionInstructionText = this.QuestionInstructionText;
            if (questionInstructionText.Length > 0)
            {
                instruction.Text = questionInstructionText;
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
            Literal userNameLabel = container.UserNameLabel;
            string userNameLabelText = this.UserNameLabelText;
            if (userNameLabelText.Length > 0)
            {
                userNameLabel.Text = userNameLabelText;
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
            Control userName = container.UserName;
            if (this.UserNameInternal.Length > 0)
            {
                userName.Visible = true;
            }
            else
            {
                userName.Visible = false;
            }
            if (userName is WebControl)
            {
                ((WebControl) userName).TabIndex = this.TabIndex;
            }
            Literal questionLabel = container.QuestionLabel;
            string questionLabelText = this.QuestionLabelText;
            if (questionLabelText.Length > 0)
            {
                questionLabel.Text = questionLabelText;
                if (this._labelStyle != null)
                {
                    LoginUtil.SetTableCellStyle(questionLabel, this.LabelStyle);
                }
                questionLabel.Visible = true;
            }
            else
            {
                questionLabel.Visible = false;
            }
            Control question = container.Question;
            if (this.Question.Length > 0)
            {
                question.Visible = true;
            }
            else
            {
                question.Visible = false;
            }
            Literal answerLabel = container.AnswerLabel;
            string answerLabelText = this.AnswerLabelText;
            if (answerLabelText.Length > 0)
            {
                answerLabel.Text = answerLabelText;
                if (this._labelStyle != null)
                {
                    LoginUtil.SetTableCellStyle(answerLabel, this.LabelStyle);
                }
                answerLabel.Visible = true;
            }
            else
            {
                answerLabel.Visible = false;
            }
            WebControl answerTextBox = (WebControl) container.AnswerTextBox;
            if (this._textBoxStyle != null)
            {
                answerTextBox.ApplyStyle(this.TextBoxStyle);
            }
            answerTextBox.TabIndex = this.TabIndex;
            answerTextBox.AccessKey = this.AccessKey;
            bool flag = this.CurrentView == View.Question;
            RequiredFieldValidator answerRequired = container.AnswerRequired;
            answerRequired.ErrorMessage = this.AnswerRequiredErrorMessage;
            answerRequired.ToolTip = this.AnswerRequiredErrorMessage;
            answerRequired.Enabled = flag;
            answerRequired.Visible = flag;
            if (this._validatorTextStyle != null)
            {
                answerRequired.ApplyStyle(this._validatorTextStyle);
            }
            LinkButton linkButton = container.LinkButton;
            ImageButton imageButton = container.ImageButton;
            Button pushButton = container.PushButton;
            WebControl control4 = null;
            switch (this.SubmitButtonType)
            {
                case ButtonType.Button:
                    pushButton.Text = this.SubmitButtonText;
                    control4 = pushButton;
                    break;

                case ButtonType.Image:
                    imageButton.ImageUrl = this.SubmitButtonImageUrl;
                    imageButton.AlternateText = this.SubmitButtonText;
                    control4 = imageButton;
                    break;

                case ButtonType.Link:
                    linkButton.Text = this.SubmitButtonText;
                    control4 = linkButton;
                    break;
            }
            linkButton.Visible = false;
            imageButton.Visible = false;
            pushButton.Visible = false;
            control4.Visible = true;
            control4.TabIndex = this.TabIndex;
            if (this._submitButtonStyle != null)
            {
                control4.ApplyStyle(this.SubmitButtonStyle);
            }
            HyperLink helpPageLink = container.HelpPageLink;
            string helpPageText = this.HelpPageText;
            Image helpPageIcon = container.HelpPageIcon;
            if (helpPageText.Length > 0)
            {
                helpPageLink.Text = helpPageText;
                helpPageLink.NavigateUrl = this.HelpPageUrl;
                helpPageLink.TabIndex = this.TabIndex;
                helpPageLink.Visible = true;
            }
            else
            {
                helpPageLink.Visible = false;
            }
            string helpPageIconUrl = this.HelpPageIconUrl;
            bool flag2 = helpPageIconUrl.Length > 0;
            helpPageIcon.Visible = flag2;
            if (flag2)
            {
                helpPageIcon.ImageUrl = helpPageIconUrl;
                helpPageIcon.AlternateText = helpPageText;
            }
            if (helpPageLink.Visible || helpPageIcon.Visible)
            {
                if (this._hyperLinkStyle != null)
                {
                    TableItemStyle style = new TableItemStyle();
                    style.CopyFrom(this.HyperLinkStyle);
                    style.Font.Reset();
                    LoginUtil.SetTableCellStyle(helpPageLink, style);
                    helpPageLink.Font.CopyFrom(this.HyperLinkStyle.Font);
                    helpPageLink.ForeColor = this.HyperLinkStyle.ForeColor;
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

        internal void SetSuccessChildProperties()
        {
            SuccessContainer child = this._successContainer;
            Util.CopyBaseAttributesToInnerControl(this, child);
            child.ApplyStyle(base.ControlStyle);
            if (this.SuccessTemplate == null)
            {
                child.BorderTable.CellPadding = this.BorderPadding;
                child.BorderTable.CellSpacing = 0;
                Literal successTextLabel = child.SuccessTextLabel;
                string successText = this.SuccessText;
                if (successText.Length > 0)
                {
                    successTextLabel.Text = successText;
                    if (this._successTextStyle != null)
                    {
                        LoginUtil.SetTableCellStyle(successTextLabel, this._successTextStyle);
                    }
                    LoginUtil.SetTableCellVisible(successTextLabel, true);
                }
                else
                {
                    LoginUtil.SetTableCellVisible(successTextLabel, false);
                }
            }
        }

        internal void SetUserNameChildProperties()
        {
            this.SetUserNameCommonChildProperties();
            if (this.UserNameTemplate == null)
            {
                this.SetUserNameDefaultChildProperties();
            }
        }

        private void SetUserNameCommonChildProperties()
        {
            Util.CopyBaseAttributesToInnerControl(this, this._userNameContainer);
            this._userNameContainer.ApplyStyle(base.ControlStyle);
        }

        private void SetUserNameDefaultChildProperties()
        {
            UserNameContainer container = this._userNameContainer;
            if (this.UserNameTemplate == null)
            {
                this._userNameContainer.BorderTable.CellPadding = this.BorderPadding;
                this._userNameContainer.BorderTable.CellSpacing = 0;
                Literal title = container.Title;
                string userNameTitleText = this.UserNameTitleText;
                if (userNameTitleText.Length > 0)
                {
                    title.Text = userNameTitleText;
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
                Literal instruction = container.Instruction;
                string userNameInstructionText = this.UserNameInstructionText;
                if (userNameInstructionText.Length > 0)
                {
                    instruction.Text = userNameInstructionText;
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
                Literal userNameLabel = container.UserNameLabel;
                string userNameLabelText = this.UserNameLabelText;
                if (userNameLabelText.Length > 0)
                {
                    userNameLabel.Text = userNameLabelText;
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
                WebControl userNameTextBox = (WebControl) container.UserNameTextBox;
                if (this._textBoxStyle != null)
                {
                    userNameTextBox.ApplyStyle(this.TextBoxStyle);
                }
                userNameTextBox.TabIndex = this.TabIndex;
                userNameTextBox.AccessKey = this.AccessKey;
                bool flag = this.CurrentView == View.UserName;
                RequiredFieldValidator userNameRequired = container.UserNameRequired;
                userNameRequired.ErrorMessage = this.UserNameRequiredErrorMessage;
                userNameRequired.ToolTip = this.UserNameRequiredErrorMessage;
                userNameRequired.Enabled = flag;
                userNameRequired.Visible = flag;
                if (this._validatorTextStyle != null)
                {
                    userNameRequired.ApplyStyle(this._validatorTextStyle);
                }
                LinkButton linkButton = container.LinkButton;
                ImageButton imageButton = container.ImageButton;
                Button pushButton = container.PushButton;
                WebControl control2 = null;
                switch (this.SubmitButtonType)
                {
                    case ButtonType.Button:
                        pushButton.Text = this.SubmitButtonText;
                        control2 = pushButton;
                        break;

                    case ButtonType.Image:
                        imageButton.ImageUrl = this.SubmitButtonImageUrl;
                        imageButton.AlternateText = this.SubmitButtonText;
                        control2 = imageButton;
                        break;

                    case ButtonType.Link:
                        linkButton.Text = this.SubmitButtonText;
                        control2 = linkButton;
                        break;
                }
                linkButton.Visible = false;
                imageButton.Visible = false;
                pushButton.Visible = false;
                control2.Visible = true;
                control2.TabIndex = this.TabIndex;
                if (this._submitButtonStyle != null)
                {
                    control2.ApplyStyle(this.SubmitButtonStyle);
                }
                HyperLink helpPageLink = container.HelpPageLink;
                string helpPageText = this.HelpPageText;
                Image helpPageIcon = container.HelpPageIcon;
                if (helpPageText.Length > 0)
                {
                    helpPageLink.Text = helpPageText;
                    helpPageLink.NavigateUrl = this.HelpPageUrl;
                    helpPageLink.Visible = true;
                    helpPageLink.TabIndex = this.TabIndex;
                }
                else
                {
                    helpPageLink.Visible = false;
                }
                string helpPageIconUrl = this.HelpPageIconUrl;
                bool flag2 = helpPageIconUrl.Length > 0;
                helpPageIcon.Visible = flag2;
                if (flag2)
                {
                    helpPageIcon.ImageUrl = helpPageIconUrl;
                    helpPageIcon.AlternateText = helpPageText;
                }
                if (helpPageLink.Visible || helpPageIcon.Visible)
                {
                    if (this._hyperLinkStyle != null)
                    {
                        Style style = new TableItemStyle();
                        style.CopyFrom(this.HyperLinkStyle);
                        style.Font.Reset();
                        LoginUtil.SetTableCellStyle(helpPageLink, style);
                        helpPageLink.Font.CopyFrom(this.HyperLinkStyle.Font);
                        helpPageLink.ForeColor = this.HyperLinkStyle.ForeColor;
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
        }

        private void SetUserNameEditableChildProperties()
        {
            string userNameInternal = this.UserNameInternal;
            if (userNameInternal.Length > 0)
            {
                ITextControl userNameTextBox = (ITextControl) this._userNameContainer.UserNameTextBox;
                if (userNameTextBox != null)
                {
                    userNameTextBox.Text = userNameInternal;
                }
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._submitButtonStyle != null)
            {
                ((IStateManager) this._submitButtonStyle).TrackViewState();
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
            if (this._failureTextStyle != null)
            {
                ((IStateManager) this._failureTextStyle).TrackViewState();
            }
            if (this._successTextStyle != null)
            {
                ((IStateManager) this._successTextStyle).TrackViewState();
            }
            if (this._mailDefinition != null)
            {
                ((IStateManager) this._mailDefinition).TrackViewState();
            }
            if (this._validatorTextStyle != null)
            {
                ((IStateManager) this._validatorTextStyle).TrackViewState();
            }
        }

        private void UpdateValidators()
        {
            if ((this.UserNameTemplate == null) && (this._userNameContainer != null))
            {
                bool flag = this.CurrentView == View.UserName;
                this._userNameContainer.UserNameRequired.Enabled = flag;
                this._userNameContainer.UserNameRequired.Visible = flag;
            }
            if ((this.QuestionTemplate == null) && (this._questionContainer != null))
            {
                bool flag2 = this.CurrentView == View.Question;
                this._questionContainer.AnswerRequired.Enabled = flag2;
                this._questionContainer.AnswerRequired.Visible = flag2;
            }
        }

        private void UserNameTextChanged(object source, EventArgs e)
        {
            this.UserName = ((ITextControl) source).Text;
        }

        [Themeable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Filterable(false), Browsable(false)]
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
        }

        private string AnswerInternal
        {
            get
            {
                string answer = this.Answer;
                if (string.IsNullOrEmpty(answer) && (this._questionContainer != null))
                {
                    ITextControl answerTextBox = (ITextControl) this._questionContainer.AnswerTextBox;
                    if ((answerTextBox != null) && (answerTextBox.Text != null))
                    {
                        return answerTextBox.Text;
                    }
                }
                return answer;
            }
        }

        [WebSysDefaultValue("PasswordRecovery_DefaultAnswerLabelText"), WebCategory("Appearance"), Localizable(true), WebSysDescription("PasswordRecovery_AnswerLabelText")]
        public virtual string AnswerLabelText
        {
            get
            {
                object obj2 = this.ViewState["AnswerLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultAnswerLabelText");
            }
            set
            {
                this.ViewState["AnswerLabelText"] = value;
            }
        }

        [Localizable(true), WebCategory("Validation"), WebSysDefaultValue("PasswordRecovery_DefaultAnswerRequiredErrorMessage"), WebSysDescription("LoginControls_AnswerRequiredErrorMessage")]
        public virtual string AnswerRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["AnswerRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultAnswerRequiredErrorMessage");
            }
            set
            {
                this.ViewState["AnswerRequiredErrorMessage"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("Login_BorderPadding"), DefaultValue(1)]
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
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("PasswordRecovery_InvalidBorderPadding"));
                }
                this.ViewState["BorderPadding"] = value;
            }
        }

        private bool ConvertingToTemplate
        {
            get
            {
                return (base.DesignMode && this._convertingToTemplate);
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
                if ((value < View.UserName) || (value > View.Success))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != this.CurrentView)
                {
                    this._currentView = value;
                    this.UpdateValidators();
                }
            }
        }

        [NotifyParentProperty(true), WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_FailureTextStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null)]
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

        [WebSysDescription("PasswordRecovery_GeneralFailureText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("PasswordRecovery_DefaultGeneralFailureText")]
        public virtual string GeneralFailureText
        {
            get
            {
                object obj2 = this.ViewState["GeneralFailureText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultGeneralFailureText");
            }
            set
            {
                this.ViewState["GeneralFailureText"] = value;
            }
        }

        [WebCategory("Links"), WebSysDescription("LoginControls_HelpPageIconUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, DefaultValue("")]
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

        [DefaultValue(""), WebSysDescription("ChangePassword_HelpPageText"), WebCategory("Links"), Localizable(true)]
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

        [DefaultValue(""), UrlProperty, WebCategory("Links"), WebSysDescription("LoginControls_HelpPageUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
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

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_HyperLinkStyle"), WebCategory("Styles")]
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

        [NotifyParentProperty(true), WebSysDescription("WebControl_InstructionTextStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("LoginControls_LabelStyle"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [Themeable(false), WebSysDescription("PasswordRecovery_MailDefinition"), WebCategory("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebCategory("Data"), WebSysDescription("MembershipProvider_Name"), Themeable(false), DefaultValue("")]
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

        [Browsable(false), Filterable(false), Themeable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Question
        {
            virtual get
            {
                if (this._question == null)
                {
                    return string.Empty;
                }
                return this._question;
            }
            private set
            {
                this._question = value;
            }
        }

        [WebCategory("Appearance"), Localizable(true), WebSysDescription("PasswordRecovery_QuestionFailureText"), WebSysDefaultValue("PasswordRecovery_DefaultQuestionFailureText")]
        public virtual string QuestionFailureText
        {
            get
            {
                object obj2 = this.ViewState["QuestionFailureText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultQuestionFailureText");
            }
            set
            {
                this.ViewState["QuestionFailureText"] = value;
            }
        }

        [Localizable(true), WebSysDescription("PasswordRecovery_QuestionInstructionText"), WebCategory("Appearance"), WebSysDefaultValue("PasswordRecovery_DefaultQuestionInstructionText")]
        public virtual string QuestionInstructionText
        {
            get
            {
                object obj2 = this.ViewState["QuestionInstructionText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultQuestionInstructionText");
            }
            set
            {
                this.ViewState["QuestionInstructionText"] = value;
            }
        }

        [Localizable(true), WebSysDefaultValue("PasswordRecovery_DefaultQuestionLabelText"), WebSysDescription("PasswordRecovery_QuestionLabelText"), WebCategory("Appearance")]
        public virtual string QuestionLabelText
        {
            get
            {
                object obj2 = this.ViewState["QuestionLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultQuestionLabelText");
            }
            set
            {
                this.ViewState["QuestionLabelText"] = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("PasswordRecovery_QuestionTemplate"), Browsable(false), TemplateContainer(typeof(PasswordRecovery))]
        public virtual ITemplate QuestionTemplate
        {
            get
            {
                return this._questionTemplate;
            }
            set
            {
                this._questionTemplate = value;
                base.ChildControlsCreated = false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("PasswordRecovery_QuestionTemplateContainer"), Browsable(false)]
        public Control QuestionTemplateContainer
        {
            get
            {
                this.EnsureChildControls();
                return this._questionContainer;
            }
        }

        [WebSysDescription("PasswordRecovery_QuestionTitleText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("PasswordRecovery_DefaultQuestionTitleText")]
        public virtual string QuestionTitleText
        {
            get
            {
                object obj2 = this.ViewState["QuestionTitleText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultQuestionTitleText");
            }
            set
            {
                this.ViewState["QuestionTitleText"] = value;
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

        [UrlProperty, Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), WebSysDescription("ChangePassword_ChangePasswordButtonImageUrl"), WebCategory("Appearance")]
        public virtual string SubmitButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["SubmitButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["SubmitButtonImageUrl"] = value;
            }
        }

        [WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("PasswordRecovery_SubmitButtonStyle"), NotifyParentProperty(true), DefaultValue((string) null)]
        public Style SubmitButtonStyle
        {
            get
            {
                if (this._submitButtonStyle == null)
                {
                    this._submitButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._submitButtonStyle).TrackViewState();
                    }
                }
                return this._submitButtonStyle;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("ChangePassword_ChangePasswordButtonText"), Localizable(true), WebSysDefaultValue("PasswordRecovery_DefaultSubmitButtonText")]
        public virtual string SubmitButtonText
        {
            get
            {
                object obj2 = this.ViewState["SubmitButtonText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultSubmitButtonText");
            }
            set
            {
                this.ViewState["SubmitButtonText"] = value;
            }
        }

        [WebCategory("Appearance"), DefaultValue(0), WebSysDescription("PasswordRecovery_SubmitButtonType")]
        public virtual ButtonType SubmitButtonType
        {
            get
            {
                object obj2 = this.ViewState["SubmitButtonType"];
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
                this.ViewState["SubmitButtonType"] = value;
            }
        }

        [Themeable(false), WebCategory("Behavior"), UrlProperty, WebSysDescription("LoginControls_SuccessPageUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue("")]
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

        [TemplateContainer(typeof(PasswordRecovery)), Browsable(false), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("PasswordRecovery_SuccessTemplate")]
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

        [WebSysDescription("PasswordRecovery_SuccessTemplateContainer"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control SuccessTemplateContainer
        {
            get
            {
                this.EnsureChildControls();
                return this._successContainer;
            }
        }

        [Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("PasswordRecovery_DefaultSuccessText"), WebSysDescription("PasswordRecovery_SuccessText")]
        public virtual string SuccessText
        {
            get
            {
                object obj2 = this.ViewState["SuccessText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultSuccessText");
            }
            set
            {
                this.ViewState["SuccessText"] = value;
            }
        }

        [WebCategory("Styles"), NotifyParentProperty(true), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("PasswordRecovery_SuccessTextStyle"), PersistenceMode(PersistenceMode.InnerProperty)]
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

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        [NotifyParentProperty(true), WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("LoginControls_TextBoxStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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

        [WebCategory("Layout"), DefaultValue(0), WebSysDescription("LoginControls_TextLayout")]
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

        [WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("LoginControls_TitleTextStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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

        [DefaultValue(""), WebSysDescription("UserName_InitialValue"), Localizable(true), WebCategory("Appearance")]
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

        [WebSysDescription("PasswordRecovery_UserNameFailureText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("PasswordRecovery_DefaultUserNameFailureText")]
        public virtual string UserNameFailureText
        {
            get
            {
                object obj2 = this.ViewState["UserNameFailureText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultUserNameFailureText");
            }
            set
            {
                this.ViewState["UserNameFailureText"] = value;
            }
        }

        [WebSysDefaultValue("PasswordRecovery_DefaultUserNameInstructionText"), WebSysDescription("PasswordRecovery_UserNameInstructionText"), Localizable(true), WebCategory("Appearance")]
        public virtual string UserNameInstructionText
        {
            get
            {
                object obj2 = this.ViewState["UserNameInstructionText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultUserNameInstructionText");
            }
            set
            {
                this.ViewState["UserNameInstructionText"] = value;
            }
        }

        internal string UserNameInternal
        {
            get
            {
                string userName = this.UserName;
                if (string.IsNullOrEmpty(userName) && (this._userNameContainer != null))
                {
                    ITextControl userNameTextBox = this._userNameContainer.UserNameTextBox as ITextControl;
                    if (userNameTextBox != null)
                    {
                        return userNameTextBox.Text;
                    }
                }
                return userName;
            }
        }

        [WebSysDefaultValue("PasswordRecovery_DefaultUserNameLabelText"), WebSysDescription("PasswordRecovery_UserNameLabelText"), Localizable(true), WebCategory("Appearance")]
        public virtual string UserNameLabelText
        {
            get
            {
                object obj2 = this.ViewState["UserNameLabelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultUserNameLabelText");
            }
            set
            {
                this.ViewState["UserNameLabelText"] = value;
            }
        }

        [Localizable(true), WebSysDescription("ChangePassword_UserNameRequiredErrorMessage"), WebSysDefaultValue("PasswordRecovery_DefaultUserNameRequiredErrorMessage"), WebCategory("Validation")]
        public virtual string UserNameRequiredErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["UserNameRequiredErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultUserNameRequiredErrorMessage");
            }
            set
            {
                this.ViewState["UserNameRequiredErrorMessage"] = value;
            }
        }

        [WebSysDescription("PasswordRecovery_UserNameTemplate"), TemplateContainer(typeof(PasswordRecovery)), Browsable(false), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate UserNameTemplate
        {
            get
            {
                return this._userNameTemplate;
            }
            set
            {
                this._userNameTemplate = value;
                base.ChildControlsCreated = false;
            }
        }

        [WebSysDescription("PasswordRecovery_UserNameTemplateContainer"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control UserNameTemplateContainer
        {
            get
            {
                this.EnsureChildControls();
                return this._userNameContainer;
            }
        }

        [WebCategory("Appearance"), Localizable(true), WebSysDefaultValue("PasswordRecovery_DefaultUserNameTitleText"), WebSysDescription("PasswordRecovery_UserNameTitleText")]
        public virtual string UserNameTitleText
        {
            get
            {
                object obj2 = this.ViewState["UserNameTitleText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("PasswordRecovery_DefaultUserNameTitleText");
            }
            set
            {
                this.ViewState["UserNameTitleText"] = value;
            }
        }

        [WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("ChangePassword_ValidatorTextStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
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

        private sealed class DefaultQuestionTemplate : ITemplate
        {
            private PasswordRecovery _owner;

            public DefaultQuestionTemplate(PasswordRecovery owner)
            {
                this._owner = owner;
            }

            private void CreateControls(PasswordRecovery.QuestionContainer questionContainer)
            {
                string uniqueID = this._owner.UniqueID;
                questionContainer.Title = new Literal();
                questionContainer.Instruction = new Literal();
                questionContainer.UserNameLabel = new Literal();
                questionContainer.UserName = new Literal();
                questionContainer.QuestionLabel = new Literal();
                questionContainer.Question = new Literal();
                questionContainer.UserName.ID = "UserName";
                questionContainer.Question.ID = "Question";
                TextBox forControl = new TextBox {
                    ID = "Answer"
                };
                questionContainer.AnswerTextBox = forControl;
                questionContainer.AnswerLabel = new LabelLiteral(forControl);
                bool flag = this._owner.CurrentView == PasswordRecovery.View.Question;
                RequiredFieldValidator validator = new RequiredFieldValidator {
                    ID = "AnswerRequired",
                    ValidationGroup = uniqueID,
                    ControlToValidate = forControl.ID,
                    Display = ValidatorDisplay.Static,
                    Text = System.Web.SR.GetString("LoginControls_DefaultRequiredFieldValidatorText"),
                    Enabled = flag,
                    Visible = flag
                };
                questionContainer.AnswerRequired = validator;
                LinkButton button = new LinkButton {
                    ID = "SubmitLinkButton",
                    ValidationGroup = uniqueID,
                    CommandName = PasswordRecovery.SubmitButtonCommandName
                };
                questionContainer.LinkButton = button;
                ImageButton button2 = new ImageButton {
                    ID = "SubmitImageButton",
                    ValidationGroup = uniqueID,
                    CommandName = PasswordRecovery.SubmitButtonCommandName
                };
                questionContainer.ImageButton = button2;
                Button button3 = new Button {
                    ID = "SubmitButton",
                    ValidationGroup = uniqueID,
                    CommandName = PasswordRecovery.SubmitButtonCommandName
                };
                questionContainer.PushButton = button3;
                questionContainer.HelpPageLink = new HyperLink();
                questionContainer.HelpPageLink.ID = "HelpLink";
                questionContainer.HelpPageIcon = new Image();
                Literal literal = new Literal {
                    ID = "FailureText"
                };
                questionContainer.FailureTextLabel = literal;
            }

            private void LayoutControls(PasswordRecovery.QuestionContainer questionContainer)
            {
                if (this._owner.TextLayout == LoginTextLayout.TextOnLeft)
                {
                    this.LayoutTextOnLeft(questionContainer);
                }
                else
                {
                    this.LayoutTextOnTop(questionContainer);
                }
            }

            private void LayoutTextOnLeft(PasswordRecovery.QuestionContainer questionContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(questionContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(questionContainer.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(questionContainer.UserNameLabel);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(questionContainer.UserName);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(questionContainer.QuestionLabel);
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(questionContainer.Question);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(questionContainer.AnswerLabel);
                if (this._owner.ConvertingToTemplate)
                {
                    questionContainer.AnswerLabel.RenderAsLabel = true;
                }
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(questionContainer.AnswerTextBox);
                cell.Controls.Add(questionContainer.AnswerRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(questionContainer.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(questionContainer.LinkButton);
                cell.Controls.Add(questionContainer.ImageButton);
                cell.Controls.Add(questionContainer.PushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2
                };
                cell.Controls.Add(questionContainer.HelpPageIcon);
                cell.Controls.Add(questionContainer.HelpPageLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                questionContainer.LayoutTable = child;
                questionContainer.BorderTable = table2;
                questionContainer.Controls.Add(table2);
            }

            private void LayoutTextOnTop(PasswordRecovery.QuestionContainer questionContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(questionContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(questionContainer.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(questionContainer.UserNameLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(questionContainer.UserName);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(questionContainer.QuestionLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(questionContainer.Question);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(questionContainer.AnswerLabel);
                if (this._owner.ConvertingToTemplate)
                {
                    questionContainer.AnswerLabel.RenderAsLabel = true;
                }
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(questionContainer.AnswerTextBox);
                cell.Controls.Add(questionContainer.AnswerRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(questionContainer.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(questionContainer.LinkButton);
                cell.Controls.Add(questionContainer.ImageButton);
                cell.Controls.Add(questionContainer.PushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(questionContainer.HelpPageIcon);
                cell.Controls.Add(questionContainer.HelpPageLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                questionContainer.LayoutTable = child;
                questionContainer.BorderTable = table2;
                questionContainer.Controls.Add(table2);
            }

            void ITemplate.InstantiateIn(Control container)
            {
                PasswordRecovery.QuestionContainer questionContainer = (PasswordRecovery.QuestionContainer) container;
                this.CreateControls(questionContainer);
                this.LayoutControls(questionContainer);
            }
        }

        private sealed class DefaultSuccessTemplate : ITemplate
        {
            private PasswordRecovery _owner;

            public DefaultSuccessTemplate(PasswordRecovery owner)
            {
                this._owner = owner;
            }

            private void CreateControls(PasswordRecovery.SuccessContainer successContainer)
            {
                successContainer.SuccessTextLabel = new Literal();
            }

            private void LayoutControls(PasswordRecovery.SuccessContainer successContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell();
                cell.Controls.Add(successContainer.SuccessTextLabel);
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
                PasswordRecovery.SuccessContainer successContainer = (PasswordRecovery.SuccessContainer) container;
                this.CreateControls(successContainer);
                this.LayoutControls(successContainer);
            }
        }

        private sealed class DefaultUserNameTemplate : ITemplate
        {
            private PasswordRecovery _owner;

            public DefaultUserNameTemplate(PasswordRecovery owner)
            {
                this._owner = owner;
            }

            private void CreateControls(PasswordRecovery.UserNameContainer userNameContainer)
            {
                string uniqueID = this._owner.UniqueID;
                userNameContainer.Title = new Literal();
                userNameContainer.Instruction = new Literal();
                TextBox forControl = new TextBox {
                    ID = "UserName"
                };
                userNameContainer.UserNameTextBox = forControl;
                userNameContainer.UserNameLabel = new LabelLiteral(forControl);
                bool flag = this._owner.CurrentView == PasswordRecovery.View.UserName;
                RequiredFieldValidator validator = new RequiredFieldValidator {
                    ID = "UserNameRequired",
                    ValidationGroup = uniqueID,
                    ControlToValidate = forControl.ID,
                    Display = ValidatorDisplay.Static,
                    Text = System.Web.SR.GetString("LoginControls_DefaultRequiredFieldValidatorText"),
                    Enabled = flag,
                    Visible = flag
                };
                userNameContainer.UserNameRequired = validator;
                LinkButton button = new LinkButton {
                    ID = "SubmitLinkButton",
                    ValidationGroup = uniqueID,
                    CommandName = PasswordRecovery.SubmitButtonCommandName
                };
                userNameContainer.LinkButton = button;
                ImageButton button2 = new ImageButton {
                    ID = "SubmitImageButton",
                    ValidationGroup = uniqueID,
                    CommandName = PasswordRecovery.SubmitButtonCommandName
                };
                userNameContainer.ImageButton = button2;
                Button button3 = new Button {
                    ID = "SubmitButton",
                    ValidationGroup = uniqueID,
                    CommandName = PasswordRecovery.SubmitButtonCommandName
                };
                userNameContainer.PushButton = button3;
                userNameContainer.HelpPageLink = new HyperLink();
                userNameContainer.HelpPageLink.ID = "HelpLink";
                userNameContainer.HelpPageIcon = new Image();
                Literal literal = new Literal {
                    ID = "FailureText"
                };
                userNameContainer.FailureTextLabel = literal;
            }

            private void LayoutControls(PasswordRecovery.UserNameContainer userNameContainer)
            {
                if (this._owner.TextLayout == LoginTextLayout.TextOnLeft)
                {
                    this.LayoutTextOnLeft(userNameContainer);
                }
                else
                {
                    this.LayoutTextOnTop(userNameContainer);
                }
            }

            private void LayoutTextOnLeft(PasswordRecovery.UserNameContainer userNameContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(userNameContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(userNameContainer.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(userNameContainer.UserNameLabel);
                if (this._owner.ConvertingToTemplate)
                {
                    userNameContainer.UserNameLabel.RenderAsLabel = true;
                }
                row.Cells.Add(cell);
                cell = new TableCell();
                cell.Controls.Add(userNameContainer.UserNameTextBox);
                cell.Controls.Add(userNameContainer.UserNameRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(userNameContainer.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2,
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(userNameContainer.LinkButton);
                cell.Controls.Add(userNameContainer.ImageButton);
                cell.Controls.Add(userNameContainer.PushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    ColumnSpan = 2
                };
                cell.Controls.Add(userNameContainer.HelpPageIcon);
                cell.Controls.Add(userNameContainer.HelpPageLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                userNameContainer.LayoutTable = child;
                userNameContainer.BorderTable = table2;
                userNameContainer.Controls.Add(table2);
            }

            private void LayoutTextOnTop(PasswordRecovery.UserNameContainer userNameContainer)
            {
                Table child = new Table {
                    CellPadding = 0
                };
                TableRow row = new LoginUtil.DisappearingTableRow();
                TableCell cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(userNameContainer.Title);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(userNameContainer.Instruction);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(userNameContainer.UserNameLabel);
                if (this._owner.ConvertingToTemplate)
                {
                    userNameContainer.UserNameLabel.RenderAsLabel = true;
                }
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(userNameContainer.UserNameTextBox);
                cell.Controls.Add(userNameContainer.UserNameRequired);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Center
                };
                cell.Controls.Add(userNameContainer.FailureTextLabel);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                cell.Controls.Add(userNameContainer.LinkButton);
                cell.Controls.Add(userNameContainer.ImageButton);
                cell.Controls.Add(userNameContainer.PushButton);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                row = new LoginUtil.DisappearingTableRow();
                cell = new TableCell();
                cell.Controls.Add(userNameContainer.HelpPageIcon);
                cell.Controls.Add(userNameContainer.HelpPageLink);
                row.Cells.Add(cell);
                child.Rows.Add(row);
                Table table2 = LoginUtil.CreateChildTable(this._owner.ConvertingToTemplate);
                row = new TableRow();
                cell = new TableCell();
                cell.Controls.Add(child);
                row.Cells.Add(cell);
                table2.Rows.Add(row);
                userNameContainer.LayoutTable = child;
                userNameContainer.BorderTable = table2;
                userNameContainer.Controls.Add(table2);
            }

            void ITemplate.InstantiateIn(Control container)
            {
                PasswordRecovery.UserNameContainer userNameContainer = (PasswordRecovery.UserNameContainer) container;
                this.CreateControls(userNameContainer);
                this.LayoutControls(userNameContainer);
            }
        }

        internal sealed class QuestionContainer : LoginUtil.GenericContainer<PasswordRecovery>, INonBindingContainer, INamingContainer
        {
            private LabelLiteral _answerLabel;
            private RequiredFieldValidator _answerRequired;
            private Control _answerTextBox;
            private Control _failureTextLabel;
            private Image _helpPageIcon;
            private HyperLink _helpPageLink;
            private System.Web.UI.WebControls.ImageButton _imageButton;
            private Literal _instruction;
            private System.Web.UI.WebControls.LinkButton _linkButton;
            private Button _pushButton;
            private Control _question;
            private Literal _questionLabel;
            private Literal _title;
            private Control _userName;
            private Literal _userNameLabel;

            public QuestionContainer(PasswordRecovery owner) : base(owner)
            {
            }

            public LabelLiteral AnswerLabel
            {
                get
                {
                    return this._answerLabel;
                }
                set
                {
                    this._answerLabel = value;
                }
            }

            public RequiredFieldValidator AnswerRequired
            {
                get
                {
                    return this._answerRequired;
                }
                set
                {
                    this._answerRequired = value;
                }
            }

            public Control AnswerTextBox
            {
                get
                {
                    if (this._answerTextBox != null)
                    {
                        return this._answerTextBox;
                    }
                    return base.FindRequiredControl<IEditableTextControl>("Answer", "PasswordRecovery_NoAnswerTextBox");
                }
                set
                {
                    this._answerTextBox = value;
                }
            }

            protected override bool ConvertingToTemplate
            {
                get
                {
                    return base.Owner.ConvertingToTemplate;
                }
            }

            public Control FailureTextLabel
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

            public Image HelpPageIcon
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

            public HyperLink HelpPageLink
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

            public System.Web.UI.WebControls.ImageButton ImageButton
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

            public Literal Instruction
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

            public System.Web.UI.WebControls.LinkButton LinkButton
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

            public Button PushButton
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

            public Control Question
            {
                get
                {
                    if (this._question != null)
                    {
                        return this._question;
                    }
                    return base.FindOptionalControl<ITextControl>("Question");
                }
                set
                {
                    this._question = value;
                }
            }

            public Literal QuestionLabel
            {
                get
                {
                    return this._questionLabel;
                }
                set
                {
                    this._questionLabel = value;
                }
            }

            public Literal Title
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

            public Control UserName
            {
                get
                {
                    if (this._userName != null)
                    {
                        return this._userName;
                    }
                    return base.FindOptionalControl<ITextControl>("UserName");
                }
                set
                {
                    this._userName = value;
                }
            }

            public Literal UserNameLabel
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
        }

        internal sealed class SuccessContainer : LoginUtil.GenericContainer<PasswordRecovery>, INonBindingContainer, INamingContainer
        {
            private Literal _successTextLabel;

            public SuccessContainer(PasswordRecovery owner) : base(owner)
            {
            }

            protected override bool ConvertingToTemplate
            {
                get
                {
                    return base.Owner.ConvertingToTemplate;
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
        }

        internal sealed class UserNameContainer : LoginUtil.GenericContainer<PasswordRecovery>, INonBindingContainer, INamingContainer
        {
            private Control _failureTextLabel;
            private Image _helpPageIcon;
            private HyperLink _helpPageLink;
            private System.Web.UI.WebControls.ImageButton _imageButton;
            private Literal _instruction;
            private System.Web.UI.WebControls.LinkButton _linkButton;
            private Button _pushButton;
            private Literal _title;
            private LabelLiteral _userNameLabel;
            private RequiredFieldValidator _userNameRequired;
            private Control _userNameTextBox;

            public UserNameContainer(PasswordRecovery owner) : base(owner)
            {
            }

            protected override bool ConvertingToTemplate
            {
                get
                {
                    return base.Owner.ConvertingToTemplate;
                }
            }

            public Control FailureTextLabel
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

            public Image HelpPageIcon
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

            public HyperLink HelpPageLink
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

            public System.Web.UI.WebControls.ImageButton ImageButton
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

            public Literal Instruction
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

            public System.Web.UI.WebControls.LinkButton LinkButton
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

            public Button PushButton
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

            public Literal Title
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

            public LabelLiteral UserNameLabel
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

            public RequiredFieldValidator UserNameRequired
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

            public Control UserNameTextBox
            {
                get
                {
                    if (this._userNameTextBox != null)
                    {
                        return this._userNameTextBox;
                    }
                    return base.FindRequiredControl<IEditableTextControl>("UserName", "PasswordRecovery_NoUserNameTextBox");
                }
                set
                {
                    this._userNameTextBox = value;
                }
            }
        }

        internal enum View
        {
            UserName,
            Question,
            Success
        }
    }
}

