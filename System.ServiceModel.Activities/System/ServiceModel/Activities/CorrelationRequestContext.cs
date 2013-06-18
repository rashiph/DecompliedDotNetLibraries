namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;

    internal class CorrelationRequestContext
    {
        private System.Exception exceptionOnReply;
        private AsyncWaitHandle receivedReplyEvent;

        public void Cancel()
        {
            this.receivedReplyEvent.Set();
        }

        public void EnsureAsyncWaitHandle()
        {
            this.receivedReplyEvent = new AsyncWaitHandle();
        }

        public void ReceiveAsyncReply(System.ServiceModel.OperationContext operationContext, Message reply, System.Exception replyException)
        {
            this.OperationContext = operationContext;
            this.exceptionOnReply = replyException;
            this.Reply = reply;
            this.receivedReplyEvent.Set();
        }

        public void ReceiveReply(System.ServiceModel.OperationContext operationContext, Message reply)
        {
            this.OperationContext = operationContext;
            this.Reply = reply;
        }

        public bool TryGetReply()
        {
            if (this.exceptionOnReply != null)
            {
                throw FxTrace.Exception.AsError(this.exceptionOnReply);
            }
            return (this.Reply != null);
        }

        public bool WaitForReplyAsync(Action<object, TimeoutException> onReceiveReply, object state)
        {
            return (this.TryGetReply() || this.receivedReplyEvent.WaitAsync(onReceiveReply, state, TimeSpan.MaxValue));
        }

        public System.ServiceModel.Activities.Dispatcher.CorrelationKeyCalculator CorrelationKeyCalculator { get; set; }

        public System.Exception Exception { get; set; }

        public System.ServiceModel.OperationContext OperationContext { get; set; }

        public Message Reply { get; set; }
    }
}

