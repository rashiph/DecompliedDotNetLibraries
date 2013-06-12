namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ProgressChangedEventArgs : EventArgs
    {
        private readonly int progressPercentage;
        private readonly object userState;

        public ProgressChangedEventArgs(int progressPercentage, object userState)
        {
            this.progressPercentage = progressPercentage;
            this.userState = userState;
        }

        [SRDescription("Async_ProgressChangedEventArgs_ProgressPercentage")]
        public int ProgressPercentage
        {
            get
            {
                return this.progressPercentage;
            }
        }

        [SRDescription("Async_ProgressChangedEventArgs_UserState")]
        public object UserState
        {
            get
            {
                return this.userState;
            }
        }
    }
}

