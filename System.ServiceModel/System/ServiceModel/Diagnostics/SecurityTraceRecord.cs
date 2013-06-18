namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;

    internal class SecurityTraceRecord : TraceRecord
    {
        private string traceName;

        internal SecurityTraceRecord(string traceName)
        {
            if (string.IsNullOrEmpty(traceName))
            {
                this.traceName = "Empty";
            }
            else
            {
                this.traceName = traceName;
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId(this.traceName);
            }
        }
    }
}

