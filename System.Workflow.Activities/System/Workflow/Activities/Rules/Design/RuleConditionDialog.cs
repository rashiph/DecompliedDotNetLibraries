namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    public class RuleConditionDialog : System.Windows.Forms.Form
    {
        private Button cancelButton;
        private IContainer components;
        private ErrorProvider conditionErrorProvider;
        private Label conditionLabel;
        private IntellisenseTextBox conditionTextBox;
        private Label headerLabel;
        private PictureBox headerPictureBox;
        private Button okButton;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private RuleExpressionCondition ruleExpressionCondition;
        private Parser ruleParser;
        private IServiceProvider serviceProvider;
        private Exception syntaxException;
        private bool wasOKed;

        public RuleConditionDialog(Activity activity, CodeExpression expression)
        {
            ITypeProvider provider;
            this.ruleExpressionCondition = new RuleExpressionCondition();
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            this.InitializeComponent();
            this.serviceProvider = activity.Site;
            if (this.serviceProvider != null)
            {
                IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                if (service != null)
                {
                    this.Font = (Font) service.Styles["DialogFont"];
                }
                provider = (ITypeProvider) this.serviceProvider.GetService(typeof(ITypeProvider));
                if (provider == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingService, new object[] { typeof(ITypeProvider).FullName }));
                }
                WorkflowDesignerLoader loader = this.serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                if (loader != null)
                {
                    loader.Flush();
                }
            }
            else
            {
                TypeProvider provider2 = new TypeProvider(null);
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    provider2.AddAssembly(assembly);
                }
                provider = provider2;
            }
            RuleValidation validation = new RuleValidation(activity, provider, false);
            this.ruleParser = new Parser(validation);
            this.InitializeDialog(expression);
        }

        public RuleConditionDialog(System.Type activityType, ITypeProvider typeProvider, CodeExpression expression)
        {
            this.ruleExpressionCondition = new RuleExpressionCondition();
            if (activityType == null)
            {
                throw new ArgumentNullException("activityType");
            }
            this.InitializeComponent();
            RuleValidation validation = new RuleValidation(activityType, typeProvider);
            this.ruleParser = new Parser(validation);
            this.InitializeDialog(expression);
        }

        private void ConditionTextBox_PopulateAutoCompleteList(object sender, AutoCompletionEventArgs e)
        {
            e.AutoCompleteValues = this.ruleParser.GetExpressionCompletions(e.Prefix);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void conditionTextBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                this.ruleExpressionCondition = this.ruleParser.ParseCondition(this.conditionTextBox.Text);
                if (!string.IsNullOrEmpty(this.conditionTextBox.Text))
                {
                    this.conditionTextBox.Text = this.ruleExpressionCondition.ToString().Replace("\n", "\r\n");
                }
                this.conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
                this.syntaxException = null;
            }
            catch (Exception exception)
            {
                this.syntaxException = exception;
                this.conditionErrorProvider.SetError(this.conditionTextBox, exception.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(RuleConditionDialog));
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.headerLabel = new Label();
            this.headerPictureBox = new PictureBox();
            this.conditionTextBox = new IntellisenseTextBox();
            this.conditionLabel = new Label();
            this.conditionErrorProvider = new ErrorProvider(this.components);
            this.okCancelTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.headerPictureBox).BeginInit();
            ((ISupportInitialize) this.conditionErrorProvider).BeginInit();
            base.SuspendLayout();
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.CausesValidation = false;
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            manager.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Name = "okButton";
            this.okButton.Click += new EventHandler(this.okButton_Click);
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            manager.ApplyResources(this.headerLabel, "headerLabel");
            this.headerLabel.Name = "headerLabel";
            manager.ApplyResources(this.headerPictureBox, "headerPictureBox");
            this.headerPictureBox.Name = "headerPictureBox";
            this.headerPictureBox.TabStop = false;
            this.conditionTextBox.AcceptsReturn = true;
            manager.ApplyResources(this.conditionTextBox, "conditionTextBox");
            this.conditionTextBox.Name = "conditionTextBox";
            this.conditionTextBox.Validating += new CancelEventHandler(this.conditionTextBox_Validating);
            manager.ApplyResources(this.conditionLabel, "conditionLabel");
            this.conditionLabel.Name = "conditionLabel";
            this.conditionErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            this.conditionErrorProvider.ContainerControl = this;
            base.AcceptButton = this.okButton;
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.cancelButton;
            base.Controls.Add(this.conditionLabel);
            base.Controls.Add(this.conditionTextBox);
            base.Controls.Add(this.okCancelTableLayoutPanel);
            base.Controls.Add(this.headerLabel);
            base.Controls.Add(this.headerPictureBox);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "RuleConditionDialog";
            base.ShowInTaskbar = false;
            base.FormClosing += new FormClosingEventHandler(this.RuleConditionDialog_FormClosing);
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.headerPictureBox).EndInit();
            ((ISupportInitialize) this.conditionErrorProvider).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeDialog(CodeExpression expression)
        {
            base.HelpRequested += new HelpEventHandler(this.OnHelpRequested);
            base.HelpButtonClicked += new CancelEventHandler(this.OnHelpClicked);
            if (expression != null)
            {
                this.ruleExpressionCondition.Expression = RuleExpressionWalker.Clone(expression);
                this.conditionTextBox.Text = this.ruleExpressionCondition.ToString().Replace("\n", "\r\n");
            }
            else
            {
                this.conditionTextBox.Text = string.Empty;
            }
            this.conditionTextBox.PopulateAutoCompleteList += new EventHandler<AutoCompletionEventArgs>(this.ConditionTextBox_PopulateAutoCompleteList);
            this.conditionTextBox.PopulateToolTipList += new EventHandler<AutoCompletionEventArgs>(this.ConditionTextBox_PopulateAutoCompleteList);
            try
            {
                this.ruleParser.ParseCondition(this.conditionTextBox.Text);
                this.conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
            }
            catch (RuleSyntaxException exception)
            {
                this.conditionErrorProvider.SetError(this.conditionTextBox, exception.Message);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.wasOKed = true;
        }

        private void OnHelpClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.ShowHelp();
        }

        private void OnHelpRequested(object sender, HelpEventArgs e)
        {
            this.ShowHelp();
        }

        private void RuleConditionDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.wasOKed && (this.syntaxException != null))
            {
                e.Cancel = true;
                System.Workflow.Activities.Rules.Design.DesignerHelpers.DisplayError(Messages.Error_ConditionParser + "\n" + this.syntaxException.Message, this.Text, this.serviceProvider);
                if (this.syntaxException is RuleSyntaxException)
                {
                    this.conditionTextBox.SelectionStart = ((RuleSyntaxException) this.syntaxException).Position;
                }
                this.conditionTextBox.SelectionLength = 0;
                this.conditionTextBox.ScrollToCaret();
                this.wasOKed = false;
            }
        }

        private void ShowHelp()
        {
            if (this.serviceProvider != null)
            {
                IHelpService service = this.serviceProvider.GetService(typeof(IHelpService)) as IHelpService;
                if (service != null)
                {
                    service.ShowHelpFromKeyword(base.GetType().FullName + ".UI");
                }
                else
                {
                    IUIService service2 = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                    if (service2 != null)
                    {
                        service2.ShowError(Messages.NoHelp);
                    }
                }
            }
            else
            {
                IUIService service3 = (IUIService) this.GetService(typeof(IUIService));
                if (service3 != null)
                {
                    service3.ShowError(Messages.NoHelp);
                }
            }
        }

        public CodeExpression Expression
        {
            get
            {
                return this.ruleExpressionCondition.Expression;
            }
        }
    }
}

