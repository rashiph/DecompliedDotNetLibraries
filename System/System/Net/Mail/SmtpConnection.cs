namespace System.Net.Mail
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    internal class SmtpConnection
    {
        private ISmtpAuthenticationModule[] authenticationModules;
        private const string authExtension = "auth";
        private const string authGssapi = "gssapi";
        private const string authLogin = "login";
        private const string authNtlm = "ntlm";
        private const string authWDigest = "wdigest";
        private System.Net.BufferBuilder bufferBuilder = new System.Net.BufferBuilder();
        private ChannelBinding channelBindingToken;
        internal SmtpClient client;
        private X509CertificateCollection clientCertificates;
        private ConnectionPool connectionPool;
        private ICredentialsByHost credentials;
        private bool enableSsl;
        private string[] extensions;
        private bool isClosed;
        private bool isConnected;
        private bool isStreamOpen;
        private static readonly CreateConnectionDelegate m_CreateConnectionCallback = new CreateConnectionDelegate(SmtpConnection.CreateSmtpPooledStream);
        private EventHandler onCloseHandler;
        internal SmtpTransport parent;
        private PooledStream pooledStream;
        private SmtpReplyReaderFactory responseReader;
        private static readonly ContextCallback s_AuthenticateCallback = new ContextCallback(SmtpConnection.AuthenticateCallback);
        private bool sawNegotiate;
        private bool serverSupportsStartTls;
        private const int sizeOfAuthExtension = 4;
        private const int sizeOfAuthString = 5;
        private SupportedAuth supportedAuth;
        private int timeout = 0x186a0;

        internal SmtpConnection(SmtpTransport parent, SmtpClient client, ICredentialsByHost credentials, ISmtpAuthenticationModule[] authenticationModules)
        {
            this.client = client;
            this.credentials = credentials;
            this.authenticationModules = authenticationModules;
            this.parent = parent;
            this.onCloseHandler = new EventHandler(this.OnClose);
        }

        internal void Abort()
        {
            if (!this.isClosed)
            {
                lock (this)
                {
                    if (!this.isClosed && (this.pooledStream != null))
                    {
                        if (this.channelBindingToken != null)
                        {
                            this.channelBindingToken.Close();
                        }
                        this.pooledStream.Close(0);
                        this.connectionPool.PutConnection(this.pooledStream, this.pooledStream.Owner, this.Timeout, false);
                    }
                    this.isClosed = true;
                }
            }
            this.isConnected = false;
        }

        private static void AuthenticateCallback(object state)
        {
            AuthenticateCallbackContext context = (AuthenticateCallbackContext) state;
            context.result = context.module.Authenticate(null, context.credential, context.thisPtr, context.spn, context.token);
        }

        internal bool AuthSupported(ISmtpAuthenticationModule module)
        {
            if (module is SmtpLoginAuthenticationModule)
            {
                if ((this.supportedAuth & SupportedAuth.Login) > SupportedAuth.None)
                {
                    return true;
                }
            }
            else if (module is SmtpNegotiateAuthenticationModule)
            {
                if ((this.supportedAuth & SupportedAuth.GSSAPI) > SupportedAuth.None)
                {
                    this.sawNegotiate = true;
                    return true;
                }
            }
            else if (module is SmtpNtlmAuthenticationModule)
            {
                if (!this.sawNegotiate && ((this.supportedAuth & SupportedAuth.NTLM) > SupportedAuth.None))
                {
                    return true;
                }
            }
            else if ((module is SmtpDigestAuthenticationModule) && ((this.supportedAuth & SupportedAuth.WDigest) > SupportedAuth.None))
            {
                return true;
            }
            return false;
        }

        internal IAsyncResult BeginFlush(AsyncCallback callback, object state)
        {
            return this.pooledStream.UnsafeBeginWrite(this.bufferBuilder.GetBuffer(), 0, this.bufferBuilder.Length, callback, state);
        }

        internal IAsyncResult BeginGetConnection(ServicePoint servicePoint, ContextAwareResult outerResult, AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Associate(Logging.Web, this, servicePoint);
            }
            if ((this.EnableSsl && (this.ClientCertificates != null)) && (this.ClientCertificates.Count > 0))
            {
                this.connectionPool = ConnectionPoolManager.GetConnectionPool(servicePoint, this.ClientCertificates.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), m_CreateConnectionCallback);
            }
            else
            {
                this.connectionPool = ConnectionPoolManager.GetConnectionPool(servicePoint, "", m_CreateConnectionCallback);
            }
            ConnectAndHandshakeAsyncResult result = new ConnectAndHandshakeAsyncResult(this, servicePoint.Host, servicePoint.Port, outerResult, callback, state);
            result.GetConnection(false);
            return result;
        }

        private static PooledStream CreateSmtpPooledStream(ConnectionPool pool)
        {
            return new SmtpPooledStream(pool, TimeSpan.MaxValue, false);
        }

        internal void EndFlush(IAsyncResult result)
        {
            this.pooledStream.EndWrite(result);
            this.bufferBuilder.Reset();
        }

        internal void EndGetConnection(IAsyncResult result)
        {
            ConnectAndHandshakeAsyncResult.End(result);
        }

        internal void Flush()
        {
            this.pooledStream.Write(this.bufferBuilder.GetBuffer(), 0, this.bufferBuilder.Length);
            this.bufferBuilder.Reset();
        }

        internal Stream GetClosableStream()
        {
            ClosableStream stream = new ClosableStream(this.pooledStream.NetworkStream, this.onCloseHandler);
            this.isStreamOpen = true;
            return stream;
        }

        internal void GetConnection(ServicePoint servicePoint)
        {
            if (this.isConnected)
            {
                throw new InvalidOperationException(SR.GetString("SmtpAlreadyConnected"));
            }
            if (Logging.On)
            {
                Logging.Associate(Logging.Web, this, servicePoint);
            }
            this.connectionPool = ConnectionPoolManager.GetConnectionPool(servicePoint, "", m_CreateConnectionCallback);
            PooledStream pooledStream = this.connectionPool.GetConnection(this, null, this.Timeout);
            while ((((SmtpPooledStream) pooledStream).creds != null) && (((SmtpPooledStream) pooledStream).creds != this.credentials))
            {
                this.connectionPool.PutConnection(pooledStream, pooledStream.Owner, this.Timeout, false);
                pooledStream = this.connectionPool.GetConnection(this, null, this.Timeout);
            }
            if (Logging.On)
            {
                Logging.Associate(Logging.Web, this, pooledStream);
            }
            lock (this)
            {
                this.pooledStream = pooledStream;
            }
            ((SmtpPooledStream) pooledStream).creds = this.credentials;
            this.responseReader = new SmtpReplyReaderFactory(pooledStream.NetworkStream);
            pooledStream.UpdateLifetime();
            if (((SmtpPooledStream) pooledStream).previouslyUsed)
            {
                this.isConnected = true;
            }
            else
            {
                LineInfo info = this.responseReader.GetNextReplyReader().ReadLine();
                if (info.StatusCode != SmtpStatusCode.ServiceReady)
                {
                    throw new SmtpException(info.StatusCode, info.Line, true);
                }
                try
                {
                    this.extensions = EHelloCommand.Send(this, this.client.clientDomain);
                    this.ParseExtensions(this.extensions);
                }
                catch (SmtpException exception)
                {
                    if ((exception.StatusCode != SmtpStatusCode.CommandUnrecognized) && (exception.StatusCode != SmtpStatusCode.CommandNotImplemented))
                    {
                        throw exception;
                    }
                    HelloCommand.Send(this, this.client.clientDomain);
                    this.supportedAuth = SupportedAuth.Login;
                }
                if (this.enableSsl)
                {
                    if (!this.serverSupportsStartTls && !(pooledStream.NetworkStream is TlsStream))
                    {
                        throw new SmtpException(SR.GetString("MailServerDoesNotSupportStartTls"));
                    }
                    StartTlsCommand.Send(this);
                    TlsStream stream2 = new TlsStream(servicePoint.Host, pooledStream.NetworkStream, this.clientCertificates, servicePoint, this.client, null);
                    pooledStream.NetworkStream = stream2;
                    this.channelBindingToken = stream2.GetChannelBinding(ChannelBindingKind.Unique);
                    this.responseReader = new SmtpReplyReaderFactory(pooledStream.NetworkStream);
                    this.extensions = EHelloCommand.Send(this, this.client.clientDomain);
                    this.ParseExtensions(this.extensions);
                }
                if (this.credentials != null)
                {
                    for (int i = 0; i < this.authenticationModules.Length; i++)
                    {
                        Authorization authorization;
                        if (this.AuthSupported(this.authenticationModules[i]))
                        {
                            NetworkCredential credential = this.credentials.GetCredential(servicePoint.Host, servicePoint.Port, this.authenticationModules[i].AuthenticationType);
                            if (credential != null)
                            {
                                authorization = this.SetContextAndTryAuthenticate(this.authenticationModules[i], credential, null);
                                if ((authorization != null) && (authorization.Message != null))
                                {
                                    info = AuthCommand.Send(this, this.authenticationModules[i].AuthenticationType, authorization.Message);
                                    if (info.StatusCode != SmtpStatusCode.CommandParameterNotImplemented)
                                    {
                                        goto Label_0363;
                                    }
                                }
                            }
                        }
                        continue;
                    Label_02F2:
                        authorization = this.authenticationModules[i].Authenticate(info.Line, null, this, this.client.TargetName, this.channelBindingToken);
                        if (authorization == null)
                        {
                            throw new SmtpException(SR.GetString("SmtpAuthenticationFailed"));
                        }
                        info = AuthCommand.Send(this, authorization.Message);
                        if (info.StatusCode == ((SmtpStatusCode) 0xeb))
                        {
                            this.authenticationModules[i].CloseContext(this);
                            this.isConnected = true;
                            return;
                        }
                    Label_0363:
                        if (info.StatusCode == ((SmtpStatusCode) 0x14e))
                        {
                            goto Label_02F2;
                        }
                    }
                }
                this.isConnected = true;
            }
        }

        private void OnClose(object sender, EventArgs args)
        {
            this.isStreamOpen = false;
            DataStopCommand.Send(this);
        }

        internal void ParseExtensions(string[] extensions)
        {
            this.supportedAuth = SupportedAuth.None;
            foreach (string str in extensions)
            {
                if (string.Compare(str, 0, "auth", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    foreach (string str2 in str.Remove(0, 4).Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (string.Compare(str2, "login", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.supportedAuth |= SupportedAuth.Login;
                        }
                        else if (string.Compare(str2, "ntlm", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.supportedAuth |= SupportedAuth.NTLM;
                        }
                        else if (string.Compare(str2, "gssapi", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.supportedAuth |= SupportedAuth.GSSAPI;
                        }
                        else if (string.Compare(str2, "wdigest", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.supportedAuth |= SupportedAuth.WDigest;
                        }
                    }
                }
                else if (string.Compare(str, 0, "dsn ", 0, 3, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ((SmtpPooledStream) this.pooledStream).dsnEnabled = true;
                }
                else if (string.Compare(str, 0, "STARTTLS", 0, 8, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.serverSupportsStartTls = true;
                }
            }
        }

        internal void ReleaseConnection()
        {
            if (!this.isClosed)
            {
                lock (this)
                {
                    if (!this.isClosed && (this.pooledStream != null))
                    {
                        if (this.channelBindingToken != null)
                        {
                            this.channelBindingToken.Close();
                        }
                        ((SmtpPooledStream) this.pooledStream).previouslyUsed = true;
                        this.connectionPool.PutConnection(this.pooledStream, this.pooledStream.Owner, this.Timeout);
                    }
                    this.isClosed = true;
                }
            }
            this.isConnected = false;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        private Authorization SetContextAndTryAuthenticate(ISmtpAuthenticationModule module, NetworkCredential credential, ContextAwareResult context)
        {
            if ((credential is SystemNetworkCredential) && ComNetOS.IsWinNt)
            {
                WindowsIdentity identity = (context == null) ? null : context.Identity;
                try
                {
                    IDisposable disposable = (identity == null) ? null : identity.Impersonate();
                    if (disposable != null)
                    {
                        using (disposable)
                        {
                            return module.Authenticate(null, credential, this, this.client.TargetName, this.channelBindingToken);
                        }
                    }
                    ExecutionContext executionContext = (context == null) ? null : context.ContextCopy;
                    if (executionContext != null)
                    {
                        AuthenticateCallbackContext state = new AuthenticateCallbackContext(this, module, credential, this.client.TargetName, this.channelBindingToken);
                        ExecutionContext.Run(executionContext, s_AuthenticateCallback, state);
                        return state.result;
                    }
                    return module.Authenticate(null, credential, this, this.client.TargetName, this.channelBindingToken);
                }
                catch
                {
                    throw;
                }
            }
            return module.Authenticate(null, credential, this, this.client.TargetName, this.channelBindingToken);
        }

        internal System.Net.BufferBuilder BufferBuilder
        {
            get
            {
                return this.bufferBuilder;
            }
        }

        internal X509CertificateCollection ClientCertificates
        {
            get
            {
                return this.clientCertificates;
            }
            set
            {
                this.clientCertificates = value;
            }
        }

        internal bool DSNEnabled
        {
            get
            {
                return ((this.pooledStream != null) && ((SmtpPooledStream) this.pooledStream).dsnEnabled);
            }
        }

        internal bool EnableSsl
        {
            get
            {
                return this.enableSsl;
            }
            set
            {
                this.enableSsl = value;
            }
        }

        internal bool IsConnected
        {
            get
            {
                return this.isConnected;
            }
        }

        internal bool IsStreamOpen
        {
            get
            {
                return this.isStreamOpen;
            }
        }

        internal SmtpReplyReaderFactory Reader
        {
            get
            {
                return this.responseReader;
            }
        }

        internal int Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = value;
            }
        }

        private class AuthenticateCallbackContext
        {
            internal readonly NetworkCredential credential;
            internal readonly ISmtpAuthenticationModule module;
            internal Authorization result;
            internal readonly string spn;
            internal readonly SmtpConnection thisPtr;
            internal readonly ChannelBinding token;

            internal AuthenticateCallbackContext(SmtpConnection thisPtr, ISmtpAuthenticationModule module, NetworkCredential credential, string spn, ChannelBinding Token)
            {
                this.thisPtr = thisPtr;
                this.module = module;
                this.credential = credential;
                this.spn = spn;
                this.token = Token;
                this.result = null;
            }
        }

        private class ConnectAndHandshakeAsyncResult : LazyAsyncResult
        {
            private static AsyncCallback authenticateCallback = new AsyncCallback(SmtpConnection.ConnectAndHandshakeAsyncResult.AuthenticateCallback);
            private static AsyncCallback authenticateContinueCallback = new AsyncCallback(SmtpConnection.ConnectAndHandshakeAsyncResult.AuthenticateContinueCallback);
            private string authResponse;
            private SmtpConnection connection;
            private int currentModule;
            private static AsyncCallback handshakeCallback = new AsyncCallback(SmtpConnection.ConnectAndHandshakeAsyncResult.HandshakeCallback);
            private string host;
            private static readonly GeneralAsyncDelegate m_ConnectionCreatedCallback = new GeneralAsyncDelegate(SmtpConnection.ConnectAndHandshakeAsyncResult.ConnectionCreatedCallback);
            private readonly ContextAwareResult m_OuterResult;
            private int port;
            private static AsyncCallback sendEHelloCallback = new AsyncCallback(SmtpConnection.ConnectAndHandshakeAsyncResult.SendEHelloCallback);
            private static AsyncCallback sendHelloCallback = new AsyncCallback(SmtpConnection.ConnectAndHandshakeAsyncResult.SendHelloCallback);

            internal ConnectAndHandshakeAsyncResult(SmtpConnection connection, string host, int port, ContextAwareResult outerResult, AsyncCallback callback, object state) : base(null, state, callback)
            {
                this.currentModule = -1;
                this.connection = connection;
                this.host = host;
                this.port = port;
                this.m_OuterResult = outerResult;
            }

            private void Authenticate()
            {
                if (this.connection.credentials != null)
                {
                    while (++this.currentModule < this.connection.authenticationModules.Length)
                    {
                        ISmtpAuthenticationModule module = this.connection.authenticationModules[this.currentModule];
                        if (this.connection.AuthSupported(module))
                        {
                            NetworkCredential credential = this.connection.credentials.GetCredential(this.host, this.port, module.AuthenticationType);
                            if (credential != null)
                            {
                                Authorization authorization = this.connection.SetContextAndTryAuthenticate(module, credential, this.m_OuterResult);
                                if ((authorization != null) && (authorization.Message != null))
                                {
                                    IAsyncResult result = AuthCommand.BeginSend(this.connection, this.connection.authenticationModules[this.currentModule].AuthenticationType, authorization.Message, authenticateCallback, this);
                                    if (!result.CompletedSynchronously)
                                    {
                                        return;
                                    }
                                    LineInfo info = AuthCommand.EndSend(result);
                                    if (info.StatusCode == ((SmtpStatusCode) 0x14e))
                                    {
                                        this.authResponse = info.Line;
                                        if (!this.AuthenticateContinue())
                                        {
                                            return;
                                        }
                                    }
                                    else if (info.StatusCode == ((SmtpStatusCode) 0xeb))
                                    {
                                        module.CloseContext(this.connection);
                                        this.connection.isConnected = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                this.connection.isConnected = true;
                base.InvokeCallback();
            }

            private static void AuthenticateCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SmtpConnection.ConnectAndHandshakeAsyncResult asyncState = (SmtpConnection.ConnectAndHandshakeAsyncResult) result.AsyncState;
                    try
                    {
                        LineInfo info = AuthCommand.EndSend(result);
                        if (info.StatusCode == ((SmtpStatusCode) 0x14e))
                        {
                            asyncState.authResponse = info.Line;
                            if (!asyncState.AuthenticateContinue())
                            {
                                return;
                            }
                        }
                        else if (info.StatusCode == ((SmtpStatusCode) 0xeb))
                        {
                            asyncState.connection.authenticationModules[asyncState.currentModule].CloseContext(asyncState.connection);
                            asyncState.connection.isConnected = true;
                            asyncState.InvokeCallback();
                            return;
                        }
                        asyncState.Authenticate();
                    }
                    catch (Exception exception)
                    {
                        asyncState.InvokeCallback(exception);
                    }
                }
            }

            private bool AuthenticateContinue()
            {
                while (true)
                {
                    Authorization authorization = this.connection.authenticationModules[this.currentModule].Authenticate(this.authResponse, null, this.connection, this.connection.client.TargetName, this.connection.channelBindingToken);
                    if (authorization == null)
                    {
                        throw new SmtpException(SR.GetString("SmtpAuthenticationFailed"));
                    }
                    IAsyncResult result = AuthCommand.BeginSend(this.connection, authorization.Message, authenticateContinueCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    LineInfo info = AuthCommand.EndSend(result);
                    if (info.StatusCode == ((SmtpStatusCode) 0xeb))
                    {
                        this.connection.authenticationModules[this.currentModule].CloseContext(this.connection);
                        this.connection.isConnected = true;
                        base.InvokeCallback();
                        return false;
                    }
                    if (info.StatusCode != ((SmtpStatusCode) 0x14e))
                    {
                        return true;
                    }
                    this.authResponse = info.Line;
                }
            }

            private static void AuthenticateContinueCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SmtpConnection.ConnectAndHandshakeAsyncResult asyncState = (SmtpConnection.ConnectAndHandshakeAsyncResult) result.AsyncState;
                    try
                    {
                        LineInfo info = AuthCommand.EndSend(result);
                        if (info.StatusCode == ((SmtpStatusCode) 0xeb))
                        {
                            asyncState.connection.authenticationModules[asyncState.currentModule].CloseContext(asyncState.connection);
                            asyncState.connection.isConnected = true;
                            asyncState.InvokeCallback();
                        }
                        else
                        {
                            if (info.StatusCode == ((SmtpStatusCode) 0x14e))
                            {
                                asyncState.authResponse = info.Line;
                                if (!asyncState.AuthenticateContinue())
                                {
                                    return;
                                }
                            }
                            asyncState.Authenticate();
                        }
                    }
                    catch (Exception exception)
                    {
                        asyncState.InvokeCallback(exception);
                    }
                }
            }

            private static void ConnectionCreatedCallback(object request, object state)
            {
                SmtpConnection.ConnectAndHandshakeAsyncResult owningObject = (SmtpConnection.ConnectAndHandshakeAsyncResult) request;
                if (state is Exception)
                {
                    owningObject.InvokeCallback((Exception) state);
                }
                else
                {
                    SmtpPooledStream pooledStream = (SmtpPooledStream) ((PooledStream) state);
                    try
                    {
                        while ((pooledStream.creds != null) && (pooledStream.creds != owningObject.connection.credentials))
                        {
                            owningObject.connection.connectionPool.PutConnection(pooledStream, pooledStream.Owner, owningObject.connection.Timeout, false);
                            pooledStream = (SmtpPooledStream) owningObject.connection.connectionPool.GetConnection(owningObject, m_ConnectionCreatedCallback, owningObject.connection.Timeout);
                            if (pooledStream == null)
                            {
                                return;
                            }
                        }
                        if (Logging.On)
                        {
                            Logging.Associate(Logging.Web, owningObject.connection, pooledStream);
                        }
                        pooledStream.Owner = owningObject.connection;
                        pooledStream.creds = owningObject.connection.credentials;
                        lock (owningObject.connection)
                        {
                            if (owningObject.connection.isClosed)
                            {
                                owningObject.connection.connectionPool.PutConnection(pooledStream, pooledStream.Owner, owningObject.connection.Timeout, false);
                                owningObject.InvokeCallback(null);
                                return;
                            }
                            owningObject.connection.pooledStream = pooledStream;
                        }
                        owningObject.Handshake();
                    }
                    catch (Exception exception)
                    {
                        owningObject.InvokeCallback(exception);
                    }
                }
            }

            internal static void End(IAsyncResult result)
            {
                object obj2 = ((SmtpConnection.ConnectAndHandshakeAsyncResult) result).InternalWaitForCompletion();
                if (obj2 is Exception)
                {
                    throw ((Exception) obj2);
                }
            }

            internal void GetConnection(bool synchronous)
            {
                if (this.connection.isConnected)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpAlreadyConnected"));
                }
                SmtpPooledStream pooledStream = (SmtpPooledStream) this.connection.connectionPool.GetConnection(this, synchronous ? null : m_ConnectionCreatedCallback, this.connection.Timeout);
                if (pooledStream != null)
                {
                    try
                    {
                        while ((pooledStream.creds != null) && (pooledStream.creds != this.connection.credentials))
                        {
                            this.connection.connectionPool.PutConnection(pooledStream, pooledStream.Owner, this.connection.Timeout, false);
                            pooledStream = (SmtpPooledStream) this.connection.connectionPool.GetConnection(this, synchronous ? null : m_ConnectionCreatedCallback, this.connection.Timeout);
                            if (pooledStream == null)
                            {
                                return;
                            }
                        }
                        pooledStream.creds = this.connection.credentials;
                        pooledStream.Owner = this.connection;
                        lock (this.connection)
                        {
                            this.connection.pooledStream = pooledStream;
                        }
                        this.Handshake();
                    }
                    catch (Exception exception)
                    {
                        base.InvokeCallback(exception);
                    }
                }
            }

            private void Handshake()
            {
                this.connection.responseReader = new SmtpReplyReaderFactory(this.connection.pooledStream.NetworkStream);
                this.connection.pooledStream.UpdateLifetime();
                if (((SmtpPooledStream) this.connection.pooledStream).previouslyUsed)
                {
                    this.connection.isConnected = true;
                    base.InvokeCallback();
                }
                else
                {
                    SmtpReplyReader nextReplyReader = this.connection.Reader.GetNextReplyReader();
                    IAsyncResult result = nextReplyReader.BeginReadLine(handshakeCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        LineInfo info = nextReplyReader.EndReadLine(result);
                        if (info.StatusCode != SmtpStatusCode.ServiceReady)
                        {
                            throw new SmtpException(info.StatusCode, info.Line, true);
                        }
                        try
                        {
                            if (!this.SendEHello())
                            {
                            }
                        }
                        catch
                        {
                            if (!this.SendHello())
                            {
                            }
                        }
                    }
                }
            }

            private static void HandshakeCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SmtpConnection.ConnectAndHandshakeAsyncResult asyncState = (SmtpConnection.ConnectAndHandshakeAsyncResult) result.AsyncState;
                    try
                    {
                        try
                        {
                            LineInfo info = asyncState.connection.Reader.CurrentReader.EndReadLine(result);
                            if (info.StatusCode != SmtpStatusCode.ServiceReady)
                            {
                                asyncState.InvokeCallback(new SmtpException(info.StatusCode, info.Line, true));
                            }
                            else if (!asyncState.SendEHello())
                            {
                            }
                        }
                        catch (SmtpException)
                        {
                            if (!asyncState.SendHello())
                            {
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        asyncState.InvokeCallback(exception);
                    }
                }
            }

            private bool SendEHello()
            {
                IAsyncResult result = EHelloCommand.BeginSend(this.connection, this.connection.client.clientDomain, sendEHelloCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                this.connection.extensions = EHelloCommand.EndSend(result);
                this.connection.ParseExtensions(this.connection.extensions);
                if (this.connection.pooledStream.NetworkStream is TlsStream)
                {
                    this.Authenticate();
                    return true;
                }
                if (this.connection.EnableSsl)
                {
                    if (!this.connection.serverSupportsStartTls && !(this.connection.pooledStream.NetworkStream is TlsStream))
                    {
                        throw new SmtpException(SR.GetString("MailServerDoesNotSupportStartTls"));
                    }
                    this.SendStartTls();
                }
                else
                {
                    this.Authenticate();
                }
                return true;
            }

            private static void SendEHelloCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SmtpConnection.ConnectAndHandshakeAsyncResult asyncState = (SmtpConnection.ConnectAndHandshakeAsyncResult) result.AsyncState;
                    try
                    {
                        try
                        {
                            asyncState.connection.extensions = EHelloCommand.EndSend(result);
                            asyncState.connection.ParseExtensions(asyncState.connection.extensions);
                            if (asyncState.connection.pooledStream.NetworkStream is TlsStream)
                            {
                                asyncState.Authenticate();
                                return;
                            }
                        }
                        catch (SmtpException exception)
                        {
                            if ((exception.StatusCode != SmtpStatusCode.CommandUnrecognized) && (exception.StatusCode != SmtpStatusCode.CommandNotImplemented))
                            {
                                throw exception;
                            }
                            if (!asyncState.SendHello())
                            {
                                return;
                            }
                        }
                        if (asyncState.connection.EnableSsl)
                        {
                            if (!asyncState.connection.serverSupportsStartTls && !(asyncState.connection.pooledStream.NetworkStream is TlsStream))
                            {
                                throw new SmtpException(SR.GetString("MailServerDoesNotSupportStartTls"));
                            }
                            asyncState.SendStartTls();
                        }
                        else
                        {
                            asyncState.Authenticate();
                        }
                    }
                    catch (Exception exception2)
                    {
                        asyncState.InvokeCallback(exception2);
                    }
                }
            }

            private bool SendHello()
            {
                IAsyncResult result = HelloCommand.BeginSend(this.connection, this.connection.client.clientDomain, sendHelloCallback, this);
                if (result.CompletedSynchronously)
                {
                    this.connection.supportedAuth = SupportedAuth.Login;
                    HelloCommand.EndSend(result);
                    this.Authenticate();
                    return true;
                }
                return false;
            }

            private static void SendHelloCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SmtpConnection.ConnectAndHandshakeAsyncResult asyncState = (SmtpConnection.ConnectAndHandshakeAsyncResult) result.AsyncState;
                    try
                    {
                        HelloCommand.EndSend(result);
                        asyncState.Authenticate();
                    }
                    catch (Exception exception)
                    {
                        asyncState.InvokeCallback(exception);
                    }
                }
            }

            private bool SendStartTls()
            {
                IAsyncResult result = StartTlsCommand.BeginSend(this.connection, new AsyncCallback(SmtpConnection.ConnectAndHandshakeAsyncResult.SendStartTlsCallback), this);
                if (result.CompletedSynchronously)
                {
                    StartTlsCommand.EndSend(result);
                    TlsStream stream = new TlsStream(this.connection.pooledStream.ServicePoint.Host, this.connection.pooledStream.NetworkStream, this.connection.ClientCertificates, this.connection.pooledStream.ServicePoint, this.connection.client, this.m_OuterResult.ContextCopy);
                    this.connection.pooledStream.NetworkStream = stream;
                    this.connection.responseReader = new SmtpReplyReaderFactory(this.connection.pooledStream.NetworkStream);
                    this.SendEHello();
                    return true;
                }
                return false;
            }

            private static void SendStartTlsCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SmtpConnection.ConnectAndHandshakeAsyncResult asyncState = (SmtpConnection.ConnectAndHandshakeAsyncResult) result.AsyncState;
                    try
                    {
                        StartTlsCommand.EndSend(result);
                        TlsStream stream = new TlsStream(asyncState.connection.pooledStream.ServicePoint.Host, asyncState.connection.pooledStream.NetworkStream, asyncState.connection.ClientCertificates, asyncState.connection.pooledStream.ServicePoint, asyncState.connection.client, asyncState.m_OuterResult.ContextCopy);
                        asyncState.connection.pooledStream.NetworkStream = stream;
                        asyncState.connection.responseReader = new SmtpReplyReaderFactory(asyncState.connection.pooledStream.NetworkStream);
                        asyncState.SendEHello();
                    }
                    catch (Exception exception)
                    {
                        asyncState.InvokeCallback(exception);
                    }
                }
            }
        }
    }
}

