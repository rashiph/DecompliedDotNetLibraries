namespace System.DirectoryServices.Protocols
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Threading;

    internal class LdapAsyncResult : IAsyncResult
    {
        private LdapAsyncWaitHandle asyncWaitHandle;
        internal AsyncCallback callback;
        internal bool completed;
        private bool completedSynchronously;
        internal ManualResetEvent manualResetEvent;
        internal bool partialResults;
        internal LdapRequestState resultObject;
        private object stateObject;

        public LdapAsyncResult(AsyncCallback callbackRoutine, object state, bool partialResults)
        {
            this.stateObject = state;
            this.callback = callbackRoutine;
            this.manualResetEvent = new ManualResetEvent(false);
            this.partialResults = partialResults;
        }

        public override bool Equals(object o)
        {
            return (((o is LdapAsyncResult) && (o != null)) && (this == ((LdapAsyncResult) o)));
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
                    this.asyncWaitHandle = new LdapAsyncWaitHandle(this.manualResetEvent.SafeWaitHandle);
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

        internal sealed class LdapAsyncWaitHandle : WaitHandle
        {
            public LdapAsyncWaitHandle(SafeWaitHandle handle)
            {
                base.SafeWaitHandle = handle;
            }

            ~LdapAsyncWaitHandle()
            {
                base.SafeWaitHandle = null;
            }
        }
    }
}

