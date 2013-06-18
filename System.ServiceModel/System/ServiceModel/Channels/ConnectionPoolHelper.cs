namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal abstract class ConnectionPoolHelper
    {
        private bool closed;
        private IConnectionInitiator connectionInitiator;
        private string connectionKey;
        private ConnectionPool connectionPool;
        private bool isConnectionFromPool;
        private IConnection rawConnection;
        private IConnection upgradedConnection;
        private Uri via;

        public ConnectionPoolHelper(ConnectionPool connectionPool, IConnectionInitiator connectionInitiator, Uri via)
        {
            this.connectionInitiator = connectionInitiator;
            this.connectionPool = connectionPool;
            this.via = via;
        }

        public void Abort()
        {
            this.ReleaseConnection(true, TimeSpan.Zero);
        }

        protected abstract IConnection AcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper);
        protected abstract IAsyncResult BeginAcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state);
        public IAsyncResult BeginEstablishConnection(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new EstablishConnectionAsyncResult(this, timeout, callback, state);
        }

        public void Close(TimeSpan timeout)
        {
            this.ReleaseConnection(false, timeout);
        }

        protected abstract TimeoutException CreateNewConnectionTimeoutException(TimeSpan timeout, TimeoutException innerException);
        protected abstract IConnection EndAcceptPooledConnection(IAsyncResult result);
        public IConnection EndEstablishConnection(IAsyncResult result)
        {
            return EstablishConnectionAsyncResult.End(result);
        }

        public IConnection EstablishConnection(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            IConnection connection = null;
            IConnection upgradedConnection = null;
            bool isConnectionFromPool = true;
            while (isConnectionFromPool)
            {
                connection = this.TakeConnection(timeoutHelper.RemainingTime());
                if (connection == null)
                {
                    isConnectionFromPool = false;
                }
                else
                {
                    bool flag2 = false;
                    try
                    {
                        try
                        {
                            upgradedConnection = this.AcceptPooledConnection(connection, ref timeoutHelper);
                            flag2 = true;
                            break;
                        }
                        catch (CommunicationException exception)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            }
                        }
                        catch (TimeoutException exception2)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        if (!flag2)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information, 0x40030, System.ServiceModel.SR.GetString("TraceCodeFailedAcceptFromPool", new object[] { timeoutHelper.RemainingTime() }));
                            }
                            this.connectionPool.ReturnConnection(this.connectionKey, connection, false, TimeSpan.Zero);
                        }
                    }
                }
            }
            if (!isConnectionFromPool)
            {
                bool flag3 = false;
                TimeSpan span = timeoutHelper.RemainingTime();
                try
                {
                    try
                    {
                        connection = this.connectionInitiator.Connect(this.via, span);
                    }
                    catch (TimeoutException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateNewConnectionTimeoutException(span, exception3));
                    }
                    this.connectionInitiator = null;
                    upgradedConnection = this.AcceptPooledConnection(connection, ref timeoutHelper);
                    flag3 = true;
                }
                finally
                {
                    if (!flag3)
                    {
                        this.connectionKey = null;
                        if (connection != null)
                        {
                            connection.Abort();
                        }
                    }
                }
            }
            this.SnapshotConnection(upgradedConnection, connection, isConnectionFromPool);
            return upgradedConnection;
        }

        private void ReleaseConnection(bool abort, TimeSpan timeout)
        {
            string connectionKey;
            IConnection upgradedConnection;
            IConnection rawConnection;
            lock (this.ThisLock)
            {
                this.closed = true;
                connectionKey = this.connectionKey;
                upgradedConnection = this.upgradedConnection;
                rawConnection = this.rawConnection;
                this.upgradedConnection = null;
                this.rawConnection = null;
            }
            if (upgradedConnection != null)
            {
                try
                {
                    if (this.isConnectionFromPool)
                    {
                        this.connectionPool.ReturnConnection(connectionKey, rawConnection, !abort, timeout);
                    }
                    else if (abort)
                    {
                        upgradedConnection.Abort();
                    }
                    else
                    {
                        this.connectionPool.AddConnection(connectionKey, rawConnection, timeout);
                    }
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    upgradedConnection.Abort();
                }
            }
        }

        private void SnapshotConnection(IConnection upgradedConnection, IConnection rawConnection, bool isConnectionFromPool)
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    upgradedConnection.Abort();
                    if (isConnectionFromPool)
                    {
                        this.connectionPool.ReturnConnection(this.connectionKey, rawConnection, false, TimeSpan.Zero);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("OperationAbortedDuringConnectionEstablishment", new object[] { this.via })));
                }
                this.upgradedConnection = upgradedConnection;
                this.rawConnection = rawConnection;
                this.isConnectionFromPool = isConnectionFromPool;
            }
        }

        private IConnection TakeConnection(TimeSpan timeout)
        {
            return this.connectionPool.TakeConnection(null, this.via, timeout, out this.connectionKey);
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class EstablishConnectionAsyncResult : AsyncResult
        {
            private bool cleanupConnection;
            private TimeSpan connectTimeout;
            private IConnection currentConnection;
            private bool newConnection;
            private static AsyncCallback onConnect;
            private static AsyncCallback onProcessConnection = Fx.ThunkCallback(new AsyncCallback(ConnectionPoolHelper.EstablishConnectionAsyncResult.OnProcessConnection));
            private ConnectionPoolHelper parent;
            private IConnection rawConnection;
            private TimeoutHelper timeoutHelper;

            public EstablishConnectionAsyncResult(ConnectionPoolHelper parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool flag = false;
                bool flag2 = false;
                try
                {
                    flag2 = this.Begin();
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.Cleanup();
                    }
                }
                if (flag2)
                {
                    this.Cleanup();
                    base.Complete(true);
                }
            }

            private bool Begin()
            {
                bool flag;
                IConnection connection = this.parent.TakeConnection(this.timeoutHelper.RemainingTime());
                this.TrackConnection(connection);
                if (this.OpenUsingConnectionPool(out flag))
                {
                    return true;
                }
                if (flag)
                {
                    return false;
                }
                return this.OpenUsingNewConnection();
            }

            private void Cleanup()
            {
                if (this.cleanupConnection)
                {
                    if (this.newConnection)
                    {
                        if (this.currentConnection != null)
                        {
                            this.currentConnection.Abort();
                            this.currentConnection = null;
                        }
                    }
                    else if (this.rawConnection != null)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information, 0x40030, System.ServiceModel.SR.GetString("TraceCodeFailedAcceptFromPool", new object[] { this.timeoutHelper.RemainingTime() }));
                        }
                        this.parent.connectionPool.ReturnConnection(this.parent.connectionKey, this.rawConnection, false, this.timeoutHelper.RemainingTime());
                        this.currentConnection = null;
                        this.rawConnection = null;
                    }
                    this.cleanupConnection = false;
                }
            }

            public static IConnection End(IAsyncResult result)
            {
                return AsyncResult.End<ConnectionPoolHelper.EstablishConnectionAsyncResult>(result).currentConnection;
            }

            private bool HandleConnect(IAsyncResult connectResult)
            {
                try
                {
                    this.TrackConnection(this.parent.connectionInitiator.EndConnect(connectResult));
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.CreateNewConnectionTimeoutException(this.connectTimeout, exception));
                }
                if (this.ProcessConnection())
                {
                    this.SnapshotConnection();
                    return true;
                }
                return false;
            }

            private bool HandleProcessConnection(IAsyncResult result)
            {
                this.currentConnection = this.parent.EndAcceptPooledConnection(result);
                this.cleanupConnection = false;
                return true;
            }

            private static void OnConnect(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    ConnectionPoolHelper.EstablishConnectionAsyncResult asyncState = (ConnectionPoolHelper.EstablishConnectionAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleConnect(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        asyncState.Cleanup();
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnProcessConnection(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    ConnectionPoolHelper.EstablishConnectionAsyncResult asyncState = (ConnectionPoolHelper.EstablishConnectionAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        bool flag2 = false;
                        try
                        {
                            flag = asyncState.HandleProcessConnection(result);
                            if (flag)
                            {
                                flag2 = true;
                            }
                        }
                        catch (CommunicationException exception2)
                        {
                            if (!asyncState.newConnection)
                            {
                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                                }
                                asyncState.Cleanup();
                                flag = asyncState.Begin();
                            }
                            else
                            {
                                flag = true;
                                exception = exception2;
                            }
                        }
                        catch (TimeoutException exception3)
                        {
                            if (!asyncState.newConnection)
                            {
                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                                }
                                asyncState.Cleanup();
                                flag = asyncState.Begin();
                            }
                            else
                            {
                                flag = true;
                                exception = exception3;
                            }
                        }
                        if (flag2)
                        {
                            asyncState.SnapshotConnection();
                        }
                    }
                    catch (Exception exception4)
                    {
                        if (Fx.IsFatal(exception4))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception4;
                    }
                    if (flag)
                    {
                        asyncState.Cleanup();
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool OpenUsingConnectionPool(out bool openingFromPool)
            {
                openingFromPool = true;
                while (this.currentConnection != null)
                {
                    bool flag = false;
                    try
                    {
                        if (this.ProcessConnection())
                        {
                            flag = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (CommunicationException exception)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                        this.Cleanup();
                    }
                    catch (TimeoutException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                        this.Cleanup();
                    }
                    if (flag)
                    {
                        this.SnapshotConnection();
                        return true;
                    }
                    IConnection connection = this.parent.TakeConnection(this.timeoutHelper.RemainingTime());
                    this.TrackConnection(connection);
                }
                openingFromPool = false;
                return false;
            }

            private bool OpenUsingNewConnection()
            {
                IAsyncResult result;
                this.newConnection = true;
                try
                {
                    this.connectTimeout = this.timeoutHelper.RemainingTime();
                    if (onConnect == null)
                    {
                        onConnect = Fx.ThunkCallback(new AsyncCallback(ConnectionPoolHelper.EstablishConnectionAsyncResult.OnConnect));
                    }
                    result = this.parent.connectionInitiator.BeginConnect(this.parent.via, this.connectTimeout, onConnect, this);
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.CreateNewConnectionTimeoutException(this.connectTimeout, exception));
                }
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleConnect(result);
            }

            private bool ProcessConnection()
            {
                IAsyncResult result = this.parent.BeginAcceptPooledConnection(this.rawConnection, ref this.timeoutHelper, onProcessConnection, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleProcessConnection(result);
            }

            private void SnapshotConnection()
            {
                this.parent.SnapshotConnection(this.currentConnection, this.rawConnection, !this.newConnection);
            }

            private void TrackConnection(IConnection connection)
            {
                this.cleanupConnection = true;
                this.rawConnection = connection;
                this.currentConnection = connection;
            }
        }
    }
}

