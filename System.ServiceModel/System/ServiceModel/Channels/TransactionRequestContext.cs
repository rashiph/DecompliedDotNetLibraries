namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    internal sealed class TransactionRequestContext : RequestContextBase
    {
        private RequestContext innerContext;
        private ITransactionChannel transactionChannel;

        public TransactionRequestContext(ITransactionChannel transactionChannel, ChannelBase channel, RequestContext innerContext, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(innerContext.RequestMessage, defaultCloseTimeout, defaultSendTimeout)
        {
            this.transactionChannel = transactionChannel;
            this.innerContext = innerContext;
        }

        protected override void OnAbort()
        {
            if (this.innerContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
            this.innerContext.Abort();
        }

        protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.innerContext == null)
            {
                throw TraceUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName), message);
            }
            if (message != null)
            {
                this.transactionChannel.WriteIssuedTokens(message, MessageDirection.Output);
            }
            return this.innerContext.BeginReply(message, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.innerContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
            this.innerContext.Close(timeout);
        }

        protected override void OnEndReply(IAsyncResult result)
        {
            if (this.innerContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
            this.innerContext.EndReply(result);
        }

        protected override void OnReply(Message message, TimeSpan timeout)
        {
            if (this.innerContext == null)
            {
                throw TraceUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName), message);
            }
            if (message != null)
            {
                this.transactionChannel.WriteIssuedTokens(message, MessageDirection.Output);
            }
            this.innerContext.Reply(message, timeout);
        }
    }
}

