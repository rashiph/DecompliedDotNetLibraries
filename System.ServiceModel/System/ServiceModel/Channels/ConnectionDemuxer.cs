namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    internal sealed class ConnectionDemuxer : IDisposable
    {
        private ConnectionAcceptor acceptor;
        private TimeSpan channelInitializationTimeout;
        private List<InitialServerConnectionReader> connectionReaders = new List<InitialServerConnectionReader>();
        private TimeSpan idleTimeout;
        private bool isDisposed;
        private int maxPooledConnections;
        private ConnectionModeCallback onCachedConnectionModeKnown;
        private ConnectionClosedCallback onConnectionClosed;
        private ConnectionModeCallback onConnectionModeKnown;
        private ServerSessionPreambleCallback onSessionPreambleKnown;
        private ServerSingletonPreambleCallback onSingletonPreambleKnown;
        private int pooledConnectionCount;
        private Action pooledConnectionDequeuedCallback;
        private Action<object> reuseConnectionCallback;
        private ServerSessionPreambleDemuxCallback serverSessionPreambleCallback;
        private SingletonPreambleDemuxCallback singletonPreambleCallback;
        private TransportSettingsCallback transportSettingsCallback;
        private Action<Uri> viaDelegate;

        public ConnectionDemuxer(IConnectionListener listener, int maxAccepts, int maxPendingConnections, TimeSpan channelInitializationTimeout, TimeSpan idleTimeout, int maxPooledConnections, TransportSettingsCallback transportSettingsCallback, SingletonPreambleDemuxCallback singletonPreambleCallback, ServerSessionPreambleDemuxCallback serverSessionPreambleCallback, System.ServiceModel.Channels.ErrorCallback errorCallback)
        {
            this.acceptor = new ConnectionAcceptor(listener, maxAccepts, maxPendingConnections, new ConnectionAvailableCallback(this.OnConnectionAvailable), errorCallback);
            this.channelInitializationTimeout = channelInitializationTimeout;
            this.idleTimeout = idleTimeout;
            this.maxPooledConnections = maxPooledConnections;
            this.onConnectionClosed = new ConnectionClosedCallback(this.OnConnectionClosed);
            this.transportSettingsCallback = transportSettingsCallback;
            this.singletonPreambleCallback = singletonPreambleCallback;
            this.serverSessionPreambleCallback = serverSessionPreambleCallback;
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    return;
                }
                this.isDisposed = true;
            }
            for (int i = 0; i < this.connectionReaders.Count; i++)
            {
                this.connectionReaders[i].Dispose();
            }
            this.connectionReaders.Clear();
            this.acceptor.Dispose();
        }

        private void OnCachedConnectionModeKnown(ConnectionModeReader modeReader)
        {
            this.OnConnectionModeKnownCore(modeReader, true);
        }

        private void OnConnectionAvailable(IConnection connection, Action connectionDequeuedCallback)
        {
            ConnectionModeReader reader = this.SetupModeReader(connection, false);
            if (reader != null)
            {
                reader.StartReading(this.channelInitializationTimeout, connectionDequeuedCallback);
            }
            else
            {
                connectionDequeuedCallback();
            }
        }

        private void OnConnectionClosed(InitialServerConnectionReader connectionReader)
        {
            lock (this.ThisLock)
            {
                if (!this.isDisposed)
                {
                    this.connectionReaders.Remove(connectionReader);
                }
            }
        }

        private void OnConnectionModeKnown(ConnectionModeReader modeReader)
        {
            this.OnConnectionModeKnownCore(modeReader, false);
        }

        private void OnConnectionModeKnownCore(ConnectionModeReader modeReader, bool isCached)
        {
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    return;
                }
                this.connectionReaders.Remove(modeReader);
            }
            bool flag = true;
            try
            {
                FramingMode connectionMode;
                try
                {
                    connectionMode = modeReader.GetConnectionMode();
                }
                catch (CommunicationException exception)
                {
                    TraceEventType exceptionEventType = modeReader.Connection.ExceptionEventType;
                    if (DiagnosticUtility.ShouldTrace(exceptionEventType))
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, exceptionEventType);
                    }
                    return;
                }
                catch (TimeoutException exception2)
                {
                    if (!isCached)
                    {
                        exception2 = new TimeoutException(System.ServiceModel.SR.GetString("ChannelInitializationTimeout", new object[] { this.channelInitializationTimeout }), exception2);
                        ErrorBehavior.ThrowAndCatch(exception2);
                    }
                    TraceEventType type = modeReader.Connection.ExceptionEventType;
                    if (DiagnosticUtility.ShouldTrace(type))
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, type);
                    }
                    return;
                }
                switch (connectionMode)
                {
                    case FramingMode.Singleton:
                        this.OnSingletonConnection(modeReader.Connection, modeReader.ConnectionDequeuedCallback, modeReader.StreamPosition, modeReader.BufferOffset, modeReader.BufferSize, modeReader.GetRemainingTimeout());
                        break;

                    case FramingMode.Duplex:
                        this.OnDuplexConnection(modeReader.Connection, modeReader.ConnectionDequeuedCallback, modeReader.StreamPosition, modeReader.BufferOffset, modeReader.BufferSize, modeReader.GetRemainingTimeout());
                        break;

                    default:
                    {
                        Exception innerException = new InvalidDataException(System.ServiceModel.SR.GetString("FramingModeNotSupported", new object[] { connectionMode }));
                        Exception exception4 = new ProtocolException(innerException.Message, innerException);
                        FramingEncodingString.AddFaultString(exception4, "http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedMode");
                        ErrorBehavior.ThrowAndCatch(exception4);
                        return;
                    }
                }
                flag = false;
            }
            catch (Exception exception5)
            {
                if (Fx.IsFatal(exception5))
                {
                    throw;
                }
                if (!System.ServiceModel.Dispatcher.ExceptionHandler.HandleTransportExceptionHelper(exception5))
                {
                    throw;
                }
            }
            finally
            {
                if (flag)
                {
                    modeReader.Dispose();
                }
            }
        }

        private void OnDuplexConnection(IConnection connection, Action connectionDequeuedCallback, long streamPosition, int offset, int size, TimeSpan timeout)
        {
            if (this.onSessionPreambleKnown == null)
            {
                this.onSessionPreambleKnown = new ServerSessionPreambleCallback(this.OnSessionPreambleKnown);
            }
            ServerSessionPreambleConnectionReader item = new ServerSessionPreambleConnectionReader(connection, connectionDequeuedCallback, streamPosition, offset, size, this.transportSettingsCallback, this.onConnectionClosed, this.onSessionPreambleKnown);
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    item.Dispose();
                    return;
                }
                this.connectionReaders.Add(item);
            }
            item.StartReading(this.viaDelegate, timeout);
        }

        private void OnSessionPreambleKnown(ServerSessionPreambleConnectionReader serverSessionPreambleReader)
        {
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    return;
                }
                this.connectionReaders.Remove(serverSessionPreambleReader);
            }
            this.serverSessionPreambleCallback(serverSessionPreambleReader, this);
        }

        private void OnSingletonConnection(IConnection connection, Action connectionDequeuedCallback, long streamPosition, int offset, int size, TimeSpan timeout)
        {
            if (this.onSingletonPreambleKnown == null)
            {
                this.onSingletonPreambleKnown = new ServerSingletonPreambleCallback(this.OnSingletonPreambleKnown);
            }
            ServerSingletonPreambleConnectionReader item = new ServerSingletonPreambleConnectionReader(connection, connectionDequeuedCallback, streamPosition, offset, size, this.transportSettingsCallback, this.onConnectionClosed, this.onSingletonPreambleKnown);
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    item.Dispose();
                    return;
                }
                this.connectionReaders.Add(item);
            }
            item.StartReading(this.viaDelegate, timeout);
        }

        private void OnSingletonPreambleKnown(ServerSingletonPreambleConnectionReader serverSingletonPreambleReader)
        {
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    return;
                }
                this.connectionReaders.Remove(serverSingletonPreambleReader);
            }
            ISingletonChannelListener listener = this.singletonPreambleCallback(serverSingletonPreambleReader);
            TimeoutHelper helper = new TimeoutHelper(listener.ReceiveTimeout);
            IConnection upgradedConnection = serverSingletonPreambleReader.CompletePreamble(helper.RemainingTime());
            RequestContext requestContext = new ServerSingletonConnectionReader(serverSingletonPreambleReader, upgradedConnection, this).ReceiveRequest(helper.RemainingTime());
            listener.ReceiveRequest(requestContext, serverSingletonPreambleReader.ConnectionDequeuedCallback, true);
        }

        private void PooledConnectionDequeuedCallback()
        {
            lock (this.ThisLock)
            {
                this.pooledConnectionCount--;
            }
        }

        public void ReuseConnection(IConnection connection, TimeSpan closeTimeout)
        {
            connection.ExceptionEventType = TraceEventType.Information;
            ConnectionModeReader modeReader = this.SetupModeReader(connection, true);
            if (modeReader != null)
            {
                if (this.reuseConnectionCallback == null)
                {
                    this.reuseConnectionCallback = new Action<object>(this.ReuseConnectionCallback);
                }
                ActionItem.Schedule(this.reuseConnectionCallback, new ReuseConnectionState(modeReader, closeTimeout));
            }
        }

        private void ReuseConnectionCallback(object state)
        {
            ReuseConnectionState state2 = (ReuseConnectionState) state;
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.pooledConnectionCount >= this.maxPooledConnections)
                {
                    flag = true;
                }
                else
                {
                    this.pooledConnectionCount++;
                }
            }
            if (flag)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x40006, System.ServiceModel.SR.GetString("TraceCodeServerMaxPooledConnectionsQuotaReached", new object[] { this.maxPooledConnections }), new StringTraceRecord("MaxOutboundConnectionsPerEndpoint", this.maxPooledConnections.ToString(CultureInfo.InvariantCulture)), this, null);
                }
                state2.ModeReader.CloseFromPool(state2.CloseTimeout);
            }
            else
            {
                if (this.pooledConnectionDequeuedCallback == null)
                {
                    this.pooledConnectionDequeuedCallback = new Action(this.PooledConnectionDequeuedCallback);
                }
                state2.ModeReader.StartReading(this.idleTimeout, this.pooledConnectionDequeuedCallback);
            }
        }

        private ConnectionModeReader SetupModeReader(IConnection connection, bool isCached)
        {
            ConnectionModeReader reader;
            if (isCached)
            {
                if (this.onCachedConnectionModeKnown == null)
                {
                    this.onCachedConnectionModeKnown = new ConnectionModeCallback(this.OnCachedConnectionModeKnown);
                }
                reader = new ConnectionModeReader(connection, this.onCachedConnectionModeKnown, this.onConnectionClosed);
            }
            else
            {
                if (this.onConnectionModeKnown == null)
                {
                    this.onConnectionModeKnown = new ConnectionModeCallback(this.OnConnectionModeKnown);
                }
                reader = new ConnectionModeReader(connection, this.onConnectionModeKnown, this.onConnectionClosed);
            }
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    reader.Dispose();
                    return null;
                }
                this.connectionReaders.Add(reader);
                return reader;
            }
        }

        public void StartDemuxing()
        {
            this.StartDemuxing(null);
        }

        public void StartDemuxing(Action<Uri> viaDelegate)
        {
            this.viaDelegate = viaDelegate;
            this.acceptor.StartAccepting();
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class ReuseConnectionState
        {
            private TimeSpan closeTimeout;
            private ConnectionModeReader modeReader;

            public ReuseConnectionState(ConnectionModeReader modeReader, TimeSpan closeTimeout)
            {
                this.modeReader = modeReader;
                this.closeTimeout = closeTimeout;
            }

            public TimeSpan CloseTimeout
            {
                get
                {
                    return this.closeTimeout;
                }
            }

            public ConnectionModeReader ModeReader
            {
                get
                {
                    return this.modeReader;
                }
            }
        }
    }
}

