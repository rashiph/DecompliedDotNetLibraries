namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class EnlistmentTraceRecord : TraceRecord
    {
        private EnlistmentOptions enOptions;
        private EnlistmentTraceIdentifier enTraceId;
        private EnlistmentType enType;
        private static EnlistmentTraceRecord record = new EnlistmentTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource, EnlistmentTraceIdentifier enTraceId, EnlistmentType enType, EnlistmentOptions enOptions)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.enTraceId = enTraceId;
                record.enType = enType;
                record.enOptions = enOptions;
                DiagnosticTrace.TraceEvent(TraceEventType.Information, "http://msdn.microsoft.com/2004/06/System/Transactions/Enlistment", System.Transactions.SR.GetString("TraceEnlistment"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            TraceHelper.WriteEnId(xml, this.enTraceId);
            xml.WriteElementString("EnlistmentType", this.enType.ToString());
            xml.WriteElementString("EnlistmentOptions", this.enOptions.ToString());
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/EnlistmentTraceRecord";
            }
        }
    }
}

