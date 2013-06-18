namespace System.Web.Services.Protocols
{
    using System;
    using System.Threading;

    internal class CompletedAsyncResult : IAsyncResult
    {
        private object asyncState;
        private bool completedSynchronously;

        internal CompletedAsyncResult(object asyncState, bool completedSynchronously)
        {
            this.asyncState = asyncState;
            this.completedSynchronously = completedSynchronously;
        }

        public object AsyncState
        {
            get
            {
                return this.asyncState;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return null;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return this.completedSynchronously;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return true;
            }
        }
    }
}

