namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class SplashPiece : FormPiece
    {
        private SplashInfo info;
        private const int initialDelay = 0x9c4;
        private Label lblNote;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private PictureBox pictureWait;
        private const int showDelay = 0x3e8;
        private Timer splashTimer;

        public SplashPiece(UserInterfaceForm parentForm, SplashInfo info)
        {
            this.info = info;
            base.SuspendLayout();
            this.InitializeComponent();
            this.InitializeContent();
            base.ResumeLayout(false);
            parentForm.SuspendLayout();
            parentForm.Text = Resources.GetString("UI_SplashTitle");
            parentForm.MinimizeBox = false;
            parentForm.MaximizeBox = false;
            parentForm.ControlBox = true;
            parentForm.ResumeLayout(false);
            this.splashTimer = new Timer();
            this.splashTimer.Tick += new EventHandler(this.SplashTimer_Tick);
            if (info.initializedAsWait)
            {
                this.splashTimer.Interval = 0x9c4;
                this.splashTimer.Tag = null;
                this.splashTimer.Enabled = true;
            }
            else
            {
                this.ShowSplash(parentForm);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.End();
            }
        }

        private void End()
        {
            this.info.initializedAsWait = false;
            this.splashTimer.Tag = this;
            this.splashTimer.Dispose();
            this.info.pieceReady.Set();
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(SplashPiece));
            this.pictureWait = new PictureBox();
            this.lblNote = new Label();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            ((ISupportInitialize) this.pictureWait).BeginInit();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.pictureWait, "pictureWait");
            this.pictureWait.Name = "pictureWait";
            this.pictureWait.TabStop = false;
            manager.ApplyResources(this.lblNote, "lblNote");
            this.lblNote.Name = "lblNote";
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.Controls.Add(this.pictureWait, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.lblNote, 0, 1);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            manager.ApplyResources(this, "$this");
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.Name = "SplashPiece";
            ((ISupportInitialize) this.pictureWait).EndInit();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeContent()
        {
            this.pictureWait.Image = Resources.GetImage("splash.gif");
        }

        public override bool OnClosing()
        {
            bool flag = base.OnClosing();
            this.info.cancelled = true;
            this.End();
            return flag;
        }

        private void ShowSplash(Form parentForm)
        {
            this.info.initializedAsWait = false;
            parentForm.Visible = true;
            this.splashTimer.Interval = 0x3e8;
            this.splashTimer.Tag = this;
            this.splashTimer.Enabled = true;
            this.info.pieceReady.Reset();
        }

        private void SplashTimer_Tick(object sender, EventArgs e)
        {
            if (this.splashTimer.Enabled)
            {
                this.splashTimer.Enabled = false;
                if (this.splashTimer.Tag != null)
                {
                    this.info.pieceReady.Set();
                }
                else
                {
                    this.ShowSplash(base.FindForm());
                }
            }
        }
    }
}

