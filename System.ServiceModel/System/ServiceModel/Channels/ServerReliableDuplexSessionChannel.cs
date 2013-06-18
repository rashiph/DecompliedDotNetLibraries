namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class ServerReliableDuplexSessionChannel : ReliableDuplexSessionChannel
    {
        private ReliableChannelListenerBase<IDuplexSessionChannel> listener;
        private string perfCounterId;

        public ServerReliableDuplexSessionChannel(ReliableChannelListenerBase<IDuplexSessionChannel> listener, IReliableChannelBinder binder, FaultHelper faultHelper, UniqueId inputID, UniqueId outputID) : base(listener, listener, binder)
        {
            this.listener = listener;
            DuplexServerReliableSession session = new DuplexServerReliableSession(this, listener, faultHelper, inputID, outputID);
            base.SetSession(session);
            session.Open(TimeSpan.Zero);
            base.SetConnections();
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                this.perfCounterId = this.listener.Uri.ToString().ToUpperInvariant();
            }
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

        private IAsyncResult BeginUnregisterChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.listener.OnReliableChannelBeginClose(base.ReliableSession.InputID, base.ReliableSession.OutputID, timeout, callback, state);
        }

        private void EndUnregisterChannel(IAsyncResult result)
        {
            this.listener.OnReliableChannelEndClose(result);
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.listener.OnReliableChannelAbort(base.ReliableSession.InputID, base.ReliableSession.OutputID);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.OnBeginClose), new OperationWithTimeoutBeginCallback(this.BeginUnregisterChannel) };
            OperationEndCallback[] endOperations = new OperationEndCallback[] { new OperationEndCallback(this.OnEndClose), new OperationEndCallback(this.EndUnregisterChannel) };
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnClose(helper.RemainingTime());
            this.listener.OnReliableChannelClose(base.ReliableSession.InputID, base.ReliableSession.OutputID, helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.SessionFaulted(this.perfCounterId);
            }
        }

        protected override void OnMessageDropped()
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MessageDropped(this.perfCounterId);
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
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

        protected override void ProcessMessage(WsrmMessageInfo info)
        {
            if (base.ReliableSession.ProcessInfo(info, null) && base.ReliableSession.VerifyDuplexProtocolElements(info, null))
            {
                if (info.CreateSequenceInfo == null)
                {
                    base.ProcessDuplexMessage(info);
                }
                else
                {
                    EndpointAddress address;
                    if (WsrmUtilities.ValidateCreateSequence<IDuplexSessionChannel>(info, this.listener, base.Binder.Channel, out address))
                    {
                        Message response = WsrmUtilities.CreateCreateSequenceResponse(base.Settings.MessageVersion, base.Settings.ReliableMessagingVersion, true, info.CreateSequenceInfo, base.Settings.Ordered, base.ReliableSession.InputID, address);
                        using (info.Message)
                        {
                            using (response)
                            {
                                if (((IServerReliableChannelBinder) base.Binder).AddressResponse(info.Message, response))
                                {
                                    base.Binder.Send(response, base.DefaultSendTimeout);
                                }
                            }
                            return;
                        }
                    }
                    base.ReliableSession.OnLocalFault(info.FaultException, info.FaultReply, null);
                }
            }
        }

        private class DuplexServerReliableSession : ServerReliableSession, IDuplexSession, IInputSession, IOutputSession, ISession
        {
            private ServerReliableDuplexSessionChannel channel;

            public DuplexServerReliableSession(ServerReliableDuplexSessionChannel channel, ReliableChannelListenerBase<IDuplexSessionChannel> listener, FaultHelper faultHelper, UniqueId inputID, UniqueId outputID) : base(channel, listener, (IServerReliableChannelBinder) channel.Binder, faultHelper, inputID, outputID)
            {
                this.channel = channel;
            }

            public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
            {
                return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
            }

            public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.channel.OnBeginCloseOutputSession(timeout, callback, state);
            }

            public void CloseOutputSession()
            {
                this.CloseOutputSession(this.channel.DefaultCloseTimeout);
            }

            public void CloseOutputSession(TimeSpan timeout)
            {
                this.channel.OnCloseOutputSession(timeout);
            }

            public void EndCloseOutputSession(IAsyncResult result)
            {
                this.channel.OnEndCloseOutputSession(result);
            }
        }
    }
}

