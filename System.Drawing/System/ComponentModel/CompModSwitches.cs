namespace System.ComponentModel
{
    using System;
    using System.Diagnostics;

    internal static class CompModSwitches
    {
        private static TraceSwitch handleLeak;
        private static BooleanSwitch traceCollect;

        public static TraceSwitch HandleLeak
        {
            get
            {
                if (handleLeak == null)
                {
                    handleLeak = new TraceSwitch("HANDLELEAK", "HandleCollector: Track Win32 Handle Leaks");
                }
                return handleLeak;
            }
        }

        public static BooleanSwitch TraceCollect
        {
            get
            {
                if (traceCollect == null)
                {
                    traceCollect = new BooleanSwitch("TRACECOLLECT", "HandleCollector: Trace HandleCollector operations");
                }
                return traceCollect;
            }
        }
    }
}

