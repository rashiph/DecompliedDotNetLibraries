namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    internal class RuntimeEnvironment : IDisposable
    {
        [ThreadStatic]
        private static WorkflowRuntime workflowRuntime;

        public RuntimeEnvironment(WorkflowRuntime runtime)
        {
            workflowRuntime = runtime;
        }

        void IDisposable.Dispose()
        {
            workflowRuntime = null;
        }

        internal static WorkflowRuntime CurrentRuntime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return workflowRuntime;
            }
        }
    }
}

