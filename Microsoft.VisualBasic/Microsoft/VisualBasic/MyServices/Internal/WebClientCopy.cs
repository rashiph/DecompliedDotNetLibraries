namespace Microsoft.VisualBasic.MyServices.Internal
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class WebClientCopy
    {
        [AccessedThroughProperty("m_ProgressDialog")]
        private ProgressDialog _m_ProgressDialog;
        [AccessedThroughProperty("m_WebClient")]
        private WebClient _m_WebClient;
        private Exception m_ExceptionEncounteredDuringFileTransfer;
        private int m_Percentage = 0;

        public WebClientCopy(WebClient client, ProgressDialog dialog)
        {
            this.m_WebClient = client;
            this.m_ProgressDialog = dialog;
        }

        private void CloseProgressDialog()
        {
            if (this.m_ProgressDialog != null)
            {
                this.m_ProgressDialog.IndicateClosing();
                if (this.m_ProgressDialog.IsHandleCreated)
                {
                    ProgressDialog progressDialog = this.m_ProgressDialog;
                    this.m_ProgressDialog.BeginInvoke(new MethodInvoker(progressDialog.CloseDialog));
                }
                else
                {
                    this.m_ProgressDialog.Close();
                }
            }
        }

        public void DownloadFile(Uri address, string destinationFileName)
        {
            if (this.m_ProgressDialog != null)
            {
                this.m_WebClient.DownloadFileAsync(address, destinationFileName);
                this.m_ProgressDialog.ShowProgressDialog();
            }
            else
            {
                this.m_WebClient.DownloadFile(address, destinationFileName);
            }
            if ((this.m_ExceptionEncounteredDuringFileTransfer != null) && ((this.m_ProgressDialog == null) || !this.m_ProgressDialog.UserCanceledTheDialog))
            {
                throw this.m_ExceptionEncounteredDuringFileTransfer;
            }
        }

        private void InvokeIncrement(int progressPercentage)
        {
            if ((this.m_ProgressDialog != null) && this.m_ProgressDialog.IsHandleCreated)
            {
                int num = progressPercentage - this.m_Percentage;
                this.m_Percentage = progressPercentage;
                if (num > 0)
                {
                    ProgressDialog progressDialog = this.m_ProgressDialog;
                    this.m_ProgressDialog.BeginInvoke(new DoIncrement(progressDialog.Increment), new object[] { num });
                }
            }
        }

        private void m_ProgressDialog_UserCancelledEvent()
        {
            this.m_WebClient.CancelAsync();
        }

        private void m_WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    this.m_ExceptionEncounteredDuringFileTransfer = e.Error;
                }
                if (!e.Cancelled && (e.Error == null))
                {
                    this.InvokeIncrement(100);
                }
            }
            finally
            {
                this.CloseProgressDialog();
            }
        }

        private void m_WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.InvokeIncrement(e.ProgressPercentage);
        }

        private void m_WebClient_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    this.m_ExceptionEncounteredDuringFileTransfer = e.Error;
                }
                if (!e.Cancelled && (e.Error == null))
                {
                    this.InvokeIncrement(100);
                }
            }
            finally
            {
                this.CloseProgressDialog();
            }
        }

        private void m_WebClient_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            long num = (e.BytesSent * 100L) / e.TotalBytesToSend;
            this.InvokeIncrement((int) num);
        }

        public void UploadFile(string sourceFileName, Uri address)
        {
            if (this.m_ProgressDialog != null)
            {
                this.m_WebClient.UploadFileAsync(address, sourceFileName);
                this.m_ProgressDialog.ShowProgressDialog();
            }
            else
            {
                this.m_WebClient.UploadFile(address, sourceFileName);
            }
            if ((this.m_ExceptionEncounteredDuringFileTransfer != null) && ((this.m_ProgressDialog == null) || !this.m_ProgressDialog.UserCanceledTheDialog))
            {
                throw this.m_ExceptionEncounteredDuringFileTransfer;
            }
        }

        private ProgressDialog m_ProgressDialog
        {
            get
            {
                return this._m_ProgressDialog;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                ProgressDialog.UserHitCancelEventHandler handler = new ProgressDialog.UserHitCancelEventHandler(this.m_ProgressDialog_UserCancelledEvent);
                if (this._m_ProgressDialog != null)
                {
                    this._m_ProgressDialog.UserHitCancel -= handler;
                }
                this._m_ProgressDialog = value;
                if (this._m_ProgressDialog != null)
                {
                    this._m_ProgressDialog.UserHitCancel += handler;
                }
            }
        }

        private WebClient m_WebClient
        {
            get
            {
                return this._m_WebClient;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                DownloadProgressChangedEventHandler handler = new DownloadProgressChangedEventHandler(this.m_WebClient_DownloadProgressChanged);
                AsyncCompletedEventHandler handler2 = new AsyncCompletedEventHandler(this.m_WebClient_DownloadFileCompleted);
                UploadProgressChangedEventHandler handler3 = new UploadProgressChangedEventHandler(this.m_WebClient_UploadProgressChanged);
                UploadFileCompletedEventHandler handler4 = new UploadFileCompletedEventHandler(this.m_WebClient_UploadFileCompleted);
                if (this._m_WebClient != null)
                {
                    this._m_WebClient.DownloadProgressChanged -= handler;
                    this._m_WebClient.DownloadFileCompleted -= handler2;
                    this._m_WebClient.UploadProgressChanged -= handler3;
                    this._m_WebClient.UploadFileCompleted -= handler4;
                }
                this._m_WebClient = value;
                if (this._m_WebClient != null)
                {
                    this._m_WebClient.DownloadProgressChanged += handler;
                    this._m_WebClient.DownloadFileCompleted += handler2;
                    this._m_WebClient.UploadProgressChanged += handler3;
                    this._m_WebClient.UploadFileCompleted += handler4;
                }
            }
        }

        private delegate void DoIncrement(int Increment);
    }
}

