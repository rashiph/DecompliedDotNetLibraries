namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    internal class DatagramAdapter
    {
        internal static IChannelListener<IInputChannel> GetInputListener(IChannelListener<IInputSessionChannel> inner, ServiceThrottle throttle, IDefaultCommunicationTimeouts timeouts)
        {
            return new InputDatagramAdapterListener(inner, throttle, timeouts);
        }

        internal static IOutputChannel GetOutputChannel(Source<IOutputSessionChannel> channelSource, IDefaultCommunicationTimeouts timeouts)
        {
            return new OutputDatagramAdapterChannel(channelSource, timeouts);
        }

        internal static IChannelListener<IReplyChannel> GetReplyListener(IChannelListener<IReplySessionChannel> inner, ServiceThrottle throttle, IDefaultCommunicationTimeouts timeouts)
        {
            return new ReplyDatagramAdapterListener(inner, throttle, timeouts);
        }

        internal static IRequestChannel GetRequestChannel(Source<IRequestSessionChannel> channelSource, IDefaultCommunicationTimeouts timeouts)
        {
            return new RequestDatagramAdapterChannel(channelSource, timeouts);
        }

        private abstract class DatagramAdapterChannelBase<TSessionChannel> : CommunicationObject, IChannel, ICommunicationObject where TSessionChannel: class, IChannel
        {
            private List<TSessionChannel> activeChannels;
            private TSessionChannel channel;
            private ChannelParameterCollection channelParameters;
            private DatagramAdapter.Source<TSessionChannel> channelSource;
            private TimeSpan defaultCloseTimeout;
            private TimeSpan defaultOpenTimeout;
            private TimeSpan defaultSendTimeout;

            protected DatagramAdapterChannelBase(DatagramAdapter.Source<TSessionChannel> channelSource, IDefaultCommunicationTimeouts timeouts)
            {
                if (channelSource == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelSource");
                }
                this.channelParameters = new ChannelParameterCollection(this);
                this.channelSource = channelSource;
                this.defaultCloseTimeout = timeouts.CloseTimeout;
                this.defaultOpenTimeout = timeouts.OpenTimeout;
                this.defaultSendTimeout = timeouts.SendTimeout;
                this.activeChannels = new List<TSessionChannel>();
            }

            public T GetProperty<T>() where T: class
            {
                if (typeof(T) == typeof(ChannelParameterCollection))
                {
                    return (T) this.channelParameters;
                }
                TSessionChannel local = this.channelSource();
                local.Abort();
                return local.GetProperty<T>();
            }

            protected override void OnAbort()
            {
                TSessionChannel channel;
                TSessionChannel[] localArray;
                lock (base.ThisLock)
                {
                    channel = this.channel;
                    localArray = new TSessionChannel[this.activeChannels.Count];
                    this.activeChannels.CopyTo(localArray);
                }
                if (channel != null)
                {
                    channel.Abort();
                }
                foreach (TSessionChannel local2 in localArray)
                {
                    local2.Abort();
                }
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                TSessionChannel channel;
                TSessionChannel[] localArray;
                lock (base.ThisLock)
                {
                    channel = this.channel;
                    localArray = new TSessionChannel[this.activeChannels.Count];
                    this.activeChannels.CopyTo(localArray);
                }
                if (this.channel == null)
                {
                    return new CloseCollectionAsyncResult(timeout, callback, state, (IList<ICommunicationObject>) localArray);
                }
                TSessionChannel local1 = channel;
                TSessionChannel local2 = channel;
                return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(local1.BeginClose), new ChainedEndHandler(local2.EndClose), (ICommunicationObject[]) localArray);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TSessionChannel channel;
                TSessionChannel[] localArray;
                lock (base.ThisLock)
                {
                    channel = this.channel;
                    localArray = new TSessionChannel[this.activeChannels.Count];
                    this.activeChannels.CopyTo(localArray);
                }
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (channel != null)
                {
                    channel.Close(helper.RemainingTime());
                }
                foreach (TSessionChannel local2 in localArray)
                {
                    local2.Close(helper.RemainingTime());
                }
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                if (result is CloseCollectionAsyncResult)
                {
                    CloseCollectionAsyncResult.End(result);
                }
                else
                {
                    ChainedAsyncResult.End(result);
                }
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
            }

            protected void RemoveChannel(TSessionChannel channel)
            {
                lock (base.ThisLock)
                {
                    this.activeChannels.Remove(channel);
                }
            }

            protected bool ReturnChannel(TSessionChannel channel)
            {
                lock (base.ThisLock)
                {
                    if (this.channel == null)
                    {
                        this.activeChannels.Remove(channel);
                        this.channel = channel;
                        return true;
                    }
                }
                return false;
            }

            protected TSessionChannel TakeChannel()
            {
                TSessionChannel channel;
                lock (base.ThisLock)
                {
                    base.ThrowIfDisposedOrNotOpen();
                    if (this.channel == null)
                    {
                        channel = this.channelSource();
                    }
                    else
                    {
                        channel = this.channel;
                        this.channel = default(TSessionChannel);
                    }
                    this.activeChannels.Add(channel);
                }
                return channel;
            }

            protected ChannelParameterCollection ChannelParameters
            {
                get
                {
                    return this.channelParameters;
                }
            }

            protected override TimeSpan DefaultCloseTimeout
            {
                get
                {
                    return this.defaultCloseTimeout;
                }
            }

            protected override TimeSpan DefaultOpenTimeout
            {
                get
                {
                    return this.defaultOpenTimeout;
                }
            }

            protected TimeSpan DefaultSendTimeout
            {
                get
                {
                    return this.defaultSendTimeout;
                }
            }
        }

        private abstract class DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType> : DelegatingChannelListener<TChannel>, ISessionThrottleNotification where TChannel: class, IChannel where TSessionChannel: class, IChannel where ItemType: class
        {
            private static AsyncCallback acceptCallbackDelegate;
            private bool acceptLoopDone;
            private Action channelPumpAfterExceptionDelegate;
            private static Action<object> channelPumpDelegate;
            private SessionChannelCollection<TChannel, TSessionChannel, ItemType> channels;
            private IChannelListener<TSessionChannel> listener;
            private ServiceThrottle throttle;
            private int usageCount;
            private IWaiter<TChannel, TSessionChannel, ItemType> waiter;

            static DatagramAdapterListenerBase()
            {
                DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.acceptCallbackDelegate = Fx.ThunkCallback(new AsyncCallback(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.AcceptCallbackStatic));
                DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.channelPumpDelegate = new Action<object>(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.ChannelPump);
            }

            protected DatagramAdapterListenerBase(IChannelListener<TSessionChannel> listener, ServiceThrottle throttle, IDefaultCommunicationTimeouts timeouts) : base(timeouts, listener)
            {
                if (listener == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listener");
                }
                this.channels = new SessionChannelCollection<TChannel, TSessionChannel, ItemType>(this.ThisLock);
                this.listener = listener;
                this.throttle = throttle;
                this.channelPumpAfterExceptionDelegate = new Action(this.ChannelPump);
            }

            private void AcceptCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously && this.FinishAccept(result))
                {
                    this.ChannelPump();
                }
            }

            private static void AcceptCallbackStatic(IAsyncResult result)
            {
                ((DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>) result.AsyncState).AcceptCallback(result);
            }

            private void AcceptLoopDone()
            {
                lock (this.ThisLock)
                {
                    bool acceptLoopDone = this.acceptLoopDone;
                    this.acceptLoopDone = true;
                    if (this.waiter != null)
                    {
                        this.waiter.Signal();
                    }
                }
            }

            private IAsyncResult BeginWaitForAcceptLoop(TimeSpan timeout, AsyncCallback callback, object state)
            {
                AsyncWaiter<TChannel, TSessionChannel, ItemType> waiter = null;
                lock (this.ThisLock)
                {
                    if (!this.acceptLoopDone)
                    {
                        waiter = new AsyncWaiter<TChannel, TSessionChannel, ItemType>(timeout, callback, state);
                        this.waiter = waiter;
                    }
                }
                if (waiter != null)
                {
                    return waiter;
                }
                return new CompletedAsyncResult(callback, state);
            }

            protected abstract IAsyncResult CallBeginReceive(TSessionChannel channel, AsyncCallback callback, object state);
            protected abstract ItemType CallEndReceive(TSessionChannel channel, IAsyncResult result);
            private void ChannelPump()
            {
                while (this.listener.State == CommunicationState.Opened)
                {
                    IAsyncResult result = null;
                    Exception exception = null;
                    try
                    {
                        result = this.listener.BeginAcceptChannel(TimeSpan.MaxValue, DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.acceptCallbackDelegate, this);
                    }
                    catch (ObjectDisposedException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                    }
                    catch (CommunicationException exception3)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                        }
                    }
                    catch (Exception exception4)
                    {
                        if (Fx.IsFatal(exception4))
                        {
                            throw;
                        }
                        exception = exception4;
                    }
                    if (exception != null)
                    {
                        this.Enqueue(exception, this.channelPumpAfterExceptionDelegate);
                        return;
                    }
                    if (!result.CompletedSynchronously)
                    {
                        break;
                    }
                    if (!this.FinishAccept(result))
                    {
                        return;
                    }
                }
            }

            private static void ChannelPump(object state)
            {
                ((DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>) state).ChannelPump();
            }

            internal void DecrementUsageCount()
            {
                bool flag;
                lock (this.ThisLock)
                {
                    this.usageCount--;
                    flag = this.usageCount == 0;
                }
                if (flag)
                {
                    this.channels.AbortChannels();
                }
            }

            private void EndWaitForAcceptLoop(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    AsyncWaiter<TChannel, TSessionChannel, ItemType>.End(result);
                }
            }

            protected abstract void Enqueue(ItemType item, Action callback);
            protected abstract void Enqueue(Exception exception, Action callback);
            private bool FinishAccept(IAsyncResult result)
            {
                TSessionChannel channel = default(TSessionChannel);
                Exception exception = null;
                try
                {
                    channel = this.listener.EndAcceptChannel(result);
                }
                catch (ObjectDisposedException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (CommunicationException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                }
                catch (Exception exception4)
                {
                    if (Fx.IsFatal(exception4))
                    {
                        throw;
                    }
                    exception = exception4;
                }
                if (exception != null)
                {
                    this.Enqueue(exception, this.channelPumpAfterExceptionDelegate);
                }
                else if (channel == null)
                {
                    this.AcceptLoopDone();
                }
                else if (base.State == CommunicationState.Opened)
                {
                    DatagramAdapterReceiver<TChannel, TSessionChannel, ItemType>.Pump((DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>) this, channel);
                }
                else
                {
                    try
                    {
                        channel.Close();
                    }
                    catch (CommunicationException exception5)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                        }
                    }
                    catch (TimeoutException exception6)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception6, TraceEventType.Information);
                        }
                    }
                    catch (Exception exception7)
                    {
                        if (Fx.IsFatal(exception7))
                        {
                            throw;
                        }
                        exception = exception7;
                    }
                    if (exception != null)
                    {
                        this.Enqueue(exception, this.channelPumpAfterExceptionDelegate);
                    }
                }
                return ((channel != null) && this.throttle.AcquireSession(this));
            }

            internal void IncrementUsageCount()
            {
                lock (this.ThisLock)
                {
                    this.usageCount++;
                }
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), new ChainedBeginHandler(this.BeginWaitForAcceptLoop), new ChainedEndHandler(this.EndWaitForAcceptLoop));
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.OnClose(helper.RemainingTime());
                this.WaitForAcceptLoop(helper.RemainingTime());
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                base.OnEndOpen(result);
                ActionItem.Schedule(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.channelPumpDelegate, this);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                base.OnOpen(timeout);
                ActionItem.Schedule(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.channelPumpDelegate, this);
            }

            public void ThrottleAcquired()
            {
                ActionItem.Schedule(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.channelPumpDelegate, this);
            }

            private void WaitForAcceptLoop(TimeSpan timeout)
            {
                SyncWaiter<TChannel, TSessionChannel, ItemType> waiter = null;
                lock (this.ThisLock)
                {
                    if (!this.acceptLoopDone)
                    {
                        waiter = new SyncWaiter<TChannel, TSessionChannel, ItemType>(this);
                        this.waiter = waiter;
                    }
                }
                if (waiter != null)
                {
                    waiter.Wait(timeout);
                }
            }

            internal SessionChannelCollection<TChannel, TSessionChannel, ItemType> Channels
            {
                get
                {
                    return this.channels;
                }
            }

            internal object ThisLock
            {
                get
                {
                    return base.ThisLock;
                }
            }

            internal class AsyncWaiter : AsyncResult, DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.IWaiter
            {
                private bool timedOut;
                private readonly IOThreadTimer timer;
                private static Action<object> timerCallback;

                static AsyncWaiter()
                {
                    DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.AsyncWaiter.timerCallback = new Action<object>(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.AsyncWaiter.TimerCallback);
                }

                internal AsyncWaiter(TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    if (timeout != TimeSpan.MaxValue)
                    {
                        this.timer = new IOThreadTimer(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.AsyncWaiter.timerCallback, this, false);
                        this.timer.Set(timeout);
                    }
                }

                internal static bool End(IAsyncResult result)
                {
                    AsyncResult.End<DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.AsyncWaiter>(result);
                    return !((DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.AsyncWaiter) result).timedOut;
                }

                public void Signal()
                {
                    if ((this.timer == null) || this.timer.Cancel())
                    {
                        base.Complete(false);
                    }
                }

                private static void TimerCallback(object state)
                {
                    DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.AsyncWaiter waiter = (DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.AsyncWaiter) state;
                    waiter.timedOut = true;
                    waiter.Complete(false);
                }
            }

            private class DatagramAdapterReceiver
            {
                private ServiceModelActivity activity;
                private TSessionChannel channel;
                private static EventHandler faultedDelegate;
                private Action itemDequeuedDelegate;
                private DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType> parent;
                private static AsyncCallback receiveCallbackDelegate;
                private static Action<object> startNextReceiveDelegate;

                static DatagramAdapterReceiver()
                {
                    DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.receiveCallbackDelegate = Fx.ThunkCallback(new AsyncCallback(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.ReceiveCallbackStatic));
                    DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.startNextReceiveDelegate = new Action<object>(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.StartNextReceive);
                }

                private DatagramAdapterReceiver(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType> parent, TSessionChannel channel)
                {
                    this.parent = parent;
                    this.channel = channel;
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        this.activity = ServiceModelActivity.Current;
                    }
                    if (DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.faultedDelegate == null)
                    {
                        DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.faultedDelegate = new EventHandler(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.FaultedCallback);
                    }
                    this.channel.Faulted += DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.faultedDelegate;
                    this.channel.Closed += new EventHandler(this.ClosedCallback);
                    this.itemDequeuedDelegate = new Action(this.StartNextReceive);
                    this.parent.channels.Add(channel);
                    try
                    {
                        channel.Open();
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
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, 0x8003f, System.ServiceModel.SR.GetString("TraceCodeFailedToOpenIncomingChannel"));
                        }
                        channel.Abort();
                        this.parent.Enqueue(exception3, null);
                    }
                }

                private void ClosedCallback(object sender, EventArgs e)
                {
                    TSessionChannel item = (TSessionChannel) sender;
                    this.parent.channels.Remove(item);
                    this.parent.throttle.DeactivateChannel();
                }

                private static void FaultedCallback(object sender, EventArgs e)
                {
                    ((IChannel) sender).Abort();
                }

                private void FinishReceive(IAsyncResult result)
                {
                    ItemType item = default(ItemType);
                    Exception exception = null;
                    try
                    {
                        item = this.parent.CallEndReceive(this.channel, result);
                    }
                    catch (ObjectDisposedException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                    }
                    catch (CommunicationException exception3)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                        }
                    }
                    catch (Exception exception4)
                    {
                        if (Fx.IsFatal(exception4))
                        {
                            throw;
                        }
                        exception = exception4;
                    }
                    if (exception != null)
                    {
                        this.parent.Enqueue(exception, this.itemDequeuedDelegate);
                    }
                    else if (item != null)
                    {
                        this.parent.Enqueue(item, this.itemDequeuedDelegate);
                    }
                    else
                    {
                        try
                        {
                            this.channel.Close();
                        }
                        catch (CommunicationException exception5)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                            }
                        }
                        catch (TimeoutException exception6)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception6, TraceEventType.Information);
                            }
                        }
                        catch (Exception exception7)
                        {
                            if (Fx.IsFatal(exception7))
                            {
                                throw;
                            }
                            exception = exception7;
                        }
                        if (exception != null)
                        {
                            this.parent.Enqueue(exception, this.itemDequeuedDelegate);
                        }
                    }
                }

                internal static void Pump(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType> listener, TSessionChannel channel)
                {
                    DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver state = new DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver(listener, channel);
                    ActionItem.Schedule(DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.startNextReceiveDelegate, state);
                }

                private static void ReceiveCallbackStatic(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        ((DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver) result.AsyncState).FinishReceive(result);
                    }
                }

                private void StartNextReceive()
                {
                    if (this.channel.State == CommunicationState.Opened)
                    {
                        using (ServiceModelActivity.BoundOperation(this.activity))
                        {
                            IAsyncResult result = null;
                            Exception exception = null;
                            try
                            {
                                result = this.parent.CallBeginReceive(this.channel, DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver.receiveCallbackDelegate, this);
                            }
                            catch (ObjectDisposedException exception2)
                            {
                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                                }
                            }
                            catch (CommunicationException exception3)
                            {
                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                                }
                            }
                            catch (Exception exception4)
                            {
                                if (Fx.IsFatal(exception4))
                                {
                                    throw;
                                }
                                exception = exception4;
                            }
                            if (exception != null)
                            {
                                this.parent.Enqueue(exception, this.itemDequeuedDelegate);
                            }
                            else if (result.CompletedSynchronously)
                            {
                                this.FinishReceive(result);
                            }
                        }
                    }
                }

                private static void StartNextReceive(object state)
                {
                    ((DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.DatagramAdapterReceiver) state).StartNextReceive();
                }
            }

            internal interface IWaiter
            {
                void Signal();
            }

            internal class SessionChannelCollection : SynchronizedCollection<TSessionChannel>
            {
                private EventHandler onChannelClosed;
                private EventHandler onChannelFaulted;

                internal SessionChannelCollection(object syncRoot) : base(syncRoot)
                {
                    this.onChannelClosed = new EventHandler(this.OnChannelClosed);
                    this.onChannelFaulted = new EventHandler(this.OnChannelFaulted);
                }

                public void AbortChannels()
                {
                    lock (base.SyncRoot)
                    {
                        for (int i = base.Count - 1; i >= 0; i--)
                        {
                            base[i].Abort();
                        }
                    }
                }

                private void AddingChannel(TSessionChannel channel)
                {
                    channel.Faulted += this.onChannelFaulted;
                    channel.Closed += this.onChannelClosed;
                }

                protected override void ClearItems()
                {
                    List<TSessionChannel> items = base.Items;
                    for (int i = 0; i < items.Count; i++)
                    {
                        this.RemovingChannel(items[i]);
                    }
                    base.ClearItems();
                }

                protected override void InsertItem(int index, TSessionChannel item)
                {
                    this.AddingChannel(item);
                    base.InsertItem(index, item);
                }

                private void OnChannelClosed(object sender, EventArgs args)
                {
                    TSessionChannel item = (TSessionChannel) sender;
                    base.Remove(item);
                }

                private void OnChannelFaulted(object sender, EventArgs args)
                {
                    TSessionChannel item = (TSessionChannel) sender;
                    base.Remove(item);
                }

                protected override void RemoveItem(int index)
                {
                    TSessionChannel channel = base.Items[index];
                    base.RemoveItem(index);
                    this.RemovingChannel(channel);
                }

                private void RemovingChannel(TSessionChannel channel)
                {
                    channel.Faulted -= this.onChannelFaulted;
                    channel.Closed -= this.onChannelClosed;
                    channel.Abort();
                }

                protected override void SetItem(int index, TSessionChannel item)
                {
                    TSessionChannel channel = base.Items[index];
                    this.AddingChannel(item);
                    base.SetItem(index, item);
                    this.RemovingChannel(channel);
                }
            }

            internal class SyncWaiter : DatagramAdapter.DatagramAdapterListenerBase<TChannel, TSessionChannel, ItemType>.IWaiter
            {
                private bool didSignal;
                private object thisLock;
                private ManualResetEvent wait;

                internal SyncWaiter(object thisLock)
                {
                    this.thisLock = thisLock;
                }

                public void Signal()
                {
                    lock (this.ThisLock)
                    {
                        this.didSignal = true;
                        if (this.wait != null)
                        {
                            this.wait.Set();
                        }
                    }
                }

                public bool Wait(TimeSpan timeout)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.didSignal)
                        {
                            this.wait = new ManualResetEvent(false);
                        }
                    }
                    if ((this.wait == null) || TimeoutHelper.WaitOne(this.wait, timeout))
                    {
                        if (this.wait != null)
                        {
                            this.wait.Close();
                            this.wait = null;
                        }
                        return true;
                    }
                    lock (this.ThisLock)
                    {
                        this.wait.Close();
                        this.wait = null;
                    }
                    return false;
                }

                private object ThisLock
                {
                    get
                    {
                        return this.thisLock;
                    }
                }
            }
        }

        private class InputDatagramAdapterAcceptor : InputChannelAcceptor
        {
            internal DatagramAdapter.InputDatagramAdapterListener listener;

            internal InputDatagramAdapterAcceptor(DatagramAdapter.InputDatagramAdapterListener listener) : base(listener)
            {
                this.listener = listener;
            }

            protected override InputChannel OnCreateChannel()
            {
                return new DatagramAdapter.InputDatagramAdapterChannel(this.listener);
            }
        }

        private class InputDatagramAdapterChannel : InputChannel
        {
            private DatagramAdapter.InputDatagramAdapterListener listener;

            internal InputDatagramAdapterChannel(DatagramAdapter.InputDatagramAdapterListener listener) : base(listener, null)
            {
                this.listener = listener;
            }

            public override T GetProperty<T>() where T: class
            {
                lock (this.listener.ThisLock)
                {
                    if (this.listener.Channels.Count > 0)
                    {
                        return this.listener.Channels[0].GetProperty<T>();
                    }
                    return default(T);
                }
            }

            protected override void OnClosed()
            {
                base.OnClosed();
                this.listener.DecrementUsageCount();
            }

            protected override void OnOpening()
            {
                this.listener.IncrementUsageCount();
                base.OnOpening();
            }
        }

        private class InputDatagramAdapterListener : DatagramAdapter.DatagramAdapterListenerBase<IInputChannel, IInputSessionChannel, Message>
        {
            private SingletonChannelAcceptor<IInputChannel, InputChannel, Message> acceptor;

            internal InputDatagramAdapterListener(IChannelListener<IInputSessionChannel> listener, ServiceThrottle throttle, IDefaultCommunicationTimeouts timeouts) : base(listener, throttle, timeouts)
            {
                this.acceptor = new DatagramAdapter.InputDatagramAdapterAcceptor(this);
                base.Acceptor = this.acceptor;
            }

            protected override IAsyncResult CallBeginReceive(IInputSessionChannel channel, AsyncCallback callback, object state)
            {
                return channel.BeginReceive(TimeSpan.MaxValue, callback, state);
            }

            protected override Message CallEndReceive(IInputSessionChannel channel, IAsyncResult result)
            {
                return channel.EndReceive(result);
            }

            protected override void Enqueue(Exception exception, Action callback)
            {
                this.acceptor.Enqueue(exception, callback);
            }

            protected override void Enqueue(Message message, Action callback)
            {
                this.acceptor.Enqueue(message, callback);
            }
        }

        private class OutputDatagramAdapterChannel : DatagramAdapter.DatagramAdapterChannelBase<IOutputSessionChannel>, IOutputChannel, IChannel, ICommunicationObject
        {
            private EndpointAddress remoteAddress;
            private Uri via;

            internal OutputDatagramAdapterChannel(DatagramAdapter.Source<IOutputSessionChannel> channelSource, IDefaultCommunicationTimeouts timeouts) : base(channelSource, timeouts)
            {
                IOutputSessionChannel channel = channelSource();
                try
                {
                    this.remoteAddress = channel.RemoteAddress;
                    this.via = channel.Via;
                    channel.Close();
                }
                finally
                {
                    channel.Abort();
                }
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SendAsyncResult(this, message, timeout, callback, state);
            }

            public void EndSend(IAsyncResult result)
            {
                SendAsyncResult.End(result);
            }

            public void Send(Message message)
            {
                this.Send(message, base.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                IOutputSessionChannel innerChannel = base.TakeChannel();
                bool flag = true;
                try
                {
                    if (innerChannel.State == CommunicationState.Created)
                    {
                        base.ChannelParameters.PropagateChannelParameters(innerChannel);
                        innerChannel.Open(helper.RemainingTime());
                    }
                    innerChannel.Send(message, helper.RemainingTime());
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        innerChannel.Abort();
                        base.RemoveChannel(innerChannel);
                    }
                }
                if (!base.ReturnChannel(innerChannel))
                {
                    try
                    {
                        innerChannel.Close(helper.RemainingTime());
                    }
                    finally
                    {
                        base.RemoveChannel(innerChannel);
                    }
                }
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return this.remoteAddress;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.via;
                }
            }

            private class SendAsyncResult : AsyncResult
            {
                private DatagramAdapter.OutputDatagramAdapterChannel adapter;
                private bool hasCompletedAsynchronously;
                private Message message;
                private TimeoutHelper timeoutHelper;

                public SendAsyncResult(DatagramAdapter.OutputDatagramAdapterChannel adapter, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.hasCompletedAsynchronously = true;
                    this.adapter = adapter;
                    this.message = message;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    IOutputSessionChannel innerChannel = this.adapter.TakeChannel();
                    try
                    {
                        if (innerChannel.State == CommunicationState.Created)
                        {
                            this.adapter.ChannelParameters.PropagateChannelParameters(innerChannel);
                            innerChannel.BeginOpen(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnOpenComplete)), innerChannel);
                        }
                        else
                        {
                            innerChannel.BeginSend(message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnSendComplete)), innerChannel);
                        }
                    }
                    catch
                    {
                        innerChannel.Abort();
                        this.adapter.RemoveChannel(innerChannel);
                        throw;
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<DatagramAdapter.OutputDatagramAdapterChannel.SendAsyncResult>(result);
                }

                private void OnCloseComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IOutputSessionChannel asyncState = (IOutputSessionChannel) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.EndClose(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    this.adapter.RemoveChannel(asyncState);
                    base.Complete(this.hasCompletedAsynchronously, exception);
                }

                private void OnOpenComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IOutputSessionChannel asyncState = (IOutputSessionChannel) result.AsyncState;
                    try
                    {
                        asyncState.EndOpen(result);
                        asyncState.BeginSend(this.message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnSendComplete)), asyncState);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.Abort();
                        this.adapter.RemoveChannel(asyncState);
                        base.Complete(this.hasCompletedAsynchronously, exception);
                    }
                }

                private void OnSendComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IOutputSessionChannel asyncState = (IOutputSessionChannel) result.AsyncState;
                    try
                    {
                        asyncState.EndSend(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.Abort();
                        this.adapter.RemoveChannel(asyncState);
                        base.Complete(this.hasCompletedAsynchronously, exception);
                        return;
                    }
                    if (!this.adapter.ReturnChannel(asyncState))
                    {
                        try
                        {
                            asyncState.BeginClose(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnCloseComplete)), asyncState);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            this.adapter.RemoveChannel(asyncState);
                            base.Complete(this.hasCompletedAsynchronously, exception2);
                        }
                    }
                    else
                    {
                        base.Complete(this.hasCompletedAsynchronously);
                    }
                }
            }
        }

        private class ReplyDatagramAdapterAcceptor : ReplyChannelAcceptor
        {
            internal DatagramAdapter.ReplyDatagramAdapterListener listener;

            internal ReplyDatagramAdapterAcceptor(DatagramAdapter.ReplyDatagramAdapterListener listener) : base(listener)
            {
                this.listener = listener;
            }

            protected override ReplyChannel OnCreateChannel()
            {
                return new DatagramAdapter.ReplyDatagramAdapterChannel(this.listener);
            }
        }

        private class ReplyDatagramAdapterChannel : ReplyChannel
        {
            private DatagramAdapter.ReplyDatagramAdapterListener listener;

            internal ReplyDatagramAdapterChannel(DatagramAdapter.ReplyDatagramAdapterListener listener) : base(listener, null)
            {
                this.listener = listener;
            }

            public override T GetProperty<T>() where T: class
            {
                lock (this.listener.ThisLock)
                {
                    if (this.listener.Channels.Count > 0)
                    {
                        return this.listener.Channels[0].GetProperty<T>();
                    }
                    return default(T);
                }
            }

            protected override void OnClosed()
            {
                base.OnClosed();
                this.listener.DecrementUsageCount();
            }

            protected override void OnOpening()
            {
                this.listener.IncrementUsageCount();
                base.OnOpening();
            }
        }

        private class ReplyDatagramAdapterListener : DatagramAdapter.DatagramAdapterListenerBase<IReplyChannel, IReplySessionChannel, RequestContext>
        {
            private SingletonChannelAcceptor<IReplyChannel, ReplyChannel, RequestContext> acceptor;

            internal ReplyDatagramAdapterListener(IChannelListener<IReplySessionChannel> listener, ServiceThrottle throttle, IDefaultCommunicationTimeouts timeouts) : base(listener, throttle, timeouts)
            {
                this.acceptor = new DatagramAdapter.ReplyDatagramAdapterAcceptor(this);
                base.Acceptor = this.acceptor;
            }

            protected override IAsyncResult CallBeginReceive(IReplySessionChannel channel, AsyncCallback callback, object state)
            {
                return channel.BeginReceiveRequest(TimeSpan.MaxValue, callback, state);
            }

            protected override RequestContext CallEndReceive(IReplySessionChannel channel, IAsyncResult result)
            {
                return channel.EndReceiveRequest(result);
            }

            protected override void Enqueue(Exception exception, Action callback)
            {
                this.acceptor.Enqueue(exception, callback);
            }

            protected override void Enqueue(RequestContext request, Action callback)
            {
                this.acceptor.Enqueue(request, callback);
            }
        }

        private class RequestDatagramAdapterChannel : DatagramAdapter.DatagramAdapterChannelBase<IRequestSessionChannel>, IRequestChannel, IChannel, ICommunicationObject
        {
            private EndpointAddress remoteAddress;
            private Uri via;

            internal RequestDatagramAdapterChannel(DatagramAdapter.Source<IRequestSessionChannel> channelSource, IDefaultCommunicationTimeouts timeouts) : base(channelSource, timeouts)
            {
                IRequestSessionChannel channel = channelSource();
                try
                {
                    this.remoteAddress = channel.RemoteAddress;
                    this.via = channel.Via;
                    channel.Close();
                }
                finally
                {
                    channel.Abort();
                }
            }

            public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
            {
                return this.BeginRequest(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new RequestAsyncResult(this, message, timeout, callback, state);
            }

            public Message EndRequest(IAsyncResult result)
            {
                return RequestAsyncResult.End(result);
            }

            public Message Request(Message request)
            {
                return this.Request(request, base.DefaultSendTimeout);
            }

            public Message Request(Message request, TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                IRequestSessionChannel innerChannel = base.TakeChannel();
                bool flag = true;
                Message message = null;
                try
                {
                    if (innerChannel.State == CommunicationState.Created)
                    {
                        base.ChannelParameters.PropagateChannelParameters(innerChannel);
                        innerChannel.Open(helper.RemainingTime());
                    }
                    message = innerChannel.Request(request, helper.RemainingTime());
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        innerChannel.Abort();
                        base.RemoveChannel(innerChannel);
                    }
                }
                if (!base.ReturnChannel(innerChannel))
                {
                    try
                    {
                        innerChannel.Close(helper.RemainingTime());
                    }
                    finally
                    {
                        base.RemoveChannel(innerChannel);
                    }
                }
                return message;
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return this.remoteAddress;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.via;
                }
            }

            private class RequestAsyncResult : AsyncResult
            {
                private DatagramAdapter.RequestDatagramAdapterChannel adapter;
                private bool hasCompletedAsynchronously;
                private Message message;
                private Message reply;
                private TimeoutHelper timeoutHelper;

                public RequestAsyncResult(DatagramAdapter.RequestDatagramAdapterChannel adapter, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.hasCompletedAsynchronously = true;
                    this.adapter = adapter;
                    this.message = message;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    IRequestSessionChannel innerChannel = this.adapter.TakeChannel();
                    try
                    {
                        if (innerChannel.State == CommunicationState.Created)
                        {
                            this.adapter.ChannelParameters.PropagateChannelParameters(innerChannel);
                            innerChannel.BeginOpen(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnOpenComplete)), innerChannel);
                        }
                        else
                        {
                            innerChannel.BeginRequest(message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnRequestComplete)), innerChannel);
                        }
                    }
                    catch
                    {
                        innerChannel.Abort();
                        this.adapter.RemoveChannel(innerChannel);
                        throw;
                    }
                }

                public static Message End(IAsyncResult result)
                {
                    return AsyncResult.End<DatagramAdapter.RequestDatagramAdapterChannel.RequestAsyncResult>(result).reply;
                }

                private void OnCloseComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IRequestSessionChannel asyncState = (IRequestSessionChannel) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.EndClose(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    this.adapter.RemoveChannel(asyncState);
                    base.Complete(this.hasCompletedAsynchronously, exception);
                }

                private void OnOpenComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IRequestSessionChannel asyncState = (IRequestSessionChannel) result.AsyncState;
                    try
                    {
                        asyncState.EndOpen(result);
                        asyncState.BeginRequest(this.message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnRequestComplete)), asyncState);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.Abort();
                        this.adapter.RemoveChannel(asyncState);
                        base.Complete(this.hasCompletedAsynchronously, exception);
                    }
                }

                private void OnRequestComplete(IAsyncResult result)
                {
                    this.hasCompletedAsynchronously &= result.CompletedSynchronously;
                    IRequestSessionChannel asyncState = (IRequestSessionChannel) result.AsyncState;
                    try
                    {
                        this.reply = asyncState.EndRequest(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.Abort();
                        this.adapter.RemoveChannel(asyncState);
                        base.Complete(this.hasCompletedAsynchronously, exception);
                        return;
                    }
                    if (!this.adapter.ReturnChannel(asyncState))
                    {
                        try
                        {
                            asyncState.BeginClose(this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.OnCloseComplete)), asyncState);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            this.adapter.RemoveChannel(asyncState);
                            base.Complete(this.hasCompletedAsynchronously, exception2);
                        }
                    }
                    else
                    {
                        base.Complete(this.hasCompletedAsynchronously);
                    }
                }
            }
        }

        internal delegate T Source<T>();
    }
}

