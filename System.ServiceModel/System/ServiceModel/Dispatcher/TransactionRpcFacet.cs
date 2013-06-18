namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;

    internal class TransactionRpcFacet
    {
        internal Transaction Clone;
        internal Transaction Current;
        internal DependentTransaction dependentClone;
        internal bool IsCompleted;
        internal MessageRpc rpc;
        private TransactionScope scope;
        private bool transactionSetComplete;

        internal TransactionRpcFacet()
        {
            this.IsCompleted = true;
        }

        internal TransactionRpcFacet(ref MessageRpc rpc)
        {
            this.IsCompleted = true;
            this.rpc = rpc;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void Complete(Exception error)
        {
            if (!object.ReferenceEquals(this.Current, null))
            {
                TransactedBatchContext transactedBatchContext = this.rpc.TransactedBatchContext;
                if (transactedBatchContext != null)
                {
                    if (error == null)
                    {
                        transactedBatchContext.Complete();
                    }
                    else
                    {
                        transactedBatchContext.ForceRollback();
                    }
                    transactedBatchContext.InDispatch = false;
                }
                else if (this.transactionSetComplete)
                {
                    this.rpc.InstanceContext.Transaction.CompletePendingTransaction(this.Current, null);
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0xe0007, System.ServiceModel.SR.GetString("TraceCodeTxCompletionStatusCompletedForSetComplete", new object[] { this.Current.TransactionInformation.LocalIdentifier, this.rpc.Operation.Name }));
                    }
                }
                else if (this.IsCompleted || (error != null))
                {
                    this.rpc.InstanceContext.Transaction.CompletePendingTransaction(this.Current, error);
                }
                if (this.rpc.Operation.IsInsideTransactedReceiveScope)
                {
                    this.CompleteDependentClone();
                }
                this.Current = null;
            }
        }

        internal void Completed()
        {
            if (this.scope != null)
            {
                if (this.rpc.Operation.TransactionAutoComplete)
                {
                    try
                    {
                        this.Current.Rollback();
                    }
                    catch (ObjectDisposedException exception)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionInvalidSetTransactionComplete", new object[] { this.rpc.Operation.Name, this.rpc.Host.Description.Name })));
                }
                if (this.transactionSetComplete)
                {
                    try
                    {
                        this.Current.Rollback();
                    }
                    catch (ObjectDisposedException exception2)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMultiSetTransactionComplete", new object[] { this.rpc.Operation.Name, this.rpc.Host.Description.Name })));
                }
                this.transactionSetComplete = true;
                this.IsCompleted = true;
                this.scope.Complete();
            }
        }

        internal void CompleteDependentClone()
        {
            if (this.dependentClone != null)
            {
                this.dependentClone.Complete();
            }
        }

        internal void CreateDependentClone()
        {
            if ((this.dependentClone == null) && (this.Clone != null))
            {
                this.dependentClone = this.Clone.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
            }
        }

        internal void SetIncomplete()
        {
            this.IsCompleted = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ThreadEnter(ref Exception error)
        {
            Transaction clone = this.Clone;
            if ((clone != null) && (error == null))
            {
                this.scope = this.rpc.InstanceContext.Transaction.CreateTransactionScope(clone);
                this.transactionSetComplete = false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ThreadLeave()
        {
            if (this.scope != null)
            {
                if (!this.transactionSetComplete)
                {
                    this.scope.Complete();
                }
                try
                {
                    this.scope.Dispose();
                }
                catch (TransactionException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(System.ServiceModel.SR.GetString("SFxTransactionAsyncAborted"), "TransactionAborted", true));
                }
            }
        }
    }
}

