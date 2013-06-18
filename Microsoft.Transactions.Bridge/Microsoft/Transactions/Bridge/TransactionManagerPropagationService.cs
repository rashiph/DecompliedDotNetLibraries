namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Runtime;

    internal abstract class TransactionManagerPropagationService
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TransactionManagerPropagationService()
        {
        }

        public abstract void CreateSubordinateEnlistment(Enlistment enlistment, TransactionManagerCallback callback, object state);
        public abstract void CreateSuperiorEnlistment(Enlistment enlistment, EnlistmentOptions enlistmentOptions, TransactionManagerCallback callback, object state);
        public abstract void CreateTransaction(Enlistment enlistment, EnlistmentOptions enlistmentOptions, TransactionManagerCallback callback, object state);
    }
}

