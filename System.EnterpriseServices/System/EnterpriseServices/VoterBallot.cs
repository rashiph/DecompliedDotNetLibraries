namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Transactions;

    internal class VoterBallot : ITransactionVoterBallotAsync2, IEnlistmentNotification
    {
        private Enlistment enlistment;
        private ITransactionVoterNotifyAsync2 notification;
        private PreparingEnlistment preparingEnlistment;
        private const int S_OK = 0;
        private Transaction transaction;

        internal VoterBallot(ITransactionVoterNotifyAsync2 notification, Transaction transaction)
        {
            this.transaction = transaction;
            this.notification = notification;
            this.enlistment = transaction.EnlistVolatile(this, EnlistmentOptions.None);
        }

        public void Commit(Enlistment enlistment)
        {
            enlistment.Done();
            this.notification.Committed(false, 0, 0);
            Marshal.ReleaseComObject(this.notification);
            this.notification = null;
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
            this.notification.InDoubt();
            Marshal.ReleaseComObject(this.notification);
            this.notification = null;
        }

        public void Prepare(PreparingEnlistment enlistment)
        {
            this.preparingEnlistment = enlistment;
            this.notification.VoteRequest();
        }

        public void Rollback(Enlistment enlistment)
        {
            enlistment.Done();
            this.notification.Aborted(0, false, 0, 0);
            Marshal.ReleaseComObject(this.notification);
            this.notification = null;
        }

        public void VoteRequestDone(int hr, int reason)
        {
            if (this.preparingEnlistment == null)
            {
                Marshal.ThrowExceptionForHR(-2147418113);
            }
            if (hr == 0)
            {
                this.preparingEnlistment.Prepared();
            }
            else
            {
                this.preparingEnlistment.ForceRollback();
            }
        }
    }
}

