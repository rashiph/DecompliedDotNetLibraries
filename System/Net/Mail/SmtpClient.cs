namespace System.Net.Mail
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Net.Configuration;
    using System.Net.NetworkInformation;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    public class SmtpClient : IDisposable
    {
        private static AsyncCallback _ContextSafeCompleteCallback = new AsyncCallback(SmtpClient.ContextSafeCompleteCallback);
        private AsyncOperation asyncOp;
        private bool cancelled;
        internal string clientDomain;
        private static int defaultPort = 0x19;
        private SmtpDeliveryMethod deliveryMethod;
        private bool disposed;
        private string host;
        private bool inCall;
        private static MailSettingsSectionGroupInternal mailConfiguration;
        private const int maxPortValue = 0xffff;
        private MailMessage message;
        private SendOrPostCallback onSendCompletedDelegate;
        private ContextAwareResult operationCompletedResult;
        private string pickupDirectoryLocation;
        private int port;
        private MailAddressCollection recipients;
        private System.Net.ServicePoint servicePoint;
        private bool servicePointChanged;
        private string targetName;
        private bool timedOut;
        private System.Threading.Timer timer;
        private SmtpTransport transport;
        private MailWriter writer;

        public event SendCompletedEventHandler SendCompleted;

        public SmtpClient()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, "SmtpClient", ".ctor", "");
            }
            try
            {
                this.Initialize();
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, "SmtpClient", ".ctor", this);
                }
            }
        }

        public SmtpClient(string host)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, "SmtpClient", ".ctor", "host=" + host);
            }
            try
            {
                this.host = host;
                this.Initialize();
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, "SmtpClient", ".ctor", this);
                }
            }
        }

        public SmtpClient(string host, int port)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, "SmtpClient", ".ctor", string.Concat(new object[] { "host=", host, ", port=", port }));
            }
            try
            {
                if (port < 0)
                {
                    throw new ArgumentOutOfRangeException("port");
                }
                this.host = host;
                this.port = port;
                this.Initialize();
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, "SmtpClient", ".ctor", this);
                }
            }
        }

        private void Abort()
        {
            try
            {
                this.transport.Abort();
            }
            catch
            {
            }
        }

        private void CheckHostAndPort()
        {
            if ((this.host == null) || (this.host.Length == 0))
            {
                throw new InvalidOperationException(SR.GetString("UnspecifiedHost"));
            }
            if ((this.port <= 0) || (this.port > 0xffff))
            {
                throw new InvalidOperationException(SR.GetString("InvalidPort"));
            }
        }

        private void Complete(Exception exception, IAsyncResult result)
        {
            ContextAwareResult asyncState = (ContextAwareResult) result.AsyncState;
            try
            {
                if (this.cancelled)
                {
                    exception = null;
                    this.Abort();
                }
                else if ((exception != null) && (!(exception is SmtpFailedRecipientException) || ((SmtpFailedRecipientException) exception).fatal))
                {
                    this.Abort();
                    if (!(exception is SmtpException))
                    {
                        exception = new SmtpException(SR.GetString("SmtpSendMailFailure"), exception);
                    }
                }
                else
                {
                    if (this.writer != null)
                    {
                        this.writer.Close();
                    }
                    this.transport.ReleaseConnection();
                }
            }
            finally
            {
                asyncState.InvokeCallback(exception);
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                this.transport.EndGetConnection(result);
                if (this.cancelled)
                {
                    this.Complete(null, result);
                }
                else
                {
                    this.transport.BeginSendMail((this.message.Sender != null) ? this.message.Sender : this.message.From, this.recipients, this.message.BuildDeliveryStatusNotificationString(), new AsyncCallback(this.SendMailCallback), result.AsyncState);
                }
            }
            catch (Exception exception)
            {
                this.Complete(exception, result);
            }
        }

        private static void ContextSafeCompleteCallback(IAsyncResult ar)
        {
            ContextAwareResult result = (ContextAwareResult) ar;
            SmtpClient asyncState = (SmtpClient) ar.AsyncState;
            Exception error = result.Result as Exception;
            AsyncOperation asyncOp = asyncState.asyncOp;
            AsyncCompletedEventArgs arg = new AsyncCompletedEventArgs(error, asyncState.cancelled, asyncOp.UserSuppliedState);
            asyncState.InCall = false;
            asyncOp.PostOperationCompleted(asyncState.onSendCompletedDelegate, arg);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                if (this.InCall && !this.cancelled)
                {
                    this.cancelled = true;
                    this.Abort();
                }
                if (this.transport != null)
                {
                    this.transport.CloseIdleConnections(this.ServicePoint);
                }
                if (this.timer != null)
                {
                    this.timer.Dispose();
                }
                this.disposed = true;
            }
        }

        private void GetConnection()
        {
            if (!this.transport.IsConnected)
            {
                this.transport.GetConnection(this.ServicePoint);
            }
        }

        internal MailWriter GetFileMailWriter(string pickupDirectory)
        {
            string str2;
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, "SmtpClient.Send() pickupDirectory=" + pickupDirectory);
            }
            if (!Path.IsPathRooted(pickupDirectory))
            {
                throw new SmtpException(SR.GetString("SmtpNeedAbsolutePickupDirectory"));
            }
            do
            {
                string str = Guid.NewGuid().ToString() + ".eml";
                str2 = Path.Combine(pickupDirectory, str);
            }
            while (System.IO.File.Exists(str2));
            return new MailWriter(new FileStream(str2, FileMode.CreateNew));
        }

        private void Initialize()
        {
            if ((this.port == defaultPort) || (this.port == 0))
            {
                new SmtpPermission(SmtpAccess.Connect).Demand();
            }
            else
            {
                new SmtpPermission(SmtpAccess.ConnectToUnrestrictedPort).Demand();
            }
            this.transport = new SmtpTransport(this);
            if (Logging.On)
            {
                Logging.Associate(Logging.Web, this, this.transport);
            }
            this.onSendCompletedDelegate = new SendOrPostCallback(this.SendCompletedWaitCallback);
            if (MailConfiguration.Smtp != null)
            {
                if (MailConfiguration.Smtp.Network != null)
                {
                    if ((this.host == null) || (this.host.Length == 0))
                    {
                        this.host = MailConfiguration.Smtp.Network.Host;
                    }
                    if (this.port == 0)
                    {
                        this.port = MailConfiguration.Smtp.Network.Port;
                    }
                    this.transport.Credentials = MailConfiguration.Smtp.Network.Credential;
                    this.transport.EnableSsl = MailConfiguration.Smtp.Network.EnableSsl;
                    if (MailConfiguration.Smtp.Network.TargetName != null)
                    {
                        this.targetName = MailConfiguration.Smtp.Network.TargetName;
                    }
                    this.clientDomain = MailConfiguration.Smtp.Network.ClientDomain;
                }
                this.deliveryMethod = MailConfiguration.Smtp.DeliveryMethod;
                if (MailConfiguration.Smtp.SpecifiedPickupDirectory != null)
                {
                    this.pickupDirectoryLocation = MailConfiguration.Smtp.SpecifiedPickupDirectory.PickupDirectoryLocation;
                }
            }
            if ((this.host != null) && (this.host.Length != 0))
            {
                this.host = this.host.Trim();
            }
            if (this.port == 0)
            {
                this.port = defaultPort;
            }
            if (this.targetName == null)
            {
                this.targetName = "SMTPSVC/" + this.host;
            }
            if (this.clientDomain == null)
            {
                string hostName = IPGlobalProperties.InternalGetIPGlobalProperties().HostName;
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hostName.Length; i++)
                {
                    char ch = hostName[i];
                    if (ch <= '\x007f')
                    {
                        builder.Append(ch);
                    }
                }
                if (builder.Length > 0)
                {
                    this.clientDomain = builder.ToString();
                }
                else
                {
                    this.clientDomain = "LocalHost";
                }
            }
        }

        protected void OnSendCompleted(AsyncCompletedEventArgs e)
        {
            if (this.SendCompleted != null)
            {
                this.SendCompleted(this, e);
            }
        }

        public void Send(MailMessage message)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "Send", message);
            }
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            try
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.Web, this, "Send", "DeliveryMethod=" + this.DeliveryMethod.ToString());
                }
                if (Logging.On)
                {
                    Logging.Associate(Logging.Web, this, message);
                }
                SmtpFailedRecipientException exception = null;
                if (this.InCall)
                {
                    throw new InvalidOperationException(SR.GetString("net_inasync"));
                }
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }
                if (this.DeliveryMethod == SmtpDeliveryMethod.Network)
                {
                    this.CheckHostAndPort();
                }
                MailAddressCollection recipients = new MailAddressCollection();
                if (message.From == null)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpFromRequired"));
                }
                if (message.To != null)
                {
                    foreach (MailAddress address in message.To)
                    {
                        recipients.Add(address);
                    }
                }
                if (message.Bcc != null)
                {
                    foreach (MailAddress address2 in message.Bcc)
                    {
                        recipients.Add(address2);
                    }
                }
                if (message.CC != null)
                {
                    foreach (MailAddress address3 in message.CC)
                    {
                        recipients.Add(address3);
                    }
                }
                if (recipients.Count == 0)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpRecipientRequired"));
                }
                this.transport.IdentityRequired = false;
                try
                {
                    this.InCall = true;
                    this.timedOut = false;
                    this.timer = new System.Threading.Timer(new TimerCallback(this.TimeOutCallback), null, this.Timeout, this.Timeout);
                    switch (this.DeliveryMethod)
                    {
                        case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                            if (this.EnableSsl)
                            {
                                throw new SmtpException(SR.GetString("SmtpPickupDirectoryDoesnotSupportSsl"));
                            }
                            break;

                        case SmtpDeliveryMethod.PickupDirectoryFromIis:
                            if (this.EnableSsl)
                            {
                                throw new SmtpException(SR.GetString("SmtpPickupDirectoryDoesnotSupportSsl"));
                            }
                            goto Label_0235;

                        default:
                            goto Label_0244;
                    }
                    MailWriter fileMailWriter = this.GetFileMailWriter(this.PickupDirectoryLocation);
                    goto Label_0276;
                Label_0235:
                    fileMailWriter = this.GetFileMailWriter(IisPickupDirectory.GetPickupDirectory());
                    goto Label_0276;
                Label_0244:
                    this.GetConnection();
                    fileMailWriter = this.transport.SendMail((message.Sender != null) ? message.Sender : message.From, recipients, message.BuildDeliveryStatusNotificationString(), out exception);
                Label_0276:
                    this.message = message;
                    message.Send(fileMailWriter, this.DeliveryMethod != SmtpDeliveryMethod.Network);
                    fileMailWriter.Close();
                    this.transport.ReleaseConnection();
                    if ((this.DeliveryMethod == SmtpDeliveryMethod.Network) && (exception != null))
                    {
                        throw exception;
                    }
                }
                catch (Exception exception2)
                {
                    if (Logging.On)
                    {
                        Logging.Exception(Logging.Web, this, "Send", exception2);
                    }
                    if ((exception2 is SmtpFailedRecipientException) && !((SmtpFailedRecipientException) exception2).fatal)
                    {
                        throw;
                    }
                    this.Abort();
                    if (this.timedOut)
                    {
                        throw new SmtpException(SR.GetString("net_timeout"));
                    }
                    if (((exception2 is SecurityException) || (exception2 is AuthenticationException)) || (exception2 is SmtpException))
                    {
                        throw;
                    }
                    throw new SmtpException(SR.GetString("SmtpSendMailFailure"), exception2);
                }
                finally
                {
                    this.InCall = false;
                    if (this.timer != null)
                    {
                        this.timer.Dispose();
                    }
                }
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "Send", (string) null);
                }
            }
        }

        public void Send(string from, string recipients, string subject, string body)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            MailMessage message = new MailMessage(from, recipients, subject, body);
            this.Send(message);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(MailMessage message, object userToken)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "SendAsync", "DeliveryMethod=" + this.DeliveryMethod.ToString());
            }
            try
            {
                if (this.InCall)
                {
                    throw new InvalidOperationException(SR.GetString("net_inasync"));
                }
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }
                if (this.DeliveryMethod == SmtpDeliveryMethod.Network)
                {
                    this.CheckHostAndPort();
                }
                this.recipients = new MailAddressCollection();
                if (message.From == null)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpFromRequired"));
                }
                if (message.To != null)
                {
                    foreach (MailAddress address in message.To)
                    {
                        this.recipients.Add(address);
                    }
                }
                if (message.Bcc != null)
                {
                    foreach (MailAddress address2 in message.Bcc)
                    {
                        this.recipients.Add(address2);
                    }
                }
                if (message.CC != null)
                {
                    foreach (MailAddress address3 in message.CC)
                    {
                        this.recipients.Add(address3);
                    }
                }
                if (this.recipients.Count == 0)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpRecipientRequired"));
                }
                try
                {
                    CredentialCache cache;
                    this.InCall = true;
                    this.cancelled = false;
                    this.message = message;
                    this.transport.IdentityRequired = ((this.Credentials != null) && ComNetOS.IsWinNt) && (((this.Credentials is SystemNetworkCredential) || ((cache = this.Credentials as CredentialCache) == null)) || cache.IsDefaultInCache);
                    this.asyncOp = AsyncOperationManager.CreateOperation(userToken);
                    switch (this.DeliveryMethod)
                    {
                        case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                            if (this.EnableSsl)
                            {
                                throw new SmtpException(SR.GetString("SmtpPickupDirectoryDoesnotSupportSsl"));
                            }
                            break;

                        case SmtpDeliveryMethod.PickupDirectoryFromIis:
                            if (this.EnableSsl)
                            {
                                throw new SmtpException(SR.GetString("SmtpPickupDirectoryDoesnotSupportSsl"));
                            }
                            goto Label_02B2;

                        default:
                            goto Label_0329;
                    }
                    this.writer = this.GetFileMailWriter(this.PickupDirectoryLocation);
                    message.Send(this.writer, this.DeliveryMethod != SmtpDeliveryMethod.Network);
                    if (this.writer != null)
                    {
                        this.writer.Close();
                    }
                    this.transport.ReleaseConnection();
                    AsyncCompletedEventArgs arg = new AsyncCompletedEventArgs(null, false, this.asyncOp.UserSuppliedState);
                    this.InCall = false;
                    this.asyncOp.PostOperationCompleted(this.onSendCompletedDelegate, arg);
                    return;
                Label_02B2:
                    this.writer = this.GetFileMailWriter(IisPickupDirectory.GetPickupDirectory());
                    message.Send(this.writer, this.DeliveryMethod != SmtpDeliveryMethod.Network);
                    if (this.writer != null)
                    {
                        this.writer.Close();
                    }
                    this.transport.ReleaseConnection();
                    AsyncCompletedEventArgs args2 = new AsyncCompletedEventArgs(null, false, this.asyncOp.UserSuppliedState);
                    this.InCall = false;
                    this.asyncOp.PostOperationCompleted(this.onSendCompletedDelegate, args2);
                    return;
                Label_0329:
                    this.operationCompletedResult = new ContextAwareResult(this.transport.IdentityRequired, true, null, this, _ContextSafeCompleteCallback);
                    lock (this.operationCompletedResult.StartPostingAsyncOp())
                    {
                        this.transport.BeginGetConnection(this.ServicePoint, this.operationCompletedResult, new AsyncCallback(this.ConnectCallback), this.operationCompletedResult);
                        this.operationCompletedResult.FinishPostingAsyncOp();
                    }
                }
                catch (Exception exception)
                {
                    this.InCall = false;
                    if (Logging.On)
                    {
                        Logging.Exception(Logging.Web, this, "Send", exception);
                    }
                    if ((exception is SmtpFailedRecipientException) && !((SmtpFailedRecipientException) exception).fatal)
                    {
                        throw;
                    }
                    this.Abort();
                    if (this.timedOut)
                    {
                        throw new SmtpException(SR.GetString("net_timeout"));
                    }
                    if (((exception is SecurityException) || (exception is AuthenticationException)) || (exception is SmtpException))
                    {
                        throw;
                    }
                    throw new SmtpException(SR.GetString("SmtpSendMailFailure"), exception);
                }
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "SendAsync", (string) null);
                }
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(string from, string recipients, string subject, string body, object userToken)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            this.SendAsync(new MailMessage(from, recipients, subject, body), userToken);
        }

        public void SendAsyncCancel()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "SendAsyncCancel", (string) null);
            }
            try
            {
                if (this.InCall && !this.cancelled)
                {
                    this.cancelled = true;
                    this.Abort();
                }
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "SendAsyncCancel", (string) null);
                }
            }
        }

        private void SendCompletedWaitCallback(object operationState)
        {
            this.OnSendCompleted((AsyncCompletedEventArgs) operationState);
        }

        private void SendMailCallback(IAsyncResult result)
        {
            try
            {
                this.writer = this.transport.EndSendMail(result);
            }
            catch (Exception exception)
            {
                if (!(exception is SmtpFailedRecipientException) || ((SmtpFailedRecipientException) exception).fatal)
                {
                    this.Complete(exception, result);
                    return;
                }
            }
            try
            {
                if (this.cancelled)
                {
                    this.Complete(null, result);
                }
                else
                {
                    this.message.BeginSend(this.writer, this.DeliveryMethod != SmtpDeliveryMethod.Network, new AsyncCallback(this.SendMessageCallback), result.AsyncState);
                }
            }
            catch (Exception exception2)
            {
                this.Complete(exception2, result);
            }
        }

        private void SendMessageCallback(IAsyncResult result)
        {
            try
            {
                this.message.EndSend(result);
                this.Complete(null, result);
            }
            catch (Exception exception)
            {
                this.Complete(exception, result);
            }
        }

        private void TimeOutCallback(object state)
        {
            if (!this.timedOut)
            {
                this.timedOut = true;
                this.Abort();
            }
        }

        public X509CertificateCollection ClientCertificates
        {
            get
            {
                return this.transport.ClientCertificates;
            }
        }

        public ICredentialsByHost Credentials
        {
            get
            {
                return this.transport.Credentials;
            }
            set
            {
                if (this.InCall)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpInvalidOperationDuringSend"));
                }
                this.transport.Credentials = value;
            }
        }

        public SmtpDeliveryMethod DeliveryMethod
        {
            get
            {
                return this.deliveryMethod;
            }
            set
            {
                this.deliveryMethod = value;
            }
        }

        public bool EnableSsl
        {
            get
            {
                return this.transport.EnableSsl;
            }
            set
            {
                this.transport.EnableSsl = value;
            }
        }

        public string Host
        {
            get
            {
                return this.host;
            }
            set
            {
                if (this.InCall)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpInvalidOperationDuringSend"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value == string.Empty)
                {
                    throw new ArgumentException(SR.GetString("net_emptystringset"), "value");
                }
                value = value.Trim();
                if (value != this.host)
                {
                    this.host = value;
                    this.servicePointChanged = true;
                }
            }
        }

        internal bool InCall
        {
            get
            {
                return this.inCall;
            }
            set
            {
                this.inCall = value;
            }
        }

        internal static MailSettingsSectionGroupInternal MailConfiguration
        {
            get
            {
                if (mailConfiguration == null)
                {
                    mailConfiguration = MailSettingsSectionGroupInternal.GetSection();
                }
                return mailConfiguration;
            }
        }

        public string PickupDirectoryLocation
        {
            get
            {
                return this.pickupDirectoryLocation;
            }
            set
            {
                this.pickupDirectoryLocation = value;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                if (this.InCall)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpInvalidOperationDuringSend"));
                }
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != defaultPort)
                {
                    new SmtpPermission(SmtpAccess.ConnectToUnrestrictedPort).Demand();
                }
                if (value != this.port)
                {
                    this.port = value;
                    this.servicePointChanged = true;
                }
            }
        }

        public System.Net.ServicePoint ServicePoint
        {
            get
            {
                this.CheckHostAndPort();
                if ((this.servicePoint == null) || this.servicePointChanged)
                {
                    this.servicePoint = ServicePointManager.FindServicePoint(this.host, this.port);
                    this.servicePointChanged = false;
                }
                return this.servicePoint;
            }
        }

        public string TargetName
        {
            get
            {
                return this.targetName;
            }
            set
            {
                this.targetName = value;
            }
        }

        public int Timeout
        {
            get
            {
                return this.transport.Timeout;
            }
            set
            {
                if (this.InCall)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpInvalidOperationDuringSend"));
                }
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.transport.Timeout = value;
            }
        }

        public bool UseDefaultCredentials
        {
            get
            {
                return (this.transport.Credentials is SystemNetworkCredential);
            }
            set
            {
                if (this.InCall)
                {
                    throw new InvalidOperationException(SR.GetString("SmtpInvalidOperationDuringSend"));
                }
                this.transport.Credentials = value ? CredentialCache.DefaultNetworkCredentials : null;
            }
        }
    }
}

