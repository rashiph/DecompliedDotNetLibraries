namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;

    public abstract class CommunicationObject : ICommunicationObject
    {
        private bool aborted;
        private bool closeCalled;
        private object eventSender;
        private ExceptionQueue exceptionQueue;
        private object mutex;
        private bool onClosedCalled;
        private bool onClosingCalled;
        private bool onOpenedCalled;
        private bool onOpeningCalled;
        private bool raisedClosed;
        private bool raisedClosing;
        private bool raisedFaulted;
        private CommunicationState state;
        private bool traceOpenAndClose;

        public event EventHandler Closed;

        public event EventHandler Closing;

        public event EventHandler Faulted;

        public event EventHandler Opened;

        public event EventHandler Opening;

        protected CommunicationObject() : this(new object())
        {
        }

        protected CommunicationObject(object mutex)
        {
            this.mutex = mutex;
            this.eventSender = this;
            this.state = CommunicationState.Created;
        }

        internal CommunicationObject(object mutex, object eventSender)
        {
            this.mutex = mutex;
            this.eventSender = eventSender;
            this.state = CommunicationState.Created;
        }

        public void Abort()
        {
            lock (this.ThisLock)
            {
                if (this.aborted || (this.state == CommunicationState.Closed))
                {
                    return;
                }
                this.aborted = true;
                this.state = CommunicationState.Closing;
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80002, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectAborted", new object[] { TraceUtility.CreateSourceString(this) }), this);
            }
            bool flag = true;
            try
            {
                this.OnClosing();
                if (!this.onClosingCalled)
                {
                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosing"), Guid.Empty, this);
                }
                this.OnAbort();
                this.OnClosed();
                if (!this.onClosedCalled)
                {
                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosed"), Guid.Empty, this);
                }
                flag = false;
            }
            finally
            {
                if (flag && DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x80003, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectAbortFailed", new object[] { this.GetCommunicationObjectType().ToString() }), this);
                }
            }
        }

        internal void AddPendingException(Exception exception)
        {
            lock (this.ThisLock)
            {
                if (this.exceptionQueue == null)
                {
                    this.exceptionQueue = new ExceptionQueue(this.ThisLock);
                }
            }
            this.exceptionQueue.AddException(exception);
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.BeginClose(this.DefaultCloseTimeout, callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            using ((DiagnosticUtility.ShouldUseActivity && this.TraceOpenAndClose) ? this.CreateCloseActivity() : null)
            {
                CommunicationState state2;
                lock (this.ThisLock)
                {
                    state2 = this.state;
                    if (state2 != CommunicationState.Closed)
                    {
                        this.state = CommunicationState.Closing;
                    }
                    this.closeCalled = true;
                }
                switch (state2)
                {
                    case CommunicationState.Created:
                    case CommunicationState.Opening:
                    case CommunicationState.Faulted:
                        this.Abort();
                        if (state2 == CommunicationState.Faulted)
                        {
                            throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
                        }
                        return new AlreadyClosedAsyncResult(callback, state);

                    case CommunicationState.Opened:
                    {
                        bool flag = true;
                        try
                        {
                            this.OnClosing();
                            if (!this.onClosingCalled)
                            {
                                throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosing"), Guid.Empty, this);
                            }
                            IAsyncResult result = new CloseAsyncResult(this, timeout, callback, state);
                            flag = false;
                            return result;
                        }
                        finally
                        {
                            if (flag)
                            {
                                if (DiagnosticUtility.ShouldTraceWarning)
                                {
                                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x80004, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectCloseFailed", new object[] { this.GetCommunicationObjectType().ToString() }), this);
                                }
                                this.Abort();
                            }
                        }
                        break;
                    }
                    case CommunicationState.Closing:
                    case CommunicationState.Closed:
                        break;

                    default:
                        throw Fx.AssertAndThrow("CommunicationObject.BeginClose: Unknown CommunicationState");
                }
                return new AlreadyClosedAsyncResult(callback, state);
            }
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.BeginOpen(this.DefaultOpenTimeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            lock (this.ThisLock)
            {
                this.ThrowIfDisposedOrImmutable();
                this.state = CommunicationState.Opening;
            }
            bool flag = true;
            try
            {
                this.OnOpening();
                if (!this.onOpeningCalled)
                {
                    throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnOpening"), Guid.Empty, this);
                }
                IAsyncResult result = new OpenAsyncResult(this, timeout, callback, state);
                flag = false;
                result2 = result;
            }
            finally
            {
                if (flag)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x80005, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectOpenFailed", new object[] { this.GetCommunicationObjectType().ToString() }), this);
                    }
                    this.Fault();
                }
            }
            return result2;
        }

        public void Close()
        {
            this.Close(this.DefaultCloseTimeout);
        }

        public void Close(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            using ((DiagnosticUtility.ShouldUseActivity && this.TraceOpenAndClose) ? this.CreateCloseActivity() : null)
            {
                CommunicationState state;
                lock (this.ThisLock)
                {
                    state = this.state;
                    if (state != CommunicationState.Closed)
                    {
                        this.state = CommunicationState.Closing;
                    }
                    this.closeCalled = true;
                }
                switch (state)
                {
                    case CommunicationState.Created:
                    case CommunicationState.Opening:
                    case CommunicationState.Faulted:
                        this.Abort();
                        if (state == CommunicationState.Faulted)
                        {
                            throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
                        }
                        return;

                    case CommunicationState.Opened:
                    {
                        bool flag = true;
                        try
                        {
                            TimeoutHelper helper = new TimeoutHelper(timeout);
                            this.OnClosing();
                            if (!this.onClosingCalled)
                            {
                                throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosing"), Guid.Empty, this);
                            }
                            this.OnClose(helper.RemainingTime());
                            this.OnClosed();
                            if (!this.onClosedCalled)
                            {
                                throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnClosed"), Guid.Empty, this);
                            }
                            flag = false;
                            return;
                        }
                        finally
                        {
                            if (flag)
                            {
                                if (DiagnosticUtility.ShouldTraceWarning)
                                {
                                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x80004, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectCloseFailed", new object[] { this.GetCommunicationObjectType().ToString() }), this);
                                }
                                this.Abort();
                            }
                        }
                        break;
                    }
                    case CommunicationState.Closing:
                    case CommunicationState.Closed:
                        return;
                }
                throw Fx.AssertAndThrow("CommunicationObject.BeginClose: Unknown CommunicationState");
            }
        }

        internal Exception CreateAbortedException()
        {
            return new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("CommunicationObjectAborted1", new object[] { this.GetCommunicationObjectType().ToString() }));
        }

        private Exception CreateBaseClassMethodNotCalledException(string method)
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("CommunicationObjectBaseClassMethodNotCalled", new object[] { this.GetCommunicationObjectType().ToString(), method }));
        }

        private ServiceModelActivity CreateCloseActivity()
        {
            ServiceModelActivity activity = null;
            activity = ServiceModelActivity.CreateBoundedActivity();
            if (DiagnosticUtility.ShouldUseActivity)
            {
                ServiceModelActivity.Start(activity, this.CloseActivityName, ActivityType.Close);
            }
            return activity;
        }

        internal Exception CreateClosedException()
        {
            if (!this.closeCalled)
            {
                return this.CreateAbortedException();
            }
            return new ObjectDisposedException(this.GetCommunicationObjectType().ToString());
        }

        internal Exception CreateFaultedException()
        {
            return new CommunicationObjectFaultedException(System.ServiceModel.SR.GetString("CommunicationObjectFaulted1", new object[] { this.GetCommunicationObjectType().ToString() }));
        }

        private Exception CreateImmutableException()
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("CommunicationObjectCannotBeModifiedInState", new object[] { this.GetCommunicationObjectType().ToString(), this.state.ToString() }));
        }

        private Exception CreateNotOpenException()
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("CommunicationObjectCannotBeUsed", new object[] { this.GetCommunicationObjectType().ToString(), this.state.ToString() }));
        }

        internal bool DoneReceivingInCurrentState()
        {
            this.ThrowPending();
            switch (this.state)
            {
                case CommunicationState.Created:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opening:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opened:
                    return false;

                case CommunicationState.Closing:
                    return true;

                case CommunicationState.Closed:
                    return true;

                case CommunicationState.Faulted:
                    return true;
            }
            throw Fx.AssertAndThrow("DoneReceivingInCurrentState: Unknown CommunicationObject.state");
        }

        public void EndClose(IAsyncResult result)
        {
            if (result is AlreadyClosedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                CloseAsyncResult.End(result);
            }
        }

        public void EndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected void Fault()
        {
            lock (this.ThisLock)
            {
                if (((this.state == CommunicationState.Closed) || (this.state == CommunicationState.Closing)) || (this.state == CommunicationState.Faulted))
                {
                    return;
                }
                this.state = CommunicationState.Faulted;
            }
            this.OnFaulted();
        }

        internal void Fault(Exception exception)
        {
            lock (this.ThisLock)
            {
                if (this.exceptionQueue == null)
                {
                    this.exceptionQueue = new ExceptionQueue(this.ThisLock);
                }
            }
            if ((exception != null) && DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x8000a, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectFaultReason"), exception, null);
            }
            this.exceptionQueue.AddException(exception);
            this.Fault();
        }

        protected virtual System.Type GetCommunicationObjectType()
        {
            return base.GetType();
        }

        internal Exception GetPendingException()
        {
            ExceptionQueue exceptionQueue = this.exceptionQueue;
            if (exceptionQueue != null)
            {
                return exceptionQueue.GetException();
            }
            return null;
        }

        internal Exception GetTerminalException()
        {
            Exception pendingException = this.GetPendingException();
            if (pendingException != null)
            {
                return pendingException;
            }
            switch (this.state)
            {
                case CommunicationState.Closing:
                case CommunicationState.Closed:
                    return new CommunicationException(System.ServiceModel.SR.GetString("CommunicationObjectCloseInterrupted1", new object[] { this.GetCommunicationObjectType().ToString() }));

                case CommunicationState.Faulted:
                    return this.CreateFaultedException();
            }
            throw Fx.AssertAndThrow("GetTerminalException: Invalid CommunicationObject.state");
        }

        protected abstract void OnAbort();
        protected abstract IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void OnClose(TimeSpan timeout);
        protected virtual void OnClosed()
        {
            this.onClosedCalled = true;
            lock (this.ThisLock)
            {
                if (this.raisedClosed)
                {
                    return;
                }
                this.raisedClosed = true;
                this.state = CommunicationState.Closed;
            }
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80007, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectClosed", new object[] { TraceUtility.CreateSourceString(this) }), this);
            }
            EventHandler closed = this.Closed;
            if (closed != null)
            {
                try
                {
                    closed(this.eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected virtual void OnClosing()
        {
            this.onClosingCalled = true;
            lock (this.ThisLock)
            {
                if (this.raisedClosing)
                {
                    return;
                }
                this.raisedClosing = true;
            }
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80006, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectClosing", new object[] { TraceUtility.CreateSourceString(this) }), this);
            }
            EventHandler closing = this.Closing;
            if (closing != null)
            {
                try
                {
                    closing(this.eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected abstract void OnEndClose(IAsyncResult result);
        protected abstract void OnEndOpen(IAsyncResult result);
        protected virtual void OnFaulted()
        {
            lock (this.ThisLock)
            {
                if (this.raisedFaulted)
                {
                    return;
                }
                this.raisedFaulted = true;
            }
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x8000b, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectFaulted", new object[] { this.GetCommunicationObjectType().ToString() }), this);
            }
            EventHandler faulted = this.Faulted;
            if (faulted != null)
            {
                try
                {
                    faulted(this.eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected abstract void OnOpen(TimeSpan timeout);
        protected virtual void OnOpened()
        {
            this.onOpenedCalled = true;
            lock (this.ThisLock)
            {
                if (this.aborted || (this.state != CommunicationState.Opening))
                {
                    return;
                }
                this.state = CommunicationState.Opened;
            }
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x8000d, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectOpened", new object[] { TraceUtility.CreateSourceString(this) }), this);
            }
            EventHandler opened = this.Opened;
            if (opened != null)
            {
                try
                {
                    opened(this.eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected virtual void OnOpening()
        {
            this.onOpeningCalled = true;
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x8000c, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectOpening", new object[] { TraceUtility.CreateSourceString(this) }), this);
            }
            EventHandler opening = this.Opening;
            if (opening != null)
            {
                try
                {
                    opening(this.eventSender, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        public void Open()
        {
            this.Open(this.DefaultOpenTimeout);
        }

        public void Open(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            if (TD.CommunicationObjectOpenStartIsEnabled())
            {
                TD.CommunicationObjectOpenStart();
            }
            using (ServiceModelActivity activity = (DiagnosticUtility.ShouldUseActivity && this.TraceOpenAndClose) ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, this.OpenActivityName, this.OpenActivityType);
                }
                lock (this.ThisLock)
                {
                    this.ThrowIfDisposedOrImmutable();
                    this.state = CommunicationState.Opening;
                }
                bool flag = true;
                try
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    this.OnOpening();
                    if (!this.onOpeningCalled)
                    {
                        throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnOpening"), Guid.Empty, this);
                    }
                    this.OnOpen(helper.RemainingTime());
                    this.OnOpened();
                    if (!this.onOpenedCalled)
                    {
                        throw TraceUtility.ThrowHelperError(this.CreateBaseClassMethodNotCalledException("OnOpened"), Guid.Empty, this);
                    }
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, 0x80005, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectOpenFailed", new object[] { this.GetCommunicationObjectType().ToString() }), this);
                        }
                        this.Fault();
                    }
                }
            }
            if (TD.CommunicationObjectOpenStopIsEnabled())
            {
                TD.CommunicationObjectOpenStop();
            }
        }

        internal void ThrowIfAborted()
        {
            if (this.aborted && !this.closeCalled)
            {
                throw TraceUtility.ThrowHelperError(this.CreateAbortedException(), Guid.Empty, this);
            }
        }

        internal void ThrowIfClosed()
        {
            this.ThrowPending();
            switch (this.state)
            {
                case CommunicationState.Created:
                case CommunicationState.Opening:
                case CommunicationState.Opened:
                case CommunicationState.Closing:
                    return;

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
            }
            throw Fx.AssertAndThrow("ThrowIfClosed: Unknown CommunicationObject.state");
        }

        internal void ThrowIfClosedOrNotOpen()
        {
            this.ThrowPending();
            switch (this.state)
            {
                case CommunicationState.Created:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opening:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opened:
                case CommunicationState.Closing:
                    return;

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
            }
            throw Fx.AssertAndThrow("ThrowIfClosedOrNotOpen: Unknown CommunicationObject.state");
        }

        internal void ThrowIfClosedOrOpened()
        {
            this.ThrowPending();
            switch (this.state)
            {
                case CommunicationState.Created:
                case CommunicationState.Opening:
                    return;

                case CommunicationState.Opened:
                    throw TraceUtility.ThrowHelperError(this.CreateImmutableException(), Guid.Empty, this);

                case CommunicationState.Closing:
                    throw TraceUtility.ThrowHelperError(this.CreateImmutableException(), Guid.Empty, this);

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
            }
            throw Fx.AssertAndThrow("ThrowIfClosedOrOpened: Unknown CommunicationObject.state");
        }

        protected internal void ThrowIfDisposed()
        {
            this.ThrowPending();
            switch (this.state)
            {
                case CommunicationState.Created:
                case CommunicationState.Opening:
                case CommunicationState.Opened:
                    return;

                case CommunicationState.Closing:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
            }
            throw Fx.AssertAndThrow("ThrowIfDisposed: Unknown CommunicationObject.state");
        }

        protected internal void ThrowIfDisposedOrImmutable()
        {
            this.ThrowPending();
            switch (this.state)
            {
                case CommunicationState.Created:
                    return;

                case CommunicationState.Opening:
                    throw TraceUtility.ThrowHelperError(this.CreateImmutableException(), Guid.Empty, this);

                case CommunicationState.Opened:
                    throw TraceUtility.ThrowHelperError(this.CreateImmutableException(), Guid.Empty, this);

                case CommunicationState.Closing:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
            }
            throw Fx.AssertAndThrow("ThrowIfDisposedOrImmutable: Unknown CommunicationObject.state");
        }

        protected internal void ThrowIfDisposedOrNotOpen()
        {
            this.ThrowPending();
            switch (this.state)
            {
                case CommunicationState.Created:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opening:
                    throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);

                case CommunicationState.Opened:
                    return;

                case CommunicationState.Closing:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateClosedException(), Guid.Empty, this);

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
            }
            throw Fx.AssertAndThrow("ThrowIfDisposedOrNotOpen: Unknown CommunicationObject.state");
        }

        internal void ThrowIfFaulted()
        {
            this.ThrowPending();
            switch (this.state)
            {
                case CommunicationState.Created:
                case CommunicationState.Opening:
                case CommunicationState.Opened:
                case CommunicationState.Closing:
                case CommunicationState.Closed:
                    return;

                case CommunicationState.Faulted:
                    throw TraceUtility.ThrowHelperError(this.CreateFaultedException(), Guid.Empty, this);
            }
            throw Fx.AssertAndThrow("ThrowIfFaulted: Unknown CommunicationObject.state");
        }

        internal void ThrowIfNotOpened()
        {
            if ((this.state == CommunicationState.Created) || (this.state == CommunicationState.Opening))
            {
                throw TraceUtility.ThrowHelperError(this.CreateNotOpenException(), Guid.Empty, this);
            }
        }

        internal void ThrowPending()
        {
            ExceptionQueue exceptionQueue = this.exceptionQueue;
            if (exceptionQueue != null)
            {
                Exception exception = exceptionQueue.GetException();
                if (exception != null)
                {
                    throw TraceUtility.ThrowHelperError(exception, Guid.Empty, this);
                }
            }
        }

        internal bool Aborted
        {
            get
            {
                return this.aborted;
            }
        }

        internal virtual string CloseActivityName
        {
            get
            {
                return System.ServiceModel.SR.GetString("ActivityClose", new object[] { base.GetType().FullName });
            }
        }

        protected abstract TimeSpan DefaultCloseTimeout { get; }

        protected abstract TimeSpan DefaultOpenTimeout { get; }

        internal object EventSender
        {
            get
            {
                return this.eventSender;
            }
            set
            {
                this.eventSender = value;
            }
        }

        internal TimeSpan InternalCloseTimeout
        {
            get
            {
                return this.DefaultCloseTimeout;
            }
        }

        internal TimeSpan InternalOpenTimeout
        {
            get
            {
                return this.DefaultOpenTimeout;
            }
        }

        protected bool IsDisposed
        {
            get
            {
                return (this.state == CommunicationState.Closed);
            }
        }

        internal virtual string OpenActivityName
        {
            get
            {
                return System.ServiceModel.SR.GetString("ActivityOpen", new object[] { base.GetType().FullName });
            }
        }

        internal virtual ActivityType OpenActivityType
        {
            get
            {
                return ActivityType.Open;
            }
        }

        public CommunicationState State
        {
            get
            {
                return this.state;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.mutex;
            }
        }

        internal bool TraceOpenAndClose
        {
            get
            {
                return this.traceOpenAndClose;
            }
            set
            {
                this.traceOpenAndClose = value && DiagnosticUtility.ShouldUseActivity;
            }
        }

        private class AlreadyClosedAsyncResult : CompletedAsyncResult
        {
            public AlreadyClosedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }
        }

        private class CloseAsyncResult : TraceAsyncResult
        {
            private CommunicationObject communicationObject;
            private static AsyncResult.AsyncCompletion onCloseCompletion = new AsyncResult.AsyncCompletion(CommunicationObject.CloseAsyncResult.OnCloseCompletion);
            private TimeoutHelper timeout;

            public CloseAsyncResult(CommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = communicationObject;
                this.timeout = new TimeoutHelper(timeout);
                base.OnCompleting = new Action<AsyncResult, Exception>(this.OnCloseCompleted);
                if (this.InvokeClose())
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CommunicationObject.CloseAsyncResult>(result);
            }

            private bool InvokeClose()
            {
                IAsyncResult result = this.communicationObject.OnBeginClose(this.timeout.RemainingTime(), base.PrepareAsyncCompletion(onCloseCompletion), this);
                return (result.CompletedSynchronously && OnCloseCompletion(result));
            }

            private void NotifyClosed()
            {
                this.communicationObject.OnClosed();
                if (!this.communicationObject.onClosedCalled)
                {
                    throw TraceUtility.ThrowHelperError(this.communicationObject.CreateBaseClassMethodNotCalledException("OnClosed"), Guid.Empty, this.communicationObject);
                }
            }

            private void OnCloseCompleted(AsyncResult result, Exception exception)
            {
                if (exception != null)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x80004, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectCloseFailed", new object[] { this.communicationObject.GetCommunicationObjectType().ToString() }), this, exception);
                    }
                    this.communicationObject.Abort();
                }
            }

            private static bool OnCloseCompletion(IAsyncResult result)
            {
                CommunicationObject.CloseAsyncResult asyncState = (CommunicationObject.CloseAsyncResult) result.AsyncState;
                asyncState.communicationObject.OnEndClose(result);
                asyncState.NotifyClosed();
                return true;
            }
        }

        private class ExceptionQueue
        {
            private Queue<Exception> exceptions = new Queue<Exception>();
            private object thisLock;

            internal ExceptionQueue(object thisLock)
            {
                this.thisLock = thisLock;
            }

            public void AddException(Exception exception)
            {
                if (exception != null)
                {
                    lock (this.ThisLock)
                    {
                        this.exceptions.Enqueue(exception);
                    }
                }
            }

            public Exception GetException()
            {
                lock (this.ThisLock)
                {
                    if (this.exceptions.Count > 0)
                    {
                        return this.exceptions.Dequeue();
                    }
                }
                return null;
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private CommunicationObject communicationObject;
            private static AsyncResult.AsyncCompletion onOpenCompletion = new AsyncResult.AsyncCompletion(CommunicationObject.OpenAsyncResult.OnOpenCompletion);
            private TimeoutHelper timeout;

            public OpenAsyncResult(CommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = communicationObject;
                this.timeout = new TimeoutHelper(timeout);
                base.OnCompleting = new Action<AsyncResult, Exception>(this.OnOpenCompleted);
                if (this.InvokeOpen())
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CommunicationObject.OpenAsyncResult>(result);
            }

            private bool InvokeOpen()
            {
                IAsyncResult result = this.communicationObject.OnBeginOpen(this.timeout.RemainingTime(), base.PrepareAsyncCompletion(onOpenCompletion), this);
                return (result.CompletedSynchronously && OnOpenCompletion(result));
            }

            private void NotifyOpened()
            {
                this.communicationObject.OnOpened();
                if (!this.communicationObject.onOpenedCalled)
                {
                    throw TraceUtility.ThrowHelperError(this.communicationObject.CreateBaseClassMethodNotCalledException("OnOpened"), Guid.Empty, this.communicationObject);
                }
            }

            private void OnOpenCompleted(AsyncResult result, Exception exception)
            {
                if (exception != null)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x80005, System.ServiceModel.SR.GetString("TraceCodeCommunicationObjectOpenFailed", new object[] { this.communicationObject.GetCommunicationObjectType().ToString() }), this, exception);
                    }
                    this.communicationObject.Fault();
                }
            }

            private static bool OnOpenCompletion(IAsyncResult result)
            {
                CommunicationObject.OpenAsyncResult asyncState = (CommunicationObject.OpenAsyncResult) result.AsyncState;
                asyncState.communicationObject.OnEndOpen(result);
                asyncState.NotifyOpened();
                return true;
            }
        }
    }
}

