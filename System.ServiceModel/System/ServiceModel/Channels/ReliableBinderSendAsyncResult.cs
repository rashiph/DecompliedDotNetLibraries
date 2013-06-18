namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class ReliableBinderSendAsyncResult : ReliableOutputAsyncResult
    {
        public ReliableBinderSendAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
        }

        protected override IAsyncResult BeginOperation(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.Binder.BeginSend(base.Message, timeout, base.MaskingMode, callback, state);
        }

        public static void End(IAsyncResult result)
        {
            Exception exception;
            End(result, out exception);
        }

        public static void End(IAsyncResult result, out Exception handledException)
        {
            ReliableBinderSendAsyncResult result2 = AsyncResult.End<ReliableBinderSendAsyncResult>(result);
            handledException = result2.HandledException;
        }

        protected override void EndOperation(IAsyncResult result)
        {
            base.Binder.EndSend(result);
        }
    }
}

