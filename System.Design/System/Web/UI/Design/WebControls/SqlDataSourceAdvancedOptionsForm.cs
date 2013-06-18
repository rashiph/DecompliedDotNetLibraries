namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;

    internal sealed class SqlDataSourceAdvancedOptionsForm : DesignerForm
    {
        private Button _cancelButton;
        private CheckBox _generateCheckBox;
        private Label _generateHelpLabel;
        private Label _helpLabel;
        private Button _okButton;
        private CheckBox _optimisticCheckBox;
        private Label _optimisticHelpLabel;

        public SqlDataSourceAdvancedOptionsForm(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.InitializeComponent();
            this.InitializeUI();
        }

        private void InitializeComponent()
        {
            this._helpLabel = new Label();
            this._generateCheckBox = new CheckBox();
            this._generateHelpLabel = new Label();
            this._optimisticCheckBox = new CheckBox();
            this._optimisticHelpLabel = new Label();
            this._okButton = new Button();
            this._cancelButton = new Button();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(12, 12);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x176, 0x20);
            this._helpLabel.TabIndex = 10;
            this._generateCheckBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._generateCheckBox.Location = new Point(12, 0x34);
            this._generateCheckBox.Name = "_generateCheckBox";
            this._generateCheckBox.Size = new Size(0x176, 0x12);
            this._generateCheckBox.TabIndex = 20;
            this._generateCheckBox.CheckedChanged += new EventHandler(this.OnGenerateCheckBoxCheckedChanged);
            this._generateHelpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._generateHelpLabel.Location = new Point(0x1d, 0x49);
            this._generateHelpLabel.Name = "_generateHelpLabel";
            this._generateHelpLabel.Size = new Size(0x165, 0x30);
            this._generateHelpLabel.TabIndex = 30;
            this._optimisticCheckBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._optimisticCheckBox.Location = new Point(12, 0x84);
            this._optimisticCheckBox.Name = "_optimisticCheckBox";
            this._optimisticCheckBox.Size = new Size(0x176, 0x12);
            this._optimisticCheckBox.TabIndex = 40;
            this._optimisticHelpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._optimisticHelpLabel.Location = new Point(0x1d, 0x99);
            this._optimisticHelpLabel.Name = "_optimisticHelpLabel";
            this._optimisticHelpLabel.Size = new Size(0x165, 0x34);
            this._optimisticHelpLabel.TabIndex = 50;
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.Location = new Point(230, 0xd1);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 60;
            this._okButton.Click += new EventHandler(this.OnOkButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0x137, 0xd1);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 70;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x18e, 0xf4);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._optimisticHelpLabel);
            base.Controls.Add(this._optimisticCheckBox);
            base.Controls.Add(this._generateHelpLabel);
            base.Controls.Add(this._generateCheckBox);
            base.Controls.Add(this._helpLabel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "SqlDataSourceAdvancedOptionsForm";
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceAdvancedOptionsForm_HelpLabel");
            this._generateCheckBox.Text = System.Design.SR.GetString("SqlDataSourceAdvancedOptionsForm_GenerateCheckBox");
            this._generateHelpLabel.Text = System.Design.SR.GetString("SqlDataSourceAdvancedOptionsForm_GenerateHelpLabel");
            this._optimisticCheckBox.Text = System.Design.SR.GetString("SqlDataSourceAdvancedOptionsForm_OptimisticCheckBox");
            this._optimisticHelpLabel.Text = System.Design.SR.GetString("SqlDataSourceAdvancedOptionsForm_OptimisticLabel");
            this.Text = System.Design.SR.GetString("SqlDataSourceAdvancedOptionsForm_Caption");
            this._generateCheckBox.AccessibleDescription = this._generateHelpLabel.Text;
            this._optimisticCheckBox.AccessibleDescription = this._optimisticHelpLabel.Text;
            this._okButton.Text = System.Design.SR.GetString("OK");
            this._cancelButton.Text = System.Design.SR.GetString("Cancel");
            this.UpdateFonts();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.UpdateFonts();
        }

        private void OnGenerateCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            this.UpdateEnabledState();
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        public void SetAllowAutogenerate(bool allowAutogenerate)
        {
            if (!allowAutogenerate)
            {
                this._generateCheckBox.Checked = false;
                this._generateCheckBox.Enabled = false;
                this._generateHelpLabel.Enabled = false;
                this.UpdateEnabledState();
            }
        }

        private void UpdateEnabledState()
        {
            bool flag = this._generateCheckBox.Checked;
            this._optimisticCheckBox.Enabled = flag;
            this._optimisticHelpLabel.Enabled = flag;
            if (!flag)
            {
                this._optimisticCheckBox.Checked = false;
            }
        }

        private void UpdateFonts()
        {
            Font font = new Font(this.Font, FontStyle.Bold);
            this._generateCheckBox.Font = font;
            this._optimisticCheckBox.Font = font;
        }

        public bool GenerateStatements
        {
            get
            {
                return this._generateCheckBox.Checked;
            }
            set
            {
                this._generateCheckBox.Checked = value;
                this.UpdateEnabledState();
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.SqlDataSource.AdvancedOptions";
            }
        }

        public bool OptimisticConcurrency
        {
            get
            {
                return this._optimisticCheckBox.Checked;
            }
            set
            {
                this._optimisticCheckBox.Checked = value;
                this.UpdateEnabledState();
            }
        }
    }
}

