namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class ErrorHandlingAcceptor
    {
        private readonly IListenerBinder binder;
        private readonly ChannelDispatcher dispatcher;

        internal ErrorHandlingAcceptor(IListenerBinder binder, ChannelDispatcher dispatcher)
        {
            this.binder = binder;
            this.dispatcher = dispatcher;
        }

        internal IAsyncResult BeginTryAccept(TimeSpan timeout, AsyncCallback callback, object state)
        {
            try
            {
                return this.binder.BeginAccept(timeout, callback, state);
            }
            catch (CommunicationObjectAbortedException)
            {
                return new ErrorHandlingCompletedAsyncResult(true, callback, state);
            }
            catch (CommunicationObjectFaultedException)
            {
                return new ErrorHandlingCompletedAsyncResult(true, callback, state);
            }
            catch (TimeoutException)
            {
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
            catch (CommunicationException exception)
            {
                this.HandleError(exception);
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.HandleErrorOrAbort(exception2);
                return new ErrorHandlingCompletedAsyncResult(false, callback, state);
            }
        }

        internal IAsyncResult BeginWaitForChannel(AsyncCallback callback, object state)
        {
            try
            {
                return this.binder.Listener.BeginWaitForChannel(TimeSpan.MaxValue, callback, state);
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
                this.binder.Listener.Close();
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

        internal bool EndTryAccept(IAsyncResult result, out IChannelBinder channelBinder)
        {
            ErrorHandlingCompletedAsyncResult result2 = result as ErrorHandlingCompletedAsyncResult;
            if (result2 != null)
            {
                channelBinder = null;
                return CompletedAsyncResult<bool>.End(result2);
            }
            try
            {
                channelBinder = this.binder.EndAccept(result);
                if (channelBinder != null)
                {
                    this.dispatcher.PendingChannels.Add(channelBinder.Channel);
                }
                return true;
            }
            catch (CommunicationObjectAbortedException)
            {
                channelBinder = null;
                return true;
            }
            catch (CommunicationObjectFaultedException)
            {
                channelBinder = null;
                return true;
            }
            catch (TimeoutException)
            {
                channelBinder = null;
                return false;
            }
            catch (CommunicationException exception)
            {
                this.HandleError(exception);
                channelBinder = null;
                return false;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.HandleErrorOrAbort(exception2);
                channelBinder = null;
                return false;
            }
        }

        internal void EndWaitForChannel(IAsyncResult result)
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
                    this.binder.Listener.EndWaitForChannel(result);
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
            if (this.dispatcher != null)
            {
                this.dispatcher.HandleError(e);
            }
        }

        internal bool TryAccept(TimeSpan timeout, out IChannelBinder channelBinder)
        {
            try
            {
                channelBinder = this.binder.Accept(timeout);
                if (channelBinder != null)
                {
                    this.dispatcher.PendingChannels.Add(channelBinder.Channel);
                }
                return true;
            }
            catch (CommunicationObjectAbortedException)
            {
                channelBinder = null;
                return true;
            }
            catch (CommunicationObjectFaultedException)
            {
                channelBinder = null;
                return true;
            }
            catch (TimeoutException)
            {
                channelBinder = null;
                return false;
            }
            catch (CommunicationException exception)
            {
                this.HandleError(exception);
                channelBinder = null;
                return false;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.HandleErrorOrAbort(exception2);
                channelBinder = null;
                return false;
            }
        }

        internal void WaitForChannel()
        {
            try
            {
                this.binder.Listener.WaitForChannel(TimeSpan.MaxValue);
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

