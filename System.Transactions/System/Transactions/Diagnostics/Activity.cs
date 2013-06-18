namespace System.Transactions.Diagnostics
{
    using System;

    internal class Activity : IDisposable
    {
        private bool emitTransfer;
        private bool mustDispose;
        private Guid newGuid;
        private Guid oldGuid;

        private Activity(ref Guid newGuid, bool emitTransfer)
        {
            this.emitTransfer = emitTransfer;
            if (DiagnosticTrace.ShouldCorrelate && (newGuid != Guid.Empty))
            {
                this.newGuid = newGuid;
                this.oldGuid = DiagnosticTrace.GetActivityId();
                if (this.oldGuid != newGuid)
                {
                    this.mustDispose = true;
                    if (this.emitTransfer)
                    {
                        DiagnosticTrace.TraceTransfer(newGuid);
                    }
                    DiagnosticTrace.SetActivityId(newGuid);
                }
            }
        }

        internal static Activity CreateActivity(Guid newGuid, bool emitTransfer)
        {
            Activity activity = null;
            if ((DiagnosticTrace.ShouldCorrelate && (newGuid != Guid.Empty)) && (newGuid != DiagnosticTrace.GetActivityId()))
            {
                activity = new Activity(ref newGuid, emitTransfer);
            }
            return activity;
        }

        public void Dispose()
        {
            if (this.mustDispose)
            {
                this.mustDispose = false;
                if (this.emitTransfer)
                {
                    DiagnosticTrace.TraceTransfer(this.oldGuid);
                }
                DiagnosticTrace.SetActivityId(this.oldGuid);
            }
        }
    }
}

