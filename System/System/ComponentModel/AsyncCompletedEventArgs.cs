namespace System.ComponentModel
{
    using System;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class AsyncCompletedEventArgs : EventArgs
    {
        private readonly bool cancelled;
        private readonly Exception error;
        private readonly object userState;

        public AsyncCompletedEventArgs(Exception error, bool cancelled, object userState)
        {
            this.error = error;
            this.cancelled = cancelled;
            this.userState = userState;
        }

        protected void RaiseExceptionIfNecessary()
        {
            if (this.Error != null)
            {
                throw new TargetInvocationException(SR.GetString("Async_ExceptionOccurred"), this.Error);
            }
            if (this.Cancelled)
            {
                throw new InvalidOperationException(SR.GetString("Async_OperationCancelled"));
            }
        }

        [SRDescription("Async_AsyncEventArgs_Cancelled")]
        public bool Cancelled
        {
            get
            {
                return this.cancelled;
            }
        }

        [SRDescription("Async_AsyncEventArgs_Error")]
        public Exception Error
        {
            get
            {
                return this.error;
            }
        }

        [SRDescription("Async_AsyncEventArgs_UserState")]
        public object UserState
        {
            get
            {
                return this.userState;
            }
        }
    }
}

