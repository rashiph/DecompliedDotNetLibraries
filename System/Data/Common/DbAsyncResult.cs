namespace System.Data.Common
{
    using System;
    using System.Data.ProviderBase;
    using System.Threading;

    internal sealed class DbAsyncResult : IAsyncResult
    {
        private readonly AsyncCallback _callback;
        private DbConnectionInternal _connectionInternal;
        private static ContextCallback _contextCallback = new ContextCallback(DbAsyncResult.AsyncCallback_Context);
        private readonly string _endMethodName;
        private ExecutionContext _execContext;
        private bool _fCompleted;
        private bool _fCompletedSynchronously;
        private readonly ManualResetEvent _manualResetEvent;
        private object _owner;
        private readonly object _stateObject;

        internal DbAsyncResult(object owner, string endMethodName, AsyncCallback callback, object stateObject, ExecutionContext execContext)
        {
            this._owner = owner;
            this._endMethodName = endMethodName;
            this._callback = callback;
            this._stateObject = stateObject;
            this._manualResetEvent = new ManualResetEvent(false);
            this._execContext = execContext;
        }

        private static void AsyncCallback_Context(object state)
        {
            DbAsyncResult ar = (DbAsyncResult) state;
            if (ar._callback != null)
            {
                ar._callback(ar);
            }
        }

        internal void CompareExchangeOwner(object owner, string method)
        {
            object obj2 = Interlocked.CompareExchange(ref this._owner, null, owner);
            if (obj2 != owner)
            {
                if (obj2 != null)
                {
                    throw ADP.IncorrectAsyncResult();
                }
                throw ADP.MethodCalledTwice(method);
            }
        }

        private void ExecuteCallback(object asyncResult)
        {
            DbAsyncResult state = (DbAsyncResult) asyncResult;
            if (state._callback != null)
            {
                if (state._execContext != null)
                {
                    ExecutionContext.Run(state._execContext, _contextCallback, state);
                }
                else
                {
                    state._callback(this);
                }
            }
        }

        internal void Reset()
        {
            this._fCompleted = false;
            this._fCompletedSynchronously = false;
            this._manualResetEvent.Reset();
        }

        internal void SetCompleted()
        {
            this._fCompleted = true;
            this._manualResetEvent.Set();
            if (this._callback != null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ExecuteCallback), this);
            }
        }

        internal void SetCompletedSynchronously()
        {
            this._fCompletedSynchronously = true;
        }

        internal DbConnectionInternal ConnectionInternal
        {
            get
            {
                return this._connectionInternal;
            }
            set
            {
                this._connectionInternal = value;
            }
        }

        internal string EndMethodName
        {
            get
            {
                return this._endMethodName;
            }
        }

        object IAsyncResult.AsyncState
        {
            get
            {
                return this._stateObject;
            }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get
            {
                return this._manualResetEvent;
            }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get
            {
                return this._fCompletedSynchronously;
            }
        }

        bool IAsyncResult.IsCompleted
        {
            get
            {
                return this._fCompleted;
            }
        }
    }
}

