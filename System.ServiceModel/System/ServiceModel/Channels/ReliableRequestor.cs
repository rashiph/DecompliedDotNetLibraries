namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal abstract class ReliableRequestor
    {
        private InterruptibleWaitObject abortHandle = new InterruptibleWaitObject(false, false);
        private IReliableChannelBinder binder;
        private bool isCreateSequence;
        private ActionHeader messageAction;
        private BodyWriter messageBody;
        private WsrmMessageHeader messageHeader;
        private UniqueId messageId;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private TimeSpan originalTimeout;
        private string timeoutString1Index;

        protected ReliableRequestor()
        {
        }

        public void Abort(CommunicationObject communicationObject)
        {
            this.abortHandle.Abort(communicationObject);
        }

        public IAsyncResult BeginRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new RequestAsyncResult(this, timeout, callback, state);
        }

        private Message CreateRequestMessage()
        {
            Message request = Message.CreateMessage(this.messageVersion, this.messageAction, this.messageBody);
            request.Properties.AllowOutputBatching = false;
            if (this.messageHeader != null)
            {
                request.Headers.Insert(0, this.messageHeader);
            }
            if (this.messageId != null)
            {
                request.Headers.MessageId = this.messageId;
                RequestReplyCorrelator.PrepareRequest(request);
                EndpointAddress localAddress = this.binder.LocalAddress;
                if (localAddress == null)
                {
                    request.Headers.ReplyTo = null;
                    return request;
                }
                if (this.messageVersion.Addressing == AddressingVersion.WSAddressingAugust2004)
                {
                    request.Headers.ReplyTo = localAddress;
                    return request;
                }
                if (this.messageVersion.Addressing != AddressingVersion.WSAddressing10)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { this.messageVersion.Addressing })));
                }
                request.Headers.ReplyTo = localAddress.IsAnonymous ? null : localAddress;
            }
            return request;
        }

        public Message EndRequest(IAsyncResult result)
        {
            return RequestAsyncResult.End(result);
        }

        private bool EnsureChannel()
        {
            if (this.IsCreateSequence)
            {
                IClientReliableChannelBinder binder = (IClientReliableChannelBinder) this.binder;
                return binder.EnsureChannelForRequest();
            }
            return true;
        }

        public virtual void Fault(CommunicationObject communicationObject)
        {
            this.abortHandle.Fault(communicationObject);
        }

        public abstract WsrmMessageInfo GetInfo();
        private TimeSpan GetNextRequestTimeout(TimeSpan remainingTimeout, out TimeoutHelper iterationTimeout, out bool lastIteration)
        {
            iterationTimeout = new TimeoutHelper(ReliableMessagingConstants.RequestorIterationTime);
            lastIteration = remainingTimeout <= iterationTimeout.RemainingTime();
            return remainingTimeout;
        }

        private bool HandleException(Exception exception, bool lastIteration)
        {
            if (!this.IsCreateSequence)
            {
                return this.binder.IsHandleable(exception);
            }
            if (exception is QuotaExceededException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(exception.Message, exception));
            }
            return (((this.binder.IsHandleable(exception) && !(exception is MessageSecurityException)) && (!(exception is SecurityNegotiationException) && !(exception is SecurityAccessDeniedException))) && ((this.binder.State == CommunicationState.Opened) && !lastIteration));
        }

        protected abstract IAsyncResult OnBeginRequest(Message request, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract Message OnEndRequest(bool last, IAsyncResult result);
        protected abstract Message OnRequest(Message request, TimeSpan timeout, bool last);
        public Message Request(TimeSpan timeout)
        {
            TimeoutHelper helper2;
            bool flag;
            Message message;
            this.originalTimeout = timeout;
            TimeoutHelper helper = new TimeoutHelper(this.originalTimeout);
        Label_0014:
            message = null;
            Message response = null;
            bool flag2 = false;
            TimeSpan span = this.GetNextRequestTimeout(helper.RemainingTime(), out helper2, out flag);
            try
            {
                if (this.EnsureChannel())
                {
                    message = this.CreateRequestMessage();
                    response = this.OnRequest(message, span, flag);
                    flag2 = true;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception) || !this.HandleException(exception, flag))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            finally
            {
                if (message != null)
                {
                    message.Close();
                }
            }
            if (flag2 && this.ValidateReply(response))
            {
                return response;
            }
            if (!flag)
            {
                this.abortHandle.Wait(helper2.RemainingTime());
                goto Label_0014;
            }
            this.ThrowTimeoutException();
            return null;
        }

        public abstract void SetInfo(WsrmMessageInfo info);
        public void SetRequestResponsePattern()
        {
            if (this.messageId != null)
            {
                throw Fx.AssertAndThrow("Initialize messageId only once.");
            }
            this.messageId = new UniqueId();
        }

        private void ThrowTimeoutException()
        {
            if (this.timeoutString1Index != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString(this.timeoutString1Index, new object[] { this.originalTimeout })));
            }
        }

        private bool ValidateReply(Message response)
        {
            if (this.messageId != null)
            {
                return (response != null);
            }
            return true;
        }

        public IReliableChannelBinder Binder
        {
            protected get
            {
                return this.binder;
            }
            set
            {
                this.binder = value;
            }
        }

        public bool IsCreateSequence
        {
            protected get
            {
                return this.isCreateSequence;
            }
            set
            {
                this.isCreateSequence = value;
            }
        }

        public ActionHeader MessageAction
        {
            set
            {
                this.messageAction = value;
            }
        }

        public BodyWriter MessageBody
        {
            set
            {
                this.messageBody = value;
            }
        }

        public WsrmMessageHeader MessageHeader
        {
            get
            {
                return this.messageHeader;
            }
            set
            {
                this.messageHeader = value;
            }
        }

        public UniqueId MessageId
        {
            get
            {
                return this.messageId;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            set
            {
                this.messageVersion = value;
            }
        }

        public string TimeoutString1Index
        {
            set
            {
                this.timeoutString1Index = value;
            }
        }

        private class RequestAsyncResult : AsyncResult
        {
            private TimeoutHelper iterationTimeoutHelper;
            private bool lastIteration;
            private Message request;
            private static AsyncCallback requestCallback = Fx.ThunkCallback(new AsyncCallback(ReliableRequestor.RequestAsyncResult.RequestCallback));
            private ReliableRequestor requestor;
            private Message response;
            private TimeoutHelper timeoutHelper;
            private static AsyncCallback waitCallback = Fx.ThunkCallback(new AsyncCallback(ReliableRequestor.RequestAsyncResult.WaitCallback));

            public RequestAsyncResult(ReliableRequestor requestor, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.requestor = requestor;
                this.requestor.originalTimeout = timeout;
                this.timeoutHelper = new TimeoutHelper(this.requestor.originalTimeout);
                if (this.Request(null))
                {
                    base.Complete(true);
                }
            }

            public static Message End(IAsyncResult result)
            {
                return AsyncResult.End<ReliableRequestor.RequestAsyncResult>(result).response;
            }

            private bool EndWait(IAsyncResult result)
            {
                this.requestor.abortHandle.EndWait(result);
                return this.Request(null);
            }

            private bool Request(IAsyncResult requestResult)
            {
                bool flag;
            Label_0000:
                flag = false;
                bool flag2 = true;
                TimeSpan timeout = (requestResult == null) ? this.requestor.GetNextRequestTimeout(this.timeoutHelper.RemainingTime(), out this.iterationTimeoutHelper, out this.lastIteration) : TimeSpan.Zero;
                try
                {
                    if ((requestResult == null) && this.requestor.EnsureChannel())
                    {
                        this.request = this.requestor.CreateRequestMessage();
                        requestResult = this.requestor.OnBeginRequest(this.request, timeout, requestCallback, this);
                        if (!requestResult.CompletedSynchronously)
                        {
                            flag2 = false;
                            return false;
                        }
                    }
                    if (requestResult != null)
                    {
                        this.response = this.requestor.OnEndRequest(this.lastIteration, requestResult);
                        flag = true;
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception) || !this.requestor.HandleException(exception, this.lastIteration))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                finally
                {
                    if (flag2 && (this.request != null))
                    {
                        this.request.Close();
                        this.request = null;
                    }
                    requestResult = null;
                }
                if (!flag || !this.requestor.ValidateReply(this.response))
                {
                    if (!this.lastIteration)
                    {
                        IAsyncResult result = this.requestor.abortHandle.BeginWait(this.iterationTimeoutHelper.RemainingTime(), waitCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.requestor.abortHandle.EndWait(result);
                        goto Label_0000;
                    }
                    this.requestor.ThrowTimeoutException();
                }
                return true;
            }

            private static void RequestCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    Exception exception;
                    ReliableRequestor.RequestAsyncResult asyncState = (ReliableRequestor.RequestAsyncResult) result.AsyncState;
                    try
                    {
                        flag = asyncState.Request(result);
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
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void WaitCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    Exception exception;
                    ReliableRequestor.RequestAsyncResult asyncState = (ReliableRequestor.RequestAsyncResult) result.AsyncState;
                    try
                    {
                        flag = asyncState.EndWait(result);
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
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }
    }
}

