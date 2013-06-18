namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class ErrorPiece : ModalPiece
    {
        private string _errorMessage;
        private string _linkUrl;
        private string _linkUrlMessage;
        private string _logFileLocation;
        private Button btnOk;
        private Button btnSupport;
        private LinkLabel errorLink;
        private Label lblMessage;
        private TableLayoutPanel okDetailsTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private PictureBox pictureIcon;

        public ErrorPiece(UserInterfaceForm parentForm, string errorTitle, string errorMessage, string logFileLocation, string linkUrl, string linkUrlMessage, ManualResetEvent modalEvent)
        {
            this._errorMessage = errorMessage;
            this._logFileLocation = logFileLocation;
            this._linkUrl = linkUrl;
            this._linkUrlMessage = linkUrlMessage;
            base._modalResult = UserInterfaceModalResult.Ok;
            base._modalEvent = modalEvent;
            base.SuspendLayout();
            this.InitializeComponent();
            this.InitializeContent();
            base.ResumeLayout(false);
            parentForm.SuspendLayout();
            parentForm.SwitchUserInterfacePiece(this);
            parentForm.Text = errorTitle;
            parentForm.MinimizeBox = false;
            parentForm.MaximizeBox = false;
            parentForm.ControlBox = true;
            parentForm.ActiveControl = this.btnOk;
            parentForm.ResumeLayout(false);
            parentForm.PerformLayout();
            parentForm.Visible = true;
            if (Form.ActiveForm != parentForm)
            {
                parentForm.Activate();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            base._modalResult = UserInterfaceModalResult.Ok;
            base._modalEvent.Set();
            base.Enabled = false;
        }

        private void btnSupport_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("notepad.exe", this._logFileLocation);
            }
            catch (Win32Exception)
            {
            }
        }

        private void errorLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.errorLink.Links[this.errorLink.Links.IndexOf(e.Link)].Visited = true;
            if ((this._linkUrl != null) && UserInterface.IsValidHttpUrl(this._linkUrl))
            {
                UserInterface.LaunchUrlInBrowser(e.Link.LinkData.ToString());
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ErrorPiece));
            this.lblMessage = new Label();
            this.pictureIcon = new PictureBox();
            this.btnOk = new Button();
            this.btnSupport = new Button();
            this.okDetailsTableLayoutPanel = new TableLayoutPanel();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.errorLink = new LinkLabel();
            ((ISupportInitialize) this.pictureIcon).BeginInit();
            this.okDetailsTableLayoutPanel.SuspendLayout();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.lblMessage, "lblMessage");
            this.lblMessage.Name = "lblMessage";
            manager.ApplyResources(this.pictureIcon, "pictureIcon");
            this.pictureIcon.Name = "pictureIcon";
            this.pictureIcon.TabStop = false;
            manager.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.MinimumSize = new Size(0x4e, 0x1c);
            this.btnOk.Name = "btnOk";
            this.btnOk.Click += new EventHandler(this.btnOk_Click);
            manager.ApplyResources(this.btnSupport, "btnSupport");
            this.btnSupport.MinimumSize = new Size(0x4e, 0x1c);
            this.btnSupport.Name = "btnSupport";
            this.btnSupport.Click += new EventHandler(this.btnSupport_Click);
            manager.ApplyResources(this.okDetailsTableLayoutPanel, "okDetailsTableLayoutPanel");
            this.overarchingTableLayoutPanel.SetColumnSpan(this.okDetailsTableLayoutPanel, 2);
            this.okDetailsTableLayoutPanel.Controls.Add(this.btnOk, 0, 0);
            this.okDetailsTableLayoutPanel.Controls.Add(this.btnSupport, 1, 0);
            this.okDetailsTableLayoutPanel.Name = "okDetailsTableLayoutPanel";
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.Controls.Add(this.pictureIcon, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.okDetailsTableLayoutPanel, 0, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.lblMessage, 1, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.errorLink, 1, 1);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            manager.ApplyResources(this.errorLink, "errorLink");
            this.errorLink.MinimumSize = new Size(280, 0x20);
            this.errorLink.Name = "errorLink";
            this.errorLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.errorLink_LinkClicked);
            manager.ApplyResources(this, "$this");
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.Name = "ErrorPiece";
            ((ISupportInitialize) this.pictureIcon).EndInit();
            this.okDetailsTableLayoutPanel.ResumeLayout(false);
            this.okDetailsTableLayoutPanel.PerformLayout();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeContent()
        {
            Bitmap image = (Bitmap) Resources.GetImage("information.bmp");
            image.MakeTransparent();
            this.pictureIcon.Image = image;
            this.lblMessage.Text = this._errorMessage;
            if ((this._linkUrl != null) && (this._linkUrlMessage != null))
            {
                string str = Resources.GetString("UI_ErrorClickHereHere");
                this.errorLink.Text = this._linkUrlMessage;
                int start = this._linkUrlMessage.LastIndexOf(str, StringComparison.Ordinal);
                this.errorLink.Links.Add(start, str.Length, this._linkUrl);
            }
            else
            {
                this.errorLink.Text = string.Empty;
                this.errorLink.Links.Clear();
            }
            if (this._logFileLocation == null)
            {
                this.btnSupport.Enabled = false;
            }
        }
    }
}

