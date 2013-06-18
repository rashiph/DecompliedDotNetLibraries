namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Threading;

    internal class SocketConnectionInitiator : IConnectionInitiator
    {
        private int bufferSize;
        private ConnectionBufferPool connectionBufferPool;

        public SocketConnectionInitiator(int bufferSize)
        {
            this.bufferSize = bufferSize;
            this.connectionBufferPool = new ConnectionBufferPool(bufferSize);
        }

        public IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4002b, System.ServiceModel.SR.GetString("TraceCodeInitiatingTcpConnection"), new StringTraceRecord("Uri", uri.ToString()), this, null);
            }
            return new ConnectAsyncResult(uri, timeout, callback, state);
        }

        public IConnection Connect(Uri uri, TimeSpan timeout)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4002b, System.ServiceModel.SR.GetString("TraceCodeInitiatingTcpConnection"), new StringTraceRecord("Uri", uri.ToString()), this, null);
            }
            int port = uri.Port;
            IPAddress[] iPAddresses = GetIPAddresses(uri);
            Socket socket = null;
            SocketException innerException = null;
            if (port == -1)
            {
                port = 0x328;
            }
            int invalidAddressCount = 0;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            for (int i = 0; i < iPAddresses.Length; i++)
            {
                if (helper.RemainingTime() == TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateTimeoutException(uri, helper.OriginalTimeout, iPAddresses, invalidAddressCount, innerException));
                }
                AddressFamily addressFamily = iPAddresses[i].AddressFamily;
                if ((addressFamily == AddressFamily.InterNetworkV6) && !Socket.OSSupportsIPv6)
                {
                    iPAddresses[i] = null;
                }
                else
                {
                    DateTime utcNow = DateTime.UtcNow;
                    try
                    {
                        socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(new IPEndPoint(iPAddresses[i], port));
                        innerException = null;
                        break;
                    }
                    catch (SocketException exception2)
                    {
                        invalidAddressCount++;
                        TraceConnectFailure(socket, exception2, uri, (TimeSpan) (DateTime.UtcNow - utcNow));
                        innerException = exception2;
                        socket.Close();
                    }
                }
            }
            if (socket == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("NoIPEndpointsFoundForHost", new object[] { uri.Host })));
            }
            if (innerException != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertConnectException(innerException, uri, helper.ElapsedTime(), innerException));
            }
            return this.CreateConnection(socket);
        }

        public static Exception ConvertConnectException(SocketException socketException, Uri remoteUri, TimeSpan timeSpent, Exception innerException)
        {
            if (socketException.ErrorCode == 6)
            {
                return new CommunicationObjectAbortedException(socketException.Message, socketException);
            }
            if ((((socketException.ErrorCode == 0x2741) || (socketException.ErrorCode == 0x274d)) || ((socketException.ErrorCode == 0x2742) || (socketException.ErrorCode == 0x2743))) || (((socketException.ErrorCode == 0x2750) || (socketException.ErrorCode == 0x2751)) || (socketException.ErrorCode == 0x274c)))
            {
                if (timeSpent == TimeSpan.MaxValue)
                {
                    return new EndpointNotFoundException(System.ServiceModel.SR.GetString("TcpConnectError", new object[] { remoteUri.AbsoluteUri, socketException.ErrorCode, socketException.Message }), innerException);
                }
                return new EndpointNotFoundException(System.ServiceModel.SR.GetString("TcpConnectErrorWithTimeSpan", new object[] { remoteUri.AbsoluteUri, socketException.ErrorCode, socketException.Message, timeSpent }), innerException);
            }
            if (socketException.ErrorCode == 0x2747)
            {
                return new InsufficientMemoryException(System.ServiceModel.SR.GetString("TcpConnectNoBufs"), innerException);
            }
            if (((socketException.ErrorCode == 8) || (socketException.ErrorCode == 0x5aa)) || (socketException.ErrorCode == 14))
            {
                return new InsufficientMemoryException(System.ServiceModel.SR.GetString("InsufficentMemory"), socketException);
            }
            if (timeSpent == TimeSpan.MaxValue)
            {
                return new CommunicationException(System.ServiceModel.SR.GetString("TcpConnectError", new object[] { remoteUri.AbsoluteUri, socketException.ErrorCode, socketException.Message }), innerException);
            }
            return new CommunicationException(System.ServiceModel.SR.GetString("TcpConnectErrorWithTimeSpan", new object[] { remoteUri.AbsoluteUri, socketException.ErrorCode, socketException.Message, timeSpent }), innerException);
        }

        private IConnection CreateConnection(Socket socket)
        {
            return new SocketConnection(socket, this.connectionBufferPool, false);
        }

        private static TimeoutException CreateTimeoutException(Uri uri, TimeSpan timeout, IPAddress[] addresses, int invalidAddressCount, SocketException innerException)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < invalidAddressCount; i++)
            {
                if (addresses[i] != null)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(addresses[i].ToString());
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TcpConnectingToViaTimedOut", new object[] { uri.AbsoluteUri, timeout.ToString(), invalidAddressCount, addresses.Length, builder.ToString() }), innerException));
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            Socket socket = ConnectAsyncResult.End(result);
            return this.CreateConnection(socket);
        }

        private static IPAddress[] GetIPAddresses(Uri uri)
        {
            if ((uri.HostNameType == UriHostNameType.IPv4) || (uri.HostNameType == UriHostNameType.IPv6))
            {
                IPAddress address = IPAddress.Parse(uri.DnsSafeHost);
                return new IPAddress[] { address };
            }
            IPHostEntry entry = null;
            try
            {
                entry = DnsCache.Resolve(uri.Host);
            }
            catch (SocketException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("UnableToResolveHost", new object[] { uri.Host }), exception));
            }
            if (entry.AddressList.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("UnableToResolveHost", new object[] { uri.Host })));
            }
            return entry.AddressList;
        }

        public static void TraceConnectFailure(Socket socket, SocketException socketException, Uri remoteUri, TimeSpan timeSpentInConnect)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Exception exception = ConvertConnectException(socketException, remoteUri, timeSpentInConnect, socketException);
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x4002f, System.ServiceModel.SR.GetString("TraceCodeTcpConnectError"), socket, exception);
            }
        }

        private class ConnectAsyncResult : AsyncResult
        {
            private IPAddress[] addresses;
            private DateTime connectStartTime;
            private int currentIndex;
            private int invalidAddressCount;
            private SocketException lastException;
            private static AsyncCallback onConnect = Fx.ThunkCallback(new AsyncCallback(SocketConnectionInitiator.ConnectAsyncResult.OnConnect));
            private int port;
            private Socket socket;
            private static Action<object> startConnectCallback;
            private TimeSpan timeout;
            private TimeoutHelper timeoutHelper;
            private Uri uri;

            public ConnectAsyncResult(Uri uri, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.uri = uri;
                this.addresses = SocketConnectionInitiator.GetIPAddresses(uri);
                this.port = uri.Port;
                if (this.port == -1)
                {
                    this.port = 0x328;
                }
                this.currentIndex = 0;
                this.timeout = timeout;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (Thread.CurrentThread.IsThreadPoolThread)
                {
                    if (this.StartConnect())
                    {
                        base.Complete(true);
                    }
                }
                else
                {
                    if (startConnectCallback == null)
                    {
                        startConnectCallback = new Action<object>(SocketConnectionInitiator.ConnectAsyncResult.StartConnectCallback);
                    }
                    ActionItem.Schedule(startConnectCallback, this);
                }
            }

            public static Socket End(IAsyncResult result)
            {
                return AsyncResult.End<SocketConnectionInitiator.ConnectAsyncResult>(result).socket;
            }

            private static void OnConnect(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag = false;
                    Exception exception = null;
                    SocketConnectionInitiator.ConnectAsyncResult asyncState = (SocketConnectionInitiator.ConnectAsyncResult) result.AsyncState;
                    try
                    {
                        asyncState.socket.EndConnect(result);
                        flag = true;
                    }
                    catch (SocketException exception2)
                    {
                        asyncState.TraceConnectFailure(exception2);
                        asyncState.lastException = exception2;
                        asyncState.currentIndex++;
                        try
                        {
                            flag = asyncState.StartConnect();
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception3;
                        }
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool StartConnect()
            {
                while (this.currentIndex < this.addresses.Length)
                {
                    if (this.timeoutHelper.RemainingTime() == TimeSpan.Zero)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(SocketConnectionInitiator.CreateTimeoutException(this.uri, this.timeoutHelper.OriginalTimeout, this.addresses, this.invalidAddressCount, this.lastException));
                    }
                    AddressFamily addressFamily = this.addresses[this.currentIndex].AddressFamily;
                    if ((addressFamily == AddressFamily.InterNetworkV6) && !Socket.OSSupportsIPv6)
                    {
                        this.addresses[this.currentIndex++] = null;
                    }
                    else
                    {
                        this.connectStartTime = DateTime.UtcNow;
                        try
                        {
                            IPEndPoint remoteEP = new IPEndPoint(this.addresses[this.currentIndex], this.port);
                            this.socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
                            IAsyncResult asyncResult = this.socket.BeginConnect(remoteEP, onConnect, this);
                            if (!asyncResult.CompletedSynchronously)
                            {
                                return false;
                            }
                            this.socket.EndConnect(asyncResult);
                            return true;
                        }
                        catch (SocketException exception)
                        {
                            this.invalidAddressCount++;
                            this.TraceConnectFailure(exception);
                            this.lastException = exception;
                            this.currentIndex++;
                            continue;
                        }
                    }
                }
                if (this.socket == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("NoIPEndpointsFoundForHost", new object[] { this.uri.Host })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(SocketConnectionInitiator.ConvertConnectException(this.lastException, this.uri, this.timeoutHelper.ElapsedTime(), this.lastException));
            }

            private static void StartConnectCallback(object state)
            {
                SocketConnectionInitiator.ConnectAsyncResult result = (SocketConnectionInitiator.ConnectAsyncResult) state;
                bool flag = false;
                Exception exception = null;
                try
                {
                    flag = result.StartConnect();
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
                    result.Complete(false, exception);
                }
            }

            private void TraceConnectFailure(SocketException exception)
            {
                SocketConnectionInitiator.TraceConnectFailure(this.socket, exception, this.uri, (TimeSpan) (DateTime.UtcNow - this.connectStartTime));
                this.socket.Close();
            }
        }
    }
}

