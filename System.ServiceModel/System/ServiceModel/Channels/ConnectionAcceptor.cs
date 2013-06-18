namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    internal class ConnectionAcceptor : IDisposable
    {
        private AsyncCallback acceptCompletedCallback;
        private ConnectionAvailableCallback callback;
        private int connections;
        private System.ServiceModel.Channels.ErrorCallback errorCallback;
        private bool isDisposed;
        private IConnectionListener listener;
        private int maxAccepts;
        private int maxPendingConnections;
        private Action onConnectionDequeued;
        private int pendingAccepts;
        private Action<object> scheduleAcceptCallback;

        public ConnectionAcceptor(IConnectionListener listener, int maxAccepts, int maxPendingConnections, ConnectionAvailableCallback callback) : this(listener, maxAccepts, maxPendingConnections, callback, null)
        {
        }

        public ConnectionAcceptor(IConnectionListener listener, int maxAccepts, int maxPendingConnections, ConnectionAvailableCallback callback, System.ServiceModel.Channels.ErrorCallback errorCallback)
        {
            if (maxAccepts <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxAccepts", maxAccepts, System.ServiceModel.SR.GetString("ValueMustBePositive")));
            }
            this.listener = listener;
            this.maxAccepts = maxAccepts;
            this.maxPendingConnections = maxPendingConnections;
            this.callback = callback;
            this.errorCallback = errorCallback;
            this.onConnectionDequeued = new Action(this.OnConnectionDequeued);
            this.acceptCompletedCallback = Fx.ThunkCallback(new AsyncCallback(this.AcceptCompletedCallback));
            this.scheduleAcceptCallback = new Action<object>(this.ScheduleAcceptCallback);
        }

        private void AcceptCompletedCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                this.HandleCompletedAccept(result);
            }
        }

        private void AcceptIfNecessary(bool startAccepting)
        {
            if (this.IsAcceptNecessary)
            {
                lock (this.ThisLock)
                {
                    while (this.IsAcceptNecessary)
                    {
                        IAsyncResult state = null;
                        Exception exception = null;
                        try
                        {
                            state = this.listener.BeginAccept(this.acceptCompletedCallback, null);
                        }
                        catch (CommunicationException exception2)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                            }
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            if (startAccepting)
                            {
                                throw;
                            }
                            if ((this.errorCallback == null) && !System.ServiceModel.Dispatcher.ExceptionHandler.HandleTransportExceptionHelper(exception3))
                            {
                                throw;
                            }
                            exception = exception3;
                        }
                        if ((exception != null) && (this.errorCallback != null))
                        {
                            this.errorCallback(exception);
                        }
                        if (state != null)
                        {
                            if (state.CompletedSynchronously)
                            {
                                ActionItem.Schedule(this.scheduleAcceptCallback, state);
                            }
                            this.pendingAccepts++;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                if (!this.isDisposed)
                {
                    this.isDisposed = true;
                    this.listener.Dispose();
                }
            }
        }

        private void HandleCompletedAccept(IAsyncResult result)
        {
            IConnection connection = null;
            lock (this.ThisLock)
            {
                bool flag = false;
                Exception exception = null;
                try
                {
                    if (!this.isDisposed)
                    {
                        connection = this.listener.EndAccept(result);
                        if (connection != null)
                        {
                            if (DiagnosticUtility.ShouldTraceWarning && ((this.connections + 1) >= this.maxPendingConnections))
                            {
                                TraceUtility.TraceEvent(TraceEventType.Warning, 0x40024, System.ServiceModel.SR.GetString("TraceCodeMaxPendingConnectionsReached"), new StringTraceRecord("MaxPendingConnections", this.maxPendingConnections.ToString(CultureInfo.InvariantCulture)), this, null);
                            }
                            this.connections++;
                        }
                    }
                    flag = true;
                }
                catch (CommunicationException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    if ((this.errorCallback == null) && !System.ServiceModel.Dispatcher.ExceptionHandler.HandleTransportExceptionHelper(exception3))
                    {
                        throw;
                    }
                    exception = exception3;
                }
                finally
                {
                    if (!flag)
                    {
                        connection = null;
                    }
                    this.pendingAccepts--;
                }
                if ((exception != null) && (this.errorCallback != null))
                {
                    this.errorCallback(exception);
                }
            }
            this.AcceptIfNecessary(false);
            if (connection != null)
            {
                this.callback(connection, this.onConnectionDequeued);
            }
        }

        private void OnConnectionDequeued()
        {
            lock (this.ThisLock)
            {
                this.connections--;
            }
            this.AcceptIfNecessary(false);
        }

        private void ScheduleAcceptCallback(object state)
        {
            this.HandleCompletedAccept((IAsyncResult) state);
        }

        public void StartAccepting()
        {
            this.listener.Listen();
            this.AcceptIfNecessary(true);
        }

        public int ConnectionCount
        {
            get
            {
                return this.connections;
            }
        }

        private bool IsAcceptNecessary
        {
            get
            {
                return (((this.pendingAccepts < this.maxAccepts) && ((this.connections + this.pendingAccepts) < this.maxPendingConnections)) && !this.isDisposed);
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }
    }
}

