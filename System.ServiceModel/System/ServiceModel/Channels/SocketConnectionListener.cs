namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class SocketConnectionListener : IConnectionListener, IDisposable
    {
        private ConnectionBufferPool connectionBufferPool;
        private bool isDisposed;
        private bool isListening;
        private Socket listenSocket;
        private IPEndPoint localEndpoint;
        private ISocketListenerSettings settings;
        private SocketAsyncEventArgsPool socketAsyncEventArgsPool;
        private bool useOnlyOverlappedIO;

        private SocketConnectionListener(ISocketListenerSettings settings, bool useOnlyOverlappedIO)
        {
            this.settings = settings;
            this.useOnlyOverlappedIO = useOnlyOverlappedIO;
            this.connectionBufferPool = new ConnectionBufferPool(settings.BufferSize);
        }

        public SocketConnectionListener(IPEndPoint localEndpoint, ISocketListenerSettings settings, bool useOnlyOverlappedIO) : this(settings, useOnlyOverlappedIO)
        {
            this.localEndpoint = localEndpoint;
        }

        public SocketConnectionListener(Socket listenSocket, ISocketListenerSettings settings, bool useOnlyOverlappedIO) : this(settings, useOnlyOverlappedIO)
        {
            this.listenSocket = listenSocket;
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            return new AcceptAsyncResult(this, callback, state);
        }

        public static Exception ConvertListenException(SocketException socketException, IPEndPoint localEndpoint)
        {
            if (socketException.ErrorCode == 6)
            {
                return new CommunicationObjectAbortedException(socketException.Message, socketException);
            }
            if (socketException.ErrorCode == 0x2740)
            {
                return new AddressAlreadyInUseException(System.ServiceModel.SR.GetString("TcpAddressInUse", new object[] { localEndpoint.ToString() }), socketException);
            }
            return new CommunicationException(System.ServiceModel.SR.GetString("TcpListenError", new object[] { socketException.ErrorCode, socketException.Message, localEndpoint.ToString() }), socketException);
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                if (!this.isDisposed)
                {
                    if (this.listenSocket != null)
                    {
                        this.listenSocket.Close();
                    }
                    this.isDisposed = true;
                }
            }
        }

        public IConnection EndAccept(IAsyncResult result)
        {
            Socket socket = AcceptAsyncResult.End(result);
            if (socket == null)
            {
                return null;
            }
            if (this.useOnlyOverlappedIO)
            {
                socket.UseOnlyOverlappedIO = true;
            }
            return new SocketConnection(socket, this.connectionBufferPool, false);
        }

        private static int GetAcceptBufferSize(Socket listenSocket)
        {
            return ((listenSocket.LocalEndPoint.Serialize().Size + 0x10) * 2);
        }

        private bool InternalBeginAccept(Func<Socket, bool> acceptAsyncFunc)
        {
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString(), System.ServiceModel.SR.GetString("SocketListenerDisposed")));
                }
                if (!this.isListening)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SocketListenerNotListening")));
                }
                return acceptAsyncFunc(this.listenSocket);
            }
        }

        public void Listen()
        {
            BackoffTimeoutHelper helper = new BackoffTimeoutHelper(TimeSpan.FromSeconds(1.0));
            lock (this.ThisLock)
            {
                if (this.listenSocket != null)
                {
                    this.listenSocket.Listen(this.settings.ListenBacklog);
                    this.isListening = true;
                }
                while (!this.isListening)
                {
                    try
                    {
                        this.listenSocket = new Socket(this.localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        if ((this.localEndpoint.AddressFamily == AddressFamily.InterNetworkV6) && this.settings.TeredoEnabled)
                        {
                            this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPProtectionLevel, 10);
                        }
                        this.listenSocket.Bind(this.localEndpoint);
                        this.listenSocket.Listen(this.settings.ListenBacklog);
                        this.isListening = true;
                        continue;
                    }
                    catch (SocketException exception)
                    {
                        bool flag = false;
                        if ((exception.ErrorCode == 0x2740) && !helper.IsExpired())
                        {
                            helper.WaitAndBackoff();
                            flag = true;
                        }
                        if (!flag)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertListenException(exception, this.localEndpoint));
                        }
                        continue;
                    }
                }
                this.socketAsyncEventArgsPool = new SocketAsyncEventArgsPool(GetAcceptBufferSize(this.listenSocket));
            }
        }

        private void ReturnSocketAsyncEventArgs(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            this.socketAsyncEventArgsPool.Return(socketAsyncEventArgs);
        }

        private SocketAsyncEventArgs TakeSocketAsyncEventArgs()
        {
            return this.socketAsyncEventArgsPool.Take();
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class AcceptAsyncResult : AsyncResult
        {
            private static EventHandler<SocketAsyncEventArgs> acceptAsyncCompleted = new EventHandler<SocketAsyncEventArgs>(SocketConnectionListener.AcceptAsyncResult.AcceptAsyncCompleted);
            private SocketConnectionListener listener;
            private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(SocketConnectionListener.AcceptAsyncResult.OnInternalCompleting);
            private Socket socket;
            private SocketAsyncEventArgs socketAsyncEventArgs;
            private static Action<object> startAccept;

            public AcceptAsyncResult(SocketConnectionListener listener, AsyncCallback callback, object state) : base(callback, state)
            {
                this.listener = listener;
                this.socketAsyncEventArgs = listener.TakeSocketAsyncEventArgs();
                this.socketAsyncEventArgs.UserToken = this;
                this.socketAsyncEventArgs.Completed += acceptAsyncCompleted;
                base.OnCompleting = onCompleting;
                if (!Thread.CurrentThread.IsThreadPoolThread)
                {
                    if (startAccept == null)
                    {
                        startAccept = new Action<object>(SocketConnectionListener.AcceptAsyncResult.StartAccept);
                    }
                    ActionItem.Schedule(startAccept, this);
                }
                else
                {
                    bool flag;
                    bool flag2 = false;
                    try
                    {
                        flag = this.StartAccept();
                        flag2 = true;
                    }
                    finally
                    {
                        if (!flag2)
                        {
                            this.ReturnSocketAsyncEventArgs();
                        }
                    }
                    if (flag)
                    {
                        base.Complete(true);
                    }
                }
            }

            private static void AcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
            {
                SocketConnectionListener.AcceptAsyncResult userToken = (SocketConnectionListener.AcceptAsyncResult) e.UserToken;
                Exception exception = userToken.HandleAcceptAsyncCompleted();
                if ((exception != null) && ShouldAcceptRecover((SocketException) exception))
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                    StartAccept(userToken);
                }
                else
                {
                    userToken.Complete(false, exception);
                }
            }

            private bool DoAcceptAsync(Socket listenSocket)
            {
                if (listenSocket.AcceptAsync(this.socketAsyncEventArgs))
                {
                    return false;
                }
                Exception exception = this.HandleAcceptAsyncCompleted();
                if (exception != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
                return true;
            }

            public static Socket End(IAsyncResult result)
            {
                return AsyncResult.End<SocketConnectionListener.AcceptAsyncResult>(result).socket;
            }

            private Exception HandleAcceptAsyncCompleted()
            {
                Exception exception = null;
                if (this.socketAsyncEventArgs.SocketError == SocketError.Success)
                {
                    this.socket = this.socketAsyncEventArgs.AcceptSocket;
                }
                else
                {
                    exception = new SocketException((int) this.socketAsyncEventArgs.SocketError);
                }
                this.socketAsyncEventArgs.AcceptSocket = null;
                return exception;
            }

            private static void OnInternalCompleting(AsyncResult result, Exception exception)
            {
                (result as SocketConnectionListener.AcceptAsyncResult).ReturnSocketAsyncEventArgs();
            }

            private void ReturnSocketAsyncEventArgs()
            {
                if (this.socketAsyncEventArgs != null)
                {
                    this.socketAsyncEventArgs.UserToken = null;
                    this.socketAsyncEventArgs.AcceptSocket = null;
                    this.socketAsyncEventArgs.Completed -= acceptAsyncCompleted;
                    this.listener.ReturnSocketAsyncEventArgs(this.socketAsyncEventArgs);
                    this.socketAsyncEventArgs = null;
                }
            }

            private static bool ShouldAcceptRecover(SocketException exception)
            {
                if (((exception.ErrorCode != 0x2746) && (exception.ErrorCode != 0x2728)) && (exception.ErrorCode != 0x2747))
                {
                    return (exception.ErrorCode == 0x274c);
                }
                return true;
            }

            private bool StartAccept()
            {
                bool flag;
            Label_0000:
                try
                {
                    flag = this.listener.InternalBeginAccept(new Func<Socket, bool>(this.DoAcceptAsync));
                }
                catch (SocketException exception)
                {
                    if (!ShouldAcceptRecover(exception))
                    {
                        throw;
                    }
                    goto Label_0000;
                }
                return flag;
            }

            private static void StartAccept(object state)
            {
                bool flag;
                SocketConnectionListener.AcceptAsyncResult result = (SocketConnectionListener.AcceptAsyncResult) state;
                Exception exception = null;
                try
                {
                    flag = result.StartAccept();
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
        }
    }
}

