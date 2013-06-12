namespace System.Net
{
    using System;
    using System.Threading;

    internal class AsyncProtocolRequest
    {
        private AsyncProtocolCallback _Callback;
        private int _CompletionStatus;
        public object AsyncState;
        public byte[] Buffer;
        public int Count;
        public int Offset;
        public int Result;
        private const int StatusCheckedOnSyncCompletion = 2;
        private const int StatusCompleted = 1;
        private const int StatusNotStarted = 0;
        public LazyAsyncResult UserAsyncResult;

        public AsyncProtocolRequest(LazyAsyncResult userAsyncResult)
        {
            this.UserAsyncResult = userAsyncResult;
        }

        internal void CompleteRequest(int result)
        {
            this.Result = result;
            switch (Interlocked.Exchange(ref this._CompletionStatus, 1))
            {
                case 1:
                    throw new InternalException();

                case 2:
                    this._CompletionStatus = 0;
                    this._Callback(this);
                    break;
            }
        }

        internal void CompleteUser()
        {
            this.UserAsyncResult.InvokeCallback();
        }

        internal void CompleteUser(object userResult)
        {
            this.UserAsyncResult.InvokeCallback(userResult);
        }

        internal void CompleteWithError(Exception e)
        {
            this.UserAsyncResult.InvokeCallback(e);
        }

        public void SetNextRequest(byte[] buffer, int offset, int count, AsyncProtocolCallback callback)
        {
            if (this._CompletionStatus != 0)
            {
                throw new InternalException();
            }
            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;
            this._Callback = callback;
        }

        internal object AsyncObject
        {
            get
            {
                return this.UserAsyncResult.AsyncObject;
            }
        }

        internal bool IsUserCompleted
        {
            get
            {
                return this.UserAsyncResult.InternalPeekCompleted;
            }
        }

        public bool MustCompleteSynchronously
        {
            get
            {
                switch (Interlocked.Exchange(ref this._CompletionStatus, 2))
                {
                    case 2:
                        throw new InternalException();

                    case 1:
                        this._CompletionStatus = 0;
                        return true;
                }
                return false;
            }
        }
    }
}

