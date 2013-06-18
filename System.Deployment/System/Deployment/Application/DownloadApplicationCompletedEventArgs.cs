namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;

    public class DownloadApplicationCompletedEventArgs : AsyncCompletedEventArgs
    {
        private string _logFilePath;
        private string _shortcutAppId;

        internal DownloadApplicationCompletedEventArgs(AsyncCompletedEventArgs e, string logFilePath, string shortcutAppId) : base(e.Error, e.Cancelled, e.UserState)
        {
            this._logFilePath = logFilePath;
            this._shortcutAppId = shortcutAppId;
        }

        public string LogFilePath
        {
            get
            {
                return this._logFilePath;
            }
        }

        public string ShortcutAppId
        {
            get
            {
                return this._shortcutAppId;
            }
        }
    }
}

