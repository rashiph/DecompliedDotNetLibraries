namespace System.Security.Policy
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Windows.Forms;

    internal class TrustManagerPromptUI : Form
    {
        private Button btnCancel;
        private Button btnInstall;
        private IContainer components;
        private bool controlToolTip;
        private Label lblFrom;
        private Label lblName;
        private Label lblPublisher;
        private Label lblQuestion;
        private Label lineLabel;
        private LinkLabel linkLblFromUrl;
        private LinkLabel linkLblMoreInformation;
        private LinkLabel linkLblName;
        private LinkLabel linkLblPublisher;
        private string m_appName;
        private X509Certificate2 m_certificate;
        private string m_defaultBrowserExePath;
        private string m_deploymentUrl;
        private TrustManagerPromptOptions m_options;
        private string m_publisherName;
        private string m_supportUrl;
        private PictureBox pictureBoxQuestion;
        private PictureBox pictureBoxWarning;
        private TableLayoutPanel tableLayoutPanelButtons;
        private TableLayoutPanel tableLayoutPanelInfo;
        private TableLayoutPanel tableLayoutPanelOuter;
        private TableLayoutPanel tableLayoutPanelQuestion;
        private ToolTip toolTipFromUrl;
        private TableLayoutPanel warningTextTableLayoutPanel;

        internal TrustManagerPromptUI(string appName, string defaultBrowserExePath, string supportUrl, string deploymentUrl, string publisherName, X509Certificate2 certificate, TrustManagerPromptOptions options)
        {
            this.m_appName = appName;
            this.m_defaultBrowserExePath = defaultBrowserExePath;
            this.m_supportUrl = supportUrl;
            this.m_deploymentUrl = deploymentUrl;
            this.m_publisherName = publisherName;
            this.m_certificate = certificate;
            this.m_options = options;
            this.InitializeComponent();
            this.LoadResources();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }
                this.controlToolTip = false;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(TrustManagerPromptUI));
            this.tableLayoutPanelOuter = new TableLayoutPanel();
            this.warningTextTableLayoutPanel = new TableLayoutPanel();
            this.pictureBoxWarning = new PictureBox();
            this.linkLblMoreInformation = new LinkLabel();
            this.tableLayoutPanelQuestion = new TableLayoutPanel();
            this.lblQuestion = new Label();
            this.pictureBoxQuestion = new PictureBox();
            this.tableLayoutPanelButtons = new TableLayoutPanel();
            this.btnInstall = new Button();
            this.btnCancel = new Button();
            this.tableLayoutPanelInfo = new TableLayoutPanel();
            this.lblName = new Label();
            this.lblFrom = new Label();
            this.lblPublisher = new Label();
            this.linkLblName = new LinkLabel();
            this.linkLblFromUrl = new LinkLabel();
            this.linkLblPublisher = new LinkLabel();
            this.lineLabel = new Label();
            this.toolTipFromUrl = new ToolTip(this.components);
            this.tableLayoutPanelOuter.SuspendLayout();
            this.warningTextTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.pictureBoxWarning).BeginInit();
            this.tableLayoutPanelQuestion.SuspendLayout();
            ((ISupportInitialize) this.pictureBoxQuestion).BeginInit();
            this.tableLayoutPanelButtons.SuspendLayout();
            this.tableLayoutPanelInfo.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.tableLayoutPanelOuter, "tableLayoutPanelOuter");
            this.tableLayoutPanelOuter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 510f));
            this.tableLayoutPanelOuter.Controls.Add(this.warningTextTableLayoutPanel, 0, 4);
            this.tableLayoutPanelOuter.Controls.Add(this.tableLayoutPanelQuestion, 0, 0);
            this.tableLayoutPanelOuter.Controls.Add(this.tableLayoutPanelButtons, 0, 2);
            this.tableLayoutPanelOuter.Controls.Add(this.tableLayoutPanelInfo, 0, 1);
            this.tableLayoutPanelOuter.Controls.Add(this.lineLabel, 0, 3);
            this.tableLayoutPanelOuter.Margin = new Padding(0, 0, 0, 12);
            this.tableLayoutPanelOuter.Name = "tableLayoutPanelOuter";
            this.tableLayoutPanelOuter.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelOuter.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelOuter.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelOuter.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelOuter.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.warningTextTableLayoutPanel, "warningTextTableLayoutPanel");
            this.warningTextTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.warningTextTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.warningTextTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.warningTextTableLayoutPanel.Controls.Add(this.pictureBoxWarning, 0, 0);
            this.warningTextTableLayoutPanel.Controls.Add(this.linkLblMoreInformation, 1, 0);
            this.warningTextTableLayoutPanel.Margin = new Padding(12, 6, 0, 0);
            this.warningTextTableLayoutPanel.Name = "warningTextTableLayoutPanel";
            this.warningTextTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.pictureBoxWarning, "pictureBoxWarning");
            this.pictureBoxWarning.Margin = new Padding(0, 0, 3, 0);
            this.pictureBoxWarning.Name = "pictureBoxWarning";
            this.pictureBoxWarning.TabStop = false;
            manager.ApplyResources(this.linkLblMoreInformation, "linkLblMoreInformation");
            this.linkLblMoreInformation.Margin = new Padding(3, 0, 3, 0);
            this.linkLblMoreInformation.Name = "linkLblMoreInformation";
            this.linkLblMoreInformation.LinkClicked += new LinkLabelLinkClickedEventHandler(this.TrustManagerPromptUI_ShowMoreInformation);
            manager.ApplyResources(this.tableLayoutPanelQuestion, "tableLayoutPanelQuestion");
            this.tableLayoutPanelQuestion.BackColor = SystemColors.Window;
            this.tableLayoutPanelQuestion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanelQuestion.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58f));
            this.tableLayoutPanelQuestion.Controls.Add(this.lblQuestion, 0, 0);
            this.tableLayoutPanelQuestion.Controls.Add(this.pictureBoxQuestion, 1, 0);
            this.tableLayoutPanelQuestion.Margin = new Padding(0);
            this.tableLayoutPanelQuestion.Name = "tableLayoutPanelQuestion";
            this.tableLayoutPanelQuestion.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.lblQuestion, "lblQuestion");
            this.lblQuestion.Margin = new Padding(12, 12, 12, 0);
            this.lblQuestion.Name = "lblQuestion";
            manager.ApplyResources(this.pictureBoxQuestion, "pictureBoxQuestion");
            this.pictureBoxQuestion.Margin = new Padding(0);
            this.pictureBoxQuestion.Name = "pictureBoxQuestion";
            this.pictureBoxQuestion.TabStop = false;
            manager.ApplyResources(this.tableLayoutPanelButtons, "tableLayoutPanelButtons");
            this.tableLayoutPanelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.tableLayoutPanelButtons.Controls.Add(this.btnInstall, 0, 0);
            this.tableLayoutPanelButtons.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanelButtons.Margin = new Padding(0, 6, 12, 12);
            this.tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
            this.tableLayoutPanelButtons.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.btnInstall, "btnInstall");
            this.btnInstall.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.btnInstall.Margin = new Padding(0, 0, 3, 0);
            this.btnInstall.MinimumSize = new Size(0x4b, 0x17);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Padding = new Padding(10, 0, 10, 0);
            manager.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Margin = new Padding(3, 0, 0, 0);
            this.btnCancel.MinimumSize = new Size(0x4b, 0x17);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new Padding(10, 0, 10, 0);
            manager.ApplyResources(this.tableLayoutPanelInfo, "tableLayoutPanelInfo");
            this.tableLayoutPanelInfo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelInfo.Controls.Add(this.lblName, 0, 0);
            this.tableLayoutPanelInfo.Controls.Add(this.linkLblName, 0, 1);
            this.tableLayoutPanelInfo.Controls.Add(this.lblFrom, 0, 2);
            this.tableLayoutPanelInfo.Controls.Add(this.linkLblFromUrl, 0, 3);
            this.tableLayoutPanelInfo.Controls.Add(this.lblPublisher, 0, 4);
            this.tableLayoutPanelInfo.Controls.Add(this.linkLblPublisher, 0, 5);
            this.tableLayoutPanelInfo.Margin = new Padding(30, 0x16, 12, 3);
            this.tableLayoutPanelInfo.Name = "tableLayoutPanelInfo";
            this.tableLayoutPanelInfo.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelInfo.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelInfo.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelInfo.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelInfo.RowStyles.Add(new RowStyle());
            this.tableLayoutPanelInfo.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.lblName, "lblName");
            this.lblName.Margin = new Padding(0, 0, 3, 0);
            this.lblName.Name = "lblName";
            manager.ApplyResources(this.lblFrom, "lblFrom");
            this.lblFrom.Margin = new Padding(0, 8, 3, 0);
            this.lblFrom.Name = "lblFrom";
            manager.ApplyResources(this.lblPublisher, "lblPublisher");
            this.lblPublisher.Margin = new Padding(0, 8, 3, 0);
            this.lblPublisher.Name = "lblPublisher";
            manager.ApplyResources(this.linkLblName, "linkLblName");
            this.linkLblName.AutoEllipsis = true;
            this.linkLblName.Margin = new Padding(3, 0, 3, 8);
            this.linkLblName.Name = "linkLblName";
            this.linkLblName.TabStop = true;
            this.linkLblName.UseMnemonic = false;
            this.linkLblName.LinkClicked += new LinkLabelLinkClickedEventHandler(this.TrustManagerPromptUI_ShowSupportPage);
            manager.ApplyResources(this.linkLblFromUrl, "linkLblFromUrl");
            this.linkLblFromUrl.AutoEllipsis = true;
            this.linkLblFromUrl.Margin = new Padding(3, 0, 3, 8);
            this.linkLblFromUrl.Name = "linkLblFromUrl";
            this.linkLblFromUrl.TabStop = true;
            this.linkLblFromUrl.UseMnemonic = false;
            this.linkLblFromUrl.MouseEnter += new EventHandler(this.linkLblFromUrl_MouseEnter);
            this.linkLblFromUrl.MouseLeave += new EventHandler(this.linkLblFromUrl_MouseLeave);
            manager.ApplyResources(this.linkLblPublisher, "linkLblPublisher");
            this.linkLblPublisher.AutoEllipsis = true;
            this.linkLblPublisher.Margin = new Padding(3, 0, 3, 0);
            this.linkLblPublisher.Name = "linkLblPublisher";
            this.linkLblPublisher.TabStop = true;
            this.linkLblPublisher.UseMnemonic = false;
            this.linkLblPublisher.LinkClicked += new LinkLabelLinkClickedEventHandler(this.TrustManagerPromptUI_ShowPublisherCertificate);
            manager.ApplyResources(this.lineLabel, "lineLabel");
            this.lineLabel.BackColor = SystemColors.ControlDark;
            this.lineLabel.Margin = new Padding(0);
            this.lineLabel.Name = "lineLabel";
            base.AcceptButton = this.btnCancel;
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.CancelButton = this.btnCancel;
            base.Controls.Add(this.tableLayoutPanelOuter);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "TrustManagerPromptUI";
            base.VisibleChanged += new EventHandler(this.TrustManagerPromptUI_VisibleChanged);
            base.Load += new EventHandler(this.TrustManagerPromptUI_Load);
            this.tableLayoutPanelOuter.ResumeLayout(false);
            this.tableLayoutPanelOuter.PerformLayout();
            this.warningTextTableLayoutPanel.ResumeLayout(false);
            this.warningTextTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.pictureBoxWarning).EndInit();
            this.tableLayoutPanelQuestion.ResumeLayout(false);
            this.tableLayoutPanelQuestion.PerformLayout();
            ((ISupportInitialize) this.pictureBoxQuestion).EndInit();
            this.tableLayoutPanelButtons.ResumeLayout(false);
            this.tableLayoutPanelButtons.PerformLayout();
            this.tableLayoutPanelInfo.ResumeLayout(false);
            this.tableLayoutPanelInfo.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void linkLblFromUrl_MouseEnter(object sender, EventArgs e)
        {
            if (!this.controlToolTip && (this.toolTipFromUrl != null))
            {
                System.Windows.Forms.IntSecurity.AllWindows.Assert();
                try
                {
                    this.controlToolTip = true;
                    this.toolTipFromUrl.Show(this.linkLblFromUrl.Text, this.linkLblFromUrl);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.toolTipFromUrl, this.toolTipFromUrl.Handle), 0x418, 0, 600);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                    this.controlToolTip = false;
                }
            }
        }

        private void linkLblFromUrl_MouseLeave(object sender, EventArgs e)
        {
            if ((!this.controlToolTip && (this.toolTipFromUrl != null)) && this.toolTipFromUrl.GetHandleCreated())
            {
                this.toolTipFromUrl.RemoveAll();
                System.Windows.Forms.IntSecurity.AllWindows.Assert();
                try
                {
                    this.toolTipFromUrl.Hide(this);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        private void LoadGlobeBitmap()
        {
            Bitmap bitmap;
            lock (typeof(Form))
            {
                bitmap = new Bitmap(typeof(Form), "TrustManagerGlobe.bmp");
            }
            if (bitmap != null)
            {
                bitmap.MakeTransparent();
                this.pictureBoxQuestion.Image = bitmap;
            }
        }

        private void LoadResources()
        {
            base.SuspendAllLayout(this);
            this.LoadGlobeBitmap();
            this.UpdateFonts();
            if ((this.m_options & TrustManagerPromptOptions.StopApp) != TrustManagerPromptOptions.None)
            {
                this.btnInstall.Visible = false;
                this.btnCancel.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_Close");
                this.btnCancel.DialogResult = DialogResult.OK;
            }
            else
            {
                if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
                {
                    this.btnCancel.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_DoNotInstall");
                }
                else
                {
                    this.btnCancel.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_DoNotRun");
                }
                this.btnInstall.DialogResult = DialogResult.OK;
                this.btnCancel.DialogResult = DialogResult.Cancel;
            }
            this.linkLblName.Links.Clear();
            this.linkLblPublisher.Links.Clear();
            this.linkLblFromUrl.Links.Clear();
            this.linkLblMoreInformation.Links.Clear();
            this.linkLblName.Text = this.m_appName;
            if (((this.m_defaultBrowserExePath != null) && (this.m_certificate != null)) && ((this.m_supportUrl != null) && (this.m_supportUrl.Length > 0)))
            {
                this.linkLblName.Links.Add(0, this.m_appName.Length, this.m_supportUrl);
            }
            if (this.linkLblName.Links.Count == 0)
            {
                this.lblName.Text = StripOutAccelerator(this.lblName.Text);
            }
            this.linkLblFromUrl.Text = this.m_deploymentUrl;
            if (this.m_publisherName == null)
            {
                this.linkLblPublisher.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_UnknownPublisher");
                if (this.m_certificate != null)
                {
                    this.linkLblPublisher.Links.Add(0, this.linkLblPublisher.Text.Length);
                }
            }
            else
            {
                this.linkLblPublisher.Text = this.m_publisherName;
                if (this.m_publisherName.Length > 0)
                {
                    this.linkLblPublisher.Links.Add(0, this.m_publisherName.Length);
                }
            }
            if (this.linkLblPublisher.Links.Count == 0)
            {
                this.lblPublisher.Text = StripOutAccelerator(this.lblPublisher.Text);
            }
            if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
            {
                this.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_InstallTitle");
            }
            else
            {
                this.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_RunTitle");
                this.btnInstall.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_Run");
            }
            if ((this.m_options & TrustManagerPromptOptions.StopApp) != TrustManagerPromptOptions.None)
            {
                this.lblQuestion.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_BlockedApp");
            }
            else if (this.m_publisherName == null)
            {
                if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
                {
                    this.lblQuestion.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_NoPublisherInstallQuestion");
                }
                else
                {
                    this.lblQuestion.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_NoPublisherRunQuestion");
                }
            }
            else if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
            {
                this.lblQuestion.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_InstallQuestion");
            }
            else
            {
                this.lblQuestion.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_RunQuestion");
            }
            if ((this.m_options & TrustManagerPromptOptions.StopApp) != TrustManagerPromptOptions.None)
            {
                if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
                {
                    this.linkLblMoreInformation.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_InstalledAppBlockedWarning");
                }
                else
                {
                    this.linkLblMoreInformation.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_RunAppBlockedWarning");
                }
                this.linkLblMoreInformation.TabStop = false;
                this.linkLblMoreInformation.AccessibleDescription = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_WarningAccessibleDescription");
                this.linkLblMoreInformation.AccessibleName = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_WarningAccessibleName");
            }
            else
            {
                string str = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_MoreInformation");
                if ((this.m_options & TrustManagerPromptOptions.LocalComputerSource) != TrustManagerPromptOptions.None)
                {
                    if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
                    {
                        this.linkLblMoreInformation.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_InstallFromLocalMachineWarning", new object[] { str });
                    }
                    else
                    {
                        this.linkLblMoreInformation.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_RunFromLocalMachineWarning", new object[] { str });
                    }
                }
                else if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
                {
                    this.linkLblMoreInformation.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_InstallWarning", new object[] { str });
                }
                else
                {
                    this.linkLblMoreInformation.Text = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_RunWarning", new object[] { str });
                }
                this.linkLblMoreInformation.TabStop = true;
                this.linkLblMoreInformation.AccessibleDescription = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_MoreInformationAccessibleDescription");
                this.linkLblMoreInformation.AccessibleName = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_MoreInformationAccessibleName");
                this.linkLblMoreInformation.Links.Add(new LinkLabel.Link(this.linkLblMoreInformation.Text.Length - str.Length, str.Length));
            }
            if (((this.m_options & TrustManagerPromptOptions.StopApp) != TrustManagerPromptOptions.None) || (this.m_publisherName == null))
            {
                if (((this.m_options & TrustManagerPromptOptions.RequiresPermissions) == TrustManagerPromptOptions.None) && ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None))
                {
                    this.LoadWarningBitmap(TrustManagerWarningLevel.Yellow);
                }
                else
                {
                    this.LoadWarningBitmap(TrustManagerWarningLevel.Red);
                }
            }
            else if ((this.m_options & TrustManagerPromptOptions.RequiresPermissions) == TrustManagerPromptOptions.None)
            {
                this.LoadWarningBitmap(TrustManagerWarningLevel.Green);
            }
            else
            {
                this.LoadWarningBitmap(TrustManagerWarningLevel.Yellow);
            }
            if ((this.m_options & TrustManagerPromptOptions.StopApp) != TrustManagerPromptOptions.None)
            {
                if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
                {
                    base.AccessibleDescription = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_AccessibleDescription_InstallBlocked");
                }
                else
                {
                    base.AccessibleDescription = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_AccessibleDescription_RunBlocked");
                }
            }
            else if ((this.m_options & TrustManagerPromptOptions.RequiresPermissions) != TrustManagerPromptOptions.None)
            {
                if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
                {
                    base.AccessibleDescription = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_AccessibleDescription_InstallWithElevatedPermissions");
                }
                else
                {
                    base.AccessibleDescription = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_AccessibleDescription_RunWithElevatedPermissions");
                }
            }
            else if ((this.m_options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
            {
                base.AccessibleDescription = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_AccessibleDescription_InstallConfirmation");
            }
            else
            {
                base.AccessibleDescription = System.Windows.Forms.SR.GetString("TrustManagerPromptUI_AccessibleDescription_RunConfirmation");
            }
            base.ResumeAllLayout(this, true);
        }

        private void LoadWarningBitmap(TrustManagerWarningLevel warningLevel)
        {
            Bitmap bitmap;
            switch (warningLevel)
            {
                case TrustManagerWarningLevel.Green:
                    bitmap = new Bitmap(typeof(Form), "TrustManagerOK.bmp");
                    this.pictureBoxWarning.AccessibleDescription = string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TrustManager_WarningIconAccessibleDescription_LowRisk"), new object[] { this.pictureBoxWarning.AccessibleDescription });
                    break;

                case TrustManagerWarningLevel.Yellow:
                    bitmap = new Bitmap(typeof(Form), "TrustManagerWarning.bmp");
                    this.pictureBoxWarning.AccessibleDescription = string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TrustManager_WarningIconAccessibleDescription_MediumRisk"), new object[] { this.pictureBoxWarning.AccessibleDescription });
                    break;

                default:
                    bitmap = new Bitmap(typeof(Form), "TrustManagerHighRisk.bmp");
                    this.pictureBoxWarning.AccessibleDescription = string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TrustManager_WarningIconAccessibleDescription_HighRisk"), new object[] { this.pictureBoxWarning.AccessibleDescription });
                    break;
            }
            if (bitmap != null)
            {
                bitmap.MakeTransparent();
                this.pictureBoxWarning.Image = bitmap;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
            base.OnHandleDestroyed(e);
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Window)
            {
                this.UpdateFonts();
            }
            base.Invalidate();
        }

        private static string StripOutAccelerator(string text)
        {
            int index = text.IndexOf('&');
            if (index == -1)
            {
                return text;
            }
            if (((index > 0) && (text[index - 1] == '(')) && ((text.Length > (index + 2)) && (text[index + 2] == ')')))
            {
                return text.Remove(index - 1, 4);
            }
            return text.Replace("&", "");
        }

        private void TrustManagerPromptUI_Load(object sender, EventArgs e)
        {
            base.ActiveControl = this.btnCancel;
        }

        private void TrustManagerPromptUI_ShowMoreInformation(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                using (TrustManagerMoreInformation information = new TrustManagerMoreInformation(this.m_options, this.m_publisherName))
                {
                    information.ShowDialog(this);
                }
            }
            catch (Exception)
            {
            }
        }

        private void TrustManagerPromptUI_ShowPublisherCertificate(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                X509Certificate2UI.DisplayCertificate(this.m_certificate, base.Handle);
            }
            catch (Exception)
            {
            }
        }

        private void TrustManagerPromptUI_ShowSupportPage(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(this.m_defaultBrowserExePath, e.Link.LinkData.ToString());
            }
            catch (Exception)
            {
            }
        }

        private void TrustManagerPromptUI_VisibleChanged(object sender, EventArgs e)
        {
            if (base.Visible && (Form.ActiveForm != this))
            {
                base.Activate();
                base.ActiveControl = this.btnCancel;
            }
        }

        private void UpdateFonts()
        {
            this.Font = SystemFonts.MessageBoxFont;
            this.lblQuestion.Font = this.lblPublisher.Font = this.lblFrom.Font = this.lblName.Font = new Font(this.Font, FontStyle.Bold);
            this.linkLblPublisher.MaximumSize = this.linkLblFromUrl.MaximumSize = this.linkLblName.MaximumSize = new Size(0, this.Font.Height + 2);
        }
    }
}

