namespace System.Web.UI.Design.Util
{
    using System;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal abstract class TaskForm : TaskFormBase
    {
        private Button _cancelButton;
        private TableLayoutPanel _dialogButtonsTableLayoutPanel;
        private Label _dummyLabel1;
        private Button _okButton;

        public TaskForm(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.InitializeComponent();
            this.InitializeUI();
        }

        private void InitializeComponent()
        {
            this._dialogButtonsTableLayoutPanel = new TableLayoutPanel();
            this._okButton = new Button();
            this._cancelButton = new Button();
            this._dummyLabel1 = new Label();
            this._dialogButtonsTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            this._dialogButtonsTableLayoutPanel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._dialogButtonsTableLayoutPanel.AutoSize = true;
            this._dialogButtonsTableLayoutPanel.ColumnCount = 3;
            this._dialogButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this._dialogButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 6f));
            this._dialogButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this._dialogButtonsTableLayoutPanel.Controls.Add(this._okButton);
            this._dialogButtonsTableLayoutPanel.Controls.Add(this._dummyLabel1);
            this._dialogButtonsTableLayoutPanel.Controls.Add(this._cancelButton);
            this._dialogButtonsTableLayoutPanel.Location = new Point(0x194, 0x17d);
            this._dialogButtonsTableLayoutPanel.Name = "_dialogButtonsTableLayoutPanel";
            this._dialogButtonsTableLayoutPanel.RowStyles.Add(new RowStyle());
            this._dialogButtonsTableLayoutPanel.Size = new Size(0x9c, 0x17);
            this._dialogButtonsTableLayoutPanel.TabIndex = 100;
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._okButton.AutoSize = true;
            this._okButton.DialogResult = DialogResult.OK;
            this._okButton.Enabled = false;
            this._okButton.Location = new Point(0, 0);
            this._okButton.Margin = new Padding(0);
            this._okButton.MinimumSize = new Size(0x4b, 0x17);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 50;
            this._okButton.Click += new EventHandler(this.OnOKButtonClick);
            this._dummyLabel1.Location = new Point(0x4b, 0);
            this._dummyLabel1.Margin = new Padding(0);
            this._dummyLabel1.Name = "_dummyLabel1";
            this._dummyLabel1.Size = new Size(6, 0);
            this._dummyLabel1.TabIndex = 20;
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._cancelButton.AutoSize = true;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0x51, 0);
            this._cancelButton.Margin = new Padding(0);
            this._cancelButton.MinimumSize = new Size(0x4b, 0x17);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 70;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AcceptButton = this._okButton;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.CancelButton = this._cancelButton;
            base.Controls.Add(this._dialogButtonsTableLayoutPanel);
            this._dialogButtonsTableLayoutPanel.ResumeLayout(false);
            this._dialogButtonsTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            this._cancelButton.Text = System.Design.SR.GetString("Wizard_CancelButton");
            this._okButton.Text = System.Design.SR.GetString("OKCaption");
        }

        protected virtual void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        protected virtual void OnOKButtonClick(object sender, EventArgs e)
        {
        }

        protected Button OKButton
        {
            get
            {
                return this._okButton;
            }
        }
    }
}

