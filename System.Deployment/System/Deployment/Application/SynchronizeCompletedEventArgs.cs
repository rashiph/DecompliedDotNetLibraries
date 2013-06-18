namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;

    internal class SynchronizeCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly string _groupName;

        internal SynchronizeCompletedEventArgs(Exception error, bool cancelled, object userState, string groupName) : base(error, cancelled, userState)
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

