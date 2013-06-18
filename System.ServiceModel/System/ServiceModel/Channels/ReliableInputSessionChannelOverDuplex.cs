namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class ReliableInputSessionChannelOverDuplex : ReliableInputSessionChannel
    {
        private TimeSpan acknowledgementInterval;
        private bool acknowledgementScheduled;
        private IOThreadTimer acknowledgementTimer;
        private Guard guard;
        private int pendingAcknowledgements;

        public ReliableInputSessionChannelOverDuplex(ReliableChannelListenerBase<IInputSessionChannel> listener, IServerReliableChannelBinder binder, FaultHelper faultHelper, UniqueId inputID) : base(listener, binder, faultHelper, inputID)
        {
            this.guard = new Guard(0x7fffffff);
            this.acknowledgementInterval = listener.AcknowledgementInterval;
            this.acknowledgementTimer = new IOThreadTimer(new Action<object>(this.OnAcknowledgementTimeoutElapsed), null, true);
            base.DeliveryStrategy.DequeueCallback = new Action(this.OnDeliveryStrategyItemDequeued);
            if (binder.HasSession)
            {
                try
                {
                    base.StartReceiving(false);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.ReliableSession.OnUnknownException(exception);
                }
            }
        }

        protected override void AbortGuards()
        {
            this.guard.Abort();
        }

        protected override IAsyncResult BeginCloseGuards(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.guard.BeginClose(timeout, callback, state);
        }

        protected override void CloseGuards(TimeSpan timeout)
        {
            this.guard.Close(timeout);
        }

        protected override void EndCloseGuards(IAsyncResult result)
        {
            this.guard.EndClose(result);
        }

        protected override bool HandleReceiveComplete(IAsyncResult result)
        {
            RequestContext context;
            if (!base.Binder.EndTryReceive(result, out context))
            {
                return true;
            }
            if (context == null)
            {
                bool flag = false;
                lock (base.ThisLock)
                {
                    flag = base.Connection.Terminate();
                }
                if (!flag && (base.Binder.State == CommunicationState.Opened))
                {
                    Exception e = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityClose"));
                    base.ReliableSession.OnLocalFault(e, (Message) null, null);
                }
                return false;
            }
            Message requestMessage = context.RequestMessage;
            context.Close();
            WsrmMessageInfo info = WsrmMessageInfo.Get(base.Listener.MessageVersion, base.Listener.ReliableMessagingVersion, base.Binder.Channel, base.Binder.GetInnerSession(), requestMessage);
            base.StartReceiving(false);
            this.ProcessMessage(info);
            return false;
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
                    using (Message message = base.CreateAcknowledgmentMessage())
                    {
                        base.Binder.Send(message, base.DefaultSendTimeout);
                    }
                }
                finally
                {
                    this.guard.Exit();
                }
            }
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.acknowledgementTimer.Cancel();
        }

        private void OnDeliveryStrategyItemDequeued()
        {
            if (base.AdvertisedZero)
            {
                this.OnAcknowledgementTimeoutElapsed(null);
            }
        }

        protected override void OnQuotaAvailable()
        {
            this.OnAcknowledgementTimeoutElapsed(null);
        }

        public void ProcessDemuxedMessage(WsrmMessageInfo info)
        {
            try
            {
                this.ProcessMessage(info);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.ReliableSession.OnUnknownException(exception);
            }
        }

        private void ProcessMessage(WsrmMessageInfo info)
        {
            bool flag = true;
            try
            {
                if (!base.ReliableSession.ProcessInfo(info, null))
                {
                    flag = false;
                    return;
                }
                if (!base.ReliableSession.VerifySimplexProtocolElements(info, null))
                {
                    flag = false;
                    return;
                }
                base.ReliableSession.OnRemoteActivity(false);
                if (info.CreateSequenceInfo != null)
                {
                    EndpointAddress address;
                    if (WsrmUtilities.ValidateCreateSequence<IInputSessionChannel>(info, base.Listener, base.Binder.Channel, out address))
                    {
                        Message response = WsrmUtilities.CreateCreateSequenceResponse(base.Listener.MessageVersion, base.Listener.ReliableMessagingVersion, false, info.CreateSequenceInfo, base.Listener.Ordered, base.ReliableSession.InputID, address);
                        using (response)
                        {
                            if (base.Binder.AddressResponse(info.Message, response))
                            {
                                base.Binder.Send(response, base.DefaultSendTimeout);
                            }
                            return;
                        }
                    }
                    base.ReliableSession.OnLocalFault(info.FaultException, info.FaultReply, null);
                    return;
                }
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = info.AckRequestedInfo != null;
                bool flag5 = false;
                Message message2 = null;
                WsrmFault fault = null;
                Exception e = null;
                bool flag6 = base.Listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                bool flag7 = base.Listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                if (info.SequencedMessageInfo != null)
                {
                    lock (base.ThisLock)
                    {
                        if (base.Aborted || (base.State == CommunicationState.Faulted))
                        {
                            return;
                        }
                        long sequenceNumber = info.SequencedMessageInfo.SequenceNumber;
                        bool isLast = flag6 && info.SequencedMessageInfo.LastMessage;
                        if (!base.Connection.IsValid(sequenceNumber, isLast))
                        {
                            if (flag6)
                            {
                                fault = new LastMessageNumberExceededFault(base.ReliableSession.InputID);
                            }
                            else
                            {
                                message2 = new SequenceClosedFault(base.ReliableSession.InputID).CreateMessage(base.Listener.MessageVersion, base.Listener.ReliableMessagingVersion);
                                flag4 = true;
                                if (PerformanceCounters.PerformanceCountersEnabled)
                                {
                                    PerformanceCounters.MessageDropped(base.perfCounterId);
                                }
                            }
                        }
                        else if (base.Connection.Ranges.Contains(sequenceNumber))
                        {
                            if (PerformanceCounters.PerformanceCountersEnabled)
                            {
                                PerformanceCounters.MessageDropped(base.perfCounterId);
                            }
                            flag4 = true;
                        }
                        else if (flag6 && (info.Action == "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage"))
                        {
                            base.Connection.Merge(sequenceNumber, isLast);
                            if (base.Connection.AllAdded)
                            {
                                flag3 = true;
                                base.ReliableSession.CloseSession();
                            }
                        }
                        else if (base.State == CommunicationState.Closing)
                        {
                            if (flag6)
                            {
                                fault = SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedSessionClosedBeforeDone"), System.ServiceModel.SR.GetString("SessionClosedBeforeDone"));
                            }
                            else
                            {
                                message2 = new SequenceClosedFault(base.ReliableSession.InputID).CreateMessage(base.Listener.MessageVersion, base.Listener.ReliableMessagingVersion);
                                flag4 = true;
                                if (PerformanceCounters.PerformanceCountersEnabled)
                                {
                                    PerformanceCounters.MessageDropped(base.perfCounterId);
                                }
                            }
                        }
                        else if (base.DeliveryStrategy.CanEnqueue(sequenceNumber) && (base.Listener.Ordered || base.Connection.CanMerge(sequenceNumber)))
                        {
                            base.Connection.Merge(sequenceNumber, isLast);
                            flag2 = base.DeliveryStrategy.Enqueue(info.Message, sequenceNumber);
                            flag = false;
                            this.pendingAcknowledgements++;
                            if (this.pendingAcknowledgements == base.Listener.MaxTransferWindowSize)
                            {
                                flag4 = true;
                            }
                            if (base.Connection.AllAdded)
                            {
                                flag3 = true;
                                base.ReliableSession.CloseSession();
                            }
                        }
                        else if (PerformanceCounters.PerformanceCountersEnabled)
                        {
                            PerformanceCounters.MessageDropped(base.perfCounterId);
                        }
                        if (base.Connection.IsLastKnown)
                        {
                            flag4 = true;
                        }
                        if ((!flag4 && (this.pendingAcknowledgements > 0)) && (!this.acknowledgementScheduled && (fault == null)))
                        {
                            this.acknowledgementScheduled = true;
                            this.acknowledgementTimer.Set(this.acknowledgementInterval);
                        }
                        goto Label_0643;
                    }
                }
                if (flag6 && (info.TerminateSequenceInfo != null))
                {
                    bool flag10;
                    lock (base.ThisLock)
                    {
                        flag10 = !base.Connection.Terminate();
                    }
                    if (flag10)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedEarlyTerminateSequence"), System.ServiceModel.SR.GetString("EarlyTerminateSequence"));
                    }
                }
                else if (flag7 && ((info.TerminateSequenceInfo != null) || (info.CloseSequenceInfo != null)))
                {
                    bool flag12 = info.TerminateSequenceInfo != null;
                    WsrmRequestInfo info2 = flag12 ? ((WsrmRequestInfo) info.TerminateSequenceInfo) : ((WsrmRequestInfo) info.CloseSequenceInfo);
                    long last = flag12 ? info.TerminateSequenceInfo.LastMsgNumber : info.CloseSequenceInfo.LastMsgNumber;
                    if (!WsrmUtilities.ValidateWsrmRequest(base.ReliableSession, info2, base.Binder, null))
                    {
                        return;
                    }
                    bool isLastLargeEnough = true;
                    bool flag14 = true;
                    lock (base.ThisLock)
                    {
                        if (!base.Connection.IsLastKnown)
                        {
                            if (flag12)
                            {
                                if (base.Connection.SetTerminateSequenceLast(last, out isLastLargeEnough))
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
                                flag3 = base.Connection.SetCloseSequenceLast(last);
                                isLastLargeEnough = flag3;
                            }
                            if (flag3)
                            {
                                base.ReliableSession.SetFinalAck(base.Connection.Ranges);
                                base.DeliveryStrategy.Dispose();
                            }
                        }
                        else
                        {
                            flag14 = last == base.Connection.Last;
                            if ((flag12 && flag14) && base.Connection.IsSequenceClosed)
                            {
                                flag5 = true;
                            }
                        }
                    }
                    if (!isLastLargeEnough)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedSmallLastMsgNumber"), System.ServiceModel.SR.GetString("SmallLastMsgNumberExceptionString"));
                    }
                    else if (!flag14)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedInconsistentLastMsgNumber"), System.ServiceModel.SR.GetString("InconsistentLastMsgNumberExceptionString"));
                    }
                    else
                    {
                        message2 = flag12 ? WsrmUtilities.CreateTerminateResponseMessage(base.Listener.MessageVersion, info2.MessageId, base.ReliableSession.InputID) : WsrmUtilities.CreateCloseSequenceResponse(base.Listener.MessageVersion, info2.MessageId, base.ReliableSession.InputID);
                        flag4 = true;
                    }
                }
            Label_0643:
                if (fault != null)
                {
                    base.ReliableSession.OnLocalFault(fault.CreateException(), fault, null);
                }
                else
                {
                    if (flag4)
                    {
                        lock (base.ThisLock)
                        {
                            if (this.acknowledgementScheduled)
                            {
                                this.acknowledgementTimer.Cancel();
                                this.acknowledgementScheduled = false;
                            }
                            this.pendingAcknowledgements = 0;
                        }
                        if (message2 != null)
                        {
                            base.AddAcknowledgementHeader(message2);
                        }
                        else
                        {
                            message2 = base.CreateAcknowledgmentMessage();
                        }
                    }
                    if (message2 != null)
                    {
                        using (message2)
                        {
                            if (this.guard.Enter())
                            {
                                try
                                {
                                    base.Binder.Send(message2, base.DefaultSendTimeout);
                                }
                                finally
                                {
                                    this.guard.Exit();
                                }
                            }
                        }
                    }
                    if (flag5)
                    {
                        lock (base.ThisLock)
                        {
                            base.Connection.Terminate();
                        }
                    }
                    if (e != null)
                    {
                        base.ReliableSession.OnRemoteFault(e);
                    }
                    else
                    {
                        if (flag2)
                        {
                            base.Dispatch();
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
                    info.Message.Close();
                }
            }
        }
    }
}

