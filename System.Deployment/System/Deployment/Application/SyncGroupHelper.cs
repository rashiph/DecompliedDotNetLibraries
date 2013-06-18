namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    internal class SyncGroupHelper : IDownloadNotification
    {
        private bool _cancellationPending;
        private readonly AsyncOperation asyncOperation;
        private readonly string groupName;
        private readonly SendOrPostCallback progressReporter;
        private readonly object userState;

        public SyncGroupHelper(string groupName, object userState, AsyncOperation asyncOp, SendOrPostCallback progressReporterDelegate)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException("groupName");
            }
            this.groupName = groupName;
            this.userState = userState;
            this.asyncOperation = asyncOp;
            this.progressReporter = progressReporterDelegate;
        }

        public void CancelAsync()
        {
            this._cancellationPending = true;
        }

        public void DownloadCompleted(object sender, DownloadEventArgs e)
        {
        }

        public void DownloadModified(object sender, DownloadEventArgs e)
        {
            if (this._cancellationPending)
            {
                ((FileDownloader) sender).Cancel();
            }
            this.asyncOperation.Post(this.progressReporter, new DeploymentProgressChangedEventArgs(e.Progress, this.userState, e.BytesCompleted, e.BytesTotal, DeploymentProgressState.DownloadingApplicationFiles, this.groupName));
        }

        public void SetComplete()
        {
        }

        public bool CancellationPending
        {
            get
            {
                return this._cancellationPending;
            }
        }

        public string Group
        {
            get
            {
                return this.groupName;
            }
        }

        public object UserState
        {
            get
            {
                return this.userState;
            }
        }
    }
}

