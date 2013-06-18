namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class ErrorHandlingReceiver
    {
        private IChannelBinder binder;
        private ChannelDispatcher dispatcher;

        internal ErrorHandlingReceiver(IChannelBinder binder, ChannelDispatcher dispatcher)
        {
            this.binder = binder;
            this.dispatcher = dispatcher;
        }

        internal IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            try
            {
                return this.binder.BeginTryReceive(timeout, callback, state);
            }
            catch (CommunicationObjectAbortedException)
            {
                return new ErrorHandlingCompletedAsyncResult(true, callback, state);
            }
            catch (CommunicationObjectFaultedException)
            {
                return new ErrorHandlingCompletedAsyncResult(true, callback, state);
            }
            catch (CommunicationException exception)
            {
                this.HandleError(exception);
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
            catch (TimeoutException exception2)
            {
                this.HandleError(exception2);
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                this.HandleErrorOrAbort(exception3);
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
        }

        internal IAsyncResult BeginWaitForMessage(AsyncCallback callback, object state)
        {
            try
            {
                return this.binder.BeginWaitForMessage(TimeSpan.MaxValue, callback, state);
            }
            catch (CommunicationObjectAbortedException)
            {
                return new WaitCompletedAsyncResult(callback, state);
            }
            catch (CommunicationObjectFaultedException)
            {
                return new WaitCompletedAsyncResult(callback, state);
            }
            catch (CommunicationException exception)
            {
                this.HandleError(exception);
                return new WaitCompletedAsyncResult(callback, state);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.HandleErrorOrAbort(exception2);
                return new WaitCompletedAsyncResult(callback, state);
            }
        }

        internal void Close()
        {
            try
            {
                this.binder.Channel.Close();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.HandleError(exception);
            }
        }

        internal bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            ErrorHandlingCompletedAsyncResult result2 = result as ErrorHandlingCompletedAsyncResult;
            if (result2 != null)
            {
                requestContext = null;
                return CompletedAsyncResult<bool>.End(result2);
            }
            try
            {
                return this.binder.EndTryReceive(result, out requestContext);
            }
            catch (CommunicationObjectAbortedException)
            {
                requestContext = null;
                return true;
            }
            catch (CommunicationObjectFaultedException)
            {
                requestContext = null;
                return true;
            }
            catch (CommunicationException exception)
            {
                this.HandleError(exception);
                requestContext = null;
                return false;
            }
            catch (TimeoutException exception2)
            {
                this.HandleError(exception2);
                requestContext = null;
                return false;
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                this.HandleErrorOrAbort(exception3);
                requestContext = null;
                return false;
            }
        }

        internal void EndWaitForMessage(IAsyncResult result)
        {
            WaitCompletedAsyncResult result2 = result as WaitCompletedAsyncResult;
            if (result2 != null)
            {
                CompletedAsyncResult.End(result2);
            }
            else
            {
                try
                {
                    this.binder.EndWaitForMessage(result);
                }
                catch (CommunicationObjectAbortedException)
                {
                }
                catch (CommunicationObjectFaultedException)
                {
                }
                catch (CommunicationException exception)
                {
                    this.HandleError(exception);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    this.HandleErrorOrAbort(exception2);
                }
            }
        }

        private void HandleError(Exception e)
        {
            if (this.dispatcher != null)
            {
                this.dispatcher.HandleError(e);
            }
        }

        private void HandleErrorOrAbort(Exception e)
        {
            if (((this.dispatcher == null) || !this.dispatcher.HandleError(e)) && this.binder.HasSession)
            {
                this.binder.Abort();
            }
        }

        internal bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            try
            {
                return this.binder.TryReceive(timeout, out requestContext);
            }
            catch (CommunicationObjectAbortedException)
            {
                requestContext = null;
                return true;
            }
            catch (CommunicationObjectFaultedException)
            {
                requestContext = null;
                return true;
            }
            catch (CommunicationException exception)
            {
                this.HandleError(exception);
                requestContext = null;
                return false;
            }
            catch (TimeoutException exception2)
            {
                this.HandleError(exception2);
                requestContext = null;
                return false;
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                this.HandleErrorOrAbort(exception3);
                requestContext = null;
                return false;
            }
        }

        internal void WaitForMessage()
        {
            try
            {
                this.binder.WaitForMessage(TimeSpan.MaxValue);
            }
            catch (CommunicationObjectAbortedException)
            {
            }
            catch (CommunicationObjectFaultedException)
            {
            }
            catch (CommunicationException exception)
            {
                this.HandleError(exception);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.HandleErrorOrAbort(exception2);
            }
        }

        private class ErrorHandlingCompletedAsyncResult : CompletedAsyncResult<bool>
        {
            internal ErrorHandlingCompletedAsyncResult(bool data, AsyncCallback callback, object state) : base(data, callback, state)
            {
            }
        }

        private class WaitCompletedAsyncResult : CompletedAsyncResult
        {
            internal WaitCompletedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }
        }
    }
}

