namespace System.ComponentModel
{
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    internal static class CompModSwitches
    {
        private static BooleanSwitch commonDesignerServices;
        private static TraceSwitch eventLog;

        public static BooleanSwitch CommonDesignerServices
        {
            get
            {
                if (commonDesignerServices == null)
                {
                    commonDesignerServices = new BooleanSwitch("CommonDesignerServices", "Assert if any common designer service is not found.");
                }
                return commonDesignerServices;
            }
        }

        public static TraceSwitch EventLog
        {
            get
            {
                if (eventLog == null)
                {
                    eventLog = new TraceSwitch("EventLog", "Enable tracing for the EventLog component.");
                }
                return eventLog;
            }
        }
    }
}

