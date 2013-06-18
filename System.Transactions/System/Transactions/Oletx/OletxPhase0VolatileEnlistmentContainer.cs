namespace System.Transactions.Oletx
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal class OletxPhase0VolatileEnlistmentContainer : OletxVolatileEnlistmentContainer
    {
        private bool aborting;
        private IPhase0EnlistmentShim phase0EnlistmentShim = null;
        private bool tmWentDown;

        internal OletxPhase0VolatileEnlistmentContainer(RealOletxTransaction realOletxTransaction)
        {
            base.realOletxTransaction = realOletxTransaction;
            base.phase = -1;
            this.aborting = false;
            this.tmWentDown = false;
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
            IPhase0EnlistmentShim shim = null;
            lock (this)
            {
                if (DiagnosticTrace.Verbose)
                {
                    string methodName = "OletxPhase0VolatileEnlistmentContainer.DecrementOutstandingNotifications, outstandingNotifications = " + this.outstandingNotifications.ToString(CultureInfo.CurrentCulture) + ", incompleteDependentClones = " + this.incompleteDependentClones.ToString(CultureInfo.CurrentCulture);
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), methodName);
                }
                base.outstandingNotifications--;
                base.collectedVoteYes = base.collectedVoteYes && voteYes;
                if ((base.outstandingNotifications == 0) && (base.incompleteDependentClones == 0))
                {
                    if ((base.phase == 0) && !base.alreadyVoted)
                    {
                        flag = true;
                        base.alreadyVoted = true;
                        shim = this.phase0EnlistmentShim;
                    }
                    base.realOletxTransaction.DecrementUndecidedEnlistments();
                }
            }
            try
            {
                if (flag && (shim != null))
                {
                    shim.Phase0Done(base.collectedVoteYes && !base.realOletxTransaction.Doomed);
                }
            }
            catch (COMException exception)
            {
                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN == exception.ErrorCode) || (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE == exception.ErrorCode))
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
                else
                {
                    if (System.Transactions.Oletx.NativeMethods.XACT_E_PROTOCOL != exception.ErrorCode)
                    {
                        throw;
                    }
                    this.phase0EnlistmentShim = null;
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
            }
            if (DiagnosticTrace.Verbose)
            {
                string str = "OletxPhase0VolatileEnlistmentContainer.DecrementOutstandingNotifications";
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), str);
            }
        }

        internal override void DependentCloneCompleted()
        {
            bool flag = false;
            lock (this)
            {
                if (DiagnosticTrace.Verbose)
                {
                    string methodName = "OletxPhase0VolatileEnlistmentContainer.DependentCloneCompleted, outstandingNotifications = " + this.outstandingNotifications.ToString(CultureInfo.CurrentCulture) + ", incompleteDependentClones = " + this.incompleteDependentClones.ToString(CultureInfo.CurrentCulture) + ", phase = " + this.phase.ToString(CultureInfo.CurrentCulture);
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), methodName);
                }
                base.incompleteDependentClones--;
                if ((base.incompleteDependentClones == 0) && (base.phase == 0))
                {
                    base.outstandingNotifications++;
                    flag = true;
                }
            }
            if (flag)
            {
                this.DecrementOutstandingNotifications(true);
            }
            if (DiagnosticTrace.Verbose)
            {
                string str = "OletxPhase0VolatileEnlistmentContainer.DependentCloneCompleted";
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
            if (TransactionStatus.Committed == outcome)
            {
                this.Committed();
            }
            else if (TransactionStatus.Aborted == outcome)
            {
                this.Aborted();
            }
            else if (TransactionStatus.InDoubt == outcome)
            {
                this.InDoubt();
            }
        }

        internal void Phase0Request(bool abortHint)
        {
            OletxVolatileEnlistment enlistment = null;
            int count = 0;
            OletxCommittableTransaction committableTransaction = null;
            bool flag = false;
            lock (this)
            {
                if (DiagnosticTrace.Verbose)
                {
                    string methodName = "OletxPhase0VolatileEnlistmentContainer.Phase0Request, abortHint = " + abortHint.ToString(CultureInfo.CurrentCulture) + ", phase = " + this.phase.ToString(CultureInfo.CurrentCulture);
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), methodName);
                }
                this.aborting = abortHint;
                committableTransaction = base.realOletxTransaction.committableTransaction;
                if ((committableTransaction != null) && !committableTransaction.CommitCalled)
                {
                    flag = true;
                    this.aborting = true;
                }
                if ((2 == base.phase) || (-1 == base.phase))
                {
                    if (-1 == base.phase)
                    {
                        base.phase = 0;
                    }
                    if ((this.aborting || this.tmWentDown) || (flag || (2 == base.phase)))
                    {
                        if (this.phase0EnlistmentShim != null)
                        {
                            try
                            {
                                this.phase0EnlistmentShim.Phase0Done(false);
                                base.alreadyVoted = true;
                            }
                            catch (COMException exception)
                            {
                                if (DiagnosticTrace.Verbose)
                                {
                                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                                }
                            }
                        }
                        return;
                    }
                    base.outstandingNotifications = base.enlistmentList.Count;
                    count = base.enlistmentList.Count;
                    if (count == 0)
                    {
                        base.outstandingNotifications = 1;
                    }
                }
                else
                {
                    if (DiagnosticTrace.Critical)
                    {
                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPhase0VolatileEnlistmentContainer.Phase0Request, phase != -1");
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
                }
            }
            if (count == 0)
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
                string str = "OletxPhase0VolatileEnlistmentContainer.Phase0Request, abortHint = " + abortHint.ToString(CultureInfo.CurrentCulture);
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), str);
            }
        }

        internal override void RollbackFromTransaction()
        {
            lock (this)
            {
                if (DiagnosticTrace.Verbose)
                {
                    string methodName = "OletxPhase0VolatileEnlistmentContainer.RollbackFromTransaction, outstandingNotifications = " + this.outstandingNotifications.ToString(CultureInfo.CurrentCulture) + ", incompleteDependentClones = " + this.incompleteDependentClones.ToString(CultureInfo.CurrentCulture);
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), methodName);
                }
                if ((base.phase == 0) && ((0 < base.outstandingNotifications) || (0 < base.incompleteDependentClones)))
                {
                    base.alreadyVoted = true;
                    if (this.Phase0EnlistmentShim != null)
                    {
                        this.Phase0EnlistmentShim.Phase0Done(false);
                    }
                }
            }
            if (DiagnosticTrace.Verbose)
            {
                string str = "OletxPhase0VolatileEnlistmentContainer.RollbackFromTransaction";
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), str);
            }
        }

        internal void TMDown()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPhase0VolatileEnlistmentContainer.TMDown");
            }
            this.tmWentDown = true;
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPhase0VolatileEnlistmentContainer.TMDown");
            }
        }

        internal bool NewEnlistmentsAllowed
        {
            get
            {
                return (-1 == base.phase);
            }
        }

        internal IPhase0EnlistmentShim Phase0EnlistmentShim
        {
            get
            {
                lock (this)
                {
                    return this.phase0EnlistmentShim;
                }
            }
            set
            {
                lock (this)
                {
                    if (this.aborting || this.tmWentDown)
                    {
                        value.Phase0Done(false);
                    }
                    this.phase0EnlistmentShim = value;
                }
            }
        }
    }
}

