namespace System.Security.Policy
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal class TrustManagerMoreInformation : Form
    {
        private Button btnClose;
        private IContainer components;
        private Label lblInstallation;
        private Label lblInstallationContent;
        private Label lblLocation;
        private Label lblLocationContent;
        private Label lblMachineAccess;
        private Label lblMachineAccessContent;
        private Label lblPublisher;
        private Label lblPublisherContent;
        private PictureBox pictureBoxInstallation;
        private PictureBox pictureBoxLocation;
        private PictureBox pictureBoxMachineAccess;
        private PictureBox pictureBoxPublisher;
        private TableLayoutPanel tableLayoutPanel;

        internal TrustManagerMoreInformation(TrustManagerPromptOptions options, string publisherName)
        {
            this.InitializeComponent();
            this.Font = SystemFonts.MessageBoxFont;
            this.lblMachineAccess.Font = this.lblPublisher.Font = this.lblInstallation.Font = this.lblLocation.Font = new Font(this.lblMachineAccess.Font, FontStyle.Bold);
            this.FillContent(options, publisherName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void FillContent(TrustManagerPromptOptions options, string publisherName)
        {
            TrustManagerWarningLevel green;
            string str;
            LoadWarningBitmap((publisherName == null) ? TrustManagerWarningLevel.Red : TrustManagerWarningLevel.Green, this.pictureBoxPublisher);
            LoadWarningBitmap(((options & (TrustManagerPromptOptions.WillHaveFullTrust | TrustManagerPromptOptions.RequiresPermissions)) != TrustManagerPromptOptions.None) ? TrustManagerWarningLevel.Red : TrustManagerWarningLevel.Green, this.pictureBoxMachineAccess);
            LoadWarningBitmap(((options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None) ? TrustManagerWarningLevel.Yellow : TrustManagerWarningLevel.Green, this.pictureBoxInstallation);
            if ((options & (TrustManagerPromptOptions.TrustedSitesSource | TrustManagerPromptOptions.LocalComputerSource | TrustManagerPromptOptions.LocalNetworkSource)) != TrustManagerPromptOptions.None)
            {
                green = TrustManagerWarningLevel.Green;
            }
            else if ((options & TrustManagerPromptOptions.UntrustedSitesSource) != TrustManagerPromptOptions.None)
            {
                green = TrustManagerWarningLevel.Red;
            }
            else
            {
                green = TrustManagerWarningLevel.Yellow;
            }
            LoadWarningBitmap(green, this.pictureBoxLocation);
            if (publisherName == null)
            {
                this.lblPublisherContent.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_UnknownPublisher");
            }
            else
            {
                this.lblPublisherContent.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_KnownPublisher", new object[] { publisherName });
            }
            if ((options & (TrustManagerPromptOptions.WillHaveFullTrust | TrustManagerPromptOptions.RequiresPermissions)) != TrustManagerPromptOptions.None)
            {
                this.lblMachineAccessContent.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_UnsafeAccess");
            }
            else
            {
                this.lblMachineAccessContent.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_SafeAccess");
            }
            if ((options & TrustManagerPromptOptions.AddsShortcut) != TrustManagerPromptOptions.None)
            {
                this.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_InstallTitle");
                this.lblInstallationContent.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_WithShortcut");
            }
            else
            {
                this.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_RunTitle");
                this.lblInstallationContent.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_WithoutShortcut");
            }
            if ((options & TrustManagerPromptOptions.LocalNetworkSource) != TrustManagerPromptOptions.None)
            {
                str = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_LocalNetworkSource");
            }
            else if ((options & TrustManagerPromptOptions.LocalComputerSource) != TrustManagerPromptOptions.None)
            {
                str = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_LocalComputerSource");
            }
            else if ((options & TrustManagerPromptOptions.InternetSource) != TrustManagerPromptOptions.None)
            {
                str = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_InternetSource");
            }
            else if ((options & TrustManagerPromptOptions.TrustedSitesSource) != TrustManagerPromptOptions.None)
            {
                str = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_TrustedSitesSource");
            }
            else
            {
                str = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_UntrustedSitesSource");
            }
            this.lblLocationContent.Text = System.Windows.Forms.SR.GetString("TrustManagerMoreInfo_Location", new object[] { str });
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(TrustManagerMoreInformation));
            this.tableLayoutPanel = new TableLayoutPanel();
            this.pictureBoxPublisher = new PictureBox();
            this.pictureBoxMachineAccess = new PictureBox();
            this.pictureBoxInstallation = new PictureBox();
            this.pictureBoxLocation = new PictureBox();
            this.lblPublisher = new Label();
            this.lblPublisherContent = new Label();
            this.lblMachineAccess = new Label();
            this.lblMachineAccessContent = new Label();
            this.lblInstallation = new Label();
            this.lblInstallationContent = new Label();
            this.lblLocation = new Label();
            this.lblLocationContent = new Label();
            this.btnClose = new Button();
            this.tableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.pictureBoxPublisher).BeginInit();
            ((ISupportInitialize) this.pictureBoxMachineAccess).BeginInit();
            ((ISupportInitialize) this.pictureBoxInstallation).BeginInit();
            ((ISupportInitialize) this.pictureBoxLocation).BeginInit();
            base.SuspendLayout();
            manager.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 389f));
            this.tableLayoutPanel.Controls.Add(this.pictureBoxPublisher, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.pictureBoxMachineAccess, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.pictureBoxInstallation, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.pictureBoxLocation, 0, 6);
            this.tableLayoutPanel.Controls.Add(this.lblPublisher, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.lblPublisherContent, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.lblMachineAccess, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.lblMachineAccessContent, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.lblInstallation, 1, 4);
            this.tableLayoutPanel.Controls.Add(this.lblInstallationContent, 1, 5);
            this.tableLayoutPanel.Controls.Add(this.lblLocation, 1, 6);
            this.tableLayoutPanel.Controls.Add(this.lblLocationContent, 1, 7);
            this.tableLayoutPanel.Controls.Add(this.btnClose, 1, 8);
            this.tableLayoutPanel.Margin = new Padding(12);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.pictureBoxPublisher, "pictureBoxPublisher");
            this.pictureBoxPublisher.Margin = new Padding(0, 0, 3, 0);
            this.pictureBoxPublisher.Name = "pictureBoxPublisher";
            this.tableLayoutPanel.SetRowSpan(this.pictureBoxPublisher, 2);
            this.pictureBoxPublisher.TabStop = false;
            manager.ApplyResources(this.pictureBoxMachineAccess, "pictureBoxMachineAccess");
            this.pictureBoxMachineAccess.Margin = new Padding(0, 10, 3, 0);
            this.pictureBoxMachineAccess.Name = "pictureBoxMachineAccess";
            this.tableLayoutPanel.SetRowSpan(this.pictureBoxMachineAccess, 2);
            this.pictureBoxMachineAccess.TabStop = false;
            manager.ApplyResources(this.pictureBoxInstallation, "pictureBoxInstallation");
            this.pictureBoxInstallation.Margin = new Padding(0, 10, 3, 0);
            this.pictureBoxInstallation.Name = "pictureBoxInstallation";
            this.tableLayoutPanel.SetRowSpan(this.pictureBoxInstallation, 2);
            this.pictureBoxInstallation.TabStop = false;
            manager.ApplyResources(this.pictureBoxLocation, "pictureBoxLocation");
            this.pictureBoxLocation.Margin = new Padding(0, 10, 3, 0);
            this.pictureBoxLocation.Name = "pictureBoxLocation";
            this.tableLayoutPanel.SetRowSpan(this.pictureBoxLocation, 2);
            this.pictureBoxLocation.TabStop = false;
            manager.ApplyResources(this.lblPublisher, "lblPublisher");
            this.lblPublisher.Margin = new Padding(3, 0, 0, 0);
            this.lblPublisher.Name = "lblPublisher";
            manager.ApplyResources(this.lblPublisherContent, "lblPublisherContent");
            this.lblPublisherContent.Margin = new Padding(3, 0, 0, 10);
            this.lblPublisherContent.Name = "lblPublisherContent";
            manager.ApplyResources(this.lblMachineAccess, "lblMachineAccess");
            this.lblMachineAccess.Margin = new Padding(3, 10, 0, 0);
            this.lblMachineAccess.Name = "lblMachineAccess";
            manager.ApplyResources(this.lblMachineAccessContent, "lblMachineAccessContent");
            this.lblMachineAccessContent.Margin = new Padding(3, 0, 0, 10);
            this.lblMachineAccessContent.Name = "lblMachineAccessContent";
            manager.ApplyResources(this.lblInstallation, "lblInstallation");
            this.lblInstallation.Margin = new Padding(3, 10, 0, 0);
            this.lblInstallation.Name = "lblInstallation";
            manager.ApplyResources(this.lblInstallationContent, "lblInstallationContent");
            this.lblInstallationContent.Margin = new Padding(3, 0, 0, 10);
            this.lblInstallationContent.Name = "lblInstallationContent";
            manager.ApplyResources(this.lblLocation, "lblLocation");
            this.lblLocation.Margin = new Padding(3, 10, 0, 0);
            this.lblLocation.Name = "lblLocation";
            manager.ApplyResources(this.lblLocationContent, "lblLocationContent");
            this.lblLocationContent.Margin = new Padding(3, 0, 0, 10);
            this.lblLocationContent.Name = "lblLocationContent";
            manager.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.btnClose.DialogResult = DialogResult.Cancel;
            this.btnClose.Margin = new Padding(0, 10, 0, 0);
            this.btnClose.MinimumSize = new Size(0x4b, 0x17);
            this.btnClose.Name = "btnClose";
            this.btnClose.Padding = new Padding(10, 0, 10, 0);
            this.tableLayoutPanel.SetColumnSpan(this.btnClose, 2);
            base.AcceptButton = this.btnClose;
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.CancelButton = this.btnClose;
            base.Controls.Add(this.tableLayoutPanel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "TrustManagerMoreInformation";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.pictureBoxPublisher).EndInit();
            ((ISupportInitialize) this.pictureBoxMachineAccess).EndInit();
            ((ISupportInitialize) this.pictureBoxInstallation).EndInit();
            ((ISupportInitialize) this.pictureBoxLocation).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private static void LoadWarningBitmap(TrustManagerWarningLevel warningLevel, PictureBox pictureBox)
        {
            Bitmap bitmap;
            switch (warningLevel)
            {
                case TrustManagerWarningLevel.Green:
                    bitmap = new Bitmap(typeof(Form), "TrustManagerOKSm.bmp");
                    pictureBox.AccessibleDescription = string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TrustManager_WarningIconAccessibleDescription_LowRisk"), new object[] { pictureBox.AccessibleDescription });
                    break;

                case TrustManagerWarningLevel.Yellow:
                    bitmap = new Bitmap(typeof(Form), "TrustManagerWarningSm.bmp");
                    pictureBox.AccessibleDescription = string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TrustManager_WarningIconAccessibleDescription_MediumRisk"), new object[] { pictureBox.AccessibleDescription });
                    break;

                default:
                    bitmap = new Bitmap(typeof(Form), "TrustManagerHighRiskSm.bmp");
                    pictureBox.AccessibleDescription = string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TrustManager_WarningIconAccessibleDescription_HighRisk"), new object[] { pictureBox.AccessibleDescription });
                    break;
            }
            if (bitmap != null)
            {
                bitmap.MakeTransparent();
                pictureBox.Image = bitmap;
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
                this.Font = SystemFonts.MessageBoxFont;
                this.lblLocation.Font = this.lblInstallation.Font = this.lblMachineAccess.Font = this.lblPublisher.Font = new Font(this.Font, FontStyle.Bold);
            }
            base.Invalidate();
        }
    }
}

