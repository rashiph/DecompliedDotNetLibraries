namespace System.Runtime
{
    using System;
    using System.Runtime.InteropServices;

    internal class CompletedAsyncResult<TResult, TParameter> : AsyncResult
    {
        private TParameter parameter;
        private TResult resultData;

        public CompletedAsyncResult(TResult resultData, TParameter parameter, AsyncCallback callback, object state) : base(callback, state)
        {
            this.resultData = resultData;
            this.parameter = parameter;
            base.Complete(true);
        }

        public static TResult End(IAsyncResult result, out TParameter parameter)
        {
            Fx.AssertAndThrowFatal(result.IsCompleted, "CompletedAsyncResult<T> was not completed!");
            CompletedAsyncResult<TResult, TParameter> result2 = AsyncResult.End<CompletedAsyncResult<TResult, TParameter>>(result);
            parameter = result2.parameter;
            return result2.resultData;
        }
    }
}

