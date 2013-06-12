namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventRecordWrittenEventArgs : EventArgs
    {
        private Exception exception;
        private System.Diagnostics.Eventing.Reader.EventRecord record;

        internal EventRecordWrittenEventArgs(EventLogException exception)
        {
            this.exception = exception;
        }

        internal EventRecordWrittenEventArgs(EventLogRecord record)
        {
            this.record = record;
        }

        public Exception EventException
        {
            get
            {
                return this.exception;
            }
        }

        public System.Diagnostics.Eventing.Reader.EventRecord EventRecord
        {
            get
            {
                return this.record;
            }
        }
    }
}

