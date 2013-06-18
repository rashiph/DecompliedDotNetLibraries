namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;

    internal abstract class FramingDuplexSessionChannel : TransportOutputChannel, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
    {
        private System.ServiceModel.Channels.BufferManager bufferManager;
        private System.Security.Authentication.ExtendedProtection.ChannelBinding channelBindingToken;
        private IConnection connection;
        private ConnectionDuplexSession duplexSession;
        private bool exposeConnectionProperty;
        private bool isInputSessionClosed;
        private bool isOutputSessionClosed;
        private EndpointAddress localAddress;
        private Uri localVia;
        private System.ServiceModel.Channels.MessageEncoder messageEncoder;
        private SynchronizedMessageSource messageSource;
        private SecurityMessageProperty remoteSecurity;
        private ThreadNeutralSemaphore sendLock;

        protected FramingDuplexSessionChannel(ConnectionOrientedTransportChannelListener channelListener, EndpointAddress localAddress, Uri localVia, bool exposeConnectionProperty) : this(channelListener, channelListener, localAddress, localVia, EndpointAddress.AnonymousAddress, channelListener.MessageVersion.Addressing.AnonymousUri, exposeConnectionProperty)
        {
            this.duplexSession = ConnectionDuplexSession.CreateSession(this, channelListener.Upgrade);
        }

        protected FramingDuplexSessionChannel(ChannelManagerBase factory, IConnectionOrientedTransportFactorySettings settings, EndpointAddress remoteAddresss, Uri via, bool exposeConnectionProperty) : this(factory, settings, EndpointAddress.AnonymousAddress, settings.MessageVersion.Addressing.AnonymousUri, remoteAddresss, via, exposeConnectionProperty)
        {
            this.duplexSession = ConnectionDuplexSession.CreateSession(this, settings.Upgrade);
        }

        private FramingDuplexSessionChannel(ChannelManagerBase manager, IConnectionOrientedTransportFactorySettings settings, EndpointAddress localAddress, Uri localVia, EndpointAddress remoteAddresss, Uri via, bool exposeConnectionProperty) : base(manager, remoteAddresss, via, settings.ManualAddressing, settings.MessageVersion)
        {
            this.localAddress = localAddress;
            this.localVia = localVia;
            this.exposeConnectionProperty = exposeConnectionProperty;
            this.bufferManager = settings.BufferManager;
            this.sendLock = new ThreadNeutralSemaphore(1);
        }

        protected void ApplyChannelBinding(Message message)
        {
            ChannelBindingUtility.TryAddToMessage(this.channelBindingToken, message, false);
        }

        private IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseOutputSessionAsyncResult(this, timeout, callback, state);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (base.DoneReceivingInCurrentState())
            {
                return new DoneReceivingAsyncResult(callback, state);
            }
            bool flag = true;
            try
            {
                IAsyncResult result = this.messageSource.BeginReceive(timeout, callback, state);
                flag = false;
                result2 = result;
            }
            finally
            {
                if (flag)
                {
                    base.Fault();
                }
            }
            return result2;
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TryReceiveAsyncResult(this, timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (base.DoneReceivingInCurrentState())
            {
                return new DoneReceivingAsyncResult(callback, state);
            }
            bool flag = true;
            try
            {
                IAsyncResult result = this.messageSource.BeginWaitForMessage(timeout, callback, state);
                flag = false;
                result2 = result;
            }
            finally
            {
                if (flag)
                {
                    base.Fault();
                }
            }
            return result2;
        }

        private void CloseOutputSession(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            base.ThrowIfFaulted();
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.sendLock.TryEnter(timeoutHelper.RemainingTime()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("CloseTimedOut", new object[] { timeout }), ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
            }
            try
            {
                base.ThrowIfFaulted();
                if (!this.isOutputSessionClosed)
                {
                    this.isOutputSessionClosed = true;
                    bool flag = true;
                    try
                    {
                        this.Connection.Write(SessionEncoder.EndBytes, 0, SessionEncoder.EndBytes.Length, true, timeoutHelper.RemainingTime());
                        this.OnOutputSessionClosed(ref timeoutHelper);
                        flag = false;
                    }
                    finally
                    {
                        if (flag)
                        {
                            base.Fault();
                        }
                    }
                }
            }
            finally
            {
                this.sendLock.Exit();
            }
        }

        private void CompleteClose(TimeSpan timeout)
        {
            this.ReturnConnectionIfNecessary(false, timeout);
        }

        private ArraySegment<byte> EncodeMessage(Message message)
        {
            return SessionEncoder.EncodeMessageFrame(this.MessageEncoder.WriteMessage(message, 0x7fffffff, this.bufferManager, 6));
        }

        private void EndCloseOutputSession(IAsyncResult result)
        {
            CloseOutputSessionAsyncResult.End(result);
        }

        public Message EndReceive(IAsyncResult result)
        {
            Message message2;
            base.ThrowIfNotOpened();
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            DoneReceivingAsyncResult result2 = result as DoneReceivingAsyncResult;
            if (result2 != null)
            {
                DoneReceivingAsyncResult.End(result2);
                return null;
            }
            bool flag = true;
            Message message = null;
            try
            {
                message = this.messageSource.EndReceive(result);
                this.OnReceiveMessage(message);
                flag = false;
                message2 = message;
            }
            finally
            {
                if (flag)
                {
                    if (message != null)
                    {
                        message.Close();
                        message = null;
                    }
                    base.Fault();
                }
            }
            return message2;
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return TryReceiveAsyncResult.End(result, out message);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            bool flag3;
            base.ThrowIfNotOpened();
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            DoneReceivingAsyncResult result2 = result as DoneReceivingAsyncResult;
            if (result2 != null)
            {
                return DoneReceivingAsyncResult.End(result2);
            }
            bool flag = true;
            try
            {
                bool flag2 = this.messageSource.EndWaitForMessage(result);
                flag = !flag2;
                flag3 = flag2;
            }
            finally
            {
                if (flag)
                {
                    base.Fault();
                }
            }
            return flag3;
        }

        protected override void OnAbort()
        {
            this.ReturnConnectionIfNecessary(true, TimeSpan.Zero);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();
            return new SendAsyncResult(this, message, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CloseOutputSession(helper.RemainingTime());
            if (!this.isInputSessionClosed)
            {
                Message message = this.messageSource.Receive(helper.RemainingTime());
                if (message != null)
                {
                    using (message)
                    {
                        throw TraceUtility.ThrowHelperError(ProtocolException.ReceiveShutdownReturnedNonNull(message), message);
                    }
                }
                this.OnInputSessionClosed();
            }
            this.CompleteClose(helper.RemainingTime());
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.channelBindingToken != null)
            {
                this.channelBindingToken.Close();
                this.channelBindingToken = null;
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            if (TD.MessageSentByTransportIsEnabled())
            {
                TD.MessageSentByTransport(this.RemoteAddress.Uri.AbsoluteUri);
            }
            SendAsyncResult.End(result);
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            this.ReturnConnectionIfNecessary(true, TimeSpan.Zero);
        }

        private void OnInputSessionClosed()
        {
            lock (base.ThisLock)
            {
                if (!this.isInputSessionClosed)
                {
                    this.isInputSessionClosed = true;
                }
            }
        }

        private void OnOutputSessionClosed(ref TimeoutHelper timeoutHelper)
        {
            bool flag = false;
            lock (base.ThisLock)
            {
                if (this.isInputSessionClosed)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                this.ReturnConnectionIfNecessary(false, timeoutHelper.RemainingTime());
            }
        }

        private void OnReceiveMessage(Message message)
        {
            if (message == null)
            {
                this.OnInputSessionClosed();
            }
            else
            {
                this.PrepareMessage(message);
            }
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            base.ThrowIfDisposedOrNotOpen();
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!this.sendLock.TryEnter(helper.RemainingTime()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("SendToViaTimedOut", new object[] { this.Via, timeout }), ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
            }
            try
            {
                base.ThrowIfDisposedOrNotOpen();
                this.ThrowIfOutputSessionClosed();
                bool flag = false;
                try
                {
                    this.ApplyChannelBinding(message);
                    bool allowOutputBatching = message.Properties.AllowOutputBatching;
                    ArraySegment<byte> segment = this.EncodeMessage(message);
                    this.Connection.Write(segment.Array, segment.Offset, segment.Count, !allowOutputBatching, helper.RemainingTime(), this.bufferManager);
                    flag = true;
                    if (TD.MessageSentByTransportIsEnabled())
                    {
                        TD.MessageSentByTransport(this.RemoteAddress.Uri.AbsoluteUri);
                    }
                }
                finally
                {
                    if (!flag)
                    {
                        base.Fault();
                    }
                }
            }
            finally
            {
                this.sendLock.Exit();
            }
        }

        protected virtual void PrepareMessage(Message message)
        {
            message.Properties.Via = this.localVia;
            if (this.exposeConnectionProperty)
            {
                message.Properties[ConnectionMessageProperty.Name] = this.connection;
            }
            this.ApplyChannelBinding(message);
            if (TD.MessageReceivedByTransportIsEnabled())
            {
                TD.MessageReceivedByTransport(this.LocalAddress.Uri.AbsoluteUri);
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40013, System.ServiceModel.SR.GetString("TraceCodeMessageReceived"), MessageTransmitTraceRecord.CreateReceiveTraceRecord(message, this.LocalAddress), this, null, message);
            }
        }

        public Message Receive()
        {
            return this.Receive(base.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = null;
            Message message2;
            if (base.DoneReceivingInCurrentState())
            {
                return null;
            }
            bool flag = true;
            try
            {
                message = this.messageSource.Receive(timeout);
                this.OnReceiveMessage(message);
                flag = false;
                message2 = message;
            }
            finally
            {
                if (flag)
                {
                    if (message != null)
                    {
                        message.Close();
                        message = null;
                    }
                    base.Fault();
                }
            }
            return message2;
        }

        protected abstract void ReturnConnectionIfNecessary(bool abort, TimeSpan timeout);
        protected void SetChannelBinding(System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding)
        {
            this.channelBindingToken = channelBinding;
        }

        protected void SetMessageSource(IMessageSource messageSource)
        {
            this.messageSource = new SynchronizedMessageSource(messageSource);
        }

        private void ThrowIfOutputSessionClosed()
        {
            if (this.isOutputSessionClosed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SendCannotBeCalledAfterCloseOutputSession")));
            }
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            try
            {
                message = this.Receive(timeout);
                return true;
            }
            catch (TimeoutException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                message = null;
                return false;
            }
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            bool flag3;
            if (base.DoneReceivingInCurrentState())
            {
                return true;
            }
            bool flag = true;
            try
            {
                bool flag2 = this.messageSource.WaitForMessage(timeout);
                flag = !flag2;
                flag3 = flag2;
            }
            finally
            {
                if (flag)
                {
                    base.Fault();
                }
            }
            return flag3;
        }

        protected System.ServiceModel.Channels.BufferManager BufferManager
        {
            get
            {
                return this.bufferManager;
            }
        }

        protected System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                return this.channelBindingToken;
            }
        }

        protected IConnection Connection
        {
            get
            {
                return this.connection;
            }
            set
            {
                this.connection = value;
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.localAddress;
            }
        }

        protected Uri LocalVia
        {
            get
            {
                return this.localVia;
            }
        }

        protected System.ServiceModel.Channels.MessageEncoder MessageEncoder
        {
            get
            {
                return this.messageEncoder;
            }
            set
            {
                this.messageEncoder = value;
            }
        }

        public SecurityMessageProperty RemoteSecurity
        {
            get
            {
                return this.remoteSecurity;
            }
            protected set
            {
                this.remoteSecurity = value;
            }
        }

        public IDuplexSession Session
        {
            get
            {
                return this.duplexSession;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private FramingDuplexSessionChannel channel;
            private static AsyncCallback onCloseInputSession = Fx.ThunkCallback(new AsyncCallback(FramingDuplexSessionChannel.CloseAsyncResult.OnCloseInputSession));
            private static AsyncCallback onCloseOutputSession = Fx.ThunkCallback(new AsyncCallback(FramingDuplexSessionChannel.CloseAsyncResult.OnCloseOutputSession));
            private static Action<object> onCompleteCloseScheduled;
            private TimeoutHelper timeoutHelper;

            public CloseAsyncResult(FramingDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channel = channel;
                this.timeoutHelper = new TimeoutHelper(timeout);
                IAsyncResult result = channel.BeginCloseOutputSession(this.timeoutHelper.RemainingTime(), onCloseOutputSession, this);
                if (result.CompletedSynchronously && this.HandleCloseOutputSession(result, true))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<FramingDuplexSessionChannel.CloseAsyncResult>(result);
            }

            private bool HandleCloseInputSession(IAsyncResult result, bool isStillSynchronous)
            {
                Message message = this.channel.messageSource.EndReceive(result);
                if (message != null)
                {
                    using (message)
                    {
                        throw TraceUtility.ThrowHelperError(ProtocolException.ReceiveShutdownReturnedNonNull(message), message);
                    }
                }
                this.channel.OnInputSessionClosed();
                return this.ScheduleCompleteClose(isStillSynchronous);
            }

            private bool HandleCloseOutputSession(IAsyncResult result, bool isStillSynchronous)
            {
                this.channel.EndCloseOutputSession(result);
                if (this.channel.isInputSessionClosed)
                {
                    return this.ScheduleCompleteClose(isStillSynchronous);
                }
                IAsyncResult result2 = this.channel.messageSource.BeginReceive(this.timeoutHelper.RemainingTime(), onCloseInputSession, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleCloseInputSession(result2, isStillSynchronous);
            }

            private static void OnCloseInputSession(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    FramingDuplexSessionChannel.CloseAsyncResult asyncState = (FramingDuplexSessionChannel.CloseAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleCloseInputSession(result, false);
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
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnCloseOutputSession(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    FramingDuplexSessionChannel.CloseAsyncResult asyncState = (FramingDuplexSessionChannel.CloseAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleCloseOutputSession(result, false);
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
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private void OnCompleteCloseScheduled()
            {
                this.channel.CompleteClose(this.timeoutHelper.RemainingTime());
            }

            private static void OnCompleteCloseScheduled(object state)
            {
                FramingDuplexSessionChannel.CloseAsyncResult result = (FramingDuplexSessionChannel.CloseAsyncResult) state;
                Exception exception = null;
                try
                {
                    result.OnCompleteCloseScheduled();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                result.Complete(false, exception);
            }

            private bool ScheduleCompleteClose(bool isStillSynchronous)
            {
                if (isStillSynchronous)
                {
                    if (onCompleteCloseScheduled == null)
                    {
                        onCompleteCloseScheduled = new Action<object>(FramingDuplexSessionChannel.CloseAsyncResult.OnCompleteCloseScheduled);
                    }
                    ActionItem.Schedule(onCompleteCloseScheduled, this);
                    return false;
                }
                this.OnCompleteCloseScheduled();
                return true;
            }
        }

        private class CloseOutputSessionAsyncResult : AsyncResult
        {
            private FramingDuplexSessionChannel channel;
            private static FastAsyncCallback onEnterComplete = new FastAsyncCallback(FramingDuplexSessionChannel.CloseOutputSessionAsyncResult.OnEnterComplete);
            private static AsyncCallback onWriteComplete = Fx.ThunkCallback(new AsyncCallback(FramingDuplexSessionChannel.CloseOutputSessionAsyncResult.OnWriteComplete));
            private TimeoutHelper timeoutHelper;

            public CloseOutputSessionAsyncResult(FramingDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                channel.ThrowIfNotOpened();
                channel.ThrowIfFaulted();
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.channel = channel;
                if (channel.sendLock.EnterAsync(this.timeoutHelper.RemainingTime(), onEnterComplete, this))
                {
                    bool flag = false;
                    bool flag2 = false;
                    try
                    {
                        flag = this.WriteEndBytes();
                        flag2 = true;
                    }
                    finally
                    {
                        if (!flag2)
                        {
                            this.Cleanup(false, true);
                        }
                    }
                    if (flag)
                    {
                        this.Cleanup(true, true);
                        base.Complete(true);
                    }
                }
            }

            private void Cleanup(bool success, bool lockTaken)
            {
                try
                {
                    if (!success)
                    {
                        this.channel.Fault();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.channel.sendLock.Exit();
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<FramingDuplexSessionChannel.CloseOutputSessionAsyncResult>(result);
            }

            private void HandleWriteEndBytesComplete(IAsyncResult result)
            {
                this.channel.Connection.EndWrite(result);
                this.channel.OnOutputSessionClosed(ref this.timeoutHelper);
            }

            private static void OnEnterComplete(object state, Exception asyncException)
            {
                FramingDuplexSessionChannel.CloseOutputSessionAsyncResult result = (FramingDuplexSessionChannel.CloseOutputSessionAsyncResult) state;
                bool flag = false;
                Exception exception = asyncException;
                if (exception != null)
                {
                    flag = true;
                }
                else
                {
                    try
                    {
                        flag = result.WriteEndBytes();
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
                }
                if (flag)
                {
                    result.Cleanup(exception == null, asyncException == null);
                    result.Complete(false, exception);
                }
            }

            private static void OnWriteComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    FramingDuplexSessionChannel.CloseOutputSessionAsyncResult asyncState = (FramingDuplexSessionChannel.CloseOutputSessionAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.HandleWriteEndBytesComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Cleanup(exception == null, true);
                    asyncState.Complete(false, exception);
                }
            }

            private bool WriteEndBytes()
            {
                this.channel.ThrowIfFaulted();
                if (!this.channel.isOutputSessionClosed)
                {
                    this.channel.isOutputSessionClosed = true;
                    IAsyncResult result = this.channel.Connection.BeginWrite(SessionEncoder.EndBytes, 0, SessionEncoder.EndBytes.Length, true, this.timeoutHelper.RemainingTime(), onWriteComplete, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.HandleWriteEndBytesComplete(result);
                }
                return true;
            }
        }

        private class ConnectionDuplexSession : IDuplexSession, IInputSession, IOutputSession, ISession
        {
            private FramingDuplexSessionChannel channel;
            private string id;
            private static System.ServiceModel.Channels.UriGenerator uriGenerator;

            private ConnectionDuplexSession(FramingDuplexSessionChannel channel)
            {
                this.channel = channel;
            }

            public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
            {
                return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
            }

            public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.channel.BeginCloseOutputSession(timeout, callback, state);
            }

            public void CloseOutputSession()
            {
                this.CloseOutputSession(this.channel.DefaultCloseTimeout);
            }

            public void CloseOutputSession(TimeSpan timeout)
            {
                this.channel.CloseOutputSession(timeout);
            }

            public static FramingDuplexSessionChannel.ConnectionDuplexSession CreateSession(FramingDuplexSessionChannel channel, StreamUpgradeProvider upgrade)
            {
                if (upgrade is StreamSecurityUpgradeProvider)
                {
                    return new SecureConnectionDuplexSession(channel);
                }
                return new FramingDuplexSessionChannel.ConnectionDuplexSession(channel);
            }

            public void EndCloseOutputSession(IAsyncResult result)
            {
                this.channel.EndCloseOutputSession(result);
            }

            public string Id
            {
                get
                {
                    if (this.id == null)
                    {
                        lock (this.channel.ThisLock)
                        {
                            if (this.id == null)
                            {
                                this.id = UriGenerator.Next();
                            }
                        }
                    }
                    return this.id;
                }
            }

            private static System.ServiceModel.Channels.UriGenerator UriGenerator
            {
                get
                {
                    if (uriGenerator == null)
                    {
                        uriGenerator = new System.ServiceModel.Channels.UriGenerator();
                    }
                    return uriGenerator;
                }
            }

            private class SecureConnectionDuplexSession : FramingDuplexSessionChannel.ConnectionDuplexSession, ISecuritySession, ISession
            {
                private EndpointIdentity remoteIdentity;

                public SecureConnectionDuplexSession(FramingDuplexSessionChannel channel) : base(channel)
                {
                }

                EndpointIdentity ISecuritySession.RemoteIdentity
                {
                    get
                    {
                        if (this.remoteIdentity == null)
                        {
                            SecurityMessageProperty remoteSecurity = base.channel.RemoteSecurity;
                            if (((remoteSecurity != null) && (remoteSecurity.ServiceSecurityContext != null)) && ((remoteSecurity.ServiceSecurityContext.IdentityClaim != null) && (remoteSecurity.ServiceSecurityContext.PrimaryIdentity != null)))
                            {
                                this.remoteIdentity = EndpointIdentity.CreateIdentity(remoteSecurity.ServiceSecurityContext.IdentityClaim);
                            }
                        }
                        return this.remoteIdentity;
                    }
                }
            }
        }

        private class SendAsyncResult : TraceAsyncResult
        {
            private byte[] buffer;
            private FramingDuplexSessionChannel channel;
            private Message message;
            private static FastAsyncCallback onEnterComplete = new FastAsyncCallback(FramingDuplexSessionChannel.SendAsyncResult.OnEnterComplete);
            private static AsyncCallback onWriteComplete = Fx.ThunkCallback(new AsyncCallback(FramingDuplexSessionChannel.SendAsyncResult.OnWriteComplete));
            private TimeoutHelper timeoutHelper;

            public SendAsyncResult(FramingDuplexSessionChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.channel = channel;
                this.message = message;
                if (channel.sendLock.EnterAsync(this.timeoutHelper.RemainingTime(), onEnterComplete, this))
                {
                    bool flag = false;
                    bool flag2 = false;
                    try
                    {
                        flag = this.WriteCore();
                        flag2 = true;
                    }
                    finally
                    {
                        if (!flag2)
                        {
                            this.Cleanup(false, true);
                        }
                    }
                    if (flag)
                    {
                        this.Cleanup(true, true);
                        base.Complete(true);
                    }
                }
            }

            private void Cleanup(bool success, bool lockTaken)
            {
                try
                {
                    if (!success)
                    {
                        this.channel.Fault();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.channel.sendLock.Exit();
                    }
                }
                if (this.buffer != null)
                {
                    this.channel.bufferManager.ReturnBuffer(this.buffer);
                    this.buffer = null;
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<FramingDuplexSessionChannel.SendAsyncResult>(result);
            }

            private static void OnEnterComplete(object state, Exception asyncException)
            {
                FramingDuplexSessionChannel.SendAsyncResult result = (FramingDuplexSessionChannel.SendAsyncResult) state;
                bool flag = false;
                Exception exception = asyncException;
                if (exception != null)
                {
                    flag = true;
                }
                else
                {
                    try
                    {
                        flag = result.WriteCore();
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
                }
                if (flag)
                {
                    result.Cleanup(exception == null, asyncException == null);
                    result.Complete(false, exception);
                }
            }

            private static void OnWriteComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    FramingDuplexSessionChannel.SendAsyncResult asyncState = (FramingDuplexSessionChannel.SendAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.channel.Connection.EndWrite(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Cleanup(exception == null, true);
                    asyncState.Complete(false, exception);
                }
            }

            private bool WriteCore()
            {
                this.channel.ThrowIfDisposedOrNotOpen();
                this.channel.ThrowIfOutputSessionClosed();
                this.channel.ApplyChannelBinding(this.message);
                bool allowOutputBatching = this.message.Properties.AllowOutputBatching;
                ArraySegment<byte> segment = this.channel.EncodeMessage(this.message);
                this.message = null;
                this.buffer = segment.Array;
                IAsyncResult result = this.channel.Connection.BeginWrite(segment.Array, segment.Offset, segment.Count, !allowOutputBatching, this.timeoutHelper.RemainingTime(), onWriteComplete, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                this.channel.Connection.EndWrite(result);
                return true;
            }
        }

        private class TryReceiveAsyncResult : AsyncResult
        {
            private FramingDuplexSessionChannel channel;
            private Message message;
            private static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(FramingDuplexSessionChannel.TryReceiveAsyncResult.OnReceive));
            private bool receiveSuccess;

            public TryReceiveAsyncResult(FramingDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channel = channel;
                bool flag = false;
                try
                {
                    IAsyncResult result = this.channel.BeginReceive(timeout, onReceive, this);
                    if (result.CompletedSynchronously)
                    {
                        this.CompleteReceive(result);
                        flag = true;
                    }
                }
                catch (TimeoutException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    flag = true;
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private void CompleteReceive(IAsyncResult result)
            {
                this.message = this.channel.EndReceive(result);
                this.receiveSuccess = true;
            }

            public static bool End(IAsyncResult result, out Message message)
            {
                FramingDuplexSessionChannel.TryReceiveAsyncResult result2 = AsyncResult.End<FramingDuplexSessionChannel.TryReceiveAsyncResult>(result);
                message = result2.message;
                return result2.receiveSuccess;
            }

            private static void OnReceive(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    FramingDuplexSessionChannel.TryReceiveAsyncResult asyncState = (FramingDuplexSessionChannel.TryReceiveAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.CompleteReceive(result);
                    }
                    catch (TimeoutException exception2)
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
                        exception = exception3;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }
    }
}

