namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    public class PreparingEnlistment : Enlistment
    {
        internal PreparingEnlistment(InternalEnlistment enlistment) : base(enlistment)
        {
        }

        public void ForceRollback()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "PreparingEnlistment.ForceRollback");
            }
            if (DiagnosticTrace.Warning)
            {
                EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.ForceRollback);
            }
            lock (base.internalEnlistment.SyncRoot)
            {
                base.internalEnlistment.State.ForceRollback(base.internalEnlistment, null);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "PreparingEnlistment.ForceRollback");
            }
        }

        public void ForceRollback(Exception e)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "PreparingEnlistment.ForceRollback");
            }
            if (DiagnosticTrace.Warning)
            {
                EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.ForceRollback);
            }
            lock (base.internalEnlistment.SyncRoot)
            {
                base.internalEnlistment.State.ForceRollback(base.internalEnlistment, e);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "PreparingEnlistment.ForceRollback");
            }
        }

        public void Prepared()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "PreparingEnlistment.Prepared");
                EnlistmentCallbackPositiveTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.Prepared);
            }
            lock (base.internalEnlistment.SyncRoot)
            {
                base.internalEnlistment.State.Prepared(base.internalEnlistment);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "PreparingEnlistment.Prepared");
            }
        }

        public byte[] RecoveryInformation()
        {
            byte[] buffer;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "PreparingEnlistment.RecoveryInformation");
            }
            try
            {
                lock (base.internalEnlistment.SyncRoot)
                {
                    return base.internalEnlistment.State.RecoveryInformation(base.internalEnlistment);
                }
            }
            finally
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "PreparingEnlistment.RecoveryInformation");
                }
            }
            return buffer;
        }
    }
}

