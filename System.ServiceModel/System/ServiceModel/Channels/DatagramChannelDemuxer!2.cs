namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal abstract class DatagramChannelDemuxer<TInnerChannel, TInnerItem> : TypedChannelDemuxer, IChannelDemuxer where TInnerChannel: class, IChannel where TInnerItem: class, IDisposable
    {
        private bool abortOngoingOpen;
        private IChannelDemuxFailureHandler demuxFailureHandler;
        private MessageFilterTable<IChannelListener> filterTable;
        private TInnerChannel innerChannel;
        private IChannelListener<TInnerChannel> innerListener;
        private Action onItemDequeued;
        private static AsyncCallback onReceiveComplete;
        private int openCount;
        private ThreadNeutralSemaphore openSemaphore;
        private Exception pendingInnerListenerOpenException;
        private static Action<object> startReceivingStatic;

        static DatagramChannelDemuxer()
        {
            DatagramChannelDemuxer<TInnerChannel, TInnerItem>.onReceiveComplete = Fx.ThunkCallback(new AsyncCallback(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OnReceiveCompleteStatic));
            DatagramChannelDemuxer<TInnerChannel, TInnerItem>.startReceivingStatic = new Action<object>(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.StartReceivingStatic);
        }

        public DatagramChannelDemuxer(BindingContext context)
        {
            this.filterTable = new MessageFilterTable<IChannelListener>();
            this.innerListener = context.BuildInnerChannelListener<TInnerChannel>();
            if (context.BindingParameters != null)
            {
                this.demuxFailureHandler = context.BindingParameters.Find<IChannelDemuxFailureHandler>();
            }
            this.openSemaphore = new ThreadNeutralSemaphore(1);
        }

        protected abstract void AbortItem(TInnerItem item);
        private void AbortState()
        {
            if (this.innerChannel != null)
            {
                this.innerChannel.Abort();
            }
            this.innerListener.Abort();
        }

        protected abstract IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state);
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(ChannelDemuxerFilter filter) where TChannel: class, IChannel
        {
            LayeredChannelListener<TChannel> listener = this.CreateListener<TChannel>(filter);
            listener.InnerChannelListener = this.innerListener;
            return listener;
        }

        protected abstract LayeredChannelListener<TChannel> CreateListener<TChannel>(ChannelDemuxerFilter filter) where TChannel: class, IChannel;
        protected abstract void Dispatch(IChannelListener listener);
        protected abstract void EndpointNotFound(TInnerItem item);
        protected abstract TInnerItem EndReceive(IAsyncResult result);
        protected abstract void EnqueueAndDispatch(IChannelListener listener, TInnerItem item, Action dequeuedCallback, bool canDispatchOnThisThread);
        protected abstract void EnqueueAndDispatch(IChannelListener listener, Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread);
        protected abstract Message GetMessage(TInnerItem item);
        private bool HandleReceiveResult(IAsyncResult result)
        {
            TInnerItem local;
            try
            {
                local = this.EndReceive(result);
            }
            catch (CommunicationObjectFaultedException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                return true;
            }
            catch (CommunicationObjectAbortedException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
                return true;
            }
            catch (ObjectDisposedException exception3)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
                return true;
            }
            catch (CommunicationException exception4)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                }
                return false;
            }
            catch (TimeoutException exception5)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                }
                return false;
            }
            catch (Exception exception6)
            {
                if (Fx.IsFatal(exception6))
                {
                    throw;
                }
                this.HandleUnknownException(exception6);
                return true;
            }
            if (local == null)
            {
                if ((this.innerChannel.State == CommunicationState.Opened) && DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, 0x40023, System.ServiceModel.SR.GetString("TraceCodePrematureDatagramEof"), null, this.innerChannel, null);
                }
                return true;
            }
            try
            {
                return this.ProcessItem(local);
            }
            catch (CommunicationException exception7)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception7, TraceEventType.Information);
                }
                return false;
            }
            catch (TimeoutException exception8)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception8, TraceEventType.Information);
                }
                return false;
            }
            catch (Exception exception9)
            {
                if (Fx.IsFatal(exception9))
                {
                    throw;
                }
                this.HandleUnknownException(exception9);
                return true;
            }
        }

        protected void HandleUnknownException(Exception exception)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
            }
            IChannelListener listener = null;
            lock (this.ThisLock)
            {
                if (this.filterTable.Count > 0)
                {
                    KeyValuePair<MessageFilter, IChannelListener>[] array = new KeyValuePair<MessageFilter, IChannelListener>[this.filterTable.Count];
                    this.filterTable.CopyTo(array, 0);
                    listener = array[0].Value;
                    if (this.onItemDequeued == null)
                    {
                        this.onItemDequeued = new Action(this.OnItemDequeued);
                    }
                    this.EnqueueAndDispatch(listener, exception, this.onItemDequeued, false);
                }
            }
        }

        private IChannelListener MatchListener(Message message)
        {
            IChannelListener data = null;
            lock (this.ThisLock)
            {
                if (this.filterTable.GetMatchingValue(message, out data))
                {
                    return data;
                }
            }
            return null;
        }

        public IAsyncResult OnBeginOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.filterTable.ContainsKey(filter.Filter))
                {
                    this.filterTable.Remove(filter.Filter);
                    if (--this.openCount == 0)
                    {
                        flag = true;
                    }
                }
            }
            if (!flag)
            {
                return new CompletedAsyncResult(callback, state);
            }
            return new CloseAsyncResult<TInnerChannel, TInnerItem>((DatagramChannelDemuxer<TInnerChannel, TInnerItem>) this, timeout, callback, state);
        }

        public IAsyncResult OnBeginOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult<TInnerChannel, TInnerItem>((DatagramChannelDemuxer<TInnerChannel, TInnerItem>) this, filter, listener, timeout, callback, state);
        }

        public void OnEndOuterListenerClose(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                CloseAsyncResult<TInnerChannel, TInnerItem>.End(result);
            }
        }

        public void OnEndOuterListenerOpen(IAsyncResult result)
        {
            OpenAsyncResult<TInnerChannel, TInnerItem>.End(result);
        }

        private void OnItemDequeued()
        {
            this.StartReceiving();
        }

        public void OnOuterListenerAbort(ChannelDemuxerFilter filter)
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.filterTable.ContainsKey(filter.Filter))
                {
                    this.filterTable.Remove(filter.Filter);
                    if (--this.openCount == 0)
                    {
                        flag = true;
                        this.abortOngoingOpen = true;
                    }
                }
            }
            if (flag)
            {
                this.AbortState();
            }
        }

        public void OnOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout)
        {
            bool flag = false;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            lock (this.ThisLock)
            {
                if (this.filterTable.ContainsKey(filter.Filter))
                {
                    this.filterTable.Remove(filter.Filter);
                    if (--this.openCount == 0)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                bool flag2 = false;
                try
                {
                    if (this.innerChannel != null)
                    {
                        this.innerChannel.Close(helper.RemainingTime());
                    }
                    this.innerListener.Close(helper.RemainingTime());
                    flag2 = true;
                }
                finally
                {
                    if (!flag2)
                    {
                        this.AbortState();
                    }
                }
            }
        }

        public void OnOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.openSemaphore.Enter(helper.RemainingTime());
            try
            {
                if (this.ShouldOpenInnerListener(filter, listener))
                {
                    try
                    {
                        this.innerListener.Open(helper.RemainingTime());
                        this.innerChannel = this.innerListener.AcceptChannel(helper.RemainingTime());
                        this.innerChannel.Open(helper.RemainingTime());
                        lock (this.ThisLock)
                        {
                            if (this.abortOngoingOpen)
                            {
                                this.AbortState();
                                return;
                            }
                        }
                        ActionItem.Schedule(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.startReceivingStatic, this);
                        return;
                    }
                    catch (Exception exception)
                    {
                        this.pendingInnerListenerOpenException = exception;
                        throw;
                    }
                }
                this.ThrowPendingOpenExceptionIfAny();
            }
            finally
            {
                this.openSemaphore.Exit();
            }
        }

        private void OnReceiveComplete(IAsyncResult result)
        {
            if (!this.HandleReceiveResult(result))
            {
                this.StartReceiving();
            }
        }

        private static void OnReceiveCompleteStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((DatagramChannelDemuxer<TInnerChannel, TInnerItem>) result.AsyncState).OnReceiveComplete(result);
            }
        }

        private bool ProcessItem(TInnerItem item)
        {
            bool flag;
            try
            {
                Message message = this.GetMessage(item);
                IChannelListener listener = null;
                try
                {
                    listener = this.MatchListener(message);
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    return false;
                }
                catch (MultipleFilterMatchesException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                    return false;
                }
                catch (XmlException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                    return false;
                }
                catch (Exception exception4)
                {
                    if (Fx.IsFatal(exception4))
                    {
                        throw;
                    }
                    this.HandleUnknownException(exception4);
                    return true;
                }
                if (listener == null)
                {
                    ErrorBehavior.ThrowAndCatch(new EndpointNotFoundException(System.ServiceModel.SR.GetString("UnableToDemuxChannel", new object[] { message.Headers.Action })), message);
                    this.EndpointNotFound(item);
                    item = default(TInnerItem);
                    return false;
                }
                if (this.onItemDequeued == null)
                {
                    this.onItemDequeued = new Action(this.OnItemDequeued);
                }
                this.EnqueueAndDispatch(listener, item, this.onItemDequeued, false);
                item = default(TInnerItem);
                flag = true;
            }
            finally
            {
                if (item != null)
                {
                    this.AbortItem(item);
                }
            }
            return flag;
        }

        private bool ShouldOpenInnerListener(ChannelDemuxerFilter filter, IChannelListener listener)
        {
            lock (this.ThisLock)
            {
                if ((listener.State == CommunicationState.Closed) || (listener.State == CommunicationState.Closing))
                {
                    return false;
                }
                this.filterTable.Add(filter.Filter, listener, filter.Priority);
                if (++this.openCount == 1)
                {
                    this.abortOngoingOpen = false;
                    return true;
                }
            }
            return false;
        }

        private void StartReceiving()
        {
            IAsyncResult result;
        Label_0000:
            if (this.innerChannel.State != CommunicationState.Opened)
            {
                return;
            }
            try
            {
                result = this.BeginReceive(TimeSpan.MaxValue, DatagramChannelDemuxer<TInnerChannel, TInnerItem>.onReceiveComplete, this);
            }
            catch (CommunicationObjectFaultedException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                return;
            }
            catch (CommunicationObjectAbortedException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
                return;
            }
            catch (ObjectDisposedException exception3)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
                return;
            }
            catch (CommunicationException exception4)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                }
                goto Label_0000;
            }
            catch (TimeoutException exception5)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                }
                goto Label_0000;
            }
            catch (Exception exception6)
            {
                if (Fx.IsFatal(exception6))
                {
                    throw;
                }
                this.HandleUnknownException(exception6);
                return;
            }
            if (!result.CompletedSynchronously || this.HandleReceiveResult(result))
            {
                return;
            }
            goto Label_0000;
        }

        private static void StartReceivingStatic(object state)
        {
            ((DatagramChannelDemuxer<TInnerChannel, TInnerItem>) state).StartReceiving();
        }

        private void ThrowPendingOpenExceptionIfAny()
        {
            if (this.pendingInnerListenerOpenException != null)
            {
                if (this.pendingInnerListenerOpenException is CommunicationObjectAbortedException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("PreviousChannelDemuxerOpenFailed", new object[] { this.pendingInnerListenerOpenException.ToString() })));
                }
                if (this.pendingInnerListenerOpenException is CommunicationObjectFaultedException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationObjectFaultedException(System.ServiceModel.SR.GetString("PreviousChannelDemuxerOpenFailed", new object[] { this.pendingInnerListenerOpenException.ToString() })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(System.ServiceModel.SR.GetString("PreviousChannelDemuxerOpenFailed", new object[] { this.pendingInnerListenerOpenException.ToString() })));
            }
        }

        protected IChannelDemuxFailureHandler DemuxFailureHandler
        {
            get
            {
                return this.demuxFailureHandler;
            }
        }

        protected TInnerChannel InnerChannel
        {
            get
            {
                return this.innerChannel;
            }
        }

        protected IChannelListener<TInnerChannel> InnerListener
        {
            get
            {
                return this.innerListener;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private DatagramChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer;
            private bool closedInnerChannel;
            private static AsyncCallback sharedCallback;
            private TimeoutHelper timeoutHelper;

            static CloseAsyncResult()
            {
                DatagramChannelDemuxer<TInnerChannel, TInnerItem>.CloseAsyncResult.sharedCallback = Fx.ThunkCallback(new AsyncCallback(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.CloseAsyncResult.SharedCallback));
            }

            public CloseAsyncResult(DatagramChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channelDemuxer = channelDemuxer;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (channelDemuxer.innerChannel != null)
                {
                    bool flag = false;
                    try
                    {
                        IAsyncResult result = channelDemuxer.innerChannel.BeginClose(this.timeoutHelper.RemainingTime(), DatagramChannelDemuxer<TInnerChannel, TInnerItem>.CloseAsyncResult.sharedCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            flag = true;
                            return;
                        }
                        channelDemuxer.innerChannel.EndClose(result);
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.channelDemuxer.AbortState();
                        }
                    }
                }
                if (this.OnInnerChannelClosed())
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<DatagramChannelDemuxer<TInnerChannel, TInnerItem>.CloseAsyncResult>(result);
            }

            private bool OnInnerChannelClosed()
            {
                this.closedInnerChannel = true;
                bool flag = false;
                try
                {
                    IAsyncResult result = this.channelDemuxer.innerListener.BeginClose(this.timeoutHelper.RemainingTime(), DatagramChannelDemuxer<TInnerChannel, TInnerItem>.CloseAsyncResult.sharedCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        flag = true;
                        return false;
                    }
                    this.channelDemuxer.innerListener.EndClose(result);
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.channelDemuxer.AbortState();
                    }
                }
                return true;
            }

            private static void SharedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    DatagramChannelDemuxer<TInnerChannel, TInnerItem>.CloseAsyncResult asyncState = (DatagramChannelDemuxer<TInnerChannel, TInnerItem>.CloseAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    bool flag2 = false;
                    try
                    {
                        if (!asyncState.closedInnerChannel)
                        {
                            asyncState.channelDemuxer.innerChannel.EndClose(result);
                            flag = asyncState.OnInnerChannelClosed();
                            flag2 = true;
                        }
                        else
                        {
                            asyncState.channelDemuxer.innerListener.EndClose(result);
                            flag = true;
                            flag2 = true;
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    finally
                    {
                        if (!flag2)
                        {
                            asyncState.channelDemuxer.AbortState();
                        }
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private static AsyncCallback acceptChannelCallback;
            private DatagramChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer;
            private ChannelDemuxerFilter filter;
            private IChannelListener listener;
            private static AsyncCallback openChannelCallback;
            private bool openInnerListener;
            private static AsyncCallback openListenerCallback;
            private TimeoutHelper timeoutHelper;
            private static FastAsyncCallback waitOverCallback;

            static OpenAsyncResult()
            {
                DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.waitOverCallback = new FastAsyncCallback(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.WaitOverCallback);
                DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.openListenerCallback = Fx.ThunkCallback(new AsyncCallback(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.OpenListenerCallback));
                DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.acceptChannelCallback = Fx.ThunkCallback(new AsyncCallback(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.AcceptChannelCallback));
                DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.openChannelCallback = Fx.ThunkCallback(new AsyncCallback(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.OpenChannelCallback));
            }

            public OpenAsyncResult(DatagramChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer, ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channelDemuxer = channelDemuxer;
                this.filter = filter;
                this.listener = listener;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (this.channelDemuxer.openSemaphore.EnterAsync(this.timeoutHelper.RemainingTime(), DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.waitOverCallback, this))
                {
                    bool flag = false;
                    bool flag2 = false;
                    try
                    {
                        flag2 = this.OnWaitOver();
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.Cleanup();
                        }
                    }
                    if (flag2)
                    {
                        this.Cleanup();
                        base.Complete(true);
                    }
                }
            }

            private static void AcceptChannelCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult asyncState = (DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        flag = asyncState.OnEndAcceptChannel(result);
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
                        if (exception != null)
                        {
                            asyncState.channelDemuxer.pendingInnerListenerOpenException = exception;
                        }
                        asyncState.Cleanup();
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private void Cleanup()
            {
                this.channelDemuxer.openSemaphore.Exit();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult>(result);
            }

            private bool OnEndAcceptChannel(IAsyncResult result)
            {
                this.channelDemuxer.innerChannel = this.channelDemuxer.innerListener.EndAcceptChannel(result);
                IAsyncResult result2 = this.channelDemuxer.innerChannel.BeginOpen(this.timeoutHelper.RemainingTime(), DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.acceptChannelCallback, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                this.OnEndOpenChannel(result2);
                return true;
            }

            private void OnEndOpenChannel(IAsyncResult result)
            {
                this.channelDemuxer.innerChannel.EndOpen(result);
                lock (this.channelDemuxer.ThisLock)
                {
                    if (this.channelDemuxer.abortOngoingOpen)
                    {
                        this.channelDemuxer.AbortState();
                        return;
                    }
                }
                ActionItem.Schedule(DatagramChannelDemuxer<TInnerChannel, TInnerItem>.startReceivingStatic, this.channelDemuxer);
            }

            private bool OnInnerListenerEndOpen(IAsyncResult result)
            {
                this.channelDemuxer.innerListener.EndOpen(result);
                result = this.channelDemuxer.innerListener.BeginAcceptChannel(this.timeoutHelper.RemainingTime(), DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.acceptChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                return this.OnEndAcceptChannel(result);
            }

            private bool OnOpenInnerListener()
            {
                bool flag;
                try
                {
                    IAsyncResult result = this.channelDemuxer.innerListener.BeginOpen(this.timeoutHelper.RemainingTime(), DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.openListenerCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.OnInnerListenerEndOpen(result);
                    flag = true;
                }
                catch (Exception exception)
                {
                    this.channelDemuxer.pendingInnerListenerOpenException = exception;
                    throw;
                }
                return flag;
            }

            private bool OnWaitOver()
            {
                this.openInnerListener = this.channelDemuxer.ShouldOpenInnerListener(this.filter, this.listener);
                if (!this.openInnerListener)
                {
                    this.channelDemuxer.ThrowPendingOpenExceptionIfAny();
                    return true;
                }
                return this.OnOpenInnerListener();
            }

            private static void OpenChannelCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult asyncState = (DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.OnEndOpenChannel(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (exception != null)
                    {
                        asyncState.channelDemuxer.pendingInnerListenerOpenException = exception;
                    }
                    asyncState.Cleanup();
                    asyncState.Complete(false, exception);
                }
            }

            private static void OpenListenerCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult asyncState = (DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.OnInnerListenerEndOpen(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (exception != null)
                    {
                        asyncState.channelDemuxer.pendingInnerListenerOpenException = exception;
                    }
                    asyncState.Cleanup();
                    asyncState.Complete(false, exception);
                }
            }

            private static void WaitOverCallback(object state, Exception asyncException)
            {
                DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult result = (DatagramChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult) state;
                Exception exception = asyncException;
                bool flag = false;
                if (exception != null)
                {
                    flag = true;
                }
                else
                {
                    try
                    {
                        flag = result.OnWaitOver();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                }
                if (flag)
                {
                    result.Cleanup();
                    result.Complete(false, exception);
                }
            }
        }
    }
}

