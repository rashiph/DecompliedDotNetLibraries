namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public abstract class WorkflowRole
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowRole()
        {
        }

        public abstract IList<string> GetIdentities();
        public abstract bool IncludesIdentity(string identity);

        public abstract string Name { get; set; }
    }
}

