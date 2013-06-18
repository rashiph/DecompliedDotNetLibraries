namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class ApplySecurityAndSendAsyncResult<MessageSenderType> : AsyncResult where MessageSenderType: class
    {
        private readonly System.ServiceModel.Security.SecurityProtocol binding;
        private readonly MessageSenderType channel;
        private SecurityProtocolCorrelationState newCorrelationState;
        private volatile bool secureOutgoingMessageDone;
        private static AsyncCallback sharedCallback;
        private TimeoutHelper timeoutHelper;

        static ApplySecurityAndSendAsyncResult()
        {
            ApplySecurityAndSendAsyncResult<MessageSenderType>.sharedCallback = Fx.ThunkCallback(new AsyncCallback(ApplySecurityAndSendAsyncResult<MessageSenderType>.SharedCallback));
        }

        public ApplySecurityAndSendAsyncResult(System.ServiceModel.Security.SecurityProtocol binding, MessageSenderType channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.binding = binding;
            this.channel = channel;
            this.timeoutHelper = new TimeoutHelper(timeout);
        }

        protected void Begin(Message message, SecurityProtocolCorrelationState correlationState)
        {
            IAsyncResult result = this.binding.BeginSecureOutgoingMessage(message, this.timeoutHelper.RemainingTime(), correlationState, ApplySecurityAndSendAsyncResult<MessageSenderType>.sharedCallback, this);
            if (result.CompletedSynchronously)
            {
                this.binding.EndSecureOutgoingMessage(result, out message, out this.newCorrelationState);
                if (this.OnSecureOutgoingMessageComplete(message))
                {
                    base.Complete(true);
                }
            }
        }

        protected abstract IAsyncResult BeginSendCore(MessageSenderType channel, Message message, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void EndSendCore(MessageSenderType channel, IAsyncResult result);
        protected static void OnEnd(ApplySecurityAndSendAsyncResult<MessageSenderType> self)
        {
            AsyncResult.End<ApplySecurityAndSendAsyncResult<MessageSenderType>>(self);
        }

        private bool OnSecureOutgoingMessageComplete(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            }
            this.secureOutgoingMessageDone = true;
            IAsyncResult result = this.BeginSendCore(this.channel, message, this.timeoutHelper.RemainingTime(), ApplySecurityAndSendAsyncResult<MessageSenderType>.sharedCallback, this);
            if (!result.CompletedSynchronously)
            {
                return false;
            }
            this.EndSendCore(this.channel, result);
            return this.OnSendComplete();
        }

        private bool OnSendComplete()
        {
            this.OnSendCompleteCore(this.timeoutHelper.RemainingTime());
            return true;
        }

        protected abstract void OnSendCompleteCore(TimeSpan timeout);
        private static void SharedCallback(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("result"));
            }
            if (!result.CompletedSynchronously)
            {
                ApplySecurityAndSendAsyncResult<MessageSenderType> asyncState = result.AsyncState as ApplySecurityAndSendAsyncResult<MessageSenderType>;
                if (asyncState == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidAsyncResult"), "result"));
                }
                bool flag = false;
                Exception exception = null;
                try
                {
                    if (!asyncState.secureOutgoingMessageDone)
                    {
                        Message message;
                        asyncState.binding.EndSecureOutgoingMessage(result, out message, out asyncState.newCorrelationState);
                        flag = asyncState.OnSecureOutgoingMessageComplete(message);
                    }
                    else
                    {
                        asyncState.EndSendCore(asyncState.channel, result);
                        flag = asyncState.OnSendComplete();
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
                if (flag)
                {
                    asyncState.Complete(false, exception);
                }
            }
        }

        protected SecurityProtocolCorrelationState CorrelationState
        {
            get
            {
                return this.newCorrelationState;
            }
        }

        protected System.ServiceModel.Security.SecurityProtocol SecurityProtocol
        {
            get
            {
                return this.binding;
            }
        }
    }
}

