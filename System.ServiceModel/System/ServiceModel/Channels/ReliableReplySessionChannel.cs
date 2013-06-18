namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class ReliableReplySessionChannel : ReplyChannel, IReplySessionChannel, IReplyChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
        private List<long> acked;
        private static Action<object> asyncReceiveComplete = new Action<object>(ReliableReplySessionChannel.AsyncReceiveCompleteStatic);
        private IServerReliableChannelBinder binder;
        private ReplyHelper closeSequenceReplyHelper;
        private ReliableInputConnection connection;
        private bool contextAborted;
        private DeliveryStrategy<RequestContext> deliveryStrategy;
        private ReliableRequestContext lastReply;
        private bool lastReplyAcked;
        private long lastReplySequenceNumber;
        private ReliableChannelListenerBase<IReplySessionChannel> listener;
        private InterruptibleWaitObject messagingCompleteWaitObject;
        private long nextReplySequenceNumber;
        private static AsyncCallback onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(ReliableReplySessionChannel.OnReceiveCompletedStatic));
        private string perfCounterId;
        private Dictionary<long, ReliableRequestContext> requestsByReplySequenceNumber;
        private Dictionary<long, ReliableRequestContext> requestsByRequestSequenceNumber;
        private ServerReliableSession session;
        private ReplyHelper terminateSequenceReplyHelper;

        public ReliableReplySessionChannel(ReliableChannelListenerBase<IReplySessionChannel> listener, IServerReliableChannelBinder binder, FaultHelper faultHelper, UniqueId inputID, UniqueId outputID) : base(listener, binder.LocalAddress)
        {
            this.acked = new List<long>();
            this.lastReplySequenceNumber = -9223372036854775808L;
            this.requestsByRequestSequenceNumber = new Dictionary<long, ReliableRequestContext>();
            this.requestsByReplySequenceNumber = new Dictionary<long, ReliableRequestContext>();
            this.listener = listener;
            this.connection = new ReliableInputConnection();
            this.connection.ReliableMessagingVersion = this.listener.ReliableMessagingVersion;
            this.binder = binder;
            this.session = new ServerReliableSession(this, listener, binder, faultHelper, inputID, outputID);
            this.session.UnblockChannelCloseCallback = new ChannelReliableSession.UnblockChannelCloseHandler(this.UnblockClose);
            if (this.listener.Ordered)
            {
                this.deliveryStrategy = new OrderedDeliveryStrategy<RequestContext>(this, this.listener.MaxTransferWindowSize, true);
            }
            else
            {
                this.deliveryStrategy = new UnorderedDeliveryStrategy<RequestContext>(this, this.listener.MaxTransferWindowSize);
            }
            this.binder.Faulted += new BinderExceptionHandler(this.OnBinderFaulted);
            this.binder.OnException += new BinderExceptionHandler(this.OnBinderException);
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.messagingCompleteWaitObject = new InterruptibleWaitObject(false);
            }
            this.session.Open(TimeSpan.Zero);
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                this.perfCounterId = this.listener.Uri.ToString().ToUpperInvariant();
            }
            if (binder.HasSession)
            {
                try
                {
                    this.StartReceiving(false);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.session.OnUnknownException(exception);
                }
            }
        }

        private void AbortContexts()
        {
            lock (base.ThisLock)
            {
                if (this.contextAborted)
                {
                    return;
                }
                this.contextAborted = true;
            }
            foreach (ReliableRequestContext context in this.requestsByRequestSequenceNumber.Values)
            {
                context.Abort();
            }
            this.requestsByRequestSequenceNumber.Clear();
            this.requestsByReplySequenceNumber.Clear();
            if ((this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && (this.lastReply != null))
            {
                this.lastReply.Abort();
            }
        }

        private void AddAcknowledgementHeader(Message message)
        {
            WsrmUtilities.AddAcknowledgementHeader(this.listener.ReliableMessagingVersion, message, this.session.InputID, this.connection.Ranges, this.connection.IsLastKnown, this.listener.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount);
        }

        private static void AsyncReceiveCompleteStatic(object state)
        {
            IAsyncResult result = (IAsyncResult) state;
            ReliableReplySessionChannel asyncState = (ReliableReplySessionChannel) result.AsyncState;
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
                asyncState.session.OnUnknownException(exception);
            }
        }

        private IAsyncResult BeginCloseBinder(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.binder.BeginClose(timeout, MaskingMode.Handled, callback, state);
        }

        private IAsyncResult BeginCloseOutput(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                ReliableRequestContext lastReply = this.lastReply;
                if (lastReply == null)
                {
                    return new CloseOutputCompletedAsyncResult(callback, state);
                }
                return lastReply.BeginReplyInternal(null, timeout, callback, state);
            }
            lock (base.ThisLock)
            {
                base.ThrowIfClosed();
                this.CreateCloseSequenceReplyHelper();
            }
            return this.closeSequenceReplyHelper.BeginWaitAndReply(timeout, callback, state);
        }

        private IAsyncResult BeginTerminateSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            lock (base.ThisLock)
            {
                base.ThrowIfClosed();
                this.CreateTerminateSequenceReplyHelper();
            }
            return this.terminateSequenceReplyHelper.BeginWaitAndReply(timeout, callback, state);
        }

        private IAsyncResult BeginUnregisterChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.listener.OnReliableChannelBeginClose(this.session.InputID, this.session.OutputID, timeout, callback, state);
        }

        private void CloseOutput(TimeSpan timeout)
        {
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                ReliableRequestContext lastReply = this.lastReply;
                if (lastReply != null)
                {
                    lastReply.ReplyInternal(null, timeout);
                }
            }
            else
            {
                lock (base.ThisLock)
                {
                    base.ThrowIfClosed();
                    this.CreateCloseSequenceReplyHelper();
                }
                this.closeSequenceReplyHelper.WaitAndReply(timeout);
            }
        }

        private bool ContainsRequest(long requestSeqNum)
        {
            lock (base.ThisLock)
            {
                bool flag = this.requestsByRequestSequenceNumber.ContainsKey(requestSeqNum);
                if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    return (flag || (((this.lastReply != null) && (this.lastReply.RequestSequenceNumber == requestSeqNum)) && !this.lastReplyAcked));
                }
                return flag;
            }
        }

        private Message CreateAcknowledgement(SequenceRangeCollection ranges)
        {
            return WsrmUtilities.CreateAcknowledgmentMessage(this.MessageVersion, this.listener.ReliableMessagingVersion, this.session.InputID, ranges, this.connection.IsLastKnown, this.listener.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount);
        }

        private bool CreateCloseSequenceReplyHelper()
        {
            if ((base.State == CommunicationState.Faulted) || base.Aborted)
            {
                return false;
            }
            if (this.closeSequenceReplyHelper == null)
            {
                this.closeSequenceReplyHelper = new ReplyHelper(this, CloseSequenceReplyProvider.Instance, true);
            }
            return true;
        }

        private Message CreateSequenceClosedFault()
        {
            Message message = new SequenceClosedFault(this.session.InputID).CreateMessage(this.listener.MessageVersion, this.listener.ReliableMessagingVersion);
            this.AddAcknowledgementHeader(message);
            return message;
        }

        private bool CreateTerminateSequenceReplyHelper()
        {
            if ((base.State == CommunicationState.Faulted) || base.Aborted)
            {
                return false;
            }
            if (this.terminateSequenceReplyHelper == null)
            {
                this.terminateSequenceReplyHelper = new ReplyHelper(this, TerminateSequenceReplyProvider.Instance, false);
            }
            return true;
        }

        private void EndCloseBinder(IAsyncResult result)
        {
            this.binder.EndClose(result);
        }

        private void EndCloseOutput(IAsyncResult result)
        {
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (result is CloseOutputCompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    this.lastReply.EndReplyInternal(result);
                }
            }
            else
            {
                this.closeSequenceReplyHelper.EndWaitAndReply(result);
            }
        }

        private void EndTerminateSequence(IAsyncResult result)
        {
            this.terminateSequenceReplyHelper.EndWaitAndReply(result);
            this.OnTerminateSequenceCompleted();
        }

        private void EndUnregisterChannel(IAsyncResult result)
        {
            this.listener.OnReliableChannelEndClose(result);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IReplySessionChannel))
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
                return (T) FaultConverter.GetDefaultFaultConverter(this.listener.MessageVersion);
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
                    flag = this.connection.Terminate();
                }
                if (!flag && (this.Binder.State == CommunicationState.Opened))
                {
                    Exception e = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityClose"));
                    this.session.OnLocalFault(e, (Message) null, null);
                }
                return false;
            }
            WsrmMessageInfo info = WsrmMessageInfo.Get(this.listener.MessageVersion, this.listener.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), context.RequestMessage);
            this.StartReceiving(false);
            this.ProcessRequest(context, info);
            return false;
        }

        protected override void OnAbort()
        {
            if (this.closeSequenceReplyHelper != null)
            {
                this.closeSequenceReplyHelper.Abort();
            }
            this.connection.Abort(this);
            if (this.terminateSequenceReplyHelper != null)
            {
                this.terminateSequenceReplyHelper.Abort();
            }
            this.session.Abort();
            this.AbortContexts();
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.messagingCompleteWaitObject.Abort(this);
            }
            this.listener.OnReliableChannelAbort(this.session.InputID, this.session.OutputID);
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfCloseInvalid();
            bool flag = this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.BeginCloseOutput), flag ? new OperationWithTimeoutBeginCallback(this.connection.BeginClose) : new OperationWithTimeoutBeginCallback(this.BeginTerminateSequence), flag ? new OperationWithTimeoutBeginCallback(this.messagingCompleteWaitObject.BeginWait) : new OperationWithTimeoutBeginCallback(this.connection.BeginClose), new OperationWithTimeoutBeginCallback(this.session.BeginClose), new OperationWithTimeoutBeginCallback(this.BeginCloseBinder), new OperationWithTimeoutBeginCallback(this.BeginUnregisterChannel), new OperationWithTimeoutBeginCallback(this.OnBeginClose) };
            OperationEndCallback[] endOperations = new OperationEndCallback[] { new OperationEndCallback(this.EndCloseOutput), flag ? new OperationEndCallback(this.connection.EndClose) : new OperationEndCallback(this.EndTerminateSequence), flag ? new OperationEndCallback(this.messagingCompleteWaitObject.EndWait) : new OperationEndCallback(this.connection.EndClose), new OperationEndCallback(this.session.EndClose), new OperationEndCallback(this.EndCloseBinder), new OperationEndCallback(this.EndUnregisterChannel), new OperationEndCallback(this.OnEndClose) };
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        private void OnBinderException(IReliableChannelBinder sender, Exception exception)
        {
            if (exception is QuotaExceededException)
            {
                this.session.OnLocalFault(exception, (Message) null, null);
            }
            else
            {
                base.EnqueueAndDispatch(exception, null, false);
            }
        }

        private void OnBinderFaulted(IReliableChannelBinder sender, Exception exception)
        {
            this.binder.Abort();
            exception = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityFaulted"), exception);
            this.session.OnLocalFault(exception, (Message) null, null);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.ThrowIfCloseInvalid();
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CloseOutput(helper.RemainingTime());
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.connection.Close(helper.RemainingTime());
                this.messagingCompleteWaitObject.Wait(helper.RemainingTime());
            }
            else
            {
                this.TerminateSequence(helper.RemainingTime());
                this.connection.Close(helper.RemainingTime());
            }
            this.session.Close(helper.RemainingTime());
            this.binder.Close(helper.RemainingTime(), MaskingMode.Handled);
            this.listener.OnReliableChannelClose(this.session.InputID, this.session.OutputID, helper.RemainingTime());
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnClosed()
        {
            this.deliveryStrategy.Dispose();
            this.binder.Faulted -= new BinderExceptionHandler(this.OnBinderFaulted);
            if ((this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && (this.lastReply != null))
            {
                this.lastReply.Abort();
            }
            base.OnClosed();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnFaulted()
        {
            this.session.OnFaulted();
            this.UnblockClose();
            base.OnFaulted();
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.SessionFaulted(this.perfCounterId);
            }
        }

        private static void OnReceiveCompletedStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableReplySessionChannel asyncState = (ReliableReplySessionChannel) result.AsyncState;
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
                    asyncState.session.OnUnknownException(exception);
                }
            }
        }

        private void OnTerminateSequenceCompleted()
        {
            if ((this.session.Settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && this.connection.IsSequenceClosed)
            {
                lock (base.ThisLock)
                {
                    this.connection.Terminate();
                }
            }
        }

        private bool PrepareReply(ReliableRequestContext context)
        {
            lock (base.ThisLock)
            {
                if ((base.Aborted || (base.State == CommunicationState.Faulted)) || (base.State == CommunicationState.Closed))
                {
                    return false;
                }
                long requestSequenceNumber = context.RequestSequenceNumber;
                bool flag = this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                if (flag && (this.connection.Last == requestSequenceNumber))
                {
                    if (this.lastReply == null)
                    {
                        this.lastReply = context;
                    }
                    this.requestsByRequestSequenceNumber.Remove(requestSequenceNumber);
                    if (!this.connection.AllAdded || (base.State != CommunicationState.Closing))
                    {
                        return false;
                    }
                }
                else
                {
                    if (base.State == CommunicationState.Closing)
                    {
                        return false;
                    }
                    if (!context.HasReply)
                    {
                        this.requestsByRequestSequenceNumber.Remove(requestSequenceNumber);
                        return true;
                    }
                }
                if (this.nextReplySequenceNumber == 0x7fffffffffffffffL)
                {
                    MessageNumberRolloverFault fault = new MessageNumberRolloverFault(this.session.OutputID);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
                }
                context.SetReplySequenceNumber(this.nextReplySequenceNumber += 1L);
                if (flag && (this.connection.Last == requestSequenceNumber))
                {
                    if (!context.HasReply)
                    {
                        this.lastReplyAcked = true;
                    }
                    this.lastReplySequenceNumber = this.nextReplySequenceNumber;
                    context.SetLastReply(this.lastReplySequenceNumber);
                }
                else if (context.HasReply)
                {
                    this.requestsByReplySequenceNumber.Add(this.nextReplySequenceNumber, context);
                }
                return true;
            }
        }

        private Message PrepareReplyMessage(long replySequenceNumber, bool isLast, SequenceRangeCollection ranges, Message reply)
        {
            this.AddAcknowledgementHeader(reply);
            WsrmUtilities.AddSequenceHeader(this.listener.ReliableMessagingVersion, reply, this.session.OutputID, replySequenceNumber, isLast);
            return reply;
        }

        private void ProcessAcknowledgment(WsrmAcknowledgmentInfo info)
        {
            lock (base.ThisLock)
            {
                if (((!base.Aborted && (base.State != CommunicationState.Faulted)) && (base.State != CommunicationState.Closed)) && (this.requestsByReplySequenceNumber.Count > 0))
                {
                    long key;
                    this.acked.Clear();
                    foreach (KeyValuePair<long, ReliableRequestContext> pair in this.requestsByReplySequenceNumber)
                    {
                        key = pair.Key;
                        if (info.Ranges.Contains(key))
                        {
                            this.acked.Add(key);
                        }
                    }
                    for (int i = 0; i < this.acked.Count; i++)
                    {
                        key = this.acked[i];
                        this.requestsByRequestSequenceNumber.Remove(this.requestsByReplySequenceNumber[key].RequestSequenceNumber);
                        this.requestsByReplySequenceNumber.Remove(key);
                    }
                    if (((this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && !this.lastReplyAcked) && (this.lastReplySequenceNumber != -9223372036854775808L))
                    {
                        this.lastReplyAcked = info.Ranges.Contains(this.lastReplySequenceNumber);
                    }
                }
            }
        }

        private void ProcessAckRequested(RequestContext context)
        {
            try
            {
                using (Message message = this.CreateAcknowledgement(this.connection.Ranges))
                {
                    context.Reply(message);
                }
            }
            finally
            {
                context.RequestMessage.Close();
                context.Close();
            }
        }

        public void ProcessDemuxedRequest(RequestContext context, WsrmMessageInfo info)
        {
            try
            {
                this.ProcessRequest(context, info);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.session.OnUnknownException(exception);
            }
        }

        private void ProcessRequest(RequestContext context, WsrmMessageInfo info)
        {
            bool flag = true;
            bool flag2 = true;
            try
            {
                EndpointAddress address;
                if (!this.session.ProcessInfo(info, context))
                {
                    flag = false;
                    flag2 = false;
                    return;
                }
                if (!this.session.VerifyDuplexProtocolElements(info, context))
                {
                    flag = false;
                    flag2 = false;
                    return;
                }
                this.session.OnRemoteActivity(false);
                if (info.CreateSequenceInfo == null)
                {
                    goto Label_0102;
                }
                if (WsrmUtilities.ValidateCreateSequence<IReplySessionChannel>(info, this.listener, this.binder.Channel, out address))
                {
                    Message response = WsrmUtilities.CreateCreateSequenceResponse(this.listener.MessageVersion, this.listener.ReliableMessagingVersion, true, info.CreateSequenceInfo, this.listener.Ordered, this.session.InputID, address);
                    using (context)
                    {
                        using (response)
                        {
                            if (this.Binder.AddressResponse(info.Message, response))
                            {
                                context.Reply(response, base.DefaultSendTimeout);
                            }
                        }
                        goto Label_00FB;
                    }
                }
                this.session.OnLocalFault(info.FaultException, info.FaultReply, context);
            Label_00FB:
                flag2 = false;
                return;
            Label_0102:
                flag2 = false;
                if (info.AcknowledgementInfo != null)
                {
                    this.ProcessAcknowledgment(info.AcknowledgementInfo);
                    flag2 = info.Action == WsrmIndex.GetSequenceAcknowledgementActionString(this.listener.ReliableMessagingVersion);
                }
                if (!flag2)
                {
                    flag = false;
                    if (info.SequencedMessageInfo != null)
                    {
                        this.ProcessSequencedMessage(context, info.Action, info.SequencedMessageInfo);
                    }
                    else if (info.TerminateSequenceInfo != null)
                    {
                        if (this.listener.ReliableMessagingVersion != ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                        {
                            if (info.TerminateSequenceInfo.Identifier != this.session.InputID)
                            {
                                WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnsupportedTerminateSequence"), System.ServiceModel.SR.GetString("UnsupportedTerminateSequenceExceptionString"));
                                this.session.OnLocalFault(fault.CreateException(), fault, context);
                                flag = false;
                                flag2 = false;
                                return;
                            }
                            this.ProcessShutdown11(context, info);
                        }
                        else
                        {
                            this.ProcessTerminateSequenceFeb2005(context, info);
                        }
                    }
                    else if (info.CloseSequenceInfo != null)
                    {
                        this.ProcessShutdown11(context, info);
                    }
                    else if (info.AckRequestedInfo != null)
                    {
                        this.ProcessAckRequested(context);
                    }
                }
                if ((this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && this.IsMessagingCompleted)
                {
                    this.messagingCompleteWaitObject.Set();
                }
            }
            finally
            {
                if (flag)
                {
                    info.Message.Close();
                }
                if (flag2)
                {
                    context.Close();
                }
            }
        }

        private void ProcessSequencedMessage(RequestContext context, string action, WsrmSequencedMessageInfo info)
        {
            ReliableRequestContext lastReply = null;
            WsrmFault fault = null;
            bool flag6;
            bool flag = false;
            bool allAdded = false;
            bool flag3 = this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
            ReliableMessagingVersion reliableMessagingVersion = this.listener.ReliableMessagingVersion;
            ReliableMessagingVersion version2 = ReliableMessagingVersion.WSReliableMessaging11;
            long sequenceNumber = info.SequenceNumber;
            bool isLast = flag3 && info.LastMessage;
            bool flag5 = flag3 && (action == "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage");
            Message message = null;
            lock (base.ThisLock)
            {
                if ((base.Aborted || (base.State == CommunicationState.Faulted)) || (base.State == CommunicationState.Closed))
                {
                    context.RequestMessage.Close();
                    context.Abort();
                    return;
                }
                flag6 = this.connection.Ranges.Contains(sequenceNumber);
                if (!this.connection.IsValid(sequenceNumber, isLast))
                {
                    if (flag3)
                    {
                        fault = new LastMessageNumberExceededFault(this.session.InputID);
                    }
                    else
                    {
                        message = this.CreateSequenceClosedFault();
                        if (PerformanceCounters.PerformanceCountersEnabled)
                        {
                            PerformanceCounters.MessageDropped(this.perfCounterId);
                        }
                    }
                }
                else if (flag6)
                {
                    if (PerformanceCounters.PerformanceCountersEnabled)
                    {
                        PerformanceCounters.MessageDropped(this.perfCounterId);
                    }
                    if (!this.requestsByRequestSequenceNumber.TryGetValue(info.SequenceNumber, out lastReply))
                    {
                        if ((this.lastReply != null) && (this.lastReply.RequestSequenceNumber == info.SequenceNumber))
                        {
                            lastReply = this.lastReply;
                        }
                        else
                        {
                            lastReply = new ReliableRequestContext(context, info.SequenceNumber, this, true);
                        }
                    }
                    lastReply.SetAckRanges(this.connection.Ranges);
                }
                else if ((base.State == CommunicationState.Closing) && !flag5)
                {
                    if (flag3)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedSessionClosedBeforeDone"), System.ServiceModel.SR.GetString("SessionClosedBeforeDone"));
                    }
                    else
                    {
                        message = this.CreateSequenceClosedFault();
                        if (PerformanceCounters.PerformanceCountersEnabled)
                        {
                            PerformanceCounters.MessageDropped(this.perfCounterId);
                        }
                    }
                }
                else if ((this.deliveryStrategy.CanEnqueue(sequenceNumber) && (this.requestsByReplySequenceNumber.Count < this.listener.MaxTransferWindowSize)) && (this.listener.Ordered || this.connection.CanMerge(sequenceNumber)))
                {
                    this.connection.Merge(sequenceNumber, isLast);
                    lastReply = new ReliableRequestContext(context, info.SequenceNumber, this, false);
                    lastReply.SetAckRanges(this.connection.Ranges);
                    if (!flag5)
                    {
                        flag = this.deliveryStrategy.Enqueue(lastReply, sequenceNumber);
                        this.requestsByRequestSequenceNumber.Add(info.SequenceNumber, lastReply);
                    }
                    else
                    {
                        this.lastReply = lastReply;
                    }
                    allAdded = this.connection.AllAdded;
                }
                else if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.MessageDropped(this.perfCounterId);
                }
            }
            if (fault == null)
            {
                if (lastReply == null)
                {
                    if (message != null)
                    {
                        using (message)
                        {
                            context.Reply(message);
                        }
                    }
                    context.RequestMessage.Close();
                    context.Close();
                }
                else if (flag6 && lastReply.CheckForReplyOrAddInnerContext(context))
                {
                    lastReply.SendReply(context, MaskingMode.All);
                }
                else
                {
                    if (!flag6 && flag5)
                    {
                        lastReply.Close();
                    }
                    if (flag)
                    {
                        base.Dispatch();
                    }
                    if (allAdded)
                    {
                        ActionItem.Schedule(new Action<object>(this.ShutdownCallback), null);
                    }
                }
            }
            else
            {
                this.session.OnLocalFault(fault.CreateException(), fault, context);
            }
        }

        private void ProcessShutdown11(RequestContext context, WsrmMessageInfo info)
        {
            bool flag = true;
            try
            {
                bool flag2 = info.TerminateSequenceInfo != null;
                WsrmRequestInfo info2 = flag2 ? ((WsrmRequestInfo) info.TerminateSequenceInfo) : ((WsrmRequestInfo) info.CloseSequenceInfo);
                long last = flag2 ? info.TerminateSequenceInfo.LastMsgNumber : info.CloseSequenceInfo.LastMsgNumber;
                if (!WsrmUtilities.ValidateWsrmRequest(this.session, info2, this.binder, context))
                {
                    flag = false;
                }
                else
                {
                    bool flag3 = false;
                    Exception e = null;
                    ReplyHelper closeSequenceReplyHelper = null;
                    bool flag4 = true;
                    bool isLastLargeEnough = true;
                    bool flag6 = true;
                    lock (base.ThisLock)
                    {
                        if (!this.connection.IsLastKnown)
                        {
                            if (this.requestsByRequestSequenceNumber.Count == 0)
                            {
                                if (flag2)
                                {
                                    if (this.connection.SetTerminateSequenceLast(last, out isLastLargeEnough))
                                    {
                                        flag3 = true;
                                    }
                                    else if (isLastLargeEnough)
                                    {
                                        e = new ProtocolException(System.ServiceModel.SR.GetString("EarlyTerminateSequence"));
                                    }
                                }
                                else
                                {
                                    flag3 = this.connection.SetCloseSequenceLast(last);
                                    isLastLargeEnough = flag3;
                                }
                                if (flag3)
                                {
                                    if (!this.CreateCloseSequenceReplyHelper())
                                    {
                                        return;
                                    }
                                    if (flag2)
                                    {
                                        closeSequenceReplyHelper = this.closeSequenceReplyHelper;
                                    }
                                    this.session.SetFinalAck(this.connection.Ranges);
                                    this.deliveryStrategy.Dispose();
                                }
                            }
                            else
                            {
                                flag4 = false;
                            }
                        }
                        else
                        {
                            flag6 = last == this.connection.Last;
                        }
                    }
                    WsrmFault fault = null;
                    if (!isLastLargeEnough)
                    {
                        string faultReason = System.ServiceModel.SR.GetString("SequenceTerminatedSmallLastMsgNumber");
                        string exceptionMessage = System.ServiceModel.SR.GetString("SmallLastMsgNumberExceptionString");
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, faultReason, exceptionMessage);
                    }
                    else if (!flag4)
                    {
                        string str3 = System.ServiceModel.SR.GetString("SequenceTerminatedNotAllRepliesAcknowledged");
                        string str4 = System.ServiceModel.SR.GetString("NotAllRepliesAcknowledgedExceptionString");
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, str3, str4);
                    }
                    else if (!flag6)
                    {
                        string str5 = System.ServiceModel.SR.GetString("SequenceTerminatedInconsistentLastMsgNumber");
                        string str6 = System.ServiceModel.SR.GetString("InconsistentLastMsgNumberExceptionString");
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, str5, str6);
                    }
                    else if (e != null)
                    {
                        Message message = WsrmUtilities.CreateTerminateMessage(this.MessageVersion, this.listener.ReliableMessagingVersion, this.session.OutputID);
                        this.AddAcknowledgementHeader(message);
                        using (message)
                        {
                            context.Reply(message);
                        }
                        this.session.OnRemoteFault(e);
                        return;
                    }
                    if (fault != null)
                    {
                        this.session.OnLocalFault(fault.CreateException(), fault, context);
                        flag = false;
                    }
                    else
                    {
                        if (flag2)
                        {
                            if (closeSequenceReplyHelper != null)
                            {
                                closeSequenceReplyHelper.UnblockWaiter();
                            }
                            lock (base.ThisLock)
                            {
                                if (!this.CreateTerminateSequenceReplyHelper())
                                {
                                    return;
                                }
                            }
                        }
                        ReplyHelper helper2 = flag2 ? this.terminateSequenceReplyHelper : this.closeSequenceReplyHelper;
                        if (!helper2.TransferRequestContext(context, info))
                        {
                            helper2.Reply(context, info, base.DefaultSendTimeout, MaskingMode.All);
                            if (flag2)
                            {
                                this.OnTerminateSequenceCompleted();
                            }
                        }
                        else
                        {
                            flag = false;
                        }
                        if (flag3)
                        {
                            ActionItem.Schedule(new Action<object>(this.ShutdownCallback), null);
                        }
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    context.RequestMessage.Close();
                    context.Close();
                }
            }
        }

        private void ProcessTerminateSequenceFeb2005(RequestContext context, WsrmMessageInfo info)
        {
            bool flag = true;
            try
            {
                bool flag2;
                bool flag3;
                Message message = null;
                lock (base.ThisLock)
                {
                    flag2 = !this.connection.Terminate();
                    flag3 = this.requestsByRequestSequenceNumber.Count == 0;
                }
                WsrmFault fault = null;
                if (flag2)
                {
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedEarlyTerminateSequence"), System.ServiceModel.SR.GetString("EarlyTerminateSequence"));
                }
                else if (!flag3)
                {
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedBeforeReplySequenceAcked"), System.ServiceModel.SR.GetString("EarlyRequestTerminateSequence"));
                }
                if (fault != null)
                {
                    this.session.OnLocalFault(fault.CreateException(), fault, context);
                    flag = false;
                }
                else
                {
                    message = WsrmUtilities.CreateTerminateMessage(this.MessageVersion, this.listener.ReliableMessagingVersion, this.session.OutputID);
                    this.AddAcknowledgementHeader(message);
                    using (message)
                    {
                        context.Reply(message);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    context.RequestMessage.Close();
                    context.Close();
                }
            }
        }

        private void ShutdownCallback(object state)
        {
            base.Shutdown();
        }

        private void StartReceiving(bool canBlock)
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
            lock (base.ThisLock)
            {
                base.ThrowIfClosed();
                this.CreateTerminateSequenceReplyHelper();
            }
            this.terminateSequenceReplyHelper.WaitAndReply(timeout);
            this.OnTerminateSequenceCompleted();
        }

        private void ThrowIfCloseInvalid()
        {
            bool flag = false;
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if ((this.PendingRequestContexts != 0) || (this.connection.Ranges.Count > 1))
                {
                    flag = true;
                }
            }
            else if ((this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && (this.PendingRequestContexts != 0))
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

        private void UnblockClose()
        {
            this.AbortContexts();
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.messagingCompleteWaitObject.Fault(this);
            }
            else
            {
                if (this.closeSequenceReplyHelper != null)
                {
                    this.closeSequenceReplyHelper.Fault();
                }
                if (this.terminateSequenceReplyHelper != null)
                {
                    this.terminateSequenceReplyHelper.Fault();
                }
            }
            this.connection.Fault(this);
        }

        public IServerReliableChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        private bool IsMessagingCompleted
        {
            get
            {
                lock (base.ThisLock)
                {
                    return ((this.connection.AllAdded && (this.requestsByRequestSequenceNumber.Count == 0)) && this.lastReplyAcked);
                }
            }
        }

        private System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.listener.MessageVersion;
            }
        }

        private int PendingRequestContexts
        {
            get
            {
                lock (base.ThisLock)
                {
                    return (this.requestsByRequestSequenceNumber.Count - this.requestsByReplySequenceNumber.Count);
                }
            }
        }

        public IInputSession Session
        {
            get
            {
                return this.session;
            }
        }

        private class CloseOutputCompletedAsyncResult : CompletedAsyncResult
        {
            public CloseOutputCompletedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }
        }

        private class CloseSequenceReplyProvider : ReliableReplySessionChannel.ReplyProvider
        {
            private static ReliableReplySessionChannel.CloseSequenceReplyProvider instance = new ReliableReplySessionChannel.CloseSequenceReplyProvider();

            private CloseSequenceReplyProvider()
            {
            }

            internal override Message Provide(ReliableReplySessionChannel channel, WsrmMessageInfo requestInfo)
            {
                Message message = WsrmUtilities.CreateCloseSequenceResponse(channel.MessageVersion, requestInfo.CloseSequenceInfo.MessageId, channel.session.InputID);
                channel.AddAcknowledgementHeader(message);
                return message;
            }

            internal static ReliableReplySessionChannel.ReplyProvider Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new ReliableReplySessionChannel.CloseSequenceReplyProvider();
                    }
                    return instance;
                }
            }
        }

        private class ReliableRequestContext : RequestContextBase
        {
            private MessageBuffer bufferedReply;
            private ReliableReplySessionChannel channel;
            private List<RequestContext> innerContexts;
            private bool isLastReply;
            private bool outcomeKnown;
            private SequenceRangeCollection ranges;
            private long replySequenceNumber;
            private long requestSequenceNumber;

            public ReliableRequestContext(RequestContext context, long requestSequenceNumber, ReliableReplySessionChannel channel, bool outcome) : base(context.RequestMessage, channel.DefaultCloseTimeout, channel.DefaultSendTimeout)
            {
                this.innerContexts = new List<RequestContext>();
                this.channel = channel;
                this.requestSequenceNumber = requestSequenceNumber;
                this.outcomeKnown = outcome;
                if (!outcome)
                {
                    this.innerContexts.Add(context);
                }
            }

            private void AbortInnerContexts()
            {
                for (int i = 0; i < this.innerContexts.Count; i++)
                {
                    this.innerContexts[i].Abort();
                    this.innerContexts[i].RequestMessage.Close();
                }
                this.innerContexts.Clear();
            }

            internal IAsyncResult BeginReplyInternal(Message reply, TimeSpan timeout, AsyncCallback callback, object state)
            {
                IAsyncResult result2;
                bool flag = true;
                bool flag2 = true;
                try
                {
                    lock (base.ThisLock)
                    {
                        if (this.ranges == null)
                        {
                            throw Fx.AssertAndThrow("this.ranges != null");
                        }
                        if (base.Aborted)
                        {
                            flag = false;
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("RequestContextAborted")));
                        }
                        if (this.outcomeKnown)
                        {
                            flag = false;
                            flag2 = false;
                        }
                        else
                        {
                            if ((reply != null) && (this.bufferedReply == null))
                            {
                                this.bufferedReply = reply.CreateBufferedCopy(0x7fffffff);
                            }
                            if (!this.channel.PrepareReply(this))
                            {
                                flag = false;
                                flag2 = false;
                            }
                            else
                            {
                                this.outcomeKnown = true;
                            }
                        }
                    }
                    if (!flag2)
                    {
                        return new ReplyCompletedAsyncResult(callback, state);
                    }
                    IAsyncResult result = new ReplyAsyncResult(this, timeout, callback, state);
                    flag = false;
                    result2 = result;
                }
                finally
                {
                    if (flag)
                    {
                        this.AbortInnerContexts();
                        this.Abort();
                    }
                }
                return result2;
            }

            public bool CheckForReplyOrAddInnerContext(RequestContext innerContext)
            {
                lock (base.ThisLock)
                {
                    if (this.outcomeKnown)
                    {
                        return true;
                    }
                    this.innerContexts.Add(innerContext);
                    return false;
                }
            }

            internal void EndReplyInternal(IAsyncResult result)
            {
                if (result is ReplyCompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    bool flag = true;
                    try
                    {
                        ReplyAsyncResult.End(result);
                        this.innerContexts.Clear();
                        flag = false;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.AbortInnerContexts();
                            this.Abort();
                        }
                    }
                }
            }

            protected override void OnAbort()
            {
                bool outcomeKnown;
                lock (base.ThisLock)
                {
                    outcomeKnown = this.outcomeKnown;
                    this.outcomeKnown = true;
                }
                if (!outcomeKnown)
                {
                    this.AbortInnerContexts();
                }
                if (this.channel.ContainsRequest(this.requestSequenceNumber))
                {
                    Exception e = new ProtocolException(System.ServiceModel.SR.GetString("ReliableRequestContextAborted"));
                    this.channel.session.OnLocalFault(e, (Message) null, null);
                }
            }

            protected override IAsyncResult OnBeginReply(Message reply, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.BeginReplyInternal(reply, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                if (!base.ReplyInitiated)
                {
                    this.OnReply(null, timeout);
                }
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                this.EndReplyInternal(result);
            }

            protected override void OnReply(Message reply, TimeSpan timeout)
            {
                this.ReplyInternal(reply, timeout);
            }

            internal void ReplyInternal(Message reply, TimeSpan timeout)
            {
                bool flag = true;
                try
                {
                    lock (base.ThisLock)
                    {
                        if (this.ranges == null)
                        {
                            throw Fx.AssertAndThrow("this.ranges != null");
                        }
                        if (base.Aborted)
                        {
                            flag = false;
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("RequestContextAborted")));
                        }
                        if (this.outcomeKnown)
                        {
                            flag = false;
                            return;
                        }
                        if ((reply != null) && (this.bufferedReply == null))
                        {
                            this.bufferedReply = reply.CreateBufferedCopy(0x7fffffff);
                        }
                        if (!this.channel.PrepareReply(this))
                        {
                            flag = false;
                            return;
                        }
                        this.outcomeKnown = true;
                    }
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    for (int i = 0; i < this.innerContexts.Count; i++)
                    {
                        this.SendReply(this.innerContexts[i], MaskingMode.Handled, ref timeoutHelper);
                    }
                    this.innerContexts.Clear();
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        this.AbortInnerContexts();
                        this.Abort();
                    }
                }
            }

            public void SendReply(RequestContext context, MaskingMode maskingMode)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(base.DefaultSendTimeout);
                this.SendReply(context, maskingMode, ref timeoutHelper);
            }

            private void SendReply(RequestContext context, MaskingMode maskingMode, ref TimeoutHelper timeoutHelper)
            {
                Message message;
                if (!this.outcomeKnown)
                {
                    throw Fx.AssertAndThrow("this.outcomeKnown");
                }
                if (this.bufferedReply != null)
                {
                    message = this.bufferedReply.CreateMessage();
                    this.channel.PrepareReplyMessage(this.replySequenceNumber, this.isLastReply, this.ranges, message);
                }
                else
                {
                    message = this.channel.CreateAcknowledgement(this.ranges);
                }
                this.channel.binder.SetMaskingMode(context, maskingMode);
                using (message)
                {
                    context.Reply(message, timeoutHelper.RemainingTime());
                }
                context.Close(timeoutHelper.RemainingTime());
            }

            public void SetAckRanges(SequenceRangeCollection ranges)
            {
                if (this.ranges == null)
                {
                    this.ranges = ranges;
                }
            }

            public void SetLastReply(long sequenceNumber)
            {
                this.replySequenceNumber = sequenceNumber;
                this.isLastReply = true;
                if (this.bufferedReply == null)
                {
                    this.bufferedReply = Message.CreateMessage(this.channel.MessageVersion, "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage").CreateBufferedCopy(0x7fffffff);
                }
            }

            public void SetReplySequenceNumber(long sequenceNumber)
            {
                this.replySequenceNumber = sequenceNumber;
            }

            public bool HasReply
            {
                get
                {
                    return (this.bufferedReply != null);
                }
            }

            public long RequestSequenceNumber
            {
                get
                {
                    return this.requestSequenceNumber;
                }
            }

            private class ReplyAsyncResult : AsyncResult
            {
                private ReliableReplySessionChannel.ReliableRequestContext context;
                private int currentContext;
                private Message reply;
                private static AsyncCallback replyCompleteStatic = Fx.ThunkCallback(new AsyncCallback(ReliableReplySessionChannel.ReliableRequestContext.ReplyAsyncResult.ReplyCompleteStatic));
                private TimeoutHelper timeoutHelper;

                public ReplyAsyncResult(ReliableReplySessionChannel.ReliableRequestContext thisContext, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.context = thisContext;
                    if (this.SendReplies())
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<ReliableReplySessionChannel.ReliableRequestContext.ReplyAsyncResult>(result);
                }

                private void HandleReplyComplete(IAsyncResult result)
                {
                    RequestContext context = this.context.innerContexts[this.currentContext];
                    try
                    {
                        context.EndReply(result);
                        context.Close(this.timeoutHelper.RemainingTime());
                        this.currentContext++;
                    }
                    finally
                    {
                        this.reply.Close();
                        this.reply = null;
                    }
                }

                private static void ReplyCompleteStatic(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        ReliableReplySessionChannel.ReliableRequestContext.ReplyAsyncResult asyncState = null;
                        bool flag = false;
                        try
                        {
                            asyncState = (ReliableReplySessionChannel.ReliableRequestContext.ReplyAsyncResult) result.AsyncState;
                            asyncState.HandleReplyComplete(result);
                            flag = asyncState.SendReplies();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                            flag = true;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private bool SendReplies()
                {
                    while (this.currentContext < this.context.innerContexts.Count)
                    {
                        if (this.context.bufferedReply != null)
                        {
                            this.reply = this.context.bufferedReply.CreateMessage();
                            this.context.channel.PrepareReplyMessage(this.context.replySequenceNumber, this.context.isLastReply, this.context.ranges, this.reply);
                        }
                        else
                        {
                            this.reply = this.context.channel.CreateAcknowledgement(this.context.ranges);
                        }
                        RequestContext context = this.context.innerContexts[this.currentContext];
                        this.context.channel.binder.SetMaskingMode(context, MaskingMode.Handled);
                        IAsyncResult result = context.BeginReply(this.reply, this.timeoutHelper.RemainingTime(), replyCompleteStatic, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.HandleReplyComplete(result);
                    }
                    return true;
                }
            }

            private class ReplyCompletedAsyncResult : CompletedAsyncResult
            {
                public ReplyCompletedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
                {
                }
            }
        }

        private class ReplyHelper
        {
            private Message asyncMessage;
            private bool canTransfer = true;
            private ReliableReplySessionChannel channel;
            private WsrmMessageInfo info;
            private ReliableReplySessionChannel.ReplyProvider replyProvider;
            private RequestContext requestContext;
            private bool throwTimeoutOnWait;
            private InterruptibleWaitObject waitHandle;

            internal ReplyHelper(ReliableReplySessionChannel channel, ReliableReplySessionChannel.ReplyProvider replyProvider, bool throwTimeoutOnWait)
            {
                this.channel = channel;
                this.replyProvider = replyProvider;
                this.throwTimeoutOnWait = throwTimeoutOnWait;
                this.waitHandle = new InterruptibleWaitObject(false, this.throwTimeoutOnWait);
            }

            internal void Abort()
            {
                this.Cleanup(true);
            }

            private IAsyncResult BeginReply(TimeSpan timeout, AsyncCallback callback, object state)
            {
                IAsyncResult result2;
                lock (this.ThisLock)
                {
                    this.canTransfer = false;
                }
                if (this.requestContext == null)
                {
                    return new ReplyCompletedAsyncResult(callback, state);
                }
                this.asyncMessage = this.replyProvider.Provide(this.channel, this.info);
                bool flag = true;
                try
                {
                    this.channel.binder.SetMaskingMode(this.requestContext, MaskingMode.Handled);
                    IAsyncResult result = this.requestContext.BeginReply(this.asyncMessage, timeout, callback, state);
                    flag = false;
                    result2 = result;
                }
                finally
                {
                    if (flag)
                    {
                        this.asyncMessage.Close();
                        this.asyncMessage = null;
                    }
                }
                return result2;
            }

            internal IAsyncResult BeginWaitAndReply(TimeSpan timeout, AsyncCallback callback, object state)
            {
                OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.waitHandle.BeginWait), new OperationWithTimeoutBeginCallback(this.BeginReply) };
                OperationEndCallback[] endOperations = new OperationEndCallback[] { new OperationEndCallback(this.waitHandle.EndWait), new OperationEndCallback(this.EndReply) };
                return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
            }

            private void Cleanup(bool abort)
            {
                lock (this.ThisLock)
                {
                    this.canTransfer = false;
                }
                if (abort)
                {
                    this.waitHandle.Abort(this.channel);
                }
                else
                {
                    this.waitHandle.Fault(this.channel);
                }
            }

            private void EndReply(IAsyncResult result)
            {
                ReplyCompletedAsyncResult result2 = result as ReplyCompletedAsyncResult;
                if (result2 != null)
                {
                    result2.End();
                }
                else
                {
                    try
                    {
                        this.requestContext.EndReply(result);
                    }
                    finally
                    {
                        if (this.asyncMessage != null)
                        {
                            this.asyncMessage.Close();
                        }
                    }
                }
            }

            internal void EndWaitAndReply(IAsyncResult result)
            {
                OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
            }

            internal void Fault()
            {
                this.Cleanup(false);
            }

            internal void Reply(RequestContext context, WsrmMessageInfo info, TimeSpan timeout, MaskingMode maskingMode)
            {
                using (Message message = this.replyProvider.Provide(this.channel, info))
                {
                    this.channel.binder.SetMaskingMode(context, maskingMode);
                    context.Reply(message, timeout);
                }
            }

            internal bool TransferRequestContext(RequestContext requestContext, WsrmMessageInfo info)
            {
                RequestContext context = null;
                WsrmMessageInfo info2 = null;
                lock (this.ThisLock)
                {
                    if (!this.canTransfer)
                    {
                        return false;
                    }
                    context = this.requestContext;
                    info2 = this.info;
                    this.requestContext = requestContext;
                    this.info = info;
                }
                this.waitHandle.Set();
                if (context != null)
                {
                    info2.Message.Close();
                    context.Close();
                }
                return true;
            }

            internal void UnblockWaiter()
            {
                this.TransferRequestContext(null, null);
            }

            internal void WaitAndReply(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.waitHandle.Wait(helper.RemainingTime());
                lock (this.ThisLock)
                {
                    this.canTransfer = false;
                    if (this.requestContext == null)
                    {
                        return;
                    }
                }
                this.Reply(this.requestContext, this.info, helper.RemainingTime(), MaskingMode.Handled);
            }

            private object ThisLock
            {
                get
                {
                    return this.channel.ThisLock;
                }
            }

            private class ReplyCompletedAsyncResult : CompletedAsyncResult
            {
                internal ReplyCompletedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
                {
                }

                public void End()
                {
                    AsyncResult.End<ReliableReplySessionChannel.ReplyHelper.ReplyCompletedAsyncResult>(this);
                }
            }
        }

        private abstract class ReplyProvider
        {
            protected ReplyProvider()
            {
            }

            internal abstract Message Provide(ReliableReplySessionChannel channel, WsrmMessageInfo info);
        }

        private class TerminateSequenceReplyProvider : ReliableReplySessionChannel.ReplyProvider
        {
            private static ReliableReplySessionChannel.TerminateSequenceReplyProvider instance = new ReliableReplySessionChannel.TerminateSequenceReplyProvider();

            private TerminateSequenceReplyProvider()
            {
            }

            internal override Message Provide(ReliableReplySessionChannel channel, WsrmMessageInfo requestInfo)
            {
                Message message = WsrmUtilities.CreateTerminateResponseMessage(channel.MessageVersion, requestInfo.TerminateSequenceInfo.MessageId, channel.session.InputID);
                channel.AddAcknowledgementHeader(message);
                return message;
            }

            internal static ReliableReplySessionChannel.ReplyProvider Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new ReliableReplySessionChannel.TerminateSequenceReplyProvider();
                    }
                    return instance;
                }
            }
        }
    }
}

