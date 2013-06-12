namespace System.Net
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;

    public class ServicePoint
    {
        internal const int LoopbackConnectionLimit = 0x7fffffff;
        private Uri m_Address;
        private bool m_AddressListFailed;
        private BindIPEndPoint m_BindIPEndPointDelegate;
        private object m_CachedChannelBinding;
        private object m_ClientCertificateOrBytes;
        private static readonly AsyncCallback m_ConnectCallbackDelegate = new AsyncCallback(ServicePoint.ConnectSocketCallback);
        private bool m_ConnectedSinceDns;
        private Hashtable m_ConnectionGroupList;
        private int m_ConnectionLeaseTimeout;
        private TimerThread.Queue m_ConnectionLeaseTimerQueue;
        private int m_ConnectionLimit;
        private string m_ConnectionName;
        private int m_CurrentAddressInfoIndex;
        private int m_CurrentConnections;
        private bool m_Expect100Continue;
        private TimerThread.Timer m_ExpiringTimer;
        private string m_Host;
        private TriState m_HostLoopbackGuess;
        private bool m_HostMode;
        private string m_HostName;
        private System.Net.HttpBehaviour m_HttpBehaviour;
        private DateTime m_IdleSince;
        private TimerThread.Queue m_IdlingQueue;
        private bool m_IPAddressesAreLoopback;
        private IPAddress[] m_IPAddressInfoList;
        private DateTime m_LastDnsResolve;
        private string m_LookupString;
        private int m_Port;
        private bool m_ProxyServicePoint;
        private int m_ReceiveBufferSize;
        private object m_ServerCertificateOrBytes;
        private int m_TcpKeepAliveInterval;
        private int m_TcpKeepAliveTime;
        private bool m_Understands100Continue;
        private bool m_UseNagleAlgorithm;
        private bool m_UserChangedLimit;
        private bool m_UseTcpKeepAlive;

        internal ServicePoint(Uri address, TimerThread.Queue defaultIdlingQueue, int defaultConnectionLimit, string lookupString, bool userChangedLimit, bool proxyServicePoint)
        {
            this.m_HostName = string.Empty;
            this.m_ProxyServicePoint = proxyServicePoint;
            this.m_Address = address;
            this.m_ConnectionName = address.Scheme;
            this.m_Host = address.DnsSafeHost;
            this.m_Port = address.Port;
            this.m_IdlingQueue = defaultIdlingQueue;
            this.m_ConnectionLimit = defaultConnectionLimit;
            this.m_HostLoopbackGuess = TriState.Unspecified;
            this.m_LookupString = lookupString;
            this.m_UserChangedLimit = userChangedLimit;
            this.m_UseNagleAlgorithm = ServicePointManager.UseNagleAlgorithm;
            this.m_Expect100Continue = ServicePointManager.Expect100Continue;
            this.m_ConnectionGroupList = new Hashtable(10);
            this.m_ConnectionLeaseTimeout = -1;
            this.m_ReceiveBufferSize = -1;
            this.m_UseTcpKeepAlive = ServicePointManager.s_UseTcpKeepAlive;
            this.m_TcpKeepAliveTime = ServicePointManager.s_TcpKeepAliveTime;
            this.m_TcpKeepAliveInterval = ServicePointManager.s_TcpKeepAliveInterval;
            this.m_Understands100Continue = true;
            this.m_HttpBehaviour = System.Net.HttpBehaviour.Unknown;
            this.m_IdleSince = DateTime.Now;
            this.m_ExpiringTimer = this.m_IdlingQueue.CreateTimer(ServicePointManager.IdleServicePointTimeoutDelegate, this);
        }

        internal ServicePoint(string host, int port, TimerThread.Queue defaultIdlingQueue, int defaultConnectionLimit, string lookupString, bool userChangedLimit, bool proxyServicePoint)
        {
            this.m_HostName = string.Empty;
            this.m_ProxyServicePoint = proxyServicePoint;
            this.m_ConnectionName = "ByHost:" + host + ":" + port.ToString(CultureInfo.InvariantCulture);
            this.m_IdlingQueue = defaultIdlingQueue;
            this.m_ConnectionLimit = defaultConnectionLimit;
            this.m_HostLoopbackGuess = TriState.Unspecified;
            this.m_LookupString = lookupString;
            this.m_UserChangedLimit = userChangedLimit;
            this.m_ConnectionGroupList = new Hashtable(10);
            this.m_ConnectionLeaseTimeout = -1;
            this.m_ReceiveBufferSize = -1;
            this.m_Host = host;
            this.m_Port = port;
            this.m_HostMode = true;
            this.m_IdleSince = DateTime.Now;
            this.m_ExpiringTimer = this.m_IdlingQueue.CreateTimer(ServicePointManager.IdleServicePointTimeoutDelegate, this);
        }

        private void BindUsingDelegate(Socket socket, IPEndPoint remoteIPEndPoint)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(remoteIPEndPoint.Address, remoteIPEndPoint.Port);
            int retryCount = 0;
            while (retryCount < 0x7fffffff)
            {
                IPEndPoint localEP = this.BindIPEndPointDelegate(this, remoteEndPoint, retryCount);
                if (localEP == null)
                {
                    break;
                }
                try
                {
                    socket.InternalBind(localEP);
                    break;
                }
                catch
                {
                }
                retryCount++;
            }
            if (retryCount == 0x7fffffff)
            {
                throw new OverflowException("Reached maximum number of BindIPEndPointDelegate retries");
            }
        }

        public bool CloseConnectionGroup(string connectionGroupName)
        {
            if ((!this.ReleaseConnectionGroup(HttpWebRequest.GenerateConnectionGroup(connectionGroupName, false, false).ToString()) && !this.ReleaseConnectionGroup(HttpWebRequest.GenerateConnectionGroup(connectionGroupName, true, false).ToString())) && !ConnectionPoolManager.RemoveConnectionPool(this, connectionGroupName))
            {
                return false;
            }
            return true;
        }

        private void CompleteGetConnection(Socket socket, Socket socket6, Socket finalSocket, IPAddress address)
        {
            if (finalSocket.AddressFamily == AddressFamily.InterNetwork)
            {
                if (socket6 != null)
                {
                    socket6.Close();
                    socket6 = null;
                }
            }
            else if (socket != null)
            {
                socket.Close();
                socket = null;
            }
            if (!this.UseNagleAlgorithm)
            {
                finalSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, 1);
            }
            if (this.ReceiveBufferSize != -1)
            {
                finalSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, this.ReceiveBufferSize);
            }
            if (this.m_UseTcpKeepAlive)
            {
                byte[] optionInValue = new byte[12];
                optionInValue[0] = 1;
                optionInValue[4] = (byte) (this.m_TcpKeepAliveTime & 0xff);
                optionInValue[5] = (byte) ((this.m_TcpKeepAliveTime >> 8) & 0xff);
                optionInValue[6] = (byte) ((this.m_TcpKeepAliveTime >> 0x10) & 0xff);
                optionInValue[7] = (byte) ((this.m_TcpKeepAliveTime >> 0x18) & 0xff);
                optionInValue[8] = (byte) (this.m_TcpKeepAliveInterval & 0xff);
                optionInValue[9] = (byte) ((this.m_TcpKeepAliveInterval >> 8) & 0xff);
                optionInValue[10] = (byte) ((this.m_TcpKeepAliveInterval >> 0x10) & 0xff);
                optionInValue[11] = (byte) ((this.m_TcpKeepAliveInterval >> 0x18) & 0xff);
                finalSocket.IOControl(IOControlCode.KeepAliveValues, optionInValue, null);
            }
        }

        private WebExceptionStatus ConnectSocket(Socket s4, Socket s6, ref Socket socket, ref IPAddress address, ConnectSocketState state, int timeout, out Exception exception)
        {
            return this.ConnectSocketInternal(false, s4, s6, ref socket, ref address, state, null, timeout, out exception);
        }

        private static void ConnectSocketCallback(IAsyncResult asyncResult)
        {
            ConnectSocketState asyncState = (ConnectSocketState) asyncResult.AsyncState;
            Socket socket = null;
            IPAddress address = null;
            Exception exception = null;
            Exception e = null;
            WebExceptionStatus connectFailure = WebExceptionStatus.ConnectFailure;
            try
            {
                connectFailure = asyncState.servicePoint.ConnectSocketInternal(asyncState.connectFailure, asyncState.s4, asyncState.s6, ref socket, ref address, asyncState, asyncResult, -1, out exception);
            }
            catch (SocketException exception3)
            {
                e = exception3;
            }
            catch (ObjectDisposedException exception4)
            {
                e = exception4;
            }
            switch (connectFailure)
            {
                case WebExceptionStatus.Pending:
                    return;

                case WebExceptionStatus.Success:
                    try
                    {
                        asyncState.servicePoint.CompleteGetConnection(asyncState.s4, asyncState.s6, socket, address);
                    }
                    catch (SocketException exception5)
                    {
                        e = exception5;
                    }
                    catch (ObjectDisposedException exception6)
                    {
                        e = exception6;
                    }
                    break;

                default:
                    e = new WebException(NetRes.GetWebStatusString(connectFailure), ((connectFailure == WebExceptionStatus.ProxyNameResolutionFailure) || (connectFailure == WebExceptionStatus.NameResolutionFailure)) ? asyncState.servicePoint.Host : null, exception, connectFailure, null, WebExceptionInternalStatus.ServicePointFatal);
                    break;
            }
            try
            {
                asyncState.pooledStream.ConnectionCallback(asyncState.owner, e, socket, address);
            }
            catch
            {
                if ((socket == null) || !socket.CleanedUp)
                {
                    throw;
                }
            }
        }

        private WebExceptionStatus ConnectSocketInternal(bool connectFailure, Socket s4, Socket s6, ref Socket socket, ref IPAddress address, ConnectSocketState state, IAsyncResult asyncResult, int timeout, out Exception exception)
        {
            exception = null;
            bool timedOut = false;
            IPAddress[] addresses = null;
            for (int i = 0; i < 2; i++)
            {
                int currentIndex;
                int num3 = 0;
                if (asyncResult == null)
                {
                    addresses = this.GetIPAddressInfoList(out currentIndex, addresses, timeout, out timedOut);
                    if (((addresses == null) || (addresses.Length == 0)) || timedOut)
                    {
                        break;
                    }
                }
                else
                {
                    addresses = state.addresses;
                    currentIndex = state.currentIndex;
                    num3 = state.i;
                    i = state.unsuccessfulAttempts;
                }
                while (num3 < addresses.Length)
                {
                    IPAddress address2 = addresses[currentIndex];
                    try
                    {
                        Socket socket2;
                        IPEndPoint remoteIPEndPoint = new IPEndPoint(address2, this.m_Port);
                        if (remoteIPEndPoint.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            socket2 = s4;
                        }
                        else
                        {
                            socket2 = s6;
                        }
                        if (state != null)
                        {
                            if (asyncResult == null)
                            {
                                state.addresses = addresses;
                                state.currentIndex = currentIndex;
                                state.i = num3;
                                state.unsuccessfulAttempts = i;
                                state.connectFailure = connectFailure;
                                if ((this.BindIPEndPointDelegate != null) && !socket2.IsBound)
                                {
                                    this.BindUsingDelegate(socket2, remoteIPEndPoint);
                                }
                                socket2.UnsafeBeginConnect(remoteIPEndPoint, m_ConnectCallbackDelegate, state);
                                return WebExceptionStatus.Pending;
                            }
                            IAsyncResult result = asyncResult;
                            asyncResult = null;
                            socket2.EndConnect(result);
                        }
                        else
                        {
                            if ((this.BindIPEndPointDelegate != null) && !socket2.IsBound)
                            {
                                this.BindUsingDelegate(socket2, remoteIPEndPoint);
                            }
                            socket2.InternalConnect(remoteIPEndPoint);
                        }
                        socket = socket2;
                        address = address2;
                        exception = null;
                        this.UpdateCurrentIndex(addresses, currentIndex);
                        return WebExceptionStatus.Success;
                    }
                    catch (ObjectDisposedException)
                    {
                        return WebExceptionStatus.RequestCanceled;
                    }
                    catch (Exception exception2)
                    {
                        if (NclUtilities.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                        connectFailure = true;
                    }
                    currentIndex++;
                    if (currentIndex >= addresses.Length)
                    {
                        currentIndex = 0;
                    }
                    num3++;
                }
            }
            this.Failed(addresses);
            if (connectFailure)
            {
                return WebExceptionStatus.ConnectFailure;
            }
            if (timedOut)
            {
                return WebExceptionStatus.Timeout;
            }
            if (!this.InternalProxyServicePoint)
            {
                return WebExceptionStatus.NameResolutionFailure;
            }
            return WebExceptionStatus.ProxyNameResolutionFailure;
        }

        [Conditional("DEBUG")]
        internal void Debug(int requestHash)
        {
            using (IEnumerator enumerator = this.m_ConnectionGroupList.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((ConnectionGroup) enumerator.Current) != null)
                    {
                    }
                }
            }
        }

        internal void DecrementConnection()
        {
            lock (this)
            {
                this.m_CurrentConnections--;
                if (this.m_CurrentConnections == 0)
                {
                    this.m_IdleSince = DateTime.Now;
                    this.m_ExpiringTimer = this.m_IdlingQueue.CreateTimer(ServicePointManager.IdleServicePointTimeoutDelegate, this);
                }
                else if (this.m_CurrentConnections < 0)
                {
                    this.m_CurrentConnections = 0;
                }
            }
        }

        private void Failed(IPAddress[] addresses)
        {
            if (addresses == this.m_IPAddressInfoList)
            {
                lock (this)
                {
                    if (addresses == this.m_IPAddressInfoList)
                    {
                        this.m_AddressListFailed = true;
                    }
                }
            }
        }

        private ConnectionGroup FindConnectionGroup(string connName, bool dontCreate)
        {
            string str = ConnectionGroup.MakeQueryStr(connName);
            ConnectionGroup group = this.m_ConnectionGroupList[str] as ConnectionGroup;
            if ((group == null) && !dontCreate)
            {
                group = new ConnectionGroup(this, connName);
                this.m_ConnectionGroupList[str] = group;
            }
            return group;
        }

        internal Socket GetConnection(PooledStream PooledStream, object owner, bool async, out IPAddress address, ref Socket abortSocket, ref Socket abortSocket6, int timeout)
        {
            Socket socket = null;
            Socket socket2 = null;
            Socket socket3 = null;
            Exception exception = null;
            WebExceptionStatus connectFailure = WebExceptionStatus.ConnectFailure;
            address = null;
            if (Socket.OSSupportsIPv4)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            if (Socket.OSSupportsIPv6)
            {
                socket2 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            abortSocket = socket;
            abortSocket6 = socket2;
            ConnectSocketState state = null;
            if (async)
            {
                state = new ConnectSocketState(this, PooledStream, owner, socket, socket2);
            }
            connectFailure = this.ConnectSocket(socket, socket2, ref socket3, ref address, state, timeout, out exception);
            if (connectFailure == WebExceptionStatus.Pending)
            {
                return null;
            }
            if (connectFailure != WebExceptionStatus.Success)
            {
                throw new WebException(NetRes.GetWebStatusString(connectFailure), ((connectFailure == WebExceptionStatus.ProxyNameResolutionFailure) || (connectFailure == WebExceptionStatus.NameResolutionFailure)) ? this.Host : null, exception, connectFailure, null, WebExceptionInternalStatus.ServicePointFatal);
            }
            if (socket3 == null)
            {
                throw new IOException(SR.GetString("net_io_transportfailure"));
            }
            this.CompleteGetConnection(socket, socket2, socket3, address);
            return socket3;
        }

        private IPAddress[] GetIPAddressInfoList(out int currentIndex, IPAddress[] addresses, int timeout, out bool timedOut)
        {
            IPHostEntry ipHostEntry = null;
            currentIndex = 0;
            bool flag = false;
            bool flag2 = false;
            timedOut = false;
            lock (this)
            {
                if (((addresses != null) && !this.m_ConnectedSinceDns) && (!this.m_AddressListFailed && (addresses == this.m_IPAddressInfoList)))
                {
                    return null;
                }
                if (((this.m_IPAddressInfoList == null) || this.m_AddressListFailed) || ((addresses == this.m_IPAddressInfoList) || this.HasTimedOut))
                {
                    this.m_CurrentAddressInfoIndex = 0;
                    this.m_ConnectedSinceDns = false;
                    this.m_AddressListFailed = false;
                    this.m_LastDnsResolve = DateTime.UtcNow;
                    flag = true;
                }
            }
            if (flag)
            {
                try
                {
                    ipHostEntry = Dns.InternalResolveFast(this.m_Host, timeout, out timedOut);
                    if (timedOut)
                    {
                        flag2 = true;
                    }
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    flag2 = true;
                }
            }
            lock (this)
            {
                if (flag)
                {
                    this.m_IPAddressInfoList = null;
                    if ((!flag2 && (ipHostEntry != null)) && ((ipHostEntry.AddressList != null) && (ipHostEntry.AddressList.Length > 0)))
                    {
                        this.SetAddressList(ipHostEntry);
                    }
                }
                if ((this.m_IPAddressInfoList != null) && (this.m_IPAddressInfoList.Length > 0))
                {
                    currentIndex = this.m_CurrentAddressInfoIndex;
                    if (ServicePointManager.EnableDnsRoundRobin)
                    {
                        this.m_CurrentAddressInfoIndex++;
                        if (this.m_CurrentAddressInfoIndex >= this.m_IPAddressInfoList.Length)
                        {
                            this.m_CurrentAddressInfoIndex = 0;
                        }
                    }
                    return this.m_IPAddressInfoList;
                }
            }
            return null;
        }

        internal void IncrementConnection()
        {
            lock (this)
            {
                this.m_CurrentConnections++;
                if (this.m_CurrentConnections == 1)
                {
                    this.m_ExpiringTimer.Cancel();
                    this.m_ExpiringTimer = null;
                }
            }
        }

        private static bool IsAddressListLoopback(IPAddress[] addressList)
        {
            IPAddress[] localAddresses = null;
            try
            {
                localAddresses = NclUtilities.LocalAddresses;
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.Web, SR.GetString("net_log_retrieving_localhost_exception", new object[] { exception }));
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_resolved_servicepoint_may_not_be_remote_server"));
                }
            }
            int index = 0;
            while (index < addressList.Length)
            {
                if (!IPAddress.IsLoopback(addressList[index]))
                {
                    if (localAddresses == null)
                    {
                        break;
                    }
                    int num2 = 0;
                    while (num2 < localAddresses.Length)
                    {
                        if (addressList[index].Equals(localAddresses[num2]))
                        {
                            break;
                        }
                        num2++;
                    }
                    if (num2 >= localAddresses.Length)
                    {
                        break;
                    }
                }
                index++;
            }
            return (index == addressList.Length);
        }

        internal void ReleaseAllConnectionGroups()
        {
            ArrayList list = new ArrayList(this.m_ConnectionGroupList.Count);
            lock (this)
            {
                foreach (ConnectionGroup group in this.m_ConnectionGroupList.Values)
                {
                    list.Add(group);
                }
                this.m_ConnectionGroupList.Clear();
            }
            foreach (ConnectionGroup group2 in list)
            {
                group2.DisableKeepAliveOnConnections();
            }
        }

        internal bool ReleaseConnectionGroup(string connName)
        {
            lock (this)
            {
                ConnectionGroup group = this.FindConnectionGroup(connName, true);
                if (group == null)
                {
                    return false;
                }
                group.DisableKeepAliveOnConnections();
                this.m_ConnectionGroupList.Remove(connName);
            }
            return true;
        }

        private void ResolveConnectionLimit()
        {
            int connectionLimit = this.ConnectionLimit;
            foreach (ConnectionGroup group in this.m_ConnectionGroupList.Values)
            {
                group.ConnectionLimit = connectionLimit;
            }
        }

        private void SetAddressList(IPHostEntry ipHostEntry)
        {
            bool iPAddressesAreLoopback = this.m_IPAddressesAreLoopback;
            bool flag2 = this.m_IPAddressInfoList == null;
            this.m_IPAddressesAreLoopback = IsAddressListLoopback(ipHostEntry.AddressList);
            this.m_IPAddressInfoList = ipHostEntry.AddressList;
            this.m_HostName = ipHostEntry.HostName;
            if (flag2 || (iPAddressesAreLoopback != this.m_IPAddressesAreLoopback))
            {
                this.ResolveConnectionLimit();
            }
        }

        internal void SetCachedChannelBinding(Uri uri, ChannelBinding binding)
        {
            if (uri.Scheme == Uri.UriSchemeHttps)
            {
                this.m_CachedChannelBinding = (binding != null) ? ((object) binding) : ((object) DBNull.Value);
            }
        }

        public void SetTcpKeepAlive(bool enabled, int keepAliveTime, int keepAliveInterval)
        {
            if (enabled)
            {
                this.m_UseTcpKeepAlive = true;
                if (keepAliveTime <= 0)
                {
                    throw new ArgumentOutOfRangeException("keepAliveTime");
                }
                if (keepAliveInterval <= 0)
                {
                    throw new ArgumentOutOfRangeException("keepAliveInterval");
                }
                this.m_TcpKeepAliveTime = keepAliveTime;
                this.m_TcpKeepAliveInterval = keepAliveInterval;
            }
            else
            {
                this.m_UseTcpKeepAlive = false;
                this.m_TcpKeepAliveTime = 0;
                this.m_TcpKeepAliveInterval = 0;
            }
        }

        internal RemoteCertValidationCallback SetupHandshakeDoneProcedure(TlsStream secureStream, object request)
        {
            return HandshakeDoneProcedure.CreateAdapter(this, secureStream, request);
        }

        internal virtual void SubmitRequest(HttpWebRequest request)
        {
            this.SubmitRequest(request, null);
        }

        internal void SubmitRequest(HttpWebRequest request, string connName)
        {
            Connection connection;
            ConnectionGroup group;
            bool forcedsubmit = false;
            lock (this)
            {
                group = this.FindConnectionGroup(connName, false);
            }
            do
            {
                connection = group.FindConnection(request, connName, out forcedsubmit);
            }
            while ((connection != null) && !connection.SubmitRequest(request, forcedsubmit));
        }

        internal void UpdateClientCertificate(X509Certificate certificate)
        {
            if (certificate != null)
            {
                this.m_ClientCertificateOrBytes = certificate.GetRawCertData();
            }
            else
            {
                this.m_ClientCertificateOrBytes = null;
            }
        }

        private void UpdateCurrentIndex(IPAddress[] addresses, int currentIndex)
        {
            if ((addresses == this.m_IPAddressInfoList) && ((this.m_CurrentAddressInfoIndex != currentIndex) || !this.m_ConnectedSinceDns))
            {
                lock (this)
                {
                    if (addresses == this.m_IPAddressInfoList)
                    {
                        if (!ServicePointManager.EnableDnsRoundRobin)
                        {
                            this.m_CurrentAddressInfoIndex = currentIndex;
                        }
                        this.m_ConnectedSinceDns = true;
                    }
                }
            }
        }

        internal void UpdateServerCertificate(X509Certificate certificate)
        {
            if (certificate != null)
            {
                this.m_ServerCertificateOrBytes = certificate.GetRawCertData();
            }
            else
            {
                this.m_ServerCertificateOrBytes = null;
            }
        }

        public Uri Address
        {
            get
            {
                if (this.m_HostMode)
                {
                    throw new NotSupportedException(SR.GetString("net_servicePointAddressNotSupportedInHostMode"));
                }
                if (this.m_ProxyServicePoint)
                {
                    ExceptionHelper.WebPermissionUnrestricted.Demand();
                }
                return this.m_Address;
            }
        }

        public BindIPEndPoint BindIPEndPointDelegate
        {
            get
            {
                return this.m_BindIPEndPointDelegate;
            }
            set
            {
                ExceptionHelper.InfrastructurePermission.Demand();
                this.m_BindIPEndPointDelegate = value;
            }
        }

        internal object CachedChannelBinding
        {
            get
            {
                return this.m_CachedChannelBinding;
            }
        }

        public X509Certificate Certificate
        {
            get
            {
                object serverCertificateOrBytes = this.m_ServerCertificateOrBytes;
                if ((serverCertificateOrBytes != null) && (serverCertificateOrBytes.GetType() == typeof(byte[])))
                {
                    return (this.m_ServerCertificateOrBytes = new X509Certificate((byte[]) serverCertificateOrBytes));
                }
                return (serverCertificateOrBytes as X509Certificate);
            }
        }

        public X509Certificate ClientCertificate
        {
            get
            {
                object clientCertificateOrBytes = this.m_ClientCertificateOrBytes;
                if ((clientCertificateOrBytes != null) && (clientCertificateOrBytes.GetType() == typeof(byte[])))
                {
                    return (this.m_ClientCertificateOrBytes = new X509Certificate((byte[]) clientCertificateOrBytes));
                }
                return (clientCertificateOrBytes as X509Certificate);
            }
        }

        public int ConnectionLeaseTimeout
        {
            get
            {
                return this.m_ConnectionLeaseTimeout;
            }
            set
            {
                if (!ValidationHelper.ValidateRange(value, -1, 0x7fffffff))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != this.m_ConnectionLeaseTimeout)
                {
                    this.m_ConnectionLeaseTimeout = value;
                    this.m_ConnectionLeaseTimerQueue = null;
                }
            }
        }

        internal TimerThread.Queue ConnectionLeaseTimerQueue
        {
            get
            {
                if (this.m_ConnectionLeaseTimerQueue == null)
                {
                    TimerThread.Queue orCreateQueue = TimerThread.GetOrCreateQueue(this.ConnectionLeaseTimeout);
                    this.m_ConnectionLeaseTimerQueue = orCreateQueue;
                }
                return this.m_ConnectionLeaseTimerQueue;
            }
        }

        public int ConnectionLimit
        {
            get
            {
                if ((!this.m_UserChangedLimit && (this.m_IPAddressInfoList == null)) && (this.m_HostLoopbackGuess == TriState.Unspecified))
                {
                    lock (this)
                    {
                        if ((!this.m_UserChangedLimit && (this.m_IPAddressInfoList == null)) && (this.m_HostLoopbackGuess == TriState.Unspecified))
                        {
                            IPAddress address = null;
                            if (IPAddress.TryParse(this.m_Host, out address))
                            {
                                this.m_HostLoopbackGuess = IsAddressListLoopback(new IPAddress[] { address }) ? TriState.True : TriState.False;
                            }
                            else
                            {
                                this.m_HostLoopbackGuess = NclUtilities.GuessWhetherHostIsLoopback(this.m_Host) ? TriState.True : TriState.False;
                            }
                        }
                    }
                }
                if (!this.m_UserChangedLimit && !((this.m_IPAddressInfoList == null) ? (this.m_HostLoopbackGuess != TriState.True) : !this.m_IPAddressesAreLoopback))
                {
                    return 0x7fffffff;
                }
                return this.m_ConnectionLimit;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (!this.m_UserChangedLimit || (this.m_ConnectionLimit != value))
                {
                    lock (this)
                    {
                        if (!this.m_UserChangedLimit || (this.m_ConnectionLimit != value))
                        {
                            this.m_ConnectionLimit = value;
                            this.m_UserChangedLimit = true;
                            this.ResolveConnectionLimit();
                        }
                    }
                }
            }
        }

        public string ConnectionName
        {
            get
            {
                return this.m_ConnectionName;
            }
        }

        public int CurrentConnections
        {
            get
            {
                int num = 0;
                lock (this)
                {
                    foreach (ConnectionGroup group in this.m_ConnectionGroupList.Values)
                    {
                        num += group.CurrentConnections;
                    }
                }
                return num;
            }
        }

        public bool Expect100Continue
        {
            get
            {
                return this.m_Expect100Continue;
            }
            set
            {
                this.m_Expect100Continue = value;
            }
        }

        private bool HasTimedOut
        {
            get
            {
                int dnsRefreshTimeout = ServicePointManager.DnsRefreshTimeout;
                return ((dnsRefreshTimeout != -1) && ((this.m_LastDnsResolve + new TimeSpan(0, 0, 0, 0, dnsRefreshTimeout)) < DateTime.UtcNow));
            }
        }

        internal string Host
        {
            get
            {
                if (this.m_HostMode)
                {
                    return this.m_Host;
                }
                return this.m_Address.Host;
            }
        }

        internal string Hostname
        {
            get
            {
                return this.m_HostName;
            }
        }

        internal System.Net.HttpBehaviour HttpBehaviour
        {
            get
            {
                return this.m_HttpBehaviour;
            }
            set
            {
                this.m_HttpBehaviour = value;
                this.m_Understands100Continue = this.m_Understands100Continue && ((this.m_HttpBehaviour > System.Net.HttpBehaviour.HTTP10) || (this.m_HttpBehaviour == System.Net.HttpBehaviour.Unknown));
            }
        }

        public DateTime IdleSince
        {
            get
            {
                return this.m_IdleSince;
            }
        }

        internal Uri InternalAddress
        {
            get
            {
                return this.m_Address;
            }
        }

        internal bool InternalProxyServicePoint
        {
            get
            {
                return this.m_ProxyServicePoint;
            }
        }

        internal string LookupString
        {
            get
            {
                return this.m_LookupString;
            }
        }

        public int MaxIdleTime
        {
            get
            {
                return this.m_IdlingQueue.Duration;
            }
            set
            {
                if (!ValidationHelper.ValidateRange(value, -1, 0x7fffffff))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != this.m_IdlingQueue.Duration)
                {
                    lock (this)
                    {
                        if ((this.m_ExpiringTimer == null) || this.m_ExpiringTimer.Cancel())
                        {
                            this.m_IdlingQueue = TimerThread.GetOrCreateQueue(value);
                            if (this.m_ExpiringTimer != null)
                            {
                                TimeSpan span = (TimeSpan) (DateTime.Now - this.m_IdleSince);
                                double totalMilliseconds = span.TotalMilliseconds;
                                int num2 = (totalMilliseconds >= 2147483647.0) ? 0x7fffffff : ((int) totalMilliseconds);
                                int durationMilliseconds = (value == -1) ? -1 : ((num2 >= value) ? 0 : (value - num2));
                                this.m_ExpiringTimer = TimerThread.CreateQueue(durationMilliseconds).CreateTimer(ServicePointManager.IdleServicePointTimeoutDelegate, this);
                            }
                        }
                    }
                }
            }
        }

        internal int Port
        {
            get
            {
                return this.m_Port;
            }
        }

        public virtual Version ProtocolVersion
        {
            get
            {
                if ((this.m_HttpBehaviour <= System.Net.HttpBehaviour.HTTP10) && (this.m_HttpBehaviour != System.Net.HttpBehaviour.Unknown))
                {
                    return HttpVersion.Version10;
                }
                return HttpVersion.Version11;
            }
        }

        public int ReceiveBufferSize
        {
            get
            {
                return this.m_ReceiveBufferSize;
            }
            set
            {
                if (!ValidationHelper.ValidateRange(value, -1, 0x7fffffff))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_ReceiveBufferSize = value;
            }
        }

        public bool SupportsPipelining
        {
            get
            {
                if (this.m_HttpBehaviour <= System.Net.HttpBehaviour.HTTP10)
                {
                    return (this.m_HttpBehaviour == System.Net.HttpBehaviour.Unknown);
                }
                return true;
            }
        }

        internal bool Understands100Continue
        {
            get
            {
                return this.m_Understands100Continue;
            }
            set
            {
                this.m_Understands100Continue = value;
            }
        }

        public bool UseNagleAlgorithm
        {
            get
            {
                return this.m_UseNagleAlgorithm;
            }
            set
            {
                this.m_UseNagleAlgorithm = value;
            }
        }

        private class ConnectSocketState
        {
            internal IPAddress[] addresses;
            internal bool connectFailure;
            internal int currentIndex;
            internal int i;
            internal object owner;
            internal PooledStream pooledStream;
            internal Socket s4;
            internal Socket s6;
            internal ServicePoint servicePoint;
            internal int unsuccessfulAttempts;

            internal ConnectSocketState(ServicePoint servicePoint, PooledStream pooledStream, object owner, Socket s4, Socket s6)
            {
                this.servicePoint = servicePoint;
                this.pooledStream = pooledStream;
                this.owner = owner;
                this.s4 = s4;
                this.s6 = s6;
            }
        }

        private class HandshakeDoneProcedure
        {
            private object m_Request;
            private TlsStream m_SecureStream;
            private ServicePoint m_ServicePoint;

            private HandshakeDoneProcedure(ServicePoint serviePoint, TlsStream secureStream, object request)
            {
                this.m_ServicePoint = serviePoint;
                this.m_SecureStream = secureStream;
                this.m_Request = request;
            }

            private bool CertValidationCallback(string hostName, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                this.m_ServicePoint.UpdateServerCertificate(certificate);
                this.m_ServicePoint.UpdateClientCertificate(this.m_SecureStream.ClientCertificate);
                bool flag = true;
                if ((ServicePointManager.GetLegacyCertificatePolicy() != null) && (this.m_Request is WebRequest))
                {
                    flag = false;
                    bool flag2 = ServicePointManager.CertPolicyValidationCallback.Invoke(hostName, this.m_ServicePoint, certificate, (WebRequest) this.m_Request, chain, sslPolicyErrors);
                    if (!flag2 && (!ServicePointManager.CertPolicyValidationCallback.UsesDefault || (ServicePointManager.ServerCertificateValidationCallback == null)))
                    {
                        return flag2;
                    }
                }
                if (ServicePointManager.ServerCertificateValidationCallback != null)
                {
                    flag = false;
                    return ServicePointManager.ServerCertValidationCallback.Invoke(this.m_Request, certificate, chain, sslPolicyErrors);
                }
                if (flag)
                {
                    return (sslPolicyErrors == SslPolicyErrors.None);
                }
                return true;
            }

            internal static RemoteCertValidationCallback CreateAdapter(ServicePoint serviePoint, TlsStream secureStream, object request)
            {
                ServicePoint.HandshakeDoneProcedure procedure = new ServicePoint.HandshakeDoneProcedure(serviePoint, secureStream, request);
                return new RemoteCertValidationCallback(procedure.CertValidationCallback);
            }
        }
    }
}

