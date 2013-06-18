namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Principal;
    using System.Threading;

    internal class TcpClientTransportSink : BaseChannelSinkWithProperties, IClientChannelSink, IChannelSinkBase
    {
        private TcpClientChannel _channel;
        private string _connectionGroupName;
        private string _machineAndPort;
        private ProtectionLevel _protectionLevel;
        private int _receiveTimeout;
        private int _retryCount;
        private string _securityDomain;
        private string _securityPassword;
        private string _securityUserName;
        private SocketCachePolicy _socketCachePolicy;
        private TimeSpan _socketCacheTimeout;
        private string _spn;
        private TokenImpersonationLevel _tokenImpersonationLevel;
        private bool authSet;
        internal SocketCache ClientSocketCache;
        private const string ConnectionGroupNameKey = "connectiongroupname";
        private const string DomainKey = "domain";
        private string m_machineName;
        private int m_port;
        private const string PasswordKey = "password";
        private const string ProtectionLevelKey = "protectionlevel";
        private const string ReceiveTimeoutKey = "timeout";
        private const string RetryCountKey = "retrycount";
        private static ICollection s_keySet;
        private const string SocketCachePolicyKey = "socketcachepolicy";
        private const string SocketCacheTimeoutKey = "socketcachetimeout";
        private const string SPNKey = "serviceprincipalname";
        private const string TokenImpersonationLevelKey = "tokenimpersonationlevel";
        private const string UserNameKey = "username";

        internal TcpClientTransportSink(string channelURI, TcpClientChannel channel)
        {
            string str;
            this._socketCacheTimeout = TimeSpan.FromSeconds(10.0);
            this._spn = string.Empty;
            this._retryCount = 1;
            this._tokenImpersonationLevel = TokenImpersonationLevel.Identification;
            this._protectionLevel = ProtectionLevel.EncryptAndSign;
            this._channel = channel;
            string uriString = TcpChannelHelper.ParseURL(channelURI, out str);
            this.ClientSocketCache = new SocketCache(new SocketHandlerFactory(this.CreateSocketHandler), this._socketCachePolicy, this._socketCacheTimeout);
            Uri uri = new Uri(uriString);
            if (uri.IsDefaultPort)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_UrlMustHavePort"), new object[] { channelURI }));
            }
            this.m_machineName = uri.Host;
            this.m_port = uri.Port;
            this._machineAndPort = this.m_machineName + ":" + this.m_port;
        }

        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            TcpClientSocketHandler handler = this.SendRequestWithRetry(msg, headers, stream);
            if (handler.OneWayRequest)
            {
                handler.ReturnToCache();
            }
            else
            {
                handler.DataArrivedCallback = new WaitCallback(this.ReceiveCallback);
                handler.DataArrivedCallbackState = sinkStack;
                handler.BeginReadMessage();
            }
        }

        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
        {
            throw new NotSupportedException();
        }

        private Stream CreateAuthenticatedStream(Stream netStream, string machinePortAndSid)
        {
            NetworkCredential defaultCredentials = null;
            NegotiateStream stream = null;
            if (this._securityUserName != null)
            {
                defaultCredentials = new NetworkCredential(this._securityUserName, this._securityPassword, this._securityDomain);
            }
            else
            {
                defaultCredentials = (NetworkCredential) CredentialCache.DefaultCredentials;
            }
            try
            {
                stream = new NegotiateStream(netStream);
                stream.AuthenticateAsClient(defaultCredentials, this._spn, this._protectionLevel, this._tokenImpersonationLevel);
            }
            catch (IOException exception)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_AuthenticationFailed"), new object[0]), exception);
            }
            return stream;
        }

        private SocketHandler CreateSocketHandler(Socket socket, SocketCache socketCache, string machinePortAndSid)
        {
            Stream netStream = new SocketStream(socket);
            if (this._channel.IsSecured)
            {
                netStream = this.CreateAuthenticatedStream(netStream, machinePortAndSid);
            }
            return new TcpClientSocketHandler(socket, machinePortAndSid, netStream, this);
        }

        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        private string GetSid()
        {
            if (this._connectionGroupName != null)
            {
                return this._connectionGroupName;
            }
            return CoreChannel.GetCurrentSidString();
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            Debugger.NotifyOfCrossThreadDependency();
            TcpClientSocketHandler handler = this.SendRequestWithRetry(msg, requestHeaders, requestStream);
            responseHeaders = handler.ReadHeaders();
            responseStream = handler.GetResponseStream();
        }

        private void ReceiveCallback(object state)
        {
            TcpClientSocketHandler handler = null;
            IClientChannelSinkStack dataArrivedCallbackState = null;
            try
            {
                handler = (TcpClientSocketHandler) state;
                dataArrivedCallbackState = (IClientChannelSinkStack) handler.DataArrivedCallbackState;
                ITransportHeaders headers = handler.ReadHeaders();
                Stream responseStream = handler.GetResponseStream();
                dataArrivedCallbackState.AsyncProcessResponse(headers, responseStream);
            }
            catch (Exception exception)
            {
                try
                {
                    if (dataArrivedCallbackState != null)
                    {
                        dataArrivedCallbackState.DispatchException(exception);
                    }
                }
                catch
                {
                }
            }
        }

        private TcpClientSocketHandler SendRequestWithRetry(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream)
        {
            long position = 0L;
            bool flag = true;
            bool canSeek = requestStream.CanSeek;
            if (canSeek)
            {
                position = requestStream.Position;
            }
            TcpClientSocketHandler socket = null;
            string machinePortAndSid = this._machineAndPort + (this._channel.IsSecured ? ("/" + this.GetSid()) : null);
            if (this.authSet && !this._channel.IsSecured)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_AuthenticationConfigClient"));
            }
            bool openNew = (this._channel.IsSecured && (this._securityUserName != null)) && (this._connectionGroupName == null);
            try
            {
                socket = (TcpClientSocketHandler) this.ClientSocketCache.GetSocket(machinePortAndSid, openNew);
                socket.SendRequest(msg, requestHeaders, requestStream);
            }
            catch (SocketException)
            {
                for (int i = 0; ((i < this._retryCount) && canSeek) && flag; i++)
                {
                    try
                    {
                        requestStream.Position = position;
                        socket = (TcpClientSocketHandler) this.ClientSocketCache.GetSocket(machinePortAndSid, openNew);
                        socket.SendRequest(msg, requestHeaders, requestStream);
                        flag = false;
                    }
                    catch (SocketException)
                    {
                    }
                }
                if (flag)
                {
                    throw;
                }
            }
            requestStream.Close();
            return socket;
        }

        public override object this[object key]
        {
            get
            {
                string str = key as string;
                if (str != null)
                {
                    switch (str.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "username":
                            return this._securityUserName;

                        case "password":
                            return null;

                        case "domain":
                            return this._securityDomain;

                        case "socketcachetimeout":
                            return this._socketCacheTimeout;

                        case "timeout":
                            return this._receiveTimeout;

                        case "socketcachepolicy":
                            return this._socketCachePolicy.ToString();

                        case "retrycount":
                            return this._retryCount;

                        case "connectiongroupname":
                            return this._connectionGroupName;

                        case "tokenimpersonationlevel":
                            if (!this.authSet)
                            {
                                break;
                            }
                            return this._tokenImpersonationLevel.ToString();

                        case "protectionlevel":
                            if (!this.authSet)
                            {
                                break;
                            }
                            return this._protectionLevel.ToString();
                    }
                }
                return null;
            }
            set
            {
                string str = key as string;
                if (str != null)
                {
                    switch (str.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "username":
                            this._securityUserName = (string) value;
                            return;

                        case "password":
                            this._securityPassword = (string) value;
                            return;

                        case "domain":
                            this._securityDomain = (string) value;
                            return;

                        case "socketcachetimeout":
                        {
                            int num = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                            if (num < 0)
                            {
                                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_SocketTimeoutNegative"));
                            }
                            this._socketCacheTimeout = TimeSpan.FromSeconds((double) num);
                            this.ClientSocketCache.SocketTimeout = this._socketCacheTimeout;
                            return;
                        }
                        case "timeout":
                            this._receiveTimeout = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                            this.ClientSocketCache.ReceiveTimeout = this._receiveTimeout;
                            return;

                        case "socketcachepolicy":
                            this._socketCachePolicy = (value is SocketCachePolicy) ? ((SocketCachePolicy) value) : ((SocketCachePolicy) System.Enum.Parse(typeof(SocketCachePolicy), (string) value, true));
                            this.ClientSocketCache.CachePolicy = this._socketCachePolicy;
                            return;

                        case "retrycount":
                            this._retryCount = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                            return;

                        case "connectiongroupname":
                            this._connectionGroupName = (string) value;
                            return;

                        case "tokenimpersonationlevel":
                            this._tokenImpersonationLevel = (value is TokenImpersonationLevel) ? ((TokenImpersonationLevel) value) : ((TokenImpersonationLevel) System.Enum.Parse(typeof(TokenImpersonationLevel), (string) value, true));
                            this.authSet = true;
                            return;

                        case "protectionlevel":
                            this._protectionLevel = (value is ProtectionLevel) ? ((ProtectionLevel) value) : ((ProtectionLevel) System.Enum.Parse(typeof(ProtectionLevel), (string) value, true));
                            this.authSet = true;
                            return;

                        case "serviceprincipalname":
                            this._spn = (string) value;
                            this.authSet = true;
                            return;
                    }
                }
            }
        }

        public override ICollection Keys
        {
            get
            {
                if (s_keySet == null)
                {
                    ArrayList list = new ArrayList(6);
                    list.Add("username");
                    list.Add("password");
                    list.Add("domain");
                    list.Add("socketcachetimeout");
                    list.Add("socketcachepolicy");
                    list.Add("retrycount");
                    list.Add("tokenimpersonationlevel");
                    list.Add("protectionlevel");
                    list.Add("connectiongroupname");
                    list.Add("timeout");
                    s_keySet = list;
                }
                return s_keySet;
            }
        }

        public IClientChannelSink NextChannelSink
        {
            get
            {
                return null;
            }
        }
    }
}

