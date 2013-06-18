namespace System.Workflow.Activities
{
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal static class WorkflowActivityTrace
    {
        private static TraceSource activity = new TraceSource("System.Workflow.Activities");
        private static TraceSource rules;

        static WorkflowActivityTrace()
        {
            activity.Switch = new SourceSwitch("System.Workflow.Activities", SourceLevels.Off.ToString());
            rules = new TraceSource("System.Workflow.Activities.Rules");
            rules.Switch = new SourceSwitch("System.Workflow.Activities.Rules", SourceLevels.Off.ToString());
            foreach (TraceListener listener in Trace.Listeners)
            {
                if (!(listener is DefaultTraceListener))
                {
                    activity.Listeners.Add(listener);
                    rules.Listeners.Add(listener);
                }
            }
        }

        internal static TraceSource Activity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return activity;
            }
        }

        internal static TraceSource Rules
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return rules;
            }
        }
    }
}

