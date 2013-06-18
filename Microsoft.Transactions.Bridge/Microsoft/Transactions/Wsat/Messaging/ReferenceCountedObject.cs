namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Threading;

    internal abstract class ReferenceCountedObject
    {
        private int refCount = 1;

        protected ReferenceCountedObject()
        {
        }

        public void AddRef()
        {
            if (Interlocked.Increment(ref this.refCount) <= 0)
            {
                DiagnosticUtility.FailFast("Reference count below 0");
            }
        }

        protected abstract void Close();
        public void Release()
        {
            int num = Interlocked.Decrement(ref this.refCount);
            if (num < 0)
            {
                DiagnosticUtility.FailFast("Reference count below 0");
            }
            if (num == 0)
            {
                this.Close();
            }
        }
    }
}

