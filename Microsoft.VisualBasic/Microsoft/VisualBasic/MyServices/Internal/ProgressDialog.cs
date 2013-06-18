namespace Microsoft.VisualBasic.MyServices.Internal
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;
    using System.Windows.Forms;

    internal class ProgressDialog : Form
    {
        [AccessedThroughProperty("ButtonCloseDialog")]
        private Button _ButtonCloseDialog;
        [AccessedThroughProperty("LabelInfo")]
        private Label _LabelInfo;
        [AccessedThroughProperty("ProgressBarWork")]
        private ProgressBar _ProgressBarWork;
        private const int BORDER_SIZE = 20;
        private IContainer components;
        private bool m_Canceled;
        private bool m_CloseDialogInvoked;
        private bool m_Closing;
        private ManualResetEvent m_FormClosableSemaphore;
        private const int WS_THICKFRAME = 0x40000;

        public event UserHitCancelEventHandler UserHitCancel;

        internal ProgressDialog()
        {
            base.Shown += new EventHandler(this.ProgressDialog_Activated);
            base.FormClosing += new FormClosingEventHandler(this.ProgressDialog_FormClosing);
            base.Resize += new EventHandler(this.ProgressDialog_Resize);
            this.m_Canceled = false;
            this.m_FormClosableSemaphore = new ManualResetEvent(false);
            this.InitializeComponent();
        }

        private void ButtonCloseDialog_Click(object sender, EventArgs e)
        {
            this.ButtonCloseDialog.Enabled = false;
            this.m_Canceled = true;
            UserHitCancelEventHandler userHitCancelEvent = this.UserHitCancelEvent;
            if (userHitCancelEvent != null)
            {
                userHitCancelEvent();
            }
        }

        public void CloseDialog()
        {
            this.m_CloseDialogInvoked = true;
            this.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }
                if (this.m_FormClosableSemaphore != null)
                {
                    this.m_FormClosableSemaphore.Dispose();
                    this.m_FormClosableSemaphore = null;
                }
            }
            base.Dispose(disposing);
        }

        public void Increment(int incrementAmount)
        {
            this.ProgressBarWork.Increment(incrementAmount);
        }

        public void IndicateClosing()
        {
            this.m_Closing = true;
        }

        [DebuggerStepThrough]
        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ProgressDialog));
            this.LabelInfo = new Label();
            this.ProgressBarWork = new ProgressBar();
            this.ButtonCloseDialog = new Button();
            this.SuspendLayout();
            manager.ApplyResources(this.LabelInfo, "LabelInfo", CultureInfo.CurrentUICulture);
            Size size2 = new Size(300, 0);
            this.LabelInfo.MaximumSize = size2;
            this.LabelInfo.Name = "LabelInfo";
            manager.ApplyResources(this.ProgressBarWork, "ProgressBarWork", CultureInfo.CurrentUICulture);
            this.ProgressBarWork.Name = "ProgressBarWork";
            manager.ApplyResources(this.ButtonCloseDialog, "ButtonCloseDialog", CultureInfo.CurrentUICulture);
            this.ButtonCloseDialog.Name = "ButtonCloseDialog";
            manager.ApplyResources(this, "$this", CultureInfo.CurrentUICulture);
            this.Controls.Add(this.ButtonCloseDialog);
            this.Controls.Add(this.ProgressBarWork);
            this.Controls.Add(this.LabelInfo);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDialog";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ProgressDialog_Activated(object sender, EventArgs e)
        {
            this.m_FormClosableSemaphore.Set();
        }

        private void ProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (((e.CloseReason == CloseReason.UserClosing) & !this.m_CloseDialogInvoked) && ((this.ProgressBarWork.Value < 100) & !this.m_Canceled))
            {
                e.Cancel = true;
                this.m_Canceled = true;
                UserHitCancelEventHandler userHitCancelEvent = this.UserHitCancelEvent;
                if (userHitCancelEvent != null)
                {
                    userHitCancelEvent();
                }
            }
        }

        private void ProgressDialog_Resize(object sender, EventArgs e)
        {
            Size size3 = new Size(this.ClientSize.Width - 20, 0);
            this.LabelInfo.MaximumSize = size3;
        }

        public void ShowProgressDialog()
        {
            try
            {
                if (!this.m_Closing)
                {
                    this.ShowDialog();
                }
            }
            finally
            {
                this.FormClosableSemaphore.Set();
            }
        }

        internal virtual Button ButtonCloseDialog
        {
            get
            {
                return this._ButtonCloseDialog;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                EventHandler handler = new EventHandler(this.ButtonCloseDialog_Click);
                if (this._ButtonCloseDialog != null)
                {
                    this._ButtonCloseDialog.Click -= handler;
                }
                this._ButtonCloseDialog = value;
                if (this._ButtonCloseDialog != null)
                {
                    this._ButtonCloseDialog.Click += handler;
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecuritySafeCritical]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.Style |= 0x40000;
                return createParams;
            }
        }

        public ManualResetEvent FormClosableSemaphore
        {
            get
            {
                return this.m_FormClosableSemaphore;
            }
        }

        internal virtual Label LabelInfo
        {
            get
            {
                return this._LabelInfo;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this._LabelInfo = value;
            }
        }

        public string LabelText
        {
            get
            {
                return this.LabelInfo.Text;
            }
            set
            {
                this.LabelInfo.Text = value;
            }
        }

        internal virtual ProgressBar ProgressBarWork
        {
            get
            {
                return this._ProgressBarWork;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this._ProgressBarWork = value;
            }
        }

        public bool UserCanceledTheDialog
        {
            get
            {
                return this.m_Canceled;
            }
        }

        public delegate void UserHitCancelEventHandler();
    }
}

