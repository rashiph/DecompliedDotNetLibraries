namespace System.Net.Sockets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Configuration;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public class Socket : IDisposable
    {
        private System.Net.Sockets.AddressFamily addressFamily;
        internal const int DefaultCloseTimeout = -1;
        private bool isListening;
        private object m_AcceptQueueOrConnectResult;
        private ManualResetEvent m_AsyncEvent;
        private AsyncEventBits m_BlockEventBits;
        private bool m_BoundToThreadPool;
        private CacheSet m_Caches;
        private int m_CloseTimeout;
        private DynamicWinsockMethods m_DynamicWinsockMethods;
        private SafeCloseSocket m_Handle;
        private int m_IntCleanedUp;
        private bool m_IsConnected;
        private bool m_IsDisconnected;
        private bool m_NonBlockingConnectInProgress;
        private EndPoint m_NonBlockingConnectRightEndPoint;
        private SocketAddress m_PermittedRemoteAddress;
        private bool m_ReceivingPacketInformation;
        private RegisteredWaitHandle m_RegisteredWait;
        internal EndPoint m_RemoteEndPoint;
        internal EndPoint m_RightEndPoint;
        private const int microcnv = 0xf4240;
        private static readonly int protocolInformationSize = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.WSAPROTOCOL_INFO));
        private System.Net.Sockets.ProtocolType protocolType;
        internal static bool s_Initialized;
        private static object s_InternalSyncObject;
        private static bool s_LoggingEnabled;
        internal static bool s_OSSupportsIPv6;
        internal static bool s_PerfCountersEnabled;
        private static WaitOrTimerCallback s_RegisteredWaitCallback;
        internal static bool s_SupportsIPv4;
        internal static bool s_SupportsIPv6;
        private System.Net.Sockets.SocketType socketType;
        private bool useOverlappedIO;
        internal static bool UseOverlappedIO;
        private bool willBlock;
        private bool willBlockInternal;

        private Socket(SafeCloseSocket fd)
        {
            this.willBlock = true;
            this.willBlockInternal = true;
            this.m_CloseTimeout = -1;
            s_LoggingEnabled = Logging.On;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Socket", (string) null);
            }
            InitializeSockets();
            if ((fd == null) || fd.IsInvalid)
            {
                throw new ArgumentException(SR.GetString("net_InvalidSocketHandle"));
            }
            this.m_Handle = fd;
            this.addressFamily = System.Net.Sockets.AddressFamily.Unknown;
            this.socketType = System.Net.Sockets.SocketType.Unknown;
            this.protocolType = System.Net.Sockets.ProtocolType.Unknown;
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Socket", (string) null);
            }
        }

        public unsafe Socket(SocketInformation socketInformation)
        {
            this.willBlock = true;
            this.willBlockInternal = true;
            this.m_CloseTimeout = -1;
            s_LoggingEnabled = Logging.On;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Socket", this.addressFamily);
            }
            ExceptionHelper.UnrestrictedSocketPermission.Demand();
            InitializeSockets();
            if ((socketInformation.ProtocolInformation == null) || (socketInformation.ProtocolInformation.Length < protocolInformationSize))
            {
                throw new ArgumentException(SR.GetString("net_sockets_invalid_socketinformation"), "socketInformation.ProtocolInformation");
            }
            fixed (byte* numRef = socketInformation.ProtocolInformation)
            {
                this.m_Handle = SafeCloseSocket.CreateWSASocket(numRef);
                UnsafeNclNativeMethods.OSSOCK.WSAPROTOCOL_INFO wsaprotocol_info = (UnsafeNclNativeMethods.OSSOCK.WSAPROTOCOL_INFO) Marshal.PtrToStructure((IntPtr) numRef, typeof(UnsafeNclNativeMethods.OSSOCK.WSAPROTOCOL_INFO));
                this.addressFamily = wsaprotocol_info.iAddressFamily;
                this.socketType = (System.Net.Sockets.SocketType) wsaprotocol_info.iSocketType;
                this.protocolType = (System.Net.Sockets.ProtocolType) wsaprotocol_info.iProtocol;
            }
            if (this.m_Handle.IsInvalid)
            {
                SocketException exception = new SocketException();
                if (exception.ErrorCode == 0x2726)
                {
                    throw new ArgumentException(SR.GetString("net_sockets_invalid_socketinformation"), "socketInformation");
                }
                throw exception;
            }
            if ((this.addressFamily != System.Net.Sockets.AddressFamily.InterNetwork) && (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6))
            {
                throw new NotSupportedException(SR.GetString("net_invalidversion"));
            }
            this.m_IsConnected = socketInformation.IsConnected;
            this.willBlock = !socketInformation.IsNonBlocking;
            this.InternalSetBlocking(this.willBlock);
            this.isListening = socketInformation.IsListening;
            this.UseOnlyOverlappedIO = socketInformation.UseOnlyOverlappedIO;
            if (socketInformation.RemoteEndPoint != null)
            {
                this.m_RightEndPoint = socketInformation.RemoteEndPoint;
                this.m_RemoteEndPoint = socketInformation.RemoteEndPoint;
            }
            else
            {
                SocketError notSocket;
                EndPoint any = null;
                if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    any = IPEndPoint.Any;
                }
                else if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    any = IPEndPoint.IPv6Any;
                }
                SocketAddress socketAddress = any.Serialize();
                try
                {
                    notSocket = UnsafeNclNativeMethods.OSSOCK.getsockname(this.m_Handle, socketAddress.m_Buffer, ref socketAddress.m_Size);
                }
                catch (ObjectDisposedException)
                {
                    notSocket = SocketError.NotSocket;
                }
                if (notSocket == SocketError.Success)
                {
                    try
                    {
                        this.m_RightEndPoint = any.Create(socketAddress);
                    }
                    catch
                    {
                    }
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Socket", (string) null);
            }
        }

        public Socket(System.Net.Sockets.AddressFamily addressFamily, System.Net.Sockets.SocketType socketType, System.Net.Sockets.ProtocolType protocolType)
        {
            this.willBlock = true;
            this.willBlockInternal = true;
            this.m_CloseTimeout = -1;
            s_LoggingEnabled = Logging.On;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Socket", addressFamily);
            }
            InitializeSockets();
            this.m_Handle = SafeCloseSocket.CreateWSASocket(addressFamily, socketType, protocolType);
            if (this.m_Handle.IsInvalid)
            {
                throw new SocketException();
            }
            this.addressFamily = addressFamily;
            this.socketType = socketType;
            this.protocolType = protocolType;
            IPProtectionLevel iPProtectionLevel = SettingsSectionInternal.Section.IPProtectionLevel;
            if (iPProtectionLevel != IPProtectionLevel.Unspecified)
            {
                this.SetIPProtectionLevel(iPProtectionLevel);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Socket", (string) null);
            }
        }

        public Socket Accept()
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Accept", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (this.m_RightEndPoint == null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustbind"));
            }
            if (!this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustlisten"));
            }
            if (this.m_IsDisconnected)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_disconnectedAccept"));
            }
            this.ValidateBlockingMode();
            SocketAddress socketAddress = this.m_RightEndPoint.Serialize();
            SafeCloseSocket fd = SafeCloseSocket.Accept(this.m_Handle, socketAddress.m_Buffer, ref socketAddress.m_Size);
            if (fd.IsInvalid)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Accept", socketException);
                }
                throw socketException;
            }
            Socket socket2 = this.CreateAcceptSocket(fd, this.m_RightEndPoint.Create(socketAddress), false);
            if (s_LoggingEnabled)
            {
                Logging.PrintInfo(Logging.Sockets, socket2, SR.GetString("net_log_socket_accepted", new object[] { socket2.RemoteEndPoint, socket2.LocalEndPoint }));
                Logging.Exit(Logging.Sockets, this, "Accept", socket2);
            }
            return socket2;
        }

        public bool AcceptAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            int num;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "AcceptAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (e.m_BufferList != null)
            {
                throw new ArgumentException(SR.GetString("net_multibuffernotsupported"), "BufferList");
            }
            if (this.m_RightEndPoint == null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustbind"));
            }
            if (!this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustlisten"));
            }
            if (e.AcceptSocket == null)
            {
                e.AcceptSocket = new Socket(this.addressFamily, this.socketType, this.protocolType);
            }
            else if ((e.AcceptSocket.m_RightEndPoint != null) && !e.AcceptSocket.m_IsDisconnected)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_namedmustnotbebound", new object[] { "AcceptSocket" }));
            }
            e.StartOperationCommon(this);
            e.StartOperationAccept();
            this.BindToCompletionPort();
            SocketError success = SocketError.Success;
            try
            {
                if (!this.AcceptEx(this.m_Handle, e.AcceptSocket.m_Handle, (e.m_PtrSingleBuffer != IntPtr.Zero) ? e.m_PtrSingleBuffer : e.m_PtrAcceptBuffer, (e.m_PtrSingleBuffer != IntPtr.Zero) ? (e.Count - e.m_AcceptAddressBufferCount) : 0, e.m_AcceptAddressBufferCount / 2, e.m_AcceptAddressBufferCount / 2, out num, e.m_PtrNativeOverlapped))
                {
                    success = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            catch (Exception exception)
            {
                e.Complete();
                throw exception;
            }
            if ((success != SocketError.Success) && (success != SocketError.IOPending))
            {
                e.FinishOperationSyncFailure(success, num, SocketFlags.None);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "AcceptAsync", flag);
            }
            return flag;
        }

        private void AcceptCallback(object nullState)
        {
            bool flag = true;
            System.Collections.Queue acceptQueue = this.GetAcceptQueue();
            while (flag)
            {
                LazyAsyncResult result = null;
                SocketError operationAborted = SocketError.OperationAborted;
                SocketAddress socketAddress = null;
                SafeCloseSocket fd = null;
                Exception exception = null;
                object obj2 = null;
                lock (this)
                {
                    if (acceptQueue.Count == 0)
                    {
                        break;
                    }
                    result = (LazyAsyncResult) acceptQueue.Peek();
                    if (!this.CleanedUp)
                    {
                        socketAddress = this.m_RightEndPoint.Serialize();
                        try
                        {
                            fd = SafeCloseSocket.Accept(this.m_Handle, socketAddress.m_Buffer, ref socketAddress.m_Size);
                            operationAborted = fd.IsInvalid ? ((SocketError) Marshal.GetLastWin32Error()) : SocketError.Success;
                        }
                        catch (ObjectDisposedException)
                        {
                            operationAborted = SocketError.OperationAborted;
                        }
                        catch (Exception exception2)
                        {
                            if (NclUtilities.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                    }
                    if ((operationAborted == SocketError.WouldBlock) && (exception == null))
                    {
                        if (this.SetAsyncEventSelect(AsyncEventBits.FdAccept))
                        {
                            break;
                        }
                        exception = new ObjectDisposedException(base.GetType().FullName);
                    }
                    if (exception != null)
                    {
                        obj2 = exception;
                    }
                    else if (operationAborted == SocketError.Success)
                    {
                        obj2 = this.CreateAcceptSocket(fd, this.m_RightEndPoint.Create(socketAddress), true);
                    }
                    else
                    {
                        result.ErrorCode = (int) operationAborted;
                    }
                    acceptQueue.Dequeue();
                    if (acceptQueue.Count == 0)
                    {
                        if (!this.CleanedUp)
                        {
                            this.UnsetAsyncEventSelect();
                        }
                        flag = false;
                    }
                }
                try
                {
                    result.InvokeCallback(obj2);
                    continue;
                }
                catch
                {
                    if (flag)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.AcceptCallback), nullState);
                    }
                    throw;
                }
            }
        }

        private bool AcceptEx(SafeCloseSocket listenSocketHandle, SafeCloseSocket acceptSocketHandle, IntPtr buffer, int len, int localAddressLength, int remoteAddressLength, out int bytesReceived, System.Runtime.InteropServices.SafeHandle overlapped)
        {
            this.EnsureDynamicWinsockMethods();
            return this.m_DynamicWinsockMethods.GetDelegate<AcceptExDelegate>(listenSocketHandle)(listenSocketHandle, acceptSocketHandle, buffer, len, localAddressLength, remoteAddressLength, out bytesReceived, overlapped);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            if (this.CanUseAcceptEx)
            {
                return this.BeginAccept(0, callback, state);
            }
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginAccept", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            AcceptAsyncResult asyncResult = new AcceptAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            this.DoBeginAccept(asyncResult);
            asyncResult.FinishPostingAsyncOp(ref this.Caches.AcceptClosureCache);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginAccept", asyncResult);
            }
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginAccept(int receiveSize, AsyncCallback callback, object state)
        {
            return this.BeginAccept(null, receiveSize, callback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginAccept(Socket acceptSocket, int receiveSize, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginAccept", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (receiveSize < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            AcceptOverlappedAsyncResult asyncResult = new AcceptOverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            this.DoBeginAccept(acceptSocket, receiveSize, asyncResult);
            asyncResult.FinishPostingAsyncOp(ref this.Caches.AcceptClosureCache);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginAccept", asyncResult);
            }
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginConnect", remoteEP);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustnotlisten"));
            }
            DnsEndPoint point = remoteEP as DnsEndPoint;
            if (point != null)
            {
                if ((point.AddressFamily != System.Net.Sockets.AddressFamily.Unspecified) && (point.AddressFamily != this.addressFamily))
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                return this.BeginConnect(point.Host, point.Port, callback, state);
            }
            if (this.CanUseConnectEx(remoteEP))
            {
                return this.BeginConnectEx(remoteEP, true, callback, state);
            }
            EndPoint point2 = remoteEP;
            SocketAddress socketAddress = this.CheckCacheRemote(ref point2, true);
            ConnectAsyncResult asyncResult = new ConnectAsyncResult(this, point2, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            this.DoBeginConnect(point2, socketAddress, asyncResult);
            asyncResult.FinishPostingAsyncOp(ref this.Caches.ConnectClosureCache);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginConnect", asyncResult);
            }
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginConnect", address);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if (this.addressFamily != address.AddressFamily)
            {
                throw new NotSupportedException(SR.GetString("net_invalidversion"));
            }
            IAsyncResult retObject = this.BeginConnect(new IPEndPoint(address, port), requestCallback, state);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginConnect", retObject);
            }
            return retObject;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginConnect", host);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if ((this.addressFamily != System.Net.Sockets.AddressFamily.InterNetwork) && (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6))
            {
                throw new NotSupportedException(SR.GetString("net_invalidversion"));
            }
            if (this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustnotlisten"));
            }
            MultipleAddressConnectAsyncResult result = new MultipleAddressConnectAsyncResult(null, port, this, state, requestCallback);
            result.StartPostingAsyncOp(false);
            IAsyncResult result2 = Dns.UnsafeBeginGetHostAddresses(host, new AsyncCallback(Socket.DnsCallback), result);
            if (result2.CompletedSynchronously && DoDnsCallback(result2, result))
            {
                result.InvokeCallback();
            }
            result.FinishPostingAsyncOp(ref this.Caches.ConnectClosureCache);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginConnect", result);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginConnect", addresses);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (addresses == null)
            {
                throw new ArgumentNullException("addresses");
            }
            if (addresses.Length == 0)
            {
                throw new ArgumentException(SR.GetString("net_invalidAddressList"), "addresses");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if ((this.addressFamily != System.Net.Sockets.AddressFamily.InterNetwork) && (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6))
            {
                throw new NotSupportedException(SR.GetString("net_invalidversion"));
            }
            if (this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustnotlisten"));
            }
            MultipleAddressConnectAsyncResult context = new MultipleAddressConnectAsyncResult(addresses, port, this, state, requestCallback);
            context.StartPostingAsyncOp(false);
            if (DoMultipleAddressConnectCallback(PostOneBeginConnect(context), context))
            {
                context.InvokeCallback();
            }
            context.FinishPostingAsyncOp(ref this.Caches.ConnectClosureCache);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginConnect", context);
            }
            return context;
        }

        private IAsyncResult BeginConnectEx(EndPoint remoteEP, bool flowContext, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginConnectEx", "");
            }
            EndPoint point = remoteEP;
            SocketAddress address = flowContext ? this.CheckCacheRemote(ref point, true) : this.SnapshotAndSerialize(ref point);
            if (this.m_RightEndPoint == null)
            {
                if (point.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    this.InternalBind(new IPEndPoint(IPAddress.Any, 0));
                }
                else
                {
                    this.InternalBind(new IPEndPoint(IPAddress.IPv6Any, 0));
                }
            }
            ConnectOverlappedAsyncResult retObject = new ConnectOverlappedAsyncResult(this, point, state, callback);
            if (flowContext)
            {
                retObject.StartPostingAsyncOp(false);
            }
            retObject.SetUnmanagedStructures(address.m_Buffer);
            EndPoint rightEndPoint = this.m_RightEndPoint;
            if (this.m_RightEndPoint == null)
            {
                this.m_RightEndPoint = point;
            }
            SocketError success = SocketError.Success;
            try
            {
                int num;
                if (!this.ConnectEx(this.m_Handle, Marshal.UnsafeAddrOfPinnedArrayElement(address.m_Buffer, 0), address.m_Size, IntPtr.Zero, 0, out num, retObject.OverlappedHandle))
                {
                    success = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            catch
            {
                retObject.InternalCleanup();
                this.m_RightEndPoint = rightEndPoint;
                throw;
            }
            if (success == SocketError.Success)
            {
                this.SetToConnected();
            }
            success = retObject.CheckAsyncCallOverlappedResult(success);
            if (success != SocketError.Success)
            {
                this.m_RightEndPoint = rightEndPoint;
                SocketException socketException = new SocketException(success);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginConnectEx", socketException);
                }
                throw socketException;
            }
            retObject.FinishPostingAsyncOp(ref this.Caches.ConnectClosureCache);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginConnectEx", retObject);
            }
            return retObject;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginDisconnect(bool reuseSocket, AsyncCallback callback, object state)
        {
            DisconnectOverlappedAsyncResult asyncResult = new DisconnectOverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            this.DoBeginDisconnect(reuseSocket, asyncResult);
            asyncResult.FinishPostingAsyncOp();
            return asyncResult;
        }

        private IAsyncResult BeginDownLevelSendFile(string fileName, bool flowContext, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginSendFile", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!this.Connected)
            {
                throw new NotSupportedException(SR.GetString("net_notconnected"));
            }
            FileStream stream = null;
            if ((fileName != null) && (fileName.Length > 0))
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            DownLevelSendFileAsyncResult result = null;
            IAsyncResult result2 = null;
            try
            {
                result = new DownLevelSendFileAsyncResult(stream, this, state, callback);
                if (flowContext)
                {
                    result.StartPostingAsyncOp(false);
                }
                result2 = stream.BeginRead(result.buffer, 0, result.buffer.Length, new AsyncCallback(Socket.DownLevelSendFileCallback), result);
            }
            catch (Exception exception)
            {
                if (!NclUtilities.IsFatal(exception))
                {
                    DownLevelSendFileCleanup(stream);
                }
                throw;
            }
            if (result2.CompletedSynchronously)
            {
                DoDownLevelSendFileCallback(result2, result);
            }
            result.FinishPostingAsyncOp(ref this.Caches.SendClosureCache);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginSendFile", 0);
            }
            return result;
        }

        internal IAsyncResult BeginMultipleSend(BufferOffsetSize[] buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            this.DoBeginMultipleSend(buffers, socketFlags, asyncResult);
            asyncResult.FinishPostingAsyncOp(ref this.Caches.SendClosureCache);
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginReceive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            SocketError error;
            IAsyncResult result = this.BeginReceive(buffers, socketFlags, out error, callback, state);
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                throw new SocketException(error);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginReceive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginReceive", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }
            if (buffers.Count == 0)
            {
                throw new ArgumentException(SR.GetString("net_sockets_zerolist", new object[] { "buffers" }), "buffers");
            }
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            errorCode = this.DoBeginReceive(buffers, socketFlags, asyncResult);
            if ((errorCode != SocketError.Success) && (errorCode != SocketError.IOPending))
            {
                asyncResult = null;
            }
            else
            {
                asyncResult.FinishPostingAsyncOp(ref this.Caches.ReceiveClosureCache);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginReceive", asyncResult);
            }
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            SocketError error;
            IAsyncResult result = this.BeginReceive(buffer, offset, size, socketFlags, out error, callback, state);
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                throw new SocketException(error);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginReceive", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            errorCode = this.DoBeginReceive(buffer, offset, size, socketFlags, asyncResult);
            if ((errorCode != SocketError.Success) && (errorCode != SocketError.IOPending))
            {
                asyncResult = null;
            }
            else
            {
                asyncResult.FinishPostingAsyncOp(ref this.Caches.ReceiveClosureCache);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginReceive", asyncResult);
            }
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginReceiveFrom", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (remoteEP.AddressFamily != this.addressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidEndPointAddressFamily", new object[] { remoteEP.AddressFamily, this.addressFamily }), "remoteEP");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (this.m_RightEndPoint == null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustbind"));
            }
            EndPoint point = remoteEP;
            SocketAddress socketAddress = this.SnapshotAndSerialize(ref point);
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            this.DoBeginReceiveFrom(buffer, offset, size, socketFlags, point, socketAddress, asyncResult);
            asyncResult.FinishPostingAsyncOp(ref this.Caches.ReceiveClosureCache);
            if (asyncResult.CompletedSynchronously && !asyncResult.SocketAddressOriginal.Equals(asyncResult.SocketAddress))
            {
                try
                {
                    remoteEP = point.Create(asyncResult.SocketAddress);
                }
                catch
                {
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginReceiveFrom", asyncResult);
            }
            return asyncResult;
        }

        public IAsyncResult BeginReceiveMessageFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginReceiveMessageFrom", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsPostWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (remoteEP.AddressFamily != this.addressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidEndPointAddressFamily", new object[] { remoteEP.AddressFamily, this.addressFamily }), "remoteEP");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (this.m_RightEndPoint == null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustbind"));
            }
            ReceiveMessageOverlappedAsyncResult retObject = new ReceiveMessageOverlappedAsyncResult(this, state, callback);
            retObject.StartPostingAsyncOp(false);
            EndPoint rightEndPoint = this.m_RightEndPoint;
            EndPoint point2 = remoteEP;
            SocketAddress socketAddress = this.SnapshotAndSerialize(ref point2);
            SocketError socketError = SocketError.SocketError;
            try
            {
                int num;
                retObject.SetUnmanagedStructures(buffer, offset, size, socketAddress, socketFlags, ref this.Caches.ReceiveOverlappedCache);
                retObject.SocketAddressOriginal = point2.Serialize();
                this.SetReceivingPacketInformation();
                if (this.m_RightEndPoint == null)
                {
                    this.m_RightEndPoint = point2;
                }
                socketError = this.WSARecvMsg(this.m_Handle, Marshal.UnsafeAddrOfPinnedArrayElement(retObject.m_MessageBuffer, 0), out num, retObject.OverlappedHandle, IntPtr.Zero);
                if (socketError != SocketError.Success)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                    if (socketError == SocketError.MessageSize)
                    {
                        socketError = SocketError.IOPending;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                this.m_RightEndPoint = rightEndPoint;
                throw;
            }
            finally
            {
                socketError = retObject.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                this.m_RightEndPoint = rightEndPoint;
                retObject.ExtractCache(ref this.Caches.ReceiveOverlappedCache);
                SocketException socketException = new SocketException(socketError);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginReceiveMessageFrom", socketException);
                }
                throw socketException;
            }
            retObject.FinishPostingAsyncOp(ref this.Caches.ReceiveClosureCache);
            if (retObject.CompletedSynchronously && !retObject.SocketAddressOriginal.Equals(retObject.SocketAddress))
            {
                try
                {
                    remoteEP = point2.Create(retObject.SocketAddress);
                }
                catch
                {
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginReceiveMessageFrom", retObject);
            }
            return retObject;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSend(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            SocketError error;
            IAsyncResult result = this.BeginSend(buffers, socketFlags, out error, callback, state);
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                throw new SocketException(error);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSend(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginSend", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }
            if (buffers.Count == 0)
            {
                throw new ArgumentException(SR.GetString("net_sockets_zerolist", new object[] { "buffers" }), "buffers");
            }
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            errorCode = this.DoBeginSend(buffers, socketFlags, asyncResult);
            asyncResult.FinishPostingAsyncOp(ref this.Caches.SendClosureCache);
            if ((errorCode != SocketError.Success) && (errorCode != SocketError.IOPending))
            {
                asyncResult = null;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginSend", asyncResult);
            }
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            SocketError error;
            IAsyncResult result = this.BeginSend(buffer, offset, size, socketFlags, out error, callback, state);
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                throw new SocketException(error);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginSend", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            errorCode = this.DoBeginSend(buffer, offset, size, socketFlags, asyncResult);
            if ((errorCode != SocketError.Success) && (errorCode != SocketError.IOPending))
            {
                asyncResult = null;
            }
            else
            {
                asyncResult.FinishPostingAsyncOp(ref this.Caches.SendClosureCache);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginSend", asyncResult);
            }
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSendFile(string fileName, AsyncCallback callback, object state)
        {
            if (!ComNetOS.IsWinNt)
            {
                return this.BeginDownLevelSendFile(fileName, true, callback, state);
            }
            return this.BeginSendFile(fileName, null, null, TransmitFileOptions.UseDefaultWorkerThread, callback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSendFile(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags, AsyncCallback callback, object state)
        {
            TransmitFileOverlappedAsyncResult asyncResult = new TransmitFileOverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            this.DoBeginSendFile(fileName, preBuffer, postBuffer, flags, asyncResult);
            asyncResult.FinishPostingAsyncOp(ref this.Caches.SendClosureCache);
            return asyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginSendTo", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            EndPoint point = remoteEP;
            SocketAddress socketAddress = this.CheckCacheRemote(ref point, false);
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            asyncResult.StartPostingAsyncOp(false);
            this.DoBeginSendTo(buffer, offset, size, socketFlags, point, socketAddress, asyncResult);
            asyncResult.FinishPostingAsyncOp(ref this.Caches.SendClosureCache);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginSendTo", asyncResult);
            }
            return asyncResult;
        }

        public void Bind(EndPoint localEP)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Bind", localEP);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP");
            }
            EndPoint remoteEP = localEP;
            IPEndPoint point2 = localEP as IPEndPoint;
            if (point2 != null)
            {
                point2 = point2.Snapshot();
                remoteEP = point2;
                new SocketPermission(NetworkAccess.Accept, this.Transport, point2.Address.ToString(), point2.Port).Demand();
            }
            else
            {
                ExceptionHelper.UnmanagedPermission.Demand();
            }
            SocketAddress socketAddress = this.CallSerializeCheckDnsEndPoint(remoteEP);
            this.DoBind(remoteEP, socketAddress);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Bind", "");
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal void BindToCompletionPort()
        {
            if (!this.m_BoundToThreadPool && !UseOverlappedIO)
            {
                lock (this)
                {
                    if (!this.m_BoundToThreadPool)
                    {
                        try
                        {
                            ThreadPool.BindHandle(this.m_Handle);
                            this.m_BoundToThreadPool = true;
                        }
                        catch (Exception exception)
                        {
                            if (NclUtilities.IsFatal(exception))
                            {
                                throw;
                            }
                            this.Close(0);
                            throw;
                        }
                    }
                }
            }
        }

        private SocketAddress CallSerializeCheckDnsEndPoint(EndPoint remoteEP)
        {
            if (remoteEP is DnsEndPoint)
            {
                throw new ArgumentException(SR.GetString("net_sockets_invalid_dnsendpoint", new object[] { "remoteEP" }), "remoteEP");
            }
            return remoteEP.Serialize();
        }

        public static void CancelConnectAsync(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            e.CancelConnectAsync();
        }

        private bool CanUseConnectEx(EndPoint remoteEP)
        {
            if ((!ComNetOS.IsPostWin2K || (this.socketType != System.Net.Sockets.SocketType.Stream)) || ((this.m_RightEndPoint == null) && !(remoteEP.GetType() == typeof(IPEndPoint))))
            {
                return false;
            }
            if (!Thread.CurrentThread.IsThreadPoolThread && !SettingsSectionInternal.Section.AlwaysUseCompletionPortsForConnect)
            {
                return this.m_IsDisconnected;
            }
            return true;
        }

        private SocketAddress CheckCacheRemote(ref EndPoint remoteEP, bool isOverwrite)
        {
            IPEndPoint point = remoteEP as IPEndPoint;
            if (point != null)
            {
                point = point.Snapshot();
                remoteEP = point;
            }
            SocketAddress address = this.CallSerializeCheckDnsEndPoint(remoteEP);
            SocketAddress permittedRemoteAddress = this.m_PermittedRemoteAddress;
            if ((permittedRemoteAddress != null) && permittedRemoteAddress.Equals(address))
            {
                return permittedRemoteAddress;
            }
            if (point != null)
            {
                new SocketPermission(NetworkAccess.Connect, this.Transport, point.Address.ToString(), point.Port).Demand();
            }
            else
            {
                ExceptionHelper.UnmanagedPermission.Demand();
            }
            if ((this.m_PermittedRemoteAddress == null) || isOverwrite)
            {
                this.m_PermittedRemoteAddress = address;
            }
            return address;
        }

        private void CheckSetOptionPermissions(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            if ((((optionLevel != SocketOptionLevel.Tcp) || (((optionName != SocketOptionName.Debug) && (optionName != SocketOptionName.AcceptConnection)) && (optionName != SocketOptionName.AcceptConnection))) && ((optionLevel != SocketOptionLevel.Udp) || ((optionName != SocketOptionName.Debug) && (optionName != SocketOptionName.ChecksumCoverage)))) && (((optionLevel != SocketOptionLevel.Socket) || ((((optionName != SocketOptionName.KeepAlive) && (optionName != SocketOptionName.Linger)) && ((optionName != SocketOptionName.DontLinger) && (optionName != SocketOptionName.SendBuffer))) && (((optionName != SocketOptionName.ReceiveBuffer) && (optionName != SocketOptionName.SendTimeout)) && ((optionName != SocketOptionName.ExclusiveAddressUse) && (optionName != SocketOptionName.ReceiveTimeout))))) && ((optionLevel != SocketOptionLevel.IPv6) || (optionName != SocketOptionName.IPProtectionLevel))))
            {
                ExceptionHelper.UnmanagedPermission.Demand();
            }
        }

        public void Close()
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Close", (string) null);
            }
            this.Dispose();
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Close", (string) null);
            }
        }

        public void Close(int timeout)
        {
            if (timeout < -1)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            this.m_CloseTimeout = timeout;
            this.Dispose();
        }

        private void CompleteAcceptResults(object nullState)
        {
            System.Collections.Queue acceptQueue = this.GetAcceptQueue();
            bool flag = true;
            while (flag)
            {
                LazyAsyncResult result = null;
                lock (this)
                {
                    if (acceptQueue.Count == 0)
                    {
                        break;
                    }
                    result = (LazyAsyncResult) acceptQueue.Dequeue();
                    if (acceptQueue.Count == 0)
                    {
                        flag = false;
                    }
                }
                try
                {
                    result.InvokeCallback(new SocketException(SocketError.OperationAborted));
                    continue;
                }
                catch
                {
                    if (flag)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.CompleteAcceptResults), null);
                    }
                    throw;
                }
            }
        }

        public void Connect(EndPoint remoteEP)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (this.m_IsDisconnected)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_disconnectedConnect"));
            }
            if (this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustnotlisten"));
            }
            this.ValidateBlockingMode();
            DnsEndPoint point = remoteEP as DnsEndPoint;
            if (point != null)
            {
                if ((point.AddressFamily != System.Net.Sockets.AddressFamily.Unspecified) && (point.AddressFamily != this.addressFamily))
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                this.Connect(point.Host, point.Port);
            }
            else
            {
                EndPoint point2 = remoteEP;
                SocketAddress socketAddress = this.CheckCacheRemote(ref point2, true);
                if (!this.Blocking)
                {
                    this.m_NonBlockingConnectRightEndPoint = point2;
                    this.m_NonBlockingConnectInProgress = true;
                }
                this.DoConnect(point2, socketAddress);
            }
        }

        public void Connect(IPAddress address, int port)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Connect", address);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if (this.addressFamily != address.AddressFamily)
            {
                throw new NotSupportedException(SR.GetString("net_invalidversion"));
            }
            IPEndPoint remoteEP = new IPEndPoint(address, port);
            this.Connect(remoteEP);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Connect", (string) null);
            }
        }

        public void Connect(string host, int port)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Connect", host);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if ((this.addressFamily != System.Net.Sockets.AddressFamily.InterNetwork) && (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6))
            {
                throw new NotSupportedException(SR.GetString("net_invalidversion"));
            }
            IPAddress[] hostAddresses = Dns.GetHostAddresses(host);
            this.Connect(hostAddresses, port);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Connect", (string) null);
            }
        }

        public void Connect(IPAddress[] addresses, int port)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Connect", addresses);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (addresses == null)
            {
                throw new ArgumentNullException("addresses");
            }
            if (addresses.Length == 0)
            {
                throw new ArgumentException(SR.GetString("net_sockets_invalid_ipaddress_length"), "addresses");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if ((this.addressFamily != System.Net.Sockets.AddressFamily.InterNetwork) && (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6))
            {
                throw new NotSupportedException(SR.GetString("net_invalidversion"));
            }
            Exception exception = null;
            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == this.addressFamily)
                {
                    try
                    {
                        this.Connect(new IPEndPoint(address, port));
                        exception = null;
                        break;
                    }
                    catch (Exception exception2)
                    {
                        if (NclUtilities.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                }
            }
            if (exception != null)
            {
                throw exception;
            }
            if (!this.Connected)
            {
                throw new ArgumentException(SR.GetString("net_invalidAddressList"), "addresses");
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Connect", (string) null);
            }
        }

        public bool ConnectAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "ConnectAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (e.m_BufferList != null)
            {
                throw new ArgumentException(SR.GetString("net_multibuffernotsupported"), "BufferList");
            }
            if (e.RemoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustnotlisten"));
            }
            EndPoint remoteEndPoint = e.RemoteEndPoint;
            DnsEndPoint endPoint = remoteEndPoint as DnsEndPoint;
            if (endPoint != null)
            {
                if (s_LoggingEnabled)
                {
                    Logging.PrintInfo(Logging.Sockets, "Socket#" + ValidationHelper.HashString(this) + "::ConnectAsync Connecting to a DnsEndPoint");
                }
                if ((endPoint.AddressFamily != System.Net.Sockets.AddressFamily.Unspecified) && (endPoint.AddressFamily != this.addressFamily))
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                MultipleConnectAsync args = new SingleSocketMultipleConnectAsync(this, true);
                e.StartOperationCommon(this);
                e.StartOperationWrapperConnect(args);
                flag = args.StartConnectAsync(e, endPoint);
            }
            else
            {
                int num;
                if (this.addressFamily != e.RemoteEndPoint.AddressFamily)
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                e.m_SocketAddress = this.CheckCacheRemote(ref remoteEndPoint, false);
                if (this.m_RightEndPoint == null)
                {
                    if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        this.InternalBind(new IPEndPoint(IPAddress.Any, 0));
                    }
                    else
                    {
                        this.InternalBind(new IPEndPoint(IPAddress.IPv6Any, 0));
                    }
                }
                EndPoint rightEndPoint = this.m_RightEndPoint;
                if (this.m_RightEndPoint == null)
                {
                    this.m_RightEndPoint = remoteEndPoint;
                }
                e.StartOperationCommon(this);
                e.StartOperationConnect();
                this.BindToCompletionPort();
                SocketError success = SocketError.Success;
                try
                {
                    if (!this.ConnectEx(this.m_Handle, e.m_PtrSocketAddressBuffer, e.m_SocketAddress.m_Size, e.m_PtrSingleBuffer, e.Count, out num, e.m_PtrNativeOverlapped))
                    {
                        success = (SocketError) Marshal.GetLastWin32Error();
                    }
                }
                catch (Exception exception)
                {
                    this.m_RightEndPoint = rightEndPoint;
                    e.Complete();
                    throw exception;
                }
                if ((success != SocketError.Success) && (success != SocketError.IOPending))
                {
                    e.FinishOperationSyncFailure(success, num, SocketFlags.None);
                    flag = false;
                }
                else
                {
                    flag = true;
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "ConnectAsync", flag);
            }
            return flag;
        }

        public static bool ConnectAsync(System.Net.Sockets.SocketType socketType, System.Net.Sockets.ProtocolType protocolType, SocketAsyncEventArgs e)
        {
            bool flag;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, (string) null, "ConnectAsync", "");
            }
            if (e.m_BufferList != null)
            {
                throw new ArgumentException(SR.GetString("net_multibuffernotsupported"), "BufferList");
            }
            if (e.RemoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            EndPoint remoteEndPoint = e.RemoteEndPoint;
            DnsEndPoint endPoint = remoteEndPoint as DnsEndPoint;
            if (endPoint != null)
            {
                Socket socket = null;
                MultipleConnectAsync args = null;
                if (endPoint.AddressFamily == System.Net.Sockets.AddressFamily.Unspecified)
                {
                    args = new MultipleSocketMultipleConnectAsync(socketType, protocolType);
                }
                else
                {
                    socket = new Socket(endPoint.AddressFamily, socketType, protocolType);
                    args = new SingleSocketMultipleConnectAsync(socket, false);
                }
                e.StartOperationCommon(socket);
                e.StartOperationWrapperConnect(args);
                flag = args.StartConnectAsync(e, endPoint);
            }
            else
            {
                flag = new Socket(remoteEndPoint.AddressFamily, socketType, protocolType).ConnectAsync(e);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, (string) null, "ConnectAsync", flag);
            }
            return flag;
        }

        private void ConnectCallback()
        {
            LazyAsyncResult acceptQueueOrConnectResult = (LazyAsyncResult) this.m_AcceptQueueOrConnectResult;
            if (!acceptQueueOrConnectResult.InternalPeekCompleted)
            {
                NetworkEvents networkEvents = new NetworkEvents {
                    Events = AsyncEventBits.FdConnect
                };
                SocketError operationAborted = SocketError.OperationAborted;
                object result = null;
                try
                {
                    if (!this.CleanedUp)
                    {
                        try
                        {
                            operationAborted = UnsafeNclNativeMethods.OSSOCK.WSAEnumNetworkEvents(this.m_Handle, this.m_AsyncEvent.SafeWaitHandle, ref networkEvents);
                            if (operationAborted != SocketError.Success)
                            {
                                operationAborted = (SocketError) Marshal.GetLastWin32Error();
                            }
                            else
                            {
                                operationAborted = (SocketError) networkEvents.ErrorCodes[4];
                            }
                            this.UnsetAsyncEventSelect();
                        }
                        catch (ObjectDisposedException)
                        {
                            operationAborted = SocketError.OperationAborted;
                        }
                    }
                    if (operationAborted == SocketError.Success)
                    {
                        this.SetToConnected();
                    }
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    result = exception;
                }
                if (!acceptQueueOrConnectResult.InternalPeekCompleted)
                {
                    acceptQueueOrConnectResult.ErrorCode = (int) operationAborted;
                    acceptQueueOrConnectResult.InvokeCallback(result);
                }
            }
        }

        private bool ConnectEx(SafeCloseSocket socketHandle, IntPtr socketAddress, int socketAddressSize, IntPtr buffer, int dataLength, out int bytesSent, System.Runtime.InteropServices.SafeHandle overlapped)
        {
            this.EnsureDynamicWinsockMethods();
            return this.m_DynamicWinsockMethods.GetDelegate<ConnectExDelegate>(socketHandle)(socketHandle, socketAddress, socketAddressSize, buffer, dataLength, out bytesSent, overlapped);
        }

        private Socket CreateAcceptSocket(SafeCloseSocket fd, EndPoint remoteEP, bool needCancelSelect)
        {
            Socket socket = new Socket(fd);
            return this.UpdateAcceptSocket(socket, remoteEP, needCancelSelect);
        }

        public void Disconnect(bool reuseSocket)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Disconnect", (string) null);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsPostWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
            }
            SocketError success = SocketError.Success;
            if (!this.DisconnectEx_Blocking(this.m_Handle.DangerousGetHandle(), IntPtr.Zero, reuseSocket ? 2 : 0, 0))
            {
                success = (SocketError) Marshal.GetLastWin32Error();
            }
            if (success != SocketError.Success)
            {
                SocketException socketException = new SocketException(success);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Disconnect", socketException);
                }
                throw socketException;
            }
            this.SetToDisconnected();
            this.m_RemoteEndPoint = null;
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Disconnect", (string) null);
            }
        }

        public bool DisconnectAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "DisconnectAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            e.StartOperationCommon(this);
            e.StartOperationDisconnect();
            this.BindToCompletionPort();
            SocketError success = SocketError.Success;
            try
            {
                if (!this.DisconnectEx(this.m_Handle, e.m_PtrNativeOverlapped, e.DisconnectReuseSocket ? 2 : 0, 0))
                {
                    success = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            catch (Exception exception)
            {
                e.Complete();
                throw exception;
            }
            if ((success != SocketError.Success) && (success != SocketError.IOPending))
            {
                e.FinishOperationSyncFailure(success, 0, SocketFlags.None);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "DisconnectAsync", flag);
            }
            return flag;
        }

        private bool DisconnectEx(SafeCloseSocket socketHandle, System.Runtime.InteropServices.SafeHandle overlapped, int flags, int reserved)
        {
            this.EnsureDynamicWinsockMethods();
            return this.m_DynamicWinsockMethods.GetDelegate<DisconnectExDelegate>(socketHandle)(socketHandle, overlapped, flags, reserved);
        }

        private bool DisconnectEx_Blocking(IntPtr socketHandle, IntPtr overlapped, int flags, int reserved)
        {
            this.EnsureDynamicWinsockMethods();
            return this.m_DynamicWinsockMethods.GetDelegate<DisconnectExDelegate_Blocking>(this.m_Handle)(socketHandle, overlapped, flags, reserved);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                int num;
                try
                {
                    if (s_LoggingEnabled)
                    {
                        Logging.Enter(Logging.Sockets, this, "Dispose", (string) null);
                    }
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                }
                while ((num = Interlocked.CompareExchange(ref this.m_IntCleanedUp, 1, 0)) == 2)
                {
                    Thread.SpinWait(1);
                }
                if (num == 1)
                {
                    try
                    {
                        if (s_LoggingEnabled)
                        {
                            Logging.Exit(Logging.Sockets, this, "Dispose", (string) null);
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (NclUtilities.IsFatal(exception2))
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    this.SetToDisconnected();
                    AsyncEventBits fdNone = AsyncEventBits.FdNone;
                    if (this.m_BlockEventBits != AsyncEventBits.FdNone)
                    {
                        this.UnsetAsyncEventSelect();
                        if (this.m_BlockEventBits == AsyncEventBits.FdConnect)
                        {
                            LazyAsyncResult acceptQueueOrConnectResult = this.m_AcceptQueueOrConnectResult as LazyAsyncResult;
                            if ((acceptQueueOrConnectResult != null) && !acceptQueueOrConnectResult.InternalPeekCompleted)
                            {
                                fdNone = AsyncEventBits.FdConnect;
                            }
                        }
                        else if (this.m_BlockEventBits == AsyncEventBits.FdAccept)
                        {
                            System.Collections.Queue queue = this.m_AcceptQueueOrConnectResult as System.Collections.Queue;
                            if ((queue != null) && (queue.Count != 0))
                            {
                                fdNone = AsyncEventBits.FdAccept;
                            }
                        }
                    }
                    try
                    {
                        int closeTimeout = this.m_CloseTimeout;
                        if (closeTimeout == 0)
                        {
                            this.m_Handle.Dispose();
                        }
                        else
                        {
                            SocketError error;
                            if (!this.willBlock || !this.willBlockInternal)
                            {
                                int argp = 0;
                                error = UnsafeNclNativeMethods.OSSOCK.ioctlsocket(this.m_Handle, -2147195266, ref argp);
                            }
                            if (closeTimeout < 0)
                            {
                                this.m_Handle.CloseAsIs();
                            }
                            else
                            {
                                error = UnsafeNclNativeMethods.OSSOCK.shutdown(this.m_Handle, 1);
                                if (UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_Handle, SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ref closeTimeout, 4) != SocketError.Success)
                                {
                                    this.m_Handle.Dispose();
                                }
                                else if (UnsafeNclNativeMethods.OSSOCK.recv(this.m_Handle.DangerousGetHandle(), null, 0, SocketFlags.None) != 0)
                                {
                                    this.m_Handle.Dispose();
                                }
                                else
                                {
                                    int num4 = 0;
                                    if ((UnsafeNclNativeMethods.OSSOCK.ioctlsocket(this.m_Handle, 0x4004667f, ref num4) != SocketError.Success) || (num4 != 0))
                                    {
                                        this.m_Handle.Dispose();
                                    }
                                    else
                                    {
                                        this.m_Handle.CloseAsIs();
                                    }
                                }
                            }
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    if (this.m_Caches != null)
                    {
                        OverlappedCache.InterlockedFree(ref this.m_Caches.SendOverlappedCache);
                        OverlappedCache.InterlockedFree(ref this.m_Caches.ReceiveOverlappedCache);
                    }
                    switch (fdNone)
                    {
                        case AsyncEventBits.FdConnect:
                            ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(((LazyAsyncResult) this.m_AcceptQueueOrConnectResult).InvokeCallback), new SocketException(SocketError.OperationAborted));
                            break;

                        case AsyncEventBits.FdAccept:
                            ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.CompleteAcceptResults), null);
                            break;
                    }
                    if (this.m_AsyncEvent != null)
                    {
                        this.m_AsyncEvent.Close();
                    }
                }
            }
        }

        private static void DnsCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                bool flag = false;
                MultipleAddressConnectAsyncResult asyncState = (MultipleAddressConnectAsyncResult) result.AsyncState;
                try
                {
                    flag = DoDnsCallback(result, asyncState);
                }
                catch (Exception exception)
                {
                    asyncState.InvokeCallback(exception);
                }
                if (flag)
                {
                    asyncState.InvokeCallback();
                }
            }
        }

        private void DoBeginAccept(LazyAsyncResult asyncResult)
        {
            if (this.m_RightEndPoint == null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustbind"));
            }
            if (!this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustlisten"));
            }
            bool flag = false;
            SocketError success = SocketError.Success;
            System.Collections.Queue acceptQueue = this.GetAcceptQueue();
            lock (this)
            {
                if (acceptQueue.Count == 0)
                {
                    SocketAddress socketAddress = this.m_RightEndPoint.Serialize();
                    this.InternalSetBlocking(false);
                    SafeCloseSocket fd = null;
                    try
                    {
                        fd = SafeCloseSocket.Accept(this.m_Handle, socketAddress.m_Buffer, ref socketAddress.m_Size);
                        success = fd.IsInvalid ? ((SocketError) Marshal.GetLastWin32Error()) : SocketError.Success;
                    }
                    catch (ObjectDisposedException)
                    {
                        success = SocketError.NotSocket;
                    }
                    switch (success)
                    {
                        case SocketError.WouldBlock:
                            acceptQueue.Enqueue(asyncResult);
                            if (!this.SetAsyncEventSelect(AsyncEventBits.FdAccept))
                            {
                                acceptQueue.Dequeue();
                                throw new ObjectDisposedException(base.GetType().FullName);
                            }
                            goto Label_0117;

                        case SocketError.Success:
                            asyncResult.Result = this.CreateAcceptSocket(fd, this.m_RightEndPoint.Create(socketAddress), false);
                            break;

                        default:
                            asyncResult.ErrorCode = (int) success;
                            break;
                    }
                    this.InternalSetBlocking(true);
                    flag = true;
                }
                else
                {
                    acceptQueue.Enqueue(asyncResult);
                }
            }
        Label_0117:
            if (!flag)
            {
                return;
            }
            if (success == SocketError.Success)
            {
                asyncResult.InvokeCallback();
            }
            else
            {
                SocketException socketException = new SocketException(success);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginAccept", socketException);
                }
                throw socketException;
            }
        }

        private void DoBeginAccept(Socket acceptSocket, int receiveSize, AcceptOverlappedAsyncResult asyncResult)
        {
            int num2;
            if (!ComNetOS.IsWinNt)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
            if (this.m_RightEndPoint == null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustbind"));
            }
            if (!this.isListening)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustlisten"));
            }
            if (acceptSocket == null)
            {
                acceptSocket = new Socket(this.addressFamily, this.socketType, this.protocolType);
            }
            else if (acceptSocket.m_RightEndPoint != null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_namedmustnotbebound", new object[] { "acceptSocket" }));
            }
            asyncResult.AcceptSocket = acceptSocket;
            int addressBufferLength = this.m_RightEndPoint.Serialize().Size + 0x10;
            byte[] buffer = new byte[receiveSize + (addressBufferLength * 2)];
            asyncResult.SetUnmanagedStructures(buffer, addressBufferLength);
            SocketError success = SocketError.Success;
            if (!this.AcceptEx(this.m_Handle, acceptSocket.m_Handle, Marshal.UnsafeAddrOfPinnedArrayElement(asyncResult.Buffer, 0), receiveSize, addressBufferLength, addressBufferLength, out num2, asyncResult.OverlappedHandle))
            {
                success = (SocketError) Marshal.GetLastWin32Error();
            }
            success = asyncResult.CheckAsyncCallOverlappedResult(success);
            if (success != SocketError.Success)
            {
                SocketException socketException = new SocketException(success);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginAccept", socketException);
                }
                throw socketException;
            }
        }

        private void DoBeginConnect(EndPoint endPointSnapshot, SocketAddress socketAddress, LazyAsyncResult asyncResult)
        {
            EndPoint rightEndPoint = this.m_RightEndPoint;
            if (this.m_AcceptQueueOrConnectResult != null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_no_duplicate_async"));
            }
            this.m_AcceptQueueOrConnectResult = asyncResult;
            if (!this.SetAsyncEventSelect(AsyncEventBits.FdConnect))
            {
                this.m_AcceptQueueOrConnectResult = null;
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            IntPtr handle = this.m_Handle.DangerousGetHandle();
            if (this.m_RightEndPoint == null)
            {
                this.m_RightEndPoint = endPointSnapshot;
            }
            SocketError socketError = UnsafeNclNativeMethods.OSSOCK.WSAConnect(handle, socketAddress.m_Buffer, socketAddress.m_Size, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (socketError != SocketError.Success)
            {
                socketError = (SocketError) Marshal.GetLastWin32Error();
            }
            if (socketError != SocketError.WouldBlock)
            {
                bool flag = true;
                if (socketError == SocketError.Success)
                {
                    this.SetToConnected();
                }
                else
                {
                    asyncResult.ErrorCode = (int) socketError;
                }
                if (Interlocked.Exchange<RegisteredWaitHandle>(ref this.m_RegisteredWait, null) == null)
                {
                    flag = false;
                }
                this.UnsetAsyncEventSelect();
                if (socketError == SocketError.Success)
                {
                    if (flag)
                    {
                        asyncResult.InvokeCallback();
                    }
                }
                else
                {
                    this.m_RightEndPoint = rightEndPoint;
                    SocketException socketException = new SocketException(socketError);
                    this.UpdateStatusAfterSocketError(socketException);
                    this.m_AcceptQueueOrConnectResult = null;
                    if (s_LoggingEnabled)
                    {
                        Logging.Exception(Logging.Sockets, this, "BeginConnect", socketException);
                    }
                    throw socketException;
                }
            }
        }

        private void DoBeginDisconnect(bool reuseSocket, DisconnectOverlappedAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginDisconnect", (string) null);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsPostWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
            }
            asyncResult.SetUnmanagedStructures(null);
            SocketError success = SocketError.Success;
            if (!this.DisconnectEx(this.m_Handle, asyncResult.OverlappedHandle, reuseSocket ? 2 : 0, 0))
            {
                success = (SocketError) Marshal.GetLastWin32Error();
            }
            if (success == SocketError.Success)
            {
                this.SetToDisconnected();
                this.m_RemoteEndPoint = null;
            }
            success = asyncResult.CheckAsyncCallOverlappedResult(success);
            if (success != SocketError.Success)
            {
                SocketException socketException = new SocketException(success);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginDisconnect", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginDisconnect", asyncResult);
            }
        }

        private void DoBeginMultipleSend(BufferOffsetSize[] buffers, SocketFlags socketFlags, OverlappedAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginMultipleSend", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            SocketError socketError = SocketError.SocketError;
            try
            {
                int num;
                asyncResult.SetUnmanagedStructures(buffers, ref this.Caches.SendOverlappedCache);
                socketError = UnsafeNclNativeMethods.OSSOCK.WSASend(this.m_Handle, asyncResult.m_WSABuffers, asyncResult.m_WSABuffers.Length, out num, socketFlags, asyncResult.OverlappedHandle, IntPtr.Zero);
                if (socketError != SocketError.Success)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                socketError = asyncResult.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                asyncResult.ExtractCache(ref this.Caches.SendOverlappedCache);
                SocketException socketException = new SocketException(socketError);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginMultipleSend", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginMultipleSend", asyncResult);
            }
        }

        private SocketError DoBeginReceive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, OverlappedAsyncResult asyncResult)
        {
            SocketError socketError = SocketError.SocketError;
            try
            {
                int num;
                asyncResult.SetUnmanagedStructures(buffers, ref this.Caches.ReceiveOverlappedCache);
                socketError = UnsafeNclNativeMethods.OSSOCK.WSARecv(this.m_Handle, asyncResult.m_WSABuffers, asyncResult.m_WSABuffers.Length, out num, ref socketFlags, asyncResult.OverlappedHandle, IntPtr.Zero);
                if (socketError != SocketError.Success)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                socketError = asyncResult.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                asyncResult.ExtractCache(ref this.Caches.ReceiveOverlappedCache);
                this.UpdateStatusAfterSocketError(socketError);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginReceive", new SocketException(socketError));
                }
            }
            return socketError;
        }

        private SocketError DoBeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, OverlappedAsyncResult asyncResult)
        {
            SocketError socketError = SocketError.SocketError;
            try
            {
                int num;
                asyncResult.SetUnmanagedStructures(buffer, offset, size, null, false, ref this.Caches.ReceiveOverlappedCache);
                socketError = UnsafeNclNativeMethods.OSSOCK.WSARecv(this.m_Handle, ref asyncResult.m_SingleBuffer, 1, out num, ref socketFlags, asyncResult.OverlappedHandle, IntPtr.Zero);
                if (socketError != SocketError.Success)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                socketError = asyncResult.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                asyncResult.ExtractCache(ref this.Caches.ReceiveOverlappedCache);
                this.UpdateStatusAfterSocketError(socketError);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginReceive", new SocketException(socketError));
                }
                asyncResult.InvokeCallback(new SocketException(socketError));
            }
            return socketError;
        }

        private void DoBeginReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint endPointSnapshot, SocketAddress socketAddress, OverlappedAsyncResult asyncResult)
        {
            EndPoint rightEndPoint = this.m_RightEndPoint;
            SocketError socketError = SocketError.SocketError;
            try
            {
                int num;
                asyncResult.SetUnmanagedStructures(buffer, offset, size, socketAddress, true, ref this.Caches.ReceiveOverlappedCache);
                asyncResult.SocketAddressOriginal = endPointSnapshot.Serialize();
                if (this.m_RightEndPoint == null)
                {
                    this.m_RightEndPoint = endPointSnapshot;
                }
                socketError = UnsafeNclNativeMethods.OSSOCK.WSARecvFrom(this.m_Handle, ref asyncResult.m_SingleBuffer, 1, out num, ref socketFlags, asyncResult.GetSocketAddressPtr(), asyncResult.GetSocketAddressSizePtr(), asyncResult.OverlappedHandle, IntPtr.Zero);
                if (socketError != SocketError.Success)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            catch (ObjectDisposedException)
            {
                this.m_RightEndPoint = rightEndPoint;
                throw;
            }
            finally
            {
                socketError = asyncResult.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                this.m_RightEndPoint = rightEndPoint;
                asyncResult.ExtractCache(ref this.Caches.ReceiveOverlappedCache);
                SocketException socketException = new SocketException(socketError);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginReceiveFrom", socketException);
                }
                throw socketException;
            }
        }

        private SocketError DoBeginSend(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, OverlappedAsyncResult asyncResult)
        {
            SocketError socketError = SocketError.SocketError;
            try
            {
                int num;
                asyncResult.SetUnmanagedStructures(buffers, ref this.Caches.SendOverlappedCache);
                socketError = UnsafeNclNativeMethods.OSSOCK.WSASend(this.m_Handle, asyncResult.m_WSABuffers, asyncResult.m_WSABuffers.Length, out num, socketFlags, asyncResult.OverlappedHandle, IntPtr.Zero);
                if (socketError != SocketError.Success)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                socketError = asyncResult.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                asyncResult.ExtractCache(ref this.Caches.SendOverlappedCache);
                this.UpdateStatusAfterSocketError(socketError);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginSend", new SocketException(socketError));
                }
            }
            return socketError;
        }

        private SocketError DoBeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, OverlappedAsyncResult asyncResult)
        {
            SocketError socketError = SocketError.SocketError;
            try
            {
                int num;
                asyncResult.SetUnmanagedStructures(buffer, offset, size, null, false, ref this.Caches.SendOverlappedCache);
                socketError = UnsafeNclNativeMethods.OSSOCK.WSASend(this.m_Handle, ref asyncResult.m_SingleBuffer, 1, out num, socketFlags, asyncResult.OverlappedHandle, IntPtr.Zero);
                if (socketError != SocketError.Success)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                socketError = asyncResult.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                asyncResult.ExtractCache(ref this.Caches.SendOverlappedCache);
                this.UpdateStatusAfterSocketError(socketError);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginSend", new SocketException(socketError));
                }
            }
            return socketError;
        }

        private void DoBeginSendFile(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags, TransmitFileOverlappedAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "BeginSendFile", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsWinNt)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
            if (!this.Connected)
            {
                throw new NotSupportedException(SR.GetString("net_notconnected"));
            }
            FileStream fileStream = null;
            if ((fileName != null) && (fileName.Length > 0))
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            System.Runtime.InteropServices.SafeHandle fileHandle = null;
            if (fileStream != null)
            {
                ExceptionHelper.UnmanagedPermission.Assert();
                try
                {
                    fileHandle = fileStream.SafeFileHandle;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            SocketError socketError = SocketError.SocketError;
            try
            {
                asyncResult.SetUnmanagedStructures(preBuffer, postBuffer, fileStream, flags, ref this.Caches.SendOverlappedCache);
                bool flag = false;
                if (fileHandle != null)
                {
                    flag = UnsafeNclNativeMethods.OSSOCK.TransmitFile(this.m_Handle, fileHandle, 0, 0, asyncResult.OverlappedHandle, asyncResult.TransmitFileBuffers, flags);
                }
                else
                {
                    flag = UnsafeNclNativeMethods.OSSOCK.TransmitFile2(this.m_Handle, IntPtr.Zero, 0, 0, asyncResult.OverlappedHandle, asyncResult.TransmitFileBuffers, flags);
                }
                if (!flag)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                }
                else
                {
                    socketError = SocketError.Success;
                }
            }
            finally
            {
                socketError = asyncResult.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                asyncResult.ExtractCache(ref this.Caches.SendOverlappedCache);
                SocketException socketException = new SocketException(socketError);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginSendFile", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "BeginSendFile", socketError);
            }
        }

        private void DoBeginSendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint endPointSnapshot, SocketAddress socketAddress, OverlappedAsyncResult asyncResult)
        {
            EndPoint rightEndPoint = this.m_RightEndPoint;
            SocketError socketError = SocketError.SocketError;
            try
            {
                int num;
                asyncResult.SetUnmanagedStructures(buffer, offset, size, socketAddress, false, ref this.Caches.SendOverlappedCache);
                if (this.m_RightEndPoint == null)
                {
                    this.m_RightEndPoint = endPointSnapshot;
                }
                socketError = UnsafeNclNativeMethods.OSSOCK.WSASendTo(this.m_Handle, ref asyncResult.m_SingleBuffer, 1, out num, socketFlags, asyncResult.GetSocketAddressPtr(), asyncResult.SocketAddress.Size, asyncResult.OverlappedHandle, IntPtr.Zero);
                if (socketError != SocketError.Success)
                {
                    socketError = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            catch (ObjectDisposedException)
            {
                this.m_RightEndPoint = rightEndPoint;
                throw;
            }
            finally
            {
                socketError = asyncResult.CheckAsyncCallOverlappedResult(socketError);
            }
            if (socketError != SocketError.Success)
            {
                this.m_RightEndPoint = rightEndPoint;
                asyncResult.ExtractCache(ref this.Caches.SendOverlappedCache);
                SocketException socketException = new SocketException(socketError);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "BeginSendTo", socketException);
                }
                throw socketException;
            }
        }

        private void DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
        {
            if (UnsafeNclNativeMethods.OSSOCK.bind(this.m_Handle, socketAddress.m_Buffer, socketAddress.m_Size) != SocketError.Success)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "DoBind", socketException);
                }
                throw socketException;
            }
            if (this.m_RightEndPoint == null)
            {
                this.m_RightEndPoint = endPointSnapshot;
            }
        }

        private void DoConnect(EndPoint endPointSnapshot, SocketAddress socketAddress)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Connect", endPointSnapshot);
            }
            if (UnsafeNclNativeMethods.OSSOCK.WSAConnect(this.m_Handle.DangerousGetHandle(), socketAddress.m_Buffer, socketAddress.m_Size, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != SocketError.Success)
            {
                SocketException socketException = new SocketException(endPointSnapshot);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Connect", socketException);
                }
                throw socketException;
            }
            if (this.m_RightEndPoint == null)
            {
                this.m_RightEndPoint = endPointSnapshot;
            }
            this.SetToConnected();
            if (s_LoggingEnabled)
            {
                Logging.PrintInfo(Logging.Sockets, this, SR.GetString("net_log_socket_connected", new object[] { this.LocalEndPoint, this.RemoteEndPoint }));
                Logging.Exit(Logging.Sockets, this, "Connect", "");
            }
        }

        private static bool DoDnsCallback(IAsyncResult result, MultipleAddressConnectAsyncResult context)
        {
            IPAddress[] addressArray = Dns.EndGetHostAddresses(result);
            context.addresses = addressArray;
            return DoMultipleAddressConnectCallback(PostOneBeginConnect(context), context);
        }

        private static void DoDownLevelSendFileCallback(IAsyncResult result, DownLevelSendFileAsyncResult context)
        {
            try
            {
            Label_0000:
                if (!context.writing)
                {
                    int size = context.fileStream.EndRead(result);
                    if (size > 0)
                    {
                        context.writing = true;
                        result = context.socket.BeginSend(context.buffer, 0, size, SocketFlags.None, new AsyncCallback(Socket.DownLevelSendFileCallback), context);
                        if (result.CompletedSynchronously)
                        {
                            goto Label_0000;
                        }
                    }
                    else
                    {
                        DownLevelSendFileCleanup(context.fileStream);
                        context.InvokeCallback();
                    }
                }
                else
                {
                    context.socket.EndSend(result);
                    context.writing = false;
                    result = context.fileStream.BeginRead(context.buffer, 0, context.buffer.Length, new AsyncCallback(Socket.DownLevelSendFileCallback), context);
                    if (result.CompletedSynchronously)
                    {
                        goto Label_0000;
                    }
                }
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
                DownLevelSendFileCleanup(context.fileStream);
                context.InvokeCallback(exception);
            }
        }

        private static bool DoMultipleAddressConnectCallback(object result, MultipleAddressConnectAsyncResult context)
        {
            while (result != null)
            {
                Exception exception = result as Exception;
                if (exception == null)
                {
                    try
                    {
                        context.socket.EndConnect((IAsyncResult) result);
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                    }
                }
                if (exception == null)
                {
                    return true;
                }
                if (++context.index >= context.addresses.Length)
                {
                    throw exception;
                }
                context.lastException = exception;
                result = PostOneBeginConnect(context);
            }
            return false;
        }

        private void DownLevelSendFile(string fileName)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "SendFile", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!this.Connected)
            {
                throw new NotSupportedException(SR.GetString("net_notconnected"));
            }
            this.ValidateBlockingMode();
            FileStream fileStream = null;
            if ((fileName != null) && (fileName.Length > 0))
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            try
            {
                SocketError success = SocketError.Success;
                byte[] buffer = new byte[0xfa00];
                while (true)
                {
                    int size = fileStream.Read(buffer, 0, buffer.Length);
                    if (size == 0)
                    {
                        break;
                    }
                    this.Send(buffer, 0, size, SocketFlags.None);
                }
                if (s_LoggingEnabled)
                {
                    Logging.Exit(Logging.Sockets, this, "SendFile", success);
                }
            }
            finally
            {
                DownLevelSendFileCleanup(fileStream);
            }
        }

        private static void DownLevelSendFileCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                DownLevelSendFileAsyncResult asyncState = (DownLevelSendFileAsyncResult) result.AsyncState;
                DoDownLevelSendFileCallback(result, asyncState);
            }
        }

        private static void DownLevelSendFileCleanup(FileStream fileStream)
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }

        public unsafe SocketInformation DuplicateAndClose(int targetProcessId)
        {
            SocketError error;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "DuplicateAndClose", (string) null);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            ExceptionHelper.UnrestrictedSocketPermission.Demand();
            SocketInformation information = new SocketInformation {
                ProtocolInformation = new byte[protocolInformationSize]
            };
            fixed (byte* numRef = information.ProtocolInformation)
            {
                error = (SocketError) UnsafeNclNativeMethods.OSSOCK.WSADuplicateSocket(this.m_Handle, (uint) targetProcessId, numRef);
            }
            if (error != SocketError.Success)
            {
                SocketException e = new SocketException();
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "DuplicateAndClose", e);
                }
                throw e;
            }
            information.IsConnected = this.Connected;
            information.IsNonBlocking = !this.Blocking;
            information.IsListening = this.isListening;
            information.UseOnlyOverlappedIO = this.UseOnlyOverlappedIO;
            information.RemoteEndPoint = this.m_RemoteEndPoint;
            this.Close(-1);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "DuplicateAndClose", (string) null);
            }
            return information;
        }

        public Socket EndAccept(IAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndAccept", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if ((ComNetOS.IsWinNt && (asyncResult != null)) && (asyncResult is AcceptOverlappedAsyncResult))
            {
                int num;
                byte[] buffer;
                return this.EndAccept(out buffer, out num, asyncResult);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            AcceptAsyncResult result = asyncResult as AcceptAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndAccept" }));
            }
            object retObject = result.InternalWaitForCompletion();
            result.EndCalled = true;
            Exception exception = retObject as Exception;
            if (exception != null)
            {
                throw exception;
            }
            if (result.ErrorCode != 0)
            {
                SocketException socketException = new SocketException(result.ErrorCode);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndAccept", socketException);
                }
                throw socketException;
            }
            Socket socket = (Socket) retObject;
            if (s_LoggingEnabled)
            {
                Logging.PrintInfo(Logging.Sockets, socket, SR.GetString("net_log_socket_accepted", new object[] { socket.RemoteEndPoint, socket.LocalEndPoint }));
                Logging.Exit(Logging.Sockets, this, "EndAccept", retObject);
            }
            return socket;
        }

        public Socket EndAccept(out byte[] buffer, IAsyncResult asyncResult)
        {
            int num;
            byte[] buffer2;
            Socket socket = this.EndAccept(out buffer2, out num, asyncResult);
            buffer = new byte[num];
            Array.Copy(buffer2, buffer, num);
            return socket;
        }

        public Socket EndAccept(out byte[] buffer, out int bytesTransferred, IAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndAccept", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsWinNt)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            AcceptOverlappedAsyncResult result = asyncResult as AcceptOverlappedAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndAccept" }));
            }
            Socket socket = (Socket) result.InternalWaitForCompletion();
            bytesTransferred = result.BytesTransferred;
            buffer = result.Buffer;
            result.EndCalled = true;
            if (s_PerfCountersEnabled && (bytesTransferred > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesReceived, (long) bytesTransferred);
            }
            if (result.ErrorCode != 0)
            {
                SocketException socketException = new SocketException(result.ErrorCode);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndAccept", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.PrintInfo(Logging.Sockets, socket, SR.GetString("net_log_socket_accepted", new object[] { socket.RemoteEndPoint, socket.LocalEndPoint }));
                Logging.Exit(Logging.Sockets, this, "EndAccept", socket);
            }
            return socket;
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndConnect", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = null;
            EndPoint endPoint = null;
            ConnectOverlappedAsyncResult result2 = asyncResult as ConnectOverlappedAsyncResult;
            if (result2 == null)
            {
                MultipleAddressConnectAsyncResult result3 = asyncResult as MultipleAddressConnectAsyncResult;
                if (result3 == null)
                {
                    ConnectAsyncResult result4 = asyncResult as ConnectAsyncResult;
                    if (result4 != null)
                    {
                        endPoint = result4.RemoteEndPoint;
                        result = result4;
                    }
                }
                else
                {
                    endPoint = result3.RemoteEndPoint;
                    result = result3;
                }
            }
            else
            {
                endPoint = result2.RemoteEndPoint;
                result = result2;
            }
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndConnect" }));
            }
            result.InternalWaitForCompletion();
            result.EndCalled = true;
            this.m_AcceptQueueOrConnectResult = null;
            if (result.Result is Exception)
            {
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndConnect", (Exception) result.Result);
                }
                throw ((Exception) result.Result);
            }
            if (result.ErrorCode != 0)
            {
                SocketException socketException = new SocketException(result.ErrorCode, endPoint);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndConnect", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.PrintInfo(Logging.Sockets, this, SR.GetString("net_log_socket_connected", new object[] { this.LocalEndPoint, this.RemoteEndPoint }));
                Logging.Exit(Logging.Sockets, this, "EndConnect", "");
            }
        }

        public void EndDisconnect(IAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndDisconnect", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsPostWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as LazyAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndDisconnect" }));
            }
            result.InternalWaitForCompletion();
            result.EndCalled = true;
            if (result.ErrorCode != 0)
            {
                SocketException socketException = new SocketException(result.ErrorCode);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndDisconnect", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "EndDisconnect", (string) null);
            }
        }

        private void EndDownLevelSendFile(IAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndSendFile", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as DownLevelSendFileAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndSendFile" }));
            }
            result.InternalWaitForCompletion();
            result.EndCalled = true;
            Exception exception = result.Result as Exception;
            if (exception != null)
            {
                throw exception;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "EndSendFile", "");
            }
        }

        internal int EndMultipleSend(IAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndMultipleSend", asyncResult);
            }
            OverlappedAsyncResult result = asyncResult as OverlappedAsyncResult;
            int retObject = (int) result.InternalWaitForCompletion();
            result.EndCalled = true;
            result.ExtractCache(ref this.Caches.SendOverlappedCache);
            if (s_PerfCountersEnabled && (retObject > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesSent, (long) retObject);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsSent);
                }
            }
            if (result.ErrorCode != 0)
            {
                SocketException e = new SocketException(result.ErrorCode);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndMultipleSend", e);
                }
                throw e;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "EndMultipleSend", retObject);
            }
            return retObject;
        }

        public int EndReceive(IAsyncResult asyncResult)
        {
            SocketError error;
            int num = this.EndReceive(asyncResult, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException(error);
            }
            return num;
        }

        public int EndReceive(IAsyncResult asyncResult, out SocketError errorCode)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndReceive", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            OverlappedAsyncResult result = asyncResult as OverlappedAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndReceive" }));
            }
            int retObject = (int) result.InternalWaitForCompletion();
            result.EndCalled = true;
            result.ExtractCache(ref this.Caches.ReceiveOverlappedCache);
            if (s_PerfCountersEnabled && (retObject > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesReceived, (long) retObject);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsReceived);
                }
            }
            errorCode = (SocketError) result.ErrorCode;
            if (errorCode != SocketError.Success)
            {
                this.UpdateStatusAfterSocketError(errorCode);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndReceive", new SocketException(errorCode));
                    Logging.Exit(Logging.Sockets, this, "EndReceive", 0);
                }
                return 0;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "EndReceive", retObject);
            }
            return retObject;
        }

        public int EndReceiveFrom(IAsyncResult asyncResult, ref EndPoint endPoint)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndReceiveFrom", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }
            if (endPoint.AddressFamily != this.addressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidEndPointAddressFamily", new object[] { endPoint.AddressFamily, this.addressFamily }), "endPoint");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            OverlappedAsyncResult result = asyncResult as OverlappedAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndReceiveFrom" }));
            }
            SocketAddress address = this.CallSerializeCheckDnsEndPoint(endPoint);
            int retObject = (int) result.InternalWaitForCompletion();
            result.EndCalled = true;
            result.ExtractCache(ref this.Caches.ReceiveOverlappedCache);
            result.SocketAddress.SetSize(result.GetSocketAddressSizePtr());
            if (!address.Equals(result.SocketAddress))
            {
                try
                {
                    endPoint = endPoint.Create(result.SocketAddress);
                }
                catch
                {
                }
            }
            if (s_PerfCountersEnabled && (retObject > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesReceived, (long) retObject);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsReceived);
                }
            }
            if (result.ErrorCode != 0)
            {
                SocketException socketException = new SocketException(result.ErrorCode);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndReceiveFrom", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "EndReceiveFrom", retObject);
            }
            return retObject;
        }

        public int EndReceiveMessageFrom(IAsyncResult asyncResult, ref SocketFlags socketFlags, ref EndPoint endPoint, out IPPacketInformation ipPacketInformation)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndReceiveMessageFrom", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }
            if (endPoint.AddressFamily != this.addressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidEndPointAddressFamily", new object[] { endPoint.AddressFamily, this.addressFamily }), "endPoint");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            ReceiveMessageOverlappedAsyncResult result = asyncResult as ReceiveMessageOverlappedAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndReceiveMessageFrom" }));
            }
            SocketAddress address = this.CallSerializeCheckDnsEndPoint(endPoint);
            int retObject = (int) result.InternalWaitForCompletion();
            result.EndCalled = true;
            result.ExtractCache(ref this.Caches.ReceiveOverlappedCache);
            result.SocketAddress.SetSize(result.GetSocketAddressSizePtr());
            if (!address.Equals(result.SocketAddress))
            {
                try
                {
                    endPoint = endPoint.Create(result.SocketAddress);
                }
                catch
                {
                }
            }
            if (s_PerfCountersEnabled && (retObject > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesReceived, (long) retObject);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsReceived);
                }
            }
            if ((result.ErrorCode != 0) && (result.ErrorCode != 0x2738))
            {
                SocketException socketException = new SocketException(result.ErrorCode);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndReceiveMessageFrom", socketException);
                }
                throw socketException;
            }
            socketFlags = result.m_flags;
            ipPacketInformation = result.m_IPPacketInformation;
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "EndReceiveMessageFrom", retObject);
            }
            return retObject;
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            SocketError error;
            int num = this.EndSend(asyncResult, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException(error);
            }
            return num;
        }

        public int EndSend(IAsyncResult asyncResult, out SocketError errorCode)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndSend", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            OverlappedAsyncResult result = asyncResult as OverlappedAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndSend" }));
            }
            int retObject = (int) result.InternalWaitForCompletion();
            result.EndCalled = true;
            result.ExtractCache(ref this.Caches.SendOverlappedCache);
            if (s_PerfCountersEnabled && (retObject > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesSent, (long) retObject);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsSent);
                }
            }
            errorCode = (SocketError) result.ErrorCode;
            if (errorCode != SocketError.Success)
            {
                this.UpdateStatusAfterSocketError(errorCode);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndSend", new SocketException(errorCode));
                    Logging.Exit(Logging.Sockets, this, "EndSend", 0);
                }
                return 0;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "EndSend", retObject);
            }
            return retObject;
        }

        public void EndSendFile(IAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndSendFile", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsWinNt)
            {
                this.EndDownLevelSendFile(asyncResult);
            }
            else
            {
                if (!ComNetOS.IsWinNt)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
                }
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                TransmitFileOverlappedAsyncResult result = asyncResult as TransmitFileOverlappedAsyncResult;
                if ((result == null) || (result.AsyncObject != this))
                {
                    throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
                }
                if (result.EndCalled)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndSendFile" }));
                }
                result.InternalWaitForCompletion();
                result.EndCalled = true;
                result.ExtractCache(ref this.Caches.SendOverlappedCache);
                if ((result.Flags & (TransmitFileOptions.ReuseSocket | TransmitFileOptions.Disconnect)) != TransmitFileOptions.UseDefaultWorkerThread)
                {
                    this.SetToDisconnected();
                    this.m_RemoteEndPoint = null;
                }
                if (result.ErrorCode != 0)
                {
                    SocketException socketException = new SocketException(result.ErrorCode);
                    this.UpdateStatusAfterSocketError(socketException);
                    if (s_LoggingEnabled)
                    {
                        Logging.Exception(Logging.Sockets, this, "EndSendFile", socketException);
                    }
                    throw socketException;
                }
                if (s_LoggingEnabled)
                {
                    Logging.Exit(Logging.Sockets, this, "EndSendFile", "");
                }
            }
        }

        public int EndSendTo(IAsyncResult asyncResult)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "EndSendTo", asyncResult);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            OverlappedAsyncResult result = asyncResult as OverlappedAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndSendTo" }));
            }
            int retObject = (int) result.InternalWaitForCompletion();
            result.EndCalled = true;
            result.ExtractCache(ref this.Caches.SendOverlappedCache);
            if (s_PerfCountersEnabled && (retObject > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesSent, (long) retObject);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsSent);
                }
            }
            if (result.ErrorCode != 0)
            {
                SocketException socketException = new SocketException(result.ErrorCode);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "EndSendTo", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "EndSendTo", retObject);
            }
            return retObject;
        }

        private void EnsureDynamicWinsockMethods()
        {
            if (this.m_DynamicWinsockMethods == null)
            {
                this.m_DynamicWinsockMethods = DynamicWinsockMethods.GetMethods(this.addressFamily, this.socketType, this.protocolType);
            }
        }

        ~Socket()
        {
            this.Dispose(false);
        }

        internal void GetAcceptExSockaddrs(IntPtr buffer, int receiveDataLength, int localAddressLength, int remoteAddressLength, out IntPtr localSocketAddress, out int localSocketAddressLength, out IntPtr remoteSocketAddress, out int remoteSocketAddressLength)
        {
            this.EnsureDynamicWinsockMethods();
            this.m_DynamicWinsockMethods.GetDelegate<GetAcceptExSockaddrsDelegate>(this.m_Handle)(buffer, receiveDataLength, localAddressLength, remoteAddressLength, out localSocketAddress, out localSocketAddressLength, out remoteSocketAddress, out remoteSocketAddressLength);
        }

        private System.Collections.Queue GetAcceptQueue()
        {
            if (this.m_AcceptQueueOrConnectResult == null)
            {
                Interlocked.CompareExchange(ref this.m_AcceptQueueOrConnectResult, new System.Collections.Queue(0x10), null);
            }
            return (System.Collections.Queue) this.m_AcceptQueueOrConnectResult;
        }

        private IPv6MulticastOption getIPv6MulticastOpt(SocketOptionName optionName)
        {
            IPv6MulticastRequest optionValue = new IPv6MulticastRequest();
            int size = IPv6MulticastRequest.Size;
            if (UnsafeNclNativeMethods.OSSOCK.getsockopt(this.m_Handle, SocketOptionLevel.IP, optionName, out optionValue, ref size) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "getIPv6MulticastOpt", socketException);
                }
                throw socketException;
            }
            return new IPv6MulticastOption(new IPAddress(optionValue.MulticastAddress), (long) optionValue.InterfaceIndex);
        }

        private LingerOption getLingerOpt()
        {
            Linger optionValue = new Linger();
            int optionLength = 4;
            if (UnsafeNclNativeMethods.OSSOCK.getsockopt(this.m_Handle, SocketOptionLevel.Socket, SocketOptionName.Linger, out optionValue, ref optionLength) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "getLingerOpt", socketException);
                }
                throw socketException;
            }
            return new LingerOption(optionValue.OnOff != 0, optionValue.Time);
        }

        private MulticastOption getMulticastOpt(SocketOptionName optionName)
        {
            IPMulticastRequest optionValue = new IPMulticastRequest();
            int size = IPMulticastRequest.Size;
            if (UnsafeNclNativeMethods.OSSOCK.getsockopt(this.m_Handle, SocketOptionLevel.IP, optionName, out optionValue, ref size) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "getMulticastOpt", socketException);
                }
                throw socketException;
            }
            IPAddress group = new IPAddress(optionValue.MulticastAddress);
            return new MulticastOption(group, new IPAddress(optionValue.InterfaceAddress));
        }

        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if ((optionLevel == SocketOptionLevel.Socket) && (optionName == SocketOptionName.Linger))
            {
                return this.getLingerOpt();
            }
            if ((optionLevel == SocketOptionLevel.IP) && ((optionName == SocketOptionName.AddMembership) || (optionName == SocketOptionName.DropMembership)))
            {
                return this.getMulticastOpt(optionName);
            }
            if ((optionLevel == SocketOptionLevel.IPv6) && ((optionName == SocketOptionName.AddMembership) || (optionName == SocketOptionName.DropMembership)))
            {
                return this.getIPv6MulticastOpt(optionName);
            }
            int optionValue = 0;
            int optionLength = 4;
            if (UnsafeNclNativeMethods.OSSOCK.getsockopt(this.m_Handle, optionLevel, optionName, out optionValue, ref optionLength) != SocketError.SocketError)
            {
                return optionValue;
            }
            SocketException socketException = new SocketException();
            this.UpdateStatusAfterSocketError(socketException);
            if (s_LoggingEnabled)
            {
                Logging.Exception(Logging.Sockets, this, "GetSocketOption", socketException);
            }
            throw socketException;
        }

        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            int optionLength = (optionValue != null) ? optionValue.Length : 0;
            if (UnsafeNclNativeMethods.OSSOCK.getsockopt(this.m_Handle, optionLevel, optionName, optionValue, ref optionLength) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "GetSocketOption", socketException);
                }
                throw socketException;
            }
        }

        public byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            byte[] optionValue = new byte[optionLength];
            int num = optionLength;
            if (UnsafeNclNativeMethods.OSSOCK.getsockopt(this.m_Handle, optionLevel, optionName, optionValue, ref num) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "GetSocketOption", socketException);
                }
                throw socketException;
            }
            if (optionLength != num)
            {
                byte[] dst = new byte[num];
                Buffer.BlockCopy(optionValue, 0, dst, 0, num);
                optionValue = dst;
            }
            return optionValue;
        }

        internal static void InitializeSockets()
        {
            if (!s_Initialized)
            {
                lock (InternalSyncObject)
                {
                    if (!s_Initialized)
                    {
                        WSAData lpWSAData = new WSAData();
                        if (UnsafeNclNativeMethods.OSSOCK.WSAStartup(0x202, out lpWSAData) != SocketError.Success)
                        {
                            throw new SocketException();
                        }
                        if (!ComNetOS.IsWinNt)
                        {
                            UseOverlappedIO = true;
                        }
                        bool flag = true;
                        bool flag2 = true;
                        SafeCloseSocket.InnerSafeCloseSocket socket = UnsafeNclNativeMethods.OSSOCK.WSASocket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.IP, IntPtr.Zero, 0, 0);
                        if (socket.IsInvalid && (Marshal.GetLastWin32Error() == 0x273f))
                        {
                            flag = false;
                        }
                        socket.Close();
                        SafeCloseSocket.InnerSafeCloseSocket socket2 = UnsafeNclNativeMethods.OSSOCK.WSASocket(System.Net.Sockets.AddressFamily.InterNetworkV6, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.IP, IntPtr.Zero, 0, 0);
                        if (socket2.IsInvalid && (Marshal.GetLastWin32Error() == 0x273f))
                        {
                            flag2 = false;
                        }
                        socket2.Close();
                        flag2 = flag2 && ComNetOS.IsPostWin2K;
                        if (flag2)
                        {
                            s_OSSupportsIPv6 = true;
                            flag2 = SettingsSectionInternal.Section.Ipv6Enabled;
                        }
                        s_SupportsIPv4 = flag;
                        s_SupportsIPv6 = flag2;
                        s_PerfCountersEnabled = NetworkingPerfCounters.Instance.Enabled;
                        s_Initialized = true;
                    }
                }
            }
        }

        internal void InternalBind(EndPoint localEP)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "InternalBind", localEP);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            EndPoint remoteEP = localEP;
            SocketAddress socketAddress = this.SnapshotAndSerialize(ref remoteEP);
            this.DoBind(remoteEP, socketAddress);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "InternalBind", "");
            }
        }

        internal void InternalConnect(EndPoint remoteEP)
        {
            EndPoint point = remoteEP;
            SocketAddress socketAddress = this.SnapshotAndSerialize(ref point);
            this.DoConnect(point, socketAddress);
        }

        internal void InternalSetBlocking(bool desired)
        {
            bool flag;
            this.InternalSetBlocking(desired, out flag);
        }

        private SocketError InternalSetBlocking(bool desired, out bool current)
        {
            SocketError notSocket;
            if (this.CleanedUp)
            {
                current = this.willBlock;
                return SocketError.Success;
            }
            int argp = desired ? 0 : -1;
            try
            {
                notSocket = UnsafeNclNativeMethods.OSSOCK.ioctlsocket(this.m_Handle, -2147195266, ref argp);
                if (notSocket == SocketError.SocketError)
                {
                    notSocket = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            catch (ObjectDisposedException)
            {
                notSocket = SocketError.NotSocket;
            }
            if (notSocket == SocketError.Success)
            {
                this.willBlockInternal = argp == 0;
            }
            current = this.willBlockInternal;
            return notSocket;
        }

        internal void InternalShutdown(SocketShutdown how)
        {
            if (!this.CleanedUp && !this.m_Handle.IsInvalid)
            {
                try
                {
                    UnsafeNclNativeMethods.OSSOCK.shutdown(this.m_Handle, (int) how);
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        public int IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (ioControlCode == -2147195266)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_useblocking"));
            }
            ExceptionHelper.UnmanagedPermission.Demand();
            int bytesTransferred = 0;
            if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(this.m_Handle.DangerousGetHandle(), ioControlCode, optionInValue, (optionInValue != null) ? optionInValue.Length : 0, optionOutValue, (optionOutValue != null) ? optionOutValue.Length : 0, out bytesTransferred, SafeNativeOverlapped.Zero, IntPtr.Zero) != SocketError.SocketError)
            {
                return bytesTransferred;
            }
            SocketException socketException = new SocketException();
            this.UpdateStatusAfterSocketError(socketException);
            if (s_LoggingEnabled)
            {
                Logging.Exception(Logging.Sockets, this, "IOControl", socketException);
            }
            throw socketException;
        }

        public int IOControl(IOControlCode ioControlCode, byte[] optionInValue, byte[] optionOutValue)
        {
            return this.IOControl((int) ioControlCode, optionInValue, optionOutValue);
        }

        internal int IOControl(IOControlCode ioControlCode, IntPtr optionInValue, int inValueSize, IntPtr optionOutValue, int outValueSize)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (((int) ioControlCode) == -2147195266)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_useblocking"));
            }
            int bytesTransferred = 0;
            if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking_Internal(this.m_Handle.DangerousGetHandle(), (uint) ioControlCode, optionInValue, inValueSize, optionOutValue, outValueSize, out bytesTransferred, SafeNativeOverlapped.Zero, IntPtr.Zero) != SocketError.SocketError)
            {
                return bytesTransferred;
            }
            SocketException socketException = new SocketException();
            this.UpdateStatusAfterSocketError(socketException);
            if (s_LoggingEnabled)
            {
                Logging.Exception(Logging.Sockets, this, "IOControl", socketException);
            }
            throw socketException;
        }

        public void Listen(int backlog)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Listen", backlog);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (UnsafeNclNativeMethods.OSSOCK.listen(this.m_Handle, backlog) != SocketError.Success)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Listen", socketException);
                }
                throw socketException;
            }
            this.isListening = true;
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Listen", "");
            }
        }

        private static void MicrosecondsToTimeValue(long microSeconds, ref TimeValue socketTime)
        {
            socketTime.Seconds = (int) (microSeconds / 0xf4240L);
            socketTime.Microseconds = (int) (microSeconds % 0xf4240L);
        }

        private static void MultipleAddressConnectCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                bool flag = false;
                MultipleAddressConnectAsyncResult asyncState = (MultipleAddressConnectAsyncResult) result.AsyncState;
                try
                {
                    flag = DoMultipleAddressConnectCallback(result, asyncState);
                }
                catch (Exception exception)
                {
                    asyncState.InvokeCallback(exception);
                }
                if (flag)
                {
                    asyncState.InvokeCallback();
                }
            }
        }

        internal void MultipleSend(BufferOffsetSize[] buffers, SocketFlags socketFlags)
        {
            SocketError error;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "MultipleSend", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            WSABuffer[] buffersArray = new WSABuffer[buffers.Length];
            GCHandle[] handleArray = null;
            try
            {
                int num;
                handleArray = new GCHandle[buffers.Length];
                for (int i = 0; i < buffers.Length; i++)
                {
                    handleArray[i] = GCHandle.Alloc(buffers[i].Buffer, GCHandleType.Pinned);
                    buffersArray[i].Length = buffers[i].Size;
                    buffersArray[i].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffers[i].Buffer, buffers[i].Offset);
                }
                error = UnsafeNclNativeMethods.OSSOCK.WSASend_Blocking(this.m_Handle.DangerousGetHandle(), buffersArray, buffersArray.Length, out num, socketFlags, SafeNativeOverlapped.Zero, IntPtr.Zero);
            }
            finally
            {
                if (handleArray != null)
                {
                    for (int j = 0; j < handleArray.Length; j++)
                    {
                        if (handleArray[j].IsAllocated)
                        {
                            handleArray[j].Free();
                        }
                    }
                }
            }
            if (error != SocketError.Success)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "MultipleSend", socketException);
                }
                throw socketException;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "MultipleSend", "");
            }
        }

        public bool Poll(int microSeconds, SelectMode mode)
        {
            int num;
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            IntPtr handle = this.m_Handle.DangerousGetHandle();
            IntPtr[] ptrArray = new IntPtr[] { (IntPtr) 1, handle };
            TimeValue socketTime = new TimeValue();
            if (microSeconds != -1)
            {
                MicrosecondsToTimeValue((long) ((ulong) microSeconds), ref socketTime);
                num = UnsafeNclNativeMethods.OSSOCK.select(0, (mode == SelectMode.SelectRead) ? ptrArray : null, (mode == SelectMode.SelectWrite) ? ptrArray : null, (mode == SelectMode.SelectError) ? ptrArray : null, ref socketTime);
            }
            else
            {
                num = UnsafeNclNativeMethods.OSSOCK.select(0, (mode == SelectMode.SelectRead) ? ptrArray : null, (mode == SelectMode.SelectWrite) ? ptrArray : null, (mode == SelectMode.SelectError) ? ptrArray : null, IntPtr.Zero);
            }
            if (num == -1)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Poll", socketException);
                }
                throw socketException;
            }
            if (((int) ptrArray[0]) == 0)
            {
                return false;
            }
            return (ptrArray[1] == handle);
        }

        private static object PostOneBeginConnect(MultipleAddressConnectAsyncResult context)
        {
            IPAddress address = context.addresses[context.index];
            if (address.AddressFamily != context.socket.AddressFamily)
            {
                if (context.lastException == null)
                {
                    return new ArgumentException(SR.GetString("net_invalidAddressList"), "context");
                }
                return context.lastException;
            }
            try
            {
                EndPoint remoteEP = new IPEndPoint(address, context.port);
                context.socket.CheckCacheRemote(ref remoteEP, true);
                IAsyncResult result = context.socket.UnsafeBeginConnect(remoteEP, new AsyncCallback(Socket.MultipleAddressConnectCallback), context);
                if (result.CompletedSynchronously)
                {
                    return result;
                }
            }
            catch (Exception exception)
            {
                if (((exception is OutOfMemoryException) || (exception is StackOverflowException)) || (exception is ThreadAbortException))
                {
                    throw;
                }
                return exception;
            }
            return null;
        }

        public int Receive(byte[] buffer)
        {
            return this.Receive(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None);
        }

        public int Receive(IList<ArraySegment<byte>> buffers)
        {
            return this.Receive(buffers, SocketFlags.None);
        }

        public int Receive(byte[] buffer, SocketFlags socketFlags)
        {
            return this.Receive(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags);
        }

        public int Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
        {
            SocketError error;
            int num = this.Receive(buffers, socketFlags, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException(error);
            }
            return num;
        }

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return this.Receive(buffer, 0, size, socketFlags);
        }

        public int Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
        {
            int num2;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Receive", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }
            if (buffers.Count == 0)
            {
                throw new ArgumentException(SR.GetString("net_sockets_zerolist", new object[] { "buffers" }), "buffers");
            }
            this.ValidateBlockingMode();
            int count = buffers.Count;
            WSABuffer[] bufferArray = new WSABuffer[count];
            GCHandle[] handleArray = null;
            errorCode = SocketError.Success;
            try
            {
                handleArray = new GCHandle[count];
                for (int i = 0; i < count; i++)
                {
                    ArraySegment<byte> segment = buffers[i];
                    ValidationHelper.ValidateSegment(segment);
                    handleArray[i] = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
                    bufferArray[i].Length = segment.Count;
                    bufferArray[i].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(segment.Array, segment.Offset);
                }
                errorCode = UnsafeNclNativeMethods.OSSOCK.WSARecv_Blocking(this.m_Handle.DangerousGetHandle(), bufferArray, count, out num2, ref socketFlags, SafeNativeOverlapped.Zero, IntPtr.Zero);
                if (errorCode == SocketError.SocketError)
                {
                    errorCode = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                if (handleArray != null)
                {
                    for (int j = 0; j < handleArray.Length; j++)
                    {
                        if (handleArray[j].IsAllocated)
                        {
                            handleArray[j].Free();
                        }
                    }
                }
            }
            if (errorCode != SocketError.Success)
            {
                this.UpdateStatusAfterSocketError(errorCode);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Receive", new SocketException(errorCode));
                    Logging.Exit(Logging.Sockets, this, "Receive", 0);
                }
                return 0;
            }
            if (s_PerfCountersEnabled)
            {
                bool flag = (socketFlags & SocketFlags.Peek) != SocketFlags.None;
                if ((num2 > 0) && !flag)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesReceived, (long) num2);
                    if (this.Transport == TransportType.Udp)
                    {
                        NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsReceived);
                    }
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Receive", num2);
            }
            return num2;
        }

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            SocketError error;
            int num = this.Receive(buffer, offset, size, socketFlags, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException(error);
            }
            return num;
        }

        public unsafe int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
        {
            int num;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Receive", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            this.ValidateBlockingMode();
            errorCode = SocketError.Success;
            if (buffer.Length == 0)
            {
                num = UnsafeNclNativeMethods.OSSOCK.recv(this.m_Handle.DangerousGetHandle(), null, 0, socketFlags);
            }
            else
            {
                fixed (byte* numRef = buffer)
                {
                    num = UnsafeNclNativeMethods.OSSOCK.recv(this.m_Handle.DangerousGetHandle(), numRef + offset, size, socketFlags);
                }
            }
            if (num == -1)
            {
                errorCode = (SocketError) Marshal.GetLastWin32Error();
                this.UpdateStatusAfterSocketError(errorCode);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Receive", new SocketException(errorCode));
                    Logging.Exit(Logging.Sockets, this, "Receive", 0);
                }
                return 0;
            }
            if (s_PerfCountersEnabled)
            {
                bool flag = (socketFlags & SocketFlags.Peek) != SocketFlags.None;
                if ((num > 0) && !flag)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesReceived, (long) num);
                    if (this.Transport == TransportType.Udp)
                    {
                        NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsReceived);
                    }
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Dump(Logging.Sockets, this, "Receive", buffer, offset, num);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Receive", num);
            }
            return num;
        }

        public bool ReceiveAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            int num;
            SocketError error;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "ReceiveAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            e.StartOperationCommon(this);
            e.StartOperationReceive();
            this.BindToCompletionPort();
            SocketFlags socketFlags = e.m_SocketFlags;
            try
            {
                if (e.m_Buffer != null)
                {
                    error = UnsafeNclNativeMethods.OSSOCK.WSARecv(this.m_Handle, ref e.m_WSABuffer, 1, out num, ref socketFlags, e.m_PtrNativeOverlapped, IntPtr.Zero);
                }
                else
                {
                    error = UnsafeNclNativeMethods.OSSOCK.WSARecv(this.m_Handle, e.m_WSABufferArray, e.m_WSABufferArray.Length, out num, ref socketFlags, e.m_PtrNativeOverlapped, IntPtr.Zero);
                }
            }
            catch (Exception exception)
            {
                e.Complete();
                throw exception;
            }
            if (error != SocketError.Success)
            {
                error = (SocketError) Marshal.GetLastWin32Error();
            }
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                e.FinishOperationSyncFailure(error, num, socketFlags);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "ReceiveAsync", flag);
            }
            return flag;
        }

        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP)
        {
            return this.ReceiveFrom(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return this.ReceiveFrom(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return this.ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);
        }

        public unsafe int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            int num;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "ReceiveFrom", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (remoteEP.AddressFamily != this.addressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidEndPointAddressFamily", new object[] { remoteEP.AddressFamily, this.addressFamily }), "remoteEP");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (this.m_RightEndPoint == null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustbind"));
            }
            this.ValidateBlockingMode();
            EndPoint point = remoteEP;
            SocketAddress address = this.SnapshotAndSerialize(ref point);
            SocketAddress address2 = point.Serialize();
            if (buffer.Length == 0)
            {
                num = UnsafeNclNativeMethods.OSSOCK.recvfrom(this.m_Handle.DangerousGetHandle(), null, 0, socketFlags, address.m_Buffer, ref address.m_Size);
            }
            else
            {
                fixed (byte* numRef = buffer)
                {
                    num = UnsafeNclNativeMethods.OSSOCK.recvfrom(this.m_Handle.DangerousGetHandle(), numRef + offset, size, socketFlags, address.m_Buffer, ref address.m_Size);
                }
            }
            SocketException socketException = null;
            if (num == -1)
            {
                socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "ReceiveFrom", socketException);
                }
                if (socketException.ErrorCode != 0x2738)
                {
                    throw socketException;
                }
            }
            if (!address2.Equals(address))
            {
                try
                {
                    remoteEP = point.Create(address);
                }
                catch
                {
                }
                if (this.m_RightEndPoint == null)
                {
                    this.m_RightEndPoint = point;
                }
            }
            if (socketException != null)
            {
                throw socketException;
            }
            if (s_PerfCountersEnabled && (num > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesReceived, (long) num);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsReceived);
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Dump(Logging.Sockets, this, "ReceiveFrom", buffer, offset, size);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "ReceiveFrom", num);
            }
            return num;
        }

        public bool ReceiveFromAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            int num;
            SocketError error;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "ReceiveFromAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (e.RemoteEndPoint == null)
            {
                throw new ArgumentNullException("RemoteEndPoint");
            }
            if (e.RemoteEndPoint.AddressFamily != this.addressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidEndPointAddressFamily", new object[] { e.RemoteEndPoint.AddressFamily, this.addressFamily }), "RemoteEndPoint");
            }
            EndPoint remoteEndPoint = e.RemoteEndPoint;
            e.m_SocketAddress = this.SnapshotAndSerialize(ref remoteEndPoint);
            e.StartOperationCommon(this);
            e.StartOperationReceiveFrom();
            this.BindToCompletionPort();
            SocketFlags socketFlags = e.m_SocketFlags;
            try
            {
                if (e.m_Buffer != null)
                {
                    error = UnsafeNclNativeMethods.OSSOCK.WSARecvFrom(this.m_Handle, ref e.m_WSABuffer, 1, out num, ref socketFlags, e.m_PtrSocketAddressBuffer, e.m_PtrSocketAddressBufferSize, e.m_PtrNativeOverlapped, IntPtr.Zero);
                }
                else
                {
                    error = UnsafeNclNativeMethods.OSSOCK.WSARecvFrom(this.m_Handle, e.m_WSABufferArray, e.m_WSABufferArray.Length, out num, ref socketFlags, e.m_PtrSocketAddressBuffer, e.m_PtrSocketAddressBufferSize, e.m_PtrNativeOverlapped, IntPtr.Zero);
                }
            }
            catch (Exception exception)
            {
                e.Complete();
                throw exception;
            }
            if (error != SocketError.Success)
            {
                error = (SocketError) Marshal.GetLastWin32Error();
            }
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                e.FinishOperationSyncFailure(error, num, socketFlags);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "ReceiveFromAsync", flag);
            }
            return flag;
        }

        public int ReceiveMessageFrom(byte[] buffer, int offset, int size, ref SocketFlags socketFlags, ref EndPoint remoteEP, out IPPacketInformation ipPacketInformation)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "ReceiveMessageFrom", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsPostWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (remoteEP.AddressFamily != this.addressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidEndPointAddressFamily", new object[] { remoteEP.AddressFamily, this.addressFamily }), "remoteEP");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (this.m_RightEndPoint == null)
            {
                throw new InvalidOperationException(SR.GetString("net_sockets_mustbind"));
            }
            this.ValidateBlockingMode();
            EndPoint point = remoteEP;
            SocketAddress socketAddress = this.SnapshotAndSerialize(ref point);
            ReceiveMessageOverlappedAsyncResult result = new ReceiveMessageOverlappedAsyncResult(this, null, null);
            result.SetUnmanagedStructures(buffer, offset, size, socketAddress, socketFlags);
            SocketAddress address2 = point.Serialize();
            int bytesTransferred = 0;
            SocketError success = SocketError.Success;
            this.SetReceivingPacketInformation();
            try
            {
                if (this.WSARecvMsg_Blocking(this.m_Handle.DangerousGetHandle(), Marshal.UnsafeAddrOfPinnedArrayElement(result.m_MessageBuffer, 0), out bytesTransferred, IntPtr.Zero, IntPtr.Zero) == SocketError.SocketError)
                {
                    success = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                result.SyncReleaseUnmanagedStructures();
            }
            switch (success)
            {
                case SocketError.Success:
                case SocketError.MessageSize:
                    if (!address2.Equals(result.m_SocketAddress))
                    {
                        try
                        {
                            remoteEP = point.Create(result.m_SocketAddress);
                        }
                        catch
                        {
                        }
                        if (this.m_RightEndPoint == null)
                        {
                            this.m_RightEndPoint = point;
                        }
                    }
                    socketFlags = result.m_flags;
                    ipPacketInformation = result.m_IPPacketInformation;
                    if (s_LoggingEnabled)
                    {
                        Logging.Exit(Logging.Sockets, this, "ReceiveMessageFrom", success);
                    }
                    return bytesTransferred;
            }
            SocketException socketException = new SocketException(success);
            this.UpdateStatusAfterSocketError(socketException);
            if (s_LoggingEnabled)
            {
                Logging.Exception(Logging.Sockets, this, "ReceiveMessageFrom", socketException);
            }
            throw socketException;
        }

        public bool ReceiveMessageFromAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            int num;
            SocketError error;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "ReceiveMessageFromAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (e.RemoteEndPoint == null)
            {
                throw new ArgumentNullException("RemoteEndPoint");
            }
            if (e.RemoteEndPoint.AddressFamily != this.addressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidEndPointAddressFamily", new object[] { e.RemoteEndPoint.AddressFamily, this.addressFamily }), "RemoteEndPoint");
            }
            EndPoint remoteEndPoint = e.RemoteEndPoint;
            e.m_SocketAddress = this.SnapshotAndSerialize(ref remoteEndPoint);
            this.SetReceivingPacketInformation();
            e.StartOperationCommon(this);
            e.StartOperationReceiveMessageFrom();
            this.BindToCompletionPort();
            try
            {
                error = this.WSARecvMsg(this.m_Handle, e.m_PtrWSAMessageBuffer, out num, e.m_PtrNativeOverlapped, IntPtr.Zero);
            }
            catch (Exception exception)
            {
                e.Complete();
                throw exception;
            }
            if (error != SocketError.Success)
            {
                error = (SocketError) Marshal.GetLastWin32Error();
            }
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                e.FinishOperationSyncFailure(error, num, SocketFlags.None);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "ReceiveMessageFromAsync", flag);
            }
            return flag;
        }

        private static void RegisteredWaitCallback(object state, bool timedOut)
        {
            Socket socket = (Socket) state;
            if (Interlocked.Exchange<RegisteredWaitHandle>(ref socket.m_RegisteredWait, null) != null)
            {
                switch (socket.m_BlockEventBits)
                {
                    case AsyncEventBits.FdAccept:
                        socket.AcceptCallback(null);
                        break;

                    case AsyncEventBits.FdConnect:
                        socket.ConnectCallback();
                        break;
                }
            }
        }

        public static void Select(IList checkRead, IList checkWrite, IList checkError, int microSeconds)
        {
            int num;
            if ((((checkRead == null) || (checkRead.Count == 0)) && ((checkWrite == null) || (checkWrite.Count == 0))) && ((checkError == null) || (checkError.Count == 0)))
            {
                throw new ArgumentNullException(SR.GetString("net_sockets_empty_select"));
            }
            if ((checkRead != null) && (checkRead.Count > 0x10000))
            {
                object[] args = new object[] { "checkRead", 0x10000.ToString(NumberFormatInfo.CurrentInfo) };
                throw new ArgumentOutOfRangeException("checkRead", SR.GetString("net_sockets_toolarge_select", args));
            }
            if ((checkWrite != null) && (checkWrite.Count > 0x10000))
            {
                object[] objArray2 = new object[] { "checkWrite", 0x10000.ToString(NumberFormatInfo.CurrentInfo) };
                throw new ArgumentOutOfRangeException("checkWrite", SR.GetString("net_sockets_toolarge_select", objArray2));
            }
            if ((checkError != null) && (checkError.Count > 0x10000))
            {
                object[] objArray3 = new object[] { "checkError", 0x10000.ToString(NumberFormatInfo.CurrentInfo) };
                throw new ArgumentOutOfRangeException("checkError", SR.GetString("net_sockets_toolarge_select", objArray3));
            }
            IntPtr[] readfds = SocketListToFileDescriptorSet(checkRead);
            IntPtr[] writefds = SocketListToFileDescriptorSet(checkWrite);
            IntPtr[] exceptfds = SocketListToFileDescriptorSet(checkError);
            if (microSeconds != -1)
            {
                TimeValue socketTime = new TimeValue();
                MicrosecondsToTimeValue((long) ((ulong) microSeconds), ref socketTime);
                num = UnsafeNclNativeMethods.OSSOCK.select(0, readfds, writefds, exceptfds, ref socketTime);
            }
            else
            {
                num = UnsafeNclNativeMethods.OSSOCK.select(0, readfds, writefds, exceptfds, IntPtr.Zero);
            }
            if (num == -1)
            {
                throw new SocketException();
            }
            SelectFileDescriptor(checkRead, readfds);
            SelectFileDescriptor(checkWrite, writefds);
            SelectFileDescriptor(checkError, exceptfds);
        }

        private static void SelectFileDescriptor(IList socketList, IntPtr[] fileDescriptorSet)
        {
            if ((socketList != null) && (socketList.Count != 0))
            {
                if (((int) fileDescriptorSet[0]) == 0)
                {
                    socketList.Clear();
                }
                else
                {
                    lock (socketList)
                    {
                        for (int i = 0; i < socketList.Count; i++)
                        {
                            Socket socket = socketList[i] as Socket;
                            int num2 = 0;
                            while (num2 < ((int) fileDescriptorSet[0]))
                            {
                                if (fileDescriptorSet[num2 + 1] == socket.m_Handle.DangerousGetHandle())
                                {
                                    break;
                                }
                                num2++;
                            }
                            if (num2 == ((int) fileDescriptorSet[0]))
                            {
                                socketList.RemoveAt(i--);
                            }
                        }
                    }
                }
            }
        }

        public int Send(byte[] buffer)
        {
            return this.Send(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None);
        }

        public int Send(IList<ArraySegment<byte>> buffers)
        {
            return this.Send(buffers, SocketFlags.None);
        }

        public int Send(byte[] buffer, SocketFlags socketFlags)
        {
            return this.Send(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags);
        }

        public int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
        {
            SocketError error;
            int num = this.Send(buffers, socketFlags, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException(error);
            }
            return num;
        }

        public int Send(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return this.Send(buffer, 0, size, socketFlags);
        }

        public int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
        {
            int num2;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Send", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }
            if (buffers.Count == 0)
            {
                throw new ArgumentException(SR.GetString("net_sockets_zerolist", new object[] { "buffers" }), "buffers");
            }
            this.ValidateBlockingMode();
            errorCode = SocketError.Success;
            int count = buffers.Count;
            WSABuffer[] buffersArray = new WSABuffer[count];
            GCHandle[] handleArray = null;
            try
            {
                handleArray = new GCHandle[count];
                for (int i = 0; i < count; i++)
                {
                    ArraySegment<byte> segment = buffers[i];
                    ValidationHelper.ValidateSegment(segment);
                    handleArray[i] = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
                    buffersArray[i].Length = segment.Count;
                    buffersArray[i].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(segment.Array, segment.Offset);
                }
                errorCode = UnsafeNclNativeMethods.OSSOCK.WSASend_Blocking(this.m_Handle.DangerousGetHandle(), buffersArray, count, out num2, socketFlags, SafeNativeOverlapped.Zero, IntPtr.Zero);
                if (errorCode == SocketError.SocketError)
                {
                    errorCode = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                if (handleArray != null)
                {
                    for (int j = 0; j < handleArray.Length; j++)
                    {
                        if (handleArray[j].IsAllocated)
                        {
                            handleArray[j].Free();
                        }
                    }
                }
            }
            if (errorCode != SocketError.Success)
            {
                this.UpdateStatusAfterSocketError(errorCode);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Send", new SocketException(errorCode));
                    Logging.Exit(Logging.Sockets, this, "Send", 0);
                }
                return 0;
            }
            if (s_PerfCountersEnabled && (num2 > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesSent, (long) num2);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsSent);
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Send", num2);
            }
            return num2;
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            SocketError error;
            int num = this.Send(buffer, offset, size, socketFlags, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException(error);
            }
            return num;
        }

        public unsafe int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
        {
            int num;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Send", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            errorCode = SocketError.Success;
            this.ValidateBlockingMode();
            if (buffer.Length == 0)
            {
                num = UnsafeNclNativeMethods.OSSOCK.send(this.m_Handle.DangerousGetHandle(), null, 0, socketFlags);
            }
            else
            {
                fixed (byte* numRef = buffer)
                {
                    num = UnsafeNclNativeMethods.OSSOCK.send(this.m_Handle.DangerousGetHandle(), numRef + offset, size, socketFlags);
                }
            }
            if (num == -1)
            {
                errorCode = (SocketError) Marshal.GetLastWin32Error();
                this.UpdateStatusAfterSocketError(errorCode);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Send", new SocketException(errorCode));
                    Logging.Exit(Logging.Sockets, this, "Send", 0);
                }
                return 0;
            }
            if (s_PerfCountersEnabled && (num > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesSent, (long) num);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsSent);
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Dump(Logging.Sockets, this, "Send", buffer, offset, size);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "Send", num);
            }
            return num;
        }

        public bool SendAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            int num;
            SocketError error;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "SendAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            e.StartOperationCommon(this);
            e.StartOperationSend();
            this.BindToCompletionPort();
            try
            {
                if (e.m_Buffer != null)
                {
                    error = UnsafeNclNativeMethods.OSSOCK.WSASend(this.m_Handle, ref e.m_WSABuffer, 1, out num, e.m_SocketFlags, e.m_PtrNativeOverlapped, IntPtr.Zero);
                }
                else
                {
                    error = UnsafeNclNativeMethods.OSSOCK.WSASend(this.m_Handle, e.m_WSABufferArray, e.m_WSABufferArray.Length, out num, e.m_SocketFlags, e.m_PtrNativeOverlapped, IntPtr.Zero);
                }
            }
            catch (Exception exception)
            {
                e.Complete();
                throw exception;
            }
            if (error != SocketError.Success)
            {
                error = (SocketError) Marshal.GetLastWin32Error();
            }
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                e.FinishOperationSyncFailure(error, num, SocketFlags.None);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "SendAsync", flag);
            }
            return flag;
        }

        public void SendFile(string fileName)
        {
            if (!ComNetOS.IsWinNt)
            {
                this.DownLevelSendFile(fileName);
            }
            else
            {
                this.SendFile(fileName, null, null, TransmitFileOptions.UseDefaultWorkerThread);
            }
        }

        public void SendFile(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "SendFile", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!ComNetOS.IsWinNt)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
            if (!this.Connected)
            {
                throw new NotSupportedException(SR.GetString("net_notconnected"));
            }
            this.ValidateBlockingMode();
            TransmitFileOverlappedAsyncResult result = new TransmitFileOverlappedAsyncResult(this);
            FileStream fileStream = null;
            if ((fileName != null) && (fileName.Length > 0))
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            System.Runtime.InteropServices.SafeHandle fileHandle = null;
            if (fileStream != null)
            {
                ExceptionHelper.UnmanagedPermission.Assert();
                try
                {
                    fileHandle = fileStream.SafeFileHandle;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            SocketError success = SocketError.Success;
            try
            {
                result.SetUnmanagedStructures(preBuffer, postBuffer, fileStream, TransmitFileOptions.UseDefaultWorkerThread, true);
                if ((fileHandle != null) ? !UnsafeNclNativeMethods.OSSOCK.TransmitFile_Blocking(this.m_Handle.DangerousGetHandle(), fileHandle, 0, 0, SafeNativeOverlapped.Zero, result.TransmitFileBuffers, flags) : !UnsafeNclNativeMethods.OSSOCK.TransmitFile_Blocking2(this.m_Handle.DangerousGetHandle(), IntPtr.Zero, 0, 0, SafeNativeOverlapped.Zero, result.TransmitFileBuffers, flags))
                {
                    success = (SocketError) Marshal.GetLastWin32Error();
                }
            }
            finally
            {
                result.SyncReleaseUnmanagedStructures();
            }
            if (success != SocketError.Success)
            {
                SocketException socketException = new SocketException(success);
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "SendFile", socketException);
                }
                throw socketException;
            }
            if ((result.Flags & (TransmitFileOptions.ReuseSocket | TransmitFileOptions.Disconnect)) != TransmitFileOptions.UseDefaultWorkerThread)
            {
                this.SetToDisconnected();
                this.m_RemoteEndPoint = null;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "SendFile", success);
            }
        }

        public bool SendPacketsAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            SocketError success;
            bool flag2;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "SendPacketsAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!this.Connected)
            {
                throw new NotSupportedException(SR.GetString("net_notconnected"));
            }
            e.StartOperationCommon(this);
            e.StartOperationSendPackets();
            this.BindToCompletionPort();
            try
            {
                flag2 = this.TransmitPackets(this.m_Handle, e.m_PtrSendPacketsDescriptor, e.m_SendPacketsElements.Length, e.m_SendPacketsSendSize, e.m_PtrNativeOverlapped, e.m_SendPacketsFlags);
            }
            catch (Exception exception)
            {
                e.Complete();
                throw exception;
            }
            if (!flag2)
            {
                success = (SocketError) Marshal.GetLastWin32Error();
            }
            else
            {
                success = SocketError.Success;
            }
            if ((success != SocketError.Success) && (success != SocketError.IOPending))
            {
                e.FinishOperationSyncFailure(success, 0, SocketFlags.None);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "SendPacketsAsync", flag);
            }
            return flag;
        }

        public int SendTo(byte[] buffer, EndPoint remoteEP)
        {
            return this.SendTo(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None, remoteEP);
        }

        public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return this.SendTo(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags, remoteEP);
        }

        public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return this.SendTo(buffer, 0, size, socketFlags, remoteEP);
        }

        public unsafe int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            int num;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "SendTo", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            this.ValidateBlockingMode();
            EndPoint point = remoteEP;
            SocketAddress address = this.CheckCacheRemote(ref point, false);
            if (buffer.Length == 0)
            {
                num = UnsafeNclNativeMethods.OSSOCK.sendto(this.m_Handle.DangerousGetHandle(), null, 0, socketFlags, address.m_Buffer, address.m_Size);
            }
            else
            {
                fixed (byte* numRef = buffer)
                {
                    num = UnsafeNclNativeMethods.OSSOCK.sendto(this.m_Handle.DangerousGetHandle(), numRef + offset, size, socketFlags, address.m_Buffer, address.m_Size);
                }
            }
            if (num == -1)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "SendTo", socketException);
                }
                throw socketException;
            }
            if (this.m_RightEndPoint == null)
            {
                this.m_RightEndPoint = point;
            }
            if (s_PerfCountersEnabled && (num > 0))
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesSent, (long) num);
                if (this.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsSent);
                }
            }
            if (s_LoggingEnabled)
            {
                Logging.Dump(Logging.Sockets, this, "SendTo", buffer, offset, size);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "SendTo", num);
            }
            return num;
        }

        public bool SendToAsync(SocketAsyncEventArgs e)
        {
            bool flag;
            int num;
            SocketError error;
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "SendToAsync", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (e.RemoteEndPoint == null)
            {
                throw new ArgumentNullException("RemoteEndPoint");
            }
            EndPoint remoteEndPoint = e.RemoteEndPoint;
            e.m_SocketAddress = this.CheckCacheRemote(ref remoteEndPoint, false);
            e.StartOperationCommon(this);
            e.StartOperationSendTo();
            this.BindToCompletionPort();
            try
            {
                if (e.m_Buffer != null)
                {
                    error = UnsafeNclNativeMethods.OSSOCK.WSASendTo(this.m_Handle, ref e.m_WSABuffer, 1, out num, e.m_SocketFlags, e.m_PtrSocketAddressBuffer, e.m_SocketAddress.m_Size, e.m_PtrNativeOverlapped, IntPtr.Zero);
                }
                else
                {
                    error = UnsafeNclNativeMethods.OSSOCK.WSASendTo(this.m_Handle, e.m_WSABufferArray, e.m_WSABufferArray.Length, out num, e.m_SocketFlags, e.m_PtrSocketAddressBuffer, e.m_SocketAddress.m_Size, e.m_PtrNativeOverlapped, IntPtr.Zero);
                }
            }
            catch (Exception exception)
            {
                e.Complete();
                throw exception;
            }
            if (error != SocketError.Success)
            {
                error = (SocketError) Marshal.GetLastWin32Error();
            }
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                e.FinishOperationSyncFailure(error, num, SocketFlags.None);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "SendToAsync", flag);
            }
            return flag;
        }

        private bool SetAsyncEventSelect(AsyncEventBits blockEventBits)
        {
            if (this.m_RegisteredWait != null)
            {
                return false;
            }
            if (this.m_AsyncEvent == null)
            {
                Interlocked.CompareExchange<ManualResetEvent>(ref this.m_AsyncEvent, new ManualResetEvent(false), null);
                if (s_RegisteredWaitCallback == null)
                {
                    s_RegisteredWaitCallback = new WaitOrTimerCallback(Socket.RegisteredWaitCallback);
                }
            }
            if (Interlocked.CompareExchange(ref this.m_IntCleanedUp, 2, 0) != 0)
            {
                return false;
            }
            this.m_BlockEventBits = blockEventBits;
            this.m_RegisteredWait = ThreadPool.UnsafeRegisterWaitForSingleObject(this.m_AsyncEvent, s_RegisteredWaitCallback, this, -1, true);
            Interlocked.Exchange(ref this.m_IntCleanedUp, 0);
            SocketError notSocket = SocketError.NotSocket;
            try
            {
                notSocket = UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(this.m_Handle, this.m_AsyncEvent.SafeWaitHandle, blockEventBits);
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
            }
            if (notSocket == SocketError.SocketError)
            {
                this.UpdateStatusAfterSocketError(notSocket);
            }
            this.willBlockInternal = false;
            return (notSocket == SocketError.Success);
        }

        public void SetIPProtectionLevel(IPProtectionLevel level)
        {
            if (level == IPProtectionLevel.Unspecified)
            {
                throw new ArgumentException(SR.GetString("net_sockets_invalid_optionValue_all"), "level");
            }
            if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                this.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPProtectionLevel, (int) level);
            }
            else
            {
                if (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                this.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IPProtectionLevel, (int) level);
            }
        }

        private void setIPv6MulticastOption(SocketOptionName optionName, IPv6MulticastOption MR)
        {
            IPv6MulticastRequest mreq = new IPv6MulticastRequest {
                MulticastAddress = MR.Group.GetAddressBytes(),
                InterfaceIndex = (int) MR.InterfaceIndex
            };
            if (UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_Handle, SocketOptionLevel.IPv6, optionName, ref mreq, IPv6MulticastRequest.Size) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "setIPv6MulticastOption", socketException);
                }
                throw socketException;
            }
        }

        private void setLingerOption(LingerOption lref)
        {
            Linger linger = new Linger {
                OnOff = lref.Enabled ? ((ushort) 1) : ((ushort) 0),
                Time = (ushort) lref.LingerTime
            };
            if (UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_Handle, SocketOptionLevel.Socket, SocketOptionName.Linger, ref linger, 4) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "setLingerOption", socketException);
                }
                throw socketException;
            }
        }

        private void setMulticastOption(SocketOptionName optionName, MulticastOption MR)
        {
            IPMulticastRequest mreq = new IPMulticastRequest {
                MulticastAddress = (int) MR.Group.m_Address
            };
            if (MR.LocalAddress != null)
            {
                mreq.InterfaceAddress = (int) MR.LocalAddress.m_Address;
            }
            else
            {
                int num = IPAddress.HostToNetworkOrder(MR.InterfaceIndex);
                mreq.InterfaceAddress = num;
            }
            if (UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_Handle, SocketOptionLevel.IP, optionName, ref mreq, IPMulticastRequest.Size) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "setMulticastOption", socketException);
                }
                throw socketException;
            }
        }

        internal void SetReceivingPacketInformation()
        {
            if (!this.m_ReceivingPacketInformation)
            {
                if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    this.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                }
                else if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    this.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true);
                }
                this.m_ReceivingPacketInformation = true;
            }
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            this.CheckSetOptionPermissions(optionLevel, optionName);
            if (UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_Handle, optionLevel, optionName, optionValue, (optionValue != null) ? optionValue.Length : 0) == SocketError.SocketError)
            {
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "SetSocketOption", socketException);
                }
                throw socketException;
            }
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            this.SetSocketOption(optionLevel, optionName, optionValue ? 1 : 0);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            this.CheckSetOptionPermissions(optionLevel, optionName);
            this.SetSocketOption(optionLevel, optionName, optionValue, false);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (optionValue == null)
            {
                throw new ArgumentNullException("optionValue");
            }
            this.CheckSetOptionPermissions(optionLevel, optionName);
            if ((optionLevel == SocketOptionLevel.Socket) && (optionName == SocketOptionName.Linger))
            {
                LingerOption lref = optionValue as LingerOption;
                if (lref == null)
                {
                    throw new ArgumentException(SR.GetString("net_sockets_invalid_optionValue", new object[] { "LingerOption" }), "optionValue");
                }
                if ((lref.LingerTime < 0) || (lref.LingerTime > 0xffff))
                {
                    throw new ArgumentException(SR.GetString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[] { 0, 0xffff }), "optionValue.LingerTime");
                }
                this.setLingerOption(lref);
            }
            else if ((optionLevel == SocketOptionLevel.IP) && ((optionName == SocketOptionName.AddMembership) || (optionName == SocketOptionName.DropMembership)))
            {
                MulticastOption mR = optionValue as MulticastOption;
                if (mR == null)
                {
                    throw new ArgumentException(SR.GetString("net_sockets_invalid_optionValue", new object[] { "MulticastOption" }), "optionValue");
                }
                this.setMulticastOption(optionName, mR);
            }
            else
            {
                if ((optionLevel != SocketOptionLevel.IPv6) || ((optionName != SocketOptionName.AddMembership) && (optionName != SocketOptionName.DropMembership)))
                {
                    throw new ArgumentException(SR.GetString("net_sockets_invalid_optionValue_all"), "optionValue");
                }
                IPv6MulticastOption option3 = optionValue as IPv6MulticastOption;
                if (option3 == null)
                {
                    throw new ArgumentException(SR.GetString("net_sockets_invalid_optionValue", new object[] { "IPv6MulticastOption" }), "optionValue");
                }
                this.setIPv6MulticastOption(optionName, option3);
            }
        }

        internal void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue, bool silent)
        {
            if (!silent || (!this.CleanedUp && !this.m_Handle.IsInvalid))
            {
                SocketError success = SocketError.Success;
                try
                {
                    success = UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_Handle, optionLevel, optionName, ref optionValue, 4);
                }
                catch
                {
                    if (!silent || !this.m_Handle.IsInvalid)
                    {
                        throw;
                    }
                    return;
                }
                if (((optionName == SocketOptionName.PacketInformation) && (optionValue == 0)) && (success == SocketError.Success))
                {
                    this.m_ReceivingPacketInformation = false;
                }
                if (!silent && (success == SocketError.SocketError))
                {
                    SocketException socketException = new SocketException();
                    this.UpdateStatusAfterSocketError(socketException);
                    if (s_LoggingEnabled)
                    {
                        Logging.Exception(Logging.Sockets, this, "SetSocketOption", socketException);
                    }
                    throw socketException;
                }
            }
        }

        internal void SetToConnected()
        {
            if (!this.m_IsConnected)
            {
                this.m_IsConnected = true;
                this.m_IsDisconnected = false;
                if (s_PerfCountersEnabled)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketConnectionsEstablished);
                }
            }
        }

        internal void SetToDisconnected()
        {
            if (this.m_IsConnected)
            {
                this.m_IsConnected = false;
                this.m_IsDisconnected = true;
                if (!this.CleanedUp)
                {
                    this.UnsetAsyncEventSelect();
                }
            }
        }

        public void Shutdown(SocketShutdown how)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "Shutdown", how);
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            SocketError socketError = (UnsafeNclNativeMethods.OSSOCK.shutdown(this.m_Handle, (int) how) != SocketError.SocketError) ? SocketError.Success : ((SocketError) Marshal.GetLastWin32Error());
            switch (socketError)
            {
                case SocketError.Success:
                case SocketError.NotSocket:
                    this.SetToDisconnected();
                    this.InternalSetBlocking(this.willBlockInternal);
                    if (s_LoggingEnabled)
                    {
                        Logging.Exit(Logging.Sockets, this, "Shutdown", "");
                    }
                    return;
            }
            SocketException socketException = new SocketException(socketError);
            this.UpdateStatusAfterSocketError(socketException);
            if (s_LoggingEnabled)
            {
                Logging.Exception(Logging.Sockets, this, "Shutdown", socketException);
            }
            throw socketException;
        }

        private SocketAddress SnapshotAndSerialize(ref EndPoint remoteEP)
        {
            IPEndPoint point = remoteEP as IPEndPoint;
            if (point != null)
            {
                point = point.Snapshot();
                remoteEP = point;
            }
            return this.CallSerializeCheckDnsEndPoint(remoteEP);
        }

        private static IntPtr[] SocketListToFileDescriptorSet(IList socketList)
        {
            if ((socketList == null) || (socketList.Count == 0))
            {
                return null;
            }
            IntPtr[] ptrArray = new IntPtr[socketList.Count + 1];
            ptrArray[0] = (IntPtr) socketList.Count;
            for (int i = 0; i < socketList.Count; i++)
            {
                if (!(socketList[i] is Socket))
                {
                    throw new ArgumentException(SR.GetString("net_sockets_select", new object[] { socketList[i].GetType().FullName, typeof(Socket).FullName }), "socketList");
                }
                ptrArray[i + 1] = ((Socket) socketList[i]).m_Handle.DangerousGetHandle();
            }
            return ptrArray;
        }

        private bool TransmitPackets(SafeCloseSocket socketHandle, IntPtr packetArray, int elementCount, int sendSize, SafeNativeOverlapped overlapped, TransmitFileOptions flags)
        {
            this.EnsureDynamicWinsockMethods();
            return this.m_DynamicWinsockMethods.GetDelegate<TransmitPacketsDelegate>(socketHandle)(socketHandle, packetArray, elementCount, sendSize, overlapped, flags);
        }

        internal IAsyncResult UnsafeBeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            if (this.CanUseConnectEx(remoteEP))
            {
                return this.BeginConnectEx(remoteEP, false, callback, state);
            }
            EndPoint point = remoteEP;
            SocketAddress socketAddress = this.SnapshotAndSerialize(ref point);
            ConnectAsyncResult asyncResult = new ConnectAsyncResult(this, point, state, callback);
            this.DoBeginConnect(point, socketAddress, asyncResult);
            return asyncResult;
        }

        internal IAsyncResult UnsafeBeginMultipleSend(BufferOffsetSize[] buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            this.DoBeginMultipleSend(buffers, socketFlags, asyncResult);
            return asyncResult;
        }

        internal IAsyncResult UnsafeBeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "UnsafeBeginReceive", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            this.DoBeginReceive(buffer, offset, size, socketFlags, asyncResult);
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "UnsafeBeginReceive", asyncResult);
            }
            return asyncResult;
        }

        internal IAsyncResult UnsafeBeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            if (s_LoggingEnabled)
            {
                Logging.Enter(Logging.Sockets, this, "UnsafeBeginSend", "");
            }
            if (this.CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            OverlappedAsyncResult asyncResult = new OverlappedAsyncResult(this, state, callback);
            SocketError socketError = this.DoBeginSend(buffer, offset, size, socketFlags, asyncResult);
            if ((socketError != SocketError.Success) && (socketError != SocketError.IOPending))
            {
                throw new SocketException(socketError);
            }
            if (s_LoggingEnabled)
            {
                Logging.Exit(Logging.Sockets, this, "UnsafeBeginSend", asyncResult);
            }
            return asyncResult;
        }

        private void UnsetAsyncEventSelect()
        {
            RegisteredWaitHandle registeredWait = this.m_RegisteredWait;
            if (registeredWait != null)
            {
                this.m_RegisteredWait = null;
                registeredWait.Unregister(null);
            }
            SocketError notSocket = SocketError.NotSocket;
            try
            {
                notSocket = UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(this.m_Handle, IntPtr.Zero, AsyncEventBits.FdNone);
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
            }
            if (this.m_AsyncEvent != null)
            {
                try
                {
                    this.m_AsyncEvent.Reset();
                }
                catch (ObjectDisposedException)
                {
                }
            }
            if (notSocket == SocketError.SocketError)
            {
                this.UpdateStatusAfterSocketError(notSocket);
            }
            this.InternalSetBlocking(this.willBlock);
        }

        internal Socket UpdateAcceptSocket(Socket socket, EndPoint remoteEP, bool needCancelSelect)
        {
            socket.addressFamily = this.addressFamily;
            socket.socketType = this.socketType;
            socket.protocolType = this.protocolType;
            socket.m_RightEndPoint = this.m_RightEndPoint;
            socket.m_RemoteEndPoint = remoteEP;
            socket.SetToConnected();
            socket.willBlock = this.willBlock;
            if (needCancelSelect)
            {
                socket.UnsetAsyncEventSelect();
                return socket;
            }
            socket.InternalSetBlocking(this.willBlock);
            return socket;
        }

        internal void UpdateStatusAfterSocketError(SocketError errorCode)
        {
            if (this.m_IsConnected && (this.m_Handle.IsInvalid || (((errorCode != SocketError.WouldBlock) && (errorCode != SocketError.IOPending)) && (errorCode != SocketError.NoBufferSpaceAvailable))))
            {
                this.SetToDisconnected();
            }
        }

        internal void UpdateStatusAfterSocketError(SocketException socketException)
        {
            this.UpdateStatusAfterSocketError((SocketError) socketException.NativeErrorCode);
        }

        private void ValidateBlockingMode()
        {
            if (this.willBlock && !this.willBlockInternal)
            {
                throw new InvalidOperationException(SR.GetString("net_invasync"));
            }
        }

        private SocketError WSARecvMsg(SafeCloseSocket socketHandle, IntPtr msg, out int bytesTransferred, System.Runtime.InteropServices.SafeHandle overlapped, IntPtr completionRoutine)
        {
            this.EnsureDynamicWinsockMethods();
            return this.m_DynamicWinsockMethods.GetDelegate<WSARecvMsgDelegate>(socketHandle)(socketHandle, msg, out bytesTransferred, overlapped, completionRoutine);
        }

        private SocketError WSARecvMsg_Blocking(IntPtr socketHandle, IntPtr msg, out int bytesTransferred, IntPtr overlapped, IntPtr completionRoutine)
        {
            this.EnsureDynamicWinsockMethods();
            return this.m_DynamicWinsockMethods.GetDelegate<WSARecvMsgDelegate_Blocking>(this.m_Handle)(socketHandle, msg, out bytesTransferred, overlapped, completionRoutine);
        }

        public System.Net.Sockets.AddressFamily AddressFamily
        {
            get
            {
                return this.addressFamily;
            }
        }

        public int Available
        {
            get
            {
                if (this.CleanedUp)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                int argp = 0;
                if (UnsafeNclNativeMethods.OSSOCK.ioctlsocket(this.m_Handle, 0x4004667f, ref argp) != SocketError.SocketError)
                {
                    return argp;
                }
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "Available", socketException);
                }
                throw socketException;
            }
        }

        public bool Blocking
        {
            get
            {
                return this.willBlock;
            }
            set
            {
                bool flag;
                if (this.CleanedUp)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                SocketError socketError = this.InternalSetBlocking(value, out flag);
                if (socketError != SocketError.Success)
                {
                    SocketException socketException = new SocketException(socketError);
                    this.UpdateStatusAfterSocketError(socketException);
                    if (s_LoggingEnabled)
                    {
                        Logging.Exception(Logging.Sockets, this, "Blocking", socketException);
                    }
                    throw socketException;
                }
                this.willBlock = flag;
            }
        }

        private CacheSet Caches
        {
            get
            {
                if (this.m_Caches == null)
                {
                    this.m_Caches = new CacheSet();
                }
                return this.m_Caches;
            }
        }

        private bool CanUseAcceptEx
        {
            get
            {
                if (!ComNetOS.IsWinNt)
                {
                    return false;
                }
                if (!Thread.CurrentThread.IsThreadPoolThread && !SettingsSectionInternal.Section.AlwaysUseCompletionPortsForAccept)
                {
                    return this.m_IsDisconnected;
                }
                return true;
            }
        }

        internal bool CleanedUp
        {
            get
            {
                return (this.m_IntCleanedUp == 1);
            }
        }

        public bool Connected
        {
            get
            {
                if (this.m_NonBlockingConnectInProgress && this.Poll(0, SelectMode.SelectWrite))
                {
                    this.m_IsConnected = true;
                    this.m_RightEndPoint = this.m_NonBlockingConnectRightEndPoint;
                    this.m_NonBlockingConnectInProgress = false;
                }
                return this.m_IsConnected;
            }
        }

        public bool DontFragment
        {
            get
            {
                if (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                if (((int) this.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment)) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                if (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                this.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, value ? 1 : 0);
            }
        }

        public bool EnableBroadcast
        {
            get
            {
                if (((int) this.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast)) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, value ? 1 : 0);
            }
        }

        public bool ExclusiveAddressUse
        {
            get
            {
                if (((int) this.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse)) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                if (this.IsBound)
                {
                    throw new InvalidOperationException(SR.GetString("net_sockets_mustnotbebound"));
                }
                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, value ? 1 : 0);
            }
        }

        public IntPtr Handle
        {
            get
            {
                ExceptionHelper.UnmanagedPermission.Demand();
                return this.m_Handle.DangerousGetHandle();
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        public bool IsBound
        {
            get
            {
                return (this.m_RightEndPoint != null);
            }
        }

        internal static bool LegacySupportsIPv6
        {
            get
            {
                InitializeSockets();
                return s_SupportsIPv6;
            }
        }

        public LingerOption LingerState
        {
            get
            {
                return (LingerOption) this.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
            }
            set
            {
                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value);
            }
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                if (this.CleanedUp)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                if (this.m_NonBlockingConnectInProgress && this.Poll(0, SelectMode.SelectWrite))
                {
                    this.m_IsConnected = true;
                    this.m_RightEndPoint = this.m_NonBlockingConnectRightEndPoint;
                    this.m_NonBlockingConnectInProgress = false;
                }
                if (this.m_RightEndPoint == null)
                {
                    return null;
                }
                SocketAddress socketAddress = this.m_RightEndPoint.Serialize();
                if (UnsafeNclNativeMethods.OSSOCK.getsockname(this.m_Handle, socketAddress.m_Buffer, ref socketAddress.m_Size) == SocketError.Success)
                {
                    return this.m_RightEndPoint.Create(socketAddress);
                }
                SocketException socketException = new SocketException();
                this.UpdateStatusAfterSocketError(socketException);
                if (s_LoggingEnabled)
                {
                    Logging.Exception(Logging.Sockets, this, "LocalEndPoint", socketException);
                }
                throw socketException;
            }
        }

        public bool MulticastLoopback
        {
            get
            {
                if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (((int) this.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback)) == 0)
                    {
                        return false;
                    }
                    return true;
                }
                if (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                if (((int) this.GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback)) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    this.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, value ? 1 : 0);
                }
                else
                {
                    if (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        throw new NotSupportedException(SR.GetString("net_invalidversion"));
                    }
                    this.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback, value ? 1 : 0);
                }
            }
        }

        public bool NoDelay
        {
            get
            {
                if (((int) this.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug)) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                this.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, value ? 1 : 0);
            }
        }

        public static bool OSSupportsIPv4
        {
            get
            {
                InitializeSockets();
                return s_SupportsIPv4;
            }
        }

        public static bool OSSupportsIPv6
        {
            get
            {
                InitializeSockets();
                return s_OSSupportsIPv6;
            }
        }

        public System.Net.Sockets.ProtocolType ProtocolType
        {
            get
            {
                return this.protocolType;
            }
        }

        public int ReceiveBufferSize
        {
            get
            {
                return (int) this.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
            }
        }

        public int ReceiveTimeout
        {
            get
            {
                return (int) this.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value == -1)
                {
                    value = 0;
                }
                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
            }
        }

        public EndPoint RemoteEndPoint
        {
            get
            {
                if (this.CleanedUp)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                if (this.m_RemoteEndPoint == null)
                {
                    if (this.m_NonBlockingConnectInProgress && this.Poll(0, SelectMode.SelectWrite))
                    {
                        this.m_IsConnected = true;
                        this.m_RightEndPoint = this.m_NonBlockingConnectRightEndPoint;
                        this.m_NonBlockingConnectInProgress = false;
                    }
                    if (this.m_RightEndPoint == null)
                    {
                        return null;
                    }
                    SocketAddress socketAddress = this.m_RightEndPoint.Serialize();
                    if (UnsafeNclNativeMethods.OSSOCK.getpeername(this.m_Handle, socketAddress.m_Buffer, ref socketAddress.m_Size) != SocketError.Success)
                    {
                        SocketException socketException = new SocketException();
                        this.UpdateStatusAfterSocketError(socketException);
                        if (s_LoggingEnabled)
                        {
                            Logging.Exception(Logging.Sockets, this, "RemoteEndPoint", socketException);
                        }
                        throw socketException;
                    }
                    try
                    {
                        this.m_RemoteEndPoint = this.m_RightEndPoint.Create(socketAddress);
                    }
                    catch
                    {
                    }
                }
                return this.m_RemoteEndPoint;
            }
        }

        internal SafeCloseSocket SafeHandle
        {
            get
            {
                return this.m_Handle;
            }
        }

        public int SendBufferSize
        {
            get
            {
                return (int) this.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
            }
        }

        public int SendTimeout
        {
            get
            {
                return (int) this.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value == -1)
                {
                    value = 0;
                }
                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
            }
        }

        public System.Net.Sockets.SocketType SocketType
        {
            get
            {
                return this.socketType;
            }
        }

        [Obsolete("SupportsIPv4 is obsoleted for this type, please use OSSupportsIPv4 instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static bool SupportsIPv4
        {
            get
            {
                InitializeSockets();
                return s_SupportsIPv4;
            }
        }

        [Obsolete("SupportsIPv6 is obsoleted for this type, please use OSSupportsIPv6 instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static bool SupportsIPv6
        {
            get
            {
                InitializeSockets();
                return s_SupportsIPv6;
            }
        }

        internal TransportType Transport
        {
            get
            {
                if (this.protocolType == System.Net.Sockets.ProtocolType.Tcp)
                {
                    return TransportType.Tcp;
                }
                if (this.protocolType != System.Net.Sockets.ProtocolType.Udp)
                {
                    return TransportType.All;
                }
                return TransportType.Udp;
            }
        }

        public short Ttl
        {
            get
            {
                if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return (short) ((int) this.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress));
                }
                if (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new NotSupportedException(SR.GetString("net_invalidversion"));
                }
                return (short) ((int) this.GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.ReuseAddress));
            }
            set
            {
                if ((value < 0) || (value > 0xff))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.addressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    this.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, (int) value);
                }
                else
                {
                    if (this.addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        throw new NotSupportedException(SR.GetString("net_invalidversion"));
                    }
                    this.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.ReuseAddress, (int) value);
                }
            }
        }

        public bool UseOnlyOverlappedIO
        {
            get
            {
                return this.useOverlappedIO;
            }
            set
            {
                if (this.m_BoundToThreadPool)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_completionportwasbound"));
                }
                this.useOverlappedIO = value;
            }
        }

        private class CacheSet
        {
            internal CallbackClosure AcceptClosureCache;
            internal CallbackClosure ConnectClosureCache;
            internal CallbackClosure ReceiveClosureCache;
            internal OverlappedCache ReceiveOverlappedCache;
            internal CallbackClosure SendClosureCache;
            internal OverlappedCache SendOverlappedCache;
        }

        private class DownLevelSendFileAsyncResult : ContextAwareResult
        {
            internal byte[] buffer;
            internal FileStream fileStream;
            internal Socket socket;
            internal bool writing;

            internal DownLevelSendFileAsyncResult(FileStream stream, Socket socket, object myState, AsyncCallback myCallBack) : base(socket, myState, myCallBack)
            {
                this.socket = socket;
                this.fileStream = stream;
                this.buffer = new byte[0xfa00];
            }
        }

        private class MultipleAddressConnectAsyncResult : ContextAwareResult
        {
            internal IPAddress[] addresses;
            internal int index;
            internal Exception lastException;
            internal int port;
            internal Socket socket;

            internal MultipleAddressConnectAsyncResult(IPAddress[] addresses, int port, Socket socket, object myState, AsyncCallback myCallBack) : base(socket, myState, myCallBack)
            {
                this.addresses = addresses;
                this.port = port;
                this.socket = socket;
            }

            internal EndPoint RemoteEndPoint
            {
                get
                {
                    if (((this.addresses != null) && (this.index > 0)) && (this.index < this.addresses.Length))
                    {
                        return new IPEndPoint(this.addresses[this.index], this.port);
                    }
                    return null;
                }
            }
        }
    }
}

