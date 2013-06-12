namespace System.Net.Mime
{
    using System;
    using System.Net;
    using System.Threading;

    internal class MultiAsyncResult : LazyAsyncResult
    {
        private object context;
        private int outstanding;

        internal MultiAsyncResult(object context, AsyncCallback callback, object state) : base(context, state, callback)
        {
            this.context = context;
        }

        internal void CompleteSequence()
        {
            this.Decrement();
        }

        private void Decrement()
        {
            if (Interlocked.Decrement(ref this.outstanding) == -1)
            {
                base.InvokeCallback(base.Result);
            }
        }

        internal static object End(IAsyncResult result)
        {
            MultiAsyncResult result2 = (MultiAsyncResult) result;
            result2.InternalWaitForCompletion();
            return result2.Result;
        }

        internal void Enter()
        {
            this.Increment();
        }

        private void Increment()
        {
            Interlocked.Increment(ref this.outstanding);
        }

        internal void Leave()
        {
            this.Decrement();
        }

        internal void Leave(object result)
        {
            base.Result = result;
            this.Decrement();
        }

        internal object Context
        {
            get
            {
                return this.context;
            }
        }
    }
}

