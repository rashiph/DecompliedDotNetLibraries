namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;

    internal class WsatEtwTraceListener : TraceListener
    {
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void NotSupported()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType severity, int id, object data)
        {
            this.TraceInternal(eventCache, data.ToString(), id, severity);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType severity, int id, params object[] data)
        {
            this.NotSupported();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string message)
        {
            this.TraceInternal(eventCache, message, id, severity);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string format, params object[] args)
        {
            this.TraceInternal(eventCache, (args == null) ? format : string.Format(CultureInfo.CurrentCulture, format, args), id, severity);
        }

        private void TraceInternal(TraceEventCache eventCache, string xmlApplicationData, int eventId, TraceEventType type)
        {
            try
            {
                EtwTrace.Trace(xmlApplicationData, TraceTypeOf(type), eventId);
            }
            catch (Win32Exception exception)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Exception thrown from ETW Trace : {0} ", exception.Message);
                }
            }
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            try
            {
                EtwTrace.TraceTransfer(relatedActivityId);
            }
            catch (Win32Exception exception)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Exception thrown from ETW Trace : {0} ", exception.Message);
                }
            }
        }

        private static TraceType TraceTypeOf(TraceEventType type)
        {
            switch (type)
            {
                case TraceEventType.Start:
                    return TraceType.Start;

                case TraceEventType.Stop:
                    return TraceType.Stop;

                case TraceEventType.Suspend:
                    return TraceType.Suspend;

                case TraceEventType.Resume:
                    return TraceType.Resume;

                case TraceEventType.Transfer:
                    return TraceType.Transfer;
            }
            return TraceType.Trace;
        }

        public override void Write(string text)
        {
            this.WriteLine(text);
        }

        public override void WriteLine(string text)
        {
            EtwTrace.Trace(text, TraceType.Trace, 0);
        }
    }
}

