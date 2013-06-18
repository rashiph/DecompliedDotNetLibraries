namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;

    internal class UpdatePiece : ModalPiece
    {
        private UserInterfaceInfo _info;
        private Button btnOk;
        private Button btnSkip;
        private TableLayoutPanel contentTableLayoutPanel;
        private TableLayoutPanel descriptionTableLayoutPanel;
        private GroupBox groupDivider;
        private GroupBox groupRule;
        private Label lblApplication;
        private Label lblFrom;
        private Label lblFromId;
        private Label lblHeader;
        private Label lblSubHeader;
        private LinkLabel linkAppId;
        private TableLayoutPanel okSkipTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private PictureBox pictureDesktop;

        public UpdatePiece(UserInterfaceForm parentForm, UserInterfaceInfo info, ManualResetEvent modalEvent)
        {
            this._info = info;
            base._modalEvent = modalEvent;
            base._modalResult = UserInterfaceModalResult.Cancel;
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
            this.linkAppId.Font = new Font(this.linkAppId.Font, this.linkAppId.Font.Style | FontStyle.Bold);
            this.lblFromId.Font = new Font(this.lblFromId.Font, this.lblFromId.Font.Style | FontStyle.Bold);
            parentForm.ActiveControl = this.btnOk;
            parentForm.ResumeLayout(false);
            parentForm.PerformLayout();
            parentForm.Visible = true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            base._modalResult = UserInterfaceModalResult.Ok;
            base._modalEvent.Set();
            base.Enabled = false;
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            base._modalResult = UserInterfaceModalResult.Skip;
            base._modalEvent.Set();
            base.Enabled = false;
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(UpdatePiece));
            this.descriptionTableLayoutPanel = new TableLayoutPanel();
            this.pictureDesktop = new PictureBox();
            this.lblSubHeader = new Label();
            this.lblHeader = new Label();
            this.lblApplication = new Label();
            this.linkAppId = new LinkLabel();
            this.lblFrom = new Label();
            this.lblFromId = new Label();
            this.groupRule = new GroupBox();
            this.groupDivider = new GroupBox();
            this.btnOk = new Button();
            this.btnSkip = new Button();
            this.contentTableLayoutPanel = new TableLayoutPanel();
            this.okSkipTableLayoutPanel = new TableLayoutPanel();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.descriptionTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.pictureDesktop).BeginInit();
            this.contentTableLayoutPanel.SuspendLayout();
            this.okSkipTableLayoutPanel.SuspendLayout();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.descriptionTableLayoutPanel, "descriptionTableLayoutPanel");
            this.descriptionTableLayoutPanel.BackColor = SystemColors.Window;
            this.descriptionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400f));
            this.descriptionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60f));
            this.descriptionTableLayoutPanel.Controls.Add(this.pictureDesktop, 1, 0);
            this.descriptionTableLayoutPanel.Controls.Add(this.lblSubHeader, 0, 1);
            this.descriptionTableLayoutPanel.Controls.Add(this.lblHeader, 0, 0);
            this.descriptionTableLayoutPanel.Margin = new Padding(0);
            this.descriptionTableLayoutPanel.Name = "descriptionTableLayoutPanel";
            this.descriptionTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.descriptionTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.pictureDesktop, "pictureDesktop");
            this.pictureDesktop.Margin = new Padding(3, 0, 0, 0);
            this.pictureDesktop.Name = "pictureDesktop";
            this.descriptionTableLayoutPanel.SetRowSpan(this.pictureDesktop, 2);
            this.pictureDesktop.TabStop = false;
            manager.ApplyResources(this.lblSubHeader, "lblSubHeader");
            this.lblSubHeader.Margin = new Padding(0x1d, 3, 3, 8);
            this.lblSubHeader.Name = "lblSubHeader";
            manager.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.Margin = new Padding(10, 11, 3, 0);
            this.lblHeader.Name = "lblHeader";
            manager.ApplyResources(this.lblApplication, "lblApplication");
            this.lblApplication.Margin = new Padding(0, 0, 3, 3);
            this.lblApplication.Name = "lblApplication";
            manager.ApplyResources(this.linkAppId, "linkAppId");
            this.linkAppId.AutoEllipsis = true;
            this.linkAppId.Margin = new Padding(3, 0, 0, 3);
            this.linkAppId.Name = "linkAppId";
            this.linkAppId.TabStop = true;
            this.linkAppId.UseMnemonic = false;
            this.linkAppId.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkAppId_LinkClicked);
            manager.ApplyResources(this.lblFrom, "lblFrom");
            this.lblFrom.Margin = new Padding(0, 3, 3, 0);
            this.lblFrom.Name = "lblFrom";
            manager.ApplyResources(this.lblFromId, "lblFromId");
            this.lblFromId.AutoEllipsis = true;
            this.lblFromId.Margin = new Padding(3, 3, 0, 0);
            this.lblFromId.Name = "lblFromId";
            this.lblFromId.UseMnemonic = false;
            manager.ApplyResources(this.groupRule, "groupRule");
            this.groupRule.Margin = new Padding(0, 0, 0, 3);
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
            this.btnOk.Margin = new Padding(0, 0, 3, 0);
            this.btnOk.MinimumSize = new Size(0x4b, 0x17);
            this.btnOk.Name = "btnOk";
            this.btnOk.Padding = new Padding(10, 0, 10, 0);
            this.btnOk.Click += new EventHandler(this.btnOk_Click);
            manager.ApplyResources(this.btnSkip, "btnSkip");
            this.btnSkip.Margin = new Padding(3, 0, 0, 0);
            this.btnSkip.MinimumSize = new Size(0x4b, 0x17);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Padding = new Padding(10, 0, 10, 0);
            this.btnSkip.Click += new EventHandler(this.btnSkip_Click);
            manager.ApplyResources(this.contentTableLayoutPanel, "contentTableLayoutPanel");
            this.contentTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.contentTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.contentTableLayoutPanel.Controls.Add(this.lblApplication, 0, 0);
            this.contentTableLayoutPanel.Controls.Add(this.lblFrom, 0, 1);
            this.contentTableLayoutPanel.Controls.Add(this.linkAppId, 1, 0);
            this.contentTableLayoutPanel.Controls.Add(this.lblFromId, 1, 1);
            this.contentTableLayoutPanel.Margin = new Padding(20, 15, 12, 0x12);
            this.contentTableLayoutPanel.Name = "contentTableLayoutPanel";
            this.contentTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.contentTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.okSkipTableLayoutPanel, "okSkipTableLayoutPanel");
            this.okSkipTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okSkipTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okSkipTableLayoutPanel.Controls.Add(this.btnOk, 0, 0);
            this.okSkipTableLayoutPanel.Controls.Add(this.btnSkip, 1, 0);
            this.okSkipTableLayoutPanel.Margin = new Padding(0, 7, 8, 6);
            this.okSkipTableLayoutPanel.Name = "okSkipTableLayoutPanel";
            this.okSkipTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.overarchingTableLayoutPanel.Controls.Add(this.descriptionTableLayoutPanel, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.okSkipTableLayoutPanel, 0, 4);
            this.overarchingTableLayoutPanel.Controls.Add(this.contentTableLayoutPanel, 0, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.groupRule, 0, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.groupDivider, 0, 3);
            this.overarchingTableLayoutPanel.Margin = new Padding(0);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this, "$this");
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.Name = "UpdatePiece";
            this.descriptionTableLayoutPanel.ResumeLayout(false);
            this.descriptionTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.pictureDesktop).EndInit();
            this.contentTableLayoutPanel.ResumeLayout(false);
            this.contentTableLayoutPanel.PerformLayout();
            this.okSkipTableLayoutPanel.ResumeLayout(false);
            this.okSkipTableLayoutPanel.PerformLayout();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeContent()
        {
            this.pictureDesktop.Image = Resources.GetImage("setup.bmp");
            this.lblSubHeader.Text = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_UpdateSubHeader"), new object[] { UserInterface.LimitDisplayTextLength(this._info.productName) });
            this.linkAppId.Text = this._info.productName;
            this.linkAppId.Links.Clear();
            if (UserInterface.IsValidHttpUrl(this._info.supportUrl))
            {
                this.linkAppId.Links.Add(0, this._info.productName.Length, this._info.supportUrl);
            }
            this.lblFromId.Text = this._info.sourceSite;
        }

        private void linkAppId_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkAppId.LinkVisited = true;
            UserInterface.LaunchUrlInBrowser(e.Link.LinkData.ToString());
        }
    }
}

