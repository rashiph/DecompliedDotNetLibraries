namespace System.DirectoryServices.Protocols
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Threading;

    internal class DsmlAsyncResult : IAsyncResult
    {
        private DsmlAsyncWaitHandle asyncWaitHandle;
        internal AsyncCallback callback;
        internal bool completed;
        private bool completedSynchronously;
        internal bool hasValidRequest;
        internal ManualResetEvent manualResetEvent;
        internal RequestState resultObject;
        private object stateObject;

        public DsmlAsyncResult(AsyncCallback callbackRoutine, object state)
        {
            this.stateObject = state;
            this.callback = callbackRoutine;
            this.manualResetEvent = new ManualResetEvent(false);
        }

        public override bool Equals(object o)
        {
            return (((o is DsmlAsyncResult) && (o != null)) && (this == ((DsmlAsyncResult) o)));
        }

        public override int GetHashCode()
        {
            return this.manualResetEvent.GetHashCode();
        }

        object IAsyncResult.AsyncState
        {
            get
            {
                return this.stateObject;
            }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get
            {
                if (this.asyncWaitHandle == null)
                {
                    this.asyncWaitHandle = new DsmlAsyncWaitHandle(this.manualResetEvent.SafeWaitHandle);
                }
                return this.asyncWaitHandle;
            }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get
            {
                return this.completedSynchronously;
            }
        }

        bool IAsyncResult.IsCompleted
        {
            get
            {
                return this.completed;
            }
        }

        internal sealed class DsmlAsyncWaitHandle : WaitHandle
        {
            public DsmlAsyncWaitHandle(SafeWaitHandle handle)
            {
                base.SafeWaitHandle = handle;
            }

            ~DsmlAsyncWaitHandle()
            {
                base.SafeWaitHandle = null;
            }
        }
    }
}

