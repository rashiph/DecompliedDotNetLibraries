namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;

    internal abstract class ReliableChannelBinder<TChannel> : IReliableChannelBinder where TChannel: class, IChannel
    {
        private bool aborted;
        private TimeSpan defaultCloseTimeout;
        private MaskingMode defaultMaskingMode;
        private TimeSpan defaultSendTimeout;
        private AsyncCallback onCloseChannelComplete;
        private CommunicationState state;
        private ChannelSynchronizer<TChannel> synchronizer;
        private object thisLock;

        public event EventHandler ConnectionLost;

        public event BinderExceptionHandler Faulted;

        public event BinderExceptionHandler OnException;

        protected ReliableChannelBinder(TChannel channel, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
        {
            this.thisLock = new object();
            if ((maskingMode != MaskingMode.None) && (maskingMode != MaskingMode.All))
            {
                throw Fx.AssertAndThrow("ReliableChannelBinder was implemented with only 2 default masking modes, None and All.");
            }
            this.defaultMaskingMode = maskingMode;
            this.defaultCloseTimeout = defaultCloseTimeout;
            this.defaultSendTimeout = defaultSendTimeout;
            this.synchronizer = new ChannelSynchronizer<TChannel>((ReliableChannelBinder<TChannel>) this, channel, faultMode);
        }

        public void Abort()
        {
            TChannel local;
            lock (this.ThisLock)
            {
                this.aborted = true;
                if (this.state == CommunicationState.Closed)
                {
                    return;
                }
                this.state = CommunicationState.Closing;
                local = this.synchronizer.StopSynchronizing(true);
                if (!this.MustCloseChannel)
                {
                    local = default(TChannel);
                }
            }
            this.synchronizer.UnblockWaiters();
            this.OnShutdown();
            this.OnAbort();
            if (local != null)
            {
                local.Abort();
            }
            this.TransitionToClosed();
        }

        protected virtual void AddOutputHeaders(Message message)
        {
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginClose(timeout, this.defaultMaskingMode, callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
        {
            TChannel local;
            this.ThrowIfTimeoutNegative(timeout);
            if (this.CloseCore(out local))
            {
                return new CompletedAsyncResult(callback, state);
            }
            return new CloseAsyncResult<TChannel>((ReliableChannelBinder<TChannel>) this, local, timeout, maskingMode, callback, state);
        }

        protected virtual IAsyncResult BeginCloseChannel(TChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channel.BeginClose(timeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfTimeoutNegative(timeout);
            if (this.OnOpening(this.defaultMaskingMode))
            {
                try
                {
                    return this.OnBeginOpen(timeout, callback, state);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.Fault(null);
                    if (this.defaultMaskingMode == MaskingMode.None)
                    {
                        throw;
                    }
                    this.RaiseOnException(exception);
                }
            }
            return new BinderCompletedAsyncResult<TChannel>(callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, timeout, this.defaultMaskingMode, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
        {
            SendAsyncResult<TChannel> result = new SendAsyncResult<TChannel>((ReliableChannelBinder<TChannel>) this, callback, state);
            result.Start(message, timeout, maskingMode);
            return result;
        }

        protected abstract IAsyncResult BeginTryGetChannel(TimeSpan timeout, AsyncCallback callback, object state);
        public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginTryReceive(timeout, this.defaultMaskingMode, callback, state);
        }

        public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
        {
            if (this.ValidateInputOperation(timeout))
            {
                return new TryReceiveAsyncResult<TChannel>((ReliableChannelBinder<TChannel>) this, timeout, maskingMode, callback, state);
            }
            return new CompletedAsyncResult(callback, state);
        }

        internal IAsyncResult BeginWaitForPendingOperations(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.synchronizer.BeginWaitForPendingOperations(timeout, callback, state);
        }

        public void Close(TimeSpan timeout)
        {
            this.Close(timeout, this.defaultMaskingMode);
        }

        public void Close(TimeSpan timeout, MaskingMode maskingMode)
        {
            TChannel local;
            this.ThrowIfTimeoutNegative(timeout);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!this.CloseCore(out local))
            {
                try
                {
                    this.OnShutdown();
                    this.OnClose(helper.RemainingTime());
                    if (local != null)
                    {
                        this.CloseChannel(local, helper.RemainingTime());
                    }
                    this.TransitionToClosed();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.Abort();
                    if (!this.HandleException(exception, maskingMode))
                    {
                        throw;
                    }
                }
            }
        }

        private void CloseChannel(TChannel channel)
        {
            if (!this.MustCloseChannel)
            {
                throw Fx.AssertAndThrow("MustCloseChannel is false when there is no receive loop and this method is called when there is a receive loop.");
            }
            if (this.onCloseChannelComplete == null)
            {
                this.onCloseChannelComplete = Fx.ThunkCallback(new AsyncCallback(this.OnCloseChannelComplete));
            }
            try
            {
                IAsyncResult result = channel.BeginClose(this.onCloseChannelComplete, channel);
                if (result.CompletedSynchronously)
                {
                    channel.EndClose(result);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.HandleException(exception, MaskingMode.All);
            }
        }

        protected virtual void CloseChannel(TChannel channel, TimeSpan timeout)
        {
            channel.Close(timeout);
        }

        private bool CloseCore(out TChannel channel)
        {
            channel = default(TChannel);
            bool flag = true;
            bool flag2 = false;
            lock (this.ThisLock)
            {
                if ((this.state == CommunicationState.Closing) || (this.state == CommunicationState.Closed))
                {
                    return true;
                }
                if (this.state == CommunicationState.Opened)
                {
                    this.state = CommunicationState.Closing;
                    channel = this.synchronizer.StopSynchronizing(true);
                    flag = false;
                    if (!this.MustCloseChannel)
                    {
                        channel = default(TChannel);
                    }
                    if (((TChannel) channel) != null)
                    {
                        switch (channel.State)
                        {
                            case CommunicationState.Created:
                            case CommunicationState.Opening:
                            case CommunicationState.Faulted:
                                flag2 = true;
                                goto Label_00AF;

                            case CommunicationState.Closing:
                            case CommunicationState.Closed:
                                channel = default(TChannel);
                                break;
                        }
                    }
                }
            }
        Label_00AF:
            this.synchronizer.UnblockWaiters();
            if (flag)
            {
                this.Abort();
                return true;
            }
            if (flag2)
            {
                channel.Abort();
                channel = default(TChannel);
            }
            return false;
        }

        public void EndClose(IAsyncResult result)
        {
            CloseAsyncResult<TChannel> result2 = result as CloseAsyncResult<TChannel>;
            if (result2 != null)
            {
                result2.End();
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        protected virtual void EndCloseChannel(TChannel channel, IAsyncResult result)
        {
            channel.EndClose(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            BinderCompletedAsyncResult<TChannel> result2 = result as BinderCompletedAsyncResult<TChannel>;
            if (result2 != null)
            {
                result2.End();
            }
            else
            {
                try
                {
                    this.OnEndOpen(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.Fault(null);
                    if (this.defaultMaskingMode == MaskingMode.None)
                    {
                        throw;
                    }
                    this.RaiseOnException(exception);
                    return;
                }
                this.synchronizer.StartSynchronizing();
                this.OnOpened();
            }
        }

        public void EndSend(IAsyncResult result)
        {
            SendAsyncResult<TChannel>.End(result);
        }

        protected abstract bool EndTryGetChannel(IAsyncResult result);
        public virtual bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            TryReceiveAsyncResult<TChannel> result2 = result as TryReceiveAsyncResult<TChannel>;
            if (result2 != null)
            {
                return result2.End(out requestContext);
            }
            CompletedAsyncResult.End(result);
            requestContext = null;
            return true;
        }

        public void EndWaitForPendingOperations(IAsyncResult result)
        {
            this.synchronizer.EndWaitForPendingOperations(result);
        }

        protected void Fault(Exception e)
        {
            lock (this.ThisLock)
            {
                if (this.state == CommunicationState.Created)
                {
                    throw Fx.AssertAndThrow("The binder should not detect the inner channel's faults until after the binder is opened.");
                }
                if ((this.state == CommunicationState.Faulted) || (this.state == CommunicationState.Closed))
                {
                    return;
                }
                this.state = CommunicationState.Faulted;
                this.synchronizer.StopSynchronizing(false);
            }
            this.synchronizer.UnblockWaiters();
            BinderExceptionHandler faulted = this.Faulted;
            if (faulted != null)
            {
                faulted(this, e);
            }
        }

        private Exception GetClosedException(MaskingMode maskingMode)
        {
            if (ReliableChannelBinderHelper.MaskHandled(maskingMode))
            {
                return null;
            }
            if (this.aborted)
            {
                return new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("CommunicationObjectAborted1", new object[] { base.GetType().ToString() }));
            }
            return new ObjectDisposedException(base.GetType().ToString());
        }

        private Exception GetClosedOrFaultedException(MaskingMode maskingMode)
        {
            if (this.state == CommunicationState.Faulted)
            {
                return this.GetFaultedException(maskingMode);
            }
            if ((this.state != CommunicationState.Closing) && (this.state != CommunicationState.Closed))
            {
                throw Fx.AssertAndThrow("Caller is attempting to get a terminal exception in a non-terminal state.");
            }
            return this.GetClosedException(maskingMode);
        }

        private Exception GetFaultedException(MaskingMode maskingMode)
        {
            if (ReliableChannelBinderHelper.MaskHandled(maskingMode))
            {
                return null;
            }
            return new CommunicationObjectFaultedException(System.ServiceModel.SR.GetString("CommunicationObjectFaulted1", new object[] { base.GetType().ToString() }));
        }

        public abstract ISession GetInnerSession();
        public void HandleException(Exception e)
        {
            this.HandleException(e, MaskingMode.All);
        }

        protected bool HandleException(Exception e, MaskingMode maskingMode)
        {
            if (this.TolerateFaults && (e is CommunicationObjectFaultedException))
            {
                return true;
            }
            if (this.IsHandleable(e))
            {
                return ReliableChannelBinderHelper.MaskHandled(maskingMode);
            }
            bool flag = ReliableChannelBinderHelper.MaskUnhandled(maskingMode);
            if (flag)
            {
                this.RaiseOnException(e);
            }
            return flag;
        }

        protected bool HandleException(Exception e, MaskingMode maskingMode, bool autoAborted)
        {
            return (((this.TolerateFaults && autoAborted) && (e is CommunicationObjectAbortedException)) || this.HandleException(e, maskingMode));
        }

        protected abstract bool HasSecuritySession(TChannel channel);
        public bool IsHandleable(Exception e)
        {
            if (e is ProtocolException)
            {
                return false;
            }
            return ((e is CommunicationException) || (e is TimeoutException));
        }

        protected abstract void OnAbort();
        protected abstract IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        protected virtual IAsyncResult OnBeginSend(TChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("The derived class does not support the BeginSend operation.");
        }

        protected virtual IAsyncResult OnBeginTryReceive(TChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("The derived class does not support the BeginTryReceive operation.");
        }

        protected abstract void OnClose(TimeSpan timeout);
        private void OnCloseChannelComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                TChannel asyncState = (TChannel) result.AsyncState;
                try
                {
                    asyncState.EndClose(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.HandleException(exception, MaskingMode.All);
                }
            }
        }

        protected abstract void OnEndClose(IAsyncResult result);
        protected abstract void OnEndOpen(IAsyncResult result);
        protected virtual void OnEndSend(TChannel channel, IAsyncResult result)
        {
            throw Fx.AssertAndThrow("The derived class does not support the EndSend operation.");
        }

        protected virtual bool OnEndTryReceive(TChannel channel, IAsyncResult result, out RequestContext requestContext)
        {
            throw Fx.AssertAndThrow("The derived class does not support the EndTryReceive operation.");
        }

        private void OnInnerChannelFaulted()
        {
            if (this.TolerateFaults)
            {
                EventHandler connectionLost = this.ConnectionLost;
                if (connectionLost != null)
                {
                    connectionLost(this, EventArgs.Empty);
                }
            }
        }

        protected abstract void OnOpen(TimeSpan timeout);
        private void OnOpened()
        {
            lock (this.ThisLock)
            {
                if (this.state == CommunicationState.Opening)
                {
                    this.state = CommunicationState.Opened;
                }
            }
        }

        private bool OnOpening(MaskingMode maskingMode)
        {
            lock (this.ThisLock)
            {
                if (this.state != CommunicationState.Created)
                {
                    Exception closedOrFaultedException = null;
                    if ((this.state == CommunicationState.Opening) || (this.state == CommunicationState.Opened))
                    {
                        if (!ReliableChannelBinderHelper.MaskUnhandled(maskingMode))
                        {
                            closedOrFaultedException = new InvalidOperationException(System.ServiceModel.SR.GetString("CommunicationObjectCannotBeModifiedInState", new object[] { base.GetType().ToString(), this.state.ToString() }));
                        }
                    }
                    else
                    {
                        closedOrFaultedException = this.GetClosedOrFaultedException(maskingMode);
                    }
                    if (closedOrFaultedException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(closedOrFaultedException);
                    }
                    return false;
                }
                this.state = CommunicationState.Opening;
                return true;
            }
        }

        protected virtual void OnSend(TChannel channel, Message message, TimeSpan timeout)
        {
            throw Fx.AssertAndThrow("The derived class does not support the Send operation.");
        }

        protected virtual void OnShutdown()
        {
        }

        protected virtual bool OnTryReceive(TChannel channel, TimeSpan timeout, out RequestContext requestContext)
        {
            throw Fx.AssertAndThrow("The derived class does not support the TryReceive operation.");
        }

        public void Open(TimeSpan timeout)
        {
            this.ThrowIfTimeoutNegative(timeout);
            if (this.OnOpening(this.defaultMaskingMode))
            {
                try
                {
                    this.OnOpen(timeout);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.Fault(null);
                    if (this.defaultMaskingMode == MaskingMode.None)
                    {
                        throw;
                    }
                    this.RaiseOnException(exception);
                    return;
                }
                this.synchronizer.StartSynchronizing();
                this.OnOpened();
            }
        }

        private void RaiseOnException(Exception e)
        {
            BinderExceptionHandler onException = this.OnException;
            if (onException != null)
            {
                onException(this, e);
            }
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.Send(message, timeout, this.defaultMaskingMode);
        }

        public void Send(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            if (this.ValidateOutputOperation(message, timeout, maskingMode))
            {
                bool autoAborted = false;
                try
                {
                    TChannel local;
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    if (!this.synchronizer.TryGetChannelForOutput(helper.RemainingTime(), maskingMode, out local))
                    {
                        if (!ReliableChannelBinderHelper.MaskHandled(maskingMode))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnSend", new object[] { timeout })));
                        }
                    }
                    else if (local != null)
                    {
                        this.AddOutputHeaders(message);
                        try
                        {
                            this.OnSend(local, message, helper.RemainingTime());
                        }
                        finally
                        {
                            autoAborted = this.Synchronizer.Aborting;
                            this.synchronizer.ReturnChannel();
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!this.HandleException(exception, maskingMode, autoAborted))
                    {
                        throw;
                    }
                }
            }
        }

        public void SetMaskingMode(RequestContext context, MaskingMode maskingMode)
        {
            ((BinderRequestContext<TChannel>) context).SetMaskingMode(maskingMode);
        }

        private bool ThrowIfNotOpenedAndNotMasking(MaskingMode maskingMode, bool throwDisposed)
        {
            lock (this.ThisLock)
            {
                if (this.State == CommunicationState.Created)
                {
                    throw Fx.AssertAndThrow("Messaging operations cannot be called when the binder is in the Created state.");
                }
                if (this.State == CommunicationState.Opening)
                {
                    throw Fx.AssertAndThrow("Messaging operations cannot be called when the binder is in the Opening state.");
                }
                if (this.State == CommunicationState.Opened)
                {
                    return true;
                }
                if (throwDisposed)
                {
                    Exception closedOrFaultedException = this.GetClosedOrFaultedException(maskingMode);
                    if (closedOrFaultedException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(closedOrFaultedException);
                    }
                }
                return false;
            }
        }

        private void ThrowIfTimeoutNegative(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, "SFxTimeoutOutOfRange0"));
            }
        }

        private void TransitionToClosed()
        {
            lock (this.ThisLock)
            {
                if (((this.state != CommunicationState.Closing) && (this.state != CommunicationState.Closed)) && (this.state != CommunicationState.Faulted))
                {
                    throw Fx.AssertAndThrow("Caller cannot transition to the Closed state from a non-terminal state.");
                }
                this.state = CommunicationState.Closed;
            }
        }

        protected abstract bool TryGetChannel(TimeSpan timeout);
        public virtual bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            return this.TryReceive(timeout, out requestContext, this.defaultMaskingMode);
        }

        public virtual bool TryReceive(TimeSpan timeout, out RequestContext requestContext, MaskingMode maskingMode)
        {
            if (maskingMode != MaskingMode.None)
            {
                throw Fx.AssertAndThrow("This method was implemented only for the case where we do not mask exceptions.");
            }
            if (!this.ValidateInputOperation(timeout))
            {
                requestContext = null;
                return true;
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            while (true)
            {
                bool autoAborted = false;
                try
                {
                    TChannel local;
                    bool flag2 = !this.synchronizer.TryGetChannelForInput(this.CanGetChannelForReceive, helper.RemainingTime(), out local);
                    if (local == null)
                    {
                        requestContext = null;
                        return flag2;
                    }
                    try
                    {
                        flag2 = this.OnTryReceive(local, helper.RemainingTime(), out requestContext);
                        if (!flag2 || (requestContext != null))
                        {
                            return flag2;
                        }
                        this.synchronizer.OnReadEof();
                    }
                    finally
                    {
                        autoAborted = this.Synchronizer.Aborting;
                        this.synchronizer.ReturnChannel();
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!this.HandleException(exception, maskingMode, autoAborted))
                    {
                        throw;
                    }
                }
            }
        }

        protected bool ValidateInputOperation(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, "SFxTimeoutOutOfRange0"));
            }
            return this.ThrowIfNotOpenedAndNotMasking(MaskingMode.All, false);
        }

        protected bool ValidateOutputOperation(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, "SFxTimeoutOutOfRange0"));
            }
            return this.ThrowIfNotOpenedAndNotMasking(maskingMode, true);
        }

        internal void WaitForPendingOperations(TimeSpan timeout)
        {
            this.synchronizer.WaitForPendingOperations(timeout);
        }

        protected RequestContext WrapMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }
            return new MessageRequestContext<TChannel>((ReliableChannelBinder<TChannel>) this, message);
        }

        public RequestContext WrapRequestContext(RequestContext context)
        {
            if (context == null)
            {
                return null;
            }
            if (!this.TolerateFaults && (this.defaultMaskingMode == MaskingMode.None))
            {
                return context;
            }
            return new RequestRequestContext<TChannel>((ReliableChannelBinder<TChannel>) this, context, context.RequestMessage);
        }

        protected abstract bool CanGetChannelForReceive { get; }

        public abstract bool CanSendAsynchronously { get; }

        public IChannel Channel
        {
            get
            {
                return this.synchronizer.CurrentChannel;
            }
        }

        public virtual ChannelParameterCollection ChannelParameters
        {
            get
            {
                return null;
            }
        }

        public bool Connected
        {
            get
            {
                return this.synchronizer.Connected;
            }
        }

        public MaskingMode DefaultMaskingMode
        {
            get
            {
                return this.defaultMaskingMode;
            }
        }

        public TimeSpan DefaultSendTimeout
        {
            get
            {
                return this.defaultSendTimeout;
            }
        }

        public abstract bool HasSession { get; }

        public abstract EndpointAddress LocalAddress { get; }

        protected abstract bool MustCloseChannel { get; }

        protected abstract bool MustOpenChannel { get; }

        public abstract EndpointAddress RemoteAddress { get; }

        public CommunicationState State
        {
            get
            {
                return this.state;
            }
        }

        protected ChannelSynchronizer<TChannel> Synchronizer
        {
            get
            {
                return this.synchronizer;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private bool TolerateFaults
        {
            get
            {
                return this.synchronizer.TolerateFaults;
            }
        }

        private sealed class BinderCompletedAsyncResult : CompletedAsyncResult
        {
            public BinderCompletedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }

            public void End()
            {
                CompletedAsyncResult.End(this);
            }
        }

        private abstract class BinderRequestContext : RequestContextBase
        {
            private ReliableChannelBinder<TChannel> binder;
            private System.ServiceModel.Channels.MaskingMode maskingMode;

            public BinderRequestContext(ReliableChannelBinder<TChannel> binder, Message message) : base(message, binder.defaultCloseTimeout, binder.defaultSendTimeout)
            {
                this.binder = binder;
                this.maskingMode = binder.defaultMaskingMode;
            }

            public void SetMaskingMode(System.ServiceModel.Channels.MaskingMode maskingMode)
            {
                if (this.binder.defaultMaskingMode != System.ServiceModel.Channels.MaskingMode.All)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                this.maskingMode = maskingMode;
            }

            protected ReliableChannelBinder<TChannel> Binder
            {
                get
                {
                    return this.binder;
                }
            }

            protected System.ServiceModel.Channels.MaskingMode MaskingMode
            {
                get
                {
                    return this.maskingMode;
                }
            }
        }

        protected class ChannelSynchronizer
        {
            private bool aborting;
            private static Action<object> asyncGetChannelCallback;
            private ReliableChannelBinder<TChannel> binder;
            private int count;
            private TChannel currentChannel;
            private InterruptibleWaitObject drainEvent;
            private TolerateFaultsMode faultMode;
            private Queue<IWaiter<TChannel>> getChannelQueue;
            private bool innerChannelFaulted;
            private EventHandler onChannelFaulted;
            private State<TChannel> state;
            private object thisLock;
            private bool tolerateFaults;
            private Queue<IWaiter<TChannel>> waitQueue;

            static ChannelSynchronizer()
            {
                ReliableChannelBinder<TChannel>.ChannelSynchronizer.asyncGetChannelCallback = new Action<object>(ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncGetChannelCallback);
            }

            public ChannelSynchronizer(ReliableChannelBinder<TChannel> binder, TChannel channel, TolerateFaultsMode faultMode)
            {
                this.tolerateFaults = true;
                this.thisLock = new object();
                this.binder = binder;
                this.currentChannel = channel;
                this.faultMode = faultMode;
            }

            public TChannel AbortCurentChannel()
            {
                lock (this.ThisLock)
                {
                    if (!this.tolerateFaults)
                    {
                        throw Fx.AssertAndThrow("It is only valid to abort the current channel when masking faults");
                    }
                    if (this.state == State<TChannel>.ChannelOpening)
                    {
                        this.aborting = true;
                    }
                    else if (this.state == State<TChannel>.ChannelOpened)
                    {
                        if (this.count == 0)
                        {
                            this.state = State<TChannel>.NoChannel;
                        }
                        else
                        {
                            this.aborting = true;
                            this.state = State<TChannel>.ChannelClosing;
                        }
                    }
                    else
                    {
                        return default(TChannel);
                    }
                    return this.currentChannel;
                }
            }

            private static void AsyncGetChannelCallback(object state)
            {
                ((AsyncWaiter<TChannel>) state).GetChannel(false);
            }

            private IAsyncResult BeginTryGetChannel(bool canGetChannel, bool canCauseFault, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
            {
                TChannel data = default(TChannel);
                AsyncWaiter<TChannel> item = null;
                bool flag = false;
                bool flag2 = false;
                lock (this.ThisLock)
                {
                    if (!this.ThrowIfNecessary(maskingMode))
                    {
                        data = default(TChannel);
                    }
                    else if (this.state == State<TChannel>.ChannelOpened)
                    {
                        if (this.currentChannel == null)
                        {
                            throw Fx.AssertAndThrow("Field currentChannel cannot be null in the ChannelOpened state.");
                        }
                        this.count++;
                        data = this.currentChannel;
                    }
                    else if (!this.tolerateFaults && ((this.state == State<TChannel>.NoChannel) || (this.state == State<TChannel>.ChannelClosing)))
                    {
                        if (canCauseFault)
                        {
                            flag2 = true;
                        }
                        data = default(TChannel);
                    }
                    else if ((!canGetChannel || (this.state == State<TChannel>.ChannelOpening)) || (this.state == State<TChannel>.ChannelClosing))
                    {
                        item = new AsyncWaiter<TChannel>((ReliableChannelBinder<TChannel>.ChannelSynchronizer) this, canGetChannel, default(TChannel), timeout, maskingMode, this.binder.ChannelParameters, callback, state);
                        this.GetQueue(canGetChannel).Enqueue(item);
                    }
                    else
                    {
                        if (this.state != State<TChannel>.NoChannel)
                        {
                            throw Fx.AssertAndThrow("The state must be NoChannel.");
                        }
                        item = new AsyncWaiter<TChannel>((ReliableChannelBinder<TChannel>.ChannelSynchronizer) this, canGetChannel, this.GetCurrentChannelIfCreated(), timeout, maskingMode, this.binder.ChannelParameters, callback, state);
                        this.state = State<TChannel>.ChannelOpening;
                        flag = true;
                    }
                }
                if (flag2)
                {
                    this.binder.Fault(null);
                }
                if (item == null)
                {
                    return new CompletedAsyncResult<TChannel>(data, callback, state);
                }
                if (flag)
                {
                    item.GetChannel(true);
                    return item;
                }
                item.Wait();
                return item;
            }

            public IAsyncResult BeginTryGetChannelForInput(bool canGetChannel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.BeginTryGetChannel(canGetChannel, false, timeout, MaskingMode.All, callback, state);
            }

            public IAsyncResult BeginTryGetChannelForOutput(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
            {
                return this.BeginTryGetChannel(true, true, timeout, maskingMode, callback, state);
            }

            public IAsyncResult BeginWaitForPendingOperations(TimeSpan timeout, AsyncCallback callback, object state)
            {
                lock (this.ThisLock)
                {
                    if (this.drainEvent != null)
                    {
                        throw Fx.AssertAndThrow("The WaitForPendingOperations operation may only be invoked once.");
                    }
                    if (this.count > 0)
                    {
                        this.drainEvent = new InterruptibleWaitObject(false, false);
                    }
                }
                if (this.drainEvent != null)
                {
                    return this.drainEvent.BeginWait(timeout, callback, state);
                }
                return new SynchronizerCompletedAsyncResult<TChannel>(callback, state);
            }

            private bool CompleteSetChannel(IWaiter<TChannel> waiter, out TChannel channel)
            {
                if (waiter == null)
                {
                    throw Fx.AssertAndThrow("Argument waiter cannot be null.");
                }
                bool flag = false;
                lock (this.ThisLock)
                {
                    if (this.ValidateOpened())
                    {
                        channel = this.currentChannel;
                        return true;
                    }
                    channel = default(TChannel);
                    flag = this.state == State<TChannel>.Closed;
                }
                if (flag)
                {
                    waiter.Close();
                }
                else
                {
                    waiter.Fault();
                }
                return false;
            }

            public bool EndTryGetChannel(IAsyncResult result, out TChannel channel)
            {
                AsyncWaiter<TChannel> waiter = result as AsyncWaiter<TChannel>;
                if (waiter != null)
                {
                    return waiter.End(out channel);
                }
                channel = CompletedAsyncResult<TChannel>.End(result);
                return true;
            }

            public void EndWaitForPendingOperations(IAsyncResult result)
            {
                SynchronizerCompletedAsyncResult<TChannel> result2 = result as SynchronizerCompletedAsyncResult<TChannel>;
                if (result2 != null)
                {
                    result2.End();
                }
                else
                {
                    this.drainEvent.EndWait(result);
                }
            }

            public bool EnsureChannel()
            {
                bool flag = false;
                lock (this.ThisLock)
                {
                    if (this.ValidateOpened())
                    {
                        if (this.state != State<TChannel>.ChannelOpened)
                        {
                            if (this.state != State<TChannel>.NoChannel)
                            {
                                throw Fx.AssertAndThrow("The caller may only invoke this EnsureChannel during the CreateSequence negotiation. ChannelOpening and ChannelClosing are invalid states during this phase of the negotiation.");
                            }
                            if (!this.tolerateFaults)
                            {
                                flag = true;
                                goto Label_008C;
                            }
                            if (this.GetCurrentChannelIfCreated() != null)
                            {
                                return true;
                            }
                            if (!this.binder.TryGetChannel(TimeSpan.Zero))
                            {
                                goto Label_008C;
                            }
                            if (this.currentChannel == null)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            Label_008C:
                if (flag)
                {
                    this.binder.Fault(null);
                }
                return false;
            }

            private IWaiter<TChannel> GetChannelWaiter()
            {
                if ((this.getChannelQueue != null) && (this.getChannelQueue.Count != 0))
                {
                    return this.getChannelQueue.Dequeue();
                }
                return null;
            }

            private TChannel GetCurrentChannelIfCreated()
            {
                if (this.state != State<TChannel>.NoChannel)
                {
                    throw Fx.AssertAndThrow("This method may only be called in the NoChannel state.");
                }
                if ((this.currentChannel != null) && (this.currentChannel.State == CommunicationState.Created))
                {
                    return this.currentChannel;
                }
                return default(TChannel);
            }

            private Queue<IWaiter<TChannel>> GetQueue(bool canGetChannel)
            {
                if (canGetChannel)
                {
                    if (this.getChannelQueue == null)
                    {
                        this.getChannelQueue = new Queue<IWaiter<TChannel>>();
                    }
                    return this.getChannelQueue;
                }
                if (this.waitQueue == null)
                {
                    this.waitQueue = new Queue<IWaiter<TChannel>>();
                }
                return this.waitQueue;
            }

            private void OnChannelFaulted(object sender, EventArgs e)
            {
                TChannel local = (TChannel) sender;
                bool flag = false;
                bool flag2 = false;
                lock (this.ThisLock)
                {
                    if ((this.currentChannel != local) || !this.ValidateOpened())
                    {
                        return;
                    }
                    if (this.state == State<TChannel>.ChannelOpened)
                    {
                        if (this.count == 0)
                        {
                            local.Faulted -= this.onChannelFaulted;
                        }
                        flag = !this.tolerateFaults;
                        this.state = State<TChannel>.ChannelClosing;
                        this.innerChannelFaulted = true;
                        if (!flag && (this.count == 0))
                        {
                            this.state = State<TChannel>.NoChannel;
                            this.aborting = false;
                            flag2 = true;
                            this.innerChannelFaulted = false;
                        }
                    }
                }
                if (flag)
                {
                    this.binder.Fault(null);
                }
                local.Abort();
                if (flag2)
                {
                    this.binder.OnInnerChannelFaulted();
                }
            }

            private bool OnChannelOpened(IWaiter<TChannel> waiter)
            {
                if (waiter == null)
                {
                    throw Fx.AssertAndThrow("Argument waiter cannot be null.");
                }
                bool flag = false;
                bool flag2 = false;
                Queue<IWaiter<TChannel>> waiters = null;
                Queue<IWaiter<TChannel>> waitQueue = null;
                TChannel channel = default(TChannel);
                lock (this.ThisLock)
                {
                    if (this.currentChannel == null)
                    {
                        throw Fx.AssertAndThrow("Caller must ensure that field currentChannel is set before opening the channel.");
                    }
                    if (this.ValidateOpened())
                    {
                        if (this.state != State<TChannel>.ChannelOpening)
                        {
                            throw Fx.AssertAndThrow("This method may only be called in the ChannelOpening state.");
                        }
                        this.state = State<TChannel>.ChannelOpened;
                        this.SetTolerateFaults();
                        this.count++;
                        this.count += (this.getChannelQueue == null) ? 0 : this.getChannelQueue.Count;
                        this.count += (this.waitQueue == null) ? 0 : this.waitQueue.Count;
                        waiters = this.getChannelQueue;
                        waitQueue = this.waitQueue;
                        channel = this.currentChannel;
                        this.getChannelQueue = null;
                        this.waitQueue = null;
                    }
                    else
                    {
                        flag = this.state == State<TChannel>.Closed;
                        flag2 = this.state == State<TChannel>.Faulted;
                    }
                }
                if (flag)
                {
                    waiter.Close();
                    return false;
                }
                if (flag2)
                {
                    waiter.Fault();
                    return false;
                }
                this.SetWaiters(waiters, channel);
                this.SetWaiters(waitQueue, channel);
                return true;
            }

            private void OnGetChannelFailed()
            {
                IWaiter<TChannel> state = null;
                lock (this.ThisLock)
                {
                    if (!this.ValidateOpened())
                    {
                        return;
                    }
                    if (this.state != State<TChannel>.ChannelOpening)
                    {
                        throw Fx.AssertAndThrow("The state must be set to ChannelOpening before the caller attempts to open the channel.");
                    }
                    state = this.GetChannelWaiter();
                    if (state == null)
                    {
                        this.state = State<TChannel>.NoChannel;
                        return;
                    }
                }
                if (state is SyncWaiter<TChannel>)
                {
                    state.GetChannel(false);
                }
                else
                {
                    ActionItem.Schedule(ReliableChannelBinder<TChannel>.ChannelSynchronizer.asyncGetChannelCallback, state);
                }
            }

            public void OnReadEof()
            {
                lock (this.ThisLock)
                {
                    if (this.count <= 0)
                    {
                        throw Fx.AssertAndThrow("Caller must ensure that OnReadEof is called before ReturnChannel.");
                    }
                    if (this.ValidateOpened())
                    {
                        if ((this.state != State<TChannel>.ChannelOpened) && (this.state != State<TChannel>.ChannelClosing))
                        {
                            throw Fx.AssertAndThrow("Since count is positive, the only valid states are ChannelOpened and ChannelClosing.");
                        }
                        if (this.currentChannel.State != CommunicationState.Faulted)
                        {
                            this.state = State<TChannel>.ChannelClosing;
                        }
                    }
                }
            }

            private bool RemoveWaiter(IWaiter<TChannel> waiter)
            {
                Queue<IWaiter<TChannel>> queue = waiter.CanGetChannel ? this.getChannelQueue : this.waitQueue;
                bool flag = false;
                lock (this.ThisLock)
                {
                    if (!this.ValidateOpened())
                    {
                        return false;
                    }
                    for (int i = queue.Count; i > 0; i--)
                    {
                        IWaiter<TChannel> objB = queue.Dequeue();
                        if (object.ReferenceEquals(waiter, objB))
                        {
                            flag = true;
                        }
                        else
                        {
                            queue.Enqueue(objB);
                        }
                    }
                }
                return flag;
            }

            public void ReturnChannel()
            {
                bool flag2;
                TChannel channel = default(TChannel);
                IWaiter<TChannel> channelWaiter = null;
                bool flag = false;
                bool innerChannelFaulted = false;
                lock (this.ThisLock)
                {
                    if (this.count <= 0)
                    {
                        throw Fx.AssertAndThrow("Method ReturnChannel() can only be called after TryGetChannel or EndTryGetChannel returns a channel.");
                    }
                    this.count--;
                    flag2 = (this.count == 0) && (this.drainEvent != null);
                    if (this.ValidateOpened())
                    {
                        if ((this.state != State<TChannel>.ChannelOpened) && (this.state != State<TChannel>.ChannelClosing))
                        {
                            throw Fx.AssertAndThrow("ChannelOpened and ChannelClosing are the only 2 valid states when count is positive.");
                        }
                        if (this.currentChannel.State == CommunicationState.Faulted)
                        {
                            flag = !this.tolerateFaults;
                            this.innerChannelFaulted = true;
                            this.state = State<TChannel>.ChannelClosing;
                        }
                        if ((!flag && (this.state == State<TChannel>.ChannelClosing)) && (this.count == 0))
                        {
                            channel = this.currentChannel;
                            innerChannelFaulted = this.innerChannelFaulted;
                            this.innerChannelFaulted = false;
                            this.state = State<TChannel>.NoChannel;
                            this.aborting = false;
                            channelWaiter = this.GetChannelWaiter();
                            if (channelWaiter != null)
                            {
                                this.state = State<TChannel>.ChannelOpening;
                            }
                        }
                    }
                }
                if (flag)
                {
                    this.binder.Fault(null);
                }
                if (flag2)
                {
                    this.drainEvent.Set();
                }
                if (channel != null)
                {
                    channel.Faulted -= this.onChannelFaulted;
                    if (channel.State == CommunicationState.Opened)
                    {
                        this.binder.CloseChannel(channel);
                    }
                    else
                    {
                        channel.Abort();
                    }
                    if (channelWaiter != null)
                    {
                        channelWaiter.GetChannel(false);
                    }
                }
                if (innerChannelFaulted)
                {
                    this.binder.OnInnerChannelFaulted();
                }
            }

            public bool SetChannel(TChannel channel)
            {
                lock (this.ThisLock)
                {
                    if ((this.state != State<TChannel>.ChannelOpening) && (this.state != State<TChannel>.NoChannel))
                    {
                        throw Fx.AssertAndThrow("SetChannel is only valid in the NoChannel and ChannelOpening states");
                    }
                    if (!this.tolerateFaults)
                    {
                        throw Fx.AssertAndThrow("SetChannel is only valid when masking faults");
                    }
                    if (this.ValidateOpened())
                    {
                        this.currentChannel = channel;
                        return true;
                    }
                    return false;
                }
            }

            private void SetTolerateFaults()
            {
                if (this.faultMode == TolerateFaultsMode.Never)
                {
                    this.tolerateFaults = false;
                }
                else if (this.faultMode == TolerateFaultsMode.IfNotSecuritySession)
                {
                    this.tolerateFaults = !this.binder.HasSecuritySession(this.currentChannel);
                }
                if (this.onChannelFaulted == null)
                {
                    this.onChannelFaulted = new EventHandler(this.OnChannelFaulted);
                }
                this.currentChannel.Faulted += this.onChannelFaulted;
            }

            private void SetWaiters(Queue<IWaiter<TChannel>> waiters, TChannel channel)
            {
                if ((waiters != null) && (waiters.Count > 0))
                {
                    foreach (IWaiter<TChannel> waiter in waiters)
                    {
                        waiter.Set(channel);
                    }
                }
            }

            public void StartSynchronizing()
            {
                lock (this.ThisLock)
                {
                    if (this.state == State<TChannel>.Created)
                    {
                        this.state = State<TChannel>.NoChannel;
                    }
                    else
                    {
                        if (this.state != State<TChannel>.Closed)
                        {
                            throw Fx.AssertAndThrow("Abort is the only operation that can race with Open.");
                        }
                        goto Label_008E;
                    }
                    if ((((this.currentChannel != null) || this.binder.TryGetChannel(TimeSpan.Zero)) && (this.currentChannel != null)) && !this.binder.MustOpenChannel)
                    {
                        this.state = State<TChannel>.ChannelOpened;
                        this.SetTolerateFaults();
                    }
                Label_008E:;
                }
            }

            public TChannel StopSynchronizing(bool close)
            {
                lock (this.ThisLock)
                {
                    if ((this.state != State<TChannel>.Faulted) && (this.state != State<TChannel>.Closed))
                    {
                        this.state = close ? State<TChannel>.Closed : State<TChannel>.Faulted;
                        if ((this.currentChannel != null) && (this.onChannelFaulted != null))
                        {
                            this.currentChannel.Faulted -= this.onChannelFaulted;
                        }
                    }
                    return this.currentChannel;
                }
            }

            private bool ThrowIfNecessary(MaskingMode maskingMode)
            {
                Exception closedException;
                if (this.ValidateOpened())
                {
                    return true;
                }
                if (this.state == State<TChannel>.Closed)
                {
                    closedException = this.binder.GetClosedException(maskingMode);
                }
                else
                {
                    closedException = this.binder.GetFaultedException(maskingMode);
                }
                if (closedException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(closedException);
                }
                return false;
            }

            private bool TryGetChannel(bool canGetChannel, bool canCauseFault, TimeSpan timeout, MaskingMode maskingMode, out TChannel channel)
            {
                SyncWaiter<TChannel> item = null;
                bool flag = false;
                bool flag2 = false;
                lock (this.ThisLock)
                {
                    if (!this.ThrowIfNecessary(maskingMode))
                    {
                        channel = default(TChannel);
                        return true;
                    }
                    if (this.state == State<TChannel>.ChannelOpened)
                    {
                        if (this.currentChannel == null)
                        {
                            throw Fx.AssertAndThrow("Field currentChannel cannot be null in the ChannelOpened state.");
                        }
                        this.count++;
                        channel = this.currentChannel;
                        return true;
                    }
                    if (!this.tolerateFaults && ((this.state == State<TChannel>.ChannelClosing) || (this.state == State<TChannel>.NoChannel)))
                    {
                        if (!canCauseFault)
                        {
                            channel = default(TChannel);
                            return true;
                        }
                        flag = true;
                    }
                    else if ((!canGetChannel || (this.state == State<TChannel>.ChannelOpening)) || (this.state == State<TChannel>.ChannelClosing))
                    {
                        item = new SyncWaiter<TChannel>((ReliableChannelBinder<TChannel>.ChannelSynchronizer) this, canGetChannel, default(TChannel), timeout, maskingMode, this.binder.ChannelParameters);
                        this.GetQueue(canGetChannel).Enqueue(item);
                    }
                    else
                    {
                        if (this.state != State<TChannel>.NoChannel)
                        {
                            throw Fx.AssertAndThrow("The state must be NoChannel.");
                        }
                        item = new SyncWaiter<TChannel>((ReliableChannelBinder<TChannel>.ChannelSynchronizer) this, canGetChannel, this.GetCurrentChannelIfCreated(), timeout, maskingMode, this.binder.ChannelParameters);
                        this.state = State<TChannel>.ChannelOpening;
                        flag2 = true;
                    }
                }
                if (flag)
                {
                    this.binder.Fault(null);
                    channel = default(TChannel);
                    return true;
                }
                if (flag2)
                {
                    item.GetChannel(true);
                }
                return item.TryWait(out channel);
            }

            public bool TryGetChannelForInput(bool canGetChannel, TimeSpan timeout, out TChannel channel)
            {
                return this.TryGetChannel(canGetChannel, false, timeout, MaskingMode.All, out channel);
            }

            public bool TryGetChannelForOutput(TimeSpan timeout, MaskingMode maskingMode, out TChannel channel)
            {
                return this.TryGetChannel(true, true, timeout, maskingMode, out channel);
            }

            public void UnblockWaiters()
            {
                Queue<IWaiter<TChannel>> getChannelQueue;
                Queue<IWaiter<TChannel>> waitQueue;
                lock (this.ThisLock)
                {
                    getChannelQueue = this.getChannelQueue;
                    waitQueue = this.waitQueue;
                    this.getChannelQueue = null;
                    this.waitQueue = null;
                }
                bool close = this.state == State<TChannel>.Closed;
                this.UnblockWaiters(getChannelQueue, close);
                this.UnblockWaiters(waitQueue, close);
            }

            private void UnblockWaiters(Queue<IWaiter<TChannel>> waiters, bool close)
            {
                if ((waiters != null) && (waiters.Count > 0))
                {
                    foreach (IWaiter<TChannel> waiter in waiters)
                    {
                        if (close)
                        {
                            waiter.Close();
                        }
                        else
                        {
                            waiter.Fault();
                        }
                    }
                }
            }

            private bool ValidateOpened()
            {
                if (this.state == State<TChannel>.Created)
                {
                    throw Fx.AssertAndThrow("This operation expects that the synchronizer has been opened.");
                }
                return ((this.state != State<TChannel>.Closed) && (this.state != State<TChannel>.Faulted));
            }

            public void WaitForPendingOperations(TimeSpan timeout)
            {
                lock (this.ThisLock)
                {
                    if (this.drainEvent != null)
                    {
                        throw Fx.AssertAndThrow("The WaitForPendingOperations operation may only be invoked once.");
                    }
                    if (this.count > 0)
                    {
                        this.drainEvent = new InterruptibleWaitObject(false, false);
                    }
                }
                if (this.drainEvent != null)
                {
                    this.drainEvent.Wait(timeout);
                }
            }

            public bool Aborting
            {
                get
                {
                    return this.aborting;
                }
            }

            public bool Connected
            {
                get
                {
                    if (this.state != State<TChannel>.ChannelOpened)
                    {
                        return (this.state == State<TChannel>.ChannelOpening);
                    }
                    return true;
                }
            }

            public TChannel CurrentChannel
            {
                get
                {
                    return this.currentChannel;
                }
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }

            public bool TolerateFaults
            {
                get
                {
                    return this.tolerateFaults;
                }
            }

            public sealed class AsyncWaiter : AsyncResult, ReliableChannelBinder<TChannel>.ChannelSynchronizer.IWaiter
            {
                private bool canGetChannel;
                private TChannel channel;
                private ChannelParameterCollection channelParameters;
                private bool isSynchronous;
                private MaskingMode maskingMode;
                private static AsyncCallback onOpenComplete;
                private static Action<object> onTimeoutElapsed;
                private static AsyncCallback onTryGetChannelComplete;
                private ReliableChannelBinder<TChannel>.ChannelSynchronizer synchronizer;
                private bool timedOut;
                private TimeoutHelper timeoutHelper;
                private IOThreadTimer timer;
                private bool timerCancelled;

                static AsyncWaiter()
                {
                    ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.onOpenComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.OnOpenComplete));
                    ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.onTimeoutElapsed = new Action<object>(ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.OnTimeoutElapsed);
                    ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.onTryGetChannelComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.OnTryGetChannelComplete));
                }

                public AsyncWaiter(ReliableChannelBinder<TChannel>.ChannelSynchronizer synchronizer, bool canGetChannel, TChannel channel, TimeSpan timeout, MaskingMode maskingMode, ChannelParameterCollection channelParameters, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.isSynchronous = true;
                    if (!canGetChannel && (channel != null))
                    {
                        throw Fx.AssertAndThrow("This waiter must wait for a channel thus argument channel must be null.");
                    }
                    this.synchronizer = synchronizer;
                    this.canGetChannel = canGetChannel;
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.maskingMode = maskingMode;
                    this.channelParameters = channelParameters;
                }

                private void CancelTimer()
                {
                    lock (this.ThisLock)
                    {
                        if (!this.timerCancelled)
                        {
                            if (this.timer != null)
                            {
                                this.timer.Cancel();
                            }
                            this.timerCancelled = true;
                        }
                    }
                }

                public void Close()
                {
                    this.CancelTimer();
                    this.channel = default(TChannel);
                    base.Complete(false, this.synchronizer.binder.GetClosedException(this.maskingMode));
                }

                private bool CompleteOpen(IAsyncResult result)
                {
                    this.channel.EndOpen(result);
                    return this.OnChannelOpened();
                }

                private bool CompleteTryGetChannel(IAsyncResult result)
                {
                    if (!this.synchronizer.binder.EndTryGetChannel(result))
                    {
                        this.timedOut = true;
                        this.OnGetChannelFailed();
                        return true;
                    }
                    if (this.synchronizer.CompleteSetChannel(this, out this.channel))
                    {
                        return this.OpenChannel();
                    }
                    if (!base.IsCompleted)
                    {
                        throw Fx.AssertAndThrow("CompleteSetChannel must complete the IWaiter if it returns false.");
                    }
                    return false;
                }

                public bool End(out TChannel channel)
                {
                    AsyncResult.End<ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter>(this);
                    channel = this.channel;
                    return !this.timedOut;
                }

                public void Fault()
                {
                    this.CancelTimer();
                    this.channel = default(TChannel);
                    base.Complete(false, this.synchronizer.binder.GetFaultedException(this.maskingMode));
                }

                private bool GetChannel()
                {
                    if (this.channel != null)
                    {
                        return this.OpenChannel();
                    }
                    IAsyncResult result = this.synchronizer.binder.BeginTryGetChannel(this.timeoutHelper.RemainingTime(), ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.onTryGetChannelComplete, this);
                    return (result.CompletedSynchronously && this.CompleteTryGetChannel(result));
                }

                public void GetChannel(bool onUserThread)
                {
                    if (!this.CanGetChannel)
                    {
                        throw Fx.AssertAndThrow("This waiter must wait for a channel thus the caller cannot attempt to get a channel.");
                    }
                    this.isSynchronous = onUserThread;
                    if (onUserThread)
                    {
                        bool flag = true;
                        try
                        {
                            if (this.GetChannel())
                            {
                                base.Complete(true);
                            }
                            flag = false;
                        }
                        finally
                        {
                            if (flag)
                            {
                                this.OnGetChannelFailed();
                            }
                        }
                    }
                    else
                    {
                        bool channel = false;
                        Exception exception = null;
                        try
                        {
                            this.CancelTimer();
                            channel = this.GetChannel();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            this.OnGetChannelFailed();
                            exception = exception2;
                        }
                        if (channel || (exception != null))
                        {
                            base.Complete(false, exception);
                        }
                    }
                }

                private bool OnChannelOpened()
                {
                    if (this.synchronizer.OnChannelOpened(this))
                    {
                        return true;
                    }
                    if (!base.IsCompleted)
                    {
                        throw Fx.AssertAndThrow("OnChannelOpened must complete the IWaiter if it returns false.");
                    }
                    return false;
                }

                private void OnGetChannelFailed()
                {
                    if (this.channel != null)
                    {
                        this.channel.Abort();
                    }
                    this.synchronizer.OnGetChannelFailed();
                }

                private static void OnOpenComplete(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter asyncState = (ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        asyncState.isSynchronous = false;
                        try
                        {
                            flag = asyncState.CompleteOpen(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false);
                        }
                        else if (exception != null)
                        {
                            asyncState.OnGetChannelFailed();
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private void OnTimeoutElapsed()
                {
                    if (this.synchronizer.RemoveWaiter(this))
                    {
                        this.timedOut = true;
                        base.Complete(this.isSynchronous, null);
                    }
                }

                private static void OnTimeoutElapsed(object state)
                {
                    ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter waiter = (ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter) state;
                    waiter.isSynchronous = false;
                    waiter.OnTimeoutElapsed();
                }

                private static void OnTryGetChannelComplete(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter asyncState = (ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter) result.AsyncState;
                        asyncState.isSynchronous = false;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.CompleteTryGetChannel(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        if (flag || (exception != null))
                        {
                            if (exception != null)
                            {
                                asyncState.OnGetChannelFailed();
                            }
                            asyncState.Complete(asyncState.isSynchronous, exception);
                        }
                    }
                }

                private bool OpenChannel()
                {
                    if (!this.synchronizer.binder.MustOpenChannel)
                    {
                        return this.OnChannelOpened();
                    }
                    if (this.channelParameters != null)
                    {
                        this.channelParameters.PropagateChannelParameters(this.channel);
                    }
                    IAsyncResult result = this.channel.BeginOpen(this.timeoutHelper.RemainingTime(), ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.onOpenComplete, this);
                    return (result.CompletedSynchronously && this.CompleteOpen(result));
                }

                public void Set(TChannel channel)
                {
                    this.CancelTimer();
                    this.channel = channel;
                    base.Complete(false);
                }

                public void Wait()
                {
                    lock (this.ThisLock)
                    {
                        if (this.timerCancelled)
                        {
                            return;
                        }
                        if (this.timeoutHelper.RemainingTime() > TimeSpan.Zero)
                        {
                            this.timer = new IOThreadTimer(ReliableChannelBinder<TChannel>.ChannelSynchronizer.AsyncWaiter.onTimeoutElapsed, this, true);
                            this.timer.Set(this.timeoutHelper.RemainingTime());
                            return;
                        }
                    }
                    this.OnTimeoutElapsed();
                }

                public bool CanGetChannel
                {
                    get
                    {
                        return this.canGetChannel;
                    }
                }

                private object ThisLock
                {
                    get
                    {
                        return this;
                    }
                }
            }

            public interface IWaiter
            {
                void Close();
                void Fault();
                void GetChannel(bool onUserThread);
                void Set(TChannel channel);

                bool CanGetChannel { get; }
            }

            private enum State
            {
                public const ReliableChannelBinder<TChannel>.ChannelSynchronizer.State ChannelClosing = ReliableChannelBinder<TChannel>.ChannelSynchronizer.State.ChannelClosing;,
                public const ReliableChannelBinder<TChannel>.ChannelSynchronizer.State ChannelOpened = ReliableChannelBinder<TChannel>.ChannelSynchronizer.State.ChannelOpened;,
                public const ReliableChannelBinder<TChannel>.ChannelSynchronizer.State ChannelOpening = ReliableChannelBinder<TChannel>.ChannelSynchronizer.State.ChannelOpening;,
                public const ReliableChannelBinder<TChannel>.ChannelSynchronizer.State Closed = ReliableChannelBinder<TChannel>.ChannelSynchronizer.State.Closed;,
                public const ReliableChannelBinder<TChannel>.ChannelSynchronizer.State Created = ReliableChannelBinder<TChannel>.ChannelSynchronizer.State.Created;,
                public const ReliableChannelBinder<TChannel>.ChannelSynchronizer.State Faulted = ReliableChannelBinder<TChannel>.ChannelSynchronizer.State.Faulted;,
                public const ReliableChannelBinder<TChannel>.ChannelSynchronizer.State NoChannel = ReliableChannelBinder<TChannel>.ChannelSynchronizer.State.NoChannel;
            }

            private sealed class SynchronizerCompletedAsyncResult : CompletedAsyncResult
            {
                public SynchronizerCompletedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
                {
                }

                public void End()
                {
                    CompletedAsyncResult.End(this);
                }
            }

            private sealed class SyncWaiter : ReliableChannelBinder<TChannel>.ChannelSynchronizer.IWaiter
            {
                private bool canGetChannel;
                private TChannel channel;
                private ChannelParameterCollection channelParameters;
                private AutoResetEvent completeEvent;
                private Exception exception;
                private bool getChannel;
                private MaskingMode maskingMode;
                private ReliableChannelBinder<TChannel>.ChannelSynchronizer synchronizer;
                private TimeoutHelper timeoutHelper;

                public SyncWaiter(ReliableChannelBinder<TChannel>.ChannelSynchronizer synchronizer, bool canGetChannel, TChannel channel, TimeSpan timeout, MaskingMode maskingMode, ChannelParameterCollection channelParameters)
                {
                    this.completeEvent = new AutoResetEvent(false);
                    if (!canGetChannel && (channel != null))
                    {
                        throw Fx.AssertAndThrow("This waiter must wait for a channel thus argument channel must be null.");
                    }
                    this.synchronizer = synchronizer;
                    this.canGetChannel = canGetChannel;
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.maskingMode = maskingMode;
                    this.channelParameters = channelParameters;
                }

                public void Close()
                {
                    this.exception = this.synchronizer.binder.GetClosedException(this.maskingMode);
                    this.completeEvent.Set();
                }

                public void Fault()
                {
                    this.exception = this.synchronizer.binder.GetFaultedException(this.maskingMode);
                    this.completeEvent.Set();
                }

                public void GetChannel(bool onUserThread)
                {
                    if (!this.CanGetChannel)
                    {
                        throw Fx.AssertAndThrow("This waiter must wait for a channel thus the caller cannot attempt to get a channel.");
                    }
                    this.getChannel = true;
                    this.completeEvent.Set();
                }

                public void Set(TChannel channel)
                {
                    if (channel == null)
                    {
                        throw Fx.AssertAndThrow("Argument channel cannot be null. Caller must call Fault or Close instead.");
                    }
                    this.channel = channel;
                    this.completeEvent.Set();
                }

                private bool TryGetChannel()
                {
                    TChannel channel;
                    if (this.channel != null)
                    {
                        channel = this.channel;
                    }
                    else if (this.synchronizer.binder.TryGetChannel(this.timeoutHelper.RemainingTime()))
                    {
                        if (!this.synchronizer.CompleteSetChannel(this, out channel))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        this.synchronizer.OnGetChannelFailed();
                        return false;
                    }
                    if (this.synchronizer.binder.MustOpenChannel)
                    {
                        bool flag = true;
                        if (this.channelParameters != null)
                        {
                            this.channelParameters.PropagateChannelParameters(channel);
                        }
                        try
                        {
                            channel.Open(this.timeoutHelper.RemainingTime());
                            flag = false;
                        }
                        finally
                        {
                            if (flag)
                            {
                                channel.Abort();
                                this.synchronizer.OnGetChannelFailed();
                            }
                        }
                    }
                    if (this.synchronizer.OnChannelOpened(this))
                    {
                        this.Set(channel);
                    }
                    return true;
                }

                public bool TryWait(out TChannel channel)
                {
                    if (!this.Wait())
                    {
                        channel = default(TChannel);
                        return false;
                    }
                    if (this.getChannel && !this.TryGetChannel())
                    {
                        channel = default(TChannel);
                        return false;
                    }
                    this.completeEvent.Close();
                    if (this.exception != null)
                    {
                        if (this.channel != null)
                        {
                            throw Fx.AssertAndThrow("User of IWaiter called both Set and Fault or Close.");
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.exception);
                    }
                    channel = this.channel;
                    return true;
                }

                private bool Wait()
                {
                    if (!TimeoutHelper.WaitOne(this.completeEvent, this.timeoutHelper.RemainingTime()))
                    {
                        if (this.synchronizer.RemoveWaiter(this))
                        {
                            return false;
                        }
                        TimeoutHelper.WaitOne(this.completeEvent, TimeSpan.MaxValue);
                    }
                    return true;
                }

                public bool CanGetChannel
                {
                    get
                    {
                        return this.canGetChannel;
                    }
                }
            }
        }

        private sealed class CloseAsyncResult : AsyncResult
        {
            private ReliableChannelBinder<TChannel> binder;
            private TChannel channel;
            private MaskingMode maskingMode;
            private static AsyncCallback onBinderCloseComplete;
            private static AsyncCallback onChannelCloseComplete;
            private TimeoutHelper timeoutHelper;

            static CloseAsyncResult()
            {
                ReliableChannelBinder<TChannel>.CloseAsyncResult.onBinderCloseComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.CloseAsyncResult.OnBinderCloseComplete));
                ReliableChannelBinder<TChannel>.CloseAsyncResult.onChannelCloseComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.CloseAsyncResult.OnChannelCloseComplete));
            }

            public CloseAsyncResult(ReliableChannelBinder<TChannel> binder, TChannel channel, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state) : base(callback, state)
            {
                this.binder = binder;
                this.channel = channel;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.maskingMode = maskingMode;
                bool flag = false;
                try
                {
                    this.binder.OnShutdown();
                    IAsyncResult result = this.binder.OnBeginClose(timeout, ReliableChannelBinder<TChannel>.CloseAsyncResult.onBinderCloseComplete, this);
                    if (result.CompletedSynchronously)
                    {
                        flag = this.CompleteBinderClose(true, result);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.binder.Abort();
                    if (!this.binder.HandleException(exception, this.maskingMode))
                    {
                        throw;
                    }
                    flag = true;
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private bool CompleteBinderClose(bool synchronous, IAsyncResult result)
            {
                this.binder.OnEndClose(result);
                if (this.channel != null)
                {
                    result = this.binder.BeginCloseChannel(this.channel, this.timeoutHelper.RemainingTime(), ReliableChannelBinder<TChannel>.CloseAsyncResult.onChannelCloseComplete, this);
                    return (result.CompletedSynchronously && this.CompleteChannelClose(synchronous, result));
                }
                this.binder.TransitionToClosed();
                return true;
            }

            private bool CompleteChannelClose(bool synchronous, IAsyncResult result)
            {
                this.binder.EndCloseChannel(this.channel, result);
                this.binder.TransitionToClosed();
                return true;
            }

            public void End()
            {
                AsyncResult.End<ReliableChannelBinder<TChannel>.CloseAsyncResult>(this);
            }

            private Exception HandleAsyncException(Exception e)
            {
                this.binder.Abort();
                if (this.binder.HandleException(e, this.maskingMode))
                {
                    return null;
                }
                return e;
            }

            private static void OnBinderCloseComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    Exception exception;
                    ReliableChannelBinder<TChannel>.CloseAsyncResult asyncState = (ReliableChannelBinder<TChannel>.CloseAsyncResult) result.AsyncState;
                    try
                    {
                        flag = asyncState.CompleteBinderClose(false, result);
                        exception = null;
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
                    if (flag)
                    {
                        if (exception != null)
                        {
                            exception = asyncState.HandleAsyncException(exception);
                        }
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnChannelCloseComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    Exception exception;
                    ReliableChannelBinder<TChannel>.CloseAsyncResult asyncState = (ReliableChannelBinder<TChannel>.CloseAsyncResult) result.AsyncState;
                    try
                    {
                        flag = asyncState.CompleteChannelClose(false, result);
                        exception = null;
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
                    if (flag)
                    {
                        if (exception != null)
                        {
                            exception = asyncState.HandleAsyncException(exception);
                        }
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }

        protected abstract class InputAsyncResult<TBinder> : AsyncResult where TBinder: ReliableChannelBinder<TChannel>
        {
            private bool autoAborted;
            private TBinder binder;
            private bool canGetChannel;
            private TChannel channel;
            private bool isSynchronous;
            private MaskingMode maskingMode;
            private static AsyncCallback onInputComplete;
            private static AsyncCallback onTryGetChannelComplete;
            private bool success;
            private TimeoutHelper timeoutHelper;

            static InputAsyncResult()
            {
                ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>.onInputComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>.OnInputCompleteStatic));
                ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>.onTryGetChannelComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>.OnTryGetChannelCompleteStatic));
            }

            public InputAsyncResult(TBinder binder, bool canGetChannel, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state) : base(callback, state)
            {
                this.isSynchronous = true;
                this.binder = binder;
                this.canGetChannel = canGetChannel;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.maskingMode = maskingMode;
            }

            protected abstract IAsyncResult BeginInput(TBinder binder, TChannel channel, TimeSpan timeout, AsyncCallback callback, object state);
            private bool CompleteInput(IAsyncResult result)
            {
                bool flag;
                try
                {
                    this.success = this.EndInput(this.binder, this.channel, result, out flag);
                }
                finally
                {
                    this.autoAborted = this.binder.Synchronizer.Aborting;
                    this.binder.synchronizer.ReturnChannel();
                }
                return !flag;
            }

            private bool CompleteTryGetChannel(IAsyncResult result, out bool complete)
            {
                complete = false;
                this.success = this.binder.synchronizer.EndTryGetChannel(result, out this.channel);
                if (this.channel == null)
                {
                    complete = true;
                    return false;
                }
                bool flag = true;
                IAsyncResult result2 = null;
                try
                {
                    result2 = this.BeginInput(this.binder, this.channel, this.timeoutHelper.RemainingTime(), ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>.onInputComplete, this);
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        this.autoAborted = this.binder.Synchronizer.Aborting;
                        this.binder.synchronizer.ReturnChannel();
                    }
                }
                if (result2.CompletedSynchronously)
                {
                    if (this.CompleteInput(result2))
                    {
                        complete = false;
                        return true;
                    }
                    complete = true;
                    return false;
                }
                complete = false;
                return false;
            }

            public bool End()
            {
                AsyncResult.End<ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>>(this);
                return this.success;
            }

            protected abstract bool EndInput(TBinder binder, TChannel channel, IAsyncResult result, out bool complete);
            private void OnInputComplete(IAsyncResult result)
            {
                bool flag;
                this.isSynchronous = false;
                Exception exception = null;
                try
                {
                    flag = this.CompleteInput(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    if (!this.binder.HandleException(exception2, this.maskingMode, this.autoAborted))
                    {
                        exception = exception2;
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.StartOnNonUserThread();
                }
                else
                {
                    base.Complete(this.isSynchronous, exception);
                }
            }

            private static void OnInputCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>) result.AsyncState).OnInputComplete(result);
                }
            }

            private void OnTryGetChannelComplete(IAsyncResult result)
            {
                this.isSynchronous = false;
                bool flag = false;
                bool complete = false;
                Exception exception = null;
                try
                {
                    flag = this.CompleteTryGetChannel(result, out complete);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    if (!this.binder.HandleException(exception2, this.maskingMode, this.autoAborted))
                    {
                        exception = exception2;
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
                if (complete && flag)
                {
                    throw Fx.AssertAndThrow("The derived class' implementation of CompleteTryGetChannel() cannot indicate that the asynchronous operation should complete and retry.");
                }
                if (flag)
                {
                    this.StartOnNonUserThread();
                }
                else if (complete || (exception != null))
                {
                    base.Complete(this.isSynchronous, exception);
                }
            }

            private static void OnTryGetChannelCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>) result.AsyncState).OnTryGetChannelComplete(result);
                }
            }

            protected bool Start()
            {
                bool flag;
                bool flag2;
                do
                {
                    flag = false;
                    flag2 = false;
                    this.autoAborted = false;
                    try
                    {
                        IAsyncResult result = this.binder.synchronizer.BeginTryGetChannelForInput(this.canGetChannel, this.timeoutHelper.RemainingTime(), ReliableChannelBinder<TChannel>.InputAsyncResult<TBinder>.onTryGetChannelComplete, this);
                        if (result.CompletedSynchronously)
                        {
                            flag = this.CompleteTryGetChannel(result, out flag2);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!this.binder.HandleException(exception, this.maskingMode, this.autoAborted))
                        {
                            throw;
                        }
                        flag = true;
                    }
                    if (flag2 && flag)
                    {
                        throw Fx.AssertAndThrow("The derived class' implementation of CompleteTryGetChannel() cannot indicate that the asynchronous operation should complete and retry.");
                    }
                }
                while (flag);
                return flag2;
            }

            private void StartOnNonUserThread()
            {
                bool flag = false;
                Exception exception = null;
                try
                {
                    flag = this.Start();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (flag || (exception != null))
                {
                    base.Complete(false, exception);
                }
            }
        }

        private sealed class MessageRequestContext : ReliableChannelBinder<TChannel>.BinderRequestContext
        {
            public MessageRequestContext(ReliableChannelBinder<TChannel> binder, Message message) : base(binder, message)
            {
            }

            protected override void OnAbort()
            {
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReplyAsyncResult<TChannel>((ReliableChannelBinder<TChannel>.MessageRequestContext) this, message, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                ReplyAsyncResult<TChannel>.End(result);
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                if (message != null)
                {
                    base.Binder.Send(message, timeout, base.MaskingMode);
                }
            }

            private class ReplyAsyncResult : AsyncResult
            {
                private ReliableChannelBinder<TChannel>.MessageRequestContext context;
                private static AsyncCallback onSend;

                public ReplyAsyncResult(ReliableChannelBinder<TChannel>.MessageRequestContext context, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    if (message != null)
                    {
                        if (ReliableChannelBinder<TChannel>.MessageRequestContext.ReplyAsyncResult.onSend == null)
                        {
                            ReliableChannelBinder<TChannel>.MessageRequestContext.ReplyAsyncResult.onSend = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.MessageRequestContext.ReplyAsyncResult.OnSend));
                        }
                        this.context = context;
                        IAsyncResult result = context.Binder.BeginSend(message, timeout, context.MaskingMode, ReliableChannelBinder<TChannel>.MessageRequestContext.ReplyAsyncResult.onSend, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        context.Binder.EndSend(result);
                    }
                    base.Complete(true);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<ReliableChannelBinder<TChannel>.MessageRequestContext.ReplyAsyncResult>(result);
                }

                private static void OnSend(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        ReliableChannelBinder<TChannel>.MessageRequestContext.ReplyAsyncResult asyncState = (ReliableChannelBinder<TChannel>.MessageRequestContext.ReplyAsyncResult) result.AsyncState;
                        try
                        {
                            asyncState.context.Binder.EndSend(result);
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

        protected abstract class OutputAsyncResult<TBinder> : AsyncResult where TBinder: ReliableChannelBinder<TChannel>
        {
            private bool autoAborted;
            private TBinder binder;
            private TChannel channel;
            private bool hasChannel;
            private System.ServiceModel.Channels.MaskingMode maskingMode;
            private Message message;
            private static AsyncCallback onOutputComplete;
            private static AsyncCallback onTryGetChannelComplete;
            private TimeSpan timeout;
            private TimeoutHelper timeoutHelper;

            static OutputAsyncResult()
            {
                ReliableChannelBinder<TChannel>.OutputAsyncResult<TBinder>.onTryGetChannelComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.OutputAsyncResult<TBinder>.OnTryGetChannelCompleteStatic));
                ReliableChannelBinder<TChannel>.OutputAsyncResult<TBinder>.onOutputComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinder<TChannel>.OutputAsyncResult<TBinder>.OnOutputCompleteStatic));
            }

            public OutputAsyncResult(TBinder binder, AsyncCallback callback, object state) : base(callback, state)
            {
                this.binder = binder;
            }

            protected abstract IAsyncResult BeginOutput(TBinder binder, TChannel channel, Message message, TimeSpan timeout, System.ServiceModel.Channels.MaskingMode maskingMode, AsyncCallback callback, object state);
            private void Cleanup()
            {
                if (this.hasChannel)
                {
                    this.autoAborted = this.binder.Synchronizer.Aborting;
                    this.binder.synchronizer.ReturnChannel();
                }
            }

            private bool CompleteOutput(IAsyncResult result)
            {
                this.EndOutput(this.binder, this.channel, this.maskingMode, result);
                this.Cleanup();
                return true;
            }

            private bool CompleteTryGetChannel(IAsyncResult result)
            {
                bool flag = !this.binder.synchronizer.EndTryGetChannel(result, out this.channel);
                if (flag || (this.channel == null))
                {
                    this.Cleanup();
                    if (flag && !ReliableChannelBinderHelper.MaskHandled(this.maskingMode))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(this.GetTimeoutString(this.timeout)));
                    }
                    return true;
                }
                this.hasChannel = true;
                result = this.BeginOutput(this.binder, this.channel, this.message, this.timeoutHelper.RemainingTime(), this.maskingMode, ReliableChannelBinder<TChannel>.OutputAsyncResult<TBinder>.onOutputComplete, this);
                return (result.CompletedSynchronously && this.CompleteOutput(result));
            }

            protected abstract void EndOutput(TBinder binder, TChannel channel, System.ServiceModel.Channels.MaskingMode maskingMode, IAsyncResult result);
            protected abstract string GetTimeoutString(TimeSpan timeout);
            private void OnOutputComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = this.CompleteOutput(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        this.Cleanup();
                        flag = true;
                        if (!this.binder.HandleException(exception2, this.maskingMode, this.autoAborted))
                        {
                            exception = exception2;
                        }
                    }
                    if (flag)
                    {
                        base.Complete(false, exception);
                    }
                }
            }

            private static void OnOutputCompleteStatic(IAsyncResult result)
            {
                ((ReliableChannelBinder<TChannel>.OutputAsyncResult<TBinder>) result.AsyncState).OnOutputComplete(result);
            }

            private void OnTryGetChannelComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = this.CompleteTryGetChannel(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        this.Cleanup();
                        flag = true;
                        if (!this.binder.HandleException(exception2, this.maskingMode, this.autoAborted))
                        {
                            exception = exception2;
                        }
                    }
                    if (flag)
                    {
                        base.Complete(false, exception);
                    }
                }
            }

            private static void OnTryGetChannelCompleteStatic(IAsyncResult result)
            {
                ((ReliableChannelBinder<TChannel>.OutputAsyncResult<TBinder>) result.AsyncState).OnTryGetChannelComplete(result);
            }

            public void Start(Message message, TimeSpan timeout, System.ServiceModel.Channels.MaskingMode maskingMode)
            {
                if (!this.binder.ValidateOutputOperation(message, timeout, maskingMode))
                {
                    base.Complete(true);
                }
                else
                {
                    this.message = message;
                    this.timeout = timeout;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.maskingMode = maskingMode;
                    bool flag = false;
                    try
                    {
                        IAsyncResult result = this.binder.synchronizer.BeginTryGetChannelForOutput(this.timeoutHelper.RemainingTime(), this.maskingMode, ReliableChannelBinder<TChannel>.OutputAsyncResult<TBinder>.onTryGetChannelComplete, this);
                        if (result.CompletedSynchronously)
                        {
                            flag = this.CompleteTryGetChannel(result);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.Cleanup();
                        if (!this.binder.HandleException(exception, this.maskingMode, this.autoAborted))
                        {
                            throw;
                        }
                        flag = true;
                    }
                    if (flag)
                    {
                        base.Complete(true);
                    }
                }
            }

            public System.ServiceModel.Channels.MaskingMode MaskingMode
            {
                get
                {
                    return this.maskingMode;
                }
            }
        }

        private sealed class RequestRequestContext : ReliableChannelBinder<TChannel>.BinderRequestContext
        {
            private RequestContext innerContext;

            public RequestRequestContext(ReliableChannelBinder<TChannel> binder, RequestContext innerContext, Message message) : base(binder, message)
            {
                if ((binder.defaultMaskingMode != MaskingMode.All) && !binder.TolerateFaults)
                {
                    throw Fx.AssertAndThrow("This request context is designed to catch exceptions. Thus it cannot be used if the caller expects no exception handling.");
                }
                if (innerContext == null)
                {
                    throw Fx.AssertAndThrow("Argument innerContext cannot be null.");
                }
                this.innerContext = innerContext;
            }

            protected override void OnAbort()
            {
                this.innerContext.Abort();
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                try
                {
                    if (message != null)
                    {
                        base.Binder.AddOutputHeaders(message);
                    }
                    return this.innerContext.BeginReply(message, timeout, callback, state);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!base.Binder.HandleException(exception, base.MaskingMode))
                    {
                        throw;
                    }
                    this.innerContext.Abort();
                }
                return new ReliableChannelBinder<TChannel>.BinderCompletedAsyncResult(callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                try
                {
                    this.innerContext.Close(timeout);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!base.Binder.HandleException(exception, base.MaskingMode))
                    {
                        throw;
                    }
                    this.innerContext.Abort();
                }
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                ReliableChannelBinder<TChannel>.BinderCompletedAsyncResult result2 = result as ReliableChannelBinder<TChannel>.BinderCompletedAsyncResult;
                if (result2 != null)
                {
                    result2.End();
                }
                else
                {
                    try
                    {
                        this.innerContext.EndReply(result);
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!base.Binder.HandleException(exception, base.MaskingMode))
                        {
                            throw;
                        }
                        this.innerContext.Abort();
                    }
                }
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                try
                {
                    if (message != null)
                    {
                        base.Binder.AddOutputHeaders(message);
                    }
                    this.innerContext.Reply(message, timeout);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!base.Binder.HandleException(exception, base.MaskingMode))
                    {
                        throw;
                    }
                    this.innerContext.Abort();
                }
            }
        }

        private sealed class SendAsyncResult : ReliableChannelBinder<TChannel>.OutputAsyncResult<ReliableChannelBinder<TChannel>>
        {
            public SendAsyncResult(ReliableChannelBinder<TChannel> binder, AsyncCallback callback, object state) : base(binder, callback, state)
            {
            }

            protected override IAsyncResult BeginOutput(ReliableChannelBinder<TChannel> binder, TChannel channel, Message message, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
            {
                binder.AddOutputHeaders(message);
                return binder.OnBeginSend(channel, message, timeout, callback, state);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReliableChannelBinder<TChannel>.SendAsyncResult>(result);
            }

            protected override void EndOutput(ReliableChannelBinder<TChannel> binder, TChannel channel, MaskingMode maskingMode, IAsyncResult result)
            {
                binder.OnEndSend(channel, result);
            }

            protected override string GetTimeoutString(TimeSpan timeout)
            {
                return System.ServiceModel.SR.GetString("TimeoutOnSend", new object[] { timeout });
            }
        }

        private sealed class TryReceiveAsyncResult : ReliableChannelBinder<TChannel>.InputAsyncResult<ReliableChannelBinder<TChannel>>
        {
            private RequestContext requestContext;

            public TryReceiveAsyncResult(ReliableChannelBinder<TChannel> binder, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state) : base(binder, binder.CanGetChannelForReceive, timeout, maskingMode, callback, state)
            {
                if (base.Start())
                {
                    base.Complete(true);
                }
            }

            protected override IAsyncResult BeginInput(ReliableChannelBinder<TChannel> binder, TChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return binder.OnBeginTryReceive(channel, timeout, callback, state);
            }

            public bool End(out RequestContext requestContext)
            {
                requestContext = this.requestContext;
                return base.End();
            }

            protected override bool EndInput(ReliableChannelBinder<TChannel> binder, TChannel channel, IAsyncResult result, out bool complete)
            {
                bool flag = binder.OnEndTryReceive(channel, result, out this.requestContext);
                complete = !flag || (this.requestContext != null);
                if (!complete)
                {
                    binder.synchronizer.OnReadEof();
                }
                return flag;
            }
        }
    }
}

