namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.Transactions;

    internal sealed class PersistenceContextEnlistment : IEnlistmentNotification
    {
        private static Action<object> commitCallback;
        private List<PersistenceContext> enlistedContexts;
        private Enlistment enlistment;
        private static Action<object> indoubtCallback;
        private static Action<object> prepareCallback;
        private PreparingEnlistment preparingEnlistment;
        private static Action<object> rollbackCallback;
        private object ThisLock = new object();
        private bool tooLateForMoreUndo;
        private Transaction transaction;

        internal PersistenceContextEnlistment(PersistenceContext context, Transaction transaction)
        {
            this.transaction = transaction;
            this.enlistedContexts = new List<PersistenceContext>();
            this.enlistedContexts.Add(context);
        }

        internal void AddToEnlistment(PersistenceContext context)
        {
            lock (this.ThisLock)
            {
                if (this.tooLateForMoreUndo)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.PersistenceTooLateToEnlist));
                }
                this.enlistedContexts.Add(context);
            }
        }

        internal static void DoCommit(object state)
        {
            PersistenceContextEnlistment enlistment = state as PersistenceContextEnlistment;
            lock (enlistment.ThisLock)
            {
                foreach (PersistenceContext context in enlistment.enlistedContexts)
                {
                    context.ScheduleNextTransactionWaiter();
                }
            }
            lock (PersistenceContext.Enlistments)
            {
                PersistenceContext.Enlistments.Remove(enlistment.transaction.GetHashCode());
            }
            enlistment.enlistment.Done();
        }

        internal static void DoIndoubt(object state)
        {
            PersistenceContextEnlistment enlistment = state as PersistenceContextEnlistment;
            lock (enlistment.ThisLock)
            {
                enlistment.tooLateForMoreUndo = true;
                foreach (PersistenceContext context in enlistment.enlistedContexts)
                {
                    context.Abort();
                    context.ScheduleNextTransactionWaiter();
                }
            }
            lock (PersistenceContext.Enlistments)
            {
                PersistenceContext.Enlistments.Remove(enlistment.transaction.GetHashCode());
            }
            enlistment.enlistment.Done();
        }

        internal static void DoPrepare(object state)
        {
            PersistenceContextEnlistment enlistment = state as PersistenceContextEnlistment;
            lock (enlistment.ThisLock)
            {
                enlistment.tooLateForMoreUndo = true;
            }
            enlistment.preparingEnlistment.Prepared();
        }

        internal static void DoRollback(object state)
        {
            PersistenceContextEnlistment enlistment = state as PersistenceContextEnlistment;
            lock (enlistment.ThisLock)
            {
                enlistment.tooLateForMoreUndo = true;
                foreach (PersistenceContext context in enlistment.enlistedContexts)
                {
                    context.Abort();
                    context.ScheduleNextTransactionWaiter();
                }
            }
            lock (PersistenceContext.Enlistments)
            {
                PersistenceContext.Enlistments.Remove(enlistment.transaction.GetHashCode());
            }
            enlistment.enlistment.Done();
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            this.enlistment = enlistment;
            ActionItem.Schedule(CommitCallback, this);
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            this.enlistment = enlistment;
            ActionItem.Schedule(IndoubtCallback, this);
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            this.preparingEnlistment = preparingEnlistment;
            ActionItem.Schedule(PrepareCallback, this);
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            this.enlistment = enlistment;
            ActionItem.Schedule(RollbackCallback, this);
        }

        internal static Action<object> CommitCallback
        {
            get
            {
                if (commitCallback == null)
                {
                    commitCallback = new Action<object>(PersistenceContextEnlistment.DoCommit);
                }
                return commitCallback;
            }
        }

        internal static Action<object> IndoubtCallback
        {
            get
            {
                if (indoubtCallback == null)
                {
                    indoubtCallback = new Action<object>(PersistenceContextEnlistment.DoIndoubt);
                }
                return indoubtCallback;
            }
        }

        internal static Action<object> PrepareCallback
        {
            get
            {
                if (prepareCallback == null)
                {
                    prepareCallback = new Action<object>(PersistenceContextEnlistment.DoPrepare);
                }
                return prepareCallback;
            }
        }

        internal static Action<object> RollbackCallback
        {
            get
            {
                if (rollbackCallback == null)
                {
                    rollbackCallback = new Action<object>(PersistenceContextEnlistment.DoRollback);
                }
                return rollbackCallback;
            }
        }
    }
}

