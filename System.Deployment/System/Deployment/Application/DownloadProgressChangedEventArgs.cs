namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;

    public class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        private long _bytesCompleted;
        private long _bytesTotal;
        private DeploymentProgressState _deploymentProgressState;

        internal DownloadProgressChangedEventArgs(int progressPercentage, object userState, long bytesCompleted, long bytesTotal, DeploymentProgressState downloadProgressState) : base(progressPercentage, userState)
        {
            this._bytesCompleted = bytesCompleted;
            this._bytesTotal = bytesTotal;
            this._deploymentProgressState = downloadProgressState;
        }

        public long BytesDownloaded
        {
            get
            {
                return this._bytesCompleted;
            }
        }

        public DeploymentProgressState State
        {
            get
            {
                return this._deploymentProgressState;
            }
        }

        public long TotalBytesToDownload
        {
            get
            {
                return this._bytesTotal;
            }
        }
    }
}

