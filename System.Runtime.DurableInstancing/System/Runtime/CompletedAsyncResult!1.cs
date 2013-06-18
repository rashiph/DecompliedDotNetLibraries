namespace System.Runtime
{
    using System;

    internal class CompletedAsyncResult<T> : AsyncResult
    {
        private T data;

        public CompletedAsyncResult(T data, AsyncCallback callback, object state) : base(callback, state)
        {
            this.data = data;
            base.Complete(true);
        }

        public static T End(IAsyncResult result)
        {
            Fx.AssertAndThrowFatal(result.IsCompleted, "CompletedAsyncResult<T> was not completed!");
            return AsyncResult.End<CompletedAsyncResult<T>>(result).data;
        }
    }
}

