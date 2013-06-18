namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class UserInterfaceForm : Form
    {
        private FormPiece currentPiece;
        private ManualResetEvent onLoadEvent;
        private SplashInfo splashPieceInfo;

        public UserInterfaceForm(ManualResetEvent readyEvent, SplashInfo splashInfo)
        {
            this.onLoadEvent = readyEvent;
            this.splashPieceInfo = splashInfo;
            base.SuspendLayout();
            this.InitializeComponent();
            this.InitializeContent();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public ErrorPiece ConstructErrorPiece(string title, string message, string logFileLocation, string linkUrl, string linkUrlMessage, ManualResetEvent modalEvent)
        {
            return new ErrorPiece(this, title, message, logFileLocation, linkUrl, linkUrlMessage, modalEvent);
        }

        public MaintenancePiece ConstructMaintenancePiece(UserInterfaceInfo info, MaintenanceInfo maintenanceInfo, ManualResetEvent modalEvent)
        {
            return new MaintenancePiece(this, info, maintenanceInfo, modalEvent);
        }

        public PlatformPiece ConstructPlatformPiece(string platformDetectionErrorMsg, Uri supportUrl, ManualResetEvent modalEvent)
        {
            return new PlatformPiece(this, platformDetectionErrorMsg, supportUrl, modalEvent);
        }

        public ProgressPiece ConstructProgressPiece(UserInterfaceInfo info)
        {
            return new ProgressPiece(this, info);
        }

        public UpdatePiece ConstructUpdatePiece(UserInterfaceInfo info, ManualResetEvent modalEvent)
        {
            return new UpdatePiece(this, info, modalEvent);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                base.Icon.Dispose();
                base.Icon = null;
                if (this.currentPiece != null)
                {
                    this.currentPiece.Dispose();
                }
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(UserInterfaceForm));
            base.SuspendLayout();
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ControlBox = false;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "UserInterfaceForm";
            base.ShowIcon = false;
            base.ResumeLayout(false);
        }

        private void InitializeContent()
        {
            base.Icon = Resources.GetIcon("form.ico");
            this.Font = SystemFonts.MessageBoxFont;
            this.currentPiece = new SplashPiece(this, this.splashPieceInfo);
            base.Controls.Add(this.currentPiece);
        }

        private bool IsRightToLeft(Control control)
        {
            if (control.RightToLeft == RightToLeft.Yes)
            {
                return true;
            }
            if (control.RightToLeft == RightToLeft.No)
            {
                return false;
            }
            return (((control.RightToLeft == RightToLeft.Inherit) && (control.Parent != null)) && this.IsRightToLeft(control.Parent));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!this.currentPiece.OnClosing())
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = true;
                base.Hide();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.onLoadEvent.Set();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible && (Form.ActiveForm != this))
            {
                base.Activate();
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (this.splashPieceInfo.initializedAsWait)
            {
                base.SetVisibleCore(false);
            }
            else
            {
                base.SetVisibleCore(value);
            }
        }

        public void ShowSimpleMessageBox(string message, string caption)
        {
            MessageBoxOptions options = 0;
            if (this.IsRightToLeft(this))
            {
                options |= MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
            }
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, options);
        }

        public void SwitchUserInterfacePiece(FormPiece piece)
        {
            FormPiece currentPiece = null;
            currentPiece = this.currentPiece;
            this.currentPiece = piece;
            this.currentPiece.Dock = DockStyle.Fill;
            base.SuspendLayout();
            base.Controls.Add(this.currentPiece);
            if (currentPiece != null)
            {
                base.Controls.Remove(currentPiece);
                currentPiece.Dispose();
            }
            base.ClientSize = this.currentPiece.ClientSize;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public delegate ErrorPiece ConstructErrorPieceDelegate(string title, string message, string logFileLocation, string linkUrl, string linkUrlMessage, ManualResetEvent modalEvent);

        public delegate MaintenancePiece ConstructMaintenancePieceDelegate(UserInterfaceInfo info, MaintenanceInfo maintenanceInfo, ManualResetEvent modalEvent);

        public delegate PlatformPiece ConstructPlatformPieceDelegate(string platformDetectionErrorMsg, Uri supportUrl, ManualResetEvent modalEvent);

        public delegate ProgressPiece ConstructProgressPieceDelegate(UserInterfaceInfo info);

        public delegate UpdatePiece ConstructUpdatePieceDelegate(UserInterfaceInfo info, ManualResetEvent modalEvent);

        public delegate void ShowSimpleMessageBoxDelegate(string messsage, string caption);
    }
}

