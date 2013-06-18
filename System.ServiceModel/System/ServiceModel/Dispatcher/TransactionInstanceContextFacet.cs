namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;

    internal sealed class TransactionInstanceContextFacet
    {
        internal Transaction Attached;
        private Transaction current;
        private InstanceContext instanceContext;
        private object mutex;
        private IResumeMessageRpc paused;
        private Dictionary<Transaction, RemoveReferenceRM> pending;
        private bool shouldReleaseInstance;
        internal Transaction waiting;

        internal TransactionInstanceContextFacet(InstanceContext instanceContext)
        {
            this.instanceContext = instanceContext;
            this.mutex = instanceContext.ThisLock;
        }

        internal void AddReference(ref MessageRpc rpc, Transaction tx, bool updateCallCount)
        {
            lock (this.mutex)
            {
                if (this.pending == null)
                {
                    this.pending = new Dictionary<Transaction, RemoveReferenceRM>();
                }
                if (tx != null)
                {
                    RemoveReferenceRM erm;
                    if (this.pending == null)
                    {
                        this.pending = new Dictionary<Transaction, RemoveReferenceRM>();
                    }
                    if (!this.pending.TryGetValue(tx, out erm))
                    {
                        RemoveReferenceRM erm2 = new RemoveReferenceRM(this.instanceContext, tx, rpc.Operation.Name) {
                            CallCount = 1L
                        };
                        this.pending.Add(tx, erm2);
                    }
                    else if (updateCallCount)
                    {
                        erm.CallCount += 1L;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void CheckIfTxCompletedAndUpdateAttached(ref MessageRpc rpc, bool isConcurrent)
        {
            if (rpc.Transaction.Current != null)
            {
                lock (this.mutex)
                {
                    if (!isConcurrent)
                    {
                        if (this.shouldReleaseInstance)
                        {
                            this.shouldReleaseInstance = false;
                            if (rpc.Error == null)
                            {
                                rpc.Error = TransactionBehavior.CreateFault(System.ServiceModel.SR.GetString("SFxTransactionAsyncAborted"), "TransactionAborted", true);
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(rpc.Error, TraceEventType.Error);
                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    TraceUtility.TraceEvent(TraceEventType.Information, 0xe0009, System.ServiceModel.SR.GetString("TraceCodeTxCompletionStatusCompletedForAsyncAbort", new object[] { rpc.Transaction.Current.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                                }
                            }
                        }
                        if (rpc.Transaction.IsCompleted || (rpc.Error != null))
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                if (rpc.Error != null)
                                {
                                    TraceUtility.TraceEvent(TraceEventType.Information, 0xe0006, System.ServiceModel.SR.GetString("TraceCodeTxCompletionStatusCompletedForError", new object[] { rpc.Transaction.Current.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                                }
                                else
                                {
                                    TraceUtility.TraceEvent(TraceEventType.Information, 0xe0005, System.ServiceModel.SR.GetString("TraceCodeTxCompletionStatusCompletedForAutocomplete", new object[] { rpc.Transaction.Current.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                                }
                            }
                            this.Attached = null;
                            if (this.waiting != null)
                            {
                                DiagnosticUtility.FailFast("waiting should be null when resetting current");
                            }
                            this.current = null;
                        }
                        else
                        {
                            this.Attached = rpc.Transaction.Current;
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information, 0xe000a, System.ServiceModel.SR.GetString("TraceCodeTxCompletionStatusRemainsAttached", new object[] { rpc.Transaction.Current.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                            }
                        }
                    }
                    else if (!this.pending.ContainsKey(rpc.Transaction.Current) && (rpc.Error == null))
                    {
                        rpc.Error = TransactionBehavior.CreateFault(System.ServiceModel.SR.GetString("SFxTransactionAsyncAborted"), "TransactionAborted", true);
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(rpc.Error, TraceEventType.Error);
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information, 0xe0009, System.ServiceModel.SR.GetString("TraceCodeTxCompletionStatusCompletedForAsyncAbort", new object[] { rpc.Transaction.Current.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                        }
                    }
                }
            }
        }

        internal static void Complete(Transaction transaction, Exception error)
        {
            try
            {
                if (error == null)
                {
                    CommittableTransaction transaction2 = transaction as CommittableTransaction;
                    if (transaction2 != null)
                    {
                        transaction2.Commit();
                    }
                    else
                    {
                        DependentTransaction transaction3 = transaction as DependentTransaction;
                        if (transaction3 != null)
                        {
                            transaction3.Complete();
                        }
                    }
                }
                else
                {
                    transaction.Rollback();
                }
            }
            catch (TransactionException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(System.ServiceModel.SR.GetString("SFxTransactionAsyncAborted"), "TransactionAborted", true));
            }
        }

        internal void CompletePendingTransaction(Transaction transaction, Exception error)
        {
            lock (this.mutex)
            {
                if (this.pending.ContainsKey(transaction))
                {
                    Complete(transaction, error);
                }
            }
        }

        internal TransactionScope CreateTransactionScope(Transaction transaction)
        {
            lock (this.mutex)
            {
                if (this.pending.ContainsKey(transaction))
                {
                    try
                    {
                        return new TransactionScope(transaction);
                    }
                    catch (TransactionException exception)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(System.ServiceModel.SR.GetString("SFxTransactionAsyncAborted"), "TransactionAborted", true));
        }

        internal void RemoveReference(Transaction tx)
        {
            lock (this.mutex)
            {
                if (tx.Equals(this.current))
                {
                    if (this.waiting != null)
                    {
                        bool flag;
                        this.current = this.waiting;
                        this.waiting = null;
                        if (this.instanceContext.Behavior.ReleaseServiceInstanceOnTransactionComplete)
                        {
                            this.instanceContext.ReleaseServiceInstance();
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information, 0xe000c, System.ServiceModel.SR.GetString("TraceCodeTxReleaseServiceInstanceOnCompletion", new object[] { tx.TransactionInformation.LocalIdentifier }));
                            }
                        }
                        this.paused.Resume(out flag);
                        if (!flag)
                        {
                        }
                    }
                    else
                    {
                        this.shouldReleaseInstance = true;
                        this.current = null;
                    }
                }
                if ((this.pending != null) && this.pending.ContainsKey(tx))
                {
                    this.pending.Remove(tx);
                }
            }
        }

        internal void SetCurrent(ref MessageRpc rpc)
        {
            Transaction current = rpc.Transaction.Current;
            if (current == null)
            {
                DiagnosticUtility.FailFast("we should never get here with a requestTransaction null");
            }
            lock (this.mutex)
            {
                if (this.current == null)
                {
                    this.current = current;
                }
                else if (this.current != current)
                {
                    this.waiting = current;
                    this.paused = rpc.Pause();
                }
                else
                {
                    rpc.Transaction.Current = this.current;
                }
            }
        }

        internal bool ShouldReleaseInstance
        {
            get
            {
                return this.shouldReleaseInstance;
            }
            set
            {
                this.shouldReleaseInstance = value;
            }
        }

        private sealed class RemoveReferenceRM : TransactionInstanceContextFacet.VolatileBase
        {
            private long callCount;
            private EndpointDispatcher endpointDispatcher;
            private string operation;

            internal RemoveReferenceRM(InstanceContext instanceContext, Transaction tx, string operation) : base(instanceContext, tx)
            {
                this.operation = operation;
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    this.endpointDispatcher = PerformanceCounters.GetEndpointDispatcher();
                }
                AspNetEnvironment.Current.IncrementBusyCount();
                if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceIncrementBusyCount(base.GetType().FullName);
                }
            }

            public override void Commit(Enlistment enlistment)
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxCommitted(this.endpointDispatcher, this.CallCount);
                }
                base.Commit(enlistment);
            }

            protected override void Completed()
            {
                base.InstanceContext.Transaction.RemoveReference(base.Transaction);
                AspNetEnvironment.Current.DecrementBusyCount();
                if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceDecrementBusyCount(base.GetType().FullName);
                }
            }

            public override void InDoubt(Enlistment enlistment)
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxInDoubt(this.endpointDispatcher, this.CallCount);
                }
                base.InDoubt(enlistment);
            }

            public override void Rollback(Enlistment enlistment)
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxAborted(this.endpointDispatcher, this.CallCount);
                }
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0xe000d, System.ServiceModel.SR.GetString("TraceCodeTxAsyncAbort", new object[] { base.Transaction.TransactionInformation.LocalIdentifier }));
                }
                base.Rollback(enlistment);
            }

            public override void SinglePhaseCommit(SinglePhaseEnlistment enlistment)
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxCommitted(this.endpointDispatcher, this.CallCount);
                }
                base.SinglePhaseCommit(enlistment);
            }

            internal long CallCount
            {
                get
                {
                    return this.callCount;
                }
                set
                {
                    this.callCount = value;
                }
            }
        }

        private abstract class VolatileBase : ISinglePhaseNotification, IEnlistmentNotification
        {
            protected System.ServiceModel.InstanceContext InstanceContext;
            protected System.Transactions.Transaction Transaction;

            protected VolatileBase(System.ServiceModel.InstanceContext instanceContext, System.Transactions.Transaction transaction)
            {
                this.InstanceContext = instanceContext;
                this.Transaction = transaction;
                this.Transaction.EnlistVolatile((ISinglePhaseNotification) this, EnlistmentOptions.None);
            }

            public virtual void Commit(Enlistment enlistment)
            {
                this.Completed();
            }

            protected abstract void Completed();
            public virtual void InDoubt(Enlistment enlistment)
            {
                this.Completed();
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }

            public virtual void Rollback(Enlistment enlistment)
            {
                this.Completed();
            }

            public virtual void SinglePhaseCommit(SinglePhaseEnlistment enlistment)
            {
                enlistment.Committed();
                this.Completed();
            }
        }
    }
}

