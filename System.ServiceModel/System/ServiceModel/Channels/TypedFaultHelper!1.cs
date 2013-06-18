namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    internal abstract class TypedFaultHelper<TState> : FaultHelper
    {
        private InterruptibleWaitObject closeHandle;
        private TimeSpan defaultCloseTimeout;
        private TimeSpan defaultSendTimeout;
        private Dictionary<IReliableChannelBinder, TState> faultList;
        private AsyncCallback onBinderCloseComplete;
        private AsyncCallback onSendFaultComplete;
        private Action<object> sendFaultCallback;

        protected TypedFaultHelper(TimeSpan defaultSendTimeout, TimeSpan defaultCloseTimeout)
        {
            this.faultList = new Dictionary<IReliableChannelBinder, TState>();
            this.defaultSendTimeout = defaultSendTimeout;
            this.defaultCloseTimeout = defaultCloseTimeout;
        }

        public override void Abort()
        {
            Dictionary<IReliableChannelBinder, TState> faultList;
            InterruptibleWaitObject closeHandle;
            lock (base.ThisLock)
            {
                faultList = this.faultList;
                this.faultList = null;
                closeHandle = this.closeHandle;
            }
            if ((faultList == null) || (faultList.Count == 0))
            {
                if (closeHandle != null)
                {
                    closeHandle.Set();
                }
            }
            else
            {
                foreach (KeyValuePair<IReliableChannelBinder, TState> pair in faultList)
                {
                    this.AbortState(pair.Value, true);
                    pair.Key.Abort();
                }
                if (closeHandle != null)
                {
                    closeHandle.Set();
                }
            }
        }

        private void AbortBinder(IReliableChannelBinder binder)
        {
            try
            {
                binder.Abort();
            }
            finally
            {
                this.RemoveBinder(binder);
            }
        }

        protected abstract void AbortState(TState state, bool isOnAbortThread);
        private void AfterClose()
        {
            this.Abort();
        }

        private void AsyncCloseBinder(IReliableChannelBinder binder)
        {
            if (this.onBinderCloseComplete == null)
            {
                this.onBinderCloseComplete = Fx.ThunkCallback(new AsyncCallback(this.OnBinderCloseComplete));
            }
            IAsyncResult result = binder.BeginClose(this.defaultCloseTimeout, this.onBinderCloseComplete, binder);
            if (result.CompletedSynchronously)
            {
                this.CompleteBinderClose(binder, result);
            }
        }

        private bool BeforeClose()
        {
            lock (base.ThisLock)
            {
                if ((this.faultList == null) || (this.faultList.Count == 0))
                {
                    return true;
                }
                this.closeHandle = new InterruptibleWaitObject(false, false);
            }
            return false;
        }

        public override IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.BeforeClose())
            {
                return new AlreadyClosedAsyncResult<TState>(callback, state);
            }
            return this.closeHandle.BeginWait(timeout, callback, state);
        }

        protected abstract IAsyncResult BeginSendFault(IReliableChannelBinder binder, TState state, TimeSpan timeout, AsyncCallback callback, object asyncState);
        public override void Close(TimeSpan timeout)
        {
            if (!this.BeforeClose())
            {
                this.closeHandle.Wait(timeout);
                this.AfterClose();
            }
        }

        private void CompleteBinderClose(IReliableChannelBinder binder, IAsyncResult result)
        {
            try
            {
                binder.EndClose(result);
            }
            finally
            {
                this.RemoveBinder(binder);
            }
        }

        private void CompleteSendFault(IReliableChannelBinder binder, TState state, IAsyncResult result)
        {
            bool flag = true;
            try
            {
                this.EndSendFault(binder, state, result);
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    this.AbortState(state, false);
                    this.AbortBinder(binder);
                }
            }
            this.AsyncCloseBinder(binder);
        }

        public override void EndClose(IAsyncResult result)
        {
            if (result is AlreadyClosedAsyncResult<TState>)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                this.closeHandle.EndWait(result);
            }
            this.AfterClose();
        }

        protected abstract void EndSendFault(IReliableChannelBinder binder, TState state, IAsyncResult result);
        protected abstract TState GetState(RequestContext requestContext, Message faultMessage);
        private void OnBinderCloseComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                IReliableChannelBinder asyncState = (IReliableChannelBinder) result.AsyncState;
                try
                {
                    this.CompleteBinderClose(asyncState, result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.HandleException(exception);
                }
            }
        }

        private void OnSendFaultComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                IReliableChannelBinder asyncState;
                TState local;
                lock (base.ThisLock)
                {
                    if (this.faultList == null)
                    {
                        return;
                    }
                    asyncState = (IReliableChannelBinder) result.AsyncState;
                    local = this.faultList[asyncState];
                }
                try
                {
                    this.CompleteSendFault(asyncState, local, result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.HandleException(exception);
                }
            }
        }

        protected void RemoveBinder(IReliableChannelBinder binder)
        {
            InterruptibleWaitObject closeHandle;
            lock (base.ThisLock)
            {
                if (this.faultList == null)
                {
                    return;
                }
                this.faultList.Remove(binder);
                if ((this.closeHandle == null) || (this.faultList.Count > 0))
                {
                    return;
                }
                this.faultList = null;
                closeHandle = this.closeHandle;
            }
            closeHandle.Set();
        }

        protected void SendFault(IReliableChannelBinder binder, TState state)
        {
            IAsyncResult result;
            bool flag = true;
            try
            {
                result = this.BeginSendFault(binder, state, this.defaultSendTimeout, this.onSendFaultComplete, binder);
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    this.AbortState(state, false);
                    this.AbortBinder(binder);
                }
            }
            if (result.CompletedSynchronously)
            {
                this.CompleteSendFault(binder, state, result);
            }
        }

        public override void SendFaultAsync(IReliableChannelBinder binder, RequestContext requestContext, Message faultMessage)
        {
            try
            {
                bool flag = true;
                TState state = this.GetState(requestContext, faultMessage);
                lock (base.ThisLock)
                {
                    if (this.faultList != null)
                    {
                        flag = false;
                        this.faultList.Add(binder, state);
                        if (this.onSendFaultComplete == null)
                        {
                            this.onSendFaultComplete = Fx.ThunkCallback(new AsyncCallback(this.OnSendFaultComplete));
                        }
                    }
                }
                if (flag)
                {
                    this.AbortState(state, false);
                    binder.Abort();
                }
                else if (Thread.CurrentThread.IsThreadPoolThread)
                {
                    this.SendFault(binder, state);
                }
                else
                {
                    if (this.sendFaultCallback == null)
                    {
                        this.sendFaultCallback = new Action<object>(this.SendFaultCallback);
                    }
                    ActionItem.Schedule(this.sendFaultCallback, binder);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                binder.HandleException(exception);
            }
        }

        private void SendFaultCallback(object callbackState)
        {
            IReliableChannelBinder binder;
            TState local;
            lock (base.ThisLock)
            {
                if (this.faultList == null)
                {
                    return;
                }
                binder = (IReliableChannelBinder) callbackState;
                local = this.faultList[binder];
            }
            try
            {
                this.SendFault(binder, local);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                binder.HandleException(exception);
            }
        }

        private class AlreadyClosedAsyncResult : CompletedAsyncResult
        {
            public AlreadyClosedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }
        }
    }
}

