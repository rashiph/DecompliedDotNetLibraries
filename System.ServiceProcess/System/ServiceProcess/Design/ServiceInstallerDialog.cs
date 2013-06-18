namespace System.ServiceProcess.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime;
    using System.ServiceProcess;
    using System.Windows.Forms;

    public class ServiceInstallerDialog : Form
    {
        private Button cancelButton;
        private TextBox confirmPassword;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button okButton;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private TextBox passwordEdit;
        private ServiceInstallerDialogResult result;
        private TextBox usernameEdit;

        public ServiceInstallerDialog()
        {
            this.InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.result = ServiceInstallerDialogResult.Canceled;
            base.DialogResult = DialogResult.Cancel;
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ServiceInstallerDialog));
            this.okButton = new Button();
            this.passwordEdit = new TextBox();
            this.cancelButton = new Button();
            this.confirmPassword = new TextBox();
            this.usernameEdit = new TextBox();
            this.label1 = new Label();
            this.label2 = new Label();
            this.label3 = new Label();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.okButton, "okButton");
            this.okButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Margin = new Padding(0, 0, 3, 0);
            this.okButton.MinimumSize = new Size(0x4b, 0x17);
            this.okButton.Name = "okButton";
            this.okButton.Padding = new Padding(10, 0, 10, 0);
            this.okButton.Click += new EventHandler(this.okButton_Click);
            manager.ApplyResources(this.passwordEdit, "passwordEdit");
            this.passwordEdit.Margin = new Padding(3, 3, 0, 3);
            this.passwordEdit.Name = "passwordEdit";
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Margin = new Padding(3, 0, 0, 0);
            this.cancelButton.MinimumSize = new Size(0x4b, 0x17);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Padding = new Padding(10, 0, 10, 0);
            this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
            manager.ApplyResources(this.confirmPassword, "confirmPassword");
            this.confirmPassword.Margin = new Padding(3, 3, 0, 3);
            this.confirmPassword.Name = "confirmPassword";
            manager.ApplyResources(this.usernameEdit, "usernameEdit");
            this.usernameEdit.Margin = new Padding(3, 0, 0, 3);
            this.usernameEdit.Name = "usernameEdit";
            manager.ApplyResources(this.label1, "label1");
            this.label1.Margin = new Padding(0, 0, 3, 3);
            this.label1.Name = "label1";
            manager.ApplyResources(this.label2, "label2");
            this.label2.Margin = new Padding(0, 3, 3, 3);
            this.label2.Name = "label2";
            manager.ApplyResources(this.label3, "label3");
            this.label3.Margin = new Padding(0, 3, 3, 3);
            this.label3.Name = "label3";
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.overarchingTableLayoutPanel.SetColumnSpan(this.okCancelTableLayoutPanel, 2);
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Margin = new Padding(0, 6, 0, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.overarchingTableLayoutPanel.Controls.Add(this.label1, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.okCancelTableLayoutPanel, 0, 3);
            this.overarchingTableLayoutPanel.Controls.Add(this.label2, 0, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.confirmPassword, 1, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.label3, 0, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.passwordEdit, 1, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.usernameEdit, 1, 0);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            base.AcceptButton = this.okButton;
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.CancelButton = this.cancelButton;
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ServiceInstallerDialog";
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.HelpButtonClicked += new CancelEventHandler(this.ServiceInstallerDialog_HelpButtonClicked);
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
        }

        [STAThread]
        public static void Main()
        {
            Application.Run(new ServiceInstallerDialog());
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.result = ServiceInstallerDialogResult.OK;
            if (this.passwordEdit.Text == this.confirmPassword.Text)
            {
                base.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBoxOptions options = 0;
                Control parent = this;
                while (parent.RightToLeft == RightToLeft.Inherit)
                {
                    parent = parent.Parent;
                }
                if (parent.RightToLeft == RightToLeft.Yes)
                {
                    options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
                }
                base.DialogResult = DialogResult.None;
                MessageBox.Show(Res.GetString("Label_MissmatchedPasswords"), Res.GetString("Label_SetServiceLogin"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, options);
                this.passwordEdit.Text = string.Empty;
                this.confirmPassword.Text = string.Empty;
                this.passwordEdit.Focus();
            }
        }

        private void ServiceInstallerDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        public string Password
        {
            get
            {
                return this.passwordEdit.Text;
            }
            set
            {
                this.passwordEdit.Text = value;
            }
        }

        public ServiceInstallerDialogResult Result
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.result;
            }
        }

        public string Username
        {
            get
            {
                return this.usernameEdit.Text;
            }
            set
            {
                this.usernameEdit.Text = value;
            }
        }
    }
}

