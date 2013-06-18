namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class ReliableInputSessionChannelOverReply : ReliableInputSessionChannel
    {
        public ReliableInputSessionChannelOverReply(ReliableChannelListenerBase<IInputSessionChannel> listener, IServerReliableChannelBinder binder, FaultHelper faultHelper, UniqueId inputID) : base(listener, binder, faultHelper, inputID)
        {
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

        protected override bool HandleReceiveComplete(IAsyncResult result)
        {
            RequestContext context;
            if (!base.Binder.EndTryReceive(result, out context))
            {
                return true;
            }
            if (context == null)
            {
                bool flag2 = false;
                lock (base.ThisLock)
                {
                    flag2 = base.Connection.Terminate();
                }
                if (!flag2 && (base.Binder.State == CommunicationState.Opened))
                {
                    Exception e = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityClose"));
                    base.ReliableSession.OnLocalFault(e, (Message) null, null);
                }
                return false;
            }
            WsrmMessageInfo info = WsrmMessageInfo.Get(base.Listener.MessageVersion, base.Listener.ReliableMessagingVersion, base.Binder.Channel, base.Binder.GetInnerSession(), context.RequestMessage);
            base.StartReceiving(false);
            this.ProcessRequest(context, info);
            return false;
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
                base.ReliableSession.OnUnknownException(exception);
            }
        }

        private void ProcessRequest(RequestContext context, WsrmMessageInfo info)
        {
            bool flag = true;
            bool flag2 = true;
            try
            {
                EndpointAddress address;
                bool flag3;
                if (!base.ReliableSession.ProcessInfo(info, context))
                {
                    flag = false;
                    flag2 = false;
                    return;
                }
                if (!base.ReliableSession.VerifySimplexProtocolElements(info, context))
                {
                    flag = false;
                    flag2 = false;
                    return;
                }
                base.ReliableSession.OnRemoteActivity(false);
                if (info.CreateSequenceInfo == null)
                {
                    goto Label_0104;
                }
                if (WsrmUtilities.ValidateCreateSequence<IInputSessionChannel>(info, base.Listener, base.Binder.Channel, out address))
                {
                    Message response = WsrmUtilities.CreateCreateSequenceResponse(base.Listener.MessageVersion, base.Listener.ReliableMessagingVersion, false, info.CreateSequenceInfo, base.Listener.Ordered, base.ReliableSession.InputID, address);
                    using (context)
                    {
                        using (response)
                        {
                            if (base.Binder.AddressResponse(info.Message, response))
                            {
                                context.Reply(response, base.DefaultSendTimeout);
                            }
                        }
                        goto Label_00FB;
                    }
                }
                base.ReliableSession.OnLocalFault(info.FaultException, info.FaultReply, context);
            Label_00FB:
                flag = false;
                flag2 = false;
                return;
            Label_0104:
                flag3 = false;
                bool allAdded = false;
                bool flag5 = false;
                WsrmFault fault = null;
                Message message2 = null;
                Exception e = null;
                bool flag6 = base.Listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                bool flag7 = base.Listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                bool flag8 = info.AckRequestedInfo != null;
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
                        }
                        else if (flag6 && (info.Action == "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage"))
                        {
                            base.Connection.Merge(sequenceNumber, isLast);
                            allAdded = base.Connection.AllAdded;
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
                                if (PerformanceCounters.PerformanceCountersEnabled)
                                {
                                    PerformanceCounters.MessageDropped(base.perfCounterId);
                                }
                            }
                        }
                        else if (base.DeliveryStrategy.CanEnqueue(sequenceNumber) && (base.Listener.Ordered || base.Connection.CanMerge(sequenceNumber)))
                        {
                            base.Connection.Merge(sequenceNumber, isLast);
                            flag3 = base.DeliveryStrategy.Enqueue(info.Message, sequenceNumber);
                            allAdded = base.Connection.AllAdded;
                            flag2 = false;
                        }
                        else if (PerformanceCounters.PerformanceCountersEnabled)
                        {
                            PerformanceCounters.MessageDropped(base.perfCounterId);
                        }
                        goto Label_05CE;
                    }
                }
                if (flag6 && (info.TerminateSequenceInfo != null))
                {
                    bool flag11;
                    lock (base.ThisLock)
                    {
                        flag11 = !base.Connection.Terminate();
                    }
                    if (!flag11)
                    {
                        return;
                    }
                    fault = SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedEarlyTerminateSequence"), System.ServiceModel.SR.GetString("EarlyTerminateSequence"));
                }
                else if (flag7 && ((info.TerminateSequenceInfo != null) || (info.CloseSequenceInfo != null)))
                {
                    bool flag13 = info.TerminateSequenceInfo != null;
                    WsrmRequestInfo info2 = flag13 ? ((WsrmRequestInfo) info.TerminateSequenceInfo) : ((WsrmRequestInfo) info.CloseSequenceInfo);
                    long last = flag13 ? info.TerminateSequenceInfo.LastMsgNumber : info.CloseSequenceInfo.LastMsgNumber;
                    if (!WsrmUtilities.ValidateWsrmRequest(base.ReliableSession, info2, base.Binder, context))
                    {
                        flag2 = false;
                        flag = false;
                        return;
                    }
                    bool isLastLargeEnough = true;
                    bool flag15 = true;
                    lock (base.ThisLock)
                    {
                        if (!base.Connection.IsLastKnown)
                        {
                            if (flag13)
                            {
                                if (base.Connection.SetTerminateSequenceLast(last, out isLastLargeEnough))
                                {
                                    allAdded = true;
                                }
                                else if (isLastLargeEnough)
                                {
                                    e = new ProtocolException(System.ServiceModel.SR.GetString("EarlyTerminateSequence"));
                                }
                            }
                            else
                            {
                                allAdded = base.Connection.SetCloseSequenceLast(last);
                                isLastLargeEnough = allAdded;
                            }
                            if (allAdded)
                            {
                                base.ReliableSession.SetFinalAck(base.Connection.Ranges);
                                base.DeliveryStrategy.Dispose();
                            }
                        }
                        else
                        {
                            flag15 = last == base.Connection.Last;
                            if ((flag13 && flag15) && base.Connection.IsSequenceClosed)
                            {
                                flag5 = true;
                            }
                        }
                    }
                    if (!isLastLargeEnough)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedSmallLastMsgNumber"), System.ServiceModel.SR.GetString("SmallLastMsgNumberExceptionString"));
                    }
                    else if (!flag15)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedInconsistentLastMsgNumber"), System.ServiceModel.SR.GetString("InconsistentLastMsgNumberExceptionString"));
                    }
                    else
                    {
                        message2 = flag13 ? WsrmUtilities.CreateTerminateResponseMessage(base.Listener.MessageVersion, info2.MessageId, base.ReliableSession.InputID) : WsrmUtilities.CreateCloseSequenceResponse(base.Listener.MessageVersion, info2.MessageId, base.ReliableSession.InputID);
                        flag8 = true;
                    }
                }
            Label_05CE:
                if (fault != null)
                {
                    base.ReliableSession.OnLocalFault(fault.CreateException(), fault, context);
                    flag2 = false;
                    flag = false;
                }
                else
                {
                    if ((message2 != null) && flag8)
                    {
                        base.AddAcknowledgementHeader(message2);
                    }
                    else if (message2 == null)
                    {
                        message2 = base.CreateAcknowledgmentMessage();
                    }
                    using (message2)
                    {
                        context.Reply(message2);
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
                        if (flag3)
                        {
                            base.Dispatch();
                        }
                        if (allAdded)
                        {
                            ActionItem.Schedule(new Action<object>(this.ShutdownCallback), null);
                        }
                    }
                }
            }
            finally
            {
                if (flag2)
                {
                    info.Message.Close();
                }
                if (flag)
                {
                    context.Close();
                }
            }
        }
    }
}

