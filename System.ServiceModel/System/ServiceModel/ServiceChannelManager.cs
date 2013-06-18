namespace System.ServiceModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal class ServiceChannelManager : LifetimeManager
    {
        private int activityCount;
        private ICommunicationWaiter activityWaiter;
        private int activityWaiterCount;
        private InstanceContextEmptyCallback emptyCallback;
        private IChannel firstIncomingChannel;
        private ChannelCollection incomingChannels;
        private InstanceContext instanceContext;
        private ChannelCollection outgoingChannels;

        public ServiceChannelManager(InstanceContext instanceContext) : this(instanceContext, null)
        {
        }

        public ServiceChannelManager(InstanceContext instanceContext, InstanceContextEmptyCallback emptyCallback) : base(instanceContext.ThisLock)
        {
            this.instanceContext = instanceContext;
            this.emptyCallback = emptyCallback;
        }

        public void AddIncomingChannel(IChannel channel)
        {
            bool flag = false;
            lock (base.ThisLock)
            {
                if (base.State == LifetimeState.Opened)
                {
                    if (this.firstIncomingChannel == null)
                    {
                        if (this.incomingChannels == null)
                        {
                            this.firstIncomingChannel = channel;
                            this.ChannelAdded(channel);
                        }
                        else
                        {
                            if (this.incomingChannels.Contains(channel))
                            {
                                return;
                            }
                            this.incomingChannels.Add(channel);
                        }
                    }
                    else
                    {
                        this.EnsureIncomingChannelCollection();
                        if (this.incomingChannels.Contains(channel))
                        {
                            return;
                        }
                        this.incomingChannels.Add(channel);
                    }
                    flag = true;
                }
            }
            if (!flag)
            {
                channel.Abort();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
            }
        }

        public IAsyncResult BeginCloseInput(TimeSpan timeout, AsyncCallback callback, object state)
        {
            CloseCommunicationAsyncResult result = null;
            lock (base.ThisLock)
            {
                if (this.activityCount > 0)
                {
                    result = new CloseCommunicationAsyncResult(timeout, callback, state, base.ThisLock);
                    ICommunicationWaiter activityWaiter = this.activityWaiter;
                    this.activityWaiter = result;
                    Interlocked.Increment(ref this.activityWaiterCount);
                }
            }
            if (result != null)
            {
                return result;
            }
            return new CompletedAsyncResult(callback, state);
        }

        private void ChannelAdded(IChannel channel)
        {
            base.IncrementBusyCount();
            channel.Closed += new EventHandler(this.OnChannelClosed);
        }

        private void ChannelRemoved(IChannel channel)
        {
            channel.Closed -= new EventHandler(this.OnChannelClosed);
            base.DecrementBusyCount();
        }

        public void CloseInput(TimeSpan timeout)
        {
            SyncCommunicationWaiter waiter = null;
            lock (base.ThisLock)
            {
                if (this.activityCount > 0)
                {
                    waiter = new SyncCommunicationWaiter(base.ThisLock);
                    ICommunicationWaiter activityWaiter = this.activityWaiter;
                    this.activityWaiter = waiter;
                    Interlocked.Increment(ref this.activityWaiterCount);
                }
            }
            if (waiter != null)
            {
                CommunicationWaitResult result = waiter.Wait(timeout, false);
                if (Interlocked.Decrement(ref this.activityWaiterCount) == 0)
                {
                    waiter.Dispose();
                    this.activityWaiter = null;
                }
                switch (result)
                {
                    case CommunicationWaitResult.Expired:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("SfxCloseTimedOutWaitingForDispatchToComplete")));

                    case CommunicationWaitResult.Aborted:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
                }
            }
        }

        public void DecrementActivityCount()
        {
            ICommunicationWaiter activityWaiter = null;
            bool flag = false;
            lock (base.ThisLock)
            {
                int activityCount = this.activityCount;
                if (--this.activityCount == 0)
                {
                    if (this.activityWaiter != null)
                    {
                        activityWaiter = this.activityWaiter;
                        Interlocked.Increment(ref this.activityWaiterCount);
                    }
                    if (base.BusyCount == 0)
                    {
                        flag = true;
                    }
                }
            }
            if (activityWaiter != null)
            {
                activityWaiter.Signal();
                if (Interlocked.Decrement(ref this.activityWaiterCount) == 0)
                {
                    activityWaiter.Dispose();
                    this.activityWaiter = null;
                }
            }
            if (flag && (base.State == LifetimeState.Opened))
            {
                this.OnEmpty();
            }
        }

        public void EndCloseInput(IAsyncResult result)
        {
            if (result is CloseCommunicationAsyncResult)
            {
                CloseCommunicationAsyncResult.End(result);
                if (Interlocked.Decrement(ref this.activityWaiterCount) == 0)
                {
                    this.activityWaiter.Dispose();
                    this.activityWaiter = null;
                }
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        private void EnsureIncomingChannelCollection()
        {
            lock (base.ThisLock)
            {
                if (this.incomingChannels == null)
                {
                    this.incomingChannels = new ChannelCollection(this, base.ThisLock);
                    if (this.firstIncomingChannel != null)
                    {
                        this.incomingChannels.Add(this.firstIncomingChannel);
                        this.ChannelRemoved(this.firstIncomingChannel);
                        this.firstIncomingChannel = null;
                    }
                }
            }
        }

        public void IncrementActivityCount()
        {
            lock (base.ThisLock)
            {
                if (base.State == LifetimeState.Closed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
                }
                this.activityCount++;
            }
        }

        protected override void IncrementBusyCount()
        {
            base.IncrementBusyCount();
        }

        protected override void OnAbort()
        {
            IChannel[] channelArray = this.SnapshotChannels();
            for (int i = 0; i < channelArray.Length; i++)
            {
                channelArray[i].Abort();
            }
            ICommunicationWaiter activityWaiter = null;
            lock (base.ThisLock)
            {
                if (this.activityWaiter != null)
                {
                    activityWaiter = this.activityWaiter;
                    Interlocked.Increment(ref this.activityWaiterCount);
                }
            }
            if (activityWaiter != null)
            {
                activityWaiter.Signal();
                if (Interlocked.Decrement(ref this.activityWaiterCount) == 0)
                {
                    activityWaiter.Dispose();
                    this.activityWaiter = null;
                }
            }
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.BeginCloseInput), new ChainedEndHandler(this.EndCloseInput), new ChainedBeginHandler(this.OnBeginCloseContinue), new ChainedEndHandler(this.OnEndCloseContinue));
        }

        private IAsyncResult OnBeginCloseContinue(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            return base.OnBeginClose(helper.RemainingTime(), callback, state);
        }

        private void OnChannelClosed(object sender, EventArgs args)
        {
            this.RemoveChannel((IChannel) sender);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CloseInput(helper.RemainingTime());
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnEmpty()
        {
            if (this.emptyCallback != null)
            {
                this.emptyCallback(this.instanceContext);
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        private void OnEndCloseContinue(IAsyncResult result)
        {
            base.OnEndClose(result);
        }

        public bool RemoveChannel(IChannel channel)
        {
            lock (base.ThisLock)
            {
                if (this.firstIncomingChannel == channel)
                {
                    this.firstIncomingChannel = null;
                    this.ChannelRemoved(channel);
                    return true;
                }
                if ((this.incomingChannels != null) && this.incomingChannels.Contains(channel))
                {
                    this.incomingChannels.Remove(channel);
                    return true;
                }
                if ((this.outgoingChannels != null) && this.outgoingChannels.Contains(channel))
                {
                    this.outgoingChannels.Remove(channel);
                    return true;
                }
            }
            return false;
        }

        public IChannel[] SnapshotChannels()
        {
            lock (base.ThisLock)
            {
                int num = (this.outgoingChannels != null) ? this.outgoingChannels.Count : 0;
                if (this.firstIncomingChannel != null)
                {
                    IChannel[] array = new IChannel[1 + num];
                    array[0] = this.firstIncomingChannel;
                    if (num > 0)
                    {
                        this.outgoingChannels.CopyTo(array, 1);
                    }
                    return array;
                }
                if (this.incomingChannels != null)
                {
                    IChannel[] channelArray2 = new IChannel[this.incomingChannels.Count + num];
                    this.incomingChannels.CopyTo(channelArray2, 0);
                    if (num > 0)
                    {
                        this.outgoingChannels.CopyTo(channelArray2, this.incomingChannels.Count);
                    }
                    return channelArray2;
                }
                if (num > 0)
                {
                    IChannel[] channelArray3 = new IChannel[num];
                    this.outgoingChannels.CopyTo(channelArray3, 0);
                    return channelArray3;
                }
            }
            return EmptyArray<IChannel>.Allocate(0);
        }

        public int ActivityCount
        {
            get
            {
                return this.activityCount;
            }
        }

        public ICollection<IChannel> IncomingChannels
        {
            get
            {
                this.EnsureIncomingChannelCollection();
                return this.incomingChannels;
            }
        }

        public bool IsBusy
        {
            get
            {
                if (this.ActivityCount > 0)
                {
                    return true;
                }
                if (base.BusyCount > 0)
                {
                    return true;
                }
                ICollection<IChannel> outgoingChannels = this.outgoingChannels;
                return ((outgoingChannels != null) && (outgoingChannels.Count > 0));
            }
        }

        public ICollection<IChannel> OutgoingChannels
        {
            get
            {
                if (this.outgoingChannels == null)
                {
                    lock (base.ThisLock)
                    {
                        if (this.outgoingChannels == null)
                        {
                            this.outgoingChannels = new ChannelCollection(this, base.ThisLock);
                        }
                    }
                }
                return this.outgoingChannels;
            }
        }

        private class ChannelCollection : ICollection<IChannel>, IEnumerable<IChannel>, IEnumerable
        {
            private ServiceChannelManager channelManager;
            private HashSet<IChannel> hashSet = new HashSet<IChannel>();
            private object syncRoot;

            public ChannelCollection(ServiceChannelManager channelManager, object syncRoot)
            {
                if (syncRoot == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
                }
                this.channelManager = channelManager;
                this.syncRoot = syncRoot;
            }

            public void Add(IChannel channel)
            {
                lock (this.syncRoot)
                {
                    if (this.hashSet.Add(channel))
                    {
                        this.channelManager.ChannelAdded(channel);
                    }
                }
            }

            public void Clear()
            {
                lock (this.syncRoot)
                {
                    foreach (IChannel channel in this.hashSet)
                    {
                        this.channelManager.ChannelRemoved(channel);
                    }
                    this.hashSet.Clear();
                }
            }

            public bool Contains(IChannel channel)
            {
                lock (this.syncRoot)
                {
                    return ((channel != null) && this.hashSet.Contains(channel));
                }
            }

            public void CopyTo(IChannel[] array, int arrayIndex)
            {
                lock (this.syncRoot)
                {
                    this.hashSet.CopyTo(array, arrayIndex);
                }
            }

            public bool Remove(IChannel channel)
            {
                lock (this.syncRoot)
                {
                    bool flag = false;
                    if (channel != null)
                    {
                        flag = this.hashSet.Remove(channel);
                        if (flag)
                        {
                            this.channelManager.ChannelRemoved(channel);
                        }
                    }
                    return flag;
                }
            }

            IEnumerator<IChannel> IEnumerable<IChannel>.GetEnumerator()
            {
                lock (this.syncRoot)
                {
                    return this.hashSet.GetEnumerator();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                lock (this.syncRoot)
                {
                    return this.hashSet.GetEnumerator();
                }
            }

            public int Count
            {
                get
                {
                    lock (this.syncRoot)
                    {
                        return this.hashSet.Count;
                    }
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

