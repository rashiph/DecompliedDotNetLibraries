namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;

    public interface IStartWorkflow
    {
        Guid StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues);
    }
}

