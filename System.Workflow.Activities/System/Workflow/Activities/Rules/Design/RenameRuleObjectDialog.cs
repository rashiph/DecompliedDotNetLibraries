namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.Activities.Rules;

    internal class RenameRuleObjectDialog : System.Windows.Forms.Form
    {
        private Button cancelButton;
        private IContainer components;
        private string name;
        private NameValidatorDelegate nameValidator;
        private Label newNamelabel;
        private Button okButton;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private BasicBrowserDialog parent;
        private TextBox ruleNameTextBox;
        private IServiceProvider serviceProvider;

        public RenameRuleObjectDialog(IServiceProvider serviceProvider, string oldName, NameValidatorDelegate nameValidator, BasicBrowserDialog parent)
        {
            if (oldName == null)
            {
                throw new ArgumentNullException("oldName");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (nameValidator == null)
            {
                throw new ArgumentNullException("nameValidator");
            }
            this.serviceProvider = serviceProvider;
            this.name = oldName;
            this.nameValidator = nameValidator;
            this.parent = parent;
            this.InitializeComponent();
            this.ruleNameTextBox.Text = oldName;
            this.Text = parent.RenameTitleText;
            this.newNamelabel.Text = parent.NewNameLabelText;
            base.Icon = null;
            IUIService service = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
            if (service != null)
            {
                this.Font = (Font) service.Styles["DialogFont"];
            }
        }

        private static MessageBoxOptions DetermineOptions(object sender)
        {
            MessageBoxOptions options = 0;
            Control parent = sender as Control;
            RightToLeft inherit = RightToLeft.Inherit;
            while ((inherit == RightToLeft.Inherit) && (parent != null))
            {
                inherit = parent.RightToLeft;
                parent = parent.Parent;
            }
            if (inherit == RightToLeft.Yes)
            {
                options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
            }
            return options;
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
            ComponentResourceManager manager = new ComponentResourceManager(typeof(RenameRuleObjectDialog));
            this.cancelButton = new Button();
            this.okButton = new Button();
            this.newNamelabel = new Label();
            this.ruleNameTextBox = new TextBox();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.okCancelTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Click += new EventHandler(this.OnCancel);
            manager.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Name = "okButton";
            this.okButton.Click += new EventHandler(this.OnOk);
            manager.ApplyResources(this.newNamelabel, "newNamelabel");
            this.newNamelabel.Name = "newNamelabel";
            this.newNamelabel.UseCompatibleTextRendering = true;
            manager.ApplyResources(this.ruleNameTextBox, "ruleNameTextBox");
            this.ruleNameTextBox.Name = "ruleNameTextBox";
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            base.AcceptButton = this.okButton;
            manager.ApplyResources(this, "$this");
            base.CancelButton = this.cancelButton;
            base.Controls.Add(this.okCancelTableLayoutPanel);
            base.Controls.Add(this.ruleNameTextBox);
            base.Controls.Add(this.newNamelabel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "RenameRuleObjectDialog";
            base.ShowInTaskbar = false;
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnOk(object sender, EventArgs e)
        {
            string text = this.ruleNameTextBox.Text;
            if (text.Trim().Length == 0)
            {
                string emptyNameErrorText = this.parent.EmptyNameErrorText;
                IUIService service = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    service.ShowError(emptyNameErrorText);
                }
                else
                {
                    MessageBox.Show(emptyNameErrorText, Messages.InvalidConditionNameCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, DetermineOptions(sender));
                }
                base.DialogResult = DialogResult.None;
            }
            else if ((this.name != text) && !this.nameValidator(text))
            {
                string duplicateNameErrorText = this.parent.DuplicateNameErrorText;
                IUIService service2 = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
                if (service2 != null)
                {
                    service2.ShowError(duplicateNameErrorText);
                }
                else
                {
                    MessageBox.Show(duplicateNameErrorText, Messages.InvalidConditionNameCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, DetermineOptions(sender));
                }
                base.DialogResult = DialogResult.None;
            }
            else
            {
                this.name = text;
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
        }

        public string RuleObjectName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        public delegate bool NameValidatorDelegate(string name);
    }
}

