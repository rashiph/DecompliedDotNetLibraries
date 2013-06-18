namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;

    public class DownloadFileGroupCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly string _groupName;

        internal DownloadFileGroupCompletedEventArgs(Exception error, bool cancelled, object userState, string groupName) : base(error, cancelled, userState)
        {
            this._groupName = groupName;
        }

        public string Group
        {
            get
            {
                return this._groupName;
            }
        }
    }
}

