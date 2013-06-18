namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Xml;

    internal abstract class SessionChannelDemuxer<TInnerChannel, TInnerItem> : TypedChannelDemuxer, IChannelDemuxer where TInnerChannel: class, IChannel where TInnerItem: class, IDisposable
    {
        private bool abortOngoingOpen;
        private IChannelDemuxFailureHandler demuxFailureHandler;
        private MessageFilterTable<InputQueueChannelListener<TInnerChannel>> filterTable;
        private IChannelListener<TInnerChannel> innerListener;
        private static AsyncCallback onAcceptComplete;
        private Action onItemDequeued;
        private static AsyncCallback onPeekComplete;
        private Action<object> onStartAccepting;
        private int openCount;
        private ThreadNeutralSemaphore openSemaphore;
        private TimeSpan peekTimeout;
        private Exception pendingExceptionOnOpen;
        private static WaitCallback scheduleAcceptStatic;
        private static Action<object> startAcceptStatic;
        private FlowThrottle throttle;

        static SessionChannelDemuxer()
        {
            SessionChannelDemuxer<TInnerChannel, TInnerItem>.onAcceptComplete = Fx.ThunkCallback(new AsyncCallback(SessionChannelDemuxer<TInnerChannel, TInnerItem>.OnAcceptCompleteStatic));
            SessionChannelDemuxer<TInnerChannel, TInnerItem>.onPeekComplete = Fx.ThunkCallback(new AsyncCallback(SessionChannelDemuxer<TInnerChannel, TInnerItem>.OnPeekCompleteStatic));
            SessionChannelDemuxer<TInnerChannel, TInnerItem>.scheduleAcceptStatic = new WaitCallback(SessionChannelDemuxer<TInnerChannel, TInnerItem>.ScheduleAcceptStatic);
            SessionChannelDemuxer<TInnerChannel, TInnerItem>.startAcceptStatic = new Action<object>(SessionChannelDemuxer<TInnerChannel, TInnerItem>.StartAcceptStatic);
        }

        public SessionChannelDemuxer(BindingContext context, TimeSpan peekTimeout, int maxPendingSessions)
        {
            if (context.BindingParameters != null)
            {
                this.demuxFailureHandler = context.BindingParameters.Find<IChannelDemuxFailureHandler>();
            }
            this.innerListener = context.BuildInnerChannelListener<TInnerChannel>();
            this.filterTable = new MessageFilterTable<InputQueueChannelListener<TInnerChannel>>();
            this.openSemaphore = new ThreadNeutralSemaphore(1);
            this.peekTimeout = peekTimeout;
            this.throttle = new FlowThrottle(SessionChannelDemuxer<TInnerChannel, TInnerItem>.scheduleAcceptStatic, maxPendingSessions, null, null);
        }

        protected abstract void AbortItem(TInnerItem item);
        private bool BeginAcceptChannel(bool requiresThrottle, out IAsyncResult result)
        {
            result = null;
            if (requiresThrottle && !this.throttle.Acquire(this))
            {
                return false;
            }
            bool flag = true;
            try
            {
                result = this.innerListener.BeginAcceptChannel(TimeSpan.MaxValue, SessionChannelDemuxer<TInnerChannel, TInnerItem>.onAcceptComplete, this);
                flag = false;
            }
            catch (CommunicationObjectFaultedException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                return false;
            }
            catch (CommunicationObjectAbortedException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
                return false;
            }
            catch (ObjectDisposedException exception3)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
                return false;
            }
            catch (CommunicationException exception4)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                }
                return true;
            }
            catch (TimeoutException exception5)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                }
                return true;
            }
            catch (Exception exception6)
            {
                if (Fx.IsFatal(exception6))
                {
                    throw;
                }
                this.HandleUnknownException(exception6);
                flag = false;
                return false;
            }
            finally
            {
                if (flag)
                {
                    this.throttle.Release();
                }
            }
            return true;
        }

        protected abstract IAsyncResult BeginReceive(TInnerChannel channel, AsyncCallback callback, object state);
        protected abstract IAsyncResult BeginReceive(TInnerChannel channel, TimeSpan timeout, AsyncCallback callback, object state);
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(ChannelDemuxerFilter filter) where TChannel: class, IChannel
        {
            return new InputQueueChannelListener<TChannel>(filter, this) { InnerChannelListener = this.innerListener };
        }

        protected abstract TInnerChannel CreateChannel(ChannelManagerBase channelManager, TInnerChannel innerChannel, TInnerItem firstItem);
        private bool EndAcceptChannel(IAsyncResult result, out TInnerChannel channel)
        {
            channel = default(TInnerChannel);
            bool flag = true;
            try
            {
                channel = this.innerListener.EndAcceptChannel(result);
                flag = ((TInnerChannel) channel) == null;
            }
            catch (CommunicationObjectFaultedException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                return false;
            }
            catch (CommunicationObjectAbortedException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
                return false;
            }
            catch (ObjectDisposedException exception3)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
                return false;
            }
            catch (CommunicationException exception4)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                }
                return true;
            }
            catch (TimeoutException exception5)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                }
                return true;
            }
            catch (Exception exception6)
            {
                if (Fx.IsFatal(exception6))
                {
                    throw;
                }
                this.HandleUnknownException(exception6);
                flag = false;
                return false;
            }
            finally
            {
                if (flag)
                {
                    this.throttle.Release();
                }
            }
            return (((TInnerChannel) channel) != null);
        }

        protected abstract void EndpointNotFound(TInnerChannel channel, TInnerItem item);
        protected abstract TInnerItem EndReceive(TInnerChannel channel, IAsyncResult result);
        protected abstract Message GetMessage(TInnerItem item);
        private void HandlePeekResult(IAsyncResult result)
        {
            TInnerItem local2;
            TInnerChannel channel = default(TInnerChannel);
            bool flag = false;
            bool flag2 = true;
            try
            {
                PeekAsyncResult<TInnerChannel, TInnerItem>.End(result, out channel, out local2);
                flag2 = local2 == null;
            }
            catch (ObjectDisposedException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                flag = true;
                return;
            }
            catch (CommunicationException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
                flag = true;
                return;
            }
            catch (TimeoutException exception3)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
                flag = true;
                return;
            }
            catch (Exception exception4)
            {
                if (Fx.IsFatal(exception4))
                {
                    throw;
                }
                this.HandleUnknownException(exception4);
                flag2 = false;
                return;
            }
            finally
            {
                if (flag && (channel != null))
                {
                    channel.Abort();
                }
                if (flag2)
                {
                    this.throttle.Release();
                }
            }
            if (local2 != null)
            {
                flag2 = true;
                try
                {
                    this.ProcessItem(channel, local2);
                    flag2 = false;
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
                    this.HandleUnknownException(exception7);
                    flag2 = false;
                }
                finally
                {
                    if (flag2)
                    {
                        this.throttle.Release();
                    }
                }
            }
        }

        protected void HandleUnknownException(Exception exception)
        {
            InputQueueChannelListener<TInnerChannel> listener = null;
            lock (this.ThisLock)
            {
                if (this.filterTable.Count > 0)
                {
                    KeyValuePair<MessageFilter, InputQueueChannelListener<TInnerChannel>>[] array = new KeyValuePair<MessageFilter, InputQueueChannelListener<TInnerChannel>>[this.filterTable.Count];
                    this.filterTable.CopyTo(array, 0);
                    listener = array[0].Value;
                    if (this.onItemDequeued == null)
                    {
                        this.onItemDequeued = new Action(this.OnItemDequeued);
                    }
                    listener.InputQueueAcceptor.EnqueueAndDispatch(exception, this.onItemDequeued, false);
                }
            }
        }

        private InputQueueChannelListener<TInnerChannel> MatchListener(Message message)
        {
            InputQueueChannelListener<TInnerChannel> data = null;
            lock (this.ThisLock)
            {
                if (this.filterTable.GetMatchingValue(message, out data))
                {
                    return data;
                }
            }
            return null;
        }

        private static void OnAcceptCompleteStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((SessionChannelDemuxer<TInnerChannel, TInnerItem>) result.AsyncState).OnStartAcceptingCallback(result);
            }
        }

        public IAsyncResult OnBeginOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.ShouldCloseInnerListener(filter, false))
            {
                bool flag = false;
                try
                {
                    IAsyncResult result = this.innerListener.BeginClose(timeout, callback, state);
                    flag = true;
                    return result;
                }
                finally
                {
                    if (!flag)
                    {
                        this.innerListener.Abort();
                    }
                }
            }
            return new CompletedAsyncResult(callback, state);
        }

        public IAsyncResult OnBeginOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult<TInnerChannel, TInnerItem>((SessionChannelDemuxer<TInnerChannel, TInnerItem>) this, filter, listener, timeout, callback, state);
        }

        public void OnEndOuterListenerClose(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                bool flag = false;
                try
                {
                    this.innerListener.EndClose(result);
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.innerListener.Abort();
                    }
                }
            }
        }

        public void OnEndOuterListenerOpen(IAsyncResult result)
        {
            OpenAsyncResult<TInnerChannel, TInnerItem>.End(result);
        }

        private void OnItemDequeued()
        {
            this.throttle.Release();
        }

        public void OnOuterListenerAbort(ChannelDemuxerFilter filter)
        {
            if (this.ShouldCloseInnerListener(filter, true))
            {
                this.innerListener.Abort();
            }
        }

        public void OnOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout)
        {
            if (this.ShouldCloseInnerListener(filter, false))
            {
                bool flag = false;
                try
                {
                    this.innerListener.Close(timeout);
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.innerListener.Abort();
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
                if (this.ShouldStartAccepting(filter, listener))
                {
                    try
                    {
                        this.innerListener.Open(helper.RemainingTime());
                        this.StartAccepting(true);
                        lock (this.ThisLock)
                        {
                            if (this.abortOngoingOpen)
                            {
                                this.innerListener.Abort();
                            }
                        }
                        return;
                    }
                    catch (Exception exception)
                    {
                        this.pendingExceptionOnOpen = exception;
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

        private static void OnPeekCompleteStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                SessionChannelDemuxer<TInnerChannel, TInnerItem> asyncState = (SessionChannelDemuxer<TInnerChannel, TInnerItem>) result.AsyncState;
                bool flag = true;
                try
                {
                    asyncState.HandlePeekResult(result);
                    flag = false;
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (ObjectDisposedException exception2)
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
                    asyncState.HandleUnknownException(exception3);
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        asyncState.throttle.Release();
                    }
                }
            }
        }

        private void OnStartAcceptingCallback(object state)
        {
            IAsyncResult result = (IAsyncResult) state;
            TInnerChannel channel = default(TInnerChannel);
            if ((result == null) || this.EndAcceptChannel(result, out channel))
            {
                this.StartAccepting(channel);
            }
        }

        private void PeekChannel(TInnerChannel channel)
        {
            bool flag = true;
            try
            {
                IAsyncResult result = new PeekAsyncResult<TInnerChannel, TInnerItem>((SessionChannelDemuxer<TInnerChannel, TInnerItem>) this, channel, SessionChannelDemuxer<TInnerChannel, TInnerItem>.onPeekComplete, this);
                flag = false;
                if (!result.CompletedSynchronously)
                {
                    return;
                }
                channel = default(TInnerChannel);
                this.HandlePeekResult(result);
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
            catch (ObjectDisposedException exception3)
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
                this.HandleUnknownException(exception4);
                flag = false;
            }
            if (channel != null)
            {
                channel.Abort();
            }
            if (flag)
            {
                this.throttle.Release();
            }
        }

        private void ProcessItem(TInnerChannel channel, TInnerItem item)
        {
            InputQueueChannelListener<TInnerChannel> channelManager = null;
            TInnerChannel local = default(TInnerChannel);
            bool flag = true;
            try
            {
                Message message = this.GetMessage(item);
                try
                {
                    channelManager = this.MatchListener(message);
                    flag = channelManager == null;
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    return;
                }
                catch (MultipleFilterMatchesException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                    return;
                }
                catch (XmlException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                    return;
                }
                finally
                {
                    if (flag)
                    {
                        this.throttle.Release();
                    }
                }
                if (channelManager == null)
                {
                    try
                    {
                        throw TraceUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("UnableToDemuxChannel", new object[] { message.Headers.Action })), message);
                    }
                    catch (EndpointNotFoundException exception4)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                        }
                        this.EndpointNotFound(channel, item);
                        channel = default(TInnerChannel);
                        item = default(TInnerItem);
                    }
                    return;
                }
                local = this.CreateChannel(channelManager, channel, item);
                channel = default(TInnerChannel);
                item = default(TInnerItem);
            }
            finally
            {
                if (item != null)
                {
                    this.AbortItem(item);
                }
                if (channel != null)
                {
                    channel.Abort();
                }
            }
            bool flag2 = false;
            try
            {
                if (this.onItemDequeued == null)
                {
                    this.onItemDequeued = new Action(this.OnItemDequeued);
                }
                channelManager.InputQueueAcceptor.EnqueueAndDispatch(local, this.onItemDequeued, false);
                flag2 = true;
            }
            catch (Exception exception5)
            {
                if (Fx.IsFatal(exception5))
                {
                    throw;
                }
                this.HandleUnknownException(exception5);
            }
            finally
            {
                if (!flag2)
                {
                    this.throttle.Release();
                    local.Abort();
                }
            }
        }

        private static void ScheduleAcceptStatic(object state)
        {
            ActionItem.Schedule(SessionChannelDemuxer<TInnerChannel, TInnerItem>.startAcceptStatic, state);
        }

        private bool ShouldCloseInnerListener(ChannelDemuxerFilter filter, bool aborted)
        {
            lock (this.ThisLock)
            {
                if (this.filterTable.ContainsKey(filter.Filter))
                {
                    this.filterTable.Remove(filter.Filter);
                    if (--this.openCount == 0)
                    {
                        if (aborted)
                        {
                            this.abortOngoingOpen = true;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ShouldStartAccepting(ChannelDemuxerFilter filter, IChannelListener listener)
        {
            lock (this.ThisLock)
            {
                if ((listener.State == CommunicationState.Closed) || (listener.State == CommunicationState.Closing))
                {
                    return false;
                }
                this.filterTable.Add(filter.Filter, (InputQueueChannelListener<TInnerChannel>) listener, filter.Priority);
                if (++this.openCount == 1)
                {
                    this.abortOngoingOpen = false;
                    return true;
                }
            }
            return false;
        }

        private void StartAccepting(bool requiresThrottle)
        {
            IAsyncResult result;
            if (this.BeginAcceptChannel(requiresThrottle, out result) && ((result == null) || result.CompletedSynchronously))
            {
                ActionItem.Schedule(this.OnStartAccepting, result);
            }
        }

        private void StartAccepting(TInnerChannel channelToPeek)
        {
            IAsyncResult result;
            bool flag;
        Label_0000:
            flag = this.BeginAcceptChannel(true, out result);
            if (channelToPeek != null)
            {
                if (flag && ((result == null) || result.CompletedSynchronously))
                {
                    ActionItem.Schedule(this.OnStartAccepting, result);
                }
                this.PeekChannel(channelToPeek);
            }
            else if (flag && ((result == null) || (result.CompletedSynchronously && this.EndAcceptChannel(result, out channelToPeek))))
            {
                goto Label_0000;
            }
        }

        private static void StartAcceptStatic(object state)
        {
            ((SessionChannelDemuxer<TInnerChannel, TInnerItem>) state).StartAccepting(false);
        }

        private void ThrowPendingOpenExceptionIfAny()
        {
            if (this.pendingExceptionOnOpen != null)
            {
                if (this.pendingExceptionOnOpen is CommunicationObjectAbortedException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("PreviousChannelDemuxerOpenFailed", new object[] { this.pendingExceptionOnOpen.ToString() })));
                }
                if (this.pendingExceptionOnOpen is CommunicationObjectFaultedException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationObjectFaultedException(System.ServiceModel.SR.GetString("PreviousChannelDemuxerOpenFailed", new object[] { this.pendingExceptionOnOpen.ToString() })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(System.ServiceModel.SR.GetString("PreviousChannelDemuxerOpenFailed", new object[] { this.pendingExceptionOnOpen.ToString() })));
            }
        }

        protected IChannelDemuxFailureHandler DemuxFailureHandler
        {
            get
            {
                return this.demuxFailureHandler;
            }
        }

        private Action<object> OnStartAccepting
        {
            get
            {
                if (this.onStartAccepting == null)
                {
                    this.onStartAccepting = new Action<object>(this.OnStartAcceptingCallback);
                }
                return this.onStartAccepting;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private SessionChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer;
            private ChannelDemuxerFilter filter;
            private IChannelListener listener;
            private static AsyncCallback openListenerCallback;
            private bool startAccepting;
            private TimeoutHelper timeoutHelper;
            private static FastAsyncCallback waitOverCallback;

            static OpenAsyncResult()
            {
                SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.waitOverCallback = new FastAsyncCallback(SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.WaitOverCallback);
                SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.openListenerCallback = Fx.ThunkCallback(new AsyncCallback(SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.OpenListenerCallback));
            }

            public OpenAsyncResult(SessionChannelDemuxer<TInnerChannel, TInnerItem> channelDemuxer, ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channelDemuxer = channelDemuxer;
                this.filter = filter;
                this.listener = listener;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (this.channelDemuxer.openSemaphore.EnterAsync(this.timeoutHelper.RemainingTime(), SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.waitOverCallback, this))
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

            private void Cleanup()
            {
                this.channelDemuxer.openSemaphore.Exit();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult>(result);
            }

            private void OnEndInnerListenerOpen(IAsyncResult result)
            {
                this.channelDemuxer.innerListener.EndOpen(result);
                this.channelDemuxer.StartAccepting(true);
                lock (this.channelDemuxer.ThisLock)
                {
                    if (this.channelDemuxer.abortOngoingOpen)
                    {
                        this.channelDemuxer.innerListener.Abort();
                    }
                }
            }

            private bool OnStartAccepting()
            {
                bool flag;
                try
                {
                    IAsyncResult result = this.channelDemuxer.innerListener.BeginOpen(this.timeoutHelper.RemainingTime(), SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult.openListenerCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.OnEndInnerListenerOpen(result);
                    flag = true;
                }
                catch (Exception exception)
                {
                    this.channelDemuxer.pendingExceptionOnOpen = exception;
                    throw;
                }
                return flag;
            }

            private bool OnWaitOver()
            {
                this.startAccepting = this.channelDemuxer.ShouldStartAccepting(this.filter, this.listener);
                if (!this.startAccepting)
                {
                    this.channelDemuxer.ThrowPendingOpenExceptionIfAny();
                    return true;
                }
                return this.OnStartAccepting();
            }

            private static void OpenListenerCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult asyncState = (SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.OnEndInnerListenerOpen(result);
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
                        asyncState.channelDemuxer.pendingExceptionOnOpen = exception;
                    }
                    asyncState.Cleanup();
                    asyncState.Complete(false, exception);
                }
            }

            private static void WaitOverCallback(object state, Exception asyncException)
            {
                SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult result = (SessionChannelDemuxer<TInnerChannel, TInnerItem>.OpenAsyncResult) state;
                bool flag = false;
                Exception exception = asyncException;
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

        private class PeekAsyncResult : AsyncResult
        {
            private TInnerChannel channel;
            private SessionChannelDemuxer<TInnerChannel, TInnerItem> demuxer;
            private TInnerItem item;
            private static AsyncCallback onOpenComplete;
            private static AsyncCallback onReceiveComplete;

            static PeekAsyncResult()
            {
                SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult.onOpenComplete = Fx.ThunkCallback(new AsyncCallback(SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult.OnOpenCompleteStatic));
                SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult.onReceiveComplete = Fx.ThunkCallback(new AsyncCallback(SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult.OnReceiveCompleteStatic));
            }

            public PeekAsyncResult(SessionChannelDemuxer<TInnerChannel, TInnerItem> demuxer, TInnerChannel channel, AsyncCallback callback, object state) : base(callback, state)
            {
                this.demuxer = demuxer;
                this.channel = channel;
                IAsyncResult result = this.channel.BeginOpen(SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult.onOpenComplete, this);
                if (result.CompletedSynchronously && this.HandleOpenComplete(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result, out TInnerChannel channel, out TInnerItem item)
            {
                SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult result2 = AsyncResult.End<SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult>(result);
                channel = result2.channel;
                item = result2.item;
            }

            private bool HandleOpenComplete(IAsyncResult result)
            {
                IAsyncResult result2;
                this.channel.EndOpen(result);
                if (this.demuxer.peekTimeout == ChannelDemuxer.UseDefaultReceiveTimeout)
                {
                    result2 = this.demuxer.BeginReceive(this.channel, SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult.onReceiveComplete, this);
                }
                else
                {
                    result2 = this.demuxer.BeginReceive(this.channel, this.demuxer.peekTimeout, SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult.onReceiveComplete, this);
                }
                if (result2.CompletedSynchronously)
                {
                    this.HandleReceiveComplete(result2);
                    return true;
                }
                return false;
            }

            private void HandleReceiveComplete(IAsyncResult result)
            {
                this.item = this.demuxer.EndReceive(this.channel, result);
            }

            private static void OnOpenCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult asyncState = (SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleOpenComplete(result);
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
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnReceiveCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult asyncState = (SessionChannelDemuxer<TInnerChannel, TInnerItem>.PeekAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.HandleReceiveComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }
    }
}

