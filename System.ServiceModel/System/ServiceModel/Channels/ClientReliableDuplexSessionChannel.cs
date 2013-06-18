namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;
    using System.Xml;

    internal class ClientReliableDuplexSessionChannel : ReliableDuplexSessionChannel
    {
        private ChannelParameterCollection channelParameters;
        private DuplexClientReliableSession clientSession;
        private TimeoutHelper closeTimeoutHelper;
        private bool closing;
        private static AsyncCallback onReconnectComplete = Fx.ThunkCallback(new AsyncCallback(ClientReliableDuplexSessionChannel.OnReconnectComplete));
        private static Action<object> onReconnectTimerElapsed = new Action<object>(ClientReliableDuplexSessionChannel.OnReconnectTimerElapsed);

        public ClientReliableDuplexSessionChannel(ChannelManagerBase factory, IReliableFactorySettings settings, IReliableChannelBinder binder, FaultHelper faultHelper, LateBoundChannelParameterCollection channelParameters, UniqueId inputID) : base(factory, settings, binder)
        {
            this.clientSession = new DuplexClientReliableSession(this, settings, faultHelper, inputID);
            this.clientSession.PollingCallback = new ClientReliableSession.PollingHandler(this.PollingCallback);
            base.SetSession(this.clientSession);
            this.channelParameters = channelParameters;
            channelParameters.SetChannel(this);
            ((IClientReliableChannelBinder) binder).ConnectionLost += new EventHandler(this.OnConnectionLost);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(ChannelParameterCollection))
            {
                return (T) this.channelParameters;
            }
            return base.GetProperty<T>();
        }

        private void HandleReconnectComplete(IAsyncResult result)
        {
            bool flag = true;
            try
            {
                base.Binder.EndSend(result);
                flag = false;
                lock (base.ThisLock)
                {
                    if (base.Binder.Connected)
                    {
                        this.clientSession.ResumePolling(base.OutputConnection.Strategy.QuotaRemaining == 0);
                    }
                    else
                    {
                        this.WaitForReconnect();
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!flag)
                {
                    throw;
                }
                this.WaitForReconnect();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.closeTimeoutHelper = new TimeoutHelper(timeout);
            this.closing = true;
            return base.OnBeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReliableChannelOpenAsyncResult(base.Binder, base.ReliableSession, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.closeTimeoutHelper = new TimeoutHelper(timeout);
            this.closing = true;
            base.OnClose(timeout);
        }

        private void OnConnectionLost(object sender, EventArgs args)
        {
            lock (base.ThisLock)
            {
                if (((base.State == CommunicationState.Opened) || (base.State == CommunicationState.Closing)) && (!base.Binder.Connected && this.clientSession.StopPolling()))
                {
                    this.Reconnect();
                }
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ReliableChannelOpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            bool flag = true;
            try
            {
                base.Binder.Open(helper.RemainingTime());
                base.ReliableSession.Open(helper.RemainingTime());
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    base.Binder.Close(helper.RemainingTime());
                }
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            base.SetConnections();
            if (Thread.CurrentThread.IsThreadPoolThread)
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
            else
            {
                ActionItem.Schedule(new Action<object>(ClientReliableDuplexSessionChannel.StartReceivingStatic), this);
            }
        }

        private static void OnReconnectComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((ClientReliableDuplexSessionChannel) result.AsyncState).HandleReconnectComplete(result);
            }
        }

        private static void OnReconnectTimerElapsed(object state)
        {
            ClientReliableDuplexSessionChannel channel = (ClientReliableDuplexSessionChannel) state;
            lock (channel.ThisLock)
            {
                if (((channel.State == CommunicationState.Opened) || (channel.State == CommunicationState.Closing)) && !channel.Binder.Connected)
                {
                    channel.Reconnect();
                }
                else
                {
                    channel.clientSession.ResumePolling(channel.OutputConnection.Strategy.QuotaRemaining == 0);
                }
            }
        }

        protected override void OnRemoteActivity()
        {
            base.ReliableSession.OnRemoteActivity(base.OutputConnection.Strategy.QuotaRemaining == 0);
        }

        private void PollingCallback()
        {
            using (Message message = WsrmUtilities.CreateAckRequestedMessage(base.Settings.MessageVersion, base.Settings.ReliableMessagingVersion, base.ReliableSession.OutputID))
            {
                base.Binder.Send(message, base.DefaultSendTimeout);
            }
        }

        protected override void ProcessMessage(WsrmMessageInfo info)
        {
            if (base.ReliableSession.ProcessInfo(info, null) && base.ReliableSession.VerifyDuplexProtocolElements(info, null))
            {
                base.ProcessDuplexMessage(info);
            }
        }

        private void Reconnect()
        {
            bool flag = true;
            try
            {
                Message message = WsrmUtilities.CreateAckRequestedMessage(base.Settings.MessageVersion, base.Settings.ReliableMessagingVersion, base.ReliableSession.OutputID);
                TimeSpan timeout = this.closing ? this.closeTimeoutHelper.RemainingTime() : this.DefaultCloseTimeout;
                IAsyncResult result = base.Binder.BeginSend(message, timeout, onReconnectComplete, this);
                flag = false;
                if (result.CompletedSynchronously)
                {
                    this.HandleReconnectComplete(result);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!flag)
                {
                    throw;
                }
                this.WaitForReconnect();
            }
        }

        private static void StartReceivingStatic(object state)
        {
            ClientReliableDuplexSessionChannel channel = (ClientReliableDuplexSessionChannel) state;
            try
            {
                channel.StartReceiving(true);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                channel.ReliableSession.OnUnknownException(exception);
            }
        }

        private void WaitForReconnect()
        {
            TimeSpan span;
            if (this.closing)
            {
                span = TimeoutHelper.Divide(this.closeTimeoutHelper.RemainingTime(), 2);
            }
            else
            {
                span = TimeoutHelper.Divide(base.DefaultSendTimeout, 2);
            }
            new IOThreadTimer(onReconnectTimerElapsed, this, false).Set(span);
        }

        private class DuplexClientReliableSession : ClientReliableSession, IDuplexSession, IInputSession, IOutputSession, ISession
        {
            private ClientReliableDuplexSessionChannel channel;

            public DuplexClientReliableSession(ClientReliableDuplexSessionChannel channel, IReliableFactorySettings settings, FaultHelper helper, UniqueId inputID) : base(channel, settings, (IClientReliableChannelBinder) channel.Binder, helper, inputID)
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

