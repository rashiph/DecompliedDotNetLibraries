namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;

    internal class ProgressPiece : FormPiece, IDownloadNotification
    {
        private Bitmap _appIconBitmap;
        private bool _appIconShown;
        private static long[] _bytesFormatRanges = new long[] { 0x400L, 0x2800L, 0x19000L, 0x100000L, 0xa00000L, 0x6400000L, 0x40000000L, 0x280000000L, 0x1900000000L };
        private static string[] _bytesFormatStrings = new string[] { "UI_ProgressBytesInBytes", "UI_ProgressBytesIn1KB", "UI_ProgressBytesIn10KB", "UI_ProgressBytesIn100KB", "UI_ProgressBytesIn1MB", "UI_ProgressBytesIn10MB", "UI_ProgressBytesIn100MB", "UI_ProgressBytesIn1GB", "UI_ProgressBytesIn10GB", "UI_ProgressBytesIn100GB" };
        private DownloadEventArgs _downloadData;
        private UserInterfaceInfo _info;
        private UserInterfaceForm _parentForm;
        private bool _userCancelling;
        private Button btnCancel;
        private TableLayoutPanel contentTableLayoutPanel;
        private MethodInvoker disableMethodInvoker;
        private GroupBox groupDivider;
        private GroupBox groupRule;
        private Label lblApplication;
        private Label lblFrom;
        private Label lblFromId;
        private Label lblHeader;
        private Label lblProgressText;
        private Label lblSubHeader;
        private LinkLabel linkAppId;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private PictureBox pictureAppIcon;
        private PictureBox pictureDesktop;
        private ProgressBar progress;
        private TableLayoutPanel topTextTableLayoutPanel;
        private MethodInvoker updateUIMethodInvoker;

        public ProgressPiece(UserInterfaceForm parentForm, UserInterfaceInfo info)
        {
            this._info = info;
            base.SuspendLayout();
            this.InitializeComponent();
            this.InitializeContent();
            base.ResumeLayout(false);
            parentForm.SuspendLayout();
            parentForm.SwitchUserInterfacePiece(this);
            parentForm.Text = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitle"), new object[] { 0, this._info.formTitle });
            parentForm.MinimizeBox = true;
            parentForm.MaximizeBox = false;
            parentForm.ControlBox = true;
            this.lblHeader.Font = new Font(this.lblHeader.Font, this.lblHeader.Font.Style | FontStyle.Bold);
            this.linkAppId.Font = new Font(this.linkAppId.Font, this.linkAppId.Font.Style | FontStyle.Bold);
            this.lblFromId.Font = new Font(this.lblFromId.Font, this.lblFromId.Font.Style | FontStyle.Bold);
            parentForm.ActiveControl = this.btnCancel;
            parentForm.ResumeLayout(false);
            parentForm.PerformLayout();
            parentForm.Visible = true;
            this.updateUIMethodInvoker = new MethodInvoker(this.UpdateUI);
            this.disableMethodInvoker = new MethodInvoker(this.Disable);
            this._parentForm = parentForm;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this._userCancelling = true;
            this.Disable();
            this._parentForm.Visible = false;
        }

        private void Disable()
        {
            this.lblProgressText.Text = Resources.GetString("UI_ProgressDone");
            base.Enabled = false;
        }

        public void DownloadCompleted(object sender, DownloadEventArgs e)
        {
            base.BeginInvoke(this.disableMethodInvoker);
        }

        public void DownloadModified(object sender, DownloadEventArgs e)
        {
            if (this._userCancelling)
            {
                ((FileDownloader) sender).Cancel();
            }
            else
            {
                this._downloadData = e;
                if (((this._info.iconFilePath != null) && (this._appIconBitmap == null)) && ((e.Cookie != null) && File.Exists(this._info.iconFilePath)))
                {
                    using (Icon icon = Icon.ExtractAssociatedIcon(this._info.iconFilePath))
                    {
                        this._appIconBitmap = this.TryGet32x32Bitmap(icon);
                    }
                }
                base.BeginInvoke(this.updateUIMethodInvoker);
            }
        }

        private static string FormatBytes(long bytes)
        {
            int index = Array.BinarySearch<long>(_bytesFormatRanges, bytes);
            index = (index >= 0) ? (index + 1) : ~index;
            return string.Format(CultureInfo.CurrentUICulture, Resources.GetString(_bytesFormatStrings[index]), new object[] { (index == 0) ? ((float) bytes) : (((float) bytes) / ((float) _bytesFormatRanges[((index - 1) / 3) * 3])) });
        }

        private static string FormatProgressText(long completed, long total)
        {
            return string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressText"), new object[] { FormatBytes(completed), FormatBytes(total) });
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ProgressPiece));
            this.topTextTableLayoutPanel = new TableLayoutPanel();
            this.pictureDesktop = new PictureBox();
            this.lblSubHeader = new Label();
            this.lblHeader = new Label();
            this.pictureAppIcon = new PictureBox();
            this.lblApplication = new Label();
            this.linkAppId = new LinkLabel();
            this.lblFrom = new Label();
            this.lblFromId = new Label();
            this.progress = new ProgressBar();
            this.lblProgressText = new Label();
            this.groupRule = new GroupBox();
            this.groupDivider = new GroupBox();
            this.btnCancel = new Button();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.contentTableLayoutPanel = new TableLayoutPanel();
            this.topTextTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.pictureDesktop).BeginInit();
            ((ISupportInitialize) this.pictureAppIcon).BeginInit();
            this.overarchingTableLayoutPanel.SuspendLayout();
            this.contentTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.topTextTableLayoutPanel, "topTextTableLayoutPanel");
            this.topTextTableLayoutPanel.BackColor = SystemColors.Window;
            this.topTextTableLayoutPanel.Controls.Add(this.pictureDesktop, 1, 0);
            this.topTextTableLayoutPanel.Controls.Add(this.lblSubHeader, 0, 1);
            this.topTextTableLayoutPanel.Controls.Add(this.lblHeader, 0, 0);
            this.topTextTableLayoutPanel.MinimumSize = new Size(0x1f2, 0x3d);
            this.topTextTableLayoutPanel.Name = "topTextTableLayoutPanel";
            manager.ApplyResources(this.pictureDesktop, "pictureDesktop");
            this.pictureDesktop.MinimumSize = new Size(0x3d, 0x3d);
            this.pictureDesktop.Name = "pictureDesktop";
            this.topTextTableLayoutPanel.SetRowSpan(this.pictureDesktop, 2);
            this.pictureDesktop.TabStop = false;
            manager.ApplyResources(this.lblSubHeader, "lblSubHeader");
            this.lblSubHeader.Name = "lblSubHeader";
            manager.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.AutoEllipsis = true;
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.UseMnemonic = false;
            manager.ApplyResources(this.pictureAppIcon, "pictureAppIcon");
            this.pictureAppIcon.Name = "pictureAppIcon";
            this.pictureAppIcon.TabStop = false;
            manager.ApplyResources(this.lblApplication, "lblApplication");
            this.lblApplication.Name = "lblApplication";
            manager.ApplyResources(this.linkAppId, "linkAppId");
            this.linkAppId.AutoEllipsis = true;
            this.linkAppId.Name = "linkAppId";
            this.linkAppId.UseMnemonic = false;
            this.linkAppId.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkAppId_LinkClicked);
            manager.ApplyResources(this.lblFrom, "lblFrom");
            this.lblFrom.Name = "lblFrom";
            manager.ApplyResources(this.lblFromId, "lblFromId");
            this.lblFromId.AutoEllipsis = true;
            this.lblFromId.MinimumSize = new Size(0x180, 0x20);
            this.lblFromId.Name = "lblFromId";
            this.lblFromId.UseMnemonic = false;
            manager.ApplyResources(this.progress, "progress");
            this.contentTableLayoutPanel.SetColumnSpan(this.progress, 2);
            this.progress.Name = "progress";
            this.progress.TabStop = false;
            manager.ApplyResources(this.lblProgressText, "lblProgressText");
            this.contentTableLayoutPanel.SetColumnSpan(this.lblProgressText, 2);
            this.lblProgressText.Name = "lblProgressText";
            manager.ApplyResources(this.groupRule, "groupRule");
            this.groupRule.BackColor = SystemColors.ControlDark;
            this.groupRule.FlatStyle = FlatStyle.Flat;
            this.groupRule.Name = "groupRule";
            this.groupRule.TabStop = false;
            manager.ApplyResources(this.groupDivider, "groupDivider");
            this.groupDivider.BackColor = SystemColors.ControlDark;
            this.groupDivider.FlatStyle = FlatStyle.Flat;
            this.groupDivider.Name = "groupDivider";
            this.groupDivider.TabStop = false;
            manager.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.MinimumSize = new Size(0x4b, 0x17);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.Controls.Add(this.contentTableLayoutPanel, 0, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.topTextTableLayoutPanel, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.groupRule, 0, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.btnCancel, 0, 4);
            this.overarchingTableLayoutPanel.Controls.Add(this.groupDivider, 0, 3);
            this.overarchingTableLayoutPanel.MinimumSize = new Size(0x203, 240);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            manager.ApplyResources(this.contentTableLayoutPanel, "contentTableLayoutPanel");
            this.contentTableLayoutPanel.Controls.Add(this.pictureAppIcon, 0, 0);
            this.contentTableLayoutPanel.Controls.Add(this.lblApplication, 1, 0);
            this.contentTableLayoutPanel.Controls.Add(this.lblFrom, 1, 1);
            this.contentTableLayoutPanel.Controls.Add(this.lblProgressText, 1, 3);
            this.contentTableLayoutPanel.Controls.Add(this.linkAppId, 2, 0);
            this.contentTableLayoutPanel.Controls.Add(this.progress, 1, 2);
            this.contentTableLayoutPanel.Controls.Add(this.lblFromId, 2, 1);
            this.contentTableLayoutPanel.MinimumSize = new Size(0x1d2, 0x7b);
            this.contentTableLayoutPanel.Name = "contentTableLayoutPanel";
            manager.ApplyResources(this, "$this");
            base.Controls.Add(this.overarchingTableLayoutPanel);
            this.MinimumSize = new Size(0x1f2, 240);
            base.Name = "ProgressPiece";
            this.topTextTableLayoutPanel.ResumeLayout(false);
            this.topTextTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.pictureDesktop).EndInit();
            ((ISupportInitialize) this.pictureAppIcon).EndInit();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            this.contentTableLayoutPanel.ResumeLayout(false);
            this.contentTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeContent()
        {
            this.pictureDesktop.Image = Resources.GetImage("setup.bmp");
            this.lblHeader.Text = this._info.formTitle;
            using (Icon icon = Resources.GetIcon("defaultappicon.ico"))
            {
                this.pictureAppIcon.Image = this.TryGet32x32Bitmap(icon);
            }
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

        public override bool OnClosing()
        {
            bool flag = base.OnClosing();
            if (!base.Enabled)
            {
                return false;
            }
            this._userCancelling = true;
            return flag;
        }

        private Bitmap TryGet32x32Bitmap(Icon icon)
        {
            using (Icon icon2 = new Icon(icon, 0x20, 0x20))
            {
                Bitmap bitmap = icon2.ToBitmap();
                bitmap.MakeTransparent();
                return bitmap;
            }
        }

        private void UpdateUI()
        {
            if (!base.IsDisposed)
            {
                base.SuspendLayout();
                this.lblProgressText.Text = FormatProgressText(this._downloadData.BytesCompleted, this._downloadData.BytesTotal);
                this.progress.Minimum = 0;
                int bytesCompleted = 0;
                int num2 = 0;
                long bytesTotal = this._downloadData.BytesTotal;
                if (bytesTotal > 0x7fffffffL)
                {
                    float num4 = 1f;
                    num4 = ((float) bytesTotal) / 2.147484E+09f;
                    bytesCompleted = (int) (((float) this._downloadData.BytesCompleted) / num4);
                    num2 = 0x7fffffff;
                }
                else
                {
                    bytesCompleted = (int) this._downloadData.BytesCompleted;
                    num2 = (int) bytesTotal;
                }
                this.progress.Maximum = num2;
                this.progress.Value = bytesCompleted;
                base.FindForm().Text = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitle"), new object[] { this._downloadData.Progress, this._info.formTitle });
                if (!this._appIconShown && (this._appIconBitmap != null))
                {
                    this.pictureAppIcon.Image = this._appIconBitmap;
                    this._appIconShown = true;
                }
                base.ResumeLayout(false);
            }
        }
    }
}

