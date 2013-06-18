namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal abstract class RequestContextBase : RequestContext
    {
        private bool aborted;
        private TimeSpan defaultCloseTimeout;
        private TimeSpan defaultSendTimeout;
        private bool replyInitiated;
        private bool replySent;
        private Message requestMessage;
        private Exception requestMessageException;
        private CommunicationState state = CommunicationState.Opened;
        private object thisLock = new object();

        protected RequestContextBase(Message requestMessage, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
        {
            this.defaultSendTimeout = defaultSendTimeout;
            this.defaultCloseTimeout = defaultCloseTimeout;
            this.requestMessage = requestMessage;
        }

        public override void Abort()
        {
            lock (this.ThisLock)
            {
                if (this.state == CommunicationState.Closed)
                {
                    return;
                }
                this.state = CommunicationState.Closing;
                this.aborted = true;
            }
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x4001e, System.ServiceModel.SR.GetString("TraceCodeRequestContextAbort"), this);
            }
            try
            {
                this.OnAbort();
            }
            finally
            {
                this.state = CommunicationState.Closed;
            }
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return this.BeginReply(message, this.defaultSendTimeout, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            lock (this.thisLock)
            {
                this.ThrowIfInvalidReply();
                this.replyInitiated = true;
            }
            return this.OnBeginReply(message, timeout, callback, state);
        }

        public override void Close()
        {
            this.Close(this.defaultCloseTimeout);
        }

        public override void Close(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.state != CommunicationState.Opened)
                {
                    return;
                }
                this.state = CommunicationState.Closing;
                if (!this.replyInitiated)
                {
                    this.replyInitiated = true;
                    flag = true;
                }
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            bool flag2 = true;
            try
            {
                if (flag)
                {
                    this.OnReply(null, helper.RemainingTime());
                }
                this.OnClose(helper.RemainingTime());
                this.state = CommunicationState.Closed;
                flag2 = false;
            }
            finally
            {
                if (flag2)
                {
                    this.Abort();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (this.replySent)
                {
                    this.Close();
                }
                else
                {
                    this.Abort();
                }
            }
        }

        public override void EndReply(IAsyncResult result)
        {
            this.OnEndReply(result);
            this.replySent = true;
        }

        protected abstract void OnAbort();
        protected abstract IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void OnClose(TimeSpan timeout);
        protected abstract void OnEndReply(IAsyncResult result);
        protected abstract void OnReply(Message message, TimeSpan timeout);
        public void ReInitialize(Message requestMessage)
        {
            this.state = CommunicationState.Opened;
            this.requestMessageException = null;
            this.replySent = false;
            this.replyInitiated = false;
            this.aborted = false;
            this.requestMessage = requestMessage;
        }

        public override void Reply(Message message)
        {
            this.Reply(message, this.defaultSendTimeout);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            lock (this.thisLock)
            {
                this.ThrowIfInvalidReply();
                this.replyInitiated = true;
            }
            this.OnReply(message, timeout);
            this.replySent = true;
        }

        protected void SetRequestMessage(Exception requestMessageException)
        {
            this.requestMessageException = requestMessageException;
        }

        protected void SetRequestMessage(Message requestMessage)
        {
            this.requestMessage = requestMessage;
        }

        protected void ThrowIfInvalidReply()
        {
            if ((this.state == CommunicationState.Closed) || (this.state == CommunicationState.Closing))
            {
                if (this.aborted)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("RequestContextAborted")));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
            if (this.replyInitiated)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ReplyAlreadySent")));
            }
        }

        public bool Aborted
        {
            get
            {
                return this.aborted;
            }
        }

        public TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.defaultCloseTimeout;
            }
        }

        public TimeSpan DefaultSendTimeout
        {
            get
            {
                return this.defaultSendTimeout;
            }
        }

        protected bool ReplyInitiated
        {
            get
            {
                return this.replyInitiated;
            }
        }

        public override Message RequestMessage
        {
            get
            {
                if (this.requestMessageException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.requestMessageException);
                }
                return this.requestMessage;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

