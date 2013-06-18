namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;

    internal class ChannelTracker<TChannel, TState> : CommunicationObject where TChannel: IChannel where TState: class
    {
        private EventHandler onInnerChannelClosed;
        private EventHandler onInnerChannelFaulted;
        private Dictionary<TChannel, TState> receivers;

        public ChannelTracker()
        {
            this.receivers = new Dictionary<TChannel, TState>();
            this.onInnerChannelClosed = new EventHandler(this.OnInnerChannelClosed);
            this.onInnerChannelFaulted = new EventHandler(this.OnInnerChannelFaulted);
        }

        public void Add(TChannel channel, TState channelReceiver)
        {
            bool flag = false;
            lock (this.receivers)
            {
                if (base.State != CommunicationState.Opened)
                {
                    flag = true;
                }
                else
                {
                    this.receivers.Add(channel, channelReceiver);
                }
            }
            if (flag)
            {
                channel.Abort();
            }
        }

        private TChannel[] GetChannels()
        {
            lock (this.receivers)
            {
                TChannel[] array = new TChannel[this.receivers.Keys.Count];
                this.receivers.Keys.CopyTo(array, 0);
                this.receivers.Clear();
                return array;
            }
        }

        protected override void OnAbort()
        {
            TChannel[] channels = this.GetChannels();
            for (int i = 0; i < channels.Length; i++)
            {
                channels[i].Abort();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TChannel[] channels = this.GetChannels();
            List<ICommunicationObject> collection = new List<ICommunicationObject>();
            for (int i = 0; i < channels.Length; i++)
            {
                collection.Add(channels[i]);
            }
            return new CloseCollectionAsyncResult(timeout, callback, state, collection);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            TChannel[] channels = this.GetChannels();
            for (int i = 0; i < channels.Length; i++)
            {
                bool flag = false;
                try
                {
                    channels[i].Close(helper.RemainingTime());
                    flag = true;
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (TimeoutException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                finally
                {
                    if (!flag)
                    {
                        channels[i].Abort();
                    }
                }
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseCollectionAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        private void OnInnerChannelClosed(object sender, EventArgs e)
        {
            TChannel channel = (TChannel) sender;
            this.Remove(channel);
            channel.Faulted -= this.onInnerChannelFaulted;
            channel.Closed -= this.onInnerChannelClosed;
        }

        private void OnInnerChannelFaulted(object sender, EventArgs e)
        {
            ((TChannel) sender).Abort();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        public void PrepareChannel(TChannel channel)
        {
            channel.Faulted += this.onInnerChannelFaulted;
            channel.Closed += this.onInnerChannelClosed;
        }

        public bool Remove(TChannel channel)
        {
            lock (this.receivers)
            {
                return this.receivers.Remove(channel);
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return ServiceDefaults.CloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return ServiceDefaults.OpenTimeout;
            }
        }
    }
}

