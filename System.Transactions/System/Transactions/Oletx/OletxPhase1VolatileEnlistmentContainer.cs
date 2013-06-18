namespace System.Transactions.Oletx
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal class OletxPhase1VolatileEnlistmentContainer : OletxVolatileEnlistmentContainer
    {
        private IVoterBallotShim voterBallotShim = null;
        internal IntPtr voterHandle = IntPtr.Zero;

        internal OletxPhase1VolatileEnlistmentContainer(RealOletxTransaction realOletxTransaction)
        {
            base.realOletxTransaction = realOletxTransaction;
            base.phase = -1;
            base.outstandingNotifications = 0;
            base.incompleteDependentClones = 0;
            base.alreadyVoted = false;
            base.collectedVoteYes = true;
            base.enlistmentList = new ArrayList();
            realOletxTransaction.IncrementUndecidedEnlistments();
        }

        internal override void Aborted()
        {
            OletxVolatileEnlistment enlistment = null;
            int count = 0;
            lock (this)
            {
                base.phase = 2;
                count = base.enlistmentList.Count;
            }
            for (int i = 0; i < count; i++)
            {
                enlistment = base.enlistmentList[i] as OletxVolatileEnlistment;
                if (enlistment == null)
                {
                    if (DiagnosticTrace.Critical)
                    {
                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
                }
                enlistment.Rollback();
            }
        }

        internal override void AddDependentClone()
        {
            lock (this)
            {
                if (-1 != base.phase)
                {
                    throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                }
                base.incompleteDependentClones++;
            }
        }

        internal void AddEnlistment(OletxVolatileEnlistment enlistment)
        {
            lock (this)
            {
                if (-1 != base.phase)
                {
                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TooLate"), null);
                }
                base.enlistmentList.Add(enlistment);
            }
        }

        internal override void Committed()
        {
            OletxVolatileEnlistment enlistment = null;
            int count = 0;
            lock (this)
            {
                base.phase = 2;
                count = base.enlistmentList.Count;
            }
            for (int i = 0; i < count; i++)
            {
                enlistment = base.enlistmentList[i] as OletxVolatileEnlistment;
                if (enlistment == null)
                {
                    if (DiagnosticTrace.Critical)
                    {
                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
                }
                enlistment.Commit();
            }
        }

        internal override void DecrementOutstandingNotifications(bool voteYes)
        {
            bool flag = false;
            IVoterBallotShim voterBallotShim = null;
            lock (this)
            {
                if (DiagnosticTrace.Verbose)
                {
                    string methodName = "OletxPhase1VolatileEnlistmentContainer.DecrementOutstandingNotifications, outstandingNotifications = " + this.outstandingNotifications.ToString(CultureInfo.CurrentCulture) + ", incompleteDependentClones = " + this.incompleteDependentClones.ToString(CultureInfo.CurrentCulture);
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), methodName);
                }
                base.outstandingNotifications--;
                base.collectedVoteYes = base.collectedVoteYes && voteYes;
                if (base.outstandingNotifications == 0)
                {
                    if ((1 == base.phase) && !base.alreadyVoted)
                    {
                        flag = true;
                        base.alreadyVoted = true;
                        voterBallotShim = this.VoterBallotShim;
                    }
                    base.realOletxTransaction.DecrementUndecidedEnlistments();
                }
            }
            try
            {
                if (flag)
                {
                    if (base.collectedVoteYes && !base.realOletxTransaction.Doomed)
                    {
                        if (voterBallotShim != null)
                        {
                            voterBallotShim.Vote(true);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (voterBallotShim != null)
                            {
                                voterBallotShim.Vote(false);
                            }
                            this.Aborted();
                        }
                        finally
                        {
                            HandleTable.FreeHandle(this.voterHandle);
                        }
                    }
                }
            }
            catch (COMException exception)
            {
                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception.ErrorCode))
                {
                    throw;
                }
                lock (this)
                {
                    if (1 == base.phase)
                    {
                        this.InDoubt();
                    }
                }
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
            }
            if (DiagnosticTrace.Verbose)
            {
                string str = "OletxPhase1VolatileEnlistmentContainer.DecrementOutstandingNotifications";
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), str);
            }
        }

        internal override void DependentCloneCompleted()
        {
            if (DiagnosticTrace.Verbose)
            {
                string methodName = "OletxPhase1VolatileEnlistmentContainer.DependentCloneCompleted, outstandingNotifications = " + this.outstandingNotifications.ToString(CultureInfo.CurrentCulture) + ", incompleteDependentClones = " + this.incompleteDependentClones.ToString(CultureInfo.CurrentCulture) + ", phase = " + this.phase.ToString(CultureInfo.CurrentCulture);
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), methodName);
            }
            lock (this)
            {
                base.incompleteDependentClones--;
            }
            if (DiagnosticTrace.Verbose)
            {
                string str = "OletxPhase1VolatileEnlistmentContainer.DependentCloneCompleted";
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), str);
            }
        }

        internal override void InDoubt()
        {
            OletxVolatileEnlistment enlistment = null;
            int count = 0;
            lock (this)
            {
                base.phase = 2;
                count = base.enlistmentList.Count;
            }
            for (int i = 0; i < count; i++)
            {
                enlistment = base.enlistmentList[i] as OletxVolatileEnlistment;
                if (enlistment == null)
                {
                    if (DiagnosticTrace.Critical)
                    {
                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
                }
                enlistment.InDoubt();
            }
        }

        internal override void OutcomeFromTransaction(TransactionStatus outcome)
        {
            bool flag2 = false;
            bool flag = false;
            lock (this)
            {
                if ((1 == base.phase) && (0 < base.outstandingNotifications))
                {
                    if (TransactionStatus.Aborted == outcome)
                    {
                        flag2 = true;
                    }
                    else if (TransactionStatus.InDoubt == outcome)
                    {
                        flag = true;
                    }
                }
            }
            if (flag2)
            {
                this.Aborted();
            }
            if (flag)
            {
                this.InDoubt();
            }
        }

        internal override void RollbackFromTransaction()
        {
            bool flag = false;
            IVoterBallotShim voterBallotShim = null;
            lock (this)
            {
                if (DiagnosticTrace.Verbose)
                {
                    string methodName = "OletxPhase1VolatileEnlistmentContainer.RollbackFromTransaction, outstandingNotifications = " + this.outstandingNotifications.ToString(CultureInfo.CurrentCulture) + ", incompleteDependentClones = " + this.incompleteDependentClones.ToString(CultureInfo.CurrentCulture);
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), methodName);
                }
                if ((1 == base.phase) && (0 < base.outstandingNotifications))
                {
                    base.alreadyVoted = true;
                    flag = true;
                    voterBallotShim = this.voterBallotShim;
                }
            }
            if (flag)
            {
                try
                {
                    if (voterBallotShim != null)
                    {
                        voterBallotShim.Vote(false);
                    }
                    this.Aborted();
                }
                catch (COMException exception)
                {
                    if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception.ErrorCode))
                    {
                        throw;
                    }
                    lock (this)
                    {
                        if (1 == base.phase)
                        {
                            this.InDoubt();
                        }
                    }
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
                finally
                {
                    HandleTable.FreeHandle(this.voterHandle);
                }
            }
            if (DiagnosticTrace.Verbose)
            {
                string str = "OletxPhase1VolatileEnlistmentContainer.RollbackFromTransaction";
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), str);
            }
        }

        internal void VoteRequest()
        {
            OletxVolatileEnlistment enlistment = null;
            int count = 0;
            bool flag = false;
            lock (this)
            {
                if (DiagnosticTrace.Verbose)
                {
                    string methodName = "OletxPhase1VolatileEnlistmentContainer.VoteRequest";
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), methodName);
                }
                base.phase = 1;
                if (0 < base.incompleteDependentClones)
                {
                    flag = true;
                    base.outstandingNotifications = 1;
                }
                else
                {
                    base.outstandingNotifications = base.enlistmentList.Count;
                    count = base.enlistmentList.Count;
                    if (count == 0)
                    {
                        base.outstandingNotifications = 1;
                    }
                }
                base.realOletxTransaction.TooLateForEnlistments = true;
            }
            if (flag)
            {
                this.DecrementOutstandingNotifications(false);
            }
            else if (count == 0)
            {
                this.DecrementOutstandingNotifications(true);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    enlistment = base.enlistmentList[i] as OletxVolatileEnlistment;
                    if (enlistment == null)
                    {
                        if (DiagnosticTrace.Critical)
                        {
                            InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                        }
                        throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
                    }
                    enlistment.Prepare(this);
                }
            }
            if (DiagnosticTrace.Verbose)
            {
                string str = "OletxPhase1VolatileEnlistmentContainer.VoteRequest";
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), str);
            }
        }

        internal IVoterBallotShim VoterBallotShim
        {
            get
            {
                lock (this)
                {
                    return this.voterBallotShim;
                }
            }
            set
            {
                lock (this)
                {
                    this.voterBallotShim = value;
                }
            }
        }
    }
}

