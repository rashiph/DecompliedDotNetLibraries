namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IWorkflowChangeDiff
    {
        IList<WorkflowChangeAction> Diff(object originalDefinition, object changedDefinition);
    }
}

