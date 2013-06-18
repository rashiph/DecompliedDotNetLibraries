namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class ReliableDuplexSessionChannel : DuplexChannel, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
    {
        private bool acknowledgementScheduled;
        private IOThreadTimer acknowledgementTimer;
        private ulong ackVersion;
        private bool advertisedZero;
        private static Action<object> asyncReceiveComplete = new Action<object>(ReliableDuplexSessionChannel.AsyncReceiveCompleteStatic);
        private IReliableChannelBinder binder;
        private InterruptibleWaitObject closeOutputWaitObject;
        private SendWaitReliableRequestor closeRequestor;
        private DeliveryStrategy<Message> deliveryStrategy;
        private Guard guard;
        private ReliableInputConnection inputConnection;
        private Exception maxRetryCountException;
        private static AsyncCallback onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(ReliableDuplexSessionChannel.OnReceiveCompletedStatic));
        private ReliableOutputConnection outputConnection;
        private int pendingAcknowledgements;
        private ChannelReliableSession session;
        private IReliableFactorySettings settings;
        private SendWaitReliableRequestor terminateRequestor;

        protected ReliableDuplexSessionChannel(ChannelManagerBase manager, IReliableFactorySettings settings, IReliableChannelBinder binder) : base(manager, binder.LocalAddress)
        {
            this.ackVersion = 1L;
            this.guard = new Guard(0x7fffffff);
            this.binder = binder;
            this.settings = settings;
            this.acknowledgementTimer = new IOThreadTimer(new Action<object>(this.OnAcknowledgementTimeoutElapsed), null, true);
            this.binder.Faulted += new BinderExceptionHandler(this.OnBinderFaulted);
            this.binder.OnException += new BinderExceptionHandler(this.OnBinderException);
        }

        private void AddPendingAcknowledgements(Message message)
        {
            lock (base.ThisLock)
            {
                if (this.pendingAcknowledgements > 0)
                {
                    this.acknowledgementTimer.Cancel();
                    this.acknowledgementScheduled = false;
                    this.pendingAcknowledgements = 0;
                    this.ackVersion += (ulong) 1L;
                    int bufferRemaining = this.GetBufferRemaining();
                    WsrmUtilities.AddAcknowledgementHeader(this.settings.ReliableMessagingVersion, message, this.session.InputID, this.inputConnection.Ranges, this.inputConnection.IsLastKnown, bufferRemaining);
                }
            }
        }

        private static void AsyncReceiveCompleteStatic(object state)
        {
            IAsyncResult result = (IAsyncResult) state;
            ReliableDuplexSessionChannel asyncState = (ReliableDuplexSessionChannel) result.AsyncState;
            try
            {
                if (asyncState.HandleReceiveComplete(result))
                {
                    asyncState.StartReceiving(true);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                asyncState.ReliableSession.OnUnknownException(exception);
            }
        }

        private IAsyncResult BeginCloseBinder(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.binder.BeginClose(timeout, MaskingMode.Handled, callback, state);
        }

        private IAsyncResult BeginCloseSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CreateCloseRequestor();
            return this.closeRequestor.BeginRequest(timeout, callback, state);
        }

        private IAsyncResult BeginInternalCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool flag = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.outputConnection.BeginClose), flag ? new OperationWithTimeoutBeginCallback(this.BeginCloseSequence) : null, new OperationWithTimeoutBeginCallback(this.BeginTerminateSequence) };
            OperationEndCallback[] endOperations = new OperationEndCallback[] { new OperationEndCallback(this.outputConnection.EndClose), flag ? new OperationEndCallback(this.EndCloseSequence) : null, new OperationEndCallback(this.EndTerminateSequence) };
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        private IAsyncResult BeginTerminateSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.outputConnection.CheckForTermination())
                {
                    this.session.CloseSession();
                }
                Message message = WsrmUtilities.CreateTerminateMessage(this.settings.MessageVersion, reliableMessagingVersion, this.session.OutputID);
                return this.binder.BeginSend(message, timeout, MaskingMode.Handled, callback, state);
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            this.CreateTerminateRequestor();
            return this.terminateRequestor.BeginRequest(timeout, callback, state);
        }

        private void CloseSequence(TimeSpan timeout)
        {
            this.CreateCloseRequestor();
            this.closeRequestor.Request(timeout);
        }

        private void ConfigureRequestor(ReliableRequestor requestor)
        {
            requestor.MessageVersion = this.settings.MessageVersion;
            requestor.Binder = this.binder;
            requestor.SetRequestResponsePattern();
        }

        private Message CreateAcknowledgmentMessage()
        {
            lock (base.ThisLock)
            {
                this.ackVersion += (ulong) 1L;
            }
            int bufferRemaining = this.GetBufferRemaining();
            return WsrmUtilities.CreateAcknowledgmentMessage(this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, this.session.InputID, this.inputConnection.Ranges, this.inputConnection.IsLastKnown, bufferRemaining);
        }

        private void CreateCloseRequestor()
        {
            SendWaitReliableRequestor requestor = new SendWaitReliableRequestor();
            this.ConfigureRequestor(requestor);
            requestor.TimeoutString1Index = "TimeoutOnClose";
            requestor.MessageAction = WsrmIndex.GetCloseSequenceActionHeader(this.settings.MessageVersion.Addressing);
            requestor.MessageBody = new System.ServiceModel.Channels.CloseSequence(this.session.OutputID, this.outputConnection.Last);
            lock (base.ThisLock)
            {
                base.ThrowIfClosed();
                this.closeRequestor = requestor;
            }
        }

        private void CreateTerminateRequestor()
        {
            SendWaitReliableRequestor requestor = new SendWaitReliableRequestor();
            this.ConfigureRequestor(requestor);
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            requestor.MessageAction = WsrmIndex.GetTerminateSequenceActionHeader(this.settings.MessageVersion.Addressing, reliableMessagingVersion);
            requestor.MessageBody = new System.ServiceModel.Channels.TerminateSequence(reliableMessagingVersion, this.session.OutputID, this.outputConnection.Last);
            lock (base.ThisLock)
            {
                base.ThrowIfClosed();
                this.terminateRequestor = requestor;
                if (this.inputConnection.IsLastKnown)
                {
                    this.session.CloseSession();
                }
            }
        }

        private void EndCloseBinder(IAsyncResult result)
        {
            this.binder.EndClose(result);
        }

        private void EndCloseSequence(IAsyncResult result)
        {
            this.closeRequestor.EndRequest(result);
        }

        private void EndInternalCloseOutputSession(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        private void EndTerminateSequence(IAsyncResult result)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.binder.EndSend(result);
            }
            else
            {
                if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
                {
                    throw Fx.AssertAndThrow("Reliable messaging version not supported.");
                }
                this.terminateRequestor.EndRequest(result);
            }
        }

        private int GetBufferRemaining()
        {
            int num = -1;
            if (this.settings.FlowControlEnabled)
            {
                num = this.settings.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount;
                this.advertisedZero = num == 0;
            }
            return num;
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IDuplexSessionChannel))
            {
                return (T) this;
            }
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            T local2 = this.binder.Channel.GetProperty<T>();
            if ((local2 == null) && (typeof(T) == typeof(FaultConverter)))
            {
                return (T) FaultConverter.GetDefaultFaultConverter(this.settings.MessageVersion);
            }
            return local2;
        }

        private bool HandleReceiveComplete(IAsyncResult result)
        {
            RequestContext context;
            if (!this.Binder.EndTryReceive(result, out context))
            {
                return true;
            }
            if (context == null)
            {
                bool flag = false;
                lock (base.ThisLock)
                {
                    flag = this.inputConnection.Terminate();
                }
                if (!flag && (this.Binder.State == CommunicationState.Opened))
                {
                    Exception e = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityClose"));
                    this.ReliableSession.OnLocalFault(e, (Message) null, null);
                }
                return false;
            }
            Message requestMessage = context.RequestMessage;
            context.Close();
            WsrmMessageInfo info = WsrmMessageInfo.Get(this.settings.MessageVersion, this.settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), requestMessage);
            this.StartReceiving(false);
            this.ProcessMessage(info);
            return false;
        }

        private void InternalCloseOutputSession(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.outputConnection.Close(helper.RemainingTime());
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                this.CloseSequence(helper.RemainingTime());
            }
            this.TerminateSequence(helper.RemainingTime());
        }

        protected override void OnAbort()
        {
            if (this.outputConnection != null)
            {
                this.outputConnection.Abort(this);
            }
            if (this.inputConnection != null)
            {
                this.inputConnection.Abort(this);
            }
            this.guard.Abort();
            ReliableRequestor closeRequestor = this.closeRequestor;
            if (closeRequestor != null)
            {
                closeRequestor.Abort(this);
            }
            closeRequestor = this.terminateRequestor;
            if (closeRequestor != null)
            {
                closeRequestor.Abort(this);
            }
            this.session.Abort();
        }

        private void OnAcknowledgementTimeoutElapsed(object state)
        {
            lock (base.ThisLock)
            {
                this.acknowledgementScheduled = false;
                this.pendingAcknowledgements = 0;
                if (((base.State == CommunicationState.Closing) || (base.State == CommunicationState.Closed)) || (base.State == CommunicationState.Faulted))
                {
                    return;
                }
            }
            if (this.guard.Enter())
            {
                try
                {
                    using (Message message = this.CreateAcknowledgmentMessage())
                    {
                        this.binder.Send(message, base.DefaultSendTimeout);
                    }
                }
                finally
                {
                    this.guard.Exit();
                }
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OperationWithTimeoutBeginCallback callback2;
            OperationEndCallback callback3;
            OperationWithTimeoutBeginCallback callback4;
            OperationEndCallback callback5;
            this.ThrowIfCloseInvalid();
            if (this.outputConnection == null)
            {
                callback2 = null;
                callback3 = null;
            }
            else if (this.closeOutputWaitObject == null)
            {
                callback2 = new OperationWithTimeoutBeginCallback(this.BeginInternalCloseOutputSession);
                callback3 = new OperationEndCallback(this.EndInternalCloseOutputSession);
            }
            else
            {
                callback2 = new OperationWithTimeoutBeginCallback(this.closeOutputWaitObject.BeginWait);
                callback3 = new OperationEndCallback(this.closeOutputWaitObject.EndWait);
            }
            if (this.inputConnection == null)
            {
                callback4 = null;
                callback5 = null;
            }
            else
            {
                callback4 = new OperationWithTimeoutBeginCallback(this.inputConnection.BeginClose);
                callback5 = new OperationEndCallback(this.inputConnection.EndClose);
            }
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { callback2, callback4, new OperationWithTimeoutBeginCallback(this.guard.BeginClose), new OperationWithTimeoutBeginCallback(this.session.BeginClose), new OperationWithTimeoutBeginCallback(this.BeginCloseBinder), new OperationWithTimeoutBeginCallback(this.OnBeginClose) };
            OperationEndCallback[] endOperations = new OperationEndCallback[] { callback3, callback5, new OperationEndCallback(this.guard.EndClose), new OperationEndCallback(this.session.EndClose), new OperationEndCallback(this.EndCloseBinder), new OperationEndCallback(this.OnEndClose) };
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        protected IAsyncResult OnBeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            bool flag = false;
            lock (base.ThisLock)
            {
                base.ThrowIfNotOpened();
                base.ThrowIfFaulted();
                if ((base.State != CommunicationState.Opened) || (this.closeOutputWaitObject != null))
                {
                    flag = true;
                }
                else
                {
                    this.closeOutputWaitObject = new InterruptibleWaitObject(false, true);
                }
            }
            if (flag)
            {
                return new CompletedAsyncResult(callback, state);
            }
            bool flag2 = true;
            try
            {
                IAsyncResult result = this.BeginInternalCloseOutputSession(timeout, callback, state);
                flag2 = false;
                result2 = result;
            }
            finally
            {
                if (flag2)
                {
                    this.session.OnLocalFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.session.OutputID, System.ServiceModel.SR.GetString("CloseOutputSessionErrorReason"), null), null);
                    this.closeOutputWaitObject.Fault(this);
                }
            }
            return result2;
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.outputConnection.BeginAddMessage(message, timeout, null, callback, state);
        }

        private IAsyncResult OnBeginSendAckRequestedHandler(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.session.OnLocalActivity();
            ReliableBinderSendAsyncResult result = new ReliableBinderSendAsyncResult(callback, state) {
                Binder = this.binder,
                MaskingMode = MaskingMode.Handled,
                Message = WsrmUtilities.CreateAckRequestedMessage(this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, this.ReliableSession.OutputID)
            };
            result.Begin(timeout);
            return result;
        }

        private IAsyncResult OnBeginSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
        {
            if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
            {
                this.session.OnLocalFault(new CommunicationException(System.ServiceModel.SR.GetString("MaximumRetryCountExceeded"), this.maxRetryCountException), SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                return new CompletedAsyncResult(callback, state);
            }
            this.session.OnLocalActivity();
            this.AddPendingAcknowledgements(attemptInfo.Message);
            ReliableBinderSendAsyncResult result = new ReliableBinderSendAsyncResult(callback, state) {
                Binder = this.binder,
                MessageAttemptInfo = attemptInfo,
                MaskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None
            };
            if (attemptInfo.RetryCount < this.settings.MaxRetryCount)
            {
                result.MaskingMode |= MaskingMode.Handled;
                result.SaveHandledException = false;
            }
            else
            {
                result.SaveHandledException = true;
            }
            result.Begin(timeout);
            return result;
        }

        private void OnBinderException(IReliableChannelBinder sender, Exception exception)
        {
            if (exception is QuotaExceededException)
            {
                if (((base.State == CommunicationState.Opening) || (base.State == CommunicationState.Opened)) || (base.State == CommunicationState.Closing))
                {
                    this.session.OnLocalFault(exception, SequenceTerminatedFault.CreateQuotaExceededFault(this.session.OutputID), null);
                }
            }
            else
            {
                base.EnqueueAndDispatch(exception, null, false);
            }
        }

        private void OnBinderFaulted(IReliableChannelBinder sender, Exception exception)
        {
            this.binder.Abort();
            if (((base.State == CommunicationState.Opening) || (base.State == CommunicationState.Opened)) || (base.State == CommunicationState.Closing))
            {
                exception = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityFaulted"), exception);
                this.session.OnLocalFault(exception, (Message) null, null);
            }
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.ThrowIfCloseInvalid();
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.outputConnection != null)
            {
                if (this.closeOutputWaitObject != null)
                {
                    this.closeOutputWaitObject.Wait(helper.RemainingTime());
                }
                else
                {
                    this.InternalCloseOutputSession(helper.RemainingTime());
                }
                this.inputConnection.Close(helper.RemainingTime());
            }
            this.guard.Close(helper.RemainingTime());
            this.session.Close(helper.RemainingTime());
            this.binder.Close(helper.RemainingTime(), MaskingMode.Handled);
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            this.binder.Faulted -= new BinderExceptionHandler(this.OnBinderFaulted);
            if (this.deliveryStrategy != null)
            {
                this.deliveryStrategy.Dispose();
            }
        }

        protected void OnCloseOutputSession(TimeSpan timeout)
        {
            lock (base.ThisLock)
            {
                base.ThrowIfNotOpened();
                base.ThrowIfFaulted();
                if ((base.State != CommunicationState.Opened) || (this.closeOutputWaitObject != null))
                {
                    return;
                }
                this.closeOutputWaitObject = new InterruptibleWaitObject(false, true);
            }
            bool flag = true;
            try
            {
                this.InternalCloseOutputSession(timeout);
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    this.session.OnLocalFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.session.OutputID, System.ServiceModel.SR.GetString("CloseOutputSessionErrorReason"), null), null);
                    this.closeOutputWaitObject.Fault(this);
                }
                else
                {
                    this.closeOutputWaitObject.Set();
                }
            }
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.acknowledgementTimer.Cancel();
        }

        private void OnComponentException(Exception exception)
        {
            this.ReliableSession.OnUnknownException(exception);
        }

        private void OnComponentFaulted(Exception faultException, WsrmFault fault)
        {
            this.session.OnLocalFault(faultException, fault, null);
        }

        private void OnDeliveryStrategyItemDequeued()
        {
            if (this.advertisedZero)
            {
                this.OnAcknowledgementTimeoutElapsed(null);
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected void OnEndCloseOutputSession(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                bool flag = true;
                try
                {
                    this.EndInternalCloseOutputSession(result);
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        this.session.OnLocalFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.session.OutputID, System.ServiceModel.SR.GetString("CloseOutputSessionErrorReason"), null), null);
                        this.closeOutputWaitObject.Fault(this);
                    }
                    else
                    {
                        this.closeOutputWaitObject.Set();
                    }
                }
            }
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            if (!this.outputConnection.EndAddMessage(result))
            {
                this.ThrowInvalidAddException();
            }
        }

        private void OnEndSendAckRequestedHandler(IAsyncResult result)
        {
            ReliableBinderSendAsyncResult.End(result);
        }

        private void OnEndSendHandler(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                Exception exception;
                ReliableBinderSendAsyncResult.End(result, out exception);
                ReliableBinderSendAsyncResult result2 = (ReliableBinderSendAsyncResult) result;
                if (result2.MessageAttemptInfo.RetryCount == this.settings.MaxRetryCount)
                {
                    this.maxRetryCountException = exception;
                }
            }
        }

        protected override void OnFaulted()
        {
            this.session.OnFaulted();
            this.UnblockClose();
            base.OnFaulted();
        }

        protected virtual void OnMessageDropped()
        {
        }

        protected override void OnOpened()
        {
            base.OnOpened();
        }

        private static void OnReceiveCompletedStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableDuplexSessionChannel asyncState = (ReliableDuplexSessionChannel) result.AsyncState;
                try
                {
                    if (asyncState.HandleReceiveComplete(result))
                    {
                        asyncState.StartReceiving(true);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.ReliableSession.OnUnknownException(exception);
                }
            }
        }

        protected virtual void OnRemoteActivity()
        {
            this.session.OnRemoteActivity(false);
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            if (!this.outputConnection.AddMessage(message, timeout, null))
            {
                this.ThrowInvalidAddException();
            }
        }

        private void OnSendAckRequestedHandler(TimeSpan timeout)
        {
            this.session.OnLocalActivity();
            using (Message message = WsrmUtilities.CreateAckRequestedMessage(this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, this.ReliableSession.OutputID))
            {
                this.binder.Send(message, timeout, MaskingMode.Handled);
            }
        }

        private void OnSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException)
        {
            using (attemptInfo.Message)
            {
                if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
                {
                    this.session.OnLocalFault(new CommunicationException(System.ServiceModel.SR.GetString("MaximumRetryCountExceeded"), this.maxRetryCountException), SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                }
                else
                {
                    this.session.OnLocalActivity();
                    this.AddPendingAcknowledgements(attemptInfo.Message);
                    MaskingMode maskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;
                    if (attemptInfo.RetryCount < this.settings.MaxRetryCount)
                    {
                        maskingMode |= MaskingMode.Handled;
                        this.binder.Send(attemptInfo.Message, timeout, maskingMode);
                    }
                    else
                    {
                        try
                        {
                            this.binder.Send(attemptInfo.Message, timeout, maskingMode);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            if (!this.binder.IsHandleable(exception))
                            {
                                throw;
                            }
                            this.maxRetryCountException = exception;
                        }
                    }
                }
            }
        }

        private WsrmFault ProcessCloseOrTerminateSequenceResponse(bool close, WsrmMessageInfo info)
        {
            SendWaitReliableRequestor requestor = close ? this.closeRequestor : this.terminateRequestor;
            if (requestor != null)
            {
                WsrmFault fault = close ? WsrmUtilities.ValidateCloseSequenceResponse(this.session, this.closeRequestor.MessageId, info, this.outputConnection.Last) : WsrmUtilities.ValidateTerminateSequenceResponse(this.session, this.terminateRequestor.MessageId, info, this.outputConnection.Last);
                if (fault != null)
                {
                    return fault;
                }
                requestor.SetInfo(info);
                return null;
            }
            string str = close ? "CloseSequence" : "TerminateSequence";
            string faultReason = System.ServiceModel.SR.GetString("ReceivedResponseBeforeRequestFaultString", new object[] { str });
            string exceptionMessage = System.ServiceModel.SR.GetString("ReceivedResponseBeforeRequestExceptionString", new object[] { str });
            return SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, faultReason, exceptionMessage);
        }

        protected void ProcessDuplexMessage(WsrmMessageInfo info)
        {
            bool flag = true;
            try
            {
                bool flag2 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                bool flag3 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                bool flag4 = false;
                if ((this.outputConnection != null) && (info.AcknowledgementInfo != null))
                {
                    flag4 = flag3 && info.AcknowledgementInfo.Final;
                    int quotaRemaining = -1;
                    if (this.settings.FlowControlEnabled)
                    {
                        quotaRemaining = info.AcknowledgementInfo.BufferRemaining;
                    }
                    this.outputConnection.ProcessTransferred(info.AcknowledgementInfo.Ranges, quotaRemaining);
                }
                this.OnRemoteActivity();
                bool flag5 = info.AckRequestedInfo != null;
                bool flag6 = false;
                bool flag7 = false;
                bool flag8 = false;
                ulong ackVersion = 0L;
                WsrmFault fault = null;
                Message message = null;
                Exception e = null;
                if (info.SequencedMessageInfo != null)
                {
                    bool flag9 = false;
                    lock (base.ThisLock)
                    {
                        if (base.Aborted || (base.State == CommunicationState.Faulted))
                        {
                            return;
                        }
                        long sequenceNumber = info.SequencedMessageInfo.SequenceNumber;
                        bool isLast = flag2 && info.SequencedMessageInfo.LastMessage;
                        if (!this.inputConnection.IsValid(sequenceNumber, isLast))
                        {
                            if (flag2)
                            {
                                fault = new LastMessageNumberExceededFault(this.ReliableSession.InputID);
                            }
                            else
                            {
                                message = new SequenceClosedFault(this.session.InputID).CreateMessage(this.settings.MessageVersion, this.settings.ReliableMessagingVersion);
                                flag6 = true;
                                this.OnMessageDropped();
                            }
                        }
                        else if (this.inputConnection.Ranges.Contains(sequenceNumber))
                        {
                            this.OnMessageDropped();
                            flag5 = true;
                        }
                        else if (flag2 && (info.Action == "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage"))
                        {
                            this.inputConnection.Merge(sequenceNumber, isLast);
                            if (this.inputConnection.AllAdded)
                            {
                                flag8 = true;
                                if (this.outputConnection.CheckForTermination())
                                {
                                    this.session.CloseSession();
                                }
                            }
                        }
                        else if (base.State == CommunicationState.Closing)
                        {
                            if (flag2)
                            {
                                fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedSessionClosedBeforeDone"), System.ServiceModel.SR.GetString("SessionClosedBeforeDone"));
                            }
                            else
                            {
                                message = new SequenceClosedFault(this.session.InputID).CreateMessage(this.settings.MessageVersion, this.settings.ReliableMessagingVersion);
                                flag6 = true;
                                this.OnMessageDropped();
                            }
                        }
                        else if (this.deliveryStrategy.CanEnqueue(sequenceNumber) && (this.Settings.Ordered || this.inputConnection.CanMerge(sequenceNumber)))
                        {
                            this.inputConnection.Merge(sequenceNumber, isLast);
                            flag9 = this.deliveryStrategy.Enqueue(info.Message, sequenceNumber);
                            flag = false;
                            ackVersion = this.ackVersion;
                            this.pendingAcknowledgements++;
                            if (this.inputConnection.AllAdded)
                            {
                                flag8 = true;
                                if (this.outputConnection.CheckForTermination())
                                {
                                    this.session.CloseSession();
                                }
                            }
                        }
                        else
                        {
                            this.OnMessageDropped();
                        }
                        if (this.inputConnection.IsLastKnown || (this.pendingAcknowledgements == this.settings.MaxTransferWindowSize))
                        {
                            flag5 = true;
                        }
                        if ((flag5 || ((this.pendingAcknowledgements > 0) && (fault == null))) && !this.acknowledgementScheduled)
                        {
                            this.acknowledgementScheduled = true;
                            this.acknowledgementTimer.Set(this.settings.AcknowledgementInterval);
                        }
                    }
                    if (flag9)
                    {
                        base.Dispatch();
                    }
                }
                else if (flag2 && (info.TerminateSequenceInfo != null))
                {
                    bool flag13;
                    lock (base.ThisLock)
                    {
                        flag13 = !this.inputConnection.Terminate();
                    }
                    if (flag13)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedEarlyTerminateSequence"), System.ServiceModel.SR.GetString("EarlyTerminateSequence"));
                    }
                }
                else if (flag3)
                {
                    if (((info.TerminateSequenceInfo != null) && (info.TerminateSequenceInfo.Identifier == this.session.InputID)) || (info.CloseSequenceInfo != null))
                    {
                        bool flag15 = info.TerminateSequenceInfo != null;
                        WsrmRequestInfo info2 = flag15 ? ((WsrmRequestInfo) info.TerminateSequenceInfo) : ((WsrmRequestInfo) info.CloseSequenceInfo);
                        long last = flag15 ? info.TerminateSequenceInfo.LastMsgNumber : info.CloseSequenceInfo.LastMsgNumber;
                        if (!WsrmUtilities.ValidateWsrmRequest(this.session, info2, this.binder, null))
                        {
                            return;
                        }
                        bool isLastLargeEnough = true;
                        bool flag17 = true;
                        lock (base.ThisLock)
                        {
                            if (!this.inputConnection.IsLastKnown)
                            {
                                if (flag15)
                                {
                                    if (this.inputConnection.SetTerminateSequenceLast(last, out isLastLargeEnough))
                                    {
                                        flag8 = true;
                                    }
                                    else if (isLastLargeEnough)
                                    {
                                        e = new ProtocolException(System.ServiceModel.SR.GetString("EarlyTerminateSequence"));
                                    }
                                }
                                else
                                {
                                    flag8 = this.inputConnection.SetCloseSequenceLast(last);
                                    isLastLargeEnough = flag8;
                                }
                                if (flag8)
                                {
                                    this.session.SetFinalAck(this.inputConnection.Ranges);
                                    if (this.terminateRequestor != null)
                                    {
                                        this.session.CloseSession();
                                    }
                                    this.deliveryStrategy.Dispose();
                                }
                            }
                            else
                            {
                                flag17 = last == this.inputConnection.Last;
                                if ((flag15 && flag17) && this.inputConnection.IsSequenceClosed)
                                {
                                    flag7 = true;
                                }
                            }
                        }
                        if (!isLastLargeEnough)
                        {
                            string faultReason = System.ServiceModel.SR.GetString("SequenceTerminatedSmallLastMsgNumber");
                            string exceptionMessage = System.ServiceModel.SR.GetString("SmallLastMsgNumberExceptionString");
                            fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, faultReason, exceptionMessage);
                        }
                        else if (!flag17)
                        {
                            string str3 = System.ServiceModel.SR.GetString("SequenceTerminatedInconsistentLastMsgNumber");
                            string str4 = System.ServiceModel.SR.GetString("InconsistentLastMsgNumberExceptionString");
                            fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, str3, str4);
                        }
                        else
                        {
                            message = flag15 ? WsrmUtilities.CreateTerminateResponseMessage(this.settings.MessageVersion, info2.MessageId, this.session.InputID) : WsrmUtilities.CreateCloseSequenceResponse(this.settings.MessageVersion, info2.MessageId, this.session.InputID);
                            flag6 = true;
                        }
                    }
                    else if (info.TerminateSequenceInfo != null)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnsupportedTerminateSequence"), System.ServiceModel.SR.GetString("UnsupportedTerminateSequenceExceptionString"));
                    }
                    else if (info.TerminateSequenceResponseInfo != null)
                    {
                        fault = this.ProcessCloseOrTerminateSequenceResponse(false, info);
                    }
                    else if (info.CloseSequenceResponseInfo != null)
                    {
                        fault = this.ProcessCloseOrTerminateSequenceResponse(true, info);
                    }
                    else if (flag4)
                    {
                        if (this.closeRequestor == null)
                        {
                            string str5 = System.ServiceModel.SR.GetString("UnsupportedCloseExceptionString");
                            string str6 = System.ServiceModel.SR.GetString("SequenceTerminatedUnsupportedClose");
                            fault = SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, str6, str5);
                        }
                        else
                        {
                            fault = WsrmUtilities.ValidateFinalAck(this.session, info, this.outputConnection.Last);
                            if (fault == null)
                            {
                                this.closeRequestor.SetInfo(info);
                            }
                        }
                    }
                    else if (info.WsrmHeaderFault != null)
                    {
                        if (!(info.WsrmHeaderFault is UnknownSequenceFault))
                        {
                            throw Fx.AssertAndThrow("Fault must be UnknownSequence fault.");
                        }
                        if (this.terminateRequestor == null)
                        {
                            throw Fx.AssertAndThrow("In wsrm11, if we start getting UnknownSequence, terminateRequestor cannot be null.");
                        }
                        this.terminateRequestor.SetInfo(info);
                    }
                }
                if (fault != null)
                {
                    this.session.OnLocalFault(fault.CreateException(), fault, null);
                }
                else
                {
                    if (flag8)
                    {
                        ActionItem.Schedule(new Action<object>(this.ShutdownCallback), null);
                    }
                    if (message != null)
                    {
                        if (flag6)
                        {
                            WsrmUtilities.AddAcknowledgementHeader(this.settings.ReliableMessagingVersion, message, this.session.InputID, this.inputConnection.Ranges, true, this.GetBufferRemaining());
                        }
                        else if (flag5)
                        {
                            this.AddPendingAcknowledgements(message);
                        }
                    }
                    else if (flag5)
                    {
                        lock (base.ThisLock)
                        {
                            if ((ackVersion != 0L) && (ackVersion != this.ackVersion))
                            {
                                return;
                            }
                            if (this.acknowledgementScheduled)
                            {
                                this.acknowledgementTimer.Cancel();
                                this.acknowledgementScheduled = false;
                            }
                            this.pendingAcknowledgements = 0;
                        }
                        message = this.CreateAcknowledgmentMessage();
                    }
                    if (message != null)
                    {
                        using (message)
                        {
                            if (this.guard.Enter())
                            {
                                try
                                {
                                    this.binder.Send(message, base.DefaultSendTimeout);
                                }
                                finally
                                {
                                    this.guard.Exit();
                                }
                            }
                        }
                    }
                    if (flag7)
                    {
                        lock (base.ThisLock)
                        {
                            this.inputConnection.Terminate();
                        }
                    }
                    if (e != null)
                    {
                        this.ReliableSession.OnRemoteFault(e);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    info.Message.Close();
                }
            }
        }

        protected abstract void ProcessMessage(WsrmMessageInfo info);
        protected void SetConnections()
        {
            this.outputConnection = new ReliableOutputConnection(this.session.OutputID, this.settings.MaxTransferWindowSize, this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, this.session.InitiationTime, true, base.DefaultSendTimeout);
            this.outputConnection.Faulted = (ComponentFaultedHandler) Delegate.Combine(this.outputConnection.Faulted, new ComponentFaultedHandler(this.OnComponentFaulted));
            this.outputConnection.OnException = (ComponentExceptionHandler) Delegate.Combine(this.outputConnection.OnException, new ComponentExceptionHandler(this.OnComponentException));
            this.outputConnection.BeginSendHandler = new BeginSendHandler(this.OnBeginSendHandler);
            this.outputConnection.EndSendHandler = new EndSendHandler(this.OnEndSendHandler);
            this.outputConnection.SendHandler = new SendHandler(this.OnSendHandler);
            this.outputConnection.BeginSendAckRequestedHandler = new OperationWithTimeoutBeginCallback(this.OnBeginSendAckRequestedHandler);
            this.outputConnection.EndSendAckRequestedHandler = new OperationEndCallback(this.OnEndSendAckRequestedHandler);
            this.outputConnection.SendAckRequestedHandler = new OperationWithTimeoutCallback(this.OnSendAckRequestedHandler);
            this.inputConnection = new ReliableInputConnection();
            this.inputConnection.ReliableMessagingVersion = this.Settings.ReliableMessagingVersion;
            if (this.settings.Ordered)
            {
                this.deliveryStrategy = new OrderedDeliveryStrategy<Message>(this, this.settings.MaxTransferWindowSize, false);
            }
            else
            {
                this.deliveryStrategy = new UnorderedDeliveryStrategy<Message>(this, this.settings.MaxTransferWindowSize);
            }
            this.deliveryStrategy.DequeueCallback = new Action(this.OnDeliveryStrategyItemDequeued);
        }

        protected void SetSession(ChannelReliableSession session)
        {
            session.UnblockChannelCloseCallback = new ChannelReliableSession.UnblockChannelCloseHandler(this.UnblockClose);
            this.session = session;
        }

        private void ShutdownCallback(object state)
        {
            base.Shutdown();
        }

        protected void StartReceiving(bool canBlock)
        {
            IAsyncResult result;
        Label_0000:
            result = this.binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
            if (result.CompletedSynchronously)
            {
                if (!canBlock)
                {
                    ActionItem.Schedule(asyncReceiveComplete, result);
                }
                else if (this.HandleReceiveComplete(result))
                {
                    goto Label_0000;
                }
            }
        }

        private void TerminateSequence(TimeSpan timeout)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.outputConnection.CheckForTermination())
                {
                    this.session.CloseSession();
                }
                Message message = WsrmUtilities.CreateTerminateMessage(this.settings.MessageVersion, reliableMessagingVersion, this.session.OutputID);
                this.binder.Send(message, timeout, MaskingMode.Handled);
            }
            else
            {
                if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
                {
                    throw Fx.AssertAndThrow("Reliable messaging version not supported.");
                }
                this.CreateTerminateRequestor();
                this.terminateRequestor.Request(timeout);
            }
        }

        private void ThrowIfCloseInvalid()
        {
            bool flag = false;
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if ((this.deliveryStrategy.EnqueuedCount > 0) || (this.inputConnection.Ranges.Count > 1))
                {
                    flag = true;
                }
            }
            else if ((this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && (this.deliveryStrategy.EnqueuedCount > 0))
            {
                flag = true;
            }
            if (flag)
            {
                WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedSessionClosedBeforeDone"), System.ServiceModel.SR.GetString("SessionClosedBeforeDone"));
                this.session.OnLocalFault(null, fault, null);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
            }
        }

        private void ThrowInvalidAddException()
        {
            if (base.State == CommunicationState.Opened)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SendCannotBeCalledAfterCloseOutputSession")));
            }
            if (base.State == CommunicationState.Faulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.GetTerminalException());
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateClosedException());
        }

        private void UnblockClose()
        {
            if (this.outputConnection != null)
            {
                this.outputConnection.Fault(this);
            }
            if (this.inputConnection != null)
            {
                this.inputConnection.Fault(this);
            }
            ReliableRequestor closeRequestor = this.closeRequestor;
            if (closeRequestor != null)
            {
                closeRequestor.Fault(this);
            }
            closeRequestor = this.terminateRequestor;
            if (closeRequestor != null)
            {
                closeRequestor.Fault(this);
            }
        }

        public IReliableChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        public override EndpointAddress LocalAddress
        {
            get
            {
                return this.binder.LocalAddress;
            }
        }

        protected ReliableOutputConnection OutputConnection
        {
            get
            {
                return this.outputConnection;
            }
        }

        protected UniqueId OutputID
        {
            get
            {
                return this.session.OutputID;
            }
        }

        protected ChannelReliableSession ReliableSession
        {
            get
            {
                return this.session;
            }
        }

        public override EndpointAddress RemoteAddress
        {
            get
            {
                return this.binder.RemoteAddress;
            }
        }

        public IDuplexSession Session
        {
            get
            {
                return (IDuplexSession) this.session;
            }
        }

        protected IReliableFactorySettings Settings
        {
            get
            {
                return this.settings;
            }
        }

        public override Uri Via
        {
            get
            {
                return this.RemoteAddress.Uri;
            }
        }
    }
}

