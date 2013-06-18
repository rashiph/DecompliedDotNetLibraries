namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;

    public interface IWorkflowRootDesigner : IRootDesigner, IDesigner, IDisposable
    {
        bool IsSupportedActivityType(Type activityType);

        CompositeActivityDesigner InvokingDesigner { get; set; }

        ReadOnlyCollection<WorkflowDesignerMessageFilter> MessageFilters { get; }

        bool SupportsLayoutPersistence { get; }
    }
}

