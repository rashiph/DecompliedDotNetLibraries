namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class WorkflowDebuggerSteppingAttribute : Attribute
    {
        private WorkflowDebuggerSteppingOption steppingOption;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowDebuggerSteppingAttribute(WorkflowDebuggerSteppingOption steppingOption)
        {
            this.steppingOption = steppingOption;
        }

        public WorkflowDebuggerSteppingOption SteppingOption
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.steppingOption;
            }
        }
    }
}

