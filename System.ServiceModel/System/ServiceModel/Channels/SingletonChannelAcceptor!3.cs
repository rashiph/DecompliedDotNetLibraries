namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType> : InputQueueChannelAcceptor<ChannelInterfaceType> where ChannelInterfaceType: class, IChannel where TChannel: InputQueueChannel<QueueItemType> where QueueItemType: class, IDisposable
    {
        private TChannel currentChannel;
        private object currentChannelLock;
        private static Action<object> onInvokeDequeuedCallback;

        public SingletonChannelAcceptor(ChannelManagerBase channelManager) : base(channelManager)
        {
            this.currentChannelLock = new object();
        }

        public override ChannelInterfaceType AcceptChannel(TimeSpan timeout)
        {
            this.EnsureChannelAvailable();
            return base.AcceptChannel(timeout);
        }

        public override IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.EnsureChannelAvailable();
            return base.BeginAcceptChannel(timeout, callback, state);
        }

        public void DispatchItems()
        {
            TChannel local = this.EnsureChannelAvailable();
            if (local != null)
            {
                local.Dispatch();
            }
        }

        public void Enqueue(QueueItemType item)
        {
            this.Enqueue(item, null);
        }

        public void Enqueue(QueueItemType item, Action dequeuedCallback)
        {
            this.Enqueue(item, dequeuedCallback, true);
        }

        public void Enqueue(Exception exception, Action dequeuedCallback)
        {
            this.Enqueue(exception, dequeuedCallback, true);
        }

        public void Enqueue(QueueItemType item, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            TChannel local = this.EnsureChannelAvailable();
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                this.OnTraceMessageReceived(item);
            }
            if (local != null)
            {
                local.EnqueueAndDispatch(item, dequeuedCallback, canDispatchOnThisThread);
            }
            else
            {
                SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.InvokeDequeuedCallback(dequeuedCallback, canDispatchOnThisThread);
                item.Dispose();
            }
        }

        public void Enqueue(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            TChannel local = this.EnsureChannelAvailable();
            if (local != null)
            {
                local.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
            }
            else
            {
                SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.InvokeDequeuedCallback(dequeuedCallback, canDispatchOnThisThread);
            }
        }

        public void EnqueueAndDispatch(QueueItemType item, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            TChannel local = this.EnsureChannelAvailable();
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                this.OnTraceMessageReceived(item);
            }
            if (local != null)
            {
                local.EnqueueAndDispatch(item, dequeuedCallback, canDispatchOnThisThread);
            }
            else
            {
                SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.InvokeDequeuedCallback(dequeuedCallback, canDispatchOnThisThread);
                item.Dispose();
            }
        }

        public override void EnqueueAndDispatch(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            TChannel local = this.EnsureChannelAvailable();
            if (local != null)
            {
                local.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
            }
            else
            {
                SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.InvokeDequeuedCallback(dequeuedCallback, canDispatchOnThisThread);
            }
        }

        public bool EnqueueWithoutDispatch(QueueItemType item, Action dequeuedCallback)
        {
            TChannel local = this.EnsureChannelAvailable();
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                this.OnTraceMessageReceived(item);
            }
            if (local != null)
            {
                return local.EnqueueWithoutDispatch(item, dequeuedCallback);
            }
            SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.InvokeDequeuedCallback(dequeuedCallback, false);
            item.Dispose();
            return false;
        }

        public override bool EnqueueWithoutDispatch(Exception exception, Action dequeuedCallback)
        {
            TChannel local = this.EnsureChannelAvailable();
            if (local != null)
            {
                return local.EnqueueWithoutDispatch(exception, dequeuedCallback);
            }
            SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.InvokeDequeuedCallback(dequeuedCallback, false);
            return false;
        }

        private TChannel EnsureChannelAvailable()
        {
            bool flag = false;
            TChannel currentChannel = this.currentChannel;
            if (currentChannel == null)
            {
                lock (this.currentChannelLock)
                {
                    if (base.IsDisposed)
                    {
                        return default(TChannel);
                    }
                    currentChannel = this.currentChannel;
                    if (currentChannel == null)
                    {
                        currentChannel = this.OnCreateChannel();
                        currentChannel.Closed += new EventHandler(this.OnChannelClosed);
                        this.currentChannel = currentChannel;
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                base.EnqueueAndDispatch((ChannelInterfaceType) currentChannel);
            }
            return currentChannel;
        }

        protected TChannel GetCurrentChannel()
        {
            return this.currentChannel;
        }

        private static void InvokeDequeuedCallback(Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            if (dequeuedCallback != null)
            {
                if (canDispatchOnThisThread)
                {
                    dequeuedCallback();
                }
                else
                {
                    if (SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.onInvokeDequeuedCallback == null)
                    {
                        SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.onInvokeDequeuedCallback = new Action<object>(SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.OnInvokeDequeuedCallback);
                    }
                    ActionItem.Schedule(SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>.onInvokeDequeuedCallback, dequeuedCallback);
                }
            }
        }

        protected void OnChannelClosed(object sender, EventArgs args)
        {
            IChannel channel = (IChannel) sender;
            lock (this.currentChannelLock)
            {
                if (channel == this.currentChannel)
                {
                    this.currentChannel = default(TChannel);
                }
            }
        }

        protected abstract TChannel OnCreateChannel();
        private static void OnInvokeDequeuedCallback(object state)
        {
            Action action = (Action) state;
            action();
        }

        protected abstract void OnTraceMessageReceived(QueueItemType item);
    }
}

