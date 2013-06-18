namespace System.Web.Management
{
    using System;
    using System.Web;

    public class WebSuccessAuditEvent : WebAuditEvent
    {
        internal WebSuccessAuditEvent()
        {
        }

        protected internal WebSuccessAuditEvent(string message, object eventSource, int eventCode) : base(message, eventSource, eventCode)
        {
        }

        protected internal WebSuccessAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode) : base(message, eventSource, eventCode, eventDetailCode)
        {
        }

        protected internal override void IncrementPerfCounters()
        {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.AUDIT_SUCCESS);
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.GLOBAL_AUDIT_SUCCESS);
        }
    }
}

