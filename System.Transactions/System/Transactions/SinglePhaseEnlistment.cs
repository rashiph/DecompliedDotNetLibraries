namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    public class SinglePhaseEnlistment : Enlistment
    {
        internal SinglePhaseEnlistment(InternalEnlistment enlistment) : base(enlistment)
        {
        }

        public void Aborted()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.Aborted");
            }
            if (DiagnosticTrace.Warning)
            {
                EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.Aborted);
            }
            lock (base.internalEnlistment.SyncRoot)
            {
                base.internalEnlistment.State.Aborted(base.internalEnlistment, null);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.Aborted");
            }
        }

        public void Aborted(Exception e)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.Aborted");
            }
            if (DiagnosticTrace.Warning)
            {
                EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.Aborted);
            }
            lock (base.internalEnlistment.SyncRoot)
            {
                base.internalEnlistment.State.Aborted(base.internalEnlistment, e);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.Aborted");
            }
        }

        public void Committed()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.Committed");
                EnlistmentCallbackPositiveTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.Committed);
            }
            lock (base.internalEnlistment.SyncRoot)
            {
                base.internalEnlistment.State.Committed(base.internalEnlistment);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.Committed");
            }
        }

        public void InDoubt()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.InDoubt");
            }
            lock (base.internalEnlistment.SyncRoot)
            {
                if (DiagnosticTrace.Warning)
                {
                    EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.InDoubt);
                }
                base.internalEnlistment.State.InDoubt(base.internalEnlistment, null);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.InDoubt");
            }
        }

        public void InDoubt(Exception e)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.InDoubt");
            }
            lock (base.internalEnlistment.SyncRoot)
            {
                if (DiagnosticTrace.Warning)
                {
                    EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.InDoubt);
                }
                base.internalEnlistment.State.InDoubt(base.internalEnlistment, e);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "SinglePhaseEnlistment.InDoubt");
            }
        }
    }
}

