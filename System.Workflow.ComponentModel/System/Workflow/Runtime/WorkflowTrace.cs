namespace System.Workflow.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal static class WorkflowTrace
    {
        private static TraceSource host;
        private static TraceSource runtime = new TraceSource("System.Workflow.Runtime");
        private static TraceSource tracking;

        static WorkflowTrace()
        {
            runtime.Switch = new SourceSwitch("System.Workflow.Runtime", SourceLevels.Off.ToString());
            tracking = new TraceSource("System.Workflow.Runtime.Tracking");
            tracking.Switch = new SourceSwitch("System.Workflow.Runtime.Tracking", SourceLevels.Off.ToString());
            host = new TraceSource("System.Workflow.Runtime.Hosting");
            host.Switch = new SourceSwitch("System.Workflow.Runtime.Hosting", SourceLevels.Off.ToString());
            BooleanSwitch switch2 = new BooleanSwitch("System.Workflow LogToFile", "Log traces to file");
            if (switch2.Enabled)
            {
                TextWriterTraceListener listener = new TextWriterTraceListener("WorkflowTrace.log");
                Trace.Listeners.Add(listener);
                runtime.Listeners.Add(listener);
                host.Listeners.Add(listener);
            }
            BooleanSwitch switch3 = new BooleanSwitch("System.Workflow LogToTraceListeners", "Trace to listeners in Trace.Listeners", "0");
            if (switch3.Enabled)
            {
                foreach (TraceListener listener2 in Trace.Listeners)
                {
                    if (!(listener2 is DefaultTraceListener))
                    {
                        runtime.Listeners.Add(listener2);
                        tracking.Listeners.Add(listener2);
                        host.Listeners.Add(listener2);
                    }
                }
            }
        }

        internal static TraceSource Host
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return host;
            }
        }

        internal static TraceSource Runtime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return runtime;
            }
        }

        internal static TraceSource Tracking
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return tracking;
            }
        }
    }
}

