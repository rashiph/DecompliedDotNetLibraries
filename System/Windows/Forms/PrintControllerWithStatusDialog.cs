namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.Security.Permissions;
    using System.Threading;

    public class PrintControllerWithStatusDialog : PrintController
    {
        private BackgroundThread backgroundThread;
        private string dialogTitle;
        private PrintDocument document;
        private int pageNumber;
        private PrintController underlyingController;

        public PrintControllerWithStatusDialog(PrintController underlyingController) : this(underlyingController, System.Windows.Forms.SR.GetString("PrintControllerWithStatusDialog_DialogTitlePrint"))
        {
        }

        public PrintControllerWithStatusDialog(PrintController underlyingController, string dialogTitle)
        {
            this.underlyingController = underlyingController;
            this.dialogTitle = dialogTitle;
        }

        public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
        {
            this.underlyingController.OnEndPage(document, e);
            if ((this.backgroundThread != null) && this.backgroundThread.canceled)
            {
                e.Cancel = true;
            }
            this.pageNumber++;
            base.OnEndPage(document, e);
        }

        public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
        {
            this.underlyingController.OnEndPrint(document, e);
            if ((this.backgroundThread != null) && this.backgroundThread.canceled)
            {
                e.Cancel = true;
            }
            if (this.backgroundThread != null)
            {
                this.backgroundThread.Stop();
            }
            base.OnEndPrint(document, e);
        }

        public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
        {
            base.OnStartPage(document, e);
            if (this.backgroundThread != null)
            {
                this.backgroundThread.UpdateLabel();
            }
            Graphics graphics = this.underlyingController.OnStartPage(document, e);
            if ((this.backgroundThread != null) && this.backgroundThread.canceled)
            {
                e.Cancel = true;
            }
            return graphics;
        }

        public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
        {
            base.OnStartPrint(document, e);
            this.document = document;
            this.pageNumber = 1;
            if (SystemInformation.UserInteractive)
            {
                this.backgroundThread = new BackgroundThread(this);
            }
            try
            {
                this.underlyingController.OnStartPrint(document, e);
            }
            catch
            {
                if (this.backgroundThread != null)
                {
                    this.backgroundThread.Stop();
                }
                throw;
            }
            finally
            {
                if ((this.backgroundThread != null) && this.backgroundThread.canceled)
                {
                    e.Cancel = true;
                }
            }
        }

        public override bool IsPreview
        {
            get
            {
                return ((this.underlyingController != null) && this.underlyingController.IsPreview);
            }
        }

        private class BackgroundThread
        {
            private bool alreadyStopped;
            internal bool canceled;
            private PrintControllerWithStatusDialog.StatusDialog dialog;
            private PrintControllerWithStatusDialog parent;
            private Thread thread;

            internal BackgroundThread(PrintControllerWithStatusDialog parent)
            {
                this.parent = parent;
                this.thread = new Thread(new ThreadStart(this.Run));
                this.thread.SetApartmentState(ApartmentState.STA);
                this.thread.Start();
            }

            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), UIPermission(SecurityAction.Assert, Window=UIPermissionWindow.AllWindows)]
            private void Run()
            {
                try
                {
                    lock (this)
                    {
                        if (this.alreadyStopped)
                        {
                            return;
                        }
                        this.dialog = new PrintControllerWithStatusDialog.StatusDialog(this, this.parent.dialogTitle);
                        this.ThreadUnsafeUpdateLabel();
                        this.dialog.Visible = true;
                    }
                    if (!this.alreadyStopped)
                    {
                        Application.Run(this.dialog);
                    }
                }
                finally
                {
                    lock (this)
                    {
                        if (this.dialog != null)
                        {
                            this.dialog.Dispose();
                            this.dialog = null;
                        }
                    }
                }
            }

            internal void Stop()
            {
                lock (this)
                {
                    if ((this.dialog != null) && this.dialog.IsHandleCreated)
                    {
                        this.dialog.BeginInvoke(new MethodInvoker(this.dialog.Close));
                    }
                    else
                    {
                        this.alreadyStopped = true;
                    }
                }
            }

            private void ThreadUnsafeUpdateLabel()
            {
                this.dialog.label1.Text = System.Windows.Forms.SR.GetString("PrintControllerWithStatusDialog_NowPrinting", new object[] { this.parent.pageNumber, this.parent.document.DocumentName });
            }

            internal void UpdateLabel()
            {
                if ((this.dialog != null) && this.dialog.IsHandleCreated)
                {
                    this.dialog.BeginInvoke(new MethodInvoker(this.ThreadUnsafeUpdateLabel));
                }
            }
        }

        private class StatusDialog : Form
        {
            private PrintControllerWithStatusDialog.BackgroundThread backgroundThread;
            private Button button1;
            internal Label label1;

            internal StatusDialog(PrintControllerWithStatusDialog.BackgroundThread backgroundThread, string dialogTitle)
            {
                this.InitializeComponent();
                this.backgroundThread = backgroundThread;
                this.Text = dialogTitle;
                this.MinimumSize = base.Size;
            }

            private void button1_Click(object sender, EventArgs e)
            {
                this.button1.Enabled = false;
                this.label1.Text = System.Windows.Forms.SR.GetString("PrintControllerWithStatusDialog_Canceling");
                this.backgroundThread.canceled = true;
            }

            private void InitializeComponent()
            {
                if (IsRTLResources)
                {
                    this.RightToLeft = RightToLeft.Yes;
                }
                this.label1 = new Label();
                this.button1 = new Button();
                this.label1.Location = new Point(8, 0x10);
                this.label1.TextAlign = ContentAlignment.MiddleCenter;
                this.label1.Size = new Size(240, 0x40);
                this.label1.TabIndex = 1;
                this.label1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this.button1.Size = new Size(0x4b, 0x17);
                this.button1.TabIndex = 0;
                this.button1.Text = System.Windows.Forms.SR.GetString("PrintControllerWithStatusDialog_Cancel");
                this.button1.Location = new Point(0x58, 0x58);
                this.button1.Anchor = AnchorStyles.Bottom;
                this.button1.Click += new EventHandler(this.button1_Click);
                base.AutoScaleDimensions = (SizeF) new Size(6, 13);
                base.AutoScaleMode = AutoScaleMode.Font;
                base.MaximizeBox = false;
                base.ControlBox = false;
                base.MinimizeBox = false;
                base.ClientSize = new Size(0x100, 0x7a);
                base.CancelButton = this.button1;
                base.SizeGripStyle = SizeGripStyle.Hide;
                base.Controls.Add(this.label1);
                base.Controls.Add(this.button1);
            }

            private static bool IsRTLResources
            {
                get
                {
                    return (System.Windows.Forms.SR.GetString("RTL") != "RTL_False");
                }
            }
        }
    }
}

