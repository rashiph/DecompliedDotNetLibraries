namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal sealed class BufferedReceiveManager : IExtension<ServiceHostBase>
    {
        private Dictionary<InstanceKey, List<BufferedReceiveMessageProperty>> bufferedProperties;
        private WorkflowServiceHost host;
        private int initialized;
        private static AsyncCallback onEndAbandon;
        private object thisLock;
        private PendingMessageThrottle throttle;

        public BufferedReceiveManager(int maxPendingMessagesPerChannel)
        {
            this.throttle = new PendingMessageThrottle(maxPendingMessagesPerChannel);
            this.thisLock = new object();
        }

        internal void AbandonBufferedReceives()
        {
            lock (this.thisLock)
            {
                foreach (List<BufferedReceiveMessageProperty> list in this.bufferedProperties.Values)
                {
                    foreach (BufferedReceiveMessageProperty property in list)
                    {
                        PropertyData userState = (PropertyData) property.UserState;
                        AbandonReceiveContext(userState.ReceiveContext);
                        this.throttle.Release(userState.ChannelKey);
                    }
                }
                this.bufferedProperties.Clear();
            }
        }

        public void AbandonBufferedReceives(HashSet<InstanceKey> associatedInstances)
        {
            foreach (InstanceKey key in associatedInstances)
            {
                lock (this.thisLock)
                {
                    if (this.bufferedProperties.ContainsKey(key))
                    {
                        foreach (BufferedReceiveMessageProperty property in this.bufferedProperties[key])
                        {
                            PropertyData userState = (PropertyData) property.UserState;
                            AbandonReceiveContext(userState.ReceiveContext);
                            this.throttle.Release(userState.ChannelKey);
                        }
                        this.bufferedProperties.Remove(key);
                    }
                }
            }
        }

        internal static void AbandonReceiveContext(ReceiveContext receiveContext)
        {
            if (receiveContext != null)
            {
                if (onEndAbandon == null)
                {
                    onEndAbandon = Fx.ThunkCallback(new AsyncCallback(BufferedReceiveManager.OnEndAbandon));
                }
                try
                {
                    IAsyncResult result = receiveContext.BeginAbandon(TimeSpan.MaxValue, onEndAbandon, receiveContext);
                    if (result.CompletedSynchronously)
                    {
                        HandleEndAbandon(result);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    FxTrace.Exception.AsWarning(exception);
                }
            }
        }

        public bool BufferReceive(OperationContext operationContext, ReceiveContext receiveContext, string bookmarkName, BufferedReceiveState state, bool retry)
        {
            bool flag = false;
            BufferedReceiveMessageProperty property = null;
            if (BufferedReceiveMessageProperty.TryGet(operationContext.IncomingMessageProperties, out property))
            {
                CorrelationMessageProperty property = null;
                if (!CorrelationMessageProperty.TryGet(operationContext.IncomingMessageProperties, out property))
                {
                    return flag;
                }
                EventHandler handler = null;
                InstanceKey instanceKey = property.CorrelationKey;
                int channelKey = operationContext.Channel.GetHashCode();
                if (!this.throttle.Acquire(channelKey))
                {
                    return flag;
                }
                try
                {
                    if (!this.UpdateProperty(property, receiveContext, channelKey, bookmarkName, state))
                    {
                        return flag;
                    }
                    if (handler == null)
                    {
                        handler = delegate (object sender, EventArgs e) {
                            lock (this.thisLock)
                            {
                                if (this.bufferedProperties.ContainsKey(instanceKey) && this.bufferedProperties[instanceKey].Remove(property))
                                {
                                    try
                                    {
                                        property.RequestContext.DelayClose(false);
                                        property.RequestContext.Abort();
                                    }
                                    catch (Exception exception)
                                    {
                                        if (Fx.IsFatal(exception))
                                        {
                                            throw;
                                        }
                                    }
                                    this.throttle.Release(channelKey);
                                }
                            }
                        };
                    }
                    receiveContext.Faulted += handler;
                    lock (this.thisLock)
                    {
                        if (receiveContext.State != ReceiveContextState.Received)
                        {
                            return flag;
                        }
                        bool flag2 = false;
                        if (retry)
                        {
                            property.RequestContext.DelayClose(true);
                            property.RegisterForReplay(operationContext);
                            property.ReplayRequest();
                            property.Notification.NotifyInvokeReceived(property.RequestContext.InnerRequestContext);
                            flag2 = true;
                        }
                        else
                        {
                            ReadOnlyCollection<BookmarkInfo> bookmarksForInstance = this.host.DurableInstanceManager.PersistenceProviderDirectory.GetBookmarksForInstance(instanceKey);
                            if (bookmarksForInstance != null)
                            {
                                for (int i = 0; i < bookmarksForInstance.Count; i++)
                                {
                                    BookmarkInfo info = bookmarksForInstance[i];
                                    if (info.BookmarkName == bookmarkName)
                                    {
                                        property.RequestContext.DelayClose(true);
                                        property.RegisterForReplay(operationContext);
                                        property.ReplayRequest();
                                        property.Notification.NotifyInvokeReceived(property.RequestContext.InnerRequestContext);
                                        flag2 = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!flag2)
                        {
                            List<BufferedReceiveMessageProperty> list;
                            if (!this.bufferedProperties.TryGetValue(instanceKey, out list))
                            {
                                list = new List<BufferedReceiveMessageProperty>();
                                this.bufferedProperties.Add(instanceKey, list);
                            }
                            property.RequestContext.DelayClose(true);
                            property.RegisterForReplay(operationContext);
                            list.Add(property);
                        }
                        else
                        {
                            this.throttle.Release(channelKey);
                        }
                        return true;
                    }
                }
                finally
                {
                    if (!flag)
                    {
                        this.throttle.Release(channelKey);
                    }
                }
            }
            return flag;
        }

        private static bool HandleEndAbandon(IAsyncResult result)
        {
            ((ReceiveContext) result.AsyncState).EndAbandon(result);
            return true;
        }

        private void Initialize()
        {
            this.bufferedProperties = new Dictionary<InstanceKey, List<BufferedReceiveMessageProperty>>();
        }

        private static void OnEndAbandon(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                try
                {
                    HandleEndAbandon(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    FxTrace.Exception.AsWarning(exception);
                }
            }
        }

        public void Retry(HashSet<InstanceKey> associatedInstances, ReadOnlyCollection<BookmarkInfo> availableBookmarks)
        {
            List<BookmarkInfo> list = new List<BookmarkInfo>(availableBookmarks);
            foreach (InstanceKey key in associatedInstances)
            {
                lock (this.thisLock)
                {
                    if (this.bufferedProperties.ContainsKey(key))
                    {
                        List<BufferedReceiveMessageProperty> list2 = this.bufferedProperties[key];
                        int index = 0;
                        while ((index < list2.Count) && (list.Count > 0))
                        {
                            BufferedReceiveMessageProperty property = list2[index];
                            int channelKey = 0;
                            bool flag = false;
                            for (int i = 0; i < list.Count; i++)
                            {
                                BookmarkInfo info = list[i];
                                PropertyData userState = (PropertyData) property.UserState;
                                if (info.BookmarkName == userState.BookmarkName)
                                {
                                    list.RemoveAt(i);
                                    channelKey = userState.ChannelKey;
                                    property.ReplayRequest();
                                    property.Notification.NotifyInvokeReceived(property.RequestContext.InnerRequestContext);
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                index++;
                            }
                            else
                            {
                                list2.RemoveAt(index);
                                this.throttle.Release(channelKey);
                            }
                        }
                    }
                }
                if (list.Count == 0)
                {
                    break;
                }
            }
        }

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {
            if (owner == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("owner"));
            }
            if (Interlocked.CompareExchange(ref this.initialized, 1, 0) != 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.BufferedReceiveBehaviorMultipleUse));
            }
            owner.ThrowIfClosedOrOpened();
            this.host = (WorkflowServiceHost) owner;
            this.Initialize();
        }

        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {
        }

        private bool UpdateProperty(BufferedReceiveMessageProperty property, ReceiveContext receiveContext, int channelKey, string bookmarkName, BufferedReceiveState state)
        {
            if (property.UserState == null)
            {
                PropertyData data = new PropertyData {
                    ReceiveContext = receiveContext,
                    ChannelKey = channelKey,
                    BookmarkName = bookmarkName,
                    State = state
                };
                property.UserState = data;
            }
            else
            {
                PropertyData userState = (PropertyData) property.UserState;
                if (userState.State == state)
                {
                    return false;
                }
                userState.State = state;
            }
            return true;
        }

        private class PendingMessageThrottle
        {
            private int maxPendingMessagesPerChannel;
            private Dictionary<int, ThrottleEntry> pendingMessages;
            private int warningRestoreLimit;

            public PendingMessageThrottle(int maxPendingMessagesPerChannel)
            {
                this.maxPendingMessagesPerChannel = maxPendingMessagesPerChannel;
                this.warningRestoreLimit = (int) Math.Floor((double) (0.7 * maxPendingMessagesPerChannel));
                this.pendingMessages = new Dictionary<int, ThrottleEntry>();
            }

            public bool Acquire(int channelKey)
            {
                lock (this.pendingMessages)
                {
                    if (!this.pendingMessages.ContainsKey(channelKey))
                    {
                        this.pendingMessages.Add(channelKey, new ThrottleEntry());
                    }
                    ThrottleEntry entry = this.pendingMessages[channelKey];
                    if (entry.Count < this.maxPendingMessagesPerChannel)
                    {
                        entry.Count++;
                        return true;
                    }
                    if (TD.MaxPendingMessagesPerChannelExceededIsEnabled() && !entry.WarningIssued)
                    {
                        TD.MaxPendingMessagesPerChannelExceeded(this.maxPendingMessagesPerChannel);
                        entry.WarningIssued = true;
                    }
                    return false;
                }
            }

            public void Release(int channelKey)
            {
                lock (this.pendingMessages)
                {
                    ThrottleEntry entry = this.pendingMessages[channelKey];
                    entry.Count--;
                    if (entry.Count == 0)
                    {
                        this.pendingMessages.Remove(channelKey);
                    }
                    else if (entry.Count < this.warningRestoreLimit)
                    {
                        entry.WarningIssued = false;
                    }
                }
            }

            private class ThrottleEntry
            {
                public int Count { get; set; }

                public bool WarningIssued { get; set; }
            }
        }

        private class PropertyData
        {
            public string BookmarkName { get; set; }

            public int ChannelKey { get; set; }

            public System.ServiceModel.Channels.ReceiveContext ReceiveContext { get; set; }

            public BufferedReceiveState State { get; set; }
        }
    }
}

