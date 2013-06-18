namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class ReliableBinderRequestAsyncResult : ReliableOutputAsyncResult
    {
        private Message reply;

        public ReliableBinderRequestAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
        }

        protected override IAsyncResult BeginOperation(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.ClientBinder.BeginRequest(base.Message, timeout, base.MaskingMode, callback, state);
        }

        public static Message End(IAsyncResult result)
        {
            Exception exception;
            return End(result, out exception);
        }

        public static Message End(IAsyncResult result, out Exception handledException)
        {
            ReliableBinderRequestAsyncResult result2 = AsyncResult.End<ReliableBinderRequestAsyncResult>(result);
            handledException = result2.HandledException;
            return result2.reply;
        }

        protected override void EndOperation(IAsyncResult result)
        {
            this.reply = this.ClientBinder.EndRequest(result);
        }

        protected IClientReliableChannelBinder ClientBinder
        {
            get
            {
                return (IClientReliableChannelBinder) base.Binder;
            }
        }

        protected Message Reply
        {
            get
            {
                return this.reply;
            }
        }
    }
}

