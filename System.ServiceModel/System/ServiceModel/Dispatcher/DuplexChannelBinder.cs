namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    internal class DuplexChannelBinder : IChannelBinder
    {
        private IDuplexChannel channel;
        private System.ServiceModel.Dispatcher.ChannelHandler channelHandler;
        private IRequestReplyCorrelator correlator;
        private TimeSpan defaultCloseTimeout;
        private TimeSpan defaultSendTimeout;
        private System.ServiceModel.Security.IdentityVerifier identityVerifier;
        private bool isSession;
        private Uri listenUri;
        private int pending;
        private List<IDuplexRequest> requests;
        private bool syncPumpEnabled;

        internal DuplexChannelBinder(IDuplexChannel channel, IRequestReplyCorrelator correlator)
        {
            this.channel = channel;
            this.correlator = correlator;
            this.channel.Faulted += new EventHandler(this.OnFaulted);
        }

        internal DuplexChannelBinder(IDuplexChannel channel, IRequestReplyCorrelator correlator, Uri listenUri) : this(channel, correlator)
        {
            this.listenUri = listenUri;
        }

        internal DuplexChannelBinder(IDuplexSessionChannel channel, IRequestReplyCorrelator correlator, bool useActiveAutoClose) : this(useActiveAutoClose ? ((IDuplexSessionChannel) new AutoCloseDuplexSessionChannel(channel)) : channel, correlator, (Uri) null)
        {
        }

        internal DuplexChannelBinder(IDuplexSessionChannel channel, IRequestReplyCorrelator correlator, Uri listenUri) : this((IDuplexChannel) channel, correlator, listenUri)
        {
            this.isSession = true;
        }

        public void Abort()
        {
            this.channel.Abort();
            this.AbortRequests();
        }

        private void AbortRequests()
        {
            lock (this.ThisLock)
            {
                if (this.requests != null)
                {
                    foreach (IDuplexRequest request in this.requests.ToArray())
                    {
                        request.Abort();
                    }
                }
                this.requests = null;
            }
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            bool flag = false;
            AsyncDuplexRequest request = null;
            try
            {
                RequestReplyCorrelator.PrepareRequest(message);
                request = new AsyncDuplexRequest(message, this, timeout, callback, state);
                lock (this.ThisLock)
                {
                    this.RequestStarting(message, request);
                }
                IAsyncResult sendResult = this.channel.BeginSend(message, timeout, Fx.ThunkCallback(new AsyncCallback(this.SendCallback)), request);
                if (sendResult.CompletedSynchronously)
                {
                    request.FinishedSend(sendResult, true);
                }
                this.EnsurePumping();
                flag = true;
                result2 = request;
            }
            finally
            {
                lock (this.ThisLock)
                {
                    if (flag)
                    {
                        request.EnableCompletion();
                    }
                    else
                    {
                        this.RequestCompleting(request);
                    }
                }
            }
            return result2;
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginSend(message, timeout, callback, state);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.channel.State == CommunicationState.Faulted)
            {
                return new ChannelFaultedAsyncResult(callback, state);
            }
            return this.channel.BeginTryReceive(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginWaitForMessage(timeout, callback, state);
        }

        public void CloseAfterFault(TimeSpan timeout)
        {
            this.channel.Close(timeout);
            this.AbortRequests();
        }

        public Message EndRequest(IAsyncResult result)
        {
            AsyncDuplexRequest request = result as AsyncDuplexRequest;
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidAsyncResult")));
            }
            return request.End();
        }

        public void EndSend(IAsyncResult result)
        {
            this.channel.EndSend(result);
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            Message message;
            if (result is ChannelFaultedAsyncResult)
            {
                this.AbortRequests();
                requestContext = null;
                return true;
            }
            if (this.channel.EndTryReceive(result, out message))
            {
                if (message != null)
                {
                    requestContext = new DuplexRequestContext(this.channel, message, this);
                }
                else
                {
                    this.AbortRequests();
                    requestContext = null;
                }
                return true;
            }
            requestContext = null;
            return false;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.channel.EndWaitForMessage(result);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnsureIncomingIdentity(SecurityMessageProperty property, EndpointAddress address, Message reply)
        {
            this.IdentityVerifier.EnsureIncomingIdentity(address, property.ServiceSecurityContext.AuthorizationContext);
        }

        public void EnsurePumping()
        {
            lock (this.ThisLock)
            {
                if (!this.syncPumpEnabled && !this.ChannelHandler.HasRegisterBeenCalled)
                {
                    System.ServiceModel.Dispatcher.ChannelHandler.Register(this.ChannelHandler);
                }
            }
        }

        private TimeoutException GetReceiveTimeoutException(TimeSpan timeout)
        {
            EndpointAddress address = this.channel.RemoteAddress ?? this.channel.LocalAddress;
            if (address != null)
            {
                return new TimeoutException(System.ServiceModel.SR.GetString("SFxRequestTimedOut2", new object[] { address, timeout }));
            }
            return new TimeoutException(System.ServiceModel.SR.GetString("SFxRequestTimedOut1", new object[] { timeout }));
        }

        internal bool HandleRequestAsReply(Message message)
        {
            UniqueId relatesTo = null;
            try
            {
                relatesTo = message.Headers.RelatesTo;
            }
            catch (MessageHeaderException)
            {
            }
            if (relatesTo == null)
            {
                return false;
            }
            return this.HandleRequestAsReplyCore(message);
        }

        private bool HandleRequestAsReplyCore(Message message)
        {
            IDuplexRequest request = this.correlator.Find<IDuplexRequest>(message, true);
            if (request != null)
            {
                request.GotReply(message);
                return true;
            }
            return false;
        }

        private void OnFaulted(object sender, EventArgs e)
        {
            this.AbortRequests();
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            SyncDuplexRequest request = null;
            bool flag = false;
            RequestReplyCorrelator.PrepareRequest(message);
            lock (this.ThisLock)
            {
                if (!this.Pumping)
                {
                    flag = true;
                    this.syncPumpEnabled = true;
                }
                if (!flag)
                {
                    request = new SyncDuplexRequest(this);
                }
                this.RequestStarting(message, request);
            }
            if (flag)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                UniqueId messageId = message.Headers.MessageId;
                try
                {
                    this.channel.Send(message, helper.RemainingTime());
                    if ((DiagnosticUtility.ShouldUseActivity && (ServiceModelActivity.Current != null)) && (ServiceModelActivity.Current.ActivityType == ActivityType.ProcessAction))
                    {
                        ServiceModelActivity.Current.Suspend();
                    }
                    while (true)
                    {
                        Message message2;
                        TimeSpan span = helper.RemainingTime();
                        if (!this.channel.TryReceive(span, out message2))
                        {
                            throw TraceUtility.ThrowHelperError(this.GetReceiveTimeoutException(timeout), message);
                        }
                        if (message2 == null)
                        {
                            this.AbortRequests();
                            return null;
                        }
                        if (message2.Headers.RelatesTo == messageId)
                        {
                            this.ThrowIfInvalidReplyIdentity(message2);
                            return message2;
                        }
                        if (!this.HandleRequestAsReply(message2))
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                EndpointDispatcher endpointDispatcher = null;
                                if ((this.ChannelHandler != null) && (this.ChannelHandler.Channel != null))
                                {
                                    endpointDispatcher = this.ChannelHandler.Channel.EndpointDispatcher;
                                }
                                TraceUtility.TraceDroppedMessage(message2, endpointDispatcher);
                            }
                            message2.Close();
                        }
                    }
                }
                finally
                {
                    lock (this.ThisLock)
                    {
                        this.RequestCompleting(null);
                        this.syncPumpEnabled = false;
                        if (this.pending > 0)
                        {
                            this.EnsurePumping();
                        }
                    }
                }
            }
            TimeoutHelper helper2 = new TimeoutHelper(timeout);
            this.channel.Send(message, helper2.RemainingTime());
            this.EnsurePumping();
            return request.WaitForReply(helper2.RemainingTime());
        }

        private void RequestCompleting(IDuplexRequest request)
        {
            this.pending--;
            if (this.pending == 0)
            {
                this.requests = null;
            }
            else if ((request != null) && (this.requests != null))
            {
                this.requests.Remove(request);
            }
        }

        private void RequestStarting(Message message, IDuplexRequest request)
        {
            if (request != null)
            {
                this.Requests.Add(request);
                this.correlator.Add<IDuplexRequest>(message, request);
            }
            this.pending++;
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.channel.Send(message, timeout);
        }

        private void SendCallback(IAsyncResult result)
        {
            AsyncDuplexRequest asyncState = result.AsyncState as AsyncDuplexRequest;
            if (!result.CompletedSynchronously)
            {
                asyncState.FinishedSend(result, false);
            }
        }

        private void ThrowIfInvalidReplyIdentity(Message reply)
        {
            if (!this.isSession)
            {
                SecurityMessageProperty security = reply.Properties.Security;
                EndpointAddress remoteAddress = this.channel.RemoteAddress;
                if ((security != null) && (remoteAddress != null))
                {
                    this.EnsureIncomingIdentity(security, remoteAddress, reply);
                }
            }
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            Message message;
            if (this.channel.State == CommunicationState.Faulted)
            {
                this.AbortRequests();
                requestContext = null;
                return true;
            }
            if (this.channel.TryReceive(timeout, out message))
            {
                if (message != null)
                {
                    requestContext = new DuplexRequestContext(this.channel, message, this);
                }
                else
                {
                    this.AbortRequests();
                    requestContext = null;
                }
                return true;
            }
            requestContext = null;
            return false;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.channel.WaitForMessage(timeout);
        }

        public IChannel Channel
        {
            get
            {
                return this.channel;
            }
        }

        internal System.ServiceModel.Dispatcher.ChannelHandler ChannelHandler
        {
            get
            {
                System.ServiceModel.Dispatcher.ChannelHandler channelHandler = this.channelHandler;
                return this.channelHandler;
            }
            set
            {
                System.ServiceModel.Dispatcher.ChannelHandler channelHandler = this.channelHandler;
                this.channelHandler = value;
            }
        }

        public TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.defaultCloseTimeout;
            }
            set
            {
                this.defaultCloseTimeout = value;
            }
        }

        public TimeSpan DefaultSendTimeout
        {
            get
            {
                return this.defaultSendTimeout;
            }
            set
            {
                this.defaultSendTimeout = value;
            }
        }

        public bool HasSession
        {
            get
            {
                return this.isSession;
            }
        }

        internal System.ServiceModel.Security.IdentityVerifier IdentityVerifier
        {
            get
            {
                if (this.identityVerifier == null)
                {
                    this.identityVerifier = System.ServiceModel.Security.IdentityVerifier.CreateDefault();
                }
                return this.identityVerifier;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.identityVerifier = value;
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.channel.LocalAddress;
            }
        }

        private bool Pumping
        {
            get
            {
                return (this.syncPumpEnabled || ((this.ChannelHandler != null) && this.ChannelHandler.HasRegisterBeenCalled));
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.channel.RemoteAddress;
            }
        }

        private List<IDuplexRequest> Requests
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.requests == null)
                    {
                        this.requests = new List<IDuplexRequest>();
                    }
                    return this.requests;
                }
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class AsyncDuplexRequest : AsyncResult, DuplexChannelBinder.IDuplexRequest
        {
            private bool aborted;
            private ServiceModelActivity activity;
            private bool enableComplete;
            private bool gotReply;
            private DuplexChannelBinder parent;
            private Message reply;
            private Exception sendException;
            private IAsyncResult sendResult;
            private bool timedOut;
            private TimeSpan timeout;
            private IOThreadTimer timer;
            private static Action<object> timerCallback = new Action<object>(DuplexChannelBinder.AsyncDuplexRequest.TimerCallback);

            internal AsyncDuplexRequest(Message message, DuplexChannelBinder parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                this.timeout = timeout;
                if (timeout != TimeSpan.MaxValue)
                {
                    this.timer = new IOThreadTimer(timerCallback, this, true);
                    this.timer.Set(timeout);
                }
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    this.activity = TraceUtility.ExtractActivity(message);
                }
            }

            public void Abort()
            {
                bool flag;
                lock (this.parent.ThisLock)
                {
                    bool isDone = this.IsDone;
                    this.aborted = true;
                    flag = !isDone && this.IsDone;
                }
                if (flag)
                {
                    this.Done(false);
                }
            }

            private void Done(bool completedSynchronously)
            {
                ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? TraceUtility.ExtractActivity(this.reply) : null;
                using (ServiceModelActivity.BoundOperation(activity))
                {
                    if (this.timer != null)
                    {
                        this.timer.Cancel();
                        this.timer = null;
                    }
                    lock (this.parent.ThisLock)
                    {
                        this.parent.RequestCompleting(this);
                    }
                    if (this.sendException != null)
                    {
                        base.Complete(completedSynchronously, this.sendException);
                    }
                    else if (this.timedOut)
                    {
                        base.Complete(completedSynchronously, this.parent.GetReceiveTimeoutException(this.timeout));
                    }
                    else
                    {
                        base.Complete(completedSynchronously);
                    }
                }
            }

            public void EnableCompletion()
            {
                bool flag;
                lock (this.parent.ThisLock)
                {
                    bool isDone = this.IsDone;
                    this.enableComplete = true;
                    flag = !isDone && this.IsDone;
                }
                if (flag)
                {
                    this.Done(true);
                }
            }

            internal Message End()
            {
                AsyncResult.End<DuplexChannelBinder.AsyncDuplexRequest>(this);
                this.parent.ThrowIfInvalidReplyIdentity(this.reply);
                return this.reply;
            }

            public void FinishedSend(IAsyncResult sendResult, bool completedSynchronously)
            {
                Exception exception = null;
                bool flag;
                try
                {
                    this.parent.channel.EndSend(sendResult);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                lock (this.parent.ThisLock)
                {
                    bool isDone = this.IsDone;
                    this.sendResult = sendResult;
                    this.sendException = exception;
                    flag = !isDone && this.IsDone;
                }
                if (flag)
                {
                    this.Done(completedSynchronously);
                }
            }

            public void GotReply(Message reply)
            {
                bool flag;
                ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? TraceUtility.ExtractActivity(reply) : null;
                using (ServiceModelActivity.BoundOperation(activity))
                {
                    lock (this.parent.ThisLock)
                    {
                        bool isDone = this.IsDone;
                        this.reply = reply;
                        this.gotReply = true;
                        flag = !isDone && this.IsDone;
                    }
                    if ((activity != null) && DiagnosticUtility.ShouldUseActivity)
                    {
                        TraceUtility.SetActivity(reply, this.activity);
                        if ((DiagnosticUtility.ShouldUseActivity && (this.activity != null)) && (FxTrace.Trace != null))
                        {
                            FxTrace.Trace.TraceTransfer(this.activity.Id);
                        }
                    }
                }
                if (DiagnosticUtility.ShouldUseActivity && (activity != null))
                {
                    activity.Stop();
                }
                if (flag)
                {
                    this.Done(false);
                }
            }

            private void TimedOut()
            {
                bool flag;
                lock (this.parent.ThisLock)
                {
                    bool isDone = this.IsDone;
                    this.timedOut = true;
                    flag = !isDone && this.IsDone;
                }
                if (flag)
                {
                    this.Done(false);
                }
            }

            private static void TimerCallback(object state)
            {
                ((DuplexChannelBinder.AsyncDuplexRequest) state).TimedOut();
            }

            private bool IsDone
            {
                get
                {
                    if (!this.enableComplete)
                    {
                        return false;
                    }
                    if (((this.sendResult == null) || !this.gotReply) && ((this.sendException == null) && !this.timedOut))
                    {
                        return this.aborted;
                    }
                    return true;
                }
            }
        }

        private class AutoCloseDuplexSessionChannel : IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
        {
            private static AsyncCallback closeInnerChannelCallback;
            private CloseState closeState;
            private IDuplexSessionChannel innerChannel;
            private Action messageDequeuedCallback;
            private InputQueue<Message> pendingMessages;
            private static AsyncCallback receiveAsyncCallback;
            private static Action<object> receiveThreadSchedulerCallback;

            public event EventHandler Closed
            {
                add
                {
                    this.innerChannel.Closed += value;
                }
                remove
                {
                    this.innerChannel.Closed -= value;
                }
            }

            public event EventHandler Closing
            {
                add
                {
                    this.innerChannel.Closing += value;
                }
                remove
                {
                    this.innerChannel.Closing -= value;
                }
            }

            public event EventHandler Faulted
            {
                add
                {
                    this.innerChannel.Faulted += value;
                }
                remove
                {
                    this.innerChannel.Faulted -= value;
                }
            }

            public event EventHandler Opened
            {
                add
                {
                    this.innerChannel.Opened += value;
                }
                remove
                {
                    this.innerChannel.Opened -= value;
                }
            }

            public event EventHandler Opening
            {
                add
                {
                    this.innerChannel.Opening += value;
                }
                remove
                {
                    this.innerChannel.Opening -= value;
                }
            }

            public AutoCloseDuplexSessionChannel(IDuplexSessionChannel innerChannel)
            {
                this.innerChannel = innerChannel;
                this.pendingMessages = new InputQueue<Message>();
                this.messageDequeuedCallback = new Action(this.StartBackgroundReceive);
                this.closeState = new CloseState();
            }

            public void Abort()
            {
                this.innerChannel.Abort();
                this.Cleanup();
            }

            public IAsyncResult BeginClose(AsyncCallback callback, object state)
            {
                return this.BeginClose(this.DefaultCloseTimeout, callback, state);
            }

            public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                bool flag;
                lock (this.ThisLock)
                {
                    flag = this.closeState.TryUserClose();
                }
                if (flag)
                {
                    return this.innerChannel.BeginClose(timeout, callback, state);
                }
                return this.closeState.BeginWaitForBackgroundClose(timeout, callback, state);
            }

            public IAsyncResult BeginOpen(AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginOpen(callback, state);
            }

            public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginOpen(timeout, callback, state);
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.pendingMessages.BeginDequeue(timeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginSend(message, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginSend(message, timeout, callback, state);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.pendingMessages.BeginDequeue(timeout, callback, state);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.pendingMessages.BeginWaitForItem(timeout, callback, state);
            }

            private void Cleanup()
            {
                this.pendingMessages.Dispose();
            }

            public void Close()
            {
                this.Close(this.DefaultCloseTimeout);
            }

            public void Close(TimeSpan timeout)
            {
                bool flag;
                lock (this.ThisLock)
                {
                    flag = this.closeState.TryUserClose();
                }
                if (flag)
                {
                    this.innerChannel.Close(timeout);
                }
                else
                {
                    this.closeState.WaitForBackgroundClose(timeout);
                }
                this.Cleanup();
            }

            private void CloseInnerChannel()
            {
                lock (this.ThisLock)
                {
                    if (!this.closeState.TryBackgroundClose() || (this.State != CommunicationState.Opened))
                    {
                        return;
                    }
                }
                IAsyncResult result = null;
                Exception exception = null;
                try
                {
                    if (closeInnerChannelCallback == null)
                    {
                        closeInnerChannelCallback = Fx.ThunkCallback(new AsyncCallback(DuplexChannelBinder.AutoCloseDuplexSessionChannel.CloseInnerChannelCallback));
                    }
                    result = this.innerChannel.BeginClose(closeInnerChannelCallback, this);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    this.innerChannel.Abort();
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.closeState.CaptureBackgroundException(exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.OnCloseInnerChannel(result);
                }
            }

            private static void CloseInnerChannelCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((DuplexChannelBinder.AutoCloseDuplexSessionChannel) result.AsyncState).OnCloseInnerChannel(result);
                }
            }

            public void EndClose(IAsyncResult result)
            {
                if (this.closeState.TryUserClose())
                {
                    this.innerChannel.EndClose(result);
                }
                else
                {
                    this.closeState.EndWaitForBackgroundClose(result);
                }
                this.Cleanup();
            }

            public void EndOpen(IAsyncResult result)
            {
                this.innerChannel.EndOpen(result);
                this.StartBackgroundReceive();
            }

            public Message EndReceive(IAsyncResult result)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public void EndSend(IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return this.pendingMessages.EndDequeue(result, out message);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.pendingMessages.EndWaitForItem(result);
            }

            public T GetProperty<T>() where T: class
            {
                return this.innerChannel.GetProperty<T>();
            }

            private void OnCloseInnerChannel(IAsyncResult result)
            {
                Exception exception = null;
                try
                {
                    this.innerChannel.EndClose(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    this.innerChannel.Abort();
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.closeState.CaptureBackgroundException(exception);
                }
                else
                {
                    this.closeState.FinishBackgroundClose();
                }
            }

            private void OnReceive(IAsyncResult result)
            {
                Message item = null;
                Exception exception = null;
                try
                {
                    item = this.innerChannel.EndReceive(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.pendingMessages.EnqueueAndDispatch(exception, this.messageDequeuedCallback, true);
                }
                else if (item == null)
                {
                    this.pendingMessages.Shutdown();
                    this.CloseInnerChannel();
                }
                else
                {
                    this.pendingMessages.EnqueueAndDispatch(item, this.messageDequeuedCallback, true);
                }
            }

            public void Open()
            {
                this.innerChannel.Open();
                this.StartBackgroundReceive();
            }

            public void Open(TimeSpan timeout)
            {
                this.innerChannel.Open(timeout);
                this.StartBackgroundReceive();
            }

            public Message Receive()
            {
                return this.Receive(this.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                return this.pendingMessages.Dequeue(timeout);
            }

            private static void ReceiveAsyncCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((DuplexChannelBinder.AutoCloseDuplexSessionChannel) result.AsyncState).OnReceive(result);
                }
            }

            private static void ReceiveThreadSchedulerCallback(object state)
            {
                IAsyncResult result = (IAsyncResult) state;
                ((DuplexChannelBinder.AutoCloseDuplexSessionChannel) result.AsyncState).OnReceive(result);
            }

            public void Send(Message message)
            {
                this.Send(message);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                this.Send(message, timeout);
            }

            private void StartBackgroundReceive()
            {
                if (receiveAsyncCallback == null)
                {
                    receiveAsyncCallback = Fx.ThunkCallback(new AsyncCallback(DuplexChannelBinder.AutoCloseDuplexSessionChannel.ReceiveAsyncCallback));
                }
                IAsyncResult state = null;
                Exception exception = null;
                try
                {
                    state = this.innerChannel.BeginReceive(TimeSpan.MaxValue, receiveAsyncCallback, this);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.pendingMessages.EnqueueAndDispatch(exception, this.messageDequeuedCallback, false);
                }
                else if (state.CompletedSynchronously)
                {
                    if (receiveThreadSchedulerCallback == null)
                    {
                        receiveThreadSchedulerCallback = new Action<object>(DuplexChannelBinder.AutoCloseDuplexSessionChannel.ReceiveThreadSchedulerCallback);
                    }
                    IOThreadScheduler.ScheduleCallbackLowPriNoFlow(receiveThreadSchedulerCallback, state);
                }
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                return this.pendingMessages.Dequeue(timeout, out message);
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.pendingMessages.WaitForItem(timeout);
            }

            private TimeSpan DefaultCloseTimeout
            {
                get
                {
                    IDefaultCommunicationTimeouts innerChannel = this.innerChannel as IDefaultCommunicationTimeouts;
                    if (innerChannel != null)
                    {
                        return innerChannel.CloseTimeout;
                    }
                    return ServiceDefaults.CloseTimeout;
                }
            }

            private TimeSpan DefaultReceiveTimeout
            {
                get
                {
                    IDefaultCommunicationTimeouts innerChannel = this.innerChannel as IDefaultCommunicationTimeouts;
                    if (innerChannel != null)
                    {
                        return innerChannel.ReceiveTimeout;
                    }
                    return ServiceDefaults.ReceiveTimeout;
                }
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return this.innerChannel.LocalAddress;
                }
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return this.innerChannel.RemoteAddress;
                }
            }

            public IDuplexSession Session
            {
                get
                {
                    return this.innerChannel.Session;
                }
            }

            public CommunicationState State
            {
                get
                {
                    return this.innerChannel.State;
                }
            }

            private object ThisLock
            {
                get
                {
                    return this;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.innerChannel.Via;
                }
            }

            private class CloseState
            {
                private InputQueue<object> backgroundCloseData;
                private bool userClose;

                public IAsyncResult BeginWaitForBackgroundClose(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return this.backgroundCloseData.BeginDequeue(timeout, callback, state);
                }

                public void CaptureBackgroundException(Exception exception)
                {
                    this.backgroundCloseData.EnqueueAndDispatch(exception, null, true);
                }

                public void EndWaitForBackgroundClose(IAsyncResult result)
                {
                    this.backgroundCloseData.EndDequeue(result);
                }

                public void FinishBackgroundClose()
                {
                    this.backgroundCloseData.Close();
                }

                public bool TryBackgroundClose()
                {
                    if (!this.userClose)
                    {
                        this.backgroundCloseData = new InputQueue<object>();
                        return true;
                    }
                    return false;
                }

                public bool TryUserClose()
                {
                    if (this.backgroundCloseData == null)
                    {
                        this.userClose = true;
                        return true;
                    }
                    return false;
                }

                public void WaitForBackgroundClose(TimeSpan timeout)
                {
                    this.backgroundCloseData.Dequeue(timeout);
                }
            }
        }

        private class ChannelFaultedAsyncResult : CompletedAsyncResult
        {
            public ChannelFaultedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }
        }

        private class DuplexRequestContext : RequestContextBase
        {
            private DuplexChannelBinder binder;
            private IDuplexChannel channel;

            internal DuplexRequestContext(IDuplexChannel channel, Message request, DuplexChannelBinder binder) : base(request, binder.DefaultCloseTimeout, binder.DefaultSendTimeout)
            {
                this.channel = channel;
                this.binder = binder;
            }

            protected override void OnAbort()
            {
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReplyAsyncResult(this, message, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                ReplyAsyncResult.End(result);
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                if (message != null)
                {
                    this.channel.Send(message, timeout);
                }
            }

            private class ReplyAsyncResult : AsyncResult
            {
                private DuplexChannelBinder.DuplexRequestContext context;
                private static AsyncCallback onSend;

                public ReplyAsyncResult(DuplexChannelBinder.DuplexRequestContext context, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    if (message != null)
                    {
                        if (onSend == null)
                        {
                            onSend = Fx.ThunkCallback(new AsyncCallback(DuplexChannelBinder.DuplexRequestContext.ReplyAsyncResult.OnSend));
                        }
                        this.context = context;
                        IAsyncResult result = context.channel.BeginSend(message, timeout, onSend, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        context.channel.EndSend(result);
                    }
                    base.Complete(true);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<DuplexChannelBinder.DuplexRequestContext.ReplyAsyncResult>(result);
                }

                private static void OnSend(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        DuplexChannelBinder.DuplexRequestContext.ReplyAsyncResult asyncState = (DuplexChannelBinder.DuplexRequestContext.ReplyAsyncResult) result.AsyncState;
                        try
                        {
                            asyncState.context.channel.EndSend(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }

        private interface IDuplexRequest
        {
            void Abort();
            void GotReply(Message reply);
        }

        private class SyncDuplexRequest : DuplexChannelBinder.IDuplexRequest
        {
            private DuplexChannelBinder parent;
            private Message reply;
            private ManualResetEvent wait = new ManualResetEvent(false);
            private int waitCount;

            internal SyncDuplexRequest(DuplexChannelBinder parent)
            {
                this.parent = parent;
            }

            public void Abort()
            {
                this.wait.Set();
            }

            private void CloseWaitHandle()
            {
                if (Interlocked.Increment(ref this.waitCount) == 2)
                {
                    this.wait.Close();
                }
            }

            public void GotReply(Message reply)
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.RequestCompleting(this);
                }
                this.reply = reply;
                this.wait.Set();
                this.CloseWaitHandle();
            }

            internal Message WaitForReply(TimeSpan timeout)
            {
                try
                {
                    if (!TimeoutHelper.WaitOne(this.wait, timeout))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.GetReceiveTimeoutException(timeout));
                    }
                }
                finally
                {
                    this.CloseWaitHandle();
                }
                this.parent.ThrowIfInvalidReplyIdentity(this.reply);
                return this.reply;
            }
        }
    }
}

