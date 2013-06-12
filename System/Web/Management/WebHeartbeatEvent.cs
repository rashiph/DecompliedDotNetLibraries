namespace System.Web.Management
{
    using System;

    public class WebHeartbeatEvent : WebManagementEvent
    {
        private static WebProcessStatistics s_procStats = new WebProcessStatistics();

        internal WebHeartbeatEvent()
        {
        }

        protected internal WebHeartbeatEvent(string message, int eventCode) : base(message, null, eventCode)
        {
        }

        internal override void FormatToString(WebEventFormatter formatter, bool includeAppInfo)
        {
            base.FormatToString(formatter, includeAppInfo);
            formatter.AppendLine(string.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_process_statistics"));
            formatter.IndentationLevel++;
            s_procStats.FormatToString(formatter);
            formatter.IndentationLevel--;
        }

        public WebProcessStatistics ProcessStatistics
        {
            get
            {
                return s_procStats;
            }
        }
    }
}

