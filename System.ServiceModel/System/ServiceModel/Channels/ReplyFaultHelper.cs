namespace System.ServiceModel.Channels
{
    using System;

    internal class ReplyFaultHelper : TypedFaultHelper<FaultState>
    {
        public ReplyFaultHelper(TimeSpan defaultSendTimeout, TimeSpan defaultCloseTimeout) : base(defaultSendTimeout, defaultCloseTimeout)
        {
        }

        protected override void AbortState(FaultState faultState, bool isOnAbortThread)
        {
            if (!isOnAbortThread)
            {
                faultState.FaultMessage.Close();
            }
            faultState.RequestContext.Abort();
        }

        protected override IAsyncResult BeginSendFault(IReliableChannelBinder binder, FaultState faultState, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return faultState.RequestContext.BeginReply(faultState.FaultMessage, timeout, callback, state);
        }

        protected override void EndSendFault(IReliableChannelBinder binder, FaultState faultState, IAsyncResult result)
        {
            faultState.RequestContext.EndReply(result);
            faultState.FaultMessage.Close();
        }

        protected override FaultState GetState(RequestContext requestContext, Message faultMessage)
        {
            return new FaultState(requestContext, faultMessage);
        }
    }
}

