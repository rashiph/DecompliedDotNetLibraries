namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Threading;

    internal class PipeAsyncResult : IAsyncResult
    {
        internal int _errorCode;
        internal int _numBytes;
        internal unsafe NativeOverlapped* _overlapped;
        internal AsyncCallback _userCallback;

        internal PipeAsyncResult(AsyncCallback callback)
        {
            this._userCallback = callback;
        }

        internal void CallUserCallback()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.CallUserCallbackWorker));
        }

        private void CallUserCallbackWorker(object callbackState)
        {
            this._userCallback(this);
        }

        public object AsyncState
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        public bool IsCompleted
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

