namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class PlatformPiece : ModalPiece
    {
        private string _errorMessage;
        private Uri _supportUrl;
        private Button btnOk;
        private Label lblMessage;
        private LinkLabel linkSupport;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private PictureBox pictureIcon;

        public PlatformPiece(UserInterfaceForm parentForm, string platformDetectionErrorMsg, Uri supportUrl, ManualResetEvent modalEvent)
        {
            this._errorMessage = platformDetectionErrorMsg;
            this._supportUrl = supportUrl;
            base._modalResult = UserInterfaceModalResult.Ok;
            base._modalEvent = modalEvent;
            base.SuspendLayout();
            this.InitializeComponent();
            this.InitializeContent();
            base.ResumeLayout(false);
            parentForm.SuspendLayout();
            parentForm.SwitchUserInterfacePiece(this);
            parentForm.Text = Resources.GetString("UI_PlatformDetectionFailedTitle");
            parentForm.MinimizeBox = false;
            parentForm.MaximizeBox = false;
            parentForm.ControlBox = true;
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

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(PlatformPiece));
            this.lblMessage = new Label();
            this.pictureIcon = new PictureBox();
            this.btnOk = new Button();
            this.linkSupport = new LinkLabel();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            ((ISupportInitialize) this.pictureIcon).BeginInit();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.lblMessage, "lblMessage");
            this.lblMessage.Name = "lblMessage";
            manager.ApplyResources(this.pictureIcon, "pictureIcon");
            this.pictureIcon.Name = "pictureIcon";
            this.pictureIcon.TabStop = false;
            manager.ApplyResources(this.btnOk, "btnOk");
            this.overarchingTableLayoutPanel.SetColumnSpan(this.btnOk, 2);
            this.btnOk.MinimumSize = new Size(0x4b, 0x17);
            this.btnOk.Name = "btnOk";
            this.btnOk.Click += new EventHandler(this.btnOk_Click);
            manager.ApplyResources(this.linkSupport, "linkSupport");
            this.linkSupport.Name = "linkSupport";
            this.linkSupport.TabStop = true;
            this.linkSupport.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkSupport_LinkClicked);
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.Controls.Add(this.pictureIcon, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.btnOk, 0, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.linkSupport, 1, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.lblMessage, 1, 0);
            this.overarchingTableLayoutPanel.MinimumSize = new Size(0x15d, 0x58);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            manager.ApplyResources(this, "$this");
            base.Controls.Add(this.overarchingTableLayoutPanel);
            this.MinimumSize = new Size(0x175, 0x70);
            base.Name = "PlatformPiece";
            ((ISupportInitialize) this.pictureIcon).EndInit();
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
            this.linkSupport.Links.Clear();
            if (this._supportUrl == null)
            {
                this.linkSupport.Text = Resources.GetString("UI_PlatformContactAdmin");
            }
            else
            {
                string str = Resources.GetString("UI_PlatformClickHere");
                string str2 = Resources.GetString("UI_PlatformClickHereHere");
                int start = str.LastIndexOf(str2, StringComparison.Ordinal);
                this.linkSupport.Text = str;
                this.linkSupport.Links.Add(start, str2.Length, this._supportUrl.AbsoluteUri);
            }
            this.lblMessage.Text = this._errorMessage;
        }

        private void linkSupport_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkSupport.Links[this.linkSupport.Links.IndexOf(e.Link)].Visited = true;
            if ((this._supportUrl != null) && UserInterface.IsValidHttpUrl(this._supportUrl.AbsoluteUri))
            {
                UserInterface.LaunchUrlInBrowser(e.Link.LinkData.ToString());
            }
        }
    }
}

