namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public sealed class WorkflowRoleCollection : List<WorkflowRole>
    {
        public bool IncludesIdentity(string identity)
        {
            if (identity != null)
            {
                foreach (WorkflowRole role in this)
                {
                    if ((role != null) && role.IncludesIdentity(identity))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

