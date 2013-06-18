namespace System.Deployment.Application
{
    using System;

    internal interface IDownloadNotification
    {
        void DownloadCompleted(object sender, DownloadEventArgs e);
        void DownloadModified(object sender, DownloadEventArgs e);
    }
}

