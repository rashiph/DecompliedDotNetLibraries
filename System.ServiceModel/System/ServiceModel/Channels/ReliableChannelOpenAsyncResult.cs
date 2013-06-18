namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;

    internal class ReliableChannelOpenAsyncResult : AsyncResult
    {
        private IReliableChannelBinder binder;
        private static AsyncCallback onBinderOpenComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelOpenAsyncResult.OnBinderOpenComplete));
        private static AsyncCallback onSessionOpenComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelOpenAsyncResult.OnSessionOpenComplete));
        private ChannelReliableSession session;
        private TimeoutHelper timeoutHelper;

        public ReliableChannelOpenAsyncResult(IReliableChannelBinder binder, ChannelReliableSession session, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.binder = binder;
            this.session = session;
            this.timeoutHelper = new TimeoutHelper(timeout);
            bool flag = false;
            bool flag2 = true;
            Exception e = null;
            try
            {
                IAsyncResult result = this.binder.BeginOpen(this.timeoutHelper.RemainingTime(), onBinderOpenComplete, this);
                flag2 = false;
                if (result.CompletedSynchronously)
                {
                    flag = this.CompleteBinderOpen(true, result);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                if (flag2 || this.CloseBinder(e))
                {
                    throw;
                }
            }
            finally
            {
                if (flag2)
                {
                    this.binder.Abort();
                }
            }
            if (flag)
            {
                base.Complete(true);
            }
        }

        private bool CloseBinder(Exception e)
        {
            IAsyncResult result = this.binder.BeginClose(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnBinderCloseComplete)), e);
            if (result.CompletedSynchronously)
            {
                this.binder.EndClose(result);
                return true;
            }
            return false;
        }

        private void CloseBinderAndComplete(Exception e)
        {
            bool flag = true;
            try
            {
                flag = this.CloseBinder(e);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            if (flag)
            {
                base.Complete(false, e);
            }
        }

        private bool CompleteBinderOpen(bool synchronous, IAsyncResult result)
        {
            this.binder.EndOpen(result);
            result = this.session.BeginOpen(this.timeoutHelper.RemainingTime(), onSessionOpenComplete, this);
            if (result.CompletedSynchronously)
            {
                this.session.EndOpen(result);
                return true;
            }
            return false;
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ReliableChannelOpenAsyncResult>(result);
        }

        private void OnBinderCloseComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                Exception asyncState = (Exception) result.AsyncState;
                try
                {
                    this.binder.EndClose(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                base.Complete(false, asyncState);
            }
        }

        private static void OnBinderOpenComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelOpenAsyncResult asyncState = (ReliableChannelOpenAsyncResult) result.AsyncState;
                bool flag = false;
                Exception exception = null;
                try
                {
                    flag = asyncState.CompleteBinderOpen(false, result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (flag)
                {
                    asyncState.Complete(false, exception);
                }
                else if (exception != null)
                {
                    asyncState.CloseBinderAndComplete(exception);
                }
            }
        }

        private static void OnSessionOpenComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelOpenAsyncResult asyncState = (ReliableChannelOpenAsyncResult) result.AsyncState;
                Exception e = null;
                try
                {
                    asyncState.session.EndOpen(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    e = exception2;
                }
                if (e != null)
                {
                    asyncState.CloseBinderAndComplete(e);
                }
                else
                {
                    asyncState.Complete(false);
                }
            }
        }
    }
}

