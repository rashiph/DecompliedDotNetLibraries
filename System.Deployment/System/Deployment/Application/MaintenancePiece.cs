namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class MaintenancePiece : ModalPiece
    {
        private UserInterfaceInfo _info;
        private MaintenanceInfo _maintenanceInfo;
        private Button btnCancel;
        private Button btnHelp;
        private Button btnOk;
        private TableLayoutPanel contentTableLayoutPanel;
        private GroupBox groupDivider;
        private GroupBox groupRule;
        private Label lblHeader;
        private Label lblSubHeader;
        private TableLayoutPanel okCancelHelpTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private PictureBox pictureDesktop;
        private PictureBox pictureRemove;
        private PictureBox pictureRestore;
        private RadioButton radioRemove;
        private RadioButton radioRestore;
        private TableLayoutPanel topTableLayoutPanel;

        public MaintenancePiece(UserInterfaceForm parentForm, UserInterfaceInfo info, MaintenanceInfo maintenanceInfo, ManualResetEvent modalEvent)
        {
            base._modalResult = UserInterfaceModalResult.Cancel;
            this._info = info;
            this._maintenanceInfo = maintenanceInfo;
            base._modalEvent = modalEvent;
            base.SuspendLayout();
            this.InitializeComponent();
            this.InitializeContent();
            base.ResumeLayout(false);
            parentForm.SuspendLayout();
            parentForm.SwitchUserInterfacePiece(this);
            parentForm.Text = this._info.formTitle;
            parentForm.MinimizeBox = false;
            parentForm.MaximizeBox = false;
            parentForm.ControlBox = true;
            this.lblHeader.Font = new Font(this.lblHeader.Font, this.lblHeader.Font.Style | FontStyle.Bold);
            parentForm.ActiveControl = this.btnCancel;
            parentForm.ResumeLayout(false);
            parentForm.PerformLayout();
            parentForm.Visible = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            base._modalResult = UserInterfaceModalResult.Cancel;
            base._modalEvent.Set();
            base.Enabled = false;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (UserInterface.IsValidHttpUrl(this._info.supportUrl))
            {
                UserInterface.LaunchUrlInBrowser(this._info.supportUrl);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            base._modalResult = UserInterfaceModalResult.Ok;
            base._modalEvent.Set();
            base.Enabled = false;
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(MaintenancePiece));
            this.lblHeader = new Label();
            this.lblSubHeader = new Label();
            this.pictureRestore = new PictureBox();
            this.pictureRemove = new PictureBox();
            this.radioRestore = new RadioButton();
            this.radioRemove = new RadioButton();
            this.groupRule = new GroupBox();
            this.groupDivider = new GroupBox();
            this.btnOk = new Button();
            this.btnCancel = new Button();
            this.btnHelp = new Button();
            this.topTableLayoutPanel = new TableLayoutPanel();
            this.pictureDesktop = new PictureBox();
            this.okCancelHelpTableLayoutPanel = new TableLayoutPanel();
            this.contentTableLayoutPanel = new TableLayoutPanel();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            ((ISupportInitialize) this.pictureRestore).BeginInit();
            ((ISupportInitialize) this.pictureRemove).BeginInit();
            this.topTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.pictureDesktop).BeginInit();
            this.okCancelHelpTableLayoutPanel.SuspendLayout();
            this.contentTableLayoutPanel.SuspendLayout();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            this.lblHeader.AutoEllipsis = true;
            manager.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.Margin = new Padding(10, 11, 3, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.UseMnemonic = false;
            manager.ApplyResources(this.lblSubHeader, "lblSubHeader");
            this.lblSubHeader.Margin = new Padding(0x1d, 3, 3, 8);
            this.lblSubHeader.Name = "lblSubHeader";
            manager.ApplyResources(this.pictureRestore, "pictureRestore");
            this.pictureRestore.Margin = new Padding(0, 0, 3, 0);
            this.pictureRestore.Name = "pictureRestore";
            this.pictureRestore.TabStop = false;
            manager.ApplyResources(this.pictureRemove, "pictureRemove");
            this.pictureRemove.Margin = new Padding(0, 0, 3, 0);
            this.pictureRemove.Name = "pictureRemove";
            this.pictureRemove.TabStop = false;
            manager.ApplyResources(this.radioRestore, "radioRestore");
            this.radioRestore.Margin = new Padding(3, 0, 0, 0);
            this.radioRestore.Name = "radioRestore";
            this.radioRestore.CheckedChanged += new EventHandler(this.radioRestore_CheckedChanged);
            manager.ApplyResources(this.radioRemove, "radioRemove");
            this.radioRemove.Margin = new Padding(3, 0, 0, 0);
            this.radioRemove.Name = "radioRemove";
            this.radioRemove.CheckedChanged += new EventHandler(this.radioRemove_CheckedChanged);
            manager.ApplyResources(this.groupRule, "groupRule");
            this.groupRule.Margin = new Padding(0);
            this.groupRule.BackColor = SystemColors.ControlDark;
            this.groupRule.FlatStyle = FlatStyle.Flat;
            this.groupRule.Name = "groupRule";
            this.groupRule.TabStop = false;
            manager.ApplyResources(this.groupDivider, "groupDivider");
            this.groupDivider.Margin = new Padding(0, 3, 0, 3);
            this.groupDivider.BackColor = SystemColors.ControlDark;
            this.groupDivider.FlatStyle = FlatStyle.Flat;
            this.groupDivider.Name = "groupDivider";
            this.groupDivider.TabStop = false;
            manager.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Margin = new Padding(0, 0, 4, 0);
            this.btnOk.MinimumSize = new Size(0x4b, 0x17);
            this.btnOk.Name = "btnOk";
            this.btnOk.Padding = new Padding(10, 0, 10, 0);
            this.btnOk.Click += new EventHandler(this.btnOk_Click);
            manager.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Margin = new Padding(2, 0, 2, 0);
            this.btnCancel.MinimumSize = new Size(0x4b, 0x17);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new Padding(10, 0, 10, 0);
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            manager.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.Margin = new Padding(4, 0, 0, 0);
            this.btnHelp.MinimumSize = new Size(0x4b, 0x17);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Padding = new Padding(10, 0, 10, 0);
            this.btnHelp.Click += new EventHandler(this.btnHelp_Click);
            manager.ApplyResources(this.topTableLayoutPanel, "topTableLayoutPanel");
            this.topTableLayoutPanel.BackColor = SystemColors.Window;
            this.topTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 87.2f));
            this.topTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.8f));
            this.topTableLayoutPanel.Controls.Add(this.pictureDesktop, 1, 0);
            this.topTableLayoutPanel.Controls.Add(this.lblHeader, 0, 0);
            this.topTableLayoutPanel.Controls.Add(this.lblSubHeader, 0, 1);
            this.topTableLayoutPanel.Margin = new Padding(0);
            this.topTableLayoutPanel.Name = "topTableLayoutPanel";
            this.topTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.topTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.pictureDesktop, "pictureDesktop");
            this.pictureDesktop.Margin = new Padding(3, 0, 0, 0);
            this.pictureDesktop.Name = "pictureDesktop";
            this.topTableLayoutPanel.SetRowSpan(this.pictureDesktop, 2);
            this.pictureDesktop.TabStop = false;
            manager.ApplyResources(this.okCancelHelpTableLayoutPanel, "okCancelHelpTableLayoutPanel");
            this.okCancelHelpTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.okCancelHelpTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.okCancelHelpTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.okCancelHelpTableLayoutPanel.Controls.Add(this.btnOk, 0, 0);
            this.okCancelHelpTableLayoutPanel.Controls.Add(this.btnCancel, 1, 0);
            this.okCancelHelpTableLayoutPanel.Controls.Add(this.btnHelp, 2, 0);
            this.okCancelHelpTableLayoutPanel.Margin = new Padding(0, 9, 8, 8);
            this.okCancelHelpTableLayoutPanel.Name = "okCancelHelpTableLayoutPanel";
            this.okCancelHelpTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.contentTableLayoutPanel, "contentTableLayoutPanel");
            this.contentTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.contentTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.contentTableLayoutPanel.Controls.Add(this.pictureRestore, 0, 0);
            this.contentTableLayoutPanel.Controls.Add(this.pictureRemove, 0, 1);
            this.contentTableLayoutPanel.Controls.Add(this.radioRemove, 1, 1);
            this.contentTableLayoutPanel.Controls.Add(this.radioRestore, 1, 0);
            this.contentTableLayoutPanel.Margin = new Padding(20, 0x16, 12, 0x16);
            this.contentTableLayoutPanel.Name = "contentTableLayoutPanel";
            this.contentTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.contentTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.overarchingTableLayoutPanel.Controls.Add(this.topTableLayoutPanel, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.okCancelHelpTableLayoutPanel, 0, 4);
            this.overarchingTableLayoutPanel.Controls.Add(this.contentTableLayoutPanel, 0, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.groupDivider, 0, 3);
            this.overarchingTableLayoutPanel.Controls.Add(this.groupRule, 0, 1);
            this.overarchingTableLayoutPanel.Margin = new Padding(0);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this, "$this");
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.Name = "MaintenancePiece";
            ((ISupportInitialize) this.pictureRestore).EndInit();
            ((ISupportInitialize) this.pictureRemove).EndInit();
            this.topTableLayoutPanel.ResumeLayout(false);
            this.topTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.pictureDesktop).EndInit();
            this.okCancelHelpTableLayoutPanel.ResumeLayout(false);
            this.okCancelHelpTableLayoutPanel.PerformLayout();
            this.contentTableLayoutPanel.ResumeLayout(false);
            this.contentTableLayoutPanel.PerformLayout();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeContent()
        {
            this.pictureDesktop.Image = Resources.GetImage("setup.bmp");
            this.pictureRestore.Enabled = (this._maintenanceInfo.maintenanceFlags & MaintenanceFlags.RestorationPossible) != MaintenanceFlags.ClearFlag;
            Bitmap image = (Bitmap) Resources.GetImage("restore.bmp");
            image.MakeTransparent();
            this.pictureRestore.Image = image;
            Bitmap bitmap2 = (Bitmap) Resources.GetImage("remove.bmp");
            bitmap2.MakeTransparent();
            this.pictureRemove.Image = bitmap2;
            this.lblHeader.Text = this._info.productName;
            this.radioRestore.Checked = (this._maintenanceInfo.maintenanceFlags & MaintenanceFlags.RestorationPossible) != MaintenanceFlags.ClearFlag;
            this.radioRestore.Enabled = (this._maintenanceInfo.maintenanceFlags & MaintenanceFlags.RestorationPossible) != MaintenanceFlags.ClearFlag;
            this.radioRemove.Checked = (this._maintenanceInfo.maintenanceFlags & MaintenanceFlags.RestorationPossible) == MaintenanceFlags.ClearFlag;
            this.btnHelp.Enabled = UserInterface.IsValidHttpUrl(this._info.supportUrl);
        }

        private void radioRemove_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioRemove.Checked)
            {
                this._maintenanceInfo.maintenanceFlags |= MaintenanceFlags.RemoveSelected;
            }
            else
            {
                this._maintenanceInfo.maintenanceFlags &= ~MaintenanceFlags.RemoveSelected;
            }
        }

        private void radioRestore_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioRestore.Checked)
            {
                this._maintenanceInfo.maintenanceFlags |= MaintenanceFlags.RestoreSelected;
            }
            else
            {
                this._maintenanceInfo.maintenanceFlags &= ~MaintenanceFlags.RestoreSelected;
            }
        }
    }
}

